using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utilities;

namespace EddiInaraService
{
    public partial class InaraService
    {
        private string commanderName { get; set; }
        private long? commanderFrontierID { get; set; }
        private string apiKey { get; set; }

        public InaraService(string apikey, string commandername = null, long? commanderfrontierID = null)
        {
            apiKey = apikey?.Trim();
            commanderName = commandername?.Trim();
            commanderFrontierID = commanderfrontierID;
        }

        private static readonly object instanceLock = new object();
        private static InaraService instance;
        public static InaraService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No Inara service instance: creating one");

                            // Set up the Inara service
                            InaraConfiguration inaraCredentials = InaraConfiguration.FromFile();
                            if (!string.IsNullOrEmpty(inaraCredentials?.apiKey))
                            {
                                // Commander name might come from EDSM credentials or from the game and companion app
                                string commanderName = inaraCredentials?.commanderName;
                                string commanderFrontierID = inaraCredentials?.commanderFrontierID;
                                if (!string.IsNullOrEmpty(commanderName))
                                {
                                    instance = new InaraService(inaraCredentials.apiKey, commanderName);
                                    Logging.Info("Configuring EDDI access to Inara profile data");
                                }
                                else
                                {
                                    instance = new InaraService(inaraCredentials.apiKey);
                                    Logging.Warn("InaraService instance is limited: Commander name not set.");
                                }
                            }
                            else
                            {
                                Logging.Warn("No InaraService instance: API key not set.");
                            }
                        }
                    }
                }
                return instance;
            }
        }

        // If you need to do some testing on Inara's API, please set the `inBeta` boolean header property to true.
        public List<InaraResponse> SendEventBatch(ref List<InaraAPIEvent> events, bool inBeta)
        {
            List<InaraResponse> inaraResponses = new List<InaraResponse>();

            if (string.IsNullOrEmpty(apiKey))
            {
                return inaraResponses;
            }

            // Flag each event with a unique ID we can use when processing responses
            List<InaraAPIEvent> indexedEvents = new List<InaraAPIEvent>();
            for (int i = 0; i < events.Count; i++)
            {
                InaraAPIEvent indexedEvent = events[i];
                indexedEvent.eventCustomID = i;
                indexedEvents.Add(indexedEvent);
            }

            var client = new RestClient("https://inara.cz/inapi/v1/");
            var request = new RestRequest(Method.POST);
            InaraSendJson inaraRequest = new InaraSendJson()
            {
                header = new Dictionary<string, object>()
                {
                    { "appName", "EDDI" },
                    { "appVersion", Constants.EDDI_VERSION.ToString() },
                    { "isDeveloped", !inBeta },
                    { "commanderName", commanderName ?? (inBeta ? "TestCmdr" : null) },
                    { "commanderFrontierID", commanderFrontierID },
                    { "APIkey", apiKey }
                },
                events = indexedEvents
            };
            request.RequestFormat = DataFormat.Json;
            request.AddBody(inaraRequest); // uses JsonSerializer

            Logging.Debug("Sending to Inara: " + client.BuildUri(request).AbsoluteUri);
            var clientResponse = client.Execute<InaraResponses>(request);
            InaraResponses response = clientResponse.Data;
            if (validateResponse(response.header))
            {
                foreach (InaraResponse inaraResponse in response.events)
                {
                    if (validateResponse(inaraResponse))
                    {
                        inaraResponses.Add(inaraResponse);
                    }
                }
            }
            return inaraResponses;
        }

        private static bool validateResponse(InaraResponse inaraResponse)
        {
            if (inaraResponse.eventStatus == 200)
            {
                return true;
            }
            else
            {
                Logging.Warn("Inara responded with: " + inaraResponse.eventStatusText, JsonConvert.SerializeObject(inaraResponse.eventData));
                return false;
            }
        }
    }

    internal class InaraSendJson
    {
        public Dictionary<string, object> header { get; set; }

        public List<InaraAPIEvent> events { get; set; }
    }

    internal class InaraResponses
    {
        public InaraResponse header { get; set; }

        public List<InaraResponse> events { get; set; }
    }

    public class InaraResponse
    {
        public int eventStatus { get; set; }

        public string eventStatusText { get; set; }

        public object eventData { get; set; }

        public int eventCustomID { get; set; } // Optional index
    }

    public abstract class InaraAPIEvent
    {
        public string eventName { get; set; }

        public string eventTimestamp => eventTimeStamp.ToString("s") + "Z";

        public object eventData { get; set; }

        public int eventCustomID { get; set; } // Optional index

        // Helper properties

        [JsonIgnore]
        public DateTime eventTimeStamp { get; set; }

        public InaraAPIEvent()
        { }
    }
}
