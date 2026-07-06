using Newtonsoft.Json;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary> This class is designed to be deserialized from either shipyard.json or the Frontier API. </summary>
    public class OutfittingInfoItem
    {
        [JsonProperty("name")]
        public string edName { get; set; }

        [JsonProperty("category")]
        public string edCategory { get; set; }

        // Station prices
        [JsonProperty("BuyPrice")]
        public long buyPrice { get; set; }

        [JsonProperty( "BuyMercCoinsPrice" )]
        public long? buyMercPrice { get; set; }

        // The Frontier API uses `cost` rather than `BuyPrice`, we normalize that here.
        [JsonProperty] // As a private property, it shall not be serialized.
        private protected long cost { set => buyPrice = value; }

        public OutfittingInfoItem()
        { }

        public OutfittingInfoItem ( string edName, int BuyPrice )
        {
            this.edName = edName;
            this.buyPrice = BuyPrice;
        }

        public OutfittingInfoItem ( string edName, string edCategory, int BuyPrice )
        {
            this.edName = edName;
            this.edCategory = edCategory;
            this.buyPrice = BuyPrice;
        }

        public Module ToModule()
        {
            var module = new Module(Module.FromEDName(edName, this) ?? new Module());
            if (module.invariantName == null)
            {
                // Unknown module; report the full object so that we can update the definitions
                Logging.Info("Module definition error: " + edName, JsonConvert.SerializeObject(this));

                // Create a basic module & supplement from the info available
                module = new Module( edName, edName, -1, "", buyPrice);
            }
            else
            {
                module.price = buyPrice;
                module.mercPrice = buyMercPrice;
            }
            return module;
        }
    }
}