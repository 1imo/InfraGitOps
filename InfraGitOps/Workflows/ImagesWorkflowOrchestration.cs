using InfraGitOps.Interfaces;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace InfraGitOps.Workflows;

public class ImagesWorkflowOrchestration : IWorkflowOrchestration
{
    public string ComponentName => "images";

    private readonly IApplier _applier;
    private readonly IEnumerable<ITransformer> _transformers;
    private readonly Subject<object> _manifestChangeSubject;

    public ImagesWorkflowOrchestration(
        IApplier applier,
        IEnumerable<ITransformer> transformers)
    {
        _applier = applier;
        _transformers = transformers;
        _manifestChangeSubject = new Subject<object>();

        SetupSubscriptions();
    }

    private void SetupSubscriptions()
    {
        _manifestChangeSubject
            .SelectMany(async manifestData => await ProcessTransformersAsync(manifestData))
            .Subscribe(async _ => await ApplyToHostAsync(_));
    }

    public async Task ExecuteAsync(object manifestData)
    {
        Console.WriteLine($"[ImagesWorkflowOrchestration] Starting workflow for {ComponentName}");
        _manifestChangeSubject.OnNext(manifestData);
        await Task.Delay(200);
    }

    private async Task<object> ProcessTransformersAsync(object manifestData)
    {
        var outboundTransformers = _transformers.Where(t => t.SourceComponent == ComponentName);
        
        foreach (var transformer in outboundTransformers)
        {
            Console.WriteLine($"[ImagesWorkflowOrchestration] Running transformer: {transformer.SourceComponent} → {transformer.TargetComponent}");
            await transformer.TransformAsync(manifestData);
        }

        return manifestData;
    }

    private async Task ApplyToHostAsync(object manifestData)
    {
        await _applier.ApplyAsync(manifestData);
    }
}
