using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataProviderService
{
    public class StarSystemSqLiteRepository : SqLiteBaseRepository, StarSystemRepository
    {
        // systemaddress and edsmid must each be unique. 
        // Furthermore, any combination of name, systemaddress, and edsmid must also be unique.
        private const string CREATE_SQL = @"
                    CREATE TABLE IF NOT EXISTS starsystems(
                        name TEXT NOT NULL,
                        systemaddress INT,
                        edsmid INT,
                        totalvisits INT NOT NULL,
                        lastvisit DATETIME,
                        starsystem TEXT NOT NULL,
                        starsystemlastupdated DATETIME NOT NULL,
                        comment TEXT,
                        CONSTRAINT systemaddress_unique UNIQUE (systemaddress),
                        CONSTRAINT edsmid_unique UNIQUE (edsmid),
                        CONSTRAINT combined_uniques UNIQUE (name, systemaddress, edsmid)
                     );
                     ";
        private const string CREATE_INDEX_SQL = @"
                    CREATE INDEX IF NOT EXISTS 
                        starsystems_idx_1 ON starsystems(name);
                    CREATE UNIQUE INDEX IF NOT EXISTS 
                        starsystems_idx_2 ON starsystems(systemaddress);
                    CREATE UNIQUE INDEX IF NOT EXISTS 
                        starsystems_idx_3 ON starsystems(edsmid);
                    ";
        private const string INSERT_SQL = @"
                    INSERT INTO starsystems(
                        name,
                        systemaddress,
                        edsmid,
                        totalvisits,
                        lastvisit,
                        starsystem,
                        starsystemlastupdated,
                        comment
                    )
                    VALUES(
                        @name, 
                        @systemaddress,
                        @edsmid,
                        @totalvisits, 
                        @lastvisit, 
                        @starsystem, 
                        @starsystemlastupdated,
                        @comment
                    );
                    PRAGMA optimize;
                    ";
        private const string UPDATE_SQL = @"
                    UPDATE starsystems
                    SET 
                        systemaddress = @systemaddress,
                        edsmid = @edsmid,
                        totalvisits = @totalvisits,
                        lastvisit = @lastvisit,
                        starsystem = @starsystem,
                        starsystemlastupdated = @starsystemlastupdated,
                        comment = @comment
                    " + WHERE_SQL;
        private const string DELETE_SQL = @"DELETE FROM starsystems" + WHERE_SQL + @"PRAGMA optimize;";
        private const string SELECT_SQL = @"SELECT * FROM starsystems" + WHERE_SQL;
        private const string SELECT_BY_NAME_SQL = @"SELECT * FROM starsystems WHERE LOWER(name) = LOWER(@name)";
        private const string TABLE_SQL = @"PRAGMA table_info(starsystems)";
        private const string ALTER_ADD_NON_UNIQUE_COLUMNS_SQL = @"ALTER TABLE starsystems ADD COLUMN comment TEXT";
        private const string UPDATE_TABLE_SQL = @"
                    PRAGMA foreign_keys=off;
                    BEGIN TRANSACTION;
                    DROP TABLE old_starsystems;
                    ALTER TABLE starsystems RENAME TO old_starsystems;"
                    + CREATE_SQL +
                    @"INSERT OR IGNORE INTO 
                        starsystems 
                    SELECT DISTINCT * FROM 
                        old_starsystems;
                    COMMIT;
                    PRAGMA foreign_keys=on;
                    ";
        private const string DELETE_DUPLICATE_NAMES_SQL = @"
                    DELETE FROM starsystems
                    WHERE starsystemlastupdated NOT IN
                    (
                        SELECT MAX(starsystemlastupdated)
                        FROM starsystems
                        GROUP BY name
                    );
                    ";
        // Prefer unique columns `systemaddress` and `edsmid` over non-unique column `name`
        private const string WHERE_SQL = @" 
                    WHERE 
                        CASE 
                            WHEN @systemaddress IS NOT NULL THEN systemaddress = @systemaddress
                            WHEN @edsmid IS NOT NULL THEN edsmid = @edsmid 
                        ELSE
                            LOWER(name) = LOWER(@name)
                        END;
                    ";

        private static StarSystemSqLiteRepository instance;

        private StarSystemSqLiteRepository()
        {
            CreateDatabase();
        }

        private static readonly object instanceLock = new object();
        public static StarSystemSqLiteRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No StarSystemSqLiteRepository instance: creating one");
                            instance = new StarSystemSqLiteRepository();
                        }
                    }
                }
                return instance;
            }
        }

        public StarSystem GetOrCreateStarSystem(string name, bool fetchIfMissing = true, bool refreshIfOutdated = true)
        {
            if (name == string.Empty) { return null; }
            return GetOrCreateStarSystems(new string[] { name }, fetchIfMissing, refreshIfOutdated).FirstOrDefault();
        }

        public List<StarSystem> GetOrCreateStarSystems(string[] names, bool fetchIfMissing = true, bool refreshIfOutdated = true)
        {
            List<StarSystem> systems = GetOrFetchStarSystems(names, fetchIfMissing, refreshIfOutdated);

            // Create a new system object for each name that isn't in the database and couldn't be fetched from a server
            foreach (string name in names)
            {
                if (systems?.Find(s => s?.systemname == name) == null)
                {
                    systems.Add(new StarSystem() { systemname = name });
                }
            }

            return systems;
        }

        public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true, bool refreshIfOutdated = true)
        {
            if (name == string.Empty) { return null; }
            return GetOrFetchStarSystems(new string[] { name }, fetchIfMissing, refreshIfOutdated).FirstOrDefault();
        }

        public List<StarSystem> GetOrFetchStarSystems(string[] names, bool fetchIfMissing = true, bool refreshIfOutdated = true)
        {
            if (names.Count() == 0) { return null; }

            List<StarSystem> systems = Instance.GetStarSystems(names, refreshIfOutdated);

            // If a system isn't found after we've read our local database, we need to fetch it.
            List<string> fetchSystems = new List<string>();
            foreach (string name in names)
            {
                if (fetchIfMissing && systems.FirstOrDefault(s => s.systemname == name) == null)
                {
                    fetchSystems.Add(name);
                }
            }

            List<StarSystem> fetchedSystems = DataProviderService.GetSystemsData(fetchSystems.ToArray());
            if (fetchedSystems?.Count > 0)
            {
                Instance.insertStarSystems(fetchedSystems);
                systems.AddRange(fetchedSystems);
            }

            return systems;
        }

        public StarSystem GetStarSystem(string name, bool refreshIfOutdated = true)
        {
            if (name == string.Empty) { return null; }
            return GetStarSystems(new string[] { name }, refreshIfOutdated).FirstOrDefault();
        }

        public List<StarSystem> GetStarSystems(string[] names, bool refreshIfOutdated = true)
        {
            if (!File.Exists(DbFile))
            {
                return null;
            }
            if (names.Count() == 0) { return null; }

            List<StarSystem> results = new List<StarSystem>();
            List<string> systemsToUpdate = new List<string>();
            List<KeyValuePair<string, string>> dataSets = Instance.ReadStarSystems(names);

            foreach (KeyValuePair<string, string> kv in dataSets)
            {
                bool needToUpdate = false;
                StarSystem result = null;

                if (kv.Value != null && kv.Value != "")
                {
                    string name = kv.Key;

                    // Old versions of the data could have a string "No volcanism" for volcanism.  If so we remove it
                    string data = ((string)kv.Value)?.Replace(@"""No volcanism""", "null");

                    // Determine whether our data is stale (We won't deserialize the the entire system if it's stale) 
                    IDictionary<string, object> system = Deserializtion.DeserializeData(data);
                    system.TryGetValue("comment", out object commentVal);
                    system.TryGetValue("lastupdated", out object lastUpdatedVal);

                    string comment = (string)commentVal ?? "";
                    DateTime? lastupdated = (DateTime?)lastUpdatedVal;

                    if (refreshIfOutdated)
                    {
                        if (lastupdated < DateTime.UtcNow.AddHours(-1))
                        {
                            // Data is stale
                            needToUpdate = true;
                        }
                        else if (lastupdated == null)
                        {
                            // Data is old format, need to refresh
                            if (Instance.OldDbFormat(name, comment, ref lastupdated))
                            {
                                needToUpdate = true;
                            }
                        }
                    }

                    if (needToUpdate)
                    {
                        // We'll need to update this star system
                        systemsToUpdate.Add(name);
                    }
                    else
                    {
                        // Deserialize the old result
                        result = DeserializeStarSystem(name, data, ref needToUpdate);
                        if (result != null)
                        {
                            results.Add(result);
                        }
                    }
                }
            }

            if (systemsToUpdate.Count > 0)
            {
                List<StarSystem> updatedSystems = DataProviderService.GetSystemsData(systemsToUpdate.ToArray());

                // If the newly fetched star system is an empty object except (for the object name), reject it
                List<string> systemsToRevert = new List<string>();
                foreach (StarSystem starSystem in updatedSystems)
                {
                    if (starSystem.systemAddress == null || starSystem.x == null || starSystem.y == null || starSystem.z == null)
                    {
                        systemsToRevert.Add(starSystem.systemname);
                    }
                }
                updatedSystems.RemoveAll(s => systemsToRevert.Contains(s.systemname));

                // Return old results when new results have been rejected
                foreach (string systemName in systemsToRevert)
                {
                    results.Add(GetStarSystem(systemName, false));
                }

                // Synchronize EDSM visits and comments
                updatedSystems = DataProviderService.syncFromStarMapService(updatedSystems);

                // Add our updated systems to our results
                results.AddRange(updatedSystems);

                // Save changes to our star systems
                Instance.updateStarSystems(updatedSystems); 
            }

            foreach (StarSystem starSystem in results)
            {
                starSystem.lastupdated = DateTime.UtcNow;
            }
            return results;
        }

        private bool OldDbFormat(string name, string comment, ref DateTime? lastupdated)
        {
            bool result = false;
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = SELECT_BY_NAME_SQL;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@name", name);
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            if (lastupdated == null)
                            {
                                lastupdated = rdr.GetDateTime(4);
                                result = true;
                            }
                            if (string.IsNullOrEmpty(comment))
                            {
                                for (int i = 0; i < rdr.FieldCount; i++)
                                {
                                    if (rdr.GetName(i) == "comment")
                                    {
                                        comment = rdr.GetString(i);
                                        result = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private List<KeyValuePair<string, string>> ReadStarSystems(string[] names)
        {
            if (names.Count() == 0) { return null; }

            List<KeyValuePair<string, string>> results = new List<KeyValuePair<string, string>>();
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    using (var transaction = con.BeginTransaction())
                    {
                        foreach (string name in names)
                        {
                            try
                            {
                                cmd.CommandText = SELECT_BY_NAME_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", name);
                                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                                {
                                    if (rdr.Read())
                                    {
                                        for (int i = 0; i < rdr.FieldCount; i++)
                                        {
                                            if (rdr.GetName(i) == "starsystem")
                                            {
                                                results.Add(new KeyValuePair<string, string>(name, rdr.GetString(i)));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (SQLiteException)
                            {
                                Logging.Warn("Problem reading data for star system '" + name + "' from database, refreshing database and re-obtaining from source.");
                                RecoverStarSystemDB();
                                Instance.GetStarSystem(name);
                            }
                        }
                    }
                }
            }
            return results;
        }

        private List<KeyValuePair<string, string>> ReadStarSystems(List<StarSystem> starSystems)
        {
            if (starSystems.Count() == 0) { return null; }

            List<KeyValuePair<string, string>> results = new List<KeyValuePair<string, string>>();
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    using (var transaction = con.BeginTransaction())
                    {
                        foreach (StarSystem starSystem in starSystems)
                        {
                            try
                            {
                                cmd.CommandText = SELECT_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", starSystem.systemname);
                                cmd.Parameters.AddWithValue("@systemaddress", starSystem.systemAddress);
                                cmd.Parameters.AddWithValue("@edsmid", starSystem.EDSMID);
                                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                                {
                                    if (rdr.Read())
                                    {
                                        for (int i = 0; i < rdr.FieldCount; i++)
                                        {
                                            if (rdr.GetName(i) == "starsystem")
                                            {
                                                results.Add(new KeyValuePair<string, string>(starSystem.systemname, rdr.GetString(i)));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (SQLiteException)
                            {
                                Logging.Warn("Problem reading data for star system '" + starSystem.systemname + "' from database, refreshing database and re-obtaining from source.");
                                RecoverStarSystemDB();
                                Instance.GetStarSystem(starSystem.systemname);
                            }
                        }
                    }
                }
            }
            return results;
        }

        private static StarSystem DeserializeStarSystem(string systemName, string data, ref bool needToUpdate)
        {
            if (systemName == string.Empty || data == string.Empty) { return null; }

            StarSystem result = null;
            try
            {
                result = JsonConvert.DeserializeObject<StarSystem>(data);
                if (result == null)
                {
                    Logging.Info("Failed to obtain system for " + systemName + " from the SQLiteRepository");
                }
            }
            catch (Exception)
            {
                Logging.Warn("Problem reading data for star system '" + systemName + "' from database, re-obtaining from source.");
                try
                {
                    result = DataProviderService.GetSystemData(systemName);
                }
                catch (Exception ex)
                {
                    Logging.Warn("Problem obtaining data from source: " + ex);
                    result = null;
                }
            }
            return result;
        }

        public void SaveStarSystem(StarSystem starSystem)
        {
            if (starSystem == null) { return; }
            SaveStarSystems(new List<StarSystem>() { starSystem });
        }

        public void SaveStarSystems(List<StarSystem> starSystems)
        {
            if (starSystems.Count() == 0) { return; }

            var delete = new List<StarSystem>();
            var update = new List<StarSystem>();
            var insert = new List<StarSystem>();

            var dbSystems = Instance.ReadStarSystems(starSystems);
            foreach (StarSystem system in starSystems)
            {
                KeyValuePair<string, string> dbSystem = dbSystems.FirstOrDefault(s => s.Key == system.systemname);
                if (dbSystem.Key == null)
                {
                    insert.Add(system);
                }
                else
                {
                    if (dbSystem.Value == null)
                    {
                        delete.Add(system);
                        insert.Add(system);
                    }
                    else
                    {
                        update.Add(system);
                    }
                }
            }

            // Delete applicable systems
            Instance.deleteStarSystems(delete);

            // Insert applicable systems
            Instance.insertStarSystems(insert);

            // Update applicable systems
            Instance.updateStarSystems(update);
        }

        // Triggered when leaving a starsystem - just save the star system
        public void LeaveStarSystem(StarSystem system)
        {
            if (system?.systemname == null) { return; }
            SaveStarSystem(system);
        }

        private void insertStarSystems(List<StarSystem> systems)
        {
            if (systems.Count == 0)
            {
                return;
            }

            List<StarSystem> updateStarSystems = new List<StarSystem>();
            List<StarSystem> insertStarSystems = new List<StarSystem>();

            var existingStarSystems = Instance.ReadStarSystems(systems);
            foreach (StarSystem systemToInsertOrUpdate in systems)
            {
                // Before we insert we attempt to fetch to ensure that we don't have it present
                if (existingStarSystems.FirstOrDefault(s => s.Key == systemToInsertOrUpdate.systemname).Value != null)
                {
                    Logging.Debug("Attempt to insert existing star system - updating instead");
                    updateStarSystems.Add(systemToInsertOrUpdate);
                }
                else
                {
                    insertStarSystems.Add(systemToInsertOrUpdate);
                }
            }

            using (var con = SimpleDbConnection())
            {
                try
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        using (var transaction = con.BeginTransaction())
                        {
                            foreach (StarSystem system in insertStarSystems)
                            {
                                Logging.Debug("Inserting new starsystem " + system.systemname);
                                cmd.CommandText = INSERT_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", system.systemname);
                                cmd.Parameters.AddWithValue("@systemaddress", system.systemAddress);
                                cmd.Parameters.AddWithValue("@edsmid", system.EDSMID);
                                cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                                cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.UtcNow);
                                cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                                cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                                cmd.Parameters.AddWithValue("@comment", system.comment);
                                cmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    handleSqlLiteException(con, ex);
                }
            }

            // Update applicable systems
            if (updateStarSystems?.Count > 0)
            {
                Instance.updateStarSystems(updateStarSystems);
            }
        }

        private void updateStarSystems(List<StarSystem> systems)
        {
            if (systems.Count == 0)
            {
                return;
            }

            using (var con = SimpleDbConnection())
            {
                try
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        using (var transaction = con.BeginTransaction())
                        {
                            foreach (StarSystem system in systems)
                            {
                                cmd.CommandText = UPDATE_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", system.systemname);
                                cmd.Parameters.AddWithValue("@systemaddress", system.systemAddress);
                                cmd.Parameters.AddWithValue("@edsmid", system.EDSMID);
                                cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                                cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.UtcNow);
                                cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                                cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                                cmd.Parameters.AddWithValue("@comment", system.comment);
                                cmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    handleSqlLiteException(con, ex);
                }
            }
        }

        private void deleteStarSystems(List<StarSystem> systems)
        {
            if (systems.Count == 0)
            {
                return;
            }

            using (var con = SimpleDbConnection())
            {
                try
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        using (var transaction = con.BeginTransaction())
                        {
                            foreach (StarSystem system in systems)
                            {
                                cmd.CommandText = DELETE_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", system.systemname);
                                cmd.Parameters.AddWithValue("@systemaddress", system.systemAddress);
                                cmd.Parameters.AddWithValue("@edsmid", system.EDSMID);
                                cmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    handleSqlLiteException(con, ex);
                }
            }
        }

        private static void CreateDatabase()
        {
            using (var con = SimpleDbConnection())
            {
                try
                {
                    con.Open();

                    // Check for obsolete star system repository conditions requiring tables to be replaced
                    bool updateTables = true;
                    using (var cmd = new SQLiteCommand(TABLE_SQL, con))
                    {
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                if (rdr.GetName(i) == "systemaddress")
                                {
                                    updateTables = false;
                                    break;
                                }
                            }
                        }
                    }

                    // Update or create our star system repository
                    if (updateTables)
                    {
                        Logging.Info("Updating starsystem repository (1)");
                        using (var cmd = new SQLiteCommand(DELETE_DUPLICATE_NAMES_SQL, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SQLiteCommand(UPDATE_TABLE_SQL, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new SQLiteCommand(CREATE_SQL, con))
                        {
                            Logging.Debug("Creating starsystem repository");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Check for updates that can be performed in-place (without replacing tables)
                    bool addTableColumns = true;
                    using (var cmd = new SQLiteCommand(TABLE_SQL, con))
                    {
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                if ("comment" == rdr.GetString(1))
                                {
                                    addTableColumns = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (addTableColumns)
                    {
                        Logging.Info("Updating starsystem repository (1)");
                        using (var cmd = new SQLiteCommand(ALTER_ADD_NON_UNIQUE_COLUMNS_SQL, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Add our indices
                    using (var cmd = new SQLiteCommand(CREATE_INDEX_SQL, con))
                    {
                        Logging.Debug("Creating starsystem index");
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    handleSqlLiteException(con, ex);
                }
            }
            Logging.Debug("Created starsystem repository");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static void RecoverStarSystemDB()
        {
            using (var con = SimpleDbConnection())
            {
                try
                {
                    con.Close();
                    SQLiteConnection.ClearAllPools();
                    File.Delete(Constants.DATA_DIR + @"\EDDI.sqlite");
                }
                catch (SQLiteException ex)
                {
                    handleSqlLiteException(con, ex);
                }
            }
            CreateDatabase();
            var updateLogs = Task.Run(() => DataProviderService.syncFromStarMapService());
        }

        private static void handleSqlLiteException(SQLiteConnection con, SQLiteException ex)
        {
            Logging.Warn("SQLite error: {0}", ex.ToString());

            try
            {
                con.BeginTransaction()?.Rollback();
            }
            catch (SQLiteException ex2)
            {

                Logging.Warn("SQLite transaction rollback failed.");
                Logging.Warn("SQLite error: {0}", ex2.ToString());

            }
            finally
            {
                con.BeginTransaction()?.Dispose();
            }
        }
    }
}
