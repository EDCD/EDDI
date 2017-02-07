using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;

namespace EddiSpeechService
{
    /// <summary>Provide speech services with a varying amount of alterations to the voice</summary>
    public class SpeechService
    {
        private SpeechServiceConfiguration configuration;

        private static readonly object activeSpeechLock = new object();
        private ISoundOut activeSpeech;
        private int activeSpeechPriority;

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
            Logging.Debug("Current UI culture is " + Thread.CurrentThread.CurrentUICulture.Name);
            // Set the culture for this thread to the installed culture, to allow better selection of TTS voices
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(CultureInfo.InstalledUICulture.Name);
            Logging.Debug("Thread UI culture is " + Thread.CurrentThread.CurrentUICulture.Name);
        }

        public void ReloadConfiguration()
        {
            configuration = SpeechServiceConfiguration.FromFile();
        }

        public void Say(Ship ship, string speech, bool wait, int priority = 3, string voice = null)
        {
            if (speech == null)
            {
                return;
            }

            Speak(speech, voice, echoDelayForShip(ship), distortionLevelForShip(ship), chorusLevelForShip(ship), reverbLevelForShip(ship), 0, wait, priority);
        }

        public void ShutUp()
        {
            StopCurrentSpeech();
        }

        //public void Transmit(Ship ship, string script, bool wait, int priority=3)
        //{
        //    if (script == null)
        //    {
        //        return;
        //    }
        //    Speak(script, null, echoDelayForShip(ship), distortionLevelForShip(ship), chorusLevelForShip(ship), reverbLevelForShip(ship), 0, true, wait, priority);
        //}

        //public void Receive(Ship ship, string script, bool wait, int priority=3)
        //{
        //    if (script == null)
        //    {
        //        return;
        //    }
        //    Speak(script, null, echoDelayForShip(ship), distortionLevelForShip(ship), chorusLevelForShip(ship), reverbLevelForShip(ship), 0, true, wait, priority);
        //}

