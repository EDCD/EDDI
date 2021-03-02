using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using EddiSpeechResponder.Service;
using EddiSpeechService;
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

        private Service.ScriptResolver scriptResolver;

        private bool subtitles;

        private bool subtitlesOnly;
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
            scriptResolver = new Service.ScriptResolver(personality?.Scripts);
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
                scriptResolver = new Service.ScriptResolver(personality.Scripts);
                Logging.Debug("Changed personality to " + newPersonality);
                return true;
            }
            else
            {
                // No it does not; ignore it
                return false;
            }
        }

        public void TestScript(string scriptName, Dictionary<string, Script> scripts)
        {
            // See if we have a sample
            List<Event> sampleEvents;
            object sample = Events.SampleByName(scriptName);
            if (sample == null)
            {
                sampleEvents = new List<Event>();
            }
            else if (sample is string)
            {
                // It's a string so a journal entry.  Parse it
                sampleEvents = JournalMonitor.ParseJournalEntry((string)sample);
            }
            else if (sample is Event)
            {
                // It's a direct event
                sampleEvents = new List<Event>() { (Event)sample };
            }
            else
            {
                Logging.Warn("Unknown sample type " + sample.GetType());
                sampleEvents = new List<Event>();
            }

            ScriptResolver testScriptResolver = new ScriptResolver(scripts);
            if (sampleEvents.Count == 0)
            {
                sampleEvents.Add(null);
            }
            foreach (Event sampleEvent in sampleEvents)
            {
                Say(testScriptResolver, ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip(), scriptName, sampleEvent, testScriptResolver.priority(scriptName));
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
            scriptResolver = new Service.ScriptResolver(personality.Scripts);
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
                if (tmp is bool b)
                {
                    sayOutLoud = !b;
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
        public void Say(Service.ScriptResolver resolver, Ship ship, string scriptName, Event theEvent = null, int? priority = null, string voice = null, bool sayOutLoud = true, bool invokedFromVA = false)
        {
            Dictionary<string, Cottle.Value> dict = resolver.createVariables(theEvent);
            string speech = resolver.resolveFromName(scriptName, dict, true);
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
    }
}
