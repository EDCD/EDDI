using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiSpeechResponder;
using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using Tests.Properties;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( nameof( IntegrationTests ) )]
    public class IntegrationTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod, DoNotParallelize]
        public void TestKeepAlive()
        {
            var monitor = EDDI.Instance.monitors.FirstOrDefault(m => m.MonitorName() == "Journal monitor");

            Assert.IsNotNull(monitor);
            EDDI.Instance.EnableMonitor( monitor.MonitorName() );
            monitor.Stop();
            Assert.AreEqual( 1, EDDI.Instance.activeMonitors.Count );

            EDDI.Instance.DisableMonitor( monitor.MonitorName() );
            Assert.AreEqual( 0, EDDI.Instance.activeMonitors.Count );

            EDDI.Instance.EnableMonitor( monitor.MonitorName() );
            monitor.Stop();

            Thread.Sleep(3000);
            Assert.AreEqual( 1, EDDI.Instance.activeMonitors.Count );

            Thread.Sleep(3000);
            Assert.AreEqual( 1, EDDI.Instance.activeMonitors.Count );

            Thread.Sleep(3000);
            Assert.AreEqual( 1, EDDI.Instance.activeMonitors.Count );

            Thread.Sleep(3000);
            Assert.AreEqual( 1, EDDI.Instance.activeMonitors.Count );

            if ( EDDI.Instance.activeMonitors == null )
            {
                Assert.Fail();
            }
            else
            {
                EDDI.Instance.activeMonitors.TryTake( out var activeMonitor );
                Assert.AreEqual( monitor, activeMonitor );
            }
        }
    }
}

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class EddiCoreTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestResponders()
        {
            var numResponders = EDDI.Instance.findResponders().Count;
            Assert.IsTrue(numResponders > 0);
        }

        [TestMethod]
        public void TestMonitors()
        {
            var numMonitors = EDDI.Instance.findMonitors().Count;
            Assert.IsTrue(numMonitors > 0);
        }

        [TestMethod, DoNotParallelize]
        public void TestJumpedEventHandler()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "dump/33656303199641", Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpLHS_20 ) );
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=LHS 20", @"{""min_max"":[{""id64"":33656303199641,""name"":""LHS 20"",""x"":11.1875,""y"":-37.375,""z"":-31.84375},{""id64"":671491433881,""name"":""LHS 2026"",""x"":43.34375,""y"":23.5625,""z"":-41.09375},{""id64"":7267487524241,""name"":""LHS 2018"",""x"":-25.28125,""y"":41.8125,""z"":-53.5625},{""id64"":5069269509521,""name"":""LHS 2029"",""x"":27.40625,""y"":31.5,""z"":-45.625},{""id64"":27059501999521,""name"":""LHS 2037"",""x"":33.78125,""y"":7.25,""z"":-14.5},{""id64"":671222867361,""name"":""LHS 203"",""x"":16.46875,""y"":-15.1875,""z"":-20.34375},{""id64"":5369246010144,""name"":""LHS 206"",""x"":20.75,""y"":-18.21875,""z"":-55.9375},{""id64"":13864825660825,""name"":""LHS 2088"",""x"":-23.3125,""y"":25.71875,""z"":-28.34375},{""id64"":13865362466209,""name"":""LHS 2065"",""x"":19.71875,""y"":11.8125,""z"":-15.84375},{""id64"":13865630967201,""name"":""LHS 2069"",""x"":38.6875,""y"":16.0625,""z"":-22.34375},{""id64"":670417757585,""name"":""LHS 2038"",""x"":-34.625,""y"":45.375,""z"":-54.03125},{""id64"":671491499417,""name"":""LHS 2094"",""x"":35.0,""y"":35.0,""z"":-40.0},{""id64"":2870246319505,""name"":""LHS 2011"",""x"":25.875,""y"":36.34375,""z"":-57.53125},{""id64"":11666339276177,""name"":""LHS 2027"",""x"":26.0625,""y"":34.53125,""z"":-52.40625},{""id64"":358663393986,""name"":""LHS 2001"",""x"":-54.0625,""y"":55.21875,""z"":-67.25},{""id64"":5370319752024,""name"":""LHS 205"",""x"":34.03125,""y"":-22.0625,""z"":12.625},{""id64"":358730437330,""name"":""LHS 200"",""x"":10.84375,""y"":-12.71875,""z"":-22.96875},{""id64"":670149256593,""name"":""LHS 207"",""x"":-47.84375,""y"":28.65625,""z"":-45.8125},{""id64"":7268561135017,""name"":""LHS 2010"",""x"":37.0625,""y"":-2.5,""z"":-4.84375},{""id64"":2931055135075,""name"":""LHS 2022"",""x"":10.53125,""y"":40.25,""z"":-58.5625}],""values"":[""LHS 20"",""LHS 2026"",""LHS 2018"",""LHS 2029"",""LHS 2037"",""LHS 203"",""LHS 206"",""LHS 2088"",""LHS 2065"",""LHS 2069"",""LHS 2038"",""LHS 2094"",""LHS 2011"",""LHS 2027"",""LHS 2001"",""LHS 205"",""LHS 200"",""LHS 207"",""LHS 2010"",""LHS 2022""]}" );

            var line = "{ \"timestamp\":\"2018-12-25T20:07:06Z\", \"event\":\"FSDJump\", \"StarSystem\":\"LHS 20\", \"SystemAddress\":33656303199641, \"StarPos\":[11.18750,-37.37500,-31.84375], \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_HighTech;\", \"SystemEconomy_Localised\":\"High Tech\", \"SystemSecondEconomy\":\"$economy_Refinery;\", \"SystemSecondEconomy_Localised\":\"Refinery\", \"SystemGovernment\":\"$government_Democracy;\", \"SystemGovernment_Localised\":\"Democracy\", \"SystemSecurity\":\"$SYSTEM_SECURITY_medium;\", \"SystemSecurity_Localised\":\"Medium Security\", \"Population\":9500553, \"JumpDist\":20.361, \"FuelUsed\":3.065896, \"FuelLevel\":19.762932, \"Factions\":[ { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"\", \"MyReputation\":6.106290 }, { \"Name\":\"Shenetserii Confederation\", \"FactionState\":\"None\", \"Government\":\"Confederacy\", \"Influence\":0.127000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":18.809999, \"PendingStates\":[ { \"State\":\"War\", \"Trend\":0 } ] }, { \"Name\":\"LHS 20 Company\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.127000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":4.950000, \"PendingStates\":[ { \"State\":\"War\", \"Trend\":0 } ] }, { \"Name\":\"Traditional LHS 20 Defence Party\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.087000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":2.640000 }, { \"Name\":\"Movement for LHS 20 Liberals\", \"FactionState\":\"CivilWar\", \"Government\":\"Democracy\", \"Influence\":0.226000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"SquadronFaction\":true, \"HomeSystem\":true, \"MyReputation\":100.000000, \"ActiveStates\":[ { \"State\":\"CivilLiberty\" }, { \"State\":\"Investment\" }, { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Nationalists of LHS 20\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.105000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000 }, { \"Name\":\"LHS 20 Organisation\", \"FactionState\":\"CivilWar\", \"Government\":\"Anarchy\", \"Influence\":0.166000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":5.940000, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"LHS 20 Engineers\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.162000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":15.000000 } ], \"SystemFaction\":{ \"Name\":\"Movement for LHS 20 Liberals\", \"FactionState\":\"CivilWar\" } }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (JumpedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(JumpedEvent));

            var result = EDDI.Instance.eventJumped( @event );

            Assert.IsTrue(result);
        }

        [TestMethod, DoNotParallelize]
        public void TestJumpedHandler_Hyperdiction()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "dump/2868635641225", Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpCephei_Sector_DQ_Y ) );
            fakeSpanshRestClient.Expect( "dump/560216410467", Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpHP_8525 ) );
            fakeSpanshRestClient.Expect( "dump/33656303199641", Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpLHS_20 ) );

            var line1 = @"{ ""timestamp"":""2024-02-20T11:10:24Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""Cephei Sector DQ-Y b1"", ""SystemAddress"":2868635641225, ""StarPos"":[-93.31250,31.00000,-73.00000], ""SystemAllegiance"":""Thargoid"", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""Нет"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""Нет"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""Нет"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Анархия"", ""Population"":0, ""Body"":""Cephei Sector DQ-Y b1 A"", ""BodyID"":1, ""BodyType"":""Star"", ""ThargoidWar"":{ ""CurrentState"":""Thargoid_Controlled"", ""NextStateSuccess"":"""", ""NextStateFailure"":""Thargoid_Controlled"", ""SuccessStateReached"":false, ""WarProgress"":0.000224, ""RemainingPorts"":0, ""EstimatedRemainingTime"":""0 Days"" }, ""JumpDist"":6.076, ""FuelUsed"":0.359144, ""FuelLevel"":31.640856 }";
            var event1 = (JumpedEvent)JournalMonitor.ParseJournalEntry(line1)[0];
            Assert.IsNotNull( event1 );
            Assert.IsInstanceOfType( event1, typeof( JumpedEvent ) );

            var line2 = @"{ ""timestamp"":""2024-02-20T11:11:12Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""HIP 8525"", ""SystemAddress"":560216410467, ""StarPos"":[-96.28125,31.65625,-71.25000], ""SystemAllegiance"":""Thargoid"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""Высокие технологии"", ""SystemSecondEconomy"":""$economy_Military;"", ""SystemSecondEconomy_Localised"":""Военная"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""Нет"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Анархия"", ""Population"":0, ""Body"":""HIP 8525 A"", ""BodyID"":1, ""BodyType"":""Star"", ""ThargoidWar"":{ ""CurrentState"":""Thargoid_Controlled"", ""NextStateSuccess"":""Thargoid_Recovery"", ""NextStateFailure"":""Thargoid_Controlled"", ""SuccessStateReached"":false, ""WarProgress"":0.006071, ""RemainingPorts"":0, ""EstimatedRemainingTime"":""0 Days"" }, ""JumpDist"":3.508, ""FuelUsed"":0.086031, ""FuelLevel"":31.554825, ""SystemFaction"":{ ""Name"":""None"" } }";
            var event2 = (JumpedEvent)JournalMonitor.ParseJournalEntry(line2)[0];
            Assert.IsNotNull( event2 );
            Assert.IsInstanceOfType( event2, typeof( JumpedEvent ) );

            var line3 = @"{ ""timestamp"":""2024-02-20T11:12:23Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""HIP 8525"", ""SystemAddress"":560216410467, ""StarPos"":[-96.28125,31.65625,-71.25000], ""SystemAllegiance"":""Thargoid"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""Высокие технологии"", ""SystemSecondEconomy"":""$economy_Military;"", ""SystemSecondEconomy_Localised"":""Военная"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""Нет"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Анархия"", ""Population"":0, ""Body"":""HIP 8525 ABC"", ""BodyID"":0, ""BodyType"":""Null"", ""ThargoidWar"":{ ""CurrentState"":""Thargoid_Controlled"", ""NextStateSuccess"":""Thargoid_Recovery"", ""NextStateFailure"":""Thargoid_Controlled"", ""SuccessStateReached"":false, ""WarProgress"":0.006071, ""RemainingPorts"":0, ""EstimatedRemainingTime"":""0 Days"" }, ""JumpDist"":3.508, ""FuelUsed"":0.086017, ""FuelLevel"":31.468807, ""SystemFaction"":{ ""Name"":""None"" } }";
            var event3 = (JumpedEvent)JournalMonitor.ParseJournalEntry(line3)[0];
            Assert.IsNotNull( event3 );
            Assert.IsInstanceOfType( event3, typeof( JumpedEvent ) );

            var line3a = @"{ ""timestamp"":""2020-11-20T02:15:00Z"", ""event"":""StartJump"", ""JumpType"":""Hyperspace"", ""StarSystem"":""LHS 20"", ""SystemAddress"":33656303199641, ""StarClass"":""M"" }";
            var event3a = (FSDEngagedEvent)JournalMonitor.ParseJournalEntry(line3a)[0];
            Assert.IsNotNull( event3a );
            Assert.IsInstanceOfType( event3a, typeof( FSDEngagedEvent ) );

            // Standard jump to Cephei Sector DQ-Y b1. Environment is supercruise.
            EDDI.Instance.eventJumped( @event1 );
            Assert.AreEqual( Constants.ENVIRONMENT_SUPERCRUISE, EDDI.Instance.Environment );
            Assert.IsNotNull( EDDI.Instance.CurrentStarSystem );
            Assert.AreEqual( 2868635641225UL, EDDI.Instance.CurrentStarSystem.systemAddress );

            // Standard jump to HIP 8525. Environment is supercruise.
            EDDI.Instance.eventJumped( @event2 );
            Assert.AreEqual( Constants.ENVIRONMENT_SUPERCRUISE, EDDI.Instance.Environment );
            Assert.IsNotNull( EDDI.Instance.CurrentStarSystem );
            Assert.AreEqual( 560216410467UL, EDDI.Instance.CurrentStarSystem.systemAddress );

            // Hyperdiction in HIP 8525. Environment is normal space rather than supercruise.
            EDDI.Instance.eventFSDEngaged( @event3a );
            EDDI.Instance.eventJumped( @event3 );
            Assert.AreEqual( Constants.ENVIRONMENT_NORMAL_SPACE, EDDI.Instance.Environment );
            Assert.IsNotNull( EDDI.Instance.CurrentStarSystem );
            Assert.AreEqual( 560216410467UL, EDDI.Instance.CurrentStarSystem.systemAddress );
        }

        [TestMethod, DoNotParallelize]
        public void TestLocationEventHandler()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "dump/5856221467362", Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpEravate ) );
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=Eravate", @"{""min_max"":[{""id64"":5856221467362,""name"":""Eravate"",""x"":-42.4375,""y"":-3.15625,""z"":59.65625}],""values"":[""Eravate""]}" );

            var line = "{ \"timestamp\":\"2018-12-27T08:05:23Z\", \"event\":\"Location\", \"Docked\":true, \"MarketID\":3230448384, \"StationName\":\"Cleve Hub\", \"StationType\":\"Orbis\", \"StarSystem\":\"Eravate\", \"SystemAddress\":5856221467362, \"StarPos\":[-42.43750,-3.15625,59.65625], \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemSecondEconomy\":\"$economy_Industrial;\", \"SystemSecondEconomy_Localised\":\"Industrial\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":740380179, \"Body\":\"Cleve Hub\", \"BodyID\":48, \"BodyType\":\"Station\", \"Powers\":[ \"Zachary Hudson\" ], \"PowerplayState\":\"Exploited\", \"Factions\":[ { \"Name\":\"Eravate School of Commerce\", \"FactionState\":\"None\", \"Government\":\"Cooperative\", \"Influence\":0.086913, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":91.840103 }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":42.790199 }, { \"Name\":\"Independent Eravate Free\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.123876, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Eravate Network\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.036963, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Traditional Eravate Autocracy\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.064935, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Eravate Life Services\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.095904, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Official Eravate Flag\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.179820, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Adle's Armada\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.411588, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"SquadronFaction\":true, \"HappiestSystem\":true, \"HomeSystem\":true, \"MyReputation\":100.000000, \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":0 } ] } ], \"SystemFaction\":{ \"Name\":\"Adle's Armada\", \"FactionState\":\"None\" } }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (LocationEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(LocationEvent));

            var result = EDDI.Instance.eventLocation( @event );
            Assert.IsTrue(result);
        }

        [TestMethod, DoNotParallelize]
        public void TestBodyScannedEventHandler()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "dump/1520309296811", @"{""system"":{""bodies"":[],""coords"":{""x"":-9581.71875,""y"":-382.28125,""z"":54409.71875},""date"":""2019-07-12 08:01:39+00"",""id64"":1520309296811,""name"":""Grea Bloae HH-T d4-44"",""stations"":[]}}" );

            var line = @"{ ""timestamp"":""2016 - 11 - 01T18: 49:07Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""StarSystem"":""Grea Bloae HH-T d4-44"", ""SystemAddress"":1520309296811, ""DistanceFromArrivalLS"":703.763611, ""TidalLock"":false, ""TerraformState"":""Terraformable"", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick carbon dioxide atmosphere"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":2.171783, ""Radius"":7622170.500000, ""SurfaceGravity"":14.899396, ""SurfaceTemperature"":836.165466, ""SurfacePressure"":33000114.000000, ""Landable"":false, ""SemiMajorAxis"":210957926400.000000, ""Eccentricity"":0.000248, ""OrbitalInclination"":0.015659, ""Periapsis"":104.416656, ""OrbitalPeriod"":48801056.000000, ""RotationPeriod"":79442.242188 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (BodyScannedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(BodyScannedEvent));

            EDDI.Instance.updateCurrentSystem( "Grea Bloae HH-T d4-44", 1520309296811UL );
            Assert.IsNotNull( EDDI.Instance.CurrentStarSystem );
            Assert.AreEqual("Grea Bloae HH-T d4-44", EDDI.Instance.CurrentStarSystem.systemname);

            // Set up conditions to test the first scan of the body
            var body = EDDI.Instance.CurrentStarSystem.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4");
            if (body != null) { body.scannedDateTime = null; }
            EDDI.Instance.eventBodyScanned( @event );
            Assert.AreEqual(@event.timestamp, EDDI.Instance.CurrentStarSystem.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4").scannedDateTime);

            // Re-scanning the same body shouldn't replace the first scan's data
            var @event2 = new BodyScannedEvent(@event.timestamp.AddSeconds(60), @event.scantype, @event.body);
            EDDI.Instance.eventBodyScanned( @event2 );
            Assert.AreEqual(@event.timestamp, EDDI.Instance.CurrentStarSystem.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4").scannedDateTime);
        }

        [TestMethod, DoNotParallelize]
        public void TestBodyMappedEventHandler()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "dump/1520309296811", @"{""system"":{""bodies"":[],""coords"":{""x"":-9581.71875,""y"":-382.28125,""z"":54409.71875},""date"":""2019-07-12 08:01:39+00"",""id64"":1520309296811,""name"":""Grea Bloae HH-T d4-44"",""stations"":[]}}" );

            var line = @"{ ""timestamp"":""2016 - 11 - 01T18: 49:07Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""BodyID"":3, ""StarSystem"":""Grea Bloae HH-T d4-44"", ""SystemAddress"":1520309296811, ""DistanceFromArrivalLS"":703.763611, ""TidalLock"":false, ""TerraformState"":""Terraformable"", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick carbon dioxide atmosphere"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":2.171783, ""Radius"":7622170.500000, ""SurfaceGravity"":14.899396, ""SurfaceTemperature"":836.165466, ""SurfacePressure"":33000114.000000, ""Landable"":false, ""SemiMajorAxis"":210957926400.000000, ""Eccentricity"":0.000248, ""OrbitalInclination"":0.015659, ""Periapsis"":104.416656, ""OrbitalPeriod"":48801056.000000, ""RotationPeriod"":79442.242188 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (BodyScannedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(BodyScannedEvent));

            EDDI.Instance.updateCurrentSystem( "Grea Bloae HH-T d4-44", 1520309296811UL );
            Assert.IsNotNull( EDDI.Instance.CurrentStarSystem );
            Assert.AreEqual("Grea Bloae HH-T d4-44", EDDI.Instance.CurrentStarSystem.systemname);

            // Set up conditions to test the first scan of the body
            var body = EDDI.Instance.CurrentStarSystem.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4");
            if (body != null) { body.scannedDateTime = null; body.mappedDateTime = null; }
            EDDI.Instance.eventBodyScanned( @event );
            var scannedBody = EDDI.Instance.CurrentStarSystem.bodies.FirstOrDefault( b => b.bodyname == "Grea Bloae HH-T d4-44 4" );
            Assert.IsNotNull(scannedBody);
            Assert.AreEqual(@event.timestamp, scannedBody.scannedDateTime);
            var event1EstimatedValue = scannedBody.estimatedvalue;

            // Map the body
            var line2 = @"{ ""timestamp"":""2016 - 11 - 01T18: 59:07Z"", ""event"":""SAAScanComplete"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""BodyID"":3, ""StarSystem"":""Grea Bloae HH-T d4-44"", ""SystemAddress"":1520309296811, ""ProbesUsed"":5, ""EfficiencyTarget"":6 }";
            events = JournalMonitor.ParseJournalEntry(line2);
            Assert.AreEqual(1, events.Count);
            var @event2 = (BodyMappedEvent)events[0];
            EDDI.Instance.eventBodyMapped( @event2 );

            Assert.AreEqual(@event.timestamp, scannedBody.scannedDateTime);
            Assert.AreEqual(@event2.timestamp, scannedBody.mappedDateTime);
            Assert.IsTrue(scannedBody.estimatedvalue > event1EstimatedValue);
        }

        [TestMethod, DoNotParallelize]
        public void TestSignalDetectedDeDuplication()
        {
            EDDI.Instance.CurrentStarSystem = new StarSystem { systemname = "TestSystem", systemAddress = 6606892846275 };
            var currentStarSystem = EDDI.Instance.CurrentStarSystem;

            var line0 = @"{ ""timestamp"":""2019-02-04T02:20:28Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";
            var line1 = @"{ ""timestamp"":""2019-02-04T02:25:03Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";
            var line2 = @"{ ""timestamp"":""2019-02-04T02:28:26Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$Fixed_Event_Life_Ring;"", ""SignalName_Localised"":""Notable stellar phenomena"" }";
            var line3 = @"{ ""timestamp"":""2019-02-04T02:38:53Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$Fixed_Event_Life_Ring;"", ""SignalName_Localised"":""Notable stellar phenomena"" }";
            var line4 = @"{ ""timestamp"":""2019-02-04T02:38:53Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";

            var event0 = (SignalDetectedEvent)JournalMonitor.ParseJournalEntry(line0).FirstOrDefault();
            EDDI.Instance.eventSignalDetected( event0 );
            Assert.AreEqual(1, currentStarSystem.signalsources.Count);
            Assert.AreEqual("Unregistered Comms Beacon", currentStarSystem.signalsources[0]);

            var event1 = (SignalDetectedEvent)JournalMonitor.ParseJournalEntry(line1).FirstOrDefault();
            EDDI.Instance.eventSignalDetected( event1 );
            Assert.AreEqual(1, currentStarSystem.signalsources.Count );

            var event2 = (SignalDetectedEvent)JournalMonitor.ParseJournalEntry(line2).FirstOrDefault();
            EDDI.Instance.eventSignalDetected( event2 );
            Assert.AreEqual(2, currentStarSystem.signalsources.Count );
            Assert.AreEqual("Notable Stellar Phenomena", currentStarSystem.signalsources[1]);

            var event3 = (SignalDetectedEvent)JournalMonitor.ParseJournalEntry(line3).FirstOrDefault();
            EDDI.Instance.eventSignalDetected( event3 );
            Assert.AreEqual(2, currentStarSystem.signalsources.Count );

            var event4 = (SignalDetectedEvent)JournalMonitor.ParseJournalEntry(line4).FirstOrDefault();
            EDDI.Instance.eventSignalDetected( event4 );
            Assert.AreEqual(2, currentStarSystem.signalsources.Count );
        }

        [TestMethod, DoNotParallelize]
        public void TestMultiSystemScanCompleted()
        {
            // If the game writes the `FSSAllBodiesFound` event multiple times for a single star system, 
            // we will take the first and reject any repetitions within the same star system.

            var line = @"{ ""timestamp"":""2019 - 07 - 01T19: 30:17Z"", ""event"":""FSSAllBodiesFound"", ""SystemName"":""Pyria Thua IX-L d7-3"", ""SystemAddress"":113321713859, ""Count"":4 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            var @event = (SystemScanComplete)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(SystemScanComplete));

            EDDI.Instance.CurrentStarSystem = new StarSystem { systemname = "TestSystem" };
            Assert.IsFalse(EDDI.Instance.CurrentStarSystem.systemScanCompleted);

            // Test whether the first `SystemScanCompleted` event is accepted and passed to monitors / responders
            var eventPassed = EDDI.Instance.eventSystemScanComplete( @event );
            Assert.IsTrue(EDDI.Instance.CurrentStarSystem.systemScanCompleted);
            Assert.IsTrue(eventPassed);

            // Test a second `SystemScanCompleted` event to make sure the repetition is surpressed and not passed to monitors / responders
            eventPassed = EDDI.Instance.eventSystemScanComplete( @event );
            Assert.IsTrue(EDDI.Instance.CurrentStarSystem.systemScanCompleted);
            Assert.IsFalse(eventPassed);

            // Switch systems and verify that the `systemScanCompleted` bool returns to it's default state
            EDDI.Instance.CurrentStarSystem = new StarSystem { systemname = "TestSystem2" };
            Assert.IsFalse(EDDI.Instance.CurrentStarSystem.systemScanCompleted);
        }

        [TestMethod, DoNotParallelize]
        public void TestShipShutdownScenario ()
        {
            var speechResponder = new SpeechResponder();
            var speechService = SpeechService.Instance;

            // The speech responder should not pause speech after a partial shutdown.
            const string line = @"{ ""timestamp"":""2024-04-20T10:49:23Z"", ""event"":""SystemsShutdown"" }";
            const string line2 = @"{ ""timestamp"":""2024-04-20T10:49:23Z"", ""event"":""MaterialCollected"", ""Category"":""Encoded"", ""Name"":""tg_shutdowndata"", ""Name_Localised"":""Massive Energy Surge Analytics"", ""Count"":1 }";
            var events = JournalMonitor.ParseJournalEntries(new [] { line, line2 } );
            var @event = (ShipShutdownEvent)events[0];
            Assert.IsNotNull( @event );
            Assert.IsTrue(@event.partialshutdown);
            speechResponder.Handle( @event );
            Assert.IsFalse( speechService.speechQueue.isQueuePaused );

            // The speech responder should pause speech after a full shutdown.
            events = JournalMonitor.ParseJournalEntries( new[] { line } );
            @event = (ShipShutdownEvent)events[ 0 ];
            Assert.IsNotNull( @event );
            Assert.IsFalse( @event.partialshutdown );
            speechResponder.Handle( @event );
            Assert.IsTrue( speechService.speechQueue.isQueuePaused );

            // While speech is paused, new speech should be added to the queue but not removed from the queue.
            speechService.speechQueue.DequeueAllSpeech();
            speechService.Say(null, "This speech should not be dequeued until speech is unpaused.");
            Thread.Sleep(TimeSpan.FromSeconds(3));
            Assert.IsTrue( speechService.speechQueue.isQueuePaused );
            Assert.IsTrue( speechService.speechQueue.hasSpeech );

            // Remove the speech from the queue again
            speechService.speechQueue.DequeueAllSpeech();
            Assert.IsTrue( speechService.speechQueue.isQueuePaused );
            Assert.IsFalse( speechService.speechQueue.hasSpeech );

            // The speech responder should unpause speech after a `Ship shutdown reboot` event.
            var rebootEvent = new ShipShutdownRebootEvent( @event.timestamp + TimeSpan.FromSeconds( 30 ) );
            speechResponder.Handle( rebootEvent );
            Assert.IsFalse( speechService.speechQueue.isQueuePaused );
        }
    }
}
