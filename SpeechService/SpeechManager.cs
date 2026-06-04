using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService.SpeechPreparation;
using EddiSpeechService.SpeechSynthesizers;
using JetBrains.Annotations;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Utilities;

namespace EddiSpeechService
{
    public class SpeechManager ( AudioManager audioManager ) : IDisposable, INotifyPropertyChanged
    {
        private SystemSpeechSynthesizer systemSpeechSynth;
        private WindowsMediaSynthesizer windowsMediaSynth;

        private const float ActiveSpeechFadeOutMilliseconds = 250;
        private static readonly object activeSpeechLock = new();
        private readonly ConcurrentDictionary<IWavePlayer, CancellationTokenSource> activeSpeechTS = new();
        private static bool discardPendingSegments;
        public List<VoiceDetails> validatedVoices { get; private set; } = [ ];
        
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

        public readonly SpeechQueue speechQueue = new();

        public async Task InitializeAsync (CancellationToken ct = default)
        {
            Logging.Info("[InitializeAsync] SpeechManager initialization started.");
            var voiceStore = new HashSet<VoiceDetails>(); // Use a Hashset to ensure no duplicates

            // Windows.Media.SpeechSynthesis isn't available on older Windows versions so we must check if we have access
            try
            {
                if ( IsWindowsMediaSynthesizerSupported() )
                {
                    Logging.Info("[InitializeAsync] Windows Media Synthesizer is supported. Creating...");
                    // Prep the Windows.Media.SpeechSynthesis synthesizer
                    windowsMediaSynth = await WindowsMediaSynthesizer.CreateAsync( voiceStore, ct ).ConfigureAwait( false );
                    Logging.Info($"[InitializeAsync] Windows Media voices loaded. Current total store count: {voiceStore.Count}");
                }
                else
                {
                    Logging.Info("[InitializeAsync] Windows Media Synthesizer is not supported on this OS version.");
                }
            }
            catch ( Exception e )
            {
                Logging.Error( $"Unable to initialize Windows.Media.SpeechSynthesis.SpeechSynthesizer, {RuntimeInformation.OSDescription}", e );
            }

            // Prep the System.Speech synthesizer
            try
            {
                Logging.Info("[InitializeAsync] Initializing System Speech Synthesizer...");
                systemSpeechSynth = new SystemSpeechSynthesizer();
                systemSpeechSynth.Initialize( voiceStore );
                Logging.Info($"[InitializeAsync] System Speech voices loaded. Current total store count: {voiceStore.Count}");
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

            // Prep the Azure Speech SDK synthesizer
            try
            {
                var config = ConfigService.Instance.speechServiceConfiguration;
                Logging.Info($"[InitializeAsync] Azure Speech Config check: Key length = {config?.AzureApiKey?.Length ?? 0}, Region = '{config?.AzureRegion}'");
                if (!string.IsNullOrWhiteSpace(config.AzureApiKey) && !string.IsNullOrWhiteSpace(config.AzureRegion))
                {
                    Logging.Info("[InitializeAsync] Querying Azure Speech Services for neural voices...");
                    var azureVoices = await RetrieveAzureVoicesAsync(config.AzureApiKey, config.AzureRegion, ct).ConfigureAwait(false);
                    Logging.Info($"[InitializeAsync] Azure Speech query returned {azureVoices.Count} voices.");
                    foreach (var voice in azureVoices)
                    {
                        voiceStore.Add(voice);
                    }
                    Logging.Info($"[InitializeAsync] Loaded Azure voices. Current total store count: {voiceStore.Count}");
                }
            }
            catch (Exception e)
            {
                Logging.Error("Unable to initialize Azure Speech Synthesizer", e);
            }

            // Sort results alphabetically by voice name
            validatedVoices.Clear();
            validatedVoices.AddRange( voiceStore.OrderBy( v => v.name ) );
            Logging.Info($"[InitializeAsync] SpeechManager initialization completed. Total validatedVoices: {validatedVoices.Count}");
            foreach (var v in validatedVoices)
            {
                Logging.Info($"  - Voice: '{v.name}', gender: {v.gender}, culture: {v.culturecode}, synthType: {v.synthType}");
            }

            FetchLexiconSchemas();
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

        private static void FetchLexiconSchemas ()
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
                                if ( schema != null )
                                {
                                    SpeechFormatter.lexiconSchemas.Add( schema );
                                }
                            }
                            catch ( Exception e )
                            {
                                Logging.Warn( "Failed to initialize lexicon schema validation", e );
                            }
                        }
                    }
                }

                FetchSchemasFromResource( "EddiSpeechService.Properties.pls.xsd" );
                FetchSchemasFromResource( "EddiSpeechService.Properties.xml.xsd" );
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

                if ( keysToRemove.Count > 0 )
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

        public Task EnqueueAsync ( Ship ship, string message, int priority = 3, string voice = null,
            bool radio = false, string eventType = null )
        {
            Logging.Info($"[EnqueueAsync] Called. message='{message}', voice='{voice}', priority={priority}");
            // Skip empty speech and speech containing nothing except one or more pauses / breaks.
            message = SpeechFormatter.TrimSpeech( message );
            if ( string.IsNullOrEmpty( message ) )
            {
                Logging.Info("[EnqueueAsync] Message is empty after trimming. Skipping.");
                return Task.CompletedTask;
            }

            // Queue the current speech
            var config = ConfigService.Instance.speechServiceConfiguration;
            var queuingSpeech = new EddiSpeech( message, voice, priority, eventType, ship?.Size, ship?.health, radio,
                config.DistortOnDamage );
            Logging.Info($"[EnqueueAsync] Enqueuing speech object with voice='{queuingSpeech.voice}' to priority queue {queuingSpeech.priority}");
            speechQueue.Enqueue( queuingSpeech );

            // Check the first item in the speech queue
            // Interrupt current speech when appropriate
            if ( speechQueue.TryPeek( out var peekedSpeech ) && checkSpeechInterrupt( peekedSpeech.priority ) )
            {
                Logging.Info( "[EnqueueAsync] Interrupting current speech" );
                StopCurrentSpeech();
            }

            // Ensure the speech queue is running
            EnsureSpeechQueueRunning();

            return Task.CompletedTask;
        }

        private int speechQueueRunning;
        private CancellationTokenSource speechQueueCts = new();
        private void EnsureSpeechQueueRunning ()
        {
            if ( speechQueueCts.IsCancellationRequested )
            {
                speechQueueCts = new CancellationTokenSource();
            }

            if ( Interlocked.CompareExchange( ref speechQueueRunning, 1, 0 ) != 0 )
            {
                return;
            }

            Task.Run( () => SpeakFromQueueAsync( speechQueueCts.Token ) ).SafeFireAndForget(ex => {
            {
                if ( ex is OperationCanceledException )
                {
                    Logging.Debug( "Speech queue task was cancelled", ex );
                }
                else
                {
                    Logging.Error( "Unexpected error in speech processing", ex );
                }
            } });
        }

        private async Task SpeakFromQueueAsync ( CancellationToken token )
        {
            try
            {
                while ( !token.IsCancellationRequested )
                {
                    // If paused, TryDequeue will return false; wait and retry.
                    // SpeechQueue.TryDequeue returns false when paused. :contentReference[oaicite:4]{index=4}
                    if ( !speechQueue.TryDequeue( out var nextSpeech ) )
                    {
                        if ( !speechQueue.hasSpeech )
                        {
                            break; // queue empty
                        }
                        await Task.Delay( 50, token ).ConfigureAwait( false );
                        continue;
                    }

                    try
                    {
                        Logging.Info($"[SpeakFromQueueAsync] Dequeued next speech. message='{nextSpeech.message}', voice='{nextSpeech.voice}'");
                        await SpeakAsync( nextSpeech ).ConfigureAwait( false );
                    }
                    catch ( Exception ex )
                    {
                        Logging.Warn( "Failed to handle queued speech", new Dictionary<string, Exception> { { "Exception", ex } } );
                    }

                    await Task.Yield();
                }
            }
            catch ( OperationCanceledException ) { }
            finally
            {
                Interlocked.Exchange( ref speechQueueRunning, 0 );

                // Handle race: new speech arrived after we decided to exit.
                if ( speechQueue.hasSpeech && !speechQueue.isQueuePaused && !token.IsCancellationRequested )
                {
                    EnsureSpeechQueueRunning();
                }
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
            Logging.Info($"[SpeakAsync(EddiSpeech)] Called. voice='{speech.voice}', message='{speech.message}', priority={speech.priority}");
            return SpeakAsync( speech.message, speech.voice, Configuration.EffectsLevel, speech.distortionLevel, speech.echoDelay, speech.priority, speech.radio );
        }

        public async Task SpeakAsync ( string speech, string defaultVoice, int fxLevel,
            int distortionLevel = 0, int echoDelay = 0, int priority = 3, bool radio = false )
        {
            Logging.Info($"[SpeakAsync(string)] Called. speech='{speech}', defaultVoice='{defaultVoice}', fxLevel={fxLevel}, distortionLevel={distortionLevel}, echoDelay={echoDelay}");
            if ( speech == null || speech.Trim() == "" ) { return; }

            // If the user wants to disable IPA then we remove any IPA phoneme tags here
            var config = ConfigService.Instance.speechServiceConfiguration;
            if ( config.DisableIpa && speech.Contains( "<phoneme" ) )
            {
                speech = SpeechFormatter.DisableIPA( speech );
            }

            discardPendingSegments = false;
            var segments = SpeechFormatter.SeparateSpeechSegments(speech);
            Logging.Info($"[SpeakAsync(string)] Separated into {segments.Count} segments.");

            foreach ( var segment in segments )
            {
                if ( discardPendingSegments ) { 
                    Logging.Info("[SpeakAsync(string)] discardPendingSegments is true. Aborting loop.");
                    break; 
                }

                string voice = null;
                var statement = segment;

                var isAudio = segment.Contains("<audio"); // This is an audio file, we will disable voice effects processing
                if ( isAudio )
                {
                    SpeechFormatter.UnpackAudioTags( segment, out var fileName, out var async, out var volumeOverride );
                    try
                    {
                        // Play the audio, waiting for the audio to complete unless we're in async mode
                        await audioManager.PlayAudioAsync( fileName, volumeOverride ).ConfigureAwait( !async );
                    }
                    catch ( OperationCanceledException )
                    {
                        // If cancelled, discard any pending speech segments.
                        discardPendingSegments = true;
                    }
                    catch ( FileNotFoundException fnfe )
                    {
                        await EnqueueAsync( null, $"Audio file not found at {fnfe.FileName}.", 0 ).ConfigureAwait(false);
                        Logging.Warn( fnfe.Message, fnfe );
                    }
                    catch ( NotSupportedException e )
                    {
                        await EnqueueAsync( null, "Audio file format not supported.", 0 ).ConfigureAwait(false);
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
                    Logging.Info($"[SpeakAsync(string)] Found voice override tag. Voice='{voice}', statement='{statement}'");
                }

                var resolvedVoice = voice ?? defaultVoice;
                Logging.Info($"[SpeakAsync(string)] Calling getSpeechStream for voice='{resolvedVoice}' and statement='{statement}'");
                await using ( var stream = getSpeechStream( resolvedVoice, statement ) )
                {
                    if ( stream == null )
                    {
                        Logging.Info( "[SpeakAsync(string)] getSpeechStream() returned null; nothing to say" );
                        return;
                    }
                    if ( stream.Length < 50 )
                    {
                        Logging.Info( $"[SpeakAsync(string)] getSpeechStream() returned empty/short stream of length {stream.Length}; nothing to say" );
                        return;
                    }
                    else
                    {
                        Logging.Info( "[SpeakAsync(string)] Stream length is " + stream.Length );
                    }
                    Logging.Info( "[SpeakAsync(string)] Seeking back to the beginning of the stream" );
                    stream.Seek( 0, SeekOrigin.Begin );

                    Logging.Info($"[SpeakAsync(string)] Applying effects (fxLevel: {fxLevel}, distortionLevel: {distortionLevel}, echoDelay: {echoDelay}, isRadio: {isRadio})");
                    var provider = SpeechFx.addEffectsToSource(stream, fxLevel, distortionLevel, echoDelay, isRadio );
                    Logging.Info("[SpeakAsync(string)] Playing speech stream via PlaySpeechStreamAsync...");
                    await PlaySpeechStreamAsync( provider, priority ).ConfigureAwait(false);
                    Logging.Info("[SpeakAsync(string)] Playback completed.");
                }
            }
        }

        // Obtain the speech memory stream
        private Stream getSpeechStream ( string requestedVoice, string speech )
        {
            Logging.Info($"[getSpeechStream] requestedVoice='{requestedVoice}', speech='{speech}'");
            try
            {
                var stream = speak(requestedVoice, speech);
                if ( stream is null || stream.Length == 0 )
                {
                    Logging.Info("[getSpeechStream] speak() returned null or empty. Retrying with stripped SSML.");
                    // Try again, with speech devoid of SSML
                    stream = speak( requestedVoice, GeneratedRegex.SsmlTagRegex().Replace( speech, string.Empty ) );
                }
                Logging.Info($"[getSpeechStream] Returning stream of length {(stream != null ? stream.Length.ToString() : "null")} bytes.");
                return stream;
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Speech failed (" + Encoding.Default.EncodingName + ")", ex );
                var voiceDetails = validatedVoices.FirstOrDefault( v => v.name == requestedVoice );
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
            if ( voiceDetails.synthType == "Azure" )
            {
                return SpeakAzure( voiceDetails, speech, Configuration );
            }
            throw new NotImplementedException( $"{nameof( voiceDetails )} is referencing a synthType which has not been configured." );
        }

        private async Task<List<VoiceDetails>> RetrieveAzureVoicesAsync(string key, string region, CancellationToken ct)
        {
            Logging.Info($"[RetrieveAzureVoicesAsync] Called with key length {key?.Length ?? 0}, region '{region}'");
            var voices = new List<VoiceDetails>();
            try
            {
                var speechConfig = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(key, region);
                using var synthesizer = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(speechConfig, null);
                Logging.Info("[RetrieveAzureVoicesAsync] Calling GetVoicesAsync...");
                using var result = await synthesizer.GetVoicesAsync().ConfigureAwait(false);
                Logging.Info($"[RetrieveAzureVoicesAsync] GetVoicesAsync finished. Reason: {result.Reason}");
                if (result.Reason == Microsoft.CognitiveServices.Speech.ResultReason.VoicesListRetrieved)
                {
                    foreach (var voice in result.Voices)
                    {
                        var gender = voice.Gender.ToString(); // Male, Female, Neutral
                        var culture = CultureInfo.GetCultureInfo(voice.Locale);
                        voices.Add(new VoiceDetails(voice.ShortName, gender, culture, "Azure"));
                    }
                    Logging.Info($"[RetrieveAzureVoicesAsync] Successfully parsed {voices.Count} Azure voices.");
                }
                else
                {
                    Logging.Warn($"Failed to retrieve Azure voices. Reason: {result.Reason}");
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to query Azure voices list.", ex);
            }
            return voices;
        }

        private Stream SpeakAzure(VoiceDetails voiceDetails, string speech, SpeechServiceConfiguration Configuration)
        {
            Logging.Info($"[SpeakAzure] voiceDetails.name='{voiceDetails.name}', speech='{speech}'");
            
            string key = Configuration.AzureApiKey;
            string region = Configuration.AzureRegion;

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
            {
                Logging.Error("[SpeakAzure] Azure Speech credentials are not configured.");
                return null;
            }

            MemoryStream stream = null;
            var synthTask = Task.Run(async () =>
            {
                try
                {
                    Logging.Info("[SpeakAzure task] Initializing SpeechConfig...");
                    var speechConfig = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(key, region);
                    speechConfig.SpeechSynthesisVoiceName = voiceDetails.name;
                    
                    // Set output format to RIFF (WAV) format compatible with WaveFileReader
                    speechConfig.SetSpeechSynthesisOutputFormat(Microsoft.CognitiveServices.Speech.SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm);
                    
                    using var synthesizer = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(speechConfig, null);
                    
                    string preparedSpeech = speech;
                    SpeechFormatter.PrepareSpeech(voiceDetails, ref preparedSpeech, out var useSSML);
                    Logging.Info($"[SpeakAzure task] PreparedSpeech: '{preparedSpeech}', useSSML: {useSSML}");
                    
                    Microsoft.CognitiveServices.Speech.SpeechSynthesisResult result;
                    var ratePercent = Configuration.Rate * 10;
                    var rateString = ratePercent >= 0 ? $"+{ratePercent}%" : $"{ratePercent}%";

                    if (useSSML)
                    {
                        // Azure requires a <voice name="voiceName"> tag inside the <speak> element.
                        // We will insert it after the <speak> tag and before the closing </speak> tag,
                        // and wrap the contents in a <prosody> tag to set the volume and rate.
                        int speakIndex = preparedSpeech.IndexOf("<speak");
                        if (speakIndex >= 0)
                        {
                            int speakCloseIndex = preparedSpeech.IndexOf(">", speakIndex);
                            if (speakCloseIndex >= 0)
                            {
                                int voiceInsertionIndex = speakCloseIndex + 1;
                                int lastLexiconIndex = preparedSpeech.LastIndexOf("<lexicon");
                                if (lastLexiconIndex > speakCloseIndex)
                                {
                                    int lexiconCloseIndex = preparedSpeech.IndexOf("/>", lastLexiconIndex);
                                    if (lexiconCloseIndex >= 0)
                                    {
                                        voiceInsertionIndex = lexiconCloseIndex + 2;
                                    }
                                }

                                string beforeVoice = preparedSpeech.Substring(0, voiceInsertionIndex);
                                string afterVoice = preparedSpeech.Substring(voiceInsertionIndex);
                                
                                if (afterVoice.EndsWith("</speak>"))
                                {
                                    afterVoice = afterVoice.Substring(0, afterVoice.Length - "</speak>".Length);
                                }
                                
                                preparedSpeech = beforeVoice + $"<voice name=\"{voiceDetails.name}\"><prosody volume=\"{Configuration.Volume}\" rate=\"{rateString}\">" + afterVoice + "</prosody></voice></speak>";
                            }
                        }

                        Logging.Info("[SpeakAzure task] Calling SpeakSsmlAsync with prepared SSML...");
                        result = await synthesizer.SpeakSsmlAsync(preparedSpeech).ConfigureAwait(false);
                    }
                    else
                    {
                        var xmlEscapedSpeech = System.Security.SecurityElement.Escape(preparedSpeech);
                        var ssml = $"<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"{voiceDetails.culturecode ?? "en-US"}\">" +
                                   $"<voice name=\"{voiceDetails.name}\">" +
                                   $"<prosody volume=\"{Configuration.Volume}\" rate=\"{rateString}\">" +
                                   $"{xmlEscapedSpeech}" +
                                   $"</prosody></voice></speak>";
                        Logging.Info("[SpeakAzure task] Calling SpeakSsmlAsync with prepared text-to-SSML...");
                        result = await synthesizer.SpeakSsmlAsync(ssml).ConfigureAwait(false);
                    }
                    
                    using (result)
                    {
                        Logging.Info($"[SpeakAzure task] result.Reason: {result.Reason}");
                        if (result.Reason == Microsoft.CognitiveServices.Speech.ResultReason.SynthesizingAudioCompleted)
                        {
                            stream = new MemoryStream(result.AudioData);
                            Logging.Info($"[SpeakAzure task] SynthesizingAudioCompleted successfully. Stream length: {stream.Length} bytes.");
                        }
                        else if (result.Reason == Microsoft.CognitiveServices.Speech.ResultReason.Canceled)
                        {
                            var cancellation = Microsoft.CognitiveServices.Speech.SpeechSynthesisCancellationDetails.FromResult(result);
                            Logging.Error($"[SpeakAzure task] Azure synthesis canceled. Reason: {cancellation.Reason}, ErrorDetails: {cancellation.ErrorDetails}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("[SpeakAzure task] Exception in Azure speech synthesis task", ex);
                }
            });

            try
            {
                Logging.Info("[SpeakAzure] Waiting for synthTask...");
                synthTask.Wait();
                Logging.Info($"[SpeakAzure] synthTask completed. Returning stream: {(stream != null ? "length " + stream.Length : "null")}");
            }
            catch (AggregateException ae)
            {
                Logging.Error("[SpeakAzure] AggregateException in Azure speech synthesis", ae.InnerException ?? ae);
            }

            return stream;
        }

        public async Task ReloadVoicesAsync(CancellationToken ct = default)
        {
            await InitializeAsync(ct).ConfigureAwait(false);
        }

        private Stream speak ( string requestedVoice, string speech )
        {
            Logging.Info($"[speak(string, string)] Called. requestedVoice='{requestedVoice}'");
            // Get the voice details we will use for speaking
            if ( TryResolveVoice( requestedVoice, out var voiceDetails ) )
            {
                Logging.Info($"[speak(string, string)] TryResolveVoice returned true. voiceDetails.name='{voiceDetails.name}', synthType='{voiceDetails.synthType}'");
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
            Logging.Info($"[TryResolveVoice] Called. requestedVoice='{requestedVoice}', Configuration.StandardVoice='{Configuration.StandardVoice}'");
            
            // If the requestedVoice is null and the saved configuration's standard voice is not null,
            // try to re-resolve this once using the voice saved to the configuration.
            if ( string.IsNullOrEmpty( requestedVoice ) && !string.IsNullOrEmpty( Configuration.StandardVoice ) )
            {
                Logging.Info($"[TryResolveVoice] requestedVoice is null or empty. Falling back to Configuration.StandardVoice '{Configuration.StandardVoice}'");
                return TryResolveVoice( Configuration.StandardVoice, out voiceDetails );
            }

            // If the requested voice is not null and matches one we've previously found, return that voice.
            if ( !string.IsNullOrEmpty( requestedVoice ) )
            {
                var foundVoice = validatedVoices
                    .FirstOrDefault( v => string.Equals( v.name, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) );
                if ( foundVoice != null )
                {
                    voiceDetails = foundVoice;
                    Logging.Info($"[TryResolveVoice] Found matching voice '{foundVoice.name}' in validatedVoices");
                    return true;
                }
                else
                {
                    Logging.Info($"[TryResolveVoice] Voice '{requestedVoice}' was not found in validatedVoices. validatedVoices count is {validatedVoices.Count}");
                }
            }

            // If the requested voice was not found, try to re-resolve this once using the synthesizer's default voice.
            var synthDefaultVoice = IsWindowsMediaSynthesizerSupported()
                ? windowsMediaSynth?.currentVoice ?? systemSpeechSynth?.currentVoice
                : systemSpeechSynth?.currentVoice;
            Logging.Info($"[TryResolveVoice] requested voice '{requestedVoice}' not resolved. Attempting fallback to default voice '{synthDefaultVoice}'");
            if ( !string.IsNullOrEmpty( synthDefaultVoice ) &&
                 !string.Equals( synthDefaultVoice, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) )
            {
                Logging.Debug( $"Voice '{requestedVoice}' not found, falling back to voice '{synthDefaultVoice}'." );
                return TryResolveVoice( synthDefaultVoice, out voiceDetails );
            }

            // If none of the above then we've failed to select a voice from our voice list
            Logging.Info("[TryResolveVoice] Failed to resolve any voice.");
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