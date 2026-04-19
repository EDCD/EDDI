using EddiIPC_Service.Messages;
using EddiIPC_Service.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public TestContext TestContext { get; set; }

        [TestMethod ]
        public void Serialize_DisconnectMessage_ProducesLengthPrefixedJson ()
        {
            // Arrange
            var envelope = new MessageEnvelope
            {
                Type = MessageTypes.Disconnect,
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new DisconnectData { Reason = "user_shutdown" }
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
                Type = MessageTypes.Disconnect,
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new DisconnectData { Reason = "user_shutdown" }
            } );
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes( json );
            var serialized = $"{jsonBytes.Length}\n{json}";

            // Act
            var deserialized = MessageSerializer.Deserialize( serialized );

            // Assert
            Assert.IsNotNull( deserialized );
            Assert.AreEqual( MessageTypes.Disconnect, deserialized.Type );
            Assert.AreEqual( TestMessageId, deserialized.Id );
        }

        [ TestMethod ]
        public void Serialize_Deserialize_RoundTrip_PreservesMessage ()
        {
            // Arrange
            var original = new MessageEnvelope
            {
                Type = MessageTypes.Connect,
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new ConnectData
                {
                    PluginVersion = "5.0.0",
                    PluginName = "EDDI VoiceAttack Plugin",
                    Capabilities = ServerCapabilities.AllCapabilities,
                    SupportedMessageTypes = ServerCapabilities.AllMessageTypes
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
            const string invalidMessage = "{\"type\":\"Disconnect\"}"; // No length prefix

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
            const string invalidMessage = "abc\n{\"type\":\"Disconnect\"}"; // Non-numeric length

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
                Type = MessageTypes.Disconnect
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
        public void Serialize_EventMessage_WithModifyingCollection_DoesNotThrow ()
        {
            // Arrange: Create a mutable dictionary that simulates concurrent modification
            var payload = new Dictionary<string, object> { { "key1", "value1" } };
            var envelope = new MessageEnvelope
            {
                Type = "Event",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new EventData
                {
                    EventType = "TestEvent",
                    EventName = "Test",
                    EventPayload = payload
                }
            };

            // Act: Modify the dictionary in a separate task while serializing
            var serializationTask = Task.Run( () =>
            {
                // Serialize multiple times to increase chance of collision
                for ( var i = 0; i < 10; i++ )
                {
                    var serialized = MessageSerializer.Serialize( envelope );
                    Assert.IsNotNull( serialized );
                }
            }, TestContext.CancellationToken );

            var modificationTask = Task.Run( () =>
            {
                // Modify the dictionary while serialization is happening
                for ( var i = 0; i < 100; i++ )
                {
                    payload[ $"key{i}" ] = $"value{i}";
                    if ( (i % 10) == 0 )
                    {
                        Task.Delay( 1, TestContext.CancellationToken ).Wait(TestContext.CancellationToken);  // Brief pause to increase race condition likelihood
                    }
                }
            }, TestContext.CancellationToken );

            // Assert: Both tasks should complete without exception
            try
            {
                Task.WaitAll( [ serializationTask, modificationTask ], TimeSpan.FromSeconds( 5 ) );
                Assert.IsTrue( serializationTask.IsCompletedSuccessfully );
                Assert.IsTrue( modificationTask.IsCompletedSuccessfully );
            }
            catch ( AggregateException ex )
            {
                Assert.Fail( $"Serialization failed during concurrent modification: {ex.InnerException?.Message}" );
            }
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
            const string json = "{\"type\":\"Disconnect\"}";
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
                MessageEnvelope.Create( MessageTypes.Disconnect, new DisconnectData { Reason = "user_shutdown" } ),
                MessageEnvelope.Create( MessageTypes.Connect,
                    new ConnectData
                    {
                        PluginVersion = "5.0.0",
                        PluginName = "Plugin",
                        Capabilities = [ "events" ],
                        SupportedMessageTypes = [ MessageTypes.Disconnect ]
                    } ),
                MessageEnvelope.Create( MessageTypes.Disconnect, new DisconnectData { Reason = "shutdown" } )
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

        [ TestMethod ]
        public void DeserializeMessages_Utf8PayloadSplitAcrossBuffers_ReconstructsMessage ()
        {
            // Arrange
            var envelope = new MessageEnvelope
            {
                Type = "Event",
                Timestamp = "2025-01-20T15:30:45.123Z",
                Id = TestMessageId,
                Data = new EventData
                {
                    EventType = "Test",
                    EventName = "Plugin reconnect 🚀",
                    EventPayload = new Dictionary<string, object>
                    {
                        { "host", "EDDI 🚀" },
                        { "state", "replacement" }
                    }
                }
            };
            var serialized = MessageSerializer.Serialize( envelope );
            var serializedBytes = System.Text.Encoding.UTF8.GetBytes( serialized );
            var splitIndex = FindSplitInsideMultibyteSequence( serializedBytes, "🚀" );

            // Act
            var firstSegment = serializedBytes.AsSpan( 0, splitIndex );
            var firstCount = MessageSerializer.DeserializeMessages( firstSegment, out var firstMessages,
                out var firstBytesConsumed );

            var reassembled = new byte[ serializedBytes.Length ];
            Array.Copy( serializedBytes, 0, reassembled, 0, splitIndex );
            Array.Copy( serializedBytes, splitIndex, reassembled, splitIndex, serializedBytes.Length - splitIndex );
            var secondCount = MessageSerializer.DeserializeMessages( reassembled, out var secondMessages,
                out var secondBytesConsumed );

            // Assert
            Assert.AreEqual( 0, firstCount );
            Assert.IsEmpty( firstMessages );
            Assert.AreEqual( 0, firstBytesConsumed );
            Assert.AreEqual( 1, secondCount );
            Assert.AreEqual( serializedBytes.Length, secondBytesConsumed );
            Assert.AreEqual( "Event", secondMessages[ 0 ].Type );
            Assert.AreEqual( TestMessageId, secondMessages[ 0 ].Id );
        }

        private static int FindSplitInsideMultibyteSequence ( byte[] buffer, string value )
        {
            var sequence = System.Text.Encoding.UTF8.GetBytes( value );
            for ( var i = 0; i <= (buffer.Length - sequence.Length); i++ )
            {
                if ( buffer.Skip( i ).Take( sequence.Length ).SequenceEqual( sequence ) )
                {
                    return i + 1;
                }
            }

            Assert.Fail( $"Unable to locate UTF-8 sequence for '{value}' in serialized payload." );
            return 0;
        }
    }
}
