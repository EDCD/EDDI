using EddiDataDefinitions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Utilities;

namespace EddiDataProviderService
{
    public class StarSystemSqLiteRepository : SqLiteBaseRepository
    {
        private const string TABLE_GET_SCHEMA_VERSION_SQL = @"PRAGMA user_version;";
        private const string TABLE_SET_SCHEMA_VERSION_SQL = @"PRAGMA user_version = ";

        private long SCHEMA_VERSION { get; set; }

        // Append new table columns to the end of the list to maximize compatibility with schema version 0.
        // systemaddress. 
        // Furthermore, any combination of name and systemaddress must also be unique.
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
                        CONSTRAINT combined_uniques UNIQUE (name, systemaddress)
                     );";
        private const string CREATE_INDEX_SQL = @" 
                    CREATE INDEX IF NOT EXISTS 
                        starsystems_idx_1 ON starsystems(name COLLATE NOCASE);
                    CREATE UNIQUE INDEX IF NOT EXISTS 
                        starsystems_idx_2 ON starsystems(systemaddress) WHERE systemaddress IS NOT NULL;
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
                        systemaddress
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
                        systemaddress
                    )";
        private const string UPDATE_SQL = @" 
                    UPDATE starsystems
                        SET 
                            totalvisits = @totalvisits,
                            lastvisit = @lastvisit,
                            starsystem = @starsystem,
                            starsystemlastupdated = @starsystemlastupdated,
                            comment = @comment,
                            systemaddress = @systemaddress
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
                        @systemaddress
                    )";
        private const string WHERE_SYSTEMADDRESS = @"WHERE systemaddress = @systemaddress; PRAGMA optimize;";
        private const string WHERE_NAME = @"WHERE name = @name; PRAGMA optimize;";

        public StarSystemSqLiteRepository ( SQLiteConnection connection = null )
        {
            CreateOrUpdateDatabase( connection );
        }

        public DatabaseStarSystem GetSqlStarSystem ( ulong systemAddress )
        {
            if ( systemAddress <= 0 ) { return null; }

            return GetSqlStarSystems( new[] { systemAddress } )?.FirstOrDefault();
        }

        public List<DatabaseStarSystem> GetSqlStarSystems ( ulong[] systemAddresses )
        {
            var results = new List<DatabaseStarSystem>();
            if ( !File.Exists( DbFile ) ) { return results; }

            if ( !systemAddresses.Any() ) { return results; }

            results = ReadStarSystems( systemAddresses );
            foreach ( var dbStarSystem in results )
            {
                if ( !string.IsNullOrEmpty( dbStarSystem.systemJson ) )
                {
                    // Old versions of the data could have a string "No volcanism" for volcanism.  If so we remove it
                    dbStarSystem.systemJson = dbStarSystem.systemJson?.Replace( @"""No volcanism""", "null" );

                    // Old versions of the data could have a string "InterstellarFactorsContact" for the facilitator station service.  If so we update it
                    dbStarSystem.systemJson =
                        dbStarSystem.systemJson?.Replace( @"""InterstellarFactorsContact""", @"""Facilitator""" );
                }
            }

            return results;
        }

        [NotNull, ItemNotNull]
        private List<DatabaseStarSystem> ReadStarSystems(ulong[] systemAddresses)
        {
            if (!systemAddresses.Any()) { return new List<DatabaseStarSystem>(); }

            var results = new List<DatabaseStarSystem>();
            using (var con = SimpleDbConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    using (var transaction = con.BeginTransaction())
                    {
                        foreach (var systemAddress in systemAddresses.Where( systemAddress => systemAddress > 0 ) )
                        {
                            try
                            {
                                cmd.Prepare();
                                cmd.Parameters.AddWithValue("@systemaddress", systemAddress );
                                cmd.CommandText = SELECT_SQL + WHERE_SYSTEMADDRESS;
                                var result = ReadStarSystemEntry( cmd );
                                if ( result != null )
                                {
                                    results.Add(result);
                                }
                            }
                            catch (SQLiteException sqle )
                            {
                                Logging.Warn($"Problem reading data for star system '{systemAddress}' from database.", sqle );
                            }
                        }
                        transaction.Commit();
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
                    using (var transaction = con.BeginTransaction())
                    {
                        foreach (StarSystem starSystem in starSystems)
                        {
                            try
                            {
                                cmd.Prepare();
                                    cmd.Parameters.AddWithValue("@systemaddress", starSystem.systemAddress);
                                    cmd.CommandText = SELECT_SQL + WHERE_SYSTEMADDRESS;
                                results.Add(ReadStarSystemEntry(cmd) ?? new DatabaseStarSystem(starSystem.systemname, starSystem.systemAddress, string.Empty));
                            }
                            catch (SQLiteException sqle)
                            {
                                Logging.Warn("Problem reading data for star system '" + starSystem.systemname + "' from database.", sqle );
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            return results;
        }

        private DatabaseStarSystem ReadStarSystemEntry(SQLiteCommand cmd)
        {
            string systemName = string.Empty;
            ulong? systemAddress = null;
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
                            systemAddress = rdr.IsDBNull( i ) ? null : (ulong?)rdr.GetInt64( i );

                            // Skip legacy entries with a null systemAddress for now
                            // Eventually, we want to make this a non-null key field
                            if ( systemAddress is null ) { continue; }
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
            return new DatabaseStarSystem(systemName, systemAddress ?? 0,  starSystemJson)
            {
                comment = comment,
                lastUpdated = lastUpdated,
                lastVisit = lastVisit,
                totalVisits = totalVisits
            };
        }

        public void SaveStarSystem(StarSystem starSystem)
        {
            if (starSystem == null) { return; }
            SaveStarSystems(new List<StarSystem> { starSystem });
        }

        public void SaveStarSystems(List<StarSystem> starSystems)
        {
            // Determine whether we need to delete, insert, or update each system
            var delete = new List<StarSystem>();
            var update = new List<StarSystem>();
            var insert = new List<StarSystem>();

            var dbSystems = ReadStarSystems(starSystems);

            // Determine whether to insert + delete or update the SQL record.
            // Skip records with a zero value for the systemAddress
            foreach (var system in starSystems)
            {
                if ( system.systemAddress == 0 )
                {
                    Logging.Warn($"{system.systemname} has an invalid system address ({system.systemAddress}) and can't be recorded in EDDI's star system database.");
                    continue;
                }

                var dbSystem = dbSystems.FirstOrDefault(s =>
                    s.systemAddress == system.systemAddress ||
                    s.systemName == system.systemname);

                if (dbSystem?.systemJson is null)
                {
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
            deleteStarSystems(delete.ToImmutableList());

            // Insert applicable systems
            insertStarSystems(insert.ToImmutableList() );

            // Update applicable systems
            updateStarSystems(update.ToImmutableList() );
        }

        private void insertStarSystems(ImmutableList<StarSystem> systems)
        {
            if ( systems.Count == 0)
            {
                return;
            }

            lock ( nameof(SimpleDbConnection) ) // Lock before writing to the database
            {
                using ( var con = SimpleDbConnection() )
                {
                    try
                    {
                        con.Open();
                        using ( var cmd = new SQLiteCommand( con ) )
                        {
                            using ( var transaction = con.BeginTransaction() )
                            {
                                foreach ( StarSystem system in systems )
                                {
                                    cmd.Prepare();
                                    cmd.CommandText = INSERT_SQL + VALUES_SQL;
                                    cmd.Parameters.AddWithValue( "@name", system.systemname );
                                    cmd.Parameters.AddWithValue( "@systemaddress", system.systemAddress );
                                    cmd.Parameters.AddWithValue( "@totalvisits", system.visits );
                                    cmd.Parameters.AddWithValue( "@lastvisit", system.lastvisit ?? DateTime.UtcNow );
                                    cmd.Parameters.AddWithValue( "@starsystem", JsonConvert.SerializeObject( system ) );
                                    cmd.Parameters.AddWithValue( "@starsystemlastupdated", system.lastupdated );
                                    cmd.Parameters.AddWithValue( "@comment", system.comment );
                                    Logging.Debug( "Inserting new starsystem " + system.systemAddress, system );
                                    cmd.ExecuteNonQuery();
                                }

                                transaction.Commit();
                            }
                        }
                    }
                    catch ( SQLiteException ex )
                    {
                        handleSqlLiteException( con, ex );
                    }
                }
            }
        }

        internal void updateStarSystems(IImmutableList<StarSystem> systems)
        {
            if (systems.Count == 0)
            {
                return;
            }

            lock ( nameof(SimpleDbConnection) ) // Lock before writing to the database
            {
                using ( var con = SimpleDbConnection() )
                {
                    try
                    {
                        con.Open();
                        using ( var cmd = new SQLiteCommand( con ) )
                        {
                            using ( var transaction = con.BeginTransaction() )
                            {
                                foreach ( var system in systems.ToList() )
                                {
                                    var serializedSystem = JsonConvert.SerializeObject( system );
                                    if ( string.IsNullOrEmpty( serializedSystem ) ) { continue; }

                                    if ( system.systemAddress != 0 )
                                    {
                                        cmd.CommandText = UPDATE_SQL + WHERE_SYSTEMADDRESS;
                                    }
                                    else
                                    {
                                        cmd.CommandText = UPDATE_SQL + WHERE_NAME;
                                    }

                                    cmd.Prepare();
                                    cmd.Parameters.AddWithValue( "@name", system.systemname );
                                    cmd.Parameters.AddWithValue( "@totalvisits", system.visits );
                                    cmd.Parameters.AddWithValue( "@lastvisit", system.lastvisit ?? DateTime.UtcNow );
                                    cmd.Parameters.AddWithValue( "@starsystem", serializedSystem );
                                    cmd.Parameters.AddWithValue( "@starsystemlastupdated", system.lastupdated );
                                    cmd.Parameters.AddWithValue( "@comment", system.comment );
                                    cmd.Parameters.AddWithValue( "@systemaddress", system.systemAddress );
                                    Logging.Debug( "Updating starsystem " + system.systemAddress, system );
                                    cmd.ExecuteNonQuery();
                                }

                                transaction.Commit();
                            }
                        }
                    }
                    catch ( SQLiteException ex )
                    {
                        handleSqlLiteException( con, ex );
                    }
                }
            }
        }

        private void deleteStarSystems(ImmutableList<StarSystem> systems)
        {
            if (systems.Count == 0)
            {
                return;
            }

            lock ( nameof(SimpleDbConnection) ) // Lock before writing to the database
            {
                using ( var con = SimpleDbConnection() )
                {
                    try
                    {
                        con.Open();
                        using ( var cmd = new SQLiteCommand( con ) )
                        {
                            using ( var transaction = con.BeginTransaction() )
                            {
                                foreach ( var system in systems )
                                {
                                    // Delete all possible variations of this data from the database.
                                    if ( system.systemAddress != 0 )
                                    {
                                        cmd.CommandText = DELETE_SQL + WHERE_SYSTEMADDRESS;
                                        cmd.Prepare();
                                        cmd.Parameters.AddWithValue( "@systemaddress", system.systemAddress );
                                        Logging.Debug( "Deleting starsystem " + system.systemAddress );
                                        cmd.ExecuteNonQuery();
                                    }
                                    else if ( !string.IsNullOrEmpty( system.systemname ) )
                                    {
                                        cmd.CommandText = DELETE_SQL + WHERE_NAME;
                                        cmd.Prepare();
                                        cmd.Parameters.AddWithValue( "@name", system.systemname );
                                        Logging.Debug( "Deleting starsystem " + system.systemname );
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                            }
                        }
                    }
                    catch ( SQLiteException ex )
                    {
                        handleSqlLiteException( con, ex );
                    }
                }
            }
        }

        private void CreateOrUpdateDatabase( SQLiteConnection connection = null )
        {
            lock ( nameof(SimpleDbConnection) ) // Lock before writing to the database
            {
                using ( var con = connection ?? SimpleDbConnection() )
                {
                    try
                    {
                        con.Open();

                        using ( var cmd = new SQLiteCommand( CREATE_TABLE_SQL, con ) )
                        {
                            Logging.Debug( "Preparing starsystem repository" );
                            cmd.ExecuteNonQuery();
                        }

                        // Get schema version 
                        using ( var cmd = new SQLiteCommand( TABLE_GET_SCHEMA_VERSION_SQL, con ) )
                        {
                            SCHEMA_VERSION = (long)cmd.ExecuteScalar();
                            Logging.Debug( "Starsystem repository is schema version " + SCHEMA_VERSION );
                        }

                        // Apply any necessary updates
                        if ( SCHEMA_VERSION < 1 )
                        {
                            Logging.Debug( "Updating starsystem repository to schema version 1" );
                            AddColumnIfMissing( con, "comment" );
                            SCHEMA_VERSION = 1;
                        }

                        if ( SCHEMA_VERSION < 2 )
                        {
                            Logging.Debug( "Updating starsystem repository to schema version 2" );

                            // Allocate our new columns
                            AddColumnIfMissing( con, "systemaddress" );

                            // We have to replace our table with a new copy to assign our new columns as unique
                            using ( var cmd = new SQLiteCommand( REPLACE_TABLE_SQL, con ) )
                            {
                                cmd.ExecuteNonQuery();
                            }

                            SCHEMA_VERSION = 2;
                        }

                        if ( SCHEMA_VERSION < 3 )
                        {
                            Logging.Debug( "Updating starsystem repository to schema version 3" );

                            // We will recreate our table without the "edsmid" column as we won't be indexing based on this value nor using it to evaluate uniqueness
                            // We have to replace our table with a new copy to reassign unique columns
                            using ( var cmd = new SQLiteCommand( REPLACE_TABLE_SQL, con ) )
                            {
                                cmd.ExecuteNonQuery();
                            }

                            SCHEMA_VERSION = 3;
                        }

                        // Add our indices
                        using ( var cmd = new SQLiteCommand( CREATE_INDEX_SQL, con ) )
                        {
                            Logging.Debug( "Creating starsystem index" );
                            cmd.ExecuteNonQuery();
                        }

                        // Set schema version 
                        using ( var cmd = new SQLiteCommand(
                                   TABLE_SET_SCHEMA_VERSION_SQL + SCHEMA_VERSION + ";", con ) )
                        {
                            Logging.Info( "Starsystem repository schema is version " + SCHEMA_VERSION );
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch ( SQLiteException ex )
                    {
                        handleSqlLiteException( con, ex );
                    }
                }
            }
            Logging.Debug("Starsystem repository ready.");
        }

        /// <summary> Valid columnNames are "systemaddress" and "comment" </summary>
        private static void AddColumnIfMissing(SQLiteConnection con, string columnName)
        {
            // Parameters like `DISTINCT` cannot be set on columns by this method
            string command = string.Empty;
            switch (columnName)
            {
                case "systemaddress":
                    command = @"ALTER TABLE starsystems ADD COLUMN systemaddress INT";
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

        private static void handleSqlLiteException(SQLiteConnection con, SQLiteException ex)
        {
            Logging.Warn("SQLite error: ", ex.ToString());

            try
            {
                con.BeginTransaction()?.Rollback();
            }
            catch (SQLiteException ex2)
            {
                Logging.Warn("SQLite transaction rollback failed.");
                Logging.Warn("SQLite error: ", ex2.ToString());
            }
            finally
            {
                con.Dispose();
            }
        }
    }
}
