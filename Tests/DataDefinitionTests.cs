using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
            Assert.IsFalse(starSystem.isgreen, "empty system should not be green");
            Assert.IsFalse(starSystem.isgold, "empty system should not be gold");
        }

        [TestMethod]
        public void TestGreenSystemGreenGold()
        {
            // Set up a body which would trigger the green system condition
            List<MaterialPresence> materials = new List<MaterialPresence>();
            foreach (Material material in Material.jumponiumElements)
            {
                materials.Add(new MaterialPresence(material, 0.01M));
            }
            Body jumponiumBody = new Body()
            {
                bodyname = "Jumponium Haven",
                materials = materials
            };

            // Add a body with green materials and re-test
            StarSystem starSystem = new StarSystem() { systemname = "testSystem" };
            starSystem.AddOrUpdateBody(jumponiumBody);
            Assert.IsTrue(starSystem.isgreen, "green system should be green");
            Assert.IsFalse(starSystem.isgold, "green system should not be gold");
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
            starSystem.AddOrUpdateBody(goldBody);
            Assert.IsTrue(starSystem.isgreen, "gold system should be green");
            Assert.IsTrue(starSystem.isgold, "gold system should be gold");
        }

        [TestMethod]
        public void TestAddOrUpdateBodyAdd()
        {
            StarSystem starSystem = new StarSystem() { systemname = "testSystem" };
            Body body = new Body() { bodyname = "testSystem 1" };

            starSystem.AddOrUpdateBody(body);

            Assert.AreEqual(1, starSystem.bodies.Count);
        }

        [TestMethod]
        public void TestAddOrUpdateBodyUpdate()
        {
            StarSystem starSystem = new StarSystem() { systemname = "testSystem" };
            Body body = new Body() { bodyname = "testSystem 1" };
            starSystem.AddOrUpdateBody(body);
            BodyType moon = BodyType.FromEDName("Moon");
            Body updatedBody = new Body() { bodyname = "testSystem 1", bodyType = moon };

            starSystem.AddOrUpdateBody(updatedBody);

            Assert.AreEqual(1, starSystem.bodies.Count);
            Body actualBody = starSystem.bodies[0];
            Assert.AreEqual("testSystem 1", actualBody.bodyname);
            Assert.AreEqual(moon, actualBody.bodyType);
        }

        [TestMethod]
        public void TestBlueprintFromEdNameAndGrade()
        {
            string blueprintName = "WakeScanner_Fast Scan_3";
            string blueprintTemplate = "Sensor_FastScan";
            int grade = 3;
            Blueprint blueprint = Blueprint.FromEDNameAndGrade(blueprintName, grade);
            Assert.IsNotNull(blueprint);
            Assert.AreEqual(grade, blueprint.grade);
            Assert.AreEqual(blueprintTemplate, blueprint.blueprintTemplate?.edname);
            Assert.AreEqual(3, blueprint.materials.Count);
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("phosphorus"));
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("uncutfocuscrystals"));
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("symmetrickeys"));
        }

        [TestMethod]
        public void TestBlueprintFromTemplateEdNameAndGrade()
        {
            // We should also be able to handle receiving a template name rather than a blueprint name while still providing essential info.
            string blueprintTemplate = "Sensor_FastScan";
            int grade = 3;
            Blueprint blueprintFromTemplate = Blueprint.FromEDNameAndGrade(blueprintTemplate, grade);
            Assert.IsNotNull(blueprintFromTemplate);
            Assert.AreEqual(grade, blueprintFromTemplate.grade);
            Assert.AreEqual(blueprintTemplate, blueprintFromTemplate.blueprintTemplate.edname);
            Assert.AreEqual(3, blueprintFromTemplate.materials.Count);
            string[] materials = blueprintFromTemplate.materials.Select(m => m.edname).ToArray();
            Assert.IsTrue(materials.Contains("phosphorus"));
            Assert.IsTrue(materials.Contains("uncutfocuscrystals"));
            Assert.IsTrue(materials.Contains("symmetrickeys"));
        }

        [TestMethod]
        public void TestBlueprintNameAndGrade()
        {
            string blueprintName = "Dirty Drive Tuning";
            int grade = 5;
            Blueprint blueprint = Blueprint.FromNameAndGrade(blueprintName, grade);
            Assert.IsNotNull(blueprint);
            Assert.AreEqual(128673659, blueprint.blueprintId);
            Assert.AreEqual(grade, blueprint.grade);
            Assert.AreEqual("Engine_Dirty", blueprint.blueprintTemplate?.edname);
            Assert.AreEqual(3, blueprint.materials.Count);
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("industrialfirmware"));
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("cadmium"));
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("pharmaceuticalisolators"));
        }

        [TestMethod]
        public void TestBlueprintBadNameAndGrade()
        {
            string blueprintName = "No such blueprint";
            int grade = 5;
            Blueprint blueprint = Blueprint.FromNameAndGrade(blueprintName, grade);
            Assert.IsNull(blueprint);
        }

        [TestMethod]
        public void TestBlueprintFromBlueprintID()
        {
            long blueprintId = 128740124;
            Blueprint blueprint = Blueprint.FromEliteID(blueprintId);
            Assert.IsNotNull(blueprint);
            Assert.AreEqual(3, blueprint.grade);
            Assert.IsNotNull(blueprint.blueprintTemplate);
            Assert.AreEqual(3, blueprint.materials.Count);
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("phosphorus"));
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("uncutfocuscrystals"));
            Assert.IsTrue(blueprint.materials.Select(m => m.edname).Contains("symmetrickeys"));
        }

        [TestMethod]
        public void TestBlueprintFromBadBlueprintID()
        {
            long blueprintId = -1;
            Blueprint blueprint = Blueprint.FromEliteID(blueprintId);
            Assert.IsNull(blueprint);
        }

        [TestMethod]
        [DeploymentItem("vehicle.json")]
        public void TestVehicleProperties()
        {
            string jsonString = File.ReadAllText("vehicle.json");
            JObject json = JsonConvert.DeserializeObject<JObject>(jsonString);
            Vehicle v0 = Vehicle.FromJson(0, json);
            Assert.AreEqual(0, v0.subslot, "testing v0 subslot from JSON");
            Assert.AreEqual(v0.localizedName, "SRV Scarab");
            Assert.AreEqual(v0.localizedDescription, "dual plasma repeaters");

            Vehicle v1 = Vehicle.FromJson(1, json);
            Assert.AreEqual(1, v1.subslot, "testing v1 subslot from JSON");
            Assert.AreEqual(0, v0.subslot, "testing v0 subslot after setting v1 subslot");
        }
    }
}
