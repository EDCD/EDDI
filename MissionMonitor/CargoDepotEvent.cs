using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;


namespace EddiMissionMonitor
{
    public class CargoDepotEvent : Event
    {
        public const string NAME = "Cargo depot";
        public const string DESCRIPTION = "Triggered when collecting or delivering cargo for a wing mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"CargoDepot\", \"MissionID\":26493517, \"UpdateType\":\"Deliver\", \"CargoType\":\"SyntheticMeats\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoDepotEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("updatetype", "The update type. One of: 'Collect', 'Deliver', 'WingUpdate'");
            VARIABLES.Add("cargotype", "The type of cargo (commodity)");
            VARIABLES.Add("amount", "The amount of cargo collected or delivered for this event");
            VARIABLES.Add("collected", "The total amount of cargo collected");
            VARIABLES.Add("delivered", "The total amount of cargo delivered");
            VARIABLES.Add("totaltodeliver", "The total amount of cargo to deliver to complete the mission");
            VARIABLES.Add("progress", "The total amount of cargo to delivered");
        }

        public long? missionid { get; private set; }

        public string updatetype { get; private set; }

        public CommodityDefinition commodityDefinition { get; private set; }

        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        public int? amount { get; private set; }

        public int collected { get; private set; }

        public int delivered { get; private set; }

        public int totaltodeliver { get; private set; }

        public decimal progress { get; private set; }

        public CargoDepotEvent(DateTime timestamp, long? missionid, string updatetype, CommodityDefinition commodity, int? amount, int collected, int delivered, int totaltodeliver, decimal progress) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.updatetype = updatetype;
            this.commodityDefinition = commodity;
            this.amount = amount;
            this.collected = collected;
            this.delivered = delivered;
            this.totaltodeliver = totaltodeliver;
            this.progress = progress;
        }
    }
}
