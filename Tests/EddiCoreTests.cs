using Eddi;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnitTests;

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
            EDDIMonitor monitor = ((List<EDDIMonitor>)privateObject
                .GetFieldOrProperty("monitors"))
                .FirstOrDefault(m => m.MonitorName() == "Journal monitor");

            privateObject.Invoke("EnableMonitor", new object[] { monitor.MonitorName() });
            monitor.Stop();
            Assert.AreEqual(1, ((ConcurrentBag<EDDIMonitor>)privateObject.GetFieldOrProperty("activeMonitors")).Count());

            privateObject.Invoke("DisableMonitor", new object[] { monitor.MonitorName() });
            Assert.AreEqual(0, ((ConcurrentBag<EDDIMonitor>)privateObject.GetFieldOrProperty("activeMonitors")).Count());

            privateObject.Invoke("EnableMonitor", new object[] { monitor.MonitorName() });
            monitor.Stop();

            Thread.Sleep(3000);
            Assert.AreEqual(1, ((ConcurrentBag<EDDIMonitor>)privateObject.GetFieldOrProperty("activeMonitors")).Count());

            Thread.Sleep(3000);
            Assert.AreEqual(1, ((ConcurrentBag<EDDIMonitor>)privateObject.GetFieldOrProperty("activeMonitors")).Count());

            Thread.Sleep(3000);
            Assert.AreEqual(1, ((ConcurrentBag<EDDIMonitor>)privateObject.GetFieldOrProperty("activeMonitors")).Count());

            Thread.Sleep(3000);
            Assert.AreEqual(1, ((ConcurrentBag<EDDIMonitor>)privateObject.GetFieldOrProperty("activeMonitors")).Count());

            ((ConcurrentBag<EDDIMonitor>)privateObject.GetFieldOrProperty("activeMonitors")).TryTake(out EDDIMonitor activeMonitor);
            Assert.AreEqual(monitor, activeMonitor);
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

            PrivateObject privateObject = new PrivateObject(Eddi.EDDI.Instance);
            var result = (bool)privateObject.Invoke("eventJumped", new object[] { @event });
            Assert.IsTrue(result);
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

            PrivateObject privateObject = new PrivateObject(Eddi.EDDI.Instance);
            var result = (bool)privateObject.Invoke("eventLocation", new object[] { @event });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestBodyScannedEventHandler()
        {
            string line = @"{ ""timestamp"":""2016 - 11 - 01T18: 49:07Z"", ""event"":""Scan"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""DistanceFromArrivalLS"":703.763611, ""TidalLock"":false, ""TerraformState"":""Terraformable"", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick carbon dioxide atmosphere"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":2.171783, ""Radius"":7622170.500000, ""SurfaceGravity"":14.899396, ""SurfaceTemperature"":836.165466, ""SurfacePressure"":33000114.000000, ""Landable"":false, ""SemiMajorAxis"":210957926400.000000, ""Eccentricity"":0.000248, ""OrbitalInclination"":0.015659, ""Periapsis"":104.416656, ""OrbitalPeriod"":48801056.000000, ""RotationPeriod"":79442.242188 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            BodyScannedEvent @event = (BodyScannedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(BodyScannedEvent));

            PrivateObject privateObject = new PrivateObject(Eddi.EDDI.Instance);
            privateObject.Invoke("updateCurrentSystem", new object[] { "Grea Bloae HH-T d4-44" });
            Assert.AreEqual("Grea Bloae HH-T d4-44", EDDI.Instance.CurrentStarSystem?.name);

            var result = (bool)privateObject.Invoke("eventBodyScanned", new object[] { @event });
            Assert.IsTrue(result);

            EddiDataDefinitions.Body body = EDDI.Instance.CurrentStarSystem.bodies.FirstOrDefault(b => b.name == "Grea Bloae HH-T d4-44 4");
            Assert.IsTrue(body.scanned);
        }

        [TestMethod]
        public void TestRingCurrentBody()
        {
            string line = @"{ ""timestamp"":""2018-12-02T07:59:04Z"", ""event"":""SupercruiseExit"", ""StarSystem"":""HIP 17704"", ""SystemAddress"":246119654564, ""Body"":""HIP 17704 4 A Ring"", ""BodyID"":18, ""BodyType"":""PlanetaryRing"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            EnteredNormalSpaceEvent @event = (EnteredNormalSpaceEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(EnteredNormalSpaceEvent));

            PrivateObject privateObject = new PrivateObject(Eddi.EDDI.Instance);
            privateObject.Invoke("updateCurrentStellarBody", new object[] { @event.body, @event.system, @event.bodyType, @event.systemAddress });
            Assert.AreEqual("HIP 17704 4", EDDI.Instance.CurrentStellarBody?.name);
        }

        [TestMethod]
        public void TestRingMappedCurrentBody()
        {
            string line = @"{ ""timestamp"":""2018-12-16T23:04:38Z"", ""event"":""SAAScanComplete"", ""BodyName"":""BD-01 2784 10 A Ring"", ""BodyID"":42, ""ProbesUsed"":1, ""EfficiencyTarget"":0 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            BodyMappedEvent @event = (BodyMappedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(BodyMappedEvent));

            PrivateObject privateObject = new PrivateObject(Eddi.EDDI.Instance);
            privateObject.Invoke("updateCurrentSystem", new object[] { "BD-01 2784" });
            privateObject.Invoke("eventBodyMapped", new object[] { @event });
            Assert.AreEqual("BD-01 2784 10", EDDI.Instance.CurrentStellarBody?.name);
        }
    }
}
