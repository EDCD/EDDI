#nullable enable

using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackService.Server;
using EddiVoiceAttackService.Messages;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Unit tests for ConnectionContext.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class ConnectionContextTests
    {
        private TcpClient? _testClient;

        [ TestInitialize ]
        public void Setup ()
        {
            // Create a dummy TCP client for testing
            // We'll create a listening socket to accept a connection
            var listener = new TcpListener( IPAddress.Loopback, 0 );
            listener.Start();
            var port = ( (IPEndPoint)listener.LocalEndpoint ).Port;

            listener.AcceptTcpClientAsync();
            _testClient = new TcpClient();
            _testClient.Connect( IPAddress.Loopback, port );

            listener.Stop();
        }

        [ TestCleanup ]
        public void Cleanup ()
        {
            _testClient?.Dispose();
        }

        [ TestMethod ]
        public void Constructor_CreatesUniqueSessionIds ()
        {
            // Arrange & Act
            var context1 = new ConnectionContext( _testClient! );
            var context2 = new ConnectionContext( _testClient! );

            // Assert
            Assert.IsNotNull( context1.SessionId );
            Assert.IsNotNull( context2.SessionId );
            Assert.AreNotEqual( context1.SessionId, context2.SessionId, "Session IDs should be unique" );

            context1.Dispose();
            context2.Dispose();
        }

        [ TestMethod ]
        public void SessionId_IsValidGuid ()
        {
            // Arrange & Act
            var context = new ConnectionContext( _testClient! );

            // Assert
            Assert.IsTrue( Guid.TryParse( context.SessionId, out _ ), "Session ID should be a valid GUID" );

            context.Dispose();
        }

        [ TestMethod ]
        public void EnqueueOutgoingMessage_AddsToQueue ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );
            var message = MessageEnvelope.Create( "Test", new HeartbeatData { Status = "alive", UptimeMs = 0 } );

            // Act
            context.EnqueueOutgoingMessage( message );

            // Assert
            Assert.IsTrue( context.HasOutgoingMessages(), "Queue should contain message after enqueue" );
            Assert.AreEqual( 1, context.OutgoingMessageCount, "Queue should have exactly 1 message" );

            context.Dispose();
        }

        [ TestMethod ]
        public void DequeueOutgoingMessage_ReturnsInFifoOrder ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );
            var msg1 = MessageEnvelope.Create( "Test1", new HeartbeatData { Status = "alive", UptimeMs = 1 } );
            var msg2 = MessageEnvelope.Create( "Test2", new HeartbeatData { Status = "alive", UptimeMs = 2 } );

            context.EnqueueOutgoingMessage( msg1 );
            context.EnqueueOutgoingMessage( msg2 );

            // Act
            var dequeued1 = context.DequeueOutgoingMessage();
            var dequeued2 = context.DequeueOutgoingMessage();
            var dequeued3 = context.DequeueOutgoingMessage();

            // Assert
            Assert.AreEqual( msg1.Id, dequeued1?.Id, "First dequeued message should be msg1" );
            Assert.AreEqual( msg2.Id, dequeued2?.Id, "Second dequeued message should be msg2" );
            Assert.IsNull( dequeued3, "Third dequeue should return null" );

            context.Dispose();
        }

        [ TestMethod ]
        public void IsHeartbeatTimedOut_DetectsTimeout ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );

            // Act - Set heartbeat to old timestamp
            context.LastHeartbeatUtc = DateTime.UtcNow.AddSeconds( -15 );

            // Assert
            Assert.IsTrue( context.IsHeartbeatTimedOut( 10 ), "Should detect timeout after 10 seconds" );
            Assert.IsFalse( context.IsHeartbeatTimedOut( 20 ), "Should not timeout within 20 seconds" );

            context.Dispose();
        }

        [ TestMethod ]
        public void IsHeartbeatTimedOut_ReturnsRecentTimestamp ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );

            // Act - Heartbeat is recent (just created)
            var isTimedOut = context.IsHeartbeatTimedOut( 10 );

            // Assert
            Assert.IsFalse( isTimedOut, "Recent heartbeat should not timeout" );

            context.Dispose();
        }

        [ TestMethod ]
        public void IsConnected_ReturnsTrueForValidConnection ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );

            // Act
            var isConnected = context.IsConnected;

            // Assert
            Assert.IsTrue( isConnected, "Should report connected for valid client" );

            context.Dispose();
        }

        [ TestMethod ]
        public void OutgoingMessageCount_ReturnsCorrectCount ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );
            var msg = MessageEnvelope.Create( "Test", new HeartbeatData { Status = "alive", UptimeMs = 0 } );

            // Act & Assert
            Assert.AreEqual( 0, context.OutgoingMessageCount, "Initial count should be 0" );

            context.EnqueueOutgoingMessage( msg );
            Assert.AreEqual( 1, context.OutgoingMessageCount, "Count should be 1 after enqueue" );

            context.DequeueOutgoingMessage();
            Assert.AreEqual( 0, context.OutgoingMessageCount, "Count should be 0 after dequeue" );

            context.Dispose();
        }

        [ TestMethod ]
        public void EnqueueOutgoingMessage_ThrowsOnNull ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );

            // Act & Assert
            try
            {
                context.EnqueueOutgoingMessage( null! );
                Assert.Fail( "Expected ArgumentNullException" );
            }
            catch ( ArgumentNullException )
            {
                // Expected
            }
            finally
            {
                context.Dispose();
            }
        }

        [ TestMethod ]
        public void ClientCapabilities_CanBeSet ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );

            // Act
            context.ClientCapabilities.Add( "events" );
            context.ClientCapabilities.Add( "commands" );

            // Assert
            Assert.HasCount( 2, context.ClientCapabilities );
            Assert.Contains( "events", context.ClientCapabilities );

            context.Dispose();
        }

        [ TestMethod ]
        public void IsAuthenticated_DefaultsFalse ()
        {
            // Arrange & Act
            var context = new ConnectionContext( _testClient! );

            // Assert
            Assert.IsFalse( context.IsAuthenticated, "Should default to not authenticated" );

            context.Dispose();
        }

        [ TestMethod ]
        public void Dispose_CanBeCalledMultipleTimes ()
        {
            // Arrange
            var context = new ConnectionContext( _testClient! );

            // Act & Assert - should not throw
            context.Dispose();
            context.Dispose(); // Second dispose should be safe
        }
    }
}
