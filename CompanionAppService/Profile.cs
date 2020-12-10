using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiCompanionAppService
{
    /// <summary> Profile information returned by the companion app service.
    /// To prevent accidentally passing incorrect information to EDDN, inputs should not include any information from EDDI data definitions. </summary>
    public class Profile
    {
        /// <summary>The timestamp returned from the CAPI server</summary>
        public DateTime timestamp { get; set; }

        /// <summary>The JSON object</summary>
        public JObject json { get; set; }

        /// <summary>The commander</summary>
        public FrontierApiCommander Cmdr { get; set; }

        /// <summary>The current starsystem</summary>
        public ProfileStarSystem CurrentStarSystem { get; set; }

        /// <summary>The last station the commander docked at</summary>
        public ProfileStation LastStation { get; set; }

        /// <summary>Whether this profile describes a docked commander</summary>
        public bool docked { get; set; }

        /// <summary>Whether this profile describes a currently living commander</summary>
        public bool alive { get; set; }

        /// <summary>The contexts (i.e. "capabilities") associated with this profile </summary>
        public ProfileContexts contexts { get; set; }
    }

    public class ProfileContexts
    {
        /// <summary>Whether this profile describes a commander with access to the Cobra Mk IV</summary>
        public bool allowCobraMkIV { get; set; }

        /// <summary>Whether this profile describes a commander with the Horizons expanion running</summary>
        public bool inHorizons { get; set; }
    }

    public class ProfileStarSystem
    {
        /// <summary>System Name</summary>
        public string systemName { get; set; }
    }

    public class ProfileStation
    {
        /// <summary>Unique 64 bit id value for station</summary>
        public long? marketId { get; set; }

        /// <summary>The name</summary>
        public string name { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        public long? systemAddress { get; set; }

        /// <summary>The system in which this station resides</summary>
        public string systemname { get; set; }

        /// <summary>A list of the services offered by this station</summary>
        public List<KeyValuePair<string, string>> stationServices { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>What are the economies at the station, with proportions for each</summary>
        public List<ProfileEconomyShare> economyShares { get; set; } = new List<ProfileEconomyShare>();

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

        // Admin - the last time the information present changed
        public long? updatedat;

        // Admin - the last time the market information present changed
        public long? commoditiesupdatedat;

        // Admin - the last time the outfitting information present changed
        public long? outfittingupdatedat;

        // Admin - the last time the shipyard information present changed
        public long? shipyardupdatedat;

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

            if (commoditiesupdatedat != null && (station.commoditiesupdatedat ?? 0) < commoditiesupdatedat)
            {
                try
                {
                    station.commodities = eddnCommodityMarketQuotes.Select(c => c.ToCommodityMarketQuote()).ToList();
                    station.prohibited = prohibitedCommodities.Select(p => CommodityDefinition.CommodityDefinitionFromEliteID(p.Key) ?? CommodityDefinition.FromEDName(p.Value)).ToList();
                    station.commoditiesupdatedat = commoditiesupdatedat;
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
            if (outfittingupdatedat != null && (station.outfittingupdatedat ?? 0) < outfittingupdatedat)
            {
                try 
                {
                    station.outfitting = outfitting.Select(m => m.ToModule()).ToList();
                    station.outfittingupdatedat = outfittingupdatedat;
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
            if (shipyardupdatedat != null && (station.shipyardupdatedat ?? 0) < shipyardupdatedat)
            {
                try 
                {
                    station.shipyard = ships.Select(s => s.ToShip()).ToList();
                    station.shipyardupdatedat = shipyardupdatedat;
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

    public class ProfileEconomyShare
    {
        public string edName { get; }
        public decimal proportion { get; }

        public ProfileEconomyShare(string edName, decimal proportion)
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
