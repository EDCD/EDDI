using EddiSpanshService;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    // A mock rest client for the Spansh Service
    internal class FakeSpanshHttpClient : ISpanshHttpClient
    {
        #region Implementation of ISpanshHttpClient

        public Task<HttpResponseMessage> GetAsync ( string requestUri, CancellationToken cancellationToken )
        {
            var responseContent = FetchContentFromUri(requestUri);
            return Task.FromResult( new HttpResponseMessage()
            {
                Content = new StringContent( responseContent, System.Text.Encoding.UTF8, "application/json" ),
                StatusCode = HttpStatusCode.OK
            } );
        }

        public async Task<HttpResponseMessage> PostAsync ( string requestUri, HttpContent requestContent, CancellationToken cancellationToken )
        {
            var json = await requestContent.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            requestUri += $"?={json}";
            var responseContent = FetchContentFromUri(requestUri);
            return await Task.FromResult( new HttpResponseMessage()
            {
                Content = new StringContent( responseContent, System.Text.Encoding.UTF8, "application/json" ),
                StatusCode = HttpStatusCode.OK
            } ).ConfigureAwait(false);
        }

        #endregion

        private readonly Dictionary<string, string> CannedContent = [ ];

        private string FetchContentFromUri(string requestUri)
        {
            // this will throw if given a resource not in the canned dictionaries: that's OK
            if ( CannedContent.TryGetValue(requestUri, out var content))
            {
                return content;
            }

            throw new KeyNotFoundException($"No canned content found for URI: {requestUri}");
        }

        public void Expect(string resource, string content)
        {
            CannedContent[resource] = content;
        }
    }
}