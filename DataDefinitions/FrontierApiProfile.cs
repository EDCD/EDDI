using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary> Profile information returned by the companion app service.
    /// To prevent accidentally passing incorrect information to EDDN, inputs should not include any information from EDDI data definitions. </summary>
    public class FrontierApiProfile
    {
        /// <summary>The timestamp returned from the CAPI server</summary>
        public DateTime timestamp { get; set; }

        /// <summary>The JSON object</summary>
        public JObject json { get; set; }

        /// <summary>The commander</summary>
        public FrontierApiCommander Cmdr { get; set; }

        /// <summary>The current starsystem</summary>
        public string currentStarSystem { get; set; }

        /// <summary>The name of the last station the commander docked at</summary>
        public string LastStationName { get; set; }

        /// <summary>The market id of the last station the commander docked at</summary>
        public long? LastStationMarketID { get; set; }

        /// <summary>Whether this profile describes a docked commander</summary>
        public bool docked { get; set; }

        /// <summary>Whether this profile describes an on-foot commander</summary>
        public bool onFoot { get; set; }

        /// <summary>Whether this profile describes a currently living commander</summary>
        public bool alive { get; set; }

        /// <summary>The contexts (i.e. "capabilities") associated with this profile </summary>
        public FrontierApiContexts contexts { get; set; }

        /// <summary>Create a profile given the results from a /profile call</summary>
        public static FrontierApiProfile FromJson(JObject json)
        {
            FrontierApiProfile Profile = new FrontierApiProfile
            {
                json = json,
            };

            if (json?["timestamp"] != null)
            {
                Profile.timestamp = json["timestamp"].ToObject<DateTime?>() ?? DateTime.MinValue;
            }
            if (json?["commander"] != null)
            {
                FrontierApiCommander Commander = new FrontierApiCommander
                {
                    // Caution: The "id" property here may not match the FID returned from the player journal
                    name = (string)json["commander"]["name"],
                    combatrating = CombatRating.FromRank((int?)json["commander"]["rank"]?["combat"] ?? 0),
                    traderating = TradeRating.FromRank((int?)json["commander"]["rank"]?["trade"] ?? 0),
                    explorationrating = ExplorationRating.FromRank((int?)json["commander"]["rank"]?["explore"] ?? 0),
                    cqcrating = CQCRating.FromRank((int?)json["commander"]["rank"]?["cqc"] ?? 0),
                    empirerating = EmpireRating.FromRank((int?)json["commander"]["rank"]?["empire"] ?? 0),
                    federationrating = FederationRating.FromRank((int?)json["commander"]["rank"]?["federation"] ?? 0),
                    mercenaryrating = MercenaryRating.FromRank((int?)json["commander"]["rank"]?["soldier"] ?? 0),
                    exobiologistrating = ExobiologistRating.FromRank((int?)json["commander"]["rank"]?["exobiologist"] ?? 0),
                    crimerating = (int?)json["commander"]["rank"]?["crime"] ?? 0,
                    servicerating = (int?)json["commander"]["rank"]?["service"] ?? 0,
                    powerrating = (int?)json["commander"]["rank"]?["power"] ?? 0,

                    credits = (ulong?)json["commander"]["credits"] ?? 0,
                    debt = (long?)json["commander"]["debt"] ?? 0
                };
                Profile.Cmdr = Commander;
                Profile.docked = (bool)json["commander"]["docked"];
                Profile.onFoot = (bool)json["commander"]["onfoot"];
                Profile.alive = (bool)json["commander"]["alive"];

                if (json["commander"]["capabilities"] != null)
                {
                    var contexts = new FrontierApiContexts
                    {
                        allowCobraMkIV = (bool?)json["commander"]["capabilities"]["AllowCobraMkIV"] ?? false,
                        hasHorizons = (bool?)json["commander"]["capabilities"]["Horizons"] ?? false,
                        hasOdyssey = (bool?)json["commander"]["capabilities"]["Odyssey"] ?? false
                    };
                    Profile.contexts = contexts;
                }

                Profile.currentStarSystem = json["lastSystem"] == null ? null : (string)json["lastSystem"]["name"];

                if (json["lastStarport"] != null)
                {
                    Profile.LastStationName = ((string)json["lastStarport"]?["name"])?.ReplaceEnd('+');
                    Profile.LastStationMarketID = (long?)json["lastStarport"]?["id"];
                }
            }

            return Profile;
        }
    }
}
