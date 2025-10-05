namespace InfraGitOps.Models;

public class UfwManifest
{
    public int Version { get; set; }
    public List<UfwRule>? Rules { get; set; }
}

public class UfwRule
{
    public string? Name { get; set; }
    public string? Action { get; set; }
    public string? Port { get; set; }
    public string? Protocol { get; set; }
}
