using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    // EDDN Schema reference: https://github.com/EDSM-NET/EDDN/blob/master/schemas/commodity-v3.0.json

    /// <summary> This class is designed to be deserialized from market.json or from the Frontier API.
    /// This class is designed to be serialized and passed to the commodity schema on EDDN. </summary>
    public class MarketInfoItem
    {
        [JsonProperty("name")]
        public string edName 
        {
            get => _edName;
            set => _edName = value.ToLowerInvariant()
                .Replace("$", "")
                .Replace("_name;", "");
        }
        [JsonIgnore]
        private string _edName;

        [JsonProperty]
        public string category 
        { 
            get => _category; 
            set => _category = value.ToLowerInvariant()
                .Replace("$market_category", "")
                .Replace("_", "")
                .Replace(";", ""); 
        }
        [JsonIgnore]
        private string _category;
        
        // The Frontier API uses `categoryName` rather than `category`, we normalize that here.
        [JsonProperty] // As a private property, it shall not be serialized to EDDN.
        protected virtual string categoryName { set => category = value; }

        // Station prices
        [JsonProperty]
        public int buyPrice { get; set; }
        [JsonProperty]
        public int meanPrice { get; set; }
        [JsonProperty]
        public int sellPrice { get; set; }

        // Station in-stock parameter
        [JsonProperty]
        public int stock { get; set; }
        [JsonProperty]
        public CommodityBracket? stockBracket { get; set; } // Possible values are 0, 1, 2, 3, or "" so we use an optional enum

        // Station in-demand parameters
        [JsonProperty]
        public int demand { get; set; }
        [JsonProperty]
        public CommodityBracket? demandBracket { get; set; } // Possible values are 0, 1, 2, 3, or "" so we use an optional enum

        // Implement conditional property serialization for select properties (ref. https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm)
        // This allows properties to be deserialized to the class but not serialized to EDDN.
        [JsonProperty("id")]
        public long EliteID { get; set; }
        public bool ShouldSerializeEliteID() { return false; }

        [JsonProperty]
        public bool consumer { get => _consumer; set { statusFlags.Add("Consumer"); _consumer = value; } }
        [JsonIgnore]
        private bool _consumer; 
        public bool ShouldSerializeconsumer() { return false; }

        [JsonProperty]
        public bool producer { get => _producer; set { statusFlags.Add("Producer"); _producer = value; } }
        [JsonIgnore]
        private bool _producer; 
        public bool ShouldSerializeproducer() { return false; }

        [JsonProperty]
        public bool rare { get => _rare; set { statusFlags.Add("Rare"); _rare = value; } }
        [JsonIgnore]
        private bool _rare;
        public bool ShouldSerializerare() { return false; }

        [JsonProperty]
        public HashSet<string> statusFlags { get; set; } = new HashSet<string>();
        public bool ShouldSerializestatusFlags()
        {
            // Don't serialize status flags if they are empty as the schema requires that if present they contain at least 1 element
            return (statusFlags != null && statusFlags.Count > 0);
        }

        public bool IsMarketable()
        {
            // Don't serialize non-marketable commodities such as drones / limpets
            if (category.Replace("-","").ToLowerInvariant() == "nonmarketable" || edName.ToLowerInvariant() == "drones")
            {
                return false;
            }
            return true;
        }

        public MarketInfoItem()
        { }

        public MarketInfoItem(long eliteId, string Name, string Category, int BuyPrice, int SellPrice, int MeanPrice, CommodityBracket? StockBracket, CommodityBracket? DemandBracket, int Stock, int Demand, bool Consumer, bool Producer, bool Rare)
        {
            this.EliteID = eliteId;
            this.edName = Name;
            this.category = Category;
            this.buyPrice = BuyPrice;
            this.sellPrice = SellPrice;
            this.meanPrice = MeanPrice;
            this.stockBracket = StockBracket;
            this.demandBracket = DemandBracket;
            this.stock = Stock;
            this.demand = Demand;
            this.consumer = Consumer;
            this.producer = Producer;
            this.rare = Rare;
        }

        public MarketInfoItem(long eliteId, string Name, string Category, int BuyPrice, int SellPrice, int MeanPrice, CommodityBracket? StockBracket, CommodityBracket? DemandBracket, int Stock, int Demand, HashSet<string> statusFlags)
        {
            this.EliteID = eliteId;
            this.edName = Name;
            this.category = Category;
            this.buyPrice = BuyPrice;
            this.sellPrice = SellPrice;
            this.meanPrice = MeanPrice;
            this.stockBracket = StockBracket;
            this.demandBracket = DemandBracket;
            this.stock = Stock;
            this.demand = Demand;
            this.statusFlags = statusFlags;
        }

        public CommodityMarketQuote ToCommodityMarketQuote()
        {
            var definition = CommodityDefinition.CommodityDefinitionFromEliteID(EliteID);

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
                definition = new CommodityDefinition(EliteID, -1, edName, CommodityCategory.FromEDName(category), meanPrice);
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
