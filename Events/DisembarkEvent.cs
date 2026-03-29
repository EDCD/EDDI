using EddiDataDefinitions;
using System;
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

        [PublicAPI("True if disembarking from an SRV")]
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
    }
}