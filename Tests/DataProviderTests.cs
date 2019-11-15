using EddiBgsService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiStarMapService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.Properties;

namespace UnitTests
{
    [TestClass]
    public class DataProviderTests : TestBase
    {
        FakeEdsmRestClient fakeEdsmRestClient;
        StarMapService fakeEdsmService;
        private DataProviderService dataProviderService;

        [TestInitialize]
        public void start()
        {
            fakeEdsmRestClient = new FakeEdsmRestClient();
            fakeEdsmService = new StarMapService(fakeEdsmRestClient);
            dataProviderService = new DataProviderService(fakeEdsmService);
            MakeSafe();
        }

        [TestMethod]
        public void TestDataProviderEmptySystem()
        {
            // Setup
            string resource = "api-v1/systems";
            string json = "{}";
            List<JObject> data = new List<JObject>();
            fakeEdsmRestClient.Expect(resource, json, data);

            StarSystem starSystem = dataProviderService.GetSystemData("Lagoon Sector GW-V b2-6");
            Assert.IsNotNull(starSystem.population);
        }

        [TestMethod]
        public void TestDataProviderMalformedSystem()
        {
            // Setup
            string resource = "api-v1/systems";
            string json = "{}";
            List<JObject> data = new List<JObject>();
            fakeEdsmRestClient.Expect(resource, json, data);

            StarSystem starSystem = dataProviderService.GetSystemData("Malformed with quote\" and backslash\\. So evil");
            Assert.IsNotNull(starSystem);
        }

        [TestMethod]
        public void TestDataProviderUnknown()
        {
            // Setup
            string resource = "api-v1/systems";
            string json = "{}";
            List<JObject> data = new List<JObject>();
            fakeEdsmRestClient.Expect(resource, json, data);

            StarSystem starSystem = dataProviderService.GetSystemData("Not appearing in this galaxy");
            Assert.IsNotNull(starSystem);
        }

        [TestMethod]
        public void TestLegacySystem1()
        {
            // Test legacy data that may be stored in user's local sql databases.
            // Legacy data includes all data stored in user's sql databases prior to version 3.0.1-b2
            // Note that data structures were reorganized at this time to support internationalization.
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
            // Test legacy data from api.eddp.co
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
            // Setup
            string solJson = Encoding.UTF8.GetString(Resources.Sol);
            fakeEdsmRestClient.Expect("api-v1/systems", solJson, new List<JObject>());

            string solBodiesJson = Encoding.UTF8.GetString(Resources.SolBodies);
            fakeEdsmRestClient.Expect("api-system-v1/bodies", solBodiesJson, new Dictionary<string, object>());

            string solFactionsJson = Encoding.UTF8.GetString(Resources.SolFactions);
            fakeEdsmRestClient.Expect("api-system-v1/factions", solFactionsJson, new JObject());

            string solStationsJson = Encoding.UTF8.GetString(Resources.SolStations);
            fakeEdsmRestClient.Expect("api-system-v1/stations", solStationsJson, new JObject());

            // Test system & body data in a complete star system
            StarSystem starSystem = dataProviderService.GetSystemData("Sol");

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
        public void TestNullLegacy()
        {
            StarSystem starSystem = LegacyEddpService.SetLegacyData(new StarSystem() { systemname = "No such system" });
            Assert.IsNotNull(starSystem);
            Assert.AreEqual("No such system", starSystem.systemname);
        }
    }
}
