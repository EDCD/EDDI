#nullable enable

using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Unit tests for ConnectionContext.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class ConnectionContextTests
    {
        private TcpClient? _testClient;
        private TcpClient? _acceptedClient;

        [ TestInitialize ]
        public async Task Setup ()
        {
            var listener = new TcpListener( IPAddress.Loopback, 0 );
            using var timeout = new CancellationTokenSource( TimeSpan.FromSeconds( 5 ) );
            try
            {
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                _testClient = new TcpClient();
                await _testClient.ConnectAsync( IPAddress.Loopback, port, timeout.Token );
                _acceptedClient = await listener.AcceptTcpClientAsync( timeout.Token );
            }
            catch
            {
                Cleanup();
                throw;
            }
            finally
            {
                listener.Stop();
            }
        }

        [ TestCleanup ]
        public void Cleanup ()
        {
            try { _testClient?.Dispose(); }
            finally { _acceptedClient?.Dispose(); }
        }

        [ TestMethod ]
        public void Constructor_CreatesUniqueSessionIds ()
        {
            // Arrange & Act
            using var context1 = new ConnectionContext( _testClient! );
            using var context2 = new ConnectionContext( _testClient! );

            // Assert
            Assert.IsNotNull( context1.SessionId );
            Assert.IsNotNull( context2.SessionId );
            Assert.AreNotEqual( context1.SessionId, context2.SessionId, "Session IDs should be unique" );
        }

        [ TestMethod ]
        public void SessionId_IsValidGuid ()
        {
            // Arrange & Act
            using var context = new ConnectionContext( _testClient! );

            // Assert
            Assert.IsTrue( Guid.TryParse( context.SessionId, out _ ), "Session ID should be a valid GUID" );
        }

        [ TestMethod ]
        public void EnqueueOutgoingMessage_AddsToQueue ()
        {
            // Arrange
            using var context = new ConnectionContext( _testClient! );
            var message = MessageEnvelope.Create( "Test", new DisconnectData { Reason = "user_shutdown" } );

            // Act
            context.EnqueueOutgoingMessage( message );

            // Assert
            Assert.IsTrue( context.HasOutgoingMessages(), "Queue should contain message after enqueue" );
            Assert.AreEqual( 1, context.OutgoingMessageCount, "Queue should have exactly 1 message" );
        }

        [ TestMethod ]
        public void DequeueOutgoingMessage_ReturnsInFifoOrder ()
        {
            // Arrange
            using var context = new ConnectionContext( _testClient! );
            var msg1 = MessageEnvelope.Create( "Test1", new DisconnectData { Reason = "user_shutdown" } );
            var msg2 = MessageEnvelope.Create( "Test2", new DisconnectData { Reason = "network_error" } );

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
        }

        [ TestMethod ]
        public void IsConnected_ReturnsTrueForValidConnection ()
        {
            // Arrange
            using var context = new ConnectionContext( _testClient! );

            // Act
            var isConnected = context.IsConnected;

            // Assert
            Assert.IsTrue( isConnected, "Should report connected for valid client" );
        }

        [ TestMethod ]
        public void OutgoingMessageCount_ReturnsCorrectCount ()
        {
            // Arrange
            using var context = new ConnectionContext( _testClient! );
            var msg = MessageEnvelope.Create( "Test", new DisconnectData { Reason = "user_shutdown" } );

            // Act & Assert
            Assert.AreEqual( 0, context.OutgoingMessageCount, "Initial count should be 0" );

            context.EnqueueOutgoingMessage( msg );
            Assert.AreEqual( 1, context.OutgoingMessageCount, "Count should be 1 after enqueue" );

            context.DequeueOutgoingMessage();
            Assert.AreEqual( 0, context.OutgoingMessageCount, "Count should be 0 after dequeue" );
        }

        [ TestMethod ]
        public void EnqueueOutgoingMessage_ThrowsOnNull ()
        {
            // Arrange
            using var context = new ConnectionContext( _testClient! );

            Assert.ThrowsExactly<ArgumentNullException>( () => context.EnqueueOutgoingMessage( null! ) );
        }

        [ TestMethod ]
        public void ClientCapabilities_CanBeSet ()
        {
            // Arrange
            using var context = new ConnectionContext( _testClient! );

            // Act
            context.ClientCapabilities.Add( "events" );
            context.ClientCapabilities.Add( "commands" );

            // Assert
            Assert.HasCount( 2, context.ClientCapabilities );
            Assert.Contains( "events", context.ClientCapabilities );
        }

        [ TestMethod ]
        public void IsAuthenticated_DefaultsFalse ()
        {
            // Arrange & Act
            using var context = new ConnectionContext( _testClient! );

            // Assert
            Assert.IsFalse( context.IsAuthenticated, "Should default to not authenticated" );
        }

        [ TestMethod ]
        public void Dispose_CanBeCalledMultipleTimes ()
        {
            // Arrange
            using var context = new ConnectionContext( _testClient! );

            // Act & Assert - should not throw
            context.Dispose();
            context.Dispose(); // Second dispose should be safe
        }
    }
}
