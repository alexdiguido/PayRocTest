using System.Net;
using System.Net.Sockets;

namespace Payroc.App.LoadBalancer;

/// <summary>
/// Main load balancer orchestrator
/// </summary>
public class TcpLoadBalancer : ILoadBalancer
{
    private readonly IBackendPool _pool;
    private readonly IHealthChecker _healthChecker;
    private readonly IConnectionHandler _connectionHandler;
    private readonly CancellationTokenSource _cts = new();
    private readonly Func<TcpListener> _listenerFactory;
    private TcpListener? _listener;

    public TcpLoadBalancer(
        IBackendPool pool,
        IHealthChecker healthChecker,
        IConnectionHandler connectionHandler,
        Func<TcpListener> listenerFactory)
    {
        _pool = pool;
        _healthChecker = healthChecker;
        _connectionHandler = connectionHandler;
        _listenerFactory = listenerFactory;
    }

    /// <summary>
    /// Starts the load balancer
    /// </summary>
    public async Task StartAsync()
    {
        Logger.Info("=== 1999 Load Balancer Starting ===");
        Logger.Info($"Listening on port {Config.ListenPort}");

        _ = _healthChecker.StartAsync();

        // Wait for initial health checks
        await Task.Delay(1000);

        Logger.Success($"Ready! {_pool.HealthyCount}/{Config.Backends.Count} backends healthy");

        _listener = _listenerFactory(); 
        _listener.Start();

        await AcceptConnectionsAsync(_cts.Token);
    }

    /// <summary>
    /// Stops the load balancer
    /// </summary>
    public void Stop()
    {
        Logger.Info("Shutting down...");
        _cts.Cancel();
        _listener?.Stop();
        _healthChecker.Stop();
        Logger.Success("Load balancer stopped");
    }

    /// <summary>
    /// Accepts and handles incoming connections
    /// </summary>
    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                _ = Task.Run(() => _connectionHandler.HandleClientAsync(client, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.Error($"Accept connection error: {ex.Message}");
            }
        }
    }
}