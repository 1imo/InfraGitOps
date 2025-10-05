namespace InfraGitOps.Interfaces;

public interface IWorkflowOrchestration
{
    string ComponentName { get; }
    Task ExecuteAsync(object manifestData);
}
