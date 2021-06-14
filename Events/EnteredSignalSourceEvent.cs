using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EnteredSignalSourceEvent : Event
    {
        public const string NAME = "Entered signal source";
        public const string DESCRIPTION = "Triggered when your ship enters a signal source";
        public const string SAMPLE = "{ \"timestamp\":\"2018-11-04T19:35:33Z\", \"event\":\"USSDrop\", \"USSType\":\"$USS_Type_VeryValuableSalvage;\", \"USSType_Localised\":\"High grade emissions detected\", \"USSThreat\":0 }";

        [PublicAPI("The type of the signal source")]
        public string source => signalSource.edname;

        [PublicAPI("The type of the signal source (in your local language)")]
        public string localizedsource => signalSource.localizedName ?? signalSource.fallbackLocalizedName ?? signalSource.edname;

        [PublicAPI("The threat level of the signal source (0 is lowest)")]
        public int threat { get; private set; }

        // Not intended to be user facing

        public SignalSource signalSource { get; private set; }

        public EnteredSignalSourceEvent(DateTime timestamp, SignalSource source, int threat) : base(timestamp, NAME)
        {
            this.signalSource = source;
            this.threat = threat;
        }
    }
}
