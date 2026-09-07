using EddiDataProviderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class SqLiteBaseRepositoryTests
    {
        // Selecting production storage is tested without opening the user's database.
        private sealed class Repository ( bool unitTesting ) : SqLiteBaseRepository( unitTesting )
        {
            internal string DatabasePath => DbFile;
        }

        [TestMethod]
        public void CreatingRepositoriesDoesNotResetAnOpenTestDatabaseOrChangeStorage ()
        {
            var testRepository = new Repository( true );
            using var connection = testRepository.SimpleDbConnection();
            connection.Open();
            var table = $"repository_lifetime_{Guid.NewGuid():N}";
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE TABLE {table} (value INTEGER); INSERT INTO {table} VALUES (42);";
            command.ExecuteNonQuery();
            try
            {
                var productionRepository = new Repository( false );
                var secondTestRepository = new Repository( true );
                Assert.AreEqual( Path.Combine( Constants.DATA_DIR, "EDDI.sqlite" ), productionRepository.DatabasePath );
                Assert.AreNotEqual( productionRepository.DatabasePath, testRepository.DatabasePath );
                Assert.AreEqual( testRepository.DatabasePath, secondTestRepository.DatabasePath );

                using var secondConnection = secondTestRepository.SimpleDbConnection();
                secondConnection.Open();
                using var read = secondConnection.CreateCommand();
                read.CommandText = $"SELECT value FROM {table};";
                Assert.AreEqual( 42L, read.ExecuteScalar() );
                using var originalConnection = testRepository.SimpleDbConnection();
                Assert.AreEqual( secondConnection.ConnectionString, originalConnection.ConnectionString );
            }
            finally
            {
                command.CommandText = $"DROP TABLE {table};";
                command.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void ConcurrentRepositoryConstructionKeepsEachStorageSelection ()
        {
            var testPath = new Repository( true ).DatabasePath;
            var productionPath = new Repository( false ).DatabasePath;
            Parallel.For( 0, 100, index =>
            {
                var isTest = index % 2 == 0;
                var repository = new Repository( isTest );
                Assert.AreEqual( isTest ? testPath : productionPath, repository.DatabasePath );
            } );
        }
    }
}
