using Eddi;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using System.Runtime.CompilerServices;
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

        public void Handle(Event @event)
        {
            if ( !App.FromVA || @event.fromLoad || @event is UnhandledEvent )
            {
                return;
            }
            voiceAttackEventHandler.Handle( @event );
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
            voiceAttackEventHandler?.StopEventHandling();
        }

        public void Reload()
        { }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void HandleStatus ( Status status )
        {
            if ( App.FromVA )
            {
                VoiceAttackVariables.setStatusValues( status, "Status" );
            }
        }
    }
}
