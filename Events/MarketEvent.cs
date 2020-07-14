using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class MarketEvent : Event
    {
        public const string NAME = "Market";
        public const string DESCRIPTION = "Triggered when the Market.json file has been updated";
        public const string SAMPLE = @"{  ""timestamp"":""2017-10-05T10:11:38Z"", ""event"":""Market"", ""MarketID"":128678535, ""StationName"":""Black Hide"", ""StarSystem"":""Wyrd"" }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MarketEvent()
        {
        }

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public long marketId { get; private set; }

        [VoiceAttackIgnore]
        public string station { get; private set; }

        [VoiceAttackIgnore]
        public string system { get; private set; }
        public MarketInfo info { get; private set; }

        public MarketEvent(DateTime timestamp, long marketId, string station, string system, MarketInfo info) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
            this.info = info;
        }
    }
}
