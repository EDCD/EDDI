using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;

namespace EddiInaraService
{
    public partial class InaraService
    {
        // API Documenation: https://inara.cz/inara-api-docs/

        public string commanderName { get; private set; }
        public string commanderFrontierID { get; private set; }
        private string apiKey { get; set; }

        private const string readonlyAPIkey = "9efrgisivgw8kksoosowo48kwkkw04skwcgo840";

        private ConcurrentQueue<InaraAPIEvent> queuedAPIEvents { get; set; } = new ConcurrentQueue<InaraAPIEvent>();

        public InaraService(string apikey, string commandername = null, string commanderfrontierID = null)
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
                            // Set up the Inara service
                            InaraConfiguration inaraCredentials = InaraConfiguration.FromFile();
                            // commanderName: In-game CMDR name of user (not set by user, get this from journals or 
                            // cAPI to ensure it is a correct in-game name to avoid future problems). It is recommended 
                            // to be always set when no generic API key is used (otherwise some events may not work).
                            string commanderName = inaraCredentials?.commanderName;
                            // commanderFrontierID: Commander's unique Frontier ID (is provided by journals since 3.3)
                            // in the format: 'F123456'. When not known, set nothing.
                            string commanderFrontierID = inaraCredentials?.commanderFrontierID;
                            if (!string.IsNullOrEmpty(inaraCredentials.apiKey) && !string.IsNullOrEmpty(commanderName))
                            {
                                instance = new InaraService(inaraCredentials.apiKey ?? readonlyAPIkey, commanderName, commanderFrontierID);
                                Logging.Info("Configuring EDDI access to Inara profile data");
                            }
                            else
                            {
                                instance = new InaraService(readonlyAPIkey);
                                if (string.IsNullOrEmpty(inaraCredentials.apiKey))
                                {
                                    Logging.Warn("Configuring Inara service for limited access: API key not set.");
                                }
                                if (string.IsNullOrEmpty(commanderName))
                                {
                                    Logging.Warn("Configuring Inara service for limited access: Commander name not set.");
                                }
                            }
                        }
                    }
                }
                return instance;
            }
        }

        public static void Reload()
        {
            Logging.Debug("Creating new Inara service instance.");
            instance = null;
            _ = Instance;
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

        public async void SendQueuedAPIEventsAsync(bool inBeta)
        {
            List<InaraAPIEvent> queue = new List<InaraAPIEvent>();
            while (Instance.queuedAPIEvents.TryDequeue(out InaraAPIEvent pendingEvent))
            {
                queue.Add(pendingEvent);
            }
            if (queue.Count > 0)
            {
                await Task.Run(() => Instance.SendEventBatch(ref queue, inBeta));
                InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
                inaraConfiguration.lastSync = DateTime.UtcNow;
                inaraConfiguration.ToFile();
            }
        }

        public void EnqueueAPIEvent(InaraAPIEvent inaraAPIEvent)
        {
            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            if (inaraAPIEvent.eventTimeStamp > inaraConfiguration.lastSync)
            {
                Instance.queuedAPIEvents.Enqueue(inaraAPIEvent);
            }
        }

        public void DequeueAPIEventsOfType(string eventName)
        {
            List<InaraAPIEvent> queue = new List<InaraAPIEvent>();
            while (Instance.queuedAPIEvents.TryDequeue(out InaraAPIEvent pendingEvent))
            {
                if (pendingEvent.eventName != eventName)
                {
                    queue.Add(pendingEvent);
                }
            }
            foreach (InaraAPIEvent inaraAPIEvent in queue)
            {
                Instance.EnqueueAPIEvent(inaraAPIEvent);
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

    public class InaraAPIEvent
    {
        public string eventName { get; set; }

        public string eventTimestamp => eventTimeStamp.ToString("s") + "Z";

        public object eventData { get; set; }

        public int? eventCustomID { get; set; } // Optional index

        // Helper properties

        [JsonIgnore]
        public DateTime eventTimeStamp { get; set; }

        public InaraAPIEvent(DateTime eventTimeStamp, string eventName, Dictionary<string, object> eventData, int? eventCustomID = null)
        {
            this.eventTimeStamp = eventTimeStamp;
            this.eventName = eventName;
            this.eventData = eventData;
            this.eventCustomID = eventCustomID;
        }

        public InaraAPIEvent(DateTime eventTimeStamp, string eventName, List<Dictionary<string, object>> eventData, int? eventCustomID = null)
        {
            this.eventTimeStamp = eventTimeStamp;
            this.eventName = eventName;
            this.eventData = eventData;
            this.eventCustomID = eventCustomID;
        }
    }
}
