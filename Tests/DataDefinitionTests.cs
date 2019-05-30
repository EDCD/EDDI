using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class DataDefinitionTests
    {
        [TestMethod]
        public void TestDataDefinitionReactiveArmour()
        {
            CommodityDefinition commodity = CommodityDefinition.FromEDName("$ReactiveArmour_Name;");
            Assert.AreEqual("Reactive Armour", commodity.invariantName);
            Assert.IsNotNull(commodity.EDDBID);
        }

        [TestMethod]
        public void TestDataDefinitionUnknownName()
        {
            CommodityDefinition commodity = CommodityDefinition.FromEDName("$MagicStuff_Name;");
            Assert.AreEqual("magicstuff", commodity.invariantName);
            Assert.IsNull(commodity.EDDBID);
        }

        [TestMethod]
        public void TestDataDefinitionMaterialName1()
        {
            Material material  = Material.FromName("Cracked Industrial Firmware");
            Assert.AreEqual("Cracked Industrial Firmware", material.invariantName);
            Assert.IsNotNull(material.rarity);
        }

        [TestMethod]
        public void TestDataDefinitionMaterialName2()
        {
            Material material = Material.FromName("Niobium");
            Assert.AreEqual("Niobium", material.invariantName);
            Assert.IsNotNull(material.rarity);
            Assert.AreEqual("Element", material.category.invariantName);
        }

        [TestMethod]
        public void TestGovernmentKnownName()
        {
            Government govt = Government.FromName("Anarchy");
            Assert.AreEqual("Anarchy", govt.basename);
            Assert.AreEqual("Anarchy", govt.invariantName);
        }

        [TestMethod]
        public void TestGovernmentKnownEDName()
        {
            Government govt = Government.FromEDName("$government_Anarchy");
            Assert.AreEqual("Anarchy", govt.basename);
            Assert.AreEqual("Anarchy", govt.invariantName);
        }

        [TestMethod]
        public void TestGovernmentMissingNameFallback()
        {
            Government govt = Government.FromEDName("$government_NoSuchAnimal");
            Assert.AreEqual("NoSuchAnimal", govt.basename);
            Assert.AreEqual("NoSuchAnimal", govt.localizedName);
        }

        [TestMethod]
        public void TestMaterialsExquisiteFocusCrystals()
        {
            Material efc = Material.FromName("Exquisite Focus Crystals");
            Assert.AreEqual("exquisitefocuscrystals", efc.basename);
            Assert.AreEqual("Exquisite Focus Crystals", efc.localizedName);
            Assert.AreEqual("very rare", efc.rarity.invariantName);
        }

        [TestMethod]
        public void TestCommodityTea()
        {
            CommodityDefinition trinket = CommodityDefinition.FromEDName("Tea");
            Assert.AreEqual("Tea", trinket.edname);
            Assert.AreEqual("Tea", trinket.invariantName);
        }

        [TestMethod]
        public void TestCommodityDrones()
        {
            CommodityDefinition trinket = CommodityDefinition.FromEDName("Drones");
            Assert.AreEqual("Drones", trinket.edname);
            Assert.AreEqual("Limpet", trinket.invariantName);
            Assert.AreEqual("Non-marketable", trinket.category.invariantName);
        }

        [TestMethod]
        public void TestCommodityTrinketsOfFortune()
        {
            CommodityDefinition trinket = CommodityDefinition.FromEDName("TrinketsOfFortune");
            Assert.AreEqual("TrinketsOfFortune", trinket.edname);
            Assert.AreEqual("Trinkets Of Hidden Fortune", trinket.invariantName);
        }

        [TestMethod]
        public void TestOcellusStationModel()
        {
            StationModel model = StationModel.FromEDName("Ocellus");
            Assert.AreEqual("Bernal", model.basename);
            Assert.AreEqual("Ocellus", model.edname);
        }

        [TestMethod]
        public void TestEmptySystemGreenGold()
        {
            StarSystem starSystem = new StarSystem() { systemname = "testSystem" };
            Assert.IsFalse((bool)starSystem.isgreen, "empty system should not be green");
            Assert.IsFalse((bool)starSystem.isgold, "empty system should not be gold");
        }

        [TestMethod]
        public void TestGreenSystemGreenGold()
        {
            // Set up a body which would trigger the green system condition
            List<MaterialPresence> materials = new List<MaterialPresence>();
            materials.Add(new MaterialPresence(Material.FromEDName("carbon"), 0.01M));
            materials.Add(new MaterialPresence(Material.FromEDName("germanium"), 0.01M));
            materials.Add(new MaterialPresence(Material.FromEDName("vanadium"), 0.01M));
            materials.Add(new MaterialPresence(Material.FromEDName("cadmium"), 0.01M));
            materials.Add(new MaterialPresence(Material.FromEDName("niobium"), 0.01M));
            materials.Add(new MaterialPresence(Material.FromEDName("arsenic"), 0.01M));
            materials.Add(new MaterialPresence(Material.FromEDName("yttrium"), 0.01M));
            materials.Add(new MaterialPresence(Material.FromEDName("polonium"), 0.01M));
            Body jumponiumBody = new Body()
            {
                bodyname = "Jumponium Haven",
                materials = materials
            };

            // Add a body with green materials and re-test
            StarSystem starSystem = new StarSystem() { systemname = "testSystem" };
            starSystem.bodies.Add(jumponiumBody);
            Assert.IsTrue((bool)starSystem.isgreen, "green system should be green");
            Assert.IsFalse((bool)starSystem.isgold, "green system should be gold");
        }

        [TestMethod]
        public void TestGoldSystemGreenGold()
        {
            // Set up a body which would trigger the gold system condition
            List<MaterialPresence> materials = new List<MaterialPresence>();
            foreach (Material material in Material.surfaceElements)
            {
                materials.Add(new MaterialPresence(material, 0.01M));
            }
            Body goldBody = new Body()
            {
                bodyname = "Golden",
                materials = materials
            };

            // Add a body with gold materials and re-test
            StarSystem starSystem = new StarSystem() { systemname = "testSystem" };
            starSystem.bodies.Add(goldBody);
            Assert.IsTrue((bool)starSystem.isgreen, "gold system should be green");
            Assert.IsTrue((bool)starSystem.isgold, "gold system should be gold");
        }
    }
}
