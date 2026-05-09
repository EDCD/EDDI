using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiIPC_Service.Server;
using JetBrains.Annotations;
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
    [UsedImplicitly]
    public class VoiceAttackResponder : IEddiResponder
    {
        private static readonly VoiceAttackEventHandling voiceAttackEventHandler = new();
        [UsedImplicitly] private readonly IDisposable _commandDispatcherRegistration = CommandDispatcherRegistry.RegisterCommandDispatcher( new VoiceAttackCommandDispatcher() );
        [UsedImplicitly] private readonly IDisposable _responderModeRegistration = ResponderModeRegistry.RegisterHandler( VoiceAttackResponderModeHandler.SetResponderModeAsync );

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
            if ( EDDI.Instance.FromVA && !@event.fromLoad && @event is not UnhandledEvent )
            {
                voiceAttackEventHandler.Handle( @event );
            }

            return Task.CompletedTask;
        }

        public bool Start()
        {
            if (EDDI.Instance.FromVA )
            {
                // Initialize responder mode: subscribe to EDDI events and set up VoiceAttack variable synchronization
                VoiceAttackResponderMode.InitializeAsync().SafeFireAndForget( 
                    ex => Logging.Error( "Failed to initialize VoiceAttack responder mode", ex ) );
            }

            return true;
        }

        public void Stop ()
        {
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
            if ( EDDI.Instance.FromVA )
            {
                VoiceAttackVariables.setStatusValues( status, "Status" );
            }
            return Task.CompletedTask;
        }
    }
}
