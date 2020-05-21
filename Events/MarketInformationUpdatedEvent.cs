using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MarketInformationUpdatedEvent : Event
    {
        public const string NAME = "Market information updated";
        public const string DESCRIPTION = "Triggered when market information for the currently docked station has been updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        public bool inHorizons { get; private set; }

        public string starSystem { get; private set; }

        public string stationName { get; private set; }

        public long? marketId { get; private set; }

        public List<CommodityMarketQuote> commodities { get; private set; }

        public List<string> prohibitedCommodities { get; private set; }

        public List<Module> outfitting { get; private set; }

        public List<Ship> shipyard { get; private set; }

        /// <summary>The timestamp recorded for this event must be generated from game or server data.
        /// System time (e.g. DateTime.UtcNow) cannot be trusted for reporting to EDDN and may not be used.</summary>
        public MarketInformationUpdatedEvent(DateTime timestamp, bool inHorizons, string starSystem, string stationName, long? marketId, List<CommodityMarketQuote> commodities, List<string> prohibitedCommodities, List<Module> outfitting, List<Ship> shipyard) : base(timestamp, NAME)
        {
            this.inHorizons = inHorizons;
            this.starSystem = starSystem;
            this.stationName = stationName;
            this.marketId = marketId;
            this.commodities = commodities;
            this.prohibitedCommodities = prohibitedCommodities;
            this.outfitting = outfitting;
            this.shipyard = shipyard;
        }
    }
}
