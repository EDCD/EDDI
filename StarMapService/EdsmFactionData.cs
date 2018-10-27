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

            var client = new RestClient("https://www.edsm.net/");
            var request = new RestRequest("api-system-v1/factions", Method.POST);
            request.AddParameter("systemName", system);
            request.AddParameter("systemId", edsmId);
            var clientResponse = client.Execute<JObject>(request);
            if (clientResponse.IsSuccessful)
            {
                JObject response = JObject.Parse(clientResponse.Content);
                return ParseStarMapFactions(response);
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
                        Faction Faction = ParseStarMapFaction(faction);
                        Factions.Add(Faction);
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
                Allegiance = Superpower.FromName((string)faction["allegiance"]),
                Government = Government.FromName((string)faction["government"]),
                isplayer = (bool)faction["isPlayer"],
                influence = (decimal?)faction["influence"] * 100, // Convert from a 0-1 range to a percentage
                FactionState = FactionState.FromName((string)faction["state"]),
                updatedAt = (DateTime)Dates.fromTimestamp((long?)faction["lastUpdate"])
            };
            return Faction;
        }
    }
}
