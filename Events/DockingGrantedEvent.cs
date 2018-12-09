using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DockingGrantedEvent : Event
    {
        public const string NAME = "Docking granted";
        public const string DESCRIPTION = "Triggered when your ship is granted docking permission at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingGranted\",\"MarketID\": 128666762,\"StationName\":\"Jameson Memorial\",\"StationType\":\"Orbis\",\"LandingPad\":2}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingGrantedEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has been granted docking");
            VARIABLES.Add("stationtype", "The localized model / type of the station at which the commander has been granted docking");
            VARIABLES.Add("stationDefinition", "The model / type of the station at which the commander has been granted docking (this is an object)");
            VARIABLES.Add("landingpad", "The landing pad at which the commander has been granted docking");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        [JsonProperty("stationType")]
        public StationModel stationDefinition { get; private set; }

        public string stationtype => stationDefinition.localizedName;

        [JsonProperty("landingpad")]
        public int landingpad { get; private set; }

        // Admin
        public long marketId { get; private set; }

        public DockingGrantedEvent(DateTime timestamp, string station, string stationType, long marketId, int landingpad) : base(timestamp, NAME)
        {
            this.station = station;
            this.stationDefinition = StationModel.FromEDName(stationType);
            this.marketId = marketId;
            this.landingpad = landingpad;
        }
    }
}
