using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDDIVAPlugin;
using EliteDangerousNetLogMonitor;
using System;

namespace Tests
{
    [TestClass]
    public class NetLogMonitorTests
    {
        [TestMethod]
        public void TestFinder()
        {
            string[] paths = new Finder().FindInstallationPaths();
            foreach (string path in paths)
            {
                Console.WriteLine(path);
            }
            Assert.AreEqual("on the way to 12 and a half thousand", VoiceAttackPlugin.humanize(12345));
        }
    }
}
