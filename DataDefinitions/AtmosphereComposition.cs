using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary> Atmosphere Composition </summary>
    [ method: JsonConstructor]
    public class AtmosphereComposition ( string edComposition, decimal percent = 0 )
        : ResourceBasedLocalizedEDName<AtmosphereComposition>( edComposition, edComposition )
    {
        static AtmosphereComposition()
        {
            resourceManager = Properties.AtmosphereComposition.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new AtmosphereComposition(edname, 0);

            var Water = new AtmosphereComposition("water");
            var Oxygen = new AtmosphereComposition("oxygen");
            var CarbonDioxide = new AtmosphereComposition("carbondioxide");
            var SulphurDioxide = new AtmosphereComposition("sulphurdioxide");
            var Ammonia = new AtmosphereComposition("ammonia");
            var Methane = new AtmosphereComposition("methane");
            var Nitrogen = new AtmosphereComposition("nitrogen");
            var Hydrogen = new AtmosphereComposition("hydrogen");
            var Helium = new AtmosphereComposition("helium");
            var Neon = new AtmosphereComposition("neon");
            var Argon = new AtmosphereComposition("argon");
            var Silicates = new AtmosphereComposition("silicates");
            var Iron = new AtmosphereComposition("iron");
        }

        [PublicAPI( "an element in the composition of the atmosphere (Ammonia, Nitrogen, Oxygen, Hydrogen, etc.)" ), JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string composition => localizedName;
        
        [PublicAPI( "percent share of the element in the atmosphere" ), JsonProperty]
        public decimal percent { get; set; } = percent;

        // Not intended to be user facing

        [JsonIgnore]
        public string localizedComposition => localizedName;

        [JsonIgnore]
        public string invariantComposition => invariantName;

        // dummy used to ensure that the static constructor has run
        public AtmosphereComposition() : this("")
        { }
    }
}
