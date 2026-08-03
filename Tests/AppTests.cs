using Eddi;
using EddiConfigService.Configurations;
using EddiSpeechResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class AppTests : TestBase
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init ()
        {
            // Make test environment safe (disable telemetry, unit testing flags)
            MakeSafe();

            // Ensure log directory exists so Logging won't fail when tests inspect files
            Directory.CreateDirectory( Constants.DATA_DIR );
        }

        [TestCleanup]
        public void Cleanup ()
        {
            // Try to clean up any mutex left behind by tests
            try
            {
                App.eddiMutex?.ReleaseMutex();
            }
            catch
            {
                // ignore if current thread doesn't own it or already released
            }
            finally
            {
                App.eddiMutex?.Dispose();
                App.eddiMutex = null;
            }
        }

        [TestMethod]
        public void AlreadyRunning_ReturnsTrueWhenExternalMutexHeld ()
        {
            // Ensure starting state - dispose any existing App mutex
            App.eddiMutex?.Dispose();
            App.eddiMutex = null;

            Mutex external = null;
            try
            {
                // Create an external (simulated other process) mutex and take ownership
                var mutexName = $"{Constants.EDDI_SYSTEM_MUTEX_NAME}-{Guid.NewGuid():N}";
                external = new Mutex( true, mutexName, out var externalOwner );
                Assert.IsTrue( externalOwner, "Test setup failed to obtain external mutex ownership" );

                // Now call AlreadyRunning which will create its own mutex; it should detect an existing owner
                var already = App.AlreadyRunning( mutexName );
                Assert.IsTrue( already, "AlreadyRunning should return true when an external mutex exists" );
            }
            finally
            {
                // Release & dispose external mutex
                try
                { external?.ReleaseMutex(); }
                catch
                {
                    // ignored
                }

                external?.Dispose();

                // Dispose the mutex created by AlreadyRunning (may not be owned by this thread)
                App.eddiMutex?.Dispose();
                App.eddiMutex = null;
            }
        }

        [TestMethod, DoNotParallelize]
        public void ApplyAnyOverrideCulture_AppliesValidCulture ()
        {
            // Start with a known default so we can observe the fallback behavior
            var backupDefault = CultureInfo.DefaultThreadCurrentCulture;

            // Apply override
            App.ApplyAnyOverrideCulture( new EDDIConfiguration { OverrideCulture = "fr-FR" } );

            // Default thread cultures and current thread culture should reflect override
            Assert.IsNotNull( CultureInfo.DefaultThreadCurrentCulture );
            Assert.AreEqual( "fr-FR", CultureInfo.DefaultThreadCurrentCulture.Name, "DefaultThreadCurrentCulture should be set to 'fr-FR'" );
            Assert.AreEqual( "fr-FR", Thread.CurrentThread.CurrentCulture.Name, "Current thread culture should be set to 'fr-FR'" );

            // Restore original default to avoid affecting other tests
            CultureInfo.DefaultThreadCurrentCulture = backupDefault;
            CultureInfo.DefaultThreadCurrentUICulture = backupDefault;
        }

        [TestMethod, DoNotParallelize]
        public void ApplyAnyOverrideCulture_InvalidCultureDoesNotThrow_AndSetsDefaultToNull ()
        {
            // Start with a known default so we can observe the fallback behavior
            var backupDefault = CultureInfo.DefaultThreadCurrentCulture;

            // Should not throw
            App.ApplyAnyOverrideCulture( new EDDIConfiguration { OverrideCulture = "this-is-not-a-culture" } );

            // Per implementation, invalid culture triggers ApplyCulture(null) -> DefaultThreadCurrentCulture becomes null
            Assert.IsNull( CultureInfo.DefaultThreadCurrentCulture, "DefaultThreadCurrentCulture should have been set to null when invalid override culture is provided" );

            // Restore original default to avoid affecting other tests
            CultureInfo.DefaultThreadCurrentCulture = backupDefault;
            CultureInfo.DefaultThreadCurrentUICulture = backupDefault;
        }

        [TestMethod]
        public void CrashLogger_SuppressesRollbarInternalHttpExceptions ()
        {
            // Build an HttpRequestException with a stack trace containing "Rollbar" by throwing from a helper method
            HttpRequestException httpEx = null;
            try
            {
                ThrowHttpRequestExceptionWithRollbarInStack();
            }
            catch ( HttpRequestException hre )
            {
                httpEx = hre;
            }
            Assert.IsNotNull( httpEx );

            // Wrap in AggregateException as the CrashLogger expects
            var agg = new AggregateException(httpEx);

            // Invoke private static CrashLogger via reflection and ensure it doesn't throw
            var crashLogger = typeof(App).GetMethod("CrashLogger", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull( crashLogger, "CrashLogger method not found via reflection" );

            // Should not throw
            crashLogger.Invoke( null, [ agg ] );
        }

        [TestMethod, DoNotParallelize]
        public async Task CrashLogger_LogsUnhandledExceptionAsync ()
        {
            // Ensure we have a fresh log file to inspect
            var logFile = Path.Combine(Constants.DATA_DIR, "eddi.log");
            if ( File.Exists( logFile ) )
            {
                File.Delete( logFile );
            }

            var crashLogger = typeof(App).GetMethod("CrashLogger", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull( crashLogger );

            var ex = new Exception("TestCrashLogger");

            // Invoke CrashLogger - it will call Logging.Error which writes asynchronously
            crashLogger.Invoke( null, [ ex ] );

            // Wait for the background logging task to complete and write to file (polling with timeout)
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var found = false;
            while ( sw.Elapsed < TimeSpan.FromSeconds( 5 ) )
            {
                if ( File.Exists( logFile ) )
                {
                    var contents = await File.ReadAllTextAsync(logFile, TestContext.CancellationToken);
                    if ( contents.Contains( "Unhandled exception: TestCrashLogger." ) )
                    {
                        found = true;
                        break;
                    }
                }

                await Task.Delay( 100, TestContext.CancellationToken ).ConfigureAwait( false );
            }

            Assert.IsTrue( found, "CrashLogger did not write the expected message to the log file within the timeout" );
        }

        [TestMethod, DoNotParallelize]
        [DataRow( "fr-FR", "", "eddi.fr.json" )] // Automatic culture should use the system culture default personality
        [DataRow( "fr-FR", "en", "eddi.json" )] // Override from French to English should use the English default personality
        [DataRow( "en", "fr-FR", "eddi.fr.json" )] // Override from English to French should use the French default personality
        public void PersonalityDefault_UsesOverrideCulture_NotSystemCulture ( string threadCulture, string overrideCulture, string expectedDefaultPersonalityFile )
        {
            var originalCurrentCulture = Thread.CurrentThread.CurrentCulture;
            var originalCurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
            var originalDefaultUICulture = CultureInfo.DefaultThreadCurrentUICulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo( threadCulture );
                Thread.CurrentThread.CurrentUICulture = new CultureInfo( threadCulture );
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( threadCulture );
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( threadCulture );

                App.ApplyAnyOverrideCulture( new EDDIConfiguration { OverrideCulture = overrideCulture } );
                Personality.ResetDefault();

                var personality = Personality.Default();

                Assert.IsNotNull( personality );
                Assert.EndsWith( expectedDefaultPersonalityFile, personality.dataPath );
            }
            finally
            {
                Personality.ResetDefault();

                Thread.CurrentThread.CurrentCulture = originalCurrentCulture;
                Thread.CurrentThread.CurrentUICulture = originalCurrentUICulture;
                CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
                CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUICulture;
            }
        }

        // Helper used to create an HttpRequestException whose stack trace contains the substring "Rollbar"
        private static void ThrowHttpRequestExceptionWithRollbarInStack ()
        {
            // Method name intentionally contains "Rollbar" so that the resulting stack trace includes that token.
            RollbarMarker();
        }

        private static void RollbarMarker ()
        {
            // Throw an HttpRequestException so its stack trace contains "RollbarMarker" (and thus "Rollbar")
            throw new HttpRequestException( "simulated http exception" );
        }
    }
}
