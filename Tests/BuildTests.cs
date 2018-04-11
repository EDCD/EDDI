using Eddi;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class BuildTests
    {
        [TestMethod]
        public void TestResponders()
        {
            int numResponders = EDDI.Instance.findResponders().Count;
            Assert.IsTrue(numResponders > 0);
        }


        [TestMethod]
        public void TestMonitors()
        {
            int numMonitors = EDDI.Instance.findMonitors().Count;
            Assert.IsTrue(numMonitors > 0);
        }
    }
}
