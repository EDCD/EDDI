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
using System.Text;
using System.Threading.Tasks;
using Tests.Properties;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class EddnBodyEnrichmentTests : TestBase
    {
        private static readonly DateTime eventTime = new( 2026, 11, 9, 3, 33, 1, DateTimeKind.Utc );

        [TestInitialize]
        public void Start ()
        {
            MakeSafe();
            EDDI.Instance.DataProvider = CreateTestDataProvider();
            FakeSpanshHttpClient.Expect( "system/10477373803", Encoding.UTF8.GetString( Resources.SpanshQuickStarSystemSol ) );
        }

        private static IDictionary<string, object> Data ( JObject json ) => Deserializtion.DeserializeData( json.ToString() );

        private static JObject Location ( string edType = "Location", int seconds = -120 ) => new()
        {
            ["event"] = edType, ["timestamp"] = eventTime.AddSeconds( seconds ),
            ["StarSystem"] = "Sol", ["SystemAddress"] = 10477373803UL, ["StarPos"] = new JArray( 0, 0, 0 ),
            ["Docked"] = false, ["Body"] = "Earth", ["BodyID"] = 3, ["BodyType"] = "Planet"
        };

        private static EDDNResponder CreateResponder ( bool withBody = true )
        {
            var responder = new EDDNResponder();
            responder.SetUnitTesting( true );
            responder.schemas.AddRange( [new CodexEntrySchema(), new ScanOrganicSchema()] );
            var location = Location();
            if ( !withBody ) { location.Remove( "Body" ); location.Remove( "BodyID" ); }
            responder.eddnState.GetStateInfo( "Location", Data( location ) );
            return responder;
        }

        private static Status BodyStatus ( string body = "Earth", int seconds = 0, bool onFoot = true ) =>
            new( flags2: onFoot ? Status.Flags2.OnFootOnPlanet : Status.Flags2.None )
            {
                timestamp = eventTime.AddSeconds( seconds ), bodyname = body, latitude = 12.5M, longitude = -34.5M
            };

        private static JObject BodyEvent ( string edType = "CodexEntry" )
        {
            var data = new JObject
            {
                ["event"] = edType, ["timestamp"] = eventTime, ["SystemAddress"] = 10477373803UL
            };
            if ( edType == "ScanOrganic" )
            {
                data["ScanType"] = "Log";
                data["Body"] = 3;
                data["Genus"] = "$Codex_Ent_Bacterial_Genus_Name;";
                data["Species"] = "$Codex_Ent_Bacterial_01_Name;";
            }
            else
            {
                data["System"] = "Sol";
                data["EntryID"] = 1200601;
                data["Name"] = "$Codex_Ent_Standard_Sudarsky_Class_II_Name;";
            }
            return data;
        }

        private static Task Journal ( EDDNResponder responder, JObject json ) =>
            responder.HandleAsync( new CockpitBreachedEvent( eventTime ) { raw = json.ToString() } );

        [TestMethod]
        public void ApproachBodyContinuesToReadBodyField ()
        {
            var responder = CreateResponder();
            var raw = """
                {"timestamp":"2026-10-14T09:30:04Z","event":"ApproachBody","StarSystem":"Bei Dou Sector IH-V b2-4",
                 "SystemAddress":9466510910889,"Body":"Bei Dou Sector IH-V b2-4 2","BodyID":3}
                """;
            responder.eddnState.GetStateInfo( "ApproachBody", Deserializtion.DeserializeData( raw ) );
            Assert.AreEqual( "Bei Dou Sector IH-V b2-4 2", responder.eddnState.Location.journalBodyName );
            Assert.AreEqual( 3, responder.eddnState.Location.journalBodyId );
        }

        [TestMethod]
        [DataRow( "CodexEntry" )]
        [DataRow( "ScanOrganic" )]
        public async Task MatchingCurrentBodyEnrichesAndSubmits ( string edType )
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            await Journal( responder, BodyEvent( edType ) );
            var (schema, data, _, _) = responder.eddnSender.sentMessages.Single();
            Assert.AreEqual( edType == "CodexEntry" ? "https://eddn.edcd.io/schemas/codexentry/1" : "https://eddn.edcd.io/schemas/scanorganic/1", schema );
            Assert.AreEqual( "Earth", data["BodyName"] );
            Assert.AreEqual( 3, Convert.ToInt32( data["BodyID"] ) );
            if ( edType == "ScanOrganic" )
            {
                Assert.IsFalse( data.ContainsKey( "Body" ) );
                Assert.AreEqual( 12.5M, data["Latitude"] );
                Assert.AreEqual( -34.5M, data["Longitude"] );
            }
        }

        [TestMethod]
        [DataRow( "CodexEntry", -61, false )]
        [DataRow( "CodexEntry", -60, true )]
        [DataRow( "CodexEntry", 0, true )]
        [DataRow( "CodexEntry", 60, true )]
        [DataRow( "CodexEntry", 61, false )]
        [DataRow( "ScanOrganic", -61, false )]
        [DataRow( "ScanOrganic", -60, true )]
        [DataRow( "ScanOrganic", 0, true )]
        [DataRow( "ScanOrganic", 60, true )]
        [DataRow( "ScanOrganic", 61, false )]
        public async Task StatusMustBeWithinSixtySecondsWithoutDroppingValidEvent ( string edType, int seconds, bool enrich )
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus( seconds: seconds ) );
            await Journal( responder, BodyEvent( edType ) );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( enrich, message.ContainsKey( "BodyName" ) );
            Assert.AreEqual( enrich || edType == "ScanOrganic", message.ContainsKey( "BodyID" ) );
            Assert.AreEqual( enrich && edType == "ScanOrganic", message.ContainsKey( "Latitude" ) );
        }

        [TestMethod]
        public async Task BinaryBodyMismatchAllowsStatusNameWithoutInventingId ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus( "Earth's companion" ) );
            await Journal( responder, BodyEvent() );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( "Earth's companion", message["BodyName"] );
            Assert.IsFalse( message.ContainsKey( "BodyID" ) );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public async Task UnavailableJournalBodyIdIsOmitted ( bool matchingName )
        {
            var responder = CreateResponder( false );
            if ( matchingName )
            {
                var approach = Location( "ApproachBody" );
                approach.Remove( "BodyID" );
                await Journal( responder, approach );
            }
            await responder.HandleStatusAsync( BodyStatus() );
            await Journal( responder, BodyEvent() );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( "Earth", message["BodyName"] );
            Assert.IsFalse( message.ContainsKey( "BodyID" ) );
        }

        [TestMethod]
        [DataRow( "CodexEntry" )]
        [DataRow( "ScanOrganic" )]
        public async Task NativeBodyIdIsPreservedWithoutConflictingEnrichment ( string edType )
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            var data = BodyEvent( edType );
            data[edType == "ScanOrganic" ? "Body" : "BodyID"] = 16;
            await Journal( responder, data );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( 16, Convert.ToInt32( message["BodyID"] ) );
            Assert.IsFalse( message.ContainsKey( "BodyName" ) );
            Assert.IsFalse( message.ContainsKey( "Latitude" ) );
            Assert.IsFalse( message.ContainsKey( "Longitude" ) );
        }

        [TestMethod]
        public async Task MissingJournalBodyDoesNotThrowOrDropOrganicSubmission ()
        {
            var responder = CreateResponder( false );
            await responder.HandleStatusAsync( BodyStatus() );
            await Journal( responder, BodyEvent( "ScanOrganic" ) );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( 3, Convert.ToInt32( message["BodyID"] ) );
            Assert.IsFalse( message.ContainsKey( "BodyName" ) );
            Assert.IsFalse( message.ContainsKey( "Latitude" ) );
        }

        [TestMethod]
        [DataRow( "CodexEntry", "SystemAddress" )]
        [DataRow( "CodexEntry", "System" )]
        [DataRow( "ScanOrganic", "SystemAddress" )]
        [DataRow( "ScanOrganic", "StarSystem" )]
        public async Task ConflictingSystemCannotBorrowMandatoryCoordinates ( string edType, string field )
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            var data = BodyEvent( edType );
            if ( field == "SystemAddress" ) { data[field] = 3932277478106UL; }
            else { data[field] = "Another system"; }
            await Journal( responder, data );
            Assert.IsEmpty( responder.eddnSender.sentMessages );
        }

        [TestMethod]
        [DataRow( "Latitude" )]
        [DataRow( "Longitude" )]
        public async Task ExistingCoordinateIsPreservedWithoutFillingItsPartner ( string field )
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            var data = BodyEvent( "ScanOrganic" );
            data[field] = 55M;
            await Journal( responder, data );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( 55M, Convert.ToDecimal( message[field] ) );
            Assert.IsFalse( message.ContainsKey( field == "Latitude" ? "Longitude" : "Latitude" ) );
        }

        [TestMethod]
        public async Task CoordinatesRequireOnFootSurfaceStatus ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus( onFoot: false ) );
            await Journal( responder, BodyEvent( "ScanOrganic" ) );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( "Earth", message["BodyName"] );
            Assert.IsFalse( message.ContainsKey( "Latitude" ) );
            Assert.IsFalse( message.ContainsKey( "Longitude" ) );
        }

        [TestMethod]
        [DataRow( "FSDJump" )]
        [DataRow( "CarrierJump" )]
        [DataRow( "Location" )]
        [DataRow( "partial-system" )]
        [DataRow( "LoadGame" )]
        [DataRow( "Shutdown" )]
        [DataRow( "LeaveBody" )]
        [DataRow( "Operation" )]
        public async Task LocationSessionAndBodyBoundariesInvalidateObservations ( string transition )
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            JObject data;
            if ( transition is "FSDJump" or "CarrierJump" or "Location" )
            {
                data = Location( transition, 1 );
                data.Remove( "Body" );
                data.Remove( "BodyID" );
            }
            else if ( transition == "partial-system" )
            {
                data = new JObject
                {
                    ["event"] = "Scan", ["timestamp"] = eventTime.AddSeconds( 1 ),
                    ["StarSystem"] = "Other system", ["SystemAddress"] = 3932277478106UL
                };
            }
            else
            {
                data = new JObject { ["event"] = transition, ["timestamp"] = eventTime.AddSeconds( 1 ) };
                if ( transition == "Operation" )
                {
                    data["event"] = "GameModeChange";
                    data["GameMode"] = "Operation";
                }
            }
            await Journal( responder, data );
            var cached = responder.eddnState.Location;
            Assert.IsNull( cached.journalBodyName );
            Assert.IsNull( cached.journalBodyId );
            Assert.IsNull( cached.statusBodyName );
            Assert.IsNull( cached.statusTimeStamp );

            // An old status arriving after the boundary must not get relabelled as current.
            await responder.HandleStatusAsync( BodyStatus() );
            var entry = BodyEvent();
            entry["timestamp"] = eventTime.AddSeconds( 2 );
            var message = Data( entry );
            cached.AugmentBodyNameID( message );
            cached.AugmentBodyLatLong( message );
            Assert.IsFalse( message.ContainsKey( "BodyName" ) );
            Assert.IsFalse( message.ContainsKey( "BodyID" ) );
            Assert.IsFalse( message.ContainsKey( "Latitude" ) );
        }

        [TestMethod]
        public async Task LateStatusFromPreviousSystemCannotBeReboundToNewSystem ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            var jump = Location( "FSDJump", 1 );
            jump["StarSystem"] = "Other system";
            jump["SystemAddress"] = 3932277478106UL;
            await Journal( responder, jump );
            await responder.HandleStatusAsync( BodyStatus() );
            var entry = BodyEvent();
            entry["System"] = "Other system";
            entry["SystemAddress"] = 3932277478106UL;
            entry["timestamp"] = eventTime.AddSeconds( 2 );
            var message = Data( entry );
            responder.eddnState.Location.AugmentBodyNameID( message );
            Assert.IsFalse( message.ContainsKey( "BodyName" ) );
        }

        [TestMethod]
        public async Task SessionResetRequiresFullLocationBeforeBodyEnrichmentCanResume ()
        {
            var responder = CreateResponder();
            await Journal( responder, new JObject { ["event"] = "LoadGame", ["timestamp"] = eventTime } );
            await Journal( responder, new JObject { ["event"] = "LeaveBody", ["timestamp"] = eventTime.AddSeconds( 1 ) } );
            await Journal( responder, Location( "ApproachBody", 1 ) );
            await responder.HandleStatusAsync( BodyStatus( seconds: 2 ) );
            var entry = BodyEvent();
            entry["timestamp"] = eventTime.AddSeconds( 2 );
            await Journal( responder, entry );
            Assert.IsFalse( responder.eddnSender.sentMessages.Single().data.ContainsKey( "BodyName" ) );

            await Journal( responder, Location( seconds: 3 ) );
            await responder.HandleStatusAsync( BodyStatus( seconds: 4 ) );
            entry["timestamp"] = eventTime.AddSeconds( 4 );
            await Journal( responder, entry );
            Assert.HasCount( 2, responder.eddnSender.sentMessages );
            Assert.AreEqual( "Earth", responder.eddnSender.sentMessages.Last().data["BodyName"] );
        }

        [TestMethod]
        public async Task OlderStatusDoesNotReplaceNewerBodyObservation ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            await responder.HandleStatusAsync( BodyStatus( "Stale body", -1 ) );
            await Journal( responder, BodyEvent() );
            Assert.AreEqual( "Earth", responder.eddnSender.sentMessages.Single().data["BodyName"] );
        }

        [TestMethod]
        public async Task MissingStatusBodyClearsOptionalEnrichment ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            await responder.HandleStatusAsync( BodyStatus( null, 1 ) );
            var entry = BodyEvent();
            entry["timestamp"] = eventTime.AddSeconds( 1 );
            await Journal( responder, entry );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.IsFalse( message.ContainsKey( "BodyName" ) );
            Assert.IsFalse( message.ContainsKey( "BodyID" ) );
        }

        [TestMethod]
        public async Task SupercruiseEntryDoesNotEraseSameBodyEvidence ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            await Journal( responder, new JObject { ["event"] = "SupercruiseEntry", ["timestamp"] = eventTime } );
            await Journal( responder, BodyEvent() );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( "Earth", message["BodyName"] );
            Assert.AreEqual( 3, Convert.ToInt32( message["BodyID"] ) );
        }

        [TestMethod]
        public async Task NamedBodyDoesNotNeedSystemNamePrefix ()
        {
            var responder = CreateResponder();
            var approach = Location( "ApproachBody" );
            approach["Body"] = "New Europa";
            approach["BodyID"] = 17;
            await Journal( responder, approach );
            await responder.HandleStatusAsync( BodyStatus( "New Europa" ) );
            await Journal( responder, BodyEvent() );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( "New Europa", message["BodyName"] );
            Assert.AreEqual( 17, Convert.ToInt32( message["BodyID"] ) );
        }

        [TestMethod]
        public async Task ExistingBodyNameCanReceiveMatchingIdWithoutBeingOverwritten ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            var entry = BodyEvent();
            entry["BodyName"] = "Earth";
            await Journal( responder, entry );
            Assert.AreEqual( 3, Convert.ToInt32( responder.eddnSender.sentMessages.Single().data["BodyID"] ) );
        }

        [TestMethod]
        public async Task ExplicitBodyAndCoordinatesRemainUnchanged ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            var entry = BodyEvent();
            entry["BodyName"] = "Another named body";
            entry["BodyID"] = 16;
            entry["Latitude"] = 45M;
            entry["Longitude"] = 55M;
            await Journal( responder, entry );
            var message = responder.eddnSender.sentMessages.Single().data;
            Assert.AreEqual( "Another named body", message["BodyName"] );
            Assert.AreEqual( 16, Convert.ToInt32( message["BodyID"] ) );
            Assert.AreEqual( 45M, Convert.ToDecimal( message["Latitude"] ) );
            Assert.AreEqual( 55M, Convert.ToDecimal( message["Longitude"] ) );
        }

        [TestMethod]
        public async Task StationNameIsNotUsedAsOptionalBodyName ()
        {
            var responder = CreateResponder();
            var docked = Location( "Docked" );
            docked["StationName"] = "Baade Refinery";
            docked["MarketID"] = 128000001L;
            await Journal( responder, docked );
            await responder.HandleStatusAsync( BodyStatus( "Baade Refinery" ) );
            await Journal( responder, BodyEvent() );
            Assert.IsFalse( responder.eddnSender.sentMessages.Single().data.ContainsKey( "BodyName" ) );
        }

        [TestMethod]
        public async Task MissingEventTimestampDoesNotThrowDuringOptionalEnrichment ()
        {
            var responder = CreateResponder();
            await responder.HandleStatusAsync( BodyStatus() );
            var entry = BodyEvent();
            entry.Remove( "timestamp" );
            var message = Data( entry );
            responder.eddnState.Location.AugmentBodyNameID( message );
            responder.eddnState.Location.AugmentBodyLatLong( message );
            Assert.IsFalse( message.ContainsKey( "BodyName" ) );
            Assert.IsFalse( message.ContainsKey( "Latitude" ) );
        }
    }
}
