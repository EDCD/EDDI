using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommodityCollectedEvent (
        DateTime timestamp,
        CommodityDefinition commodity,
        ulong? missionid,
        bool stolen )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commodity collected";
        public const string DESCRIPTION = "Triggered when you pick up a commodity in your ship or SRV";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CollectCargo\",\"Type\":\"agriculturalmedicines\",\"Stolen\":true}";

        [PublicAPI("The name of the commodity collected")]
        public string commodity => commodityDefinition?.localizedName;

        [PublicAPI("If the commodity is stolen")]
        public bool stolen { get; } = stolen;

        [PublicAPI("ID of the mission-related commodity, if applicable")]
        public ulong? missionid { get; } = missionid;

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; } = commodity;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var commodityName = JsonParsing.getString(data, "Type");
            var commodity = CommodityDefinition.FromEDName(commodityName);
            if ( commodity == null )
            {
                Logging.Error( "Failed to map cargo type " + commodityName + " to commodity definition", line );
            }
            var missionid = JsonParsing.getOptionalULong(data, "MissionID");
            var stolen = JsonParsing.getBool(data, "Stolen");
            events.Add( new CommodityCollectedEvent( timestamp, commodity, missionid, stolen ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
