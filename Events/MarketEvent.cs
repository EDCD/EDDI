using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

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

        public long marketId { get; private set; }
        public string station { get; private set; }
        public string system { get; private set; }

        public MarketEvent(DateTime timestamp, long marketId, string station, string system) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
        }
    }
}
