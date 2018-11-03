using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary> Atmosphere Composition </summary>
    public class AtmosphereComposition : ResourceBasedLocalizedEDName<AtmosphereComposition>
    {
        static AtmosphereComposition()
        {
            resourceManager = Properties.AtmosphereComposition.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new AtmosphereComposition(edname, 0);

            var Water = new AtmosphereComposition("water", 0);
            var Oxygen = new AtmosphereComposition("oxygen", 0);
            var CarbonDioxide = new AtmosphereComposition("carbondioxide", 0);
            var SulphurDioxide = new AtmosphereComposition("sulphurdioxide", 0);
            var Ammonia = new AtmosphereComposition("ammonia", 0);
            var Methane = new AtmosphereComposition("methane", 0);
            var Nitrogen = new AtmosphereComposition("nitrogen", 0);
            var Hydrogen = new AtmosphereComposition("hydrogen", 0);
            var Helium = new AtmosphereComposition("helium", 0);
            var Neon = new AtmosphereComposition("neon", 0);
            var Argon = new AtmosphereComposition("argon", 0);
            var Silicates = new AtmosphereComposition("silicates", 0);
            var Iron = new AtmosphereComposition("iron", 0);
        }

        [JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string composition => localizedName;
        [JsonIgnore]
        public string localizedComposition => localizedName;
        [JsonIgnore]
        public string invariantComposition => invariantName;

        [JsonProperty]
        public decimal percent { get; set; } // Percent share of the atmosphere

        // dummy used to ensure that the static constructor has run
        public AtmosphereComposition() : this("", 0)
        { }

        public AtmosphereComposition(string edComposition, decimal percent) : base(edComposition, edComposition)
        {
            this.percent = percent;
        }
    }
}
