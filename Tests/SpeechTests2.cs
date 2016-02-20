using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDDIVAPlugin;
using System.Collections.Generic;
using System;
using System.Speech.Synthesis;
using System.IO;
using System.Speech.AudioFormat;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.Streams;
using CSCore.Streams.Effects;
using System.Threading;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using EliteDangerousSpeechService;

namespace Tests
{
    [TestClass]
    public class SpeechTests
    {
        [TestMethod]
        public void testVariants()
        {
            SpeechService.Speak("Welcome to your small ship.", "IVONA 2 Emma", 0, 0, 40, -60, 0, true);
            SpeechService.Speak("Vulture x-ray whiskey tango seven one seven six requesting docking.", "IVONA 2 Emma", 0, 10, 40, -60, 0, true);
            SpeechService.Speak("Welcome to your medium ship.", "IVONA 2 Emma", 0, 0, 60, -30, 0, true);
            SpeechService.Speak("Python victor oscar Pappa fife tree fawer niner requesting docking.", "IVONA 2 Emma", 0, 10, 60, -30, 0, true);
            SpeechService.Speak("Welcome to your large ship.", "IVONA 2 Emma", 0, 0, 80, -20, 0, true);
            SpeechService.Speak("Anaconda charlie november delta one niner eight fawer requesting docking.", "IVONA 2 Emma", 0, 10, 80, -20, 0, true);
            SpeechService.Speak("Welcome to your huge ship.", "IVONA 2 Emma", 0, 0, 80, -10, 0, true);
            SpeechService.Speak("Panther Clipper charlie november delta one niner eight fawer requesting docking.", "IVONA 2 Emma", 0, 10, 100, -10, 0, true);

            //SpeechService.Speak("Welcome to your small ship.", "IVONA 2 Emma", 0, 0, 40, -50, 0, true);
            //SpeechService.Speak("Vulture x-ray whiskey tango seven one seven six requesting docking.", "IVONA 2 Emma", 0, 10, 40, -50, 0, true);
            ////SpeechService.Speak("Vulture x-ray whiskey tango seven one seven six requesting docking.", "IVONA 2 Emma", 0, 10, 40, 0, 0, true);
            //SpeechService.Speak("Welcome to your medium ship.", "IVONA 2 Emma", 0, 0, 60, -25, 0, true);
            //SpeechService.Speak("Python victor oscar Pappa fife tree fawer niner requesting docking.", "IVONA 2 Emma", 0, 10, 60, -25, 0, true);
            ////SpeechService.Speak("Python victor oscar Pappa fife tree fawer niner requesting docking.", "IVONA 2 Emma", 0, 10, 60, 0, 0, true);
            //SpeechService.Speak("Welcome to your large ship.", "IVONA 2 Emma", 0, 0, 80, -10, 0, true);
            //SpeechService.Speak("Anaconda charlie november delta one niner eight fawer requesting docking.", "IVONA 2 Emma", 0, 10, 80, -10, 0, true);
            ////SpeechService.Speak("Anaconda charlie november delta one niner eight fawer requesting docking.", "IVONA 2 Emma", 0, 10, 80, 0, 0, true);
            //SpeechService.Speak("Welcome to your huge ship.", "IVONA 2 Emma", 0, 0, 80, -5, 0, true);
            //SpeechService.Speak("Panther Clipper charlie november delta one niner eight fawer requesting docking.", "IVONA 2 Emma", 0, 10, 100, -5, 0, true);
            ////SpeechService.Speak("Panther Clipper charlie november delta one niner eight fawer requesting docking.", "IVONA 2 Emma", 0, 10, 100, 0, 0, true);
        }
        //synth.Speak("Anaconda golf foxtrot lima one niner six eight requesting docking.");


        [TestMethod]
        public void TestChorus()
        {
            SpeechService.Speak("Chorus level 0", "IVONA 2 Emma", 0, 0, 0, 0, 0, true);
            SpeechService.Speak("Chorus level 20", "IVONA 2 Emma", 0, 0, 20, 0, 0, true);
            SpeechService.Speak("Chorus level 40", "IVONA 2 Emma", 0, 0, 40, 0, 0, true);
            SpeechService.Speak("Chorus level 60", "IVONA 2 Emma", 0, 0, 60, 0, 0, true);
            SpeechService.Speak("Chorus level 80", "IVONA 2 Emma", 0, 0, 80, 0, 0, true);
            SpeechService.Speak("Chorus level 100", "IVONA 2 Emma", 0, 0, 100, 0, 0, true);
        }

