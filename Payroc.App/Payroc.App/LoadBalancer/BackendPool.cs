using Payroc.App.LoadBalancer.BackendSelectionStrategies;

namespace Payroc.App.LoadBalancer;

public class BackendPool : IBackendPool
{
    private readonly List<BackendServer> _backends = new();
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
        lock (_lock)
        {
            var healthyBackends = _backends.Where(b => b.IsHealthy).ToList();
            if (healthyBackends.Count == 0)
            {
                Logger.Warn("No healthy backends available!");
                return null;
            }
            return _selectionStrategy.Select(healthyBackends);
        }
    }

    public void UpdateBackendHealth(BackendServer backend, bool isHealthy)
    {
        lock (_lock)
        {
            var wasHealthy = backend.IsHealthy;
            backend.IsHealthy = isHealthy;
            backend.LastChecked = DateTime.UtcNow;

            if (wasHealthy != isHealthy)
            {
                Logger.Info($"Backend {backend.Address} status changed: {(isHealthy ? "UP" : "DOWN")}");
            }
        }
    }

    public IReadOnlyList<BackendServer> GetAllBackends()
    {
        lock (_lock)
        {
            return _backends.ToList();
        }
    }

    public int HealthyCount
    {
        get
        {
            lock (_lock)
            {
                return _backends.Count(b => b.IsHealthy);
            }
        }
    }
}