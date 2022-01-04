using System.CommandLine;
using CopyDirectoryAsyncProviderNS;
using ExecuteCliCommandAsyncProviderNS;
using GetNotAccessibleCsprojectInfoListAsyncProviderNS;

namespace MakeSolutionWithAccessibleCsprojectsAsyncProviderNS;

public static class MakeSolutionWithAccessibleCsprojectsAsyncProvider
{
    public static async Task MakeSolutionWithAccessibleCsprojectsAsync(
        FileInfo solutionFileInfo,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        var solutionWithAccessibleProjectsFileInfo = new FileInfo(
            fileName: Path.Combine(
                path1: solutionFileInfo.DirectoryName!,
                path2: Path.GetFileNameWithoutExtension(path: solutionFileInfo.FullName) + ".only-accessible.generated.sln"
            )
        );

        await CopyDirectoryAsyncProvider.CopyFileAsync(
            sourceFileInfo: solutionFileInfo,
            targetFileInfo: solutionWithAccessibleProjectsFileInfo,
            cancellationToken: cancellationToken
        );

        var notAccessibleCsprojectInfoList = await GetNotAccessibleCsprojectInfoListAsyncProvider.GetNotAccessibleCsprojectInfoListAsync(
            solutionFileInfo: solutionFileInfo,
            cancellationToken: cancellationToken
        );

        if (notAccessibleCsprojectInfoList.Any())
        {
            var cliCommandText =
                $"dotnet sln \"{solutionWithAccessibleProjectsFileInfo.FullName}\" remove {string.Join(separator: " ", notAccessibleCsprojectInfoList.Select(notAccessibleCsprojectInfo => $"\"{notAccessibleCsprojectInfo.EntryInSolutionFile}\""))}";

            await ExecuteCliCommandAsyncProvider.ExecuteCliCommandAsync(
                cliCommandText: cliCommandText,
                console: console,
                cancellationToken: cancellationToken
            );
        }
    }
}