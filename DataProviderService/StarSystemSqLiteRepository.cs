using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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
                    VALUES(@name, @totalvisits, @lastvisit, @starsystem, @starsystemlastupdated)";
        private const string UPDATE_SQL = @"
                    UPDATE starsystems
                    SET totalvisits = @totalvisits
                       ,lastvisit = @lastvisit
                       ,starsystem = @starsystem
                       ,starsystemlastupdated = @starsystemlastupdated
                    WHERE name = @name";
        private const string DELETE_SQL = @"
                    DELETE FROM starsystems
                    WHERE name = @name";
        private const string SELECT_BY_NAME_SQL = @"
                    SELECT totalvisits,
                           lastvisit,
                           starsystem,
                           starsystemlastupdated,
                           comment
                    FROM starsystems
                    WHERE name = @name";
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

        private static readonly object editLock = new object();

        public StarSystem GetOrCreateStarSystem(string name, bool fetchIfMissing = true)
        {
            StarSystem system = GetStarSystem(name, fetchIfMissing);
            if (system == null)
            {
                if (fetchIfMissing)
                {
                    system = DataProviderService.GetSystemData(name, null, null, null);
                }
                if (system == null)
                {
                    system = new StarSystem();
                    system.name = name;
                }

                insertStarSystem(system);
            }
            return system;
        }

        public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true)
        {
            StarSystem system = GetStarSystem(name, fetchIfMissing);
            if (system == null)
            {
                if (fetchIfMissing)
                {
                    system = DataProviderService.GetSystemData(name, null, null, null);
                }
                if (system != null)
                {
                    insertStarSystem(system);
                }
            }
            return system;
        }

        public StarSystem GetStarSystem(string name, bool refreshIfOutdated = true)
        {
            if (!File.Exists(DbFile)) return null;

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
                                result = JsonConvert.DeserializeObject<StarSystem>(data);
                                if (result == null)
                                {
                                    Logging.Info("Failed to obtain system for " + name);
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
                                        if (!rdr.IsDBNull(4)) result.comment = rdr.GetString(4);
                                    }
                                    if (refreshIfOutdated && result.lastupdated < DateTime.Now.AddHours(-1))
                                    {
                                        // Data is stale
                                        StarSystem updatedResult = DataProviderService.GetSystemData(name, null, null, null);
                                        updatedResult.visits = result.visits;
                                        updatedResult.lastvisit = result.lastvisit;
                                        updatedResult.lastupdated = DateTime.Now;
                                        result = updatedResult;
                                        needToUpdate = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (needToUpdate)
                {
                    updateStarSystem(result);
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Problem reading data from database, attempting to re-obtain from source: " + ex);
                try
                {
                    result = DataProviderService.GetSystemData(name, null, null, null);
                }
                catch (Exception ex2)
                {
                    Logging.Warn("Problem obtaining data from source: " + ex2);
                    result = null;
                }
            }

            return result;
        }

        public void SaveStarSystem(StarSystem starSystem)
        {
            if (GetStarSystem(starSystem.name, false) == null)
            {
                deleteStarSystem(starSystem);
                insertStarSystem(starSystem);
            }
            else
            {
                updateStarSystem(starSystem);
            }
        }

        public void SaveStarSystems(List<StarSystem> starSystems)
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    foreach (StarSystem system in starSystems)
                    {
                        if (GetStarSystem(system.name, false) == null)
                        {
                            lock (editLock)
                            {
                                // Delete the system
                                cmd.CommandText = DELETE_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", system.name);
                                cmd.ExecuteNonQuery();

                                // Re-insert the system
                                Logging.Debug("Creating new starsystem " + system.name);
                                if (system.lastvisit == null)
                                {
                                    // DB constraints don't allow this to be null
                                    system.lastvisit = DateTime.Now;
                                }

                                cmd.CommandText = INSERT_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", system.name);
                                cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                                cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.Now);
                                cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                                cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            lock (editLock)
                            {
                                // Update the system
                                cmd.CommandText = UPDATE_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                                cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.Now);
                                cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                                cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                                cmd.Parameters.AddWithValue("@name", system.name);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        // Triggered when leaving a starsystem - just update lastvisit
        public void LeaveStarSystem(StarSystem system)
        {
            system.lastvisit = DateTime.Now;
            SaveStarSystem(system);
        }

        private void insertStarSystem(StarSystem system)
        {
            lock (editLock)
            {
                // Before we insert we attempt to fetch to ensure that we don't have it present
                StarSystem existingStarSystem = GetStarSystem(system.name, false);
                if (existingStarSystem != null)
                {
                    Logging.Debug("Attempt to insert existing star system - updating instead");
                    updateStarSystem(system);
                }
                else
                {
                    Logging.Debug("Creating new starsystem " + system.name);
                    if (system.lastvisit == null)
                    {
                        // DB constraints don't allow this to be null
                        system.lastvisit = DateTime.Now;
                    }

                    using (var con = SimpleDbConnection())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(con))
                        {
                            cmd.CommandText = INSERT_SQL;
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@name", system.name);
                            cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                            cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                            cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private void updateStarSystem(StarSystem system)
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = UPDATE_SQL;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                    cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit ?? DateTime.Now);
                    cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                    cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                    cmd.Parameters.AddWithValue("@name", system.name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void deleteStarSystem(StarSystem system)
        {
            using (var con = SimpleDbConnection())
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
        }

        private static void CreateDatabase()
        {
            using (var con = SimpleDbConnection())
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
            Logging.Debug("Created starsystem repository");
        }
    }
}
