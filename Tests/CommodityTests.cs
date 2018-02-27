using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using EddiDataDefinitions;

namespace Tests
{
    [TestClass]
    public class CommodityTests
    {
        [TestMethod]
        public void TestMalformedCommodityName()
        {
            string malformedCommodityName = "I gotta quote\" and a backslash\\, I'm really bad.";
            var badCommoditity = CommodityDefinitions.FromName(malformedCommodityName);
            Assert.AreEqual(badCommoditity.name, malformedCommodityName);
        }
    }
}
