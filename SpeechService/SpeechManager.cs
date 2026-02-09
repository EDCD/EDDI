using EddiConfigService;
using EddiDataDefinitions;
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

namespace EddiSpeechService
{
    public class SpeechManager : IDisposable, INotifyPropertyChanged
    {
        private readonly SoundManager SoundManager;
        public readonly AudioManager AudioManager;

        private const float ActiveSpeechFadeOutMilliseconds = 250;
        private static readonly object activeSpeechLock = new object();
        private readonly ConcurrentDictionary<IWavePlayer, CancellationTokenSource> activeSpeechTS = new ConcurrentDictionary<IWavePlayer, CancellationTokenSource>();
        private static bool discardPendingSegments;
        public List<VoiceDetails> allVoices { get; }

        private readonly SystemSpeechSynthesizer systemSpeechSynth;
        private readonly WindowsMediaSynthesizer windowsMediaSynth;

        internal int activeSpeechPriority;

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

        public readonly SpeechQueue speechQueue = new SpeechQueue();

        public SpeechManager ()
        {
            SoundManager = new SoundManager();
            AudioManager = new AudioManager( SoundManager );
            
            var voiceStore = new HashSet<VoiceDetails>(); // Use a Hashset to ensure no duplicates

            FetchLexiconSchemas();

            // Windows.Media.SpeechSynthesis isn't available on older Windows versions so we must check if we have access
            try
            {
                if ( IsWindowsMediaSynthesizerSupported() )
                {
                    // Prep the Windows.Media.SpeechSynthesis synthesizer
                    windowsMediaSynth = WindowsMediaSynthesizer.CreateAsync( voiceStore ).GetAwaiter().GetResult();
                }
            }
            catch ( Exception e )
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
            allVoices = voiceStore.OrderBy( v => v.name ).ToList();
        }

        public void Dispose ()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void Dispose ( bool disposing )
        {
            if ( disposing )
            {
                systemSpeechSynth?.Dispose();
                if ( IsWindowsMediaSynthesizerSupported() )
                {
                    windowsMediaSynth?.Dispose();
                }
            }
        }

        private static bool IsWindowsMediaSynthesizerSupported ()
        {
            return OSInfo.TryGetWindowsVersion( out var osVersion ) &&
                   osVersion >= new System.Version( 10, 0, 17763, 0 );
        }

        private void FetchLexiconSchemas ()
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
                                SpeechFormatter.lexiconSchemas.Add( schema );
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

        private async Task PlaySpeechStreamAsync ( IWaveProvider provider, int priority )
        {
            var fadeProvider = new FadeInOutSampleProvider( provider.ToSampleProvider() );
            using ( var soundOut = SoundManager.GetSoundOut( fadeProvider.ToWaveProvider() ) )
            {
                if ( soundOut is null ) { return; }
                var cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await StartSpeechAsync( soundOut, priority, cancellationTokenSource ).ConfigureAwait(false);

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
                            await Task.Delay((int)fadeOutStartMs, cancellationTokenSource.Token).ConfigureAwait(false);
                            fadeProvider.BeginFadeOut(fadeOutMs);
                        }
                    }, cancellationTokenSource.Token);

                    // Wait for playback to finish or cancellation
                    while ( soundOut.PlaybackState == PlaybackState.Playing )
                    {
                        await Task.Delay( 10, cancellationTokenSource.Token ).ConfigureAwait(false);
                    }

