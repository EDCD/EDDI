using EliteDangerousCompanionAppService;
using EliteDangerousDataProviderService;
using EliteDangerousDataDefinitions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace EDDIVAPlugin
{
    public class VoiceAttackPlugin
    {
        private static int minEmpireRatingForTitle = 3;
        private static int minFederationRatingForTitle = 1;

        private static CompanionAppService app;
        private static string dbPath;

        private static Commander Cmdr;
        private static StarSystem CurrentStarSystem;
        private static StarSystem LastStarSystem;

        public static string VA_DisplayName()
        {
            return "EDDI 0.6.0";
        }

        public static string VA_DisplayInfo()
        {
            return "Elite Dangerous Data Interface\r\nVersion 0.6.0";
        }

        public static Guid VA_Id()
        {
            return new Guid("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");
        }

        public static void VA_Init1(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            // Set up and/or open our database
            String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
            System.IO.Directory.CreateDirectory(dataDir);

            // Obtain our credentials
            Credentials credentials = Credentials.FromFile();
            if (credentials == null)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to access credentials file; please log in", null);
                return;
            }
            if (String.IsNullOrEmpty(credentials.appId) || String.IsNullOrEmpty(credentials.machineId) || String.IsNullOrEmpty(credentials.machineToken))
            {
                setPluginStatus(ref textValues, "Failed", "Credentials file does not contain required information; please log in", null);
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

            app = new CompanionAppService(credentials);

            setPluginStatus(ref textValues, "Operational", null, null);

            InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            InvokeNewSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
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
                case "profile":
                    InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                case "system":
                    InvokeNewSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    return;
                default:
                    InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    InvokeNewSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
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

                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to send ship data to coriolis", e);
            }
        }

        public static void InvokeUpdateProfile(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                // Obtain the command profile
                Cmdr = app.Profile();

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
                setDecimal(ref decimalValues, "Ship health", Cmdr.Ship.Health);
                setInt(ref intValues, "Ship cargo capacity", Cmdr.Ship.CargoCapacity);
                setInt(ref intValues, "Ship cargo carried", Cmdr.Ship.CargoCarried);

                setString(ref textValues, "Ship bulkheads", Cmdr.Ship.Bulkheads.Name);
                setDecimal(ref decimalValues, "Ship bulkheads health", Cmdr.Ship.Bulkheads.Health);

                setString(ref textValues, "Ship power plant", Cmdr.Ship.PowerPlant.Class + Cmdr.Ship.PowerPlant.Grade);
                setDecimal(ref decimalValues, "Ship power plant health", Cmdr.Ship.PowerPlant.Health);

                setString(ref textValues, "Ship thrusters", Cmdr.Ship.Thrusters.Class + Cmdr.Ship.Thrusters.Grade);
                setDecimal(ref decimalValues, "Ship thrusters health", Cmdr.Ship.Thrusters.Health);

                setString(ref textValues, "Ship frame shift drive", Cmdr.Ship.FrameShiftDrive.Class + Cmdr.Ship.FrameShiftDrive.Grade);
                setDecimal(ref decimalValues, "Ship frame shift drive health", Cmdr.Ship.FrameShiftDrive.Health);

                setString(ref textValues, "Ship life support", Cmdr.Ship.LifeSupport.Class + Cmdr.Ship.LifeSupport.Grade);
                setDecimal(ref decimalValues, "Ship life support health", Cmdr.Ship.LifeSupport.Health);

                setString(ref textValues, "Ship power distributor", Cmdr.Ship.PowerDistributor.Class + Cmdr.Ship.PowerDistributor.Grade);
                setDecimal(ref decimalValues, "Ship power distributor health", Cmdr.Ship.PowerDistributor.Health);

                setString(ref textValues, "Ship sensors", Cmdr.Ship.Sensors.Class + Cmdr.Ship.Sensors.Grade);
                setDecimal(ref decimalValues, "Ship sensors health", Cmdr.Ship.Sensors.Health);

                setString(ref textValues, "Ship fuel tank", Cmdr.Ship.FuelTank.Class + Cmdr.Ship.FuelTank.Grade);
                //                setInt(ref intValues, "Ship fuel tank capacity", 0); // TODO

                // Hardpoints
                int weaponHardpoints = 0;
                foreach (Hardpoint Hardpoint in Cmdr.Ship.Hardpoints)
                {
                    if (Hardpoint.Size > 0)
                    {
                        weaponHardpoints++;
                    }
                }
                setInt(ref intValues, "Ship hardpoints", weaponHardpoints);
                setInt(ref intValues, "Ship utility slots", Cmdr.Ship.Hardpoints.Count - weaponHardpoints);

                // Compartments
                //foreach (Compartment Compartment in Cmdr.Ship.Hardpoints)
                //{
                //    if (Compartment.Size > 0)
                //    {
                //        weaponHardpoints++;
                //    }
                //}
                setInt(ref intValues, "Ship compartments", Cmdr.Ship.Compartments.Count);


                //
                // Stored ships data
                //
                int currentStoredShip = 1;
                foreach (Ship StoredShip in Cmdr.StoredShips)
                {
                    string varBase = "Stored ship " + currentStoredShip;
                    setString(ref textValues, varBase + " model", StoredShip.Model);
                    setString(ref textValues, varBase + " system", StoredShip.StarSystem);
                    setString(ref textValues, varBase + " location", StoredShip.Location);
                    currentStoredShip++;
                }
                setInt(ref intValues, "Stored ships", Cmdr.StoredShips.Count);

                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to access system data", e);
            }
        }

        public static void InvokeNewSystem(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                if (Cmdr == null)
                {
                    // Refetch the profile to set our system
                    InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    if (Cmdr == null)
                    {
                        // Still no luck; assume an error of some sort has been logged by InvokeUpdateProfile()
                        return;
                    }
                }

                StarSystem ThisStarSystem = DataProviderService.GetSystemData(Cmdr.StarSystem);
                if (CurrentStarSystem == null || ThisStarSystem.Name != CurrentStarSystem.Name)
                {
                    // The star system has changed; update the data
                    LastStarSystem = CurrentStarSystem;
                    CurrentStarSystem = ThisStarSystem;

                    setString(ref textValues, "System name", CurrentStarSystem.Name);
                    setInt(ref intValues, "System population", (int)(CurrentStarSystem.Population / 1000));
                    setString(ref textValues, "System population", humanize(CurrentStarSystem.Population));
                    setString(ref textValues, "System allegiance", CurrentStarSystem.Allegiance);
                    setString(ref textValues, "System government", CurrentStarSystem.Government);
                    setString(ref textValues, "System faction", CurrentStarSystem.Faction);
                    setString(ref textValues, "System primary economy", CurrentStarSystem.PrimaryEconomy);
                    setString(ref textValues, "System state", CurrentStarSystem.State);
                    setString(ref textValues, "System security", CurrentStarSystem.Security);
                    setString(ref textValues, "System power", CurrentStarSystem.Power);
                    setString(ref textValues, "System power state", CurrentStarSystem.PowerState);

                    setDecimal(ref decimalValues, "System X", CurrentStarSystem.X);
                    setDecimal(ref decimalValues, "System Y", CurrentStarSystem.Y);
                    setDecimal(ref decimalValues, "System Z", CurrentStarSystem.Z);

                    // Allegiance-specific rank
                    string systemRank = "Commander";
                    if (CurrentStarSystem.Allegiance == "Federation" && Cmdr.FederationRating >= minFederationRatingForTitle)
                    {
                        systemRank = Cmdr.FederationRank;
                    }
                    else if (CurrentStarSystem.Allegiance == "Empire" && Cmdr.EmpireRating >= minEmpireRatingForTitle)
                    {
                        systemRank = Cmdr.EmpireRank;
                    }
                    setString(ref textValues, "System rank", systemRank);

                    // Stations

                    // Outposts

                    if (LastStarSystem != null)
                    {
                        setString(ref textValues, "Last system name", LastStarSystem.Name);
                        setInt(ref intValues, "Last system population", (int)(LastStarSystem.Population / 1000));
                        setString(ref textValues, "Last system population", humanize(LastStarSystem.Population));
                        setString(ref textValues, "Last system allegiance", LastStarSystem.Allegiance);
                        setString(ref textValues, "Last system government", LastStarSystem.Government);
                        setString(ref textValues, "Last system faction", LastStarSystem.Faction);
                        setString(ref textValues, "Last system primary economy", LastStarSystem.PrimaryEconomy);
                        setString(ref textValues, "Last system state", LastStarSystem.State);
                        setString(ref textValues, "Last system security", LastStarSystem.Security);
                        setString(ref textValues, "Last system power", LastStarSystem.Power);
                        setString(ref textValues, "Last system power state", LastStarSystem.PowerState);
                    }
                }

                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to obtain system data", e);
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

        private static void setPluginStatus(ref Dictionary<string, string> values, string status, string error, Exception exception)
        {
            setString(ref values, "EDDI status", status);
            if (status == "Operational")
            {
                setString(ref values, "EDDI error", null);
                setString(ref values, "EDDI exception", null);
            }
            else
            {
                setString(ref values, "EDDI error", error);
                setString(ref values, "EDDI exception", exception.ToString());
            }
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
                    if (nextDigit < 6)
                    {
                        return "Over " + number + " " + order;
                    }
                    else
                    {
                        return "Nearly " + (number + 1) + " " + order;
                    }
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

//        public class SystemEntry
//        {
//            public string name { get; set; }
//            public int visits { get; set; }
//            public long timestamp { get; set; }
//            public long thisVisit { get; set; }
//            public long lastVisit { get; set; }
//            public dynamic data { get; set; }
//        }

//        private static SystemEntry fetchSystemData(string systemName)
//        {
//            SystemEntry entry = new SystemEntry();
//            entry.name = systemName;
//            entry.visits = 1;
//            entry.timestamp = (long)(TimeZoneInfo.ConvertTimeToUtc(DateTime.Now) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
//            entry.thisVisit = (long)(TimeZoneInfo.ConvertTimeToUtc(DateTime.Now) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
//            entry.lastVisit = entry.thisVisit - 36000;
////            entry.data = GetSystemData(systemName);

//            return entry;
//        }
    }
}
