using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        public List<string> allvoices => allVoices.Select(v => v.name).ToList();

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
            using (var synth = new System.Speech.Synthesis.SpeechSynthesizer())
            {
                var systemSpeechVoices = synth
                    .GetInstalledVoices()
                    .Where(v => v.Enabled && !v.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                    .ToList();
                foreach (var voice in systemSpeechVoices)
                {
                    allVoices.Add(new VoiceDetails(voice.VoiceInfo.Name, voice.VoiceInfo.Gender.ToString(), voice.VoiceInfo.Culture, nameof(System.Speech.Synthesis)));
                }
            }

            // Get all available voices from Windows.Media.SpeechSynthesis
            foreach (var voice in Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices)
            {
                allVoices.Add(new VoiceDetails(voice.DisplayName, voice.Gender.ToString(), CultureInfo.GetCultureInfo(voice.Language), nameof(Windows.Media.SpeechSynthesis)));
            }

            // Sort results alphabetically by voice name
            allVoices = allVoices.OrderBy(v => v.name).ToList();
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
                        @"(<transmit.*?>[\s\S]*?<\/transmit>)",
                        @"(<voice.*?>[\s\S]*?<\/voice>)",
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
        private Stream getSpeechStream(string voice, string speech)
        {
            try
            {
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

            if (voiceDetails?.synthType is nameof(System.Speech.Synthesis))
            {
                Logging.Debug($"Selecting {nameof(System.Speech.Synthesis)} synthesizer");
                return SystemSpeechSynthesis(voiceDetails, speech);
            }
            else if (voiceDetails?.synthType is nameof(Windows.Media.SpeechSynthesis))
            {
                Logging.Debug($"Selecting {nameof(Windows.Media.SpeechSynthesis)} synthesizer");
                return WindowsMediaSpeechSynthesis(voiceDetails, speech).AsStreamForRead();
            }
            return null;
        }

        private static void PrepareSpeech(VoiceDetails voice, ref string speech, out bool useSSML)
        {
            var lexicons = voice.GetLexicons();
            if (speech.Contains("<") || lexicons.Any())
            {
                // Keep XML version at 1.0. Version 1.1 is not recommended for general use. https://en.wikipedia.org/wiki/XML#Versions
                var xmlHeader = @"<?xml version=""1.0"" encoding=""UTF-8""?>";

                // SSML "speak" tag must use version 1.0. This synthesizer rejects version 1.1.
                var speakHeader = $@"<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd"" xml:lang=""{voice.culturecode}"">";
                var speakFooter = @"</speak>";

                // Lexicons are applied as a child element to the `speak` element
                var lexiconString = lexicons.Aggregate(string.Empty, (current, lexiconFile) => current + $"<lexicon uri=\"{lexiconFile}\" type=\"application/pls+xml\"/>");

                var speakBody = lexiconString + escapeSsml(speech);

                // Put it all together
                speech = xmlHeader + speakHeader + speakBody + speakFooter;

                if (voice.name.StartsWith("CereVoice "))
                {
                    // Cereproc voices do not respect `SpeakSsml` (particularly for IPA), but they do handle SSML via the `Speak` method.
                    Logging.Debug("Working around CereVoice SSML support");
                    useSSML = false;
                }
                else
                {
                    useSSML = true;
                }
            }
            else
            {
                useSSML = false;
            }
        }

        private MemoryStream SystemSpeechSynthesis(VoiceDetails voice, string speech)
        {
            if (voice is null || speech is null) { return null; }

            // Speak using the system's native speech synthesizer (System.Speech.Synthesis). 
            var stream = new MemoryStream();
            var synthThread = new Thread(() =>
            {
                using (var synth = new System.Speech.Synthesis.SpeechSynthesizer())
                {
                    try
                    {
                        if (!voice.name.Equals(synth.Voice.Name))
                        {
                            Logging.Debug("Selecting voice " + voice);
                            synth.SelectVoice(voice.name);
                        }
                        synth.Rate = Configuration.Rate;
                        synth.Volume = Configuration.Volume;
                        synth.SetOutputToWaveStream(stream);
                        Logging.Debug(JsonConvert.SerializeObject(Configuration));
                        PrepareSpeech(voice, ref speech, out var useSSML);
                        if (useSSML)
                        {
                            Logging.Debug("Feeding SSML to synthesizer: " + speech);
                            synth.SpeakSsml(speech);
                        }
                        else
                        {
                            Logging.Debug("Feeding normal text to synthesizer: " + speech);
                            synth.Speak(speech);
                        }
                        stream.Position = 0;
                    }
                    catch (ThreadAbortException)
                    {
                        Logging.Debug("Thread aborted");
                    }
                    catch (Exception ex)
                    {
                        Logging.Warn("Speech failed: ", ex);
                        var badSpeech = new Dictionary<string, object>()
                        {
                            {"voice", voice },
                            {"speech", speech},
                        };
                        string badSpeechJSON = JsonConvert.SerializeObject(badSpeech);
                        Logging.Info("Speech failed", badSpeechJSON);
                    }
                }
            });
            synthThread.Start();
            synthThread.Join();
            return stream;
        }

        private Windows.Media.SpeechSynthesis.SpeechSynthesisStream WindowsMediaSpeechSynthesis(VoiceDetails voice, string speech)
        {
            if (voice is null || speech is null) { return null; }

            // Speak using the Windows.Media.SpeechSynthesis speech synthesizer. 
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = null;
            var synthThread = new Thread(() =>
            {
                using (var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer())
                {
                    try
                    {
                        double ConvertSpeakingRate(int rate)
                        {
                            // Convert from rate from -10 to 10 (with 0 being normal speed) to rate from 0.5X to 3X (with 1.0 being normal speed)
                            var result = 1.0;
                            if (rate < 0)
                            {
                                result += rate * 0.05;
                            }
                            else if (rate > 0)
                            {
                                result += rate * 0.2;
                            }
                            return result;
                        }

                        if (!voice.name.Equals(synth.Voice.DisplayName))
                        {
                            Logging.Debug("Selecting voice " + voice);
                            synth.Voice = Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.DisplayName == voice.name);
                        }
                        synth.Options.SpeakingRate = ConvertSpeakingRate(Configuration.Rate);
                        synth.Options.AudioVolume = Math.Round((double)Configuration.Volume / 100);
                        Logging.Debug(JsonConvert.SerializeObject(Configuration));

                        PrepareSpeech(voice, ref speech, out var useSSML);
                        if (useSSML)
                        {
                            Logging.Debug("Feeding SSML to synthesizer: " + speech);
                            stream = synth.SynthesizeSsmlToStreamAsync(speech).AsTask().Result;
                        }
                        else
                        {
                            Logging.Debug("Feeding normal text to synthesizer: " + speech);
                            stream = synth.SynthesizeTextToStreamAsync(speech).AsTask().Result;
                        }
                        stream.Seek(0);
                    }
                    catch (ThreadAbortException)
                    {
                        Logging.Debug("Thread aborted");
                    }
                    catch (Exception ex)
                    {
                        Logging.Warn("Speech failed: ", ex);
                        var badSpeech = new Dictionary<string, object>() 
                        {
                            {"voice", voice },
                            {"speech", speech},
                        };
                        string badSpeechJSON = JsonConvert.SerializeObject(badSpeech);
                        Logging.Info("Speech failed", badSpeechJSON);
                    }
                }
            });
            synthThread.Start();
            synthThread.Join();
            return stream;
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

        public event PropertyChangedEventHandler PropertyChanged;

        [JetBrains.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [PublicAPI]
    public class VoiceDetails
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
            HashSet<string> GetLexiconsFromDirectory(string directory)
            {
                // When multiple lexicons are referenced, their precedence goes from lower to higher with document order.
                // Precedence means that a token is first looked up in the lexicon with highest precedence.
                // Only if not found in that lexicon, the next lexicon is searched and so on until a first match or until all lexicons have been used for lookup. (https://www.w3.org/TR/2004/REC-speech-synthesis-20040907/#S3.1.4).

                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(culturecode)) { return null; }
                DirectoryInfo dir = new DirectoryInfo(directory);
                if (dir.Exists)
                {
                    foreach (var file in dir.GetFiles("*.pls", SearchOption.AllDirectories).Where(f => f.Name.ToLowerInvariant().Contains(Culture.IetfLanguageTag.ToLowerInvariant())))
                    {
                        result.Add(file.FullName);
                    }
                }
                else
                {
                    dir.Create();
                }
                return result;
            }

            // Add lexicons from our installation directory
            result.UnionWith(GetLexiconsFromDirectory(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + @"\lexicons"));

            // Add lexicons from our user configuration (allowing these to overwrite any prior lexeme values)
            result.UnionWith(GetLexiconsFromDirectory(Constants.DATA_DIR + @"\lexicons"));

            return result;
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
            //else if (name.Contains("IVONA") && Culture.Name == "en-GB")
            //{
            //    guess = "en-uk";
            //}
            else
            {
                // Trust the voice's information (with the complete country/region code)
                guess = Culture.Name;
            }
            Logging.Debug($"Best guess culture for {name} is {guess}"); return guess;
        }
    }
}
