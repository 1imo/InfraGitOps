namespace InfraGitOps.Models;

public class NginxManifest
{
    public int Version { get; set; }
    public List<NginxServer>? Servers { get; set; }
}

public class NginxServer
{
    public string? ServerName { get; set; }
    public int Port { get; set; }
    public string? ProxyPass { get; set; }
}