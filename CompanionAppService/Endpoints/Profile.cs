using EddiDataDefinitions;
using EddiSpeechService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EddiCompanionAppService
{
    public partial class CompanionAppService
    {
        private const string PROFILE_URL = "/profile";

        // We cache the profile to avoid spamming the service
        private FrontierApiProfile cachedProfile;
        private DateTime cachedProfileExpires;

        public FrontierApiProfile Profile(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedProfileExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug("Returning cached profile");
                return cachedProfile;
            }

            try
            {
                string data = obtainData(ServerURL() + PROFILE_URL, out DateTime timestamp);

                if (data == null || data == "Profile unavailable")
                {
                    // Happens if there is a problem with the API.  Logging in again might clear this...
                    relogin();
                    if (CurrentState != State.Authorized)
                    {
                        // No luck; give up
                        SpeechService.Instance.Say(null, Properties.CapiResources.frontier_api_lost, 0);
                        Logout();
                    }
                    else
                    {
                        // Looks like login worked; try again
                        data = obtainData(ServerURL() + PROFILE_URL, out timestamp);

                        if (data == null || data == "Profile unavailable")

                        {
                            // No luck with a relogin; give up
                            SpeechService.Instance.Say(null, Properties.CapiResources.frontier_api_lost, 0);
                            Logout();
                            throw new EliteDangerousCompanionAppException("Failed to obtain data from Frontier server (" + CurrentState + ")");
                        }
                    }
                }

                try
                {
                    cachedProfile = ProfileFromJson(data, timestamp);
                }
                catch (JsonException ex)
                {
                    Logging.Error("Failed to parse companion profile", ex);
                    cachedProfile = null;
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as Rollbar is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            if (cachedProfile != null)
            {
                cachedProfileExpires = DateTime.UtcNow.AddSeconds(30);
                Logging.Debug("Profile is " + JsonConvert.SerializeObject(cachedProfile));
            }

            return cachedProfile;
        }

        /// <summary>Create a  profile given the results from a /profile call</summary>
        public static FrontierApiProfile ProfileFromJson(string data, DateTime timestamp)
        {
            if (!string.IsNullOrEmpty(data))
            {
                return ProfileFromJson(JObject.Parse(data), timestamp);
            }
            return null;
        }

        /// <summary>Create a profile given the results from a /profile call</summary>
        public static FrontierApiProfile ProfileFromJson(JObject json, DateTime timestamp)
        {
            FrontierApiProfile Profile = new FrontierApiProfile
            {
                json = json,
                timestamp = timestamp
            };

            if (json["commander"] != null)
            {
                FrontierApiCommander Commander = new FrontierApiCommander
                {
                    // Caution: The "id" property here may not match the FID returned from the player journal
                    name = (string)json["commander"]["name"],
                    combatrating = CombatRating.FromRank((int?)json["commander"]["rank"]["combat"] ?? 0),
                    traderating = TradeRating.FromRank((int?)json["commander"]["rank"]["trade"] ?? 0),
                    explorationrating = ExplorationRating.FromRank((int?)json["commander"]["rank"]["explore"] ?? 0),
                    cqcrating = CQCRating.FromRank((int?)json["commander"]["rank"]["cqc"] ?? 0),
                    empirerating = EmpireRating.FromRank((int?)json["commander"]["rank"]["empire"] ?? 0),
                    federationrating = FederationRating.FromRank((int?)json["commander"]["rank"]["federation"] ?? 0),
                    mercenaryrating = MercenaryRating.FromRank((int?)json["commander"]["rank"]["soldier"] ?? 0),
                    exobiologistrating = ExobiologistRating.FromRank((int?)json["commander"]["rank"]["exobiologist"] ?? 0),
                    crimerating = (int?)json["commander"]["rank"]["crime"] ?? 0,
                    servicerating = (int?)json["commander"]["rank"]["service"] ?? 0,
                    powerrating = (int?)json["commander"]["rank"]["power"] ?? 0,

                    credits = (long?)json["commander"]["credits"] ?? 0,
                    debt = (long?)json["commander"]["debt"] ?? 0
                };
                Profile.Cmdr = Commander;
                Profile.docked = (bool)json["commander"]["docked"];
                Profile.onFoot = (bool)json["commander"]["onfoot"];
                Profile.alive = (bool)json["commander"]["alive"];

                if (json["commander"]["capabilities"] != null)
                {
                    var contexts = new FrontierApiProfileContexts
                    {
                        allowCobraMkIV = (bool?)json["commander"]["capabilities"]["AllowCobraMkIV"] ?? false,
                        hasHorizons = (bool?)json["commander"]["capabilities"]["Horizons"] ?? false,
                        hasOdyssey = (bool?)json["commander"]["capabilities"]["Odyssey"] ?? false
                    };
                    Profile.contexts = contexts;
                }

                string systemName = json["lastSystem"] == null ? null : (string)json["lastSystem"]["name"];
                if (systemName != null)
                {
                    Profile.CurrentStarSystem = new FrontierApiProfileStarSystem
                    {
                        // Caution: The "id" property here may not match the systemAddress
                        systemName = systemName,
                        // Caution: The "faction" property here may return the edName for the faction rather than the invariant name
                    };
                }

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