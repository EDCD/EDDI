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
        private const string TABLE_GET_SCHEMA_VERSION_SQL = @"PRAGMA user_version;";
        private const string TABLE_SET_SCHEMA_VERSION_SQL = @"PRAGMA user_version = ";

        public long SCHEMA_VERSION { get; private set; } = 0;

        // Append new table columns to the end of the list to maximize compatibility with schema version 0.
        // systemaddress and edsmid must each be unique. 
        // Furthermore, any combination of name, systemaddress, and edsmid must also be unique.
        private const string CREATE_TABLE_SQL = @" 
                    CREATE TABLE IF NOT EXISTS starsystems
                    (
                        name TEXT NOT NULL COLLATE NOCASE,
                        totalvisits INT NOT NULL,
                        lastvisit DATETIME,
                        starsystem TEXT NOT NULL,
                        starsystemlastupdated DATETIME NOT NULL,
                        comment TEXT,
                        systemaddress INT UNIQUE,
                        edsmid INT UNIQUE,
                        CONSTRAINT combined_uniques UNIQUE (name, systemaddress, edsmid)
                     );";
        private const string CREATE_INDEX_SQL = @" 
                    CREATE INDEX IF NOT EXISTS 
                        starsystems_idx_1 ON starsystems(name COLLATE NOCASE);
                    CREATE UNIQUE INDEX IF NOT EXISTS 
                        starsystems_idx_2 ON starsystems(systemaddress) WHERE systemaddress IS NOT NULL;
                    CREATE UNIQUE INDEX IF NOT EXISTS 
                        starsystems_idx_3 ON starsystems(edsmid) WHERE edsmid IS NOT NULL;
                    ";
        private const string TABLE_INFO_SQL = @"PRAGMA table_info(starsystems)";
        private const string REPLACE_TABLE_SQL = @" 
                    PRAGMA foreign_keys=off;
                    BEGIN TRANSACTION;
                    DROP TABLE IF EXISTS old_starsystems;
                    ALTER TABLE starsystems RENAME TO old_starsystems;"
                    + CREATE_TABLE_SQL + INSERT_SQL + @"
                    SELECT DISTINCT
                        name,
                        totalvisits,
                        lastvisit,
                        starsystem,
                        starsystemlastupdated,
                        comment,
                        systemaddress,
                        edsmid
                    FROM old_starsystems;
                    DROP TABLE old_starsystems;
                    COMMIT;
                    PRAGMA foreign_keys=on; 
                    VACUUM;
                    PRAGMA optimize;";

        private const string INSERT_SQL = @" 
                    INSERT INTO starsystems
                    (
                        name,
                        totalvisits,
                        lastvisit,
                        starsystem,
                        starsystemlastupdated,
                        comment,
                        systemaddress,
                        edsmid
                    )";
        private const string UPDATE_SQL = @" 
                    UPDATE starsystems
                        SET 
                            totalvisits = @totalvisits,
                            lastvisit = @lastvisit,
                            starsystem = @starsystem,
                            starsystemlastupdated = @starsystemlastupdated,
                            comment = @comment,
                            systemaddress = @systemaddress,
                            edsmid = @edsmid
                    ";
        private const string DELETE_SQL = @"DELETE FROM starsystems ";
        private const string SELECT_SQL = @"SELECT * FROM starsystems ";
        private const string VALUES_SQL = @" 
                    VALUES
                    (
                        @name, 
                        @totalvisits, 
                        @lastvisit, 
                        @starsystem, 
                        @starsystemlastupdated,
                        @comment,
                        @systemaddress,
                        @edsmid
                    )";
        private const string WHERE_SYSTEMADDRESS = @"WHERE systemaddress = @systemaddress; PRAGMA optimize;";
        private const string WHERE_EDSMID = @"WHERE edsmid = @edsmid; PRAGMA optimize;";
        private const string WHERE_NAME = @"WHERE name = @name; PRAGMA optimize;";

        private static StarSystemSqLiteRepository instance;

        private StarSystemSqLiteRepository()
        { }

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
                            CreateOrUpdateDatabase();
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
                Instance.SaveStarSystems(fetchedSystems);
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
                    system.TryGetValue("systemAddress", out object systemAddressVal);
                    system.TryGetValue("EDSMID", out object edsmIdVal);

                    string comment = (string)commentVal ?? "";
                    DateTime? lastupdated = (DateTime?)lastUpdatedVal;
                    long? systemAddress = (long?)systemAddressVal;
                    long? edsmId = (long?)edsmIdVal;

                    if (refreshIfOutdated)
                    {
                        if (lastupdated < DateTime.UtcNow.AddHours(-1))
                        {
                            // Data is stale
                            needToUpdate = true;
                        }
                        else if (lastupdated is null)
                        {
                            // We have no record of ever updating this star system
                            needToUpdate = true;
                        }
                        else if (SCHEMA_VERSION >= 2 && (systemAddress is null || edsmId is null))
                        {
                            // Obtain data for optimized data searches starting with schema version 2
                            needToUpdate = true;
                        }
                    }

                    if (needToUpdate)
                    {
                        // We want to update this star system
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
                    if (starSystem.systemAddress == null)
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
                                cmd.CommandText = SELECT_SQL + WHERE_NAME;
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

        private List<StarSystemDatabaseResult> ReadStarSystems(List<StarSystem> starSystems)
        {
            if (starSystems.Count() == 0) { return null; }

            List<StarSystemDatabaseResult> results = new List<StarSystemDatabaseResult>();
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
                                if (starSystem.systemAddress != null)
                                {
                                    cmd.CommandText = SELECT_SQL + WHERE_SYSTEMADDRESS;
                                }
                                else if (starSystem.EDSMID != null)
                                {
                                    cmd.CommandText = SELECT_SQL + WHERE_EDSMID;
                                }
                                else
                                {
                                    cmd.CommandText = SELECT_SQL + WHERE_NAME;
                                }
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", starSystem.systemname);
                                cmd.Parameters.AddWithValue("@systemaddress", starSystem.systemAddress);
                                cmd.Parameters.AddWithValue("@edsmid", starSystem.EDSMID);
                                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                                {
                                    if (rdr.Read())
                                    {
                                        string systemName = null;
                                        long? systemAddress = null;
                                        long? edsmId = null;
                                        string starSystemJson = null;
                                        for (int i = 0; i < rdr.FieldCount; i++)
                                        {
                                            if (SCHEMA_VERSION >= 2 && rdr.GetName(i) == "systemaddress")
                                            {
                                                systemAddress = rdr.IsDBNull(i) ? null : (long?)rdr.GetInt64(i);
                                            }
                                            if (SCHEMA_VERSION >= 2 && rdr.GetName(i) == "edsmid")
                                            {
                                                edsmId = rdr.IsDBNull(i) ? null : (long?)rdr.GetInt64(i);
                                            }
                                            if (rdr.GetName(i) == "name")
                                            {
                                                systemName = rdr.GetString(i);
                                            }
                                            if (rdr.GetName(i) == "starsystem")
                                            {
                                                starSystemJson = rdr.GetString(i);
                                            }
                                        }
                                        if ((systemAddress != null || edsmId != null || systemName != null) && starSystemJson != null)
                                        {
                                            results.Add(new StarSystemDatabaseResult(systemName, systemAddress, edsmId, starSystemJson));
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
                StarSystemDatabaseResult dbSystem = dbSystems.FirstOrDefault(s =>
                    s.systemAddress != null && s.systemAddress == system.systemAddress ? true :
                    s.edsmId != null && s.edsmId == system.EDSMID ? true :
                    s.systemName == system.systemname ? true : false);

                if (dbSystem?.starSystemJson is null ||
                    (dbSystem.systemAddress is null && dbSystem.edsmId is null))
                {
                    // If we're updating to schema version 2, systemAddress and edsmId will both be null. 
                    // Use our delete method to purge all obsolete copies of the star system from the database,
                    // then re-add the star system.
                    delete.Add(system);
                    insert.Add(system);
                }
                else
                {
                    update.Add(system);
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
                                Logging.Debug("Inserting new starsystem " + system.systemname);
                                cmd.CommandText = INSERT_SQL + VALUES_SQL;
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
                                if (system.systemAddress != null)
                                {
                                    cmd.CommandText = UPDATE_SQL + WHERE_SYSTEMADDRESS;
                                }
                                else if (system.EDSMID != null)
                                {
                                    cmd.CommandText = UPDATE_SQL + WHERE_EDSMID;
                                }
                                else
                                {
                                    cmd.CommandText = UPDATE_SQL + WHERE_NAME;
                                }
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", system.systemname);
                                cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                                cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.UtcNow);
                                cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                                cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                                cmd.Parameters.AddWithValue("@comment", system.comment);
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
                                // Delete all possible variations of this data from the database.
                                if (system.systemAddress != null)
                                {
                                    cmd.CommandText = DELETE_SQL + WHERE_SYSTEMADDRESS;
                                    cmd.Prepare();
                                    cmd.Parameters.AddWithValue("@systemaddress", system.systemAddress);
                                    cmd.ExecuteNonQuery();
                                }
                                if (system.EDSMID != null)
                                {
                                    cmd.CommandText = DELETE_SQL + WHERE_EDSMID;
                                    cmd.Prepare();
                                    cmd.Parameters.AddWithValue("@edsmid", system.EDSMID);
                                    cmd.ExecuteNonQuery();
                                }
                                if (!string.IsNullOrEmpty(system.systemname))
                                {
                                    cmd.CommandText = DELETE_SQL + WHERE_NAME;
                                    cmd.Prepare();
                                    cmd.Parameters.AddWithValue("@name", system.systemname);
                                    cmd.ExecuteNonQuery();
                                }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2100:Review SQL queries for security vulnerabilities")]
        // The schema version is a constant set at the top of the file, thus usage here is perfectly correct.
        private static void CreateOrUpdateDatabase()
        {
            using (var con = SimpleDbConnection())
            {
                try
                {
                    con.Open();

                    using (var cmd = new SQLiteCommand(CREATE_TABLE_SQL, con))
                    {
                        Logging.Debug("Preparing starsystem repository");
                        cmd.ExecuteNonQuery();
                    }

                    // Get schema version 
                    using (var cmd = new SQLiteCommand(TABLE_GET_SCHEMA_VERSION_SQL, con))
                    {
                        Instance.SCHEMA_VERSION = (long)cmd.ExecuteScalar();
                        Logging.Debug("Starsystem repository is schema version " + Instance.SCHEMA_VERSION);
                    }

                    // Apply any necessary updates
                    if (Instance.SCHEMA_VERSION < 1)
                    {
                        Logging.Debug("Updating starsystem repository to schema version 1");
                        AddColumnIfMissing(con, "comment");
                        Instance.SCHEMA_VERSION = 1;
                    }
                    if (Instance.SCHEMA_VERSION < 2)
                    {
                        Logging.Debug("Updating starsystem repository to schema version 2");

                        // Allocate our new columns
                        AddColumnIfMissing(con, "systemaddress");
                        AddColumnIfMissing(con, "edsmid");

                        // We have to replace our table with a new copy to assign our new columns as unique
                        using (var cmd = new SQLiteCommand(REPLACE_TABLE_SQL, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        Instance.SCHEMA_VERSION = 2;
                    }

                    // Add our indices
                    using (var cmd = new SQLiteCommand(CREATE_INDEX_SQL, con))
                    {
                        Logging.Debug("Creating starsystem index");
                        cmd.ExecuteNonQuery();
                    }

                    // Set schema version 
                    using (var cmd = new SQLiteCommand(TABLE_SET_SCHEMA_VERSION_SQL + Instance.SCHEMA_VERSION + ";", con))
                    {
                        Logging.Info("Starsystem repository schema is version " + Instance.SCHEMA_VERSION);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    handleSqlLiteException(con, ex);
                }
            }
            Logging.Debug("Starsystem repository ready.");
        }

        /// <summary> Valid columnNames are "systemaddress", "edsmid", and "comment" </summary>
        private static bool AddColumnIfMissing(SQLiteConnection con, string columnName)
        {
            bool result = false;

            // Parameters like `DISTINCT` cannot be set on columns by this method
            string command = string.Empty;
            switch (columnName)
            {
                case "systemaddress":
                    command = @"ALTER TABLE starsystems ADD COLUMN systemaddress INT";
                    break;
                case "edsmid":
                    command = @"ALTER TABLE starsystems ADD COLUMN edsmid int";
                    break;
                case "comment":
                    command = @"ALTER TABLE starsystems ADD COLUMN comment TEXT;";
                    break;
            }
            if (!string.IsNullOrEmpty(command))
            {
                bool columnExists = false;
                using (var cmd = new SQLiteCommand(TABLE_INFO_SQL, con))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            if (columnName == rdr.GetString(1))
                            {
                                columnExists = true;
                                break;
                            }
                        }
                    }
                }
                if (!columnExists)
                {
                    Logging.Debug("Updating starsystem repository with new column " + columnName);
                    try
                    {
                        using (var cmd = new SQLiteCommand(command, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        result = true;
                    }
                    catch (SQLiteException ex)
                    {
                        handleSqlLiteException(con, ex);
                    }
                }
            }
            return result;
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
            CreateOrUpdateDatabase();
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
                con.Dispose();
            }
        }

        protected internal class StarSystemDatabaseResult
        {
            // Data as read from columns in our database
            public string systemName { get; private set; } = null;
            public long? systemAddress { get; private set; } = null;
            public long? edsmId { get; private set; } = null;
            public string starSystemJson { get; private set; } = null;

            public StarSystemDatabaseResult(string systemName, long? systemAddress, long? edsmId, string starSystemJson)
            {
                this.systemName = systemName;
                this.systemAddress = systemAddress;
                this.edsmId = edsmId;
                this.starSystemJson = starSystemJson;
            }
        }
    }
}
