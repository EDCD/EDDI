#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackService.Server;
using EddiVoiceAttackService.Messages;

namespace Tests.EddiVoiceAttackService;

/// <summary>
/// Unit tests for MessageRouter.
/// </summary>
[TestClass]
public class MessageRouterTests
{
    private TcpClient? _testClient;
    private ConnectionContext? _testContext;

    [TestInitialize]
    public void Setup()
    {
        // Create a dummy TCP client for testing
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var task = listener.AcceptTcpClientAsync();
        _testClient = new TcpClient();
        _testClient.Connect(IPAddress.Loopback, port);
        
        listener.Stop();
        _testContext = new ConnectionContext(_testClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _testContext?.Dispose();
        _testClient?.Dispose();
    }

    [TestMethod]
    public void RegisterHandler_AddsHandlerSuccessfully()
    {
        // Arrange
        var router = new MessageRouter();

        async Task TestHandler(MessageEnvelope msg, ConnectionContext ctx)
        {
            await Task.CompletedTask;
        }

        // Act
        router.RegisterHandler("Test", TestHandler);

        // Assert
        Assert.IsTrue(router.HasHandlers("Test"), "Handler should be registered");
        Assert.AreEqual(1, router.GetHandlerCount("Test"), "Should have 1 handler");
    }

    [TestMethod]
    public void RegisterHandler_SupportsMultipleHandlersPerType()
    {
        // Arrange
        var router = new MessageRouter();

        async Task Handler1(MessageEnvelope msg, ConnectionContext ctx) => await Task.CompletedTask;
        async Task Handler2(MessageEnvelope msg, ConnectionContext ctx) => await Task.CompletedTask;

        // Act
        router.RegisterHandler("Test", Handler1);
        router.RegisterHandler("Test", Handler2);

        // Assert
        Assert.AreEqual(2, router.GetHandlerCount("Test"), "Should support 2 handlers for same type");
    }

    [TestMethod]
    public async Task RouteAsync_CallsRegisteredHandler()
    {
        // Arrange
        var router = new MessageRouter();
        var handlerCalled = false;
        var receivedMessage = (MessageEnvelope?)null;
        var receivedContext = (ConnectionContext?)null;

        async Task TestHandler(MessageEnvelope msg, ConnectionContext ctx)
        {
            handlerCalled = true;
            receivedMessage = msg;
            receivedContext = ctx;
            await Task.CompletedTask;
        }

        router.RegisterHandler("Heartbeat", TestHandler);
        var message = MessageEnvelope.Create("Heartbeat", new HeartbeatData { Status = "alive", UptimeMs = 0 });

        // Act
        await router.RouteAsync(message, _testContext!);

        // Assert
        Assert.IsTrue(handlerCalled, "Handler should be called");
        Assert.AreEqual(message.Id, receivedMessage?.Id, "Should pass correct message");
        Assert.AreEqual(_testContext!.SessionId, receivedContext?.SessionId, "Should pass correct context");
    }

    [TestMethod]
    public async Task RouteAsync_ExecutesMultipleHandlersSequentially()
    {
        // Arrange
        var router = new MessageRouter();
        var executionOrder = new List<int>();

        async Task Handler1(MessageEnvelope msg, ConnectionContext ctx)
        {
            executionOrder.Add(1);
            await Task.CompletedTask;
        }

        async Task Handler2(MessageEnvelope msg, ConnectionContext ctx)
        {
            executionOrder.Add(2);
            await Task.CompletedTask;
        }

        router.RegisterHandler("Test", Handler1);
        router.RegisterHandler("Test", Handler2);
        var message = MessageEnvelope.Create("Test", new HeartbeatData { Status = "alive", UptimeMs = 0 });

        // Act
        await router.RouteAsync(message, _testContext!);

        // Assert
        Assert.HasCount(2, executionOrder, "Both handlers should execute");
        Assert.AreEqual(1, executionOrder[0], "First handler should execute first");
        Assert.AreEqual(2, executionOrder[1], "Second handler should execute second");
    }

    [TestMethod]
    public async Task RouteAsync_ContinuesOnHandlerError()
    {
        // Arrange
        var router = new MessageRouter();
        var handler2Called = false;

        async Task FailingHandler(MessageEnvelope msg, ConnectionContext ctx)
        {
            throw new InvalidOperationException("Test error");
        }

        async Task SuccessHandler(MessageEnvelope msg, ConnectionContext ctx)
        {
            handler2Called = true;
            await Task.CompletedTask;
        }

        router.RegisterHandler("Test", FailingHandler);
        router.RegisterHandler("Test", SuccessHandler);
        var message = MessageEnvelope.Create("Test", new HeartbeatData { Status = "alive", UptimeMs = 0 });

        // Act - should not throw
        await router.RouteAsync(message, _testContext!);

        // Assert
        Assert.IsTrue(handler2Called, "Second handler should execute even if first throws");
    }

    [TestMethod]
    public async Task RouteAsync_DoesNotThrowWhenNoHandlersRegistered()
    {
        // Arrange
        var router = new MessageRouter();
        var message = MessageEnvelope.Create("Unhandled", new HeartbeatData { Status = "alive", UptimeMs = 0 });

        // Act & Assert - should not throw
        await router.RouteAsync(message, _testContext!);
    }

    [TestMethod]
    public void UnregisterHandlers_RemovesAllHandlers()
    {
        // Arrange
        var router = new MessageRouter();
        
        async Task Handler(MessageEnvelope msg, ConnectionContext ctx) => await Task.CompletedTask;

        router.RegisterHandler("Test", Handler);
        router.RegisterHandler("Test", Handler);

        // Act
        router.UnregisterHandlers("Test");

        // Assert
        Assert.IsFalse(router.HasHandlers("Test"), "Handlers should be unregistered");
        Assert.AreEqual(0, router.GetHandlerCount("Test"), "Handler count should be 0");
    }

    [TestMethod]
    public void HasHandlers_ReturnsFalseWhenNotRegistered()
    {
        // Arrange
        var router = new MessageRouter();

        // Act & Assert
        Assert.IsFalse(router.HasHandlers("Unregistered"), "Should return false for unregistered type");
    }

    [TestMethod]
    public void ClearAllHandlers_RemovesAllHandlers()
    {
        // Arrange
        var router = new MessageRouter();
        
        async Task Handler(MessageEnvelope msg, ConnectionContext ctx) => await Task.CompletedTask;

        router.RegisterHandler("Type1", Handler);
        router.RegisterHandler("Type2", Handler);
        router.RegisterHandler("Type3", Handler);

        // Act
        router.ClearAllHandlers();

        // Assert
        Assert.IsFalse(router.HasHandlers("Type1"), "Type1 handlers should be cleared");
        Assert.IsFalse(router.HasHandlers("Type2"), "Type2 handlers should be cleared");
        Assert.IsFalse(router.HasHandlers("Type3"), "Type3 handlers should be cleared");
    }

    [TestMethod]
    public void RegisterHandler_ThrowsOnNullMessageType()
    {
        // Arrange
        var router = new MessageRouter();
        async Task Handler(MessageEnvelope msg, ConnectionContext ctx) => await Task.CompletedTask;

        // Act & Assert
        try
        {
            router.RegisterHandler(null!, Handler);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void RegisterHandler_ThrowsOnNullHandler()
    {
        // Arrange
        var router = new MessageRouter();

        // Act & Assert
        try
        {
            router.RegisterHandler("Test", null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task RouteAsync_ThrowsOnNullMessage()
    {
        // Arrange
        var router = new MessageRouter();

        // Act & Assert
        try
        {
            await router.RouteAsync(null!, _testContext!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task RouteAsync_ThrowsOnNullContext()
    {
        // Arrange
        var router = new MessageRouter();
        var message = MessageEnvelope.Create("Test", new HeartbeatData { Status = "alive", UptimeMs = 0 });

        // Act & Assert
        try
        {
            await router.RouteAsync(message, null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }
}
