using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDDIVAPlugin;
using System.Collections.Generic;
using System;
using System.Speech.Synthesis;
using System.IO;
using System.Speech.AudioFormat;
using System.Threading;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using EliteDangerousSpeechService;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using EliteDangerousDataDefinitions;

namespace Tests
{
    [TestClass]
    public class SpeechTests
    {
        [TestMethod]
        public void TestPhonemes()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            using (MemoryStream stream = new MemoryStream())
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveStream(stream);

                synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ʃjɛ\">Tse</phoneme> system.</s></speak>");
                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"bliːiː\">Bleae</phoneme> <phoneme alphabet=\"ipa\" ph=\"θuːə\">Thua</phoneme> system.</s></speak>");
                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the Amnemoi system.</s></speak>");
                //synth.Speak("You are travelling to the Barnard's Star system.");
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

        [TestMethod]
        public void TestCallsign()
        {
            SpeechService.Instance.Say(null, ShipDefinitions.FromEliteID(128049309), Translations.CallSign("GAB-1655"), true, true);
        }


        [TestMethod]
        public void TestSsml()
        {
            SpeechService.Instance.Say(null, ShipDefinitions.FromEliteID(128049363), "You are travelling to the " + Translations.StarSystem("Hotas") + " system.", true, true);
        }

        [TestMethod]
        public void TestPowerplay()
        {
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Aisling Duval") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Archon Delaine") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Arissa Lavigny-Duval") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Denton Patreus") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Edmund Mahon") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Felicia Winters") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Pranav Antal") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Zachary Hudson") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Zemina Torval") + ".");
            SpeechService.Instance.Say(null, ShipDefinitions.FromEliteID(128049363), Translations.Power("Li Yong-Rui") + ".", true, true);
        }

        [TestMethod]
        public void TestExtendedSource()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            using (MemoryStream stream = new MemoryStream())
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveStream(stream);
                synth.Speak("Test.");
                stream.Seek(0, SeekOrigin.Begin);

                IWaveSource source = new ExtendedDurationWaveSource(new WaveFileReader(stream), 2000).AppendSource(x => new DmoWavesReverbEffect(x) { ReverbMix = -10 });

                var soundOut = new WasapiOut();
                soundOut.Stopped += (s, e) => waitHandle.Set();

                soundOut.Initialize(source);
                soundOut.Play();

                waitHandle.WaitOne();
                soundOut.Dispose();
                source.Dispose();
            }
        }

        [TestMethod]
        public void TestDamage()
        {
            Ship ship = ShipDefinitions.FromEliteID(128049363);
            ship.health = 100;
            SpeechService.Instance.Say(null, ship, "Systems fully operational.", true, true);
            ship.health = 80;
            SpeechService.Instance.Say(null, ship, "Systems at 80%.", true, true);
            ship.health = 60;
            SpeechService.Instance.Say(null, ship, "Systems at 60%.", true, true);
            ship.health = 40;
            SpeechService.Instance.Say(null, ship, "Systems at 40%.", true, true);
            ship.health = 20;
            SpeechService.Instance.Say(null, ship, "Systems at 20%.", true, true);
            ship.health = 0;
            SpeechService.Instance.Say(null, ship, "Systems critical.", true, true);
        }

        [TestMethod]
        public void TestVariants()
        {
            SpeechService.Instance.Say(null, ShipDefinitions.FromEliteID(128049309), "Welcome to your Vulture.  Weapons online.", true, true);
            SpeechService.Instance.Transmit(null, ShipDefinitions.FromEliteID(128049309), "Vulture x-ray whiskey tango seven one seven six requesting docking.", true, true);
            SpeechService.Instance.Say(null, ShipDefinitions.FromEliteID(128049339), "Welcome to your Python.  Scanning at full range.", true, true);
            SpeechService.Instance.Transmit(null, ShipDefinitions.FromEliteID(128049339), "Python victor oscar Pappa fife tree fawer niner requesting docking.", true, true);
            SpeechService.Instance.Say(null, ShipDefinitions.FromEliteID(128049363), "Welcome to your Anaconda.  All systems operational.", true, true);
            SpeechService.Instance.Transmit(null, ShipDefinitions.FromEliteID(128049363), "Anaconda charlie november delta one niner eight fawer requesting docking.", true, true);
        }

        [TestMethod]
        public void TestChorus()
        {
            SpeechService.Instance.Speak("Chorus level 0", null, 0, 0, 0, 0, 0, true, true, true);
            SpeechService.Instance.Speak("Chorus level 20", null, 0, 0, 20, 0, 0, true, true, true);
            SpeechService.Instance.Speak("Chorus level 40", null, 0, 0, 40, 0, 0, true, true, true);
            SpeechService.Instance.Speak("Chorus level 60", null, 0, 0, 60, 0, 0, true, true, true);
            SpeechService.Instance.Speak("Chorus level 80", null, 0, 0, 80, 0, 0, true, true, true);
            SpeechService.Instance.Speak("Chorus level 100", null, 0, 0, 100, 0, 0, true, true, true);
        }

        [TestMethod]
        public void TestSendAndReceive()
        {
            SpeechService.Instance.Say(null, ShipDefinitions.FromEliteID(128049339), "Issuing docking request.  Please stand by.", true, true);
            SpeechService.Instance.Transmit(null, ShipDefinitions.FromEliteID(128049339), "Anaconda golf foxtrot lima one niner six eight requesting docking.", true, true);
            SpeechService.Instance.Receive(null, ShipDefinitions.FromEliteID(128049339), "Roger golf foxtrot lima one niner six eight docking request received", true, true);
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
                string pathedString = SpeechService.Instance.SpeechFromScript(pathingString);
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
                string pathedString = SpeechService.Instance.SpeechFromScript(pathingString);
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
                string pathedString = SpeechService.Instance.SpeechFromScript(pathingString);
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
                string pathedString = SpeechService.Instance.SpeechFromScript(pathingString);
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
                string pathedString = SpeechService.Instance.SpeechFromScript(pathingString);
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
                string pathedString = SpeechService.Instance.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString7()
        {
            string pathingString = @"[{TXT:Ship model} {TXT:Ship callsign (spoken)};This is {TXT:Ship model} {TXT:Ship callsign (spoken)}] [requesting docking permission;requesting docking clearance;requesting permission to dock;requesting clearance to dock].";
            List<string> pathingOptions = new List<string>() {
                "{TXT:Ship model} {TXT:Ship callsign (spoken)} requesting docking permission."
                ,"This is {TXT:Ship model} {TXT:Ship callsign (spoken)} requesting docking permission."
                ,"{TXT:Ship model} {TXT:Ship callsign (spoken)} requesting docking clearance."
                ,"This is {TXT:Ship model} {TXT:Ship callsign (spoken)} requesting docking clearance."
                ,"{TXT:Ship model} {TXT:Ship callsign (spoken)} requesting clearance to dock."
                ,"This is {TXT:Ship model} {TXT:Ship callsign (spoken)} requesting clearance to dock."
                ,"{TXT:Ship model} {TXT:Ship callsign (spoken)} requesting permission to dock."
                ,"This is {TXT:Ship model} {TXT:Ship callsign (spoken)} requesting permission to dock."
            };

            HashSet<string> pathingResults = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                string pathedString = SpeechService.Instance.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestSpeech()
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            using (MemoryStream stream = new MemoryStream())
            {
                synth.SetOutputToWaveStream(stream);
                synth.Speak("This is a test.");
                stream.Seek(0, SeekOrigin.Begin);
                IWaveSource source = new WaveFileReader(stream);
                EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                var soundOut = new WasapiOut();
                DmoEchoEffect echoSource = new DmoEchoEffect(source);
                soundOut.Initialize(echoSource);
                soundOut.Stopped += (s, e) => waitHandle.Set();
                soundOut.Play();
                waitHandle.WaitOne();
                soundOut.Dispose();
                source.Dispose();
            }
        }

        [TestMethod]
        public void TestDropOff()
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            using (MemoryStream stream = new MemoryStream())
            {
                synth.SetOutputToWaveStream(stream);
                synth.Speak("Testing drop-off.");
                stream.Seek(0, SeekOrigin.Begin);
                IWaveSource source = new WaveFileReader(stream);
                EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                var soundOut = new WasapiOut();
                soundOut.Initialize(source);
                soundOut.Stopped += (s, e) => waitHandle.Set();
                soundOut.Play();
                waitHandle.WaitOne();
                soundOut.Dispose();
                source.Dispose();
            }
            SpeechService.Instance.Speak("Testing drop-off.", null, 50, 1, 30, 40, 0, false, true, true);
        }

        [TestMethod]
        public void TestSectorTranslations()
        {
            Assert.AreEqual("Swoiwns N Y dash B a95 dash 0", Translations.StarSystem("Swoiwns NY-B a95-0"));
            Assert.AreEqual("P P M 5 2 8 7", Translations.StarSystem("PPM 5287"));
        }

        [TestMethod]
        public void TestSpeechHumanize1()
        {
            Assert.AreEqual("on the way to 12 and a half thousand", Translations.Humanize(12345));
        }

        [TestMethod]
        public void TestSpeechHumanize2()
        {
            Assert.AreEqual(null, Translations.Humanize(null));
        }

        [TestMethod]
        public void TestSpeechHumanize3()
        {
            Assert.AreEqual("zero", Translations.Humanize(0));
        }

        [TestMethod]
        public void TestSpeechHumanize4()
        {
            Assert.AreEqual("0.16", Translations.Humanize(0.15555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize5()
        {
            Assert.AreEqual("0.016", Translations.Humanize(0.015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize6()
        {
            Assert.AreEqual("0.0016", Translations.Humanize(0.0015555555M));
        }
    }
}
