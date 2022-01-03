// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using GitSubmoduleInitAccessibleAsyncProviderNS;
using RunCliTaskProviderNS;

var gitRootDirectoryPathOption = new Option<string>(
    name: "--git-root-directory-path"
);

var rootCommand = new RootCommand
{
    gitRootDirectoryPathOption
};

rootCommand.SetHandler(
    handle: (
        string gitRootDirectoryPath,
        IConsole console,
        CancellationToken cancellationToken
    ) =>
    {
        var gitRootDirectoryInfo = new DirectoryInfo(path: gitRootDirectoryPath);
        return RunCliTaskProvider.RunCliTask(
            console: console,
            taskName:
            $"Running `{nameof(GitSubmoduleInitAccessibleAsyncProvider.GitSubmoduleInitAccessibleAsync)}` in directory `{gitRootDirectoryInfo.FullName}`",
            runCliTaskFunc: cancellationToken2 =>
                GitSubmoduleInitAccessibleAsyncProvider.GitSubmoduleInitAccessibleAsync(
                    gitRootDirectoryInfo: gitRootDirectoryInfo,
                    cancellationToken: cancellationToken2
                ),
            cancellationToken: cancellationToken
        );
    },
    gitRootDirectoryPathOption
);

return await rootCommand.InvokeAsync(args: args);