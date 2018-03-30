using Eddi;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class BuildTests
    {
        [TestMethod]
        public void TestResponders()
        {
            Assert.IsTrue(EDDI.Instance.findResponders().Count > 0);
        }


        [TestMethod]
        public void TestMonitors()
        {
            Assert.IsTrue(EDDI.Instance.findMonitors().Count > 0);
        }
    }
}
