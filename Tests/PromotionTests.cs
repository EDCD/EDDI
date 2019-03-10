using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class PromotionTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
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
