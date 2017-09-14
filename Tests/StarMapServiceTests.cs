using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiStarMapService;

namespace Tests
{
    [TestClass]
    public class StarMapServiceTests
    {
        [TestMethod]
        public void TestSolLog()
        {
            StarMapService service = new StarMapService("secret", "McDonald", "http://beta.edsm.net:8080/");
            service.sendStarMapLog(DateTime.Now, "Sol", null, null, null);

            StarMapInfo info = service.getStarMapInfo("Sol");
            Assert.IsTrue(info.Visits >= 1);  // can be any number thanks to repeated testing
        }

        [TestMethod]
        public void TestProximaCentauriComment()
        {
            StarMapService service = new StarMapService("secret", "McDonald", "http://beta.edsm.net:8080/");
            service.sendStarMapLog(DateTime.Now, "Proxima Centauri", null, null, null);

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
            StarMapService service = new StarMapService("secret", "McDonald", "http://beta.edsm.net:8080/");
            Dictionary<string, StarMapLogInfo> logs = service.getStarMapLog();
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestWriteDecimal()
        {
            decimal val1 = 0.735m;
            Assert.AreEqual("0,735", val1.ToString("0.000", new System.Globalization.CultureInfo("fr-FR")));
            Assert.AreEqual("0.735", val1.ToString("0.000", new System.Globalization.CultureInfo("en-US")));
            decimal val2 = 1234.735m;
            Assert.AreEqual("1234,735", val2.ToString("0.000", new System.Globalization.CultureInfo("fr-FR")));
            Assert.AreEqual("1234.735", val2.ToString("0.000", new System.Globalization.CultureInfo("en-US")));
        }
    }
}
