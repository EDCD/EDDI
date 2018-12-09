using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class UndockedEvent : Event
    {
        public const string NAME = "Undocked";
        public const string DESCRIPTION = "Triggered when your ship undocks from a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Undocked\",\"StationName\":\"Long Sight Base\", \"MarketID\": 128678023}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static UndockedEvent()
        {
            VARIABLES.Add("station", "The station from which the commander has undocked");
            VARIABLES.Add("marketId", "Market ID of the station from which the commander has undocked");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        // Admin
        [JsonProperty("marketId")]
        public long marketId { get; private set; }

        public UndockedEvent(DateTime timestamp, string station, long marketId) : base(timestamp, NAME)
        {
            this.station = station;
            this.marketId = marketId;
        }
    }
}