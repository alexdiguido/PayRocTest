using Payroc.App.LoadBalancer;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payroc.App.Tests.LoadBalancer;

public class TcpLoadBalancerTests
{
    [Fact]
    public async Task StartAsync_ShouldStartHealthCheckerAndLogReady()
    {
        // Arrange
        var pool = new BackendPoolMock { HealthyCount = 2 };
        var healthChecker = new HealthCheckerMock();
        var connectionHandler = new ConnectionHandlerMock();
        var listener = new TcpListener(IPAddress.Loopback, 0);
        var loadBalancer = new TcpLoadBalancer(pool, healthChecker, connectionHandler, () => listener);

        // Act
        var acceptTask = Task.Run(async () =>
        {
            await Task.Delay(500);
            loadBalancer.Stop();
        });

        await loadBalancer.StartAsync();

        // Assert
        Assert.True(healthChecker.Started);
        Assert.True(healthChecker.Stopped);
    }

    [Fact]
    public void Stop_ShouldCancelListenerAndStopHealthChecker()
    {
        // Arrange
        var pool = new BackendPoolMock();
        var healthChecker = new HealthCheckerMock();
        var connectionHandler = new ConnectionHandlerMock();
        var listener = new TcpListener(IPAddress.Loopback, 0);
        var loadBalancer = new TcpLoadBalancer(pool, healthChecker, connectionHandler, () => listener);

        // Act
        loadBalancer.Stop();

        // Assert
        Assert.True(healthChecker.Stopped);
    }

    [Fact]
    public async Task AcceptConnectionsAsync_ShouldHandleClientConnections()
    {
        // Arrange
        var pool = new BackendPoolMock { HealthyCount = 1 };
        var healthChecker = new HealthCheckerMock();
        var connectionHandler = new ConnectionHandlerMock();
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var loadBalancer = new TcpLoadBalancer(pool, healthChecker, connectionHandler, () => listener);

        // Act
        var client = new TcpClient();
        var connectTask = client.ConnectAsync("127.0.0.1", port);
        var acceptTask = Task.Run(async () =>
        {
            await loadBalancer.StartAsync();
        });

        await connectTask;
        await Task.Delay(500);

        // Wait for the connection to be handled
        var maxWaitMs = 2000;
        var waited = 0;
        while (!connectionHandler.Handled && waited < maxWaitMs)
        {
            await Task.Delay(50);
            waited += 50;
        }

        loadBalancer.Stop();

        // Assert
        Assert.True(connectionHandler.Handled);
    }
}