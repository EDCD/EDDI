using EddiIPC_Service.Messages;
using EddiIPC_Service.Messaging;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Unit tests for message serialization/deserialization.
    /// Uses TDD approach: tests define the contract before implementation.
    /// </summary>
    [ TestClass, TestCategory( "UnitTests" )]
    public class MessageSerializerTests
    {
        private const string TestMessageId = "12345678-1234-1234-1234-123456789012";

        [ TestMethod ]
        public void Serialize_HeartbeatMessage_ProducesLengthPrefixedJson ()
        {
            // Arrange
            var envelope = new MessageEnvelope
            {
                Type = "Heartbeat",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new HeartbeatData { Status = "alive", UptimeMs = 5000 }
            };

            // Act
            var serialized = MessageSerializer.Serialize( envelope );

            // Assert
            Assert.IsNotNull( serialized );
            Assert.Contains( '\n', serialized, "Should contain length prefix separator" );

            // Verify format: [LENGTH]\n[PAYLOAD]
            var parts = serialized.Split( '\n', 2 );
            Assert.HasCount( 2, parts, "Should have exactly 2 parts (length and payload)" );

            // Verify length prefix is valid
            Assert.IsTrue( int.TryParse( parts[ 0 ], out var length ), "First part should be integer" );
            Assert.IsGreaterThan( 0, length, "Length should be positive" );

            // Verify payload matches declared length
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes( parts[ 1 ] );
            Assert.HasCount( length, payloadBytes, "Payload byte count should match declared length" );
        }

        [ TestMethod ]
        public void Deserialize_ValidLengthPrefixedMessage_ReturnsMessageEnvelope ()
        {
            // Arrange
            var json = JsonConvert.SerializeObject( new MessageEnvelope
            {
                Type = "Heartbeat",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new HeartbeatData { Status = "alive", UptimeMs = 5000 }
            } );
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes( json );
            var serialized = $"{jsonBytes.Length}\n{json}";

            // Act
            var deserialized = MessageSerializer.Deserialize( serialized );

            // Assert
            Assert.IsNotNull( deserialized );
            Assert.AreEqual( "Heartbeat", deserialized.Type );
            Assert.AreEqual( TestMessageId, deserialized.Id );
        }

        [ TestMethod ]
        public void Serialize_Deserialize_RoundTrip_PreservesMessage ()
        {
            // Arrange
            var original = new MessageEnvelope
            {
                Type = "Connect",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new ConnectData
                {
                    PluginVersion = "5.0.0",
                    PluginName = "EDDI VoiceAttack Plugin",
                    Capabilities = [ "events", "commands" ],
                    SupportedMessageTypes = [ "Connect", "Heartbeat", "Disconnect" ]
                }
            };

            // Act
            var serialized = MessageSerializer.Serialize( original );
            var deserialized = MessageSerializer.Deserialize( serialized );

            // Assert
            Assert.AreEqual( original.Type, deserialized.Type );
            Assert.AreEqual( original.Id, deserialized.Id );
            Assert.AreEqual( original.Timestamp, deserialized.Timestamp );
        }

        [ TestMethod ]
        public void Deserialize_MissingLengthPrefix_ThrowsException ()
        {
            // Arrange
            const string invalidMessage = "{\"type\":\"Heartbeat\"}"; // No length prefix

            // Act & Assert
            try
            {
                MessageSerializer.Deserialize( invalidMessage );
                Assert.Fail( "Expected ArgumentException" );
            }
            catch ( ArgumentException )
            {
                // Expected
            }
        }

        [ TestMethod ]
        public void Deserialize_InvalidLengthPrefix_ThrowsException ()
        {
            // Arrange
            const string invalidMessage = "abc\n{\"type\":\"Heartbeat\"}"; // Non-numeric length

            // Act & Assert
            try
            {
                MessageSerializer.Deserialize( invalidMessage );
                Assert.Fail( "Expected ArgumentException" );
            }
            catch ( ArgumentException )
            {
                // Expected
            }
        }

        [ TestMethod ]
        public void Deserialize_MalformedJson_ThrowsException ()
        {
            // Arrange
            const string json = "{invalid json}";
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes( json );
            var malformedMessage = $"{jsonBytes.Length}\n{json}";

            // Act & Assert
            try
            {
                MessageSerializer.Deserialize( malformedMessage );
                Assert.Fail( "Expected ArgumentException" );
            }
            catch ( ArgumentException )
            {
                // Expected
            }
        }

        [ TestMethod ]
        public void Deserialize_MissingRequiredField_ThrowsException ()
        {
            // Arrange
            var json = JsonConvert.SerializeObject( new
            {
                Type = "Heartbeat"
            } ); // Missing Id, Timestamp, Version, Data
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes( json );
            var incompleteMessage = $"{jsonBytes.Length}\n{json}";

            // Act & Assert
            try
            {
                MessageSerializer.Deserialize( incompleteMessage );
                Assert.Fail( "Expected ArgumentException" );
            }
            catch ( ArgumentException )
            {
                // Expected
            }
        }

        [ TestMethod ]
        public void Serialize_EventMessage_IncludesEventData ()
        {
            // Arrange
            var envelope = new MessageEnvelope
            {
                Type = "Event",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new EventData
                {
                    EventType = "LocationChanged",
                    EventName = "Location",
                    EventPayload =
                        new Dictionary<string, object> { { "system", "Sol" }, { "station", "Starport" } }
                }
            };

            // Act
            var serialized = MessageSerializer.Serialize( envelope );

            // Assert
            Assert.Contains( "LocationChanged", serialized );
            Assert.Contains( "Sol", serialized );
        }

        [ TestMethod ]
        public void Serialize_CommandMessage_IncludesCommandData ()
        {
            // Arrange
            var envelope = new MessageEnvelope
            {
                Type = "Command",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new CommandData
                {
                    Command = "enable_monitor",
                    Target = "Journal Monitor",
                    Parameters = new Dictionary<string, object> { { "debug", true } }
                }
            };

            // Act
            var serialized = MessageSerializer.Serialize( envelope );
            var deserialized = MessageSerializer.Deserialize( serialized );

            // Assert
            Assert.AreEqual( "Command", deserialized.Type );
            Assert.IsNotNull( deserialized.Data );
        }

        [ TestMethod ]
        public void Serialize_EmptyDataField_StillValid ()
        {
            // Arrange
            var envelope = new MessageEnvelope
            {
                Type = "Disconnect",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new DisconnectData { Reason = "user_shutdown", Message = "User requested disconnect" }
            };

            // Act
            var serialized = MessageSerializer.Serialize( envelope );
            var deserialized = MessageSerializer.Deserialize( serialized );

            // Assert
            Assert.AreEqual( "Disconnect", deserialized.Type );
            Assert.IsNotNull( deserialized.Data );
        }

        [ TestMethod ]
        public void Deserialize_UtfEncodedPayload_HandlesCorrectly ()
        {
            // Arrange - Test with non-ASCII characters
            var json = JsonConvert.SerializeObject( new MessageEnvelope
            {
                Type = "Event",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new EventData
                {
                    EventType = "Test",
                    EventName = "Test Event with émojis 🚀",
                    EventPayload = new Dictionary<string, object>()
                }
            } );
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes( json );
            var serialized = $"{jsonBytes.Length}\n{json}";

            // Act
            var deserialized = MessageSerializer.Deserialize( serialized );

            // Assert
            Assert.IsNotNull( deserialized );
            Assert.AreEqual( TestMessageId, deserialized.Id );
        }

        [ TestMethod ]
        public void Deserialize_PayloadLengthMismatch_ThrowsException ()
        {
            // Arrange - Declare wrong length
            const string json = "{\"type\":\"Heartbeat\"}";
            const int wrongLength = 999; // Actual payload is much shorter
            var malformedMessage = $"{wrongLength}\n{json}";

            // Act & Assert - Should throw when byte count doesn't match
            try
            {
                MessageSerializer.Deserialize( malformedMessage );
                Assert.Fail( "Expected ArgumentException" );
            }
            catch ( ArgumentException )
            {
                // Expected
            }
        }

        [ TestMethod ]
        public void Serialize_MultipleMessages_EachHasCorrectLength ()
        {
            // Arrange
            var messages = new List<MessageEnvelope>
            {
                MessageEnvelope.Create( "Heartbeat", new HeartbeatData { Status = "alive", UptimeMs = 1000 } ),
                MessageEnvelope.Create( "Connect",
                    new ConnectData
                    {
                        PluginVersion = "5.0.0",
                        PluginName = "Plugin",
                        Capabilities = [ "events" ],
                        SupportedMessageTypes = [ "Heartbeat" ]
                    } ),
                MessageEnvelope.Create( "Disconnect", new DisconnectData { Reason = "shutdown" } )
            };

            // Act & Assert
            foreach ( var msg in messages )
            {
                var serialized = MessageSerializer.Serialize( msg );
                var parts = serialized.Split( '\n', 2 );
                var declaredLength = int.Parse( parts[ 0 ] );
                var actualLength = System.Text.Encoding.UTF8.GetByteCount( parts[ 1 ] );
                Assert.AreEqual( declaredLength, actualLength, $"Length mismatch for {msg.Type}" );
            }
        }
    }
}
