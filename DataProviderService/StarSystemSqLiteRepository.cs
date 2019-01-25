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
        private const string CREATE_SQL = @"
                    CREATE TABLE IF NOT EXISTS starsystems(
                     name TEXT NOT NULL
                     ,totalvisits INT NOT NULL
                     ,lastvisit DATETIME NOT NULL
                     ,starsystem TEXT NOT NULL
                     ,starsystemlastupdated DATETIME NOT NULL)";
        private const string CREATE_INDEX_SQL = @"
                    CREATE INDEX IF NOT EXISTS starsystems_idx_1
                    ON starsystems(name)";
        private const string INSERT_SQL = @"
                    INSERT INTO starsystems(
                       name
                     , totalvisits
                     , lastvisit
                     , starsystem
                     , starsystemlastupdated)
                    VALUES(@name, @totalvisits, @lastvisit, @starsystem, @starsystemlastupdated);
                    PRAGMA optimize; ";
        private const string UPDATE_SQL = @"
                    UPDATE starsystems
                    SET totalvisits = @totalvisits
                       ,lastvisit = @lastvisit
                       ,starsystem = @starsystem
                       ,starsystemlastupdated = @starsystemlastupdated
                    WHERE LOWER(name) = LOWER(@name)";
        private const string DELETE_SQL = @"
                    DELETE FROM starsystems
                    WHERE LOWER(name) = LOWER(@name);
                    PRAGMA optimize;";
        private const string SELECT_BY_NAME_SQL = @"
                    SELECT totalvisits,
                           lastvisit,
                           starsystem,
                           starsystemlastupdated,
                           comment
                    FROM starsystems
                    WHERE LOWER(name) = LOWER(@name)";
        private const string TABLE_SQL = @"PRAGMA table_info(starsystems)";
        private const string ALTER_ADD_COMMENT_SQL = @"ALTER TABLE starsystems ADD COLUMN comment TEXT";

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

        public StarSystem GetOrCreateStarSystem(string name, bool fetchIfMissing = true)
        {
            return GetOrCreateStarSystems(new string[] { name }).FirstOrDefault();
        }

        public List<StarSystem> GetOrCreateStarSystems(string[] names, bool fetchIfMissing = true)
        {
            List<StarSystem> systems = Instance.GetStarSystems(names, fetchIfMissing);
            List<string> fetchSystems = new List<string>();

            // If a system isn't found after we've read our local database, we need to fetch it.
            foreach (string name in names)
            {
                if (fetchIfMissing && systems.FirstOrDefault(s => s.name == name) == null)
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

            foreach (string name in fetchSystems)
            {
                if (fetchedSystems.Find(s => s.name == name) == null)
                {
                    systems.Add(new StarSystem() { name = name });
                }
            }

            return systems;
        }

        public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true)
        {
            return GetOrFetchStarSystems(new string[] { name }).FirstOrDefault();
        }

        public List<StarSystem> GetOrFetchStarSystems(string[] names, bool fetchIfMissing = true)
        {
            List<StarSystem> systems = Instance.GetStarSystems(names, fetchIfMissing);
            List<string> fetchSystems = new List<string>();

            // If a system isn't found after we've read our local database, we need to fetch it.
            foreach (string name in names)
            {
                if (fetchIfMissing && systems.FirstOrDefault(s => s.name == name) == null)
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
            return GetStarSystems(new string[] { name }).FirstOrDefault();
        }

        public List<StarSystem> GetStarSystems(string[] names, bool refreshIfOutdated = true)
        {
            if (!File.Exists(DbFile))
            {
                return null;
            }

            List<StarSystem> results = new List<StarSystem>();
            List<StarSystem> systemsToUpdate = new List<StarSystem>();
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
                    system.TryGetValue("visits", out object visitVal);
                    system.TryGetValue("comment", out object commentVal);
                    system.TryGetValue("lastvisit", out object lastVisitVal);
                    system.TryGetValue("lastupdated", out object lastUpdatedVal);
                    system.TryGetValue("systemAddress", out object systemAddressVal);

                    int visits = (int)(long)visitVal;
                    string comment = (string)commentVal ?? "";
                    DateTime? lastvisit = (DateTime?)lastVisitVal;
                    DateTime? lastupdated = (DateTime?)lastUpdatedVal;
                    long? systemAddress = (long?)systemAddressVal;

                    if (lastvisit == null || lastupdated == null || comment == "")
                    {
                        if (Instance.OldDataFormat(name, ref visits, comment, ref lastupdated, ref lastvisit))
                        {
                            needToUpdate = true;
                        }
                    }

                    if (refreshIfOutdated && lastupdated < DateTime.UtcNow.AddHours(-1))
                    {
                        // Data is stale
                        StarSystem updatedResult = DataProviderService.GetSystemData(name);
                        if (updatedResult.systemAddress == null && systemAddress != null)
                        {
                            // The "updated" data might be a basic system, empty except for the name. If so, return the old result.
                            updatedResult = DeserializeStarSystem(name, data, ref needToUpdate);
                        }
                        else
                        {
                            needToUpdate = true;
                        }
                        result = updatedResult;
                    }
                    else
                    {
                        // Data is fresh
                        result = DeserializeStarSystem(name, data, ref needToUpdate);
                    }

                    result.visits = visits;
                    result.comment = comment;
                    result.lastvisit = lastvisit;
                    result.lastupdated = DateTime.UtcNow;

                    if (needToUpdate)
                    {
                        systemsToUpdate.Add(result);
                    }
                }
                results.Add(result);
            }
            Instance.updateStarSystems(systemsToUpdate);
            return results;
        }

        private bool OldDataFormat(string name, ref int visits, string comment, ref DateTime? lastupdated, ref DateTime? lastvisit)
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
                            if (lastvisit == null)
                            {
                                // Old-style system; need to update
                                visits = rdr.GetInt32(0);
                                lastvisit = rdr.GetDateTime(1);
                                result = true;
                            }
                            if (lastupdated == null)
                            {
                                lastupdated = rdr.GetDateTime(4);
                                result = true;
                            }
                            if (comment == "")
                            {
                                if (!rdr.IsDBNull(4))
                                {
                                    comment = rdr.GetString(4);
                                    result = true;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private string ReadStarSystem(string name)
        {
            return (string)Instance.ReadStarSystems(new string[] { name }).FirstOrDefault().Value;
        }

        private List<KeyValuePair<string, string>> ReadStarSystems(string[] names)
        {
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
                                        results.Add(new KeyValuePair<string, string>(name, rdr.GetString(2)));
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

        private static StarSystem DeserializeStarSystem(string systemName, string data, ref bool needToUpdate)
        {
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
                    needToUpdate = true;
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
            SaveStarSystems(new List<StarSystem>() { starSystem });
        }

        public void SaveStarSystems(List<StarSystem> starSystems)
        {
            List<StarSystem> deleteAndReInsert = new List<StarSystem>();
            List<StarSystem> update = new List<StarSystem>();

            foreach (KeyValuePair<string, string> dbSystem in Instance.ReadStarSystems(starSystems.Select(s => s.name).ToArray()))
            {
                foreach (StarSystem system in starSystems)
                {
                    if (system.name == dbSystem.Key)
                    {
                        if (dbSystem.Value == null)
                        {
                            deleteAndReInsert.Add(system);
                        }
                        else
                        {
                            update.Add(system);
                        }
                    }
                }
            }

            // Delete and re-insert applicable systems
            if (deleteAndReInsert?.Count > 0)
            {
                Instance.deleteStarSystems(deleteAndReInsert);
                Instance.insertStarSystems(deleteAndReInsert);
            }

            // Update applicable systems
            if (update?.Count > 0)
            {
                Instance.updateStarSystems(update);
            }
        }

        // Triggered when leaving a starsystem - just update lastvisit
        public void LeaveStarSystem(StarSystem system)
        {
            system.lastvisit = DateTime.UtcNow;
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

            foreach (StarSystem systemToInsertOrUpdate in systems)
            {
                // Before we insert we attempt to fetch to ensure that we don't have it present
                StarSystem existingStarSystem = Instance.GetStarSystem(systemToInsertOrUpdate.name, false);
                if (existingStarSystem != null)
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
                                Logging.Debug("Inserting new starsystem " + system.name);
                                if (system.lastvisit == null)
                                {
                                    // DB constraints don't allow this to be null
                                    system.lastvisit = DateTime.UtcNow;
                                }

                                cmd.CommandText = INSERT_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", system.name);
                                cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                                cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.UtcNow);
                                cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                                cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
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
                                cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                                cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.UtcNow);
                                cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                                cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                                cmd.Parameters.AddWithValue("@name", system.name);
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
                                cmd.Parameters.AddWithValue("@name", system.name);
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
                    using (var cmd = new SQLiteCommand(CREATE_SQL, con))
                    {
                        Logging.Debug("Creating starsystem repository");
                        cmd.ExecuteNonQuery();
                    }

                    // Add an index
                    using (var cmd = new SQLiteCommand(CREATE_INDEX_SQL, con))
                    {
                        Logging.Debug("Creating starsystem index");
                        cmd.ExecuteNonQuery();
                    }

                    // Also need to update if an older version
                    bool hasComment = false;
                    using (var cmd = new SQLiteCommand(TABLE_SQL, con))
                    {
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                if ("comment" == rdr.GetString(1))
                                {
                                    hasComment = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!hasComment)
                    {
                        Logging.Info("Updating starsystem repository (1)");
                        using (var cmd = new SQLiteCommand(ALTER_ADD_COMMENT_SQL, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
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
            var updateLogs = Task.Run(() => DataProviderService.syncFromStarMapService(true));
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
