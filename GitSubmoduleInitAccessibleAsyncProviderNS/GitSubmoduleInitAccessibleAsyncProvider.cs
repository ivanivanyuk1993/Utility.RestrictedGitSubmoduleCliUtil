using System.CommandLine;
using ExecuteCliCommandAsyncProviderNS;
using GetListOfAccessibleGitSubmodulesAsyncProviderNS;
using GetListOfDirectSubmodulesAsyncProviderNS;
using GitmodulesFileNameProviderNS;

namespace GitSubmoduleInitAccessibleAsyncProviderNS;

public static class GitSubmoduleInitAccessibleAsyncProvider
{
    public static async Task GitSubmoduleInitAccessibleAsync(
        DirectoryInfo gitRootDirectoryInfo,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        var existsGitmodules = new FileInfo(
            fileName: Path.Combine(
                path1: gitRootDirectoryInfo.FullName,
                path2: GitmodulesFileNameProvider.GitmodulesFileName
            )
        ).Exists;

        if (existsGitmodules)
        {
            var gitSubmoduleInfoList = await GetListOfDirectSubmodulesAsyncProvider.GetListOfDirectSubmodulesAsync(
                gitRootDirectoryInfo: gitRootDirectoryInfo,
                cancellationToken: cancellationToken
            );

            var listOfAccessibleGitSubmodules = await GetListOfAccessibleGitSubmodulesAsyncProvider.GetListOfAccessibleGitSubmodulesAsync(
                gitSubmoduleInfoList: gitSubmoduleInfoList,
                console: console,
                cancellationToken: cancellationToken
            );

            var submoduleIniTasks = listOfAccessibleGitSubmodules
                .Select(async accessibleGitSubmoduleInfo =>
                {
                    await ExecuteCliCommandAsyncProvider.ExecuteCliCommandAsync(
                        cliCommandText:
                        $"git -C \"{gitRootDirectoryInfo.FullName}\" submodule update --init --remote -- \"{accessibleGitSubmoduleInfo.Path}\"",
                        console: console,
                        cancellationToken: cancellationToken
                    );
                    await GitSubmoduleInitAccessibleAsync(
                        gitRootDirectoryInfo: new DirectoryInfo(
                            path: Path.Combine(
                                path1: gitRootDirectoryInfo.FullName,
                                path2: accessibleGitSubmoduleInfo.Path
                            )
                        ),
                        console: console,
                        cancellationToken: cancellationToken
                    );
                });
            await Task.WhenAll(tasks: submoduleIniTasks);
        }
    }
}