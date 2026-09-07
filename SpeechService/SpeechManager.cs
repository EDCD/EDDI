using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService.SpeechPreparation;
using EddiSpeechService.SpeechProviders;
using EddiSpeechService.SpeechSynthesizers;
using JetBrains.Annotations;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechService
{
    public class SpeechManager : IDisposable, INotifyPropertyChanged
    {
        private readonly AudioManager audioManager;
        private readonly IReadOnlyList<IWebSpeechProvider> webSpeechProviders;
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

        public SpeechManager ( AudioManager audioManager )
            : this( audioManager, [ new AzureSpeechProvider(), new AmazonPollySpeechProvider(), new OpenAISpeechProvider() ] )
        { }

        internal SpeechManager (
            AudioManager audioManager,
            IEnumerable<IWebSpeechProvider> webSpeechProviders )
        {
            this.audioManager = audioManager;
            this.webSpeechProviders = webSpeechProviders?.ToList() ?? [];
        }

        public IReadOnlyList<WebSpeechProviderDescriptor> WebProviderDescriptors =>
            webSpeechProviders.Select( provider => provider.Descriptor ).ToList();

        public WebSpeechProvider CreateWebProviderProfile ( string providerType )
        {
            var provider =  webSpeechProviders.FirstOrDefault( p => p.ProviderType == providerType )
                ?? throw new InvalidOperationException( $"No web speech provider is available for provider type '{providerType}'." );

            return provider.CreateProfile();
        }

        public async Task InitializeAsync (CancellationToken ct = default)
        {
            Logging.Debug("[InitializeAsync] SpeechManager initialization started.");
            SpeechFormatter.EnsureLexiconSchemasLoaded();
            var voiceStore = new HashSet<VoiceDetails>(); // Use a Hashset to ensure no duplicates

            // Windows.Media.SpeechSynthesis isn't available on older Windows versions so we must check if we have access
            try
            {
                if ( IsWindowsMediaSynthesizerSupported() )
                {
                    Logging.Debug("Windows Media Synthesizer is supported. Creating...");
                    // Prep the Windows.Media.SpeechSynthesis synthesizer
                    windowsMediaSynth = await WindowsMediaSynthesizer.CreateAsync( voiceStore, ct ).ConfigureAwait( false );
                    Logging.Debug($"Windows Media voices loaded. Current total store count: {voiceStore.Count}");
                }
                else
                {
                    Logging.Debug("Windows Media Synthesizer is not supported on this OS version.");
                }
            }
            catch ( Exception e )
            {
                Logging.Error( $"Unable to initialize Windows.Media.SpeechSynthesis.SpeechSynthesizer, {RuntimeInformation.OSDescription}", e );
            }

            // Prep the System.Speech synthesizer
            try
            {
                Logging.Debug("Initializing System Speech Synthesizer...");
                systemSpeechSynth = new SystemSpeechSynthesizer();
                systemSpeechSynth.Initialize( voiceStore );
                Logging.Debug($"System Speech voices loaded. Current total store count: {voiceStore.Count}");
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

            var config = ConfigService.Instance.speechServiceConfiguration;
            MigrateLegacyWebProviderConfigurations( config );
            await LoadWebProviderVoicesAsync( voiceStore, config, ct ).ConfigureAwait(false);

            // Sort results alphabetically by voice name
            validatedVoices.Clear();
            validatedVoices.AddRange( voiceStore.OrderBy( v => v.name ) );
            Logging.Debug($"SpeechManager initialization completed. Total validatedVoices: {validatedVoices.Count}");
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

                foreach ( var provider in webSpeechProviders.OfType<IDisposable>() )
                {
                    provider.Dispose();
                }
            }
        }

        private static bool IsWindowsMediaSynthesizerSupported ()
        {
            return OSInfo.TryGetWindowsVersion( out var osVersion ) &&
                   osVersion >= new System.Version( 10, 0, 17763, 0 );
        }

        internal async Task LoadWebProviderVoicesAsync (
            HashSet<VoiceDetails> voiceStore,
            SpeechServiceConfiguration config,
            CancellationToken ct )
        {
            if ( config?.SpeechProviderConfigurations == null )
            {
                return;
            }

            foreach ( var profile in config.SpeechProviderConfigurations.Where( p => p.Enabled ) )
            {
                var provider = webSpeechProviders.FirstOrDefault( p => p.ProviderType == profile.ProviderType );
                if ( provider == null )
                {
                    Logging.Warn( $"No web speech provider is available for profile '{profile.DisplayName}' ({profile.ProviderType})." );
                    continue;
                }

                if ( !provider.IsConfigured( profile ) )
                {
                    Logging.Debug( $"Web speech provider profile '{profile.DisplayName}' is not configured." );
                    continue;
                }

                try
                {
                    var voices = await provider.GetVoicesAsync( profile, ct ).ConfigureAwait( false );
                    foreach ( var voice in voices )
                    {
                        voiceStore.Add( voice );
                    }
                    Logging.Debug( $"Loaded {voices.Count} web voices from '{profile.DisplayName}'." );
                }
                catch ( OperationCanceledException )
                {
                    throw;
                }
                catch ( Exception ex )
                {
                    Logging.Warn( $"Failed to load web speech voices from '{profile.DisplayName}'.", ex );
                }
            }
        }

        internal void MigrateLegacyWebProviderConfigurations ( SpeechServiceConfiguration config )
        {
            foreach ( var provider in webSpeechProviders )
            {
                provider.MigrateLegacyConfiguration( config );
            }
        }

        internal async Task<Stream> GetWebProviderSpeechStreamAsync (
            VoiceDetails voiceDetails,
            string speech,
            SpeechServiceConfiguration config,
            CancellationToken ct )
        {
            var profile =  config.SpeechProviderConfigurations.FirstOrDefault( p =>
                p.Id == voiceDetails.providerProfileId &&
                p.ProviderType == voiceDetails.synthType )  ?? throw new InvalidOperationException( $"No web speech provider profile was found for voice '{voiceDetails.name}'." );

            var provider =  webSpeechProviders.FirstOrDefault( p => p.ProviderType == profile.ProviderType )
                ?? throw new InvalidOperationException( $"No web speech provider is available for profile '{profile.DisplayName}'." );

            Logging.Info(
                $"Using web speech provider '{profile.DisplayName}' ({profile.ProviderType}) for voice '{voiceDetails.name}' with key '{voiceDetails.voiceKey}'." );

            return await provider
                .SynthesizeAsync( profile, voiceDetails, speech, config, ct )
                .ConfigureAwait( false );
        }

        public async Task ValidateWebProviderProfileAsync (
            WebSpeechProvider profile,
            CancellationToken ct = default )
        {
            ArgumentNullException.ThrowIfNull( profile, nameof( profile ) );

            var provider =  webSpeechProviders.FirstOrDefault( p => p.ProviderType == profile.ProviderType )
                ?? throw new InvalidOperationException( $"No web speech provider is available for profile '{profile.DisplayName}'." );

            if ( !provider.IsConfigured( profile ) )
            {
                throw new InvalidOperationException(
                    $"Web speech provider profile '{profile.DisplayName}' is missing required configuration." );
            }

            await provider.ValidateAsync( profile, ct ).ConfigureAwait( false );
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
            Logging.Debug($"voice='{voice}', priority={priority}");
            // Skip empty speech and speech containing nothing except one or more pauses / breaks.
            message = SpeechFormatter.TrimSpeech( message );
            if ( string.IsNullOrEmpty( message ) )
            {
                Logging.Debug("Message is empty after trimming. Skipping.");
                return Task.CompletedTask;
            }

            // Queue the current speech
            var config = ConfigService.Instance.speechServiceConfiguration;
            var queuingSpeech = new EddiSpeech( message, voice, priority, eventType, ship?.Size, ship?.health, radio,
                config.DistortOnDamage );
            Logging.Debug($"Enqueuing speech object with voice='{queuingSpeech.voice}' to priority queue {queuingSpeech.priority}");
            speechQueue.Enqueue( queuingSpeech );

            // Check the first item in the speech queue
            // Interrupt current speech when appropriate
            if ( speechQueue.TryPeek( out var peekedSpeech ) && checkSpeechInterrupt( peekedSpeech.priority ) )
            {
                Logging.Debug( "Interrupting current speech" );
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
                        Logging.Debug($"Dequeued next speech. voice='{nextSpeech.voice}'");
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
            Logging.Debug($"voice='{speech.voice}', priority={speech.priority}");
            return SpeakAsync( speech.message, speech.voice, Configuration.EffectsLevel, speech.distortionLevel, speech.echoDelay, speech.priority, speech.radio );
        }

        public async Task SpeakAsync ( string speech, string defaultVoice, int fxLevel,
            int distortionLevel = 0, int echoDelay = 0, int priority = 3, bool radio = false )
        {
            Logging.Debug($"defaultVoice='{defaultVoice}', fxLevel={fxLevel}, distortionLevel={distortionLevel}, echoDelay={echoDelay}");
            if ( speech == null || speech.Trim() == "" ) { return; }

            // If the user wants to disable IPA then we remove any IPA phoneme tags here
            var config = ConfigService.Instance.speechServiceConfiguration;
            if ( config.DisableIpa && speech.Contains( "<phoneme" ) )
            {
                speech = SpeechFormatter.DisableIPA( speech );
            }

            discardPendingSegments = false;
            var segments = SpeechFormatter.SeparateSpeechSegments(speech);
            Logging.Debug($"Separated into {segments.Count} segments.");

            foreach ( var segment in segments )
            {
                if ( discardPendingSegments ) { 
                    Logging.Debug("discardPendingSegments is true. Aborting loop.");
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
                    Logging.Debug($"Found voice override tag. Voice='{voice}'");
                }

                var resolvedVoice = voice ?? defaultVoice;
                Logging.Debug($"Calling GetSpeechStreamAsync for voice='{resolvedVoice}'");
                await using ( var stream = await GetSpeechStreamAsync( resolvedVoice, statement ).ConfigureAwait( false ) )
                {
                    if ( stream == null )
                    {
                        Logging.Debug( "GetSpeechStreamAsync() returned null; nothing to say" );
                        return;
                    }
                    if ( stream.Length < 50 )
                    {
                        Logging.Debug( $"GetSpeechStreamAsync() returned empty/short stream of length {stream.Length}; nothing to say" );
                        return;
                    }
                    else
                    {
                        Logging.Debug( "Stream length is " + stream.Length );
                    }
                    Logging.Debug( "Seeking back to the beginning of the stream" );
                    stream.Seek( 0, SeekOrigin.Begin );

                    Logging.Debug($"Applying effects (fxLevel: {fxLevel}, distortionLevel: {distortionLevel}, echoDelay: {echoDelay}, isRadio: {isRadio})");
                    var provider = SpeechFx.addEffectsToSource(stream, fxLevel, distortionLevel, echoDelay, isRadio );
                    Logging.Debug("Playing speech stream via PlaySpeechStreamAsync...");
                    await PlaySpeechStreamAsync( provider, priority ).ConfigureAwait(false);
                    Logging.Debug("Playback completed.");
                }
            }
        }

        // Obtain the speech memory stream
        private async Task<Stream> GetSpeechStreamAsync ( string requestedVoice, string speech )
        {
            Logging.Debug($"RequestedVoice='{requestedVoice}'");
            try
            {
                var stream = await SynthesizeSpeechAsync(requestedVoice, speech).ConfigureAwait(false);
                if ( stream is null || stream.Length == 0 )
                {
                    Logging.Debug( "SynthesizeSpeechAsync() returned null or empty. Retrying with stripped SSML." );
                    // Try again, with speech devoid of SSML
                    stream = await SynthesizeSpeechAsync( requestedVoice, GeneratedRegex.SsmlTagRegex().Replace( speech, string.Empty ) ).ConfigureAwait(false);
                }
                Logging.Debug($"Returning stream of length {(stream != null ? stream.Length.ToString() : "null")} bytes.");
                return stream;
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Speech failed (" + Encoding.Default.EncodingName + ")", ex );
                var voiceDetails = validatedVoices.FirstOrDefault( v =>
                    string.Equals( v.voiceKey, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) ||
                    string.Equals( v.name, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) );
                if ( voiceDetails?.synthType is nameof( Windows.Media ) && requestedVoice != windowsMediaSynth.currentVoice )
                {
                    // Try falling back to our Windows Media default voice.
                    Logging.Warn( $"{ex.Message}, retrying with Windows Media Synthesizer default voice.", ex );
                    return await GetSpeechStreamAsync( windowsMediaSynth.currentVoice, speech ).ConfigureAwait(false);
                }
                if ( requestedVoice != systemSpeechSynth?.currentVoice )
                {
                    // Try falling back to our System Speech default voice.
                    Logging.Warn( $"{ex.Message}, retrying with System Speech Synthesizer default voice.", ex );
                    return await GetSpeechStreamAsync( systemSpeechSynth?.currentVoice, speech ).ConfigureAwait(false);
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

        public async Task ReloadVoicesAsync(CancellationToken ct = default)
        {
            await InitializeAsync(ct).ConfigureAwait(false);
        }

        private async Task<Stream> SynthesizeSpeechAsync ( string requestedVoice, string speech )
        {
            Logging.Debug($"requestedVoice='{requestedVoice}'");
            // Get the voice details we will use for speaking
            if ( TryResolveVoice( requestedVoice, out var voiceDetails ) )
            {
                Logging.Debug($"TryResolveVoice returned true. voiceDetails.name='{voiceDetails.name}', synthType='{voiceDetails.synthType}'");
                try
                {
                    if ( !string.IsNullOrEmpty( voiceDetails.providerProfileId ) )
                    {
                        return await GetWebProviderSpeechStreamAsync(
                            voiceDetails,
                            speech,
                            ConfigService.Instance.speechServiceConfiguration,
                            CancellationToken.None ).ConfigureAwait(false);
                    }

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
            Logging.Debug($"requestedVoice='{requestedVoice}', Configuration.StandardVoice='{Configuration.StandardVoice}'");
            
            // If the requestedVoice is null and the saved configuration's standard voice is not null,
            // try to re-resolve this once using the voice saved to the configuration.
            if ( string.IsNullOrEmpty( requestedVoice ) && !string.IsNullOrEmpty( Configuration.StandardVoice ) )
            {
                Logging.Debug($"requestedVoice is null or empty. Falling back to Configuration.StandardVoice '{Configuration.StandardVoice}'");
                return TryResolveVoice( Configuration.StandardVoice, out voiceDetails );
            }

            // If the requested voice is not null and matches one we've previously found, return that voice.
            if ( !string.IsNullOrEmpty( requestedVoice ) )
            {
                var foundVoice = validatedVoices.FirstOrDefault( v =>
                    string.Equals( v.voiceKey, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) );
                if ( foundVoice == null )
                {
                    var legacyMatches = validatedVoices
                        .Where( v => string.Equals( v.name, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) )
                        .Take( 2 )
                        .ToList();
                    if ( legacyMatches.Count == 1 )
                    {
                        foundVoice = legacyMatches[0];
                    }
                    else if ( legacyMatches.Count > 1 )
                    {
                        Logging.Warn( $"Voice '{requestedVoice}' matched multiple provider-qualified voices. Please select the provider-specific voice." );
                    }
                }

                if ( foundVoice != null )
                {
                    voiceDetails = foundVoice;
                    Logging.Debug($"Found matching voice '{foundVoice.name}' in validatedVoices");
                    return true;
                }
                else
                {
                    Logging.Debug($"Voice '{requestedVoice}' was not found in validatedVoices. validatedVoices count is {validatedVoices.Count}");
                }
            }

            // If the requested voice was not found, try to re-resolve this once using the synthesizer's default voice.
            var synthDefaultVoice = IsWindowsMediaSynthesizerSupported()
                ? windowsMediaSynth?.currentVoice ?? systemSpeechSynth?.currentVoice
                : systemSpeechSynth?.currentVoice;
            Logging.Debug($"Requested voice '{requestedVoice}' not resolved. Attempting fallback to default voice '{synthDefaultVoice}'");
            if ( !string.IsNullOrEmpty( synthDefaultVoice ) &&
                 !string.Equals( synthDefaultVoice, requestedVoice, StringComparison.InvariantCultureIgnoreCase ) )
            {
                Logging.Debug( $"Voice '{requestedVoice}' not found, falling back to voice '{synthDefaultVoice}'." );
                return TryResolveVoice( synthDefaultVoice, out voiceDetails );
            }

            // If none of the above then we've failed to select a voice from our voice list
            Logging.Debug("Failed to resolve any voice.");
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
