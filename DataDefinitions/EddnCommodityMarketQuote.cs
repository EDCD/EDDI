using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class EddnCommodityMarketQuote
    {
        // This class is designed to be serialized and passed to the commodity schema on EDDN.

        // Schema reference: https://github.com/EDSM-NET/EDDN/blob/master/schemas/commodity-v3.0.json
        [JsonProperty("name")]
        public string edName { get; }
        public int meanPrice { get; }
        public int buyPrice { get; }
        public int stock { get; }
        public CommodityBracket? stockBracket { get; } // Possible values are 0, 1, 2, 3, or "" so we use an optional enum
        public int sellPrice { get; }
        public int demand { get; }
        public CommodityBracket? demandBracket { get; } // Possible values are 0, 1, 2, 3, or "" so we use an optional enum
        public List<string> statusFlags { get; } = new List<string>();

        // Properties which are not included in the EDDN schema but which may someday be included (until then, we mark them with [JsonIgnore]):
        [JsonIgnore]
        public long eliteId { get; }
        [JsonIgnore]
        public string category { get; }

        // The raw output
        [JsonIgnore]
        public string raw { get; }

        // This implements conditional property serialization for the `statusFlags` property (ref. https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm)
        public bool ShouldSerializestatusFlags()
        {
            // Don't serialize status flags if they are empty as the schema requires that if present they contain at least 1 element
            return (statusFlags != null && statusFlags.Count > 0);
        }

        public bool IsMarketable()
        {
            // Don't serialize non-marketable commodities such as drones / limpets
            if (category.ToLowerInvariant() == "nonmarketable" || edName.ToLowerInvariant() == "drones")
            {
                return false;
            }
            return true;
        }

        public EddnCommodityMarketQuote(JObject capiJSON)
        {
            eliteId = (long)capiJSON["id"];
            edName = (string)capiJSON["name"];
            category = (string)capiJSON["categoryname"];
            meanPrice = (int)capiJSON["meanPrice"];
            stock = (int)capiJSON["stock"];
            buyPrice = (int)capiJSON["buyPrice"];
            stockBracket = capiJSON["stockBracket"]?.ToObject<CommodityBracket?>();
            sellPrice = (int)capiJSON["sellPrice"];
            demand = (int)capiJSON["demand"];
            demandBracket = capiJSON["demandBracket"]?.ToObject<CommodityBracket?>();
            if (capiJSON["statusFlags"] != null)
            {
                foreach (string statusFlag in capiJSON["statusFlags"])
                {
                    statusFlags.Add(statusFlag);
                }
            }
            raw = JsonConvert.SerializeObject(capiJSON);
        }

        public EddnCommodityMarketQuote(MarketInfoItem item)
        {
            eliteId = item.id;
            edName = item.name.ToLowerInvariant()
                ?.Replace("$", "")
                ?.Replace("_name;", "");
            category = item.category.ToLowerInvariant()
                ?.Replace("$market_category", "")
                ?.Replace("_", "")
                ?.Replace(";", "");
            meanPrice = item.meanprice;
            stock = item.stock;
            buyPrice = item.buyprice;
            stockBracket = item.stockbracket;
            sellPrice = item.sellprice;
            demand = item.demand;
            demandBracket = item.demandbracket;
            if (item.producer)
            {
                statusFlags.Add("Producer");
            }
            if (item.consumer)
            {
                statusFlags.Add("Consumer");
            }
            if (item.rare)
            {
                statusFlags.Add("Rare");
            }
            raw = JsonConvert.SerializeObject(item);
        }

        public CommodityMarketQuote ToCommodityMarketQuote()
        {
            var definition = CommodityDefinition.CommodityDefinitionFromEliteID(eliteId);

            // Fleet carriers return zero and do not display the true average price. We must disregard that information so preserve the true average price.
            // The average pricing data is the only data which may reference our internal definition, and even then only to obtain an average price.
            if (definition != null)
            {
                definition.avgprice = meanPrice > 0 ? meanPrice : definition.avgprice;
            }
            if (edName != definition?.edname)
            {
                // Unknown or obsolete commodity definition; report the full object so that we can update the definitions 
                Logging.Info("Commodity definition error: " + edName, JsonConvert.SerializeObject(this));
            }

            if (definition is null)
            {
                definition = new CommodityDefinition(eliteId, -1, edName, CommodityCategory.FromEDName(category), meanPrice);
            }
            var quote = new CommodityMarketQuote(definition)
            {
                buyprice = buyPrice,
                stock = stock,
                stockbracket = stockBracket,
                sellprice = sellPrice,
                demand = demand,
                demandbracket = demandBracket,
                StatusFlags = statusFlags
            };

            return quote;
        }
    }
}
