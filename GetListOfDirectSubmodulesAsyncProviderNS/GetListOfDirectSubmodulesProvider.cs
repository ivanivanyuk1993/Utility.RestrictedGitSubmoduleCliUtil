using System.CommandLine;
using System.Text.RegularExpressions;
using GetGitRepositoryUrlAsyncProviderNS;
using GitmodulesFileNameProviderNS;
using GitSubmoduleInfoNS;
using NormalizeLineBreaksProviderNS;

namespace GetListOfDirectSubmodulesAsyncProviderNS;

public static class GetListOfDirectSubmodulesAsyncProvider
{
    private static readonly Regex ListOfDirectSubmodulesRegex = new(
        pattern: "\\[submodule \"(.+)\"\\]\\n\\tpath = (.+)\\n\\turl = (.+)",
        options: RegexOptions.ExplicitCapture & RegexOptions.Singleline
    );

    public static async Task<GitSubmoduleInfo[]> GetListOfDirectSubmodulesAsync(
        IConsole console,
        DirectoryInfo gitRootDirectoryInfo,
        CancellationToken cancellationToken
    )
    {
        var gitModulesFilePath = Path.Combine(
            path1: gitRootDirectoryInfo.FullName,
            path2: GitmodulesFileNameProvider.GitmodulesFileName
        );

        var parentUrlOrError = await GetGitRepositoryUrlAsyncProvider.GetGitRepositoryUrlAsync(
            console: console,
            directoryInfo: gitRootDirectoryInfo,
            cancellationToken: cancellationToken
        );

        // todo DRY
        if (parentUrlOrError.IsError)
        {
            throw parentUrlOrError.Error;
        }

        var gitModulesTextContent = await File.ReadAllTextAsync(
            path: gitModulesFilePath,
            cancellationToken: cancellationToken
        );

        var matchCollection = ListOfDirectSubmodulesRegex.Matches(input: gitModulesTextContent.NormalizeLineBreaks());

        return matchCollection
            .Select(selector: match =>
            {
                var urlFromGitmodules = match.Groups[groupnum: 3].Value;
                var isRelativeUrl =
                    urlFromGitmodules.StartsWith(value: "./")
                    ||
                    urlFromGitmodules.StartsWith(value: "../");
                return new GitSubmoduleInfo(
                    path: match.Groups[groupnum: 2].Value,
                    submodule: match.Groups[groupnum: 1].Value,
                    absoluteUrl: isRelativeUrl
                        ? Path.GetRelativePath(
                            path: Path
                                .Combine(
                                    path1: parentUrlOrError.Value,
                                    path2: urlFromGitmodules
                                ),
                            relativeTo: "."
                        )
                            // We need it to have correct result in Windows
                            .Replace(
                                oldValue: "\\",
                                newValue: "/"
                            )
                        : urlFromGitmodules,
                    urlFromGitmodules: urlFromGitmodules
                );
            })
            .ToArray();
    }
}