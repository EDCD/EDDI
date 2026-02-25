using EddiDataDefinitions;
using System;
using System.Collections.Generic;
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

        public DockingCancelledEvent(DateTime timestamp, string station, StationModel stationType, long? marketId) : base(timestamp, NAME)
        {
            this.station = station;
            this.stationDefinition = stationType;
            this.marketId = marketId;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading
            var marketId = JsonParsing.getLong(data, "MarketID");
            EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );
            events.Add( new DockingCancelledEvent( timestamp, stationLocalizedName ?? stationName, stationType, marketId ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
