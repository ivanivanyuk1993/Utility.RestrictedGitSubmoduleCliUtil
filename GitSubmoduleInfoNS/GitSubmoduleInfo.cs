namespace GitSubmoduleInfoNS;

public class GitSubmoduleInfo
{
    public GitSubmoduleInfo(
        string absoluteUrl,
        string path,
        string submodule,
        string urlFromGitmodules
    )
    {
        AbsoluteUrl = absoluteUrl;
        Path = path;
        Submodule = submodule;
        UrlFromGitmodules = urlFromGitmodules;
    }

    public string AbsoluteUrl { get; }
    public string Path { get; }
    public string Submodule { get; }
    /// <summary>
    ///     Can be relative
    /// </summary>
    public string UrlFromGitmodules { get; }
}