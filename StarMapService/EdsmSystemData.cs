﻿using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        /// <summary> Exactly one system name is required. </summary>
        public static StarSystem GetStarMapSystem(string system)
        {
            if (system == null) { return null; }
            var client = new RestClient("https://www.edsm.net/");
            var request = new RestRequest("api-v1/system", Method.POST);
            request.AddParameter("systemName", system);
            request.AddParameter("showId", 1);
            request.AddParameter("showCoordinates", 1);
            request.AddParameter("showInformation", 1);
            var clientResponse = client.Execute<JObject>(request);
            if (clientResponse.IsSuccessful)
            {
                JObject response = JObject.Parse(clientResponse.Content);
                return ParseStarMapSystem(response, system);
            }
            return null;
        }

        /// <summary> At least one system name is required. </summary>
        public static List<StarSystem> GetStarMapSystems(string[] systems)
        {
            if (systems == null) { return null; }
            var client = new RestClient("https://www.edsm.net/");
            var request = new RestRequest("api-v1/systems", Method.POST);
            request.AddParameter("systemName", systems);
            request.AddParameter("showId", 1);
            request.AddParameter("showCoordinates", 1);
            request.AddParameter("showInformation", 1);
            var clientResponse = client.Execute<JArray>(request);
            if (clientResponse.IsSuccessful)
            {
                JArray responses = JArray.Parse(clientResponse.Content);
                List<StarSystem> starSystems = new List<StarSystem>();
                foreach (JObject response in responses)
                {
                    if (response != null)
                    {
                        starSystems.Add(ParseStarMapSystem(response, (string)response["name"]));
                    }
                }
                return starSystems;
            }
            return null;
        }

        /// <summary> Get star systems around a specified system in a sphere or shell, with a maximum radius of 200 light years. </summary>
        public static List<Dictionary<string, object>> GetStarMapSystemsSphere(string starSystem, int minRadiusLy = 0, int maxRadiusLy = 200)
        {
            if (starSystem == null) { return null; }
            var client = new RestClient("https://www.edsm.net/");
            var request = new RestRequest("api-v1/sphere-systems", Method.POST);
            request.AddParameter("systemName", starSystem);
            request.AddParameter("minRadius", minRadiusLy);
            request.AddParameter("radius", maxRadiusLy);
            request.AddParameter("showId", 1);
            request.AddParameter("showCoordinates", 1);
            request.AddParameter("showInformation", 1);
            var clientResponse = client.Execute<JArray>(request);
            if (clientResponse.IsSuccessful)
            {
                JArray responses = JArray.Parse(clientResponse.Content);
                List<Dictionary<string, object>> distantSystems = new List<Dictionary<string, object>>();
                foreach (JObject response in responses)
                {
                    Dictionary<string, object> distantSystem = new Dictionary<string, object>()
                {
                    { "distance", (decimal)response["distance"] },
                    { "system", ParseStarMapSystem(response, (string)response["name"]) }
                };
                    distantSystems.Add(distantSystem);
                }
                return distantSystems;
            }
            return null;
        }

        /// <summary> Get star systems around a specified system in a cube, with a maximum cube size of 200 light years. </summary>
        public static List<StarSystem> GetStarMapSystemsCube(string starSystem, int cubeLy = 200)
        {
            if (starSystem == null) { return null; }
            var client = new RestClient("https://www.edsm.net/");
            var request = new RestRequest("api-v1/cube-systems", Method.POST);
            request.AddParameter("systemName", starSystem);
            request.AddParameter("size", cubeLy);
            request.AddParameter("showId", 1);
            request.AddParameter("showCoordinates", 1);
            request.AddParameter("showInformation", 1);
            var clientResponse = client.Execute<JArray>(request);
            if (clientResponse.IsSuccessful)
            {
                JArray responses = clientResponse.Data;
                List<StarSystem> starSystems = new List<StarSystem>();
                foreach (JObject response in responses)
                {
                    if (response != null)
                    {
                        starSystems.Add(ParseStarMapSystem(response, (string)response["name"]));
                    }
                }
                return starSystems;
            }
            return null;
        }

        private static StarSystem ParseStarMapSystem(JObject response, string system)
        {
            StarSystem starSystem = new StarSystem
            {
                name = (string)response["name"],
                EDSMID = (long?)response["id"]
            };

            if (response["coords"] is JObject)
            {
                var coords = response["coords"].ToObject<Dictionary<string, decimal?>>();
                starSystem.x = coords["x"];
                starSystem.y = coords["y"];
                starSystem.z = coords["z"];
            }

            if ((bool?)response["requirePermit"] is true)
            {
                starSystem.requirespermit = true;
                starSystem.permitname = (string)response["permitName"];
            }

            if (response["information"] is JObject information)
            {
                starSystem.Reserve = ReserveLevel.FromName((string)information["reserve"]);
                starSystem.population = (long?)information["population"];

                // Populated system data
                if (starSystem.population > 0)
                {
                    Faction controllingFaction = new Faction
                    {
                        name = (string)information["faction"],
                        Allegiance = Superpower.FromNameOrEdName((string)information["allegiance"]),
                        Government = Government.FromName((string)information["government"]),
                        FactionState = FactionState.FromName((string)information["factionState"])
                    };
                    starSystem.Faction = controllingFaction;

                    starSystem.securityLevel = SecurityLevel.FromName((string)information["security"]);
                    starSystem.Economies = new List<Economy>()
                    {
                        Economy.FromName((string)information["economy"]),
                        Economy.FromName((string)information["secondEconomy"])
                    };
                }
            }

            starSystem.lastupdated = DateTime.UtcNow;
            return starSystem;
        }
    }
}
