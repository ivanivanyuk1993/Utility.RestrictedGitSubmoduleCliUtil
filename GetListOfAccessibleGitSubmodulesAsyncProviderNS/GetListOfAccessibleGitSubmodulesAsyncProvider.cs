using System.CommandLine.IO;
using CliExitCodeProviderNS;
using ExecuteCliCommandAsyncProviderNS;
using GitSubmoduleInfoNS;

namespace GetListOfAccessibleGitSubmodulesAsyncProviderNS;

/// <summary>
///     Notice that <see cref="GetListOfAccessibleGitSubmodulesAsyncProvider"/> runs concurrently and some git
///     providers will not handle it well(like `Cloud Source Repositories`, which I strongly advice to not use)
/// </summary>
public static class GetListOfAccessibleGitSubmodulesAsyncProvider
{
    /// <summary>
    ///     Sometimes git provider doesn't answer, hence we need to retry some times
    /// </summary>
    private const uint RetryCount = 3;

    public static async Task<GitSubmoduleInfo[]> GetListOfAccessibleGitSubmodulesAsync(
        IEnumerable<GitSubmoduleInfo> gitSubmoduleInfoList,
        CancellationToken cancellationToken
    )
    {
        var systemConsole = new SystemConsole();
        var gitSubmoduleInfoAndIsAccessibleEnumerable = gitSubmoduleInfoList
            .Select(selector: async gitSubmoduleInfo =>
            {
                var isSuccessfulCliExitCode = false;

                var retriesLeft = RetryCount;
                while (retriesLeft > 0)
                {
                    isSuccessfulCliExitCode =(
                        await ExecuteCliCommandAsyncProvider.ExecuteCliCommandAsync(
                            cliCommandText: $"git ls-remote \"{gitSubmoduleInfo.Url}\"",
                            console: systemConsole,
                            cancellationToken: cancellationToken
                        )
                    ).IsSuccessfulCliExitCode();

                    if (isSuccessfulCliExitCode)
                    {
                        break;
                    }

                    --retriesLeft;
                }

                return new
                {
                    GitSubmoduleInfo = gitSubmoduleInfo,
                    IsAccessible = isSuccessfulCliExitCode
                };
            });

        var gitSubmoduleInfoAndIsAccessibleList = await Task.WhenAll(tasks: gitSubmoduleInfoAndIsAccessibleEnumerable);
        return gitSubmoduleInfoAndIsAccessibleList
            .Where(gitSubmoduleInfoAndIsAccessible => gitSubmoduleInfoAndIsAccessible.IsAccessible)
            .Select(gitSubmoduleInfoAndIsAccessible => gitSubmoduleInfoAndIsAccessible.GitSubmoduleInfo)
            .ToArray();
    }
}