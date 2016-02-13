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
using System.Threading.Tasks;

namespace EliteDangerousSpeechService
{
    /// <summary>Provide speech services with a varying amount of alterations to the voice</summary>
    public class SpeechService
    {
        private readonly Random random = new Random();

        public void Say(Ship ship, string script)
        {
            script = script.Replace("$=", ship.Name == null ? "your ship" : ship.Name);
            Speak(script, null, 0, 0, chorusLevelForShip(ship), reverbLevelForShip(ship), 0, true);
        }

        public void Transmit(Ship ship, string script)
        {
            if (ship.CallSign == null)
            {
                script = script.Replace("$=", "Unidentified " + ship.Model == null ? "ship" : ship.Model);
            }
            else
            {
                script = script.Replace("$=", "" + ship.Model + " " + Translations.CallSign(ship.CallSign));
            }
            Speak(script, null, 0, 0, chorusLevelForShip(ship), reverbLevelForRadio(ship), 0, true);
        }

        public void Receive(Ship ship, string script)
        {
            if (ship.CallSign == null)
            {
                script = script.Replace("$=", "Unidentified " + ship.Model == null ? "ship" : ship.Model);
            }
            else
            {
                script = script.Replace("$=", "" + ship.Model + " " + Translations.CallSign(ship.CallSign));
            }
            Speak(script, null, 0, 0, chorusLevelForShip(ship), reverbLevelForRadio(ship), 0, true);
        }

        public void Speak(string script, string voice, int echoDelay, int distortionLevel, int chorusLevel, int reverbLevel, int compressLevel, bool waitForCompletion)
        {
            if (script == null) { return; }

            try
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                using (MemoryStream stream = new MemoryStream())
                {
                    if (voice != null)
                    {
                        try
                        {
                            synth.SelectVoice(voice);
                        }
                        catch { }
                    }

                    //synth.SetOutputToAudioStream(stream, new System.Speech.AudioFormat.SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
                    synth.SetOutputToWaveStream(stream);
                    synth.Speak(SpeechFromScript(script));

                    // If we are applying any effects we need to extend the stream so that we catch them.  Here we calculate
                    // how much additional time to add to the stream
                    decimal timeToAdd = 0.5M; // Start with half a second as breathing space
                    if (echoDelay != 0)
                    {
                        // Add time due to echo delay
                        timeToAdd = ((decimal)echoDelay) / 1000;
                    }
                    if (chorusLevel != 0)
                    {
                        timeToAdd = Math.Max(timeToAdd, 1);
                    }
                    if (reverbLevel != 0)
                    {
                        timeToAdd = Math.Max(timeToAdd, ((decimal)reverbLevel) / 1000);
                    }
                    if (timeToAdd > 0)
                    {
                        int bytesToAdd = (int)(timeToAdd * 44100);
                        byte[] empty = new byte[bytesToAdd];
                        stream.Write(empty, 0, bytesToAdd);
                    }

                    // Finished mucking about with the stream itself so reset
                    stream.Seek(0, SeekOrigin.Begin);

                    // Start with our base source
                    //IWaveSource baseSource = new RawDataReader(stream, new WaveFormat());
                    IWaveSource baseSource = new WaveFileReader(stream);
                    IWaveSource source = baseSource;

                    // Add various effects...

                    DmoChorusEffect chorusSource = null;
                    if (chorusLevel != 0)
                    {
                        chorusSource = new DmoChorusEffect(source);
                        chorusSource.Depth = chorusLevel;
                        chorusSource.WetDryMix = 90;
                        source = chorusSource;
                    }

                    DmoWavesReverbEffect reverbSource = null;
                    if (reverbLevel != 0)
                    {
                        reverbSource = new DmoWavesReverbEffect(source);
                        reverbSource.ReverbMix = reverbLevel;
                        //reverbSource.ReverbTime = reverbLevel;
                        source = reverbSource;
                    }

                    DmoEchoEffect echoSource = null;
                    if (echoDelay != 0)
                    {
                        echoSource = new DmoEchoEffect(source);
                        echoSource.LeftDelay = echoDelay;
                        echoSource.RightDelay = echoDelay;
                        echoSource.WetDryMix = 8;
                        source = echoSource;
                    }

                    DmoDistortionEffect distortSource = null;
                    if (distortionLevel != 0)
                    {
                        distortSource = new DmoDistortionEffect(source);
                        distortSource.Edge = distortionLevel;
                        distortSource.PreLowpassCutoff = 4800;
                        source = distortSource;
                    }

                    // We should be able to use a waithandle but for some reason it isn't working, so do a sleep-and-stop method
                    //EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    var soundOut = new WasapiOut();
                    soundOut.Initialize(source);

                    //soundOut.Stopped += (s, e) => waitHandle.Set();

                    soundOut.Play();

                    //WaitHandle.WaitOne();
                    Thread.Sleep((int)(source.Length * 1000 / source.WaveFormat.BytesPerSecond));

                    soundOut.Stop();
                    soundOut.Dispose();
                    if (distortSource != null) distortSource.Dispose();
                    if (echoSource != null) echoSource.Dispose();
                    if (chorusSource != null) chorusSource.Dispose();
                    baseSource.Dispose();
                }
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Environment.GetEnvironmentVariable("AppData") + @"\EDDI\speech.log", true)) { file.WriteLine("" + System.Threading.Thread.CurrentThread.ManagedThreadId + ": Caught exception " + ex); }
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

        private int chorusLevelForShip(Ship ship)
        {
            int chorusLevel = 60; // Default
            switch (ship.Size)
            {
                case ShipSize.Small:
                    chorusLevel = 40;
                    break;
                case ShipSize.Medium:
                    chorusLevel = 60;
                    break;
                case ShipSize.Large:
                    chorusLevel = 80;
                    break;
                case ShipSize.Huge:
                    chorusLevel = 100;
                    break;
            }
            return chorusLevel;
        }

        private static int reverbLevelForShip(Ship ship)
        {
            int reverbLevel = 0;
            switch (ship.Size)
            {
                case ShipSize.Small:
                    reverbLevel = -50;
                    break;
                case ShipSize.Medium:
                    reverbLevel = -25;
                    break;
                case ShipSize.Large:
                    reverbLevel = -10;
                    break;
                case ShipSize.Huge:
                    reverbLevel = -2;
                    break;
            }
            return reverbLevel;
        }

        private static int reverbLevelForRadio(Ship ship)
        {
            return 0;
            //int reverbLevel = 0;
            //switch (ship.Size)
            //{
            //    case ShipSize.Small:
            //        reverbLevel = -50;
            //        break;
            //    case ShipSize.Medium:
            //        reverbLevel = -25;
            //        break;
            //    case ShipSize.Large:
            //        reverbLevel = -10;
            //        break;
            //    case ShipSize.Huge:
            //        reverbLevel = -2;
            //        break;
            //}
            //return reverbLevel;
        }
    }
}