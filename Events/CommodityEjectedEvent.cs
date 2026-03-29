using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommodityEjectedEvent (
        DateTime timestamp,
        CommodityDefinition commodity,
        int amount,
        long? missionid,
        bool abandoned )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commodity ejected";
        public const string DESCRIPTION = "Triggered when you eject a commodity from your ship or SRV";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"EjectCargo\",\"Type\":\"agriculturalmedicines\",\"Count\":2,\"Abandoned\":true}";

        [PublicAPI("The name of the commodity ejected")]
        public string commodity => commodityDefinition?.localizedName;

        [PublicAPI("The amount of commodity ejected")]
        public int amount { get; } = amount;

        [PublicAPI("True if the cargo has been abandoned")]
        public bool abandoned { get; } = abandoned;

        [PublicAPI("ID of the mission-related commodity, if applicable")]
        public long? missionid { get; } = missionid;

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; } = commodity;
    }
}
