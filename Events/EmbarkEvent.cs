using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EmbarkEvent (
        DateTime timestamp,
        bool toSrv,
        bool toTransport,
        bool toMultiCrew,
        int? toLocalId,
        string system,
        ulong systemAddress,
        string body,
        int? bodyId,
        bool? onStation,
        bool? onPlanet,
        string station,
        long? marketId,
        StationModel stationModel )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Embark";
        public const string DESCRIPTION = "Triggered when you transition from on foot to a ship or SRV";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-03T22:22:12Z\", \"event\":\"Embark\", \"SRV\":false, \"Taxi\":false, \"Multicrew\":false, \"ID\":6, \"StarSystem\":\"Sumod\", \"SystemAddress\":3961847269739, \"Body\":\"Sharp Dock\", \"BodyID\":56, \"OnStation\":true, \"OnPlanet\":false, \"StationName\":\"Sharp Dock\", \"StationType\":\"Coriolis\", \"MarketID\":3223952128 }";

        [PublicAPI("The name of the star system in which the commander is embarking")]
        public string systemname { get; } = system;

        [PublicAPI( "The numeric system address of the star system from which the commander is embarking" )]
        public ulong systemAddress { get;} = systemAddress;

        [PublicAPI("The name of the body from which the commander is embarking (if any)")]
        public string bodyname { get; private set; } = body;

        [PublicAPI( "The numeric ID of the body from which the commander is embarking (if any)" )]
        public int? bodyId { get; } = bodyId;

        [PublicAPI("The name of the station from which the commander is embarking (if any)")]
        public string station { get; private set; } = station;

        [PublicAPI( "The numeric ID of the station from which the commander is embarking (if any)" )]
        public long? marketId { get; } = marketId;

        [PublicAPI("The type of station from which the commander is embarking (if any)")]
        public string stationtype => (stationModel ?? StationModel.None).localizedName;
        
        [PublicAPI("True if embarking to another player's ship")]
        public bool tomulticrew { get; } = toMultiCrew;

        [PublicAPI("True if embarking to your own ship")]
        public bool toship => toLocalId != null && !tosrv && !totransport && !tomulticrew;

        [PublicAPI("True if embarking to an SRV or Nomad")]
        public bool tosrv { get; } = toSrv;

        [PublicAPI("True if embarking to a transport ship (e.g. taxi or dropship)")]
        public bool totransport { get; } = toTransport;

        [PublicAPI("True if embarking from a station")]
        public bool? onstation { get; } = onStation;

        [PublicAPI("True if embarking from a planet")]
        public bool? onplanet { get; } = onPlanet;

        [PublicAPI( "A list of currently deployed vessels, as objects" )]
        public List<VesselDefinition> deployedVessels { get; set; }

        // Not intended to be user facing
        public int? toLocalId { get; } = toLocalId;

        public StationModel stationModel { get; } = stationModel;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var toSRV = JsonParsing.getBool(data, "SRV"); // true if getting out of SRV, false if getting out of a ship 
            var toTaxi = JsonParsing.getBool(data, "Taxi"); //  true when getting out of a transport ship (e.g. Apex Taxi or Frontline Solutions dropship)
            var toMultiCrew = JsonParsing.getBool(data, "Multicrew"); //  true when getting out of another player’s vessel
            var toLocalId = JsonParsing.getOptionalInt(data, "ID"); // player’s ship ID (if player's own vessel)

            var system = JsonParsing.getString(data, "StarSystem");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var body = JsonParsing.getString(data, "Body");
            var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
            var onStation = JsonParsing.getOptionalBool(data, "OnStation");
            var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");

            var marketId = JsonParsing.getOptionalLong(data, "MarketID");
            EventParsing.StationNameAndType( data, out var stationName, out _, out var stationModel );  // if at a station

            events.Add( new EmbarkEvent( timestamp, toSRV, toTaxi, toMultiCrew, toLocalId, system, systemAddress, body, bodyId, onStation, onPlanet, stationName, marketId, stationModel ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
