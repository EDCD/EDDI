using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiSpeechResponder.ScriptResolverService;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiSpeechResponder
{
    /// <summary>
    /// A responder that responds to events with a speech
    /// </summary>
    public class SpeechResponder : IEddiResponder
    {
        // The file to log speech
        private static readonly string LogFile = Constants.DATA_DIR + @"\speechresponder.out";

        public ObservableCollection<Personality> Personalities { get; private set; }

        public Personality CurrentPersonality
        {
            get => _personality 
                   ?? Personalities.FirstOrDefault(p => p.Name == Configuration.Personality) 
                   ?? Personalities.FirstOrDefault()
                   ?? Personality.Default();
            set
            {
                // Set the updated personality
                if (_personality != value)
                {
                    _personality = value ?? Personalities.FirstOrDefault() ?? Personality.Default();
                    ScriptResolver = new ScriptResolver(_personality.Scripts);

                    // Update our configuration also
                    Configuration.Personality = _personality?.Name;
                    ConfigService.Instance.speechResponderConfiguration = Configuration;

                    RaiseOnUIThread(PersonalityChanged, _personality);
                }
            }
        }

        public static EventHandler PersonalityChanged;

        public SpeechResponderConfiguration Configuration
        {
            get => _configuration;
            private set
            {
                _configuration = value;
                ConfigService.Instance.speechResponderConfiguration = value;
            }
        }

        public ScriptResolver ScriptResolver
        {
            get => _scriptResolver ?? new ScriptResolver(CurrentPersonality.Scripts);
            private set => _scriptResolver = value;
        }

        private Personality _personality;
        private SpeechResponderConfiguration _configuration;
        private ScriptResolver _scriptResolver;

        public string ResponderName()
        {
            return Properties.SpeechResponder.ResourceManager.GetString( "name", CultureInfo.InvariantCulture );
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
            Configuration = ConfigService.Instance.speechResponderConfiguration;
            Personalities = GetPersonalities();
            SetPersonality(Configuration.Personality);
        }

        #region Personalities

        private ObservableCollection<Personality> GetPersonalities()
        {
            if (!(Personalities is null)) { return Personalities; }

            // Initialize our collection and add our default personality
            Personalities = new ObservableCollection<Personality> { Personality.Default() };

            // Add our custom personalities
            foreach (var customPersonality in Personality.AllFromDirectory())
            {
                if (customPersonality != null)
                {
                    Personalities.Add(customPersonality);
                }
            }
            return Personalities;
        }

        /// <summary>
        /// Change the personality for the speech responder
        /// </summary>
        /// <returns>true if the speech responder is now using the new personality, otherwise false</returns>
        public bool SetPersonality(string newPersonalityName)
        {
            if (newPersonalityName == Configuration?.Personality)
            {
                // Already set to this personality
                return true;
            }

            // Ensure that this personality exists
            var newPersonality = Personalities.FirstOrDefault(p =>
                p.Name.Equals(newPersonalityName, StringComparison.InvariantCultureIgnoreCase));
            if (newPersonality != null)
            {
                // Yes it does; use it
                CurrentPersonality = newPersonality;
                Logging.Debug($"Personality set to \"{newPersonality.Name}\"");
                return true;
            }

            // No it does not; ignore it
            Logging.Warn($"Personality \"{newPersonalityName}\" not found.");
            return false;
        }

        internal void CopyCurrentPersonality(string personalityName, string personalityDescription, bool disableScripts)
        {
            var newPersonality = CurrentPersonality.Copy(personalityName?.Trim(), personalityDescription?.Trim());
            if (disableScripts) { EnableOrDisableAllScripts(newPersonality, false); }
            Personalities.Add(newPersonality);
        }

        internal void RemoveCurrentPersonality()
        {
            // Remove the personality from the list and the local filesystem
            var oldPersonality = CurrentPersonality;
            Personalities.Remove(oldPersonality);
            oldPersonality.RemoveFile();
        }

        internal void SavePersonality()
        {
            if (CurrentPersonality is null) { return; }
            CurrentPersonality.ToFile();
        }

        #endregion

        #region Scripts

        internal void EnableOrDisableAllScripts(Personality targetPersonality, bool desiredState)
        {
            foreach (var kvScript in targetPersonality.Scripts)
            {
                var script = kvScript.Value;
                if (script.Responder)
                {
                    script.Enabled = desiredState;
                }
            }
            SavePersonality();
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
            else if (sample is string s)
            {
                // It's a string so a journal entry.  Parse it
                sampleEvents = JournalMonitor.ParseJournalEntry(s, false, true);
            }
            else if (sample is Event e)
            {
                // It's a direct event
                sampleEvents = new List<Event>{e};
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
                Say(testScriptResolver, null, scriptName, sampleEvent, testScriptResolver.priority(scriptName));
            }
        }

        #endregion

        public bool Start()
        {
            EDDI.Instance.State["speechresponder_quiet"] = false;
            Logging.Info( $"Initialized {ResponderName()}" );
            return true;
        }

        public void Stop()
        {
            EDDI.Instance.State["speechresponder_quiet"] = true;
            SpeechService.Instance.ShutUp();
            SpeechService.Instance.StopAudio();
        }

        public void Reload()
        {
            Configuration = ConfigService.Instance.speechResponderConfiguration;
            Personalities = GetPersonalities();
            SetPersonality(Configuration.Personality);
            Logging.Debug($"Reloaded {ResponderName()}");
        }

        public void Handle(Event @event)
        {
            if (@event.fromLoad)
            {
                return;
            }

            if (@event is BodyScannedEvent bodyScannedEvent)
            {
                if (bodyScannedEvent.scantype?.Contains("NavBeacon") ?? false)
                {
                    // Suppress scan details from nav beacons
                    return;
                }
            }
            else if (@event is StarScannedEvent starScannedEvent)
            {
                if (starScannedEvent.scantype?.Contains("NavBeacon") ?? false)
                {
                    // Suppress scan details from nav beacons
                    return;
                }
                else if (EDDI.Instance.CurrentStarSystem?.bodies?
                    .FirstOrDefault(s => s.bodyname == starScannedEvent.bodyname)?
                    .scannedDateTime < starScannedEvent.timestamp)
                {
                    // Suppress voicing new scans after the first scan occurrence
                    return;
                }
            }

            // Restore speech after a forced shutdown effect affecting the speech responder
            if ( @event is ShipShutdownRebootEvent )
            {
                Logging.Debug( "Unpausing speech after ship shutdown." );
                SpeechService.Instance.speechQueue.Unpause();
            }

            Say(@event);

            // Simulate a forced shutdown effect affecting the speech responder until the ship's system is rebooted
            if ( @event is ShipShutdownEvent shipShutdownEvent && !shipShutdownEvent.partialshutdown  )
            {
                Logging.Debug( "Pausing speech during ship shutdown." );
                SpeechService.Instance.StopCurrentSpeech();
                SpeechService.Instance.speechQueue.Pause();
            }
        }

        private void Say(Event @event)
        {
            Ship ship = null;
            if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
            {
                ship = EDDI.Instance.CurrentShip;
            }
            Say(ScriptResolver, ship, @event.type, @event, null, null, SayOutLoud());
        }

        private static bool SayOutLoud()
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
            Say(ScriptResolver, ship, scriptName, theEvent, priority, voice, sayOutLoud, invokedFromVA);
        }

        // Say something with a custom resolver
        private void Say(ScriptResolver resolver, Ship ship, string scriptName, Event theEvent = null, int? priority = null, string voice = null, bool sayOutLoud = true, bool invokedFromVA = false)
        {
            var dict = resolver.CompileVariables(theEvent);
            string speech = resolver.resolveFromName(scriptName, dict, true);
            if (speech != null && Configuration != null)
            {
                if (Configuration.Subtitles)
                {
                    // Log a tidied version of the speech
                    string tidiedSpeech = Regex.Replace(speech, "<.*?>", string.Empty).Trim();
                    if (!string.IsNullOrEmpty(tidiedSpeech))
                    {
                        log(tidiedSpeech);
                    }
                }
                if (sayOutLoud && !(Configuration.Subtitles && Configuration.SubtitlesOnly))
                {
                    Logging.Debug($"Sending speach '{speech}' to SpeechService.", new Dictionary<string, object>()
                    {
                        { "Ship", ship },
                        { "ScriptName", scriptName },
                        { "Event", theEvent }, 
                        { "Priority", priority }, 
                        { "Voice", voice },
                        { "InvokedFromVA", invokedFromVA }
                    });
                    SpeechService.Instance.Say(ship, speech, priority ?? resolver.priority(scriptName), voice, false, theEvent?.type, invokedFromVA);
                }
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow(this);
        }

        public void HandleStatus ( Status status )
        {
            CustomFunctions.OrbitalVelocity.currentAltitudeMeters = status.altitude;
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

        static void RaiseOnUIThread(EventHandler handler, object sender)
        {
            if (handler != null)
            {
                SynchronizationContext uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
                if (uiSyncContext == null)
                {
                    handler(sender, EventArgs.Empty);
                }
                else
                {
                    uiSyncContext.Send(delegate { handler(sender, EventArgs.Empty); }, null);
                }
            }
        }
    }
}
