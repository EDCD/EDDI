using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
                response = @"{""name"":""" + system + @"""";
                if (x.HasValue)
                {
                    response = response + @", ""x"":" + ((decimal)x).ToString(CultureInfo.InvariantCulture);
                }
                if (y.HasValue)
                {
                    response = response + @", ""y"":" + ((decimal)y).ToString(CultureInfo.InvariantCulture);
                }
                if (z.HasValue)
                {
                    response = response + @", ""z"":" + ((decimal)z).ToString(CultureInfo.InvariantCulture);
                }
                response = response + @", ""stations"":[]";
                response = response + @", ""bodies"":[]}";
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
                StarSystem.primaryeconomy = (string)json["primary_economy"];
                StarSystem.state = (string)json["state"] == "None" ? null : (string)json["state"];
                StarSystem.security = (string)json["security"];
                StarSystem.power = (string)json["power"] == "None" ? null : (string)json["power"];
                StarSystem.powerstate = (string)json["power_state"];
                StarSystem.updatedat = (long?)json["updated_at"];

                StarSystem.stations = StationsFromEDDP(StarSystem.name, json);
                StarSystem.bodies = BodiesFromEDDP(StarSystem.name, json);
            }

            StarSystem.lastupdated = DateTime.Now;

            return StarSystem;
        }

        public static List<Station> StationsFromEDDP(string systemName, dynamic json)
        {
            List<Station> Stations = new List<Station>();

            if (json["stations"] != null)
            {
                foreach (dynamic station in json["stations"])
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
                            Logging.Report("Unknown station model " + ((string)station["type"]));
                        }
                    }

                    string largestpad = (string)station["max_landing_pad_size"];
                    if (largestpad == "S") { largestpad = "Small"; }
                    if (largestpad == "M") { largestpad = "Medium"; }
                    if (largestpad == "L") { largestpad = "Large"; }
                    Station.largestpad = largestpad;

                    Station.commodities = CommoditiesFromEDDP(station);
                    Station.commoditiesupdatedat = (long?)station["market_updated_at"];

                    Logging.Debug("Station is " + JsonConvert.SerializeObject(Station));
                    Stations.Add(Station);
                }
            }
            return Stations;
        }

        public static List<Commodity> CommoditiesFromEDDP(dynamic json)
        {
            List<Commodity> commodities = new List<Commodity>();
            if (json["commodities"] != null)
            {
                foreach (dynamic commodity in json["commodities"])
                {
                    commodities.Add(new Commodity((int)(long)commodity["id"], (string)commodity["name"], (int?)(long?)commodity["buy_price"], (int?)(long?)commodity["demand"], (int?)(long?)commodity["sell_price"], (int?)(long?)commodity["supply"]));
                }
            }
            return commodities;
        }

        public static List<Body> BodiesFromEDDP(string systemName, dynamic json)
        {
            List<Body> Bodies = new List<Body>();

            if (json["bodies"] != null)
            {
                foreach (dynamic body in json["bodies"])
                {
                    if (body["group_name"] == "Belt")
                    {
                        // Not interested in asteroid belts
                        continue;
                    }

                    Body Body = new Body();

                    // General items
                    Body.EDDBID = (long)body["id"];
                    Body.name = (string)body["name"];
                    Body.systemname = systemName;
                    Body.type = body["group_name"];
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
                    }

                    if (Body.type == "Planet")
                    {
                        // Planet-specific items
                        Body.landable = (bool?)body["is_landable"];
                        Body.periapsis = (decimal?)(double?)body["arg_of_periapsis"];
                        Body.atmosphere = (string)body["atmosphere_type_name"];
                        Body.tilt = (decimal?)(double?)body["axis_tilt"];
                        Body.earthmass = (decimal?)(double?)body["earth_masses"];
                        Body.gravity = (decimal?)(double?)body["gravity"];
                        Body.eccentricity = (decimal?)(double?)body["orbital_eccentricity"];
                        Body.inclination = (decimal?)(double?)body["orbital_inclination"];
                        Body.orbitalperiod = (decimal?)(double?)body["orbital_period"];
                        Body.radius = (long?)body["radius"];
                        Body.rotationalperiod = (decimal?)(double?)body["rotational_period"];
                        Body.semimajoraxis = (decimal?)(double?)body["semi_major_axis"];
                        Body.pressure = (decimal?)(double?)body["surface_pressure"];
                        Body.terraformstate = (string)body["terraforming_state_name"];
                        Body.planettype = (string)body["type_name"];
                        // Volcanism might be a simple name or an object
                        if (body["volcanism_type_name"] != null)
                        {
                            Body.volcanism = Volcanism.FromName((string)body["volcanism_type_name"]);
                        }
                        if (body["volcanism"] != null)
                        {
                            Body.volcanism = new Volcanism((string)body["volcanism"]["type"], (string)body["volcanism"]["composition"], (string)body["volcanism"]["amount"]);
                        }
                        if (body["materials"] != null)
                        {
                            List<MaterialPresence> Materials = new List<MaterialPresence>();
                            foreach (dynamic materialJson in body["materials"])
                            {
                                Material material = Material.FromName((string)materialJson["material_name"]);
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
    }
}
