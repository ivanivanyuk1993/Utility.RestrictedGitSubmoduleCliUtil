using System.Text.RegularExpressions;
using CsprojectInfoNS;

namespace GetNotAccessibleCsprojectInfoListAsyncProviderNS;

public static class GetNotAccessibleCsprojectInfoListAsyncProvider
{
    private static readonly Regex CsprojectListRegex = new(
        pattern: "Project\\(\"{(.+)}\"\\) = \"(.+)\", \"(.+)\", \"{(.+)}\"",
        options: RegexOptions.ExplicitCapture & RegexOptions.Singleline
    );

    public static async Task<CsprojectInfo[]> GetNotAccessibleCsprojectInfoListAsync(
        FileInfo solutionFileInfo,
        CancellationToken cancellationToken
    )
    {
        var solutionFileText = await File.ReadAllTextAsync(
            path: solutionFileInfo.FullName,
            cancellationToken: cancellationToken
        );

        var matchCollection = CsprojectListRegex.Matches(input: solutionFileText);

        return matchCollection
            .Select(selector: match =>
            {
                var entryInSolutionFile = match.Groups[groupnum: 3].Value;
                return new CsprojectInfo(
                    entryInSolutionFile: entryInSolutionFile,
                    fileInfo: new FileInfo(
                        fileName: Path.Combine(
                            paths: entryInSolutionFile
                                .Split(separator: "\\")
                                .Prepend(element: solutionFileInfo.DirectoryName!)
                                .ToArray()
                        )
                    )
                );
            })
            .Where(predicate: csprojectInfo =>
                csprojectInfo.EntryInSolutionFile.EndsWith(".csproj")
                &&
                !csprojectInfo.FileInfo.Exists
            )
            .ToArray();
    }
}