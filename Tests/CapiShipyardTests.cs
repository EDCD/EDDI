using EddiCompanionAppService;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    [DeploymentItem("Abasheli Barracks.json")]
    public class CapiShipyardTests
    {
        [TestMethod]
        public void TestShips()
        {
            // Test factions data
            string jsonString = System.IO.File.ReadAllText("Abasheli Barracks.json");
            JObject json = JsonConvert.DeserializeObject<JObject>(jsonString);
            List<Ship> ships = CompanionAppService.ShipyardFromProfile(json);
            Assert.AreEqual(8, ships.Count);
        }
    }
}
