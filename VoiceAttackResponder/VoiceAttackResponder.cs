using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiIPC_Service.Server;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiVoiceAttackResponder
{
    /// <summary>
    /// A responder for EDDI to provide information to VoiceAttack.  This is very simple, just adding events to the VoiceAttack plugin's event queue
    /// </summary>
    class VoiceAttackResponder : IEddiResponder
    {
        private static VoiceAttackEventHandling voiceAttackEventHandler;
        private IDisposable _commandDispatcherRegistration;
        private IDisposable _responderModeRegistration;

        public VoiceAttackResponder()
        {
        }

        public string ResponderName()
        {
            return "VoiceAttack responder";
        }

        public string LocalizedResponderName()
        {
            return Properties.VoiceAttack.name;
        }

        public string ResponderDescription()
        {
            return Properties.VoiceAttack.desc;
        }

        public Task HandleAsync ( Event @event )
        {
            if ( EDDI.FromVA && !@event.fromLoad && !( @event is UnhandledEvent ) )
            {
                voiceAttackEventHandler.Handle( @event );
            }

            return Task.CompletedTask;
        }

        public bool Start()
        {
            if (EDDI.FromVA)
            {
                // Set up our event responder.
                voiceAttackEventHandler = new VoiceAttackEventHandling();
                _responderModeRegistration?.Dispose();
                _commandDispatcherRegistration?.Dispose();
                _responderModeRegistration = ResponderModeRegistry.RegisterHandler( VoiceAttackResponderModeHandler.SetResponderModeAsync );
                _commandDispatcherRegistration = CommandDispatcherRegistry.RegisterCommandDispatcher(new VoiceAttackCommandDispatcher());
                Logging.Info( "Started VoiceAttack responder" );

                // Initialize responder mode: subscribe to EDDI events and set up VoiceAttack variable synchronization
                VoiceAttackResponderMode.InitializeAsync().SafeFireAndForget( 
                    ex => Logging.Error( "Failed to initialize VoiceAttack responder mode", ex ) );

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Stop ()
        {
            _commandDispatcherRegistration?.Dispose();
            _commandDispatcherRegistration = null;
            _responderModeRegistration?.Dispose();
            _responderModeRegistration = null;

            // Cancel event queue threads and wait for them to complete
            voiceAttackEventHandler?.StopEventHandlingAsync().SafeFireAndForget( ex => Logging.Warn( ex.Message, ex ) );
        }

        public void Reload()
        { }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public Task HandleStatusAsync ( Status status )
        {
            if ( EDDI.FromVA )
            {
                VoiceAttackVariables.setStatusValues( status, "Status" );
            }
            return Task.CompletedTask;
        }
    }
}
