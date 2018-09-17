using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiEddbService
{
    partial class EddbService
    {      
        /// <summary> At least one faction name is required. </summary>
        public static List<Faction> Factions(string[] factionNames)
        {
            List<Faction> factions = new List<Faction>();
            foreach (string name in factionNames)
            {
                Faction faction = GetFaction(new KeyValuePair<string, object>(FactionQuery.factionName, name)) ?? new Faction();
                if (faction.EDDBID == null)
                {
                    faction.name = name;
                }
                factions.Add(faction);
            }
            return factions;
        }

        /// <summary> At least one faction EDDBID is required. </summary>
        public static List<Faction> Factions(long[] eddbIds)
        {
            List<Faction> factions = new List<Faction>();
            foreach (long eddbId in eddbIds)
            {
                Faction faction = GetFaction(new KeyValuePair<string, object>(FactionQuery.eddbId, eddbId)) ?? new Faction();
                if (faction.EDDBID == null)
                {
                    faction.EDDBID = eddbId;
                }
                factions.Add(faction);
            }
            return factions;
        }

        /// <summary> At least one home system name is required. </summary>
        public static List<Faction> Factions(string homeSystem)
        {
            return GetFactions(new KeyValuePair<string, object>(FactionQuery.homeSystem, homeSystem)) ?? new List<Faction>();
        }

        /// <summary> Exactly one faction name is required. </summary>
        public static Faction Faction(string factionName)
        {
            Faction faction = GetFaction(new KeyValuePair<string, object>(FactionQuery.factionName, factionName)) ?? new Faction();
            if (faction.EDDBID == null)
            {
                faction.name = factionName;
            }
            return faction;
        }

        /// <summary> Exactly one faction EDDBID is required. </summary>
        public static Faction Faction(long eddbId)
        {
            Faction faction = GetFaction(new KeyValuePair<string, object>(FactionQuery.eddbId, eddbId)) ?? new Faction();
            if (faction.EDDBID == null)
            {
                faction.EDDBID = eddbId;
            }
            return faction;
        }

        private static Faction GetFaction(KeyValuePair<string, object> query)
        {
            if (query.Value != null)
            {
                List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
                queryList.Add(query);

                List<object> responses = GetData(Endpoint.factions, queryList);

                if (responses != null)
                {
                    return ParseEddbFaction(responses[0]);
                }
            }
            return null;
        }

        private static List<Faction> GetFactions(KeyValuePair<string, object> query)
        {
            if (query.Value != null)
            {
                List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
                queryList.Add(query);

                List<object> responses = GetData(Endpoint.factions, queryList);

                if (responses != null)
                {
                    List<Faction> factions = new List<Faction>();
                    foreach (object response in responses)
                    {
                        factions.Add(ParseEddbFaction(response));
                    }
                    return factions.OrderBy(x => x.name).ToList();
                }
            }
            return null;
        }

        // Though not necessary at this time, it is possible to implement methods that query the data source 
        // based on multiple criteria (e.g. state, government, etc.) by adding multiple criteria to the queryList. 

        private static Faction ParseEddbFaction(object response)
        {
            JObject factionJson = ((JObject)response);
            Faction faction = new Faction();

            faction.EDDBID = (long)factionJson["id"];
            faction.name = (string)factionJson["name"];
            faction.updatedAt = (DateTime)factionJson["updated_at"];
            faction.Government = Government.FromName((string)factionJson["government"]);
            faction.Allegiance = Superpower.FromName((string)factionJson["allegiance"]);
            faction.State = State.FromName((string)factionJson["state"]);
            faction.homeSystemEddbId = (long?)factionJson["home_system_id"];
            faction.isplayer = (bool)factionJson["is_player_faction"];

            return faction;
        }
    }
}
