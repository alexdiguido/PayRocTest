using Payroc.App.LoadBalancer;
using Payroc.App.LoadBalancer.BackendSelectionStrategies;
using System.Net.Sockets;

Console.WriteLine("Hello, World!");

// 1999 Load Balancer - Keep it simple!
var poolSelectionStrategy = new RoundRobinBackendSelectionStrategy();
var backendPool = new BackendPool(Config.Backends, poolSelectionStrategy);
var healthChecker = new HealthChecker(backendPool);
var connectionHandler = new ConnectionHandler(backendPool);
Func<TcpListener> listenerFactory = () => new TcpListener(System.Net.IPAddress.Any, Config.ListenPort);

var loadBalancer = new TcpLoadBalancer(backendPool, healthChecker, connectionHandler, listenerFactory);

// Handle Ctrl+C gracefully
Console.CancelKeyPress += (sender, args) =>
{
    args.Cancel = true;
    loadBalancer.Stop();
};

try
{
    await loadBalancer.StartAsync();
}
catch (Exception ex)
{
    Logger.Error($"Fatal error: {ex.Message}");
    return 1;
}

return 0;
