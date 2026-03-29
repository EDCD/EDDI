using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MarketEvent ( DateTime timestamp, long marketId, string station, string system, MarketInfo info )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Market";
        public const string DESCRIPTION = "Triggered when the Market.json file has been updated";
        public const string SAMPLE = @"{  ""timestamp"":""2017-10-05T10:11:38Z"", ""event"":""Market"", ""MarketID"":128678535, ""StationName"":""Black Hide"", ""StarSystem"":""Wyrd"" }";

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public string station { get; private set; } = station;

        public string system { get; private set; } = system;

        public MarketInfo info { get; private set; } = info;
    }
}
