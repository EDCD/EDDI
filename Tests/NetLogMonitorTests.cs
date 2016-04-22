using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDDIVAPlugin;
using EliteDangerousNetLogMonitor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;


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

        [TestMethod]
        public void TestRegex()
        {
            String line = @"{12:43:49} System:28(Training) Body:7 Pos:(1.08967e+007,833.411,5.93693e+006) NormalFlight";
            Regex SystemRegex = new Regex(@"^{([0-9][0-9]:[0-9][0-9]:[0-9][0-9])} System:([0-9]+)\(([^\)]+)\).* ([A-Za-z]+)$");
            Match match = SystemRegex.Match(line);
            Assert.IsTrue(match.Success);
            Console.Out.WriteLine(match.Groups[0].Value);
            Console.Out.WriteLine(match.Groups[1].Value);
            Console.Out.WriteLine(match.Groups[2].Value);
            Console.Out.WriteLine(match.Groups[3].Value);
            Console.Out.WriteLine(match.Groups[4].Value);
            Assert.AreEqual("Training", match.Groups[3].Value);
        }
    }
}
