using EliteDangerousDataDefinitions;
using EliteDangerousDataProviderService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EliteDangerousCompanionAppService
{
    public class CompanionAppService
    {
        // Translations from the internal names used by Frontier to clean human-readable
        private static Dictionary<string, string> shipTranslations = new Dictionary<string, string>()
        {
            { "Adder" , "Adder"},
            { "Anaconda", "Anaconda" },
            { "Asp", "Asp Explorer" },
            { "Asp_Scout", "Asp Scout" },
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
            { "Federation_dropship", "Federal Dropship" },
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

        private CompanionAppCredentials credentials;

        public CompanionAppService(CompanionAppCredentials credentials)
        {
            this.credentials = credentials;
        }

        ///<summary>Log in.  Returns credentials, or throws an exception if it fails</summary>
        public static CompanionAppCredentials Login(CompanionAppCredentials credentials, string username, string password)
        {
            var cookieContainer = new CookieContainer();
            AddCompanionAppCookie(cookieContainer, credentials);
            AddMachineIdCookie(cookieContainer, credentials);
            AddMachineTokenCookie(cookieContainer, credentials);

            // Send the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BASE_URL + LOGIN_URL);
            request.AllowAutoRedirect = false;  // Don't redirect or we lose the cookies

            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Mobile/11D257";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            string encodedUsername = WebUtility.UrlEncode(username);
            string encodedPassword = WebUtility.UrlEncode(password);
            byte[] data = Encoding.UTF8.GetBytes("email=" + encodedUsername + "&password=" + encodedPassword);
            request.ContentLength = data.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    // Problem with the service
                    throw new EliteDangerousCompanionAppAuthenticationException("There is a problem with the Elite: Dangerous servers; please try again later");
                }
                string location = response.Headers[HttpResponseHeader.Location];
                if (CONFIRM_URL != location && ROOT_URL != location)
                {
                    throw new EliteDangerousCompanionAppAuthenticationException("Username or password incorrect");
                }

                // Obtain the cookies from the raw information available to us
                string cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
                if (cookieHeader != null)
                {
                    Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                    if (companionAppMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.appId = companionAppMatch.Groups[1].Value;
                    }
                    Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                    if (machineIdMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.machineId = machineIdMatch.Groups[1].Value;
                    }
                    Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                    if (machineTokenMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.machineToken = machineTokenMatch.Groups[1].Value;
                    }
                }
            }

            // At this stage we should have the CompanionApp and mid values (and maybe the mtk, but not necessarily so don't check for it)
            if (credentials.appId == null)
            {
                throw new EliteDangerousCompanionAppAuthenticationException("Credentials are missing companion app ID");
            }
            if (credentials.machineId == null)
            {
                throw new EliteDangerousCompanionAppAuthenticationException("Credentials are missing machine ID");
            }

            return credentials;
        }

        ///<summary>Confirm a login.  Returns credentials, or throws an exception if it fails</summary>
        public static CompanionAppCredentials Confirm(CompanionAppCredentials credentials, string code)
        {
            var cookieContainer = new CookieContainer();
            AddCompanionAppCookie(cookieContainer, credentials);
            AddMachineIdCookie(cookieContainer, credentials);
            AddMachineTokenCookie(cookieContainer, credentials);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BASE_URL + CONFIRM_URL);
            request.AllowAutoRedirect = false;
            request.CookieContainer = cookieContainer;
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Mobile/11D257";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            string encodedCode = WebUtility.UrlEncode(code);
            byte[] data = Encoding.UTF8.GetBytes("code=" + encodedCode);
            request.ContentLength = data.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                string location = response.Headers[HttpResponseHeader.Location];
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    // Problem with the service
                    throw new EliteDangerousCompanionAppAuthenticationException("There is a problem with the Elite: Dangerous servers; please try again later");
                }
                if (ROOT_URL != location)
                {
                    throw new EliteDangerousCompanionAppAuthenticationException("Confirmation code incorrect");
                }

                // Refresh the cookies from the raw information available to us
                string cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
                if (cookieHeader != null)
                {
                    Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                    if (companionAppMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.appId = companionAppMatch.Groups[1].Value;
                    }
                    Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                    if (machineIdMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.machineId = machineIdMatch.Groups[1].Value;
                    }
                    Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                    if (machineTokenMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.machineToken = machineTokenMatch.Groups[1].Value;
                    }
                }

                // At this stage we should have the CompanionApp, mid and mtk values
                if (credentials.appId == null)
                {
                    throw new EliteDangerousCompanionAppAuthenticationException("Credentials are missing companion app ID");
                }
                if (credentials.machineId == null)
                {
                    throw new EliteDangerousCompanionAppAuthenticationException("Credentials are missing machine ID");
                }
                if (credentials.machineToken == null)
                {
                    throw new EliteDangerousCompanionAppAuthenticationException("Credentials are missing machine token");
                }
            }

            return credentials;
        }

        public Commander Profile()
        {
            var cookieContainer = new CookieContainer();
            AddCompanionAppCookie(cookieContainer, credentials);
            AddMachineIdCookie(cookieContainer, credentials);
            AddMachineTokenCookie(cookieContainer, credentials);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BASE_URL + PROFILE_URL);
            request.AllowAutoRedirect = false;
            request.CookieContainer = cookieContainer;
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Mobile/11D257";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                string location = response.Headers[HttpResponseHeader.Location];
                if (LOGIN_URL == location || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    // The user needs to log in again
                    throw new EliteDangerousCompanionAppAuthenticationException("You need to re-run the configuration application");
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // Error probably means that the service is down
                    throw new EliteDangerousCompanionAppErrorException("Elite: Dangerous service is down; please try later");
                }

                // Refresh the cookies from the raw information available to us
                string cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
                if (cookieHeader != null)
                {
                    Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                    if (companionAppMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.appId = companionAppMatch.Groups[1].Value;
                    }
                    Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                    if (machineIdMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.machineId = machineIdMatch.Groups[1].Value;
                    }
                    Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                    if (machineTokenMatch.Success)
                    {
                        if (credentials == null) { credentials = new CompanionAppCredentials(); }
                        credentials.machineToken = machineTokenMatch.Groups[1].Value;
                    }
                }

                // Update our credentials
                credentials.ToFile();

                // Obtain and parse our response
                var encoding = response.CharacterSet == ""
                            ? Encoding.UTF8
                            : Encoding.GetEncoding(response.CharacterSet);

                using (var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream, encoding);
                    string data = reader.ReadToEnd();
                    return CommanderFromProfile(data);
                }
            }
        }

        private static void AddCompanionAppCookie(CookieContainer cookies, CompanionAppCredentials credentials)
        {
            var appCookie = new Cookie();
            appCookie.Domain = "companion.orerve.net";
            appCookie.Path = "/";
            appCookie.Name = "CompanionApp";
            appCookie.Value = credentials.appId;
            appCookie.Secure = false;
            cookies.Add(appCookie);
        }

        private static void AddMachineIdCookie(CookieContainer cookies, CompanionAppCredentials credentials)
        {
            var machineIdCookie = new Cookie();
            machineIdCookie.Domain = "companion.orerve.net";
            machineIdCookie.Path = "/";
            machineIdCookie.Name = "mid";
            machineIdCookie.Value = credentials.machineId;
            machineIdCookie.Secure = true;
            // The expiry is embedded in the cookie value
            DateTime expiryDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            expiryDateTime = expiryDateTime.AddSeconds(Convert.ToInt64(credentials.machineId.Substring(0, credentials.machineId.IndexOf("%7C"))));
            machineIdCookie.Expires = expiryDateTime;
            cookies.Add(machineIdCookie);
        }

        private static void AddMachineTokenCookie(CookieContainer cookies, CompanionAppCredentials credentials)
        {
            var machineTokenCookie = new Cookie();
            machineTokenCookie.Domain = "companion.orerve.net";
            machineTokenCookie.Path = "/";
            machineTokenCookie.Name = "mtk";
            machineTokenCookie.Value = credentials.machineToken;
            machineTokenCookie.Secure = true;
            // The expiry is embedded in the cookie value
            DateTime expiryDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            expiryDateTime = expiryDateTime.AddSeconds(Convert.ToInt64(credentials.machineId.Substring(0, credentials.machineId.IndexOf("%7C"))));
            machineTokenCookie.Expires = expiryDateTime;
            cookies.Add(machineTokenCookie);
        }

        /// <summary>Create a commander profile given the results from a /profile call</summary>
        public static Commander CommanderFromProfile(string data)
        {
            return CommanderFromProfile(JObject.Parse(data));
        }


        /// <summary>Create a commander profile given the results from a /profile call</summary>
        public static Commander CommanderFromProfile(dynamic json)
        {
            Commander Commander = new Commander();

            if (json["commander"] != null)
            {
                Commander.Name = (string)json["commander"]["name"];

                Commander.CombatRating = (int)json["commander"]["rank"]["combat"];
                Commander.CombatRank = Commander.combatRanks[Commander.CombatRating];

                Commander.TradeRating = (int)json["commander"]["rank"]["trade"];
                Commander.TradeRank = Commander.tradeRanks[Commander.TradeRating];

                Commander.ExploreRating = (int)json["commander"]["rank"]["explore"];
                Commander.ExploreRank = Commander.exploreRanks[Commander.ExploreRating];

                Commander.EmpireRating = (int)json["commander"]["rank"]["empire"];
                Commander.EmpireRank = Commander.empireRanks[(int)Commander.EmpireRating];
                Commander.FederationRating = (int)json["commander"]["rank"]["federation"];
                Commander.FederationRank = Commander.federationRanks[(int)Commander.FederationRating];

                Commander.Credits = (long)json["commander"]["credits"];
                Commander.Debt = (long)json["commander"]["debt"];

                Commander.StarSystem = json["lastSystem"] == null ? null : (string)json["lastSystem"]["name"];

                Commander.Ship = ShipFromProfile(json);

                Commander.StoredShips = StoredShipsFromProfile(json, ref Commander);

                AugmentShipInfo(Commander.Ship, Commander.StoredShips);

                Commander.Outfitting = OutfittingFromProfile(json);

                Commander.LastStation = json["lastStarport"] == null ? null : (string)json["lastStarport"]["name"];
            }

            return Commander;
        }

        private static void AugmentShipInfo(Ship ship, List<Ship> storedShips)
        {
            ShipsConfiguration shipsConfiguration = ShipsConfiguration.FromFile();
            Dictionary<int, Ship> lookup = shipsConfiguration.Ships.ToDictionary(o => o.LocalId);

            Ship shipConfig;
            // Start with our current ship
            if (lookup.TryGetValue(ship.LocalId, out shipConfig))
            {
                // Already exists; grab the relevant information and supplement it
                // Ship config name might be just whitespace, in which case we unset it
                if (shipConfig.Name != null && shipConfig.Name.Trim().Length > 0)
                {
                    ship.Name = shipConfig.Name.Trim();
                }
                if (shipConfig.PhoneticName != null && shipConfig.PhoneticName.Trim().Length > 0)
                {
                    ship.PhoneticName = shipConfig.PhoneticName.Trim();
                }
                ship.CallSign = shipConfig.CallSign;
                ship.Role = shipConfig.Role;
            }
            else
            {
                // Doesn't already exist; add a callsign and default role
                ship.CallSign = Ship.generateCallsign();
                ship.Role = ShipRole.Multipurpose;
            }

            // Work through our shipyard
            foreach (Ship storedShip in storedShips)
            {

                if (lookup.TryGetValue(storedShip.LocalId, out shipConfig))
                {
                    // Already exists; grab the relevant information and supplement it
                    storedShip.Name = shipConfig.Name;
                    storedShip.CallSign = shipConfig.CallSign;
                    storedShip.Role = shipConfig.Role;
                }
                else
                {
                    // Doesn't already exist; add a callsign and default role
                    storedShip.CallSign = Ship.generateCallsign();
                    storedShip.Role = ShipRole.Multipurpose;
                }
            }

            // Update our configuration with the new data (this also removes any old redundant ships)
            shipsConfiguration.Ships = new List<Ship>();
            shipsConfiguration.Ships.Add(ship);
            shipsConfiguration.Ships.AddRange(storedShips);
            shipsConfiguration.ToFile();
        }

        public static Ship ShipFromProfile(dynamic json)
        {
            if (json["ship"] == null)
            {
                return null;
            }

            string Model = json["ship"]["name"];
            if (shipTranslations.ContainsKey(Model))
            {
                Model = shipTranslations[Model];
            }

            Ship Ship = ShipDefinitions.ShipFromModel(Model);

            Ship.LocalId = json["ship"]["id"];

            Ship.Value = (long)json["ship"]["value"]["hull"] + (long)json["ship"]["value"]["modules"];

            Ship.CargoCapacity = (int)json["ship"]["cargo"]["capacity"];
            Ship.CargoCarried = (int)json["ship"]["cargo"]["qty"];

            // Be sensible with health - round it unless it's very low
            decimal Health = (decimal)json["ship"]["health"]["hull"] / 10000;
            if (Health < 5)
            {
                Ship.Health = Math.Round(Health, 1);
            }
            else
            {
                Ship.Health = Math.Round(Health);
            }

            // Obtain the internals
            Ship.Bulkheads = ModuleFromProfile("Armour", json["ship"]["modules"]["Armour"]);
            Ship.PowerPlant = ModuleFromProfile("PowerPlant", json["ship"]["modules"]["PowerPlant"]);
            Ship.Thrusters = ModuleFromProfile("MainEngines", json["ship"]["modules"]["MainEngines"]);
            Ship.FrameShiftDrive = ModuleFromProfile("FrameShiftDrive", json["ship"]["modules"]["FrameShiftDrive"]);
            Ship.LifeSupport = ModuleFromProfile("LifeSupport", json["ship"]["modules"]["LifeSupport"]);
            Ship.PowerDistributor = ModuleFromProfile("PowerDistributor", json["ship"]["modules"]["PowerDistributor"]);
            Ship.Sensors = ModuleFromProfile("Radar", json["ship"]["modules"]["Radar"]);
            Ship.FuelTank = ModuleFromProfile("FuelTank", json["ship"]["modules"]["FuelTank"]);
            Ship.FuelTankCapacity = (decimal)json["ship"]["fuel"]["main"]["capacity"];

            // Obtain the hardpoints
            foreach (dynamic module in json["ship"]["modules"])
            {
                if (module.Name.Contains("Hardpoint"))
                {
                    Ship.Hardpoints.Add(HardpointFromProfile(module));
                }
            }

            // Obtain the compartments
            foreach (dynamic module in json["ship"]["modules"])
            {
                if (module.Name.Contains("Slot"))
                {
                    Ship.Compartments.Add(CompartmentFromProfile(module));
                }
            }

            // Obtain the cargo
            Ship.Cargo = new List<Cargo>();
            if (json["ship"]["cargo"] != null && json["ship"]["cargo"]["items"] != null)
            {
                foreach (dynamic cargoJson in json["ship"]["cargo"]["items"])
                {
                    string name = (string)cargoJson["commodity"];
                    Cargo cargo = new Cargo();
                    cargo.Commodity = CommodityDefinitions.CommodityFromCargoName(name);
                    if (cargo.Commodity.Name == null)
                    {
                        // Unknown commodity; log an error so that we can update the definitions
                        DataProviderService.LogError("No commodity definition for cargo " + cargoJson.ToString());
                        cargo.Commodity.Name = name;
                    }
                    cargo.Quantity = (int)cargoJson["qty"];
                    cargo.Cost = (long)cargoJson["value"];
                    Ship.Cargo.Add(cargo);
                }
            }

            return Ship;
        }

        public static Hardpoint HardpointFromProfile(dynamic json)
        {
            Hardpoint Hardpoint = new Hardpoint();

            string name = json.Name;
            if (name.StartsWith("Huge"))
            {
                Hardpoint.Size = 4;
            }
            else if (name.StartsWith("Large"))
            {
                Hardpoint.Size = 3;
            }
            else if (name.StartsWith("Medium"))
            {
                Hardpoint.Size = 2;
            }
            else if (name.StartsWith("Small"))
            {
                Hardpoint.Size = 1;
            }
            else if (name.StartsWith("Tiny"))
            {
                Hardpoint.Size = 0;
            }

            if (json.Value is JObject)
            {
                JToken value;
                if (json.Value.TryGetValue("module", out value))
                {
                    Hardpoint.Module = ModuleFromProfile(name, json.Value);
                }
            }

            return Hardpoint;
        }

        public static List<Ship> StoredShipsFromProfile(dynamic json, ref Commander commander)
        {
            Ship currentShip = commander.Ship;

            List<Ship> StoredShips = new List<Ship>();

            foreach (dynamic shipJson in json["ships"])
            {
                if (shipJson != null)
                {
                    dynamic ship = shipJson.Value;
                    if (ship != null)
                    {
                        if ((int)ship["id"] != currentShip.LocalId)
                        {
                            Ship Ship = new Ship();

                            if (ship["starsystem"] != null)
                            {
                                // If we have a starsystem it means that the ship is stored
                                Ship.LocalId = ship["id"];
                                Ship.Model = ship["name"];
                                if (shipTranslations.ContainsKey(Ship.Model))
                                {
                                    Ship.Model = shipTranslations[Ship.Model];
                                }

                                Ship.StarSystem = ship["starsystem"]["name"];
                                Ship.Station = ship["station"]["name"];

                                StoredShips.Add(Ship);
                            }
                        }
                    }
                }
            }

            return StoredShips;
        }

        public static Compartment CompartmentFromProfile(dynamic json)
        {
            Compartment Compartment = new Compartment();

            // Compartments have name of form "Slotnn_Sizenn"
            Match matches = Regex.Match((string)json.Name, @"Size([0-9]+)");
            if (matches.Success)
            {
                Compartment.Size = Int32.Parse(matches.Groups[1].Value);

                if (json.Value is JObject)
                {
                    JToken value;
                    if (json.Value.TryGetValue("module", out value))
                    {
                        Compartment.Module = ModuleFromProfile((string)json.Name, json.Value);
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
                        if (Module.Name == null)
                        {
                            // Unknown module; log an error so that we can update the definitions
                            DataProviderService.LogError("No definition for outfitting module " + module.ToString());
                        }
                        Module.Cost = module["cost"];
                        Modules.Add(Module);
                    }
                }
            }

            return Modules;
        }

        public static Module ModuleFromProfile(string name, dynamic json)
        {
            long id = (long)json["module"]["id"];
            Module module = ModuleDefinitions.ModuleFromEliteID(id);
            if (module.Name == null)
            {
                // Unknown module; log an error so that we can update the definitions
                DataProviderService.LogError("No definition for ship module " + json["module"].ToString());
            }

            module.Cost = (long)json["module"]["value"];
            module.Enabled = (bool)json["module"]["on"];
            module.Priority = (int)json["module"]["priority"];
            // Be sensible with health - round it unless it's very low
            decimal Health = (decimal)json["module"]["health"] / 10000;
            if (Health < 5)
            {
                module.Health = Math.Round(Health, 1);
            }
            else
            {
                module.Health = Math.Round(Health);
            }
            return module;
        }

    }
}
