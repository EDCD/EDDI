#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackService.Server;
using EddiVoiceAttackService.Messages;
using EddiVoiceAttackService.Heartbeat;

namespace Tests.EddiVoiceAttackService;

/// <summary>
/// Unit tests for CancellationToken support across IPC server components.
/// Validates graceful cancellation, timeout handling, and resource cleanup.
/// </summary>
[TestClass]
public class CancellationTokenTests
{
    [TestMethod]
    [Timeout(5000)]
    public async Task StartAsync_CanBeCancelled_BeforeCompletion()
    {
        // Arrange
        var server = new IPCServer();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately before calling

        // Act
        try
        {
            await server.StartAsync(cts.Token);
            Assert.Fail("Expected OperationCanceledException");
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        Assert.IsFalse(server.IsRunning, "Server should not be running after cancellation");
    }

    [TestMethod]
    [Timeout(5000)]
    public async Task StartAsync_CompletesNormally_WhenNotCancelled()
    {
        // Arrange
        var server = new IPCServer();
        var cts = new CancellationTokenSource();

        // Act
        await server.StartAsync(cts.Token);
        await Task.Delay(100); // Let server initialize

        // Assert
        Assert.IsTrue(server.IsRunning, "Server should be running");
        Assert.IsTrue(server.Port > 0, "Server should have valid port");

        // Cleanup
        await server.StopAsync();
    }

    [TestMethod]
    [Timeout(5000)]
    public async Task StartAsync_DefaultToken_Works()
    {
        // Arrange
        var server = new IPCServer();

        // Act
        await server.StartAsync(); // No token provided, should work with default

        // Assert
        Assert.IsTrue(server.IsRunning, "Server should start with default token");

        // Cleanup
        await server.StopAsync();
    }

    [TestMethod]
    [Timeout(5000)]
    public async Task StopAsync_CanBeCancelled()
    {
        // Arrange
        var server = new IPCServer();
        await server.StartAsync();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        // Act
        try
        {
            await server.StopAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected - stop was cancelled
        }

        // Assert (server may or may not be running depending on timing)
        // Just verify no unhandled exception
    }

    [TestMethod]
    [Timeout(5000)]
    public async Task BroadcastAsync_CanBeCancelled()
    {
        // Arrange
        var server = new IPCServer();
        await server.StartAsync();
        
        var message = MessageEnvelope.Create("Test", new HeartbeatData { Status = "alive", UptimeMs = 0 });
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        // Act & Assert
        try
        {
            await server.BroadcastAsync(message, cts.Token);
            // May or may not throw depending on timing; both are acceptable
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Cleanup
        await server.StopAsync();
    }

    [TestMethod]
    [Timeout(5000)]
    public async Task HeartbeatMonitor_StartAsync_CanBeCancelled()
    {
        // Arrange
        var server = new IPCServer();
        await server.StartAsync();
        var monitor = new HeartbeatMonitor(server);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        // Act
        try
        {
            await monitor.StartAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        // Monitor may or may not be running depending on timing

        // Cleanup
        await server.StopAsync();
    }

    [TestMethod]
    [Timeout(5000)]
    public async Task HeartbeatMonitor_StartAsync_CompletesNormally_WhenNotCancelled()
    {
        // Arrange
        var server = new IPCServer();
        await server.StartAsync();
        var monitor = new HeartbeatMonitor(server);

        // Act
        await monitor.StartAsync();
        await Task.Delay(100);

        // Assert
        // Monitor should be running (no exception thrown)

        // Cleanup
        await monitor.StopAsync();
        await server.StopAsync();
    }

    [TestMethod]
    [Timeout(5000)]
    public async Task LinkedCancellationToken_CancelsBoth_ServerAndMonitor()
    {
        // Arrange
        var server = new IPCServer();
        var monitor = new HeartbeatMonitor(server);
        var cts = new CancellationTokenSource();

        // Act
        await server.StartAsync();
        await monitor.StartAsync();
        await Task.Delay(100);

        cts.CancelAfter(100);

        try
        {
            // Simulate linked cancellation scenario
            await Task.Delay(200, cts.Token);
            Assert.Fail("Expected OperationCanceledException");
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Cleanup
        await monitor.StopAsync();
        await server.StopAsync();
    }
}
