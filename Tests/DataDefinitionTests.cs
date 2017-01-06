using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Eddi;
using EddiDataDefinitions;

namespace Tests
{
    [TestClass]
    public class DataDefinitionTests
    {
        [TestMethod]
        public void TestDataDefinitionReactiveArmour()
        {
            Commodity commodity = CommodityDefinitions.FromName("$ReactiveArmour_Name;");
            Assert.AreEqual(commodity.name, "Reactive Armour");
            Assert.IsNotNull(commodity.EDDBID);
        }

        [TestMethod]
        public void TestDataDefinitionUnknownName()
        {
            Commodity commodity = CommodityDefinitions.FromName("$MagicStuff_Name;");
            Assert.AreEqual("Magic Stuff", commodity.name);
            Assert.AreEqual(0, commodity.EDDBID);
        }
    }
}
