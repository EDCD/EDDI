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

                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"viˈga\">Vega</phoneme> system.</s></speak>");
                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ækɜˈnɑ\">Achenar</phoneme> system.</s></speak>");
                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈsɪɡni\">Cygni</phoneme> system.</s></speak>");
                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈsɪɡnəs\">Cygnus</phoneme> system.</s></speak>");
                // synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ʃɪnˈrɑːrtə\">Shinrarta</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈdezɦrə\">Dezhra</phoneme> system.</s></speak>");
                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈnjuːənɛts\">Reorte</phoneme> system.</s></speak>");
                synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the Eravate system.</s></speak>");

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
            SpeechService SpeechService = new SpeechService();
            SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049309), Translations.CallSign("GAB-1655"));
        }


        [TestMethod]
        public void TestSsml()
        {
            SpeechService SpeechService = new SpeechService();
            SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), "You are travelling to the " + Translations.StarSystem("Hotas") + " system.");
        }

        [TestMethod]
        public void TestPowerplay()
        {
            SpeechService SpeechService = new SpeechService();
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Aisling Duval") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Archon Delaine") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Arissa Lavigny-Duval") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Denton Patreus") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Edmund Mahon") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Felicia Winters") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Pranav Antal") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Zachary Hudson") + ".");
            //SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Zemina Torval") + ".");
            SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), Translations.Power("Li Yong-Rui") + ".");
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
            SpeechService SpeechService = new SpeechService();
            Ship ship = ShipDefinitions.ShipFromEliteID(128049363);
            ship.Health = 100;
            SpeechService.Say(ship, "Systems fully operational.");
            ship.Health = 80;
            SpeechService.Say(ship, "Systems at 80%.");
            ship.Health = 60;
            SpeechService.Say(ship, "Systems at 60%.");
            ship.Health = 40;
            SpeechService.Say(ship, "Systems at 40%.");
            ship.Health = 20;
            SpeechService.Say(ship, "Systems at 20%.");
            ship.Health = 0;
            SpeechService.Say(ship, "Systems critical.");
        }

        [TestMethod]
        public void TestVariants()
        {
            SpeechService SpeechService = new SpeechService();

            SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049309), "Welcome to your Vulture.  Weapons online.");
            SpeechService.Transmit(ShipDefinitions.ShipFromEliteID(128049309), "Vulture x-ray whiskey tango seven one seven six requesting docking.");
            SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049339), "Welcome to your Python.  Scanning at full range.");
            SpeechService.Transmit(ShipDefinitions.ShipFromEliteID(128049339), "Python victor oscar Pappa fife tree fawer niner requesting docking.");
            SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049363), "Welcome to your Anaconda.  All systems operational.");
            SpeechService.Transmit(ShipDefinitions.ShipFromEliteID(128049363), "Anaconda charlie november delta one niner eight fawer requesting docking.");
        }

        [TestMethod]
        public void TestChorus()
        {
            SpeechService SpeechService = new SpeechService();

            SpeechService.Speak("Chorus level 0", null, 0, 0, 0, 0, 0, true);
            SpeechService.Speak("Chorus level 20", null, 0, 0, 20, 0, 0, true);
            SpeechService.Speak("Chorus level 40", null, 0, 0, 40, 0, 0, true);
            SpeechService.Speak("Chorus level 60", null, 0, 0, 60, 0, 0, true);
            SpeechService.Speak("Chorus level 80", null, 0, 0, 80, 0, 0, true);
            SpeechService.Speak("Chorus level 100", null, 0, 0, 100, 0, 0, true);
        }

        [TestMethod]
        public void testSendAndReceive()
        {
            SpeechService SpeechService = new SpeechService();

            SpeechService.Say(ShipDefinitions.ShipFromEliteID(128049339), "Issuing docking request.  Please stand by.");
            SpeechService.Transmit(ShipDefinitions.ShipFromEliteID(128049339), "Anaconda golf foxtrot lima one niner six eight requesting docking.");
            SpeechService.Receive(ShipDefinitions.ShipFromEliteID(128049339), "Roger golf foxtrot lima one niner six eight docking request received");
        }

        [TestMethod]
        public void TestPathingString1()
        {
            SpeechService SpeechService = new SpeechService();

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
            SpeechService SpeechService = new SpeechService();

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
            SpeechService SpeechService = new SpeechService();

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
            SpeechService SpeechService = new SpeechService();

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
            SpeechService SpeechService = new SpeechService();

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
            SpeechService SpeechService = new SpeechService();

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

        [TestMethod]
        public void TestPathingString7()
        {
            SpeechService SpeechService = new SpeechService();

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
                string pathedString = SpeechService.SpeechFromScript(pathingString);
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
    }
}
