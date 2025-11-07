namespace Payroc.App.LoadBalancer.BackendSelectionStrategies;

public interface IBackendSelectionStrategy
{
    BackendServer? Select(IReadOnlyList<BackendServer> healthyBackends);
}
