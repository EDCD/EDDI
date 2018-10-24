using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiEddbService
{
    partial class EddbService
    {
        /// <summary> At least one system name is required. </summary>
        public static List<StarSystem> Systems(string[] systemNames, bool searchPopulated = true)
        {
            List<StarSystem> systems = new List<StarSystem>();
            foreach (string systemName in systemNames)
            {
                StarSystem system = GetSystem(new KeyValuePair<string, object>(SystemQuery.systemName, systemName)) ?? new StarSystem();
                if (system.EDDBID == null)
                {
                    system.name = systemName;
                }
            }
            return systems?.OrderBy(x => x.name).ToList();
        }

        /// <summary> At least one system EDDBID is required. </summary>
        public static List<StarSystem> Systems(long[] eddbIds, bool searchPopulated = true)
        {
            List<StarSystem> systems = new List<StarSystem>();
            foreach (long eddbId in eddbIds)
            {
                StarSystem system = GetSystem(new KeyValuePair<string, object>(SystemQuery.eddbId, eddbId)) ?? new StarSystem();
                if (system.EDDBID == null)
                {
                    system.EDDBID = eddbId;
                }
            }
            return systems?.OrderBy(x => x.name).ToList();
        }

        /// <summary> Exactly one system name is required. </summary>
        public static StarSystem System(string systemName, bool searchPopulated = true)
        {
            StarSystem system = GetSystem(new KeyValuePair<string, object>(SystemQuery.systemName, systemName), searchPopulated) ?? new StarSystem();
            if (system.EDDBID == null)
            {
                system.name = systemName;
            }
            return system ?? new StarSystem() { name = systemName };
        }

        /// <summary> Exactly one system EDDBID is required. </summary>
        public static StarSystem System(long eddbId, bool searchPopulated = true)
        {
            StarSystem system = GetSystem(new KeyValuePair<string, object>(SystemQuery.eddbId, eddbId), searchPopulated) ?? new StarSystem();
            if (system.EDDBID == null)
            {
                system.EDDBID = eddbId;
            }
            return system ?? new StarSystem() { EDDBID = eddbId };
        }

        /// <summary> Create a custom system query. </summary>
        private static StarSystem GetSystem(KeyValuePair<string, object> query, bool searchPopulated = true)
        {
            if (query.Value != null)
            {
                List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>
                {
                    query
                };

                List<object> responses = GetData(searchPopulated ? Endpoint.populatedSystems : Endpoint.systems, queryList);

                if (responses?.Count > 0)
                {
                    return ParseEddbSystem(responses[0]);
                }
            }
            return null;
        }

        /// <summary> Create a custom systems query. </summary>
        private static List<StarSystem> GetSystems(List<KeyValuePair<string, object>> queryList, bool searchPopulated = true)
        {
            if (queryList.Count > 0)
            {
                List<object> responses = GetData(searchPopulated ? Endpoint.populatedSystems : Endpoint.systems, queryList);

                if (responses?.Count > 0)
                {
                    List<StarSystem> systems = ParseEddbSystemsAsync(responses);
                    return systems;
                }
            }
            return null;
        }

        private static List<StarSystem> ParseEddbSystemsAsync(List<object> responses)
        {
            List<Task<StarSystem>> starSystemTasks = new List<Task<StarSystem>>();
            foreach (object response in responses)
            {
                starSystemTasks.Add(Task.Run(() => ParseEddbSystem(response)));
            }
            Task.WhenAll(starSystemTasks.ToArray());

            List<StarSystem> systems = new List<StarSystem>();
            foreach (Task<StarSystem> task in starSystemTasks)
            {
                StarSystem system = task.Result;
                if (system != null) { systems.Add(system); };
            }

            return systems;
        }

        private static StarSystem ParseEddbSystem(object response)
        {
            JObject systemJson = ((JObject)response);
            StarSystem StarSystem = new StarSystem { name = (string)systemJson["name"] };

            if ((bool?)systemJson["is_populated"] != null)
            {
                // We have real data so populate the rest of the data

                // General data
                StarSystem.EDDBID = (long?)systemJson["id"];
                StarSystem.EDSMID = (long?)systemJson["edsm_id"];
                StarSystem.x = (decimal?)systemJson["x"];
                StarSystem.y = (decimal?)systemJson["y"];
                StarSystem.z = (decimal?)systemJson["z"];
                StarSystem.Reserve = ReserveLevel.FromName((string)systemJson["reserve_type"]);

                // Populated system data
                StarSystem.population = (long?)systemJson["population"] == null ? 0 : (long?)systemJson["population"];
                // EDDB uses invariant / English localized economies
                StarSystem.Economies[0] = Economy.FromName((string)systemJson["primary_economy"]);
                // At present, EDDB does not provide any information about secondary economies.
                StarSystem.securityLevel = SecurityLevel.FromName((string)systemJson["security"]);

                // Controlling faction data
                if ((string)systemJson["controlling_minor_faction"] != null)
                {
                    Faction controllingFaction = new Faction()
                    {
                        EDDBID = (long?)systemJson["controlling_minor_faction_id"],
                        name = (string)systemJson["controlling_minor_faction"],
                        Government = Government.FromName((string)systemJson["government"]),
                        Allegiance = Superpower.FromName((string)systemJson["allegiance"]),
                        FactionState = FactionState.FromName((string)systemJson["state"])
                    };
                    StarSystem.Faction = controllingFaction;
                }

                // Powerplay details
                StarSystem.power = (string)systemJson["power"] == "None" ? null : (string)systemJson["power"];
                StarSystem.powerstate = (string)systemJson["power_state"];

                StarSystem.updatedat = Dates.fromDateTimeStringToSeconds((string)systemJson["updated_at"]);
            }

            StarSystem.lastupdated = DateTime.UtcNow;

            return StarSystem;
        }
    }
}
