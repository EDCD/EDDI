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
    [TestClass, TestCategory( "UnitTests" )]
    public class EddnSigningTests : TestBase
    {
        [TestInitialize]
        public void Start () => MakeSafe();

        private static EDDNBody Body ( string name = "Étoile A" ) => new()
        {
            schemaRef = "https://eddn.edcd.io/schemas/journal/1/test",
            header = new EDDNHeader { gameversion = "4.0", softwareName = "EDDI", softwareVersion = "test", uploaderID = "test" },
            message = new Dictionary<string, object> { ["StarSystem"] = name }
        };

        private static bool Sign ( byte[] payload, out string signature )
        {
            signature = Convert.ToHexString( HMACSHA256.HashData( Encoding.UTF8.GetBytes( "disposable-test-key" ), payload ) ).ToLowerInvariant();
            return true;
        }

        [TestMethod]
        [DataRow( 200, 0 )]
        [DataRow( 202, 0 )]
        [DataRow( 408, 60 )]
        [DataRow( 504, 60 )]
        [DataRow( 503, 120 )]
        [DataRow( 413, 60 )]
        public async Task SignsExactEnvelopeAndRetainsSignatureOnRetry ( int initialStatus, int retryDelaySeconds )
        {
            var calls = 0;
            byte[] signedBytes = null;
            bool SignOnce ( byte[] payload, out string signature )
            {
                calls++;
                signedBytes = payload.ToArray();
                return Sign( payload, out signature );
            }
            using var handler = new CaptureHandler( (HttpStatusCode)initialStatus );
            using var client = new HttpClient( handler );
            var delays = new List<TimeSpan>();
            var sender = new EDDNSender( client, SignOnce, d => { delays.Add( d ); return Task.CompletedTask; } );
            await sender.sendMessageAsync( Body() );
            Assert.AreEqual( 1, calls );
            Assert.HasCount( initialStatus >= 400 ? 2 : 1, handler.Requests);
            CollectionAssert.AreEqual( retryDelaySeconds == 0 ? Array.Empty<TimeSpan>() : new[] { TimeSpan.FromSeconds( retryDelaySeconds ) }, delays );
            var envelope = JObject.Parse( Encoding.UTF8.GetString( signedBytes ) );
            Assert.AreEqual( Body().schemaRef, (string)envelope["$schemaRef"] );
            Assert.AreEqual( "EDDI", (string)envelope["header"]["softwareName"] );
            Assert.AreEqual( "Étoile A", (string)envelope["message"]["StarSystem"] );
            foreach ( var (Payload, Signature, Compressed) in handler.Requests )
            {
                CollectionAssert.AreEqual( signedBytes, Payload );
                Sign( Payload, out var expected );
                Assert.AreEqual( expected, Signature );
            }
            Assert.AreEqual( initialStatus == 413, handler.Requests.Last().Compressed );
            Assert.IsFalse( client.DefaultRequestHeaders.Contains( "X-Signature" ) );
        }

        [TestMethod]
        [DataRow( true )]
        [DataRow( false )]
        public async Task ForbiddenIsTerminalAndDoesNotInvalidateSchema ( bool signed )
        {
            bool OptionalSign ( byte[] bytes, out string signature )
            {
                signature = null;
                return signed && Sign( bytes, out signature );
            }
            using var handler = new CaptureHandler( HttpStatusCode.Forbidden );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, OptionalSign, _ => throw new AssertFailedException( "Unexpected retry" ) );
            await sender.sendMessageAsync( Body() );
            Assert.HasCount( 1, handler.Requests);
            Assert.AreEqual( signed, handler.Requests.Single().Signature != null );
            await sender.sendMessageAsync( Body() );
            Assert.HasCount( 2, handler.Requests);
        }

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "invalid" )]
        [DataRow( null )]
        public async Task MalformedSignerDoesNotTransmit ( string value )
        {
            bool BadSign ( byte[] bytes, out string signature ) { signature = value; return true; }
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            await new EDDNSender( client, BadSign, _ => Task.CompletedTask ).sendMessageAsync( Body() );
            Assert.IsEmpty( handler.Requests);
        }

        [TestMethod]
        public async Task SignerExceptionDoesNotTransmit ()
        {
            static bool BadSign ( byte[] bytes, out string signature ) => throw new InvalidOperationException( "private details" );
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            await new EDDNSender( client, BadSign, _ => Task.CompletedTask ).sendMessageAsync( Body() );
            Assert.IsEmpty( handler.Requests);
        }

        [TestMethod]
        [DataRow( HttpRequestError.ConnectionError )]
        [DataRow( HttpRequestError.SecureConnectionError )]
        [DataRow( HttpRequestError.ResponseEnded )]
        public async Task NetworkRetryRetainsSignature ( HttpRequestError error )
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK, initialError: error );
            using var client = new HttpClient( handler );
            var calls = 0;
            bool CountingSign ( byte[] payload, out string signature ) { calls++; return Sign( payload, out signature ); }
            var delays = new List<TimeSpan>();
            await new EDDNSender( client, CountingSign, d => { delays.Add( d ); return Task.CompletedTask; } ).sendMessageAsync( Body() );
            Assert.AreEqual( 1, calls );
            Assert.HasCount( 2, handler.Requests);
            CollectionAssert.AreEqual( new[] { TimeSpan.FromSeconds( 60 ) }, delays );
            CollectionAssert.AreEqual( handler.Requests.First().Payload, handler.Requests.Last().Payload );
            Assert.AreEqual( handler.Requests.First().Signature, handler.Requests.Last().Signature );
        }

        [TestMethod]
        [DataRow( 202 )]
        [DataRow( 403 )]
        public async Task CompressedResponsesDoNotCauseFurtherRetries ( int status )
        {
            using var handler = new CaptureHandler( HttpStatusCode.RequestEntityTooLarge, (HttpStatusCode)status );
            using var client = new HttpClient( handler );
            await new EDDNSender( client, Sign, _ => Task.CompletedTask ).sendMessageAsync( Body() );
            Assert.HasCount( 2, handler.Requests);
            Assert.IsTrue( handler.Requests.Last().Compressed );
        }

        [TestMethod]
        public async Task ConcurrentMessagesKeepTheirOwnSignatures ()
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => Task.CompletedTask );
            await Task.WhenAll( Enumerable.Range( 0, 20 ).Select( i => sender.sendMessageAsync( Body( i.ToString() ) ) ) );
            Assert.HasCount( 20, handler.Requests);
            Assert.AreEqual( 20, handler.Requests.Select( r => r.Signature ).Distinct().Count() );
            foreach ( var (Payload, Signature, Compressed) in handler.Requests )
            {
                Sign( Payload, out var expected );
                Assert.AreEqual( expected, Signature );
            }
        }

        [TestMethod]
        [DataRow( 0, 408 )]
        [DataRow( 0, 504 )]
        [DataRow( 0, 503 )]
        [DataRow( 0, 413 )]
        [DataRow( 0, 0 )]
        [DataRow( 0, 400 )]
        [DataRow( 0, 403 )]
        [DataRow( 0, 426 )]
        [DataRow( 408, 503 )]
        [DataRow( 503, 413 )]
        [DataRow( 413, 408 )]
        [DataRow( 413, 413 )]
        [DataRow( 413, 0 )]
        [DataRow( 504, 0 )]
        public async Task MixedFailuresShareOneRetryBudget ( int firstStatus, int secondStatus )
        {
            // Zero represents a transient network failure rather than an HTTP response.
            using var handler = new CaptureHandler( (HttpStatusCode)firstStatus, (HttpStatusCode)secondStatus,
                firstStatus == 0 ? HttpRequestError.ConnectionError : null,
                secondStatus == 0 ? HttpRequestError.ResponseEnded : null );
            using var client = new HttpClient( handler );
            var delays = new List<TimeSpan>();
            var signingCalls = 0;
            bool CountingSign ( byte[] payload, out string signature ) { signingCalls++; return Sign( payload, out signature ); }
            var sender = new EDDNSender( client, CountingSign, d => { delays.Add( d ); return Task.CompletedTask; } );
            var body = Body();
            // Keep schema retirement in the 426 case isolated from the other sender tests.
            body.schemaRef += "/" + Guid.NewGuid().ToString( "N" );
            await sender.sendMessageAsync( body );

            Assert.HasCount( 2, handler.Requests );
            Assert.AreEqual( 1, signingCalls );
            CollectionAssert.AreEqual( new[] { TimeSpan.FromSeconds( firstStatus == 503 ? 120 : 60 ) }, delays );
            var requests = handler.Requests.ToArray();
            Assert.IsFalse( requests[0].Compressed );
            Assert.AreEqual( firstStatus == 413, requests[1].Compressed );
            CollectionAssert.AreEqual( requests[0].Payload, requests[1].Payload );
            Assert.AreEqual( requests[0].Signature, requests[1].Signature );
            if ( secondStatus == 426 )
            {
                await sender.sendMessageAsync( body );
                Assert.HasCount( 2, handler.Requests );
                Assert.AreEqual( 1, signingCalls );
            }
        }

        [TestMethod]
        [DataRow( 400 )]
        [DataRow( 403 )]
        [DataRow( 426 )]
        [DataRow( 500 )]
        public async Task TerminalResponsesDoNotWaitOrRetry ( int status )
        {
            using var handler = new CaptureHandler( (HttpStatusCode)status );
            using var client = new HttpClient( handler );
            var sender = new EDDNSender( client, Sign, _ => throw new AssertFailedException( "Unexpected retry" ) );
            var body = Body();
            body.schemaRef += "/" + Guid.NewGuid().ToString( "N" );
            await sender.sendMessageAsync( body );
            Assert.HasCount( 1, handler.Requests );
            await sender.sendMessageAsync( body );
            Assert.HasCount( status == 426 ? 1 : 2, handler.Requests );
        }

        [TestMethod]
        [DataRow( HttpRequestError.NameResolutionError )]
        [DataRow( HttpRequestError.InvalidResponse )]
        public async Task NonRetryableNetworkErrorsDoNotWaitOrRetry ( HttpRequestError error )
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK, initialError: error );
            using var client = new HttpClient( handler );
            await new EDDNSender( client, Sign, _ => throw new AssertFailedException( "Unexpected retry" ) ).sendMessageAsync( Body() );
            Assert.HasCount( 1, handler.Requests );
        }

        [TestMethod]
        [DataRow( 0 )]
        [DataRow( 408 )]
        [DataRow( 413 )]
        [DataRow( 503 )]
        public async Task UnsignedRetriesKeepPayloadWithoutAddingSignature ( int status )
        {
            using var handler = new CaptureHandler( (HttpStatusCode)status, initialError: status == 0 ? HttpRequestError.ConnectionError : null );
            using var client = new HttpClient( handler );
            static bool NoSign ( byte[] payload, out string signature ) { signature = null; return false; }
            await new EDDNSender( client, NoSign, _ => Task.CompletedTask ).sendMessageAsync( Body() );
            var requests = handler.Requests.ToArray();
            Assert.HasCount( 2, requests );
            Assert.IsTrue( requests.All( r => r.Signature == null ) );
            CollectionAssert.AreEqual( requests[0].Payload, requests[1].Payload );
            Assert.AreEqual( status == 413, requests[1].Compressed );
        }

        [TestMethod]
        public async Task RetryDoesNotTransmitBeforeDelayCompletes ()
        {
            using var handler = new CaptureHandler( HttpStatusCode.RequestEntityTooLarge );
            using var client = new HttpClient( handler );
            var delayRequested = new TaskCompletionSource<TimeSpan>( TaskCreationOptions.RunContinuationsAsynchronously );
            var releaseDelay = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            Task Delay ( TimeSpan duration ) { delayRequested.SetResult( duration ); return releaseDelay.Task; }
            var sending = new EDDNSender( client, Sign, Delay ).sendMessageAsync( Body() );
            try
            {
                Assert.AreEqual( TimeSpan.FromSeconds( 60 ), await delayRequested.Task.WaitAsync( TimeSpan.FromSeconds( 10 ) ) );
                Assert.HasCount( 1, handler.Requests );
                Assert.IsFalse( sending.IsCompleted );
                releaseDelay.SetResult();
                await sending.WaitAsync( TimeSpan.FromSeconds( 10 ) );
                Assert.HasCount( 2, handler.Requests );
                Assert.IsTrue( handler.Requests.Last().Compressed );
            }
            finally
            {
                releaseDelay.TrySetResult();
                await sending.WaitAsync( TimeSpan.FromSeconds( 10 ) );
            }
        }

        private sealed class CaptureHandler ( HttpStatusCode initialStatus, HttpStatusCode subsequentStatus = HttpStatusCode.OK,
            HttpRequestError? initialError = null, HttpRequestError? subsequentError = null ) : HttpMessageHandler
        {
            internal readonly ConcurrentQueue<(byte[] Payload, string Signature, bool Compressed)> Requests = new();
            private int attempts;

            protected override async Task<HttpResponseMessage> SendAsync ( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                var attempt = Interlocked.Increment( ref attempts );
                await Task.Yield();
                Assert.AreEqual( "/upload/", request.RequestUri.AbsolutePath );
                Assert.AreEqual( "application/json", request.Content.Headers.ContentType.MediaType );
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
                var error = attempt == 1 ? initialError : subsequentError;
                if ( error != null ) { throw new HttpRequestException( error.Value ); }
                return new HttpResponseMessage( attempt == 1 ? initialStatus : subsequentStatus ) { Content = new StringContent( "OK" ) };
            }
        }
    }
}

