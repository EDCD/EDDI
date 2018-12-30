using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class MarketInfo
    {
        [JsonProperty]
        public long id { get; set; }
        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public string category { get; set; }

        // Station prices
        [JsonProperty]
        public int buyprice { get; set; }
        [JsonProperty]
        public int meanprice { get; set; }
        [JsonProperty]
        public int sellprice { get; set; }

        // Station in-stock parameter
        [JsonProperty]
        public int stock { get; set; }
        [JsonProperty]
        public int stockbracket { get; set; }

        // Station in-demand parameters
        [JsonProperty]
        public int demand { get; set; }
        [JsonProperty]
        public int demandbracket { get; set; }

        [JsonProperty]
        public bool consumer { get; set; }
        [JsonProperty]
        public bool producer { get; set; }
        [JsonProperty]
        public bool rare { get; set; }

        public MarketInfo()
        { }

        public MarketInfo(MarketInfo MarketInfo)
        {
            this.id = MarketInfo.id;
            this.name = MarketInfo.name;
            this.category = MarketInfo.category;
            this.buyprice = MarketInfo.buyprice;
            this.meanprice = MarketInfo.meanprice;
            this.sellprice = MarketInfo.sellprice;
            this.stock = MarketInfo.stock;
            this.stockbracket = MarketInfo.stockbracket;
            this.demand = MarketInfo.demand;
            this.demandbracket = MarketInfo.demandbracket;
            this.consumer = MarketInfo.consumer;
            this.producer = MarketInfo.producer;
            this.rare = MarketInfo.rare;
        }

        public MarketInfo(long id, string Name, string Category, int BuyPrice, int SellPrice, int MeanPrice, int StockBracket, int DemandBracket, int Stock, int Demand, bool Consumer, bool Producer, bool Rare)
        {
            this.id = id;
            this.name = Name;
            this.category = Category;
            this.buyprice = BuyPrice;
            this.sellprice = SellPrice;
            this.meanprice = MeanPrice;
            this.stockbracket = StockBracket;
            this.demandbracket = DemandBracket;
            this.stock = Stock;
            this.demand = Demand;
            this.consumer = Consumer;
            this.producer = Producer;
            this.rare = Rare;
        }
    }
}
