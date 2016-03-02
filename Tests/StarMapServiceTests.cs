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
            StarMapService service = new StarMapService("secret", "McDonald", "http://beta.edsm.net:8080/");
            service.sendStarMapLog("Sol");

            StarMapInfo info = service.getStarMapInfo("Sol");
            Assert.IsTrue(info.Visits >= 1);  // can be any number thanks to repeated testing
        }

        [TestMethod]
        public void TestProximaCentauriComment()
        {
            StarMapService service = new StarMapService("secret", "McDonald", "http://beta.edsm.net:8080/");
            service.sendStarMapLog("Proxima Centauri");

            service.sendStarMapComment("Proxima Centauri", "Not so far away");

            StarMapInfo info = service.getStarMapInfo("Proxima Centauri");
            Assert.AreEqual("Not so far away", info.Comment);
        }

        [TestMethod]
        public void TestProximaDistance()
        {
            StarMapService service = new StarMapService("secret", "McDonald", "http://beta.edsm.net:8080/");
            service.sendStarMapDistance("Sol", "HIP 11658", 350.35M);
        }

        [TestMethod]
        public void TestGetLogs()
        {
            StarMapService service = new StarMapService("secret", "McDonald", "http://beta.edsm.net:8000/");
            Dictionary<string, StarMapLogInfo> logs = service.getStarMapLog();
            Assert.AreEqual(1, logs.Count);
        }
    }
}
