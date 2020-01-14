﻿using Eddi;
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

#pragma warning disable IDE0052 // Remove unread private members: instantiating this registers the Cottle Highlighting Definition
        private readonly CottleHighlightingDefinition cottleHighlightingDefinition = new CottleHighlightingDefinition();
#pragma warning restore IDE0052 // Remove unread private members

        public string ResponderName()
        {
            return "Speech responder";
        }

        public string LocalizedResponderName()
        {
            return Properties.SpeechResponder.name;
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
            Logging.Info($"Initialized {ResponderName()}");
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
            Logging.Debug($"Reloaded {ResponderName()}");
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
            Dictionary<string, Cottle.Value> dict = resolver.createVariables(theEvent);
            string speech = resolver.resolveFromName(scriptName, dict);
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
        
        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
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
