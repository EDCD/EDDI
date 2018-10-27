using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using Utilities;

namespace EddiDataProviderService
{
    /// <summary>Access to EDDP legacy server data<summary>

    // Data not currently available from EDSM and that we may wish to access from here:
    // - System: Powerplay data
    // + Station: Commodities price listings
    // - Station: Import commodities
    // - Station: Export commodities
    // - Station: Prohibited commodities 
    // - Station: Detailed shipyard data 
    // - Station: Detailed module data

    public class LegacyEddpService
    {
        private const string BASE = "http://api.eddp.co/";

        private static JObject GetData(string system)
        {
            try
            {
                string response = Net.DownloadString(BASE + "systems/" + Uri.EscapeDataString(system));
                return JsonConvert.DeserializeObject<JObject>(response);
            }
            catch (WebException)
            {
                Logging.Debug("Failed to obtain data from " + BASE + "systems/" + Uri.EscapeDataString(system));
                return null;
            }
        }

        public static void GetCommoditiesData(string station, string system, out List<CommodityMarketQuote> commodities, out long? commoditiesupdatedat)
        {
            commodities = null;
            commoditiesupdatedat = null;

            JObject response = GetData(system);

            if (response["stations"] is JArray)
            {
                foreach (JObject Station in response["stations"])
                {
                    if ((string)Station["name"] == station)
                    {
                        commodities = CommodityQuotesFromEDDP(Station);
                        commoditiesupdatedat = (long?)Station["market_updated_at"];
                    }
                }
            }
        }

        private static List<CommodityMarketQuote> CommodityQuotesFromEDDP(JObject json)
        {
            var quotes = new List<CommodityMarketQuote>();
            if (json["commodities"] != null)
            {
                foreach (JObject commodity in json["commodities"])
                {
                    CommodityDefinition commodityDefinition = CommodityDefinition.FromName((string)commodity["name"]);
                    CommodityMarketQuote quote = new CommodityMarketQuote(commodityDefinition);
                    // Annoyingly, these double-casts seem to be necessary because the boxed type is `long`. A direct cast to `int?` always returns null.
                    quote.buyprice = (int?)(long?)commodity["buy_price"] ?? quote.buyprice;
                    quote.sellprice = (int?)(long?)commodity["sell_price"] ?? quote.sellprice;
                    quote.demand = (int?)(long?)commodity["demand"] ?? quote.demand;
                    quote.stock = (int?)(long?)commodity["supply"] ?? quote.stock;
                    quotes.Add(quote);
                }
            }
            return quotes;
        }

        public static Dictionary<string, object> PowerplayFromEDDP(JObject json)
        {
            Dictionary<string, object> powerplay = new Dictionary<string, object>()
            {
                { "power", (string)json["power"] == "None" ? null : (string)json["power"] },
                { "power_state", (string)json["power_state"] } // Needs translatable powerplay states
            };
            return powerplay;
        }

        static LegacyEddpService()
        {
            // We need to not use an expect header as it causes problems when sending data to a REST service
            var errorUri = new Uri(BASE + "error");
            var errorServicePoint = ServicePointManager.FindServicePoint(errorUri);
            errorServicePoint.Expect100Continue = false;
        }
    }
}