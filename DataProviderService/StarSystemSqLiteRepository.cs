using EddiDataDefinitions;
using EddiStarMapService;
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

        public long SCHEMA_VERSION { get; private set; }

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

        private readonly IEdsmService edsmService;
        private readonly DataProviderService dataProviderService;
        private static StarSystemSqLiteRepository instance;
        private StarSystemCache starSystemCache;

        private StarSystemSqLiteRepository(IEdsmService edsmService)
        {
            this.edsmService = edsmService;
            dataProviderService = new DataProviderService(edsmService);
            starSystemCache = new StarSystemCache(30);
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
                            instance = new StarSystemSqLiteRepository(new StarMapService());
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
            return GetOrCreateStarSystems(new[] { name }, fetchIfMissing, refreshIfOutdated).FirstOrDefault();
        }

        public List<StarSystem> GetOrCreateStarSystems(string[] names, bool fetchIfMissing = true, bool refreshIfOutdated = true)
        {
            if (!names.Any()) { return new List<StarSystem>(); }
            List<StarSystem> systems = GetOrFetchStarSystems(names, fetchIfMissing, refreshIfOutdated) ?? new List<StarSystem>();

            // Create a new system object for each name that isn't in the database and couldn't be fetched from a server
            foreach (string name in names)
            {
                if (systems?.Find(s => s?.systemname == name) == null)
                {
                    systems?.Add(new StarSystem() { systemname = name });
                }
            }

            return systems;
        }

        public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true, bool refreshIfOutdated = true)
        {
            if (name == string.Empty) { return null; }
            return GetOrFetchStarSystems(new[] { name }, fetchIfMissing, refreshIfOutdated)?.FirstOrDefault();
        }

        public List<StarSystem> GetOrFetchStarSystems(string[] names, bool fetchIfMissing = true, bool refreshIfOutdated = true)
        {
            if (!names.Any()) { return new List<StarSystem>(); }
            List<StarSystem> systems = GetStarSystems(names, refreshIfOutdated) ?? new List<StarSystem>();

            // If a system isn't found after we've read our local database, we need to fetch it.
            List<string> fetchSystems = new List<string>();
            foreach (string name in names)
            {
                if (fetchIfMissing && systems.FirstOrDefault(s => s.systemname == name) == null)
                {
                    fetchSystems.Add(name);
                }
            }

            List<StarSystem> fetchedSystems = dataProviderService.GetSystemsData(fetchSystems.ToArray());
            if (fetchedSystems?.Count > 0)
            {
                Instance.SaveStarSystems(fetchedSystems);
                systems.AddRange(fetchedSystems);
            }

            return systems;
        }

        public StarSystem GetStarSystem(string name, bool refreshIfOutdated = true)
        {
            if (String.IsNullOrEmpty(name)) { return null; }
            return GetStarSystems(new[] { name }, refreshIfOutdated)?.FirstOrDefault();
        }

        public List<StarSystem> GetStarSystems(string[] names, bool refreshIfOutdated = true)
        {
            List<StarSystem> results = new List<StarSystem>();
            if (!File.Exists(DbFile)) { return results; }
            if (!names.Any()) { return results; }

            List<DatabaseStarSystem> systemsToUpdate = new List<DatabaseStarSystem>();
            List<DatabaseStarSystem> dataSets = Instance.ReadStarSystems(names);

            bool needToUpdate = false;

            for (int i = 0; i < dataSets.Count; i++)
            {
                DatabaseStarSystem dbStarSystem = dataSets[i];
                if (dbStarSystem.systemJson != null)
                {
                    // Old versions of the data could have a string "No volcanism" for volcanism.  If so we remove it
                    dbStarSystem.systemJson = dbStarSystem.systemJson?.Replace(@"""No volcanism""", "null");

                    // Old versions of the data could have a string "InterstellarFactorsContact" for the facilitator station service.  If so we update it
                    dbStarSystem.systemJson = dbStarSystem.systemJson?.Replace(@"""InterstellarFactorsContact""", @"""Facilitator""");

                    if (refreshIfOutdated)
                    {
                        if (dbStarSystem.lastUpdated < DateTime.UtcNow.AddHours(-1))
                        {
                            // Data is stale or we have no record of ever updating this star system
                            needToUpdate = true;
                        }
                        else if (SCHEMA_VERSION >= 2 && (dbStarSystem.systemAddress is null || dbStarSystem.edsmId is null))
                        {
                            // Obtain data for optimized data searches starting with schema version 2
                            needToUpdate = true;
                        }
                    }

                    if (needToUpdate)
                    {
                        // We want to update this star system (don't deserialize the old result at this time)
                        systemsToUpdate.Add(dbStarSystem);
                    }
                    else
                    {
                        // Deserialize the old result
                        StarSystem result = DeserializeStarSystem(dbStarSystem.systemName, dbStarSystem.systemJson, ref needToUpdate);
                        if (result != null)
                        {
                            results.Add(result);
                        }
                        else
                        {
                            // Something went wrong... retrieve new data.
                            systemsToUpdate.Add(dbStarSystem);
                        }
                    }
                }
            }

            if (systemsToUpdate.Count > 0)
            {
                List<StarSystem> updatedSystems = dataProviderService.GetSystemsData(systemsToUpdate.Select(s => s.systemName).ToArray());
                if (updatedSystems == null) { return results; }

                // If the newly fetched star system is an empty object except (for the object name), reject it
                // Return old results when new results have been rejected
                List<string> systemsToRevert = new List<string>();
                foreach (StarSystem starSystem in updatedSystems)
                {
                    if (starSystem.systemAddress == null)
                    {
                        systemsToRevert.Add(starSystem.systemname);
                    }
                }
                updatedSystems.RemoveAll(s => systemsToRevert.Contains(s.systemname));
                foreach (string systemName in systemsToRevert)
                {
                    results.Add(GetStarSystem(systemName, false));
                }

                // Synchronize EDSM visits and comments
                updatedSystems = dataProviderService.syncFromStarMapService(updatedSystems);

                // Update properties that aren't synced from the server and that we want to preserve
                updatedSystems = PreserveUnsyncedProperties(updatedSystems, systemsToUpdate);

                // Update the `lastupdated` timestamps for the systems we have updated
                foreach (StarSystem starSystem in updatedSystems) { starSystem.lastupdated = DateTime.UtcNow; }

                // Add our updated systems to our results
                results.AddRange(updatedSystems);

                // Save changes to our star systems
                Instance.updateStarSystems(updatedSystems);
            }
            return results;
        }

        private static List<StarSystem> PreserveUnsyncedProperties(List<StarSystem> updatedSystems, List<DatabaseStarSystem> systemsToUpdate)
        {
            if (updatedSystems is null) { return new List<StarSystem>(); }
            foreach (StarSystem updatedSystem in updatedSystems)
            {
                foreach (DatabaseStarSystem systemToUpdate in systemsToUpdate)
                {
                    if (updatedSystem.systemname == systemToUpdate.systemName)
                    {
                        IDictionary<string, object> oldStarSystem = Deserializtion.DeserializeData(systemToUpdate.systemJson);

                        if (oldStarSystem != null)
                        {
                            PreserveSystemProperties(updatedSystem, oldStarSystem);
                            PreserveBodyProperties(updatedSystem, oldStarSystem);
                            PreserveFactionProperties(updatedSystem, oldStarSystem);
                            // No station data needs to be carried over at this time.
                        }
                    }
                }
            }
            return updatedSystems;
        }

        private static void PreserveSystemProperties(StarSystem updatedSystem, IDictionary<string, object> oldStarSystem)
        {
            // Carry over StarSystem properties that we want to preserve
            updatedSystem.totalbodies = JsonParsing.getOptionalInt(oldStarSystem, "discoverableBodies") ?? 0;
            if (oldStarSystem.TryGetValue("visitLog", out object visitLogObj))
            {
                // Visits should sync from EDSM, but in case there is a problem with the connection we will also seed back in our old star system visit data
                if (visitLogObj is List<object> oldVisitLog)
                {
                    foreach (DateTime visit in oldVisitLog)
                    {
                        // The SortedSet<T> class does not accept duplicate elements so we can safely add timestamps which may be duplicates of visits already reported from EDSM.
                        // If an item is already in the set, processing continues and no exception is thrown.
                        updatedSystem.visitLog.Add(visit);
                    }
                }
            }
        }

        private static void PreserveBodyProperties(StarSystem updatedSystem, IDictionary<string, object> oldStarSystem)
        {
            // Carry over Body properties that we want to preserve (e.g. exploration data)
            oldStarSystem.TryGetValue("bodies", out object bodiesVal);
            try
            {
                List<Body> oldBodies = JsonConvert.DeserializeObject<List<Body>>(JsonConvert.SerializeObject(bodiesVal));
                updatedSystem.PreserveBodyData(oldBodies, updatedSystem.bodies);
            }
            catch (Exception e) when (e is JsonReaderException || e is JsonWriterException || e is JsonException)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "value", bodiesVal },
                    { "exception", e }
                };
                Logging.Error("Failed to read exploration data for bodies in " + updatedSystem.systemname + " from database.", data);
            }
        }

        private static void PreserveFactionProperties(StarSystem updatedSystem, IDictionary<string, object> oldStarSystem)
        {
            // Carry over Faction properties that we want to preserve (e.g. reputation data)
            oldStarSystem.TryGetValue("factions", out object factionsVal);
            try
            {
                if (factionsVal != null)
                {
                    List<Faction> oldFactions = JsonConvert.DeserializeObject<List<Faction>>(JsonConvert.SerializeObject(factionsVal));
                    if (oldFactions?.Count > 0)
                    {
                        foreach (var updatedFaction in updatedSystem.factions)
                        {
                            foreach (var oldFaction in oldFactions)
                            {
                                if (updatedFaction.name == oldFaction.name)
                                {
                                    updatedFaction.myreputation = oldFaction.myreputation;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) when (e is JsonReaderException || e is JsonWriterException || e is JsonException)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "value", factionsVal },
                    { "exception", e }
                };
                Logging.Error("Failed to read commander faction reputation data for " + updatedSystem.systemname + " from database.", data);
            }
        }

        private List<DatabaseStarSystem> ReadStarSystems(string[] names)
        {
            if (!names.Any()) { return new List<DatabaseStarSystem>(); }

            List<DatabaseStarSystem> results = new List<DatabaseStarSystem>();
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    using (con.BeginTransaction())
                    {
                        foreach (string name in names)
                        {
                            try
                            {
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@name", name);
                                cmd.CommandText = SELECT_SQL + WHERE_NAME;
                                results.Add(ReadStarSystemEntry(cmd) ?? new DatabaseStarSystem(name, null, null, string.Empty));
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

        private List<DatabaseStarSystem> ReadStarSystems(List<StarSystem> starSystems)
        {
            List<DatabaseStarSystem> results = new List<DatabaseStarSystem>();
            if (!starSystems.Any()) { return results; }
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    using (con.BeginTransaction())
                    {
                        foreach (StarSystem starSystem in starSystems)
                        {
                            try
                            {
                                cmd.Prepare();
                                if (starSystem.systemAddress != null)
                                {
                                    cmd.Parameters.AddWithValue("@systemaddress", starSystem.systemAddress);
                                    cmd.CommandText = SELECT_SQL + WHERE_SYSTEMADDRESS;
                                }
                                else if (starSystem.EDSMID != null)
                                {
                                    cmd.Parameters.AddWithValue("@edsmid", starSystem.EDSMID);
                                    cmd.CommandText = SELECT_SQL + WHERE_EDSMID;
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@name", starSystem.systemname);
                                    cmd.CommandText = SELECT_SQL + WHERE_NAME;
                                }
                                results.Add(ReadStarSystemEntry(cmd) ?? new DatabaseStarSystem(starSystem.systemname, starSystem.systemAddress, starSystem.EDSMID, string.Empty));
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

        private DatabaseStarSystem ReadStarSystemEntry(SQLiteCommand cmd)
        {
            string systemName = string.Empty;
            long? systemAddress = null;
            long? edsmId = null;
            string starSystemJson = string.Empty;
            string comment = string.Empty;
            DateTime lastUpdated = DateTime.MinValue;
            DateTime? lastVisit = null;
            int totalVisits = 0;

            using (SQLiteDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
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
                            systemName = rdr.IsDBNull(i) ? null : rdr.GetString(i);
                        }
                        if (rdr.GetName(i) == "starsystem")
                        {
                            starSystemJson = rdr.IsDBNull(i) ? null : rdr.GetString(i);
                        }
                        if (rdr.GetName(i) == "comment")
                        {
                            comment = rdr.IsDBNull(i) ? null : rdr.GetString(i);
                        }
                        if (rdr.GetName(i) == "starsystemlastupdated")
                        {
                            lastUpdated = rdr.IsDBNull(i) ? DateTime.MinValue : rdr.GetDateTime(i).ToUniversalTime();
                        }
                        if (rdr.GetName(i) == "lastvisit")
                        {
                            lastVisit = rdr.IsDBNull(i) ? null : (DateTime?)rdr.GetDateTime(i).ToUniversalTime();
                        }
                        if (rdr.GetName(i) == "totalvisits")
                        {
                            totalVisits = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
                        }
                    }
                }
            }
            return new DatabaseStarSystem(systemName, systemAddress, edsmId, starSystemJson)
            {
                comment = comment,
                lastUpdated = lastUpdated,
                lastVisit = lastVisit,
                totalVisits = totalVisits
            };
        }

        private StarSystem DeserializeStarSystem(string systemName, string data, ref bool needToUpdate)
        {
            if (systemName == string.Empty || data == string.Empty) { return null; }

            // Check our short term star system cache for a previously deserialized star system and return that if it is available.
            if (starSystemCache.Contains(systemName))
            {
                return starSystemCache.Get(systemName);
            }

            // Not found in memory, proceed with deserialization
            StarSystem result;
            try
            {
                result = JsonConvert.DeserializeObject<StarSystem>(data);
                if (result == null)
                {
                    Logging.Info("Failed to obtain system for " + systemName + " from the SQLiteRepository");
                    needToUpdate = true;
                }
            }
            catch (Exception)
            {
                Logging.Warn("Problem reading data for star system '" + systemName + "' from database, re-obtaining from source.");
                try
                {
                    result = dataProviderService.GetSystemData(systemName);
                    result = dataProviderService.syncFromStarMapService(new List<StarSystem> { result })?.FirstOrDefault(); // Synchronize EDSM visits and comments
                    needToUpdate = true;
                }
                catch (Exception ex)
                {
                    Logging.Warn("Problem obtaining data from source: " + ex);
                    result = null;
                }
            }

            // Save the deserialized star system to our short term star system cache for reference
            if (result != null)
            {
                starSystemCache.Add(result);
            }

            return result;
        }

        public void SaveStarSystem(StarSystem starSystem)
        {
            if (starSystem == null) { return; }
            SaveStarSystems(new List<StarSystem> { starSystem });
        }

        public void SaveStarSystems(List<StarSystem> starSystems)
        {
            if (!starSystems.Any()) { return; }

            // Update any star systems in our short term star system cache to minimize repeat deserialization
            foreach (var starSystem in starSystems)
            {
                starSystemCache.Remove(starSystem.systemname);
                starSystemCache.Add(starSystem);
            }

            // Determine whether we need to delete, insert, or update each system
            var delete = new List<StarSystem>();
            var update = new List<StarSystem>();
            var insert = new List<StarSystem>();

            var dbSystems = Instance.ReadStarSystems(starSystems);
            foreach (StarSystem system in starSystems)
            {
                DatabaseStarSystem dbSystem = dbSystems.FirstOrDefault(s =>
                    s.systemAddress != null && s.systemAddress == system.systemAddress ||
                    s.edsmId != null && s.edsmId == system.EDSMID ||
                    s.systemName == system.systemname);

                if (dbSystem?.systemJson is null ||
                    dbSystem?.systemAddress is null && dbSystem?.edsmId is null)
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
        private static void AddColumnIfMissing(SQLiteConnection con, string columnName)
        {
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
                    }
                    catch (SQLiteException ex)
                    {
                        handleSqlLiteException(con, ex);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void RecoverStarSystemDB()
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
            Task.Run(() => dataProviderService.syncFromStarMapService());
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
    }
}
