//using EddiDataDefinitions;
//using EddiSpeechService;
//using EddiSpeechService.SpeechEffects;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using NAudio.Wave;
//using System.Collections.Generic;
//using System.IO;
//using System.Speech.Synthesis;
//using System.Threading;
//using Utilities;

//namespace Tests
//{
//    [TestClass, TestCategory("SpeechTests")]
//    public class SpeechTests : TestBase
//    {
//        [TestInitialize]
//        public void start()
//        {
//            MakeSafe();
//        }

//        [TestMethod]
//        public void TestPhonemes()
//        {
//            EventWaitHandle waitHandle = new AutoResetEvent(false);

//            using (var stream = new MemoryStream())
//            using (var synth = new SpeechSynthesizer())
//            {
//                synth.SetOutputToWaveStream(stream);

//                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><speak version=\"1.0\" xmlns=\"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>This is your <phoneme alphabet=\"ipa\" ph=\"leɪkɒn\">Lakon</phoneme>.</s></speak>");

//                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈdɛltə\">delta</phoneme> system.</s></speak>");
//                synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈlaʊ.təns\">Luyten's</phoneme> <phoneme alphabet=\"ipa\" ph=\"stɑː\">Star</phoneme> system.</s></speak>");
//                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"bliːiː\">Bleae</phoneme> <phoneme alphabet=\"ipa\" ph=\"θuːə\">Thua</phoneme> system.</s></speak>");
//                //synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s>You are travelling to the Amnemoi system.</s></speak>");
//                //synth.Speak("You are travelling to the Barnard's Star system.");
//                stream.Seek(0, SeekOrigin.Begin);

//                IWaveSource source = new WaveFileReader(stream);

//                var soundOut = new WasapiOut();
//                soundOut.Stopped += (s, e) => waitHandle.Set();

//                soundOut.Initialize(source);
//                soundOut.Play();

//                waitHandle.WaitOne();
//                soundOut.Dispose();
//                source.Dispose();
//            }
//        }

//        [TestMethod]
//        public void TestSagAStar()
//        {
//            var SagI = "Sagittarius A*";
//            var translated = Translations.GetTranslation(SagI);
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Vulture" ), translated);
//        }

//        [TestMethod]
//        public void TestSsml1()
//        {
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Vulture" ), @"<break time=""100ms""/>Fred's ship.");
//        }

//        [TestMethod]
//        public void TestSsml2()
//        {
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Vulture" ), @"<break time=""100ms""/>7 < 10.");
//        }

//        [TestMethod]
//        public void TestSsml3()
//        {
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Vulture" ), @"<break time=""100ms""/>He said ""Foo"".");
//        }

//        [TestMethod]
//        public void TestSsml4()
//        {
//            Logging.Verbose = true;
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Vulture" ), @"<break time=""100ms""/>We're on our way to " + Translations.GetTranslation("i Bootis") + ".");
//        }

//        [TestMethod]
//        public void TestAudio()
//        {
//            EventWaitHandle waitHandle = new AutoResetEvent(false);

//            using (var stream = new MemoryStream())
//            using (var synth = new SpeechSynthesizer())
//            {
//                synth.SetOutputToWaveStream(stream);

//                synth.SpeakSsml("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s><audio src=\"C:\\Users\\jgm\\Desktop\\positive.wav\"/>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈlaʊ.təns\">Luyten's</phoneme> <phoneme alphabet=\"ipa\" ph=\"stɑː\">Star</phoneme> system.</s></speak>");
//                stream.Seek(0, SeekOrigin.Begin);

//                IWaveSource source = new WaveFileReader(stream);

//                var soundOut = new WasapiOut();
//                soundOut.Stopped += (s, e) => waitHandle.Set();

//                soundOut.Initialize(source);
//                soundOut.Play();

//                waitHandle.WaitOne();
//                soundOut.Dispose();
//                source.Dispose();
//            }
//        }

//        [TestMethod]
//        public void TestCallsign()
//        {
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Vulture" ), Translations.ICAO("GAB-1655"));
//        }

//        [TestMethod]
//        public void TestSsml()
//        {
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Anaconda" ), "You are travelling to the " + Translations.GetTranslation("Hotas") + " system.");
//        }

