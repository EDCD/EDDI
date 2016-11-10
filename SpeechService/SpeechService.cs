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

        public void Say(Ship ship, string speech, bool wait, int priority = 3)
        {
            if (speech == null)
            {
                return;
            }

            Speak(speech, null, echoDelayForShip(ship), distortionLevelForShip(ship), chorusLevelForShip(ship), reverbLevelForShip(ship), 0, wait, priority);
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

        static void synth_StateChanged(object sender, StateChangedEventArgs e)
        {
            Logging.Debug("Current state of the synthesizer: " + e.State);
        }

        public void Speak(string speech, string voice, int echoDelay, int distortionLevel, int chorusLevel, int reverbLevel, int compressLevel, bool wait = true, int priority = 3)
        {
            if (speech == null) { return; }

            Thread speechThread = new Thread(() =>
            {
                string finalSpeech = null;
                try
                {
                    using (SpeechSynthesizer synth = new SpeechSynthesizer())
                    using (MemoryStream stream = new MemoryStream())
                    {
                        if (string.IsNullOrWhiteSpace(voice))
                        {
                            voice = configuration.StandardVoice;
                        }
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
                                Logging.Error("Failed to select voice " + voice, ex);
                            }
                        }

                        Logging.Debug("Post-selection");
                        Logging.Debug("Configuration is " + configuration == null ? "<null>" : JsonConvert.SerializeObject(configuration));
                        synth.Rate = configuration.Rate;
                        Logging.Debug("Rate is " + synth.Rate);
                        synth.Volume = configuration.Volume;
                        Logging.Debug("Volume is " + synth.Volume);

                        synth.StateChanged += new EventHandler<StateChangedEventArgs>(synth_StateChanged);
                        Logging.Debug("Tracking state changes");
                        synth.SetOutputToWaveStream(stream);
                        Logging.Debug("Output set to stream");
                        if (speech.Contains("<phoneme") || speech.Contains("<break"))
                        {
                            Logging.Debug("Speech is SSML");
                            if (configuration.DisableSsml)
                            {
                                Logging.Debug("Disabling SSML at user request");
                                // User has disabled SSML so remove it
                                finalSpeech = Regex.Replace(speech, "<.*?>", string.Empty);
                                synth.Speak(finalSpeech);
                            }
                            else
                            {
                                Logging.Debug("Obtaining best guess culture");
                                string culture = bestGuessCulture(synth);
                                Logging.Debug("Best guess culture is " + culture);
                                finalSpeech = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"" + bestGuessCulture(synth) + "\"><s>" + speech + "</s></speak>";
                                Logging.Debug("SSML speech: " + finalSpeech);
                                try
                                {
                                    Logging.Debug("Speaking SSML");
                                    synth.SpeakSsml(finalSpeech);
                                    Logging.Debug("Finished speaking SSML");
                                }
                                catch (Exception ex)
                                {
                                    Logging.Error("Best guess culture of " + bestGuessCulture(synth) + " for voice " + synth.Voice.Name + " was incorrect", ex);
                                    Logging.Info("SSML does not work for the chosen voice; falling back to normal speech");
                                    // Try again without Ssml
                                    finalSpeech = Regex.Replace(speech, "<.*?>", string.Empty);
                                    synth.Speak(finalSpeech);
                                }
                            }
                        }
                        else
                        {
                            Logging.Debug("Speech does not contain SSML");
                            Logging.Debug("Speech: " + speech);
                            finalSpeech = speech;
                            Logging.Debug("Speaking normal speech");
                            synth.Speak(finalSpeech);
                            Logging.Debug("Finished speaking normal speech");
                        }
                        Logging.Debug("Seeking back to the beginning of the stream");
                        stream.Seek(0, SeekOrigin.Begin);

                        Logging.Debug("Setting up source from stream");
                        IWaveSource source = new WaveFileReader(stream);

                        // We need to extend the duration of the wave source if we have any effects going on
                        if (chorusLevel != 0 || reverbLevel != 0 || echoDelay != 0)
                        {
                            // Add a base of 500ms plus 10ms per effect level over 50
                            Logging.Debug("Extending duration by " + 500 + Math.Max(0, (configuration.EffectsLevel - 50) * 10) + "ms");
                            source = source.AppendSource(x => new ExtendedDurationWaveSource(x, 500 + Math.Max(0, (configuration.EffectsLevel - 50) * 10)));
                        }

                        // Add various effects...
                        Logging.Debug("Effects level is " + configuration.EffectsLevel + ", chorus level is " + chorusLevel + ", reverb level is " + reverbLevel + ", echo delay is " + echoDelay);
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

                        if (priority < activeSpeechPriority)
                        {
                            Logging.Debug("About to StopCurrentSpeech");
                            StopCurrentSpeech();
                            Logging.Debug("Finished StopCurrentSpeech");
                        }

                        Logging.Debug("Creating waitHandle");
                        EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                        Logging.Debug("Setting up soundOut");
                        var soundOut = GetSoundOut();
                        Logging.Debug("Setting up soundOut");
                        soundOut.Initialize(source);
                        Logging.Debug("Configuring waitHandle");
                        soundOut.Stopped += (s, e) => waitHandle.Set();

                        Logging.Debug("Starting speech");
                        StartSpeech(soundOut, priority);

                        Logging.Debug("Waiting for speech");
                        // Add a timeout, in case it doesn't come back with the signal
                        waitHandle.WaitOne(source.GetTime(source.Length));
                        Logging.Debug("Finished waiting for speech");

                        Logging.Debug("Stopping speech (just to be sure)");
                        StopCurrentSpeech();
                        Logging.Debug("Disposing of speech source");
                        source.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to speak \"" + finalSpeech + "\"", ex);
                }
            });
            Logging.Debug("Setting thread name");
            speechThread.Name = "Speech service speak";
            Logging.Debug("Setting thread background");
            speechThread.IsBackground = true;
            try
            {
                Logging.Debug("Starting speech thread");
                speechThread.Start();
                if (wait)
                {
                    Logging.Debug("Waiting for speech thread");
                    speechThread.Join();
                    Logging.Debug("Finished waiting for speech thread");
                }
            }
            catch (ThreadAbortException tax)
            {
                Thread.ResetAbort();
                Logging.Error(speech, tax);
            }
            catch (Exception ex)
            {
                Logging.Error(speech, ex);
                speechThread.Abort();
            }
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

        private void StartSpeech(ISoundOut soundout, int priority)
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