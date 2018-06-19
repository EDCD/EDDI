using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;


namespace EddiMissionMonitor
{
    public class CargoWingUpdateEvent : Event
    {
        public const string NAME = "Cargo wingupdate";
        public const string DESCRIPTION = "Triggered when a wing-mate collects or delivers cargo for a wing mission";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoWingUpdateEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("updatetype", "The update type. One of: 'Collect', 'Deliver', 'WingUpdate'");
            VARIABLES.Add("commodity", "The type of cargo (commodity)");
            VARIABLES.Add("amount", "The amount of cargo collected or delivered for this event");
            VARIABLES.Add("remaining", "The amount of cargo remaining to be delivered");
        }

        public long? missionid { get; private set; }

        public string updatetype { get; private set; }

        public CommodityDefinition commodityDefinition { get; private set; }

        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        public int? amount { get; private set; }

        public int remaining { get; private set; }

        public CargoWingUpdateEvent(DateTime timestamp, long? missionid, string updatetype, CommodityDefinition commodity, int? amount, int remaining) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.updatetype = updatetype;
            this.commodityDefinition = commodity;
            this.amount = amount;
            this.remaining = remaining;
        }
    }
}
