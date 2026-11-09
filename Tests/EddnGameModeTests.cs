using EddiCompanionAppService;
using EddiCore;
using EddiDataDefinitions;
using EddiEddnResponder;
using EddiEddnResponder.Schemas;
using EddiEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Properties;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class EddnGameModeTests : TestBase
    {
        private const string location = """
            { "timestamp":"2026-10-14T09:30:04Z", "event":"Location", "StarSystem":"Sol",
              "SystemAddress":10477373803, "StarPos":[0,0,0], "Docked":true,
              "StationName":"Test station", "MarketID":127000000, "Body":"Earth", "BodyID":3 }
            """;
        private const string navRoute = """
            { "timestamp":"2026-10-14T09:30:04Z", "event":"NavRoute",
              "Route":[{"StarSystem":"Sol","SystemAddress":10477373803,"StarPos":[0,0,0]}] }
            """;

        [TestInitialize]
        public void Start () => MakeSafe();

        private static EDDNResponder CreateResponder ()
        {
            var responder = new EDDNResponder();
            responder.SetUnitTesting( true );
            responder.schemas.AddRange( [new FSSSignalDiscoveredSchema(), new JournalSchema(), new NavRouteSchema()] );
            responder.capiSchemas.Add( new CommoditySchema() );
            return responder;
        }

        private static Task Journal ( EDDNResponder responder, string raw, bool fromLoad = false ) =>
            responder.HandleAsync( new CockpitBreachedEvent( DateTime.UtcNow ) { raw = raw, fromLoad = fromLoad } );

        private static Task Mode ( EDDNResponder responder, string mode, bool fromLoad = false ) =>
            Journal( responder, new JObject
            {
                ["timestamp"] = "2026-10-14T09:30:04Z",
                ["event"] = "GameModeChange",
                ["GameMode"] = mode
            }.ToString(), fromLoad );

        private static Task Signal ( EDDNResponder responder, string name ) =>
            Journal( responder, new JObject
            {
                ["timestamp"] = "2026-10-14T09:30:04Z",
                ["event"] = "FSSSignalDiscovered",
                ["SystemAddress"] = 10477373803UL,
                ["SignalName"] = name
            }.ToString() );

        private static void Capi ( EDDNResponder responder )
        {
            var market = DeserializeJsonResource<JObject>( Resources.capi_market_Libby_Horizons );
            market["id"] = 127000000;
            market["name"] = "Test station";
            responder.FrontierApiOnStationUpdatedEvent( null, new CompanionApiEndpointEventArgs(
                JObject.Parse( """
                    {"commander":{"docked":true},"lastSystem":{"name":"Sol"},
                     "lastStarport":{"id":127000000,"name":"Test station"}}
                    """ ), market, null, null ) );
        }

        [TestMethod]
        [DataRow( "Operation" )]
        [DataRow( "ProvingGrounds" )]
        public async Task InstanceSuppressesJournalAndCapiUntilMainGame ( string mode )
        {
            var responder = CreateResponder();
            await Mode( responder, mode );
            await Journal( responder, location );
            await Journal( responder, navRoute );
            Capi( responder );

            Assert.IsEmpty( responder.eddnSender.sentMessages );
            Assert.IsFalse( responder.eddnState.Location.StarSystemLocationIsSet() );

            await Mode( responder, "MainGame" );
            await Journal( responder, navRoute );
            await Journal( responder, location );
            Capi( responder );

            var expected = new[]
            {
                "https://eddn.edcd.io/schemas/navroute/1",
                "https://eddn.edcd.io/schemas/journal/1",
                "https://eddn.edcd.io/schemas/commodity/3"
            };
            CollectionAssert.AreEqual( expected, responder.eddnSender.sentMessages.Select( m => m.schema ).ToArray() );
        }

        [TestMethod]
        public async Task AbsentOrUnfamiliarModeDoesNotBlacklistMarketIds ()
        {
            var responder = CreateResponder();
            await Journal( responder, location );
            Capi( responder );
            await Mode( responder, "UnfamiliarMode" );
            await Journal( responder, location );
            Assert.HasCount( 3, responder.eddnSender.sentMessages );
            Assert.IsNull( responder.eddnState.GameMode );

            await Mode( responder, "Operation" );
            await Mode( responder, "UnfamiliarMode" );
            await Journal( responder, navRoute );
            Assert.HasCount( 3, responder.eddnSender.sentMessages );
            Assert.AreEqual( "Operation", responder.eddnState.GameMode );
        }

        [TestMethod]
        [DataRow( "Operation" )]
        [DataRow( "ProvingGrounds" )]
        public async Task HistoricalModeChangesReconstructEligibilityWithoutSubmitting ( string mode )
        {
            var responder = CreateResponder();
            await Mode( responder, mode, true );
            await Journal( responder, location, true );
            await Journal( responder, navRoute );
            Capi( responder );
            Assert.IsEmpty( responder.eddnSender.sentMessages );

            await Mode( responder, "MainGame", true );
            await Journal( responder, location, true );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
            Assert.IsTrue( responder.eddnState.Location.StarSystemLocationIsSet() );
            await Journal( responder, navRoute );
            Assert.HasCount( 1, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "Operation" )]
        [DataRow( "ProvingGrounds" )]
        public async Task InstanceRetainsVersionMetadataForMainGameRecovery ( string mode )
        {
            var responder = CreateResponder();
            await Mode( responder, mode, true );
            await Journal( responder, """
                {"event":"FileHeader","gameversion":"4.4.1.1","build":"test-build"}
                """, true );
            await Journal( responder, """
                {"event":"LoadGame","Horizons":true,"Odyssey":true}
                """, true );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
            Assert.IsFalse( responder.eddnState.Location.StarSystemLocationIsSet() );

            await Mode( responder, "MainGame" );
            Assert.AreEqual( "4.4.1.1", responder.eddnState.GameVersion.gameVersion );
            Assert.AreEqual( "test-build", responder.eddnState.GameVersion.gameBuild );
            Assert.IsTrue( responder.eddnState.GameVersion.inHorizons );
            Assert.IsTrue( responder.eddnState.GameVersion.inOdyssey );
        }

        [TestMethod]
        public async Task CqcModeChangesAreTrackedWhileInTelepresence ()
        {
            var responder = CreateResponder();
            var wasInTelepresence = EDDI.Instance.GameState.inTelepresence;
            try
            {
                EDDI.Instance.GameStateMutator.inTelepresence = true;
                await Mode( responder, "ProvingGrounds", true );
                Assert.AreEqual( "ProvingGrounds", responder.eddnState.GameMode );
                Capi( responder );
                await Mode( responder, "MainGame" );
                Assert.AreEqual( "MainGame", responder.eddnState.GameMode );
                await Journal( responder, navRoute );
                Assert.IsEmpty( responder.eddnSender.sentMessages );

                EDDI.Instance.GameStateMutator.inTelepresence = false;
                await Journal( responder, navRoute );
                Assert.HasCount( 1, responder.eddnSender.sentMessages );
            }
            finally
            {
                EDDI.Instance.GameStateMutator.inTelepresence = wasInTelepresence;
            }
        }

        [TestMethod]
        [DataRow( "Operation" )]
        [DataRow( "ProvingGrounds" )]
        public async Task ModeBoundariesClearLocationAndDiscardInstanceStatus ( string mode )
        {
            var responder = CreateResponder();
            var status = new Status
            {
                bodyname = "Earth", latitude = 1, longitude = 2,
                timestamp = new DateTime( 2026, 10, 14, 9, 30, 4, DateTimeKind.Utc )
            };
            await Journal( responder, location );
            await responder.HandleStatusAsync( status );
            Assert.AreEqual( "Earth", responder.eddnState.Location.journalBodyName );
            Assert.AreEqual( "Earth", responder.eddnState.Location.statusBodyName );

            await Mode( responder, mode );
            await responder.HandleStatusAsync( status );
            var cached = responder.eddnState.Location;
            Assert.IsFalse( cached.StarSystemLocationIsSet() );
            Assert.IsNull( cached.stationName );
            Assert.IsNull( cached.marketId );
            Assert.IsNull( cached.journalBodyName );
            Assert.IsNull( cached.journalBodyId );
            Assert.IsNull( cached.statusBodyName );
            Assert.IsNull( cached.statusLatitude );
            Assert.IsNull( cached.statusLongitude );
            Assert.IsNull( cached.statusTimeStamp );

            await Mode( responder, "MainGame" );
            await Journal( responder, """
                {"timestamp":"2026-10-14T09:30:04Z","event":"Scan","ScanType":"AutoScan",
                 "StarSystem":"Sol","SystemAddress":10477373803,"BodyName":"Sol","BodyID":0}
                """ );
            Assert.HasCount( 1, responder.eddnSender.sentMessages ); // Only the initial Location.
            Assert.IsFalse( cached.StarSystemLocationIsSet() );
            await Journal( responder, location );
            Assert.HasCount( 2, responder.eddnSender.sentMessages );
            Assert.IsTrue( cached.StarSystemLocationIsSet() );
        }

        [TestMethod]
        [DataRow( "Operation" )]
        [DataRow( "ProvingGrounds" )]
        public async Task FssBatchesAndSnapshotsDoNotCrossInstanceBoundaries ( string mode )
        {
            var responder = CreateResponder();
            await Journal( responder, location );
            await Signal( responder, "Before instance" );
            await Mode( responder, mode );
            await Signal( responder, "Inside instance" );
            await Journal( responder, location );
            await Mode( responder, "MainGame" );
            await Signal( responder, "After instance" );
            await Journal( responder, """{"event":"Music","MusicTrack":"NoTrack"}""" );
            Assert.HasCount( 1, responder.eddnSender.sentMessages );

            await Journal( responder, location );
            var (schema, data, eddnState, gameVersionOverride) = responder.eddnSender.sentMessages.Single( m => m.schema.EndsWith( "/fsssignaldiscovered/1" ) );
            var signals = (List<IDictionary<string, object>>)data[ "signals" ];
            Assert.AreEqual( "After instance", signals.Single()[ "SignalName" ] );
            Assert.AreEqual( "Sol", data[ "StarSystem" ] );
        }

        [TestMethod]
        public async Task RepeatedMainGameDoesNotDiscardPendingFss ()
        {
            var responder = CreateResponder();
            await Mode( responder, "MainGame" );
            await Journal( responder, location );
            await Signal( responder, "Nav beacon" );
            await Mode( responder, "MainGame" );
            Assert.HasCount( 1, responder.eddnSender.sentMessages.Where( m => m.schema.EndsWith( "/fsssignaldiscovered/1" ) ).ToList() );
        }
    }
}
