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
        public void TestOldRegex()
        {
            string line = @"{12:43:49} System:28(Training) Body:7 Pos:(1.08967e+007,833.411,5.93693e+006) NormalFlight";
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

        [TestMethod]
        public void TestRegex()
        {
            string line = @"{19:24:56} System:""Wolf 397"" StarPos:(40.000,79.219,-10.406)ly Body:23 RelPos:(-2.01138,1.32957,1.7851)km NormalFlight";
//            Regex SystemRegex = new Regex(@"^{([0-9][0-9]:[0-9][0-9]:[0-9][0-9])} System:""([^""]+)"" StarPos:\((-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+)\)ly Body:([0-9]+) RelPos:\((-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+)\)km ([A-Za-z]+)$");
            Regex SystemRegex = new Regex(@"^{([0-9][0-9]:[0-9][0-9]:[0-9][0-9])} System:""([^""]+)"" StarPos:\((-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+)\)ly .*? ([A-Za-z]+)$");
            Match match = SystemRegex.Match(line);
            Assert.IsTrue(match.Success);
            Console.Out.WriteLine(match.Groups[0].Value);
            Console.Out.WriteLine(match.Groups[1].Value);
            Console.Out.WriteLine(match.Groups[2].Value);
            Console.Out.WriteLine(match.Groups[3].Value);
            Console.Out.WriteLine(match.Groups[4].Value);
            Console.Out.WriteLine(match.Groups[5].Value);
            Console.Out.WriteLine(match.Groups[6].Value);
            Assert.AreEqual("NormalFlight", match.Groups[6].Value);
        }

        [TestMethod]
        public void TestRegex2()
        {
            string line = @"{14:52:32} System:""LTT 7251"" StarPos:(-16.688,-8.094,116.344)ly  NormalFlight";
            Regex SystemRegex = new Regex(@"^{([0-9][0-9]:[0-9][0-9]:[0-9][0-9])} System:""([^""]+)"" StarPos:\((-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+)\)ly .*? ([A-Za-z]+)$");
            Match match = SystemRegex.Match(line);
            Assert.IsTrue(match.Success);
            Console.Out.WriteLine(match.Groups[0].Value);
            Console.Out.WriteLine(match.Groups[1].Value);
            Console.Out.WriteLine(match.Groups[2].Value);
            Console.Out.WriteLine(match.Groups[3].Value);
            Console.Out.WriteLine(match.Groups[4].Value);
            Console.Out.WriteLine(match.Groups[5].Value);
            Console.Out.WriteLine(match.Groups[6].Value);
            Assert.AreEqual("NormalFlight", match.Groups[6].Value);
        }

        [TestMethod]
        public void TestRegex3()
        {
            string line = @"{15:01:14} System:""Laksak"" StarPos:(-21.531,-6.313,116.031)ly  Supercruise";
            Regex SystemRegex = new Regex(@"^{([0-9][0-9]:[0-9][0-9]:[0-9][0-9])} System:""([^""]+)"" StarPos:\((-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+)\)ly .*? ([A-Za-z]+)$");
            Match match = SystemRegex.Match(line);
            Assert.IsTrue(match.Success);
            Console.Out.WriteLine(match.Groups[0].Value);
            Console.Out.WriteLine(match.Groups[1].Value);
            Console.Out.WriteLine(match.Groups[2].Value);
            Console.Out.WriteLine(match.Groups[3].Value);
            Console.Out.WriteLine(match.Groups[4].Value);
            Console.Out.WriteLine(match.Groups[5].Value);
            Console.Out.WriteLine(match.Groups[6].Value);
            Assert.AreEqual("Supercruise", match.Groups[6].Value);
        }
    }
}
