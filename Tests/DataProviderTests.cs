using EddiDataDefinitions;
using EddiDataProviderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Tests.Properties;

namespace UnitTests
{
    [TestClass]
    public class DataProviderTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestDataProviderEmptySystem()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Lagoon Sector GW-V b2-6");
            Assert.IsNotNull(starSystem.population);
        }

        [TestMethod]
        public void TestDataProviderMalformedSystem()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Malformed with quote\" and backslash\\. So evil");
            Assert.IsNotNull(starSystem);
        }

        [TestMethod]
        public void TestDataProviderUnknown()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Not appearing in this galaxy");
            Assert.IsNotNull(starSystem);
        }

        [TestMethod]
        public void TestLegacySystem1()
        {
            /// Test legacy data that may be stored in user's local sql databases. 
            /// Legacy data includes all data stored in user's sql databases prior to version 3.0.1-b2
            /// Note that data structures were reorganized at this time to support internationalization.

            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1);

            Assert.IsNotNull(system);
            Assert.AreEqual("Macay", system.systemname);
            Assert.AreEqual(8898081, system.population);
            Assert.AreEqual(2, system.stations.Count);
            Assert.AreEqual(0, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacySystem2()
        {
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem2);

            Assert.IsNotNull(system);
            Assert.AreEqual("Lazdongand", system.systemname);
            Assert.AreEqual(75005, system.population);
            Assert.AreEqual(3, system.stations.Count);
            Assert.AreEqual(0, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacySystem3()
        {
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem3);

            Assert.IsNotNull(system);
            Assert.AreEqual("Aphros", system.systemname);
            Assert.AreEqual(0, system.population);
            Assert.AreEqual(0, system.stations.Count);
            Assert.AreEqual(8, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacySystem4()
        {
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem4);

            Assert.AreEqual("Zhu Baba", system.systemname);
            Assert.AreEqual(159918, system.population);
            Assert.AreEqual(0, system.stations.Count);
            Assert.AreEqual(30, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacyData()
        {
            /// Test legacy data from api.eddp.co 
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1);

            Assert.AreEqual("Nijland Terminal", system.stations[0].name);
            Assert.IsNull(system.stations[0].EDDBID);
            Assert.AreEqual("Pinzon Hub", system.stations[1].name);
            Assert.IsNull(system.stations[1].EDDBID);

            system = LegacyEddpService.SetLegacyData(system);
            Assert.AreEqual(32548, system.stations[0].EDDBID);
            Assert.AreEqual(58341, system.stations[1].EDDBID);
        }

        [TestMethod]
        public void TestStarSystemData()
        {
            // Test system & body data in a complete star system
            StarSystem starSystem = DataProviderService.GetSystemData("Sol");

            Assert.AreEqual("Sol", starSystem.systemname);
            Assert.AreEqual(17072, starSystem.EDDBID);
            Assert.AreEqual(0M, starSystem.x);
            Assert.AreEqual(0M, starSystem.y);
            Assert.AreEqual(0M, starSystem.z);
            Assert.IsNotNull(starSystem.population);
            Assert.IsNotNull(starSystem.Faction);
            Assert.IsNotNull(starSystem.Faction.Allegiance.invariantName);
            Assert.IsNotNull(starSystem.Faction.Government.invariantName);
            Assert.IsNotNull(starSystem.Faction.presences.FirstOrDefault(p => p.systemName == starSystem.systemname)?.FactionState?.invariantName);
            Assert.IsNotNull(starSystem.Faction.name);
            Assert.IsNotNull(starSystem.securityLevel.invariantName);
            Assert.IsNotNull(starSystem.primaryeconomy);
            Assert.AreEqual("Common", starSystem.Reserve.invariantName);
            Assert.IsNotNull(starSystem.stations.Count);
            Assert.IsNotNull(starSystem);
            Assert.IsNotNull(starSystem.bodies);
            Assert.AreNotEqual(0, starSystem.bodies.Count);
        }

        [TestMethod]
        public void TestLatency()
        {
            System.DateTime startTime = System.DateTime.UtcNow;
            StarSystem starSystem = DataProviderService.GetSystemData("Sol");
            Assert.IsTrue((System.DateTime.UtcNow - startTime).Milliseconds < 1000);
        }

        [TestMethod]
        public void TestNullLegacy()
        {
            StarSystem starSystem = LegacyEddpService.SetLegacyData(new StarSystem() { systemname = "No such system" });
            Assert.IsNotNull(starSystem);
            Assert.AreEqual("No such system", starSystem.systemname);
        }

        [TestMethod]
        public void TestBgsFactionsFromName()
        {
            // Test a known faction
            Faction faction1 = DataProviderService.GetFactionByName("The Dark Wheel");
            Assert.IsNotNull(faction1);
            Assert.AreEqual("The Dark Wheel", faction1.name);
            Assert.AreEqual("Democracy", faction1.Government.invariantName);
            Assert.AreEqual("Independent", faction1.Allegiance.invariantName);
            Assert.AreEqual(41917, faction1.EDDBID);
            Assert.AreNotEqual(System.DateTime.MinValue, faction1.updatedAt);

            // Even if the faction does not exist, we should return a basic object
            Faction faction2 = DataProviderService.GetFactionByName("No such faction");
            Assert.IsNotNull(faction2);
            Assert.AreEqual("No such faction", faction2.name);
            Assert.AreEqual(Government.None, faction2.Government);
            Assert.AreEqual(Superpower.None, faction2.Allegiance);
            Assert.AreEqual(System.DateTime.MinValue, faction2.updatedAt);
        }
    }
}
