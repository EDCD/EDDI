using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder;
using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using Utilities;

namespace SpeechTests
{
    [TestClass]
    public class SpeechTests : UnitTests.TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod, TestCategory("Speech")]
        public void TestPhonemes()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            using (MemoryStream stream = new MemoryStream())
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveStream(stream);

                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><speak version=\"1.0\" xmlns=\"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>This is your <phoneme alphabet=\"ipa\" ph=\"leɪkɒn\">Lakon</phoneme>.</s></speak>");

                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈdɛltə\">delta</phoneme> system.</s></speak>");
                synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈlaʊ.təns\">Luyten's</phoneme> <phoneme alphabet=\"ipa\" ph=\"stɑː\">Star</phoneme> system.</s></speak>");
                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"bliːiː\">Bleae</phoneme> <phoneme alphabet=\"ipa\" ph=\"θuːə\">Thua</phoneme> system.</s></speak>");
                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the Amnemoi system.</s></speak>");
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

        [TestMethod, TestCategory("Speech")]
        public void TestSagAStar()
        {
            string SagI = "Sagittarius A*";
            string translated = Translations.GetTranslation(SagI);
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049309), translated);
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSsml1()
        {
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049309), @"<break time=""100ms""/>Fred's ship.");
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSsml2()
        {
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049309), @"<break time=""100ms""/>7 < 10.");
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSsml3()
        {
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049309), @"<break time=""100ms""/>He said ""Foo"".");
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSsml4()
        {
            Logging.Verbose = true;
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049309), @"<break time=""100ms""/>We're on our way to " + Translations.GetTranslation("i Bootis") + ".");
        }

        [TestMethod, TestCategory("Speech")]
        public void TestAudio()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            using (MemoryStream stream = new MemoryStream())
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveStream(stream);

                synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s><audio src=\"C:\\Users\\jgm\\Desktop\\positive.wav\"/>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈlaʊ.təns\">Luyten's</phoneme> <phoneme alphabet=\"ipa\" ph=\"stɑː\">Star</phoneme> system.</s></speak>");
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

        [TestMethod, TestCategory("Speech")]
        public void TestCallsign()
        {
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049309), Translations.ICAO("GAB-1655"));
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSsml()
        {
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049363), "You are travelling to the " + Translations.GetTranslation("Hotas") + " system.");
        }

        [TestMethod, TestCategory("Speech")]
        public void TestPowerplay()
        {
            var ship = ShipDefinitions.FromEliteID(128049363);
            var speaker = SpeechService.Instance;
            string[] powerNames = {
                "Aisling Duval",
                "Archon Delaine",
                "Arissa Lavigny-Duval",
                "Denton Patreus",
                "Edmund Mahon",
                "Felicia Winters",
                "Pranav Antal",
                "Zachary Hudson",
                "Zemina Torval",
                "Li Yong-Rui"
            };
            foreach (string powerName in powerNames)
            {
                speaker.Say(ship, Translations.getPhoneticPower(powerName) + ".");
            }
        }

        [TestMethod, TestCategory("Speech")]
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

        [TestMethod, TestCategory("Speech")]
        public void TestDamage()
        {
            Ship ship = ShipDefinitions.FromEliteID(128049363);
            var origHealth = ship.health;
            ship.health = 100;
            SpeechService.Instance.Say(ship, "Systems fully operational.");
            ship.health = 80;
            SpeechService.Instance.Say(ship, "Systems at 80%.");
            ship.health = 60;
            SpeechService.Instance.Say(ship, "Systems at 60%.");
            ship.health = 40;
            SpeechService.Instance.Say(ship, "Systems at 40%.");
            ship.health = 20;
            SpeechService.Instance.Say(ship, "Systems at 20%.");
            ship.health = 0;
            SpeechService.Instance.Say(ship, "Systems critical.");
            ship.health = origHealth;
        }

        [TestMethod, TestCategory("Speech")]
        public void TestVariants()
        {
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049309), "Welcome to your Vulture.  Weapons online.");
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049339), "Welcome to your Python.  Scanning at full range.");
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049363), "Welcome to your Anaconda.  All systems operational.");
        }

        [TestMethod, TestCategory("Speech")]
        public void TestChorus()
        {
            SpeechService.Instance.Speak("Chorus level 0", null, 0, 0, 0, 0, 0, true);
            SpeechService.Instance.Speak("Chorus level 20", null, 0, 0, 20, 0, 0, true);
            SpeechService.Instance.Speak("Chorus level 40", null, 0, 0, 40, 0, 0, true);
            SpeechService.Instance.Speak("Chorus level 60", null, 0, 0, 60, 0, 0, true);
            SpeechService.Instance.Speak("Chorus level 80", null, 0, 0, 80, 0, 0, true);
            SpeechService.Instance.Speak("Chorus level 100", null, 0, 0, 100, 0, 0, true);
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSendAndReceive()
        {
            SpeechService.Instance.Say(ShipDefinitions.FromEliteID(128049339), "Anaconda golf foxtrot lima one niner six eight returning from orbit.", 3, null, true);
        }

        [TestMethod, TestCategory("Speech")]
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

        [TestMethod, TestCategory("Speech")]
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
            SpeechService.Instance.Speak("Testing drop-off.", null, 50, 1, 30, 40, 0, true);
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSpeechServicePhonemes()
        {
            Logging.Verbose = true;
            SpeechService.Instance.Speak("You are  docked at Jameson Memorial  in the <phoneme alphabet=\"ipa\" ph=\"ʃɪnˈrɑːrtə\">Shinrarta</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈdezɦrə\">Dezhra</phoneme> system.", null, 50, 1, 30, 40, 0, true);
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSpeechServiceQueue()
        {
            Thread thread1 = new Thread(() => SpeechService.Instance.Say(null, "Hello."))
            {
                IsBackground = true
            };

            Thread thread2 = new Thread(() => SpeechService.Instance.Say(null, "Goodbye."))
            {
                IsBackground = true
            };

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSpeechServicePhonetics1()
        {
            SpeechService.Instance.Say(null, @"Destination confirmed. your <phoneme alphabet=""ipa"" ph=""ˈkəʊbrə"">cobra</phoneme> <phoneme alphabet=""ipa"" ph=""mɑːk"">Mk.</phoneme> <phoneme alphabet=""ipa"" ph=""θriː"">III</phoneme> is travelling to the L T T 1 7 8 6 8 system. This is your first visit to this system. L T T 1 7 8 6 8 is a Federation Corporate with a population of Over 65 thousand souls, aligned to <phoneme alphabet=""ipa"" ph=""fəˈlɪʃɪə"">Felicia</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwɪntəs"">Winters</phoneme>. Kungurutii Gold Power Org is the immediate faction. There are 2 orbital stations and a single planetary station in this system.");
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSpeechServiceStress()
        {
            Logging.Verbose = true;
            for (int i = 0; i < 3; i++)
            {
                SpeechService.Instance.Say(null, "A two-second test.");
            }

            Thread.Sleep(5000);
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSpeechServiceRadio()
        {
            Logging.Verbose = true;
            SpeechService.Instance.Say(null, "Your python has touched down.", 3, null, true);
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSpeechNullInvalidVoice()
        {
            // Test null voice
            SpeechService.Instance.Say(null, "Testing null voice", 3, null, false);
            // Test invalid voice
            SpeechService.Instance.Say(null, "Testing invalid voice", 3, "No such voice", false);
        }

        [TestMethod, TestCategory("Speech"), DoNotParallelize]
        public void TestFSSDiscoveryScan()
        {
            SpeechResponder speechresponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");

            string autoscan = @"{ ""timestamp"":""2022-02-18T07:14:01Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Wolf 1414 A"", ""BodyID"":1, ""Parents"":[ {""Null"":0} ], ""StarSystem"":""Wolf 1414"", ""SystemAddress"":83718345434, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""K"", ""Subclass"":6, ""StellarMass"":0.542969, ""Radius"":485181728.000000, ""AbsoluteMagnitude"":8.023544, ""Age_MY"":3118, ""SurfaceTemperature"":3913.000000, ""Luminosity"":""V"", ""SemiMajorAxis"":198208671808.242798, ""Eccentricity"":0.295083, ""OrbitalInclination"":34.113437, ""Periapsis"":303.694287, ""OrbitalPeriod"":196849131.584167, ""AscendingNode"":31.266731, ""MeanAnomaly"":53.999015, ""RotationPeriod"":214740.119514, ""AxialTilt"":0.000000, ""WasDiscovered"":true, ""WasMapped"":false }";
            string honk = @"{ ""timestamp"":""2022-02-18T07:14:02Z"", ""event"":""FSSDiscoveryScan"", ""Progress"":0.193470, ""BodyCount"":27, ""NonBodyCount"":10, ""SystemName"":""Wolf 1414"", ""SystemAddress"":83718345434 }";
            string secondstar = @"{ ""timestamp"":""2022-02-18T07:14:03Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Wolf 1414 B"", ""BodyID"":2, ""Parents"":[ {""Null"":0} ], ""StarSystem"":""Wolf 1414"", ""SystemAddress"":83718345434, ""DistanceFromArrivalLS"":1472.392323, ""StarType"":""M"", ""Subclass"":3, ""StellarMass"":0.367188, ""Radius"":408410944.000000, ""AbsoluteMagnitude"":8.709381, ""Age_MY"":3118, ""SurfaceTemperature"":3087.000000, ""Luminosity"":""Va"", ""SemiMajorAxis"":293087559938.430786, ""Eccentricity"":0.295083, ""OrbitalInclination"":34.113437, ""Periapsis"":123.694292, ""OrbitalPeriod"":196849131.584167, ""AscendingNode"":31.266731, ""MeanAnomaly"":53.999018, ""RotationPeriod"":255361.700784, ""AxialTilt"":0.000000, ""WasDiscovered"":true, ""WasMapped"":false }";

            List<EddiEvents.Event> events = new List<EddiEvents.Event>();
            events.AddRange(EddiJournalMonitor.JournalMonitor.ParseJournalEntry(autoscan));
            events.AddRange(EddiJournalMonitor.JournalMonitor.ParseJournalEntry(honk));
            events.AddRange(EddiJournalMonitor.JournalMonitor.ParseJournalEntry(secondstar));
            foreach (EddiEvents.Event @event in events)
            {
                speechresponder.Handle(@event);
            }
        }

        [TestMethod, TestCategory("Speech")]
        public void TestSpeechPhonemes()
        {
            var line = @"<phoneme alphabet=""ipa"" ph=""iˈlɛktrə"">Electra</phoneme>";
            SpeechService.Instance.Speak(line, null, 0, 40, 0, 0, 0);
        }
    }
}
