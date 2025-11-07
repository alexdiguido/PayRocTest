
namespace Payroc.App.LoadBalancer
{
    public interface IBackendPool
    {
        int HealthyCount { get; }

        IReadOnlyList<BackendServer> GetAllBackends();
        BackendServer? GetNextHealthyBackend();
        void UpdateBackendHealth(BackendServer backend, bool isHealthy);
    }
}