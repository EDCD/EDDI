using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;

namespace Tests
{
    [TestClass]
    public class ShipTests
    {
        [TestMethod]
        public void TestShipSpokenName1()
        {
            Ship ship = new Ship();
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your ship", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName2()
        {
            Ship ship = new Ship()
            {
                name = ""
            };
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your ship", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName3()
        {
            Ship ship = ShipDefinitions.FromModel("Anaconda");
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your Anaconda", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName4()
        {
            Ship ship = ShipDefinitions.FromModel("Anaconda");
            ship.name = "Testy";
            string spokenName = ship.SpokenName();
            Assert.AreEqual("Testy", spokenName);
        }

        [TestMethod]
        public void TestJournalShipScenario1()
        {
            // Set ourselves as in beta to stop sending data to remote systems
            Eddi.EDDI.Instance.eventHandler(new FileHeaderEvent(DateTime.Now, "beta", "beta"));

            // Start a ship monitor
            ShipMonitor shipMonitor = new ShipMonitor();

            // LoadGame
            SendEvents(@"", shipMonitor);

            // Loadout
            SendEvents(@"", shipMonitor);

            // Purchase a Courier

            // Swap back to the SideWinder

            // Swap back to the Courier

            // Name the Courier

            // Swap back to the SideWinder

            // Swap back to the Courier

            // Sell the Sidewinder
        }

        private void SendEvents(string line, ShipMonitor monitor)
        {
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            foreach (Event @event in events)
            {
                monitor.PreHandle(@event);
            }
        }
    }
}
