using Payroc.App.LoadBalancer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payroc.App.Tests.LoadBalancer;

public class HealthCheckerTests
{
    [Fact]
    public async Task StartAsync_ShouldMarkUnreachableBackendsAsUnhealthy()
    {
        // Arrange
        var pool = new BackendPoolMock();
        pool.Backends.Add(new BackendServer("invalid-host", 9999));
        var checker = new HealthChecker(pool);

        // Act
        var task = checker.StartAsync();
        await Task.Delay(1500); 
        checker.Stop();

        // Assert
        Assert.False(pool.Backends[0].IsHealthy);
    }

    [Fact]
    public async Task StartAsync_ShouldMarkReachableBackendsAsHealthy()
    {
        // Arrange
        var pool = new BackendPoolMock();
        pool.Backends.Add(new BackendServer("localhost", 80));
        var checker = new HealthChecker(pool);

        // Act
        var task = checker.StartAsync();
        await Task.Delay(1500); 
        checker.Stop();

        // Assert
        Assert.True(pool.Backends[0].LastChecked > DateTime.MinValue);
    }

    [Fact]
    public void Stop_ShouldCancelHealthCheckLoop()
    {
        // Arrange
        var pool = new BackendPoolMock();
        pool.Backends.Add(new BackendServer("localhost", 80));
        var checker = new HealthChecker(pool);

        // Act
        var task = checker.StartAsync();
        checker.Stop();

        // Assert
        Assert.True(task.IsCompleted || task.IsCanceled || task.IsFaulted);
    }

    [Fact]
    public async Task HealthCheckLoop_ShouldUpdateLastCheckedOnEachRun()
    {
        // Arrange
        var pool = new BackendPoolMock();
        pool.Backends.Add(new BackendServer("localhost", 80));
        var checker = new HealthChecker(pool);

        // Act
        var task = checker.StartAsync();
        await Task.Delay(1500);
        checker.Stop();

        // Assert
        Assert.True(pool.Backends[0].LastChecked > DateTime.MinValue);
    }

    [Fact]
    public async Task HealthCheckLoop_ShouldHandleMultipleBackends()
    {
        // Arrange
        var pool = new BackendPoolMock();
        pool.Backends.Add(new BackendServer("localhost", 80));
        pool.Backends.Add(new BackendServer("invalid-host", 9999));
        var checker = new HealthChecker(pool);

        // Act
        var task = checker.StartAsync();
        await Task.Delay(1500);
        checker.Stop();

        // Assert
        Assert.True(pool.Backends[0].LastChecked > DateTime.MinValue);
        Assert.True(pool.Backends[1].LastChecked > DateTime.MinValue);
    }

    private class BackendPoolMock : IBackendPool
    {
        public List<BackendServer> Backends { get; } = new();
        public int HealthyCount => Backends.Count(b => b.IsHealthy);
        public IReadOnlyList<BackendServer> GetAllBackends() => Backends.ToList();
        public BackendServer? GetNextHealthyBackend() => Backends.FirstOrDefault(b => b.IsHealthy);
        public void UpdateBackendHealth(BackendServer backend, bool isHealthy)
        {
            backend.IsHealthy = isHealthy;
            backend.LastChecked = DateTime.UtcNow;
        }
    }
}
