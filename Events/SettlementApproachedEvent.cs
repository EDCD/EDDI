using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SettlementApproachedEvent : Event
    {
        public const string NAME = "Settlement approached";
        public const string DESCRIPTION = "Triggered when you approach a settlement";
        public const string SAMPLE = @"{ ""timestamp"":""2018-11-04T03:11:56Z"", ""event"":""ApproachSettlement"", ""Name"":""Bulmer Enterprise"", ""MarketID"":3510380288, ""Latitude"":-23.121552, ""Longitude"":-98.177559 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SettlementApproachedEvent()
        {
            VARIABLES.Add("name", "The name of the settlement");
            VARIABLES.Add("longitude", "The longitude coordinate of the settlement (if given)");
            VARIABLES.Add("latitude", "The latitude coordinate of the settlement (if given)");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        public decimal? latitude { get; private set; }
        public decimal? longitude { get; private set; }

        // Admin
        public long marketId { get; private set; }

        public SettlementApproachedEvent(DateTime timestamp, string name, long marketId, decimal? latitude, decimal? longitude) : base(timestamp, NAME)
        {
            this.name = name;
            this.marketId = marketId;
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
}
