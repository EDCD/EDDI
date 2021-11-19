﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;


namespace IntegrationTests
{
    [TestClass]
    public class NavigationServiceTests : TestBase
    {
        [TestInitialize]
        public void StartTestMissionMonitor()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestGetService()
        {
            // ToDo: Refactor the called `GetServiceRoute` method and rewrite with canned data.
            // The current method produces varied results according to the landing pad size
            // of the commander's current ship and the current conditions of the star systems
            // (security level can change when a system's faction ownership changes, and with
            // that change a facilitator may appear or disappear)

            /*
            PrivateObject eddiInstance = new PrivateObject(EDDI.Instance);
            PrivateObject navInstance = new PrivateObject(NavigationService.Instance);
            eddiInstance.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "Sol", systemAddress = 10477373803, x = 0, y = 0, z = 0 });
            eddiInstance.SetFieldOrProperty("CurrentShip", new Ship() { Size = LandingPadSize.Medium });

            // Interstellar Factors Contact
            NavigationService.Instance.GetServiceRoute("facilitator", 10000);
            string system = (string)navInstance.GetFieldOrProperty("searchSystem");
            string station = (string)navInstance.GetFieldOrProperty("searchStation");
            decimal distance = (decimal)navInstance.GetFieldOrProperty("searchDistance");
            Assert.AreEqual("WISE 0855-0714", system);
            Assert.AreEqual("Yamazaki Landing", station);
            Assert.AreEqual(7.17M, distance);

            // Manufactured Materials Trader
            NavigationService.Instance.GetServiceRoute("manufactured", 10000);
            var system = (string)navInstance.GetFieldOrProperty("searchSystem");
            var station = (string)navInstance.GetFieldOrProperty("searchStation");
            var distance = (decimal)navInstance.GetFieldOrProperty("searchDistance");
            Assert.AreEqual("Sirius", system);
            Assert.AreEqual("Patterson Enterprise", station);
            Assert.AreEqual(8.59M, distance);

            // Guardian Technology Broker
            NavigationService.Instance.GetServiceRoute("guardian", 10000);
            system = (string)navInstance.GetFieldOrProperty("searchSystem");
            station = (string)navInstance.GetFieldOrProperty("searchStation");
            distance = (decimal)navInstance.GetFieldOrProperty("searchDistance");
            Assert.AreEqual("Bhritzameno", system);
            Assert.AreEqual("Feynman Terminal", station);
            Assert.AreEqual(19.09M, distance);
            */
        }
    }
}