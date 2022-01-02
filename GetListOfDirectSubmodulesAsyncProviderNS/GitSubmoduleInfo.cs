namespace GetListOfDirectSubmodulesAsyncProviderNS;

public class GitSubmoduleInfo
{
    public GitSubmoduleInfo(string path, string submodule, string url)
    {
        Path = path;
        Submodule = submodule;
        Url = url;
    }

    public string Path { get; }
    public string Submodule { get; }
    public string Url { get; }
}