using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DockingTimedOutEvent : Event
    {
        public const string NAME = "Docking timed out";
        public const string DESCRIPTION = "Triggered when your docking request times out";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingCancelled\",\"StationName\":\"Jameson Memorial\", \"StationType\":\"Orbis\"}";

        [PublicAPI("The station at which the docking request has timed out")]
        public string station { get; private set; }

        [PublicAPI("The localized model / type of the station at which docking has timed out")]
        public string stationtype => stationDefinition?.localizedName;

        // Not intended to be user facing
        public StationModel stationDefinition { get; private set; }

        public DockingTimedOutEvent(DateTime timestamp, string station, string stationType) : base(timestamp, NAME)
        {
            this.station = station;
            this.stationDefinition = StationModel.FromEDName(stationType);
        }
    }
}
