using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class PromotionTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        private static void ParseSampleByName(string sampleName)
        {
            var sample = Events.SampleByName(sampleName) as string;
            var sampleEvents = JournalMonitor.ParseJournalEntry(sample);
            Assert.HasCount( 1, sampleEvents, $"Expected one event, got {sampleEvents.Count}");
        }

        [TestMethod]
        public void TestCommanderPromotion()
        {
            ParseSampleByName("Commander promotion");
        }
    }
}
