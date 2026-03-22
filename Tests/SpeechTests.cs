using EddiDataDefinitions;
using EddiSpeechService;
using EddiSpeechService.SpeechConversions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

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

        [TestMethod, DoNotParallelize]
        [DataRow( "This is your <phoneme alphabet=\"ipa\" ph=\"leɪkɒn\">Lakon</phoneme>." )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈdɛltə\">delta</phoneme> system." )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"ˈlaʊ.təns\">Luyten's</phoneme> <phoneme alphabet=\"ipa\" ph=\"stɑː\">Star</phoneme> system." )]
        [DataRow( "You are travelling to the <phoneme alphabet=\"ipa\" ph=\"bliːiː\">Bleae</phoneme> <phoneme alphabet=\"ipa\" ph=\"θuːə\">Thua</phoneme> system." )]
        [DataRow( "You are travelling to the Amnemoi system." )]
        [DataRow( "You are  docked at Jameson Memorial  in the <phoneme alphabet=\"ipa\" ph=\"ʃɪnˈrɑːrtə\">Shinrarta</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈdezɦrə\">Dezhra</phoneme> system." )]
        [DataRow( @"Destination confirmed. your <phoneme alphabet=""ipa"" ph=""ˈkəʊbrə"">cobra</phoneme> <phoneme alphabet=""ipa"" ph=""mɑːk"">Mk.</phoneme> <phoneme alphabet=""ipa"" ph=""θriː"">III</phoneme> is travelling to the L T T 1 7 8 6 8 system. This is your first visit to this system. L T T 1 7 8 6 8 is a Federation Corporate with a population of Over 65 thousand souls, aligned to <phoneme alphabet=""ipa"" ph=""fəˈlɪʃɪə"">Felicia</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwɪntəs"">Winters</phoneme>. Kungurutii Gold Power Org is the immediate faction. There are 2 orbital stations and a single planetary station in this system." )]
        [DataRow( @"<phoneme alphabet=""ipa"" ph=""iˈlɛktrə"">Electra</phoneme>" )]
        public async Task TestPhoneticsAsync (string inputSpeech)
        {
            await SpeechService.Instance.SpeakAsync(new EddiSpeech(inputSpeech)).ConfigureAwait(false);
        }

        [TestMethod, DoNotParallelize]
        public async Task TestSagAStar ()
        {
            var SagI = "Sagittarius A*";
            var translated = SpeechConversions.GetTranslation(SagI);
            await SpeechService.Instance.SayAsync( ShipDefinitions.FromEDModel( "Vulture" ), translated ).ConfigureAwait(false);
        }

        [TestMethod, DoNotParallelize]
        [DataRow( "Vulture", @"<break time=""100ms""/>Fred's ship." )]
        [DataRow( "Vulture", @"<break time=""100ms""/>7 < 10." )]
        [DataRow( "Vulture", @"<break time=""100ms""/>He said ""Foo""." )]
        public async Task TestSsml (string edModel, string ssml)
        {
            await SpeechService.Instance.SayAsync( ShipDefinitions.FromEDModel( edModel ), ssml ).ConfigureAwait( false );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestSsml2 ()
        {
            await SpeechService.Instance.SayAsync( ShipDefinitions.FromEDModel( "Vulture" ), @"<break time=""100ms""/>We're on our way to " + SpeechConversions.GetTranslation( "i Bootis" ) + "." ).ConfigureAwait( false );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestSsml3 ()
        {
            await SpeechService.Instance.SayAsync( ShipDefinitions.FromEDModel( "Anaconda" ), "You are travelling to the " + SpeechConversions.GetTranslation( "Hotas" ) + " system." ).ConfigureAwait( false );
        }

        [TestMethod, DoNotParallelize]
        public void TestAudio ()
        {
            var waitHandle = new AutoResetEvent(false);

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
        public async Task TestCallsign ()
        {
            await SpeechService.Instance.SayAsync( ShipDefinitions.FromEDModel( "Vulture" ), SpeechConversions.ICAO( "GAB-1655" ) ).ConfigureAwait( false );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestPowerplay ()
        {
            var ship = ShipDefinitions.FromEDModel( "Anaconda" );
            var speaker = SpeechService.Instance;
            string[] powerNames =
            [
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
            ];
            foreach ( var powerName in powerNames )
            {
                await speaker.SayAsync( ship, SpeechConversions.getPhoneticPower( powerName ) + "." ).ConfigureAwait( false );
            }
        }

        [TestMethod, DoNotParallelize]
        [DataRow( 0 )]
        [DataRow( 20 )]
        [DataRow( 40 )]
        [DataRow( 60 )]
        [DataRow( 80 )]
        [DataRow( 100 )]
        public async Task TestDamageDistortionAsync (int shipHealth)
        {
            var speech = new EddiSpeech( $"Systems at {shipHealth}%.", null, 0, null, LandingPadSize.Large, shipHealth, false, true );
            await SpeechService.Instance.SpeakAsync( speech ).ConfigureAwait(false);
        }

        [TestMethod, DoNotParallelize]
        [DataRow( 0 )]
        [DataRow( 20 )]
        [DataRow( 40 )]
        [DataRow( 60 )]
        [DataRow( 80 )]
        [DataRow( 100 )]
        public async Task TestFxLevelAsync ( int fxLevel )
        {
            await SpeechService.Instance.SpeakAsync( $"Effects level {fxLevel}", null, fxLevel ).ConfigureAwait(false);
        }

        [TestMethod, DoNotParallelize]
        [DataRow( "Your python has touched down." )]
        [DataRow( "Anaconda golf foxtrot lima one niner six eight returning from orbit." )]
        public async Task TestRadioAsync (string msg)
        {
            await SpeechService.Instance.SpeakAsync(new EddiSpeech(msg, radio: true)).ConfigureAwait(false);
        }

        [TestMethod, DoNotParallelize]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 3 )]
        public async Task TestEchoDelayAsync (int landingPadSize)
        {
            var shipSize = LandingPadSize.AllOfThem.First( s => s.sizeIndex == landingPadSize );
            await SpeechService.Instance.SpeakAsync( new EddiSpeech( $"Echo delay for a {shipSize} ship.", shipSize: shipSize ) ).ConfigureAwait(false);
        }

        [TestMethod, DoNotParallelize]
        public async Task TestSpeechNullInvalidVoiceAsync ()
        {
            await SpeechService.Instance.SpeakAsync( new EddiSpeech( "Testing null voice", null ) ).ConfigureAwait(false);
            await SpeechService.Instance.SpeakAsync( new EddiSpeech( "Testing non-valid voice", "No such voice" ) ).ConfigureAwait(false);
        }
    }
}
