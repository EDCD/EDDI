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
                     ,previousvisit DATETIME
                     ,starsystem TEXT NOT NULL
                     ,starsystemlastupdated DATETIME NOT NULL)";
        private static string INSERT_SQL = @"
                    INSERT INTO starsystems(
                       name
                     , totalvisits
                     , lastvisit
                     , previousvisit
                     , starsystem
                     , starsystemlastupdated)
                    VALUES(@name, @totalvisits, @lastvisit, @previousvisit, @starsystem, @starsystemlastupdated)";
        private static string UPDATE_SQL = @"
                    UPDATE starsystems
                    SET totalvisits = @totalvisits
                       ,lastvisit = @lastvisit
                       ,previousvisit = @previousvisit
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

        static StarSystemSqLiteRepository()
        {
            CreateDatabase();
        }

        public StarSystem GetOrCreateStarSystem(string name)
        {
            StarSystem system = GetStarSystem(name);
            if (system == null)
            {
                system = new StarSystem();
                system.name = name;
                system.visits = 0;
            }
            return system;
        }

        public StarSystem GetStarSystem(string name)
        {
            if (!File.Exists(DbFile)) return null;

            StarSystem result = null;
            try
            {
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
                                Logging.Error("visits is " + result.visits);
                                if (result.visits < 1)
                                {
                                    result.visits = rdr.GetInt32(0);
                                    Logging.Error("visits is now " + result.visits);
                                    result.lastvisit = rdr.GetDateTime(1);
                                    Logging.Error("lastvisit is now " + result.lastvisit);
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
            using (var con = SimpleDbConnection())
            {
                con.Open();

                if (GetStarSystem(starSystem.name) == null)
                {
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = INSERT_SQL;
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@name", starSystem.name);
                        cmd.Parameters.AddWithValue("@totalvisits", starSystem.visits);
                        cmd.Parameters.AddWithValue("@lastvisit", starSystem.lastvisit);
                        cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(starSystem));
                        cmd.Parameters.AddWithValue("@starsystemlastupdated", starSystem.lastupdated);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = UPDATE_SQL;
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@totalvisits", starSystem.visits);
                        cmd.Parameters.AddWithValue("@lastvisit", starSystem.lastvisit);
                        cmd.Parameters.AddWithValue("@starsystem", JsonConvert.SerializeObject(starSystem));
                        cmd.Parameters.AddWithValue("@starsystemlastupdated", starSystem.lastupdated);
                        cmd.Parameters.AddWithValue("@name", starSystem.name);
                        cmd.ExecuteNonQuery();
                    }
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
        }
    }
}
