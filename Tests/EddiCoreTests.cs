﻿using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using MathNet.Numerics.RootFinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnitTests;
using Utilities;

namespace IntegrationTests
{
    [TestClass]
    public class EddiCoreTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestKeepAlive()
        {
            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            IEddiMonitor monitor = ((List<IEddiMonitor>)privateObject
                .GetFieldOrProperty("monitors"))?
                .FirstOrDefault(m => m.MonitorName() == "Journal monitor");

            Assert.IsNotNull(monitor);
            privateObject.Invoke("EnableMonitor", new object[] { monitor.MonitorName() });
            monitor.Stop();
            Assert.AreEqual(1, ((ConcurrentBag<IEddiMonitor>)privateObject.GetFieldOrProperty("activeMonitors"))?.Count());

            privateObject.Invoke("DisableMonitor", new object[] { monitor.MonitorName() });
            Assert.AreEqual(0, ((ConcurrentBag<IEddiMonitor>)privateObject.GetFieldOrProperty("activeMonitors"))?.Count());

            privateObject.Invoke("EnableMonitor", new object[] { monitor.MonitorName() });
            monitor.Stop();

            Thread.Sleep(3000);
            Assert.AreEqual(1, ( (ConcurrentBag<IEddiMonitor>)privateObject.GetFieldOrProperty( "activeMonitors" ) )?.Count() );

            Thread.Sleep(3000);
            Assert.AreEqual(1, ( (ConcurrentBag<IEddiMonitor>)privateObject.GetFieldOrProperty( "activeMonitors" ) )?.Count() );

            Thread.Sleep(3000);
            Assert.AreEqual(1, ( (ConcurrentBag<IEddiMonitor>)privateObject.GetFieldOrProperty( "activeMonitors" ) )?.Count() );

            Thread.Sleep(3000);
            Assert.AreEqual(1, ( (ConcurrentBag<IEddiMonitor>)privateObject.GetFieldOrProperty( "activeMonitors" ) )?.Count() );

            var activeMonitors = (ConcurrentBag<IEddiMonitor>)privateObject.GetFieldOrProperty( "activeMonitors" );
            if ( activeMonitors == null )
            {
                Assert.Fail();
            }
            else
            {
                activeMonitors.TryTake( out IEddiMonitor activeMonitor );
                Assert.AreEqual( monitor, activeMonitor );
            }
        }

    }
}

