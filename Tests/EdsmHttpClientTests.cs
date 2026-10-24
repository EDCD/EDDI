using EddiStarMapService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class EdsmHttpClientTests
    {
        [TestMethod]
        [DataRow( false, "{\"result\":\"success\"}" )]
        [DataRow( true, "{\"result\":\"success\"}" )]
        [DataRow( false, "" )]
        [DataRow( true, "" )]
        public async Task RequestReturnsResponseAndSendsExpectedHeadersAndContent( bool post, string response )
        {
            const string url = "https://edsm.example.test/data";
            const string body = "{\"commanderName\":\"TestCmdr\"}";
            using var handler = new RecordingHandler( response );
            using var httpClient = new HttpClient( handler );
            var client = new StarMapService.EdsmHttpClient( httpClient );
            using var content = new StringContent( body );

            var result = post
                ? await client.PostAsync( url, content, CancellationToken.None )
                : await client.GetAsync( url, CancellationToken.None );

            Assert.AreEqual( response, result );
            Assert.AreEqual( 1, handler.Attempts );
            Assert.AreEqual( post ? HttpMethod.Post : HttpMethod.Get, handler.Method );
            Assert.AreEqual( new Uri( url ), handler.Uri );
            Assert.AreEqual( post ? body : null, handler.Body );
            Assert.AreEqual( $"{Constants.EDDI_NAME}/{Constants.EDDI_VERSION}", handler.UserAgent );
            Assert.AreEqual( "application/json", handler.Accept );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public async Task HttpFailureReturnsNull( bool post )
        {
            using var handler = new RecordingHandler( "missing", HttpStatusCode.NotFound );
            using var httpClient = new HttpClient( handler );
            var client = new StarMapService.EdsmHttpClient( httpClient );
            using var content = new StringContent( "{}" );

            var result = post
                ? await client.PostAsync( "https://edsm.example.test/missing", content, CancellationToken.None )
                : await client.GetAsync( "https://edsm.example.test/missing", CancellationToken.None );

            Assert.IsNull( result );
            Assert.AreEqual( 1, handler.Attempts );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public async Task CancellationDuringRequestReturnsNull( bool post )
        {
            using var cts = new CancellationTokenSource();
            using var handler = new RecordingHandler( "", onSend: cts.Cancel );
            using var httpClient = new HttpClient( handler );
            var client = new StarMapService.EdsmHttpClient( httpClient );
            using var content = new StringContent( "{}" );

            var result = post
                ? await client.PostAsync( "https://edsm.example.test/cancel", content, cts.Token )
                : await client.GetAsync( "https://edsm.example.test/cancel", cts.Token );

            Assert.IsNull( result );
            Assert.IsTrue( cts.IsCancellationRequested );
            Assert.AreEqual( 1, handler.Attempts );
        }

        private sealed class RecordingHandler(
            string response,
            HttpStatusCode status = HttpStatusCode.OK,
            Action onSend = null ) : HttpMessageHandler
        {
            internal int Attempts { get; private set; }
            internal HttpMethod Method { get; private set; }
            internal Uri Uri { get; private set; }
            internal string Body { get; private set; }
            internal string UserAgent { get; private set; }
            internal string Accept { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken )
            {
                Attempts++;
                Method = request.Method;
                Uri = request.RequestUri;
                Body = request.Content is null ? null : await request.Content.ReadAsStringAsync( cancellationToken );
                UserAgent = request.Headers.UserAgent.ToString();
                Accept = request.Headers.Accept.ToString();
                onSend?.Invoke();
                if ( cancellationToken.IsCancellationRequested )
                {
                    throw new TaskCanceledException( "Test request cancelled", null, cancellationToken );
                }
                return new HttpResponseMessage( status ) { Content = new StringContent( response ) };
            }
        }
    }
}
