using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return "Un Plugin qui génère un grand nombre de variables dans VoiceAttack et pouvant déclencher des actions définies par l'utilisateur.";
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