namespace UnitTests
{
    [TestClass]
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
            int numResponders = EDDI.Instance.findResponders().Count;
            Assert.IsTrue(numResponders > 0);
        }

        [TestMethod]
        public void TestMonitors()
        {
            int numMonitors = EDDI.Instance.findMonitors().Count;
            Assert.IsTrue(numMonitors > 0);
        }

        [TestMethod]
        public void TestJumpedEventHandler()
        {
            string line = "{ \"timestamp\":\"2018-12-25T20:07:06Z\", \"event\":\"FSDJump\", \"StarSystem\":\"LHS 20\", \"SystemAddress\":33656303199641, \"StarPos\":[11.18750,-37.37500,-31.84375], \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_HighTech;\", \"SystemEconomy_Localised\":\"High Tech\", \"SystemSecondEconomy\":\"$economy_Refinery;\", \"SystemSecondEconomy_Localised\":\"Refinery\", \"SystemGovernment\":\"$government_Democracy;\", \"SystemGovernment_Localised\":\"Democracy\", \"SystemSecurity\":\"$SYSTEM_SECURITY_medium;\", \"SystemSecurity_Localised\":\"Medium Security\", \"Population\":9500553, \"JumpDist\":20.361, \"FuelUsed\":3.065896, \"FuelLevel\":19.762932, \"Factions\":[ { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"\", \"MyReputation\":6.106290 }, { \"Name\":\"Shenetserii Confederation\", \"FactionState\":\"None\", \"Government\":\"Confederacy\", \"Influence\":0.127000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":18.809999, \"PendingStates\":[ { \"State\":\"War\", \"Trend\":0 } ] }, { \"Name\":\"LHS 20 Company\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.127000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":4.950000, \"PendingStates\":[ { \"State\":\"War\", \"Trend\":0 } ] }, { \"Name\":\"Traditional LHS 20 Defence Party\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.087000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":2.640000 }, { \"Name\":\"Movement for LHS 20 Liberals\", \"FactionState\":\"CivilWar\", \"Government\":\"Democracy\", \"Influence\":0.226000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"SquadronFaction\":true, \"HomeSystem\":true, \"MyReputation\":100.000000, \"ActiveStates\":[ { \"State\":\"CivilLiberty\" }, { \"State\":\"Investment\" }, { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Nationalists of LHS 20\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.105000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000 }, { \"Name\":\"LHS 20 Organisation\", \"FactionState\":\"CivilWar\", \"Government\":\"Anarchy\", \"Influence\":0.166000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":5.940000, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"LHS 20 Engineers\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.162000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":15.000000 } ], \"SystemFaction\":{ \"Name\":\"Movement for LHS 20 Liberals\", \"FactionState\":\"CivilWar\" } }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            JumpedEvent @event = (JumpedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(JumpedEvent));

            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            var result = (bool?)privateObject.Invoke("eventJumped", new object[] { @event });
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void TestJumpedHandler_Hyperdiction()
        {
            PrivateObject privateObject = new PrivateObject(EDDI.Instance);

            string line1 = @"{ ""timestamp"":""2024-02-20T11:10:24Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""Cephei Sector DQ-Y b1"", ""SystemAddress"":2868635641225, ""StarPos"":[-93.31250,31.00000,-73.00000], ""SystemAllegiance"":""Thargoid"", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""Нет"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""Нет"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""Нет"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Анархия"", ""Population"":0, ""Body"":""Cephei Sector DQ-Y b1 A"", ""BodyID"":1, ""BodyType"":""Star"", ""ThargoidWar"":{ ""CurrentState"":""Thargoid_Controlled"", ""NextStateSuccess"":"""", ""NextStateFailure"":""Thargoid_Controlled"", ""SuccessStateReached"":false, ""WarProgress"":0.000224, ""RemainingPorts"":0, ""EstimatedRemainingTime"":""0 Days"" }, ""JumpDist"":6.076, ""FuelUsed"":0.359144, ""FuelLevel"":31.640856 }";
            var event1 = (JumpedEvent)JournalMonitor.ParseJournalEntry(line1)[0];
            Assert.IsNotNull( event1 );
            Assert.IsInstanceOfType( event1, typeof( JumpedEvent ) );

            string line2 = @"{ ""timestamp"":""2024-02-20T11:11:12Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""HIP 8525"", ""SystemAddress"":560216410467, ""StarPos"":[-96.28125,31.65625,-71.25000], ""SystemAllegiance"":""Thargoid"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""Высокие технологии"", ""SystemSecondEconomy"":""$economy_Military;"", ""SystemSecondEconomy_Localised"":""Военная"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""Нет"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Анархия"", ""Population"":0, ""Body"":""HIP 8525 A"", ""BodyID"":1, ""BodyType"":""Star"", ""ThargoidWar"":{ ""CurrentState"":""Thargoid_Controlled"", ""NextStateSuccess"":""Thargoid_Recovery"", ""NextStateFailure"":""Thargoid_Controlled"", ""SuccessStateReached"":false, ""WarProgress"":0.006071, ""RemainingPorts"":0, ""EstimatedRemainingTime"":""0 Days"" }, ""JumpDist"":3.508, ""FuelUsed"":0.086031, ""FuelLevel"":31.554825, ""SystemFaction"":{ ""Name"":""None"" } }";
            var event2 = (JumpedEvent)JournalMonitor.ParseJournalEntry(line2)[0];
            Assert.IsNotNull( event2 );
            Assert.IsInstanceOfType( event2, typeof( JumpedEvent ) );

            string line3 = @"{ ""timestamp"":""2024-02-20T11:12:23Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""HIP 8525"", ""SystemAddress"":560216410467, ""StarPos"":[-96.28125,31.65625,-71.25000], ""SystemAllegiance"":""Thargoid"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""Высокие технологии"", ""SystemSecondEconomy"":""$economy_Military;"", ""SystemSecondEconomy_Localised"":""Военная"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""Нет"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Анархия"", ""Population"":0, ""Body"":""HIP 8525 ABC"", ""BodyID"":0, ""BodyType"":""Null"", ""ThargoidWar"":{ ""CurrentState"":""Thargoid_Controlled"", ""NextStateSuccess"":""Thargoid_Recovery"", ""NextStateFailure"":""Thargoid_Controlled"", ""SuccessStateReached"":false, ""WarProgress"":0.006071, ""RemainingPorts"":0, ""EstimatedRemainingTime"":""0 Days"" }, ""JumpDist"":3.508, ""FuelUsed"":0.086017, ""FuelLevel"":31.468807, ""SystemFaction"":{ ""Name"":""None"" } }";
            var event3 = (JumpedEvent)JournalMonitor.ParseJournalEntry(line3)[0];
            Assert.IsNotNull( event3 );
            Assert.IsInstanceOfType( event3, typeof( JumpedEvent ) );

            // Standard jump to Cephei Sector DQ-Y b1. Environment is supercruise.
            privateObject.Invoke("eventJumped", event1 );
            Assert.AreEqual( Constants.ENVIRONMENT_SUPERCRUISE, privateObject.GetFieldOrProperty( nameof(EDDI.Instance.Environment) ) );
            Assert.AreEqual( 2868635641225UL, ( (StarSystem)privateObject.GetFieldOrProperty( nameof(EDDI.Instance.CurrentStarSystem) ) )?.systemAddress );

            // Standard jump to HIP 8525. Environment is supercruise.
            privateObject.Invoke("eventJumped", event2 );
            Assert.AreEqual( Constants.ENVIRONMENT_SUPERCRUISE, privateObject.GetFieldOrProperty( nameof( EDDI.Instance.Environment ) ) );
            Assert.AreEqual( 560216410467UL, ( (StarSystem)privateObject.GetFieldOrProperty( nameof( EDDI.Instance.CurrentStarSystem ) ) )?.systemAddress );

            // Hyperdiction in HIP 8525. Environment is normal space rather than supercruise.
            privateObject.Invoke( "eventJumped", event3 );
            Assert.AreEqual( Constants.ENVIRONMENT_NORMAL_SPACE, privateObject.GetFieldOrProperty( nameof( EDDI.Instance.Environment ) ) );
            Assert.AreEqual( 560216410467UL, ( (StarSystem)privateObject.GetFieldOrProperty( nameof( EDDI.Instance.CurrentStarSystem ) ) )?.systemAddress );
        }

        [TestMethod]
        public void TestLocationEventHandler()
        {
            string line = "{ \"timestamp\":\"2018-12-27T08:05:23Z\", \"event\":\"Location\", \"Docked\":true, \"MarketID\":3230448384, \"StationName\":\"Cleve Hub\", \"StationType\":\"Orbis\", \"StarSystem\":\"Eravate\", \"SystemAddress\":5856221467362, \"StarPos\":[-42.43750,-3.15625,59.65625], \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemSecondEconomy\":\"$economy_Industrial;\", \"SystemSecondEconomy_Localised\":\"Industrial\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":740380179, \"Body\":\"Cleve Hub\", \"BodyID\":48, \"BodyType\":\"Station\", \"Powers\":[ \"Zachary Hudson\" ], \"PowerplayState\":\"Exploited\", \"Factions\":[ { \"Name\":\"Eravate School of Commerce\", \"FactionState\":\"None\", \"Government\":\"Cooperative\", \"Influence\":0.086913, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":91.840103 }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":42.790199 }, { \"Name\":\"Independent Eravate Free\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.123876, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Eravate Network\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.036963, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Traditional Eravate Autocracy\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.064935, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Eravate Life Services\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.095904, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Official Eravate Flag\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.179820, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Adle's Armada\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.411588, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"SquadronFaction\":true, \"HappiestSystem\":true, \"HomeSystem\":true, \"MyReputation\":100.000000, \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":0 } ] } ], \"SystemFaction\":{ \"Name\":\"Adle's Armada\", \"FactionState\":\"None\" } }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            LocationEvent @event = (LocationEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(LocationEvent));

            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            var result = (bool?)privateObject.Invoke("eventLocation", new object[] { @event });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestBodyScannedEventHandler()
        {
            string line = @"{ ""timestamp"":""2016 - 11 - 01T18: 49:07Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""StarSystem"":""Grea Bloae HH-T d4-44"", ""SystemAddress"":1520309296811, ""DistanceFromArrivalLS"":703.763611, ""TidalLock"":false, ""TerraformState"":""Terraformable"", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick carbon dioxide atmosphere"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":2.171783, ""Radius"":7622170.500000, ""SurfaceGravity"":14.899396, ""SurfaceTemperature"":836.165466, ""SurfacePressure"":33000114.000000, ""Landable"":false, ""SemiMajorAxis"":210957926400.000000, ""Eccentricity"":0.000248, ""OrbitalInclination"":0.015659, ""Periapsis"":104.416656, ""OrbitalPeriod"":48801056.000000, ""RotationPeriod"":79442.242188 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            BodyScannedEvent @event = (BodyScannedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(BodyScannedEvent));

            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            privateObject.Invoke( "updateCurrentSystem", new object[] { "Grea Bloae HH-T d4-44", 1520309296811UL } );
            Assert.AreEqual("Grea Bloae HH-T d4-44", EDDI.Instance.CurrentStarSystem?.systemname);

            // Set up conditions to test the first scan of the body
            var body = EDDI.Instance.CurrentStarSystem?.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4");
            if (body != null) { body.scannedDateTime = null; }
            privateObject.Invoke("eventBodyScanned", new object[] { @event });
            Assert.AreEqual(@event.timestamp, EDDI.Instance.CurrentStarSystem?.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4").scannedDateTime);

            // Re-scanning the same body shouldn't replace the first scan's data
            BodyScannedEvent @event2 = new BodyScannedEvent(@event.timestamp.AddSeconds(60), @event.scantype, @event.body);
            privateObject.Invoke("eventBodyScanned", new object[] { @event2 });
            Assert.AreEqual(@event.timestamp, EDDI.Instance.CurrentStarSystem?.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4").scannedDateTime);
        }

        [TestMethod]
        public void TestBodyMappedEventHandler()
        {
            string line = @"{ ""timestamp"":""2016 - 11 - 01T18: 49:07Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""BodyID"":3, ""StarSystem"":""Grea Bloae HH-T d4-44"", ""SystemAddress"":1520309296811, ""DistanceFromArrivalLS"":703.763611, ""TidalLock"":false, ""TerraformState"":""Terraformable"", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick carbon dioxide atmosphere"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":2.171783, ""Radius"":7622170.500000, ""SurfaceGravity"":14.899396, ""SurfaceTemperature"":836.165466, ""SurfacePressure"":33000114.000000, ""Landable"":false, ""SemiMajorAxis"":210957926400.000000, ""Eccentricity"":0.000248, ""OrbitalInclination"":0.015659, ""Periapsis"":104.416656, ""OrbitalPeriod"":48801056.000000, ""RotationPeriod"":79442.242188 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            BodyScannedEvent @event = (BodyScannedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(BodyScannedEvent));

            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            privateObject.Invoke( "updateCurrentSystem", new object[] { "Grea Bloae HH-T d4-44", 1520309296811UL } );
            Assert.AreEqual("Grea Bloae HH-T d4-44", EDDI.Instance.CurrentStarSystem?.systemname);

            // Set up conditions to test the first scan of the body
            var body = EDDI.Instance.CurrentStarSystem?.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4");
            if (body != null) { body.scannedDateTime = null; }
            privateObject.Invoke("eventBodyScanned", new object[] { @event });
            Assert.AreEqual(@event.timestamp, EDDI.Instance.CurrentStarSystem?.bodies.FirstOrDefault(b => b.bodyname == "Grea Bloae HH-T d4-44 4")?.scannedDateTime);
            long? event1EstimatedValue = EDDI.Instance.CurrentStarSystem?.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4").estimatedvalue;

            // Map the body
            string line2 = @"{ ""timestamp"":""2016 - 11 - 01T18: 59:07Z"", ""event"":""SAAScanComplete"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""BodyID"":3, ""StarSystem"":""Grea Bloae HH-T d4-44"", ""SystemAddress"":1520309296811, ""ProbesUsed"":5, ""EfficiencyTarget"":6 }";
            events = JournalMonitor.ParseJournalEntry(line2);
            Assert.AreEqual(1, events.Count);
            BodyMappedEvent @event2 = (BodyMappedEvent)events[0];
            privateObject.Invoke("eventBodyMapped", new object[] { @event2 });

            Assert.AreEqual(@event.timestamp, EDDI.Instance.CurrentStarSystem?.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4").scannedDateTime);
            Assert.AreEqual(@event2.timestamp, EDDI.Instance.CurrentStarSystem?.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4").mappedDateTime);
            Assert.IsTrue(EDDI.Instance.CurrentStarSystem?.bodies.Find(b => b.bodyname == "Grea Bloae HH-T d4-44 4").estimatedvalue > event1EstimatedValue);
        }

        [TestMethod]
        public void TestSignalDetectedDeDuplication()
        {
            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            privateObject.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "TestSystem", systemAddress = 6606892846275 });
            var currentStarSystem = (StarSystem)privateObject.GetFieldOrProperty("CurrentStarSystem");

            string line0 = @"{ ""timestamp"":""2019-02-04T02:20:28Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";
            string line1 = @"{ ""timestamp"":""2019-02-04T02:25:03Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";
            string line2 = @"{ ""timestamp"":""2019-02-04T02:28:26Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$Fixed_Event_Life_Ring;"", ""SignalName_Localised"":""Notable stellar phenomena"" }";
            string line3 = @"{ ""timestamp"":""2019-02-04T02:38:53Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$Fixed_Event_Life_Ring;"", ""SignalName_Localised"":""Notable stellar phenomena"" }";
            string line4 = @"{ ""timestamp"":""2019-02-04T02:38:53Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";

            JournalMonitor.ParseJournalEntry(line0);
            Assert.AreEqual(1, currentStarSystem?.signalsources.Count());
            Assert.AreEqual("Unregistered Comms Beacon", currentStarSystem?.signalsources[0]);

            JournalMonitor.ParseJournalEntry(line1);
            Assert.AreEqual(1, currentStarSystem?.signalsources.Count() );

            JournalMonitor.ParseJournalEntry(line2);
            Assert.AreEqual(2, currentStarSystem?.signalsources.Count() );
            Assert.AreEqual("Notable Stellar Phenomena", currentStarSystem?.signalsources[1]);

            JournalMonitor.ParseJournalEntry(line3);
            Assert.AreEqual(2, currentStarSystem?.signalsources.Count() );

            JournalMonitor.ParseJournalEntry(line4);
            Assert.AreEqual(2, currentStarSystem?.signalsources.Count() );
        }

        [TestMethod]
        public void TestMultiSystemScanCompleted()
        {
            // If the game writes the `FSSAllBodiesFound` event multiple times for a single star system, 
            // we will take the first and reject any repetitions within the same star system.

            string line = @"{ ""timestamp"":""2019 - 07 - 01T19: 30:17Z"", ""event"":""FSSAllBodiesFound"", ""SystemName"":""Pyria Thua IX-L d7-3"", ""SystemAddress"":113321713859, ""Count"":4 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SystemScanComplete @event = (SystemScanComplete)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(SystemScanComplete));

            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            privateObject.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "TestSystem" });
            Assert.IsFalse(EDDI.Instance.CurrentStarSystem?.systemScanCompleted);

            // Test whether the first `SystemScanCompleted` event is accepted and passed to monitors / responders
            var eventPassed = (bool?)privateObject.Invoke("eventSystemScanComplete", new object[] { @event });
            Assert.IsTrue(EDDI.Instance.CurrentStarSystem?.systemScanCompleted);
            Assert.IsTrue(eventPassed);

            // Test a second `SystemScanCompleted` event to make sure the repetition is surpressed and not passed to monitors / responders
            eventPassed = (bool?)privateObject.Invoke("eventSystemScanComplete", new object[] { @event });
            Assert.IsTrue(EDDI.Instance.CurrentStarSystem?.systemScanCompleted);
            Assert.IsFalse(eventPassed);

            // Switch systems and verify that the `systemScanCompleted` bool returns to it's default state
            privateObject.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "TestSystem2" });
            Assert.IsFalse(EDDI.Instance.CurrentStarSystem?.systemScanCompleted);
        }
    }
}
