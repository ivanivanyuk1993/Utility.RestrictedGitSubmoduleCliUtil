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
    public static async Task<GitSubmoduleInfo[]> GetListOfAccessibleGitSubmodulesAsync(
        IEnumerable<GitSubmoduleInfo> gitSubmoduleInfoList,
        CancellationToken cancellationToken
    )
    {
        var systemConsole = new SystemConsole();
        var gitSubmoduleInfoAndIsAccessibleEnumerable = gitSubmoduleInfoList
            .Select(selector: async gitSubmoduleInfo =>
            {
                var isAccessibleCliExitCode = await ExecuteCliCommandAsyncProvider.ExecuteCliCommandAsync(
                    cliCommandText: $"git ls-remote \"{gitSubmoduleInfo.Url}\"",
                    console: systemConsole,
                    cancellationToken: cancellationToken
                );
                return new
                {
                    GitSubmoduleInfo = gitSubmoduleInfo,
                    IsAccessible = isAccessibleCliExitCode.IsSuccessfulCliExitCode()
                };
            });

        var gitSubmoduleInfoAndIsAccessibleList = await Task.WhenAll(tasks: gitSubmoduleInfoAndIsAccessibleEnumerable);
        return gitSubmoduleInfoAndIsAccessibleList
            .Where(gitSubmoduleInfoAndIsAccessible => gitSubmoduleInfoAndIsAccessible.IsAccessible)
            .Select(gitSubmoduleInfoAndIsAccessible => gitSubmoduleInfoAndIsAccessible.GitSubmoduleInfo)
            .ToArray();
    }
}