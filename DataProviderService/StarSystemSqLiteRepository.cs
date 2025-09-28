using EddiDataDefinitions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private const string SELECT_NAME_SQL = @"SELECT name FROM starsystems ";
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
        private const string WHERE_SYSTEMADDRESS = @"WHERE systemaddress = @systemaddress;";
        private const string WHERE_NAME = @"WHERE name = @name;";
        private const string WHERE_NAME_STARTSWITH = @"WHERE name LIKE @name;";

        private StarSystemSqLiteRepository ( bool unitTesting = false )
        {
            SqLiteBaseRepository.unitTesting = unitTesting;
        }

        public static async Task<StarSystemSqLiteRepository> CreateAsync ( bool isUnitTesting = false )
        {
            var repository = new StarSystemSqLiteRepository(isUnitTesting);
            await repository.CreateOrUpdateDatabaseAsync().ConfigureAwait(false);
            return repository;
        }

        public async Task<DatabaseStarSystem> GetSqlStarSystemAsync ( ulong systemAddress, CancellationToken cancellationToken )
        {
            if ( systemAddress <= 0 ) { return null; }

            return ( await GetSqlStarSystemsAsync( new[] { systemAddress }, cancellationToken ).ConfigureAwait(false) )?.FirstOrDefault();
        }

        public async Task<List<DatabaseStarSystem>> GetSqlStarSystemsAsync ( ulong[] systemAddresses, CancellationToken cancellationToken )
        {
            var results = new List<DatabaseStarSystem>();
            if ( !File.Exists( DbFile ) ) { return results; }
            if ( !systemAddresses.Any() ) { return results; }
            results = await ReadStarSystemsAsync( systemAddresses, cancellationToken ).ConfigureAwait(false);
            FixLegacyDbStarSystemData(results);
            return results;
        }

        public async Task<List<DatabaseStarSystem>> GetSqlStarSystemsAsync ( string[] systemNames, CancellationToken cancellationToken )
        {
            var results = new List<DatabaseStarSystem>();
            if ( !File.Exists( DbFile ) ) { return results; }
            if ( !systemNames.Any() ) { return results; }
            results = await ReadStarSystemsAsync( systemNames, cancellationToken ).ConfigureAwait(false);
            FixLegacyDbStarSystemData( results );
            return results;
        }

        private static void FixLegacyDbStarSystemData ( List<DatabaseStarSystem> results )
        {
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
        }

        [ NotNull, ItemNotNull ]
        public async Task<List<string>> GetStarSystemNamesAsync ( string startingWith, CancellationToken cancellationToken )
        {
            if ( string.IsNullOrWhiteSpace( startingWith ) )
            {
                throw new ArgumentException( @"Input string cannot be null or empty.", nameof(startingWith) );
            }

            var results = new HashSet<string>();
            try
            {
                using ( var con = SimpleDbConnection() )
                {
                    await con.OpenAsync( cancellationToken ).ConfigureAwait( false );
                    using ( var transaction = con.BeginTransaction() )
                    {
                        using ( var cmd = con.CreateCommand() )
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText = SELECT_NAME_SQL + WHERE_NAME_STARTSWITH;
                            cmd.Parameters.AddWithValue( "@name", $"{startingWith}%" );
                            using ( var rdr = await cmd.ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false ) )
                            {
                                while ( await rdr.ReadAsync( cancellationToken ).ConfigureAwait(false) )
                                {
                                    var name = rdr["name"] as string;
                                    if ( !string.IsNullOrEmpty( name ) )
                                    {
                                        results.Add( name );
                                    }
                                }
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch ( SQLiteException sqle )
            {
                Logging.Warn( $"An error occurred while fetching star system names starting with '{startingWith}': {sqle.Message}" );
            }

            return results.ToList();
        }

        [ NotNull, ItemNotNull ]
        private async Task<List<DatabaseStarSystem>> ReadStarSystemsAsync ( ulong[] systemAddresses, CancellationToken cancellationToken )
        {
            var results = new List<DatabaseStarSystem>();
            if ( !systemAddresses.Any() ) { return results; }

            using ( var con = SimpleDbConnection() )
            {
                await con.OpenAsync( cancellationToken ).ConfigureAwait( false );
                using ( var cmd = new SQLiteCommand( con ) )
                {
                    using ( var transaction = con.BeginTransaction() )
                    {
                        foreach ( var systemAddress in systemAddresses.Where( systemAddress => systemAddress > 0 ) )
                        {
                            try
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue( "@systemaddress", systemAddress );
                                cmd.CommandText = SELECT_SQL + WHERE_SYSTEMADDRESS;
                                var result = await ReadStarSystemEntryAsync( cmd, cancellationToken ).ConfigureAwait( false );
                                if ( result != null )
                                {
                                    results.Add( result );
                                }
                            }
                            catch ( InvalidOperationException ioe )
                            {
                                Logging.Warn( $"Problem reading data for star system '{systemAddress}' from database.", ioe );
                            }
                            catch ( SQLiteException sqle )
                            {
                                Logging.Warn( $"Problem reading data for star system '{systemAddress}' from database.", sqle );
                            }
                        }

                        transaction.Commit();
                    }
                }
            }

            return results.RemoveNulls().ToList();
        }

        [NotNull, ItemNotNull]
        private async Task<List<DatabaseStarSystem>> ReadStarSystemsAsync ( string[] systemNames, CancellationToken cancellationToken )
        {
            var results = new List<DatabaseStarSystem>();
            if ( !systemNames.Any() ) { return results; }

            using ( var con = SimpleDbConnection() )
            {
                await con.OpenAsync( cancellationToken ).ConfigureAwait(false);
                using ( var cmd = new SQLiteCommand( con ) )
                {
                    using ( var transaction = con.BeginTransaction() )
                    {
                        foreach ( var systemName in systemNames.Where( sName => !string.IsNullOrEmpty(sName) ) )
                        {
                            try
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue( "@name", systemName );
                                cmd.CommandText = SELECT_SQL + WHERE_NAME;
                                var result = await ReadStarSystemEntryAsync( cmd, cancellationToken ).ConfigureAwait(false);
                                if ( result != null )
                                {
                                    results.Add( result );
                                }
                            }
                            catch (InvalidOperationException ioe )
                            {
                                Logging.Warn( $"Problem reading data for star system '{systemName}' from database.", ioe );
                            }
                            catch ( SQLiteException sqle )
                            {
                                Logging.Warn( $"Problem reading data for star system '{systemName}' from database.", sqle );
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            return results.RemoveNulls().ToList();
        }

        private async Task<List<DatabaseStarSystem>> ReadStarSystemsAsync( IList<StarSystem> starSystems, CancellationToken cancellationToken )
        {
            var results = new List<DatabaseStarSystem>();
            if ( starSystems is null || !starSystems.Any()) { return results; }
            
            var systemAddresses = starSystems.Select( s => s.systemAddress ).Distinct().ToArray();
            return await ReadStarSystemsAsync( systemAddresses, cancellationToken ).ConfigureAwait(false);
        }

        private async Task<DatabaseStarSystem> ReadStarSystemEntryAsync ( SQLiteCommand cmd, CancellationToken cancellationToken )
        {
            ulong? systemAddress = null;
            var systemName = string.Empty;
            var starSystemJson = string.Empty;
            var comment = string.Empty;
            var lastUpdated = DateTime.MinValue;
            DateTime? lastVisit = null;
            var totalVisits = 0;

            var fieldMappings = new Dictionary<string, Action<DbDataReader, int>>
            {
                { "systemaddress", (rdr, i) => systemAddress = rdr.IsDBNull(i) 
                    ? null 
                    : (ulong?)rdr.GetInt64(i) },
                { "name", (rdr, i) => systemName = rdr.IsDBNull(i) 
                    ? string.Empty 
                    : rdr.GetString(i) },
                { "starsystem", (rdr, i) => starSystemJson = rdr.IsDBNull(i) 
                    ? string.Empty 
                    : rdr.GetString(i) },
                { "comment", (rdr, i) => comment = rdr.IsDBNull(i) 
                    ? string.Empty 
                    : rdr.GetString(i) },
                { "starsystemlastupdated", (rdr, i) => lastUpdated = rdr.IsDBNull(i) 
                    ? DateTime.MinValue 
                    : rdr.GetDateTime(i).ToUniversalTime() },
                { "lastvisit", (rdr, i) => lastVisit = rdr.IsDBNull(i) 
                    ? null 
                    : (DateTime?)rdr.GetDateTime(i).ToUniversalTime() },
                { "totalvisits", (rdr, i) => totalVisits = rdr.IsDBNull(i) 
                    ? 0 
                    : rdr.GetInt32(i) }
            };

            using ( var rdr = await cmd.ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false ) )
            {
                if ( await rdr.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
                {
                    for ( var i = 0; i < rdr.FieldCount; i++ )
                    {
                        if ( fieldMappings.TryGetValue( rdr.GetName( i ), out var mapAction ) )
                        {
                            mapAction( rdr, i );
                        }
                    }
                }
            }

            if ( SCHEMA_VERSION >= 2 && systemAddress is null )
            {
                throw new InvalidOperationException( "System address cannot be null for schema version 2 or higher." );
            }

            return new DatabaseStarSystem( systemName, systemAddress ?? 0, starSystemJson )
            {
                comment = comment,
                lastUpdated = lastUpdated,
                lastVisit = lastVisit,
                totalVisits = totalVisits
            };
        }

        public async Task SaveStarSystemAsync( StarSystem starSystem, CancellationToken cancellationToken )
        {
            if (starSystem == null) { return; }
            await SaveStarSystemsAsync( new List<StarSystem> { starSystem }, cancellationToken ).ConfigureAwait(false);
        }

        public async Task SaveStarSystemsAsync( IList<StarSystem> starSystems, CancellationToken cancellationToken )
        {
            // Determine whether we need to delete, insert, or update each system
            var delete = new List<StarSystem>();
            var update = new List<StarSystem>();
            var insert = new List<StarSystem>();

            var dbSystems = await ReadStarSystemsAsync( starSystems, cancellationToken ).ConfigureAwait(false);

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
            await deleteStarSystemsAsync(delete.ToImmutableList()).ConfigureAwait(false);

            // Insert applicable systems
            await insertStarSystemsAsync(insert.ToImmutableList() ).ConfigureAwait(false);

            // Update applicable systems
            await updateStarSystemsAsync(update.ToImmutableList() ).ConfigureAwait(false);
        }

        private async Task insertStarSystemsAsync(ImmutableList<StarSystem> systems)
        {
            if ( systems.Count == 0)
            {
                return;
            }

            using ( var con = SimpleDbConnection() )
            {
                await con.OpenAsync().ConfigureAwait( false );
                using ( var cmd = new SQLiteCommand( con ) )
                {
                    using ( var transaction = con.BeginTransaction() )
                    {
                        try
                        {
                            foreach ( var system in systems )
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandText = INSERT_SQL + VALUES_SQL;
                                cmd.Parameters.AddWithValue( "@name", system.systemname );
                                cmd.Parameters.AddWithValue( "@systemaddress", system.systemAddress );
                                cmd.Parameters.AddWithValue( "@totalvisits", system.visits );
                                cmd.Parameters.AddWithValue( "@lastvisit", system.lastvisit ?? DateTime.UtcNow );
                                cmd.Parameters.AddWithValue( "@starsystem", JsonConvert.SerializeObject( system ) );
                                cmd.Parameters.AddWithValue( "@starsystemlastupdated", system.lastupdated );
                                cmd.Parameters.AddWithValue( "@comment", system.comment );
                                Logging.Debug( "Inserting new starsystem " + system.systemAddress, system );
                                await cmd.ExecuteNonQueryAsync().ConfigureAwait( false );
                            }

                            transaction.Commit();
                        }
                        catch ( SQLiteException e )
                        {
                            LogAndRollbackSqlLiteException( transaction, e );
                        }
                    }
                }
            }
        }

        private async Task updateStarSystemsAsync ( IImmutableList<StarSystem> systems )
        {
            if ( systems.Count == 0 )
            {
                return;
            }

            using ( var con = SimpleDbConnection() )
            {
                await con.OpenAsync().ConfigureAwait( false );
                using ( var cmd = new SQLiteCommand( con ) )
                {
                    using ( var transaction = con.BeginTransaction() )
                    {
                        try
                        {
                            foreach ( var system in systems )
                            {
                                var serializedSystem = JsonConvert.SerializeObject( system );

                                if ( system.systemAddress != 0 )
                                {
                                    cmd.CommandText = UPDATE_SQL + WHERE_SYSTEMADDRESS;
                                }
                                else
                                {
                                    cmd.CommandText = UPDATE_SQL + WHERE_NAME;
                                }

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue( "@name", system.systemname );
                                cmd.Parameters.AddWithValue( "@totalvisits", system.visits );
                                cmd.Parameters.AddWithValue( "@lastvisit", system.lastvisit ?? DateTime.UtcNow );
                                cmd.Parameters.AddWithValue( "@starsystem", serializedSystem );
                                cmd.Parameters.AddWithValue( "@starsystemlastupdated", system.lastupdated );
                                cmd.Parameters.AddWithValue( "@comment", system.comment );
                                cmd.Parameters.AddWithValue( "@systemaddress", system.systemAddress );
                                Logging.Debug( "Updating starsystem " + system.systemAddress, system );
                                await cmd.ExecuteNonQueryAsync().ConfigureAwait( false );
                            }

                            transaction.Commit();
                        }
                        catch ( SQLiteException ex )
                        {
                            LogAndRollbackSqlLiteException( transaction, ex );
                        }
                    }
                }
            }
        }

        private async Task deleteStarSystemsAsync ( ImmutableList<StarSystem> systems )
        {
            if ( systems.Count == 0 )
            {
                return;
            }

            using ( var con = SimpleDbConnection() )
            {
                await con.OpenAsync().ConfigureAwait( false );
                using ( var cmd = new SQLiteCommand( con ) )
                {
                    using ( var transaction = con.BeginTransaction() )
                    {
                        try
                        {
                            foreach ( var system in systems )
                            {
                                // Delete all possible variations of this data from the database.
                                if ( system.systemAddress != 0 )
                                {
                                    cmd.CommandText = DELETE_SQL + WHERE_SYSTEMADDRESS;
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue( "@systemaddress", system.systemAddress );
                                    Logging.Debug( "Deleting starsystem " + system.systemAddress );
                                    await cmd.ExecuteNonQueryAsync().ConfigureAwait( false );
                                }
                                else if ( !string.IsNullOrEmpty( system.systemname ) )
                                {
                                    cmd.CommandText = DELETE_SQL + WHERE_NAME;
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue( "@name", system.systemname );
                                    Logging.Debug( "Deleting starsystem " + system.systemname );
                                    await cmd.ExecuteNonQueryAsync().ConfigureAwait( false );
                                }
                            }

                            transaction.Commit();
                        }
                        catch ( SQLiteException ex )
                        {
                            LogAndRollbackSqlLiteException( transaction, ex );
                        }
                    }
                }
            }
        }

        private async Task CreateOrUpdateDatabaseAsync()
        {
            using ( var con = SimpleDbConnection() )
            {
                try
                {
                    con.Open();

                    using ( var cmd = new SQLiteCommand( CREATE_TABLE_SQL, con ) )
                    {
                        Logging.Debug( "Preparing starsystem repository" );
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    // Get schema version 
                    using ( var cmd = new SQLiteCommand( TABLE_GET_SCHEMA_VERSION_SQL, con ) )
                    {
                        SCHEMA_VERSION = (long)( cmd.ExecuteScalar() ?? 0 );
                        Logging.Debug( "Starsystem repository is schema version " + SCHEMA_VERSION );
                    }

                    // Apply any necessary updates
                    if ( SCHEMA_VERSION < 1 )
                    {
                        Logging.Debug( "Updating starsystem repository to schema version 1" );
                        await AddColumnIfMissingAsync( con, "comment" ).ConfigureAwait(false);
                        SCHEMA_VERSION = 1;
                    }

                    if ( SCHEMA_VERSION < 2 )
                    {
                        Logging.Debug( "Updating starsystem repository to schema version 2" );

                        // Allocate our new columns
                        await AddColumnIfMissingAsync( con, "systemaddress" ).ConfigureAwait(false);

                        // We have to replace our table with a new copy to assign our new columns as unique
                        using ( var cmd = new SQLiteCommand( REPLACE_TABLE_SQL, con ) )
                        {
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }

                        SCHEMA_VERSION = 3;
                    }

                    // Add our indices (if they don't already exist)
                    using ( var cmd = new SQLiteCommand( CREATE_INDEX_SQL, con ) )
                    {
                        Logging.Debug( "Creating starsystem index" );
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    // Optimize the database
                    using ( var cmd = new SQLiteCommand( "PRAGMA optimize;", con ) )
                    {
                        Logging.Debug( "Creating starsystem index" );
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    // Set schema version 
                    using ( var cmd = new SQLiteCommand( TABLE_SET_SCHEMA_VERSION_SQL + SCHEMA_VERSION + ";", con ) )
                    {
                        Logging.Info( "Starsystem repository schema is version " + SCHEMA_VERSION );
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
                catch ( SQLiteException ex )
                {
                    Logging.Warn( "SQLite error: ", ex.ToString() );
                }
            }

            Logging.Debug("Starsystem repository ready.");
        }

        /// <summary> Valid columnNames are "systemaddress" and "comment" </summary>
        private async Task AddColumnIfMissingAsync(SQLiteConnection con, string columnName )
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

            if ( !string.IsNullOrEmpty( command ) )
            {
                bool columnExists = false;
                using ( var cmd = new SQLiteCommand( TABLE_INFO_SQL, con ) )
                {
                    using ( var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false) )
                    {
                        while ( await rdr.ReadAsync().ConfigureAwait(false) )
                        {
                            if ( columnName == rdr.GetString( 1 ) )
                            {
                                columnExists = true;
                                break;
                            }
                        }
                    }
                }

                if ( !columnExists )
                {
                    Logging.Debug( "Updating starsystem repository with new column " + columnName );
                    try
                    {
                        using ( var cmd = new SQLiteCommand( command, con ) )
                        {
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                    catch ( SQLiteException ex )
                    {
                        Logging.Warn( "SQLite error: ", ex.ToString() );
                    }
                }
            }
        }

        private void LogAndRollbackSqlLiteException ( SQLiteTransaction transaction, SQLiteException ex )
        {
            Logging.Warn( "SQLite error: ", ex.ToString() );

            try
            {
                transaction?.Rollback();
            }
            catch ( SQLiteException ex2 )
            {
                Logging.Warn( "SQLite transaction rollback failed." );
                Logging.Warn( "SQLite error: ", ex2.ToString() );
            }
        }
    }
}
