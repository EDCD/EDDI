using Eddi;
using Utilities;
using EddiEvents;
using Newtonsoft.Json;
using System.Windows.Controls;

namespace EddiVoiceAttackResponder
{
    /// <summary>
    /// A responder for EDDI to provide information to VoiceAttack.  This is very simple, just adding events to the VoiceAttack plugin's event queue
    /// </summary>
    class VoiceAttackResponder : EDDIResponder
    {
        public string ResponderName()
        {
            return "VoiceAttack responder";
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return "A responder that generates a large number of variables within VoiceAttack as well as triggering user-defined actions.";
        }

        public VoiceAttackResponder()
        {
            Logging.Info("Started VoiceAttack responder");
        }

        public void Handle(Event theEvent)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(theEvent));
            VoiceAttackPlugin.EventQueue.Add(theEvent);
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
        }

        public void Reload()
        {
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
