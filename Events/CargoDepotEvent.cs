using EddiDataDefinitions;
using System;
using System.Collections.Generic;


namespace EddiEvents
{
    public class CargoDepotEvent : Event
    {
        public const string NAME = "Cargo depot";
        public const string DESCRIPTION = "Triggered when collecting or delivering cargo for a wing mission";
        public const string SAMPLE = "{ \"timestamp\":\"2018-06-17T04:20:21Z\", \"event\":\"CargoDepot\", \"MissionID\":391606997, \"UpdateType\":\"Deliver\", \"CargoType\":\"NonLethalWeapons\", \"CargoType_Localised\":\"Non-Lethal Weapons\", \"Count\":704, \"StartMarketID\":0, \"EndMarketID\":3224777216, \"ItemsCollected\":0, \"ItemsDelivered\":704, \"TotalItemsToDeliver\":704, \"Progress\":0.000000 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoDepotEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("updatetype", "The update type. One of: 'Collect', 'Deliver', 'WingUpdate'");
            VARIABLES.Add("cargotype", "The type of cargo (commodity)");
            VARIABLES.Add("startmarketid", "The Market ID of the 'collection' mission depot, 0 if not applicable");
            VARIABLES.Add("endmarketid", "The Market ID of the 'delivery' mission depot");
            VARIABLES.Add("amount", "The amount of cargo collected or delivered for this event");
            VARIABLES.Add("collected", "The total amount of cargo collected");
            VARIABLES.Add("delivered", "The total amount of cargo delivered");
            VARIABLES.Add("totaltodeliver", "The total amount of cargo to deliver to complete the mission");
        }

        public long? missionid { get; private set; }

        public string updatetype { get; private set; }

        public CommodityDefinition commodityDefinition { get; private set; }

        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        public int? amount { get; private set; }

        public long startmarketid { get; private set; }

        public long endmarketid { get; private set; }

        public int collected { get; private set; }

        public int delivered { get; private set; }

        public int totaltodeliver { get; private set; }

        public CargoDepotEvent(DateTime timestamp, long? missionid, string updatetype, CommodityDefinition commodity, int? amount, long startmarketid, long endmarketid, int collected, int delivered, int totaltodeliver) : base(timestamp, NAME)
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
