using System;
using System.Net;

namespace EddiEddnResponder.Sender
{
    internal sealed class EddnResponseException ( HttpStatusCode statusCode, string message ) : Exception( message )
    {
        public HttpStatusCode StatusCode { get; } = statusCode;
    }
}
