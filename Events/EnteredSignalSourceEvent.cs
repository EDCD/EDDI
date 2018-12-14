using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EnteredSignalSourceEvent : Event
    {
        public const string NAME = "Entered signal source";
        public const string DESCRIPTION = "Triggered when your ship enters a signal source";
        public const string SAMPLE = "{ \"timestamp\":\"2018-11-04T19:35:33Z\", \"event\":\"USSDrop\", \"USSType\":\"$USS_Type_VeryValuableSalvage;\", \"USSType_Localised\":\"High grade emissions detected\", \"USSThreat\":0 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredSignalSourceEvent()
        {
            VARIABLES.Add("source", "The type of the signal source");
            VARIABLES.Add("localizedsource", "The type of the signal source (in your local language)");
            VARIABLES.Add("threat", "The threat level of the signal source (0 is lowest)");
        }

        public string source => signalSource.edname;
        public string localizedsource => signalSource.localizedName ?? signalSource.fallbackLocalizedName ?? signalSource.edname;

        [JsonProperty("threat")]
        public int threat{ get; private set; }

        // Not intended to be user facing
        public SignalSource signalSource { get; private set; }

        public EnteredSignalSourceEvent(DateTime timestamp, SignalSource source, int threat) : base(timestamp, NAME)
        {
            this.signalSource = source;
            this.threat = threat;
        }
    }
}
