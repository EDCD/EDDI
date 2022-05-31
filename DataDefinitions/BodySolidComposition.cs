using Newtonsoft.Json;
using System;
using Utilities;

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

            var Ice = new SolidComposition("ice");
            var Rock = new SolidComposition("rock");
            var Metal = new SolidComposition("metal");
        }

        [PublicAPI, JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string composition => localizedName;

        [PublicAPI, JsonProperty]
        public decimal percent { get; set; } // Percent share of the solid body

        // Not intended to be user facing

        
        [JsonIgnore]
        public string localizedComposition => localizedName;

        [JsonIgnore]
        public string invariantComposition => invariantName;
        
        // dummy used to ensure that the static constructor has run
        public SolidComposition() : this("")
        { }

        [JsonConstructor]
        public SolidComposition(string edComposition, decimal percent = 0) : base(edComposition, edComposition)
        {
            this.percent = percent;
        }
    }
}
