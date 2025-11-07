namespace Payroc.App.LoadBalancer.BackendSelectionStrategies;

public class RoundRobinBackendSelectionStrategy : IBackendSelectionStrategy
{
    private int _currentIndex = -1;

    public BackendServer? Select(IReadOnlyList<BackendServer> healthyBackends)
    {
        if (healthyBackends.Count == 0)
            return null;

        _currentIndex = (_currentIndex + 1) % healthyBackends.Count;
        return healthyBackends[_currentIndex];
    }
}
