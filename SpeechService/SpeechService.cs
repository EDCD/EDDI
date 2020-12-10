using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Security;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;
using Utilities;

namespace EddiSpeechService
{
    /// <summary>Provide speech services with a varying amount of alterations to the voice</summary>
    public partial class SpeechService : INotifyPropertyChanged
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

        public List<VoiceDetails> allVoices { get; private set; } = new List<VoiceDetails>();
        public List<string> allvoices => allVoices.Select(v => v.voiceInfo.Name).ToList();

        private static readonly object activeSpeechLock = new object();
        private ISoundOut _activeSpeech;
        private ISoundOut activeSpeech
        {
            get
            {
                return _activeSpeech;
            }
            set
            {
                eddiSpeaking = value != null;
                _activeSpeech = value;
            }
        }
        private int activeSpeechPriority;

        public SpeechQueue speechQueue = SpeechQueue.Instance;

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

        private SpeechService()
        {
            Configuration = SpeechServiceConfiguration.FromFile();

            // Get all available voices from System.Speech.Synthesis
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                var systemSpeechVoices = synth
                    .GetInstalledVoices()
                    .Where(v => v.Enabled && !v.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                    .ToList();
                foreach (var voice in systemSpeechVoices)
                {
                    allVoices.Add(new VoiceDetails(voice.VoiceInfo, nameof(SpeechSynthesizer)));
                }
            }
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

        public void Speak(EddiSpeech speech)
        {
            Instance.Speak(speech.message, speech.voice, speech.echoDelay, speech.distortionLevel, speech.chorusLevel, speech.reverbLevel, speech.compressionLevel, speech.radio, speech.priority);
        }

        public void Speak(string speech, string voice, int echoDelay, int distortionLevel, int chorusLevel, int reverbLevel, int compressLevel, bool radio = false, int priority = 3)
        {
            if (speech == null || speech.Trim() == "") { return; }

            // If the user wants to disable IPA then we remove any IPA phoneme tags here
            if (Configuration.DisableIpa && speech.Contains("<phoneme"))
            {
                speech = DisableIPA(speech);
            }

            if (string.IsNullOrWhiteSpace(voice))
            {
                voice = Configuration.StandardVoice;
            }

            // Identify any statements that need to be separated into their own speech streams (e.g. audio or special voice effects)
            string[] separators =
            {
                        @"(<audio.*?>)",
                        @"(<transmit.*?>.*<\/transmit>)",
                        @"(<voice.*?>.*<\/voice>)",
                    };
            List<string> statements = SeparateSpeechStatements(speech, string.Join("|", separators));

            foreach (string Statement in statements)
            {
                string statement = Statement;

                bool isAudio = statement.Contains("<audio"); // This is an audio file, we will disable voice effects processing
                bool isRadio = statement.Contains("<transmit") || radio; // This is a radio transmission, we will enable radio voice effects processing

                if (isAudio)
                {
                    statement = Regex.Replace(statement, "^.*<audio", "<audio");
                    statement = Regex.Replace(statement, ">.*$", ">");
                }
                else if (isRadio)
                {
                    statement = statement.Replace("<transmit>", "");
                    statement = statement.Replace("</transmit>", "");
                }

                using (MemoryStream stream = getSpeechStream(voice, statement))
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

        private static string DisableIPA(string speech)
        {
            // User has disabled IPA so remove all IPA phoneme tags
            Logging.Debug("Phonetic speech is disabled, removing.");
            speech = Regex.Replace(speech, @"<phoneme.*?>", string.Empty);
            speech = Regex.Replace(speech, @"<\/phoneme>", string.Empty);
            return speech;
        }

        private static List<string> SeparateSpeechStatements(string speech, string separators)
        {
            // Separate speech into statements that can be handled differently & sequentially by the speech service
            List<string> statements = new List<string>();

            Match match = Regex.Match(speech, separators);
            if (match.Success)
            {
                string[] splitSpeech = new Regex(separators).Split(speech);
                foreach (string split in splitSpeech)
                {
                    if (Regex.Match(split, @"\S").Success) // Trim out non-word statements; match only words
                    {
                        statements.Add(split);
                    }
                }
            }
            else
            {
                statements.Add(speech);
            }
            return statements;
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
        private MemoryStream getSpeechStream(string voice, string speech)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                speak(stream, voice, speech);

                if (stream.Length == 0)
                {
                    // Try again, with speech devoid of SSML
                    speak(stream, voice, Regex.Replace(speech, "<.*?>", string.Empty));
                }
                return stream;
            }
            catch (Exception ex)
            {
                Logging.Warn("Speech failed (" + Encoding.Default.EncodingName + ")", ex);
            }
            return null;
        }

        private void speak(MemoryStream stream, string voice, string speech)
        {
            // Get the voice we will use for speaking
            VoiceDetails speakingVoiceDetails = null;
            if (!string.IsNullOrEmpty(voice))
            {
                speakingVoiceDetails = allVoices.SingleOrDefault(v => string.Equals(v.voiceInfo.Name, voice, StringComparison.InvariantCultureIgnoreCase));
            }

            if (speakingVoiceDetails != null)
            {
                switch (speakingVoiceDetails.synthType)
                {
                    case nameof(SpeechSynthesizer):
                        SystemSpeechSynthesis(stream, speakingVoiceDetails.voiceInfo, speech);
                        break;
                    default:
                        SystemSpeechSynthesis(stream, speakingVoiceDetails.voiceInfo, speech);
                        break;
                }
            }
            else
            {
                throw new NullReferenceException("Voice data not available.");
            }
        }

        private void SystemSpeechSynthesis(MemoryStream stream, VoiceInfo voice, string speech)
        {
            // Speak using the system's native speech synthesizer (System.Speech.Synthesis). SSML "speak" tag must use version 1.0.
            var synthThread = new Thread(() =>
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    try
                    {
                        if (voice != null && !voice.Equals(synth.Voice))
                        {
                            Logging.Debug("Selecting voice " + voice.Name);
                            synth.SelectVoice(voice.Name);
                        }
                        synth.Rate = Configuration.Rate;
                        synth.Volume = Configuration.Volume;
                        synth.SetOutputToWaveStream(stream);

                        Logging.Debug(JsonConvert.SerializeObject(Configuration));

                        // Keep XML version at 1.0. Version 1.1 is not recommended for general use. https://en.wikipedia.org/wiki/XML#Versions
                        var culture = bestGuessCulture(synth.Voice);
                        var lexicons = AddLexiconsSSMLv1_0(culture);
                        if (speech.Contains("<") || !string.IsNullOrEmpty(lexicons))
                        {
                            Logging.Debug("Obtaining best guess culture");
                            culture = $@" xml:lang=""{culture}""";
                            speech = @"<?xml version=""1.0"" encoding=""UTF-8""?><speak version=""1.0"" xmlns=""https://www.w3.org/2001/10/synthesis""" + culture + ">" + lexicons + escapeSsml(speech) + @"</speak>";
                            Logging.Debug("Feeding SSML to synthesizer: " + speech);
                            if (voice != null && voice.Name.StartsWith("CereVoice "))
                            {
                                // Cereproc voices do not respect `SpeakSsml` (particularly for IPA), but they do handle SSML via the `Speak` method.
                                Logging.Debug("Working around CereVoice SSML support");
                                synth.Speak(speech);
                            }
                            else
                            {
                                synth.SpeakSsml(speech);
                            }
                        }
                        else
                        {
                            Logging.Debug("Feeding normal text to synthesizer: " + speech);
                            synth.Speak(speech);
                        }
                        stream.ToArray();
                    }
                    catch (ThreadAbortException)
                    {
                        Logging.Debug("Thread aborted");
                    }
                    catch (Exception ex)
                    {
                        Logging.Warn("Speech failed: ", ex);
                        var badSpeech = new Dictionary<string, object>() {
                                {"speech", speech},
                    };
                        string badSpeechJSON = JsonConvert.SerializeObject(badSpeech);
                        Logging.Info("Speech failed", badSpeechJSON);
                    }
                }
            });
            synthThread.Start();
            synthThread.Join();
            stream.Position = 0;
        }

