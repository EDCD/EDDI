using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class OutfittingEvent : Event
    {
        public const string NAME = "Outfitting";
        public const string DESCRIPTION = "Triggered when the Outfitting.json file has been updated";
        public const string SAMPLE = @"{ ""timestamp"":""2017-10-05T10:11:38Z"", ""event"":""Outfitting"", ""MarketID"":128678535, ""StationName"":""Black Hide"", ""StarSystem"":""Wyrd"" }";

         
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static OutfittingEvent()
        {
        }

        public long marketId { get; private set; }
        public string station { get; private set; }
        public string system { get; private set; }

        public OutfittingEvent(DateTime timestamp, long marketId, string station, string system) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
        }
    }
}