using EddiCompanionAppService;
using EddiCore;
using EddiEddnResponder;
using EddiEddnResponder.Schemas;
using EddiEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class EddnStationContextTests : TestBase
    {
        private const long marketId = 3228854528L;
        private const string stationName = "Libby Horizons";

        [TestInitialize]
        public void Start () => MakeSafe();

        private static EDDNResponder CreateResponder ()
        {
            var responder = new EDDNResponder();
            responder.SetUnitTesting( true );
            responder.capiSchemas.AddRange( [new CommoditySchema(), new OutfittingSchema(), new ShipyardSchema(), new FCMaterialsSchema()] );
            return responder;
        }

        private static Task Journal ( EDDNResponder responder, JObject data, bool fromLoad = false ) =>
            responder.HandleAsync( new CockpitBreachedEvent( DateTime.UtcNow ) { raw = data.ToString(), fromLoad = fromLoad } );

        private static JObject DockingData ( string edType = "Docked" ) => new()
        {
            ["timestamp"] = "2026-10-14T09:30:04Z", ["event"] = edType,
            ["StarSystem"] = "Sol", ["SystemAddress"] = 10477373803UL,
            ["StarPos"] = new JArray( 0, 0, 0 ), ["Docked"] = true,
            ["StationName"] = stationName, ["MarketID"] = marketId
        };

        private static CompanionApiEndpointEventArgs Callback () => new(
            JObject.Parse( """
                {"commander":{"docked":true},"lastSystem":{"name":"Sol"},
                 "lastStarport":{"id":"3228854528","name":"Libby Horizons"}}
                """ ),
            JObject.Parse( """
                {"timestamp":"2026-10-14T09:30:04Z","id":3228854528,"name":"Libby Horizons",
                 "economies":{},"commodities":[{"name":"gold","meanPrice":100,"buyPrice":100,
                 "sellPrice":90,"demand":0,"demandBracket":0,"stock":5,"stockBracket":1}]}
                """ ),
            JObject.Parse( """
                {"timestamp":"2026-10-14T09:30:04Z","id":3228854528,"name":"Libby Horizons",
                 "modules":{"128":{"id":128,"name":"int_cargorack_size1_class1","cost":1000}},
                 "ships":{"shipyard_list":{"0":{"name":"sidewinder"}}}}
                """ ), null );

        private static void Capi ( EDDNResponder responder, CompanionApiEndpointEventArgs callback ) =>
            responder.FrontierApiOnStationUpdatedEvent( null, callback );

        // Reduced journal evidence: commander/account fields and unrelated faction data omitted.
        private static JObject CarrierDocking ( string stationType = "FleetCarrier" )
        {
            var data = JObject.Parse( """
                {"timestamp":"2026-11-09T03:14:08Z","event":"Docked","StationName":"J9J-9KT",
                 "StationType":"FleetCarrier","MarketID":3707594240,"StarSystem":"Bei Dou Sector IH-V b2-4",
                 "SystemAddress":9466510910889,"Taxi":false,"Multicrew":false}
                """ );
            data["StationType"] = stationType;
            return data;
        }

        private static JObject CarrierDisembark () => JObject.Parse( """
            {"timestamp":"2026-11-09T03:18:31Z","event":"Disembark","SRV":false,"Taxi":false,
             "Multicrew":false,"StarSystem":"Bei Dou Sector IH-V b2-4","SystemAddress":9466510910889,
             "Body":"Bei Dou Sector IH-V b2-4 2","BodyID":3,"OnStation":false,"OnPlanet":true}
            """ );

        private static JObject OnFootCarrierJump () => JObject.Parse( """
            {"timestamp":"2026-11-09T03:33:01Z","event":"CarrierJump","Docked":false,"OnFoot":true,
             "StarSystem":"Diaguandri","SystemAddress":670417429889,"StarPos":[-41.06250,-62.15625,-103.25000],
             "Body":"Diaguandri AB 3","BodyID":22,"BodyType":"Planet"}
            """ );

        private static CompanionApiEndpointEventArgs CarrierCallback ( bool onFoot = true )
        {
            var callback = Callback();
            callback.profileJson["commander"]["docked"] = !onFoot;
            callback.profileJson["commander"]["onfoot"] = onFoot;
            callback.profileJson["lastSystem"]["name"] = "Diaguandri";
            foreach ( var station in new[] { (JObject)callback.profileJson["lastStarport"], callback.marketJson, callback.shipyardJson } )
            {
                station["name"] = "J9J-9KT";
                station["id"] = 3707594240L;
            }
            callback.marketJson["orders"] = JObject.Parse( """
                {"onfootmicroresources":{"sales":{"128064021":{"id":128064021,"name":"graphene","price":1300,"stock":1}},"purchases":{}}}
                """ );
            return callback;
        }

        [TestMethod]
        [DataRow( "Docked", false )]
        [DataRow( "Location", false )]
        [DataRow( "CarrierJump", false )]
        [DataRow( "Location", true )]
        public async Task ConfirmedDockingAllowsMatchingCapiSchemas ( string edType, bool fromLoad )
        {
            var responder = CreateResponder();
            await Journal( responder, DockingData( edType ), fromLoad );
            Assert.AreEqual( marketId, responder.eddnState.Location.marketId );
            Assert.AreEqual( stationName, responder.eddnState.Location.stationName );
            Capi( responder, Callback() );

            var expected = new[]
            {
                "https://eddn.edcd.io/schemas/commodity/3",
                "https://eddn.edcd.io/schemas/outfitting/3",
                "https://eddn.edcd.io/schemas/shipyard/2"
            };
            CollectionAssert.AreEqual( expected, responder.eddnSender.sentMessages.Select( m => m.schema ).ToArray() );
        }

        [TestMethod]
        [DataRow( "Undocked" )]
        [DataRow( "FSDJump" )]
        [DataRow( "LoadGame" )]
        [DataRow( "Shutdown" )]
        public async Task DepartureInvalidatesStationBeforeCallback ( string edType )
        {
            var responder = CreateResponder();
            await Journal( responder, DockingData() );
            await Journal( responder, new JObject { ["event"] = edType } );
            Assert.IsNull( responder.eddnState.Location.stationName );
            Assert.IsNull( responder.eddnState.Location.marketId );
            Capi( responder, Callback() );
            Assert.IsEmpty( responder.eddnSender.sentMessages );

            // Rejection must not update any schema's duplicate-suppression state.
            await Journal( responder, DockingData() );
            Capi( responder, Callback() );
            Assert.HasCount( 3, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "Location", false )]
        [DataRow( "Location", null )]
        [DataRow( "CarrierJump", false )]
        [DataRow( "CarrierJump", null )]
        public async Task LocationWithoutConfirmedDockingClearsStation ( string edType, bool? docked )
        {
            var responder = CreateResponder();
            await Journal( responder, DockingData() );
            var data = DockingData( edType );
            if ( docked == null ) { data.Remove( "Docked" ); }
            else { data["Docked"] = docked.Value; }
            await Journal( responder, data );
            Capi( responder, Callback() );
            Assert.IsNull( responder.eddnState.Location.stationName );
            Assert.IsNull( responder.eddnState.Location.marketId );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
        }

        [TestMethod]
        public void CapiCannotEstablishDockingOnItsOwn ()
        {
            var responder = CreateResponder();
            Capi( responder, Callback() );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "station" )]
        [DataRow( "system" )]
        [DataRow( "partial-system" )]
        public async Task ChangedJournalContextRejectsOldCallback ( string change )
        {
            var responder = CreateResponder();
            await Journal( responder, DockingData() );
            var data = DockingData( change == "partial-system" ? "Scan" : "Docked" );
            if ( change == "station" )
            {
                data["MarketID"] = marketId + 1;
                data["StationName"] = "Another station";
            }
            else
            {
                data["StarSystem"] = "Another system";
                data["SystemAddress"] = 3932277478106UL;
            }
            await Journal( responder, data );
            Capi( responder, Callback() );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
            if ( change == "partial-system" )
            {
                Assert.IsNull( responder.eddnState.Location.marketId );
            }
        }

        [TestMethod]
        [DataRow( "commander.docked", "false" )]
        [DataRow( "commander.docked", null )]
        [DataRow( "lastSystem.name", "Other system" )]
        [DataRow( "lastSystem.name", null )]
        [DataRow( "lastStarport.id", "1" )]
        [DataRow( "lastStarport.id", "invalid" )]
        [DataRow( "lastStarport.id", null )]
        [DataRow( "lastStarport.name", "Other station" )]
        [DataRow( "lastStarport.name", null )]
        public async Task MismatchedOrMissingProfileRejectsEntireCallback ( string path, string value )
        {
            var responder = CreateResponder();
            await Journal( responder, DockingData() );
            var callback = Callback();
            var property = (JProperty)callback.profileJson.SelectToken( path ).Parent;
            if ( value == null ) { property.Remove(); }
            else if ( path == "commander.docked" ) { property.Value = false; }
            else { property.Value = value; }
            Capi( responder, callback );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
            Capi( responder, Callback() );
            Assert.HasCount( 3, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "market", "id", "1" )]
        [DataRow( "market", "id", null )]
        [DataRow( "market", "name", "Other station" )]
        [DataRow( "market", "name", null )]
        [DataRow( "shipyard", "id", "1" )]
        [DataRow( "shipyard", "id", "invalid" )]
        [DataRow( "shipyard", "id", null )]
        [DataRow( "shipyard", "name", "Other station" )]
        [DataRow( "shipyard", "name", null )]
        public async Task MixedStationEndpointsRejectEntireCallback ( string endpoint, string field, string value )
        {
            var responder = CreateResponder();
            await Journal( responder, DockingData() );
            var callback = Callback();
            var data = endpoint == "market" ? callback.marketJson : callback.shipyardJson;
            if ( value == null ) { data.Remove( field ); }
            else { data[field] = value; }
            Capi( responder, callback );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
            Capi( responder, Callback() );
            Assert.HasCount( 3, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "market", 1 )]
        [DataRow( "shipyard", 2 )]
        [DataRow( "neither", 0 )]
        [DataRow( "no-profile", 0 )]
        public async Task MissingEndpointsOnlyAllowAvailableMatchingSchemas ( string available, int expected )
        {
            var responder = CreateResponder();
            await Journal( responder, DockingData() );
            var callback = Callback();
            Capi( responder, new CompanionApiEndpointEventArgs(
                available == "no-profile" ? null : callback.profileJson,
                available is "market" or "no-profile" ? callback.marketJson : null,
                available is "shipyard" or "no-profile" ? callback.shipyardJson : null, null ) );
            Assert.HasCount( expected, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        public async Task MatchingCarrierJumpRestoresStationAndAllowsCarrierMaterials ()
        {
            var responder = CreateResponder();
            var data = DockingData( "CarrierJump" );
            data["StationName"] = "X9X-9XX +++";
            data["MarketID"] = 3709999999L;
            await Journal( responder, data );
            var callback = Callback();
            callback.profileJson["lastStarport"]["name"] = "X9X-9XX +++";
            callback.profileJson["lastStarport"]["id"] = 3709999999L;
            callback.marketJson["name"] = "X9X-9XX";
            callback.marketJson["id"] = 3709999999L;
            callback.marketJson["orders"] = JObject.Parse( """
                {"onfootmicroresources":{"sales":{"128064021":{"id":128064021,"name":"graphene","price":1300,"stock":1}},"purchases":{}}}
                """ );
            Capi( responder, new CompanionApiEndpointEventArgs( callback.profileJson, callback.marketJson, null, null ) );
            var expected = new[]
            {
                "https://eddn.edcd.io/schemas/commodity/3",
                "https://eddn.edcd.io/schemas/fcmaterials_capi/1"
            };
            CollectionAssert.AreEqual( expected, responder.eddnSender.sentMessages.Select( m => m.schema ).ToArray() );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public async Task OnFootCarrierJumpSubmitsSystemWithoutInventingStationContext ( bool previouslyDocked )
        {
            var responder = CreateResponder();
            if ( previouslyDocked )
            {
                await Journal( responder, DockingData() );
            }
            responder.schemas.Add( new JournalSchema() );
            // Location fields from an observed on-foot CarrierJump; faction details omitted.
            await Journal( responder, OnFootCarrierJump() );

            var (schema, data, _, _) = responder.eddnSender.sentMessages.Single();
            Assert.AreEqual( "https://eddn.edcd.io/schemas/journal/1", schema );
            Assert.AreEqual( "CarrierJump", data["event"] );
            Assert.IsTrue( (bool?)data["OnFoot"]  );
            Assert.IsFalse( (bool?)data["Docked"]  );
            Assert.AreEqual( "Diaguandri", data["StarSystem"] );
            Assert.IsFalse( data.ContainsKey( "StationName" ) );
            Assert.IsFalse( data.ContainsKey( "MarketID" ) );
            Assert.IsTrue( responder.eddnState.Location.StarSystemLocationIsSet() );
            Assert.AreEqual( 670417429889UL, responder.eddnState.Location.systemAddress );
            Assert.AreEqual( -41.06250M, responder.eddnState.Location.systemX );
            Assert.AreEqual( -62.15625M, responder.eddnState.Location.systemY );
            Assert.AreEqual( -103.25000M, responder.eddnState.Location.systemZ );
            Assert.IsNull( responder.eddnState.Location.stationName );
            Assert.IsNull( responder.eddnState.Location.marketId );

            // Even a profile reporting the ship docked in the destination system cannot
            // establish a station identity that the commander's journal did not provide.
            var callback = Callback();
            callback.profileJson["lastSystem"]["name"] = "Diaguandri";
            Capi( responder, callback );
            Assert.HasCount( 1, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "FleetCarrier", false, false )]
        [DataRow( "FleetCarrier", false, true )]
        [DataRow( "FleetCarrier", true, false )]
        [DataRow( "FleetCarrier", true, true )]
        [DataRow( "SquadronCarrier", false, false )]
        [DataRow( "SquadronCarrier", false, true )]
        [DataRow( "SquadronCarrier", true, false )]
        [DataRow( "SquadronCarrier", true, true )]
        public async Task ObservedOnFootCarrierSequenceRetainsIdentityAndAllowsMatchingCapi ( string stationType, bool fromLoad, bool onPlanet )
        {
            var responder = CreateResponder();
            await Journal( responder, CarrierDocking( stationType ), fromLoad );
            Assert.IsTrue( responder.eddnState.Location.ShipDocked );
            Assert.IsTrue( responder.eddnState.Location.AboardCarrier );
            var disembark = CarrierDisembark();
            disembark["OnPlanet"] = onPlanet;
            await Journal( responder, disembark, fromLoad );
            Assert.IsFalse( responder.eddnState.Location.ShipDocked );
            Assert.IsTrue( responder.eddnState.Location.AboardCarrier );

            // CarrierLocation describes the carrier, not the commander. The observed
            // fleet-carrier update precedes CarrierJump by 51 seconds; another carrier
            // can also report a remote location while we remain aboard this one.
            var carrierLocations = new[]
            {
                new JObject
                {
                    ["event"] = "CarrierLocation", ["timestamp"] = "2026-11-09T03:32:10Z",
                    ["CarrierType"] = stationType, ["CarrierID"] = 3707594240L,
                    ["StarSystem"] = "Diaguandri", ["SystemAddress"] = 670417429889UL, ["BodyID"] = 22
                },
                new JObject
                {
                    ["event"] = "CarrierLocation", ["timestamp"] = "2026-11-09T03:32:11Z",
                    ["CarrierType"] = stationType == "FleetCarrier" ? "SquadronCarrier" : "FleetCarrier",
                    ["CarrierID"] = 3708725248L, ["StarSystem"] = "Eafots VB-W b15-1",
                    ["SystemAddress"] = 2791594531977UL, ["BodyID"] = 9
                }
            };
            foreach ( var carrierLocation in carrierLocations )
            {
                await Journal( responder, carrierLocation, fromLoad );
                Assert.AreEqual( "Bei Dou Sector IH-V b2-4", responder.eddnState.Location.systemName );
                Assert.AreEqual( 9466510910889UL, responder.eddnState.Location.systemAddress );
                Assert.AreEqual( "J9J-9KT", responder.eddnState.Location.stationName );
                Assert.AreEqual( 3707594240L, responder.eddnState.Location.marketId );
                Assert.IsTrue( responder.eddnState.Location.AboardCarrier );
                Assert.IsFalse( responder.eddnState.Location.ShipDocked );
            }
            responder.schemas.Add( new JournalSchema() );
            await Journal( responder, OnFootCarrierJump(), fromLoad );

            var cached = responder.eddnState.Location;
            Assert.IsTrue( cached.AboardCarrier );
            Assert.IsFalse( cached.ShipDocked );
            Assert.AreEqual( "J9J-9KT", cached.stationName );
            Assert.AreEqual( 3707594240L, cached.marketId );
            Assert.AreEqual( "Diaguandri", cached.systemName );
            Assert.AreEqual( -41.06250M, cached.systemX );
            if ( fromLoad ) { Assert.IsEmpty( responder.eddnSender.sentMessages ); }
            else
            {
                var (schema, data, eddnState, gameVersionOverride) = responder.eddnSender.sentMessages.Single();
                Assert.AreEqual( "CarrierJump", data["event"] );
                Assert.IsFalse( data.ContainsKey( "StationName" ) );
                Assert.IsFalse( data.ContainsKey( "MarketID" ) );
            }

            var stale = CarrierCallback();
            stale.profileJson["lastSystem"]["name"] = "Bei Dou Sector IH-V b2-4";
            Capi( responder, stale );
            Assert.HasCount( fromLoad ? 0 : 1, responder.eddnSender.sentMessages);
            Capi( responder, CarrierCallback() );
            var expected = new[]
            {
                "https://eddn.edcd.io/schemas/commodity/3", "https://eddn.edcd.io/schemas/outfitting/3",
                "https://eddn.edcd.io/schemas/shipyard/2", "https://eddn.edcd.io/schemas/fcmaterials_capi/1"
            };
            CollectionAssert.AreEqual( expected, responder.eddnSender.sentMessages.Where( m => m.gameVersionOverride != null ).Select( m => m.schema ).ToArray() );
        }

        [TestMethod]
        [DataRow( "FleetCarrier", "Undocked" )]
        [DataRow( "FleetCarrier", "StartJump" )]
        [DataRow( "FleetCarrier", "SupercruiseEntry" )]
        [DataRow( "FleetCarrier", "FSDJump" )]
        [DataRow( "FleetCarrier", "LoadGame" )]
        [DataRow( "FleetCarrier", "Shutdown" )]
        [DataRow( "FleetCarrier", "Operation" )]
        [DataRow( "SquadronCarrier", "Undocked" )]
        [DataRow( "SquadronCarrier", "StartJump" )]
        [DataRow( "SquadronCarrier", "SupercruiseEntry" )]
        [DataRow( "SquadronCarrier", "FSDJump" )]
        [DataRow( "SquadronCarrier", "LoadGame" )]
        [DataRow( "SquadronCarrier", "Shutdown" )]
        [DataRow( "SquadronCarrier", "Operation" )]
        public async Task CarrierContinuityEndsOnDepartureSessionOrInstanceChange ( string stationType, string departure )
        {
            var responder = CreateResponder();
            await Journal( responder, CarrierDocking( stationType ) );
            await Journal( responder, CarrierDisembark() );
            var data = new JObject { ["event"] = departure };
            if ( departure == "Operation" )
            {
                data["event"] = "GameModeChange";
                data["GameMode"] = "Operation";
            }
            await Journal( responder, data );
            await Journal( responder, OnFootCarrierJump() );
            Assert.IsFalse( responder.eddnState.Location.AboardCarrier );
            Assert.IsNull( responder.eddnState.Location.marketId );
            Capi( responder, CarrierCallback() );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "FleetCarrier", "Taxi", false )]
        [DataRow( "FleetCarrier", "Taxi", true )]
        [DataRow( "FleetCarrier", "SRV", false )]
        [DataRow( "FleetCarrier", "SRV", true )]
        [DataRow( "FleetCarrier", "Multicrew", false )]
        [DataRow( "FleetCarrier", "Multicrew", true )]
        [DataRow( "SquadronCarrier", "Taxi", false )]
        [DataRow( "SquadronCarrier", "Taxi", true )]
        [DataRow( "SquadronCarrier", "SRV", false )]
        [DataRow( "SquadronCarrier", "SRV", true )]
        [DataRow( "SquadronCarrier", "Multicrew", false )]
        [DataRow( "SquadronCarrier", "Multicrew", true )]
        public async Task BoardingOtherVesselPreservesCarrierWithoutClaimingDockedOrOnFoot ( string stationType, string vessel, bool withStationIdentity )
        {
            var responder = CreateResponder();
            await Journal( responder, CarrierDocking( stationType ) );
            await Journal( responder, CarrierDisembark() );
            await Journal( responder, OnFootCarrierJump() );
            var embark = new JObject
            {
                ["event"] = "Embark", ["SRV"] = false, ["Taxi"] = false, ["Multicrew"] = false
            };
            embark[vessel] = true;
            if ( withStationIdentity )
            {
                embark["OnStation"] = true;
                embark["StationName"] = "J9J-9KT";
                embark["StationType"] = stationType;
                embark["MarketID"] = 3707594240L;
            }
            await Journal( responder, embark );

            var cached = responder.eddnState.Location;
            Assert.IsTrue( cached.AboardCarrier );
            Assert.IsFalse( cached.ShipDocked );
            Assert.IsFalse( cached.OnFootAtCarrier );
            Assert.AreEqual( "J9J-9KT", cached.stationName );
            Assert.AreEqual( 3707594240L, cached.marketId );
            Capi( responder, CarrierCallback() ); // A lagging on-foot profile must not qualify.
            Capi( responder, CarrierCallback( false ) ); // Nor does the parked own ship.
            Assert.IsEmpty( responder.eddnSender.sentMessages );

            // Being carried to another system is not departure from the carrier, even if
            // Docked describes the boarded vessel rather than the commander's own ship.
            foreach ( var docked in new[] { false, true } )
            {
                var jump = OnFootCarrierJump();
                jump["OnFoot"] = false;
                jump["Docked"] = docked;
                await Journal( responder, jump );
                Assert.IsTrue( cached.AboardCarrier );
                Assert.IsFalse( cached.ShipDocked );
                Assert.IsFalse( cached.OnFootAtCarrier );
                Assert.AreEqual( 3707594240L, cached.marketId );
                Capi( responder, CarrierCallback( false ) );
                Assert.IsEmpty( responder.eddnSender.sentMessages );
            }

            // Exiting the vessel restores on-foot posture without requiring a new Docked.
            embark["event"] = "Disembark";
            await Journal( responder, embark );
            Assert.IsTrue( cached.AboardCarrier );
            Assert.IsFalse( cached.ShipDocked );
            Assert.IsTrue( cached.OnFootAtCarrier );
            Capi( responder, CarrierCallback() );
            Assert.HasCount( 4, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "FleetCarrier", "Taxi" )]
        [DataRow( "FleetCarrier", "SRV" )]
        [DataRow( "FleetCarrier", "Multicrew" )]
        [DataRow( "SquadronCarrier", "Taxi" )]
        [DataRow( "SquadronCarrier", "SRV" )]
        [DataRow( "SquadronCarrier", "Multicrew" )]
        public async Task ConfirmedDepartureAfterBoardingClearsCarrierAndCapiEligibility ( string stationType, string vessel )
        {
            var responder = CreateResponder();
            await Journal( responder, CarrierDocking( stationType ) );
            await Journal( responder, CarrierDisembark() );
            await Journal( responder, new JObject { ["event"] = "Embark", [vessel] = true } );
            Assert.IsTrue( responder.eddnState.Location.AboardCarrier );
            await Journal( responder, new JObject { ["event"] = "Undocked" } );
            Assert.IsFalse( responder.eddnState.Location.AboardCarrier );
            Assert.IsFalse( responder.eddnState.Location.ShipDocked );
            Assert.IsFalse( responder.eddnState.Location.OnFootAtCarrier );
            Assert.IsNull( responder.eddnState.Location.marketId );
            Assert.IsNull( responder.eddnState.Location.stationName );

            await Journal( responder, OnFootCarrierJump() );
            Capi( responder, CarrierCallback() );
            Capi( responder, CarrierCallback( false ) );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
            Assert.IsFalse( responder.eddnState.Location.AboardCarrier );
        }

        [TestMethod]
        [DataRow( "FleetCarrier", "StationName" )]
        [DataRow( "FleetCarrier", "MarketID" )]
        [DataRow( "SquadronCarrier", "StationName" )]
        [DataRow( "SquadronCarrier", "MarketID" )]
        public async Task ConflictingPartialIdentityDoesNotRetainCarrier ( string stationType, string field )
        {
            var responder = CreateResponder();
            await Journal( responder, CarrierDocking( stationType ) );
            await Journal( responder, CarrierDisembark() );
            var jump = OnFootCarrierJump();
            if ( field == "StationName" ) { jump[field] = "Different carrier"; }
            else { jump[field] = 3709999999L; }
            await Journal( responder, jump );
            Assert.IsFalse( responder.eddnState.Location.AboardCarrier );
            Assert.IsNull( responder.eddnState.Location.marketId );
            Capi( responder, CarrierCallback() );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "FleetCarrier" )]
        [DataRow( "SquadronCarrier" )]
        public async Task OnFootCarrierRequiresProfilePresenceAndStillChecksEndpointIdentity ( string stationType )
        {
            var responder = CreateResponder();
            await Journal( responder, CarrierDocking( stationType ) );
            await Journal( responder, CarrierDisembark() );
            await Journal( responder, OnFootCarrierJump() );
            Capi( responder, CarrierCallback( false ) );
            var missingPresence = CarrierCallback();
            ( (JObject)missingPresence.profileJson["commander"] ).Remove( "onfoot" );
            Capi( responder, missingPresence );
            var wrongMarket = CarrierCallback();
            wrongMarket.marketJson["id"] = 3709999999L;
            Capi( responder, wrongMarket );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
            Capi( responder, CarrierCallback() );
            Assert.HasCount( 4, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "FleetCarrier" )]
        [DataRow( "SquadronCarrier" )]
        public async Task EmbarkingOwnShipAtCarrierRestoresDockedEligibility ( string stationType )
        {
            var responder = CreateResponder();
            await Journal( responder, CarrierDocking( stationType ) );
            await Journal( responder, CarrierDisembark() );
            await Journal( responder, OnFootCarrierJump() );
            await Journal( responder, JObject.Parse( """{"event":"Embark","SRV":false,"Taxi":false,"Multicrew":false}""" ) );
            Assert.IsTrue( responder.eddnState.Location.ShipDocked );
            Assert.IsTrue( responder.eddnState.Location.AboardCarrier );
            Capi( responder, CarrierCallback() );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
            Capi( responder, CarrierCallback( false ) );
            Assert.HasCount( 4, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "FleetCarrier", "Location" )]
        [DataRow( "FleetCarrier", "Disembark" )]
        [DataRow( "SquadronCarrier", "Location" )]
        [DataRow( "SquadronCarrier", "Disembark" )]
        [DataRow( "fLeEtCaRrIeR", "Location" )]
        [DataRow( "sQuAdRoNcArRiEr", "Location" )]
        public async Task ExplicitOnFootCarrierIdentityCanEstablishPresence ( string stationType, string edType )
        {
            var responder = CreateResponder();
            var data = CarrierDocking( stationType );
            data["event"] = edType;
            data["Docked"] = false;
            data["OnFoot"] = true;
            data["OnStation"] = true;
            data["StarSystem"] = "Diaguandri";
            data["SystemAddress"] = 670417429889UL;
            await Journal( responder, data );
            Assert.IsTrue( responder.eddnState.Location.AboardCarrier );
            Assert.IsFalse( responder.eddnState.Location.ShipDocked );
            Capi( responder, CarrierCallback() );
            Assert.HasCount( 4, responder.eddnSender.sentMessages );
        }

        [TestMethod]
        public async Task TelepresenceRejectsCapiWithoutPoisoningSuppressionState ()
        {
            var responder = CreateResponder();
            await Journal( responder, DockingData() );
            var previous = EDDI.Instance.GameState.inTelepresence;
            try
            {
                EDDI.Instance.GameStateMutator.inTelepresence = true;
                Capi( responder, Callback() );
                Assert.IsEmpty( responder.eddnSender.sentMessages );
                EDDI.Instance.GameStateMutator.inTelepresence = false;
                Capi( responder, Callback() );
                Assert.HasCount( 3, responder.eddnSender.sentMessages );
            }
            finally
            {
                EDDI.Instance.GameStateMutator.inTelepresence = previous;
            }
        }
    }
}
