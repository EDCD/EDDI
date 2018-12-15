using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiDataDefinitions;
using EddiDataProviderService;
using Rollbar;
using Newtonsoft.Json;

namespace UnitTests
{
    [TestClass]
    public class DataProviderTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
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
            Assert.IsNotNull(starSystem.Faction.FactionState.invariantName);
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
    }
}
