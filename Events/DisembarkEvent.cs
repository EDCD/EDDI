using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DisembarkEvent (
        DateTime timestamp,
        bool fromSrv,
        bool fromTransport,
        bool fromMultiCrew,
        int? fromLocalId,
        string system,
        ulong systemAddress,
        string body,
        int? bodyId,
        bool? onStation,
        bool? onPlanet,
        string station = null,
        long? marketId = null,
        StationModel stationModel = null )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Disembark";
        public const string DESCRIPTION = "Triggered when you transition from a ship or SRV to on foot";
        public static readonly string[] SAMPLES =
        [
            "{ \"timestamp\":\"2021-05-03T21:47:38Z\", \"event\":\"Disembark\", \"SRV\":false, \"Taxi\":false, \"Multicrew\":false, \"ID\":6, \"StarSystem\":\"Firenses\", \"SystemAddress\":2868635379121, \"Body\":\"Roberts Gateway\", \"BodyID\":44, \"OnStation\":true, \"OnPlanet\":false, \"StationName\":\"Roberts Gateway\", \"StationType\":\"Coriolis\", \"MarketID\":3221636096 }"
        ];
            
        [PublicAPI("The name of the star system where the commander is disembarking")]
        public string systemname { get; } = system;

        [PublicAPI( "The numeric system address of the star system where the commander is disembarking" )]
        public ulong systemAddress { get; } = systemAddress;

        [PublicAPI("The name of the body where the commander is disembarking (if any)")]
        public string bodyname { get; private set; } = body;

        [PublicAPI( "The numeric ID of the body where the commander is disembarking (if any)" )]
        public int? bodyId { get; } = bodyId;

        [PublicAPI("The name of the station where the commander is disembarking (if any)")]
        public string station { get; private set; } = station;

        [PublicAPI( "The numeric ID of the station where the commander is disembarking (if any)" )]
        public long? marketId { get; } = marketId;

        [PublicAPI("The type of station where the commander is disembarking (if any)")]
        public string stationtype => (stationModel ?? StationModel.None).localizedName;

        [PublicAPI("True if disembarking from another player's ship")]
        public bool frommulticrew { get; } = fromMultiCrew;

        [PublicAPI("True if disembarking from your own ship")]
        public bool fromship => fromLocalId != null && !fromsrv && !fromtransport && !frommulticrew;

        [PublicAPI( "True if disembarking from an SRV or Nomad" )]
        public bool fromsrv { get; } = fromSrv;

        [PublicAPI("True if disembarking from a transport ship (e.g. taxi or dropship)")]
        public bool fromtransport { get; } = fromTransport;

        [PublicAPI("True if disembarking to a station")]
        public bool? onstation { get; } = onStation;

        [PublicAPI("True if disembarking to a planet")]
        public bool? onplanet { get; } = onPlanet;

        [PublicAPI( "True if disembarking on a planet with no previously registered first footfall" )]
        public bool firstfootfall { get; set; }

        // Not intended to be user facing
        public int? fromLocalId { get; } = fromLocalId;

        public StationModel stationModel { get; } = stationModel;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var fromSRV = JsonParsing.getBool(data, "SRV"); // true if getting out of SRV, false if getting out of a ship 
            var fromTaxi = JsonParsing.getBool(data, "Taxi"); //  true when getting out of a transport ship (e.g. Apex Taxi or Frontline Solutions dropship)
            var fromMultiCrew = JsonParsing.getBool(data, "Multicrew"); //  true when getting out of another player’s vessel
            var fromLocalId = JsonParsing.getOptionalInt(data, "ID"); // player’s ship ID (if player's own vessel)

            var system = JsonParsing.getString(data, "StarSystem");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var body = JsonParsing.getString(data, "Body");
            var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
            var onStation = JsonParsing.getOptionalBool(data, "OnStation");
            var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");

            var marketId = JsonParsing.getOptionalLong(data, "MarketID");
            EventParsing.StationNameAndType( data, out var stationName, out _, out var stationModel );  // if at a station

            events.Add( new DisembarkEvent( timestamp, fromSRV, fromTaxi, fromMultiCrew, fromLocalId, system, systemAddress, body, bodyId, onStation, onPlanet, stationName, marketId, stationModel ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}