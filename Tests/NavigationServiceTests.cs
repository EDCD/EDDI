using EddiCore;
using EddiDataDefinitions;
using EddiNavigationService;
using EddiStarMapService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.Properties;
using UnitTests;

namespace IntegrationTests
{
    [TestClass]
    public class NavigationServiceTests : TestBase
    {
        private FakeEdsmRestClient fakeEdsmRestClient;
        private StarMapService fakeEdsmService;
        private NavigationService navigationService;

        [TestInitialize]
        public void Start()
        {
            fakeEdsmRestClient = new FakeEdsmRestClient();
            fakeEdsmService = new StarMapService(fakeEdsmRestClient);
            navigationService = new NavigationService(fakeEdsmService);
            MakeSafe();

            // Use a standard cube search around Sol for our service queries 
            string resource = "api-v1/cube-systems";
            string json = Encoding.UTF8.GetString(Resources.cubeSystemsAroundSol);
            List<JObject> data = new List<JObject>();
            fakeEdsmRestClient.Expect(resource, json, data);

            // Use a standard cube search around Sol for our service queries 
            string resource2 = "api-v1/sphere-systems";
            string json2 = Encoding.UTF8.GetString(Resources.sphereAroundSol);
            List<JObject> data2 = new List<JObject>();
            fakeEdsmRestClient.Expect(resource2, json2, data2);
        }

        [DataTestMethod, DoNotParallelize]
        [DataRow(QueryType.encoded, null, null, 10000.0, true, "EZ Aquarii", "Magnus Gateway")]
        [DataRow(QueryType.manufactured, null, null, 10000.0, true, "Sirius", "Patterson Enterprise")]
        [DataRow(QueryType.raw, null, null, 10000.0, true, "61 Cygni", "Broglie Terminal")]
        [DataRow(QueryType.guardian, null, null, 10000.0, true, "EZ Aquarii", "Magnus Gateway")]
        [DataRow(QueryType.human, null, null, 10000.0, true, "WISE 1506+7027", "Dobrovolskiy Enterprise")]
        [DataRow(QueryType.scorpion, null, null, 10000.0, true, "Gendalla", "Aksyonov Installation")]
        [DataRow(QueryType.scoop, null, null, 10.0, true, "Sol", null)]
        [DataRow(QueryType.facilitator, null, null, 10000.0, true, "Barnard's Star", "Miller Depot")]
        public void TestNavQuery(QueryType query, string stringArg0, string stringArg1, double numericArg, bool prioritizeOrbitalStations, string expectedStarSystem, string expectedStationName)
        {
            // Setup
            var privateObject = new PrivateObject(EDDI.Instance);
            var sol = new StarSystem { systemname = "Sol", systemAddress = 10477373803, x = 0.0M, y = 0.0M, z = 0.0M };
            privateObject.SetFieldOrProperty(nameof(EDDI.Instance.CurrentStarSystem), sol);
            privateObject.SetFieldOrProperty( nameof( EDDI.Instance.CurrentShip ), ShipDefinitions.FromEDModel( "Anaconda" ) );

            var result = navigationService.NavQuery(query, stringArg0, stringArg1, Convert.ToDecimal(numericArg), prioritizeOrbitalStations);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedStarSystem, result.system);
            Assert.AreEqual(expectedStationName, result.station);
        }
    }
}