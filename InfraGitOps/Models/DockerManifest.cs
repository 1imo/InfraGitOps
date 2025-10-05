namespace InfraGitOps.Models;

public class DockerManifest
{
    public int Version { get; set; }
    public List<DockerContainer>? Containers { get; set; }
}

public class DockerContainer
{
    public string? Name { get; set; }
    public string? Image { get; set; }
    public List<string>? Ports { get; set; }
}