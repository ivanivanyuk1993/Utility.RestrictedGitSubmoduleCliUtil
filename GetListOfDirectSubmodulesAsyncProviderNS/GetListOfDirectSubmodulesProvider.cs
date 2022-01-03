using System.Text.RegularExpressions;
using GitmodulesFileNameProviderNS;
using GitSubmoduleInfoNS;
using NewLineRegexPatternProviderNS;

namespace GetListOfDirectSubmodulesAsyncProviderNS;

public static class GetListOfDirectSubmodulesAsyncProvider
{
    private static readonly Regex ListOfDirectSubmodulesRegex = new(
        pattern: $"\\[submodule \"(.+)\"\\]{NewLineRegexPatternProvider.NewLineRegexPattern}\\tpath = (.+){NewLineRegexPatternProvider.NewLineRegexPattern}\\turl = (.+)",
        options: RegexOptions.ExplicitCapture & RegexOptions.Singleline
    );

    public static async Task<GitSubmoduleInfo[]> GetListOfDirectSubmodulesAsync(
        DirectoryInfo gitRootDirectoryInfo,
        CancellationToken cancellationToken
    )
    {
        var gitModulesFilePath = Path.Combine(
            path1: gitRootDirectoryInfo.FullName,
            path2: GitmodulesFileNameProvider.GitmodulesFileName
        );

        var gitModulesTextContent = await File.ReadAllTextAsync(
            path: gitModulesFilePath,
            cancellationToken: cancellationToken
        );

        var matchCollection = ListOfDirectSubmodulesRegex.Matches(input: gitModulesTextContent);

        return matchCollection
            .Select(selector: match => new GitSubmoduleInfo(
                path: match.Groups[2].Value,
                submodule: match.Groups[1].Value,
                url: match.Groups[3].Value
            ))
            .ToArray();
    }
}