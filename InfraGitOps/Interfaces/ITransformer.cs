namespace InfraGitOps.Interfaces;

public interface ITransformer
{
    Task<object> TransformAsync(object sourceManifest);
    string SourceComponent { get; }
    string TargetComponent { get; }
}