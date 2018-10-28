﻿using EddiDataDefinitions;
using EddiStarMapService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Utilities;

namespace EddiDataProviderService
{
    /// <summary>Access to EDDP data<summary>
    public class DataProviderService
    {
        //TODO: Change this to use an EDCD server or other established data service (EDSM?)
        private const string BASE = "http://api.eddp.co/";

        public static StarSystem GetSystemData(string system, decimal? x, decimal?y, decimal? z)
        {
            if (system == null) { return null; }

            // X Y and Z are provided to us with 3dp of accuracy even though they are x/32; fix that here
            if (x.HasValue)
            {
                x = (Math.Round(x.Value * 32))/32;
            }
            if (y.HasValue)
            {
                y = (Math.Round(y.Value * 32)) / 32;
            }
            if (z.HasValue)
            {
                z = (Math.Round(z.Value * 32)) / 32;
            }

            string response;
            try
            {
                response = Net.DownloadString(BASE + "systems/" + Uri.EscapeDataString(system));
            }
            catch (WebException)
            {
                response = null;
            }
            if (response == null || response == "")
            {
                // No information found on this system, or some other issue.  Create a very basic response
                Dictionary<string, object> dummyData = new Dictionary<string, object>();
                dummyData["name"] = system;
                if (x.HasValue)
                {
                    dummyData["x"] = x.GetValueOrDefault();
                }
                if (y.HasValue)
                {
                    dummyData["y"] = y.GetValueOrDefault();
                }
                if (z.HasValue)
                {
                    dummyData["z"] = z.GetValueOrDefault();
                }
                dummyData["stations"] = new Dictionary<string, object>();
                dummyData["bodies"] = new Dictionary<string, object>();
                response = JsonConvert.SerializeObject(dummyData);
                Logging.Info("Generating dummy response " + response);
            }
            return StarSystemFromEDDP(response, x, y, z);
        }

        public static StarSystem StarSystemFromEDDP(string data, decimal? x, decimal? y, decimal? z)
        {
            return StarSystemFromEDDP(JObject.Parse(data), x, y, z);
        }

        public static StarSystem StarSystemFromEDDP(dynamic json, decimal? x, decimal? y, decimal? z)
        {
            StarSystem StarSystem = new StarSystem();
            StarSystem.name = (string)json["name"];
            StarSystem.x = json["x"] == null ? x : (decimal?)json["x"];
            StarSystem.y = json["y"] == null ? y : (decimal?)json["y"];
            StarSystem.z = json["z"] == null ? y : (decimal?)json["z"];

            if (json["is_populated"] != null)
            {
                // We have real data so populate the rest of the data
                StarSystem.EDDBID = (long)json["id"];
                StarSystem.population = (long?)json["population"] == null ? 0 : (long?)json["population"];
                StarSystem.allegiance = (string)json["allegiance"];
                StarSystem.government = (string)json["government"];
                StarSystem.faction = (string)json["faction"];
                // EDDP uses invariant / English localized economies
                StarSystem.economies[0] = Economy.FromName((string)json["primary_economy"]);
                // At present, EDDP does not provide any information about secondary economies.
                StarSystem.systemState = State.FromEDName((string)json["state"]) ?? State.None;
                StarSystem.security = (string)json["security"];
                StarSystem.power = (string)json["power"] == "None" ? null : (string)json["power"];
                StarSystem.powerstate = (string)json["power_state"];
                StarSystem.updatedat = (long?)json["updated_at"];

                StarSystem.stations = StationsFromEDDP(StarSystem.name, json);
                StarSystem.bodies = BodiesFromEDDP(StarSystem.name, json);
            }

            StarSystem.lastupdated = DateTime.UtcNow;

            return StarSystem;
        }

