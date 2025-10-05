namespace InfraGitOps.Models;

public class Pm2Manifest
{
    public int Version { get; set; }
    public List<Pm2App>? Apps { get; set; }
}

public class Pm2App
{
    public string? Name { get; set; }
    public string? Script { get; set; }
    public int Instances { get; set; }
    public Dictionary<string, string>? Env { get; set; }
}