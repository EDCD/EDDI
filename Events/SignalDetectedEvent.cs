using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SignalDetectedEvent : Event
    {
        public const string NAME = "Signal detected";
        public const string DESCRIPTION = "Triggered when a signal source is detected";
        public const string SAMPLE = @"{ ""timestamp"":""2018-11-22T06:21:00Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":58132919110424, ""SignalName"":""$USS;"", ""SignalName_Localised"":""Unidentified signal source"", ""USSType"":""$USS_Type_Salvage;"", ""USSType_Localised"":""Degraded emissions"", ""SpawningState"":""$FactionState_None;"", ""SpawningState_Localised"":""None"", ""SpawningFaction"":""$faction_none;"", ""SpawningFaction_Localised"":""None"", ""ThreatLevel"":0, ""TimeRemaining"":1519.981689 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SignalDetectedEvent()
        {
            VARIABLES.Add("source", "The signal source");
            VARIABLES.Add("factionstate", "The faction state that triggered the signal source, if any");
            VARIABLES.Add("faction", "The faction affected by the signal source, if any");
            VARIABLES.Add("secondsremaining", "The time before the signal expires, in seconds");
            VARIABLES.Add("stationsignal", "True if the signal source is a station");
            VARIABLES.Add("threatlevel", "The risk posed by the signal source. Higher numbers are more dangerous.");
        }

        public string source => signalSource.localizedName ?? signalSource.fallbackLocalizedName ?? signalSource.edname;
        public string factionstate => factionState.localizedName ?? factionState.fallbackLocalizedName ?? factionState.edname;
        public string faction { get; private set; }
        public decimal? secondsremaining { get; private set; }

        public int threatlevel { get; private set; }
        public bool stationsignal { get; private set; }

        // Not intended to be user facing
        public SignalSource signalSource { get; private set; }
        public FactionState factionState { get; private set; }

        public SignalDetectedEvent(DateTime timestamp, SignalSource source, FactionState factionState, string faction, decimal? secondsRemaining, int? threatlevel, bool? isStation) : base(timestamp, NAME)
        {
            this.signalSource = source;
            this.factionState = factionState;
            this.faction = faction;
            this.secondsremaining = secondsRemaining == null ? null : (decimal?)Math.Round((decimal)secondsRemaining);
            this.threatlevel = (int)threatlevel;
            this.stationsignal = (bool)isStation;
        }
    }
}
