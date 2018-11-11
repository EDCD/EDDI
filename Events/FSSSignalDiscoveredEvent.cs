using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FSSSignalDiscoveredEvent : Event
    {
        public const string NAME = "FSS signal";
        public const string DESCRIPTION = "Triggered when zooming in on a signal using the FSS scanner";
        public const string SAMPLE = "{ \"timestamp\":\"2018-11-11T01:30:54Z\", \"event\":\"FSSSignalDiscovered\", \"SignalName\":\"Fabian City\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FSSSignalDiscoveredEvent()
        {
            VARIABLES.Add("source", "The signal source");
            VARIABLES.Add("factionstate", "The faction state that triggered the signal source, if any");
            VARIABLES.Add("faction", "The faction affected by the signal source, if any");
            VARIABLES.Add("secondsremaining", "The time before the signal expires, in seconds");
        }

        public string source => signalSource.localizedName ?? signalSource.fallbackLocalizedName;
        public string factionstate => factionState.localizedName ?? factionState.fallbackLocalizedName;
        public string faction { get; private set; }
        public decimal? secondsremaining { get; private set; }

        // Not intended to be user facing
        public FssSignal signalSource { get; private set; }
        public FactionState factionState { get; private set; }

        public FSSSignalDiscoveredEvent(DateTime timestamp, FssSignal source, FactionState factionState, string faction, decimal? secondsRemaining) : base(timestamp, NAME)
        {
            this.signalSource = source;
            this.factionState = factionState;
            this.faction = faction;
            this.secondsremaining = secondsRemaining == null ? null : (decimal?)Math.Round((decimal)secondsRemaining);
        }
    }
}
