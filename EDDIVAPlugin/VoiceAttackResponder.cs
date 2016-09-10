using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDDI;
using Utilities;
using EliteDangerousEvents;
using Newtonsoft.Json;
using System.Windows.Controls;

namespace EDDIVAPlugin
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
            return "Plugin to make events available to VoiceAttack";
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

        public UserControl ConfigurationTabItem()
        {
            return null;
        }
    }
}
