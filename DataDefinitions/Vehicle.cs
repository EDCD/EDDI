namespace EddiDataDefinitions
{
    public class Vehicle
    {
        // Definition of the vehicle
        public string EDName { get; set; }
        public string name { get; set; }
        public string loadout { get; set; }
        public string  mount{ get; set; }
        public int subslot { get; set; }
        public int rebuilds { get; set; }

        public Vehicle() { }

        public Vehicle(Vehicle Vehicle)
        {
            this.EDName = Vehicle.EDName;
            this.name = Vehicle.name;
            this.loadout = Vehicle.loadout;
            this.mount = Vehicle.mount;
            this.subslot = Vehicle.subslot;
            this.rebuilds = Vehicle.rebuilds;
        }

        public Vehicle(string EDName, string Name, string Loadout, string Mount, int SubSlot, int Rebuilds)
        {
            this.EDName = EDName;
            this.name = Name;
            this.loadout = Loadout;
            this.mount = Mount;
            this.subslot = SubSlot;
            this.rebuilds = Rebuilds;
        }
    }
}
