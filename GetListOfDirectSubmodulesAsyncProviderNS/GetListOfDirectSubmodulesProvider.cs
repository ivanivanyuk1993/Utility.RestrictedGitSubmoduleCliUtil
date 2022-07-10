using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GetGitRepositoryUrlAsyncProviderNS;
using GitmodulesFileNameProviderNS;
using GitSubmoduleInfoNS;
using NormalizeLineBreaksProviderNS;
using ValueOrErrorNS;

namespace GetListOfDirectSubmodulesAsyncProviderNS;

/// <summary>
///     todo replace <see cref="Task"/>-s with <see cref="IObservable{T}"/>-s
/// </summary>
public static class GetListOfDirectSubmodulesAsyncProvider
{
    private static readonly Regex ListOfDirectSubmodulesRegex = new(
        pattern: "\\[submodule \"(.+)\"\\]\\n\\tpath = (.+)\\n\\turl = (.+)",
        options: RegexOptions.ExplicitCapture & RegexOptions.Singleline
    );

    public static async Task<ValueOrError<GitSubmoduleInfo[], Exception>> GetListOfDirectSubmodulesAsync(
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

        return await parentUrlOrError.RunActionWithResultWithValueOrErrorReactive(
            parentUrl => Observable.FromAsync(async () =>
            {
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
                                                path1: parentUrl,
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
            }),
            Observable.Return
        );
    }
}
