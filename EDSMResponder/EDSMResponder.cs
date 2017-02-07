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
        private string system;

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
            if (EDDI.Instance.inCQC)
            {
                // We don't do anything whilst in CQC
                return;
            }

            if (starMapService != null)
            {
                // We could receive either or both of JumpingEvent and JumpedEvent.  Send EDSM update as soon as possible,
                // but don't send it twice
                if (theEvent is JumpingEvent)
                {
                    JumpingEvent jumpingEvent = (JumpingEvent)theEvent;

                    if (jumpingEvent.system != system)
                    {
                        Logging.Debug("Sending jump data to EDSM (jumping)");
                        starMapService.sendStarMapLog(jumpingEvent.timestamp, jumpingEvent.system, jumpingEvent.x, jumpingEvent.y, jumpingEvent.z);
                        system = jumpingEvent.system;
                    }
                }
                else if (theEvent is JumpedEvent)
                {
                    JumpedEvent jumpedEvent = (JumpedEvent)theEvent;

                    if (jumpedEvent.system != system)
                    {
                        Logging.Debug("Sending jump data to EDSM (jumped)");
                        starMapService.sendStarMapLog(jumpedEvent.timestamp, jumpedEvent.system, jumpedEvent.x, jumpedEvent.y, jumpedEvent.z);
                        system = jumpedEvent.system;
                    }
                }
                else if (theEvent is CommanderContinuedEvent)
                {
                    CommanderContinuedEvent continuedEvent = (CommanderContinuedEvent)theEvent;
                    starMapService.sendCredits(continuedEvent.credits, continuedEvent.loan);
                }
                else if (theEvent is CommanderProgressEvent)
                {
                    CommanderProgressEvent progressEvent = (CommanderProgressEvent)theEvent;
                    if (EDDI.Instance.Cmdr != null && EDDI.Instance.Cmdr.federationrating != null)
                    {
                        starMapService.sendRanks(EDDI.Instance.Cmdr.combatrating.rank, (int)progressEvent.combat,
                            EDDI.Instance.Cmdr.traderating.rank, (int)progressEvent.trade,
                            EDDI.Instance.Cmdr.explorationrating.rank, (int)progressEvent.exploration,
                            EDDI.Instance.Cmdr.cqcrating.rank, (int)progressEvent.cqc,
                            EDDI.Instance.Cmdr.federationrating.rank, (int)progressEvent.federation,
                            EDDI.Instance.Cmdr.empirerating.rank, (int)progressEvent.empire);
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
