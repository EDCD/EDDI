using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DockingCancelledEvent : Event
    {
        public const string NAME = "Docking cancelled";
        public const string DESCRIPTION = "Triggered when your ship cancels a docking request at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingCancelled\",\"StationName\":\"Jameson Memorial\", \"StationType\":\"Orbis\", \"MarketID\": 128666762}";

        [PublicAPI("The station at which the commander has cancelled docking")]
        public string station { get; private set; }

        [PublicAPI("The localized model / type of the station at which the commander has cancelled docking")]
        public string stationtype => stationDefinition?.localizedName;

        // These properties are not intended to be user facing

        public long? marketId { get; private set; }

        public StationModel stationDefinition { get; private set; }

        public DockingCancelledEvent(DateTime timestamp, string station, string stationType, long? marketId) : base(timestamp, NAME)
        {
            this.station = station;
            this.stationDefinition = StationModel.FromEDName(stationType);
            this.marketId = marketId;
        }
    }
}
