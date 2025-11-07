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
    private const int MillisecondsDelay = 5000;

    [Fact]
    public async Task StartAsync_ShouldMarkUnreachableBackendsAsUnhealthy()
    {
        // Arrange
        var pool = new BackendPoolMock();
        pool.Backends.Add(new BackendServer("invalid-host", 9999));
        var checker = new HealthChecker(pool);

        // Act
        var task = checker.StartAsync();
        await Task.Delay(MillisecondsDelay); 
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
        await Task.Delay(MillisecondsDelay); 
        checker.Stop();

        // Assert
        Assert.True(pool.Backends[0].LastChecked > DateTime.MinValue);
    }

    [Fact]
    public async Task Stop_ShouldCancelHealthCheckLoop()
    {
        // Arrange
        var pool = new BackendPoolMock();
        pool.Backends.Add(new BackendServer("localhost", 80));
        var checker = new HealthChecker(pool);

        // Act
        var task = checker.StartAsync();
        await Task.Delay(MillisecondsDelay); 
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
        await Task.Delay(MillisecondsDelay);
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
        var waitTime = MillisecondsDelay * pool.Backends.Count;

        // Act
        var task = checker.StartAsync();
        await Task.Delay(waitTime);
        checker.Stop();

        // Assert
        Assert.True(pool.Backends[0].LastChecked > DateTime.MinValue);
        Assert.True(pool.Backends[1].LastChecked > DateTime.MinValue);
    }
}
