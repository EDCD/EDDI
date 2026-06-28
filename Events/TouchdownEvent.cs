using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class TouchdownEvent (
        DateTime timestamp,
        decimal? longitude,
        decimal? latitude,
        string system,
        ulong systemAddress,
        string body,
        int? bodyId,
        bool? onStation,
        bool? onPlanet,
        bool? taxi,
        bool? multicrew,
        bool playercontrolled,
        SignalSource nearestDestination )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Touchdown";
        public const string DESCRIPTION = "Triggered when your ship or vessel touches down on a planet's surface";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-01T21:40:39Z\", \"event\":\"Touchdown\", \"PlayerControlled\":true, \"Taxi\":false, \"Multicrew\":false, \"StarSystem\":\"Nervi\", \"SystemAddress\":2518721481067, \"Body\":\"Nervi 2 a\", \"BodyID\":17, \"OnStation\":false, \"OnPlanet\":true, \"Latitude\":40.741577, \"Longitude\":65.081482 }";

        [PublicAPI("The name of the star system where the ship or vessel has touched down")]
        public string systemname { get; private set; } = system;

        [PublicAPI( "The numeric system address of the star system where the ship or vessel has touched down" )]
        public ulong systemAddress { get; private set; } = systemAddress;

        [PublicAPI( "The name of the body where the ship or vessel has touched down" )]
        public string bodyname { get; private set; } = body;

        [PublicAPI( "The numeric ID of the body where the ship or vessel has touched down" )]
        public int? bodyId { get; private set; } = bodyId;

        [PublicAPI( "The longitude from where the ship or vessel has touched down" )]
        public decimal? longitude { get; private set; } = longitude;

        [PublicAPI( "The latitude from where the ship or vessel has touched down" )]
        public decimal? latitude { get; private set; } = latitude;

        [PublicAPI( "True if the ship or vessel is a transport (e.g. taxi or dropship)" )]
        public bool? taxi { get; private set; } = taxi;

        [PublicAPI( "True if the ship or vessel is belongs to another player" )]
        public bool? multicrew { get; private set; } = multicrew;

        [PublicAPI( "True if the ship or vessel is controlled by the player" )]
        public bool playercontrolled { get; private set; } = playercontrolled;

        [PublicAPI( "The nearest location from where the ship or vessel has touched down" )]
        public string nearestdestination => nearestDestination.localizedName;

        [ PublicAPI( "True if the body is eligible for a first footfall" ) ]
        public bool canfirstfootfall { get; set; }

        // Not intended to be user facing

        public SignalSource nearestDestination { get; private set; } = nearestDestination;

        public bool? onstation { get; private set; } = onStation; // Always false, since `Touchdown` is currently only ever triggered when touching down on a body

        public bool? onplanet { get; private set; } = onPlanet; // Always true, since `Touchdown` is currently only ever triggered when touching down on a body

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
            var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
            var system = JsonParsing.getString(data, "StarSystem");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var body = JsonParsing.getString(data, "Body");
            var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
            var onStation = JsonParsing.getOptionalBool(data, "OnStation");
            var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");
            var playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;

            var taxi = JsonParsing.getOptionalBool(data, "Taxi");
            var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");

            // The nearest destination may be a specific destination name or a generic signal source.
            // Per the journal manual, the NearestDestination is included if within 50km of a location listed in the nav panel
            var nearestdestination = JsonParsing.getString(data, "NearestDestination");
            var nearestDestination = SignalSource.FromEDName(nearestdestination) ?? new SignalSource();
            var localizedName = JsonParsing.getString(data, "SignalName_Localised");
            if ( !string.IsNullOrEmpty( localizedName ) && !localizedName.Contains( '$' ) )
            {
                nearestDestination.fallbackLocalizedName = localizedName;
            }
            events.Add( new TouchdownEvent( timestamp, longitude, latitude, system, systemAddress, body, bodyId, onStation, onPlanet, taxi, multicrew, playercontrolled, nearestDestination ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
