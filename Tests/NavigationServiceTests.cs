using EddiDataDefinitions;
using EddiEvents;
using EddiCrimeMonitor;
using EddiMaterialMonitor;
using EddiMissionMonitor;
using EddiNavigationService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class NavigationServiceTests : TestBase
    {
        [TestInitialize]
        private void StartTestMissionMonitor()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestSetRoute()
        {
            PrivateObject eddiInstance = new PrivateObject(Eddi.EDDI.Instance);
            eddiInstance.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "Sol", systemAddress = 10477373803 , x = 0, y = 0, z = 0});

            // Set a route to a known star system and verify detination details
            Navigation.Instance.SetRoute("Merope");
            StarSystem destinationSystem = (StarSystem)eddiInstance.GetFieldOrProperty("DestinationStarSystem");
            decimal destinationDistanceLY = (decimal)eddiInstance.GetFieldOrProperty("DestinationDistanceLy");
            Assert.IsNotNull(destinationSystem);
            Assert.AreEqual("Merope", destinationSystem.systemname);
            Assert.AreEqual(-78.59375M, destinationSystem.x);
            Assert.AreEqual(-149.625M, destinationSystem.y);
            Assert.AreEqual(-340.53125M, destinationSystem.z);
            Assert.AreEqual(380.17M, destinationDistanceLY);

            // Set a route to an unknown star system and verify detination details
            // Result should be a null system at zero distance to clear, clearing the route.
            Navigation.Instance.SetRoute("Test System");
            StarSystem destinationSystem2 = (StarSystem)eddiInstance.GetFieldOrProperty("DestinationStarSystem");
            decimal destinationDistanceLY2 = (decimal)eddiInstance.GetFieldOrProperty("DestinationDistanceLy");
            Assert.IsNull(destinationSystem2);
            Assert.AreEqual(0M, destinationDistanceLY2);
        }
    }
}
