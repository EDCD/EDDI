using EddiDataDefinitions;
using EddiNavigationService;
using Microsoft.VisualStudio.TestTools.UnitTesting;


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
        public void TestSetDestination()
        {
            PrivateObject eddiInstance = new PrivateObject(Eddi.EDDI.Instance);
            eddiInstance.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "Sol", systemAddress = 10477373803, x = 0, y = 0, z = 0 });

            // Set a route to a known star system and verify detination details
            Navigation.Instance.SetDestination("Merope");
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
            Navigation.Instance.SetDestination("No Such System");
            StarSystem destinationSystem2 = (StarSystem)eddiInstance.GetFieldOrProperty("DestinationStarSystem");
            decimal destinationDistanceLY2 = (decimal)eddiInstance.GetFieldOrProperty("DestinationDistanceLy");
            Assert.IsNull(destinationSystem2);
            Assert.AreEqual(0M, destinationDistanceLY2);
        }

        [TestMethod]
        public void TestGetService()
        {
            PrivateObject eddiInstance = new PrivateObject(Eddi.EDDI.Instance);
            PrivateObject navInstance = new PrivateObject(Navigation.Instance);
            eddiInstance.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "Sol", systemAddress = 10477373803, x = 0, y = 0, z = 0 });
            eddiInstance.SetFieldOrProperty("CurrentShip", new Ship() { size = LandingPadSize.Medium } );

            // Interstellar Factors Contact
            Navigation.Instance.GetServiceRoute("facilitator", 10000);
            string system = (string)navInstance.GetFieldOrProperty("searchSystem");
            string station = (string)navInstance.GetFieldOrProperty("searchStation");
            decimal distance = (decimal)navInstance.GetFieldOrProperty("searchDistance");
            Assert.AreEqual("Alpha Centauri", system);
            Assert.AreEqual("al-Din Prospect", station);
            Assert.AreEqual(4.38M, distance);

            // Manufactured Materials Trader
            Navigation.Instance.GetServiceRoute("manufactured", 10000);
            system = (string)navInstance.GetFieldOrProperty("searchSystem");
            station = (string)navInstance.GetFieldOrProperty("searchStation");
            distance = (decimal)navInstance.GetFieldOrProperty("searchDistance");
            Assert.AreEqual("Sirius", system);
            Assert.AreEqual("Patterson Enterprise", station);
            Assert.AreEqual(8.59M, distance);

            // Guardian Technology Broker
            Navigation.Instance.GetServiceRoute("guardian", 10000);
            system = (string)navInstance.GetFieldOrProperty("searchSystem");
            station = (string)navInstance.GetFieldOrProperty("searchStation");
            distance = (decimal)navInstance.GetFieldOrProperty("searchDistance");
            Assert.AreEqual("Bhritzameno", system);
            Assert.AreEqual("Feynman Terminal", station);
            Assert.AreEqual(19.09M, distance);
        }
    }
}