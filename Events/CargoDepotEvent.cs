using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CargoDepotEvent : Event
    {
        public const string NAME = "Cargo depot";
        public const string DESCRIPTION = "Triggered when collecting or delivering cargo for a wing mission";
        public static readonly string[] SAMPLES =
        {
            "{ \"timestamp\":\"2018-06-17T04:20:21Z\", \"event\":\"CargoDepot\", \"MissionID\":391606997, \"UpdateType\":\"Deliver\", \"CargoType\":\"NonLethalWeapons\", \"CargoType_Localised\":\"Non-Lethal Weapons\", \"Count\":704, \"StartMarketID\":0, \"EndMarketID\":3224777216, \"ItemsCollected\":0, \"ItemsDelivered\":704, \"TotalItemsToDeliver\":704, \"Progress\":0.000000 }",
            "{ \"timestamp\":\"2020-06-02T07:48:09Z\", \"event\":\"CargoDepot\", \"MissionID\":585994891, \"UpdateType\":\"Collect\", \"CargoType\":\"FoodCartridges\", \"CargoType_Localised\":\"Food Cartridges\", \"Count\":98, \"StartMarketID\":3222037248, \"EndMarketID\":3223259392, \"ItemsCollected\":98, \"ItemsDelivered\":0, \"TotalItemsToDeliver\":108, \"Progress\":0.907407 }",
            "{ \"timestamp\":\"2020-06-02T08:29:27Z\", \"event\":\"CargoDepot\", \"MissionID\":585994997, \"UpdateType\":\"Deliver\", \"CargoType\":\"FoodCartridges\", \"CargoType_Localised\":\"Food Cartridges\", \"Count\":96, \"StartMarketID\":3222037248, \"EndMarketID\":3223259392, \"ItemsCollected\":96, \"ItemsDelivered\":96, \"TotalItemsToDeliver\":108, \"Progress\":0.000000 }"
        };

        [PublicAPI("The ID of the mission")]
        public long missionid { get; private set; }

        [PublicAPI("The update type. One of: 'Collect', 'Deliver', 'WingUpdate'")]
        public string updatetype { get; private set; }

        [PublicAPI("The type of cargo (commodity)")]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        [PublicAPI("The Market ID of the 'collection' mission depot, 0 if not applicable")]
        public long startmarketid { get; private set; }

        [PublicAPI("The Market ID of the 'delivery' mission depot")]
        public long endmarketid { get; private set; }

        [PublicAPI("The amount of cargo being collected or delivered for this event")]
        public int? amount { get; private set; }

        [PublicAPI("The total amount of cargo collected")]
        public int collected { get; private set; }

        [PublicAPI("The total amount of cargo delivered")]
        public int delivered { get; private set; }

        [PublicAPI("The total amount of cargo to deliver to complete the mission")]
        public int totaltodeliver { get; private set; }

        // Not intended to be user facing
        
        public CommodityDefinition commodityDefinition { get; private set; }
        
        public CargoDepotEvent(DateTime timestamp, long missionid, string updatetype, CommodityDefinition commodity, int? amount, long startmarketid, long endmarketid, int collected, int delivered, int totaltodeliver) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.updatetype = updatetype;
            this.commodityDefinition = commodity;
            this.amount = amount;
            this.startmarketid = startmarketid;
            this.endmarketid = endmarketid;
            this.collected = collected;
            this.delivered = delivered;
            this.totaltodeliver = totaltodeliver;
        }
    }
}
