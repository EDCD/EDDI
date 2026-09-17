using BuildSecrets;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEddnResponder.Toolkit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using HttpClient = System.Net.Http.HttpClient;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace EddiEddnResponder.Sender
{
    // Invalid response status codes are defined at https://github.com/EDCD/EDDN/blob/master/docs/Developers.md#server-responses
    public class EDDNSender
    {
        internal bool unitTesting;
        internal readonly List<(string schema, IDictionary<string, object> data, EDDNState eddnState, string gameVersionOverride)> sentMessages =
            [ ];
        private const string baseUrl = "https://eddn.edcd.io:4430/";
        private const int shortRetryDelaySeconds = 30;
        private const int longRetryDelaySeconds = 120;
        private readonly HttpClient httpClient;

        internal delegate bool PayloadSigner ( byte[] payload, out string signature );
        private readonly PayloadSigner signer;
        private readonly Func<TimeSpan, Task> delay;

        public EDDNSender () : this( new HttpClient(), BuildInjectedSecrets.TrySignEddnPayload, Task.Delay ) { }

        internal EDDNSender ( HttpClient client, PayloadSigner signer, Func<TimeSpan, Task> delay )
        {
            httpClient = client;
            this.signer = signer;
            this.delay = delay;
            httpClient.BaseAddress ??= new Uri( baseUrl );
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd( $"{Constants.EDDI_NAME}/{Constants.EDDI_VERSION}" );
            httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
        }

        // Schemas identified as invalid by the server
        private static readonly List<string> invalidSchemas = [ ];

        public void SendToEDDN ( string schema, IDictionary<string, object> data, EDDNState eddnState,
            string gameVersionOverride = null, bool forceTestEndpoint = false )
        {
            if ( unitTesting )
            {
                sentMessages.Add( (schema, data, eddnState, gameVersionOverride) );
                return;
            }

            SendAsync( schema, data, eddnState, gameVersionOverride, forceTestEndpoint )
                .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
        }

        private async Task SendAsync(string schema, IDictionary<string, object> data, EDDNState eddnState,
            string gameVersionOverride = null, bool forceTestEndpoint = false)
        {
            try
            {
                var body = new EDDNBody
                {
                    header = generateHeader(eddnState.GameVersion, gameVersionOverride),
                    schemaRef = schema + (forceTestEndpoint ||EDDI.Instance.ShouldUseTestEndpoints() ? "/test" : ""),
                    message = data
                };
                Logging.Debug( $"EDDN schema {schema} message is: ", body );
                await sendMessageAsync( body ).ConfigureAwait(false);
            }
            catch (ArgumentException ae)
            {
                Logging.Error("Failed to send data to EDDN", ae);
            }
            catch (NullReferenceException nre)
            {
                Logging.Error("Failed to send data to EDDN", nre);
            }
        }

        private static string generateUploaderId()
        {
            // Uploader ID is a hash of the commander's name
            //System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            //StringBuilder hash = new StringBuilder();
            //string uploader = (ConfigService.Instance.commanderConfiguration.commanderName == null ? "commander" : ConfigService.Instance.commanderConfiguration.commanderName);
            //byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(uploader), 0, Encoding.UTF8.GetByteCount(uploader));
            //foreach (byte theByte in crypto)
            //{
            //    hash.Append(theByte.ToString("x2"));
            //}
            //return hash.ToString();
            return string.IsNullOrEmpty( ConfigService.Instance.commanderConfiguration.commanderName ) 
                ? "Unknown commander" 
                : ConfigService.Instance.commanderConfiguration.commanderName;
        }

        private static EDDNHeader generateHeader(GameVersionAugmenter gameVersion, string gameVersionOverride = null)
        {
            var header = new EDDNHeader
            {
                uploaderID = generateUploaderId(),
                softwareName = Constants.EDDI_NAME,
                softwareVersion = Constants.EDDI_VERSION.ToString(),
                gameversion = string.IsNullOrEmpty(gameVersionOverride) 
                    ? gameVersion.gameVersion 
                    : gameVersionOverride,
                gamebuild = string.IsNullOrEmpty(gameVersionOverride) 
                    ? gameVersion.gameBuild 
                    : string.Empty
            };
            return header;
        }

        internal async Task sendMessageAsync(EDDNBody body)
        {
            if (!TryValidate(body)) { return; }

            var json = SerializeBody(body);
            Logging.Debug( "Sending " + json );

            var payload = Encoding.UTF8.GetBytes( json );
            string signature;
            try
            {
                if ( !signer( payload, out signature ) ) { signature = null; }
                else if ( signature == null || signature.Length != 64 || System.Linq.Enumerable.Any( signature, c => 
                          c is not ( ( >= '0' and <= '9' ) or ( >= 'a' and <= 'f' ) ) ) )
                {
                    throw new InvalidOperationException();
                }
            }
            catch ( Exception )
            {
                // Private signer exceptions may contain secret material.
                Logging.Error( "EDDN signing failed. Dropping message." );
                return;
            }

            HttpResponseMessage response = null;
            try
            {
                try
                {
                    response = await SendRequestAsync( payload, signature ).ConfigureAwait(false);
                }
                catch ( HttpRequestException hre )
                    when ( hre.HttpRequestError is HttpRequestError.ConnectionError or HttpRequestError.SecureConnectionError or HttpRequestError.ResponseEnded )
                {
                    Logging.Debug(
                        $"EDDN {body.schemaRef} transient network error ({hre.HttpRequestError}): {hre.Message}. " +
                        $"Retrying in {shortRetryDelaySeconds}s."
                    );
                    await delay( TimeSpan.FromSeconds( shortRetryDelaySeconds ) ).ConfigureAwait( false );
                    try
                    {
                        response = await SendRequestAsync( payload, signature ).ConfigureAwait( false );
                    }
                    catch ( HttpRequestException retryException )
                        when ( retryException.HttpRequestError is HttpRequestError.ConnectionError or HttpRequestError.SecureConnectionError or HttpRequestError.ResponseEnded )
                    {
                        Logging.Warn(
                            $"EDDN {body.schemaRef} failed after retry ({retryException.HttpRequestError}): {retryException.Message}. " +
                            "Dropping message."
                        );
                        return;
                    }
                }

                if ( response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout ) // Code 408 or 504
                {
                    Logging.Debug( $"Request timed out, retrying in {shortRetryDelaySeconds}s" );
                    response.Dispose();
                    await delay( TimeSpan.FromSeconds( shortRetryDelaySeconds ) ).ConfigureAwait(false);
                    response = await SendRequestAsync( payload, signature ).ConfigureAwait(false);
                }
                else if ( response.StatusCode == HttpStatusCode.ServiceUnavailable ) // Code 503
                {
                    Logging.Debug( $"Service unavailable, retrying in {longRetryDelaySeconds}s" );
                    response.Dispose();
                    await delay( TimeSpan.FromSeconds( longRetryDelaySeconds ) ).ConfigureAwait(false);
                    response = await SendRequestAsync( payload, signature ).ConfigureAwait(false);
                }
                else if ( response.StatusCode == HttpStatusCode.RequestEntityTooLarge ) // Code 413
                {
                    // Payload too large. Retry with G-Zipped data
                    Logging.Warn( "Payload too large. Retrying with gzip compression." );
                    response.Dispose();
                    response = await SendRequestAsync( payload, signature, true ).ConfigureAwait(false);
                }

                await HandleResponseAsync( body, response, signature != null ).ConfigureAwait(false);
            }
            catch ( HttpRequestException hre ) when (hre.InnerException is WebException we)
            {
                Logging.Warn( $"EDDN {body.schemaRef} Error: {we.Message}", we);
            }
            catch ( HttpRequestException hre )
            {
                Logging.Error( $"EDDN {body.schemaRef} Error: {hre.Message}", hre );
            }
            catch ( EddnResponseException ere )
            {
                Logging.Error( $"EDDN {body.schemaRef} Error {(int)ere.StatusCode}: {ere.Message}", ere );
            }
            finally
            {
                response?.Dispose();
            }
        }

        private bool TryValidate(EDDNBody body)
        {
            if ( unitTesting )
            {
                return false;
            }
            if ( invalidSchemas.Contains( body.schemaRef ) )
            {
                Logging.Warn( $"EDDN schema {body.schemaRef} is obsolete, data not sent.", body );
                return false;
            }
            if ( string.IsNullOrEmpty( body.header.gameversion ) )
            {
                Logging.Warn( "Message could not be sent, game version has not been set.", body );
                return false;
            }

            return true;
        }

        private static string SerializeBody ( EDDNBody body )
        {
            return JsonConvert.SerializeObject( body, new JsonSerializerSettings { ContractResolver = new EDDNContractResolver() } );
        }

        private async Task<HttpResponseMessage> SendRequestAsync ( byte[] payload, string signature, bool compressed = false )
        {
            using var request = new HttpRequestMessage( HttpMethod.Post, "upload/" );
            request.Content = new ByteArrayContent( compressed ? Compress( payload ) : payload );
            request.Content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );
            if ( compressed ) { request.Content.Headers.ContentEncoding.Add( "gzip" ); }
            if ( signature != null ) { request.Headers.Add( "X-Signature", signature ); }
            return await httpClient.SendAsync( request ).ConfigureAwait(false);
        }

        private static async Task HandleResponseAsync ( EDDNBody body, HttpResponseMessage response, bool signed )
        {
            var status = response.StatusCode;
            if ( status == HttpStatusCode.Forbidden )
            {
                throw new EddnResponseException( status,
                    $"EDDN rejected {(signed ? "signed" : "unsigned")} upload (HTTP 403). Check app signing configuration." );
            }
            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Logging.Debug( "Response received", responseJson );

            if ( status == HttpStatusCode.BadRequest ) // Code 400
            {
                throw new EddnResponseException( status, responseJson );
            }

            if ( status == HttpStatusCode.UpgradeRequired ) // Code 426
            {
                // Note that this deviates from the typical usage of code 426
                // (which typically indicates that this client is using an obsolete security protocol.
                invalidSchemas.Add( body.schemaRef );
                throw new EddnResponseException( status, $"Schema {body.schemaRef} is obsolete." );
            }

            if ( (int)status >= 400 && status != HttpStatusCode.Accepted )
            {
                throw new EddnResponseException( status, "Unexpected EDDN response" );
            }
        }

        internal static byte[] Compress ( byte[] data )
        {
            using ( var ms = new MemoryStream() )
            {
                using ( var gzip = new GZipStream( ms, CompressionMode.Compress ) )
                {
                    gzip.Write( data, 0, data.Length );
                }

                return ms.ToArray();
            }
        }
    }

    public sealed class EDDNContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(CommodityBracket?))
            {
                // The EDDN schema requires a value of "" rather than null for commodity brackets
                property.ValueProvider = new NullToEmptyStringValueProvider(property.ValueProvider);
            }

            return property;
        }

        sealed class NullToEmptyStringValueProvider ( IValueProvider provider ) : IValueProvider
        {
            private readonly IValueProvider Provider = provider ?? throw new ArgumentNullException(nameof(provider));

            public object GetValue(object target)
            {
                return Provider.GetValue(target) ?? "";
            }

            public void SetValue(object target, object value)
            {
                Provider.SetValue(target, value);
            }
        }
    }
}
