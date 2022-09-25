using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary> Profile information returned by the companion app service.
    /// To prevent accidentally passing incorrect information to EDDN, inputs should not include any information from EDDI data definitions. </summary>
    public class FrontierApiProfile
    {
        /// <summary>The timestamp returned from the CAPI server</summary>
        public DateTime timestamp { get; set; }

        /// <summary>The JSON object</summary>
        public JObject json { get; set; }

        /// <summary>The commander</summary>
        public FrontierApiCommander Cmdr { get; set; }

        /// <summary>The current starsystem</summary>
        public FrontierApiProfileStarSystem CurrentStarSystem { get; set; }

        /// <summary>The name of the last station the commander docked at</summary>
        public string LastStationName { get; set; }

        /// <summary>The market id of the last station the commander docked at</summary>
        public long? LastStationMarketID { get; set; }

        /// <summary>Whether this profile describes a docked commander</summary>
        public bool docked { get; set; }

        /// <summary>Whether this profile describes an on-foot commander</summary>
        public bool onFoot { get; set; }

        /// <summary>Whether this profile describes a currently living commander</summary>
        public bool alive { get; set; }

        /// <summary>The contexts (i.e. "capabilities") associated with this profile </summary>
        public FrontierApiProfileContexts contexts { get; set; }
    }

    public class FrontierApiProfileContexts
    {
        /// <summary>Whether this profile describes a commander with access to the Cobra Mk IV</summary>
        public bool allowCobraMkIV { get; set; }

        /// <summary>Whether this profile describes a commander with the Horizons expansion available</summary>
        public bool hasHorizons { get; set; }

        /// <summary>Whether this profile describes a commander with the Horizons expansion available</summary>
        public bool hasOdyssey { get; set; }
    }

    public class FrontierApiProfileStarSystem
    {
        /// <summary>System Name</summary>
        public string systemName { get; set; }
    }

    public class FrontierApiProfileStation
    {
        /// <summary>Unique 64 bit id value for station</summary>
        public long? marketId { get; set; }

        /// <summary>The name</summary>
        public string name { get; set; }

        /// <summary>A list of the services offered by this station</summary>
        public List<KeyValuePair<string, string>> stationServices { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>What are the economies at the station, with proportions for each</summary>
        public List<FrontierApiEconomyShare> economyShares { get; set; } = new List<FrontierApiEconomyShare>();

        /// <summary>Commodity market quotes as-received from the profile</summary>
        public List<MarketInfoItem> eddnCommodityMarketQuotes { get; set; } = new List<MarketInfoItem>();

        /// <summary>Prohibited commodities as-received from the profile</summary>
        public List<KeyValuePair<long, string>> prohibitedCommodities { get; set; } = new List<KeyValuePair<long, string>>();

        /// <summary>Outfitting modules as-received from the profile</summary>
        public List<OutfittingInfoItem> outfitting { get; set; } = new List<OutfittingInfoItem>();

        /// <summary>Ship models as-received from the profile</summary>
        public List<ShipyardInfoItem> ships { get; set; } = new List<ShipyardInfoItem>();

        /// <summary>The market JSON object</summary>
        public JObject json { get; set; }

        // Admin - the last time the market information present changed
        public DateTime commoditiesupdatedat;

        // Admin - the last time the outfitting information present changed
        public DateTime outfittingupdatedat;

        // Admin - the last time the shipyard information present changed
        public DateTime shipyardupdatedat;

        public Station UpdateStation(DateTime profileTimeStamp, Station station)
        {
            if (station.updatedat > Dates.fromDateTimeToSeconds(profileTimeStamp))
            {
                // The current station is already more up to date
                return station;
            }
            if (station.marketId != marketId)
            {
                // The market IDs do not match, the stations are not the same
                return station;
            }

            try
            {
                station.economyShares = economyShares.Select(e => e.ToEconomyShare()).ToList();
                station.stationServices = stationServices.Select(s => StationService.FromEDName(s.Key)).ToList();
            }
            catch (Exception e)
            {
                var data = new Dictionary<string, object>()
                    {
                        { "Exception", e },
                        { "Profile economy shares", economyShares},
                        { "Profile station services", stationServices},
                        { "Station", station }
                    };
                Logging.Error("Failed to update station economy and services from profile.", data);
            }

            if (commoditiesupdatedat != DateTime.MinValue &&
                (station.commoditiesupdatedat ?? 0) < Dates.fromDateTimeToSeconds(commoditiesupdatedat))
            {
                try
                {
                    station.commodities = eddnCommodityMarketQuotes.Select(c => c.ToCommodityMarketQuote()).Where(c => c != null).ToList();
                    station.prohibited = prohibitedCommodities.Select(p => CommodityDefinition.CommodityDefinitionFromEliteID(p.Key) ?? CommodityDefinition.FromEDName(p.Value)).ToList();
                    station.commoditiesupdatedat = Dates.fromDateTimeToSeconds(commoditiesupdatedat);
                }
                catch (Exception e)
                {
                    var data = new Dictionary<string, object>()
                    {
                        { "Exception", e },
                        { "Profile commodity quotes", eddnCommodityMarketQuotes },
                        { "Profile prohibited commodities", prohibitedCommodities},
                        { "Station", station }
                    };
                    Logging.Error("Failed to update station market from profile.", data);
                }
            }
            if (outfittingupdatedat != DateTime.MinValue && (station.outfittingupdatedat ?? 0) < Dates.fromDateTimeToSeconds(outfittingupdatedat))
            {
                try
                {
                    station.outfitting = outfitting.Select(m => m.ToModule()).Where(m => m != null).ToList();
                    station.outfittingupdatedat = Dates.fromDateTimeToSeconds(outfittingupdatedat);
                }
                catch (Exception e)
                {
                    var data = new Dictionary<string, object>()
                    {
                        { "Exception", e },
                        { "Profile outiftting", outfitting },
                        { "Station", station }
                    };
                    Logging.Error("Failed to update station outfitting from profile.", data);
                }

            }
            if (shipyardupdatedat != DateTime.MinValue && (station.shipyardupdatedat ?? 0) < Dates.fromDateTimeToSeconds(shipyardupdatedat))
            {
                try
                {
                    station.shipyard = ships.Select(s => s.ToShip()).Where(s => s != null).ToList();
                    station.shipyardupdatedat = Dates.fromDateTimeToSeconds(shipyardupdatedat);
                }
                catch (Exception e)
                {
                    var data = new Dictionary<string, object>()
                    {
                        { "Exception", e },
                        { "Profile ships", ships },
                        { "Station", station }
                    };
                    Logging.Error("Failed to update station shipyard from profile.", data);
                }
            }
            station.updatedat = Dates.fromDateTimeToSeconds(profileTimeStamp);
            return station;
        }
    }

    public class FrontierApiEconomyShare
    {
        public string edName { get; }
        public decimal proportion { get; }

        public FrontierApiEconomyShare(string edName, decimal proportion)
        {
            this.edName = edName;
            this.proportion = proportion;
        }

        public EconomyShare ToEconomyShare()
        {
            return new EconomyShare(edName, proportion);
        }
    }
}
