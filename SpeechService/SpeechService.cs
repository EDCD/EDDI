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
                SpeechManager.SayAsync(null, EddiCompanionAppService.Properties.CapiResources.frontier_api_lost, 0).GetAwaiter().GetResult();
            }
        }

        public bool checkSpeechInterrupt ( int peekedSpeechPriority ) => SpeechManager.checkSpeechInterrupt( peekedSpeechPriority );

        public Task SayAsync ( Ship ship, string message, int priority = 3, string voice = null, bool radio = false,
            string eventType = null, bool invokedFromVA = false )
        {
            return SpeechManager.SayAsync( ship, message, priority, voice, radio, eventType, invokedFromVA );
        }

        public void ShutUp () => SpeechManager.ShutUp();

        public Task SpeakAsync ( EddiSpeech speech )
        {
            return SpeechManager.SpeakAsync( speech );
        }

        public Task SpeakAsync ( string speech, string defaultVoice, int fxLevel,
            int distortionLevel = 0, int echoDelay = 0, int priority = 3, bool radio = false )
        {
            return SpeechManager.SpeakAsync( speech, defaultVoice, fxLevel, distortionLevel, echoDelay, priority, radio );
        }

        public void StopAudio () => AudioManager.StopAudio();

        public void StopCurrentSpeech () => SpeechManager.StopCurrentSpeech();
    }
}
