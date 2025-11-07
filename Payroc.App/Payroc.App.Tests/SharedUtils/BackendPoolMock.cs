using Payroc.App.LoadBalancer;

namespace Payroc.App.Tests.LoadBalancer
{
    public class BackendPoolMock : IBackendPool
    {
        public System.Collections.Generic.List<BackendServer> Backends { get; } = new();
        public int HealthyCount => Backends.Count(b => b.IsHealthy);
        public IReadOnlyList<BackendServer> GetAllBackends() => Backends;
        public BackendServer? GetNextHealthyBackend() => Backends.FirstOrDefault(b => b.IsHealthy);
        public void UpdateBackendHealth(BackendServer backend, bool isHealthy)
        {
            backend.IsHealthy = isHealthy;
            backend.LastChecked = DateTime.UtcNow;
        }
    }
}

