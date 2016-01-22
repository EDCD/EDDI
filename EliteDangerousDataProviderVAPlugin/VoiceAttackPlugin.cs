using EliteDangerousCompanionAppService;
using EliteDangerousDataProvider;
using EliteDangerousDataProviderAppService;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace EliteDangerousDataProviderVAPlugin
{
    public class VoiceAttackPlugin
    {
        private static int minEmpireRatingForTitle = 3;
        private static int minFederationRatingForTitle = 1;

        private static CompanionApp app;
        private static string dbPath;

        private static Commander Cmdr;

        public static string VA_DisplayName()
        {
            return "Elite: Dangerous Data Provider 1.0.0";
        }

        public static string VA_DisplayInfo()
        {
            return "Elite Dangerous Data Provider\r\nVersion 1.0.0";
        }

        public static Guid VA_Id()
        {
            return new Guid("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");
        }

        public static void VA_Init1(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            setPluginMessage(ref textValues, "Status", "Initialising");

            // Set up and/or open our database
            String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EliteDataProvider";
            System.IO.Directory.CreateDirectory(dataDir);


            // Obtain our credentials
            Credentials credentials = Credentials.FromFile();
            if (credentials == null)
            {
                setPluginMessage(ref textValues, "Error", "Failed to access credentials file");
                return;
            }
            if (String.IsNullOrEmpty(credentials.appId) || String.IsNullOrEmpty(credentials.machineId) || String.IsNullOrEmpty(credentials.machineToken))
            {
                setPluginMessage(ref textValues, "Error", "Credentials file does not contain required information");
                return;
            }

            // Set up our database if it isn't already
            dbPath = dataDir + "\\data.sqlite";
            using (var connection = new SQLiteConnection("Data Source=" + dbPath))
            {
                connection.Open();

                string createSystemsTableSql = "CREATE TABLE IF NOT EXISTS systems(name TEXT NOT NULL, lastdata INT NOT NULL DEFAULT (CAST(strftime('%s','now') AS INT)), visits INT NOT NULL, thisvisit INT NOT NULL DEFAULT (CAST(strftime('%s', 'now') AS INT)), lastvisit INT NOT NULL DEFAULT (CAST(strftime('%s','now') AS INT)), data TEXT NOT NULL)";
                SQLiteCommand createSystemsTableCmd = new SQLiteCommand(createSystemsTableSql, connection);
                createSystemsTableCmd.ExecuteNonQuery();

                string createProfileTableSql = "CREATE TABLE IF NOT EXISTS systems(name TEXT NOT NULL, lastdata INT NOT NULL DEFAULT (CAST(strftime('%s','now') AS INT)), visits INT NOT NULL, thisvisit INT NOT NULL DEFAULT (CAST(strftime('%s', 'now') AS INT)), lastvisit INT NOT NULL DEFAULT (CAST(strftime('%s','now') AS INT)), data TEXT NOT NULL)";
                SQLiteCommand createProfileTableCmd = new SQLiteCommand(createProfileTableSql, connection);
                createProfileTableCmd.ExecuteNonQuery();

                connection.Close();
            }

            app = new CompanionApp(credentials);

            setPluginMessage(ref textValues, "Status", "Operational");

            InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
        }

        public static void VA_Exit1(ref Dictionary<string, object> state)
        {
        }

        public static void VA_Invoke1(String context, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            switch (context)
            {
                case "coriolis":
                    InvokeCoriolis(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                default:
                    InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    InvokeNewSystem(ref Cmdr, ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
            }
        }

        public static void InvokeCoriolis(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                if (Cmdr == null || Cmdr.Ship == null || Cmdr.Ship.Model == null)
                {
                    // Refetch the profile to set our ship
                    InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    if (Cmdr == null || Cmdr.Ship == null || Cmdr.Ship.Model == null)
                    {
                        // Still no luck; assume an error of some sort has been logged by InvokeUpdateProfile()
                        return;
                    }
                }

                string shipUri = Coriolis.ShipUri(Cmdr.Ship);

                System.Diagnostics.Process.Start(shipUri);
            }
            catch (Exception e)
            {
                setPluginMessage(ref textValues, "Status", "Failed to send ship data to coriolis");
                setPluginMessage(ref textValues, "Debug", e);
            }
        }

        public static void InvokeUpdateProfile(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                // Obtain the command profile
                dynamic json = app.Profile();

                Cmdr = app.Profile();

                setPluginMessage(ref textValues, "ProfileJson", json);

                //
                // Commander data
                //
                setString(ref textValues, "Name", Cmdr.Name);
                setInt(ref intValues, "Combat rating", Cmdr.CombatRating);
                setString(ref textValues, "Combat rank", Cmdr.CombatRank);
                setInt(ref intValues, "Trade rating", Cmdr.TradeRating);
                setString(ref textValues, "Trade rank", Cmdr.TradeRank);
                setInt(ref intValues, "Explore rating", Cmdr.ExploreRating);
                setString(ref textValues, "Explore rank", Cmdr.ExploreRank);
                setInt(ref intValues, "Empire rating", Cmdr.EmpireRating);
                setString(ref textValues, "Empire rank", Cmdr.EmpireRank);
                setInt(ref intValues, "Federation rating", Cmdr.FederationRating);
                setString(ref textValues, "Federation rank", Cmdr.FederationRank);
                setInt(ref intValues, "Credits", (int)(Cmdr.Credits / 1000));
                setString(ref textValues, "Credits", humanize(Cmdr.Credits));
                setInt(ref intValues, "Debt", (int)(Cmdr.Debt / 1000));
                setString(ref textValues, "Debt", humanize(Cmdr.Debt));


                //
                // Ship data
                //
                setString(ref textValues, "Ship model", Cmdr.Ship.Model);
                setInt(ref intValues, "Ship value", (int)(Cmdr.Ship.Value / 1000));
                setString(ref textValues, "Ship value", humanize(Cmdr.Ship.Value));
                setInt(ref intValues, "Ship cargo capacity", Cmdr.Ship.CargoCapacity);


                //dynamic shipBulkheadsJson = json["ship"]["modules"]["Armour"]["module"];
                //ship.bulkheads = shipBulkheadsNames[Int32.Parse(((string)shipBulkheadsJson["name"]).Substring(((string)shipBulkheadsJson["name"]).Length - 1))];
                //ship.bulkheadsIntegrity = ((decimal)shipBulkheadsJson["health"]) / 10000;
                //setString(ref textValues, "Ship bulkheads", ship.bulkheads);
                //setDecimal(ref decimalValues, "Ship bulkheads integrity", ship.bulkheadsIntegrity);

                //dynamic shipPowerPlantJson = json["ship"]["modules"]["PowerPlant"]["module"];
                //ship.powerPlant = ModuleTypeFromName((string)shipPowerPlantJson["name"]);
                //ship.powerPlantIntegrity = ((decimal)shipPowerPlantJson["health"]) / 10000;
                //setString(ref textValues, "Ship power plant", ship.powerPlant);
                //setDecimal(ref decimalValues, "Ship power plant integrity", ship.powerPlantIntegrity);

                //dynamic shipThrustersJson = json["ship"]["modules"]["MainEngines"]["module"];
                //ship.thrusters = ModuleTypeFromName((string)shipThrustersJson["name"]);
                //ship.thrustersIntegrity = ((decimal)shipThrustersJson["health"]) / 10000;
                //ship.thrustersEnabled = (bool)shipThrustersJson["on"];
                //ship.thrustersPriority = (int)shipThrustersJson["priority"];
                //setString(ref textValues, "Ship thrusters", ship.thrusters);
                //setDecimal(ref decimalValues, "Ship thrusters integrity", ship.thrustersIntegrity);
                //setBoolean(ref booleanValues, "Ship thrusters enabled", ship.thrustersEnabled);
                //setInt(ref intValues, "Ship thrusters priority", ship.thrustersPriority);

                //dynamic shipFrameShiftDriveJson = json["ship"]["modules"]["FrameShiftDrive"]["module"];
                //ship.frameShiftDrive = ModuleTypeFromName((string)shipFrameShiftDriveJson["name"]);
                //ship.frameShiftDriveIntegrity = ((decimal)shipFrameShiftDriveJson["health"]) / 10000;
                //ship.frameShiftDriveEnabled = (bool)shipFrameShiftDriveJson["on"];
                //ship.frameShiftDrivePriority = (int)shipFrameShiftDriveJson["priority"];
                //setString(ref textValues, "Ship frame shift drive", ship.frameShiftDrive);
                //setDecimal(ref decimalValues, "Ship frame shift drive integrity", ship.frameShiftDriveIntegrity);
                //setBoolean(ref booleanValues, "Ship frame shift drive enabled", ship.frameShiftDriveEnabled);
                //setInt(ref intValues, "Ship frame shift drive priority", ship.frameShiftDrivePriority);

                //dynamic shipLifeSupportJson = json["ship"]["modules"]["LifeSupport"]["module"];
                //ship.lifeSupport = ModuleTypeFromName((string)shipLifeSupportJson["name"]);
                //ship.lifeSupportIntegrity = ((decimal)shipLifeSupportJson["health"]) / 10000;
                //ship.lifeSupportEnabled = (bool)shipLifeSupportJson["on"];
                //ship.lifeSupportPriority = (int)shipLifeSupportJson["priority"];
                //setString(ref textValues, "Ship life support", ship.lifeSupport);
                //setDecimal(ref decimalValues, "Ship life support integrity", ship.lifeSupportIntegrity);
                //setBoolean(ref booleanValues, "Ship life support enabled", ship.lifeSupportEnabled);
                //setInt(ref intValues, "Ship life support priority", ship.lifeSupportPriority);

                //dynamic shipPowerDistributorJson = json["ship"]["modules"]["PowerDistributor"]["module"];
                //ship.powerDistributor = ModuleTypeFromName((string)shipPowerDistributorJson["name"]);
                //ship.powerDistributorIntegrity = ((decimal)shipPowerDistributorJson["health"]) / 10000;
                //ship.powerDistributorEnabled = (bool)shipPowerDistributorJson["on"];
                //ship.powerDistributorPriority = (int)shipPowerDistributorJson["priority"];
                //setString(ref textValues, "Ship power distributor", ship.powerDistributor);
                //setDecimal(ref decimalValues, "Ship power distributor integrity", ship.powerDistributorIntegrity);
                //setBoolean(ref booleanValues, "Ship power distributor enabled", ship.powerDistributorEnabled);
                //setInt(ref intValues, "Ship power distributor priority", ship.powerDistributorPriority);

                //dynamic shipSensorsJson = json["ship"]["modules"]["Radar"]["module"];
                //ship.sensors = ModuleTypeFromName((string)shipSensorsJson["name"]);
                //ship.sensorsIntegrity = ((decimal)shipSensorsJson["health"]) / 10000;
                //ship.sensorsEnabled = (bool)shipSensorsJson["on"];
                //ship.sensorsPriority = (int)shipSensorsJson["priority"];
                //setString(ref textValues, "Ship sensors", ship.sensors);
                //setDecimal(ref decimalValues, "Ship sensors integrity", ship.sensorsIntegrity);
                //setBoolean(ref booleanValues, "Ship sensors enabled", ship.sensorsEnabled);
                //setInt(ref intValues, "Ship sensors priority", ship.sensorsPriority);

                //dynamic shipFuelTankJson = json["ship"]["modules"]["FuelTank"]["module"];
                //ship.fuelTank = ModuleTypeFromName((string)shipFuelTankJson["name"]);
                //setString(ref textValues, "Ship fuel tank", ship.fuelTank);

                //// Work through hardpoints
                //List<Module> Modules = new List<Module>();
                //foreach (dynamic module in json["ship"]["modules"])
                //{
                //    Modules.Add(ParseModule(module));
                //}
                //setPluginMessage(ref textValues, "Modules", Modules);
                ////foreach (string hardpointSize in hardpointSizes)
                ////{
                ////    for (int i = 1; i < 10; i++)
                ////    {
                ////        string hardpointId = hardpointSize + "Hardpoint" + i;
                ////        if (json["ship"]["modules"].ContainsKey(hardpointId))
                ////        {
                ////            dynamic hardpointJson = json["ship"]["modules"][hardpointId];
                ////            Hardpoint hardpoint = new Hardpoint();
                ////            hardpoint.size = hardpointSize;

                ////            if (hardpointJson.ContainsKey("module"))
                ////            {
                ////                hardpointJson = hardpointJson["module"];
                ////            }
                ////            ship.hardpoints.Add(hardpoint);
                ////        }
                ////    }
                ////}

                //// Work through modules


                ////
                //// System data
                ////
                //string systemName = json["lastSystem"]["name"];
                //setString(ref textValues, "System name", systemName);
                //InvokeNewSystem(systemName, ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            }
            catch (Exception e)
            {
                setPluginMessage(ref textValues, "Error", "Failed to access profile data");
                setPluginMessage(ref textValues, "Debug", e);
            }
        }

        public static void InvokeNewSystem(ref Commander Cmdr, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                if (!textValues.ContainsKey("System name"))
                {
                    // Refetch the profile to set our system
                    InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    if (!textValues.ContainsKey("System name"))
                    {
                        // Still no luck; assume an error of some sort has been logged by InvokeUpdateProfile()
                        return;
                    }
                }

                string systemName = textValues["System name"];

                // Fetch data on the current system given its name
                SystemEntry systemEntry = fetchSystemData(systemName);
                dynamic systemJson = systemEntry.data;

                string systemPrimaryEconomy = systemJson["primary_economy"];
                setString(ref textValues, "System primary economy", systemPrimaryEconomy);
                string systemGovernment = systemJson["government"];
                setString(ref textValues, "System government", systemGovernment);

                setInt(ref intValues, "System population", (int?)(systemJson["population"] / 1000));
                setString(ref textValues, "System population", humanize((long?)systemJson["population"]));

                string systemAllegiance = systemJson["allegiance"];
                setString(ref textValues, "System allegiance", systemAllegiance);
                // Allegiance-specific rank
                string systemRank = "Commander";
                if (systemAllegiance == "Federation" && Cmdr.FederationRating  >= minFederationRatingForTitle)
                {
                    systemRank = Cmdr.FederationRank;
                }
                else if (systemAllegiance == "Empire" && Cmdr.EmpireRating >= minEmpireRatingForTitle)
                {
                    systemRank = Cmdr.EmpireRank;
                }
                setString(ref textValues, "System rank", systemRank);

                // Stations

                // Outposts
            }
            catch (Exception e)
            {
                setPluginMessage(ref textValues, "Status", "Failed to obtain system data");
                setPluginMessage(ref textValues, "Debug", e);
            }
        }

        private static void setInt(ref Dictionary<string, int?> values, string key, int? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setDecimal(ref Dictionary<string, decimal?> values, string key, decimal? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setBoolean(ref Dictionary<string, bool?> values, string key, bool? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setString(ref Dictionary<string, string> values, string key, string value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setPluginMessage(ref Dictionary<string, string> values, string key, dynamic value)
        {
            setString(ref values, "EliteDangerousDataProvider_" + key, value.ToString());
        }

        public static string humanize(long? value)
        {
            if (value == null)
            {
                return null;
            }

            int number;
            int nextDigit;
            string order;
            int digits = (int)Math.Log10((double)value);
            if (digits < 3)
            {
                // Units
                number = (int)value;
                order = "";
                nextDigit = 0;
            }
            else if (digits < 6)
            {
                // Thousands
                number = (int)(value / 1000);
                order = "thousand";
                nextDigit = (((int)value) - (number * 1000)) / 100;
            }
            else if (digits < 9)
            {
                // Millions
                number = (int)(value / 1000000);
                order = "million";
                nextDigit = (((int)value) - (number * 1000000)) / 100000;
            }
            else
            {
                // Billions
                number = (int)(value / 1000000000);
                order = "billion";
                nextDigit = (int)(((long)value) - ((long)number * 1000000000)) / 100000000;
            }

            if (order == "")
            {
                return "" + number;
            }
            else
            {
                if (number > 60)
                {
                    return number + " " + order;
                }
                switch (nextDigit)
                {
                    case 0:
                        return "just over " + number + " " + order;
                    case 1:
                        return "over " + number + " " + order;
                    case 2:
                        return "well over " + number + " " + order;
                    case 3:
                        return "on the way to " + number + " and a half " + order;
                    case 4:
                        return "nearly " + number + " and a half " + order;
                    case 5:
                        return "around " + number + " and a half " + order;
                    case 6:
                        return "just over " + number + " and a half " + order;
                    case 7:
                        return "well over " + number + " and a half " + order;
                    case 8:
                        return "on the way to " + (number + 1) + " " + order;
                    case 9:
                        return "nearly " + (number + 1) + " " + order;
                    default:
                        return "around " + number + " " + order;
                }
            }

        }

        private static String ModuleTypeFromName(string name)
        {
            // Name should contain SizeX and ClassY information
            Match matches = Regex.Match(name, @"Size([0-9]+).*Class([0-9]+)");
            if (matches.Success)
            {
                return matches.Groups[1].Value + ((char)(70 - Int32.Parse(matches.Groups[2].Value))).ToString();
            }
            return null;
        }

        public class SystemEntry
        {
            public string name { get; set; }
            public int visits { get; set; }
            public long timestamp { get; set; }
            public long thisVisit { get; set; }
            public long lastVisit { get; set; }
            public dynamic data { get; set; }
        }

        private static SystemEntry fetchSystemData(string systemName)
        {
            SystemEntry entry = new SystemEntry();
            entry.name = systemName;
            entry.visits = 1;
            entry.timestamp = (long)(TimeZoneInfo.ConvertTimeToUtc(DateTime.Now) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            entry.thisVisit = (long)(TimeZoneInfo.ConvertTimeToUtc(DateTime.Now) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            entry.lastVisit = entry.thisVisit - 36000;
            entry.data = DataProviderApp.GetSystemData(systemName);

            return entry;
        }
    }
}