        [TestMethod]
        public void TestShipSizes()
        {
            SpeechService.Speak("This is a test for a small ship.", "IVONA 2 Emma", 0, 0, 60, -50, 0, true);
            SpeechService.Speak("This is a test for a medium ship.", "IVONA 2 Emma", 0, 0, 60, -25, 0, true);
            SpeechService.Speak("This is a test for a large ship.", "IVONA 2 Emma", 0, 0, 60, -10, 0, true);
            SpeechService.Speak("This is a test for a huge ship.", "IVONA 2 Emma", 0, 0, 60, -2, 0, true);
        }

        [TestMethod]
        public void TestStandard()
        {
            SpeechService.Speak("This is a test for a small ship.", null, 0, 0, 10, 0, 0, true);
            SpeechService.Speak("This is a test for a medium ship.", null, 0, 0, 10, 0, 0, true);
            SpeechService.Speak("This is a test for a large ship.", null, 0, 0, 10, 0, 0, true);

            SpeechService.Speak("This is a test.", null, 0, 0, 0, 0, 0, true);
            SpeechService.Speak("This is a test with chorus.", null, 0, 0, 10, 0, 0, true);
            //SpeechService.Speak("This is a test with echo.", null, 0, 0, 70, 0, 0, true);
            //SpeechService.Speak("This is a test with reverb.", null, 0, 0, 0, 100, 0, true);
            //SpeechService.Speak("This is a test with reverb and echo.", null, 0, 0, 70, 100, 0, true);
            //SpeechService.Speak("This is a test with chorus and reverb.", null, 0, 0, 10, 100, 0, true);
            //SpeechService.Speak("This is a test with chorus, reverb and echo.", null, 0, 0, 70, 100, 0, true);
            SpeechService.Speak("This commodity should sell for at least 6000 credits.", null, 0, 0, 10, 100, 0, true);
        }

        [TestMethod]
        public void testSendAndReceive()
        {
            SpeechService.Speak("Issuing docking request.  Please stand by.", "IVONA 2 Emma", 0, 10, 0, 100, 0, true);
            SpeechService.Speak("Anaconda golf foxtrot lima one niner six eight requesting docking.", "IVONA 2 Emma", 0, 20, 0, 0, 0, true);
            SpeechService.Speak("Roger golf foxtrot lima one niner six eight docking request received", "IVONA 2 Brian", 0, 20, 0, 0, 0, true);
        }

        [TestMethod]
        public void TestEcho()
        {
            SpeechService.Speak("This is a test for echo", null, 75, 0, 10, 0, 0, true);
        }


        [TestMethod]
        public void TestDistort()
        {
            SpeechService.Speak("Anaconda golf foxtrot lima one niner six eight requesting docking.", null, 0, 10, 0, 0, 0, true);
        }

        [TestMethod]
        public void TestDistortion()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            using (MemoryStream stream = new MemoryStream())
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                foreach (InstalledVoice voice in synth.GetInstalledVoices())
                {
                    Console.WriteLine(voice.VoiceInfo.Name);
                }

                synth.SetOutputToWaveStream(stream);
                synth.Speak("Anaconda golf foxtrot lima one niner six eight requesting docking.");
                stream.Seek(0, SeekOrigin.Begin);

                IWaveSource source = new WaveFileReader(stream);
                DmoDistortionEffect distortedSource = new DmoDistortionEffect(source);
                distortedSource.Edge = 10;
                distortedSource.PreLowpassCutoff = 4800;

                var soundOut = new WasapiOut();
                soundOut.Stopped += (s, e) => waitHandle.Set();

                soundOut.Initialize(distortedSource);
                soundOut.Play();

                waitHandle.WaitOne();

