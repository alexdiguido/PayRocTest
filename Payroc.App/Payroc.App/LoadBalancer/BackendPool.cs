using Payroc.App.LoadBalancer.BackendSelectionStrategies;
using System.Collections.Concurrent;

namespace Payroc.App.LoadBalancer;

public class BackendPool : IBackendPool
{
    private readonly ConcurrentBag<BackendServer> _backends = new();
    private readonly object _lock = new();
    private readonly IBackendSelectionStrategy _selectionStrategy;

    public BackendPool(IEnumerable<(string Host, int Port)> backendAddresses, IBackendSelectionStrategy selectionStrategy)
    {
        foreach (var (host, port) in backendAddresses)
        {
            _backends.Add(new BackendServer(host, port));
        }

        _selectionStrategy = selectionStrategy;
        Logger.Info($"Initialized backend pool with {_backends.Count} servers");
    }

    public BackendServer? GetNextHealthyBackend()
    { 
        var healthyBackends = _backends.Where(b => b.IsHealthy).ToList();
        if (healthyBackends.Count == 0)
        {
            Logger.Warn("No healthy backends available!");
            return null;
        }
        return _selectionStrategy.Select(healthyBackends);
    }

    public void UpdateBackendHealth(BackendServer backend, bool isHealthy)
    {
        var wasHealthy = backend.IsHealthy;
        backend.IsHealthy = isHealthy;
        backend.LastChecked = DateTime.UtcNow;

        if (wasHealthy != isHealthy)
        {
            Logger.Info($"Backend {backend.Address} status changed: {(isHealthy ? "UP" : "DOWN")}");
        }
    }

    public IReadOnlyList<BackendServer> GetAllBackends()
    {
        return _backends.ToList();
    }

    public int HealthyCount
    {
        get
        { 
            return _backends.Count(b => b.IsHealthy);
        }
    }
}