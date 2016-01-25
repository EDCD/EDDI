using EliteDangerousDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
            { "CobraMkIII", "Cobra MkIII" },
            { "CobraMkIV", "Cobra MkIV" },
            { "Cutter", "Imperial Cutter" },
            { "Diamondback", "Diamondback Scout" },
            { "DiamondbackXL", "Diamondback Explorer" },
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
            { "Viper", "Viper MkIII" },
            { "Viper_MkIV", "Viper MkIV" },
            { "Vulture", "Vulture" }
        };

        private static string serverRoot = "https://companion.orerve.net";

        private Credentials credentials;

        public CompanionAppService(Credentials credentials)
        {
            this.credentials = credentials;
        }

        ///<summary>Log in.  Returns credentials, or throws an exception if it fails</summary>
        public static Credentials Login(string username, string password)
        {
            Credentials credentials = null;
            string location = serverRoot + "/user/login";
            // Send the request.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(location);
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
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // A good response is a redirect status code
            if ((int)response.StatusCode < 300 || (int)response.StatusCode > 399)
            {
                throw new Exception();
            }

            // Obtain the cookies from the raw information available to us
            String cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
            if (cookieHeader != null)
            {
                Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                if (companionAppMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.appId = companionAppMatch.Groups[1].Value;
                }
                Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                if (machineIdMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.machineId = machineIdMatch.Groups[1].Value;
                }
                Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                if (machineTokenMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.machineToken = machineTokenMatch.Groups[1].Value;
                }
            }
            return credentials;
        }

        ///<summary>Confirm a login.  Returns credentials, or throws an exception if it fails</summary>
        public static Credentials Confirm(Credentials credentials, string code)
        {
            var cookieContainer = new CookieContainer();
            AddCompanionAppCookie(cookieContainer, credentials);
            AddMachineIdCookie(cookieContainer, credentials);
            AddMachineTokenCookie(cookieContainer, credentials);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverRoot + "/user/confirm");
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
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // A good response is a found or a redirect
            if ((int)response.StatusCode < 200 || (int)response.StatusCode > 399)
            {
                throw new Exception();
            }

            // Refresh the cookies from the raw information available to us
            String cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
            if (cookieHeader != null)
            {
                Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                if (companionAppMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.appId = companionAppMatch.Groups[1].Value;
                }
                Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                if (machineIdMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.machineId = machineIdMatch.Groups[1].Value;
                }
                Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                if (machineTokenMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.machineToken = machineTokenMatch.Groups[1].Value;
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

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverRoot + "/profile");
            request.AllowAutoRedirect = false;
            request.CookieContainer = cookieContainer;
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Mobile/11D257";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // A good response is a found
            if ((int)response.StatusCode < 200 || (int)response.StatusCode > 299)
            {
                throw new Exception();
            }

            // Refresh the cookies from the raw information available to us
            String cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
            if (cookieHeader != null)
            {
                Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                if (companionAppMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.appId = companionAppMatch.Groups[1].Value;
                }
                Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                if (machineIdMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.machineId = machineIdMatch.Groups[1].Value;
                }
                Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                if (machineTokenMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
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
                return CommanderFromProfile(reader.ReadToEnd());
            }
        }

        private static void AddCompanionAppCookie(CookieContainer cookies, Credentials credentials)
        {
            var appCookie = new Cookie();
            appCookie.Domain = "companion.orerve.net";
            appCookie.Path = "/";
            appCookie.Name = "CompanionApp";
            appCookie.Value = credentials.appId;
            cookies.Add(appCookie);
        }

        private static void AddMachineIdCookie(CookieContainer cookies, Credentials credentials)
        {
            var machineIdCookie = new Cookie();
            machineIdCookie.Domain = ".companion.orerve.net";
            machineIdCookie.Path = "/";
            machineIdCookie.Name = "mid";
            machineIdCookie.Value = credentials.machineId;
            cookies.Add(machineIdCookie);
        }

        private static void AddMachineTokenCookie(CookieContainer cookies, Credentials credentials)
        {
            var machineTokenCookie = new Cookie();
            machineTokenCookie.Domain = ".companion.orerve.net";
            machineTokenCookie.Path = "/";
            machineTokenCookie.Name = "mtk";
            machineTokenCookie.Value = credentials.machineToken;
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

            Commander.Name = (string)json["commander"]["name"];

            Commander.CombatRating = (int)json["commander"]["rank"]["combat"];
            Commander.CombatRank = Commander.combatRanks[Commander.CombatRating];

            Commander.TradeRating = (int)json["commander"]["rank"]["trade"];
            Commander.TradeRank = Commander.tradeRanks[Commander.TradeRating];

            Commander.ExploreRating = (int)json["commander"]["rank"]["explore"];
            Commander.ExploreRank = Commander.exploreRanks[Commander.ExploreRating];

            Commander.EmpireRating = (int)json["commander"]["rank"]["empire"];
            Commander.EmpireRank = Commander.federationRanks[(int)Commander.EmpireRating];
            Commander.FederationRating = (int)json["commander"]["rank"]["federation"];
            Commander.FederationRank = Commander.federationRanks[(int)Commander.FederationRating];

            Commander.Credits = (long)json["commander"]["credits"];
            Commander.Debt = (long)json["commander"]["debt"];

            Commander.StarSystem = (string)json["lastSystem"]["name"];

            Commander.Ship = ShipFromProfile(json);

            Commander.StoredShips = StoredShipsFromProfile(json);

            return Commander;
        }

        public static Ship ShipFromProfile(dynamic json)
        {
            Ship Ship = new Ship();

            Ship.LocalId = json["ship"]["id"];
            Ship.Model = json["ship"]["name"];
            if (shipTranslations.ContainsKey(Ship.Model))
            {
                Ship.Model = shipTranslations[Ship.Model];
            }

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

        public static List<Ship> StoredShipsFromProfile(dynamic json)
        {
            List<Ship> StoredShips = new List<Ship>();

            foreach (dynamic shipJson in json["ships"])
            {
                dynamic ship = shipJson.Value;

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
                    Ship.Location = ship["station"]["name"];

                    StoredShips.Add(Ship);
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

        public static Module ModuleFromProfile(string name, dynamic json)
        {
            long id = (long)json["module"]["id"];
            Module ModuleTemplate = ModuleDefinitions.FromID(id);
            Module Module = new Module(ModuleTemplate);

            Module.Value = (long)json["module"]["value"];
            Module.Enabled = (bool)json["module"]["on"];
            Module.Priority = (int)json["module"]["priority"];
            // Be sensible with health - round it unless it's very low
            decimal Health = (decimal)json["module"]["health"] / 10000;
            if (Health < 5)
            {
                Module.Health = Math.Round(Health, 1);
            }
            else
            {
                Module.Health = Math.Round(Health);
            }
            return Module;
        }

    }
}
