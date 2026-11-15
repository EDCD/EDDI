using EddiEddnResponder.Sender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class EddnDuplicateTests : TestBase
    {
        [TestInitialize]
        public void Start () => MakeSafe();

        private static EDDNBody Body ( string bodyName = "Earth" ) => new()
        {
            schemaRef = "https://eddn.edcd.io/schemas/journal/1/test",
            header = new EDDNHeader
            {
                uploaderID = "test", softwareName = "EDDI", softwareVersion = "test", gameversion = "4.4", gamebuild = "test"
            },
            message = new Dictionary<string, object>
            {
                ["timestamp"] = "2026-11-12T10:00:00Z", ["event"] = "Scan", ["StarSystem"] = "Sol", ["BodyName"] = bodyName
            }
        };

        private static bool Sign ( byte[] payload, out string signature )
        {
            signature = Convert.ToHexString( HMACSHA256.HashData( Encoding.UTF8.GetBytes( "disposable-test-key" ), payload ) ).ToLowerInvariant();
            return true;
        }

        [TestMethod]
        [DataRow( 200 )]
        [DataRow( 202 )]
        public async Task AcceptedDuplicatesAreSuppressedBeforeSigning ( int status )
        {
            using var handler = new CaptureHandler( (HttpStatusCode)status );
            using var client = new HttpClient( handler );
            var signingCalls = 0;
            bool CountingSign ( byte[] payload, out string signature ) { signingCalls++; return Sign( payload, out signature ); }
            var sender = new EDDNSender( client, CountingSign, _ => throw new AssertFailedException( "Unexpected retry" ) );
            await sender.sendMessageAsync( Body() );
            await sender.sendMessageAsync( Body() );
            Assert.HasCount( 1, handler.Requests );
            Assert.AreEqual( 1, signingCalls );
        }

        [TestMethod]
        public async Task ObjectKeyOrderIsIgnoredWithoutChangingTransmittedBytes ()
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => Task.CompletedTask );
            var first = Body();
            first.message["nested"] = JObject.Parse( """{"b":2,"a":1,"items":[{"z":3,"y":4}]}""" );
            var second = Body();
            second.message = second.message.Reverse().ToDictionary( p => p.Key, p => p.Value );
            second.message["nested"] = JObject.Parse( """{"items":[{"y":4,"z":3}],"a":1,"b":2}""" );
            await sender.sendMessageAsync( first );
            await sender.sendMessageAsync( second );
            var request = handler.Requests.Single();
            StringAssert.Contains( Encoding.UTF8.GetString( request.Payload ), "\"b\":2,\"a\":1" );
            Sign( request.Payload, out var signature );
            Assert.AreEqual( signature, request.Signature );
        }

        [TestMethod]
        [DataRow( "timestamp" )]
        [DataRow( "content" )]
        [DataRow( "schema" )]
        [DataRow( "uploader" )]
        [DataRow( "gameversion" )]
        [DataRow( "gamebuild" )]
        [DataRow( "softwareVersion" )]
        [DataRow( "arrayOrder" )]
        [DataRow( "decimalPrecision" )]
        [DataRow( "tinyNumber" )]
        [DataRow( "valueType" )]
        [DataRow( "nullField" )]
        public async Task ChangedMessageOrContextRemainsEligible ( string change )
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => Task.CompletedTask );
            var first = Body();
            var second = Body();
            switch ( change )
            {
                case "timestamp": second.message["timestamp"] = "2026-11-12T10:00:01Z"; break;
                case "content": second.message["BodyName"] = "Mars"; break;
                case "schema": second.schemaRef = "https://eddn.edcd.io/schemas/scanorganic/1/test"; break;
                case "uploader": second.header.uploaderID = "another commander"; break;
                case "gameversion": second.header.gameversion = "CAPI-Live-market"; break;
                case "gamebuild": second.header.gamebuild = "another build"; break;
                case "softwareVersion": second.header.softwareVersion = "another version"; break;
                case "arrayOrder": first.message["items"] = new[] { 1, 2 }; second.message["items"] = new[] { 2, 1 }; break;
                case "decimalPrecision": first.message["value"] = 1234567890123456789.000000001M; second.message["value"] = 1234567890123456789.000000002M; break;
                case "tinyNumber": first.message["value"] = 1e-100; second.message["value"] = 2e-100; break;
                case "valueType": first.message["value"] = 123; second.message["value"] = "123"; break;
                case "nullField": first.message["value"] = null; break;
            }
            await sender.sendMessageAsync( first );
            await sender.sendMessageAsync( second );
            await sender.sendMessageAsync( second );
            Assert.HasCount( 2, handler.Requests );
        }

        [TestMethod]
        public async Task ConcurrentPendingDuplicatesStayReservedAndExpireTenMinutesAfterSuccess ()
        {
            var release = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            using var handler = new CaptureHandler( HttpStatusCode.OK ) { ResponseGate = release.Task };
            using var client = new HttpClient( handler );
            var clock = new ManualTimeProvider();
            var sender = new EDDNSender( client, Sign, _ => Task.CompletedTask, clock );
            var sending = sender.sendMessageAsync( Body() );
            try
            {
                await Task.WhenAll( Enumerable.Range( 0, 20 ).Select( _ => sender.sendMessageAsync( Body() ) ) );
                clock.Advance( TimeSpan.FromHours( 1 ) );
                await sender.sendMessageAsync( Body() );
                Assert.HasCount( 1, handler.Requests );
                Assert.IsFalse( sending.IsCompleted );
            }
            finally { release.TrySetResult(); await sending.WaitAsync( TimeSpan.FromSeconds( 10 ) ); }

            clock.Advance( TimeSpan.FromMinutes( 10 ) - TimeSpan.FromSeconds( 1 ) );
            await sender.sendMessageAsync( Body() );
            Assert.HasCount( 1, handler.Requests );
            clock.Advance( TimeSpan.FromSeconds( 1 ) );
            await sender.sendMessageAsync( Body() );
            Assert.HasCount( 2, handler.Requests );
        }

        [TestMethod]
        public async Task CapacityEvictsOldestAcceptedEntry ()
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => Task.CompletedTask, new ManualTimeProvider() );
            for ( var i = 0; i < 4096; i++ ) { await sender.sendMessageAsync( Body( i.ToString() ) ); }
            await sender.sendMessageAsync( Body( "0" ) );
            Assert.HasCount( 4096, handler.Requests );
            await sender.sendMessageAsync( Body( "4096" ) );
            await sender.sendMessageAsync( Body( "1" ) );
            Assert.HasCount( 4097, handler.Requests );
            await sender.sendMessageAsync( Body( "0" ) );
            Assert.HasCount( 4098, handler.Requests );
        }

        [TestMethod]
        public async Task FullPendingCacheDoesNotEvictReservationsOrDiscardUniqueMessages ()
        {
            var release = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            using var handler = new CaptureHandler( HttpStatusCode.OK ) { ResponseGate = release.Task };
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => Task.CompletedTask );
            var sending = Enumerable.Range( 0, 4096 ).Select( i => sender.sendMessageAsync( Body( i.ToString() ) ) ).ToList();
            try
            {
                sending.Add( sender.sendMessageAsync( Body( "overflow" ) ) );
                sending.Add( sender.sendMessageAsync( Body( "overflow" ) ) );
                await sender.sendMessageAsync( Body( "0" ) );
            }
            finally { release.TrySetResult(); await Task.WhenAll( sending ).WaitAsync( TimeSpan.FromSeconds( 30 ) ); }
            Assert.HasCount( 4098, handler.Requests );
            await sender.sendMessageAsync( Body( "overflow" ) );
            await sender.sendMessageAsync( Body( "overflow" ) );
            Assert.HasCount( 4099, handler.Requests );
        }

        [TestMethod]
        [DataRow( 400 )]
        [DataRow( 403 )]
        [DataRow( 408 )]
        [DataRow( 413 )]
        [DataRow( 503 )]
        [DataRow( 500 )]
        [DataRow( 204 )]
        [DataRow( 0 )]
        public async Task FailedOrUnconfirmedSubmissionDoesNotBlockResubmission ( int status )
        {
            var retries = status is 0 or 408 or 413 or 503;
            object failure = status == 0 ? HttpRequestError.ConnectionError : (object)(HttpStatusCode)status;
            using var handler = new CaptureHandler( retries ? new[] { failure, failure, HttpStatusCode.OK } : new[] { failure, HttpStatusCode.OK } );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => Task.CompletedTask );
            await sender.sendMessageAsync( Body() );
            await sender.sendMessageAsync( Body() );
            await sender.sendMessageAsync( Body() );
            Assert.HasCount( retries ? 3 : 2, handler.Requests );
        }

        [TestMethod]
        public async Task UnexpectedCancellationReleasesReservation ()
        {
            using var handler = new CaptureHandler( new TaskCanceledException(), HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => Task.CompletedTask );
            await Assert.ThrowsAsync<OperationCanceledException>( () => sender.sendMessageAsync( Body() ) );
            await sender.sendMessageAsync( Body() );
            await sender.sendMessageAsync( Body() );
            Assert.HasCount( 2, handler.Requests );
        }

        [TestMethod]
        [DataRow( true )]
        [DataRow( false )]
        public async Task SigningFailureReleasesReservation ( bool throws )
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            var calls = 0;
            bool FlakySign ( byte[] payload, out string signature )
            {
                if ( ++calls == 1 )
                {
                    if ( throws ) { throw new InvalidOperationException( "test signer failure" ); }
                    signature = "invalid";
                    return true;
                }
                return Sign( payload, out signature );
            }
            var sender = new EDDNSender( client, FlakySign, _ => Task.CompletedTask );
            await sender.sendMessageAsync( Body() );
            await sender.sendMessageAsync( Body() );
            await sender.sendMessageAsync( Body() );
            Assert.AreEqual( 2, calls );
            Assert.HasCount( 1, handler.Requests );
        }

        [TestMethod]
        public async Task RetryUsesFrozenPayloadAndReservationDespiteCallerMutation ()
        {
            using var handler = new CaptureHandler( HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            var body = Body();
            EDDNSender sender = null;
            async Task Delay ( TimeSpan duration )
            {
                Assert.AreEqual( TimeSpan.FromSeconds( 60 ), duration );
                await sender.sendMessageAsync( Body() ); // Coalesces while the first request retries.
                Assert.HasCount( 1, handler.Requests );
                body.message["BodyName"] = "Mars";
            }
            sender = new EDDNSender( client, Sign, Delay );
            await sender.sendMessageAsync( body );
            await sender.sendMessageAsync( Body() );
            await sender.sendMessageAsync( body );
            var requests = handler.Requests.ToArray();
            Assert.HasCount( 3, requests );
            CollectionAssert.AreEqual( requests[0].Payload, requests[1].Payload );
            Assert.AreEqual( requests[0].Signature, requests[1].Signature );
            Assert.IsTrue( requests[1].Compressed );
            Assert.AreEqual( "Mars", (string)JObject.Parse( Encoding.UTF8.GetString( requests[2].Payload ) )["message"]["BodyName"] );
        }

        [TestMethod]
        public async Task SeparateSendersDoNotShareDuplicateState ()
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var firstClient = new HttpClient( handler, false );
            using var secondClient = new HttpClient( handler, false );
            var first = new EDDNSender( firstClient, Sign, _ => Task.CompletedTask );
            var second = new EDDNSender( secondClient, Sign, _ => Task.CompletedTask );
            await first.sendMessageAsync( Body() );
            await second.sendMessageAsync( Body() );
            Assert.HasCount( 2, handler.Requests );
        }

        [TestMethod]
        public async Task DistinctNavBeaconScansRemainEligibleWithoutPacing ()
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => throw new AssertFailedException( "Unexpected delay" ) );
            await Task.WhenAll( Enumerable.Range( 0, 64 ).Select( async i =>
            {
                var body = Body( "Sol " + i );
                body.message["BodyID"] = i;
                await sender.sendMessageAsync( body );
                await sender.sendMessageAsync( body );
            } ) );
            Assert.HasCount( 64, handler.Requests );
        }

        private sealed class ManualTimeProvider : TimeProvider
        {
            private long timestamp;
            public override long TimestampFrequency => TimeSpan.TicksPerSecond;
            public override long GetTimestamp () => Interlocked.Read( ref timestamp );
            internal void Advance ( TimeSpan duration ) => Interlocked.Add( ref timestamp, duration.Ticks );
        }

        private sealed class CaptureHandler ( params object[] outcomes ) : HttpMessageHandler
        {
            internal readonly ConcurrentQueue<(byte[] Payload, string Signature, bool Compressed)> Requests = new();
            internal Task ResponseGate { get; set; } = Task.CompletedTask;
            private int attempts;

            protected override async Task<HttpResponseMessage> SendAsync ( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                var attempt = Interlocked.Increment( ref attempts );
                var payload = await request.Content.ReadAsByteArrayAsync( cancellationToken );
                var compressed = request.Content.Headers.ContentEncoding.Contains( "gzip" );
                if ( compressed )
                {
                    using var input = new MemoryStream( payload );
                    using var gzip = new GZipStream( input, CompressionMode.Decompress );
                    using var output = new MemoryStream();
                    await gzip.CopyToAsync( output, cancellationToken );
                    payload = output.ToArray();
                }
                var signature = request.Headers.TryGetValues( "X-Signature", out var values ) ? values.Single() : null;
                Requests.Enqueue( (payload, signature, compressed) );
                await ResponseGate;
                var outcome = outcomes[Math.Min( attempt - 1, outcomes.Length - 1 )];
                if ( outcome is HttpRequestError error ) { throw new HttpRequestException( error ); }
                if ( outcome is Exception exception ) { throw exception; }
                return new HttpResponseMessage( (HttpStatusCode)outcome ) { Content = new StringContent( "OK" ) };
            }
        }
    }
}
