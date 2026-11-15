using EddiConfigService.Configurations;
using EddiCore.GameState;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class CollectionPersistenceTests
    {
        [TestMethod]
        public void BodyRingsAndParentsSurviveJsonRoundTrip ()
        {
            var body = new Body
            {
                bodyname = "Review planet",
                rings = [ new Ring( "Review ring", RingComposition.FromEDName( "Icy" ), 1, 2, 3 ) ],
                parents = [ new Dictionary<string, int> { [ "Star" ] = 0 } ]
            };

            var reloaded = JsonConvert.DeserializeObject<Body>( JsonConvert.SerializeObject( body ) );

            Assert.HasCount( 1, reloaded.rings );
            Assert.AreEqual( "Review ring", reloaded.rings[ 0 ].name );
            Assert.HasCount( 1, reloaded.parents );
            Assert.AreEqual( 0, reloaded.parents[ 0 ][ "Star" ] );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void BodyCollectionsRetainAddedItems ( bool resetToNull )
        {
            var body = new Body();
            if ( resetToNull )
            {
                body.rings = null;
                body.parents = null;
            }

            body.rings.Add( new Ring( "Review ring", RingComposition.FromEDName( "Icy" ), 1, 2, 3 ) );
            body.parents.Add( new Dictionary<string, int> { [ "Star" ] = 0 } );

            Assert.HasCount( 1, body.rings );
            Assert.HasCount( 1, body.parents );
            Assert.AreEqual( "Review ring", body.rings[ 0 ].name );
            Assert.AreEqual( 0, body.parents[ 0 ][ "Star" ] );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void ShipSpecialtySlotsRetainAddedItems ( bool resetToNull )
        {
            var ship = new Ship();
            if ( resetToNull ) { ship.specialtySlotSizes = null; }

            ship.specialtySlotSizes.Add( "Military01", 5 );

            Assert.AreEqual( 5, ship.specialtySlotSizes[ "Military01" ] );
        }

        [TestMethod]
        [DataRow( "{}" )]
        [DataRow( "{\"discoveredEntryIds\":null}" )]
        [DataRow( "{\"discoveredEntryIds\":[42]}" )]
        public void CodexDiscoveriesRetainChangesAfterLoading ( string json )
        {
            var configuration = JsonConvert.DeserializeObject<CodexDiscoveryConfiguration>( json );
            configuration.discoveredEntryIds.Add( 99 );

            var reloaded = JsonConvert.DeserializeObject<CodexDiscoveryConfiguration>(
                JsonConvert.SerializeObject( configuration ) );

            Assert.Contains( 99L, reloaded.discoveredEntryIds );
            if ( json.Contains( "42" ) ) { Assert.Contains( 42L, reloaded.discoveredEntryIds ); }
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void DeployedVesselsRetainAddedItems ( bool resetToNull )
        {
            var state = new EddiGameState();
            if ( resetToNull ) { state.DeployedVessels = null; }

            state.DeployedVessels.Add( 1, VesselDefinition.Fighter_Federation );

            Assert.AreSame( VesselDefinition.Fighter_Federation, state.DeployedVessels[ 1 ] );
        }
    }
}
