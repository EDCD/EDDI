using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary> Body Solid Composition </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BodySolidComposition
    {
        public BodyComposition composition { get; set; }
        public decimal percent { get; set; }

        public BodySolidComposition(BodyComposition composition, decimal percent)
        {
            this.composition = composition;
            this.percent = percent;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BodyComposition : ResourceBasedLocalizedEDName<BodyComposition>
    {
        static BodyComposition()
        {
            resourceManager = Properties.Body.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new BodyComposition(edname);

            var Ice = new BodyComposition("Ice");
            var Rock = new BodyComposition("Rock");
            var Metal = new BodyComposition("Metal");
        }

        // dummy used to ensure that the static constructor has run
        public BodyComposition() : this("")
        { }

        public BodyComposition(string edname) : base(edname, edname)
        { }
    }
}
