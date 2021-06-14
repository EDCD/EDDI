using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ShipRepairDroneEvent : Event
    {
        public const string NAME = "Repair drone";
        public const string DESCRIPTION = "Triggered when your ship is repaired via a repair limpet controller";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-02T00:08:38Z\", \"event\":\"RepairDrone\", \"HullRepaired\":103.332764 }";

        [PublicAPI("The amount of damage repaired in the ship's hull")]
        public decimal? hull { get; private set; }

        [PublicAPI("The amount of damage repaired in the ship's cockpit")]
        public decimal? cockpit { get; private set; }

        [PublicAPI("The amount of corrosion damage repaired")]
        public decimal? corrosion { get; private set; }

        public ShipRepairDroneEvent(DateTime timestamp, decimal? hull, decimal? cockpit, decimal? corrosion) : base(timestamp, NAME)
        {
            this.hull = hull;
            this.cockpit = cockpit;
            this.corrosion = corrosion;
        }
    }
}
