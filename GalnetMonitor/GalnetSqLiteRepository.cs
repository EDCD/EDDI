using EddiDataProviderService;
using Newtonsoft.Json;
using System;
using System.Data.SQLite;
using System.IO;
using Utilities;

namespace GalnetMonitor
{
    public class GalnetSqLiteRepository : SqLiteBaseRepository, GalnetRepository
    {
        private static string CREATE_SQL = @"
                    CREATE TABLE IF NOT EXISTS galnet(
                      uuid TEXT NOT NULL
                     ,category TEXT NOT NULL
                     ,title TEXT NOT NULL
                     ,content TEXT NOT NULL
                     ,published DATETIME NOT NULL
                     ,read INT NOT NULL)";
        private static string CREATE_INDEX_SQL = @"
                    CREATE INDEX IF NOT EXISTS galnet_idx_1
                    ON galnet(uuid)";
        private static string INSERT_SQL = @"
                    INSERT INTO galnet(
                       uuid
                      ,category
                      ,title
                      ,content
                      ,published
                      ,read)
                    VALUES(@uuid, @category, @title, @content, @published, 0)";
        private static string DELETE_SQL = @"DELETE FROM galnet WHERE uuid = @uuid";
        private static string MARK_READ_SQL = @"UPDATE galnet SET read = @read WHERE uuid = @uuid";
        private static string SELECT_BY_UUID_SQL = @"
                    SELECT uuid,
                           category,
                           title,
                           content,
                           published,
                           read
                    FROM galnet
                    WHERE uuid = @uuid";

        private static GalnetSqLiteRepository instance;

        private GalnetSqLiteRepository()
        {
            CreateDatabase();
        }

        private static readonly object instanceLock = new object();
        public static GalnetSqLiteRepository Instance
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
                            instance = new GalnetSqLiteRepository();
                        }
                    }
                }
                return instance;
            }
        }

        private static readonly object insertLock = new object();

        public News GetNews(string uuid)
        {
            if (!File.Exists(DbFile)) return null;
            News result = null;
            try
            {
                using (var con = SimpleDbConnection())
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = SELECT_BY_UUID_SQL;
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@uuid", uuid);
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                result = new News(rdr.GetString(0), rdr.GetDateTime(1), rdr.GetString(2), rdr.GetString(3), rdr.GetString(4));
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
            return result;
        }

        public void SaveNews(News news)
        {
            if (GetNews(news.uuid) == null)
            {
                insertNews(news);
            }
        }

        private void insertNews(News news)
        {
            lock (insertLock)
            {
                // Before we insert we attempt to fetch to ensure that we don't have it present
                News existingNews = GetNews(news.uuid);
                if (existingNews == null)
                {
                    Logging.Debug("Creating new news" + news.title);

                    using (var con = SimpleDbConnection())
                    {
                        con.Open();

                        using (var cmd = new SQLiteCommand(con))
                        {
                            cmd.CommandText = INSERT_SQL;
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@uuid", news.uuid);
                            cmd.Parameters.AddWithValue("@published", news.published);
                            cmd.Parameters.AddWithValue("@category", news.category);
                            cmd.Parameters.AddWithValue("@title", news.title);
                            cmd.Parameters.AddWithValue("@content", news.content);
                            cmd.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
            }
        }

        private void deleteNews(News news)
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = DELETE_SQL;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@uuid", news.uuid);
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public void MarkRead(News news)
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = MARK_READ_SQL;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@read", 1);
                    cmd.Parameters.AddWithValue("@uuid", news.uuid);
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public void MarkUnread(News news)
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = MARK_READ_SQL;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@read", 1);
                    cmd.Parameters.AddWithValue("@uuid", news.uuid);
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
                    Logging.Debug("Creating news repository");
                    cmd.ExecuteNonQuery();
                }

                // Add an index
                using (var cmd = new SQLiteCommand(CREATE_INDEX_SQL, con))
                {
                    Logging.Debug("Creating news index");
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
            Logging.Debug("Created news repository");
        }
    }
}
