using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EddiDataDefinitions
{
    public class CommodityMarketQuote
    {
        // should ideally be readonly but we need to set it during JSON parsing
        public CommodityDefinition definition { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (definition == null)
            {
                string _name = null;
                if (additionalJsonData != null)
                {
                    if (additionalJsonData.ContainsKey("EDName"))
                    {
                        _name = (string)additionalJsonData?["EDName"];
                    }
                    if (_name == null && additionalJsonData.ContainsKey("name"))
                    {
                        _name = (string)additionalJsonData?["name"];
                    }
                }
                if (_name != null)
                {
                    definition = CommodityDefinition.FromNameOrEDName(_name);
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
                if (this.definition == null)
                {
                    CommodityDefinition newDef = CommodityDefinition.FromNameOrEDName(value);
                    this.definition = newDef;
                }
            }
        }

        // Per-station information (prices are usually integers but not always)
        public decimal buyprice { get; set; }
        public int stock { get; set; }

        // StockBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        public CommodityBracket? stockbracket { get; set; }

        public decimal sellprice { get; set; }
        public int demand { get; set; }

        // DemandBracket can contain the values 0, 1, 2, 3 or "" (yes, really) so needs to be dynamic
        public CommodityBracket? demandbracket { get; set; }


        public long? EliteID => definition?.EliteID;
        public long? EDDBID => definition?.EDDBID;
        [Obsolete("Please use localizedName or InvariantName")]
        public string category => definition?.category.localizedName;

        // Update the definition with the new galactic average price whenever this is set.
        // Fleet carriers return zero and do not display the true average price. We must disregard that information so preserve the true average price.
        // The average pricing data is the only data which may reference our internal definition, and even then only to obtain an average price.
        public decimal avgprice
        {
            get => definition?.avgprice ?? 0;
            set
            {
                if (definition is null)
                {
                    return;
                }
                definition.avgprice = value;
            }
        }

        public bool rare => definition?.rare ?? false;
        public HashSet<string> StatusFlags { get; set; }

        [JsonConstructor]
        public CommodityMarketQuote(CommodityDefinition definition)
        {
            if (definition is null) { return; }
            this.definition = definition;
        }
    }
}