//        [TestMethod]
//        public void TestPowerplay()
//        {
//            var ship = ShipDefinitions.FromEDModel( "Anaconda" );
//            var speaker = SpeechService.Instance;
//            string[] powerNames = {
//                "Aisling Duval",
//                "Archon Delaine",
//                "Arissa Lavigny-Duval",
//                "Denton Patreus",
//                "Edmund Mahon",
//                "Felicia Winters",
//                "Pranav Antal",
//                "Zachary Hudson",
//                "Zemina Torval",
//                "Li Yong-Rui"
//            };
//            foreach (var powerName in powerNames)
//            {
//                speaker.Say(ship, Translations.getPhoneticPower(powerName) + ".");
//            }
//        }

//        [TestMethod]
//        public void TestExtendedSource()
//        {
//            EventWaitHandle waitHandle = new AutoResetEvent(false);

//            using (var stream = new MemoryStream())
//            using (var synth = new SpeechSynthesizer())
//            {
//                synth.SetOutputToWaveStream(stream);
//                synth.Speak("Test.");
//                stream.Seek(0, SeekOrigin.Begin);

//                var streams = new List<IWaveProvider>
//                {
//                    new ExtendedDurationWaveStream(new WaveFileReader(stream), 2000), 
//                    new DmoWavesReverbEffect(source) { ReverbMix = -10 }
//                };
//                var source = new ConcatenatingWaveProvider( streams );

//                var soundOut = new WasapiOut();
//                soundOut.Stopped += (s, e) => waitHandle.Set();

//                soundOut.Initialize(source);
//                soundOut.Play();

//                waitHandle.WaitOne();
//                soundOut.Dispose();
//                source.Dispose();
//            }
//        }

//        [TestMethod]
//        public void TestDamage()
//        {
//            var ship = ShipDefinitions.FromEDModel( "Anaconda" );
//            var origHealth = ship.health;
//            ship.health = 100;
//            SpeechService.Instance.Say(ship, "Systems fully operational.");
//            ship.health = 80;
//            SpeechService.Instance.Say(ship, "Systems at 80%.");
//            ship.health = 60;
//            SpeechService.Instance.Say(ship, "Systems at 60%.");
//            ship.health = 40;
//            SpeechService.Instance.Say(ship, "Systems at 40%.");
//            ship.health = 20;
//            SpeechService.Instance.Say(ship, "Systems at 20%.");
//            ship.health = 0;
//            SpeechService.Instance.Say(ship, "Systems critical.");
//            ship.health = origHealth;
//        }

//        [TestMethod]
//        public void TestVariants()
//        {
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Vulture" ), "Welcome to your Vulture.  Weapons online.");
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Python" ), "Welcome to your Python.  Scanning at full range.");
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Anaconda" ), "Welcome to your Anaconda.  All systems operational.");
//        }

//        [TestMethod]
//        public void TestChorus()
//        {
//            SpeechService.Instance.Speak("Chorus level 0", null, 0, 0, 0, 0, 0, true);
//            SpeechService.Instance.Speak("Chorus level 20", null, 0, 0, 20, 0, 0, true);
//            SpeechService.Instance.Speak("Chorus level 40", null, 0, 0, 40, 0, 0, true);
//            SpeechService.Instance.Speak("Chorus level 60", null, 0, 0, 60, 0, 0, true);
//            SpeechService.Instance.Speak("Chorus level 80", null, 0, 0, 80, 0, 0, true);
//            SpeechService.Instance.Speak("Chorus level 100", null, 0, 0, 100, 0, 0, true);
//        }

//        [TestMethod]
//        public void TestSendAndReceive()
//        {
//            SpeechService.Instance.Say(ShipDefinitions.FromEDModel( "Python" ), "Anaconda golf foxtrot lima one niner six eight returning from orbit.", 3, null, true);
//        }

