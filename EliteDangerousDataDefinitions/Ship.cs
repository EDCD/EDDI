using System.Collections.Generic;

namespace EliteDangerousDataDefinitions
{
    /// <summary>A ship</summary>
    public class Ship
    {
        /// <summary>the ID of this ship for this commander</summary>
        public int LocalId { get; set; }
        /// <summary>the model of the ship (Python, Anaconda, etc.)</summary>
        public string Model { get; set; }
        /// <summary>the value of the ship without cargo, in credits</summary>
        public long Value { get; set; }
        /// <summary>the total tonnage cargo capacity</summary>
        public int CargoCapacity { get; set; }
        /// <summary>the current tonnage cargo carried</summary>
        public int CargoCarried { get; set; }

        /// <summary>the name of the system in which this ship is stored; null if the commander is in this ship</summary>
        public string StarSystem { get; set; }
        /// <summary>the name of the location in which this ship is stored; null if the commander is in this ship</summary>
        public string Location { get; set; }

        public decimal Health { get; set; }
        public Module Bulkheads { get; set; }
        public Module PowerPlant { get; set; }
        public Module Thrusters { get; set; }
        public Module FrameShiftDrive { get; set; }
        public Module LifeSupport { get; set; }
        public Module PowerDistributor { get; set; }
        public Module Sensors { get; set; }
        public Module FuelTank { get; set; }
        public List<Hardpoint> Hardpoints { get; set; }
        public List<Compartment> Compartments { get; set; }

        public Ship()
        {
            Hardpoints = new List<Hardpoint>();
            Compartments = new List<Compartment>();
        }
    }
}