                    // Ensure fade-out is complete
                    await fadeOutTask.ContinueWith( _ => { }, TaskScheduler.Default ).ConfigureAwait(false);
                }
                catch ( OperationCanceledException )
                {
                    // Fade out on cancellation
                    fadeProvider.BeginFadeOut( ActiveSpeechFadeOutMilliseconds );
                    // ReSharper disable once MethodSupportsCancellation
                    await Task.Delay( (int)ActiveSpeechFadeOutMilliseconds + 50 ).ConfigureAwait(false);
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
                    await Task.Delay( 10, cancellationTokenSource.Token ).ConfigureAwait(false);
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
                    OnPropertyChanged( nameof( eddiSpeaking ) );
                }
            }
            catch ( OperationCanceledException )
            {
                // Operation cancelled. End gracefully.
            }
        }

        public void StopCurrentSpeech ()
        {
            Logging.Debug( "Ending active speech." );
            try
            {
                discardPendingSegments = true;
                ICollection<IWavePlayer> keysToRemove;
                lock ( activeSpeechLock )
                {
                    keysToRemove = activeSpeechTS.Keys;
                }

                if ( keysToRemove.Any() )
                {
                    keysToRemove.AsParallel().ForAll( key =>
                    {
                        if ( activeSpeechTS.TryRemove( key, out var tokenSource ) )
                        {
                            tokenSource.Cancel();
                            if ( !tokenSource.Token.WaitHandle.WaitOne( 500 ) ) // Poll at 500ms
                            {
                                Logging.Warn( "Task cancellation timed out." );
                            }

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

        public void ShutUp ()
        {
            speechQueue.DequeueAllSpeech();
            StopCurrentSpeech();
        }

        public async Task SayAsync ( Ship ship, string message, int priority = 3, string voice = null, bool radio = false, string eventType = null, bool invokedFromVA = false )
        {
            // Skip empty speech and speech containing nothing except one or more pauses / breaks.
            message = SpeechFormatter.TrimSpeech( message );
            if ( string.IsNullOrEmpty( message ) ) { return; }

            // Queue the current speech
            var Configuration = ConfigService.Instance.speechServiceConfiguration;
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
            await StartOrContinueSpeakingAsync().ConfigureAwait(false);
        }

        private CancellationTokenSource speechCts;

        private async Task StartOrContinueSpeakingAsync ()
        {
            try
            {
                if ( !eddiSpeaking )
                {
                    speechCts = new CancellationTokenSource();
                    var token = speechCts.Token;

                    while ( speechQueue.hasSpeech && !token.IsCancellationRequested )
                    {
                        if ( speechQueue.TryDequeue( out var speech ) )
                        {
                            try
                            {
                                await SpeakAsync( speech ).ConfigureAwait( false );
                            }
                            catch ( Exception ex )
                            {
                                var dict = new Dictionary<string, object>
                                {
                                    { "Speech", JsonConvert.SerializeObject( speech ) }, { "Exception", ex }
                                };
                                Logging.Warn( "Failed to handle queued speech", dict );
                            }
                        }
                        else
                        {
                            break; // Exit the loop if the queue is empty
                        }
                        await Task.Yield(); // Yield to avoid blocking the task pool
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                Logging.Debug( "Speech task was cancelled." );
            }
            catch ( Exception ex )
            {
                Logging.Error( "Unexpected error in speech processing", ex );
            }
        }

        internal bool checkSpeechInterrupt ( int peekedSpeechPriority )
        {
            // Priority 0 speech (system messages) and priority 1 speech and will interrupt current speech
            // Priority 5 speech in interruptable by any higher priority speech. 
            return ( activeSpeechPriority > peekedSpeechPriority && peekedSpeechPriority <= 1 ) ||
                   ( activeSpeechPriority >= 5 && peekedSpeechPriority < 5 );
        }

        public Task SpeakAsync ( EddiSpeech speech )
        {
            var Configuration = ConfigService.Instance.speechServiceConfiguration;
            return SpeakAsync( speech.message, speech.voice, Configuration.EffectsLevel, speech.distortionLevel, speech.echoDelay, speech.priority, speech.radio );
        }

        public async Task SpeakAsync ( string speech, string defaultVoice, int fxLevel,
            int distortionLevel = 0, int echoDelay = 0, int priority = 3, bool radio = false )
        {
            if ( speech == null || speech.Trim() == "" ) { return; }

            // If the user wants to disable IPA then we remove any IPA phoneme tags here
            var config = ConfigService.Instance.speechServiceConfiguration;
            if ( config.DisableIpa && speech.Contains( "<phoneme" ) )
            {
                speech = SpeechFormatter.DisableIPA( speech );
            }

            discardPendingSegments = false;
            var segments = SpeechFormatter.SeparateSpeechSegments(speech);

            foreach ( var segment in segments )
            {
                if ( discardPendingSegments ) { break; }

                string voice = null;
                var statement = segment;

                var isAudio = segment.Contains("<audio"); // This is an audio file, we will disable voice effects processing
                if ( isAudio )
                {
                    SpeechFormatter.UnpackAudioTags( segment, out var fileName, out var async, out var volumeOverride );
                    try
                    {
                        // Play the audio, waiting for the audio to complete unless we're in async mode
                        await AudioManager.PlayAudioAsync( fileName, volumeOverride ).ConfigureAwait( !async );
                    }
                    catch ( OperationCanceledException )
                    {
                        // If cancelled, discard any pending speech segments.
                        discardPendingSegments = true;
                    }
                    catch ( FileNotFoundException fnfe )
                    {
                        await SayAsync( null, $"Audio file not found at {fnfe.FileName}.", 0 ).ConfigureAwait(false);
                        Logging.Warn( fnfe.Message, fnfe );
                    }
                    catch ( NotSupportedException e )
                    {
                        await SayAsync( null, "Audio file format not supported.", 0 ).ConfigureAwait(false);
                        Logging.Warn( $"Skipping unsupported audio file {fileName}.", e );
                    }
                    catch ( Exception e )
                    {
                        Logging.Error( "Audio playback failed.", e );
                    }

                    continue;
                }

                var isRadio = statement.Contains("<transmit") || radio;
                if ( isRadio )
                {
                    // This is a radio transmission, we will enable radio voice effects processing
                    statement = SpeechFormatter.StripRadioTags( statement );
                }

                var isVoice = statement.Contains("<voice");
                if ( isVoice )
                {
                    // This is a voice override
                    SpeechFormatter.UnpackVoiceTags( statement, out voice, out statement );
                }

                using ( var stream = getSpeechStream( voice ?? defaultVoice, statement ) )
                {
                    if ( stream == null )
                    {
                        Logging.Debug( "getSpeechStream() returned null; nothing to say" );
                        return;
                    }
                    if ( stream.Length < 50 )
                    {
                        Logging.Debug( "getSpeechStream() returned empty stream; nothing to say" );
                        return;
                    }
                    else
                    {
                        Logging.Debug( "Stream length is " + stream.Length );
                    }
                    Logging.Debug( "Seeking back to the beginning of the stream" );
                    stream.Seek( 0, SeekOrigin.Begin );

                    var provider = SpeechFx.addEffectsToSource(stream, fxLevel, distortionLevel, echoDelay, isRadio );
                    await PlaySpeechStreamAsync( provider, priority ).ConfigureAwait(false);
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
            var Configuration = ConfigService.Instance.speechServiceConfiguration;
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
            var Configuration = ConfigService.Instance.speechServiceConfiguration;
            
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }
}