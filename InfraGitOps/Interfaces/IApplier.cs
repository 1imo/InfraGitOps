namespace InfraGitOps.Interfaces;

public interface IApplier
{
    Task ApplyAsync(object manifestData);
    string ComponentName { get; }
}