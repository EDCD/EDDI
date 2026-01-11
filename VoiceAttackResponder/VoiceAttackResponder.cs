using Eddi;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
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
        private static VoiceAttackEventHandler voiceAttackEventHandler;

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
            if ( App.FromVA && !@event.fromLoad && !( @event is UnhandledEvent ) )
            {
                voiceAttackEventHandler.Handle( @event );
            }

            return Task.CompletedTask;
        }

        public bool Start()
        {
            if (App.FromVA)
            {
                // Set up our event responder.
                voiceAttackEventHandler = new VoiceAttackEventHandler();
                Logging.Info( "Started VoiceAttack responder" );
                return true;
            }
            else
            {
                return false;
            }
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
            if ( App.FromVA )
            {
                VoiceAttackVariables.setStatusValues( status, "Status" );
            }
            return Task.CompletedTask;
        }
    }
}
