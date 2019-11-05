using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiBgsService
{
    partial class BgsService
    {
        /// <summary> The endpoint we will use for faction queries </summary>
        public const string factionEndpoint = "v4/factions?";

        public static class FactionParameters
        {
            /// <summary> Faction name. </summary>
            public const string factionName = "name";

            /// <summary> Partial faction name begins with... (at least 1 additional parameter is required) </summary>
            public const string beginsWith = "beginswith";

            /// <summary> Name of the allegiance. </summary>
            public const string allegiance = "allegiance";

            /// <summary> Name of the government type. </summary>
            public const string government = "government";
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
            List<Faction> factions = BgsService.GetFactions(factionEndpoint, queryList);

            // If a systemName is provided, we can filter factions that share a name according to whether they have a presence in a known system
            if (systemName != null && factions.Count > 1)
            {
                foreach (Faction faction in factions)
                {
                    faction.presences = faction.presences.Where(f => f.systemName == systemName)?.ToList();
                }
            }

            return factions?.FirstOrDefault() ?? new Faction()
            {
                name = factionName
            };
        }

        public static Faction GetFaction(string endpoint, List<KeyValuePair<string, object>> queryList)
        {
            return GetFactions(endpoint, queryList).FirstOrDefault();
        }

        public static List<Faction> GetFactions(string endpoint, List<KeyValuePair<string, object>> queryList)
        {
            if (queryList.Count > 0)
            {
                List<object> responses = GetData(endpoint, queryList);

                if (responses?.Count > 0)
                {
                    List<Faction> factions = ParseFactionsAsync(responses);
                    return factions.OrderBy(x => x.name).ToList();
                }
            }
            return null;
        }

        private static List<Faction> ParseFactionsAsync(List<object> responses)
        {
            List<Task<Faction>> factionTasks = new List<Task<Faction>>();
            foreach (object response in responses)
            {
                factionTasks.Add(Task.Run(() => ParseFaction(response)));
            }
            Task.WhenAll(factionTasks.ToArray());

            List<Faction> factions = new List<Faction>();
            foreach (Task<Faction> task in factionTasks)
            {
                Faction faction = task.Result;
                if (faction != null) { factions.Add(faction); };
            }

            return factions;
        }

        private static Faction ParseFaction(object response)
        {
            try
            {
                IDictionary<string, object> factionJson = Deserializtion.DeserializeData(response.ToString());
                Faction faction = new Faction
                {
                    EDDBID = (long)factionJson["eddb_id"],
                    name = (string)factionJson["name"],
                    updatedAt = (DateTime)factionJson["updated_at"],
                    Government = Government.FromName((string)factionJson["government"]),
                    Allegiance = Superpower.FromName((string)factionJson["allegiance"]),
                };

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
                    factionPresence.Happiness = Happiness.FromEDName(JsonParsing.getString(presenceJson, "happiness")) ?? Happiness.None;
                    presenceJson.TryGetValue("updated_at", out object updatedVal);
                    factionPresence.updatedAt = (DateTime?)updatedVal ?? DateTime.MinValue;

                    // Active states
                    presenceJson.TryGetValue("active_states", out object activeStatesVal);
                    if (activeStatesVal != null)
                    {
                        var activeStatesList = (List<object>)activeStatesVal;
                        foreach (IDictionary<string, object> activeState in activeStatesList)
                        {
                            factionPresence.ActiveStates.Add(FactionState.FromEDName(JsonParsing.getString(activeState, "state") ?? "None"));
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
                                FactionState.FromEDName(JsonParsing.getString(pendingState, "state") ?? "None"),
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
                                FactionState.FromEDName(JsonParsing.getString(recoveringState, "state") ?? "None"),
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