        public void Speak(string speech, string voice, int echoDelay, int distortionLevel, int chorusLevel, int reverbLevel, int compressLevel, bool wait = true, int priority = 3)
        {
            if (speech == null || speech.Trim() == "") { return; }

            // For now we rudely set distortion to 0 regardless of what the calling function wanted.  Might enable this again one day
            // if we can find a decent way of doing distortion that doesn't destroy speakers
            distortionLevel = 0;


            // Escape any ampersands
            speech = Regex.Replace(speech, "&", "&amp;");

            // If the user wants to disable SSML then we remove any tags here
            if (configuration.DisableSsml && (speech.Contains("<")))
            {
                Logging.Debug("Removing SSML");
                // User has disabled SSML so remove all tags
                speech = Regex.Replace(speech, "<.*?>", string.Empty);
                speech = Regex.Replace(speech, "&amp;", "&");
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
                    using (MemoryStream stream = getSpeechStream(voice, speech))
                    {
                        if (stream == null)
                        {
                            Logging.Debug("getSpeechStream() returned null; nothing to say");
                            return;
                        }
                        if (stream.Length == 0)
                        {
                            Logging.Debug("getSpeechStream() returned empty stream; nothing to say");
                            return;
                        }
                        Logging.Debug("Seeking back to the beginning of the stream");
                        stream.Seek(0, SeekOrigin.Begin);

                        Logging.Debug("Setting up source from stream");

                        IWaveSource source = new WaveFileReader(stream);
                        addEffectsToSource(ref source, chorusLevel, reverbLevel, echoDelay, distortionLevel);

                        if (priority < activeSpeechPriority)
                        {
                            Logging.Debug("About to StopCurrentSpeech");
                            StopCurrentSpeech();
                            Logging.Debug("Finished StopCurrentSpeech");
                        }

                        play(ref source, priority);
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

        private void addEffectsToSource(ref IWaveSource source, int chorusLevel, int reverbLevel, int echoDelay, int distortionLevel)
        {
            // Add various effects...
            Logging.Debug("Effects level is " + configuration.EffectsLevel + ", chorus level is " + chorusLevel + ", reverb level is " + reverbLevel + ", echo delay is " + echoDelay);

            // We need to extend the duration of the wave source if we have any effects going on
            if (chorusLevel != 0 || reverbLevel != 0 || echoDelay != 0)
            {
                // Add a base of 500ms plus 10ms per effect level over 50
                Logging.Debug("Extending duration by " + 500 + Math.Max(0, (configuration.EffectsLevel - 50) * 10) + "ms");
                source = source.AppendSource(x => new ExtendedDurationWaveSource(x, 500 + Math.Max(0, (configuration.EffectsLevel - 50) * 10)));
            }

            // We always have chorus
            if (chorusLevel != 0)
            {
                Logging.Debug("Adding chorus");
                source = source.AppendSource(x => new DmoChorusEffect(x) { Depth = chorusLevel, WetDryMix = Math.Min(100, (int)(180 * ((decimal)configuration.EffectsLevel) / ((decimal)100))), Delay = 16, Frequency = (configuration.EffectsLevel / 10), Feedback = 25 });
            }

            // We only have reverb and echo if we're not transmitting or receiving
            //if (!radio)
            //{
            if (reverbLevel != 0)
            {
                Logging.Debug("Adding reverb");
                // We tone down the reverb level with the distortion level, as the combination is nasty
                source = source.AppendSource(x => new DmoWavesReverbEffect(x) { ReverbTime = (int)(1 + 999 * ((decimal)configuration.EffectsLevel) / ((decimal)100)), ReverbMix = Math.Max(-96, -96 + (96 * reverbLevel / 100) - distortionLevel) });
            }

            if (echoDelay != 0)
            {
                Logging.Debug("Adding echo");
                // We tone down the echo level with the distortion level, as the combination is nasty
                source = source.AppendSource(x => new DmoEchoEffect(x) { LeftDelay = echoDelay, RightDelay = echoDelay, WetDryMix = Math.Max(5, (int)(10 * ((decimal)configuration.EffectsLevel) / ((decimal)100)) - distortionLevel), Feedback = Math.Max(0, 10 - distortionLevel / 2) });
            }
            //}

            if (configuration.EffectsLevel > 0 && distortionLevel > 0)
            {
                Logging.Debug("Adding distortion");
                source = source.AppendSource(x => new DmoDistortionEffect(x) { Edge = distortionLevel, Gain = -distortionLevel / 2, PostEQBandwidth = 4000, PostEQCenterFrequency = 4000 });
            }

            //if (radio)
            //{
            //    source = source.AppendSource(x => new DmoDistortionEffect(x) { Edge = 7, Gain = -distortionLevel / 2, PostEQBandwidth = 2000, PostEQCenterFrequency = 6000 });
            //    source = source.AppendSource(x => new DmoCompressorEffect(x) { Attack = 1, Ratio = 3, Threshold = -10 });
            //}


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
                    soundOut.Initialize(source);
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
                Logging.Warn("Speech failed", ex);
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
                                synth.SelectVoice(voice);
                                Logging.Debug("Selected voice " + synth.Voice.Name);
                            }
                            catch (Exception ex)
                            {
                                Logging.Warn("Failed to select voice " + voice, ex);
                            }
                        }
                        Logging.Debug("Post-selection");

                        Logging.Debug("Configuration is " + configuration == null ? "<null>" : JsonConvert.SerializeObject(configuration));
                        synth.Rate = configuration.Rate;
                        synth.Volume = configuration.Volume;

                        synth.SetOutputToWaveStream(stream);

                        if (speech.Contains("<"))
                        {
                            Logging.Debug("Obtaining best guess culture");
                            string culture = bestGuessCulture(synth);
                            Logging.Debug("Best guess culture is " + culture);
                            //speech = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"" + bestGuessCulture(synth) + "\" onlangfailure=\"ignorelang\"><s>" + speech + "</s></speak>";
                            speech = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"" + bestGuessCulture(synth) + "\"><s>" + speech + "</s></speak>";
                            Logging.Debug("Feeding SSML to synthesizer: " + speech);
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
                    Logging.Warn("speech failed: ", ex);
                    Logging.Error("Speech failed", @"{""speech"":""" + speech + @"""}");
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
                        // Cereproc voices don't have the correct local so we need to set it manually
                        if (synth.Voice.Name.Contains("Scotland") ||
                            synth.Voice.Name.Contains("England") ||
                            synth.Voice.Name.Contains("Ireland") ||
                            synth.Voice.Name.Contains("Wales"))
                        {
                            guess = "en-GB";
                        }
                    }
                    else
                    {
                        // Trust the voice's information
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
            // This is not affected by ship parameters
            return (int)(60 * ((decimal)configuration.EffectsLevel) / ((decimal)100));
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

                distortionLevel = Math.Min((100 - (int)ship.health) / 2, 15);
            }
            return distortionLevel;
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
    }
}