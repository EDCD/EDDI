using Eddi;
using EddiEvents;
using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiEdsmResponder
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
            return "Send details of your travels to EDSM.  EDSM is a third-party tool that provides information on the locations of star systems and keeps a log of the star systems you have visited.  It uses the data provided to crowd-source a map of the galaxy";
        }

        public EDSMResponder()
        {
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        public bool Start()
        {
            Reload();
            return starMapService != null;
        }

        public void Stop()
        {
            starMapService = null;
        }

        public void Reload()
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
                else if (EDDI.Instance.Cmdr != null)
                {
                    commanderName = EDDI.Instance.Cmdr.name;
                }
                if (commanderName != null)
                {
                    starMapService = new StarMapService(starMapCredentials.apiKey, commanderName);
                }
            }
        }

        public void Handle(Event theEvent)
        {
            if (starMapService != null)
            {
                if (theEvent is JumpedEvent)
                {
                    JumpedEvent jumpedEvent = (JumpedEvent)theEvent;

                    Logging.Debug("Sending jump data to EDSM");
                    starMapService.sendStarMapLog(jumpedEvent.timestamp, jumpedEvent.system, jumpedEvent.x, jumpedEvent.y, jumpedEvent.z);
                }
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
