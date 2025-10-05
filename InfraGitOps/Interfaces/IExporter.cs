namespace InfraGitOps.Interfaces;

public interface IExporter
{
    Task<object> ExportAsync();
    string ComponentName { get; }
}