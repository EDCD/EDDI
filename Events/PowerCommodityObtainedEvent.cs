using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PowerCommodityObtainedEvent (
        DateTime timestamp,
        Power power,
        CommodityDefinition commodity,
        int amount )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Power commodity obtained";
        public const string DESCRIPTION = "Triggered when a commander obtains a commodity from a power";
        public const string SAMPLE = "{ \"timestamp\":\"2016-12-02T16:10:26Z\", \"event\":\"PowerplayCollect\", \"Power\":\"Aisling Duval\", \"Type\":\"$aislingmediamaterials_name;\", \"Type_Localised\":\"Aisling Media Materials\", \"Count\":3 }";

        [PublicAPI("The name of the power for which the commander is obtaining the commodity")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("The name of the commodity the commander is obtaining")]
        public string commodity => commodityDefinition?.localizedName;

        [PublicAPI("The amount of the commodity the commander is obtaining")]
        public int amount { get; private set; } = amount;

        // Not intended to be user facing

        public Power Power { get; private set; } = power;

        public CommodityDefinition commodityDefinition { get; private set; } = commodity;
    }
}
