﻿using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipyardEvent : Event
    {
        public const string NAME = "Shipyard";
        public const string DESCRIPTION = "Triggered when the Shipyard.json file has been updated";
        public const string SAMPLE = @"{  ""timestamp"":""2017-10-04T10:01:38Z"", ""event"":""Shipyard"", ""MarketID"": 128122104, ""StationName"":""Seven Holm"", ""StarSystem"":""Tamor"" }";

        // Not intended to be user facing

        public long marketId { get; private set; }

        public string station { get; private set; }

        public string system { get; private set; }

        public ShipyardInfo info { get; private set; }

        public ShipyardEvent(DateTime timestamp, long marketId, string station, string system, ShipyardInfo info) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
            this.info = info;
        }
    }
}