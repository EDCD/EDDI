using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiSpeechService.SpeechEffects;
using EddiSpeechService.SpeechPreparation;
using EddiSpeechService.SpeechSynthesizers;
using JetBrains.Annotations;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiSpeechService
{
    /// <summary>Provide speech services with a varying amount of alterations to the voice</summary>
    public class SpeechService : INotifyPropertyChanged, IDisposable
    {
        private const float ActiveSpeechFadeOutMilliseconds = 250;

        public SpeechServiceConfiguration Configuration
        {
            get => configuration;
            set
            {
                if (configuration != value)
                {
                    configuration = value;
                    OnPropertyChanged();
                }
            }
        }
        private SpeechServiceConfiguration configuration;

        private readonly SystemSpeechSynthesizer systemSpeechSynth;
        private readonly WindowsMediaSynthesizer windowsMediaSynth;

        public List<VoiceDetails> allVoices { get; }
        public List<string> allvoices => allVoices
            .Where(v => !v.hideVoice)
            .Select(v => v.name)
            .ToList();

        internal readonly XmlSchemaSet lexiconSchemas = new XmlSchemaSet();

        private static readonly object activeAudioLock = new object();
        private static readonly object activeSpeechLock = new object();

        internal int activeSpeechPriority;
        private static bool discardPendingSegments;

        private readonly ConcurrentDictionary<IWavePlayer, CancellationTokenSource> activeSpeechTS = new ConcurrentDictionary<IWavePlayer, CancellationTokenSource>();
        private readonly ConcurrentDictionary<IWavePlayer, CancellationTokenSource> activeAudioTS = new ConcurrentDictionary<IWavePlayer, CancellationTokenSource>();

        public readonly SpeechQueue speechQueue = new SpeechQueue();

        public bool eddiSpeaking
        {
            get
            {
                lock ( activeSpeechLock )
                {
                    return !activeSpeechTS.IsEmpty;
                }
            }
        }

        public bool eddiAudioPlaying
        {
            get
            {
                lock ( activeAudioLock )
                {
                    return !activeAudioTS.IsEmpty;
                }
            }
        }

        private static SpeechService instance;
        private static readonly object instanceLock = new object();
        public static SpeechService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No Speech service instance: creating one");
                            instance = new SpeechService();
                        }
                    }
                }
                return instance;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                systemSpeechSynth?.Dispose();
                if ( IsWindowsMediaSynthesizerSupported() )
                {
                    windowsMediaSynth?.Dispose();
                }
            }
        }

        public SpeechService()
        {
            Configuration = SpeechServiceConfiguration.FromFile();
            var voiceStore = new HashSet<VoiceDetails>(); // Use a Hashset to ensure no duplicates

            FetchLexiconSchemas();

            // Windows.Media.SpeechSynthesis isn't available on older Windows versions so we must check if we have access
            try
            {
                if (IsWindowsMediaSynthesizerSupported())
                {
                    // Prep the Windows.Media.SpeechSynthesis synthesizer
                    windowsMediaSynth = new WindowsMediaSynthesizer(ref voiceStore);
                }
            }
            catch (Exception e)
            {
                Logging.Error( $"Unable to initialize Windows.Media.SpeechSynthesis.SpeechSynthesizer, {RuntimeInformation.OSDescription}", e );
            }
            
            // Prep the System.Speech synthesizer
            try
            {
                systemSpeechSynth = new SystemSpeechSynthesizer( ref voiceStore );
            }
            catch ( ThreadAbortException )
            {
                // Nothing to do here
            }
            catch ( Exception e )
            {
                Logging.Error(
                    $"Unable to initialize System.Speech.Synthesis.SpeechSynthesizer, {RuntimeInformation.OSDescription}",
                    e );
            }

            // Sort results alphabetically by voice name
            allVoices = voiceStore.OrderBy(v => v.name).ToList();

            // Monitor and respond appropriately to changes in the state of the CompanionAppService
            CompanionAppService.Instance.StateChanged += CompanionAppService_StateChanged;
        }

        private void FetchLexiconSchemas()
        {
            // Try to obtain and load lexicon related schemas for lexicon schema validation
            try
            {
                var thisAssembly = Assembly.GetExecutingAssembly();

                void FetchSchemasFromResource ( string resourceName )
                {
                    using ( var resourceStream = thisAssembly.GetManifestResourceStream( resourceName ) )
                    {
                        if ( resourceStream != null )
                        {
                            try
                            {
                                var schema = XmlSchema.Read( resourceStream, null );
                                lexiconSchemas.Add( schema );
                            }
                            catch ( Exception e )
                            {
                                Logging.Warn( "Failed to initialize lexicon schema validation", e );
                            }
                        }
                    }
                }

                FetchSchemasFromResource( "EddiSpeechService.Properties.pls.xsd" );
            }
            catch ( ArgumentException ae )
            {
                Logging.Warn( "Unable to load lexicon validation schema.", ae );
            }
            catch ( XmlSchemaException xmle )
            {
                Logging.Warn( $"Problem with lexicon validation schema at {xmle.SourceUri}", xmle );
            }
        }

        private static bool IsWindowsMediaSynthesizerSupported()
        {
            return OSInfo.TryGetWindowsVersion( out var osVersion ) &&
                   osVersion >= new System.Version( 10, 0, 17763, 0 );
        }

        private void CompanionAppService_StateChanged(CompanionAppService.State oldState, CompanionAppService.State newState)
        {
            if (newState == CompanionAppService.State.ConnectionLost && !CompanionAppService.unitTesting)
            {
                Say(null, EddiCompanionAppService.Properties.CapiResources.frontier_api_lost, 0);
            }
        }

        public void Say(Ship ship, string message, int priority = 3, string voice = null, bool radio = false, string eventType = null, bool invokedFromVA = false )
        {
            // Skip empty speech and speech containing nothing except one or more pauses / breaks.
            message = SpeechFormatter.TrimSpeech(message);
            if (string.IsNullOrEmpty(message)) { return; }

            // Queue the current speech
            var queuingSpeech = new EddiSpeech( message, voice, priority, eventType, ship?.Size, ship?.health, radio, Configuration.DistortOnDamage );
            speechQueue.Enqueue( queuingSpeech );

            // Check the first item in the speech queue
            if ( speechQueue.TryPeek( out var peekedSpeech ) )
            {
                // Interrupt current speech when appropriate
                if ( checkSpeechInterrupt( peekedSpeech.priority ) )
                {
                    Logging.Debug( "Interrupting current speech" );
                    StopCurrentSpeech();
                }
            }

            // Start or continue speaking from the speech queue
            Instance.StartOrContinueSpeaking( invokedFromVA );
        }

        private Task speechTask;
        private CancellationTokenSource speechCts;

        private void StartOrContinueSpeaking ( bool invokedFromVA )
        {
            try
            {
                if ( !eddiSpeaking && ( speechTask == null || speechTask.IsCompleted ) )
                {
                    speechCts = new CancellationTokenSource();
                    var token = speechCts.Token;

                    speechTask = Task.Run( async () =>
                    {
                        while ( speechQueue.hasSpeech && !token.IsCancellationRequested )
                        {
                            if ( speechQueue.TryDequeue( out var speech ) )
                            {
                                try
                                {
                                    Speak( speech );
                                }
                                catch ( Exception ex )
                                {
                                    Logging.Error( "Failed to handle queued speech", ex );
                                    Logging.Warn( $"Failed to handle speech {JsonConvert.SerializeObject( speech )}" );
                                }
                            }

                            await Task.Yield(); // Yield to avoid blocking the thread pool
                        }
                    }, token );

                    if ( invokedFromVA )
                    {
                        // When invoked from VoiceAttack, the executing command can block other commands until the executing command completes.
                        // We'll adopt the same behavior by waiting for the speech task to complete.
                        speechTask.GetAwaiter().GetResult();
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                Logging.Debug( "Speech task was cancelled." );
            }
        }

        internal bool checkSpeechInterrupt ( int peekedSpeechPriority )
        {
            // Priority 0 speech (system messages) and priority 1 speech and will interrupt current speech
            // Priority 5 speech in interruptable by any higher priority speech. 
            if ( ( activeSpeechPriority > peekedSpeechPriority && peekedSpeechPriority <= 1 ) || ( activeSpeechPriority >= 5 && peekedSpeechPriority < 5 ) )
            {
                return true;
            }
            return false;
        }

        public void Speak(EddiSpeech speech)
        {
            Instance.Speak(speech.message, speech.voice, Configuration.EffectsLevel, Configuration.Volume, speech.distortionLevel, speech.echoDelay, speech.priority, speech.radio );
        }

        public void Speak ( string speech, string defaultVoice, int fxLevel, int volume = 95, 
            int distortionLevel = 0, int echoDelay = 0, int priority = 3, bool radio = false )
        {
            if (speech == null || speech.Trim() == "") { return; }

            // If the user wants to disable IPA then we remove any IPA phoneme tags here
            if (Configuration.DisableIpa && speech.Contains("<phoneme"))
            {
                speech = SpeechFormatter.DisableIPA(speech);
            }

            discardPendingSegments = false;
            var segments = SpeechFormatter.SeparateSpeechSegments(speech);

            foreach (var segment in segments)
            {
                if ( discardPendingSegments )  { continue; }

                string voice = null;
                var statement = segment;

                var isAudio = segment.Contains("<audio"); // This is an audio file, we will disable voice effects processing
                if (isAudio)
                {
                    SpeechFormatter.UnpackAudioTags(segment, out var fileName, out var async, out var volumeOverride);
                    try
                    {
                        // Play the audio, waiting for the audio to complete unless we're in async mode
                        if ( async )
                        {
                            Task.Run( () => PlayAudio( fileName, volumeOverride ) );
                        }
                        else
                        {
                            try
                            {
                                Task.Run( () => PlayAudio( fileName, volumeOverride ) ).GetAwaiter().GetResult();
                            }
                            catch ( OperationCanceledException )
                            {
                                // If cancelled, discard any pending speech segments.
                                discardPendingSegments = true;
                            }
                        }
                    }
                    catch ( Exception e )
                    {
                        Logging.Warn( e.Message, e );
                    }

                    continue;
                }

                var isRadio = statement.Contains("<transmit") || radio; 
                if (isRadio)
                {
                    // This is a radio transmission, we will enable radio voice effects processing
                    statement = SpeechFormatter.StripRadioTags( statement );
                }

                var isVoice = statement.Contains("<voice"); 
                if (isVoice)
                {
                    // This is a voice override
                    SpeechFormatter.UnpackVoiceTags( statement, out voice, out statement );
                }

                using (var stream = getSpeechStream(voice ?? defaultVoice, statement))
                {
                    if (stream == null)
                    {
                        Logging.Debug("getSpeechStream() returned null; nothing to say");
                        return;
                    }
                    if (stream.Length < 50)
                    {
                        Logging.Debug("getSpeechStream() returned empty stream; nothing to say");
                        return;
                    }
                    else
                    {
                        Logging.Debug("Stream length is " + stream.Length);
                    }
                    Logging.Debug("Seeking back to the beginning of the stream");
                    stream.Seek(0, SeekOrigin.Begin);

                    var provider = SpeechFx.addEffectsToSource(stream, volume, fxLevel, distortionLevel, echoDelay, isRadio );
                    PlaySpeechStreamAsync( provider, priority ).GetAwaiter().GetResult();
                }
            }
        }
        
        // Obtain the speech memory stream
        private Stream getSpeechStream ( string requestedVoice, string speech )
        {
            try
            {
                var stream = speak(requestedVoice, speech);
                if ( stream is null || stream.Length == 0 )
                {
                    // Try again, with speech devoid of SSML
                    stream = speak( requestedVoice, Regex.Replace( speech, "<.*?>", string.Empty ) );
                }
                return stream;
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Speech failed (" + Encoding.Default.EncodingName + ")", ex );
                var voiceDetails = allVoices.FirstOrDefault( v => v.name == requestedVoice );
                if ( voiceDetails?.synthType is nameof( Windows.Media ) && requestedVoice != windowsMediaSynth.currentVoice )
                {
                    // Try falling back to our Windows Media default voice.
                    Logging.Warn( $"{ex.Message}, retrying with Windows Media Synthesizer default voice.", ex );
                    return getSpeechStream( windowsMediaSynth.currentVoice, speech );
                }
                if ( requestedVoice != systemSpeechSynth?.currentVoice )
                {
                    // Try falling back to our System Speech default voice.
                    Logging.Warn( $"{ex.Message}, retrying with System Speech Synthesizer default voice.", ex );
                    return getSpeechStream( systemSpeechSynth?.currentVoice, speech );
                }
            }
            return null;
        }

        private Stream speak ( [NotNull] VoiceDetails voiceDetails, string speech )
        {
            if ( voiceDetails.synthType is nameof( System ) )
            {
                return systemSpeechSynth?.Speak( voiceDetails, speech, Configuration );
            }
            if ( voiceDetails.synthType is nameof( Windows.Media ) && IsWindowsMediaSynthesizerSupported() )
            {
                return windowsMediaSynth?.Speak( voiceDetails, speech, Configuration );
            }
            throw new NotImplementedException( $"{nameof( voiceDetails )} is referencing a synthType which has not been configured." );
        }

        private Stream speak ( string requestedVoice, string speech )
        {
            // Get the voice details we will use for speaking
            if ( TryResolveVoice( requestedVoice, out var voiceDetails ) )
            {
                try
                {
                    return speak( voiceDetails, speech );
                }
                catch ( Exception e )
                {
                    Logging.Error( e.Message, e );
                }
            }
            Logging.Warn( $"Something went wrong. Unable to obtain voice {requestedVoice}." );
            return null;
        }

        /// <summary>
        /// Match and normalize the requested voice against one from our speech synthesizers.
        /// </summary>
        /// <param name="requestedVoice"></param>
        /// <param name="voiceDetails"></param>
        /// <returns>Returns true if we were able to resolve synthesizer voice details for the requested voice</returns>
        private bool TryResolveVoice ( string requestedVoice, out VoiceDetails voiceDetails )
        {
            // If the requestedVoice is null and the saved configuration's standard voice is not null,
            // try to re-resolve this once using the voice saved to the configuration.
            if ( string.IsNullOrEmpty( requestedVoice ) && !string.IsNullOrEmpty( Configuration.StandardVoice ) )
            {
                return TryResolveVoice( Configuration.StandardVoice, out voiceDetails );
            }

            // If the requested voice is not null and matches one we've previously found, return that voice.
            if ( !string.IsNullOrEmpty( requestedVoice ) )
            {
                var foundVoice = allVoices
                    .FirstOrDefault( v => string.Equals( v.name, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) );
                if ( foundVoice != null )
                {
                    voiceDetails = foundVoice;
                    return true;
                }
            }

            // If the requested voice was not found, try to re-resolve this once using the synthesizer's default voice.
            var synthDefaultVoice = IsWindowsMediaSynthesizerSupported()
                ? windowsMediaSynth?.currentVoice ?? systemSpeechSynth?.currentVoice
                : systemSpeechSynth?.currentVoice;
            if ( !string.IsNullOrEmpty( synthDefaultVoice ) &&
                 !string.Equals( synthDefaultVoice, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) )
            {
                Logging.Debug( $"Voice '{requestedVoice}' not found, falling back to voice '{synthDefaultVoice}'." );
                return TryResolveVoice( synthDefaultVoice, out voiceDetails );
            }

            // If none of the above then we've failed to select a voice from our voice list
            voiceDetails = null;
            return false;
        }

        private IWavePlayer GetSoundOut ( IWaveProvider provider )
        {
            // Try WASAPI first
            try
            {
                var wasapiOut = new WasapiOut();
                if ( TryInitializeSoundOut( wasapiOut, provider ) )
                {
                    return wasapiOut;
                }
                Logging.Warn( "Falling back to legacy DirectSoundOut." );
            }
            catch ( Exception ex )
            {
                Logging.Warn( "WASAPI output initialization failed, falling back to DirectSoundOut.", ex );
            }

            // Fallback: DirectSoundOut
            try
            {
                var directSoundOut = new DirectSoundOut();
                if ( TryInitializeSoundOut( directSoundOut, provider ) )
                {
                    return directSoundOut;
                }
            }
            catch ( Exception ex )
            {
                Logging.Warn( "DirectSoundOut output initialization failed.", ex );
            }

            Logging.Warn( "Unable to initialize any playback device." );
            return null;
        }

        private static bool TryInitializeSoundOut ( IWavePlayer soundOut, IWaveProvider provider )
        {
            try
            {
                soundOut.Init( provider );
            }
            catch ( COMException ce )
            {
                Logging.Warn( $"Failed to initialize. {ce.Source} not registered. Installation may be corrupt or Windows version may be incompatible. ", ce );
                return false;
            }
            catch ( InvalidCastException ice )
            {
                Logging.Warn( $"Failed to initialize. {ice.Message} ", ice );
                return false;
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Failed to initialize sound output: {ex.Message}", ex );
                return false;
            }
            return true;
        }

        #region Speech

        private async Task PlaySpeechStreamAsync ( IWaveProvider provider, int priority )
        {
            var fadeProvider = new FadeInOutSampleProvider( provider.ToSampleProvider() );
            using ( var soundOut = GetSoundOut( fadeProvider.ToWaveProvider() ) )
            {
                if ( soundOut is null ) { return; }
                var cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await StartSpeechAsync( soundOut, priority, cancellationTokenSource );

                    // Estimate total duration in milliseconds
                    var totalDurationMs = (provider as WaveStream)?.TotalTime.TotalMilliseconds
                                          ?? 0;
                    var fadeOutMs = (int)ActiveSpeechFadeOutMilliseconds;
                    var fadeOutStartMs = Math.Max(0, totalDurationMs - fadeOutMs);

                    // Start fade-out before playback ends
                    var fadeOutTask = Task.Run(async () =>
                    {
                        if (fadeOutStartMs > 0)
                        {
                            await Task.Delay((int)fadeOutStartMs, cancellationTokenSource.Token);
                            fadeProvider.BeginFadeOut(fadeOutMs);
                        }
                    }, cancellationTokenSource.Token);

                    // Wait for playback to finish or cancellation
                    while ( soundOut.PlaybackState == PlaybackState.Playing )
                    {
                        await Task.Delay( 10, cancellationTokenSource.Token );
                    }

                    // Ensure fade-out is complete
                    await fadeOutTask.ContinueWith( _ => { }, TaskScheduler.Default );
                }
                catch ( OperationCanceledException )
                {
                    // Fade out on cancellation
                    fadeProvider.BeginFadeOut( ActiveSpeechFadeOutMilliseconds );
                    // ReSharper disable once MethodSupportsCancellation
                    await Task.Delay( (int)ActiveSpeechFadeOutMilliseconds );
                }
                catch ( Exception e )
                {
                    Logging.Error( "Speech playback failed.", e );
                }

                // Dispose of completed speech
                lock ( activeSpeechLock )
                {
                    if ( activeSpeechTS.TryRemove( soundOut, out var ts ) )
                    {
                        ts.Dispose();
                    }

                    OnPropertyChanged( nameof( eddiSpeaking ) );
                }
            }
        }

        private async Task StartSpeechAsync ( IWavePlayer soundOut, int priority, CancellationTokenSource cancellationTokenSource )
        {
            try
            {
                // Wait for any currently playing speech to finish
                while ( eddiSpeaking )
                {
                    await Task.Delay( 10, cancellationTokenSource.Token );
                }

                lock ( activeSpeechLock )
                {
                    // Track the active speech output and its cancellation token
                    activeSpeechTS.TryAdd( soundOut, cancellationTokenSource );

                    // Set the current speech priority
                    activeSpeechPriority = priority;

                    Logging.Debug( "Setting active speech and playing sound buffer" );

                    // Start playback
                    soundOut.Play();

                    // Notify listeners that speech state has changed
                    OnPropertyChanged( nameof(eddiSpeaking) );
                }
            }
            catch ( OperationCanceledException )
            {
                // Operation cancelled. End gracefully.
            }
        }

        public void StopCurrentSpeech()
        {
            lock ( activeSpeechLock )
            {
                Logging.Debug( "Ending active speech." );
                try
                {
                    discardPendingSegments = true;
                    if ( activeSpeechTS.Keys is ICollection<IWavePlayer> keysToRemove  && keysToRemove.Any() )
                    {
                        keysToRemove.AsParallel().ForAll( key =>
                        {
                            if ( activeSpeechTS.TryRemove( key, out var tokenSource ) )
                            {
                                tokenSource.Cancel();
                                tokenSource.Token.WaitHandle.WaitOne( TimeSpan.FromSeconds( 5 ) );
                                tokenSource.Dispose();
                            }
                        } );
                    }
                }
                catch ( Exception e )
                {
                    Logging.Warn( e.Message, e );
                }
                OnPropertyChanged( nameof( eddiSpeaking ) );
            }
        }

        public void ShutUp ()
        {
            speechQueue.DequeueAllSpeech();
            StopCurrentSpeech();
        }

        #endregion

        #region Audio

        public void PlayAudio ( string fileName, decimal? volumeOverride )
        {
            try
            {
                string absolutePath = Files.GetAbsoluteFilePath(Constants.DATA_DIR, fileName);
                using ( var audioSource = new AudioFileReader( absolutePath ) )
                using ( var soundOut = GetSoundOut( audioSource ) )
                {
                    if ( soundOut == null ) { return; }

                    Logging.Debug( $"Beginning audio playback for {fileName}." );

                    if ( volumeOverride != null )
                    {
                        audioSource.Volume = Math.Max( Math.Min( (float)volumeOverride / 100, 1 ), 0 );
                    }

                    soundOut.Play();

                    var cancellationTokenSource = new CancellationTokenSource();
                    lock ( activeAudioLock )
                    {
                        activeAudioTS.TryAdd( soundOut, cancellationTokenSource );
                    }

                    try
                    {
                        var waitTime = audioSource.TotalTime;
                        Logging.Debug( $"Waiting for audio - {waitTime.TotalMilliseconds} ms (unless ended early)." );
                        Task.Delay( waitTime, cancellationTokenSource.Token ).GetAwaiter().GetResult();
                    }
                    catch ( OperationCanceledException )
                    {
                        // Graceful exit on cancellation
                    }

                    Logging.Debug( $"Ending audio playback for {fileName}." );
                    lock ( activeAudioLock )
                    {
                        if ( activeAudioTS.TryRemove( soundOut, out var ts ) )
                        {
                            ts.Dispose();
                        }
                    }
                }
            }
            catch ( FileNotFoundException fnfe )
            {
                Say( null, $"Audio file not found at {fnfe.FileName}.", 0 );
                Logging.Warn( fnfe.Message, fnfe );
            }
            catch ( NotSupportedException e )
            {
                Say( null, "Audio file format not supported.", 0 );
                Logging.Warn( $"Skipping unsupported audio file {fileName}.", e );
            }
            catch ( Exception e )
            {
                Logging.Error( "Audio playback failed.", e );
            }
        }

        public void StopAudio ()
        {
            Logging.Debug( $"Ending all audio playback." );
            discardPendingSegments = true;
            lock (activeAudioLock)
            {
                foreach ( var soundOut in activeAudioTS.Keys )
                {
                    if ( activeAudioTS.TryRemove( soundOut, out var ts ) )
                    {
                        ts.Cancel();
                        ts.Token.WaitHandle.WaitOne( TimeSpan.FromSeconds( 5 ) );
                        ts.Dispose();
                    };
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [JetBrains.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Utilities.PublicAPI]
    public class VoiceDetails : IEquatable<VoiceDetails>
    {
        [Utilities.PublicAPI]
        public string name { get; }

        [Utilities.PublicAPI]
        public string gender { get; }

        [Utilities.PublicAPI]
        public string culturecode { get; }

        public string synthType { get; }

        [Utilities.PublicAPI]
        public string cultureinvariantname { get; }

        [Utilities.PublicAPI]
        public string culturename { get; }

        public bool hideVoice { get; set; }

        internal string cultureTwoLetterISOLanguageName;
        internal string cultureIetfLanguageTag;

        internal VoiceDetails( string displayName, string gender, CultureInfo Culture, string synthType )
        {
            this.name = displayName;
            this.gender = gender;
            this.cultureinvariantname = Culture.EnglishName;
            this.culturename = Culture.NativeName;
            this.cultureTwoLetterISOLanguageName = Culture.TwoLetterISOLanguageName;
            this.cultureIetfLanguageTag = Culture.IetfLanguageTag;
            this.synthType = synthType;

            culturecode = BestGuessCulture(Culture);
        }

        private string BestGuessCulture(CultureInfo Culture)
        {
            string guess;
            if (name.Contains("CereVoice"))
            {
                // Cereproc voices do not support the normal xml:lang attribute country/region codes (like en-GB) 
                // (see https://www.cereproc.com/files/CereVoiceCloudGuide.pdf), 
                // but it does support two letter country codes so we will use those instead
                guess = Culture.Parent.Name;
            }
            else
            {
                // Trust the voice's information (with the complete country/region code)
                guess = Culture.Name;
            }
            Logging.Debug($"Best guess culture for {name} is {guess}"); return guess;
        }

        // Implement IEquatable
        public bool Equals(VoiceDetails other)
        {
            return name == other?.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
