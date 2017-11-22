using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipRepairDroneEvent : Event
    {
        public const string NAME = "Repair drone";
        public const string DESCRIPTION = "Triggered when your ship is repaired via a repair limpet controller";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-02T00:08:38Z\", \"event\":\"RepairDrone\", \"HullRepaired\":103.332764 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        static ShipRepairDroneEvent()
        {
            VARIABLES.Add("hull", "The amount of damage repaired in the ship's hull");
            VARIABLES.Add("cockpit", "The amount of damage repaired in the ship's cockpit");
            VARIABLES.Add("corrosion", "The amount of corrosion damage repaired");
        }

        public decimal? hull { get; private set; }
        public decimal? cockpit { get; private set; }
        public decimal? corrosion { get; private set; }
        
        public ShipRepairDroneEvent(DateTime timestamp, decimal? hull, decimal? cockpit, decimal? corrosion) : base(timestamp, NAME)
        {
            this.hull = hull;
            this.cockpit = cockpit;
            this.corrosion = corrosion;
        }
    }
}
