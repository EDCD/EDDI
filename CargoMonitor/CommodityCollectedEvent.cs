using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiCargoMonitor
{
    [PublicAPI]
    public class CommodityCollectedEvent : Event
    {
        public const string NAME = "Commodity collected";
        public const string DESCRIPTION = "Triggered when you pick up a commodity in your ship or SRV";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CollectCargo\",\"Type\":\"agriculturalmedicines\",\"Stolen\":true}";

        [PublicAPI("The name of the commodity collected")]
        public string commodity => commodityDefinition?.localizedName;

        [PublicAPI("If the commodity is stolen")]
        public bool stolen { get; }

        [PublicAPI("ID of the mission-related commodity, if applicable")]
        public long? missionid { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public CommodityCollectedEvent(DateTime timestamp, CommodityDefinition commodity, long? missionid, bool stolen) : base(timestamp, NAME)
        {
            this.commodityDefinition = commodity;
            this.missionid = missionid;
            this.stolen = stolen;
        }
    }
}
