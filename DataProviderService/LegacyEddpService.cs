using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using Utilities;

namespace EddiDataProviderService
{
    /// <summary>Access to EDDP data<summary>
    public class LegacyEddpService
    {
        //TODO: Change this to use an EDCD server or other established data service (EDSM?)
        private const string BASE = "http://api.eddp.co/";

        public static void GetCommoditiesData(string station, string system, out List<CommodityMarketQuote> commodities, out long? commoditiesupdatedat)
        {
            string response = string.Empty;
            commodities = null;
            commoditiesupdatedat = null;

            try
            {
                response = Net.DownloadString(BASE + "systems/" + Uri.EscapeDataString(system));
            }
            catch (WebException)
            {
                Logging.Debug("Failed to obtain commodity data from " + BASE + "systems/" + Uri.EscapeDataString(system));
            }

            if (response != null)
            {
                JObject json = JObject.Parse(response);

                if (json["stations"] != null)
                {
                    foreach (JObject Station in json["stations"])
                    {
                        if ((string)Station["name"] == station)
                        {
                            commodities = CommodityQuotesFromEDDP(Station);
                            commoditiesupdatedat = (long?)Station["market_updated_at"];
                        }
                    }
                }
            }
        }

        public static List<CommodityMarketQuote> CommodityQuotesFromEDDP(JObject json)
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

        static LegacyEddpService()
        {
            // We need to not use an expect header as it causes problems when sending data to a REST service
            var errorUri = new Uri(BASE + "error");
            var errorServicePoint = ServicePointManager.FindServicePoint(errorUri);
            errorServicePoint.Expect100Continue = false;
        }
    }
}