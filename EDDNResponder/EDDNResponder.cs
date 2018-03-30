using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Utilities;

namespace EDDNResponder
{
    /// <summary>
    /// A responder for EDDI to provide information to EDDN.
    /// </summary>
    public class EDDNResponder : EDDIResponder
    {
        // We keep track of the starsystem information locally
        public string systemName { get; private set; } = null;
        public decimal? systemX { get; private set; } = null;
        public decimal? systemY { get; private set; } = null;
        public decimal? systemZ { get; private set; } = null;

        private StarSystemRepository starSystemRepository;

        public string ResponderName()
        {
            return "EDDN responder";
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return "Send station, jump, and scan information to EDDN.  EDDN is a third-party tool that gathers information on systems and markets, and provides data for most trading tools as well as starsystem information tools such as EDDB";
        }

        public EDDNResponder() : this(EddiDataProviderService.StarSystemSqLiteRepository.Instance)
        {}

        public EDDNResponder(StarSystemRepository starSystemRepository)
        {
            this.starSystemRepository = starSystemRepository;
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
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

            Logging.Debug("Received event " + JsonConvert.SerializeObject(theEvent));

            if (theEvent is LocationEvent)
            {
                handleLocationEvent((LocationEvent)theEvent);
            }
            if (theEvent is JumpedEvent)
            {
                handleJumpedEvent((JumpedEvent)theEvent);
            }

            if (theEvent is DockedEvent)
            {
                handleDockedEvent((DockedEvent)theEvent);
            }

            if (theEvent is MarketInformationUpdatedEvent)
            {
                handleMarketInformationUpdatedEvent((MarketInformationUpdatedEvent)theEvent);
            }

            if (theEvent is JumpedEvent || theEvent is DockedEvent || theEvent is BodyScannedEvent || theEvent is StarScannedEvent)
            {
                handleRawEvent(theEvent);
            }
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

        private void handleLocationEvent(LocationEvent @event)
        {
            systemName = @event.system;
            systemX = @event.x;
            systemY = @event.y;
            systemZ = @event.z;
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            systemName = @event.system;
            systemX = @event.x;
            systemY = @event.y;
            systemZ = @event.z;
        }

        private void handleRawEvent(Event theEvent)
        {
            IDictionary<string, object> data = Deserializtion.DeserializeData(theEvent.raw);
            // Need to strip a number of entries
            data.Remove("CockpitBreach");
            data.Remove("BoostUsed");
            data.Remove("FuelLevel");
            data.Remove("FuelUsed");
            data.Remove("JumpDist");

            // Need to remove any keys ending with _Localised
            data = data.Where(x => !x.Key.EndsWith("_Localised")).ToDictionary(x => x.Key, x => x.Value);

            // Can only proceed if we know our current system

            // Need to add StarSystem to scan events - can only do so if we have the data
            if (theEvent is BeltScannedEvent || theEvent is StarScannedEvent || theEvent is BodyScannedEvent)
            {
                if (systemName == null || systemX == null || systemY == null || systemZ == null)
                {
                    Logging.Debug("Missing current starsystem information, cannot send message to EDDN");
                    return;
                }
                data.Add("StarSystem", systemName);
            }

            // Need to add StarPos to all events that don't already have them
            if (!data.ContainsKey("StarPos"))
            {
                if (systemName == null || systemX == null || systemY == null || systemZ == null)
                {
                    Logging.Debug("Missing current starsystem information, cannot send message to EDDN");
                    return;
                }
                IList<decimal> starpos = new List<decimal>();
                starpos.Add(systemX.Value);
                starpos.Add(systemY.Value);
                starpos.Add(systemZ.Value);
                data.Add("StarPos", starpos);
            }

            EDDNBody body = new EDDNBody();
            body.header = generateHeader();
            body.schemaRef = "https://eddn.edcd.io/schemas/journal/1" + (EDDI.Instance.inBeta ? "/test" : "");
            body.message = data;

            sendMessage(body);

        }

        private void handleDockedEvent(DockedEvent theEvent)
        {
            if (eventSystemNameMatches(theEvent.system))
            {
                // When we dock we have access to commodity and outfitting information
                sendCommodityInformation();
                sendOutfittingInformation();
                sendShipyardInformation();
            }
        }

        private void handleMarketInformationUpdatedEvent(MarketInformationUpdatedEvent theEvent)
        {
            // When we dock we have access to commodity and outfitting information
            sendCommodityInformation();
            sendOutfittingInformation();
            sendShipyardInformation();
        }

        private void sendCommodityInformation()
        {
            // It's possible that the commodity data, if it is here, has already come from EDDB.  We use the average price
            // as a marker: this isn't visible in EDDB, so if we have average price we know that this is data from the companion
            // API and should be reported
            if (EDDI.Instance.CurrentStation != null && EDDI.Instance.CurrentStation.commodities != null && EDDI.Instance.CurrentStation.commodities.Count > 0 && EDDI.Instance.CurrentStation.commodities[0].avgprice != null)
            {
                List<EDDNEconomy> eddnEconomies = new List<EDDNEconomy>();
                if (EDDI.Instance.CurrentStation.economies != null)
                {
                    foreach (CompanionAppEconomy economy in EDDI.Instance.CurrentStation.economies)
                    {
                        EDDNEconomy eddnEconomy = new EDDNEconomy();
                        eddnEconomy.name = economy.name;
                        eddnEconomy.proportion = economy.proportion;
                        eddnEconomies.Add(eddnEconomy);
                    }
                }

                List<EDDNCommodity> eddnCommodities = new List<EDDNCommodity>();
                foreach (Commodity commodity in EDDI.Instance.CurrentStation.commodities)
                {
                    if (commodity.category == "NonMarketable")
                    {
                        continue;
                    }
                    EDDNCommodity eddnCommodity = new EDDNCommodity();
                    eddnCommodity.name = commodity.EDName;
                    eddnCommodity.meanPrice = (int)commodity.avgprice;
                    eddnCommodity.buyPrice = (int)commodity.buyprice;
                    eddnCommodity.stock = (int)commodity.stock;
                    eddnCommodity.stockBracket = commodity.stockbracket;
                    eddnCommodity.sellPrice = (int)commodity.sellprice;
                    eddnCommodity.demand = (int)commodity.demand;
                    eddnCommodity.demandBracket = commodity.demandbracket;
                    if (commodity.StatusFlags.Count > 0)
                    {
                        eddnCommodity.statusFlags = commodity.StatusFlags;
                    }
                    eddnCommodities.Add(eddnCommodity);
                };

                // Only send the message if we have commodities
                if (eddnCommodities.Count > 0)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    data.Add("systemName", systemName);
                    data.Add("stationName", EDDI.Instance.CurrentStation.name);
                    if (eddnEconomies.Count > 0)
                    {
                        data.Add("economies", eddnEconomies);
                    }
                    data.Add("commodities", eddnCommodities);
                    if (EDDI.Instance.CurrentStation.prohibited.Count > 0)
                    {
                        data.Add("prohibited", EDDI.Instance.CurrentStation.prohibited);
                    }

                    EDDNBody body = new EDDNBody();
                    body.header = generateHeader();
                    body.schemaRef = "https://eddn.edcd.io/schemas/commodity/3" + (EDDI.Instance.inBeta ? "/test" : "");
                    body.message = data;

                    Logging.Debug("EDDN message is: " + JsonConvert.SerializeObject(body));
                    sendMessage(body);
                }
            }
        }

