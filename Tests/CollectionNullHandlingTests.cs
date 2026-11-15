using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class CollectionNullHandlingTests
    {
        [TestMethod]
        [DataRow( "{}" )]
        [DataRow( "{\"economyShares\":null}" )]
        [DataRow( "{\"economyShares\":[]}" )]
        public void StationEconomiesLoadAsAnEmptyMutableCollection ( string json )
        {
            var station = JsonConvert.DeserializeObject<Station>( json );

            Assert.IsNotNull( station.economyShares );
            Assert.IsEmpty( station.economyShares );
            station.economyShares.Add( new EconomyShare( Economy.Industrial, 1M ) );
            Assert.AreEqual( Economy.Industrial.localizedName, station.primaryeconomy );
        }

        [TestMethod]
        public void StationEconomiesNormalizeDirectNullAssignment ()
        {
            var station = new Station { economyShares = [ new EconomyShare( Economy.Industrial, 1M ) ] };

            station.economyShares = null;

            Assert.IsEmpty( station.economyShares );
            Assert.AreEqual( Economy.None.localizedName, station.primaryeconomy );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void StationEconomiesConsolidateDuplicates ( bool loadFromJson )
        {
            List<EconomyShare> shares =
            [
                new EconomyShare( Economy.Industrial, 0.2M ),
                new EconomyShare( Economy.Industrial, 0.4M ),
                new EconomyShare( Economy.Refinery, 0.4M )
            ];
            var station = loadFromJson
                ? JsonConvert.DeserializeObject<Station>( new JObject
                    { [ "economyShares" ] = JArray.FromObject( shares ) }.ToString() )
                : new Station { economyShares = shares };

            Assert.HasCount( 2, station.economyShares );
            Assert.AreEqual( Economy.Industrial.edname, station.economyShares[ 0 ].economy.edname );
            Assert.AreEqual( 0.6M, station.economyShares[ 0 ].proportion );
            Assert.AreEqual( Economy.Refinery.edname, station.economyShares[ 1 ].economy.edname );
            Assert.AreEqual( 0.4M, station.economyShares[ 1 ].proportion );

            var json = JObject.Parse( JsonConvert.SerializeObject( station ) );
            Assert.HasCount( 2, (JArray)json[ "economyShares" ] );
            Assert.IsNull( json[ "_economyShares" ] );
            var reloaded = json.ToObject<Station>();
            Assert.HasCount( 2, reloaded.economyShares );
            Assert.AreEqual( 0.6M, reloaded.economyShares[ 0 ].proportion );
            Assert.AreEqual( 0.4M, reloaded.economyShares[ 1 ].proportion );
        }

        [TestMethod]
        [DataRow( "null" )]
        [DataRow( "[]" )]
        public void StationEconomyJsonClearsExistingShares ( string jsonValue )
        {
            var station = new Station { economyShares = [ new EconomyShare( Economy.Industrial, 1M ) ] };

            JsonConvert.PopulateObject( "{\"economyShares\":" + jsonValue + "}", station );

            Assert.IsNotNull( station.economyShares );
            Assert.IsEmpty( station.economyShares );
        }

        [TestMethod]
        [DataRow( "{\"edname\":\"Tea\"}", 0 )]
        [DataRow( "{\"edname\":\"Tea\",\"missionCargo\":null}", 0 )]
        [DataRow( "{\"edname\":\"Tea\",\"missionCargo\":{}}", 0 )]
        [DataRow( "{\"edname\":\"Tea\",\"missionCargo\":{\"42\":2,\"43\":3}}", 5 )]
        public void MissionCargoJsonRecalculatesHaulage ( string json, int expectedHaulage )
        {
            var cargo = JsonConvert.DeserializeObject<Cargo>( json );

            Assert.IsNotNull( cargo.missionCargo );
            Assert.AreEqual( expectedHaulage, cargo.haulage );
            Assert.AreEqual( expectedHaulage, cargo.total );
            cargo.AddDetailedQty( 99UL, 4 );
            Assert.AreEqual( 4, cargo.missionCargo[ 99 ] );
            Assert.AreEqual( expectedHaulage + 4, cargo.haulage );

            var reloaded = JsonConvert.DeserializeObject<Cargo>( JsonConvert.SerializeObject( cargo ) );
            CollectionAssert.AreEquivalent( cargo.missionCargo.ToArray(), reloaded.missionCargo.ToArray() );
            Assert.AreEqual( cargo.haulage, reloaded.haulage );
        }

        [TestMethod]
        public void MissionCargoNormalizesNullAndClearsHaulage ()
        {
            var cargo = new Cargo( "Tea" ) { owned = 2, stolen = 1, missionCargo = new() { [ 42 ] = 5 } };
            var notifications = new List<string>();
            cargo.PropertyChanged += ( _, args ) => notifications.Add( args.PropertyName );

            cargo.missionCargo = null;

            Assert.IsEmpty( cargo.missionCargo );
            Assert.AreEqual( 0, cargo.haulage );
            Assert.AreEqual( 3, cargo.total );
            Assert.Contains( nameof( Cargo.haulage ), notifications );
        }

        [TestMethod]
        [DataRow( "null", 0 )]
        [DataRow( "{}", 0 )]
        [DataRow( "{\"43\":3}", 3 )]
        public void MissionCargoJsonReplacesExistingQuantities ( string jsonValue, int expectedHaulage )
        {
            var cargo = new Cargo( "Tea" ) { missionCargo = new() { [ 42 ] = 5 } };

            JsonConvert.PopulateObject( "{\"missionCargo\":" + jsonValue + "}", cargo );

            Assert.IsNotNull( cargo.missionCargo );
            Assert.IsFalse( cargo.missionCargo.ContainsKey( 42 ) );
            Assert.AreEqual( expectedHaulage, cargo.haulage );
        }

        [TestMethod]
        [DataRow( "{}" )]
        [DataRow( "{\"hardpoints\":null,\"compartments\":null,\"launchbays\":null}" )]
        [DataRow( "{\"hardpoints\":[],\"compartments\":[],\"launchbays\":[]}" )]
        public void ShipEquipmentJsonLoadsEmptyMutableCollections ( string json )
        {
            var ship = JsonConvert.DeserializeObject<Ship>( json );

            Assert.IsEmpty( ship.hardpoints );
            Assert.IsEmpty( ship.compartments );
            Assert.IsEmpty( ship.launchbays );
            Assert.AreEqual( 0, ship.cargocapacity );
            _ = ship.fueltanktotalcapacity;

            ship.hardpoints.Add( new Hardpoint { name = "SmallHardpoint1" } );
            ship.compartments.Add( new Compartment { name = "Slot01_Size2" } );
            ship.launchbays.Add( new LaunchBay { name = "Slot02_Size2" } );
            Assert.HasCount( 1, ship.hardpoints );
            Assert.HasCount( 1, ship.compartments );
            Assert.HasCount( 1, ship.launchbays );
        }

        [TestMethod]
        public void ShipEquipmentNullAssignmentsClearCollectionsAndNotify ()
        {
            var ship = new Ship
            {
                hardpoints = [ new Hardpoint() ],
                compartments = [ new Compartment() ],
                launchbays = [ new LaunchBay() ]
            };
            var notifications = new List<string>();
            ship.PropertyChanged += ( _, args ) => notifications.Add( args.PropertyName );

            ship.hardpoints = null;
            ship.compartments = null;
            ship.launchbays = null;

            Assert.IsEmpty( ship.hardpoints );
            Assert.IsEmpty( ship.compartments );
            Assert.IsEmpty( ship.launchbays );
            CollectionAssert.AreEquivalent( new[] { nameof( Ship.hardpoints ), nameof( Ship.compartments ),
                nameof( Ship.launchbays ) }, notifications );
        }

        [TestMethod]
        public void ShipEquipmentSurvivesJsonRoundTrip ()
        {
            var ship = new Ship
            {
                hardpoints = [ new Hardpoint { name = "SmallHardpoint1", size = 1 } ],
                compartments = [ new Compartment { name = "Slot01_Size2", size = 2 } ],
                launchbays = [ new LaunchBay { name = "Slot02_Size2", size = 2, type = "SRV" } ]
            };

            var reloaded = JsonConvert.DeserializeObject<Ship>( JsonConvert.SerializeObject( ship ) );

            Assert.HasCount( 1, reloaded.hardpoints );
            Assert.HasCount( 1, reloaded.compartments );
            Assert.HasCount( 1, reloaded.launchbays );
            Assert.AreEqual( "SmallHardpoint1", reloaded.hardpoints[ 0 ].name );
            Assert.AreEqual( 1, reloaded.hardpoints[ 0 ].size );
            Assert.AreEqual( "Slot01_Size2", reloaded.compartments[ 0 ].name );
            Assert.AreEqual( 2, reloaded.compartments[ 0 ].size );
            Assert.AreEqual( "Slot02_Size2", reloaded.launchbays[ 0 ].name );
            Assert.AreEqual( "SRV", reloaded.launchbays[ 0 ].type );
        }
    }
}
