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
        public StarSystem GetStarMapSystem(string system, bool showCoordinates = true, bool showSystemInformation = true)
        {
            if (system == null) { return null; }
            return GetStarMapSystems(new[] { system }, showCoordinates, showSystemInformation)?.FirstOrDefault();
        }

        /// <summary> At least one system name is required. </summary>
        public List<StarSystem> GetStarMapSystems(string[] systems, bool showCoordinates = true, bool showSystemInformation = true)
        {
            if (systems == null) { return new List<StarSystem>(); }

            var request = new RestRequest("api-v1/systems", Method.POST);
            foreach (string system in systems)
            {
                request.AddParameter("systemName[]", system);
            }
            request.AddParameter("showId", 1);
            request.AddParameter("showCoordinates", showCoordinates ? 1 : 0);
            request.AddParameter("showInformation", showSystemInformation ? 1 : 0);
            request.AddParameter("showPermit", showSystemInformation ? 1 : 0);
            var clientResponse = restClient.Execute<List<JObject>>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JArray responses)
                {
                    List<StarSystem> starSystems = responses
                        .AsParallel()
                        .Select(s => ParseStarMapSystem(s.ToObject<JObject>()))
                        .Where(s => s != null)
                        .ToList();
                    return starSystems;
                }
            }
            else
            {
                Logging.Debug("EDSM responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return new List<StarSystem>();
        }

        /// <summary> Partial of system name is required. </summary>
        public List<StarSystem> GetStarMapSystemsPartial(string system, bool showCoordinates = true, bool showSystemInformation = true)
        {
            if (system == null) { return new List<StarSystem>(); }

            var request = new RestRequest("api-v1/systems", Method.POST);

            // Wildcard '%' is needed for partial system name's next character
            request.AddParameter("systemName", system + "%");
            request.AddParameter("showId", 1);
            request.AddParameter("showCoordinates", showCoordinates ? 1 : 0);
            request.AddParameter("showInformation", showSystemInformation ? 1 : 0);
            request.AddParameter("showPermit", showSystemInformation ? 1 : 0);
            var clientResponse = restClient.Execute<List<JObject>>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JArray responses)
                {
                    List<StarSystem> starSystems = responses
                        .AsParallel()
                        .Select(s => ParseStarMapSystem(s.ToObject<JObject>()))
                        .Where(s => s != null)
                        .ToList();
                    return starSystems;
                }
            }
            else
            {
                Logging.Debug("EDSM responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return new List<StarSystem>(); 
        }

        /// <summary> Get star systems around a specified system in a sphere or shell, with a maximum radius of 200 light years. </summary>
        public List<Dictionary<string, object>> GetStarMapSystemsSphere(string starSystem, int minRadiusLy = 0, int maxRadiusLy = 200, bool showEdsmId = true, bool showCoordinates = true, bool showPrimaryStar = true, bool showInformation = true, bool showPermit = true)
        {
            if (starSystem == null) { return new List<Dictionary<string, object>>(); }

            var request = new RestRequest("api-v1/sphere-systems", Method.POST);
            request.AddParameter("systemName", starSystem);
            request.AddParameter("minRadius", minRadiusLy);
            request.AddParameter("radius", maxRadiusLy);
            request.AddParameter("showId", showEdsmId ? 1 : 0);
            request.AddParameter("showCoordinates", showCoordinates ? 1 : 0);
            request.AddParameter("showPrimaryStar", showPrimaryStar ? 1 : 0);
            request.AddParameter("showInformation", showInformation ? 1 : 0);
            request.AddParameter("showPermit", showPermit ? 1 : 0);
            var clientResponse = restClient.Execute<List<JObject>>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JArray responses)
                {
                    List<Dictionary<string, object>> distantSystems = responses
                        .AsParallel()
                        .Select(r => new Dictionary<string, object>()
                            {
                                { "distance", (decimal)r["distance"] },
                                { "system", ParseStarMapSystem(r.ToObject<JObject>()) }
                            })
                        .Where(s => s != null)
                        .ToList();
                    return distantSystems;
                }
            }
            else
            {
                Logging.Debug("EDSM responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return new List<Dictionary<string, object>>();
        }

        /// <summary> Get star systems around a specified system in a cube, with a maximum cube size of 200 light years. </summary>
        public List<StarSystem> GetStarMapSystemsCube(string starSystem, int cubeLy = 200, bool showEdsmId = true, bool showCoordinates = true, bool showPrimaryStar = true, bool showInformation = true, bool showPermit = true)
        {
            if (starSystem == null) { return new List<StarSystem>(); }

            var request = new RestRequest("api-v1/cube-systems", Method.POST);
            request.AddParameter("systemName", starSystem);
            request.AddParameter("size", cubeLy);
            request.AddParameter("showId", showEdsmId ? 1 : 0);
            request.AddParameter("showCoordinates", showCoordinates ? 1 : 0);
            request.AddParameter("showPrimaryStar", showPrimaryStar ? 1 : 0);
            request.AddParameter("showInformation", showInformation ? 1 : 0);
            request.AddParameter("showPermit", showPermit ? 1 : 0);
            var clientResponse = restClient.Execute<List<JObject>>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JArray responses)
                {
                    List<StarSystem> starSystems = responses
                        .AsParallel()
                        .Select(s => ParseStarMapSystem(s.ToObject<JObject>()))
                        .Where(s => s != null)
                        .ToList();
                    return starSystems;
                }
            }
            return new List<StarSystem>();
        }

        public StarSystem ParseStarMapSystem(JObject response)
        {
            StarSystem starSystem = new StarSystem
            {
                systemname = (string)response["name"],
                systemAddress = (long?)response["id64"],
                EDSMID = (long?)response["id"]
            };

            if (response["coords"] is JObject)
            {
                var coords = response["coords"].ToObject<Dictionary<string, decimal?>>();
                starSystem.x = coords["x"];
                starSystem.y = coords["y"];
                starSystem.z = coords["z"];
            }

            if (response["primaryStar"] is JObject primarystar)
            {
                Body primaryStar = new Body()
                {
                    bodyname = (string)primarystar["name"],
                    bodyType = BodyType.FromEDName("Star"),
                    distance = 0,
                    stellarclass = ((string)primarystar["type"])?.Split(' ')[0]
                };
                starSystem.AddOrUpdateBody(primaryStar);
            }

            if ((bool?)response["requirePermit"] is true)
            {
                starSystem.requirespermit = true;
                starSystem.permitname = (string)response["permitName"];
            }

            if (response["information"] is JObject information)
            {
                starSystem.Reserve = ReserveLevel.FromName((string)information["reserve"]) ?? ReserveLevel.None;
                starSystem.population = (long?)information["population"] ?? 0;

                // Populated system data
                if (starSystem.population > 0)
                {
                    Faction controllingFaction = new Faction
                    {
                        name = (string)information["faction"],
                        Allegiance = Superpower.FromName((string)information["allegiance"]) ?? Superpower.None,
                        Government = Government.FromName((string)information["government"]) ?? Government.None,
                    };
                    controllingFaction.presences.Add(new FactionPresence()
                    {
                        systemName = starSystem.systemname,
                        FactionState = FactionState.FromName((string)information["factionState"]) ?? FactionState.None,
                    });
                    starSystem.Faction = controllingFaction;

                    starSystem.securityLevel = SecurityLevel.FromName((string)information["security"]) ?? SecurityLevel.None;
                    starSystem.Economies = new List<Economy>()
                    {
                        Economy.FromName((string)information["economy"]) ?? Economy.None,
                        Economy.FromName((string)information["secondEconomy"]) ?? Economy.None
                    };
                }
            }

            starSystem.lastupdated = DateTime.UtcNow;
            return starSystem;
        }
    }
}