        private void sendOutfittingInformation()
        {
            if (EDDI.Instance.CurrentStation != null && EDDI.Instance.CurrentStation.outfitting != null)
            {
                List<string> eddnModules = new List<string>();
                foreach (Module module in EDDI.Instance.CurrentStation.outfitting)
                {
                    if ((!ModuleDefinitions.IsPP(module))
                        && (module.EDName.StartsWith("Int_") || module.EDName.StartsWith("Hpt_") || module.EDName.Contains("_Armour_"))
                        && (!(module.EDName == "Int_PlanetApproachSuite")))
                    {
                        eddnModules.Add(module.EDName);
                    }
                }

                // Only send the message if we have modules
                if (eddnModules.Count > 0)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    data.Add("systemName", EDDI.Instance.CurrentStation.systemname);
                    data.Add("stationName", EDDI.Instance.CurrentStation.name);
                    data.Add("modules", eddnModules);

                    EDDNBody body = new EDDNBody();
                    body.header = generateHeader();
                    body.schemaRef = "https://eddn.edcd.io/schemas/outfitting/2" + (EDDI.Instance.inBeta ? "/test" : "");
                    body.message = data;

                    sendMessage(body);
                }
            }
        }

        private void sendShipyardInformation()
        {
            if (EDDI.Instance.CurrentStation != null && EDDI.Instance.CurrentStation.shipyard != null)
            {
                List<string> eddnShips = new List<string>();
                foreach (Ship ship in EDDI.Instance.CurrentStation.shipyard)
                {
                        eddnShips.Add(ship.EDName);
                }

                // Only send the message if we have ships
                if (eddnShips.Count > 0)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    data.Add("systemName", EDDI.Instance.CurrentStation.systemname);
                    data.Add("stationName", EDDI.Instance.CurrentStation.name);
                    data.Add("ships", eddnShips);

                    EDDNBody body = new EDDNBody();
                    body.header = generateHeader();
                    body.schemaRef = "https://eddn.edcd.io/schemas/shipyard/2" + (EDDI.Instance.inBeta ? "/test" : "");
                    body.message = data;

                    sendMessage(body);
                }
            }
        }


        private static string generateUploaderId()
        {
            // Uploader ID is a hash of the commander's name
            //System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            //StringBuilder hash = new StringBuilder();
            //string uploader = (EDDI.Instance.Cmdr == null ? "commander" : EDDI.Instance.Cmdr.name);
            //byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(uploader), 0, Encoding.UTF8.GetByteCount(uploader));
            //foreach (byte theByte in crypto)
            //{
            //    hash.Append(theByte.ToString("x2"));
            //}
            //return hash.ToString();
            return EDDI.Instance.Cmdr == null || EDDI.Instance.Cmdr.name == null ? "Unknown commander" : EDDI.Instance.Cmdr.name;
        }

        private static EDDNHeader generateHeader()
        {
            EDDNHeader header = new EDDNHeader();
            header.softwareName = Constants.EDDI_NAME;
            header.softwareVersion = Constants.EDDI_VERSION;
            header.uploaderID = generateUploaderId();
            return header;
        }

        private static void sendMessage(EDDNBody body)
        {
            var client = new RestClient("https://eddn.edcd.io:4430/");
            var request = new RestRequest("upload/", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

            Logging.Debug("Sending " + JsonConvert.SerializeObject(body));

            Thread thread = new Thread(() =>
            {
                try
                {
                    IRestResponse response = client.Execute(request);
                    var content = response.Content; // raw content as string
                    Logging.Debug("Response content is " + content);
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to send error to EDDN", ex);
                }
            });
            thread.Name = "EDDN message";
            thread.IsBackground = true;
            thread.Start();
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        public bool eventSystemNameMatches(string eventSystem)
        {
            // Check to make sure the eventSystem given matches the systemName we expected to see.
            if (systemName == eventSystem)
            {
                return true;
            }

            StarSystem system = starSystemRepository.GetStarSystem(eventSystem);
            if (system != null)
            {
                // Provide a fallback data source for system coordinate metadata if the eventSystem does not match the systemName we expected
                systemName = system.name;
                systemX = system.x;
                systemY = system.y;
                systemZ = system.z;
                return true;
            }
            else
            {
                // Set values to null if data isn't available. If any data is null, data shall not be sent to EDDN.
                systemName = eventSystem;
                systemX = null;
                systemY = null;
                systemZ = null;
                return false;
            }
        }
    }
}
