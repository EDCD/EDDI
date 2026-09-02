using EddiCore.Upgrader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public sealed class EddiUpgraderTests : TestBase
    {
        private readonly List<string> _spokenMessages = [];

        private static JObject CreateRelease ( string version ) => JObject.Parse(
    $$"""
            {
              "tag_name": "Release/{{version}}",
              "prerelease": false,
              "assets": [
                {
                  "name": "EDDI-{{version}}.exe",
                  "content_type": "application/x-msdownload",
                  "browser_download_url": "https://example.invalid/EDDI-{{version}}.exe"
                }
              ]
            }
            """ );

        private static void InvokeProcessRelease ( JObject release )
        {
            var method = typeof( EddiUpgrader ).GetMethod(
                "ProcessRelease",
                BindingFlags.NonPublic | BindingFlags.Static );

            Assert.IsNotNull( method );
            method.Invoke( null, [ release ] );
        }

        private static void ResetUpgradeState ()
        {
            SetPrivateStaticField( "UpgradeLocation", null );
            SetPrivateStaticProperty( "UpgradeVersion", null );
        }

        private static void SetPrivateStaticField ( string name, object value )
        {
            var field = typeof( EddiUpgrader ).GetField(
                name,
                BindingFlags.NonPublic | BindingFlags.Static );

            Assert.IsNotNull( field );
            field.SetValue( null, value );
        }

        private static void SetPrivateStaticProperty ( string name, object value )
        {
            var property = typeof( EddiUpgrader ).GetProperty(
                name,
                BindingFlags.Public | BindingFlags.Static );

            Assert.IsNotNull( property );
            property.SetValue( null, value );
        }

        [TestInitialize]
        public void Start ()
        {
            MakeSafe();
            ResetUpgradeState();
            _spokenMessages.Clear();
            EddiUpgrader.SaySystemMessageAsync = message =>
            {
                _spokenMessages.Add( message );
                return Task.CompletedTask;
            };
        }

        [TestCleanup]
        public void Stop ()
        {
            EddiUpgrader.ResetTestHooks();
            ResetUpgradeState();
        }

        [TestMethod]
        public void ProcessRelease_SetsUpgradeStateWithoutSpeakingBeforeSpeechIsInitialized ()
        {
            InvokeProcessRelease( CreateRelease( "999.999.999" ) );

            Assert.IsTrue( EddiUpgrader.UpgradeAvailable );
            Assert.AreEqual( "999.999.999", EddiUpgrader.UpgradeVersion );
            Assert.HasCount( 0, _spokenMessages,
                "The update check runs before standalone startup has initialized text-to-speech voices, so it must not enqueue speech immediately." );
        }

        [TestMethod]
        public async Task AnnounceUpgradeAvailableAsync_SpeaksDetectedUpgradeVersion ()
        {
            InvokeProcessRelease( CreateRelease( "999.999.999" ) );

            await EddiUpgrader.AnnounceUpgradeIfAvailableAsync().ConfigureAwait( false );

            CollectionAssert.AreEqual(
                (string[]) [ "Eddi version 999 point 999 point 999 is now available." ],
                _spokenMessages );
        }

        [TestMethod]
        public async Task AnnounceUpgradeAvailableAsync_DoesNothingWhenNoUpgradeIsAvailable ()
        {
            await EddiUpgrader.AnnounceUpgradeIfAvailableAsync().ConfigureAwait( false );

            Assert.HasCount( 0, _spokenMessages );
        }
    }
}
