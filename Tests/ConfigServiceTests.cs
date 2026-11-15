using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class ConfigServiceTests
    {
        private static CommanderConfiguration DeserializeCommander (
            string json, List<Exception> errors )
        {
            // Capture errors per call rather than writing to the real profile's logger.
            var method = typeof( ConfigService ).GetMethod( nameof( ConfigService.FromJson ),
                BindingFlags.NonPublic | BindingFlags.Static );
            Assert.IsNotNull( method );
            return (CommanderConfiguration)method.MakeGenericMethod( typeof( CommanderConfiguration ) )
                .Invoke( null, [ json, new Action<Exception>( errors.Add ) ] );
        }

        [TestMethod]
        public void FromJson_NullOrInvalid_ReturnsDefault ()
        {
            // Null JSON should return a new instance (not null) with defaults applied
            var cfgNull = ConfigService.FromJson<CommanderConfiguration>(null);
            Assert.IsNotNull( cfgNull );
            Assert.AreEqual( "Male", cfgNull.gender );

            // Invalid JSON should be handled and return a default instance
            var errors = new List<Exception>();
            var cfgInvalid = DeserializeCommander( "not a json", errors );
            Assert.IsNotNull( cfgInvalid );
            Assert.AreEqual( "Male", cfgInvalid.gender );
            Assert.HasCount( 1, errors );
            Assert.IsInstanceOfType<JsonReaderException>( errors[ 0 ] );
        }

        [TestMethod]
        public void FromJson_ValidConfiguration_PreservesValuesWithoutReportingError ()
        {
            var errors = new List<Exception>();

            var configuration = DeserializeCommander(
                "{\"gender\":\"Female\",\"commanderName\":\"Test commander\"}", errors );

            Assert.AreEqual( "Female", configuration.gender );
            Assert.AreEqual( "Test commander", configuration.commanderName );
            Assert.IsEmpty( errors );
        }

        [TestMethod]
        public void ConvertLegacyConfigData_MigratesCommanderAndFleetCarrier ()
        {
            // Prepare configs to pass to the private migration method
            var commanderConfig = new CommanderConfiguration();
            var eddiConfig = new EDDIConfiguration();
            var fleetCarrierConfig = new FleetCarrierConfiguration();

            // Build legacy additional data to simulate previously-embedded values
            var legacyData = new Dictionary<string, JToken>()
            {
                ["CommanderName"] = JToken.FromObject("LegacyCmdr"),
                ["Gender"] = JToken.FromObject("Female"),
                ["homeSystemAddress"] = JToken.FromObject((ulong)123456789),
                ["homeStationMarketID"] = JToken.FromObject((long)987654321),
                ["PhoneticName"] = JToken.FromObject("Phonetic"),
                ["powerMerits"] = JToken.FromObject(42),
                ["squadronName"] = JToken.FromObject("MySquad"),
                ["squadronID"] = JToken.FromObject("SQID"),
                // fleetCarrier stored as a nested object in legacy data
                ["fleetCarrier"] = JToken.FromObject(new FleetCarrier(123456789, StationModel.FleetCarrier) { name = "CarrierName" })
            };

            // Set the non-public field '_additionalData' on the EDDIConfiguration instance via reflection
            var configBaseType = eddiConfig.GetType().BaseType; // Config is the base type
            if (configBaseType != null)
            {
                var additionalField = configBaseType.GetField("_additionalData", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull( additionalField, "_additionalData field not found via reflection" );
                additionalField.SetValue( eddiConfig, legacyData );

                // Create an immutable dictionary matching what ConvertLegacyConfigData expects
                var dict = new Dictionary<string, Config>()
                {
                    [nameof(CommanderConfiguration)] = commanderConfig,
                    [nameof(EDDIConfiguration)] = eddiConfig,
                    [nameof(FleetCarrierConfiguration)] = fleetCarrierConfig
                }.ToImmutableDictionary();

                // Migration only needs the supplied in-memory configurations, not the singleton.
                var method = typeof(ConfigService).GetMethod("ConvertLegacyConfigData", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull( method, "ConvertLegacyConfigData method not found via reflection" );
                method.Invoke( null, [ dict ] );

                // Verify that migration copied values into the commander configuration
                Assert.AreEqual( "LegacyCmdr", commanderConfig.commanderName );
                Assert.AreEqual( "Female", commanderConfig.gender );
                Assert.AreEqual( (ulong)123456789, commanderConfig.homeSystemAddress );
                Assert.AreEqual( (long)987654321, commanderConfig.homeStationMarketID );
                Assert.AreEqual( "Phonetic", commanderConfig.phoneticName );
                Assert.AreEqual( 42, commanderConfig.powerMerits );
                Assert.AreEqual( "MySquad", commanderConfig.squadronName );
                Assert.AreEqual( "SQID", commanderConfig.squadronTag );

                // Verify fleet carrier migration
                Assert.IsNotNull( fleetCarrierConfig.fleetCarrier );
                Assert.AreEqual( "CarrierName", fleetCarrierConfig.fleetCarrier.name );

                // Ensure legacy additional data cleared after migration
                var additionalAfter = additionalField.GetValue(eddiConfig) as IDictionary<string, JToken>;
                Assert.IsNull( additionalAfter );
            }
        }
    }
}
