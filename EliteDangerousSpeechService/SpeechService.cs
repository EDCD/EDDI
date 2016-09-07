using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using EliteDangerousDataDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;

namespace EliteDangerousSpeechService
{
    /// <summary>Provide speech services with a varying amount of alterations to the voice</summary>
    public class SpeechService
    {
        private readonly Random random = new Random();

        private SpeechServiceConfiguration configuration;

        private static readonly object activeSpeechLock = new object();
        private ISoundOut activeSpeech;

        public SpeechService(SpeechServiceConfiguration configuration = null)
        {
            this.configuration = configuration == null ? new SpeechServiceConfiguration() : configuration;
        }

        public void Say(Commander commander, Ship ship, string script)
        {
            string shipScript;
            if (ship == null || ship.name == null || ship.name.Trim().Length == 0)
            {
                shipScript = "your ship";
            }
            else if (ship.phoneticname == null || ship.phoneticname.Trim().Length == 0)
            {
                shipScript = ship.name;
            }
            else
            {
                shipScript = "<phoneme alphabet=\"ipa\" ph=\"" + ship.phoneticname + "\">" + ship.name + "</phoneme>";
            }
            script = script.Replace("$=", shipScript);

            string cmdrScript;
            if (commander == null || commander.name == null || commander.name.Trim().Length == 0)
            {
                cmdrScript = "commander";
            }
            else if (commander.phoneticname == null || commander.phoneticname.Trim().Length == 0)
            {
                cmdrScript = "commander " + commander.name;
            }
            else
            {
                cmdrScript = "commander <phoneme alphabet=\"ipa\" ph=\"" + commander.phoneticname + "\">" + commander.name + "</phoneme>";
            }
            script = script.Replace("$-", cmdrScript);

            Speak(script, null, echoDelayForShip(ship), distortionLevelForShip(ship), chorusLevelForShip(ship), reverbLevelForShip(ship), 0, false);
        }

        public void Transmit(Commander commander, Ship ship, string script)
        {
            if (ship == null)
            {
                script = script.Replace("$=", "Unidentified ship");
            }
            else if (ship.callsign == null)
            {
                script = script.Replace("$=", "Unidentified " + Translations.ShipModel(ship.model));
            }
            else
            {
                script = script.Replace("$=", "" + ship.model + " " + Translations.CallSign(ship.callsign));
            }
            Speak(script, null, echoDelayForShip(ship), distortionLevelForShip(ship), chorusLevelForShip(ship), reverbLevelForShip(ship), 0, true);
        }

        public void Receive(Commander commander, Ship ship, string script)
        {
            if (ship == null)
            {
                script = script.Replace("$=", "Unidentified ship");
            }
            else if (ship.callsign == null)
            {
                script = script.Replace("$=", "Unidentified " + Translations.ShipModel(ship.model));
            }
            else
            {
                script = script.Replace("$=", "" + ship.model + " " + Translations.CallSign(ship.callsign));
            }
            Speak(script, null, echoDelayForShip(ship), distortionLevelForShip(ship), chorusLevelForShip(ship), reverbLevelForShip(ship), 0, true);
        }

