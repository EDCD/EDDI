using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CargoWingUpdateEvent : Event
    {
        public const string NAME = "Cargo wingupdate";
        public const string DESCRIPTION = "Triggered when a wing-mate collects or delivers cargo for a wing mission";
        public const string SAMPLE = null;

        [PublicAPI("The ID of the mission")]
        public long? missionid { get; private set; }

        [PublicAPI("The update type. 'Collect' or 'Deliver'")]
        public string updatetype { get; private set; }

        [PublicAPI("The type of cargo (commodity)")]
        public string commodity => commodityDefinition?.localizedName ?? "Unknown commodity";

        [PublicAPI("The amount of cargo collected or delivered for this event")]
        public int? amount { get; private set; }

        [PublicAPI("The total amount of cargo collected")]
        public int collected { get; private set; }

        [PublicAPI("The total amount of cargo delivered")]
        public int delivered { get; private set; }

        [PublicAPI("The total amount of cargo to deliver to complete the mission")]
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