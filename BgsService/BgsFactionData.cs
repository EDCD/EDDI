﻿using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
     
namespace EddiBgsService
{
    partial class BgsService
    {
        /// <summary> The endpoint we will use for faction queries (using the BGS rest client) </summary>
        public const string factionEndpoint = "v5/factions?";

        public static class FactionParameters
        {
            /// <summary> Faction name. </summary>
            public const string factionName = "name";

            /// <summary> Faction EDDB ID. </summary>
            public const string eddbId = "eddbid";

            /// <summary> Partial faction name begins with... (at least 1 additional parameter is required) </summary>
            public const string beginsWith = "beginsWith";

            /// <summary> Name of the allegiance. </summary>
            public const string allegiance = "allegiance";

            /// <summary> Name of the government type. </summary>
            public const string government = "government";

            /// <summary> Name of the star system. </summary>
            public const string starSystem = "system";

            /// <summary> Whether to apply the system filter in the history too (input a bool). </summary>
            public const string filterSystemInHistory = "filterSystemInHistory";

            /// <summary> Name of the active state of the faction. </summary>
            public const string activeState = "activeState";

            /// <summary> Name of the pending state of the faction. </summary>
            public const string pendingState = "pendingState";

            /// <summary> Name of the recovering state of the faction. </summary>
            public const string recoveringState = "recoveringState";

            /// <summary> Factions with influence greater than the stated value (input a string value from 0 to 1). </summary>
            public const string influenceGreaterThan = "influenceGT";

            /// <summary> Factions with influence less than the stated value (input a string value from 0 to 1). </summary>
            public const string influenceLessThan = "influenceLT";

            /// <summary> Get minimal data of the faction (input a bool). </summary>
            public const string minimal = "minimal";
        }

        // Faction data from EliteBGS (allows search by faction name - EDSM can only search by system name). 
        // If a systemName is provided, we can filter factions that share a name according to whether they have a presence in a known system
        public Faction GetFactionByName(string factionName, string systemName = null)
        {
            if (string.IsNullOrEmpty(factionName)) { return null; }

            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(FactionParameters.factionName, factionName)
            };
            List<Faction> factions = GetFactions(factionEndpoint, queryList);

            // If a systemName is provided, we can filter factions that share a name according to whether they have a presence in a known system
            if (systemName != null && factions?.Count > 1)
            {
                foreach (Faction faction in factions)
                {
                    faction.presences = faction.presences.Where(f => f.systemName == systemName)?.ToList();
                }
            }

            return factions?.FirstOrDefault(f => f.name == factionName) ?? new Faction()
            {
                name = factionName
            };
        }

        public Faction GetFaction(string endpoint, List<KeyValuePair<string, object>> queryList)
        {
            return GetFactions(endpoint, queryList).FirstOrDefault();
        }

        public List<Faction> GetFactions(string endpoint, List<KeyValuePair<string, object>> queryList)
        {
            if (queryList.Count > 0)
            {
                List<object> responses = GetData(bgsRestClient, endpoint, queryList);

                if (responses?.Count > 0)
                {
                    List<Faction> factions = ParseFactionsParallel(responses);
                    return factions.OrderBy(x => x.name).ToList();
                }
            }
            return null;
        }

        private List<Faction> ParseFactionsParallel(List<object> responses)
        {
            // it is OK to allow nulls into this list; they will be handled upstream
            List<Faction> factions = responses.AsParallel().Select(ParseFaction).ToList();
            return factions;
        }

        public Faction ParseFaction(object response)
        {
            try
            {
                Logging.Debug($"Response from EliteBGS bgsRestClient endpoint {factionEndpoint} is: ", response);

                IDictionary<string, object> factionJson = Deserializtion.DeserializeData(response.ToString());
                Faction faction = new Faction
                {
                    name = (string)factionJson["name"],
                    updatedAt = (DateTime)factionJson["updated_at"],
                    Government = Government.FromName((string)factionJson["government"]),
                    Allegiance = Superpower.FromName((string)factionJson["allegiance"]),
                };
                // EDDB ID may not be present for new / unknown factions
                faction.EDDBID = JsonParsing.getOptionalLong(factionJson, "eddb_id");

                foreach (object presence in (List<object>)factionJson["faction_presence"])
                {
                    IDictionary<string, object> presenceJson = (IDictionary<string, object>)presence;
                    FactionPresence factionPresence = new FactionPresence()
                    {
                        systemName = JsonParsing.getString(presenceJson, "system_name"),
                        influence = (JsonParsing.getOptionalDecimal(presenceJson, "influence") ?? 0) * 100, // Convert from a 0-1 range to a percentage
                        FactionState = FactionState.FromEDName(JsonParsing.getString(presenceJson, "state")) ?? FactionState.None,
                    };

                    // These properties may not be present in the json, so we pass them after initializing our FactionPresence object.
                    factionPresence.Happiness = Happiness.FromEDName(JsonParsing.getString(presenceJson, "happiness")?.Replace("none", "")) ?? Happiness.None;
                    presenceJson.TryGetValue("updated_at", out object updatedVal);
                    factionPresence.updatedAt = (DateTime?)updatedVal ?? DateTime.MinValue;

                    // Active states
                    presenceJson.TryGetValue("active_states", out object activeStatesVal);
                    if (activeStatesVal != null)
                    {
                        var activeStatesList = (List<object>)activeStatesVal;
                        foreach (IDictionary<string, object> activeState in activeStatesList)
                        {
                            factionPresence.ActiveStates.Add(FactionState.FromEDName(JsonParsing.getString(activeState, "state")) ?? FactionState.None);
                        }
                    }

                    // Pending states
                    presenceJson.TryGetValue("pending_states", out object pendingStatesVal);
                    if (pendingStatesVal != null)
                    {
                        var pendingStatesList = (List<object>)pendingStatesVal;
                        foreach (IDictionary<string, object> pendingState in pendingStatesList)
                        {
                            FactionTrendingState pTrendingState = new FactionTrendingState(
                                FactionState.FromEDName(JsonParsing.getString(pendingState, "state")) ?? FactionState.None,
                                JsonParsing.getInt(pendingState, "trend")
                            );
                            factionPresence.PendingStates.Add(pTrendingState);
                        }
                    }

                    // Recovering states
                    presenceJson.TryGetValue("recovering_states", out object recoveringStatesVal);
                    if (recoveringStatesVal != null)
                    {
                        var recoveringStatesList = (List<object>)recoveringStatesVal;
                        foreach (IDictionary<string, object> recoveringState in recoveringStatesList)
                        {
                            FactionTrendingState rTrendingState = new FactionTrendingState(
                                FactionState.FromEDName(JsonParsing.getString(recoveringState, "state")) ?? FactionState.None,
                                JsonParsing.getInt(recoveringState, "trend")
                            );
                            factionPresence.RecoveringStates.Add(rTrendingState);
                        }
                    }

                    faction.presences.Add(factionPresence);
                }

                return faction;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "input", response },
                    { "exception", ex }
                };
                Logging.Error("Failed to parse BGS faction data.", data);
                return null;
            }
        }
    }
}
