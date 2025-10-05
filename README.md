# InfraGitOps

A local-machine infrastructure GitOps system managing platform-layer components with manifest-driven configuration and automatic Git synchronization.

## Components

The system manages five infrastructure components:
- **docker**: Container orchestration
- **nginx**: Web server and reverse proxy
- **pm2**: Process manager for Node.js applications
- **ufw**: Uncomplicated Firewall
- **images**: Container image management

## Architecture

### Dependency Flow
```
docker → pm2 → nginx
ufw (independent)
images (independent)
```

### Key Concepts

**Manifests**: JSON configuration files in the `Config` folder defining desired state for each component.

**Transformers**: Manifest-to-manifest connectors that propagate changes between dependent components (e.g., Docker changes update PM2 manifest, PM2 changes update Nginx manifest).

**Workflow Orchestrations**: Component-specific orchestrations using System.Reactive for pub/sub event handling within each workflow.

**Appliers**: Manifest-to-host scripts that apply configurations to the local machine.

**Exporters**: Host-to-manifest scripts that read current state from the host and update manifests.

**FlowRegistry**: Manages component dependencies and computes topological ordering for change propagation.

### Change Flow

1. FileSystemWatcher detects manifest change
2. FlowRegistry calculates affected components in dependency order
3. For each affected component:
   - Load and validate manifest
   - Workflow orchestration publishes change event
   - Transformers update dependent manifests
   - Applier applies configuration to host
4. Exporters read host state and update all manifests
5. Git commit and push changes to origin/main

## Project Structure

```
InfraGitOps/
├── Interfaces/
│   ├── ITransformer.cs           # Transform(sourceManifest) → targetManifest
│   ├── IExporter.cs              # Export() → manifestData
│   ├── IApplier.cs               # Apply(manifestData)
│   └── IWorkflowOrchestration.cs # ExecuteAsync(manifestData)
├── Models/
│   ├── DockerManifest.cs
│   ├── NginxManifest.cs
│   ├── Pm2Manifest.cs
│   ├── UfwManifest.cs
│   └── ImagesManifest.cs
├── Orchestrator/
│   ├── Orchestrator.cs           # Main coordinator with FileSystemWatcher
│   ├── FlowRegistry.cs           # Dependency management and topological sort
│   └── OrchestratorFactory.cs    # DI composition root
├── UseCases/
│   ├── ApplyManifest.cs          # Load, validate, delegate to workflow
│   ├── ExportDevice.cs           # Export all host states
│   ├── ImportManifest.cs         # Load JSON manifests
│   └── ValidateManifest.cs       # Validate manifest integrity
├── Workflows/
│   ├── DockerWorkflowOrchestration.cs
│   ├── Pm2WorkflowOrchestration.cs
│   ├── NginxWorkflowOrchestration.cs
│   ├── UfwWorkflowOrchestration.cs
│   └── ImagesWorkflowOrchestration.cs
├── Scripts/
│   ├── ApplyDocker.cs
│   ├── ApplyNginx.cs
│   ├── ApplyPm2.cs
│   ├── ApplyUfw.cs
│   └── ApplyImages.cs
├── Transformers/
│   ├── TransformerDockerToPm2.cs
│   └── TransformerPm2ToNginx.cs
├── Exporters/
│   ├── ExporterDocker.cs
│   ├── ExporterNginx.cs
│   ├── ExporterPm2.cs
│   ├── ExporterUfw.cs
│   └── ExporterImages.cs
├── Config/
│   ├── manifest_docker.json
│   ├── manifest_nginx.json
│   ├── manifest_pm2.json
│   ├── manifest_ufw.json
│   └── manifest_images.json
└── Program.cs
```

## Building and Running

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project InfraGitOps/InfraGitOps.csproj
```

### Test
```bash
dotnet test
```

All 46 tests covering unit, integration, and end-to-end scenarios.

## Usage

1. Start the application - it begins watching the Config folder
2. Press 't' to trigger a test change on the docker component
3. Press 'q' to quit

When a manifest file changes:
- System detects the change
- Calculates affected downstream components
- Runs transformers to update dependent manifests
- Applies configurations in dependency order
- Exports host state back to manifests
- Commits and pushes changes to Git

## Technologies

- **.NET 8**: Modern C# with async/await
- **System.Text.Json**: High-performance JSON serialization
- **System.Reactive**: Reactive Extensions for pub/sub event handling
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **xUnit**: Unit testing framework
- **Moq**: Mocking library for tests

## Design Principles

- **Single Responsibility**: Each class has one clear purpose
- **Dependency Injection**: All dependencies injected via constructor
- **Inversion of Control**: Composition root in OrchestratorFactory
- **Polymorphism**: Uniform interfaces (IApplier, IExporter, ITransformer, IWorkflowOrchestration)
- **Self-documenting code**: Descriptive names, no comments
- **Event-driven**: System.Reactive for change propagation within workflows
- **Ordered execution**: FlowRegistry ensures dependency-correct processing

## Test Coverage

- **FlowRegistryTests**: Dependency resolution, topological sorting, affected component calculation
- **TransformerTests**: Manifest transformation logic
- **ExporterTests**: Host state export functionality
- **ApplierTests**: Manifest application to host
- **UseCaseTests**: Import, validate, apply, export use cases
- **WorkflowOrchestrationTests**: Workflow execution for all components
- **IntegrationTests**: DI container setup, orchestrator integration
- **EndToEndTests**: Full change propagation flow

## Extending

### Add a new component
1. Create model in `Models/`
2. Implement `IApplier` in `Scripts/`
3. Implement `IExporter` in `Exporters/`
4. Create workflow orchestration in `Workflows/`
5. Register dependencies in `FlowRegistry`
6. Add to `OrchestratorFactory`
7. Create manifest JSON in `Config/`

### Add a new transformer
1. Implement `ITransformer` in `Transformers/`
2. Register in `OrchestratorFactory.RegisterTransformers`
