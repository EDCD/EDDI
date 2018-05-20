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
            StarSystem starSystem = DataProviderService.GetSystemData("Lagoon Sector GW-V b2-6", null, null, null);
            Assert.IsNotNull(starSystem.stations);
        }

        [TestMethod]
        public void TestDataProviderMalformedSystem()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Malformed with quote\" and backslash\\. So evil", null, null, null);
            Assert.IsNotNull(starSystem);
        }

        [TestMethod]
        public void TestDataProviderBodies()
        {
            // Force obtain the data from remote source
            StarSystem starSystem = DataProviderService.GetSystemData("Sol", null, null, null);
            Assert.IsNotNull(starSystem);

            Assert.IsNotNull(starSystem.bodies);
            Assert.IsFalse(starSystem.bodies.Count == 0);

            Body sol = starSystem.bodies.Find(b => b.name == "Sol");
            Assert.AreEqual(4792, sol.age);
            Assert.IsNull(sol.atmosphere);
            Assert.AreEqual(0, sol.distance);
            Assert.IsNull(sol.earthmass);
            Assert.IsNull(sol.eccentricity);
            Assert.IsNull(sol.gravity);
            Assert.IsNull(sol.inclination);
            Assert.IsNotNull(sol.landable);
            Assert.IsFalse((bool)sol.landable);
            Assert.IsNotNull(sol.mainstar);
            Assert.IsTrue((bool)sol.mainstar);
            Assert.IsNull(sol.materials);
            Assert.AreEqual("Sol", sol.name);
            Assert.IsNull(sol.orbitalperiod);
            Assert.IsNull(sol.planettype);
            Assert.IsNull(sol.pressure);
            Assert.IsNull(sol.radius);
            Assert.IsNull(sol.rotationalperiod);
            Assert.IsNull(sol.semimajoraxis);
            Assert.AreEqual(1.0, (double)sol.solarmass, 0.001);
            Assert.AreEqual(1.0, (double)sol.solarradius, 0.001);
            Assert.AreEqual("G", sol.stellarclass);
            Assert.AreEqual("Sol", sol.systemname);
            Assert.AreEqual(5778, sol.temperature);
            Assert.IsNull(sol.terraformstate);
            Assert.IsNotNull(sol.tidallylocked);
            Assert.IsFalse((bool)sol.tidallylocked);
            Assert.IsNull(sol.tilt);
            Assert.AreEqual("Star", sol.type);
            Assert.IsNull(sol.volcanism);
            // Stellar extras
            Assert.AreEqual("yellow-white", sol.chromaticity);
            Assert.AreEqual(68, sol.massprobability);
            Assert.AreEqual(49, sol.radiusprobability);
            Assert.AreEqual(51, sol.tempprobability);
            Assert.AreEqual(51, sol.ageprobability);

            Body mercury = starSystem.bodies.Find(n => n.name.Equals("Mercury"));
            Assert.IsNull(mercury.age);
            Assert.AreEqual("No atmosphere", mercury.atmosphere);
            Assert.IsNotNull(mercury.distance);
            Assert.AreEqual(0.055M, mercury.earthmass);
            Assert.AreEqual(0.2056M, mercury.eccentricity);
            Assert.AreEqual(0.38M, mercury.gravity);
            Assert.AreEqual(7, mercury.inclination);
            Assert.IsNotNull(mercury.landable);
            Assert.IsTrue((bool)mercury.landable);
            Assert.IsNull(mercury.mainstar);
            Assert.IsNotNull(mercury.materials);
            Assert.AreEqual(11, mercury.materials.Count);
            Assert.AreEqual("Iron", mercury.materials[0].material);
            Assert.AreEqual(23.5, (double)mercury.materials[0].percentage, 0.1);
            Assert.AreEqual("Mercury", mercury.materials[10].material);
            Assert.AreEqual(1.0, (double)mercury.materials[10].percentage, 0.1);
            Assert.AreEqual("Mercury", mercury.name);
            Assert.AreEqual(88.0, (double)mercury.orbitalperiod, 0.1);
            Assert.AreEqual("Metal-rich body", mercury.planettype);
            Assert.IsNotNull(mercury.pressure);
            Assert.IsNotNull(mercury.radius);
            Assert.AreEqual(2440, mercury.radius);
            Assert.AreEqual(58.6, (double)mercury.rotationalperiod, 0.1);
            Assert.AreEqual(0.39, (double)mercury.semimajoraxis, 0.01);
            Assert.IsNull(mercury.solarmass);
            Assert.IsNull(mercury.solarradius);
            Assert.IsNull(mercury.stellarclass);
            Assert.AreEqual("Sol", mercury.systemname);
            Assert.AreEqual(402, mercury.temperature);
            Assert.AreEqual("Not terraformable", mercury.terraformstate);
            Assert.IsNotNull(mercury.tidallylocked);
            Assert.IsFalse((bool)mercury.tidallylocked);
            Assert.AreEqual(2.11M, mercury.tilt);
            Assert.AreEqual("Planet", mercury.type);
            Assert.IsNull(mercury.volcanism);
        }

        [TestMethod]
        public void TestDataProviderUnknown()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Not appearing in this galaxy", null, null, null);
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
            Assert.AreEqual(3, system.stations.Count);
            Assert.AreEqual(30, system.bodies.Count);
        }
    }
}
