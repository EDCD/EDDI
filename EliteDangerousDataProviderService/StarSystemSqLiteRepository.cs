using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Data.SQLite;
using System.IO;
using Utilities;

namespace EliteDangerousDataProviderService
{
    public class StarSystemSqLiteRepository : SqLiteBaseRepository, StarSystemRepository
    {
        private static string CREATE_SQL = @"
                    CREATE TABLE IF NOT EXISTS starsystems(
                     name TEXT NOT NULL
                     ,totalvisits INT NOT NULL
                     ,lastvisit DATETIME NOT NULL
                     ,starsystem TEXT NOT NULL
                     ,starsystemlastupdated DATETIME NOT NULL)";
        private static string INSERT_SQL = @"
                    INSERT INTO starsystems(
                       name
                     , totalvisits
                     , lastvisit
                     , starsystem
                     , starsystemlastupdated)
                    VALUES(@name, @totalvisits, @lastvisit, @starsystem, @starsystemlastupdated)";
        private static string UPDATE_SQL = @"
                    UPDATE starsystems
                    SET totalvisits = @totalvisits
                       ,lastvisit = @lastvisit
                       ,starsystem = @starsystem
                       ,starsystemlastupdated = @starsystemlastupdated
                    WHERE name = @name";
        private static string SELECT_BY_NAME_SQL = @"
                    SELECT totalvisits,
                           lastvisit,
                           starsystem,
                           starsystemlastupdated,
                           comment
                    FROM starsystems
                    WHERE name = @name";
        private static string TABLE_SQL = @"PRAGMA table_info(starsystems)";
        private static string ALTER_ADD_COMMENT_SQL = @"ALTER TABLE starsystems ADD COLUMN comment TEXT";

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

        public StarSystem GetOrCreateStarSystem(string name)
        {
            StarSystem system = GetStarSystem(name);
            if (system == null)
            {
                system = DataProviderService.GetSystemData(name, null, null, null);
                if (system == null)
                {
                    Logging.Warn("Failed to obtain information for system " + name);
                    system = new StarSystem();
                    system.name = name;
                    system.visits = 0;
                }
            }
            return system;
        }

        public StarSystem GetStarSystem(string name)
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
                                result = JsonConvert.DeserializeObject<StarSystem>(rdr.GetString(2));
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
                            }
                        }
                    }
                    con.Close();
                }
                if (needToUpdate)
                {
                    updateStarSystem(result);
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Problem obtaining data: " + ex);
            }

            // TODO if star system data is out-of-date then refresh it
            return result;
        }

        public void SaveStarSystem(StarSystem starSystem)
        {
            if (GetStarSystem(starSystem.name) == null)
            {
                insertStarSystem(starSystem);
            }
            else
            {
                updateStarSystem(starSystem);
            }
        }

        private void insertStarSystem(StarSystem system)
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = INSERT_SQL;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@name", system.name);
                    cmd.Parameters.AddWithValue("@totalvisits", system.visits);
                    cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit);
                    cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                    cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                    cmd.ExecuteNonQuery();
                }
                con.Close();
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
                    cmd.Parameters.AddWithValue("@lastvisit", system.lastvisit);
                    cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(system));
                    cmd.Parameters.AddWithValue("@starsystemlastupdated", system.lastupdated);
                    cmd.Parameters.AddWithValue("@name", system.name);
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        private static void CreateDatabase()
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(CREATE_SQL, con))
                {
                    Logging.Info("Creating starsystem repository");
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
                con.Close();
            }
            Logging.Info("Created starsystem repository");
        }
    }
}
