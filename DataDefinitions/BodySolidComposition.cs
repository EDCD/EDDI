using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary> Body Solid Composition </summary>
    public class BodySolidComposition
    {
        public string composition => Composition.localizedName;
        public BodyComposition Composition { get; set; }
        public decimal percent { get; set; }

        public BodySolidComposition(BodyComposition Composition, decimal percent)
        {
            this.Composition = Composition;
            this.percent = percent;
        }
    }

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
