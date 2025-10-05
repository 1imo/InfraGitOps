using InfraGitOps.Interfaces;

namespace InfraGitOps.UseCases;

public class ApplyManifest
{
    private readonly ImportManifest _importManifest;
    private readonly ValidateManifest _validateManifest;
    private readonly IEnumerable<IWorkflowOrchestration> _workflowOrchestrations;

    public ApplyManifest(
        ImportManifest importManifest,
        ValidateManifest validateManifest,
        IEnumerable<IWorkflowOrchestration> workflowOrchestrations)
    {
        _importManifest = importManifest;
        _validateManifest = validateManifest;
        _workflowOrchestrations = workflowOrchestrations;
    }

    public async Task ApplyAsync(string component)
    {
        Console.WriteLine($"[ApplyManifest] Processing component: {component}");

        var manifestData = await _importManifest.ImportAsync(component);

        var isValid = await _validateManifest.ValidateAsync(manifestData, component);
        if (!isValid)
        {
            throw new InvalidOperationException($"Manifest validation failed for {component}");
        }

        var workflow = _workflowOrchestrations.FirstOrDefault(w => w.ComponentName == component);
        if (workflow == null)
        {
            throw new InvalidOperationException($"No workflow orchestration found for component: {component}");
        }

        await workflow.ExecuteAsync(manifestData);
    }
}