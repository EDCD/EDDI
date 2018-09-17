using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DockingTimedOutEvent : Event
    {
        public const string NAME = "Docking timed out";
        public const string DESCRIPTION = "Triggered when your docking request times out";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingCancelled\",\"StationName\":\"Jameson Memorial\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingTimedOutEvent()
        {
            VARIABLES.Add("station", "The station at which the docking request has timed out");
            VARIABLES.Add("stationtype", "The localized model / type of the station at which docking has timed out");
            VARIABLES.Add("stationDefinition", "The model / type of the station at which docking has timed out (this is an object)");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        [JsonProperty("stationType")]
        public StationModel stationDefinition { get; private set; }

        public string stationtype => stationDefinition.localizedName;

        public DockingTimedOutEvent(DateTime timestamp, string station, string stationType) : base(timestamp, NAME)
        {
            this.station = station;
            this.stationDefinition = StationModel.FromEDName(stationType);
        }
    }
}
