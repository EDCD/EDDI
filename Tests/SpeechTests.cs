using EddiDataDefinitions;
using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "SpeechTests" )]
    public class SpeechTests : TestBase
    {
        [TestInitialize]
        public void start ()
        {
            MakeSafe();
        }

        [DataTestMethod, DoNotParallelize]
        [DataRow( "This is your <phoneme alphabet=\"ipa\" ph=\"leɪkɒn\">Lakon</phoneme>.", true )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈdɛltə\">delta</phoneme> system.", true )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈlaʊ.təns\">Luyten's</phoneme> <phoneme alphabet=\"ipa\" ph=\"stɑː\">Star</phoneme> system.", true )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"bliːiː\">Bleae</phoneme> <phoneme alphabet=\"ipa\" ph=\"θuːə\">Thua</phoneme> system.", true )]
        [DataRow( "You are travelling to the Amnemoi system.", false )]
        public void TestStarSystemPhonemes (string inputSpeech, bool useSSML)
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            using ( var stream = new MemoryStream() )
            using ( var synth = new SpeechSynthesizer() )
            {
                synth.SetOutputToWaveStream( stream );

                if ( useSSML )
                {
                    synth.SpeakSsml($"<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\">{inputSpeech}</speak>" );
                }
                else
                {
                    synth.Speak( inputSpeech );
                }

                stream.Seek( 0, SeekOrigin.Begin );

                var source = new WaveFileReader(stream);

                var soundOut = new WasapiOut();
                soundOut.PlaybackStopped += ( s, e ) => waitHandle.Set();

                soundOut.Init( source );
                soundOut.Play();

                waitHandle.WaitOne();
                soundOut.Dispose();
                source.Dispose();
            }
        }

        [TestMethod, DoNotParallelize]
        public void TestSagAStar ()
        {
            var SagI = "Sagittarius A*";
            var translated = Translations.GetTranslation(SagI);
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( "Vulture" ), translated );
        }

        [DataTestMethod, DoNotParallelize]
        [DataRow( "Vulture", @"<break time=""100ms""/>Fred's ship." )]
        [DataRow( "Vulture", @"<break time=""100ms""/>7 < 10." )]
        [DataRow( "Vulture", @"<break time=""100ms""/>He said ""Foo""." )]
        public void TestSsml (string edModel, string ssml)
        {
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( edModel ), ssml );
        }

        [TestMethod, DoNotParallelize]
        public void TestSsml2 ()
        {
            Logging.Verbose = true;
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( "Vulture" ), @"<break time=""100ms""/>We're on our way to " + Translations.GetTranslation( "i Bootis" ) + "." );
        }

        [TestMethod, DoNotParallelize]
        public void TestSsml3 ()
        {
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( "Anaconda" ), "You are travelling to the " + Translations.GetTranslation( "Hotas" ) + " system." );
        }

        [TestMethod, DoNotParallelize]
        public void TestAudio ()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            using ( var stream = new MemoryStream() )
            using ( var synth = new SpeechSynthesizer() )
            {
                synth.SetOutputToWaveStream( stream );

                synth.SpeakSsml( "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><speak version = \"1.0\" xmlns = \"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\"><s><audio src=\"C:\\Users\\jgm\\Desktop\\positive.wav\"/>You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈlaʊ.təns\">Luyten's</phoneme> <phoneme alphabet=\"ipa\" ph=\"stɑː\">Star</phoneme> system.</s></speak>" );
                stream.Seek( 0, SeekOrigin.Begin );

                var source = new WaveFileReader(stream);

                var soundOut = new WasapiOut();
                soundOut.PlaybackStopped += ( s, e ) => waitHandle.Set();

                soundOut.Init( source );
                soundOut.Play();

                waitHandle.WaitOne();
                soundOut.Dispose();
                source.Dispose();
            }
        }

        [TestMethod, DoNotParallelize]
        public void TestCallsign ()
        {
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( "Vulture" ), Translations.ICAO( "GAB-1655" ) );
        }

        [TestMethod, DoNotParallelize]
        public void TestPowerplay ()
        {
            var ship = ShipDefinitions.FromEDModel( "Anaconda" );
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
            foreach ( var powerName in powerNames )
            {
                speaker.Say( ship, Translations.getPhoneticPower( powerName ) + "." );
            }
        }

        [DataTestMethod, DoNotParallelize]
        [DataRow( 0 )]
        [DataRow( 20 )]
        [DataRow( 40 )]
        [DataRow( 60 )]
        [DataRow( 80 )]
        [DataRow( 100 )]
        public void TestDamageDistortion (int shipHealth)
        {
            var speech = new EddiSpeech( $"Systems at {shipHealth}%.", null, 0, null, 
                50, LandingPadSize.Large, shipHealth, false, true );
            SpeechService.Speak( speech );
        }

        [TestMethod, DoNotParallelize]
        public void TestVariants ()
        {
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( "Vulture" ), "Welcome to your Vulture.  Weapons online." );
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( "Python" ), "Welcome to your Python.  Scanning at full range." );
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( "Anaconda" ), "Welcome to your Anaconda.  All systems operational." );
        }

        [DataTestMethod, DoNotParallelize]
        [DataRow( 0 )]
        [DataRow( 20 )]
        [DataRow( 40 )]
        [DataRow( 60 )]
        [DataRow( 80 )]
        [DataRow( 100 )]
        public void TestChorus ( int chorusLevel )
        {
            SpeechService.Instance.Speak( $"Chorus level {chorusLevel}", null, 0, 0, chorusLevel, 0, false, 0 );
        }

        [TestMethod, DoNotParallelize]
        public void TestRadio ()
        {
            SpeechService.Instance.Say( ShipDefinitions.FromEDModel( "Python" ), "Anaconda golf foxtrot lima one niner six eight returning from orbit.", 3, null, true );
        }

        [DataTestMethod, DoNotParallelize]
        [DataRow( 0 )]
        [DataRow( 100 )]
        [DataRow( 200 )]
        [DataRow( 400 )]
        [DataRow( 800 )]
        public void TestEchoDelay (int echoDelayMs)
        {
            SpeechService.Instance.Speak( $"Echo delay {echoDelayMs}", null, echoDelayMs, 0, 0, 0, false, 0 );
        }

        [TestMethod, DoNotParallelize]
        public void TestDropOff ()
        {
            var synth = new SpeechSynthesizer();
            using ( var stream = new MemoryStream() )
            {
                synth.SetOutputToWaveStream( stream );
                synth.Speak( "Testing drop-off." );
                stream.Seek( 0, SeekOrigin.Begin );
                var source = new WaveFileReader(stream);
                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                var soundOut = new WasapiOut();
                soundOut.Init( source );
                soundOut.PlaybackStopped += ( s, e ) => waitHandle.Set();
                soundOut.Play();
                waitHandle.WaitOne();
                soundOut.Dispose();
                source.Dispose();
            }
            SpeechService.Instance.Speak( "Testing drop-off.", null, 50, 1, 30, 40, true, 0 );
        }

        [TestMethod, DoNotParallelize]
        public void TestSpeechServicePhonemes ()
        {
            Logging.Verbose = true;
            SpeechService.Instance.Speak( "You are  docked at Jameson Memorial  in the <phoneme alphabet=\"ipa\" ph=\"ʃɪnˈrɑːrtə\">Shinrarta</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈdezɦrə\">Dezhra</phoneme> system.", null, 50, 1, 30, 40, true, 0 );
        }

        [TestMethod, DoNotParallelize]
        public void TestSpeechServiceQueue ()
        {
            var thread1 = new Thread(() => SpeechService.Instance.Say(null, "Hello."))
            {
                IsBackground = true
            };

            var thread2 = new Thread(() => SpeechService.Instance.Say(null, "Goodbye."))
            {
                IsBackground = true
            };

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();
        }

        [TestMethod, DoNotParallelize]
        public void TestSpeechServicePhonetics1 ()
        {
            SpeechService.Instance.Say( null, @"Destination confirmed. your <phoneme alphabet=""ipa"" ph=""ˈkəʊbrə"">cobra</phoneme> <phoneme alphabet=""ipa"" ph=""mɑːk"">Mk.</phoneme> <phoneme alphabet=""ipa"" ph=""θriː"">III</phoneme> is travelling to the L T T 1 7 8 6 8 system. This is your first visit to this system. L T T 1 7 8 6 8 is a Federation Corporate with a population of Over 65 thousand souls, aligned to <phoneme alphabet=""ipa"" ph=""fəˈlɪʃɪə"">Felicia</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwɪntəs"">Winters</phoneme>. Kungurutii Gold Power Org is the immediate faction. There are 2 orbital stations and a single planetary station in this system." );
        }

        [TestMethod, DoNotParallelize]
        public void TestSpeechServiceStress ()
        {
            Logging.Verbose = true;
            for ( var i = 0; i < 3; i++ )
            {
                SpeechService.Instance.Say( null, "A two-second test." );
            }

            Thread.Sleep( 5000 );
        }

        [TestMethod, DoNotParallelize]
        public void TestSpeechServiceRadio ()
        {
            Logging.Verbose = true;
            SpeechService.Instance.Say( null, "Your python has touched down.", 3, null, true );
        }

        [TestMethod, DoNotParallelize]
        public void TestSpeechNullInvalidVoice ()
        {
            // Test null voice
            SpeechService.Instance.Say( null, "Testing null voice", 3, null, false );
            // Test invalid voice
            SpeechService.Instance.Say( null, "Testing invalid voice", 3, "No such voice", false );
        }

        [TestMethod, DoNotParallelize]
        public void TestSpeechPhonemes ()
        {
            var line = @"<phoneme alphabet=""ipa"" ph=""iˈlɛktrə"">Electra</phoneme>";
            SpeechService.Instance.Speak( line, null, 0, 40, 0, 0, false, 0 );
        }
    }
}
