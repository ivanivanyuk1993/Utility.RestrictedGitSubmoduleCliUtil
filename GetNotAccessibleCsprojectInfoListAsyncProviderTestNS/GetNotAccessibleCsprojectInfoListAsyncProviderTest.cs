using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GetNotAccessibleCsprojectInfoListAsyncProviderNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetNotAccessibleCsprojectInfoListAsyncProviderTestNS;

[TestClass]
public class GetNotAccessibleCsprojectInfoListAsyncProviderTest
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var cancellationToken = CancellationToken.None;
        var notAccessibleCsprojectFileInfoList = await GetNotAccessibleCsprojectInfoListAsyncProvider.GetNotAccessibleCsprojectInfoListAsync(
            solutionFileInfo: new FileInfo(
                fileName: Path.Combine(
                    path1: AppDomain.CurrentDomain.BaseDirectory,
                    path2: "SolutionForTest.sln"
                )
            ),
            cancellationToken: cancellationToken
        );

        Assert.AreEqual(
            actual: notAccessibleCsprojectFileInfoList.Length,
            expected: 1
        );

        Assert.AreEqual(
            actual: notAccessibleCsprojectFileInfoList.First().EntryInSolutionFile,
            expected: "GetListOfDirectSubmodulesAsyncProviderNS\\GetListOfDirectSubmodulesAsyncProviderNS.csproj"
        );
    }
}