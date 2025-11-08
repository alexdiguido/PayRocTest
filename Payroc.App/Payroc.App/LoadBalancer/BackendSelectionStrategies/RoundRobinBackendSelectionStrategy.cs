using System.Threading;

namespace Payroc.App.LoadBalancer.BackendSelectionStrategies;

public class RoundRobinBackendSelectionStrategy : IBackendSelectionStrategy
{
    private int _currentIndex = -1;

    public BackendServer? Select(IReadOnlyList<BackendServer> healthyBackends)
    {
        if (healthyBackends.Count == 0)
            return null;

        int index = Interlocked.Increment(ref _currentIndex);
        return healthyBackends[Math.Abs(index) % healthyBackends.Count];
    }
}
