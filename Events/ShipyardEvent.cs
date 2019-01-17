using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShipyardEvent : Event
    {
        public const string NAME = "Shipyard";
        public const string DESCRIPTION = "Triggered when the Shipyard.json file has been updated";
        public const string SAMPLE = @"{  ""timestamp"":""2017-10-04T10:01:38Z"", ""event"":""Shipyard"", ""MarketID"": 128122104, ""StationName"":""Seven Holm"", ""StarSystem"":""Tamor"" }";


        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipyardEvent()
        {
        }

        public long marketId { get; private set; }
        public string station { get; private set; }
        public string system { get; private set; }

        public ShipyardEvent(DateTime timestamp, long marketId, string station, string system) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
        }
    }
}