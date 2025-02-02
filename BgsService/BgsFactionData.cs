using EddiDataDefinitions;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiBgsService
{
    partial class BgsService
    {
        /// <summary> The endpoint we will use for faction queries (using the BGS rest client) </summary>
        public const string factionEndpoint = "v5/factions";

        public static class FactionParameters
        {
            /// <summary> Faction name. </summary>
            public const string factionName = "name";
            
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
        [CanBeNull]
        public Faction GetFactionByName(string factionName, string systemName = null)
        {
            if (string.IsNullOrEmpty(factionName)) { return null; }

            var queryList = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(FactionParameters.factionName, factionName)
            };
            var factions = GetFactions(factionEndpoint, queryList);

            // If a systemName is provided, we can filter factions that share a name according to whether they have a presence in a known system
            if (systemName != null && factions?.Count > 1)
            {
                factions = factions.Where( f =>
                        f.presences.Any(
                            p => p.systemName.Equals( systemName, StringComparison.InvariantCultureIgnoreCase ) ) )
                    .ToList();
            }

            return factions?.FirstOrDefault( f =>
                f.name.Equals( factionName, StringComparison.InvariantCultureIgnoreCase ) );
        }

        public List<Faction> GetFactions(string endpoint, List<KeyValuePair<string, object>> queryList)
        {
            if (queryList.Count > 0)
            {
                var responses = GetData(bgsRestClient, endpoint, queryList);

                if (responses?.Count > 0)
                {
                    var factions = ParseFactionsParallel(responses);
                    return factions?.OrderBy(x => x.name).ToList();
                }
            }
            return null;
        }

        private List<Faction> ParseFactionsParallel(List<object> responses)
        {
            // it is OK to allow nulls into this list; they will be handled upstream
            var factions = responses.AsParallel().Select(ParseFaction).ToList();
            return factions;
        }

        public Faction ParseFaction(object response)
        {
            try
            {
                var factionJson = Deserializtion.DeserializeData(response.ToString());
                var faction = new Faction
                {
                    name = (string)factionJson["name"],
                    updatedAt = (DateTime)factionJson["updated_at"],
                    Government = Government.FromName((string)factionJson["government"]),
                    Allegiance = Superpower.FromName((string)factionJson["allegiance"])
                };

                foreach (var presence in (List<object>)factionJson["faction_presence"])
                {
                    var presenceJson = (IDictionary<string, object>)presence;
                    var factionPresence = new FactionPresence
                    {
                        systemName = JsonParsing.getString(presenceJson, "system_name"),
                        influence = (JsonParsing.getOptionalDecimal(presenceJson, "influence") ?? 0) * 100, // Convert from a 0-1 range to a percentage
                        FactionState = FactionState.FromEDName(JsonParsing.getString(presenceJson, "state")) ?? FactionState.None,

                        Happiness = Happiness.FromEDName(JsonParsing.getString(presenceJson, "happiness")?.Replace("none", "")) ?? Happiness.None
                    };

                    // These properties may not be present in the json, so we pass them after initializing our FactionPresence object.
                    presenceJson.TryGetValue("updated_at", out object updatedVal);
                    factionPresence.updatedAt = (DateTime?)updatedVal ?? DateTime.MinValue;

                    // Active states
                    presenceJson.TryGetValue("active_states", out object activeStatesVal);
                    if (activeStatesVal != null)
                    {
                        var activeStatesList = (List<object>)activeStatesVal;
                        foreach (var obj in activeStatesList)
                        {
                            if ( obj is IDictionary<string, object> activeState )
                            {
                                factionPresence.ActiveStates
                                    .Add(FactionState.FromEDName(JsonParsing.getString(activeState, "state")) ?? FactionState.None);
                            }
                        }
                    }

                    // Pending states
                    presenceJson.TryGetValue("pending_states", out object pendingStatesVal);
                    if (pendingStatesVal != null)
                    {
                        var pendingStatesList = (List<object>)pendingStatesVal;
                        foreach (var obj in pendingStatesList)
                        {
                            if ( obj is IDictionary<string, object> pendingState )
                            {
                                var pTrendingState = new FactionTrendingState(
                                    FactionState.FromEDName(JsonParsing.getString(pendingState, "state")) ?? FactionState.None,
                                    JsonParsing.getOptionalInt(pendingState, "trend")
                                );
                                factionPresence.PendingStates.Add(pTrendingState);                                
                            }
                        }
                    }

                    // Recovering states
                    presenceJson.TryGetValue("recovering_states", out object recoveringStatesVal);
                    if (recoveringStatesVal != null)
                    {
                        var recoveringStatesList = (List<object>)recoveringStatesVal;
                        foreach (var obj in recoveringStatesList)
                        {
                            if ( obj is IDictionary<string, object> recoveringState )
                            {
                                var rTrendingState = new FactionTrendingState(
                                    FactionState.FromEDName(JsonParsing.getString(recoveringState, "state")) ?? FactionState.None,
                                    JsonParsing.getOptionalInt(recoveringState, "trend")
                                );
                                factionPresence.RecoveringStates.Add(rTrendingState);                                
                            }
                        }
                    }

                    faction.presences.Add(factionPresence);
                }

                return faction;
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse BGS faction data.", ex);
                return null;
            }
        }
    }
}
