using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipRepairDroneEvent ( DateTime timestamp, decimal? hull, decimal? cockpit, decimal? corrosion )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Repair drone";
        public const string DESCRIPTION = "Triggered when your ship is repaired via a repair limpet controller";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-02T00:08:38Z\", \"event\":\"RepairDrone\", \"HullRepaired\":103.332764 }";

        [PublicAPI("The amount of damage repaired in the ship's hull")]
        public decimal? hull { get; private set; } = hull;

        [PublicAPI("The amount of damage repaired in the ship's cockpit")]
        public decimal? cockpit { get; private set; } = cockpit;

        [PublicAPI("The amount of corrosion damage repaired")]
        public decimal? corrosion { get; private set; } = corrosion;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var hull = JsonParsing.getOptionalDecimal(data, "HullRepaired");
            var cockpit = JsonParsing.getOptionalDecimal(data, "CockpitRepaired");
            var corrosion = JsonParsing.getOptionalDecimal(data, "CorrosionRepaired");
            events.Add( new ShipRepairDroneEvent( timestamp, hull, cockpit, corrosion ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
