using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class SynthesisedEvent : Event
    {
        public const string NAME = "Synthesised";
        public const string DESCRIPTION = "Triggered when you synthesise something from materials";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-21T14:17:32Z\", \"event\":\"Synthesis\", \"Name\":\"Ammo Basic\", \"Materials\":{ \"sulphur\":2, \"phosphorus\":1 } }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SynthesisedEvent()
        {
            VARIABLES.Add("synthesis", "The thing that has been synthesised");
            VARIABLES.Add("materials", "Materials used in the synthesis (as objects)");
        }

        [PublicAPI]
        public string synthesis { get; private set; }

        [PublicAPI]
        public List<MaterialAmount> materials { get; private set; }

        public SynthesisedEvent(DateTime timestamp, string synthesis, List<MaterialAmount> materials) : base(timestamp, NAME)
        {
            this.synthesis = synthesis;
            this.materials = materials;
        }
    }
}
