using EddiSpanshService;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Utilities;

namespace Tests
{
    // A mock rest client for the Spansh Service
    internal class FakeSpanshRestClient : ISpanshRestClient
    {
        public Dictionary<string, string> CannedContent = new Dictionary<string, string>();

        public Uri BuildUri(IRestRequest request)
        {
            return new Uri("fakeSpansh://" + request.Resource);
        }

        IRestResponse<T> ISpanshRestClient.Execute<T>(IRestRequest request)
        {
            // this will throw if given a resource not in the canned dictionaries: that's OK
            var content = CannedContent[request.Resource];
            IRestResponse<T> restResponse = new RestResponse<T>
            {
                Content = content,
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = HttpStatusCode.OK,
            };
            return restResponse;
        }

        IRestResponse ISpanshRestClient.Get(IRestRequest request)
        {
            var resourceString = $"{request.Resource}";

            // this will throw if given a resource not in the canned dictionaries: that's OK
            try
            {
                resourceString += request.Parameters.Any() ? "?" : string.Empty;
                for ( var i = 0; i < request.Parameters.Count; i++ )
                {
                    var parameter = request.Parameters[ i ];
                    resourceString += i > 0 ? "&" : "";
                    resourceString += $"{parameter.Name}={parameter.Value}";
                }

                var content = CannedContent[resourceString];
                IRestResponse restResponse = new RestResponse
                {
                    Content = content,
                    ResponseStatus = ResponseStatus.Completed,
                    StatusCode = HttpStatusCode.OK,
                };
                return restResponse;
            }
            catch ( KeyNotFoundException knfe )
            {
                Logging.Error( knfe.Message, knfe );
                throw;
            }
        }

        public IRestResponse Post ( IRestRequest request )
        {
            var resourceString = $"{request.Resource}";

            // this will throw if given a resource not in the canned dictionaries: that's OK
            try
            {
                resourceString += request.Parameters.Any() ? "?" : string.Empty;
                foreach ( var parameter in request.Parameters )
                {
                    resourceString += $"{parameter.Name}={parameter.Value}";
                }

                var content = CannedContent[resourceString];
                IRestResponse restResponse = content is null
                    ? new RestResponse
                    {
                        Content = string.Empty,
                        ResponseStatus = ResponseStatus.Error,
                        StatusCode = HttpStatusCode.InternalServerError
                    }
                    : new RestResponse
                    {
                        Content = content, 
                        ResponseStatus = ResponseStatus.Completed, 
                        StatusCode = HttpStatusCode.OK
                    };
                return restResponse;
            }
            catch ( KeyNotFoundException knfe )
            {
                Logging.Error( knfe.Message, knfe );
                throw;
            }
        }

        public void Expect(string resource, string content)
        {
            CannedContent[resource] = content;
        }
    }
}