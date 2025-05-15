using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityAmount
    {
        [PublicAPI, JsonIgnore]
        public string commodity => commodityDefinition.localizedName;

        [PublicAPI, JsonProperty]
        public int amount { get; }

        // Not intended to be user facing

        [JsonProperty]
        public CommodityDefinition commodityDefinition { get; set; }

        [JsonIgnore]
        public string edname => commodityDefinition.edname;

        public CommodityAmount( [JetBrains.Annotations.NotNull] CommodityDefinition commodityDefinition, int amount)
        {
            this.commodityDefinition = commodityDefinition ??
                                       throw new ArgumentNullException( nameof(commodityDefinition), @"Commodity definition cannot be null." );
            this.amount = amount;
        }
    }
}
