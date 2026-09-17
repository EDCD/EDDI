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

        private static EDDNBody Body ( string name = "Étoile 星" ) => new()
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
        [DataRow( 200 )]
        [DataRow( 202 )]
        [DataRow( 408 )]
        [DataRow( 504 )]
        [DataRow( 503 )]
        [DataRow( 413 )]
        public async Task SignsExactEnvelopeAndRetainsSignatureOnRetry ( int initialStatus )
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
            var sender = new EDDNSender( client, SignOnce, _ => Task.CompletedTask );
            await sender.sendMessageAsync( Body() );
            Assert.AreEqual( 1, calls );
            Assert.HasCount( initialStatus >= 400 ? 2 : 1, handler.Requests);
            var envelope = JObject.Parse( Encoding.UTF8.GetString( signedBytes ) );
            Assert.AreEqual( Body().schemaRef, (string)envelope["$schemaRef"] );
            Assert.AreEqual( "EDDI", (string)envelope["header"]["softwareName"] );
            Assert.AreEqual( "Étoile 星", (string)envelope["message"]["StarSystem"] );
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
        public async Task NetworkRetryRetainsSignature ()
        {
            using var handler = new CaptureHandler( HttpStatusCode.OK, failFirst: true );
            using var client = new HttpClient( handler );
            var calls = 0;
            bool CountingSign ( byte[] payload, out string signature ) { calls++; return Sign( payload, out signature ); }
            await new EDDNSender( client, CountingSign, _ => Task.CompletedTask ).sendMessageAsync( Body() );
            Assert.AreEqual( 1, calls );
            Assert.HasCount( 2, handler.Requests);
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

        private sealed class CaptureHandler ( HttpStatusCode initialStatus, HttpStatusCode subsequentStatus = HttpStatusCode.OK,
            bool failFirst = false ) : HttpMessageHandler
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
                if ( failFirst && attempt == 1 ) { throw new HttpRequestException( HttpRequestError.ConnectionError ); }
                return new HttpResponseMessage( attempt == 1 ? initialStatus : subsequentStatus ) { Content = new StringContent( "OK" ) };
            }
        }
    }
}

