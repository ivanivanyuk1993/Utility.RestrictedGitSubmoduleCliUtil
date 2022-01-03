namespace CsprojectInfoNS;

public class CsprojectInfo
{
    public CsprojectInfo(string entryInSolutionFile, FileInfo fileInfo)
    {
        EntryInSolutionFile = entryInSolutionFile;
        FileInfo = fileInfo;
    }

    public string EntryInSolutionFile { get; }
    public FileInfo FileInfo { get; }
}