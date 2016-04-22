using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousDataDefinitions
{
    /// <summary>A ship</summary>
    public class Ship
    {
        /// <summary>the ID of this ship for this commander</summary>
        public int LocalId { get; set; }
        /// <summary>the model of the ship (Python, Anaconda, etc.)</summary>
        [JsonIgnore]
        public string Model { get; set; }
        /// <summary>the size of this ship</summary>
        [JsonIgnore]
        public ShipSize Size { get; set; }
        /// <summary>the value of the ship without cargo, in credits</summary>
        [JsonIgnore]
        public long Value { get; set; }
        /// <summary>the total tonnage cargo capacity</summary>
        [JsonIgnore]
        public int CargoCapacity { get; set; }
        /// <summary>the current tonnage cargo carried</summary>
        [JsonIgnore]
        public int CargoCarried { get; set; }

        /// <summary>the specific cargo carried</summary>
        public List<Cargo> Cargo { get; set; }

        /// <summary>the callsign of this ship</summary>
        [JsonProperty("callSign")]
        public string CallSign { get; set;  }
        /// <summary>the name of this ship</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>the phonetic name of this ship</summary>
        [JsonProperty("phoneticName")]
        public string PhoneticName { get; set; }
        /// <summary>the role of this ship</summary>
        [JsonProperty("role")]
        public ShipRole Role { get; set; }

        /// <summary>the name of the system in which this ship is stored; null if the commander is in this ship</summary>
        [JsonIgnore]
        public string StarSystem { get; set; }
        /// <summary>the name of the station in which this ship is stored; null if the commander is in this ship</summary>
        [JsonIgnore]
        public string Station { get; set; }

        [JsonIgnore]
        public decimal Health { get; set; }
        [JsonIgnore]
        public Module Bulkheads { get; set; }
        [JsonIgnore]
        public Module PowerPlant { get; set; }
        [JsonIgnore]
        public Module Thrusters { get; set; }
        [JsonIgnore]
        public Module FrameShiftDrive { get; set; }
        [JsonIgnore]
        public Module LifeSupport { get; set; }
        [JsonIgnore]
        public Module PowerDistributor { get; set; }
        [JsonIgnore]
        public Module Sensors { get; set; }
        [JsonIgnore]
        public Module FuelTank { get; set; }
        [JsonIgnore]
        public decimal FuelTankCapacity { get; set; }
        [JsonIgnore]
        public List<Hardpoint> Hardpoints { get; set; }
        [JsonIgnore]
        public List<Compartment> Compartments { get; set; }

        // Admin
        // The ID in Elite: Dangerous' database
        public long EDID { get; set; }

        public Ship()
        {
            Hardpoints = new List<Hardpoint>();
            Compartments = new List<Compartment>();
        }

        public Ship(long EDID, string Model, ShipSize Size)
        {
            this.EDID = EDID;
            this.Model = Model;
            this.Size = Size;
            Hardpoints = new List<Hardpoint>();
            Compartments = new List<Compartment>();
        }

        private static Random random = new Random();
        /// <summary>
        /// A callsign is a set of three letters followed by a dash and then four numbers
        /// </summary>
        public static string generateCallsign()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numerics = "0123456789";
            return new string(Enumerable.Repeat(chars, 3)
              .Select(s => s[random.Next(s.Length)]).ToArray())
              + "-"
              + new string(Enumerable.Repeat(numerics, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    /// <summary>The role of a ship</summary>
    public enum ShipRole
    {
        Multipurpose,
        Exploring,
        Trading,
        Mining,
        Smuggling,
        Piracy,
        BountyHunting,
        Combat
    }

    /// <summary>The size of a ship</summary>
    public enum ShipSize
    {
        Small,
        Medium,
        Large,
        Huge
    }
}
