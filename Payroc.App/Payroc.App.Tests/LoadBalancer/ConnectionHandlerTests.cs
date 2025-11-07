using Payroc.App.LoadBalancer;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payroc.App.Tests.LoadBalancer
{
    public class ConnectionHandlerTests
    {
        [Fact]
        public async Task HandleClientAsync_ShouldWarnIfNoHealthyBackend()
        {
            // Arrange
            var pool = new BackendPoolMock();
            var handler = new ConnectionHandler(pool);
            var client = new TcpClient();

            // Act & Assert (no healthy backend)
            var ex = await Record.ExceptionAsync(() => handler.HandleClientAsync(client, default));
            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleClientAsync_ShouldLogAndDisposeOnException()
        {
            // Arrange
            var pool = new BackendPoolMock();
            pool.Backends.Add(new BackendServer("invalid-host", 9999) { IsHealthy = true });
            var handler = new ConnectionHandler(pool);
            var client = new TcpClient();

            // Act
            var ex = await Record.ExceptionAsync(() => handler.HandleClientAsync(client, default));

            // Assert
            Assert.Null(ex);
            Assert.False(client.Connected);
        }

        [Fact]
        public async Task HandleClientAsync_ShouldConnectToHealthyBackend()
        {
            // Arrange
            var pool = new BackendPoolMock();
            pool.Backends.Add(new BackendServer("localhost", 80) { IsHealthy = true });
            var handler = new ConnectionHandler(pool);
            var client = new TcpClient();

            // Act
            var ex = await Record.ExceptionAsync(() => handler.HandleClientAsync(client, default));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public async Task ProxyDataAsync_ShouldCompleteWhenOneSideCloses()
        {
            // Arrange
            var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;

            using var client = new TcpClient();
            await client.ConnectAsync("localhost", port);

            using var backend = listener.AcceptTcpClient();

            var cancellationToken = new CancellationTokenSource(100).Token;

            // Act & Assert
            var ex = await Record.ExceptionAsync(() =>
                typeof(ConnectionHandler)
                    .GetMethod("ProxyDataAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .Invoke(null, new object[] { client, backend, cancellationToken }) as Task
            );
            Assert.Null(ex);

            listener.Stop();
        }
    } 
}
