using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiCompanionAppService;
using EddiDataDefinitions;
using System.Collections.Generic;
using EddiDataProviderService;

namespace Tests
{
    [TestClass]
    public class DataProviderTests
    {
        [TestMethod]
        public void TestDataProviderEmptySystem()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Lagoon Sector GW-V b2-6", null, null, null);
            Assert.IsNotNull(starSystem.stations);
        }

        [TestMethod]
        public void TestDataProviderBodies()
        {
            // Force obtain the data from remote source
            StarSystem starSystem = DataProviderService.GetSystemData("Sol", null, null, null);
            Assert.IsNotNull(starSystem);

            Assert.IsNotNull(starSystem.bodies);
            Assert.IsFalse(starSystem.bodies.Count == 0);

            Body sol = starSystem.bodies.Find(b => b.type == "Star");
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
            Assert.AreEqual(1, sol.solarmass);
            Assert.AreEqual(1, sol.solarradius);
            Assert.AreEqual("G", sol.stellarclass);
            Assert.AreEqual("Sol", sol.systemname);
            Assert.AreEqual(5778, sol.temperature);
            Assert.IsNull(sol.terraformstate);
            Assert.IsNotNull(sol.tidallylocked);
            Assert.IsFalse((bool)sol.tidallylocked);
            Assert.IsNull(sol.tilt);
            Assert.AreEqual("Star", sol.type);
            Assert.IsNull(sol.volcanism);

            Body mercury = starSystem.bodies.Find(n => n.name.Equals("Mercury"));
            Assert.IsNull(mercury.age);
            Assert.AreEqual("No atmosphere", mercury.atmosphere);
            Assert.AreEqual(180, mercury.distance);
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
            Assert.AreEqual(23.5M, mercury.materials[0].percentage);
            Assert.AreEqual("Mercury", mercury.materials[10].material);
            Assert.AreEqual(1, mercury.materials[10].percentage);
            Assert.AreEqual("Mercury", mercury.name);
            Assert.AreEqual(88, mercury.orbitalperiod);
            Assert.AreEqual("Metal-rich body", mercury.planettype);
            Assert.IsNull(mercury.pressure);
            Assert.IsNotNull(mercury.radius);
            Assert.AreEqual(2440, mercury.radius);
            Assert.AreEqual(58.6M,mercury.rotationalperiod);
            Assert.AreEqual(0.39M, mercury.semimajoraxis);
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
    }
}
