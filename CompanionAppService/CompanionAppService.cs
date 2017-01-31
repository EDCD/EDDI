using CredentialManagement;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiCompanionAppService
{
    public class CompanionAppService
    {
        private static List<string> HARDPOINT_SIZES = new List<string>() { "Huge", "Large", "Medium", "Small", "Tiny" };

        // Translations from the internal names used by Frontier to clean human-readable
        private static Dictionary<string, string> shipTranslations = new Dictionary<string, string>()
        {
            { "Adder" , "Adder"},
            { "Anaconda", "Anaconda" },
            { "Asp", "Asp Explorer" },
            { "Asp_Scout", "Asp Scout" },
            { "BelugaLiner", "Beluga Liner" },
            { "CobraMkIII", "Cobra Mk. III" },
            { "CobraMkIV", "Cobra Mk. IV" },
            { "Cutter", "Imperial Cutter" },
            { "DiamondBack", "Diamondback Scout" },
            { "DiamondBackXL", "Diamondback Explorer" },
            { "Eagle", "Eagle" },
            { "Empire_Courier", "Imperial Courier" },
            { "Empire_Eagle", "Imperial Eagle" },
            { "Empire_Fighter", "Imperial Fighter" },
            { "Empire_Trader", "Imperial Clipper" },
            { "Federation_Corvette", "Federal Corvette" },
            { "Federation_Dropship", "Federal Dropship" },
            { "Federation_Dropship_MkII", "Federal Assault Ship" },
            { "Federation_Gunship", "Federal Gunship" },
            { "Federation_Fighter", "F63 Condor" },
            { "FerDeLance", "Fer-de-Lance" },
            { "Hauler", "Hauler" },
            { "Independant_Trader", "Keelback" },
            { "Orca", "Orca" },
            { "Python", "Python" },
            { "SideWinder", "Sidewinder" },
            { "Type6", "Type-6 Transporter" },
            { "Type7", "Type-7 Transporter" },
            { "Type9", "Type-9 Heavy" },
            { "Viper", "Viper Mk. III" },
            { "Viper_MkIV", "Viper Mk. IV" },
            { "Vulture", "Vulture" }
        };

        private static string BASE_URL = "https://companion.orerve.net";
        private static string ROOT_URL = "/";
        private static string LOGIN_URL = "/user/login";
        private static string CONFIRM_URL = "/user/confirm";
        private static string PROFILE_URL = "/profile";

        // We cache the profile to avoid spamming the service
        private Profile cachedProfile;
        private DateTime cachedProfileExpires;

        public enum State
        {
            NEEDS_LOGIN,
            NEEDS_CONFIRMATION,
            READY
        };
        public State CurrentState;

        public CompanionAppCredentials Credentials;

        private static CompanionAppService instance;

        private static readonly object instanceLock = new object();
        public static CompanionAppService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No companion API instance: creating one");
                            instance = new CompanionAppService();
                        }
                    }
                }
                return instance;
            }
        }
        private CompanionAppService()
        {
            Credentials = CompanionAppCredentials.FromFile();

            // Need to work out our current state.

            //If we're missing username and password then we need to log in again
            if (string.IsNullOrEmpty(Credentials.email) || string.IsNullOrEmpty(getPassword()))
            {
                CurrentState = State.NEEDS_LOGIN;
            }
            else if (string.IsNullOrEmpty(Credentials.machineId) || string.IsNullOrEmpty(Credentials.machineToken))
            {
                CurrentState = State.NEEDS_LOGIN;
            }
            else
            {
                // Looks like we're ready but test it to find out
                CurrentState = State.READY;
                try
                {
                    Profile();
                }
                catch (EliteDangerousCompanionAppException ex)
                {
                    Logging.Warn("Failed to obtain profile: " + ex.ToString());
                }
            }
        }

        ///<summary>Log in.  Throws an exception if it fails</summary>
        public void Login()
        {
            if (CurrentState != State.NEEDS_LOGIN)
            {
                // Shouldn't be here
                throw new EliteDangerousCompanionAppIllegalStateException("Service in incorrect state to login (" + CurrentState + ")");
            }

            HttpWebRequest request = GetRequest(BASE_URL + LOGIN_URL);

            // Send the request
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            string encodedUsername = WebUtility.UrlEncode(Credentials.email);
            string encodedPassword = WebUtility.UrlEncode(getPassword());
            byte[] data = Encoding.UTF8.GetBytes("email=" + encodedUsername + "&password=" + encodedPassword);
            request.ContentLength = data.Length;
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(data, 0, data.Length);
            }

            using (HttpWebResponse response = GetResponse(request))
            {
                if (response == null)
                {
                    throw new EliteDangerousCompanionAppException("Failed to contact API server");
                }
                if (response.StatusCode == HttpStatusCode.Found && response.Headers["Location"] == CONFIRM_URL)
                {
                    CurrentState = State.NEEDS_CONFIRMATION;
                }
                else if (response.StatusCode == HttpStatusCode.Found && response.Headers["Location"] == ROOT_URL)
                {
                    CurrentState = State.READY;
                }
                else
                {
                    throw new EliteDangerousCompanionAppAuthenticationException("Username or password incorrect");
                }
            }
        }

        ///<summary>Confirm a login.  Throws an exception if it fails</summary>
        public void Confirm(string code)
        {
            if (CurrentState != State.NEEDS_CONFIRMATION)
            {
                // Shouldn't be here
                throw new EliteDangerousCompanionAppIllegalStateException("Service in incorrect state to confirm login (" + CurrentState + ")");
            }

            HttpWebRequest request = GetRequest(BASE_URL + CONFIRM_URL);

            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            string encodedCode = WebUtility.UrlEncode(code);
            byte[] data = Encoding.UTF8.GetBytes("code=" + encodedCode);
            request.ContentLength = data.Length;
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(data, 0, data.Length);
            }

            using (HttpWebResponse response = GetResponse(request))
            {
                if (response == null)
                {
                    throw new EliteDangerousCompanionAppException("Failed to contact API server");
                }

                if (response.StatusCode == HttpStatusCode.Found && response.Headers["Location"] == ROOT_URL)
                {
                    CurrentState = State.READY;
                }
                else if (response.StatusCode == HttpStatusCode.Found && response.Headers["Location"] == LOGIN_URL)
                {
                    CurrentState = State.NEEDS_LOGIN;
                    throw new EliteDangerousCompanionAppAuthenticationException("Confirmation code incorrect or expired");
                }
            }
        }

        /// <summary>
        /// Log out of the companion API and remove local credentials
        /// </summary>
        public void Logout()
        {
            // Remove everything other than the local email address
            Credentials = CompanionAppCredentials.FromFile();
            Credentials.machineToken = null;
            Credentials.machineId = null;
            Credentials.appId = null;
            setPassword(null);
            Credentials.ToFile();
            CurrentState = State.NEEDS_LOGIN;
        }

        public Profile Profile(bool forceRefresh = false)
        {
            Logging.Debug("Entered");
            if (CurrentState != State.READY)
            {
                // Shouldn't be here
                Logging.Debug("Service in incorrect state to provide profile (" + CurrentState + ")");
                Logging.Debug("Leaving");
                throw new EliteDangerousCompanionAppIllegalStateException("Service in incorrect state to provide profile (" + CurrentState + ")");
            }
            if ((!forceRefresh) && cachedProfileExpires > DateTime.Now)
            {
                // return the cached version
                Logging.Debug("Returning cached profile");
                Logging.Debug("Leaving");
                return cachedProfile;
            }

            string data = obtainProfile();

            if (data == null || data == "Profile unavailable")
            {
                // Happens if there is a problem with the API.  Logging in again might clear this...
                relogin();
                data = obtainProfile();
                if (data == null || data == "Profile unavailable")
                {
                    // No luck with a relogin; give up
                    SpeechService.Instance.Say(null, "Access to companion API data has been lost.  Please update the companion app information to re-establish the connection.", false);
                    Logout();
                    throw new EliteDangerousCompanionAppException("Failed to obtain data from Frontier server (" + CurrentState + ")");
                }
            }

            try
            {
                cachedProfile = ProfileFromJson(data);
            }
            catch (JsonException ex)
            {
                Logging.Error("Failed to parse companion profile", ex);
                cachedProfile = null;
            }

            if (cachedProfile != null)
            {
                cachedProfileExpires = DateTime.Now.AddSeconds(30);
                Logging.Debug("Profile is " + JsonConvert.SerializeObject(cachedProfile));
            }

            Logging.Debug("Leaving");
            return cachedProfile;
        }

        private string obtainProfile()
        {
            HttpWebRequest request = GetRequest(BASE_URL + PROFILE_URL);
            using (HttpWebResponse response = GetResponse(request))
            {
                if (response == null)
                {
                    Logging.Debug("Failed to contact API server");
                    Logging.Debug("Leaving");
                    throw new EliteDangerousCompanionAppException("Failed to contact API server");
                }

                if (response.StatusCode == HttpStatusCode.Found && response.Headers["Location"] == LOGIN_URL)
                {
                    return null;
                }

                return getResponseData(response);
            }
        }

        /**
         * Try to relogin if there is some issue that requires it.
         * Throws an exception if it failed to log in.
         */
        private void relogin()
        {
            // Need to log in again.
            CurrentState = State.NEEDS_LOGIN;
            Login();
            if (CurrentState != State.READY)
            {
                Logging.Debug("Service in incorrect state to provide profile (" + CurrentState + ")");
                Logging.Debug("Leaving");
                throw new EliteDangerousCompanionAppIllegalStateException("Service in incorrect state to provide profile (" + CurrentState + ")");
            }
        }

        /**
         * Obtain the response data from an HTTP web response
         */
        private string getResponseData(HttpWebResponse response)
        {
            // Obtain and parse our response
            var encoding = response.CharacterSet == ""
                    ? Encoding.UTF8
                    : Encoding.GetEncoding(response.CharacterSet);

            Logging.Debug("Reading response");
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream, encoding);
                string data = reader.ReadToEnd();
                if (data == null || data.Trim() == "")
                {
                    Logging.Warn("No data returned");
                    return null;
                }
                Logging.Debug("Data is " + data);
                return data;
            }
        }

        // Set up a request with the correct parameters for talking to the companion app
        private HttpWebRequest GetRequest(string url)
        {
            Logging.Debug("Entered");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            CookieContainer cookieContainer = new CookieContainer();
            AddCompanionAppCookie(cookieContainer, Credentials);
            AddMachineIdCookie(cookieContainer, Credentials);
            AddMachineTokenCookie(cookieContainer, Credentials);
            request.CookieContainer = cookieContainer;
            request.AllowAutoRedirect = false;
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Mobile/11D257";

            Logging.Debug("Leaving");
            return request;
        }

        // Obtain a response, ensuring that we obtain the response's cookies
        private HttpWebResponse GetResponse(HttpWebRequest request)
        {
            Logging.Debug("Entered");
            Logging.Debug("Requesting " + request.RequestUri);

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                Logging.Warn("Failed to obtain response, error code " + wex.Status);
                return null;
            }
            Logging.Debug("Response is " + JsonConvert.SerializeObject(response));
            UpdateCredentials(response);
            Credentials.ToFile();

            Logging.Debug("Leaving");
            return response;
        }

        private void UpdateCredentials(HttpWebResponse response)
        {
            Logging.Debug("Entered");

            // Obtain the cookies from the raw information available to us
            string cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
            if (cookieHeader != null)
            {
                Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                if (companionAppMatch.Success)
                {
                    Credentials.appId = companionAppMatch.Groups[1].Value;
                }
                Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                if (machineIdMatch.Success)
                {
                    Credentials.machineId = machineIdMatch.Groups[1].Value;
                }
                Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                if (machineTokenMatch.Success)
                {
                    Credentials.machineToken = machineTokenMatch.Groups[1].Value;
                }
            }

            Logging.Debug("Leaving");
        }

        private static void AddCompanionAppCookie(CookieContainer cookies, CompanionAppCredentials credentials)
        {
            if (cookies != null && credentials.appId != null)
            {
                var appCookie = new Cookie();
                appCookie.Domain = "companion.orerve.net";
                appCookie.Path = "/";
                appCookie.Name = "CompanionApp";
                appCookie.Value = credentials.appId;
                appCookie.Secure = false;
                cookies.Add(appCookie);
            }
        }

        private static void AddMachineIdCookie(CookieContainer cookies, CompanionAppCredentials credentials)
        {
            if (cookies != null && credentials.machineId != null)
            {
                var machineIdCookie = new Cookie();
                machineIdCookie.Domain = "companion.orerve.net";
                machineIdCookie.Path = "/";
                machineIdCookie.Name = "mid";
                machineIdCookie.Value = credentials.machineId;
                machineIdCookie.Secure = true;
                // The expiry is embedded in the cookie value
                if (credentials.machineId.IndexOf("%7C") == -1)
                {
                    machineIdCookie.Expires = DateTime.Now.AddDays(7);
                }
                else
                {
                    string expiryseconds = credentials.machineId.Substring(0, credentials.machineId.IndexOf("%7C"));
                    DateTime expiryDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    try
                    {
                        expiryDateTime = expiryDateTime.AddSeconds(Convert.ToInt64(expiryseconds));
                        machineIdCookie.Expires = expiryDateTime;
                    }
                    catch (Exception)
                    {
                        Logging.Warn("Failed to handle machine id expiry seconds " + expiryseconds);
                        machineIdCookie.Expires = DateTime.Now.AddDays(7);
                    }
                }
                cookies.Add(machineIdCookie);
            }
        }

        private static void AddMachineTokenCookie(CookieContainer cookies, CompanionAppCredentials credentials)
        {
            if (cookies != null && credentials.machineToken != null)
            {
                var machineTokenCookie = new Cookie();
                machineTokenCookie.Domain = "companion.orerve.net";
                machineTokenCookie.Path = "/";
                machineTokenCookie.Name = "mtk";
                machineTokenCookie.Value = credentials.machineToken;
                machineTokenCookie.Secure = true;
                // The expiry is embedded in the cookie value
                if (credentials.machineToken.IndexOf("%7C") == -1)
                {
                    machineTokenCookie.Expires = DateTime.Now.AddDays(7);
                }
                else
                {
                    string expiryseconds = credentials.machineToken.Substring(0, credentials.machineToken.IndexOf("%7C"));
                    DateTime expiryDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    try
                    {
                        expiryDateTime = expiryDateTime.AddSeconds(Convert.ToInt64(expiryseconds));
                        machineTokenCookie.Expires = expiryDateTime;
                    }
                    catch (Exception)
                    {
                        Logging.Warn("Failed to handle machine token expiry seconds " + expiryseconds);
                        machineTokenCookie.Expires = DateTime.Now.AddDays(7);
                    }
                }
                cookies.Add(machineTokenCookie);
            }
        }

        /// <summary>Create a  profile given the results from a /profile call</summary>
        public static Profile ProfileFromJson(string data)
        {
            Logging.Debug("Entered");
            Profile profile = null;
            if (data != null && data != "")
            {
                profile = ProfileFromJson(JObject.Parse(data));
            }
            AugmentCmdrInfo(profile.Cmdr);
            Logging.Debug("Leaving");
            return profile;
        }

        /// <summary>Create a profile given the results from a /profile call</summary>
        public static Profile ProfileFromJson(JObject json)
        {
            Logging.Debug("Entered");
            Profile Profile = new Profile();

            if (json["commander"] != null)
            {
                Commander Commander = new Commander();
                Commander.name = (string)json["commander"]["name"];

                Commander.combatrating = CombatRating.FromRank((int)json["commander"]["rank"]["combat"]);
                Commander.traderating = TradeRating.FromRank((int)json["commander"]["rank"]["trade"]);
                Commander.explorationrating = ExplorationRating.FromRank((int)json["commander"]["rank"]["explore"]);
                Commander.cqcrating = CQCRating.FromRank((int)json["commander"]["rank"]["cqc"]);
                Commander.empirerating = EmpireRating.FromRank((int)json["commander"]["rank"]["empire"]);
                Commander.federationrating = FederationRating.FromRank((int)json["commander"]["rank"]["federation"]);

                Commander.credits = (long)json["commander"]["credits"];
                Commander.debt = (long)json["commander"]["debt"];
                Profile.Cmdr = Commander;

                string systemName = json["lastSystem"] == null ? null : (string)json["lastSystem"]["name"];
                if (systemName != null)
                {
                    Profile.CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(systemName);
                }

                Profile.Ship = ShipFromProfile(json["ship"]);

                Profile.Shipyard = ShipyardFromProfile(json, ref Profile);

                AugmentShipInfo(Profile.Ship, Profile.Shipyard);

                if (json["lastStarport"] != null)
                {
                    Profile.LastStation =  Profile.CurrentStarSystem.stations.Find(s => s.name == (string)json["lastStarport"]["name"]);
                    if (Profile.LastStation == null)
                    {
                        // Don't have a station so make one up
                        Profile.LastStation = new Station();
                        Profile.LastStation.name = (string)json["lastStarport"]["name"];
                    }

                    Profile.LastStation.systemname = Profile.CurrentStarSystem.name;
                    Profile.LastStation.outfitting = OutfittingFromProfile(json);
                    Profile.LastStation.commodities = CommoditiesFromProfile(json);
                    Profile.LastStation.shipyard = ShipyardFromProfile(json);
                }
            }

            Logging.Debug("Leaving");
            return Profile;
        }

        private static void AugmentCmdrInfo(Commander cmdr)
        {
            Logging.Debug("Entered");
            //if (cmdr != null)
            //{
            //    CommanderConfiguration cmdrConfiguration = CommanderConfiguration.FromFile();
            //    if (cmdrConfiguration.PhoneticName == null || cmdrConfiguration.PhoneticName.Trim().Length == 0)
            //    {
            //        cmdr.phoneticname = null;
            //    }
            //    else
            //    {
            //        cmdr.phoneticname = cmdrConfiguration.PhoneticName;
            //    }
            //}
            Logging.Debug("Leaving");
        }

        private static void AugmentShipInfo(Ship ship, List<Ship> storedShips)
        {
            Logging.Debug("Entered");
            ShipsConfiguration shipsConfiguration = ShipsConfiguration.FromFile();
            Dictionary<int, Ship> lookup = shipsConfiguration.Ships.ToDictionary(o => o.LocalId);

            Ship shipConfig;
            // Start with our current ship
            if (lookup.TryGetValue(ship.LocalId, out shipConfig))
            {
                // Already exists; grab the relevant information and supplement it
                // Ship config name might be just whitespace, in which case we unset it
                if (shipConfig.name != null && shipConfig.name.Trim().Length > 0)
                {
                    ship.name = shipConfig.name.Trim();
                }
                if (shipConfig.phoneticname != null && shipConfig.phoneticname.Trim().Length > 0)
                {
                    ship.phoneticname = shipConfig.phoneticname.Trim();
                }
                ship.role = shipConfig.role;
            }
            else
            {
                // Doesn't already exist; add a default role
                ship.role = Role.MultiPurpose;
            }

            // Work through our shipyard
            foreach (Ship storedShip in storedShips)
            {
                if (lookup.TryGetValue(storedShip.LocalId, out shipConfig))
                {
                    // Already exists; grab the relevant information and supplement it
                    storedShip.name = shipConfig.name;
                    storedShip.phoneticname = shipConfig.phoneticname;
                    storedShip.role = shipConfig.role;
                }
                else
                {
                    // Doesn't already exist; add a default role
                    storedShip.role = Role.MultiPurpose;
                }
            }

            // Update our configuration with the new data (this also removes any old redundant ships)
            shipsConfiguration.Ships = new List<Ship>();
            shipsConfiguration.Ships.Add(ship);
            shipsConfiguration.Ships.AddRange(storedShips);
            shipsConfiguration.ToFile();
            Logging.Debug("Leaving");
        }

        public static Ship ShipFromProfile(dynamic json)
        {
            Logging.Debug("Entered");
            if (json == null)
            {
                Logging.Debug("Leaving");
                return null;
            }

            Ship Ship = ShipDefinitions.FromEDModel((string)json["name"]);

            Ship.json = json.ToString(Formatting.None);

            Ship.LocalId = json["id"];

            // Some ship information is just skeleton data of the ship's ID.  Use value as our canary to see if there is more data
            if (json["value"] != null)
            {
                Ship.value = (long)json["value"]["hull"] + (long)json["value"]["modules"];

                Ship.cargocapacity = (int)json["cargo"]["capacity"];
                Ship.cargocarried = (int)json["cargo"]["qty"];

                // Be sensible with health - round it unless it's very low
                decimal Health = (decimal)json["health"]["hull"] / 10000;
                if (Health < 5)
                {
                    Ship.health = Math.Round(Health, 1);
                }
                else
                {
                    Ship.health = Math.Round(Health);
                }

                // Obtain the internals
                Ship.bulkheads = ModuleFromProfile("Armour", json["modules"]["Armour"]);
                Ship.powerplant = ModuleFromProfile("PowerPlant", json["modules"]["PowerPlant"]);
                Ship.thrusters = ModuleFromProfile("MainEngines", json["modules"]["MainEngines"]);
                Ship.frameshiftdrive = ModuleFromProfile("FrameShiftDrive", json["modules"]["FrameShiftDrive"]);
                Ship.lifesupport = ModuleFromProfile("LifeSupport", json["modules"]["LifeSupport"]);
                Ship.powerdistributor = ModuleFromProfile("PowerDistributor", json["modules"]["PowerDistributor"]);
                Ship.sensors = ModuleFromProfile("Radar", json["modules"]["Radar"]);
                Ship.fueltank = ModuleFromProfile("FuelTank", json["modules"]["FuelTank"]);
                Ship.fueltankcapacity = (decimal)Math.Pow(2, Ship.fueltank.@class);
                Ship.fueltanktotalcapacity = (decimal)json["fuel"]["main"]["capacity"];

                // Obtain the hardpoints.  Hardpoints can come in any order so first parse them then second put them in the correct order
                Dictionary<string, Hardpoint> hardpoints = new Dictionary<string, Hardpoint>();
                foreach (JProperty module in json["modules"])
                {
                    if (module.Name.Contains("Hardpoint"))
                    {
                        hardpoints.Add(module.Name, HardpointFromProfile(module));
                    }
                }

                foreach (string size in HARDPOINT_SIZES)
                {
                    for (int i = 1; i < 12; i++)
                    {
                        Hardpoint hardpoint;
                        hardpoints.TryGetValue(size + "Hardpoint" + i, out hardpoint);
                        if (hardpoint != null)
                        {
                            Ship.hardpoints.Add(hardpoint);
                        }
                    }
                }

                // Obtain the compartments
                foreach (dynamic module in json["modules"])
                {
                    if (module.Name.Contains("Slot"))
                    {
                        Ship.compartments.Add(CompartmentFromProfile(module));
                    }
                }

                // Obtain the cargo
                Ship.cargo = new List<Cargo>();
                if (json["cargo"] != null && json["cargo"]["items"] != null)
                {
                    foreach (dynamic cargoJson in json["cargo"]["items"])
                    {
                        if (cargoJson != null && cargoJson["commodity"] != null)
                        {
                            string name = (string)cargoJson["commodity"];
                            Cargo cargo = new Cargo();
                            cargo.commodity = CommodityDefinitions.FromName(name);
                            if (cargo.commodity.name == null)
                            {
                                // Unknown commodity; log an error so that we can update the definitions
                                Logging.Error("No commodity definition for cargo", cargoJson.ToString(Formatting.None));
                                cargo.commodity.name = name;
                            }
                            cargo.amount = (int)cargoJson["qty"];
                            cargo.price = (long)cargoJson["value"] / cargo.amount;
                            cargo.missionid = (long?)cargoJson["mission"];
                            cargo.stolen = ((int?)(long?)cargoJson["marked"]) == 1;

                            Ship.cargo.Add(cargo);
                        }
                    }
                }
            }

            Logging.Debug("Leaving");
            return Ship;
        }

        public static Hardpoint HardpointFromProfile(dynamic json)
        {
            Hardpoint Hardpoint = new Hardpoint();

            string name = json.Name;
            if (name.StartsWith("Huge"))
            {
                Hardpoint.size = 4;
            }
            else if (name.StartsWith("Large"))
            {
                Hardpoint.size = 3;
            }
            else if (name.StartsWith("Medium"))
            {
                Hardpoint.size = 2;
            }
            else if (name.StartsWith("Small"))
            {
                Hardpoint.size = 1;
            }
            else if (name.StartsWith("Tiny"))
            {
                Hardpoint.size = 0;
            }

            if (json.Value is JObject)
            {
                JToken value;
                if (json.Value.TryGetValue("module", out value))
                {
                    Hardpoint.module = ModuleFromProfile(name, json.Value);
                }
            }

            return Hardpoint;
        }

        public static List<Ship> ShipyardFromProfile(dynamic json, ref Profile profile)
        {
            Logging.Debug("Entered");

            Ship currentShip = profile.Ship;

            List<Ship> StoredShips = new List<Ship>();

            foreach (dynamic shipJson in json["ships"])
            {
                if (shipJson != null)
                {
                    // Take underlying value if present
                    JObject shipObj = shipJson.Value == null ? shipJson : shipJson.Value;
                    if (shipObj != null)
                    {
                        if ((int)shipObj["id"] != currentShip.LocalId)
                        {
                            Ship ship = ShipFromProfile(shipObj);

                            if (shipObj["starsystem"] != null)
                            {
                                // If we have a starsystem it means that the ship is stored
                                ship.starsystem = (string)shipObj["starsystem"]["name"];
                                ship.station = (string)shipObj["station"]["name"];

                                StoredShips.Add(ship);
                            }
                        }
                    }
                }
            }

            Logging.Debug("Leaving");
            return StoredShips;
        }

        public static Compartment CompartmentFromProfile(dynamic json)
        {
            Compartment Compartment = new Compartment();

            // Compartments have name of form "Slotnn_Sizenn"
            Match matches = Regex.Match((string)json.Name, @"Size([0-9]+)");
            if (matches.Success)
            {
                Compartment.size = Int32.Parse(matches.Groups[1].Value);

                if (json.Value is JObject)
                {
                    JToken value;
                    if (json.Value.TryGetValue("module", out value))
                    {
                        Compartment.module = ModuleFromProfile((string)json.Name, json.Value);
                    }
                }
            }
            return Compartment;
        }

        // Obtain the list of outfitting modules from the profile
        public static List<Module> OutfittingFromProfile(dynamic json)
        {
            List<Module> Modules = new List<Module>();

            if (json["lastStarport"] != null && json["lastStarport"]["modules"] != null)
            {
                foreach (dynamic moduleJson in json["lastStarport"]["modules"])
                {
                    dynamic module = moduleJson.Value;
                    // Not interested in paintjobs, decals, ...
                    if (module["category"] == "weapon" || module["category"] == "module")
                    {
                        Module Module = ModuleDefinitions.ModuleFromEliteID((long)module["id"]);
                        if (Module.name == null)
                        {
                            // Unknown module; log an error so that we can update the definitions
                            Logging.Error("No definition for outfitting module", module.ToString(Formatting.None));
                            // Set the name from the JSON
                            Module.EDName = (string)module["name"];
                        }
                        Module.price = module["cost"];
                        Modules.Add(Module);
                    }
                }
            }

            return Modules;
        }

        // Obtain the list of commodities from the profile
        public static List<Commodity> CommoditiesFromProfile(dynamic json)
        {
            List<Commodity> Commodities = new List<Commodity>();

            if (json["lastStarport"] != null && json["lastStarport"]["commodities"] != null)
            {
                foreach (dynamic commodity in json["lastStarport"]["commodities"])
                {
                    dynamic commodityJson = commodity.Value;
                    Commodity Commodity = CommodityDefinitions.CommodityFromEliteID((long)commodity["id"]);
                    if (Commodity == null || Commodity.name == null)
                    {
                        Commodity = new Commodity();
                        Commodity.EDName = (string)commodity["name"];
                        Commodity.category = (string)commodity["categoryName"];
                    }
                    Commodity.avgprice = (int)commodity["meanPrice"];
                    Commodity.buyprice = (int)commodity["buyPrice"];
                    Commodity.stock = (int)commodity["stock"];
                    Commodity.stockbracket = (dynamic)commodity["stockBracket"];
                    Commodity.sellprice = (int)commodity["sellPrice"];
                    Commodity.demand = (int)commodity["demand"];
                    Commodity.demandbracket = (dynamic)commodity["demandBracket"];

                    List<string> StatusFlags = new List<string>();
                    foreach (dynamic statusFlag in commodity["statusFlags"])
                    {
                        StatusFlags.Add((string)statusFlag);
                    }
                    Commodity.StatusFlags = StatusFlags;
                    Commodities.Add(Commodity);
                }
            }

            return Commodities;
        }

        // Obtain the list of ships available at the station from the profile
        public static List<Ship> ShipyardFromProfile(dynamic json)
        {
            List<Ship> Ships = new List<Ship>();

            // This information is not available at current from the companion app JSON so leave it empty

            return Ships;
        }

        public static Module ModuleFromProfile(string name, JObject json)
        {
            long id = (long)json["module"]["id"];
            Module module = ModuleDefinitions.ModuleFromEliteID(id);
            if (module.name == null)
            {
                // Unknown module; log an error so that we can update the definitions
                Logging.Error("No definition for ship module", json["module"].ToString(Formatting.None));
            }

            module.price = (long)json["module"]["value"];
            module.enabled = (bool)json["module"]["on"];
            module.priority = (int)json["module"]["priority"];
            // Be sensible with health - round it unless it's very low
            decimal Health = (decimal)json["module"]["health"] / 10000;
            if (Health < 5)
            {
                module.health = Math.Round(Health, 1);
            }
            else
            {
                module.health = Math.Round(Health);
            }

            // Flag if module has modifications
            if (json["module"]["modifiers"] != null)
            {
                module.modified = true;
            }

            return module;
        }

        public void setPassword(string password)
        {
            using (var credential = new Credential())
            {
                credential.Password = password;
                credential.Target = @"EDDIFDevApi";
                credential.Type = CredentialType.Generic;
                credential.PersistanceType = PersistanceType.Enterprise;
                credential.Save();
            }
        }

        private string getPassword()
        {
            using (var credential = new Credential())
            {
                credential.Target = @"EDDIFDevApi";
                credential.Load();
                return credential.Password;
            }
        }
    }
}
