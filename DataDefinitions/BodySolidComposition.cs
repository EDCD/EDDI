using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary> Body Solid Composition </summary>
    public class BodySolidComposition : ResourceBasedLocalizedEDName<BodySolidComposition>
    {
        static BodySolidComposition()
        {
            resourceManager = Properties.Body.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new BodySolidComposition(edname, 0);

            var Ice = new BodySolidComposition("ice", 0);
            var Rock = new BodySolidComposition("rock", 0);
            var Metal = new BodySolidComposition("metal", 0);
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
        public BodySolidComposition() : this("", 0)
        { }

        public BodySolidComposition(string edComposition, decimal percent) : base(edComposition, edComposition)
        {
            this.percent = percent;
        }
    }
}
