using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliExitCodeProviderNS;
using ExecuteCliCommandAsyncProviderNS;
using GetListOfAccessibleGitSubmodulesAsyncProviderNS;
using GetListOfDirectSubmodulesAsyncProviderNS;
using GitmodulesFileNameProviderNS;
using ValueOrErrorNS;

namespace GitSubmoduleInitAccessibleAsyncProviderNS;

/// <summary>
///     todo replace <see cref="Task"/>-s with <see cref="IObservable{T}"/>-s
/// </summary>
public static class GitSubmoduleInitAccessibleAsyncProvider
{
    public static async Task<ValueOrError<Unit, Exception>> GitSubmoduleInitAccessibleAsync(
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
            var gitSubmoduleInfoListOrError = await GetListOfDirectSubmodulesAsyncProvider.GetListOfDirectSubmodulesAsync(
                console: console,
                gitRootDirectoryInfo: gitRootDirectoryInfo,
                cancellationToken: cancellationToken
            );

            return await gitSubmoduleInfoListOrError.RunActionWithResultWithValueOrErrorReactive(
                gitSubmoduleInfoList => Observable.FromAsync(async () =>
                {
                    var listOfAccessibleGitSubmodules = await GetListOfAccessibleGitSubmodulesAsyncProvider.GetListOfAccessibleGitSubmodulesAsync(
                        gitSubmoduleInfoList: gitSubmoduleInfoList,
                        console: console,
                        cancellationToken: cancellationToken
                    );

                    var submoduleInitTasks = listOfAccessibleGitSubmodules
                        .Select(selector: async accessibleGitSubmoduleInfo =>
                        {
                            // Here we can lock git file, hence we need to retry until success, which is somewhat guaranteed,
                            // because we checked accessibility previously
                            // (unless accessibility changes in same period, but it is a rare case and can be handled manually,
                            // and build/deploy machine will always have all permissions)
                            //
                            // Notice that we can run cli command with multiple projects and option `--jobs`, but it
                            // runs too slow
                            int cliExitCode;
                            do
                            {
                                cliExitCode =
                                (
                                    await ExecuteCliCommandAsyncProvider.ExecuteCliCommandAsync(
                                        cliCommandText:
                                        $"git -C \"{gitRootDirectoryInfo.FullName}\" submodule update --init --remote -- \"{accessibleGitSubmoduleInfo.Path}\"",
                                        console: console,
                                        cancellationToken: cancellationToken
                                    )
                                ).ExitCode;
                            } while (!cliExitCode.IsSuccessfulCliExitCode());

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
                    await Task.WhenAll(tasks: submoduleInitTasks);
                    return Unit.Default;
                }),
                Observable.Return
            );
        }

        return ValueOrError<Unit, Exception>.CreateError(
            error: new Exception(
                message: $"File `{GitmodulesFileNameProvider.GitmodulesFileName}` does not exist in `{gitRootDirectoryInfo.FullName}`"
            )
        );
    }
}
