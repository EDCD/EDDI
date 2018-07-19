using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityMarketQuote
    {
        // TODO should really be readonly but we need to set it during JSON parsing
        public CommodityDefinition definition;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (definition == null)
            {
                string name = null;
                if (additionalJsonData != null)
                {
                    if (name == null && additionalJsonData.ContainsKey("EDName"))
                    {
                        name = (string)additionalJsonData?["EDName"];
                    }
                    if (name == null && additionalJsonData.ContainsKey("name"))
                    {
                        name = (string)additionalJsonData?["name"];
                    }
                }
                if (name != null)
                {
                    definition = CommodityDefinition.FromEDName(name) ?? CommodityDefinition.FromName(name);
                }
            }
            additionalJsonData = null;
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> additionalJsonData;

        public string invariantName
        {
            get => definition?.invariantName;
        }

        public string localizedName
        {
            get => definition?.localizedName;
        }

        [Obsolete("deprecated for UI usage but retained for JSON conversion from the cAPI")]
        public string name
        {
            get => definition?.localizedName;
            set
            {
                CommodityDefinition newDef = CommodityDefinition.FromName(value);
                this.definition = newDef;
            }
        }

        // Per-station information
        public int buyprice { get; set; }
        public int stock { get; set; }

        // StockBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        // VB: it can be an optional enum no prob
        public dynamic stockbracket { get; set; }

        public int sellprice { get; set; }
        public int demand { get; set; }

        // DemandBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        // VB: same type as stockbracket above
        public dynamic demandbracket { get; set; }

        public List<string> StatusFlags { get; set; }

        public long? EliteID => definition?.EliteID;
        public long? EDDBID => definition?.EDDBID;
        [Obsolete("Please use localizedName or InvariantName")]
        public string category => definition?.category.localizedName;
        public int? avgprice => definition?.avgprice;
        public bool rare => definition?.rare ?? false;

        public CommodityMarketQuote(CommodityDefinition definition)
        {
            this.definition = definition;
            stockbracket = "";
            demandbracket = "";
            StatusFlags = new List<string>();
        }

        public static CommodityMarketQuote FromCapiJson(JObject capiJSON)
        {
            CommodityDefinition commodityDef = CommodityDefinition.CommodityDefinitionFromEliteID((long)capiJSON["id"]);
            if (commodityDef == null || (string)capiJSON["name"] != commodityDef.edname)
            {
                if (commodityDef.edname != "Drones")
                {
                    // Unknown commodity; report the full object so that we can update the definitions
                    Logging.Info("Commodity definition error: " + (string)capiJSON["name"], JsonConvert.SerializeObject(capiJSON));
                }
                return null;
            }
            CommodityMarketQuote quote = new CommodityMarketQuote(commodityDef);
            quote.buyprice = (int)capiJSON["buyPrice"];
            quote.stock = (int)capiJSON["stock"];
            quote.stockbracket = (int)capiJSON["stockBracket"];
            quote.sellprice = (int)capiJSON["sellPrice"];
            quote.demand = (int)capiJSON["demand"];
            quote.demandbracket = (int)capiJSON["demandBracket"];

            List<string> StatusFlags = new List<string>();
            foreach (dynamic statusFlag in capiJSON["statusFlags"])
            {
                StatusFlags.Add((string)statusFlag);
            }
            quote.StatusFlags = StatusFlags;
            return quote;
        }
    }
}
