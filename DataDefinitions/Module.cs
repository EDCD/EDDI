namespace EddiDataDefinitions
{
    public class Module
    {
        // Definition of the module
        public string name { get; set; }
        public int @class { get; set; }
        public string grade { get; set; }
        public long value { get; set; } // The undiscounted value
        // Additional definition for some items
        public int? ShipId { get; set; } // Only for bulkheads
        public ModuleMount? mount { get; set; } // Only for weapons
        public int? clipcapacity { get; set; } // Only for weapons
        public int? hoppercapacity { get; set; } // Only for weapons

        // State of the module
        public long price { get; set; } // How much we actually paid for it
        public bool enabled { get; set; }
        public int priority { get; set; }
        public decimal health { get; set; }
        public bool modified { get; set; } // If the module has been modified

        // Admin
        // The ID in Elite: Dangerous' database
        public long EDID { get; set; }
        // The name in Elite: Dangerous' database
        public string EDName { get; set; }
        // The ID in eddb.io
        public long EDDBID { get; set; }

        public Module() { }

        public Module(Module Module)
        {
            this.EDName = Module.EDName;
            this.name = Module.name;
            this.@class = Module.@class;
            this.grade = Module.grade;
            this.value = Module.value;
            this.ShipId = Module.ShipId;
            this.mount = Module.mount;
            this.clipcapacity = Module.clipcapacity;
            this.hoppercapacity = Module.hoppercapacity;
            this.EDID = Module.EDID;
            this.EDDBID = Module.EDDBID;
            this.modified = Module.modified;
        }

        public Module(long EDID, string EDName, long EDDBID, string Name, int Class, string Grade, long Value)
        {
            this.EDID = EDID;
            this.EDName = EDName;
            this.EDDBID = EDDBID;
            this.name = Name;
            this.@class = Class;
            this.grade = Grade;
            this.value = Value;
            this.modified = false;
        }

        // Module definition for a bulkhead - requires ship ID
        public Module(long EDID, string EDName, long EDDBID, string Name, int Class, string Grade, long Value, int ShipId)
        {
            this.EDID = EDID;
            this.EDName = EDName;
            this.EDDBID = EDDBID;
            this.name = Name;
            this.@class = Class;
            this.grade = Grade;
            this.value = Value;
            this.ShipId = ShipId;
            this.modified = false;
        }

        // Module definition for a weapon - requires mount and optional ammo
        public Module(long EDID, string EDName, long EDDBID, string Name, int Class, string Grade, long Value, ModuleMount Mount, int? AmmoClipCapacity = null, int? AmmoHopperCapacity = null)
        {
            this.EDID = EDID;
            this.EDName = EDName;
            this.EDDBID = EDDBID;
            this.name = Name;
            this.@class = Class;
            this.grade = Grade;
            this.value = Value;
            this.mount = Mount;
            this.clipcapacity = AmmoClipCapacity;
            this.hoppercapacity = AmmoHopperCapacity;
            this.modified = false;
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
