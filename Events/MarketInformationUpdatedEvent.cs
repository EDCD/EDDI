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

        static MarketInformationUpdatedEvent()
        {
            VARIABLES.Add("updates", "A list of the updates triggering the event (which may include 'market', 'outfitting', and 'shipyard'");
        }

        public HashSet<string> updates { get; private set; } = new HashSet<string>();

        public bool inHorizons { get; private set; }

        public bool inOdyssey { get; private set; }

        public bool? allowCobraMkIV { get; private set; }

        public string starSystem { get; private set; }

        public string stationName { get; private set; }

        public long? marketId { get; private set; }

        // The properties below must be easily convertible to the EDDN commodities schema (ref. https://github.com/EDCD/EDDN/tree/master/schemas) and must not introduce any data from local definitions
        public List<MarketInfoItem> commodityQuotes { get; private set; }

        public List<string> prohibitedCommodities { get; private set; }

        // The property below must be easily convertible to the EDDN outfitting schema (ref. https://github.com/EDCD/EDDN/tree/master/schemas) and must not introduce any data from local definitions
        public List<string> outfittingModules { get; private set; }

        // The property below must be easily convertible to the EDDN shipyard schema (ref. https://github.com/EDCD/EDDN/tree/master/schemas) and must not introduce any data from local definitions
        public List<string> shipyardModels { get; private set; }

        /// <summary>The timestamp recorded for this event must be generated from game or server data.
        /// System time (e.g. DateTime.UtcNow) cannot be trusted for reporting to EDDN and may not be used.</summary>
        public MarketInformationUpdatedEvent(DateTime timestamp, string starSystem, string stationName, long? marketId, List<MarketInfoItem> commodityQuotes, List<string> prohibitedCommodities, List<string> outfittingModules, List<string> shipyardModels, bool inHorizons, bool inOdyssey, bool? allowCobraMkIV = null) : base(timestamp, NAME)
        {
            this.inHorizons = inHorizons;
            this.inOdyssey = inOdyssey;
            this.allowCobraMkIV = allowCobraMkIV;
            this.starSystem = starSystem;
            this.stationName = stationName;
            this.marketId = marketId;
            this.commodityQuotes = commodityQuotes;
            this.prohibitedCommodities = prohibitedCommodities;
            this.outfittingModules = outfittingModules;
            this.shipyardModels = shipyardModels;

            if (commodityQuotes != null)
            {
                updates.Add("market");
            }
            if (outfittingModules != null)
            {
                updates.Add("outfitting");
            }
            if (shipyardModels != null)
            {
                updates.Add("shipyard");
            }
        }
    }
}
