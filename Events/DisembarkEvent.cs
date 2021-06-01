using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using EddiDataDefinitions;

namespace EddiEvents
{
    public class DisembarkEvent : Event
    {
        public const string NAME = "Disembark";
        public const string DESCRIPTION = "Triggered when you transition from a ship or SRV to on foot";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-03T21:47:38Z\", \"event\":\"Disembark\", \"SRV\":false, \"Taxi\":false, \"Multicrew\":false, \"ID\":6, \"StarSystem\":\"Firenses\", \"SystemAddress\":2868635379121, \"Body\":\"Roberts Gateway\", \"BodyID\":44, \"OnStation\":true, \"OnPlanet\":false, \"StationName\":\"Roberts Gateway\", \"StationType\":\"Coriolis\", \"MarketID\":3221636096 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DisembarkEvent()
        {
            VARIABLES.Add("frommulticrew", "True if disembarking from another player's ship");
            VARIABLES.Add("fromship", "True if disembarking from your own ship");
            VARIABLES.Add("fromsrv", "True if disembarking from an SRV");
            VARIABLES.Add("fromtransport", "True if disembarking from a transport ship (e.g. taxi or dropship)");
            VARIABLES.Add("systemname", "The name of the star system where the commander is disembarking");
            VARIABLES.Add("bodyname", "The name of the body where the commander is disembarking (if any)");
            VARIABLES.Add("station", "The name of the station where the commander is disembarking (if any)");
            VARIABLES.Add("stationtype", "The type of station where the commander is disembarking (if any)");
            VARIABLES.Add("onstation", "True if disembarking to a station");
            VARIABLES.Add("onplanet", "True if disembarking to a planet");
        }

        [PublicAPI]
        public string systemname { get; }

        [PublicAPI]
        public string bodyname { get; private set; }

        [PublicAPI]
        public string station { get; private set; }

        [PublicAPI]
        public string stationtype => (stationModel ?? StationModel.None).localizedName;

        [PublicAPI]
        public bool frommulticrew { get; }

        [PublicAPI]
        public bool fromship => fromLocalId != null && !fromsrv && !fromtransport && !frommulticrew;

        [PublicAPI]
        public bool fromsrv { get; }

        [PublicAPI]
        public bool fromtransport { get; }

        [PublicAPI]
        public bool? onstation { get; }

        [PublicAPI]
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