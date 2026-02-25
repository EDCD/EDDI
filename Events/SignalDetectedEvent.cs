using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SignalDetectedEvent : Event
    {
        public const string NAME = "Signal detected";
        public const string DESCRIPTION = "Triggered when a signal source is detected";

        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"":""2018-11-22T06:21:00Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":58132919110424, ""SignalName"":""$USS;"", ""SignalName_Localised"":""Unidentified signal source"", ""USSType"":""$USS_Type_Salvage;"", ""USSType_Localised"":""Degraded emissions"", ""SpawningState"":""$FactionState_None;"", ""SpawningState_Localised"":""None"", ""SpawningFaction"":""$faction_none;"", ""SpawningFaction_Localised"":""None"", ""ThreatLevel"":0, ""TimeRemaining"":1519.981689 }",
            @"{ ""timestamp"":""2024-10-31T19:28:41Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":908620468962, ""SignalName"":""$USS_PowerEmissions;"", ""SignalName_Localised"":""Unidentified signal source"", ""SignalType"":""USS"", ""USSType"":""$USS_Type_PowerEmissions;"", ""USSType_Localised"":""Power Wreckage Signature"", ""SpawningPower"":""Nakato Kaine"", ""OpposingPower"":""Aisling Duval"", ""ThreatLevel"":3, ""TimeRemaining"":1278.169189}",
            @"{ ""timestamp"":""2024-11-10T00:40:22Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":9467047454121, ""SignalName"":""$USS_PowerConvoy;"", ""SignalName_Localised"":""Unidentified signal source"", ""SignalType"":""USS"", ""USSType"":""$USS_Type_PowerConvoy;"", ""USSType_Localised"":""Power Convoy"", ""SpawningPower"":""Yuri Grom"", ""ThreatLevel"":3, ""TimeRemaining"":124.816635 }"
        };

        [PublicAPI("The signal source")]
        public string source => signalSource.localizedName;

        [PublicAPI( "The signal type" )]
        public string signaltype => signalSource.signalType?.localizedName;

        [PublicAPI("The faction state that triggered the signal source, if any")]
        public string factionstate => signalSource.spawningState.localizedName ?? signalSource.spawningState.fallbackLocalizedName ?? signalSource.spawningState.edname;

        [PublicAPI("The faction originating the signal source, if any")]
        public string faction => signalSource.spawningFaction;

        [PublicAPI( "The powerplay power originating the signal source, if any" )]
        public string power => signalSource.spawningPower;

        [PublicAPI( "The powerplay power opposing the originating power, if any" )]
        public string opposingpower => signalSource.opposingPower;

        [PublicAPI("The time before the signal expires, in seconds")]
        public decimal? secondsremaining => signalSource.expiry is null ? null : (decimal?)((DateTime)signalSource.expiry - timestamp).TotalSeconds;

        [PublicAPI("The risk posed by the signal source. Higher numbers are more dangerous.")]
        public int threatlevel => Convert.ToInt32(signalSource.threatLevel);

        [PublicAPI("True if the signal source is a station")]
        public bool stationsignal => Convert.ToBoolean(signalSource.isStation);

        [PublicAPI("True if this is the first signal of this type detected within the star system")]
        public bool unique { get; set; }

        // Not intended to be user facing

        public SignalSource signalSource { get; private set; }

        public ulong systemAddress { get; private set; } // Caution: scan events from the destination system can register after StartJump and before we actually leave the originating system

        public SignalDetectedEvent(DateTime timestamp, ulong systemAddress, SignalSource source) : base(timestamp, NAME)
        {
            this.systemAddress = systemAddress;
            this.signalSource = source;
            this.unique = unique;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var systemAddress = JsonParsing.getULong(data, "SystemAddress");

            var source = EventParsing.SignalSource(data);
            source.systemAddress = systemAddress;

            source.spawningFaction = EventParsing.FactionName( data, "SpawningFaction" ) ?? Superpower.None.localizedName; // the minor faction, if relevant
            source.SpawningPower = Power.FromEDName( JsonParsing.getString( data, "SpawningPower" ) ); // the Powerplay power, if relevant
            source.OpposingPower = Power.FromEDName( JsonParsing.getString( data, "OpposingPower" ) ); // the opposing Powerplay power, if relevant

            var secondsRemaining = JsonParsing.getOptionalDecimal(data, "TimeRemaining"); // remaining lifetime in seconds, if relevant
            source.expiry = secondsRemaining is null ? (DateTime?)null : timestamp.AddSeconds( (double)secondsRemaining );

            var spawningstate = JsonParsing.getString(data, "SpawningState");
            var normalizedSpawningState = spawningstate?.Replace("$FactionState_", "")?.Replace("_desc;", "");
            source.spawningState = FactionState.FromEDName( normalizedSpawningState ) ?? new FactionState();
            source.spawningState.fallbackLocalizedName = JsonParsing.getString( data, "SpawningState_Localised" );

            source.threatLevel = JsonParsing.getOptionalInt( data, "ThreatLevel" ) ?? 0;

            events.Add( new SignalDetectedEvent( timestamp, systemAddress, source ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
