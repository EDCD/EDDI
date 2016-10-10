using Eddi;
using EddiDataDefinitions;
using EddiEvents;
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

namespace EDDNResponder
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
            return "Send station, jump, and scan information to EDDN.  EDDN is a third-party tool that gathers information on systems and markets, and provides data for most trading tools as well as starsystem information tools such as EDDB";
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
                data.Add("StarSystem", EDDI.Instance.CurrentStarSystem.name);
            }

            EDDNBody body = new EDDNBody();
            body.header = generateHeader();
            body.schemaRef = "http://schemas.elite-markets.net/eddn/journal/1/test";
            body.message = data;

            sendMessage(body);

        }

        private void handleDockedEvent(DockedEvent theEvent)
        {
            // When we dock we have access to commodity and outfitting information
            sendCommodityInformation();
            sendOutfittingInformation();
        }

        private void sendCommodityInformation()
        {
            if (EDDI.Instance.LastStation != null && EDDI.Instance.LastStation.commodities != null)
            {
                List<EDDNCommodity> eddnCommodities = new List<EDDNCommodity>();
                foreach (Commodity commodity in EDDI.Instance.LastStation.commodities)
                {
                    if (commodity.category == "NonMarketable")
                    {
                        continue;
                    }
                    EDDNCommodity eddnCommodity = new EDDNCommodity();
                    eddnCommodity.name = commodity.EDName;
                    eddnCommodity.meanPrice = commodity.avgprice;
                    eddnCommodity.buyPrice = commodity.buyprice;
                    eddnCommodity.stock = commodity.stock;
                    eddnCommodity.stockBracket = commodity.stockbracket;
                    eddnCommodity.sellPrice = commodity.sellprice;
                    eddnCommodity.demand = commodity.demand;
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
                    data.Add("systemName", EDDI.Instance.LastStation.systemname);
                    data.Add("stationName", EDDI.Instance.LastStation.name);
                    data.Add("commodities", eddnCommodities);

                    EDDNBody body = new EDDNBody();
                    body.header = generateHeader();
                    body.schemaRef = "http://schemas.elite-markets.net/eddn/commodity/3/test";
                    body.message = data;

                    sendMessage(body);
                }
            }
        }

        private void sendOutfittingInformation()
        {
            if (EDDI.Instance.LastStation != null && EDDI.Instance.LastStation.outfitting != null)
            {
                List<string> eddnModules = new List<string>();
                foreach (Module module in EDDI.Instance.LastStation.outfitting)
                {
                    if ((!ModuleDefinitions.IsPP(module)) && (module.EDName.StartsWith("Int_") || module.EDName.StartsWith("Hpt_") || module.EDName.Contains("_Armour_")))
                    {
                        eddnModules.Add(module.EDName);
                    }
                }

                // Only send the message if we have modules
                if (eddnModules.Count > 0)
                {
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    data.Add("systemName", EDDI.Instance.LastStation.systemname);
                    data.Add("stationName", EDDI.Instance.LastStation.name);
                    data.Add("modules", eddnModules);

                    EDDNBody body = new EDDNBody();
                    body.header = generateHeader();
                    body.schemaRef = "http://schemas.elite-markets.net/eddn/outfitting/2/test";
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
            return EDDI.Instance.Cmdr == null ? "Unknown commander" : EDDI.Instance.Cmdr.name;
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
            Logging.Debug(JsonConvert.SerializeObject(body));
            var client = new RestClient("http://eddn-gateway.elite-markets.net:8080/");
            var request = new RestRequest("upload/", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

            Logging.Debug("Sending " + JsonConvert.SerializeObject(body));

            Thread thread = new Thread(() =>
            {
                IRestResponse response = client.Execute(request);
                var content = response.Content; // raw content as string
                Logging.Debug("Response content is " + content);
            });
            thread.Name = "EDDN message";
            thread.IsBackground = true;
            thread.Start();
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }
    }
}