namespace InfraGitOps.Orchestrator;

public class FlowRegistry
{
    private readonly Dictionary<string, List<string>> _dependencies;
    private readonly Dictionary<string, List<string>> _dependents;

    public FlowRegistry()
    {
        _dependencies = new Dictionary<string, List<string>>();
        _dependents = new Dictionary<string, List<string>>();
        InitializeDependencies();
    }

    private void InitializeDependencies()
    {
        RegisterDependency("docker", new List<string>());
        RegisterDependency("pm2", new List<string> { "docker" });
        RegisterDependency("nginx", new List<string> { "pm2" });
        RegisterDependency("ufw", new List<string>());
        RegisterDependency("images", new List<string>());
    }

    private void RegisterDependency(string component, List<string> dependencies)
    {
        _dependencies[component] = dependencies;
        if (!_dependents.ContainsKey(component))
        {
            _dependents[component] = new List<string>();
        }

        foreach (var dependency in dependencies)
        {
            if (!_dependents.ContainsKey(dependency))
            {
                _dependents[dependency] = new List<string>();
            }
            _dependents[dependency].Add(component);
        }
    }

    public List<string> GetDependencies(string component)
    {
        return _dependencies.ContainsKey(component) 
            ? _dependencies[component] 
            : new List<string>();
    }

    public List<string> GetDependents(string component)
    {
        return _dependents.ContainsKey(component) 
            ? _dependents[component] 
            : new List<string>();
    }

    public List<string> GetAffectedComponents(string changedComponent)
    {
        var affected = new HashSet<string> { changedComponent };
        var queue = new Queue<string>();
        queue.Enqueue(changedComponent);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var dependents = GetDependents(current);
            
            foreach (var dependent in dependents)
            {
                if (!affected.Contains(dependent))
                {
                    affected.Add(dependent);
                    queue.Enqueue(dependent);
                }
            }
        }

        return TopologicalSort(affected.ToList());
    }

    public List<string> TopologicalSort(List<string> components)
    {
        var sorted = new List<string>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        foreach (var component in components)
        {
            if (!visited.Contains(component))
            {
                TopologicalSortHelper(component, visited, visiting, sorted, components);
            }
        }

        return sorted;
    }

    private void TopologicalSortHelper(string component, HashSet<string> visited, 
        HashSet<string> visiting, List<string> sorted, List<string> allowedComponents)
    {
        if (visited.Contains(component))
            return;

        if (visiting.Contains(component))
            throw new InvalidOperationException($"Circular dependency detected for {component}");

        visiting.Add(component);

        var dependencies = GetDependencies(component);
        foreach (var dependency in dependencies)
        {
            if (allowedComponents.Contains(dependency))
            {
                TopologicalSortHelper(dependency, visited, visiting, sorted, allowedComponents);
            }
        }

        visiting.Remove(component);
        visited.Add(component);
        sorted.Add(component);
    }

    public IEnumerable<string> GetAllComponents()
    {
        return _dependencies.Keys;
    }
}