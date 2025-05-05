using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityAmount
    {
        [PublicAPI, JsonIgnore]
        public string commodity => commodityDefinition.localizedName;

        [PublicAPI]
        public int amount { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        [JsonIgnore]
        public string edname => commodityDefinition.edname;

        public CommodityAmount( [JetBrains.Annotations.NotNull] CommodityDefinition commodity, int amount)
        {
            this.commodityDefinition = commodity ??
                                       throw new ArgumentNullException( nameof(commodity), @"Commodity cannot be null." );
            this.amount = amount;
        }
    }
}
