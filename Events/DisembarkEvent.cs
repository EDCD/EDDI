using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DisembarkEvent : Event
    {
        public const string NAME = "Disembark";
        public const string DESCRIPTION = "Triggered when you transition from a ship or SRV to on foot";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-03T21:47:38Z\", \"event\":\"Disembark\", \"SRV\":false, \"Taxi\":false, \"Multicrew\":false, \"ID\":6, \"StarSystem\":\"Firenses\", \"SystemAddress\":2868635379121, \"Body\":\"Roberts Gateway\", \"BodyID\":44, \"OnStation\":true, \"OnPlanet\":false, \"StationName\":\"Roberts Gateway\", \"StationType\":\"Coriolis\", \"MarketID\":3221636096 }";

        [PublicAPI("The name of the star system where the commander is disembarking")]
        public string systemname { get; }

        [PublicAPI("The name of the body where the commander is disembarking (if any)")]
        public string bodyname { get; private set; }

        [PublicAPI("The name of the station where the commander is disembarking (if any)")]
        public string station { get; private set; }

        [PublicAPI("The type of station where the commander is disembarking (if any)")]
        public string stationtype => (stationModel ?? StationModel.None).localizedName;

        [PublicAPI("True if disembarking from another player's ship")]
        public bool frommulticrew { get; }

        [PublicAPI("True if disembarking from your own ship")]
        public bool fromship => fromLocalId != null && !fromsrv && !fromtransport && !frommulticrew;

        [PublicAPI("True if disembarking from an SRV")]
        public bool fromsrv { get; }

        [PublicAPI("True if disembarking from a transport ship (e.g. taxi or dropship)")]
        public bool fromtransport { get; }

        [PublicAPI("True if disembarking to a station")]
        public bool? onstation { get; }

        [PublicAPI("True if disembarking to a planet")]
        public bool? onplanet { get; }

        // Not intended to be user facing
        public int? fromLocalId { get; }

        public long systemAddress { get; }

        public int? bodyId { get; }

        public long? marketId { get; }

        public StationModel stationModel { get; }

        public DisembarkEvent(DateTime timestamp, bool fromSRV, bool fromTransport, bool fromMultiCrew, int? fromLocalId, string system, long systemAddress, string body, int? bodyId, bool? onStation, bool? onPlanet, string station = null, long? marketId = null, StationModel stationModel = null) : base(timestamp, NAME)
        {
            this.fromsrv = fromSRV;
            this.fromtransport = fromTransport;
            this.frommulticrew = fromMultiCrew;
            this.fromLocalId = fromLocalId;
            this.systemname = system;
            this.systemAddress = systemAddress;
            this.bodyname = body;
            this.bodyId = bodyId;
            this.onstation = onStation;
            this.onplanet = onPlanet;
            this.station = station;
            this.marketId = marketId;
            this.stationModel = stationModel;
        }
    }
}