using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
        public int? buyprice { get; set; }
        public int? stock { get; set; }

        // StockBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        // VB: it can be an optional enum no prob
        public dynamic stockbracket { get; set; }

        public int? sellprice { get; set; }
        public int? demand { get; set; }

        // DemandBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        // VB: same type as stockbracket above
        public dynamic demandbracket { get; set; }

        public List<string> StatusFlags { get; set; }

        public long? EliteID => definition?.EliteID;
        public long? EDDBID => definition?.EDDBID;
        [Obsolete("Please use localizedName or InvariantName")]
        public string category => definition?.category.localizedName;
        public int? avgprice => definition?.avgprice;
        public bool? rare => definition?.rare;

        public CommodityMarketQuote(CommodityDefinition definition)
        {
            this.definition = definition;
            buyprice = null;
            stock = null;
            stockbracket = "";
            sellprice = null;
            demand = null;
            demandbracket = "";
            StatusFlags = new List<string>();
        }
    }
}