//        [TestMethod]
//        public void TestSpeech()
//        {
//            var synth = new SpeechSynthesizer();
//            using (var stream = new MemoryStream())
//            {
//                synth.SetOutputToWaveStream(stream);
//                synth.Speak("This is a test.");
//                stream.Seek(0, SeekOrigin.Begin);
//                IWaveSource source = new WaveFileReader(stream);
//                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
//                var soundOut = new WasapiOut();
//                var echoSource = new DmoEchoEffect(source);
//                soundOut.Initialize(echoSource);
//                soundOut.Stopped += (s, e) => waitHandle.Set();
//                soundOut.Play();
//                waitHandle.WaitOne();
//                soundOut.Dispose();
//                source.Dispose();
//            }
//        }

//        [TestMethod]
//        public void TestDropOff()
//        {
//            var synth = new SpeechSynthesizer();
//            using (var stream = new MemoryStream())
//            {
//                synth.SetOutputToWaveStream(stream);
//                synth.Speak("Testing drop-off.");
//                stream.Seek(0, SeekOrigin.Begin);
//                IWaveSource source = new WaveFileReader(stream);
//                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
//                var soundOut = new WasapiOut();
//                soundOut.Initialize(source);
//                soundOut.Stopped += (s, e) => waitHandle.Set();
//                soundOut.Play();
//                waitHandle.WaitOne();
//                soundOut.Dispose();
//                source.Dispose();
//            }
//            SpeechService.Instance.Speak("Testing drop-off.", null, 50, 1, 30, 40, 0, true);
//        }

//        [TestMethod]
//        public void TestSpeechServicePhonemes()
//        {
//            Logging.Verbose = true;
//            SpeechService.Instance.Speak("You are  docked at Jameson Memorial  in the <phoneme alphabet=\"ipa\" ph=\"ʃɪnˈrɑːrtə\">Shinrarta</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈdezɦrə\">Dezhra</phoneme> system.", null, 50, 1, 30, 40, 0, true);
//        }

//        [TestMethod]
//        public void TestSpeechServiceQueue()
//        {
//            var thread1 = new Thread(() => SpeechService.Instance.Say(null, "Hello."))
//            {
//                IsBackground = true
//            };

//            var thread2 = new Thread(() => SpeechService.Instance.Say(null, "Goodbye."))
//            {
//                IsBackground = true
//            };

//            thread1.Start();
//            thread2.Start();

//            thread1.Join();
//            thread2.Join();
//        }

//        [TestMethod]
//        public void TestSpeechServicePhonetics1()
//        {
//            SpeechService.Instance.Say(null, @"Destination confirmed. your <phoneme alphabet=""ipa"" ph=""ˈkəʊbrə"">cobra</phoneme> <phoneme alphabet=""ipa"" ph=""mɑːk"">Mk.</phoneme> <phoneme alphabet=""ipa"" ph=""θriː"">III</phoneme> is travelling to the L T T 1 7 8 6 8 system. This is your first visit to this system. L T T 1 7 8 6 8 is a Federation Corporate with a population of Over 65 thousand souls, aligned to <phoneme alphabet=""ipa"" ph=""fəˈlɪʃɪə"">Felicia</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwɪntəs"">Winters</phoneme>. Kungurutii Gold Power Org is the immediate faction. There are 2 orbital stations and a single planetary station in this system.");
//        }

//        [TestMethod]
//        public void TestSpeechServiceStress()
//        {
//            Logging.Verbose = true;
//            for (var i = 0; i < 3; i++)
//            {
//                SpeechService.Instance.Say(null, "A two-second test.");
//            }

//            Thread.Sleep(5000);
//        }

//        [TestMethod]
//        public void TestSpeechServiceRadio()
//        {
//            Logging.Verbose = true;
//            SpeechService.Instance.Say(null, "Your python has touched down.", 3, null, true);
//        }

//        [TestMethod]
//        public void TestSpeechNullInvalidVoice()
//        {
//            // Test null voice
//            SpeechService.Instance.Say(null, "Testing null voice", 3, null, false);
//            // Test invalid voice
//            SpeechService.Instance.Say(null, "Testing invalid voice", 3, "No such voice", false);
//        }

//        [TestMethod]
//        public void TestSpeechPhonemes()
//        {
//            var line = @"<phoneme alphabet=""ipa"" ph=""iˈlɛktrə"">Electra</phoneme>";
//            SpeechService.Instance.Speak(line, null, 0, 40, 0, 0, 0);
//        }
//    }
//}
