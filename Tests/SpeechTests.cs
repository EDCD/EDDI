using EddiDataDefinitions;
using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;

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
        [DataRow( "This is your <phoneme alphabet=\"ipa\" ph=\"leɪkɒn\">Lakon</phoneme>." )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈdɛltə\">delta</phoneme> system." )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈlaʊ.təns\">Luyten's</phoneme> <phoneme alphabet=\"ipa\" ph=\"stɑː\">Star</phoneme> system." )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"bliːiː\">Bleae</phoneme> <phoneme alphabet=\"ipa\" ph=\"θuːə\">Thua</phoneme> system." )]
        [DataRow( "You are travelling to the Amnemoi system." )]
        [DataRow( "You are  docked at Jameson Memorial  in the <phoneme alphabet=\"ipa\" ph=\"ʃɪnˈrɑːrtə\">Shinrarta</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈdezɦrə\">Dezhra</phoneme> system." )]
        [DataRow( @"Destination confirmed. your <phoneme alphabet=""ipa"" ph=""ˈkəʊbrə"">cobra</phoneme> <phoneme alphabet=""ipa"" ph=""mɑːk"">Mk.</phoneme> <phoneme alphabet=""ipa"" ph=""θriː"">III</phoneme> is travelling to the L T T 1 7 8 6 8 system. This is your first visit to this system. L T T 1 7 8 6 8 is a Federation Corporate with a population of Over 65 thousand souls, aligned to <phoneme alphabet=""ipa"" ph=""fəˈlɪʃɪə"">Felicia</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwɪntəs"">Winters</phoneme>. Kungurutii Gold Power Org is the immediate faction. There are 2 orbital stations and a single planetary station in this system." )]
        [DataRow( @"<phoneme alphabet=""ipa"" ph=""iˈlɛktrə"">Electra</phoneme>" )]
        public void TestPhonetics (string inputSpeech)
        {
            SpeechService.Speak(new EddiSpeech(inputSpeech));
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

        [DataTestMethod, DoNotParallelize]
        [DataRow( "Your python has touched down." )]
        [DataRow( "Anaconda golf foxtrot lima one niner six eight returning from orbit." )]
        public void TestRadio (string msg)
        {
            SpeechService.Speak(new EddiSpeech(msg, radio: true));
        }

        [DataTestMethod, DoNotParallelize]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 3 )]
        public void TestEchoDelay (int landingPadSize)
        {
            var shipSize = LandingPadSize.AllOfThem.First( s => s.sizeIndex == landingPadSize );
            SpeechService.Speak( new EddiSpeech( $"Echo delay for a {shipSize} ship.", shipSize: shipSize ) );
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
        public void TestSpeechNullInvalidVoice ()
        {
            SpeechService.Speak( new EddiSpeech( "Testing null voice", null ) );
            SpeechService.Speak( new EddiSpeech( "Testing non-valid voice", "No such voice" ) );
        }
    }
}
