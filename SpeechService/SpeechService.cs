using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using EddiDataDefinitions;
using EddiSpeechService.SpeechPreparation;
using EddiSpeechService.SpeechSynthesizers;
using Newtonsoft.Json;
using System;
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
        public List<string> allvoices => allVoices.Select(v => v.name).ToList();

        private static readonly object activeSpeechLock = new object();
        private ISoundOut _activeSpeech;
        private ISoundOut activeSpeech
        {
            get => _activeSpeech;
            set
            {
                eddiSpeaking = value != null;
                _activeSpeech = value;
            }
        }
        private int activeSpeechPriority;

        public readonly SpeechQueue speechQueue = SpeechQueue.Instance;

        private static bool _eddiSpeaking;
        public bool eddiSpeaking
        {
            get => _eddiSpeaking;
            set
            {
                if (_eddiSpeaking != value)
                {
                    _eddiSpeaking = value;
                    OnPropertyChanged();
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

        // another false positive from CA2213
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<synth>k__BackingField")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                systemSpeechSynth?.Dispose();
                windowsMediaSynth?.Dispose();
            }
        }

        private SpeechService()
        {
            Configuration = SpeechServiceConfiguration.FromFile();
            var voiceStore = new HashSet<VoiceDetails>(); // Use a Hashset to ensure no duplicates

            // Windows.Media.SpeechSynthesis isn't available on older Windows versions so we must check if we have access
            try
            {
                if (OSInfo.TryGetWindowsVersion(out var osVersion) && osVersion.Major >= 10)
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
        }

        public void Say(Ship ship, string message, int priority = 3, string voice = null, bool radio = false, string eventType = null, bool invokedFromVA = false)
        {
            if (message == null) { return; }

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

        public void ShutUp()
        {
            StopCurrentSpeech();
        }

        public static void Speak(EddiSpeech speech)
        {
            Instance.Speak(speech.message, speech.voice, speech.echoDelay, speech.distortionLevel, speech.chorusLevel, speech.reverbLevel, speech.compressionLevel, speech.radio, speech.priority);
        }

        public void Speak(string speech, string voice, int echoDelay, int distortionLevel, int chorusLevel, int reverbLevel, int compressLevel, bool radio = false, int priority = 3)
        {
            if (speech == null || speech.Trim() == "") { return; }

            // If the user wants to disable IPA then we remove any IPA phoneme tags here
            if (Configuration.DisableIpa && speech.Contains("<phoneme"))
            {
                speech = SpeechFormatter.DisableIPA(speech);
            }

            List<string> statements = SpeechFormatter.SeparateSpeechStatements(speech);

            foreach (string Statement in statements)
            {
                string statement = Statement;

                bool isAudio = statement.Contains("<audio"); // This is an audio file, we will disable voice effects processing
                if (isAudio)
                {
                    statement = SpeechFormatter.FormatAudioTags(statement);
                }

                bool isRadio = statement.Contains("<transmit") || radio; // This is a radio transmission, we will enable radio voice effects processing
                if (isRadio)
                {
                    statement = SpeechFormatter.StripRadioTags(statement);
                }

                using (Stream stream = getSpeechStream(voice, statement))
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
                    if (!isAudio)
                    {
                        source = addEffectsToSource(source, chorusLevel, reverbLevel, echoDelay, distortionLevel, isRadio);
                    }

                    play(source, priority);
                }
            }
        }

        // Play a source
        private void play(IWaveSource source, int priority)
        {
            if (source == null)
            {
                Logging.Debug("Source is null; skipping");
                return;
            }

            using (EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                ISoundOut soundOut = GetSoundOut();
                try
                {
                    try
                    {
                        soundOut.Initialize(source);
                    }
                    catch (System.Runtime.InteropServices.COMException ce)
                    {
                        Logging.Warn($"Failed to speak; {ce.Source} not registered. Installation may be corrupt or Windows version may be incompatible.", ce);
                        return;
                    }
                    // ReSharper disable once AccessToDisposedClosure
                    soundOut.Stopped += (s, e) => waitHandle.Set();

                    TimeSpan waitTime = source.GetTime(source.Length);

                    Logging.Debug("Starting speech");
                    StartSpeech(ref soundOut, priority);
                    Logging.Debug("Waiting for speech - " + waitTime);
                    // Wait for the appropriate amount of time before stopping the speech.  This is belt-and-braces approach,
                    // as we should receive the stopped signal when the buffer runs out, but there is suspicion that the stopped
                    // signal does not show up at time
                    waitHandle.WaitOne(waitTime);
                    Logging.Debug("Finished waiting for speech");
                    StopCurrentSpeech();
                }
                finally
                {
                    soundOut?.Dispose();
                }
            }
        }

        // Obtain the speech memory stream
        private Stream getSpeechStream(string voice, string speech)
        {
            try
            {
                if (string.IsNullOrEmpty(voice))
                {
                    voice = windowsMediaSynth?.voice;
                }

                if (string.IsNullOrEmpty(voice))
                {
                    voice = systemSpeechSynth?.voice;
                }

                if (string.IsNullOrEmpty(voice))
                {
                    Logging.Error("Could not obtain a voice for speaking.");
                }

                var stream = speak(voice, speech);
                if (stream.Length == 0)
                {
                    // Try again, with speech devoid of SSML
                    stream = speak(voice, Regex.Replace(speech, "<.*?>", string.Empty));
                }

                return stream;
            }
            catch (Exception ex)
            {
                Logging.Warn("Speech failed (" + Encoding.Default.EncodingName + ")", ex);
            }

            return null;
        }

        private Stream speak(string voice, string speech)
        {
            // Get the voice we will use for speaking
            VoiceDetails voiceDetails = null;
            if (!string.IsNullOrEmpty(voice))
            {
                voiceDetails = allVoices.SingleOrDefault(v => string.Equals(v.name, voice, StringComparison.InvariantCultureIgnoreCase));
            }
            return speak(voiceDetails, speech);
        }

        private Stream speak(VoiceDetails voiceDetails, string speech)
        {
            if (voiceDetails?.synthType is nameof(System.Speech.Synthesis))
            {
                return systemSpeechSynth?.Speak(voiceDetails, speech, Configuration);
            }
            else if (voiceDetails?.synthType is nameof(Windows.Media.SpeechSynthesis))
            {
                return windowsMediaSynth?.Speak(voiceDetails, speech, Configuration);
            }
            return null;
        }

        private void StartSpeech(ref ISoundOut soundout, int priority)
        {
            bool started = false;
            while (!started)
            {
                if (activeSpeech == null)
                {
                    lock (activeSpeechLock)
                    {
                        Logging.Debug("Checking to see if we can start speech");
                        if (activeSpeech == null)
                        {
                            Logging.Debug("We can - setting active speech");
                            activeSpeech = soundout;
                            activeSpeechPriority = priority;
                            started = true;
                            Logging.Debug("Playing sound buffer");
                            soundout.Play();
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        public void StopCurrentSpeech()
        {
            lock (activeSpeechLock)
            {
                if (activeSpeech != null)
                {
                    Logging.Debug("Stopping active speech");
                    FadeOutCurrentSpeech();
                    activeSpeech.Stop();
                    Logging.Debug("Disposing of active speech");
                    activeSpeech.Dispose();
                    activeSpeech = null;
                    Logging.Debug("Stopped current speech");
                }
            }
        }

        private void FadeOutCurrentSpeech()
        {
            if (activeSpeech?.PlaybackState == PlaybackState.Playing)
            {
                float fadePer10Milliseconds = (activeSpeech.Volume / ActiveSpeechFadeOutMilliseconds) * 10;
                while (activeSpeech.Volume > 0)
                {
                    activeSpeech.Volume -= fadePer10Milliseconds;
                    Thread.Sleep(10);
                }
            }
        }

        private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
            {
                return new WasapiOut();
            }
            else
            {
                return new DirectSoundOut();
            }
        }

        private void StartOrContinueSpeaking()
        {
            if (!eddiSpeaking)
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
                catch (ThreadAbortException tax)
                {
                    Logging.Debug("Thread aborted", tax);
                    Thread.ResetAbort();
                }
            }
        }

        private bool checkSpeechInterrupt(int priority)
        {
            // Priority 0 speech (system messages) and priority 1 speech and will interrupt current speech
            // Priority 5 speech in interruptable by any higher priority speech. 
            if (priority <= 1 || (activeSpeechPriority >= 5 && priority < 5))
            {
                return true;
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JetBrains.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [PublicAPI]
    public class VoiceDetails : IEquatable<VoiceDetails>
    {
        [PublicAPI]
        public string name { get; }

        [PublicAPI]
        public string gender { get; }

        [PublicAPI]
        public string culturecode { get; }

        public string synthType { get; }

        [PublicAPI]
        public string cultureinvariantname => Culture.EnglishName;

        [PublicAPI]
        public string culturename => Culture.NativeName;

        public CultureInfo Culture { get; }

        internal VoiceDetails(string displayName, string gender, CultureInfo Culture, string synthType)
        {
            this.name = displayName;
            this.gender = gender;
            this.Culture = Culture;
            this.synthType = synthType;

            culturecode = BestGuessCulture();
        }

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
                if (IsValidXML(file.FullName))
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

        private bool IsValidXML(string filename)
        {
            // Check whether the file is valid .xml (.pls is an xml-based format)
            try
            {
                var _ = System.Xml.Linq.XDocument.Load(filename);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn($"Could not load .pls file from {filename}.", ex);
                return false;
            }
        }

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
