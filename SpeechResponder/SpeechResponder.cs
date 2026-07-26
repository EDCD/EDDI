using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiSpeechResponder.ScriptResolverService;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiSpeechResponder
{
    /// <summary>
    /// A responder that responds to events with a speech
    /// </summary>
    public class SpeechResponder : IEddiResponder, INotifyPropertyChanged
    {
        // The file to log speech
        private static readonly string LogFile = Constants.DATA_DIR + @"\speechresponder.out";

        public ObservableCollection<Personality> Personalities { get; private set; }

        /// <summary>
        /// Currently selected personality for the speech responder.
        /// Changes to this property automatically update configuration and trigger script resolver updates.
        /// WPF bindings are notified through INotifyPropertyChanged.
        /// </summary>
        public Personality CurrentPersonality
        {
            get => _currentPersonality;
            set
            {
                // Validate: ensure we have a valid personality
                var newPersonality = value ?? Personalities?.FirstOrDefault() ?? Personality.Default();

                SetProperty(ref _currentPersonality, newPersonality, nameof(CurrentPersonality));
                newPersonality.ApplyScriptPersonalityState();

                // Update derived properties
                ScriptResolver = new ScriptResolver(newPersonality.Scripts);
                Configuration.Personality = newPersonality.Name;
                ConfigService.Instance.speechResponderConfiguration = Configuration;
            }
        }
        private Personality _currentPersonality;

        public SpeechResponderConfiguration Configuration
        {
            get => _configuration;
            private set
            {
                if ( _configuration != null )
                {
                    ConfigService.Instance.speechResponderConfiguration = value;
                }
                _configuration = value;
            }
        }
        private SpeechResponderConfiguration _configuration;

        /// <summary>
        /// Resolver for parsing scripts with current personality's definitions.
        /// Updated automatically when CurrentPersonality changes.
        /// </summary>
        public ScriptResolver ScriptResolver
        {
            get => _scriptResolver ?? new ScriptResolver(CurrentPersonality.Scripts);
            private set => _scriptResolver = value;
        }
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
            TrySetPersonality(Configuration.Personality);
        }

        #region Personalities

        private ObservableCollection<Personality> GetPersonalities()
        {
            if (Personalities is not null) { return Personalities; }

            // Initialize our collection and add our default personality
            Personalities = [ Personality.Default() ];

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
        public bool TrySetPersonality(string newPersonalityName)
        {
            if ( !string.IsNullOrWhiteSpace( newPersonalityName ) && _currentPersonality?.Name.Equals( newPersonalityName, StringComparison.InvariantCultureIgnoreCase ) == true )
            {
                // Already set to this personality
                return true;
            }

            if ( string.IsNullOrWhiteSpace( newPersonalityName ) )
            {
                CurrentPersonality = Personalities.FirstOrDefault() ?? Personality.Default();
                Logging.Debug( $@"Personality set to '{CurrentPersonality.Name}'" );
                return true;
            }

            // Ensure that this personality exists
            var newPersonality = Personalities.FirstOrDefault(p =>
                p.Name.Equals(newPersonalityName, StringComparison.InvariantCultureIgnoreCase));
            if (newPersonality != null)
            {
                // Yes it does; use it
                CurrentPersonality = newPersonality;
                Logging.Debug( $@"Personality set to '{CurrentPersonality.Name}'" );
                return true;
            }

            // No it does not; fall back and log a warning
            CurrentPersonality = Personalities.FirstOrDefault() ?? Personality.Default();
            Logging.Warn( $@"Personality '{newPersonalityName}' not found, falling back to '{CurrentPersonality.Name}'." );
            return false;
        }

        internal void CopyCurrentPersonality(string personalityName, string personalityDescription, bool disableScripts)
        {
            var newPersonality = CurrentPersonality.Copy(personalityName?.Trim(), personalityDescription?.Trim());
            if (disableScripts) { EnableOrDisableAllScripts(newPersonality, false); }
            Personalities.Add(newPersonality);
            CurrentPersonality = newPersonality;
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

        internal static void EnableOrDisableAllScripts(Personality targetPersonality, bool desiredState)
        {
            if (targetPersonality?.Scripts is null) { return; }

            foreach (var kvScript in targetPersonality.Scripts)
            {
                var script = kvScript.Value;
                if (script.Responder)
                {
                    script.Enabled = desiredState;
                }
            }
            targetPersonality.ToFile();
        }

        public async Task TestScriptAsync(string scriptName, Dictionary<string, Script> scripts)
        {
            // See if we have a sample
            List<Event> sampleEvents;
            var sample = Events.SampleByName(scriptName);
            if (sample == null)
            {
                sampleEvents = [ ];
            }
            else if (sample is string s)
            {
                // It's a string so a journal entry.  Parse it
                sampleEvents = ( EDDI.Instance.ObtainMonitor( "Journal monitor" ) as IJournalEntryParser )
                    ?.ParseJournalEntry(s, deferSyntheticEvents: false) ?? [ ];
            }
            else if (sample is Event e)
            {
                // It's a direct event
                sampleEvents = [ e ];
            }
            else
            {
                Logging.Warn("Unknown sample type " + sample.GetType());
                sampleEvents = [ ];
            }

            var testScriptResolver = new ScriptResolver(scripts);
            if (sampleEvents.Count == 0)
            {
                sampleEvents.Add(null);
            }
            foreach (var sampleEvent in sampleEvents)
            {
                await SayAsync( testScriptResolver, null, scriptName, sampleEvent,
                    testScriptResolver.priority( scriptName ) ).ConfigureAwait( false );
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
            TrySetPersonality(Configuration.Personality);
            Logging.Debug($"Reloaded {ResponderName()}");
        }

        public async Task HandleAsync ( Event @event )
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

                if (EDDI.Instance.GameState.CurrentStarSystem?.bodies?
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

            await SayAsync(@event).ConfigureAwait( false );

            // Simulate a forced shutdown effect affecting the speech responder until the ship's system is rebooted
            if ( @event is ShipShutdownEvent shutdownEvent && !shutdownEvent.partialshutdown )
            {
                Logging.Debug( "Pausing speech during ship shutdown." );
                SpeechService.Instance.StopCurrentSpeech();
                SpeechService.Instance.speechQueue.Pause();
            }
        }

        private async Task SayAsync(Event @event)
        {
            Ship ship = null;
            if (EDDI.Instance.GameState.Vehicle == Constants.VEHICLE_SHIP)
            {
                ship = EDDI.Instance.GameState.CurrentShip;
            }
            await SayAsync( ScriptResolver, ship, @event.type, @event, null, null, SayOutLoud() )
                .ConfigureAwait( false );
            
            return;

            static bool SayOutLoud ()
            {
                // By default we say things unless we've been told not to
                var sayOutLoud = true;
                if ( EDDI.Instance.State.TryGetValue( "speechresponder_quiet", out var tmp ) )
                {
                    if ( tmp is bool b )
                    {
                        sayOutLoud = !b;
                    }
                }
                return sayOutLoud;
            }
        }

        // Say something with the default resolver
        public async Task SayAsync(Ship ship, string scriptName, Event theEvent = null, int? priority = null, string voice = null, bool sayOutLoud = true, bool invokedFromVA = false)
        {
            await SayAsync( ScriptResolver, ship, scriptName, theEvent, priority, voice, sayOutLoud, invokedFromVA ).ConfigureAwait( false );
        }

        // Say something with a custom resolver
        private async Task SayAsync(ScriptResolver resolver, Ship ship, string scriptName, Event theEvent = null, int? priority = null, string voice = null, bool sayOutLoud = true, bool invokedFromVA = false)
        {
            var dict = resolver.CompileVariables(theEvent);

            // Generate and enqueue speech
            try
            {
                var speech = resolver.resolveFromName(scriptName, dict, true);
                if ( speech != null && Configuration != null )
                {
                    if ( Configuration.Subtitles )
                    {
                        // Log a tidied version of the speech
                        var tidiedSpeech = GeneratedRegex.SsmlTagRegex().Replace(speech, string.Empty).Trim();
                        if ( !string.IsNullOrEmpty( tidiedSpeech ) )
                        {
                            log( tidiedSpeech );
                        }
                    }
                    if ( sayOutLoud && !( Configuration.Subtitles && Configuration.SubtitlesOnly ) )
                    {
                        Logging.Debug( $"Sending speach '{speech}' to SpeechService.", new Dictionary<string, object>()
                        {
                            { "Ship", ship },
                            { "ScriptName", scriptName },
                            { "Event", theEvent },
                            { "Priority", priority },
                            { "Voice", voice },
                            { "InvokedFromVA", invokedFromVA }
                        } );
                        await SpeechService.Instance.SayAsync( ship, speech,
                                priority ?? resolver.priority( scriptName ),
                                voice, false, theEvent?.type )
                            .ConfigureAwait( false );
                    }
                }
            }
            catch ( Exception e)
            {
                Logging.Error( e.Message, e );
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow(this);
        }

        public Task HandleStatusAsync ( Status status )
        {
            CustomFunctions.OrbitalVelocity.currentAltitudeMeters = status.altitude;
            return Task.CompletedTask;
        }

        private static readonly object logLock = new();

        private static void log(string speech)
        {
            lock (logLock)
            {
                try
                {
                    using (var file = new StreamWriter(LogFile, true))
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        private void SetProperty<T> ( ref T backingField, T value, [CallerMemberName] string propertyName = null )
        {
            if ( Equals( backingField, value ) )
            { return; }
            backingField = value;
            OnPropertyChanged( propertyName );
        }

        #endregion
    }
}
