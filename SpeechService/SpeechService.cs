using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiSpeechService.SpeechPreparation;
using EddiSpeechService.SpeechSynthesizers;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Utilities;

namespace EddiSpeechService
{
    /// <summary>Provide speech services with a varying amount of alterations to the voice</summary>
    public partial class SpeechService : INotifyPropertyChanged, IDisposable
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

        private static readonly object activeAudioLock = new object();
        private static readonly object activeSpeechLock = new object();

        private int activeSpeechPriority;
        private static bool discardPendingSegments;

        private readonly ConcurrentDictionary<ISoundOut, CancellationTokenSource> activeSpeechTS = new ConcurrentDictionary<ISoundOut, CancellationTokenSource>();
        private readonly ConcurrentDictionary<ISoundOut, CancellationTokenSource> activeAudioTS = new ConcurrentDictionary<ISoundOut, CancellationTokenSource>();

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
                Logging.Warn($"Unable to initialize Windows.Media.SpeechSynthesis.SpeechSynthesizer, {RuntimeInformation.OSDescription}", e);
            }
            
            // Prep the System.Speech synthesizer
            systemSpeechSynth = new SystemSpeechSynthesizer(ref voiceStore);
            
            // Sort results alphabetically by voice name
            allVoices = voiceStore.OrderBy(v => v.name).ToList();

            // Monitor and respond appropriately to changes in the state of the CompanionAppService
            CompanionAppService.Instance.StateChanged += CompanionAppService_StateChanged;
        }

        private static bool IsWindowsMediaSynthesizerSupported()
        {
            return OSInfo.TryGetWindowsVersion( out var osVersion ) &&
                   osVersion >= new System.Version( 10, 0, 16299, 0 );
        }

        private void CompanionAppService_StateChanged(CompanionAppService.State oldState, CompanionAppService.State newState)
        {
            if (newState == CompanionAppService.State.ConnectionLost)
            {
                Say(null, EddiCompanionAppService.Properties.CapiResources.frontier_api_lost, 0);
            }
        }

        public void Say(Ship ship, string message, int priority = 3, string voice = null, bool radio = false, string eventType = null, bool invokedFromVA = false)
        {
            // Skip empty speech and speech containing nothing except one or more pauses / breaks.
            message = SpeechFormatter.TrimSpeech(message);
            if (string.IsNullOrEmpty(message)) { return; }

            Thread speechQueueHandler = new Thread(() =>
            {
                // Queue the current speech
                EddiSpeech queuingSpeech = new EddiSpeech(message, ship, priority, voice, radio, eventType);
                speechQueue.Enqueue(queuingSpeech);

                // Check the first item in the speech queue
                if (speechQueue.TryPeek(out EddiSpeech peekedSpeech))
                {
                    // Interrupt current speech when appropriate
                    if (checkSpeechInterrupt(peekedSpeech.priority))
                    {
                        Logging.Debug("Interrupting current speech");
                        StopCurrentSpeech();
                    }
                }

                // Start or continue speaking from the speech queue
                Instance.StartOrContinueSpeaking();
            })
            {
                Name = "SpeechQueueHandler",
                IsBackground = true
            };
            speechQueueHandler.Start();
            if (invokedFromVA)
            {
                // If invoked from VA, thread should terminate only after speech completes
                speechQueueHandler.Join();
            }
        }

        private void StartOrContinueSpeaking ()
        {
            if ( !eddiSpeaking )
            {
                // Put everything in a thread
                Thread speechThread = new Thread(() =>
                {
                    while (speechQueue.hasSpeech)
                    {
                        if (speechQueue.TryDequeue(out EddiSpeech speech))
                        {
                            try
                            {
                                Speak(speech);
                            }
                            catch (Exception ex)
                            {
                                Logging.Error("Failed to handle queued speech", ex);
                                Logging.Warn("Failed to handle speech " + JsonConvert.SerializeObject(speech));
                            }
                        }
                    }
                })
                {
                    Name = "Speech thread",
                    IsBackground = true
                };
                try
                {
                    speechThread.Start();
                    speechThread.Join();
                }
                catch ( ThreadAbortException tax )
                {
                    Logging.Debug( "Thread aborted", tax );
                    Thread.ResetAbort();
                }
            }
        }

        private bool checkSpeechInterrupt ( int priority )
        {
            // Priority 0 speech (system messages) and priority 1 speech and will interrupt current speech
            // Priority 5 speech in interruptable by any higher priority speech. 
            if ( priority <= 1 || ( activeSpeechPriority >= 5 && priority < 5 ) )
            {
                return true;
            }
            return false;
        }

        public static void Speak(EddiSpeech speech)
        {
            Instance.Speak(speech.message, speech.voice, speech.echoDelay, speech.distortionLevel, speech.chorusLevel, speech.reverbLevel, speech.compressionLevel, speech.radio, speech.priority);
        }

        public void Speak(string speech, string defaultVoice, int echoDelay, int distortionLevel, int chorusLevel, int reverbLevel, int compressLevel, bool radio = false, int priority = 3)
        {
            if (speech == null || speech.Trim() == "") { return; }

            // If the user wants to disable IPA then we remove any IPA phoneme tags here
            if (Configuration.DisableIpa && speech.Contains("<phoneme"))
            {
                speech = SpeechFormatter.DisableIPA(speech);
            }

            discardPendingSegments = false;
            List<string> segments = SpeechFormatter.SeparateSpeechSegments(speech);

            foreach (string segment in segments)
            {
                if ( discardPendingSegments )  { continue; }

                string voice = null;
                string statement = null;

                bool isAudio = segment.Contains("<audio"); // This is an audio file, we will disable voice effects processing
                if (isAudio)
                {
                    SpeechFormatter.UnpackAudioTags(segment, out string fileName, out bool async, out decimal? volumeOverride);
                    try
                    {
                        // Play the audio, waiting for the audio to complete unless we're in async mode
                        if (async)
                        {
                            Task.Run(() => PlayAudio(fileName, volumeOverride));
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
                    catch (Exception e)
                    {
                        Logging.Warn(e.Message, e);
                    }
                    continue;
                }

                bool isRadio = segment.Contains("<transmit") || radio; 
                if (isRadio)
                {
                    // This is a radio transmission, we will enable radio voice effects processing
                    statement = SpeechFormatter.StripRadioTags(segment);
                }

                bool isVoice = segment.Contains("<voice") || radio; 
                if (isVoice)
                {
                    // This is a voice override
                    SpeechFormatter.UnpackVoiceTags(segment, out voice, out statement);
                }

                using (Stream stream = getSpeechStream(voice ?? defaultVoice, statement ?? segment))
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

                    IWaveSource source = new WaveFileReader(stream);
                    source = addEffectsToSource(source, chorusLevel, reverbLevel, echoDelay, distortionLevel, isRadio);

                    PlaySpeechStream(source, priority);
                }
            }
        }
        
        // Obtain the speech memory stream
        public Stream getSpeechStream ( string requestedVoice, string speech )
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
                if ( requestedVoice != systemSpeechSynth.currentVoice )
                {
                    // Try falling back to our System Speech default voice.
                    Logging.Warn( $"{ex.Message}, retrying with System Speech Synthesizer default voice.", ex );
                    return getSpeechStream( systemSpeechSynth.currentVoice, speech );
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

        private ISoundOut GetSoundOut ()
        {
            if ( WasapiOut.IsSupportedOnCurrentPlatform )
            {
                return new WasapiOut();
            }
            else
            {
                return new DirectSoundOut();
            }
        }

        private static bool TryInitializeSoundOut ( ISoundOut soundOut, IWaveSource source )
        {
            try
            {
                soundOut.Initialize( source );
            }
            catch ( COMException ce )
            {
                Logging.Warn( $"Failed to initialize. {ce.Source} not registered. Installation may be corrupt or Windows version may be incompatible. ", ce );
                return false;
            }
            return true;
        }

        private static void FadeOut ( ISoundOut soundOut )
        {
            if ( soundOut?.PlaybackState == PlaybackState.Playing )
            {
                float fadePer10Milliseconds = soundOut.Volume / ActiveSpeechFadeOutMilliseconds * 10;
                while ( soundOut.Volume > 0 )
                {
                    soundOut.Volume -= fadePer10Milliseconds;
                    Thread.Sleep( 10 );
                }
            }
            soundOut?.Stop();
        }

        #region Speech

        private void PlaySpeechStream(IWaveSource source, int priority, bool useLegacySoundOut = false)
        {
            try
            {
                if ( !( source?.Length > 0 ) )
                {
                    Logging.Debug( "Skipping empty speech." );
                    return;
                }

                var waitTime = source.GetTime(source.Length);

                using ( var soundOut = GetSoundOut() )
                {
                    if ( !TryInitializeSoundOut( soundOut, source ) )
                    {
                        if ( soundOut is WasapiOut && !useLegacySoundOut )
                        {
                            Logging.Warn( "Falling back to legacy DirectSoundOut." );
                            PlaySpeechStream( source, priority, true );
                        }
                    }

                    var cancellationTokenSource = new CancellationTokenSource();
                    StartSpeech( soundOut, priority, cancellationTokenSource );

                    // Fade out and stop the speech once it completes. Complete early if cancellation is requested.
                    try
                    {
                        Logging.Debug( $"Waiting for speech - {waitTime.Milliseconds} ms (unless ended early)" );
                        Task.Delay( waitTime, cancellationTokenSource.Token ).GetAwaiter().GetResult();
                    }
                    catch ( OperationCanceledException )
                    {
                        // Nothing to do here, we're just making sure that speech completed early is handled gracefully.
                    }

                    Logging.Debug( "Finished waiting for speech" );
                    FadeOut( soundOut );
                    lock ( activeSpeechLock )
                    {
                        if ( activeSpeechTS.TryRemove( soundOut, out var ts ) )
                        {
                            ts.Dispose();
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                Logging.Error( "Speech playback failed.", e );
            }
        }

        private void StartSpeech ( ISoundOut soundout, int priority, CancellationTokenSource cancellationTokenSource )
        {
            while ( eddiSpeaking ) { Thread.Sleep( 10 ); }
            lock ( activeSpeechLock )
            {
                activeSpeechTS.TryAdd( soundout, cancellationTokenSource );
                OnPropertyChanged( nameof( eddiSpeaking ) );
                Logging.Debug( "Setting active speech and playing sound buffer" );
                activeSpeechPriority = priority;
                soundout.Play();
            }
        }

        public void StopCurrentSpeech()
        {
            lock ( activeSpeechLock )
            {
                Logging.Debug( "Ending active speech." );
                discardPendingSegments = true;
                var keysToRemove = activeSpeechTS.Keys;
                keysToRemove.AsParallel().ForAll(key =>
                {
                    if ( activeSpeechTS.TryRemove( key, out var tokenSource ) )
                    {
                        tokenSource.Cancel();
                        tokenSource.Token.WaitHandle.WaitOne( TimeSpan.FromSeconds( 5 ) );
                        tokenSource.Dispose();
                    }
                } );
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

        public void PlayAudio ( string fileName, decimal? volumeOverride, bool useLegacySoundOut = false )
        {
            try
            {
                IWaveSource audioSource;
                try
                {
                    audioSource = CodecFactory.Instance.GetCodec( fileName );
                }
                catch ( NotSupportedException e )
                {
                    Logging.Debug( $"Skipping unsupported audio file {fileName}.", e );
                    throw;
                }
                if ( !( audioSource?.Length > 0 ) ) { return; }

                var waitTime = audioSource.GetTime( audioSource.Length );

                using ( var soundOut = GetSoundOut() )
                {
                    Logging.Debug($"Beginning audio playback for {fileName}.");
                    if ( !TryInitializeSoundOut( soundOut, audioSource ) )
                    {
                        if ( soundOut is WasapiOut && !useLegacySoundOut )
                        {
                            Logging.Warn( "Falling back to legacy DirectSoundOut." );
                            PlayAudio( fileName, volumeOverride, true );
                        }
                    }

                    if ( volumeOverride != null )
                    {
                        soundOut.Volume = Math.Max( Math.Min( (float)volumeOverride / 100, 1 ), 0 );
                    }

                    soundOut.Play();

                    // Fade out and stop the audio once it completes. Complete early if cancellation is requested.
                    var cancellationTokenSource = new CancellationTokenSource();
                    lock ( activeAudioLock )
                    {
                        activeAudioTS.TryAdd(soundOut, cancellationTokenSource );
                    }
                    try
                    {
                        Logging.Debug( $"Waiting for audio - {waitTime.Milliseconds} ms (unless ended early)." );
                        Task.Delay( waitTime, cancellationTokenSource.Token ).GetAwaiter().GetResult();
                    }
                    catch ( OperationCanceledException )
                    {
                        // Nothing to do here, we're just making sure that audio completed early is handled gracefully.
                    }

                    Logging.Debug( $"Ending audio playback for {fileName}." );
                    FadeOut( soundOut );
                    lock ( activeAudioLock )
                    {
                        if ( activeAudioTS.TryRemove( soundOut, out var ts ) )
                        {
                            ts.Dispose();
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                Logging.Error("Audio playback failed.", e);            
            }
        }

        public void StopAudio ()
        {
            Logging.Debug( $"Ending all audio playback." );
            discardPendingSegments = true;
            lock ( activeAudioLock )
            {
                var keysToRemove = activeAudioTS.Keys;
                keysToRemove.AsParallel().ForAll( key =>
                {
                    if ( activeAudioTS.TryRemove( key, out var tokenSource ) )
                    {
                        tokenSource.Cancel();
                        tokenSource.Token.WaitHandle.WaitOne( TimeSpan.FromSeconds( 5 ) );
                        tokenSource.Dispose();
                    }
                } );
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
        public string cultureinvariantname => Culture.EnglishName;

        [Utilities.PublicAPI]
        public string culturename => Culture.NativeName;

        public CultureInfo Culture { get; }

        public bool hideVoice { get; set; }

        internal VoiceDetails(string displayName, string gender, CultureInfo Culture, string synthType)
        {
            this.name = displayName;
            this.gender = gender;
            this.Culture = Culture;
            this.synthType = synthType;

            culturecode = BestGuessCulture();

            // Try to obtian and load lexicon related schemas for lexicon schema validation
            try
            {
                schemas.Add( "http://www.w3.org/XML/1998/namespace", "http://www.w3.org/2001/xml.xsd" );
                schemas.Add( "http://www.w3.org/2005/01/pronunciation-lexicon",
                    "http://www.w3.org/TR/pronunciation-lexicon/pls.xsd" );
            }
            catch ( ArgumentException ae )
            {
                Logging.Warn( $"Unable to obtain lexicon validation schemas.", ae );
            }
            catch ( XmlSchemaException xmle )
            {
                Logging.Warn($"Problem with lexicon validation schema at {xmle.SourceUri}", xmle);
            }
        }

        #region Lexicons

        private XmlSchemaSet schemas = new XmlSchemaSet();

        public HashSet<string> GetLexicons()
        {
            var result = new HashSet<string>();
            HashSet<string> GetLexiconsFromDirectory(string directory, bool createIfMissing = false)
            {
                // When multiple lexicons are referenced, their precedence goes from lower to higher with document order.
                // Precedence means that a token is first looked up in the lexicon with highest precedence.
                // Only if not found in that lexicon, the next lexicon is searched and so on until a first match or until all lexicons have been used for lookup. (https://www.w3.org/TR/2004/REC-speech-synthesis-20040907/#S3.1.4).

                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(culturecode)) { return null; }
                DirectoryInfo dir = new DirectoryInfo(directory);
                if (dir.Exists)
                {
                    // Find two letter language code lexicons (these will have lower precedence than any full language code lexicons)
                    foreach (var file in dir.GetFiles("*.pls", SearchOption.AllDirectories)
                        .Where(f => $"{f.Name.ToLowerInvariant()}" == $"{Culture.TwoLetterISOLanguageName.ToLowerInvariant()}.pls"))
                    {
                        CheckAndAdd(file);
                    }
                    // Find full language code lexicons
                    foreach (var file in dir.GetFiles("*.pls", SearchOption.AllDirectories)
                        .Where(f => $"{f.Name.ToLowerInvariant()}" == $"{Culture.IetfLanguageTag.ToLowerInvariant()}.pls"))
                    {
                        CheckAndAdd(file);
                    }
                }
                else if (createIfMissing)
                {
                    dir.Create();
                }
                return result;
            }

            void CheckAndAdd(FileInfo file)
            {
                if (IsValidXML(file.FullName, out _))
                {
                    result.Add(file.FullName);
                }
                else
                {
                    file.MoveTo($"{file.FullName}.malformed");
                }
            }

            // When multiple lexicons are referenced, their precedence goes from lower to higher with document order (https://www.w3.org/TR/2004/REC-speech-synthesis-20040907/#S3.1.4) 

            // Add lexicons from our installation directory
            result.UnionWith(GetLexiconsFromDirectory(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + @"\lexicons"));

            // Add lexicons from our user configuration (allowing these to overwrite any prior lexeme values)
            result.UnionWith(GetLexiconsFromDirectory(Constants.DATA_DIR + @"\lexicons"));

            return result;
        }

        private bool IsValidXML(string filename, out XDocument xml)
        {
            // Check whether the file is valid .xml (.pls is an xml-based format)
            xml = null;
            try
            {
                // Try to load the file as xml
                xml = XDocument.Load(filename);

                // Validate the xml against the schema
                xml.Validate(schemas, ( o, e ) =>
                {
                    if ( e.Severity == XmlSeverityType.Warning || e.Severity == XmlSeverityType.Error )
                    {
                        throw new XmlSchemaValidationException( e.Message, e.Exception );
                    }
                } );
                var reader = xml.CreateReader();
                var lastNodeName = string.Empty;
                while ( reader.Read() )
                {
                    if ( reader.HasValue && 
                         reader.NodeType is XmlNodeType.Text && 
                         lastNodeName == "phoneme" && 
                         !IPA.IsValid( reader.Value ) )
                    {
                        throw new ArgumentException( $"Invalid phoneme found in lexicon file: {reader.Value}" );
                    }
                    lastNodeName = reader.Name;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn($"Could not load lexicon file '{filename}', please review.", ex);
                return false;
            }
        }

        #endregion

        private string BestGuessCulture()
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
