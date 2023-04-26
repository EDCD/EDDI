using EddiCore;
using EddiDataDefinitions;
using EddiEddnResponder.Toolkit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading;
using Utilities;

namespace EddiEddnResponder.Sender
{
    public static class EDDNSender
    {
        public static bool unitTesting = false;

        // Schemas identified as invalid by the server
        private static readonly List<string> invalidSchemas = new List<string>();

        public static void SendToEDDN(string schema, IDictionary<string, object> data, EDDNState eddnState,
            string gameVersionOverride = null)
        {
            try
            {
                var body = new EDDNBody
                {
                    header = generateHeader(eddnState.GameVersion, gameVersionOverride),
                    schemaRef = schema + (EDDI.Instance.ShouldUseTestEndpoints() ? "/test" : ""),
                    message = data
                };
                if (invalidSchemas.Contains(body.schemaRef))
                {
                    Logging.Warn($"EDDN schema {body.schemaRef} is obsolete, data not sent.", data);
                }
                else if (string.IsNullOrEmpty(eddnState.GameVersion.gameVersion))
                {
                    Logging.Warn("Message could not be sent, game version has not been set.", data);
                }
                else
                {
                    Logging.Debug($"EDDN schema {schema} message is: ", body);
                    sendMessage(body);
                }
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
            //string uploader = (EDDI.Instance.Cmdr == null ? "commander" : EDDI.Instance.Cmdr.name);
            //byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(uploader), 0, Encoding.UTF8.GetByteCount(uploader));
            //foreach (byte theByte in crypto)
            //{
            //    hash.Append(theByte.ToString("x2"));
            //}
            //return hash.ToString();
            return string.IsNullOrEmpty(EDDI.Instance.Cmdr?.name) 
                ? "Unknown commander" 
                : EDDI.Instance.Cmdr.name;
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

        private static void sendMessage(EDDNBody body)
        {
            var client = new RestClient("https://eddn.edcd.io:4430/");
            var request = new RestRequest("upload/", Method.POST);
            var msgBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings { ContractResolver = new EDDNContractResolver() });
            request.AddParameter("application/json", msgBody, ParameterType.RequestBody);

            Logging.Debug("Sending " + msgBody);

            if (unitTesting) { return; }

            Thread thread = new Thread(() =>
            {
                try
                {
                    IRestResponse response = client.Execute(request);
                    Logging.Debug("Response content is " + response.Content);
                    switch (response.StatusCode)
                    {
                        // Invalid status codes are defined at https://github.com/EDCD/EDDN/blob/master/docs/Developers.md#server-responses
                        case HttpStatusCode.BadRequest: // Code 400
                            {
                                throw new HttpListenerException(400, response.Content);
                            }
                        case HttpStatusCode.RequestTimeout: // Code 408
                        case HttpStatusCode.GatewayTimeout: // Code 504
                            {
                                Logging.Debug("Request timed out, retrying in 30 seconds.");
                                System.Threading.Tasks.Task.Run(() =>
                                {
                                    Thread.Sleep(TimeSpan.FromSeconds(30));
                                    sendMessage(body);
                                }).ConfigureAwait(false);
                                break;
                            }
                        case HttpStatusCode.RequestEntityTooLarge: // Code 413
                            {
                                // Payload too large. Retry with G-Zipped data
                                Logging.Warn("EDDN service is unable to process the message. Payload too large. Retrying with compressed data.");
                                request = new RestRequest("upload/", Method.POST);
                                request.AddHeader("Content-Encoding", "gzip");
                                var messageConverterStream = new MemoryStream(byte.Parse(msgBody));
                                using (GZipStream compressionStream = new GZipStream(messageConverterStream, CompressionMode.Compress))
                                {
                                    using ( var outputStream = new MemoryStream() )
                                    {
                                        compressionStream.CopyTo(outputStream);
                                        request.AddParameter("application/json", outputStream, ParameterType.RequestBody);
                                    }
                                }
                                response = client.Execute(request);
                                Logging.Debug("Response content is " + response.Content);
                                if (response.StatusCode != HttpStatusCode.Accepted)
                                {
                                    throw new HttpListenerException(413, "Failed to resend to EDDN service with compressed data.");
                                }
                                break;
                            }
                        case HttpStatusCode.UpgradeRequired: // Code 426
                            {
                                // Note that this deviates from the typical usage of code 426
                                // (which typically indicates that this client is using an obsolete security protocol.
                                invalidSchemas.Add(body.schemaRef);
                                throw new HttpListenerException(426, $"Schema {body.schemaRef} is obsolete.");
                            }
                        case HttpStatusCode.ServiceUnavailable: // Code 503
                            {
                                Logging.Debug("EDDN service is unavailable, retrying in 2 minutes");
                                System.Threading.Tasks.Task.Run(() =>
                                {
                                    Thread.Sleep(TimeSpan.FromMinutes(2));
                                    sendMessage(body);
                                }).ConfigureAwait(false);
                                break;
                            }
                        default:
                            {
                                if ((int)response.StatusCode >= 400)
                                {
                                    throw new HttpListenerException((int)response.StatusCode, "Unexpected EDDN service response");
                                }
                                break;
                            }
                    }
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Error($"EDDN {body.schemaRef} Error: {ex.Message}", ex);
                }
            })
            {
                Name = "EDDN message",
                IsBackground = true
            };
            thread.Start();
        }
    }

    public sealed class EDDNContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(CommodityBracket?))
            {
                // The EDDN schema requires a value of "" rather than null for commodity brackets
                property.ValueProvider = new NullToEmptyStringValueProvider(property.ValueProvider);
            }

            return property;
        }

        sealed class NullToEmptyStringValueProvider : IValueProvider
        {
            private readonly IValueProvider Provider;

            public NullToEmptyStringValueProvider(IValueProvider provider)
            {
                Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            }

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
