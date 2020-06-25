using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiInaraService
{
    public partial class InaraService : IInaraService
    {
        // API Documentation: https://inara.cz/inara-api-docs/

        // Constants
        private const string readonlyAPIkey = "9efrgisivgw8kksoosowo48kwkkw04skwcgo840";
        private const int startupDelayMilliSeconds = 1000 * 10; // 10 seconds
        private const int syncIntervalMilliSeconds = 1000 * 60 * 5; // 5 minutes
        private const int delayedSyncIntervalMilliSeconds = 1000 * 60 * 60; // 60 minutes

        // Variables
        private static bool tooManyRequests;
        public static bool bgSyncRunning { get; private set; } // This must be static so that it is visible to child threads and tasks
        private static readonly BlockingCollection<InaraAPIEvent> queuedAPIEvents = new BlockingCollection<InaraAPIEvent>();
        private static readonly List<string> invalidAPIEvents = new List<string>();
        public static EventHandler invalidAPIkey;

        public void Start(bool eddiIsBeta = false)
        {
            if (!bgSyncRunning)
            {
                Logging.Debug("Starting Inara service background sync.");
                Task.Run(() => BackgroundSync(eddiIsBeta)).ConfigureAwait(false);
            }
        }

        public void Stop()
        {
            if (bgSyncRunning)
            {
                Logging.Debug("Stopping Inara service background sync.");
                bgSyncRunning = false;
            }
        }

        private async void BackgroundSync(bool eddiIsBeta)
        {
            // Pause a short time to allow any initial events to build in the queue before our first sync
            await Task.Delay(startupDelayMilliSeconds).ConfigureAwait(false);

            bgSyncRunning = true;
            while (bgSyncRunning)
            {
                await Task.Run(() => SendQueuedAPIEvents(eddiIsBeta)).ConfigureAwait(false);
                await Task.Delay(!tooManyRequests ? syncIntervalMilliSeconds : delayedSyncIntervalMilliSeconds).ConfigureAwait(false);
                tooManyRequests = false;
            }
        }

        // If you need to do some testing on Inara's API, please set the `isDeveloped` boolean header property to true.
        public List<InaraResponse> SendEventBatch(List<InaraAPIEvent> events, InaraConfiguration inaraConfiguration, bool eddiIsBeta = false)
        {
            // We always want to return a list from this method (even if it's an empty list) rather than a null value.
            List<InaraResponse> inaraResponses = new List<InaraResponse>();

            try
            {
                if (inaraConfiguration is null) { inaraConfiguration = InaraConfiguration.FromFile(); }
                List<InaraAPIEvent> indexedEvents = IndexAndFilterAPIEvents(events, inaraConfiguration);
                if (indexedEvents.Count > 0)
                {
                    var client = new RestClient("https://inara.cz/inapi/v1/");
                    var request = new RestRequest(Method.POST);
                    InaraSendJson inaraRequest = new InaraSendJson()
                    {
                        header = new Dictionary<string, object>()
                        {
                            // Per private conversation with Artie and per Inara API docs, the `isDeveloped` property
                            // should (counterintuitively) be set to true when the an application is in development.
                            // Quote: `it's "true" because the app "is (being) developed"`
                            // Quote: `isDeveloped is meant as "the app is currently being developed and may be broken`
                            { "appName", "EDDI" },
                            { "appVersion", Constants.EDDI_VERSION.ToString() },
                            { "isDeveloped", eddiIsBeta },
                            { "commanderName", !string.IsNullOrEmpty(inaraConfiguration?.commanderName) ? inaraConfiguration.commanderName : (eddiIsBeta ? "TestCmdr" : null) },
                            { "commanderFrontierID", !string.IsNullOrEmpty(inaraConfiguration?.commanderFrontierID) ? inaraConfiguration.commanderFrontierID : null },
                            { "APIkey", !string.IsNullOrEmpty(inaraConfiguration?.apiKey) ? inaraConfiguration.apiKey : readonlyAPIkey }
                        },
                        events = indexedEvents
                    };
                    request.RequestFormat = DataFormat.Json;
                    request.AddBody(inaraRequest); // uses JsonSerializer

                    Logging.Debug("Sending to Inara: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<InaraResponses>(request);
                    if (clientResponse.IsSuccessful)
                    {
                        InaraResponses response = clientResponse.Data;
                        if (validateResponse(response.header, indexedEvents, true))
                        {
                            foreach (InaraResponse inaraResponse in response.events)
                            {
                                if (validateResponse(inaraResponse, indexedEvents))
                                {
                                    inaraResponses.Add(inaraResponse);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Inara may return null as it undergoes a nightly maintenance cycle where the servers go offline temporarily.
                        Logging.Warn("Unable to connect to the Inara server.", clientResponse.ErrorMessage);
                        ReEnqueueAPIEvents(events);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Sending data to the Inara server failed.", ex);
            }
            return inaraResponses;
        }

        private static List<InaraAPIEvent> IndexAndFilterAPIEvents(List<InaraAPIEvent> events, InaraConfiguration inaraConfiguration)
        {
            // Flag each event with a unique ID we can use when processing responses
            List<InaraAPIEvent> indexedEvents = new List<InaraAPIEvent>();
            for (int i = 0; i < events.Count; i++)
            {
                InaraAPIEvent indexedEvent = events[i];
                indexedEvent.eventCustomID = i;

                // Exclude and discard events with issues that have returned a code 400 error in this instance.
                if (invalidAPIEvents.Contains(indexedEvent.eventName)) { continue; }

                // Exclude and discard old / stale events
                if (inaraConfiguration?.lastSync > indexedEvent.eventTimeStamp) { continue; }

                // Note: the Inara Responder does not queue events while the game is in beta.

                indexedEvents.Add(indexedEvent);
            }

            return indexedEvents;
        }

        private bool validateResponse(InaraResponse inaraResponse, List<InaraAPIEvent> indexedEvents, bool header = false)
        {
            // 200 - Ok
            if (inaraResponse.eventStatus == 200) { return true; }

            // Anything else - something is wrong.
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "InaraAPIEvent", indexedEvents.Find(e => e.eventCustomID == inaraResponse.eventCustomID) },
                { "InaraResponse", inaraResponse },
                { "Stacktrace", new StackTrace() }
            };
            try
            {
                // 202 - Warning (everything is OK, but there may be multiple results for the input properties, etc.)
                // 204 - 'Soft' error (everything was formally OK, but there are no results for the properties set, etc.)
                if (inaraResponse.eventStatus == 202 || inaraResponse.eventStatus == 204)
                {
                    Logging.Warn("Inara responded with: " + (inaraResponse.eventStatusText ?? "(No response)"), JsonConvert.SerializeObject(data));
                }
                // Other errors
                else if (!string.IsNullOrEmpty(inaraResponse.eventStatusText))
                {
                    if (header)
                    {
                        Logging.Warn("Inara responded with: " + (inaraResponse.eventStatusText ?? "(No response)"), JsonConvert.SerializeObject(data));
                        if (inaraResponse.eventStatusText.Contains("Invalid API key"))
                        {
                            ReEnqueueAPIEvents(indexedEvents);
                            // The Inara API key has been rejected. We'll note and remember that.
                            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
                            inaraConfiguration.isAPIkeyValid = false;
                            inaraConfiguration.ToFile();
                            // Send internal events to the Inara Responder and the UI to handle the invalid API key appropriately
                            invalidAPIkey?.Invoke(inaraConfiguration, new EventArgs());
                        }
                        else if (inaraResponse.eventStatusText.Contains("access to API was temporarily revoked"))
                        {
                            // Note: This can be thrown by over-use of the readonly API key.
                            ReEnqueueAPIEvents(indexedEvents);
                            tooManyRequests = true;
                        }
                    }
                    else
                    {
                        // There may be an issue with a specific API event.
                        // We'll add that API event to a list and omit sending that event again in this instance.
                        Logging.Error("Inara responded with: " + inaraResponse.eventStatusText, data);
                        invalidAPIEvents.Add(indexedEvents.Find(e => e.eventCustomID == inaraResponse.eventCustomID).eventName);
                    }
                }
                else
                {
                    // Inara responded, but no status text description was given.
                    Logging.Error("Inara responded with: ", data);
                }
                return false;
            }
            catch (Exception e)
            {
                data.Add("Exception", e);
                Logging.Error("Failed to handle Inara server response", data);
                return false;
            }
        }

        private bool checkAPIcredentialsOk(InaraConfiguration inaraConfiguration)
        {
            if (!inaraConfiguration.isAPIkeyValid)
            {
                Logging.Warn("Background sync skipped: API key is invalid.");
                invalidAPIkey?.Invoke(inaraConfiguration, new EventArgs());
                return false;
            }
            if (string.IsNullOrEmpty(inaraConfiguration.apiKey))
            {
                Logging.Info("Background sync skipped: API key not set.");
                return false;
            }
            if (string.IsNullOrEmpty(inaraConfiguration.commanderName))
            {
                Logging.Debug("Background sync skipped: Commander name not set.");
                return false;
            }
            return true;
        }

        public void SendQueuedAPIEvents(bool eddiIsBeta)
        {
            List<InaraAPIEvent> queue = new List<InaraAPIEvent>();
            // The `GetConsumingEnumerable` method blocks the thread while the underlying collection is empty
            // If we haven't extracted events to send to Inara, this will wait / pause background sync until `queuedAPIEvents` is no longer empty.
            foreach (var pendingEvent in queuedAPIEvents.GetConsumingEnumerable())
            {
                queue.Add(pendingEvent);
                if (queue.Count > 0 && queuedAPIEvents.Count == 0) { break; }
            }
            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            if (checkAPIcredentialsOk(inaraConfiguration))
            {
                var responses = SendEventBatch(queue, inaraConfiguration, eddiIsBeta);
                if (responses != null && responses.Count > 0)
                {
                    inaraConfiguration.lastSync = queue.Max(e => e.eventTimeStamp);
                    inaraConfiguration.ToFile();
                }
            }
        }

        public void EnqueueAPIEvent(InaraAPIEvent inaraAPIEvent)
        {
            if (inaraAPIEvent.eventName.StartsWith("get"))
            {
                Logging.Error("Cannot enqueue 'get' Inara API events as these require an immediate response. Send these directly.");
                return;
            }
            queuedAPIEvents.Add(inaraAPIEvent);
        }

        private void ReEnqueueAPIEvents(IEnumerable<InaraAPIEvent> inaraAPIEvents)
        {
            // Re-enqueue the data so we can send it again later.
            foreach (var inaraAPIEvent in inaraAPIEvents)
            {
                // Do not re-enqueue 'get' Inara API events
                if (inaraAPIEvent.eventName.StartsWith("get")) { continue; }
                // Clear any ID / index value assigned to the data
                inaraAPIEvent.eventCustomID = null;
                EnqueueAPIEvent(inaraAPIEvent);
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

        public int eventCustomID { get; set; } // Optional index. Used to match outgoing API events to responses.
    }
}
