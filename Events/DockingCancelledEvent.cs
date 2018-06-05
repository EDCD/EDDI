using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DockingCancelledEvent : Event
    {
        public const string NAME = "Docking cancelled";
        public const string DESCRIPTION = "Triggered when your ship cancels a docking request at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingCancelled\",\"StationName\":\"Jameson Memorial\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingCancelledEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has cancelled docking");
            VARIABLES.Add("stationtype", "The localized model / type of the station at which the commander has cancelled docking");
            VARIABLES.Add("stationDefinition", "The model / type of the station at which the commander has cancelled docking (this is an object)");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        [JsonProperty("stationType")]
        public StationModels stationDefinition { get; private set; }

        public string stationtype => stationDefinition.localizedName;

        public DockingCancelledEvent(DateTime timestamp, string station, string stationType) : base(timestamp, NAME)
        {
            this.station = station;
            this.stationDefinition = StationModels.FromEDName(stationType);
        }
    }
}
