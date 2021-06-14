using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiCargoMonitor
{
    [PublicAPI]
    public class PowerCommodityDeliveredEvent : Event
    {
        public const string NAME = "Power commodity delivered";
        public const string DESCRIPTION = "Triggered when a commander delivers a commodity to a power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-02T16:10:26Z"", ""event"":""PowerplayDeliver"", ""Power"":""Aisling Duval"", ""Type"":""$aislingmediamaterials_name;"", ""Type_Localised"":""Aisling Media Materials"", ""Count"":3 }";

        [PublicAPI("The name of the power for which the commander is delivering the commodity")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("The name of the commodity the commander is delivering")]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        [PublicAPI("The amount of the commodity the commander is delivering")]
        public int amount { get; private set; }

        // Not intended to be user facing

        public Power Power { get; private set; }

        public CommodityDefinition commodityDefinition { get; private set; }

        public PowerCommodityDeliveredEvent(DateTime timestamp, Power Power, CommodityDefinition commodity, int amount) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.amount = amount;
            this.commodityDefinition = commodity;
        }
    }
}
