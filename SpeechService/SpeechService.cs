using EddiCompanionAppService;
using EddiDataDefinitions;
using System;
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
        private readonly AudioManager AudioManager;
        public readonly SpeechManager SpeechManager;

        public List<VoiceDetails> validatedVoices => SpeechManager.validatedVoices;

        public List<string> displayedVoiceNames => validatedVoices
            .Where(v => !v.hideVoice)
            .Select(v => v.name)
            .ToList();

        public SpeechQueue speechQueue => SpeechManager.speechQueue;

        public bool eddiAudioPlaying => AudioManager.eddiAudioPlaying;
        public bool eddiSpeaking => SpeechManager.eddiSpeaking;
        
        public int activeSpeechPriority
        {
            get => SpeechManager.activeSpeechPriority;
            set => SpeechManager.activeSpeechPriority = value;
        }

        private static SpeechService instance;
        private static readonly object instanceLock = new();
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
            CompanionAppService.Instance.StateChanged += ( _, newState ) =>
                CompanionAppService_StateChangedAsync( newState ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );

            AudioManager = new AudioManager();
            SpeechManager = new SpeechManager( AudioManager );
        }

        private async Task CompanionAppService_StateChangedAsync( CompanionAppService.State newState)
        {
            if ( newState == CompanionAppService.State.LoggedOut && 
                 !CompanionAppService.Instance.unitTesting )
            {
                await SpeechManager
                    .EnqueueAsync( null, EddiCompanionAppService.Properties.CapiResources.frontier_api_lost, 0 )
                    .ConfigureAwait( false );
            }
        }

        public bool checkSpeechInterrupt ( int peekedSpeechPriority ) => SpeechManager.checkSpeechInterrupt( peekedSpeechPriority );

        public async Task SayAsync ( Ship ship, string message, int priority = 3, string voice = null, bool radio = false,
            string eventType = null )
        {
            await SpeechManager.EnqueueAsync( ship, message, priority, voice, radio, eventType )
                .ConfigureAwait( false );
        }

        public void ShutUp () => SpeechManager.ShutUp();

        public async Task SpeakAsync ( EddiSpeech speech )
        {
            await SpeechManager.SpeakAsync( speech ).ConfigureAwait(false);
        }

        public async Task SpeakAsync ( string speech, string defaultVoice, int fxLevel,
            int distortionLevel = 0, int echoDelay = 0, int priority = 3, bool radio = false )
        {
            await SpeechManager.SpeakAsync( speech, defaultVoice, fxLevel, distortionLevel, echoDelay, priority, radio ).ConfigureAwait(false);
        }

        public void StopAudio () => AudioManager.StopAudio();

        public void StopCurrentSpeech () => SpeechManager.StopCurrentSpeech();
    }
}
