using EddiDataDefinitions;
using EddiDataProviderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;

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
        [DeploymentItem("sqlStarSystem1.json")]
        public void TestLegacySystem1()
        {
            /// Test legacy data that may be stored in user's local sql databases. 
            /// Legacy data includes all data stored in user's sql databases prior to version 3.0.1-b2
            /// Note that data structures were reorganized at this time to support internationalization.

            string legagySystemSql = System.IO.File.ReadAllText("sqlStarSystem1.json");

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(legagySystemSql);
            Assert.IsNotNull(system);
            Assert.AreEqual("Macay", system.name);
            Assert.AreEqual(8898081, system.population);
            Assert.AreEqual(2, system.stations.Count);
            Assert.AreEqual(0, system.bodies.Count);
        }

        [TestMethod]
        [DeploymentItem("sqlStarSystem2.json")]
        public void TestLegacySystem2()
        {
            string legagySystemSql = System.IO.File.ReadAllText("sqlStarSystem2.json");

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(legagySystemSql);
            Assert.IsNotNull(system);
            Assert.AreEqual("Lazdongand", system.name);
            Assert.AreEqual(75005, system.population);
            Assert.AreEqual(3, system.stations.Count);
            Assert.AreEqual(0, system.bodies.Count);
        }

        [TestMethod]
        [DeploymentItem("sqlStarSystem3.json")]
        public void TestLegacySystem3()
        {
            string legagySystemSql = System.IO.File.ReadAllText("sqlStarSystem3.json");

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(legagySystemSql);
            Assert.IsNotNull(system);
            Assert.AreEqual("Aphros", system.name);
            Assert.AreEqual(0, system.population);
            Assert.AreEqual(0, system.stations.Count);
            Assert.AreEqual(8, system.bodies.Count);
        }

        [TestMethod]
        [DeploymentItem("sqlStarSystem4.json")]
        public void TestLegacySystem4()
        {
            string legagySystemSql = System.IO.File.ReadAllText("sqlStarSystem4.json");

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(legagySystemSql);

            Assert.AreEqual("Zhu Baba", system.name);
            Assert.AreEqual(159918, system.population);
            Assert.AreEqual(0, system.stations.Count);
            Assert.AreEqual(30, system.bodies.Count);
        }

        [TestMethod]
        [DeploymentItem("sqlStarSystem1.json")]
        public void TestLegacyData()
        {
            /// Test legacy data from api.eddp.co 

            string legagySystemSql = System.IO.File.ReadAllText("sqlStarSystem1.json");

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(legagySystemSql);
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

            Assert.AreEqual("Sol", starSystem.name);
            Assert.AreEqual(17072, starSystem.EDDBID);
            Assert.AreEqual((decimal)0, starSystem.x);
            Assert.AreEqual((decimal)0, starSystem.y);
            Assert.AreEqual((decimal)0, starSystem.z);
            Assert.IsNotNull(starSystem.population);
            Assert.IsNotNull(starSystem.Faction);
            Assert.IsNotNull(starSystem.Faction.Allegiance.invariantName);
            Assert.IsNotNull(starSystem.Faction.Government.invariantName);
            Assert.IsNotNull(starSystem.Faction.presences.FirstOrDefault(p => p.systemName == starSystem.name)?.FactionState?.invariantName);
            Assert.IsNotNull(starSystem.Faction.name);
            Assert.IsNotNull(starSystem.securityLevel.invariantName);
            Assert.IsNotNull(starSystem.primaryeconomy);
            Assert.AreEqual("Common", starSystem.Reserve.invariantName);
            Assert.IsNotNull(starSystem.stations.Count);
            Assert.IsNotNull(starSystem);
            Assert.IsNotNull(starSystem.bodies);
            Assert.IsFalse(starSystem.bodies.Count == 0);
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
            StarSystem starSystem = LegacyEddpService.SetLegacyData(new StarSystem() { name = "No such system"});
            Assert.IsNotNull(starSystem);
            Assert.AreEqual("No such system", starSystem.name);
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