                soundOut.Dispose();
                distortedSource.Dispose();
                source.Dispose();
            }
        }

        [TestMethod]
        public void TestRandomVoice()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            List<InstalledVoice> availableVoices = new List<InstalledVoice>();
            foreach (InstalledVoice voice in new SpeechSynthesizer().GetInstalledVoices())
            {
                if (voice.Enabled == true && voice.VoiceInfo.Culture.TwoLetterISOLanguageName == "en" && (voice.VoiceInfo.Name.StartsWith("IVONA") || voice.VoiceInfo.Name.StartsWith("CereVoice") || voice.VoiceInfo.Name == "Microsoft Anna"))
                {
                    availableVoices.Add(voice);
                }
            }
            foreach (InstalledVoice availableVoice in availableVoices)
            {
                Console.WriteLine(availableVoice.VoiceInfo.Name);
            }

            for (int i = 0; i < 10; i++)
            {
                using (MemoryStream stream = new MemoryStream())
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    string selectedVoice = availableVoices.OrderBy(x => Guid.NewGuid()).FirstOrDefault().VoiceInfo.Name;
                    Console.WriteLine("Selected voice is " + selectedVoice);
                    synth.SelectVoice(selectedVoice);

                    synth.SetOutputToWaveStream(stream);
                    //synth.Speak("Anaconda golf foxtrot lima one niner six eight requesting docking.");
                    synth.Speak("Anaconda.");
                    stream.Seek(0, SeekOrigin.Begin);

                    IWaveSource source = new WaveFileReader(stream);

                    var soundOut = new WasapiOut();
                    soundOut.Stopped += (s, e) => waitHandle.Set();

                    soundOut.Initialize(source);
                    soundOut.Play();

                    waitHandle.WaitOne();

                    soundOut.Dispose();
                    source.Dispose();
                }
            }
        }

        [TestMethod]
        public void TestFlatten()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            using (MemoryStream stream = new MemoryStream())
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveStream(stream);
                synth.Speak("This is a test for flattening");
                stream.Seek(0, SeekOrigin.Begin);

                IWaveSource source = new WaveFileReader(stream);
                Equalizer equalizer = Equalizer.Create10BandEqualizer(source);
                equalizer.SampleFilters[0].SetGain(-9.6f);
                equalizer.SampleFilters[1].SetGain(-9.6f);
                equalizer.SampleFilters[2].SetGain(-9.6f);
                equalizer.SampleFilters[3].SetGain(-3.9f);
                equalizer.SampleFilters[4].SetGain(2.4f);
                equalizer.SampleFilters[5].SetGain(11.1f);
                equalizer.SampleFilters[6].SetGain(15.9f);
                equalizer.SampleFilters[7].SetGain(15.9f);
                equalizer.SampleFilters[8].SetGain(15.9f);
                equalizer.SampleFilters[9].SetGain(16.7f);

                var soundOut = new WasapiOut();
                soundOut.Stopped += (s, e) => waitHandle.Set();

                soundOut.Initialize(equalizer.ToWaveSource());
                soundOut.Play();

                waitHandle.WaitOne();

                soundOut.Dispose();
                equalizer.Dispose();
                source.Dispose();
            }
        }

        [TestMethod]
        public void TestPathingString1()
        {
            string pathingString = @"There are [4;5] lights";
            List<string> pathingOptions = new List<string>() {
                "There are 4 lights"
                , "There are 5 lights"
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = SpeechService.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString2()
        {
            string pathingString = @"There are [4;5;] lights";
            List<string> pathingOptions = new List<string>() {
                "There are 4 lights"
                , "There are 5 lights"
                , "There are  lights"
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = SpeechService.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString3()
        {
            string pathingString = @"There [are;might be;could be] [4;5;] lights;It's dark in here;";
            List<string> pathingOptions = new List<string>() {
                "There are 4 lights"
                , "There are 5 lights"
                , "There are  lights"
                ,"There might be 4 lights"
                , "There might be 5 lights"
                , "There might be  lights"
                ,"There could be 4 lights"
                , "There could be 5 lights"
                , "There could be  lights"
                , "It's dark in here"
                , ""
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = SpeechService.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString4()
        {
            string pathingString = @";;;;;;Seven;;;";
            List<string> pathingOptions = new List<string>() {
                ""
                , "Seven"
            };

            int sevenCount = 0;
            for (int i = 0; i < 10000; i++)
            {
                string pathedString = SpeechService.SpeechFromScript(pathingString);
                if (pathedString == "Seven")
                {
                    sevenCount++;
                }
            }

            Assert.IsTrue(sevenCount > 750);
            Assert.IsTrue(sevenCount < 1500);
        }

        [TestMethod]
        public void TestPathingString5()
        {
            string pathingString = @"You leave me [no choice].";
            List<string> pathingOptions = new List<string>() {
                "You leave me no choice."
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = SpeechService.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString6()
        {
            string pathingString = @"[There can be only one.]";
            List<string> pathingOptions = new List<string>() {
                "There can be only one."
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = SpeechService.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }
    }
}