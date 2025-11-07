using System.Net.Sockets;

namespace Payroc.App.LoadBalancer;

public class HealthChecker : IHealthChecker
{
    private readonly IBackendPool _pool;
    private readonly CancellationTokenSource _cts = new();

    public HealthChecker(IBackendPool pool)
    {
        _pool = pool;
    }

    public Task StartAsync()
    {
        Logger.Info("Health checker started");
        return Task.Run(() => HealthCheckLoop(_cts.Token));
    }

    public void Stop()
    {
        _cts.Cancel();
        Logger.Info("Health checker stopped");
    }

    private async Task HealthCheckLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var backends = _pool.GetAllBackends();
                var tasks = backends.Select(backend => CheckBackendHealthAsync(backend, cancellationToken));
                await Task.WhenAll(tasks);

                await Task.Delay(
                    TimeSpan.FromSeconds(Config.HealthCheckIntervalSeconds),
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.Error($"Health check loop error: {ex.Message}");
            }
        }
    }

    private async Task CheckBackendHealthAsync(BackendServer backend, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Config.HealthCheckTimeoutSeconds));

            await client.ConnectAsync(backend.Host, backend.Port, cts.Token);
            _pool.UpdateBackendHealth(backend, true);
        }
        catch
        {
            _pool.UpdateBackendHealth(backend, false);
        }
    }
}