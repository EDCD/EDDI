using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiCargoMonitor
{
    public class CommodityCollectedEvent : Event
    {
        public const string NAME = "Commodity collected";
        public const string DESCRIPTION = "Triggered when you pick up a commodity in your ship or SRV";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CollectCargo\",\"Type\":\"agriculturalmedicines\",\"Stolen\":true}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommodityCollectedEvent()
        {
            VARIABLES.Add("commodity", "The name of the commodity collected");
            VARIABLES.Add("stolen", "If the cargo is stolen");
            VARIABLES.Add("missionid", "ID of the mission-related commodity, if applicable");
        }

        [PublicAPI]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        [PublicAPI]
        public bool stolen { get; }

        [PublicAPI]
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
