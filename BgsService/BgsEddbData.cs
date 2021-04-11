using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utilities;

namespace EddiBgsService
{
    partial class BgsService
    {
        /// <summary> The endpoint we will use for EDDB data queries (using the EDDB rest client) </summary>
        public const string systemEndpoint = "v4/populatedsystems?";

        // For the time being, we are only interested in collecting powerplay data from this endpoint. It could be used for many other query types if desired.
        public static class SystemParameters
        {
            /// <summary> FDev assigned System Address of the star system </summary>
            public const string systemAddress = "systemaddress";

            /// <summary> System name. </summary>
            public const string systemName = "name";

            /// <summary> Name of the allegiance </summary>
            public const string allegianceName = "allegiancename";

            /// <summary> Name of the government type. </summary>
            public const string governmentName = "governmentname";

            /// <summary> Comma seperated names of states. </summary>
            public const string stateNames = "statenames";

            /// <summary> The primary economy of the system. </summary>
            public const string primaryEconomyName = "primaryeconomyname";

            /// <summary> Comma seperated names of powers in influence in the system. </summary>
            public const string powerNames = "power";

            /// <summary> Whether the system is permit locked (bool). </summary>
            public const string permitRequired = "permit";

            /// <summary> The name of the security status in the system. </summary>
            public const string securityName = "securityname";

            /// <summary> Name of a faction present in the system. </summary>
            public const string factionName = "factionname";

            /// <summary> Presence type of the faction. </summary>
            public const string presenceType = "presencetype";
        }

        /// <summary> Can return null </summary>
        public StarSystem GetSystemByName(string systemName)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(SystemParameters.systemName, systemName)
            };
            return GetSystem(systemEndpoint, queryList);
        }

        /// <summary> Can return null </summary>
        public StarSystem GetSystemBySystemAddress(long? systemAddress)
        {
            if (systemAddress is null) { return null; }
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(SystemParameters.systemAddress, systemAddress)
            };
            return GetSystem(systemEndpoint, queryList);
        }

        private StarSystem GetSystem(string endpoint, List<KeyValuePair<string, object>> queryList)
        {
            return GetSystems(endpoint, queryList)?.FirstOrDefault();
        }

        /// <summary> Can return null </summary>
        public List<StarSystem> GetSystems(string endpoint, List<KeyValuePair<string, object>> queryList)
        {
            if (queryList.Count > 0)
            {
                List<object> responses = GetData(eddbRestClient, endpoint, queryList);

                if (responses?.Count > 0)
                {
                    List<StarSystem> systems = ParseSystemsParallel(responses);
                    return systems.OrderBy(x => x.systemname).ToList();
                }
            }
            return null;
        }

        public StarSystem GetSystemPowerplay(StarSystem system)
        {
            StarSystem bgsSystem = GetSystemBySystemAddress(system.systemAddress) ?? GetSystemByName(system.systemname);
            if (bgsSystem is null) { return system; }
            system.Power = bgsSystem.Power;
            system.powerState = bgsSystem.powerState;
            return system;
        }

        private List<StarSystem> ParseSystemsParallel(List<object> responses)
        {
            // it is OK to allow nulls into this list; they will be handled upstream
            List<StarSystem> systems = responses.AsParallel().Select(ParseSystem).ToList();
            return systems;
        }

        public StarSystem ParseSystem(object response)
        {
            try
            {
                Logging.Debug($"Response from Elite BGS eddbRestClient endpoint {systemEndpoint} is: ", response);

                IDictionary<string, object> systemJson = Deserializtion.DeserializeData(response.ToString());
                StarSystem system = new StarSystem
                {   
                    systemname = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(JsonParsing.getString(systemJson, "name")), // This is lower case by default from the API
                    systemAddress = long.Parse(JsonParsing.getString(systemJson, "ed_system_address")), // Stored in this API as a string
                    EDSMID = JsonParsing.getOptionalLong(systemJson, "edsm_id"),
                    updatedat = Dates.fromDateTimeToSeconds(JsonParsing.getDateTime("updated_at", systemJson))
                };

                // Get powerplay data
                // Note: EDDB does not report the following powerplay state ednames: 
                // `HomeSystem`, `InPrepareRadius`, `Prepared`, `Turmoil`
                // We can identify `HomeSystem` from static data, but  `InPrepareRadius`, `Prepared`, and `Turmoil`
                // are only available from the `Jumped` and `Location` events:
                // When in conflict, EDDB does not report the names of the conflicting powers.
                string power = JsonParsing.getString(systemJson, "power");
                string powerstate = JsonParsing.getString(systemJson, "power_state");
                if (!string.IsNullOrEmpty(power))
                {
                    system.Power = Power.FromName(power) ?? Power.None;
                }
                if (!string.IsNullOrEmpty(powerstate))
                {
                    system.powerState = system.systemname == system.Power?.headquarters 
                        ? PowerplayState.HomeSystem
                        : PowerplayState.FromName(powerstate) ?? PowerplayState.None;
                }

                return system;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "input", response },
                    { "exception", ex }
                };
                Logging.Error("Failed to parse BGS EDDB data.", data);
                return null;
            }
        }
    }
}
