// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using MakeSolutionWithAccessibleCsprojectsAsyncProviderNS;
using RunCliTaskProviderNS;

var solutionFilePathOption = new Option<string>(
    name: "--solution-file-path"
);

var rootCommand = new RootCommand
{
    solutionFilePathOption
};

rootCommand.SetHandler(
    handle: (
        string solutionFilePath,
        IConsole console,
        CancellationToken cancellationToken
    ) =>
    {
        var solutionFileInfo = new FileInfo(fileName: solutionFilePath);
        return RunCliTaskProvider.RunCliTask(
            console: console,
            taskName:
            $"Running `{nameof(MakeSolutionWithAccessibleCsprojectsAsyncProvider.MakeSolutionWithAccessibleCsprojectsAsync)}` on file `{solutionFileInfo.FullName}`",
            runCliTaskFunc: cancellationToken2 =>
                MakeSolutionWithAccessibleCsprojectsAsyncProvider.MakeSolutionWithAccessibleCsprojectsAsync(
                    solutionFileInfo: solutionFileInfo,
                    cancellationToken: cancellationToken2
                ),
            cancellationToken: cancellationToken
        );
    },
    solutionFilePathOption
);

return await rootCommand.InvokeAsync(args: args);