        private string bestGuessCulture(VoiceInfo voice)
        {
            string guess = "en-US";
            if (voice != null)
            {
                if (voice.Name.Contains("CereVoice"))
                {
                    /// Cereproc voices do not support the normal xml:lang attribute country/region codes (like en-GB) 
                    /// (see https://www.cereproc.com/files/CereVoiceCloudGuide.pdf), 
                    /// but it does support two letter country codes so we will use those instead
                    guess = voice.Culture.Parent.Name;
                }
                else
                {
                    // Trust the voice's information (with the complete country/region code)
                    guess = voice.Culture.Name;
                }
                Logging.Debug("Best guess culture is " + guess);
            }
            return guess;
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

        public static string escapeSsml(string text)
        {
            // Our input text might have SSML elements in it but the rest needs escaping
            string result = text;

            // We need to make sure file names for the play function include a "/" (e.g. C:/)
            result = Regex.Replace(result, "(<.+?src=\")(.:)(.*?" + @"\/>)", "$1" + "$2%SSS%" + "$3");

            // Our valid SSML elements are audio, break, emphasis, play, phoneme, & prosody so encode these differently for now
            // Also escape any double quotes or single quotes inside the elements
            result = Regex.Replace(result, "(<[^>]*)\"", "$1%ZZZ%");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1%ZZZ%");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1%ZZZ%");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1%ZZZ%");
            result = Regex.Replace(result, "(<[^>]*)\'", "$1%WWW%");
            result = Regex.Replace(result, "(<[^>]*)\'", "$1%WWW%");
            result = Regex.Replace(result, "(<[^>]*)\'", "$1%WWW%");
            result = Regex.Replace(result, "(<[^>]*)\'", "$1%WWW%");
            result = Regex.Replace(result, "<(audio.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(break.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(play.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(phoneme.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/phoneme)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(prosody.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/prosody)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(emphasis.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/emphasis)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(transmit.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/transmit)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(voice.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/voice)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(say-as.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/say-as)>", "%XXX%$1%YYY%");

            // Cereproc uses some additional custom SSML tags (documented in https://www.cereproc.com/files/CereVoiceCloudGuide.pdf)
            result = Regex.Replace(result, "<(usel.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/usel)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(spurt.*?)>", "%XXX%$1%YYY%");
            result = Regex.Replace(result, "<(/spurt)>", "%XXX%$1%YYY%");

            // Now escape anything that is still present
            result = SecurityElement.Escape(result) ?? "";

            // Put back the characters we hid
            result = Regex.Replace(result, "%XXX%", "<");
            result = Regex.Replace(result, "%YYY%", ">");
            result = Regex.Replace(result, "%ZZZ%", "\"");
            result = Regex.Replace(result, "%WWW%", "\'");
            result = Regex.Replace(result, "%SSS%", @"\");
            return result;
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

        public void FadeOutCurrentSpeech()
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

        private void WaitForCurrentSpeech()
        {
            Logging.Debug("Waiting for current speech to end");
            while (activeSpeech != null)
            {
                Thread.Sleep(10);
            }
            Logging.Debug("Current speech ended");
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

        public void StartOrContinueSpeaking()
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
                                Instance.Speak(speech);
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

        private string AddLexiconsSSMLv1_0(string culture)
        {
            var result = string.Empty;

            // Add lexicons from our installation directory
            result += AddLexiconsFromDirectorySSMLv1_0(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + @"\lexicons", culture);

            // Add lexicons from our user configuration (allowing these to overwrite any prior lexeme values)
            result += AddLexiconsFromDirectorySSMLv1_0(Constants.DATA_DIR + @"\lexicons", culture);

            return result;
        }

        private string AddLexiconsFromDirectorySSMLv1_0(string directory, string culture)
        {
            // When multiple lexicons are referenced, their precedence goes from lower to higher with document order.
            // Precedence means that a token is first looked up in the lexicon with highest precedence.
            // Only if not found in that lexicon, the next lexicon is searched and so on until a first match or until all lexicons have been used for lookup. (https://www.w3.org/TR/2004/REC-speech-synthesis-20040907/#S3.1.4).

            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(culture)) { return null; }
            var result = string.Empty;
            DirectoryInfo dir = new DirectoryInfo(directory);
            if (dir.Exists)
            {
                foreach (FileInfo file in dir.GetFiles("*.pls", SearchOption.AllDirectories).Where(f => f.Name.Contains(culture.ToLowerInvariant())))
                {
                    result += $"<lexicon uri=\"{file.FullName}\" type=\"application/pls+xml\"/>";
                }
            }
            else
            {
                dir.Create();
            }
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VoiceDetails
    {
        public VoiceInfo voiceInfo { get; }
        public string synthType { get; }

        internal VoiceDetails(VoiceInfo voiceInfo, string synthType)
        {
            this.voiceInfo = voiceInfo;
            this.synthType = synthType;
        }
    }
}
