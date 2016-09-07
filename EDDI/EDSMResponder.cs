using EliteDangerousEvents;
using EliteDangerousStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EDDI
{
    public class EDSMResponder : EDDIResponder
    {
        private StarMapService starMapService;

        public EDSMResponder()
        {
            Logging.Info("Started EDSM responder");
        }

        public void Start()
        {
            // Set up the star map service
            StarMapConfiguration starMapCredentials = StarMapConfiguration.FromFile();
            if (starMapCredentials != null && starMapCredentials.apiKey != null)
            {
                // Commander name might come from star map credentials or the companion app's profile
                string commanderName = null;
                if (starMapCredentials.commanderName != null)
                {
                    commanderName = starMapCredentials.commanderName;
                }
                else if (Eddi.Instance.Cmdr.name != null)
                {
                    commanderName = Eddi.Instance.Cmdr.name;
                }
                if (commanderName != null)
                {
                    starMapService = new StarMapService(starMapCredentials.apiKey, commanderName);
                    Logging.Info("EDDI access to EDSM is enabled");
                }
            }
            if (starMapService == null)
            {
                Logging.Info("EDDI access to EDSM is disabled");
            }
        }

        public void Stop()
        {
            starMapService = null;
        }

        public void Handle(Event theEvent)
        {
            if (theEvent is JumpedEvent)
            {
                JumpedEvent jumpedEvent = (JumpedEvent)theEvent;

                if (starMapService != null)
                {
                    // Send jump information to EDSM
                    Logging.Error("Sending data to EDSM");
                    starMapService.sendStarMapLog(jumpedEvent.timestamp, jumpedEvent.system, jumpedEvent.x, jumpedEvent.y, jumpedEvent.z);
                }
            }
        }
    }
}
