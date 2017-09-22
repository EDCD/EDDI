using EddiDataProviderService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Utilities;

namespace GalnetMonitor
{
    public class GalnetSqLiteRepository : SqLiteBaseRepository, GalnetRepository
    {
        private const string CREATE_SQL = @"
                    CREATE TABLE IF NOT EXISTS galnet(
                      uuid TEXT NOT NULL
                     ,category TEXT NOT NULL
                     ,title TEXT NOT NULL
                     ,content TEXT NOT NULL
                     ,published DATETIME NOT NULL
                     ,read BOOLEAN NOT NULL)";
        private const string CREATE_INDEX_SQL = @"
                    CREATE UNIQUE INDEX IF NOT EXISTS galnet_idx_1
                    ON galnet(uuid)";
        private const string INSERT_SQL = @"
                    INSERT INTO galnet(
                       uuid
                      ,category
                      ,title
                      ,content
                      ,published
                      ,read)
                    VALUES(@uuid, @category, @title, @content, @published, 0)";
        private const string DELETE_SQL = @"DELETE FROM galnet WHERE uuid = @uuid";
        private const string TRUNCATE_SQL = @"DELETE FROM galnet";
        private const string MARK_READ_SQL = @"UPDATE galnet SET read = @read WHERE uuid = @uuid";
        private const string SELECT_BY_UUID_SQL = @"
                    SELECT uuid,
                           category,
                           title,
                           content,
                           published,
                           read
                    FROM galnet
                    WHERE uuid = @uuid";
        private const string SELECT_ALL_UNREAD_SQL = @"
                    SELECT uuid,
                           category,
                           title,
                           content,
                           published,
                           read
                    FROM galnet
                    WHERE read = 0
                    ORDER BY published DESC";
        private const string SELECT_CATEGORY_UNREAD_SQL = @"
                    SELECT uuid,
                           category,
                           title,
                           content,
                           published,
                           read
                    FROM galnet
                    WHERE read = 0
                      AND category = @category
                    ORDER BY published DESC";
        private const string SELECT_CATEGORY_SQL = @"
                    SELECT uuid,
                           category,
                           title,
                           content,
                           published,
                           read
                    FROM galnet
                    WHERE category = @category
                    ORDER BY published DESC";

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
                            Logging.Debug("No GalnetSqLiteRepository instance: creating one");
                            instance = new GalnetSqLiteRepository();
                        }
                    }
                }
                return instance;
            }
        }

        private static readonly object insertLock = new object();

        public News GetArticle(string uuid)
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
                                result = new News(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3), rdr.GetDateTime(4), rdr.GetBoolean(5));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Problem obtaining data: " + ex);
            }
            return result;
        }

        public List<News> GetArticles(string category = null, bool incRead = false)
        {
            if (!File.Exists(DbFile)) return null;

            List<News> result = null;
            try
            {
                using (var con = SimpleDbConnection())
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(con))
                    {
                        if (category != null)
                        {
                            if (incRead)
                            {
                                cmd.CommandText = SELECT_CATEGORY_SQL;
                            }
                            else
                            {
                                cmd.CommandText = SELECT_CATEGORY_UNREAD_SQL;
                            }
                        }
                        else
                        {
                            cmd.CommandText = SELECT_ALL_UNREAD_SQL;
                        }
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@category", category);
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                if (result == null) result = new List<News>();
                                result.Add(new News(Convert.ToString(rdr["uuid"]), Convert.ToString(rdr["category"]), Convert.ToString(rdr["title"]), Convert.ToString(rdr["content"]), Convert.ToDateTime(rdr["published"]), Convert.ToBoolean(rdr["read"])));
                            }
                        }
                    }
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
            if (GetArticle(news.id) == null)
            {
                InsertNews(news);
            }
        }

        private void InsertNews(News news)
        {
            lock (insertLock)
            {
                // Before we insert we attempt to fetch to ensure that we don't have it present
                News existingNews = GetArticle(news.id);
                if (existingNews == null)
                {
                    Logging.Debug("Creating new news" + news.title);

                    using (var con = SimpleDbConnection())
                    {
                        con.Open();

                        using (var cmd = new SQLiteCommand(con))
                        {
                            try
                            {
                                cmd.CommandText = INSERT_SQL;
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@uuid", news.id);
                                cmd.Parameters.AddWithValue("@published", news.published);
                                cmd.Parameters.AddWithValue("@category", news.category);
                                cmd.Parameters.AddWithValue("@title", news.title);
                                cmd.Parameters.AddWithValue("@content", news.content);
                                cmd.ExecuteNonQuery();
                            }
                            catch (SQLiteException sle)
                            {
                                Logging.Warn("Failed to insert news", sle);
                            }
                        }
                    }
                }
            }
        }

        public void DeleteNews()
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = TRUNCATE_SQL;
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteNews(News news)
        {
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = DELETE_SQL;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@uuid", news.id);
                    cmd.ExecuteNonQuery();
                }
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
                    cmd.Parameters.AddWithValue("@uuid", news.id);
                    cmd.ExecuteNonQuery();
                }
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
                    cmd.Parameters.AddWithValue("@uuid", news.id);
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
                    Logging.Debug("Creating galnet repository");
                    cmd.ExecuteNonQuery();
                }

                // Add an index
                using (var cmd = new SQLiteCommand(CREATE_INDEX_SQL, con))
                {
                    Logging.Debug("Creating galnet index");
                    cmd.ExecuteNonQuery();
                }
            }
            Logging.Debug("Created galnet repository");
        }
    }
}
