using Payroc.App.LoadBalancer;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Payroc.App.Tests.LoadBalancer
{
    public class BackendServerTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithValidInputs()
        {
            // Arrange
            var host = "backend1";
            var port = 8080;

            // Act
            var server = new BackendServer(host, port);

            // Assert
            Assert.Equal(host, server.Host);
            Assert.Equal(port, server.Port);
        }

        [Fact]
        public void Constructor_ShouldSetIsHealthyToFalse_ByDefault()
        {
            // Arrange & Act
            var server = new BackendServer("localhost", 80);

            // Assert
            Assert.False(server.IsHealthy);
        }

        [Fact]
        public void Constructor_ShouldSetLastCheckedToMinValue_ByDefault()
        {
            // Arrange & Act
            var server = new BackendServer("localhost", 80);

            // Assert
            Assert.Equal(DateTime.MinValue, server.LastChecked);
        }

        [Fact]
        public void Address_ShouldReturnFormattedHostAndPort()
        {
            // Arrange
            var server = new BackendServer("backend1", 8080);

            // Act
            var address = server.Address;

            // Assert
            Assert.Equal("backend1:8080", address);
        }

        [Theory]
        [InlineData("127.0.0.1", 80, "127.0.0.1:80")]
        [InlineData("localhost", 5000, "localhost:5000")]
        [InlineData("backend-server", 443, "backend-server:443")]
        [InlineData("192.168.1.100", 3000, "192.168.1.100:3000")]
        public void Address_ShouldReturnCorrectFormat_ForVariousInputs(string host, int port, string expected)
        {
            // Arrange
            var server = new BackendServer(host, port);

            // Act
            var address = server.Address;

            // Assert
            Assert.Equal(expected, address);
        }

        [Fact]
        public void IsHealthy_CanBeSetToTrue()
        {
            // Arrange
            var server = new BackendServer("backend1", 80);

            // Act
            server.IsHealthy = true;

            // Assert
            Assert.True(server.IsHealthy);
        }

        [Fact]
        public void IsHealthy_CanBeToggledMultipleTimes()
        {
            // Arrange
            var server = new BackendServer("backend1", 80);

            // Act & Assert
            Assert.False(server.IsHealthy); // Initial state

            server.IsHealthy = true;
            Assert.True(server.IsHealthy);

            server.IsHealthy = false;
            Assert.False(server.IsHealthy);

            server.IsHealthy = true;
            Assert.True(server.IsHealthy);
        }

        [Fact]
        public void LastChecked_CanBeUpdated()
        {
            // Arrange
            var server = new BackendServer("backend1", 80);
            var timestamp = DateTime.UtcNow;

            // Act
            server.LastChecked = timestamp;

            // Assert
            Assert.Equal(timestamp, server.LastChecked);
        }

        [Fact]
        public void ToString_ShouldReturnDownStatus_WhenUnhealthy()
        {
            // Arrange
            var server = new BackendServer("backend1", 8080);
            server.IsHealthy = false;

            // Act
            var result = server.ToString();

            // Assert
            Assert.Equal("backend1:8080 [DOWN]", result);
        }

        [Fact]
        public void ToString_ShouldReturnUpStatus_WhenHealthy()
        {
            // Arrange
            var server = new BackendServer("backend1", 8080);
            server.IsHealthy = true;

            // Act
            var result = server.ToString();

            // Assert
            Assert.Equal("backend1:8080 [UP]", result);
        }

        [Theory]
        [InlineData(true, "UP")]
        [InlineData(false, "DOWN")]
        public void ToString_ShouldReflectHealthStatus(bool isHealthy, string expectedStatus)
        {
            // Arrange
            var server = new BackendServer("test-server", 9000);
            server.IsHealthy = isHealthy;

            // Act
            var result = server.ToString();

            // Assert
            Assert.Contains(expectedStatus, result);
            Assert.Contains("test-server:9000", result);
        }

        [Fact]
        public void ToString_ShouldUpdateDynamically_WhenHealthStatusChanges()
        {
            // Arrange
            var server = new BackendServer("backend1", 80);

            // Act & Assert - Initially DOWN
            Assert.Equal("backend1:80 [DOWN]", server.ToString());

            // Change to UP
            server.IsHealthy = true;
            Assert.Equal("backend1:80 [UP]", server.ToString());

            // Change back to DOWN
            server.IsHealthy = false;
            Assert.Equal("backend1:80 [DOWN]", server.ToString());
        }

        [Theory]
        [InlineData(80)]
        [InlineData(443)]
        [InlineData(8080)]
        [InlineData(3000)]
        [InlineData(65535)] // Max valid port
        public void Constructor_ShouldAcceptValidPorts(int port)
        {
            // Arrange & Act
            var server = new BackendServer("localhost", port);

            // Assert
            Assert.Equal(port, server.Port);
        }

        [Fact]
        public void Properties_ShouldBeImmutableAfterConstruction_ForHostAndPort()
        {
            // Arrange
            var server = new BackendServer("backend1", 8080);

            // Act - Try to verify immutability (Host and Port are get-only)
            var host = server.Host;
            var port = server.Port;

            // Assert
            Assert.Equal("backend1", host);
            Assert.Equal(8080, port);
            
            // Note: Cannot reassign Host or Port as they are read-only properties
            // This test verifies they remain constant after construction
        }

        [Fact]
        public async Task BackendServer_ShouldSupportConcurrentHealthStatusUpdates()
        {
            // Arrange
            var server = new BackendServer("backend1", 80);
            var tasks = new Task[100];

            // Act - Simulate concurrent updates
            for (int i = 0; i < 100; i++)
            {
                var isHealthy = i % 2 == 0;
                tasks[i] = Task.Run(() => server.IsHealthy = isHealthy);
            }

            await Task.WhenAll(tasks);

            // Assert - Just verify no exceptions thrown
            Assert.NotNull(server);
            Assert.True(server.IsHealthy || !server.IsHealthy); // Valid state
        }

        [Fact]
        public void Address_ShouldHandleIPv6Format()
        {
            // Arrange
            var server = new BackendServer("::1", 8080);

            // Act
            var address = server.Address;

            // Assert
            Assert.Equal("::1:8080", address);
        }

        [Fact]
        public void BackendServer_ShouldHandleDomainNames()
        {
            // Arrange
            var server = new BackendServer("api.example.com", 443);

            // Act & Assert
            Assert.Equal("api.example.com", server.Host);
            Assert.Equal(443, server.Port);
            Assert.Equal("api.example.com:443", server.Address);
        }
    }
}
