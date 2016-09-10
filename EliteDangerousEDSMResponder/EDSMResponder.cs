using EDDI;
using EliteDangerousEvents;
using EliteDangerousStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EliteDangerousEDSMResponder
{
    public class EDSMResponder : EDDIResponder
    {
        private StarMapService starMapService;

        public string ResponderName()
        {
            return "EDSM responder";
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return "Plugin to respond to system jumps by sending the details to EDSM";
        }

        public EDSMResponder()
        {
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        public bool Start()
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
                }
            }
            return starMapService != null;
        }

        public void Stop()
        {
            starMapService = null;
        }

        public void Handle(Event theEvent)
        {
            if (starMapService != null)
            {
                if (theEvent is JumpedEvent)
                {
                    JumpedEvent jumpedEvent = (JumpedEvent)theEvent;

                    // Send jump information to EDSM
                    Logging.Error("Sending data to EDSM");
                    starMapService.sendStarMapLog(jumpedEvent.timestamp, jumpedEvent.system, jumpedEvent.x, jumpedEvent.y, jumpedEvent.z);
                }
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }
    }
}
