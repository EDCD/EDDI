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
            VARIABLES.Add("unique", "True if this is the first signal of this type detected within the star system");
        }

        public string source => signalSource.localizedName;
        public string factionstate => signalSource.spawningState.localizedName ?? signalSource.spawningState.fallbackLocalizedName ?? signalSource.spawningState.edname;
        public string faction => signalSource.spawningFaction;
        public decimal? secondsremaining => signalSource.expiry is null ? null : (decimal?)((DateTime)signalSource.expiry - timestamp).TotalSeconds;
        public int threatlevel => Convert.ToInt32(signalSource.threatLevel);
        public bool stationsignal => Convert.ToBoolean(signalSource.isStation);
        public bool unique { get; }

        // Not intended to be user facing
        public SignalSource signalSource { get; private set; }
        public long? systemAddress { get; private set; } // Caution: scan events from the destination system can register after StartJump and before we actually leave the originating system

        public SignalDetectedEvent(DateTime timestamp, long? systemAddress, SignalSource source, bool unique) : base(timestamp, NAME)
        {
            this.systemAddress = systemAddress;
            this.signalSource = source;
            this.unique = unique;
        }
    }
}
