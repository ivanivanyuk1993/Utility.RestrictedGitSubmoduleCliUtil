using System;
using System.CommandLine.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GetListOfAccessibleGitSubmodulesAsyncProviderNS;
using GitSubmoduleInfoNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetListOfAccessibleGitSubmodulesAsyncProviderTestNS;

[TestClass]
public class GetListOfAccessibleGitSubmodulesAsyncProviderTest
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var cancellationToken = CancellationToken.None;

        var urlListToTest = new []
        {
            // According to https://gitstar-ranking.com/ it is the most popular repository on github, so we assume that
            // it will live long and use it
            "git@github.com:freeCodeCamp/freeCodeCamp.git",
            $"git@github.com:freeCodeCamp/freeCodeCamp-{Guid.NewGuid().ToString()}.git"
        };

        var gitSubmoduleInfoToTest = urlListToTest
            .Select(url => new GitSubmoduleInfo(
                path: "Fake Path",
                submodule: "Fake submodule",
                absoluteUrl: url,
                urlFromGitmodules: url
            ))
            .ToArray();

        var accessibleList = await GetListOfAccessibleGitSubmodulesAsyncProvider.GetListOfAccessibleGitSubmodulesAsync(
            gitSubmoduleInfoList: gitSubmoduleInfoToTest,
            console: new SystemConsole(),
            cancellationToken: cancellationToken
        );

        Assert.AreEqual(
            actual: accessibleList.Length,
            expected: 1
        );
        Assert.AreEqual(
            actual: accessibleList.First().AbsoluteUrl,
            expected: gitSubmoduleInfoToTest.First().AbsoluteUrl
        );
    }
}