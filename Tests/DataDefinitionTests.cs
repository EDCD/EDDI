using System;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Tests.Properties;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class DataDefinitionTests : TestBase
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
            Material material = Material.FromName("Cracked Industrial Firmware");
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
            // The same station may use the model "Ocellus" for one event and "Bernal" for another. 

            StationModel model = StationModel.FromEDName("Ocellus");
            Assert.AreEqual("Ocellus", model.basename);
            Assert.AreEqual("Ocellus", model.edname);

            StationModel model2 = StationModel.FromEDName("Bernal");
            Assert.AreEqual("Bernal", model2.basename);
            Assert.AreEqual("Bernal", model2.edname);

            // Regardless of whether the edname is "Ocellus" or "Bernal", the output should be the same.
            Assert.AreEqual(model.invariantName, model2.invariantName);
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
            int grade = 3;
            Blueprint blueprint = Blueprint.FromEDNameAndGrade(blueprintName, grade);
            Assert.IsNotNull(blueprint);
            Assert.AreEqual(grade, blueprint.grade);
            Assert.AreEqual("SensorFastScan", blueprint.blueprintTemplate?.edname);
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
            Assert.AreEqual("SensorFastScan", blueprintFromTemplate.blueprintTemplate.edname);
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
            Assert.AreEqual("EngineDirty", blueprint.blueprintTemplate?.edname);
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
        public void TestVehicleProperties()
        {
            JObject json = DeserializeJsonResource<JObject>(Resources.vehicle);
            Vehicle v0 = Vehicle.FromJson(0, json);
            Assert.AreEqual(0, v0.subslot, "testing v0 subslot from JSON");
            Assert.AreEqual(v0.localizedName, "SRV Scarab");
            Assert.AreEqual(v0.localizedDescription, "dual plasma repeaters");

            Vehicle v1 = Vehicle.FromJson(1, json);
            Assert.AreEqual(1, v1.subslot, "testing v1 subslot from JSON");
            Assert.AreEqual(0, v0.subslot, "testing v0 subslot after setting v1 subslot");
        }

        [TestMethod]
        public void StationServiceISFTest()
        {
            StationService journal = StationService.FromEDName("Facilitator");
            StationService edsm = StationService.FromName("Interstellar Factors Contact");

            Assert.AreEqual("Facilitator", edsm.edname);
            Assert.AreEqual("Interstellar Factors Contact", journal.invariantName);
            Assert.AreEqual(journal.edname, edsm.edname);
            Assert.AreEqual(journal.invariantName, edsm.invariantName);
        }

        [TestMethod]
        public void FrontierApiCmdrTest()
        {
            Commander commander = new Commander()
            {
                name = "Marty McFly",
                title = "Serf",
                combatrating = CombatRating.FromRank(3),
                traderating = TradeRating.FromRank(3),
                explorationrating = ExplorationRating.FromRank(2),
                cqcrating = CQCRating.FromRank(2),
                empirerating = EmpireRating.FromRank(2),
                federationrating = FederationRating.FromRank(1),
                crimerating = 0,
                servicerating = 0,
                powerrating = 2,
                credits = 0,
                debt = 0
            };
            FrontierApiCommander frontierApiCommander = new FrontierApiCommander()
            {
                name = "Marty McFly",
                combatrating = CombatRating.FromRank(3),
                traderating = TradeRating.FromRank(2),
                explorationrating = ExplorationRating.FromRank(2),
                cqcrating = CQCRating.FromRank(2),
                empirerating = EmpireRating.FromRank(4),
                federationrating = FederationRating.FromRank(2),
                crimerating = 2,
                servicerating = 2,
                powerrating = 3,
                credits = 246486105,
                debt = 24684
            };
            FrontierApiCommander frontierApiCommander2 = new FrontierApiCommander()
            {
                name = "Doc Brown",
                combatrating = CombatRating.FromRank(0),
                traderating = TradeRating.FromRank(6),
                explorationrating = ExplorationRating.FromRank(7),
                cqcrating = CQCRating.FromRank(0),
                empirerating = EmpireRating.FromRank(1),
                federationrating = FederationRating.FromRank(2),
                crimerating = 1,
                servicerating = 1,
                powerrating = 7,
                credits = 189687,
                debt = 0
            };

            DateTime apiDateTime = DateTime.UtcNow;
            DateTime journalDateTime = DateTime.UtcNow.AddHours(1);

            Commander test1 = Commander.FromFrontierApiCmdr(commander, frontierApiCommander, apiDateTime, journalDateTime, out bool cmdr1Matches);
            
            Assert.IsTrue(cmdr1Matches);
            Assert.AreEqual("Marty McFly", test1.name);
            Assert.AreEqual("Serf", test1.title);
            Assert.AreEqual(246486105, test1.credits);
            Assert.AreEqual(24684, test1.debt);
            Assert.AreEqual(2, test1.crimerating);
            Assert.AreEqual(3, test1.combatrating.rank);
            Assert.AreEqual(3, test1.traderating.rank);
            Assert.AreEqual(2, test1.explorationrating.rank);
            Assert.AreEqual(2, test1.cqcrating.rank);
            Assert.AreEqual(4, test1.empirerating.rank);
            Assert.AreEqual(2, test1.federationrating.rank);
            Assert.AreEqual(2, test1.crimerating);
            Assert.AreEqual(2, test1.servicerating);
            // Since the journal timestamp is greater than the api timestamp, power rating is based off of the journal timestamp
            Assert.AreEqual(2, test1.powerrating);

            // Make the api timestamp fresher than the journal timestamp and re-check the power rating
            apiDateTime = DateTime.UtcNow.AddHours(2);
            Commander test2 = Commander.FromFrontierApiCmdr(commander, frontierApiCommander, apiDateTime, journalDateTime, out bool cmdr2Matches);
            Assert.IsTrue(cmdr2Matches);
            Assert.AreEqual(3, test2.powerrating);

            // Test Frontier API commander details that do not match our base commander name
            // The base commander properties should remain unchanged
            Commander test3 = Commander.FromFrontierApiCmdr(commander, frontierApiCommander2, apiDateTime, journalDateTime, out bool cmdr3Matches);
            Assert.IsFalse(cmdr3Matches);
            Assert.AreEqual("Marty McFly", test3.name);
            Assert.AreEqual("Serf", test3.title);
            Assert.AreEqual(0, test3.credits);
            Assert.AreEqual(0, test3.debt);
            Assert.AreEqual(0, test3.crimerating);
            Assert.AreEqual(3, test3.combatrating.rank);
            Assert.AreEqual(3, test3.traderating.rank);
            Assert.AreEqual(2, test3.explorationrating.rank);
            Assert.AreEqual(2, test3.cqcrating.rank);
            Assert.AreEqual(2, test3.empirerating.rank);
            Assert.AreEqual(1, test3.federationrating.rank);
            Assert.AreEqual(0, test3.crimerating);
            Assert.AreEqual(0, test3.servicerating);
            Assert.AreEqual(2, test3.powerrating);
        }
    }
}
