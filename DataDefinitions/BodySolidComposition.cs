using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary> Body Solid Composition </summary>
    public class SolidComposition : ResourceBasedLocalizedEDName<SolidComposition>
    {
        static SolidComposition()
        {
            resourceManager = Properties.Body.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new SolidComposition(edname, 0);

            var Ice = new SolidComposition("ice", 0);
            var Rock = new SolidComposition("rock", 0);
            var Metal = new SolidComposition("metal", 0);
        }

        [JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string composition => localizedName;
        [JsonIgnore]
        public string localizedComposition => localizedName;
        [JsonIgnore]
        public string invariantComposition => invariantName;

        [JsonProperty]
        public decimal percent { get; set; } // Percent share of the solid body

        // dummy used to ensure that the static constructor has run
        public SolidComposition() : this("", 0)
        { }

        public SolidComposition(string edComposition, decimal percent) : base(edComposition, edComposition)
        {
            this.percent = percent;
        }
    }
}
