using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDDI;
using Utilities;
using EliteDangerousEvents;

namespace EDDIVAPlugin
{
    /// <summary>
    /// A responder for EDDI to provide information to VoiceAttack
    /// </summary>
    class VoiceAttackResponder : EDDIResponder
    {
        public VoiceAttackResponder()
        {
            Logging.Info("Started VoiceAttack responder");
        }

        public void Handle(Event theEvent)
        {
            VoiceAttackPlugin.EventQueue.Add(theEvent);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
