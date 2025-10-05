namespace InfraGitOps.Models;

public class ImagesManifest
{
    public int Version { get; set; }
    public List<ImageDefinition>? Images { get; set; }
}

public class ImageDefinition
{
    public string? Name { get; set; }
    public string? Repository { get; set; }
    public string? Tag { get; set; }
}