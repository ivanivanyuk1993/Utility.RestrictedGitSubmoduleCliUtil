using System.CommandLine;
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
    ///     Sometimes git provider(github) doesn't answer, hence we need to retry some times
    /// </summary>
    private const uint RetryCount = 10;

    private static readonly TimeSpan RetryTimeout = TimeSpan.FromMilliseconds(value: 1e3);

    public static async Task<GitSubmoduleInfo[]> GetListOfAccessibleGitSubmodulesAsync(
        IEnumerable<GitSubmoduleInfo> gitSubmoduleInfoList,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        var gitSubmoduleInfoAndIsAccessibleEnumerable = gitSubmoduleInfoList
            .Select(selector: async gitSubmoduleInfo =>
            {
                bool isSuccessfulCliExitCode;

                var retriesLeft = RetryCount;
                while (true)
                {
                    isSuccessfulCliExitCode =(
                        await ExecuteCliCommandAsyncProvider.ExecuteCliCommandAsync(
                            cliCommandText: $"git ls-remote \"{gitSubmoduleInfo.Url}\"",
                            console: console,
                            cancellationToken: cancellationToken
                        )
                    ).IsSuccessfulCliExitCode();

                    if (isSuccessfulCliExitCode)
                    {
                        break;
                    }

                    console.Out.WriteLine(
                        value: $"{nameof(retriesLeft)} for url `{gitSubmoduleInfo.Url}`: {retriesLeft}"
                    );
                    if (--retriesLeft != 0)
                    {
                        await Task.Delay(
                            delay: RetryTimeout,
                            cancellationToken: cancellationToken
                        );
                    }
                    else
                    {
                        break;
                    }
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