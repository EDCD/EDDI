using EddiStarMapService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Tests.Properties;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class EdsmDataTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestTraffic()
        {
            // Test pilot traffic data
            var response = DeserializeJsonResource<JObject>(Resources.edsmTraffic);

            var traffic = StarMapService.ParseStarMapTraffic(response);

            Assert.IsNotNull(traffic);
            Assert.AreEqual(9631, traffic.total);
            Assert.AreEqual(892, traffic.week);
            Assert.AreEqual(193, traffic.day);
        }

        [TestMethod]
        public void TestDeaths()
        {
            // Test pilot mortality data
            var response = DeserializeJsonResource<JObject>(Resources.edsmDeaths);

            var deaths = StarMapService.ParseStarMapDeaths(response);

            Assert.IsNotNull(deaths);
            Assert.AreEqual(1068, deaths.total);
            Assert.AreEqual(31, deaths.week);
            Assert.AreEqual(4, deaths.day);
        }

        [TestMethod]
        public void TestTrafficUnknown()
        {
            // Setup
            var response = new JObject();

            // Act
            var traffic = StarMapService.ParseStarMapTraffic(response);

            // Assert
            Assert.IsNotNull(traffic);
            Assert.AreEqual(0, traffic.total);
            Assert.AreEqual(0, traffic.week);
            Assert.AreEqual(0, traffic.day);
        }
    }
}
