namespace EliteDangerousDataDefinitions
{
    public class Module
    {
        // Definition of the module
        public string Name { get; set; }
        public int Class { get; set; }
        public string Grade { get; set; }
        public long Value { get; set; } // The undiscounted value
        // Additional definition for some items
        public int? ShipId { get; set; } // Only for bulkheads
        public ModuleMount? Mount { get; set; } // Only for weapons
        public int? AmmoClipCapacity { get; set; } // Only for weapons
        public int? AmmoHopperCapacity { get; set; } // Only for weapons

        // State of the module
        public long Cost { get; set; } // How much we actually paid for it
        public bool Enabled { get; set; }
        public int Priority { get; set; }
        public decimal Health { get; set; }

        // Admin
        // The ID in Elite: Dangerous' database
        public long EDID { get; set; }
        // The ID in eddb.io
        public long EDDBID { get; set; }

        public Module() { }

        public Module(Module Module)
        {
            this.Name = Module.Name;
            this.Class = Module.Class;
            this.Grade = Module.Grade;
            this.Value = Module.Value;
            this.ShipId = Module.ShipId;
            this.Mount = Module.Mount;
            this.AmmoClipCapacity = Module.AmmoClipCapacity;
            this.AmmoHopperCapacity = Module.AmmoHopperCapacity;
            this.EDID = Module.EDID;
            this.EDDBID = Module.EDDBID;
        }

        public Module(long EDID, long EDDBID, string Name, int Class, string Grade, long Value)
        {
            this.EDID = EDDBID;
            this.EDDBID = EDDBID;
            this.Name = Name;
            this.Class = Class;
            this.Grade = Grade;
            this.Value = Value;
        }

        // Module definition for a bulkhead - requires ship ID
        public Module(long EDID, long EDDBID, string Name, int Class, string Grade, long Value, int ShipId)
        {
            this.EDID = EDID;
            this.EDDBID = EDDBID;
            this.Name = Name;
            this.Class = Class;
            this.Grade = Grade;
            this.Value = Value;
            this.ShipId = ShipId;
        }

        // Module definition for a weapon - requires mount and optional ammo
        public Module(long EDID, long EDDBID, string Name, int Class, string Grade, long Value, ModuleMount Mount, int? AmmoClipCapacity = null, int? AmmoHopperCapacity = null)
        {
            this.EDID = EDID;
            this.EDDBID = EDDBID;
            this.Name = Name;
            this.Class = Class;
            this.Grade = Grade;
            this.Value = Value;
            this.Mount = Mount;
            this.AmmoClipCapacity = AmmoClipCapacity;
            this.AmmoHopperCapacity = AmmoHopperCapacity;
        }

        /// <summary>The mount of a weapons module</summary>
        public enum ModuleMount
        {
            Fixed,
            Gimballed,
            Turreted
        }
    }
}
