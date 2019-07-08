using Newtonsoft.Json;
using System;

namespace EddiDataDefinitions
{
    public class Vehicle
    {
        // Definition of the vehicle
        public string EDName { get; set; }
        public string loadout { get; set; }
        public int subslot { get; set; }
        public int rebuilds { get; set; }

        public string localizedName => Properties.Vehicle.ResourceManager.GetString(EDName);
        [JsonIgnore, Obsolete("Please use localizedName")]
        public string name => localizedName;

        public Vehicle() { }

        public Vehicle(Vehicle Vehicle)
        {
            this.EDName = Vehicle.EDName;
            this.loadout = Vehicle.loadout;
            this.subslot = Vehicle.subslot;
            this.rebuilds = Vehicle.rebuilds;
        }

        public Vehicle(string EDName, string Loadout, int SubSlot, int Rebuilds)
        {
            this.EDName = EDName;
            this.loadout = Loadout;
            this.subslot = SubSlot;
            this.rebuilds = Rebuilds;
        }
    }
}
