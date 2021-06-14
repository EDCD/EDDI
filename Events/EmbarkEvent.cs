using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EmbarkEvent : Event
    {
        public const string NAME = "Embark";
        public const string DESCRIPTION = "Triggered when you transition from on foot to a ship or SRV";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-03T22:22:12Z\", \"event\":\"Embark\", \"SRV\":false, \"Taxi\":false, \"Multicrew\":false, \"ID\":6, \"StarSystem\":\"Sumod\", \"SystemAddress\":3961847269739, \"Body\":\"Sharp Dock\", \"BodyID\":56, \"OnStation\":true, \"OnPlanet\":false, \"StationName\":\"Sharp Dock\", \"StationType\":\"Coriolis\", \"MarketID\":3223952128 }";

        [PublicAPI("The name of the star system in which the commander is embarking")]
        public string systemname { get; }

        [PublicAPI("The name of the body from which the commander is embarking (if any)")]
        public string bodyname { get; private set; }

        [PublicAPI("The name of the station from which the commander is embarking (if any)")]
        public string station { get; private set; }

        [PublicAPI("The type of station from which the commander is embarking (if any)")]
        public string stationtype => (stationModel ?? StationModel.None).localizedName;
        
        [PublicAPI("True if embarking to another player's ship")]
        public bool tomulticrew { get; }

        [PublicAPI("True if embarking to your own ship")]
        public bool toship => toLocalId != null && !tosrv && !totransport && !tomulticrew;

        [PublicAPI("True if embarking to an SRV")]
        public bool tosrv { get; }

        [PublicAPI("True if embarking to a transport ship (e.g. taxi or dropship)")]
        public bool totransport { get; }

        [PublicAPI("True if embarking from a station")]
        public bool? onstation { get; }

        [PublicAPI("True if embarking from a planet")]
        public bool? onplanet { get; }

        // Not intended to be user facing
        public int? toLocalId { get; }

        public long systemAddress { get;}

        public int? bodyId { get; }

        public long? marketId { get; }

        public StationModel stationModel { get; }

        public EmbarkEvent(DateTime timestamp, bool toSRV, bool toTransport, bool toMultiCrew, int? toLocalId, string system, long systemAddress, string body, int? bodyId, bool? onStation, bool? onPlanet, string station, long? marketId, StationModel stationModel) : base(timestamp, NAME)
        {
            this.tosrv = toSRV;
            this.totransport = toTransport;
            this.tomulticrew = toMultiCrew;
            this.toLocalId = toLocalId;
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
