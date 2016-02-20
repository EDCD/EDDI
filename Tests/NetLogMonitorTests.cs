using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDDIVAPlugin;
using EliteDangerousNetLogMonitor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tests
{
    [TestClass]
    public class NetLogMonitorTests
    {
        [TestMethod]
        public void TestNetLogConfiguration()
        {
            NetLogConfiguration configuration = JsonConvert.DeserializeObject<NetLogConfiguration>("{\"path\":\"test\"}");
            Assert.AreEqual("test", configuration.path);
        }

        [TestMethod]
        public void TestNetLogConfiguration2()
        {
            NetLogConfiguration configuration = NetLogConfiguration.FromFile();
            Assert.AreEqual("C:\\Program Files (x86)\\Elite\\Products\\elite-dangerous-64\\Logs", configuration.path);
        }
    }
}
