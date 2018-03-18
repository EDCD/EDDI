using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiShipMonitor;
using EddiStarMapService;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Utilities;

namespace EddiEdsmResponder
{
    public class EDSMResponder : EDDIResponder
    {
        private StarMapService starMapService;
        private string system;
        private Thread updateThread;

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
            updateThread?.Abort();
            updateThread = null;
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

            if (starMapService != null && updateThread == null)
            {
                // Spin off a thread to download & sync EDSM flight logs & system comments in the background
                updateThread = new Thread(() => starMapService.Sync(starMapCredentials.lastSync));
                updateThread.IsBackground = true;
                updateThread.Name = "EDSM updater";
                updateThread.Start();
            }
        }

        public void Handle(Event theEvent)
        {
            if (EDDI.Instance.inCQC)
            {
                // We don't do anything whilst in CQC
                return;
            }

            if (EDDI.Instance.inCrew)
            {
                // We don't do anything whilst in multicrew
                return;
            }

            if (EDDI.Instance.inBeta)
            {
                // We don't send data whilst in beta
                return;
            }

            if (starMapService != null)
            {
                if (theEvent is JumpedEvent)
                {
                    JumpedEvent jumpedEvent = (JumpedEvent)theEvent;

                    if (jumpedEvent.system != system)
                    {
                        Logging.Debug("Sending jump data to EDSM (jumped)");
                        starMapService.sendStarMapLog(jumpedEvent.timestamp, jumpedEvent.system, jumpedEvent.x, jumpedEvent.y, jumpedEvent.z);
                        system = jumpedEvent.system;
                    }
                }
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
