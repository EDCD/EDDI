﻿using Cottle.Values;
using Eddi;
using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiEvents;
using EddiShipMonitor;
using EddiSpeechService;
using EddiStatusMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// A responder that responds to events with a speech
    /// </summary>
    public class SpeechResponder : EDDIResponder
    {
        // The file to log speech
        public static readonly string LogFile = Constants.DATA_DIR + @"\speechresponder.out";

        private ScriptResolver scriptResolver;

        private bool subtitles;

        private bool subtitlesOnly;

        protected static List<Event> eventQueue = new List<Event>();
        private static readonly object queueLock = new object();

        public string ResponderName()
        {
            return "Speech responder";
        }

        public string LocalizedResponderName()
        {
            return Properties.SpeechResponder.name;
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return Properties.SpeechResponder.desc;
        }

        public SpeechResponder()
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            Personality personality = null;
            if (configuration != null && configuration.Personality != null)
            {
                personality = Personality.FromName(configuration.Personality);
            }
            if (personality == null)
            {
                personality = Personality.Default();
            }
            scriptResolver = new ScriptResolver(personality?.Scripts);
            subtitles = configuration?.Subtitles ?? false;
            subtitlesOnly = configuration?.SubtitlesOnly ?? false;
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        /// <summary>
        /// Change the personality for the speech responder
        /// </summary>
        /// <returns>true if the speech responder is now using the new personality, otherwise false</returns>
        public bool SetPersonality(string newPersonality)
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            if (newPersonality == configuration.Personality)
            {
                // Already set to this personality
                return true;
            }

            // Ensure that this personality exists
            Personality personality = Personality.FromName(newPersonality);
            if (personality != null)
            {
                // Yes it does; use it
                configuration.Personality = newPersonality;
                configuration.ToFile();
                scriptResolver = new ScriptResolver(personality.Scripts);
                Logging.Debug("Changed personality to " + newPersonality);
                return true;
            }
            else
            {
                // No it does not; ignore it
                return false;
            }
        }

        public bool Start()
        {
            EDDI.Instance.State["speechresponder_quiet"] = false;
            return true;
        }

        public void Stop()
        {
            EDDI.Instance.State["speechresponder_quiet"] = true;
        }

        public void Reload()
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            Personality personality = Personality.FromName(configuration.Personality);
            if (personality == null)
            {
                Logging.Warn("Failed to find named personality; falling back to default");
                personality = Personality.Default();
                configuration.Personality = personality.Name;
                configuration.ToFile();
            }
            scriptResolver = new ScriptResolver(personality.Scripts);
            subtitles = configuration.Subtitles;
            subtitlesOnly = configuration.SubtitlesOnly;
            Logging.Debug("Reloaded " + ResponderName() + " " + ResponderVersion());
        }

        public void Handle(Event @event)
        {
            if (@event.fromLoad)
            {
                return;
            }

            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));
            StatusMonitor statusMonitor = (StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor");

            if (@event is BodyScannedEvent bodyScannedEvent)
            {
                if (bodyScannedEvent.scantype.Contains("NavBeacon"))
                {
                    // Suppress scan details from nav beacons
                    return;
                }
            }
            else if (@event is StarScannedEvent starScannedEvent)
            {
                if (starScannedEvent.scantype.Contains("NavBeacon"))
                {
                    // Suppress scan details from nav beacons
                    return;
                }
                else if (EDDI.Instance.CurrentStarSystem?.bodies?
                    .FirstOrDefault(s => s.bodyname == starScannedEvent.bodyname)?
                    .scanned < starScannedEvent.timestamp)
                {
                    // Suppress voicing new scans after the first scan occurrence
                    return;
                }
            }
            else if (@event is CommunityGoalEvent)
            {
                // Disable speech from the community goal event for the time being.
                return;
            }

            Say(@event);
        }

        private void Say(Event @event)
        {
            Say(scriptResolver, ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip(), @event.type, @event, null, null, SayOutLoud());
        }

        public bool SayOutLoud()
        {
            // By default we say things unless we've been told not to
            bool sayOutLoud = true;
            if (EDDI.Instance.State.TryGetValue("speechresponder_quiet", out object tmp))
            {
                if (tmp is bool)
                {
                    sayOutLoud = !(bool)tmp;
                }
            }
            return sayOutLoud;
        }

        // Say something with the default resolver
        public void Say(Ship ship, string scriptName, Event theEvent = null, int? priority = null, string voice = null, bool sayOutLoud = true, bool invokedFromVA = false)
        {
            Say(scriptResolver, ship, scriptName, theEvent, priority, voice, sayOutLoud, invokedFromVA);
        }

        // Say something with a custom resolver
        public void Say(ScriptResolver resolver, Ship ship, string scriptName, Event theEvent = null, int? priority = null, string voice = null, bool sayOutLoud = true, bool invokedFromVA = false)
        {
            Dictionary<string, Cottle.Value> dict = createVariables(theEvent);
            string speech = resolver.resolve(scriptName, dict);
            if (speech != null)
            {
                if (subtitles)
                {
                    // Log a tidied version of the speech
                    string tidiedSpeech = Regex.Replace(speech, "<.*?>", string.Empty).Trim();
                    if (!string.IsNullOrEmpty(tidiedSpeech))
                    {
                        log(tidiedSpeech);
                    }
                }
                if (sayOutLoud && !(subtitles && subtitlesOnly))
                {
                    SpeechService.Instance.Say(ship, speech, (priority == null ? resolver.priority(scriptName) : (int)priority), voice, false, theEvent?.type, invokedFromVA);
                }
            }
        }

        // Create Cottle variables from the EDDI information
        private Dictionary<string, Cottle.Value> createVariables(Event theEvent = null)
        {
            Dictionary<string, Cottle.Value> dict = new Dictionary<string, Cottle.Value>
            {
                ["capi_active"] = CompanionAppService.Instance?.active ?? false,
                ["destinationdistance"] = EDDI.Instance.DestinationDistanceLy,
                ["environment"] = EDDI.Instance.Environment,
                ["horizons"] = EDDI.Instance.inHorizons,
                ["va_active"] = App.FromVA,
                ["vehicle"] = EDDI.Instance.Vehicle
            };

            if (EDDI.Instance.Cmdr != null)
            {
                dict["cmdr"] = new ReflectionValue(EDDI.Instance.Cmdr);
            }

            if (EDDI.Instance.HomeStarSystem != null)
            {
                dict["homesystem"] = new ReflectionValue(EDDI.Instance.HomeStarSystem);
            }

            if (EDDI.Instance.HomeStation != null)
            {
                dict["homestation"] = new ReflectionValue(EDDI.Instance.HomeStation);
            }

            if (EDDI.Instance.SquadronStarSystem != null)
            {
                dict["squadronsystem"] = new ReflectionValue(EDDI.Instance.SquadronStarSystem);
            }

            if (EDDI.Instance.CurrentStarSystem != null)
            {
                dict["system"] = new ReflectionValue(EDDI.Instance.CurrentStarSystem);
            }

            if (EDDI.Instance.LastStarSystem != null)
            {
                dict["lastsystem"] = new ReflectionValue(EDDI.Instance.LastStarSystem);
            }

            if (EDDI.Instance.NextStarSystem != null)
            {
                dict["nextsystem"] = new ReflectionValue(EDDI.Instance.NextStarSystem);
            }

            if (EDDI.Instance.DestinationStarSystem != null)
            {
                dict["destinationsystem"] = new ReflectionValue(EDDI.Instance.DestinationStarSystem);
            }

            if (EDDI.Instance.DestinationStation != null)
            {
                dict["destinationstation"] = new ReflectionValue(EDDI.Instance.DestinationStation);
            }

            if (EDDI.Instance.CurrentStation != null)
            {
                dict["station"] = new ReflectionValue(EDDI.Instance.CurrentStation);
            }

            if (EDDI.Instance.CurrentStellarBody != null)
            {
                dict["body"] = new ReflectionValue(EDDI.Instance.CurrentStellarBody);
            }

            if (((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor"))?.currentStatus != null)
            {
                dict["status"] = new ReflectionValue(((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor"))?.currentStatus);
            }

            if (theEvent != null)
            {
                dict["event"] = new ReflectionValue(theEvent);
            }

            if (EDDI.Instance.State != null)
            {
                dict["state"] = ScriptResolver.buildState();
                Logging.Debug("State is " + JsonConvert.SerializeObject(EDDI.Instance.State));
            }

            // Obtain additional variables from each monitor
            foreach (EDDIMonitor monitor in EDDI.Instance.monitors)
            {
                IDictionary<string, object> monitorVariables = monitor.GetVariables();
                if (monitorVariables != null)
                {
                    foreach (string key in monitorVariables.Keys)
                    {
                        if (monitorVariables[key] == null)
                        {
                            dict.Remove(key);
                        }
                        else
                        {
                            dict[key] = new ReflectionValue(monitorVariables[key]);
                        }
                    }
                }
            }

            return dict;
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void OnClosingConfigurationTabItem()
        {
        }

        private static readonly object logLock = new object();
        private static void log(string speech)
        {
            lock (logLock)
            {
                try
                {
                    using (StreamWriter file = new StreamWriter(LogFile, true))
                    {
                        file.WriteLine(speech);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to write speech", ex);
                }
            }
        }

        private List<Event> TakeTypeFromEventQueue<T>()
        {
            List<Event> events = new List<Event>();
            lock (queueLock)
            {
                events = eventQueue.Where(e => e is T).ToList();
                eventQueue.RemoveAll(e => e is T);
                return events;
            }
        }

        private void AddToEventQueue(Event @event)
        {
            lock (queueLock)
            {
                eventQueue.Add(@event);
            }
        }
    }
}
