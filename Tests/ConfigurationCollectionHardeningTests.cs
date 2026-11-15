using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore.GameState;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class ConfigurationCollectionHardeningTests
    {
        private static readonly JsonSerializerSettings collectionJsonSettings = new()
        {
            ContractResolver = new CollectionContractResolver()
        };

        private sealed class CollectionContractResolver : DefaultContractResolver
        {
            protected override JsonObjectContract CreateObjectContract ( Type objectType )
            {
                var contract = base.CreateObjectContract( objectType );
                if ( objectType == typeof( ShipMonitorConfiguration ) )
                {
                    // These tests cover collection contracts, not legacy profile migration or ship augmentation.
                    // The production callback can read and delete ships.json in the user's profile.
                    contract.OnDeserializedCallbacks.Clear();
                }
                return contract;
            }
        }

        [TestMethod]
        public void CollectionJsonSettingsSuppressOnlyShipConfigurationCallbacks ()
        {
            var productionResolver = new DefaultContractResolver();
            var productionShipContract = (JsonObjectContract)productionResolver.ResolveContract( typeof( ShipMonitorConfiguration ) );
            var testShipContract = (JsonObjectContract)collectionJsonSettings.ContractResolver.ResolveContract( typeof( ShipMonitorConfiguration ) );
            Assert.IsNotEmpty( productionShipContract.OnDeserializedCallbacks );
            Assert.IsEmpty( testShipContract.OnDeserializedCallbacks );

            foreach ( var type in new[] { typeof( Body ), typeof( Ship ) } )
            {
                var productionContract = (JsonObjectContract)productionResolver.ResolveContract( type );
                var testContract = (JsonObjectContract)collectionJsonSettings.ContractResolver.ResolveContract( type );
                Assert.IsNotEmpty( productionContract.OnDeserializedCallbacks );
                Assert.AreEqual( productionContract.OnDeserializedCallbacks.Count, testContract.OnDeserializedCallbacks.Count );
            }
        }

        public static IEnumerable<object[]> ConfigurationCollections ()
        {
            yield return [ typeof( CargoMonitorConfiguration ), "cargo", "[{\"edname\":\"Tea\",\"owned\":2}]" ];
            yield return [ typeof( MaterialMonitorConfiguration ), "materials", "[{\"edname\":\"iron\",\"amount\":3}]" ];
            yield return [ typeof( ShipMonitorConfiguration ), "shipyard", "[{\"LocalId\":42,\"model\":\"Sidewinder\"}]" ];
            yield return [ typeof( ShipMonitorConfiguration ), "storedmodules", "[{\"slot\":42,\"system\":\"Sol\"}]" ];
            yield return [ typeof( CrimeMonitorConfiguration ), "criminalrecord", "[{\"faction\":\"Review faction\"}]" ];
            yield return [ typeof( CrimeMonitorConfiguration ), "homeSystems", "{\"Review faction\":\"Sol\"}" ];
        }

        public static IEnumerable<object[]> ConfigurationJsonCases ()
        {
            foreach ( var row in ConfigurationCollections() )
            {
                var propertyName = (string)row[ 1 ];
                var populatedJson = (string)row[ 2 ];
                var emptyJson = populatedJson.StartsWith( "[" ) ? "[]" : "{}";
                yield return [ row[ 0 ], propertyName, "{}", 0 ];
                yield return [ row[ 0 ], propertyName, "{\"" + propertyName + "\":null}", 0 ];
                yield return [ row[ 0 ], propertyName, "{\"" + propertyName + "\":" + emptyJson + "}", 0 ];
                yield return [ row[ 0 ], propertyName, "{\"" + propertyName + "\":" + populatedJson + "}", 1 ];
            }
        }

        [TestMethod]
        [DynamicData( nameof( ConfigurationJsonCases ) )]
        public void ConfigurationJsonRetainsNonNullCollections (
            Type configurationType, string propertyName, string json, int expectedCount )
        {
            var configuration = JsonConvert.DeserializeObject( json, configurationType, collectionJsonSettings );
            var property = configurationType.GetProperty( propertyName );
            var collection = property.GetValue( configuration );

            Assert.IsNotNull( collection );
            Assert.AreEqual( expectedCount, ( (IEnumerable)collection ).Cast<object>().Count() );

            var savedJson = JsonConvert.SerializeObject( configuration, collectionJsonSettings );
            var reloaded = JsonConvert.DeserializeObject( savedJson, configurationType, collectionJsonSettings );
            Assert.IsNotNull( property.GetValue( reloaded ) );
            Assert.AreEqual( expectedCount, ( (IEnumerable)property.GetValue( reloaded ) ).Cast<object>().Count() );
            if ( expectedCount > 0 )
            {
                // Check the supplied fields; ship deserialization also derives metadata from its model.
                var suppliedJson = JObject.Parse( json )[ propertyName ];
                var reloadedJson = JToken.FromObject( property.GetValue( reloaded ) );
                if ( suppliedJson is JArray suppliedItems )
                {
                    foreach ( var suppliedProperty in ( (JObject)suppliedItems[ 0 ] ).Properties() )
                    {
                        Assert.IsTrue( JToken.DeepEquals( suppliedProperty.Value,
                            reloadedJson[ 0 ][ suppliedProperty.Name ] ), suppliedProperty.Name );
                    }
                }
                else { Assert.IsTrue( JToken.DeepEquals( suppliedJson, reloadedJson ) ); }
            }
        }

        [TestMethod]
        [DynamicData( nameof( ConfigurationCollections ) )]
        public void ConfigurationNullAssignmentClearsCollectionAndPreservesNotifications (
            Type configurationType, string propertyName, string populatedJson )
        {
            var configuration = (Config)Activator.CreateInstance( configurationType );
            var property = configurationType.GetProperty( propertyName );
            var suppliedCollection = JsonConvert.DeserializeObject( populatedJson, property.PropertyType, collectionJsonSettings );
            property.SetValue( configuration, suppliedCollection );
            Assert.AreSame( suppliedCollection, property.GetValue( configuration ) );

            var notifications = new List<string>();
            configuration.PropertyChanged += ( _, args ) => notifications.Add( args.PropertyName );
            property.SetValue( configuration, suppliedCollection );
            Assert.IsEmpty( notifications );

            property.SetValue( configuration, null );

            Assert.IsNotNull( property.GetValue( configuration ) );
            Assert.IsEmpty( ( (IEnumerable)property.GetValue( configuration ) ).Cast<object>() );
            // The locked shipyard setter has never raised PropertyChanged.
            if ( propertyName == "shipyard" ) { Assert.IsEmpty( notifications ); }
            else { CollectionAssert.AreEqual( new[] { propertyName }, notifications ); }
        }

        [TestMethod]
        [DynamicData( nameof( ConfigurationCollections ) )]
        public void ConfigurationJsonNullClearsExistingCollection (
            Type configurationType, string propertyName, string populatedJson )
        {
            var configuration = Activator.CreateInstance( configurationType );
            var property = configurationType.GetProperty( propertyName );
            property.SetValue( configuration, JsonConvert.DeserializeObject( populatedJson, property.PropertyType, collectionJsonSettings ) );

            JsonConvert.PopulateObject( "{\"" + propertyName + "\":null}", configuration, collectionJsonSettings );

            Assert.IsNotNull( property.GetValue( configuration ) );
            Assert.IsEmpty( ( (IEnumerable)property.GetValue( configuration ) ).Cast<object>() );
        }

        [TestMethod]
        public void BodyParentsNullIsNormalizedBeforePlanetTypeInference ()
        {
            var body = new Body { bodyType = null, planetClass = PlanetClass.EarthLikeBody };
            var notifications = new List<string>();
            body.PropertyChanged += ( _, args ) => notifications.Add( args.PropertyName );

            body.parents = null;

            Assert.IsNotNull( body.parents );
            Assert.IsEmpty( body.parents );
            Assert.IsNull( body.bodyType );
            CollectionAssert.AreEqual( new[] { nameof( Body.parents ) }, notifications );
            body.parents.Add( new Dictionary<string, int> { [ "Star" ] = 0 } );
            Assert.AreEqual( 0, body.parents.Single()[ "Star" ] );
        }

        [TestMethod]
        public void BodyParentsJsonNullIsNormalizedBeforePlanetTypeInference ()
        {
            var json = new JObject
            {
                [ "Type" ] = JValue.CreateNull(),
                [ "planetClass" ] = JObject.FromObject( PlanetClass.EarthLikeBody ),
                [ "parents" ] = JValue.CreateNull()
            };

            var body = json.ToObject<Body>( JsonSerializer.Create( collectionJsonSettings ) );

            Assert.IsNotNull( body.parents );
            Assert.IsEmpty( body.parents );
            Assert.IsNull( body.bodyType );
        }

        [TestMethod]
        [DataRow( "Star", "Planet" )]
        [DataRow( "Planet", "Moon" )]
        public void BodyParentsRetainPlanetAndMoonInference ( string parentType, string expectedType )
        {
            var body = new Body { bodyType = null, planetClass = PlanetClass.EarthLikeBody };

            body.parents = [ new Dictionary<string, int> { [ parentType ] = 0 } ];

            Assert.AreEqual( expectedType, body.bodyType.edname );
            Assert.AreEqual( 0, body.parents.Single()[ parentType ] );
        }

        [TestMethod]
        public void BodyParentsNullRetainsStarInferenceWhenPlanetClassIsMissing ()
        {
            var body = new Body { bodyType = null, planetClass = null };

            body.parents = null;

            Assert.AreSame( BodyType.Star, body.bodyType );
            Assert.IsEmpty( body.parents );
        }

        [TestMethod]
        public void DeployedVesselsNullAssignmentRetainsCollectionAndReplacementNotification ()
        {
            var state = new EddiGameState();
            state.DeployedVessels.Add( 1, VesselDefinition.Fighter_Federation );
            var oldCollection = state.DeployedVessels;
            var notifications = new List<string>();
            state.PropertyChanged += ( _, args ) => notifications.Add( args.PropertyName );

            state.DeployedVessels = null;

            Assert.IsNotNull( state.DeployedVessels );
            Assert.AreNotSame( oldCollection, state.DeployedVessels );
            Assert.IsEmpty( state.DeployedVessels );
            CollectionAssert.AreEqual( new[] { nameof( state.DeployedVessels ) }, notifications );

            state.DeployedVessels.Add( 2, VesselDefinition.SRV_Scarab );
            Assert.AreSame( VesselDefinition.SRV_Scarab, state.DeployedVessels[ 2 ] );
            Assert.AreSame( VesselDefinition.Fighter_Federation, oldCollection[ 1 ] );
            // Dictionary mutations do not raise property-change notifications.
            Assert.HasCount( 1, notifications );

            var replacement = new Dictionary<int, VesselDefinition> { [ 3 ] = VesselDefinition.Fighter_Empire };
            state.DeployedVessels = replacement;
            Assert.AreSame( replacement, state.DeployedVessels );
            CollectionAssert.AreEqual( new[] { nameof( state.DeployedVessels ), nameof( state.DeployedVessels ) }, notifications );
        }
    }
}
