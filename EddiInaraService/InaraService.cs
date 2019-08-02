using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using Utilities;

namespace EddiInaraService
{
    public class InaraService
    {
        public static string commanderFrontierApiName { get; set; }

        private string commanderName { get; set; }
        private string commanderFrontierID { get; set; }
        private string apiKey { get; set; }
        private static string baseUrl = "https://inara.cz/inapi/v1/";

        // For normal use, the Inara API base URL is https://www.edsm.net/.
        // If you need to do some testing on Inara's API, please set the `isDeveloped` boolean header property to false.
        public InaraService(string apikey, string commandername, string baseURL = "https://inara.cz/inapi/v1/")
        {
            apiKey = apikey?.Trim();
            commanderName = commandername?.Trim();
            baseUrl = baseURL;
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

                            // Set up the star map service
                            InaraConfiguration inaraCredentials = InaraConfiguration.FromFile();
                            if (!string.IsNullOrEmpty(inaraCredentials?.apiKey))
                            {
                                // Commander name might come from EDSM credentials or from the game and companion app
                                string commanderName = inaraCredentials?.commanderName ?? commanderFrontierApiName;
                                if (!string.IsNullOrEmpty(commanderName))
                                {
                                    instance = new InaraService(inaraCredentials.apiKey, commanderName);
                                    Logging.Info("Configuring EDDI access to Inara profile data");
                                }
                                else
                                {
                                    Logging.Warn("No InaraService instance: Commander name not set.");
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

        public void SendDataBatch(List<InaraAPIevent> events, bool inBeta)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return;
            }

            var client = new RestClient(baseUrl);
            var request = new RestRequest(Method.POST);

            Dictionary<string, object> header = new Dictionary<string, object>()
            {
                { "appName", "EDDI" },
                { "appVersion", Constants.EDDI_VERSION },
                { "isDeveloped", !inBeta },
                { "commanderName", commanderName },
                { "commanderFrontierID", commanderFrontierID },
                { "APIkey", apiKey }
            };
            request.AddParameter("header", header);
            request.AddParameter("events", events);

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending to Inara: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<InaraResponse>(request);
                    InaraResponse response = clientResponse.Data;
                    if (response?.eventStatus != 200)
                    {
                        if (response?.eventStatusText != null)
                        {
                            Logging.Warn("Inara responded with " + response?.eventStatusText);
                        }
                        else
                        {
                            Logging.Warn(clientResponse?.ErrorMessage);
                        }
                    }
                    Thread.Sleep(200); // Add a buffer to prevent time outs.
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to send event to Inara", ex);
                }
            })
            {
                IsBackground = true,
                Name = "Inara send"
            };
            thread.Start();
        }
    }

    internal class InaraResponse
    {
        public int eventStatus { get; set; }
        public string eventStatusText { get; set; }
        public object eventData { get; set; }
    }

    public class InaraAPIevent
    {
        public string eventName { get; set; }
        public string eventTimestamp => timestamp.ToString("s") + "Z";
        public DateTime timestamp { get; set; }
        public int eventCustomID { get; set; }
        public object eventData { get; set; }
    }
}
