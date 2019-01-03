using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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
            StarSystem system = Instance.GetStarSystem(name, fetchIfMissing);
            if (system == null)
            {
                if (fetchIfMissing)
                {
                    system = DataProviderService.GetSystemData(name);
                }
                if (system == null)
                {
                    system = new StarSystem
                    {
                        name = name
                    };
                }
                Instance.insertStarSystem(system);
            }
            return system;
        }

        public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true)
        {
            StarSystem system = Instance.GetStarSystem(name, fetchIfMissing);
            if (system == null)
            {
                if (fetchIfMissing)
                {
                    system = DataProviderService.GetSystemData(name);
                }
                if (system != null)
                {
                    Instance.insertStarSystem(system);
                }
            }
            return system;
        }

        public StarSystem GetStarSystem(string name, bool refreshIfOutdated = true)
        {
            if (!File.Exists(DbFile))
            {
                return null;
            }

            StarSystem result = null;
            try
            {
                bool needToUpdate = false;
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
                                string data = rdr.GetString(2);
                                // Old versions of the data could have a string "No volcanism" for volcanism.  If so we remove it
                                data = data.Replace(@"""No volcanism""", "null");

                                try
                                {
                                    result = JsonConvert.DeserializeObject<StarSystem>(data);
                                    if (result == null)
                                    {
                                        Logging.Info("Failed to obtain system for " + name + " from the SQLiteRepository");
                                    }
                                    if (result != null)
                                    {
                                        if (result.visits < 1)
                                        {
                                            // Old-style system; need to update
                                            result.visits = rdr.GetInt32(0);
                                            result.lastvisit = rdr.GetDateTime(1);
                                            needToUpdate = true;
                                        }
                                        if (result.lastupdated == null)
                                        {
                                            result.lastupdated = rdr.GetDateTime(4);
                                        }
                                        if (result.comment == null)
                                        {
                                            if (!rdr.IsDBNull(4))
                                            {
                                                result.comment = rdr.GetString(4);
                                            }
                                        }
                                    }
                                    if (refreshIfOutdated && result.lastupdated < DateTime.UtcNow.AddHours(-1))
                                    {
                                        // Data is stale
                                        StarSystem updatedResult = DataProviderService.GetSystemData(name);
                                        if (updatedResult.systemAddress == null && result.systemAddress != null)
                                        {
                                            // The "updated" data might be a basic system, empty except for the name. 
                                            // If so, return the old result.
                                            return result;
                                        }
                                        else
                                        {
                                            updatedResult.visits = result.visits;
                                            updatedResult.lastvisit = result.lastvisit;
                                            updatedResult.lastupdated = DateTime.UtcNow;
                                            result = updatedResult;
                                            needToUpdate = true;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    Logging.Warn("Problem reading data for star system '" + name + "' from database, re-obtaining from source. ");
                                    try
                                    {
                                        result = DataProviderService.GetSystemData(name);
                                        // Recover data unique to the local user and database
                                        IDictionary<string, object> system = Deserializtion.DeserializeData(data);
                                        system.TryGetValue("visits", out object visitVal);
                                        result.visits = (int)(long)visitVal;
                                        system.TryGetValue("comment", out object commentVal);
                                        result.comment = (string)commentVal;
                                        system.TryGetValue("lastvisit", out object lastVisitVal);
                                        result.lastvisit = (DateTime?)lastVisitVal;
                                        result.lastupdated = DateTime.UtcNow;
                                        needToUpdate = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        Logging.Warn("Problem obtaining data from source: " + ex);
                                        result = null;
                                    }
                                }
                            }
                        }
                    }
                }
                if (needToUpdate)
                {
                    Instance.updateStarSystem(result);
                }
            }
            catch (SQLiteException)
            {
                Logging.Warn("Problem reading data for star system '" + name + "' from database, refreshing database and re-obtaining from source.");
                RecoverStarSystemDB();
                Instance.GetStarSystem(name);
            }

            return result;
        }

        public void SaveStarSystem(StarSystem starSystem)
        {
            if (Instance.GetStarSystem(starSystem.name, false) == null)
            {
                Instance.deleteStarSystem(starSystem);
                Instance.insertStarSystem(starSystem);
            }
            else
            {
                Instance.updateStarSystem(starSystem);
            }
        }

        public void SaveStarSystems(List<StarSystem> starSystems)
        {
            foreach (StarSystem system in starSystems)
            {
                if (Instance.GetStarSystem(system.name, false) == null)
                {
                    // Delete the system
                    Instance.deleteStarSystem(system);

                    // Re-insert the system
                    Instance.insertStarSystem(system);
                }
                else
                {
                    // Update the system
                    Instance.updateStarSystem(system);
                }
            }
        }

        // Triggered when leaving a starsystem - just update lastvisit
        public void LeaveStarSystem(StarSystem system)
        {
            system.lastvisit = DateTime.UtcNow;
            SaveStarSystem(system);
        }

        private void insertStarSystem(StarSystem system)
        {
            // Before we insert we attempt to fetch to ensure that we don't have it present
            StarSystem existingStarSystem = Instance.GetStarSystem(system.name, false);
            if (existingStarSystem != null)
            {
                Logging.Debug("Attempt to insert existing star system - updating instead");
                Instance.updateStarSystem(system);
            }
            else
            {
                Logging.Debug("Creating new starsystem " + system.name);
                if (system.lastvisit == null)
                {
                    // DB constraints don't allow this to be null
                    system.lastvisit = DateTime.UtcNow;
                }

                using (var con = SimpleDbConnection())
                {
                    try
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(con))
                        {
                            cmd.CommandText = INSERT_SQL;
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@name", system.name);
                            cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                            cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.UtcNow);
                            cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                            cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        handleSqlLiteException(con, ex);
                    }
                }
            }
        }

        private void updateStarSystem(StarSystem system)
        {
            using (var con = SimpleDbConnection())
            {
                try
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
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
                }
                catch (SQLiteException ex)
                {
                    handleSqlLiteException(con, ex);
                }
            }
        }

        private void deleteStarSystem(StarSystem system)
        {
            using (var con = SimpleDbConnection())
            {
                try
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = DELETE_SQL;
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@name", system.name);
                        cmd.ExecuteNonQuery();
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