        public static List<Station> StationsFromEDDP(string systemName, JObject json)
        {
            List<Station> Stations = new List<Station>();

            if (json["stations"] != null)
            {
                foreach (JObject station in json["stations"])
                {
                    Station Station = new Station();
                    Station.EDDBID = (long)station["id"];
                    Station.name = (string)station["name"];
                    Station.systemname = systemName;

                    Station.primaryeconomy = (string)station["primary_economy"];

                    Station.allegiance = (string)station["allegiance"];
                    Station.government = (string)station["government"];
                    Station.faction = (string)station["controlling_faction"];
                    Station.state = (string)station["state"] == "None" ? null : (string)station["state"];
                    Station.distancefromstar = (long?)station["distance_to_star"];
                    Station.hasrefuel = (bool?)station["has_refuel"];
                    Station.hasrearm = (bool?)station["has_rearm"];
                    Station.hasrepair = (bool?)station["has_repair"];
                    Station.hasoutfitting = (bool?)station["has_outfitting"];
                    Station.hasshipyard = (bool?)station["has_shipyard"];
                    Station.hasmarket = (bool?)station["has_market"];
                    Station.hasblackmarket = (bool?)station["has_blackmarket"];
                    Station.updatedat = (long?)station["updated_at"];
                    Station.outfittingupdatedat = (long?)station["outfitting_updated_at"];

                    if (((string)station["type"]) != null)
                    {
                        Station.model = ((string)station["type"]);
                        if (!stationModels.Contains((string)station["type"]))
                        {
                            Logging.Info("Unknown station model " + ((string)station["type"]));
                        }
                    }

                    string largestpad = (string)station["max_landing_pad_size"];
                    if (largestpad == "S") { largestpad = "Small"; }
                    if (largestpad == "M") { largestpad = "Medium"; }
                    if (largestpad == "L") { largestpad = "Large"; }
                    Station.largestpad = largestpad;

                    Station.commodities = CommodityQuotesFromEDDP(station);
                    Station.commoditiesupdatedat = (long?)station["market_updated_at"];

                    Logging.Debug("Station is " + JsonConvert.SerializeObject(Station));
                    Stations.Add(Station);
                }
            }
            return Stations;
        }

        public static List<CommodityMarketQuote> CommodityQuotesFromEDDP(JObject json)
        {
            var quotes = new List<CommodityMarketQuote>();
            if (json["commodities"] != null)
            {
                foreach (JObject commodity in json["commodities"])
                {
                    CommodityDefinition commodityDefinition = CommodityDefinition.FromName((string)commodity["name"]);
                    CommodityMarketQuote quote = new CommodityMarketQuote(commodityDefinition);
                    // Annoyingly, these double-casts seem to be necessary because the boxed type is `long`. A direct cast to `int?` always returns null.
                    quote.buyprice = (int?)(long?)commodity["buy_price"] ?? quote.buyprice;
                    quote.sellprice = (int?)(long?)commodity["sell_price"] ?? quote.sellprice;
                    quote.demand = (int?)(long?)commodity["demand"] ?? quote.demand;
                    quote.stock = (int?)(long?)commodity["supply"] ?? quote.stock;
                    quotes.Add(quote);
                }
            }
            return quotes;
        }

        public static List<Body> BodiesFromEDDP(string systemName, dynamic json)
        {
            List<Body> Bodies = new List<Body>();

            if (json["bodies"] != null)
            {
                foreach (dynamic body in json["bodies"])
                {
                    if ((string)body["group_name"] == "Belt")
                    {
                        // Not interested in asteroid belts
                        continue;
                    }

                    Body Body = new Body();

                    // General items
                    Body.EDDBID = (long)body["id"];
                    Body.name = (string)body["name"];
                    Body.systemname = systemName;
                    Body.Type = BodyType.FromName((string)body["group_name"]);
                    Body.distance = (long?)body["distance_to_arrival"];
                    Body.temperature = (long?)body["surface_temperature"];
                    Body.tidallylocked = (bool?)body["is_rotational_period_tidally_locked"];

                    if (Body.type == "Star")
                    {
                        // Star-specific items
                        Body.stellarclass = (string)body["spectral_class"];
                        Body.solarmass = (decimal?)(double?)body["solar_masses"];
                        Body.solarradius = (decimal?)(double?)body["solar_radius"];
                        Body.age = (long?)body["age"];
                        Body.mainstar = (bool?)body["is_main_star"];
                        Body.landable = false;
                        Body.setStellarExtras();
                    }

                    if (Body.type == "Planet")
                    {
                        // Planet-specific items
                        Body.landable = (bool?)body["is_landable"];
                        Body.periapsis = (decimal?)(double?)body["arg_of_periapsis"];
                        Body.atmosphereclass = AtmosphereClass.FromEDName((string)body["atmosphere_type_name"]);
                        Body.tilt = (decimal?)(double?)body["axis_tilt"];
                        Body.earthmass = (decimal?)(double?)body["earth_masses"];
                        Body.gravity = (decimal?)(double?)body["gravity"];
                        Body.eccentricity = (decimal?)(double?)body["orbital_eccentricity"];
                        Body.inclination = (decimal?)(double?)body["orbital_inclination"];
                        decimal? orbitalPeriodSeconds = (decimal?)(double?)body["orbital_period"];
                        Body.orbitalperiod = orbitalPeriodSeconds / (24.0M * 60.0M * 60.0M);
                        Body.radius = (long?)body["radius"];
                        Body.rotationalperiod = (decimal?)(double?)body["rotational_period"];
                        Body.semimajoraxis = (decimal?)(double?)body["semi_major_axis"];
                        Body.pressure = ConstantConverters.pascals2atm((decimal?)(double?)body["surface_pressure"]);
                        Body.terraformState = TerraformState.FromName((string)body["terraforming_state_name"]);
                        Body.planetClass = PlanetClass.FromName((string)body["type_name"]);
                        // Volcanism might be a simple name or an object
                        if (body["volcanism"] != null)
                        {
                            Body.volcanism = new Volcanism((string)body["volcanism"]["type"], (string)body["volcanism"]["composition"], (string)body["volcanism"]["amount"]);
                        }
                        else if (body["volcanism_type_name"] != null)
                        {
                            Body.volcanism = Volcanism.FromName((string)body["volcanism_type_name"]);
                        }
                        if (body["materials"] != null)
                        {
                            List<MaterialPresence> Materials = new List<MaterialPresence>();
                            foreach (dynamic materialJson in body["materials"])
                            {
                                Material material = Material.FromEDName((string)materialJson["material_name"]);
                                decimal? amount = (decimal?)(double?)materialJson["share"];
                                if (material != null && amount != null)
                                {
                                    Materials.Add(new MaterialPresence(material, (decimal)amount));
                                }
                            }
                            if (Materials.Count > 0)
                            {
                                Body.materials = Materials.OrderByDescending(o => o.percentage).ToList();
                            }
                        }
                    }

                    Bodies.Add(Body);
                }
            }
            // Sort bodies by distance
            return Bodies.OrderBy(o => o.distance).ToList();
        }

