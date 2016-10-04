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
        public void TestEmptySystem()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Lagoon Sector GW-V b2-6", null, null, null);
            Assert.IsNotNull(starSystem.stations);
        }
    }
}
