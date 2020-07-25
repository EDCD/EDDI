using EddiDataDefinitions;
using Newtonsoft.Json;
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
        public List<ProfileEconomyShare> economyShares { get; set; }

        /// <summary>Commodity market quotes as-received from the profile</summary>
        public List<EddnCommodityMarketQuote> eddnCommodityMarketQuotes { get; set; }

        /// <summary>Prohibited commodities as-received from the profile</summary>
        public List<KeyValuePair<long, string>> prohibitedCommodities { get; set; }

        /// <summary>Outfitting modules as-received from the profile</summary>
        public List<ProfileModule> outfitting { get; set; }

        /// <summary>Ship models as-received from the profile</summary>
        public List<ProfileShip> ships { get; set; }

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

            if (station.commoditiesupdatedat < commoditiesupdatedat)
            {
                try
                {
                    station.commodities = eddnCommodityMarketQuotes.Select(c => c.ToCommodityMarketQuote()).ToList();
                    station.prohibited = prohibitedCommodities.Select(p => p.Value).ToList();
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
            if (station.outfittingupdatedat < outfittingupdatedat)
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
            if (station.shipyardupdatedat < shipyardupdatedat)
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

    public class ProfileModule
    {
        public long id { get; }

        public string edName { get; }

        public string category { get; }

        public long cost { get; }

        public string raw { get; }

        public ProfileModule(long eliteId, string edName, string edCategory, long cost, string raw = null)
        {
            this.id = eliteId;
            this.edName = edName;
            this.category = edCategory;
            this.cost = cost;
            this.raw = raw;
        }

        public Module ToModule()
        {
            var module = new Module(Module.FromEliteID(id, this)
                ?? Module.FromEDName(edName, this)
                ?? new Module());
            if (module.invariantName == null)
            {
                // Unknown module; report the full object so that we can update the definitions
                Logging.Info("Module definition error: " + edName, JsonConvert.SerializeObject(this));

                // Create a basic module & supplement from the info available
                module = new Module(id, edName, -1, edName, -1, "", cost);
            }
            else
            {
                module.price = cost;
            }
            return module;
        }
    }

    public class ProfileShip
    {
        // Parameters obtained from the Frontier API (not exlusively, though these are all that we handle from the Frontier API)

        public long id { get; }

        public string edName { get; }

        public long basevalue { get; }

        public string raw { get; }

        public ProfileShip(long eliteId, string edModelName, long basevalue, string raw = null)
        {
            this.id = eliteId;
            this.edName = edModelName;
            this.basevalue = basevalue;
            this.raw = raw;
        }

        public Ship ToShip()
        {
            Ship ship = ShipDefinitions.FromEliteID(id) ?? ShipDefinitions.FromEDModel(edName);
            if (ship == null)
            {
                // Unknown ship; report the full object so that we can update the definitions 
                Logging.Info("Ship definition error: " + edName, JsonConvert.SerializeObject(this));

                // Create a basic ship definition & supplement from the info available 
                ship = new Ship
                {
                    EDName = edName
                };
            }
            ship.value = basevalue;
            return ship;
        }
    }
}
