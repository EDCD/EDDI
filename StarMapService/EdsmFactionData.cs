using EddiDataDefinitions;
using Newtonsoft.Json;
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
        public List<Faction> GetStarMapFactions(string systemName, long? edsmId = null)
        {
            if (systemName == null) { return new List<Faction>(); }

            var request = new RestRequest("api-system-v1/factions", Method.POST);
            request.AddParameter("systemName", systemName);
            request.AddParameter("systemId", edsmId);
            var clientResponse = restClient.Execute<JObject>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JObject response)
                {
                    return ParseStarMapFactionsParallel(response, systemName);
                }
            }
            else
            {
                Logging.Debug("EDSM responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return new List<Faction>();
        }

        public List<Faction> ParseStarMapFactionsParallel(JObject response, string systemName)
        {
            List<Faction> Factions = new List<Faction>();
            JArray factions = (JArray)response?["factions"];

            if (factions != null)
            {
                Factions = factions
                    .AsParallel()
                    .Select(f => ParseStarMapFaction(f.ToObject<JObject>(), systemName))
                    .Where(f => f != null)
                    .ToList();
            }
            return Factions;
        }

        private Faction ParseStarMapFaction(JObject faction, string systemName)
        {
            try
            {
                if (faction is null) { return null; }
                Faction Faction = new Faction
                {
                    name = (string)faction["name"],
                    EDSMID = (long?)faction["id"],
                    Allegiance = Superpower.FromName((string)faction["allegiance"]) ?? Superpower.None,
                    Government = Government.FromName((string)faction["government"]) ?? Government.None,
                    isplayer = (bool?)faction["isPlayer"],
                    updatedAt = Dates.fromTimestamp((long?)faction["lastUpdate"]) ?? DateTime.MinValue
                };

                Faction.presences.Add(new FactionPresence()
                {
                    systemName = systemName,
                    influence = (decimal?)faction["influence"] * 100, // Convert from a 0-1 range to a percentage
                    FactionState = FactionState.FromName((string)faction["state"]) ?? FactionState.None,
                });

                IDictionary<string, object> factionDetail = faction.ToObject<IDictionary<string, object>>();

                // Active states
                factionDetail.TryGetValue("activeStates", out object activeStatesVal);
                if (activeStatesVal != null)
                {
                    var activeStatesList = (JArray)activeStatesVal;
                    foreach (var activeStateToken in activeStatesList)
                    {
                        var activeState = activeStateToken.ToObject<IDictionary<string, object>>();
                        Faction.presences.FirstOrDefault(p => p.systemName == systemName)?
                            .ActiveStates.Add(FactionState.FromName(JsonParsing.getString(activeState, "state")) ?? FactionState.None);
                    }
                }

                // Pending states
                factionDetail.TryGetValue("pendingStates", out object pendingStatesVal);
                if (pendingStatesVal != null)
                {
                    var pendingStatesList = ((JArray)pendingStatesVal).ToList();
                    foreach (var pendingStateToken in pendingStatesList)
                    {
                        var pendingState = pendingStateToken.ToObject<IDictionary<string, object>>();
                        FactionTrendingState pTrendingState = new FactionTrendingState(
                            FactionState.FromName(JsonParsing.getString(pendingState, "state")) ?? FactionState.None,
                            JsonParsing.getInt(pendingState, "trend")
                        );
                        Faction.presences.FirstOrDefault(p => p.systemName == systemName)?
                            .PendingStates.Add(pTrendingState);
                    }
                }

                // Recovering states
                factionDetail.TryGetValue("recoveringStates", out object recoveringStatesVal);
                if (recoveringStatesVal != null)
                {
                    var recoveringStatesList = (JArray)recoveringStatesVal;
                    foreach (var recoveringStateToken in recoveringStatesList)
                    {
                        var recoveringState = recoveringStateToken.ToObject<IDictionary<string, object>>();
                        FactionTrendingState rTrendingState = new FactionTrendingState(
                            FactionState.FromName(JsonParsing.getString(recoveringState, "state")) ?? FactionState.None,
                            JsonParsing.getInt(recoveringState, "trend")
                        );
                        Faction.presences.FirstOrDefault(p => p.systemName == systemName)?
                            .RecoveringStates.Add(rTrendingState);
                    }
                }

                return Faction;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                            {
                                {"faction", JsonConvert.SerializeObject(faction)},
                                {"exception", ex.Message},
                                {"stacktrace", ex.StackTrace}
                            };
                Logging.Error("Error parsing EDSM faction result.", data);
            }
            return null;
        }
    }
}
