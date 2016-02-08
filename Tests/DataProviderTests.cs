using Microsoft.VisualStudio.TestTools.UnitTesting;
using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using System.Collections.Generic;
using EliteDangerousDataProviderService;

namespace Tests
{
    [TestClass]
    public class DataProviderTests
    {
        [TestMethod]
        public void TestEmptySystem()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Lagoon Sector GW-V b2-6");
            Assert.IsNotNull(starSystem.Stations);
        }
    }
}
