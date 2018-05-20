using CSCore;
using CSCore.Codecs.WAV;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;

namespace EddiSpeechService
{
    /// <summary>Provide speech services with a varying amount of alterations to the voice</summary>
    public class SpeechService : INotifyPropertyChanged
    {
        private SpeechServiceConfiguration configuration;

        private static readonly object activeSpeechLock = new object();
        private ISoundOut activeSpeech;
        private int activeSpeechPriority;

        private static bool _eddiSpeaking;
        public static bool eddiSpeaking
        {
            get
            {
                return _eddiSpeaking;
            }
            set
            {
                if (_eddiSpeaking != value)
                {
                    _eddiSpeaking = value;
                    Instance.NotifyPropertyChanged("eddiSpeaking");
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
            configuration = SpeechServiceConfiguration.FromFile();
        }

        public void ReloadConfiguration()
        {
            configuration = SpeechServiceConfiguration.FromFile();
        }

        public void Say(Ship ship, string speech, bool wait, int priority = 3, string voice = null, bool radio = false)
        {
            if (speech == null)
            {
                return;
            }

            if (ship == null)
            {
                // Provide basic ship definition
                ship = ShipDefinitions.FromModel("Sidewinder");
            }

            Speak(speech, voice, echoDelayForShip(ship), distortionLevelForShip(ship), chorusLevelForShip(ship), reverbLevelForShip(ship), 0, radio, wait, priority);
        }

        public void ShutUp()
        {
            StopCurrentSpeech();
        }

        public void Speak(string speech, string voice, int echoDelay, int distortionLevel, int chorusLevel, int reverbLevel, int compressLevel, bool radio = false, bool wait = true, int priority = 3)
        {
            if (speech == null || speech.Trim() == "") { return; }

            // If the user wants to disable SSML then we remove any tags here
            if (configuration.DisableSsml && (speech.Contains("<")))
            {
                Logging.Debug("Removing SSML");
                // User has disabled SSML so remove all tags
                speech = Regex.Replace(speech, "<.*?>", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(voice))
            {
                voice = configuration.StandardVoice;
            }

            // Put everything in a thread
            Thread speechThread = new Thread(() =>
            {
                try
                {
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
                                addEffectsToSource(ref source, chorusLevel, reverbLevel, echoDelay, distortionLevel, isRadio);
                            }

                            if (priority < activeSpeechPriority)
                            {
                                Logging.Debug("About to StopCurrentSpeech");
                                StopCurrentSpeech();
                                Logging.Debug("Finished StopCurrentSpeech");
                            }

                            play(ref source, priority);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("play failed: ", ex);
                }
            });

            speechThread.IsBackground = true;

            try
            {
                speechThread.Start();
                if (wait)
                {
                    speechThread.Join();
                }
            }
            catch (ThreadAbortException tax)
            {
                Logging.Error(tax);
                Thread.ResetAbort();
            }
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

        private void addEffectsToSource(ref IWaveSource source, int chorusLevel, int reverbLevel, int echoDelay, int distortionLevel, bool radio)
        {
            // Effects level is increased by damage if distortion is enabled
            int effectsLevel = fxLevel(distortionLevel);

            // Add various effects...
            Logging.Debug("Effects level is " + effectsLevel + ", chorus level is " + chorusLevel + ", reverb level is " + reverbLevel + ", echo delay is " + echoDelay);

            // We need to extend the duration of the wave source if we have any effects going on
            if (chorusLevel != 0 || reverbLevel != 0 || echoDelay != 0)
            {
                // Add a base of 500ms plus 10ms per effect level over 50
                Logging.Debug("Extending duration by " + 500 + Math.Max(0, (effectsLevel - 50) * 10) + "ms");
                source = source.AppendSource(x => new ExtendedDurationWaveSource(x, 500 + Math.Max(0, (effectsLevel - 50) * 10)));
            }

            // We always have chorus
            if (chorusLevel != 0)
            {
                source = source.AppendSource(x => new DmoChorusEffect(x) { Depth = chorusLevel, WetDryMix = Math.Min(100, (int)(180 * (effectsLevel) / ((decimal)100))), Delay = 16, Frequency = (effectsLevel / 10), Feedback = 25 });
            }

            // We only have reverb and echo if we're not transmitting or receiving
            if (!radio)
            {
                if (reverbLevel != 0)
                {
                    source = source.AppendSource(x => new DmoWavesReverbEffect(x) { ReverbTime = (int)(1 + 999 * (effectsLevel) / ((decimal)100)), ReverbMix = Math.Max(-96, -96 + (96 * reverbLevel / 100)) });
                }

                if (echoDelay != 0)
                {
                    source = source.AppendSource(x => new DmoEchoEffect(x) { LeftDelay = echoDelay, RightDelay = echoDelay, WetDryMix = Math.Max(5, (int)(10 * (effectsLevel) / ((decimal)100))), Feedback = 0 });
                }
            }
            // Apply a high pass filter for a radio effect
            else
            {
                var sampleSource = source.ToSampleSource().AppendSource(x => new BiQuadFilterSource(x));
                sampleSource.Filter = new HighpassFilter(source.WaveFormat.SampleRate, 1015);
                source = sampleSource.ToWaveSource();
            }

            // Adjust gain
            if (effectsLevel != 0 && chorusLevel != 0)
            {
                int radioGain = radio ? 7 : 0;
                source = source.AppendSource(x => new DmoCompressorEffect(x) { Gain = effectsLevel / 15 + radioGain });
            }
        }

        // Play a source
        private void play(ref IWaveSource source, int priority)
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
                        Logging.Error("Failed to speak; missing media pack?", ce);
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
                    if (soundOut != null) soundOut.Dispose();
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

        // Speak using the MS speech synthesizer
        private void speak(MemoryStream stream, string voice, string speech)
        {
            var synthThread = new Thread(() =>
            {
                try
                {
                    using (SpeechSynthesizer synth = new SpeechSynthesizer())
                    {
                        if (voice != null && !voice.Contains("Microsoft Server Speech Text to Speech Voice"))
                        {
                            try
                            {
                                Logging.Debug("Selecting voice " + voice);
                                Thread t = new Thread(() => synth.SelectVoice(voice));
                                t.Start();
                                if (!t.Join(TimeSpan.FromSeconds(2)))
                                {
                                    t.Abort();
                                    Logging.Warn("Failed to select voice " + voice + " (1)");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.Warn("Failed to select voice " + voice + " (2)", ex);
                            }
                        }
                        Logging.Debug("Post-selection");

                        Logging.Debug("Configuration is " + configuration == null ? "<null>" : JsonConvert.SerializeObject(configuration));
                        synth.Rate = configuration.Rate;
                        synth.Volume = configuration.Volume;

                        synth.SetOutputToWaveStream(stream);

                        // Keep XML version at 1.0. Version 1.1 is not recommended for general use. https://en.wikipedia.org/wiki/XML#Versions
                        if (speech.Contains("<"))
                        {
                            Logging.Debug("Obtaining best guess culture");
                            string culture = @" xml:lang=""" + bestGuessCulture(synth) + @"""";
                            Logging.Debug("Best guess culture is " + culture);
                            speech = @"<?xml version=""1.0"" encoding=""UTF-8""?><speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis""" + culture + ">" + escapeSsml(speech) + @"</speak>";
                            Logging.Debug("Feeding SSML to synthesizer: " + escapeSsml(speech));
                            synth.SpeakSsml(speech);
                        }
                        else
                        {
                            Logging.Debug("Feeding normal text to synthesizer: " + speech);
                            synth.Speak(speech);
                        }
                        stream.ToArray();
                    }
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
                    Logging.Info("Speech failed", badSpeechJSON, "", "");
                }
            });
            synthThread.Start();
            synthThread.Join();
            stream.Position = 0;
        }

        private string bestGuessCulture(SpeechSynthesizer synth)
        {
            string guess = "en-US";
            if (synth != null)
            {
                if (synth.Voice != null)
                {
                    if (synth.Voice.Name.Contains("CereVoice"))
                    {
                        /// Cereproc voices do not support the normal xml:lang attribute country/region codes (like en-GB) 
                        /// (see https://www.cereproc.com/files/CereVoiceCloudGuide.pdf), 
                        /// but it does support two letter country codes so we will use those instead
                        guess = synth.Voice.Culture.Parent.Name;
                    }
                    else
                    {
                        // Trust the voice's information (with the complete country/region code)
                        guess = synth.Voice.Culture.Name;
                    }
                }
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
                            eddiSpeaking = true;
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

        private string escapeSsml(string text)
        {
            // Our input text might have SSML elements in it but the rest needs escaping
            string result = text;

             // We need to make sure file names for the play function include a "/" (e.g. C:/)
            result = Regex.Replace(result, "(<.+?src=\")(.:)(.*?" + @"\/>)", "$1" + "$2SSSSS" + "$3");
            
            // Our valid SSML elements are audio, break, emphasis, play, phoneme, & prosody so encode these differently for now
            // Also escape any double quotes inside the elements
            result = Regex.Replace(result, "(<[^>]*)\"", "$1ZZZZZ");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1ZZZZZ");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1ZZZZZ");
            result = Regex.Replace(result, "(<[^>]*)\"", "$1ZZZZZ");
            result = Regex.Replace(result, "<(audio.*?)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(break.*?)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(play.*?)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(phoneme.*?)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(/phoneme)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(prosody.*?)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(/prosody)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(emphasis.*?)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(/emphasis)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(transmit.*?)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(/transmit)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(voice.*?)>", "XXXXX$1YYYYY");
            result = Regex.Replace(result, "<(/voice)>", "XXXXX$1YYYYY");

            // Now escape anything that is still present
            result = SecurityElement.Escape(result);

            // Put back the characters we hid
            result = Regex.Replace(result, "XXXXX", "<");
            result = Regex.Replace(result, "YYYYY", ">");
            result = Regex.Replace(result, "ZZZZZ", "\"");
            result = Regex.Replace(result, "SSSSS", @"\");
            return result;
        }

        private void StopCurrentSpeech()
        {
            lock (activeSpeechLock)
            {
                if (activeSpeech != null)
                {
                    Logging.Debug("Stopping active speech");
                    activeSpeech.Stop();
                    Logging.Debug("Disposing of active speech");
                    activeSpeech.Dispose();
                    activeSpeech = null;
                    Logging.Debug("Stopped current speech");
                    eddiSpeaking = false;
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

        private int echoDelayForShip(Ship ship)
        {
            // this is affected by ship size
            int echoDelay = 50; // Default
            if (ship != null)
            {
                if (ship.size == "Small")
                {
                    echoDelay = 50;
                }
                else if (ship.size == "Medium")
                {
                    echoDelay = 100;
                }
                else if (ship.size == "Large")
                {
                    echoDelay = 200;
                }
                else if (ship.size == "Huge")
                {
                    echoDelay = 400;
                }
            }
            return echoDelay;
        }

        private int chorusLevelForShip(Ship ship)
        {
            // This may be affected by ship parameters
            return (int)(60 * (Math.Max(fxLevel(distortionLevelForShip(ship)), (decimal)configuration.EffectsLevel) / (decimal)100));
        }

        private int reverbLevelForShip(Ship ship)
        {
            // This is not affected by ship parameters
            return (int)(80 * ((decimal)configuration.EffectsLevel) / ((decimal)100));
        }

        private int distortionLevelForShip(Ship ship)
        {
            // This is affected by ship health
            int distortionLevel = 0;
            if (ship != null && configuration.DistortOnDamage)
            {
                distortionLevel = (100 - (int)ship.health);
            }
            return distortionLevel;
        }

        private int fxLevel(decimal distortionLevel)
        {
            // Effects level is increased by damage if distortion is enabled
            int distortionFX = 0;
            if (distortionLevel > 0)
            {
                distortionFX = (int)Decimal.Round(((decimal)distortionLevel / 100) * (100 - configuration.EffectsLevel));
                Logging.Debug("Calculating effect of distortion on speech effects: +" + distortionFX);
            }
            return configuration.EffectsLevel + distortionFX;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public class BiQuadFilterSource : SampleAggregatorBase
        {
            private readonly object _lockObject = new object();
            private BiQuad _biquad;

            public BiQuad Filter
            {
                get { return _biquad; }
                set
                {
                    lock (_lockObject)
                    {
                        _biquad = value;
                    }
                }
            }

            public BiQuadFilterSource(ISampleSource source) : base(source)
            {
            }

            public override int Read(float[] buffer, int offset, int count)
            {
                int read = base.Read(buffer, offset, count);
                lock (_lockObject)
                {
                    if (Filter != null)
                    {
                        for (int i = 0; i < read; i++)
                        {
                            buffer[i + offset] = Filter.Process(buffer[i + offset]);
                        }
                    }
                }

                return read;
            }
        }
    }
}
