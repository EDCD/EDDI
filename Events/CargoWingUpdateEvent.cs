using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
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
            VARIABLES.Add("updatetype", "The update type. 'Collect' or 'Deliver'");
            VARIABLES.Add("commodity", "The type of cargo (commodity)");
            VARIABLES.Add("amount", "The amount of cargo collected or delivered for this event");
            VARIABLES.Add("collected", "The total amount of cargo collected");
            VARIABLES.Add("delivered", "The total amount of cargo delivered");
            VARIABLES.Add("totaltodeliver", "The total amount of cargo to deliver to complete the mission");
        }

        [PublicAPI]
        public long? missionid { get; private set; }

        [PublicAPI]
        public string updatetype { get; private set; }

        [PublicAPI]
        public string commodity => commodityDefinition?.localizedName ?? "Unknown commodity";

        [PublicAPI]
        public int? amount { get; private set; }

        [PublicAPI]
        public int collected { get; private set; }

        [PublicAPI]
        public int delivered { get; private set; }

        [PublicAPI]
        public int totaltodeliver { get; private set; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; private set; }

        public CargoWingUpdateEvent(DateTime timestamp, long? missionid, string updatetype, CommodityDefinition commodity, int? amount, int collected, int delivered, int totaltodeliver) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.updatetype = updatetype;
            this.commodityDefinition = commodity;
            this.amount = amount;
            this.collected = collected;
            this.delivered = delivered;
            this.totaltodeliver = totaltodeliver;
        }
    }
}