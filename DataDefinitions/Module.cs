using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public partial class Module : ResourceBasedLocalizedEDName<Module>
    {
        // Definition of the module

        [PublicAPI, JsonProperty]
        public int @class { get; set; }

        [PublicAPI, JsonProperty]
        public string grade { get; set; }

        [PublicAPI, JsonProperty]
        public long value { get; set; } // The undiscounted value

        // Additional definition for some items

        ///<summary>The localized name of the weapon mount</summary>
        [PublicAPI, JsonIgnore]
        public string mount => Mount != null ? Properties.Modules.ResourceManager.GetString(Mount.ToString()) : "";

        [PublicAPI, JsonProperty]
        public int? clipcapacity { get; set; } // Only for weapons

        [PublicAPI, JsonProperty]
        public int? hoppercapacity { get; set; } // Only for weapons

        [PublicAPI, JsonProperty]
        public int? ammoinclip { get; set; } // Only for weapons

        [PublicAPI, JsonProperty]
        public int? ammoinhopper { get; set; } // Only for weapons

        // State of the module

        [PublicAPI, JsonProperty]
        public long price { get; set; } // How much we actually paid for it

        [PublicAPI, JsonProperty]
        public bool enabled { get; set; }

        [PublicAPI, JsonProperty]
        public int priority { get; set; }

        [PublicAPI, JsonProperty]
        public int position { get; set; }

        [PublicAPI, JsonProperty]
        public decimal power { get; set; }

        [PublicAPI, JsonProperty]
        public decimal health { get; set; } = 100M;

        [PublicAPI, JsonProperty]
        public bool hot { get; set; } // False = `clean', true = `hot`

        // Engineering modification properties
        [PublicAPI, JsonProperty]
        public bool modified { get; set; } // If the module has been modified

        [PublicAPI, JsonProperty]
        public int engineerlevel { get; set; }

        [PublicAPI, JsonProperty]
        public decimal engineerquality { get; set; }

        // deprecated commodity category (exposed to Cottle and VA)
        [PublicAPI, JsonIgnore, Obsolete("Please use localizedModification instead")]
        public string modification => localizedModification;

        // Not intended to be user facing

        [JsonProperty("mount")]
        public ModuleMount Mount { get; set; } // Only for weapons

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
        public string engineerExperimentalEffectEDName { get; set; }

        [JsonProperty]
        public List<EngineeringModifier> modifiers { get; set; } = [ ];

        [JsonIgnore]
        public string localizedModification => engineermodification?.localizedName;

        public Module() : base("", "")
        { }

        public Module(Module Module) : base(Module.edname, Module.basename)
        {
            this.@class = Module.@class;
            this.grade = Module.grade;
            this.value = Module.value;
            this.Mount = Module.Mount;
            this.clipcapacity = Module.clipcapacity;
            this.hoppercapacity = Module.hoppercapacity;
            this.ammoinclip = Module.ammoinclip;
            this.ammoinhopper = Module.ammoinhopper;
            this.enabled = Module.enabled;
            this.power = Module.power;
            this.priority = Module.priority;
            this.position = Module.position;
            this.modified = Module.modified;
            this.engineermodification = Module.engineermodification;
            this.engineerlevel = Module.engineerlevel;
            this.engineerquality = Module.engineerquality;
            this.engineerExperimentalEffectEDName = Module.engineerExperimentalEffectEDName;
            this.modifiers = Module.modifiers;
        }

        public Module ( string edname, string basename, int Class, string Grade, long Value ) : base(edname, basename)
        {
            this.@class = Class;
            this.grade = Grade;
            this.value = Value;
            this.modified = false;
        }

        // Module definition for a weapon - requires mount and optional ammo
        public Module ( string edname, string basename, int Class, string Grade, long Value, ModuleMount Mount,
            int? AmmoClipCapacity = null, int? AmmoHopperCapacity = null ) : base(edname, basename)
        {
            this.@class = Class;
            this.grade = Grade;
            this.value = Value;
            this.Mount = Mount;
            this.clipcapacity = AmmoClipCapacity;
            this.hoppercapacity = AmmoHopperCapacity;
            this.modified = false;
        }

        public void UpdateFromFrontierAPIModule(Module frontierAPIModule)
        {
            if (edname == frontierAPIModule.edname)
            {
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
                modifiers = modified ? modifiers : [];
            }
        }

        public double GetFsdPowerConstant () // Power Multiplier
        {
            if ( @class == 0 ) { return 1; }

            if ( edname?.Contains( "OverchargeBooster_Mkii", StringComparison.OrdinalIgnoreCase ) ?? false )
            {
                var powerConstantFSD_MkII = new Dictionary<int, double>
                {
                    { 8, 2.5025 }
                };
                return powerConstantFSD_MkII.TryGetValue( @class, out var result )
                    ? result
                    : 1;
            }
            else
            {
                var powerConstantFSD = new Dictionary<int, double>
                {
                    { 2, 2.00 }, { 3, 2.15 }, { 4, 2.30 }, { 5, 2.45 }, { 6, 2.60 }, { 7, 2.75 }, { 8, 2.90 }
                };
                return powerConstantFSD.TryGetValue( @class, out var result )
                    ? result
                    : 1;
            }
        }

        public double GetFsdRatingConstant () // Fuel Multiplier
        {
            if ( string.IsNullOrEmpty(grade) ) { return 0; }

            if ( edname?.Contains( "OverchargeBooster_Mkii", StringComparison.OrdinalIgnoreCase ) ?? false )
            {
                var ratingConstants_SCO_MkII = new Dictionary<string, double>
                {
                    { "A", 11.0 }
                };
                return ratingConstants_SCO_MkII.TryGetValue( grade, out var rating )
                    ? rating
                    : ratingConstants_SCO_MkII.First().Value;
            }
            else if ( edname?.Contains( "hyperdrive_overcharge" ) ?? false )
            {
                var ratingConstants_SCO = new Dictionary<string, double>
                {
                    { "E", 8.0 }, { "D", 12.0 }, { "C", 12.0 }, { "B", 12.0 }, { "A", 13.0 }
                };
                return ratingConstants_SCO.TryGetValue( grade, out var rating ) 
                    ? rating 
                    : ratingConstants_SCO.First().Value;
            }
            else
            {
                var ratingConstants = new Dictionary<string, double>
                {
                    { "E", 11.0 }, { "D", 10.0 }, { "C", 8.0 }, { "B", 10.0 }, { "A", 12.0 },
                };
                return ratingConstants.TryGetValue( grade, out var rating ) 
                    ? rating 
                    : ratingConstants.First().Value;
            }
        }

        public decimal GetFsdMaxFuelPerJump ()
        {
            if ( string.IsNullOrEmpty( grade ) || @class == 0 ) { return 0; }

            // Use modified value if a modified value exists
            var maxFuelPerJump = modifiers?.FirstOrDefault(m => 
                m.EDName.Equals("MaxFuelPerJump", StringComparison.InvariantCultureIgnoreCase))?.currentValue;
            if ( maxFuelPerJump != null )
            {
                return (decimal) maxFuelPerJump;
            }

            // No modified value exists, use a base value
            decimal baseMaxFuelPerJump;
            if ( edname?.Contains( "OverchargeBooster_Mkii", StringComparison.OrdinalIgnoreCase ) ?? false )
            {
                var baseMaxFuelsPerJump_SCO_MkII = new Dictionary<string, decimal>
                {
                    { "8A", 6.8M }
                };
                baseMaxFuelsPerJump_SCO_MkII.TryGetValue( @class + grade, out baseMaxFuelPerJump );
            }
            else if ( edname?.Contains( "hyperdrive_overcharge" ) ?? false )
            {
                var baseMaxFuelsPerJump_SCO = new Dictionary<string, decimal>
                {
                    { "2E", 0.60M }, { "2D", 0.90M }, { "2C", 0.90M }, { "2B", 0.90M }, { "2A", 1.00M },
                    { "3E", 1.20M }, { "3D", 1.80M }, { "3C", 1.80M }, { "3B", 1.80M }, { "3A", 1.90M },
                    { "4E", 2.00M }, { "4D", 3.00M }, { "4C", 3.00M }, { "4B", 3.00M }, { "4A", 3.20M },
                    { "5E", 3.30M }, { "5D", 5.00M }, { "5C", 5.00M }, { "5B", 5.00M }, { "5A", 5.20M },
                    { "6E", 5.30M }, { "6D", 8.00M }, { "6C", 8.00M }, { "6B", 8.00M }, { "6A", 8.30M },
                    { "7E", 8.50M }, { "7D", 12.8M }, { "7C", 12.8M }, { "7B", 12.8M }, { "7A", 13.1M },
                    { "8E", 13.8M }, { "8D", 20.4M }, { "8C", 20.4M }, { "8B", 20.4M }, { "8A", 20.7M }

                };
                baseMaxFuelsPerJump_SCO.TryGetValue( @class + grade, out baseMaxFuelPerJump );
            }
            else
            {
                var baseMaxFuelsPerJump = new Dictionary<string, decimal>
                {
                    { "2E", 0.60M }, { "2D", 0.60M }, { "2C", 0.60M }, { "2B", 0.80M }, { "2A", 0.90M },
                    { "3E", 1.20M }, { "3D", 1.20M }, { "3C", 1.20M }, { "3B", 1.50M }, { "3A", 1.80M },
                    { "4E", 2.00M }, { "4D", 2.00M }, { "4C", 2.00M }, { "4B", 2.50M }, { "4A", 3.00M },
                    { "5E", 3.30M }, { "5D", 3.30M }, { "5C", 3.30M }, { "5B", 4.10M }, { "5A", 5.00M },
                    { "6E", 5.30M }, { "6D", 5.30M }, { "6C", 5.30M }, { "6B", 6.60M }, { "6A", 8.00M },
                    { "7E", 8.50M }, { "7D", 8.50M }, { "7C", 8.50M }, { "7B", 10.6M }, { "7A", 12.8M },
                    { "8E", 13.6M }, { "8D", 13.6M }, { "8C", 13.6M }, { "8B", 17.0M }, { "8A", 20.4M }
                };
                baseMaxFuelsPerJump.TryGetValue( @class + grade, out baseMaxFuelPerJump );
            }
            // 0 indicates no result, the `Ship` class will recalculate if the current value is 0.
            return baseMaxFuelPerJump;
        }

        /// <summary>
        /// Gets the optimal mass of this FSD.
        /// </summary>
        /// <returns>The base or modified (if modifiers are set) optimal mass of this FSD</returns>
        public double GetFsdOptimalMass ()
        {
            if ( string.IsNullOrEmpty( grade ) || @class == 0 ) { return 0; }

            // Use modified value if a modified value exists
            var optimalMass = modifiers?.FirstOrDefault( m =>
                m.EDName.Equals( "FSDOptimalMass", StringComparison.InvariantCultureIgnoreCase ) )?.currentValue;
            if ( optimalMass != null )
            {
                return (double)optimalMass;
            }

            // No modified value exists, use a base value
            return GetFsdBaseOptimalMass();
        }

        /// <summary>
        /// Gets the base optimal mass of this FSD
        /// </summary>
        /// <returns></returns>
        public double GetFsdBaseOptimalMass ()
        {
            double baseOptimalMass;

            if ( edname?.Contains( "OverchargeBooster_Mkii", StringComparison.OrdinalIgnoreCase ) ?? false )
            {
                var baseOptimalMasses_SCO_MkII = new Dictionary<string, double>
                {
                    { "8A", 4670.0 }
                };
                baseOptimalMasses_SCO_MkII.TryGetValue( @class + grade, out baseOptimalMass );
            }
            else if ( edname?.Contains( "hyperdrive_overcharge", StringComparison.OrdinalIgnoreCase ) ?? false )
            {
                var baseOptimalMasses_SCO = new Dictionary<string, double>
                {
                    { "2E", 60.000 }, { "2D", 90.000 }, { "2C", 90.000 }, { "2B", 90.000 }, { "2A", 100.00 },
                    { "3E", 100.00 }, { "3D", 150.00 }, { "3C", 150.00 }, { "3B", 150.00 }, { "3A", 167.00 },
                    { "4E", 350.00 }, { "4D", 525.00 }, { "4C", 525.00 }, { "4B", 525.00 }, { "4A", 585.00 },
                    { "5E", 700.00 }, { "5D", 1050.0 }, { "5C", 1050.0 }, { "5B", 1050.0 }, { "5A", 1175.0 },
                    { "6E", 1200.0 }, { "6D", 1800.0 }, { "6C", 1800.0 }, { "6B", 1800.0 }, { "6A", 2000.0 },
                    { "7E", 1800.0 }, { "7D", 2700.0 }, { "7C", 2700.0 }, { "7B", 2700.0 }, { "7A", 3000.0 },
                    { "8E", 1800.0 }, { "8D", 2700.0 }, { "8C", 2700.0 }, { "8B", 2700.0 }, { "8A", 3000.0 }
                };
                baseOptimalMasses_SCO.TryGetValue( @class + grade, out baseOptimalMass );
            }
            else
            {
                var baseOptimalMasses = new Dictionary<string, double>
                {
                    { "2E", 48.000 }, { "2D", 54.000 }, { "2C", 60.000 }, { "2B", 75.000 }, { "2A", 90.000 },
                    { "3E", 80.000 }, { "3D", 90.000 }, { "3C", 100.00 }, { "3B", 125.00 }, { "3A", 150.00 },
                    { "4E", 280.00 }, { "4D", 315.00 }, { "4C", 350.00 }, { "4B", 438.00 }, { "4A", 525.00 },
                    { "5E", 560.00 }, { "5D", 630.00 }, { "5C", 700.00 }, { "5B", 875.00 }, { "5A", 1050.0 },
                    { "6E", 960.00 }, { "6D", 1080.0 }, { "6C", 1200.0 }, { "6B", 1500.0 }, { "6A", 1800.0 },
                    { "7E", 1440.0 }, { "7D", 1620.0 }, { "7C", 1800.0 }, { "7B", 2250.0 }, { "7A", 2700.0 },
                    { "8E", 2800.0 }, { "8D", 4200.0 }, { "8C", 4200.0 }, { "8B", 4200.0 }, { "8A", 4670.0 }
                };
                baseOptimalMasses.TryGetValue( @class + grade, out baseOptimalMass );
            }
            // 0 indicates no result, the `Ship` class will recalculate if the current value is 0.
            return baseOptimalMass;
        }

        public double GetGuardianFSDBoost ()
        {
            if ( @class == 0 ) { return 0; }

            var guardianBoostFSD_LY = new Dictionary<int, double>()
            {
                {1, 4.00 }, {2, 6.00 }, {3, 7.75}, {4, 9.25}, {5, 10.50}
            };
            return guardianBoostFSD_LY.TryGetValue( @class, out var result )
                ? result
                : 0;
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
