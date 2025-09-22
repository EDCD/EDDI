using EddiCompanionAppService;
using EddiDataDefinitions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiSpeechService
{
    /// <summary>Provide speech services with a varying amount of alterations to the voice</summary>
    public class SpeechService
    {
        private AudioManager AudioManager => SpeechManager.AudioManager;
        public readonly SpeechManager SpeechManager = new SpeechManager();

        public List<string> allvoices => SpeechManager.allVoices
            .Where(v => !v.hideVoice)
            .Select(v => v.name)
            .ToList();

        public List<VoiceDetails> allVoices => SpeechManager.allVoices;

        public SpeechQueue speechQueue => SpeechManager.speechQueue;

        public bool eddiAudioPlaying => AudioManager.eddiAudioPlaying;
        public bool eddiSpeaking => SpeechManager.eddiSpeaking;
        
        public int activeSpeechPriority
        {
            get => SpeechManager.activeSpeechPriority;
            set => SpeechManager.activeSpeechPriority = value;
        }

        private static SpeechService instance;
        private static readonly object instanceLock = new object();
        public static SpeechService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No Speech service instance: creating one");
                            instance = new SpeechService();
                        }
                    }
                }
                return instance;
            }
        }

        public SpeechService()
        {
            // Monitor and respond appropriately to changes in the state of the CompanionAppService
            CompanionAppService.Instance.StateChanged += CompanionAppService_StateChanged;
        }

        private void CompanionAppService_StateChanged(CompanionAppService.State oldState, CompanionAppService.State newState)
        {
            if (newState == CompanionAppService.State.ConnectionLost && !CompanionAppService.unitTesting)
            {
                SpeechManager.Say(null, EddiCompanionAppService.Properties.CapiResources.frontier_api_lost, 0);
            }
        }

        public bool checkSpeechInterrupt ( int peekedSpeechPriority ) => SpeechManager.checkSpeechInterrupt( peekedSpeechPriority );

        public void Say ( Ship ship, string message, int priority = 3, string voice = null, bool radio = false,
            string eventType = null, bool invokedFromVA = false )
        {
            SpeechManager.Say( ship, message, priority, voice, radio, eventType, invokedFromVA );
        }

        public void ShutUp () => SpeechManager.ShutUp();

        public async Task SpeakAsync ( EddiSpeech speech )
        {
            await SpeechManager.SpeakAsync( speech );
        }

        public async Task SpeakAsync ( string speech, string defaultVoice, int fxLevel, int volume = 95,
            int distortionLevel = 0, int echoDelay = 0, int priority = 3, bool radio = false )
        {
            await SpeechManager.SpeakAsync( speech, defaultVoice, fxLevel, volume, distortionLevel, echoDelay, priority, radio );
        }

        public void StopAudio () => AudioManager.StopAudio();

        public void StopCurrentSpeech () => SpeechManager.StopCurrentSpeech();
    }
}
