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
        public static List<Faction> GetStarMapFactions(string system, long? edsmId = null)
        {
            if (system == null) { return null; }

            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-system-v1/factions", Method.POST);
            request.AddParameter("systemName", system);
            request.AddParameter("systemId", edsmId);
            var clientResponse = client.Execute<JObject>(request);
            if (clientResponse.IsSuccessful)
            {
                JObject response = JObject.Parse(clientResponse.Content);
                return ParseStarMapFactions(response);
            }
            else
            {
                Logging.Debug("EDSM responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return null;
        }

        private static List<Faction> ParseStarMapFactions(JObject response)
        {
            List<Faction> Factions = new List<Faction>();
            if (response != null)
            {
                JArray factions = (JArray)response["factions"];

                if (factions != null)
                {
                    foreach (JObject faction in factions)
                    {
                        try
                        {
                            Faction Faction = ParseStarMapFaction(faction);
                            Factions.Add(Faction);
                        }
                        catch (Exception ex)
                        {
                            Logging.Error("Error parsing EDSM faction result " + faction.ToString(), ex);
                        }
                    }
                }
            }
            return Factions;
        }

        private static Faction ParseStarMapFaction(JObject faction)
        {
            Faction Faction = new Faction
            {
                name = (string)faction["name"],
                EDSMID = (long?)faction["id"],
                Allegiance = Superpower.FromName((string)faction["allegiance"]) ?? Superpower.None,
                Government = Government.FromName((string)faction["government"]) ?? Government.None,
                isplayer = (bool)faction["isPlayer"],
                influence = (decimal?)faction["influence"] * 100, // Convert from a 0-1 range to a percentage
                FactionState = FactionState.FromName((string)faction["state"]) ?? FactionState.None,
                updatedAt = (DateTime)Dates.fromTimestamp((long?)faction["lastUpdate"])
            };

            IDictionary<string, object> factionDetail = faction.ToObject<IDictionary<string, object>>();
            
            // Active states
            Faction.ActiveStates = new List<FactionState>();
            factionDetail.TryGetValue("ActiveStates", out object activeStatesVal);
            if (activeStatesVal != null)
            {
                var activeStatesList = (List<object>)activeStatesVal;
                foreach (IDictionary<string, object> activeState in activeStatesList)
                {
                    Faction.ActiveStates.Add(FactionState.FromEDName(JsonParsing.getString(activeState, "State") ?? "None"));
                }
            }

            // Pending states
            Faction.PendingStates = new List<FactionTrendingState>();
            factionDetail.TryGetValue("PendingStates", out object pendingStatesVal);
            if (pendingStatesVal != null)
            {
                var pendingStatesList = (List<object>)pendingStatesVal;
                foreach (IDictionary<string, object> pendingState in pendingStatesList)
                {
                    FactionTrendingState pTrendingState = new FactionTrendingState(
                        FactionState.FromEDName(JsonParsing.getString(pendingState, "State") ?? "None"),
                        JsonParsing.getInt(pendingState, "Trend")
                    );
                    Faction.PendingStates.Add(pTrendingState);
                }
            }

            // Recovering states
            Faction.RecoveringStates = new List<FactionTrendingState>();
            factionDetail.TryGetValue("RecoveringStates", out object recoveringStatesVal);
            if (recoveringStatesVal != null)
            {
                var recoveringStatesList = (List<object>)recoveringStatesVal;
                foreach (IDictionary<string, object> recoveringState in recoveringStatesList)
                {
                    FactionTrendingState rTrendingState = new FactionTrendingState(
                        FactionState.FromEDName(JsonParsing.getString(recoveringState, "State") ?? "None"),
                        JsonParsing.getInt(recoveringState, "Trend")
                    );
                    Faction.RecoveringStates.Add(rTrendingState);
                }
            }

            return Faction;
        }
    }
}
