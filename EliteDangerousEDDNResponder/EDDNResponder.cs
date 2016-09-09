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
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
        }

        private void handleDockedEvent(DockedEvent theEvent)
        {
            // When we dock we have access to commodity, outfitting and shipyard information
            sendShipyardInformation();
            sendCommodityInformation();
            sendOutfittingInformation();
        }
        private void sendShipyardInformation()
        {
            //var client = new RestClient("http://schemas.elite-markets.net/eddn/");
            //var request = new RestRequest("commodity/3/test", Method.POST);
            //request.AddParameter("application/json", shipyardData, ParameterType.RequestBody);

            //new Thread(() =>
            //{
            //    IRestResponse response = client.Execute(request);
            //    var content = response.Content; // raw content as string
            //    Logging.Info("Response content is " + content);
            //}).Start();
        }

        private void sendCommodityInformation()
        {
            EDDNHeader header = new EDDNHeader();
            header.softwareName = Eddi.EDDI_NAME;
            header.softwareVersion = Eddi.EDDI_VERSION;
            header.uploaderID = generateUploaderId();

            EDDNCommoditiesMessage message = new EDDNCommoditiesMessage();
            message.timestamp = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            message.systemName = Eddi.Instance.LastStation.systemname;
            message.stationName = Eddi.Instance.LastStation.name;
            List<EDDNCommodity> eddnCommodities = new List<EDDNCommodity>();
            foreach (Commodity commodity in Eddi.Instance.LastStation.commodities)
            {
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
            message.commodities = eddnCommodities;

            EDDNBody body = new EDDNBody();
            body.header = header;
            body.schemaRef = "http://schemas.elite-markets.net/eddn/commodity/3/test";
            body.message = message;

            //var client = new RestClient("http://schemas.elite-markets.net/eddn/");
            //var request = new RestRequest("commodity/3/test", Method.POST);
            var client = new RestClient("http://eddn-gateway.elite-markets.net:8080/");
            var request = new RestRequest("upload/", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

            new Thread(() =>
            {
                IRestResponse response = client.Execute(request);
                var content = response.Content; // raw content as string
                Logging.Info("Response content is " + content);
            }).Start();
        }

        private void sendOutfittingInformation()
        {
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
    }

}