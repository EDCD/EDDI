using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class DockingRequestedEvent : Event
    {
        public const string NAME = "Docking requested";
        public const string DESCRIPTION = "Triggered when your ship requests docking at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingRequested\",\"StationName\":\"Jameson Memorial\", \"StationType\":\"Orbis\", \"MarketID\": 128666762}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingRequestedEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has requested docking");
            VARIABLES.Add("stationtype", "The localized model / type of the station at which the commander has requested docking");
        }

        [PublicAPI]
        public string station { get; private set; }

        [PublicAPI]
        public string stationtype => stationDefinition?.localizedName;

        // Not intended to be user facing

        public long marketId { get; private set; }

        public StationModel stationDefinition { get; private set; }

        public DockingRequestedEvent(DateTime timestamp, string station, string stationType, long marketId) : base(timestamp, NAME)
        {
            this.station = station;
            this.stationDefinition = StationModel.FromEDName(stationType);
            this.marketId = marketId;
        }
    }
}
