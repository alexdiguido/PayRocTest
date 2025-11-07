using Payroc.App.LoadBalancer;
using Payroc.App.LoadBalancer.BackendSelectionStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Payroc.App.Tests.LoadBalancer
{
    public class BackendPoolTests
    {
        [Fact]
        public void Constructor_ShouldInitializeBackends()
        {
            // Arrange
            var backends = new List<(string Host, int Port)>
            {
                ("backend1", 80),
                ("backend2", 81),
                ("backend3", 82)
            };
            var strategy = new MockSelectionStrategy();

            // Act
            var pool = new BackendPool(backends, strategy);

            // Assert
            var allBackends = pool.GetAllBackends();
            Assert.Equal(3, allBackends.Count);
            Assert.Contains(allBackends, b => b.Host == "backend1" && b.Port == 80);
            Assert.Contains(allBackends, b => b.Host == "backend2" && b.Port == 81);
            Assert.Contains(allBackends, b => b.Host == "backend3" && b.Port == 82);
        }

        [Fact]
        public void HealthyCount_ShouldReturnZero_WhenNoBackendsAreHealthy()
        {
            // Arrange
            var pool = new BackendPool(new[] { ("backend1", 80), ("backend2", 81) }, new MockSelectionStrategy());

            // Act
            var healthyCount = pool.HealthyCount;

            // Assert
            Assert.Equal(0, healthyCount);
        }

        [Fact]
        public void UpdateBackendHealth_ShouldChangeHealthStatus()
        {
            // Arrange
            var pool = new BackendPool(new[] { ("backend1", 80) }, new MockSelectionStrategy());
            var backend = pool.GetAllBackends().First();

            // Act
            pool.UpdateBackendHealth(backend, true);

            // Assert
            Assert.True(backend.IsHealthy);
            Assert.Equal(1, pool.HealthyCount);
        }

        [Fact]
        public void GetNextHealthyBackend_ShouldReturnNull_WhenNoHealthyBackends()
        {
            // Arrange
            var pool = new BackendPool(new[] { ("backend1", 80), ("backend2", 81) }, new MockSelectionStrategy());

            // Act
            var backend = pool.GetNextHealthyBackend();

            // Assert
            Assert.Null(backend);
        }

        [Fact]
        public void GetNextHealthyBackend_ShouldReturnHealthyBackend_UsingStrategy()
        {
            // Arrange
            var pool = new BackendPool(new[] { ("backend1", 80), ("backend2", 81) }, new MockSelectionStrategy());
            var all = pool.GetAllBackends();
            pool.UpdateBackendHealth(all[1], true);

            // Act
            var backend = pool.GetNextHealthyBackend();

            // Assert
            Assert.NotNull(backend);
            Assert.Equal("backend1", backend!.Host);
        }

        [Fact]
        public void GetAllBackends_ShouldReturnSnapshot()
        {
            // Arrange
            var pool = new BackendPool(new[] { ("backend1", 80), ("backend2", 81) }, new MockSelectionStrategy());

            // Act
            var all = pool.GetAllBackends();

            // Assert
            Assert.Equal(2, all.Count);
            Assert.All(all, b => Assert.False(b.IsHealthy));
        }

        [Fact]
        public void UpdateBackendHealth_ShouldUpdateLastChecked()
        {
            // Arrange
            var pool = new BackendPool(new[] { ("backend1", 80) }, new MockSelectionStrategy());
            var backend = pool.GetAllBackends().First();
            var before = backend.LastChecked;

            // Act
            pool.UpdateBackendHealth(backend, true);
            var after = backend.LastChecked;

            // Assert
            Assert.True(after > before);
        }
    }

    public class MockSelectionStrategy : IBackendSelectionStrategy
    {
        public BackendServer? Select(IReadOnlyList<BackendServer> healthyBackends)
        {
            // Always select the first healthy backend for testing
            return healthyBackends.FirstOrDefault();
        }
    }
}