        private static List<string> stationModels = new List<string>()
        {
            "Asteroid Base",
            "Civilian Outpost",
            "Commercial Outpost",
            "Coriolis Starport",
            "Industrial Outpost",
            "Megaship",
            "Military Outpost",
            "Mining Outpost",
            "Ocellus Starport",
            "Orbis Starport",
            "Orbital Engineer Base",
            "Planetary Engineer Base",
            "Planetary Outpost",
            "Planetary Port",
            "Planetary Settlement",
            "Scientific Outpost",
            "Unknown Outpost",
            "Unknown Planetary",
            "Unknown Starport",
            "Unsanctioned Outpost"
        };

        static DataProviderService()
        {
            // We need to not use an expect header as it causes problems when sending data to a REST service
            var errorUri = new Uri(BASE + "error");
            var errorServicePoint = ServicePointManager.FindServicePoint(errorUri);
            errorServicePoint.Expect100Continue = false;
        }

        public void syncFromStarMapService(StarMapService starMapService, StarMapConfiguration starMapCredentials)
        {
            Logging.Info("Syncing from EDSM");
            try
            {
                Dictionary<string, StarMapLogInfo> systems = starMapService.getStarMapLog(starMapCredentials.lastSync);
                Dictionary<string, string> comments = starMapService.getStarMapComments();
                List<StarSystem> syncSystems = new List<StarSystem>();
                foreach (string system in systems.Keys)
                {
                    StarSystem CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(system, false);
                    CurrentStarSystem.visits = systems[system].visits;
                    CurrentStarSystem.lastvisit = systems[system].lastVisit;
                    if (comments.ContainsKey(system))
                    {
                        CurrentStarSystem.comment = comments[system];
                    }
                    syncSystems.Add(CurrentStarSystem);

                    if (syncSystems.Count == StarMapService.syncBatchSize)
                    {
                        saveFromStarMapService(syncSystems);
                        syncSystems.Clear();
                    }
                }
                if (syncSystems.Count > 0)
                {
                    saveFromStarMapService(syncSystems);
                }
                Logging.Info("EDSM sync completed");
            }
            catch (EDSMException edsme)
            {
                Logging.Debug("EDSM error received: " + edsme.Message);
            }
            catch (ThreadAbortException e)
            {
                Logging.Debug("EDSM update stopped by user: " + e.Message);
            }
        }

        public static void saveFromStarMapService(List<StarSystem> syncSystems)
        {
            StarSystemSqLiteRepository.Instance.SaveStarSystems(syncSystems);
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            starMapConfiguration.lastSync = DateTime.UtcNow;
            starMapConfiguration.ToFile();
        }
    }
}