        public void Speak(string script, string voice, int echoDelay, int distortionLevel, int chorusLevel, int reverbLevel, int compressLevel, bool radio)
        {
            if (script == null) { return; }

            StopCurrentSpeech();

            new Thread(() =>
            {
                try
                {
                    using (SpeechSynthesizer synth = new SpeechSynthesizer())
                    using (MemoryStream stream = new MemoryStream())
                    {
                        if (string.IsNullOrWhiteSpace(voice))
                        {
                            voice = configuration.StandardVoice;
                        }
                        if (voice != null)
                        {
                            try
                            {
                                synth.SelectVoice(voice);
                            }
                            catch { }
                        }

                        synth.Rate = configuration.Rate;
                        synth.Volume = configuration.Volume;

                        synth.SetOutputToWaveStream(stream);
                        string speech = SpeechFromScript(script);
                        if (speech.Contains("<phoneme"))
                        {
                            speech = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"" + synth.Voice.Culture.Name + "\"><s>" + speech + "</s></speak>";
                            Logging.Debug("Final speech: " + speech);
                            synth.SpeakSsml(speech);
                        }
                        else
                        {
                            synth.Speak(speech);
                        }
                        stream.Seek(0, SeekOrigin.Begin);

                        Logging.Debug("Turned script " + script + " in to speech " + speech);

                        IWaveSource source = new WaveFileReader(stream);

                        // We need to extend the duration of the wave source if we have any effects going on
                        if (chorusLevel != 0 || reverbLevel != 0 || echoDelay != 0)
                        {
                            // Add a base of 500ms plus 10ms per effect level over 50
                            source = source.AppendSource(x => new ExtendedDurationWaveSource(x, 500 + Math.Max(0, (configuration.EffectsLevel - 50) * 10)));
                        }

                        // Add various effects...

                        // We always have chorus
                        if (chorusLevel != 0)
                        {
                            source = source.AppendSource(x => new DmoChorusEffect(x) { Depth = chorusLevel, WetDryMix = Math.Min(100, (int)(180 * ((decimal)configuration.EffectsLevel) / ((decimal)100))), Delay = 16, Frequency = (configuration.EffectsLevel / 10), Feedback = 25 });
                        }

                        // We only have reverb and echo if we're not transmitting or receiving
                        if (!radio)
                        {
                            if (reverbLevel != 0)
                            {
                                // We tone down the reverb level with the distortion level, as the combination is nasty
                                source = source.AppendSource(x => new DmoWavesReverbEffect(x) { ReverbTime = (int)(1 + 999 * ((decimal)configuration.EffectsLevel) / ((decimal)100)), ReverbMix = Math.Max(-96, -96 + (96 * reverbLevel / 100) - distortionLevel) });
                            }

                            if (echoDelay != 0)
                            {
                                // We tone down the echo level with the distortion level, as the combination is nasty
                                source = source.AppendSource(x => new DmoEchoEffect(x) { LeftDelay = echoDelay, RightDelay = echoDelay, WetDryMix = Math.Max(5, (int)(10 * ((decimal)configuration.EffectsLevel) / ((decimal)100)) - distortionLevel), Feedback = Math.Max(0, 10 - distortionLevel / 2) });
                            }
                        }

                        if (configuration.EffectsLevel > 0 && distortionLevel > 0)
                        {
                            source = source.AppendSource(x => new DmoDistortionEffect(x) { Edge = distortionLevel, Gain = -distortionLevel / 2, PostEQBandwidth = 4000, PostEQCenterFrequency = 4000 });
                        }

                        if (radio)
                        {
                            source = source.AppendSource(x => new DmoDistortionEffect(x) { Edge = 7, Gain = -distortionLevel / 2, PostEQBandwidth = 2000, PostEQCenterFrequency = 6000 });
                            source = source.AppendSource(x => new DmoCompressorEffect(x) { Attack = 1, Ratio = 3, Threshold = -10 });
                        }

                        EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                        var soundOut = new WasapiOut();
                        soundOut.Initialize(source);
                        soundOut.Stopped += (s, e) => waitHandle.Set();

                        activeSpeech = soundOut;
                        soundOut.Play();

                        // Add a timeout, in case it doesn't come back with the signal
                        waitHandle.WaitOne(source.GetTime(source.Length));

                        StopCurrentSpeech();
                        source.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to speak: " + ex);
                }
            }).Start();
        }

        // Called when the parent has exited
        public void ShutdownSpeech()
        {
            StopCurrentSpeech();
        }

        private void StopCurrentSpeech()
        {
            lock (activeSpeechLock)
            {
                if (activeSpeech != null)
                {
                    activeSpeech.Stop();
                    activeSpeech.Dispose();
                    activeSpeech = null;
                }
            }
        }
        public string SpeechFromScript(string script)
        {
            if (script == null) { return null; }

            StringBuilder sb = new StringBuilder();

            // Step 1 - resolve any options in square brackets
            Match matchResult = Regex.Match(script, @"\[[^\]]*\]|[^\[\]]+");
            while (matchResult.Success)
            {
                if (matchResult.Value.StartsWith("["))
                {
                    // Remove the brackets and pick one of the options
                    string result = matchResult.Value.Substring(1, matchResult.Value.Length - 2);
                    string[] options = result.Split(';');
                    sb.Append(options[random.Next(0, options.Length)]);
                }
                else
                {
                    // Pass it right along
                    sb.Append(matchResult.Groups[0].Value);
                }
                matchResult = matchResult.NextMatch();
            }
            string res = sb.ToString();

            // Step 2 - resolve phrases separated by semicolons
            if (res.Contains(";"))
            {
                // Pick one of the options
                string[] options = res.Split(';');
                res = options[random.Next(0, options.Length)];
            }
            return res;
        }

        private int echoDelayForShip(Ship ship)
        {
            // this is affected by ship size
            int echoDelay = 50; // Default
            if (ship != null)
            {
                switch (ship.size)
                {
                    case ShipSize.Small:
                        echoDelay = 50;
                        break;
                    case ShipSize.Medium:
                        echoDelay = 100;
                        break;
                    case ShipSize.Large:
                        echoDelay = 200;
                        break;
                    case ShipSize.Huge:
                        echoDelay = 400;
                        break;
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

                distortionLevel = Math.Min((100 - (int)ship.Health) / 2, 15);
            }
            return distortionLevel;

        }
    }
}