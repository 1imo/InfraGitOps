using InfraGitOps.Interfaces;
using InfraGitOps.Scripts;
using InfraGitOps.Exporters;
using InfraGitOps.Transformers;
using InfraGitOps.UseCases;
using InfraGitOps.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace InfraGitOps.Orchestrator;

public static class OrchestratorFactory
{
    public static IServiceProvider BuildServiceProvider(string configPath)
    {
        var services = new ServiceCollection();

        services.AddSingleton<FlowRegistry>();
        services.AddSingleton(sp => configPath);

        services.AddSingleton<ImportManifest>();
        services.AddSingleton<ValidateManifest>();
        services.AddSingleton<ApplyManifest>();
        services.AddSingleton<ExportDevice>();

        RegisterAppliers(services);
        RegisterExporters(services);
        RegisterTransformers(services);
        RegisterWorkflowOrchestrations(services);

        services.AddSingleton<Orchestrator>(sp => new Orchestrator(
            sp.GetRequiredService<FlowRegistry>(),
            sp.GetRequiredService<ApplyManifest>(),
            sp.GetRequiredService<ExportDevice>(),
            sp.GetServices<IWorkflowOrchestration>(),
            configPath
        ));

        return services.BuildServiceProvider();
    }

    private static void RegisterAppliers(ServiceCollection services)
    {
        services.AddSingleton<IApplier, ApplyDocker>();
        services.AddSingleton<IApplier, ApplyNginx>();
        services.AddSingleton<IApplier, ApplyPm2>();
        services.AddSingleton<IApplier, ApplyUfw>();
        services.AddSingleton<IApplier, ApplyImages>();
    }

    private static void RegisterExporters(ServiceCollection services)
    {
        services.AddSingleton<IExporter, ExporterDocker>();
        services.AddSingleton<IExporter, ExporterNginx>();
        services.AddSingleton<IExporter, ExporterPm2>();
        services.AddSingleton<IExporter, ExporterUfw>();
        services.AddSingleton<IExporter, ExporterImages>();
    }

    private static void RegisterTransformers(ServiceCollection services)
    {
        services.AddSingleton<ITransformer, TransformerDockerToPm2>();
        services.AddSingleton<ITransformer, TransformerPm2ToNginx>();
    }

    private static void RegisterWorkflowOrchestrations(ServiceCollection services)
    {
        services.AddSingleton<IWorkflowOrchestration>(sp => new DockerWorkflowOrchestration(
            sp.GetServices<IApplier>().First(a => a.ComponentName == "docker"),
            sp.GetServices<ITransformer>()
        ));

        services.AddSingleton<IWorkflowOrchestration>(sp => new Pm2WorkflowOrchestration(
            sp.GetServices<IApplier>().First(a => a.ComponentName == "pm2"),
            sp.GetServices<ITransformer>()
        ));

        services.AddSingleton<IWorkflowOrchestration>(sp => new NginxWorkflowOrchestration(
            sp.GetServices<IApplier>().First(a => a.ComponentName == "nginx"),
            sp.GetServices<ITransformer>()
        ));

        services.AddSingleton<IWorkflowOrchestration>(sp => new UfwWorkflowOrchestration(
            sp.GetServices<IApplier>().First(a => a.ComponentName == "ufw"),
            sp.GetServices<ITransformer>()
        ));

        services.AddSingleton<IWorkflowOrchestration>(sp => new ImagesWorkflowOrchestration(
            sp.GetServices<IApplier>().First(a => a.ComponentName == "images"),
            sp.GetServices<ITransformer>()
        ));
    }
}