using System.Data.SQLite;
using System;
using System.IO;
using Utilities;

namespace EddiDataProviderService
{
    public class SqLiteBaseRepository
    {
        // Share test storage within a process, never between runners. Constructing
        // a repository must not reset a database that another repository is using.
        private static readonly Lazy<string> TestDbFile = new( () =>
        {
            var path = Path.Combine( Path.GetTempPath(), $"EDDI_TEST_{Guid.NewGuid():N}.sqlite" );
            AppDomain.CurrentDomain.ProcessExit += ( _, _ ) =>
            {
                try { File.Delete( path ); }
                catch ( IOException ) { }
                catch ( UnauthorizedAccessException ) { }
            };
            return path;
        } );

        protected SqLiteBaseRepository ( bool unitTesting = false )
        {
            DbFile = unitTesting ? TestDbFile.Value : Path.Combine( Constants.DATA_DIR, "EDDI.sqlite" );
        }

        protected string DbFile { get; }

        public SQLiteConnection SimpleDbConnection()
        {
            return new SQLiteConnection( new SQLiteConnectionStringBuilder { DataSource = DbFile }.ConnectionString );
        }
    }
}
