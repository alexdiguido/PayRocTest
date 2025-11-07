using Payroc.App.LoadBalancer;

namespace Payroc.App.Tests.LoadBalancer;

public class HealthCheckerMock : IHealthChecker
{
    public bool Started { get; private set; }
    public bool Stopped { get; private set; }
    public Task StartAsync()
    {
        Started = true;
        return Task.CompletedTask;
    }
    public void Stop()
    {
        Stopped = true;
    }
}
