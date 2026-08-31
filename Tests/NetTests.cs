using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class NetTests : TestBase
    {
        [TestInitialize]
        public void Start ()
        {
            MakeSafe();
        }

        [TestMethod]
        public async Task DownloadStringAsyncRetriesTransientRequestFailures ()
        {
            var handler = new SequencedHandler();
            handler.EnqueueException( new HttpRequestException( "TLS handshake failed", new IOException( "connection closed" ) ) );
            handler.EnqueueException( new IOException( "stream closed" ) );
            handler.EnqueueResponse( HttpStatusCode.OK, "success" );

            using var client = new HttpClient( handler );

            var result = await Net.DownloadStringAsync( "https://example.test/data", client, 3, _ => Task.CompletedTask );

            Assert.AreEqual( "success", result );
            Assert.AreEqual( 3, handler.Attempts );
        }

        [TestMethod]
        public async Task DownloadStringAsyncDoesNotRetryHttpStatusFailures ()
        {
            var handler = new SequencedHandler();
            handler.EnqueueResponse( HttpStatusCode.NotFound, "missing" );
            handler.EnqueueResponse( HttpStatusCode.OK, "success" );

            using var client = new HttpClient( handler );

            var result = await Net.DownloadStringAsync( "https://example.test/data", client, 3, _ => Task.CompletedTask );

            Assert.IsNull( result );
            Assert.AreEqual( 1, handler.Attempts );
        }

        [TestMethod]
        public async Task DownloadStringAsyncStopsAfterConfiguredAttempts ()
        {
            var handler = new SequencedHandler();
            handler.EnqueueException( new HttpRequestException( "first failure" ) );
            handler.EnqueueException( new HttpRequestException( "second failure" ) );
            handler.EnqueueException( new HttpRequestException( "third failure" ) );
            handler.EnqueueResponse( HttpStatusCode.OK, "success" );

            using var client = new HttpClient( handler );

            var result = await Net.DownloadStringAsync( "https://example.test/data", client, 3, _ => Task.CompletedTask );

            Assert.IsNull( result );
            Assert.AreEqual( 3, handler.Attempts );
        }

        private sealed class SequencedHandler : HttpMessageHandler
        {
            private readonly Queue<Func<HttpResponseMessage>> actions = new();

            internal int Attempts { get; private set; }

            internal void EnqueueResponse ( HttpStatusCode statusCode, string content )
            {
                actions.Enqueue( () => new HttpResponseMessage( statusCode )
                {
                    Content = new StringContent( content )
                } );
            }

            internal void EnqueueException ( Exception exception )
            {
                actions.Enqueue( () => throw exception );
            }

            protected override Task<HttpResponseMessage> SendAsync ( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                Attempts++;
                return Task.FromResult( actions.Dequeue().Invoke() );
            }
        }
    }
}