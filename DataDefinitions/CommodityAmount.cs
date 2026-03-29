using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityAmount ( [ JetBrains.Annotations.NotNull ] CommodityDefinition commodityDefinition, int amount )
    {
        [PublicAPI, JsonIgnore]
        public string commodity => commodityDefinition.localizedName;

        [PublicAPI, JsonProperty]
        public int amount { get; } = amount;

        // Not intended to be user facing

        [JsonProperty]
        public CommodityDefinition commodityDefinition { get; set; } = commodityDefinition ??
                                                                       throw new ArgumentNullException( nameof(commodityDefinition), @"Commodity definition cannot be null." );

        [JsonIgnore]
        public string edname => commodityDefinition.edname;
    }
}
