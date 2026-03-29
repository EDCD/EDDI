using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipyardEvent ( DateTime timestamp, long marketId, string station, string system, ShipyardInfo info )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Shipyard";
        public const string DESCRIPTION = "Triggered when the Shipyard.json file has been updated";
        public const string SAMPLE = @"{  ""timestamp"":""2017-10-04T10:01:38Z"", ""event"":""Shipyard"", ""MarketID"": 128122104, ""StationName"":""Seven Holm"", ""StarSystem"":""Tamor"" }";

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public string station { get; private set; } = station;

        public string system { get; private set; } = system;

        public ShipyardInfo info { get; private set; } = info;
    }
}