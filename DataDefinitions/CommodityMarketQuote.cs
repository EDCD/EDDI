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

        // Update the definition with the new galactic average price whenever this is set.
        public int avgprice
        {
            get { return definition?.avgprice ?? 0; }
            set
            {
                if (definition is null)
                {
                    return;
                }
                if (!fromFDev)
                {
                    return;
                }

                definition.avgprice = value;
            }
        }

        public bool rare => definition?.rare ?? false;

        // Admin... we only want to send commodity data from the Companion API or market.json to EDDN - no data from 3rd party other sources.
        [JsonIgnore]
        public bool fromFDev { get; set; }

        public CommodityMarketQuote(CommodityDefinition definition)
        {
            this.definition = definition;
            stockbracket = "";
            demandbracket = "";
            StatusFlags = new List<string>();
            fromFDev = false;
        }

        public static CommodityMarketQuote FromCapiJson(JObject capiJSON)
        {
            CommodityMarketQuote quote = null;
            try
            {
                long eliteId = (long)capiJSON["id"];
                string edName = (string)capiJSON["name"];
                string category = (string)capiJSON["category"];
                int meanPrice = (int)capiJSON["meanPrice"];

                // `CommodityDefinition.FromEDName()` would create a new commodity if we don't already know about the commodity.
                // We'll defer using that, in favor of instead creating a more complete definition below.
                CommodityDefinition commodityDef = CommodityDefinition.CommodityDefinitionFromEliteID(eliteId);
                if (commodityDef is null)
                {
                    CommodityCategory commodityCategory = CommodityCategory.FromEDName(category);
                    if (commodityCategory != CommodityCategory.NonMarketable)
                    {
                        // Unknown commodity; report the full object so that we can update the definitions 
                        Logging.Info("Commodity definition error: " + (string)capiJSON["name"], JsonConvert.SerializeObject(capiJSON));

                        // Create a basic commodity definition from the info available 
                        commodityDef = new CommodityDefinition(eliteId, -1, edName, commodityCategory, meanPrice, false);
                    }
                }

                dynamic intStringOrNull(JObject jObject, string key)
                {
                    JToken token = jObject[key];
                    switch (token.Type)
                    {
                        case JTokenType.Integer:
                            return (int)token;
                        case JTokenType.String:
                            return (string)token;
                        case JTokenType.None:
                            return null;
                        default:
                            return token.ToString();
                    }
                }

                quote = new CommodityMarketQuote(commodityDef)
                {
                    fromFDev = true,
                    buyprice = (int)capiJSON["buyPrice"],
                    // Fleet carriers return zero and do not display the true average price. We must disregard that information so preserve the true average price.
                    avgprice = (int)capiJSON["meanPrice"] > 0 ? (int)capiJSON["meanPrice"] : commodityDef?.avgprice ?? 0,
                    stock = (int)capiJSON["stock"],
                    stockbracket = intStringOrNull(capiJSON, "stockBracket"),
                    sellprice = (int)capiJSON["sellPrice"],
                    demand = (int)capiJSON["demand"],
                    demandbracket = intStringOrNull(capiJSON, "demandBracket")
                };

                List<string> StatusFlags = new List<string>();
                if (capiJSON["statusFlags"] != null)
                {
                    foreach (string statusFlag in capiJSON["statusFlags"])
                    {
                        StatusFlags.Add(statusFlag);
                    }
                    quote.StatusFlags = StatusFlags;
                }
            }
            catch (Exception e)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "capiJson", JsonConvert.SerializeObject(capiJSON) },
                    { "Exception", e }
                };
                Logging.Error("Failed to handle Frontier API commodity json", data);
            }
            return quote;
        }

        public static CommodityMarketQuote FromMarketInfo(MarketInfo item)
        {
            CommodityMarketQuote quote = null;
            try
            {
                long eliteId = item.id;
                string edName = item.name?.ToLowerInvariant()
                    ?.Replace("$", "")
                    ?.Replace("_name;", "");
                string category = item.category.ToLowerInvariant()
                    ?.Replace("$market_category", "")
                    ?.Replace("_", "")
                    ?.Replace(";", "");

                // `CommodityDefinition.FromEDName()` would create a new commodity if we don't already know about the commodity.
                // We'll defer using that, in favor of instead creating a more complete definition below.
                CommodityDefinition commodityDef = CommodityDefinition.CommodityDefinitionFromEliteID(eliteId);
                if (commodityDef is null)
                {
                    CommodityCategory commodityCategory = CommodityCategory.FromEDName(category);
                    if (commodityCategory != CommodityCategory.NonMarketable)
                    {
                        // Unknown commodity; report the full object so that we can update the definitions 
                        Logging.Info("Commodity definition error: " + edName);

                        // Create a basic commodity definition from the info available 
                        commodityDef = new CommodityDefinition(eliteId, -1, edName, CommodityCategory.FromEDName(category), item.meanprice, false);
                    }
                }

                quote = new CommodityMarketQuote(commodityDef)
                {
                    fromFDev = true,
                    buyprice = item.buyprice,
                    // Fleet carriers return zero and do not display the true average price. We must disregard that information so preserve the true average price.
                    avgprice = item.meanprice > 0 ? item.meanprice : commodityDef?.avgprice ?? 0,
                    stock = item.stock,
                    stockbracket = item.stockbracket,
                    sellprice = item.sellprice,
                    demand = item.demand,
                    demandbracket = item.demandbracket
                };

                List<string> StatusFlags = new List<string>();
                if (item.producer)
                {
                    StatusFlags.Add("Producer");
                }
                if (item.consumer)
                {
                    StatusFlags.Add("Consumer");
                }
                quote.StatusFlags = StatusFlags;
            }
            catch (Exception e)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "item", JsonConvert.SerializeObject(item) },
                    { "Exception", e }
                };
                Logging.Error("Failed to handle market.json commodity item", data);
            }
            return quote;
        }
    }
}
