using Payroc.App.LoadBalancer;

namespace Payroc.App.Tests.LoadBalancer
{
    public class BackendPoolMock : IBackendPool
    {
        public System.Collections.Generic.List<BackendServer> Backends { get; } = new();
        private int? _healthyCountOverride;
        public int HealthyCount
        {
            get => _healthyCountOverride ?? Backends.Count(b => b.IsHealthy);
            set => _healthyCountOverride = value;
        }

        public IReadOnlyList<BackendServer> GetAllBackends() => Backends;
        public BackendServer? GetNextHealthyBackend() => Backends.FirstOrDefault(b => b.IsHealthy);
        public void UpdateBackendHealth(BackendServer backend, bool isHealthy)
        {
            backend.IsHealthy = isHealthy;
            backend.LastChecked = DateTime.UtcNow;
        }
    }
}

