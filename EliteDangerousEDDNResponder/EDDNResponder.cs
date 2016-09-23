using EDDI;
using EliteDangerousDataDefinitions;
using EliteDangerousEvents;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EliteDangerousEDDNResponder
{
    /// <summary>
    /// A responder for EDDI to provide information to EDDN.
    /// </summary>
    public class EDDNResponder : EDDIResponder
    {
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
            return "Plugin to send station market, outfitting and station information to EDDN";
        }

        public EDDNResponder()
        {
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        public void Handle(Event theEvent)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(theEvent));
            if (theEvent is DockedEvent)
            {
                handleDockedEvent((DockedEvent)theEvent);
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

            // Need to add StarSystem to scan events
            if (theEvent is StarScannedEvent || theEvent is BodyScannedEvent)
            {
                data.Add("StarSystem", Eddi.Instance.CurrentStarSystem.name);
            }
            //data.Add("timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));

            EDDNBody body = new EDDNBody();
            body.header = generateHeader();
            body.schemaRef = "http://schemas.elite-markets.net/eddn/journal/1/test";
            body.message = data;

            sendMessage(body);

        }

        private void handleDockedEvent(DockedEvent theEvent)
        {
            // When we dock we have access to commodity and outfitting information
            // Shipyard information is not always available through the companion app API so can't send it
            //sendShipyardInformation();
            sendCommodityInformation();
            sendOutfittingInformation();
        }

        private void sendShipyardInformation()
        {
            List<string> eddnShips = new List<string>();
            foreach (Ship ship in Eddi.Instance.LastStation.shipyard)
            {
                eddnShips.Add(ship.EDName);
            }

            // Only send the message if we have ships
            if (eddnShips.Count > 0)
            {
                IDictionary<string, object> data = new Dictionary<string, object>();
                data.Add("timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
                data.Add("systemName", Eddi.Instance.LastStation.systemname);
                data.Add("stationName", Eddi.Instance.LastStation.name);
                data.Add("ships", eddnShips);

                EDDNBody body = new EDDNBody();
                body.header = generateHeader();
                body.schemaRef = "http://schemas.elite-markets.net/eddn/shipyard/2/test";
                body.message = data;

                sendMessage(body);
            }
        }

        private void sendCommodityInformation()
        {
            List<EDDNCommodity> eddnCommodities = new List<EDDNCommodity>();
            foreach (Commodity commodity in Eddi.Instance.LastStation.commodities)
            {
                if (commodity.Category == "NonMarketable")
                {
                    continue;
                }
                EDDNCommodity eddnCommodity = new EDDNCommodity();
                eddnCommodity.name = commodity.EDName;
                eddnCommodity.meanPrice = commodity.AveragePrice;
                eddnCommodity.buyPrice = commodity.BuyPrice;
                eddnCommodity.stock = commodity.Stock;
                eddnCommodity.stockBracket = commodity.StockBracket;
                eddnCommodity.sellPrice = commodity.SellPrice;
                eddnCommodity.demand = commodity.Demand;
                eddnCommodity.demandBracket = commodity.DemandBracket;
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
                data.Add("systemName", Eddi.Instance.LastStation.systemname);
                data.Add("stationName", Eddi.Instance.LastStation.name);
                data.Add("commodities", eddnCommodities);

                EDDNBody body = new EDDNBody();
                body.header = generateHeader();
                body.schemaRef = "http://schemas.elite-markets.net/eddn/commodity/3/test";
                body.message = data;

                sendMessage(body);
            }
        }

        private void sendOutfittingInformation()
        {
            List<string> eddnModules = new List<string>();
            foreach (Module module in Eddi.Instance.LastStation.outfitting)
            {
                eddnModules.Add(module.EDName);
            }

            // Only send the message if we have modules
            if (eddnModules.Count > 0)
            {
                IDictionary<string, object> data = new Dictionary<string, object>();
                data.Add("timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
                data.Add("systemName", Eddi.Instance.LastStation.systemname);
                data.Add("stationName", Eddi.Instance.LastStation.name);
                data.Add("modules", eddnModules);

                EDDNBody body = new EDDNBody();
                body.header = generateHeader();
                body.schemaRef = "http://schemas.elite-markets.net/eddn/outfitting/2/test";
                body.message = data;

                sendMessage(body);
            }
        }

        private static string generateUploaderId()
        {
            // Uploader ID is a hash of the commander's name
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            StringBuilder hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(Eddi.Instance.Cmdr.name), 0, Encoding.UTF8.GetByteCount(Eddi.Instance.Cmdr.name));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
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
            //Logging.Info(JsonConvert.SerializeObject(body));
            var client = new RestClient("http://eddn-gateway.elite-markets.net:8080/");
            var request = new RestRequest("upload/", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

            Logging.Debug("Sending " + JsonConvert.SerializeObject(body));

            new Thread(() =>
            {
                IRestResponse response = client.Execute(request);
                var content = response.Content; // raw content as string
                Logging.Debug("Response content is " + content);
            }).Start();
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }
    }
}