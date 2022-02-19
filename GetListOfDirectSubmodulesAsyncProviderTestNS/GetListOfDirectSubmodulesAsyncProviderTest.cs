using System;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GetListOfDirectSubmodulesAsyncProviderNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetListOfDirectSubmodulesAsyncProviderTestNS;

[TestClass]
public class GetListOfDirectSubmodulesAsyncProviderTest
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var cancellationToken = CancellationToken.None;
        var systemConsole = new SystemConsole();
        var listOfDirectSubmodules = await GetListOfDirectSubmodulesAsyncProvider.GetListOfDirectSubmodulesAsync(
            console: systemConsole,
            gitRootDirectoryInfo: new DirectoryInfo(path: AppDomain.CurrentDomain.BaseDirectory),
            cancellationToken: cancellationToken
        );

        Assert.AreEqual(
            actual: listOfDirectSubmodules.Length,
            expected: 4
        );

        var gitSubmoduleInfo = listOfDirectSubmodules.First();

        Assert.AreEqual(
            actual: gitSubmoduleInfo.Path,
            expected: "SpinLockUtil"
        );
        Assert.AreEqual(
            actual: gitSubmoduleInfo.Submodule,
            expected: "SpinLockUtil"
        );
        Assert.AreEqual(
            actual: gitSubmoduleInfo.AbsoluteUrl,
            expected: "git@github.com:ivanivanyuk1993/Utility.SpinLockUtil.git"
        );

        gitSubmoduleInfo = listOfDirectSubmodules.Skip(1).First();
        Assert.AreEqual(
            actual: gitSubmoduleInfo.Path,
            expected: "ShardedQueue"
        );
        Assert.AreEqual(
            actual: gitSubmoduleInfo.Submodule,
            expected: "ShardedQueue"
        );
        Assert.AreEqual(
            actual: gitSubmoduleInfo.AbsoluteUrl,
            expected: "git@github.com:ivanivanyuk1993/Utility.ShardedQueue.git"
        );

        gitSubmoduleInfo = listOfDirectSubmodules.Skip(2).First();
        Assert.AreEqual(
            actual: gitSubmoduleInfo.Path,
            expected: "AsyncReadWriteLock"
        );
        Assert.AreEqual(
            actual: gitSubmoduleInfo.Submodule,
            expected: "AsyncReadWriteLock"
        );
        Assert.AreEqual(
            actual: gitSubmoduleInfo.AbsoluteUrl,
            expected: "git@github.com:ivanivanyuk1993/Utility.AsyncReadWriteLock.git"
        );
        Assert.AreEqual(
            actual: gitSubmoduleInfo.AbsoluteUrl,
            expected: "git@github.com:ivanivanyuk1993/Utility.AsyncReadWriteLock.git"
        );
    }
}