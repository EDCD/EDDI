using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiShipMonitor;
using EddiStarMapService;
using Newtonsoft.Json;
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
                else if (theEvent is CommanderContinuedEvent)
                {
                    CommanderContinuedEvent continuedEvent = (CommanderContinuedEvent)theEvent;
                    starMapService.sendCredits(continuedEvent.credits, continuedEvent.loan);
                }
                else if (theEvent is MaterialInventoryEvent)
                {
                    MaterialInventoryEvent materialInventoryEvent = (MaterialInventoryEvent)theEvent;
                    Dictionary<string, int> materials = new Dictionary<string, int>();
                    Dictionary<string, int> data = new Dictionary<string, int>();
                    foreach (MaterialAmount ma in materialInventoryEvent.inventory)
                    {
                        Material material = Material.FromEDName(ma.edname);
                        if (material.category == "Element" || material.category == "Manufactured")
                        {
                            materials.Add(material.EDName, ma.amount);
                        }
                        else
                        {
                            data.Add(material.EDName, ma.amount);
                        }
                    }
                    starMapService.sendMaterials(materials);
                    starMapService.sendData(data);
                }
                else if (theEvent is ShipLoadoutEvent)
                {
                    ShipLoadoutEvent shipLoadoutEvent = (ShipLoadoutEvent)theEvent;
                    Ship ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetShip(shipLoadoutEvent.shipid);
                    starMapService.sendShip(ship);
                }
                else if (theEvent is ShipSwappedEvent)
                {
                    ShipSwappedEvent shipSwappedEvent = (ShipSwappedEvent)theEvent;
                    if (shipSwappedEvent.shipid.HasValue)
                    {
                        starMapService.sendShipSwapped((int)shipSwappedEvent.shipid);
                    }
                }
                else if (theEvent is ShipSoldEvent)
                {
                    ShipSoldEvent shipSoldEvent = (ShipSoldEvent)theEvent;
                    if (shipSoldEvent.shipid.HasValue)
                    {
                        starMapService.sendShipSold((int)shipSoldEvent.shipid);
                    }
                }
                else if (theEvent is ShipDeliveredEvent)
                {
                    ShipDeliveredEvent shipDeliveredEvent = (ShipDeliveredEvent)theEvent;
                    if (shipDeliveredEvent.shipid.HasValue)
                    {
                        starMapService.sendShipSwapped((int)shipDeliveredEvent.shipid);
                    }
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
