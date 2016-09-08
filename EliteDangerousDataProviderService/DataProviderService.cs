using EliteDangerousDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Utilities;

namespace EliteDangerousDataProviderService
{
    /// <summary>Access to EDDP data<summary>
    public class DataProviderService
    {
        private const string BASE = "http://api.eddp.co:16161/";

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
                    response = response + @", ""x"":" + x;
                }
                if (y.HasValue)
                {
                    response = response + @", ""y"":" + y;
                }
                if (z.HasValue)
                {
                    response = response + @", ""z"":" + z;
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

            if (json["updated_at"] != null)
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
                StarSystem.powerState = (string)json["power_state"];


                StarSystem.stations = StationsFromEDDP(json);
            }

            return StarSystem;
        }

        public static List<Station> StationsFromEDDP(dynamic json)
        {
            List<Station> Stations = new List<Station>();

            if (json["stations"] != null)
            {
                foreach (dynamic station in json["stations"])
                {
                    Station Station = new Station();
                    Station.EDDBID = (long)station["id"];
                    Station.Name = (string)station["name"];

                    Station.Economies = new List<String>();
                    if (station["economies"] != null)
                    {
                        foreach (dynamic economy in station["economies"])
                        {
                            Station.Economies.Add((String)economy);
                        }
                    }

                    Station.Allegiance = (string)station["allegiance"];
                    Station.Government = (string)station["government"];
                    Station.Faction = (string)station["faction"];
                    Station.State = (string)station["state"] == "None" ? null : (string)station["state"];
                    Station.DistanceFromStar = (long?)station["distance_to_star"];
                    Station.HasRefuel = (bool?)station["has_refuel"];
                    Station.HasRearm = (bool?)station["has_rearm"];
                    Station.HasRepair = (bool?)station["has_repair"];
                    Station.HasOutfitting = (bool?)station["has_outfitting"];
                    Station.HasShipyard = (bool?)station["has_shipyard"];
                    Station.HasMarket = (bool?)station["has_market"];
                    Station.HasBlackMarket = (bool?)station["has_blackmarket"];

                    if (((string)station["type"]) != null)
                    {
                        if (StationModels.ContainsKey((string)station["type"]))
                        {
                            Station.Model = StationModels[(string)station["type"]];
                        }
                        else
                        {
                            Logging.Error("Unknown station model " + ((string)station["type"]));
                        }
                    }

                    if (((string)station["max_landing_pad_size"]) != null && ((string)station["max_landing_pad_size"]) != "None")
                    {
                        if (LandingPads.ContainsKey((string)station["max_landing_pad_size"]))
                        {
                            Station.LargestShip = LandingPads[(string)station["max_landing_pad_size"]];
                        }
                        else
                        {
                            Logging.Error("Unknown landing pad size " + ((string)station["max_landing_pad_size"]));
                        }
                    }

                    Stations.Add(Station);
                }
            }
            return Stations;
        }

        private static Dictionary<string, ShipSize> LandingPads = new Dictionary<string, ShipSize>()
        {
            { "S", ShipSize.Small },
            { "M", ShipSize.Medium },
            { "L", ShipSize.Large },
            { "H", ShipSize.Huge },
        };

        private static Dictionary<string, StationModel> StationModels = new Dictionary<string, StationModel>()
        {
            { "Civilian Outpost", StationModel.CivilianOutpost },
            { "Commercial Outpost", StationModel.CommercialOutpost },
            { "Coriolis Starport", StationModel.CoriolisStarport },
            { "Industrial Outpost", StationModel.IndustrialOutpost },
            { "Military Outpost", StationModel.MilitaryOutpost },
            { "Mining Outpost", StationModel.MiningOutpost },
            { "Ocellus Starport", StationModel.OcellusStarport },
            { "Orbis Starport", StationModel.OrbisStarport },
            { "Planetary Engineer Base", StationModel.PlanetaryEngineerBase },
            { "Planetary Outpost", StationModel.PlanetaryOutpost },
            { "Planetary Port", StationModel.PlanetaryPort },
            { "Planetary Settlement", StationModel.PlanetarySettlement },
            { "Scientific Outpost", StationModel.ScientificOutpost},
            { "Unknown Outpost", StationModel.UnknownOutpost},
            { "Unknown Planetary", StationModel.UnknownPlanetary},
            { "Unknown Starport", StationModel.UnknownStarport},
            { "Unsanctioned Outpost", StationModel.UnsanctionedOutpost},
        };

        static DataProviderService()
        {
            // We need to not use an expect header as it causes problems when sending data to a REST service
            var errorUri = new Uri("http://api.eddp.co/error");
            var errorServicePoint = ServicePointManager.FindServicePoint(errorUri);
            errorServicePoint.Expect100Continue = false;
        }
    }
}
