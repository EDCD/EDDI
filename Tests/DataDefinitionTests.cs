using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        [TestMethod]
        public void TestDataDefinitionMaterialName1()
        {
            Material material  = Material.FromName("cracked industrial firmware");
            Assert.AreEqual("Cracked Industrial Firmware", material.name);
            Assert.IsNotNull(material.rarity);
        }

        [TestMethod]
        public void TestDataDefinitionMaterialName2()
        {
            Material material = Material.FromName("Niobium");
            Assert.AreEqual("Niobium", material.name);
            Assert.IsNotNull(material.rarity);
        }
    }
}
