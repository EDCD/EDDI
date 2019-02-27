using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiEvents;
using EddiJournalMonitor;
using Rollbar;
using Eddi;
using System;

namespace UnitTests
{
    [TestClass]
    public class PromotionTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;

            // Set ourselves as in beta to stop sending data to remote systems
            EDDI.Instance.enqueueEvent(new FileHeaderEvent(DateTime.Now, "JournalBeta.txt", "beta", "beta"));

            // Don't write to permanent storage
            Utilities.Files.unitTesting = true;
        }

        private void ParseSampleByName(string sampleName)
        {
            string sample = Events.SampleByName(sampleName) as string;
            List<Event> sampleEvents = JournalMonitor.ParseJournalEntry(sample);
            Assert.IsTrue(sampleEvents.Count == 1, $"Expected one event, got {sampleEvents.Count}");
        }

        [TestMethod]
        public void TestPromotionCombat()
        {
            ParseSampleByName("Combat promotion");
        }

        [TestMethod]
        public void TestPromotionTrade()
        {
            ParseSampleByName("Trade promotion");
        }

        [TestMethod]
        public void TestPromotionExploration()
        {
            ParseSampleByName("Exploration promotion");
        }

        [TestMethod]
        public void TestPromotionFederation()
        {
            ParseSampleByName("Federation promotion");
        }

        [TestMethod]
        public void TestPromotionEmpire()
        {
            ParseSampleByName("Empire promotion");
        }
    }
}
