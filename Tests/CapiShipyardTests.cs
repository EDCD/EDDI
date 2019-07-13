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
    public class CapiShipyardTests : TestBase
    {
        [TestMethod]
        public void TestShips()
        {
            // Test factions data
            JObject json = DeserializeJsonResource<JObject>(Tests.Properties.Resources.Abasheli_Barracks);
            List<Ship> ships = CompanionAppService.ShipyardFromProfile(json);
            Assert.AreEqual(8, ships.Count);
        }
    }
}
