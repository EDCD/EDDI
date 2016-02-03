using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EliteDangerousStarMapService;

namespace Tests
{
    [TestClass]
    public class StarMapServiceTests
    {
        [TestMethod]
        public void TestSolLog()
        {
            StarMapService service = new StarMapService("8023ed4044b074115af1dd85bc8920e0ef072cb1", "Test");
            service.sendStarMapLog("Sol");

            StarMapInfo info = service.getStarMapInfo("Sol");
            Assert.IsTrue(info.Visits >= 1);  // can be any number thanks to repeated testing
        }

        [TestMethod]
        public void TestProximaCentauriComment()
        {
            StarMapService service = new StarMapService("8023ed4044b074115af1dd85bc8920e0ef072cb1", "Test");
            service.sendStarMapLog("Proxima Centauri");

            service.sendStarMapComment("Proxima Centauri", "Not so far away");

            StarMapInfo info = service.getStarMapInfo("Proxima Centauri");
            Assert.AreEqual("Not so far away", info.Comment);
        }
    }
}
