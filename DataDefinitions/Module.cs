using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public partial class Module : ResourceBasedLocalizedEDName<Module>
    {
        public enum ModuleMount
        {
            Fixed,
            Gimballed,
            Turreted
        }

        // Definition of the module
        [JsonProperty]
        public int @class { get; set; }
        [JsonProperty]
        public string grade { get; set; }
        [JsonProperty]
        public long value { get; set; } // The undiscounted value

        // Additional definition for some items
        [JsonProperty]
        public int? ShipId { get; set; } // Only for bulkheads
        [JsonProperty("mount")]
        public ModuleMount? Mount { get; set; } // Only for weapons
        ///<summary>The localized name of the weapon mount</summary>
        [JsonIgnore]
        public string mount => Mount != null ? Properties.Modules.ResourceManager.GetString(Mount.ToString()) : "";
        [JsonProperty]
        public int? clipcapacity { get; set; } // Only for weapons
        [JsonProperty]
        public int? hoppercapacity { get; set; } // Only for weapons
        [JsonProperty]
        public int? ammoinclip { get; set; } // Only for weapons
        [JsonProperty]
        public int? ammoinhopper { get; set; } // Only for weapons

        // State of the module
        [JsonProperty]
        public long price { get; set; } // How much we actually paid for it
        [JsonProperty]
        public bool enabled { get; set; }
        [JsonProperty]
        public int priority { get; set; }
        [JsonProperty]
        public int position { get; set; }
        [JsonProperty]
        public decimal power { get; set; }
        [JsonProperty]
        public decimal health { get; set; } = 100M;
        [JsonProperty]
        public bool hot { get; set; } // False = `clean', true = `hot`

        // Engineering modification properties
        [JsonProperty]
        public bool modified { get; set; } // If the module has been modified
        [JsonProperty]
        public string modificationEDName { get; set; }
        [JsonIgnore]
        public Blueprint engineermodification
        {
            get
            {
                return engineerModification ?? Blueprint.FromEliteID(blueprintId) ?? Blueprint.FromEDNameAndGrade(modificationEDName, engineerlevel);
            }
            set
            {
                engineerModification = value;
            }
        }
        [JsonIgnore]
        private Blueprint engineerModification;
        [JsonProperty]
        public long blueprintId { get; set; }
        [JsonProperty]
        public int engineerlevel { get; set; }
        [JsonProperty]
        public decimal engineerquality { get; set; }
        [JsonProperty]
        public string engineerExperimentalEffectEDName { get; set; }
        [JsonProperty]
        public List<EngineeringModifier> modifiers { get; set; } = new List<EngineeringModifier>();
        [JsonIgnore]
        public string localizedModification => engineermodification?.localizedName ?? null;

        // deprecated commodity category (exposed to Cottle and VA)
        [JsonIgnore, Obsolete("Please use localizedModification instead")]
        public string modification => localizedModification;

        // Admin
        // The ID in Elite: Dangerous' database
        [JsonProperty]
        public long EDID { get; set; }
        // The ID in eddb.io
        [JsonProperty]
        public long EDDBID { get; set; }

        [JsonIgnore]
        public string EDName { get => edname; }


        public Module() : base("", "")
        { }

        public Module(Module Module) : base(Module.edname, Module.basename)
        {
            this.@class = Module.@class;
            this.grade = Module.grade;
            this.value = Module.value;
            this.ShipId = Module.ShipId;
            this.Mount = Module.Mount;
            this.clipcapacity = Module.clipcapacity;
            this.hoppercapacity = Module.hoppercapacity;
            this.ammoinclip = Module.ammoinclip;
            this.ammoinhopper = Module.ammoinhopper;
            this.enabled = Module.enabled;
            this.power = Module.power;
            this.priority = Module.priority;
            this.position = Module.position;
            this.EDID = Module.EDID;
            this.EDDBID = Module.EDDBID;
            this.modified = Module.modified;
            this.engineermodification = Module.engineermodification;
            this.engineerlevel = Module.engineerlevel;
            this.engineerquality = Module.engineerquality;
            this.engineerExperimentalEffectEDName = Module.engineerExperimentalEffectEDName;
            this.modifiers = Module.modifiers;
        }

        public Module(long EDID, string edname, long EDDBID, string basename, int Class, string Grade, long Value) : base(edname, basename)
        {
            this.EDID = EDID;
            this.EDDBID = EDDBID;
            this.@class = Class;
            this.grade = Grade;
            this.value = Value;
            this.modified = false;
            ModulesByEliteID[EDID] = this;
        }

        // Module definition for a bulkhead - requires ship ID
        public Module(long EDID, string edname, long EDDBID, string basename, int Class, string Grade, long Value, int ShipId) : base(edname, basename)
        {
            this.EDID = EDID;
            this.EDDBID = EDDBID;
            this.@class = Class;
            this.grade = Grade;
            this.value = Value;
            this.ShipId = ShipId;
            this.modified = false;
            ModulesByEliteID[EDID] = this;
        }

        // Module definition for a weapon - requires mount and optional ammo
        public Module(long EDID, string edname, long EDDBID, string basename, int Class, string Grade, long Value, ModuleMount Mount, int? AmmoClipCapacity = null, int? AmmoHopperCapacity = null) : base(edname, basename)
        {
            this.EDID = EDID;
            this.EDDBID = EDDBID;
            this.@class = Class;
            this.grade = Grade;
            this.value = Value;
            this.Mount = Mount;
            this.clipcapacity = AmmoClipCapacity;
            this.hoppercapacity = AmmoHopperCapacity;
            this.modified = false;
            ModulesByEliteID[EDID] = this;
        }

        public void UpdateFromFrontierAPIModule(Module frontierAPIModule)
        {
            if (edname == frontierAPIModule.edname)
            {
                EDID = frontierAPIModule.EDID;
                fallbackLocalizedName = frontierAPIModule.fallbackLocalizedName;
                price = frontierAPIModule.price;
                enabled = frontierAPIModule.enabled;
                priority = frontierAPIModule.priority;
                health = frontierAPIModule.health;
                modified = frontierAPIModule.modified;
                modificationEDName = frontierAPIModule.modificationEDName;
                engineerlevel = frontierAPIModule.engineerlevel;
                engineermodification = frontierAPIModule.engineermodification;
                blueprintId = frontierAPIModule.blueprintId;
                engineerExperimentalEffectEDName = frontierAPIModule.engineerExperimentalEffectEDName;
                // Details of modifications are not presented the same in `Loadout` events and in the Frontier API,
                // so we do not update engineered modifiers from the Frontier API data
                modifiers = modified ? modifiers : new List<EngineeringModifier>();
            }
        }
    }

    public class EngineeringModifier
    {
        public string EDName { get; set; }
        public decimal? currentValue { get; set; }
        public decimal? originalValue { get; set; }
        public bool lessIsGood { get; set; }
        public string valueStr { get; set; }
    }
}
