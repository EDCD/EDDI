using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    /// <summary> This class is designed to be deserialized from FCMaterials.json or from the Frontier API.
    /// This class is designed to be serialized and passed to the FCMaterials schema on EDDN. </summary>
    public class FCMaterialInfoItem
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

        // Station prices
        [JsonProperty]
        public int price { get; set; }

        // Station in-stock parameter
        [JsonProperty]
        public int stock { get; set; }

        // Station in-demand parameters
        [JsonProperty]
        public int demand { get; set; }

        [JsonProperty("id")]
        public long EliteID { get; set; }

        public FCMaterialInfoItem()
        { }

        public FCMaterialInfoItem(long eliteId, string Name, int Price, int Stock, int Demand)
        {
            this.EliteID = eliteId;
            this.edName = Name;
            this.price = Price;
            this.stock = Stock;
            this.demand = Demand;
        }
    }
}
