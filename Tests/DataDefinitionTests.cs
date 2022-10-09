using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Properties;
using Utilities;

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
            Assert.IsNotNull(material.Rarity);
        }

        [TestMethod]
        public void TestDataDefinitionMaterialName2()
        {
            Material material = Material.FromName("Niobium");
            Assert.AreEqual("Niobium", material.invariantName);
            Assert.IsNotNull(material.Rarity);
            Assert.AreEqual("Element", material.Category.invariantName);
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
            Assert.AreEqual("very rare", efc.Rarity.invariantName);
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
            Assert.AreEqual("Non-marketable", trinket.Category.invariantName);
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
            Assert.AreEqual(v0.localizedName, "Scarab SRV");
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
        public void StationServiceLegacyTest()
        {
            // In Elite Dangerous v3.7, the edName "Workshop" is replaced by "Engineer" and the edName "SearchAndRescue" is replaced by "SearchRescue".
            // Test for backwards and forwards compatibility.
            StationService workshop = StationService.FromEDName("Workshop");
            StationService engineer = StationService.FromEDName("Engineer");

            StationService searchAndRescue = StationService.FromEDName("SearchAndRescue");
            StationService searchRescue = StationService.FromEDName("SearchRescue");

            Assert.IsTrue(workshop.DeepEquals(engineer));
            Assert.IsTrue(searchAndRescue.DeepEquals(searchRescue));
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
                mercenaryrating = MercenaryRating.Defenceless,
                exobiologistrating = ExobiologistRating.Directionless,
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
                mercenaryrating = MercenaryRating.Rookie,
                exobiologistrating = ExobiologistRating.Directionless,
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
                mercenaryrating = MercenaryRating.Defenceless,
                exobiologistrating = ExobiologistRating.Geneticist,
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
            Assert.AreEqual(2, test1.mercenaryrating.rank);
            Assert.AreEqual(0, test1.exobiologistrating.rank);
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
            Assert.AreEqual(0, test3.mercenaryrating.rank);
            Assert.AreEqual(0, test3.exobiologistrating.rank);
            Assert.AreEqual(0, test3.crimerating);
            Assert.AreEqual(0, test3.servicerating);
            Assert.AreEqual(2, test3.powerrating);
        }

        [TestMethod]
        public void TestExtensionMethods()
        {
            StarSystem system1 = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1);
            StarSystem system2 = system1.Copy();
            StarSystem system3 = system1;

            Assert.AreEqual(127.0M, system1.x);

            // system 1 and system2 are copies of one another. They do not share a reference to the same object.
            Assert.IsFalse(system1.Equals(system2));
            Assert.IsTrue(system1.DeepEquals(system2));

            // system1 and system3 are both references to the same object.
            Assert.IsTrue(system1.Equals(system3));

            // sqlStarSystem1 and system2 are copies of one another, except one property has been altered.
            // They do not share a reference to the same object and properties altered on system2 are not also altered on system1.
            // Re-constituting the system from the source confirms the difference.
            system2.x = null;
            Assert.AreEqual(127.0M, system1.x);
            Assert.IsFalse(system1.Equals(system2));
            Assert.IsFalse(system1.DeepEquals(system2));
            Assert.IsFalse(DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1).Equals(system2));

            // system1 and system3 are still both references to the same object, so they are still equal.
            // The property altered on system3 is also altered on system1.
            // We have to re-constitute the system from the source to identify the difference.
            system3.x = null;
            Assert.AreEqual(null, system1.x);
            Assert.IsTrue(system1.Equals(system3));
            Assert.IsTrue(system1.DeepEquals(system3));
            Assert.IsFalse(DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1).Equals(system3));
        }

        [TestMethod]
        public void TestCommodityMarketQuoteFromCAPIjson()
        {
            string line = @" {""id"":128066403,""categoryname"":""NonMarketable"",""name"":""Drones"",""stock"":9999999,""buyPrice"":101,""sellPrice"":101,""demand"":9999999,""legality"":"""",""meanPrice"":101,""demandBracket"":2,""stockBracket"":2,""locName"":""Limpet""} ";
            var eddnQuote = JsonConvert.DeserializeObject<MarketInfoItem>(line);

            // Test that the MarketInfoItem can be sucessfully converted to a definition based CommodityMarketQuote
            var quote = eddnQuote.ToCommodityMarketQuote();
            Assert.AreEqual(128066403, quote.definition.EliteID);
            Assert.AreEqual("Drones", quote.definition.edname);
            Assert.AreEqual("Non-marketable", quote.definition.Category.invariantName);
            Assert.AreEqual(101, quote.buyprice);
            Assert.AreEqual(101, quote.sellprice);
            Assert.AreEqual(101, quote.avgprice);
            Assert.AreEqual(CommodityBracket.Medium, quote.stockbracket);
            Assert.AreEqual(CommodityBracket.Medium, quote.demandbracket);
            Assert.AreEqual(9999999, quote.stock);
            Assert.AreEqual(9999999, quote.demand);
        }

        [TestMethod]
        public void TestCommodityMarketQuoteFromMarketJson()
        {
            var info = DeserializeJsonResource<MarketInfo>(Resources.market);
            var expectedMarketInfoItems = new List<MarketInfoItem>()
            { 
                new MarketInfoItem(128668550, "$painite_name;", "$MARKET_category_minerals;", 0, 500096, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 200),
                new MarketInfoItem(128673846, "$bromellite_name;", "$MARKET_category_minerals;", 0, 10009, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 100 ),
                new MarketInfoItem(128673848, "$lowtemperaturediamond_name;", "$MARKET_category_minerals;", 0, 500553, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 150 ),
                new MarketInfoItem(128924330, "$grandidierite_name;", "$MARKET_category_minerals;", 0, 424204, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 100),
                new MarketInfoItem(128924331, "$alexandrite_name;", "$MARKET_category_minerals;", 0, 348192, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 97),
                new MarketInfoItem(128924332, "$opal_name;", "$MARKET_category_minerals;", 0, 1014218, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 300)
            };

            Assert.AreEqual(3702012928, info.MarketID);
            Assert.AreEqual("X9K-WTW", info.StationName);
            Assert.AreEqual("Gateway", info.StarSystem);
            Assert.AreEqual(expectedMarketInfoItems.Count, info.Items.Count);

            foreach (var item in info.Items)
            {
                // Test that all items match expected definitions
                if (!expectedMarketInfoItems.Exists(i => i.DeepEquals(item)))
                {
                    Assert.Fail();
                }
            }

            // Test that the MarketInfoItem can be sucessfully converted to a definition based CommodityMarketQuote
            var quote = info.Items[0].ToCommodityMarketQuote();
            Assert.AreEqual(128668550, quote.definition.EliteID);
            Assert.AreEqual("Painite", quote.definition.edname);
            Assert.AreEqual("Minerals", quote.definition.Category.invariantName);
            Assert.AreEqual(0, quote.buyprice);
            Assert.AreEqual(500096, quote.sellprice);
            Assert.AreEqual(CommodityDefinition.FromEDName("painite")?.avgprice, quote.avgprice); // Carriers always return an average price of zero. Verify this marches our commodity definition instead. 
            Assert.AreEqual(CommodityBracket.None, quote.stockbracket);
            Assert.AreEqual(CommodityBracket.Medium, quote.demandbracket);
            Assert.AreEqual(0, quote.stock);
            Assert.AreEqual(200, quote.demand);
            Assert.AreEqual("Consumer", quote.StatusFlags.First());
        }

        [TestMethod]
        public void TestDataScanFromEDName()
        {
            DataScan dataScan = DataScan.FromEDName("$Datascan_DataPoint;");
            Assert.AreEqual("Data Point", dataScan.invariantName);
        }

        [TestMethod]
        public void TestSystemFactionParsing()
        {
            string line = @"{ ""timestamp"":""2020-05-05T06:02:31Z"", ""event"":""FSDJump"", ""StarSystem"":""HIP 19072"", ""SystemAddress"":525873416523, ""StarPos"":[-54.28125,-121.03125,-323.06250], ""SystemAllegiance"":""Alliance"", ""SystemEconomy"":""$economy_Service;"", ""SystemEconomy_Localised"":""Service"", ""SystemSecondEconomy"":""$economy_Undefined;"", ""SystemSecondEconomy_Localised"":""Unknown"", ""SystemGovernment"":""$government_Democracy;"", ""SystemGovernment_Localised"":""Democracy"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":25000, ""Body"":""HIP 19072"", ""BodyID"":0, ""BodyType"":""Star"", ""JumpDist"":20.477, ""FuelUsed"":10.372852, ""FuelLevel"":19.971060, ""Factions"":[ { ""Name"":""The Ant Hill Mob"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.294000, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000 }, { ""Name"":""Cooper Research Associates"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.706000, ""Allegiance"":""Alliance"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000, ""ActiveStates"":[ { ""State"":""Boom"" } ] }, { ""Name"":""Pilots' Federation Local Branch"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.000000, ""Allegiance"":""PilotsFederation"", ""Happiness"":"""", ""MyReputation"":100.000000 } ], ""SystemFaction"":{ ""Name"":""Cooper Research Associates"", ""FactionState"":""Boom"" } }";

            var expectedSystemFaction = new Faction()
            {
                name = "Cooper Research Associates",
                Allegiance = Superpower.Alliance,
                Government = Government.FromEDName("$government_Democracy;"),
                presences = new List<FactionPresence> 
                { 
                    new FactionPresence 
                    { 
                        systemName = "HIP 19072", 
                        FactionState = FactionState.FromEDName("Boom")
                    } 
                },
            };

            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (JumpedEvent)events[0];
            Assert.IsTrue(@event.controllingfaction.DeepEquals(expectedSystemFaction));
        }

        [DataTestMethod]
        [DataRow("citizensuitai_admin", "Administrator", 1)]
        [DataRow("citizensuitai_industrial", "Worker", 1)]
        [DataRow("citizensuitai_scientific", "Scientist", 1)]
        [DataRow("assaultsuitai_class1", "Commando", 1)]
        [DataRow("assaultsuitai_class3", "Commando", 3)]
        [DataRow("closesuitai_class2", "Striker", 2)]
        [DataRow("lightassaultsuitai_class4", "Scout", 4)]
        [DataRow("rangedsuitai_class5", "Sharpshooter", 5)]
        public void TestNpcSuitLoadout(string input, string expectedInvariant, int expectedGrade)
        {
            var result = NpcSuitLoadout.FromEDName(input);
            Assert.AreEqual(expectedInvariant, result.invariantName);
            Assert.AreEqual(expectedGrade, result.grade);
        }

        [DataTestMethod]
        [DataRow("ExplorationSuit_Class1", "Artemis Suit", 1)]
        [DataRow("FlightSuit", "Flight Suit", 1)]
        [DataRow("TacticalSuit_Class2", "Dominator Suit", 2)]
        [DataRow("UtilitySuit_Class3", "Maverick Suit", 3)]
        public void TestSuit(string input, string expectedInvariant, int expectedGrade)
        {
            var result = Suit.FromEDName(input);
            Assert.AreEqual(expectedInvariant, result.invariantName);
            Assert.AreEqual(expectedGrade, result.grade);
        }

        [DataTestMethod]
        [DataRow("ChemicalInventory", "Chemical Inventory", "Data")]
        [DataRow("$CompactLibrary_Name;", "Compact Library", "Goods")]
        [DataRow("$healthpack_name;", "Medkit", "Consumables")]
        public void TestMicroResources(string input, string expectedInvariantName, string expectedInvariantCategory)
        {
            var result = MicroResource.FromEDName(input);
            Assert.AreEqual(expectedInvariantName, result.invariantName);
            Assert.AreEqual(expectedInvariantCategory, result.Category.invariantName);
        }

        [TestMethod]
        public void TestEngineerNullLocation()
        {
            // Test that we can gracefully handle situations where we want to look up an engineer location and not all known engineers have a known location.
            Engineer.AddOrUpdate(new Engineer("NoSuchEngineer", 999999, "Known", null, null));
            try
            {
                var engineer = Engineer.FromSystemName("NoSuchSystem");
                Assert.IsNull(engineer);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestMicroResourceInfo()
        {
            var json = TestBase.DeserializeJsonResource<string>(Resources.shipLocker);
            var data = Deserializtion.DeserializeData(json);
            var info = new MicroResourceInfo().FromData(data);

            Assert.AreEqual(38, info.Items.Count);
            Assert.AreEqual("WeaponSchematic", info.Items[0].edname);
            Assert.AreEqual(1, info.Items[0].amount);
            Assert.AreEqual("Item", info.Items[0].microResource.Category.edname);
            Assert.AreEqual(770652507, info.Items[0].missionId);
            Assert.AreEqual(0, info.Items[0].ownerId);
            Assert.AreEqual(null, info.Items[0].price);

            Assert.AreEqual(33, info.Components.Count);
            Assert.AreEqual("Graphene", info.Components[1].edname);
            Assert.AreEqual(55, info.Components[1].amount);
            Assert.AreEqual("Component", info.Components[1].microResource.Category.edname);
            Assert.AreEqual(null, info.Components[1].missionId);
            Assert.AreEqual(0, info.Components[1].ownerId);
            Assert.AreEqual(null, info.Components[1].price);

            Assert.AreEqual(6, info.Consumables.Count);
            Assert.AreEqual("HealthPack", info.Consumables[0].edname);
            Assert.AreEqual(56, info.Consumables[0].amount);
            Assert.AreEqual("Consumable", info.Consumables[0].microResource.Category.edname);
            Assert.AreEqual(null, info.Consumables[0].missionId);
            Assert.AreEqual(0, info.Consumables[0].ownerId);
            Assert.AreEqual(null, info.Consumables[0].price);

            Assert.AreEqual(70, info.Data.Count);
            Assert.AreEqual("InternalCorrespondence", info.Data[0].edname);
            Assert.AreEqual(5, info.Data[0].amount);
            Assert.AreEqual("Data", info.Data[0].microResource.Category.edname);
            Assert.AreEqual(null, info.Data[0].missionId);
            Assert.AreEqual(0, info.Data[0].ownerId);
            Assert.AreEqual(null, info.Data[0].price);
        }

        [DataTestMethod]
        [DataRow("Metal-rich body", "Metal-rich body")]
        [DataRow("High metal content world", "High metal content world")]
        [DataRow("Rocky body", "Rocky body")]
        [DataRow("Rocky Ice world", "Rocky ice world")]
        [DataRow("Icy body", "Icy body")]
        [DataRow("Earth-like world", "Earth-like world")]
        [DataRow("Water world", "Water world")]
        [DataRow("Water giant", "Water giant")]
        [DataRow("Water giant with life", "Water giant with life")]
        [DataRow("Ammonia world", "Ammonia world")]
        [DataRow("Gas giant with water-based life", "Gas giant with water based life")]
        [DataRow("Gas giant with ammonia-based life", "Gas giant with ammonia based life")]
        [DataRow("Class I gas giant", "Class I gas giant")]
        [DataRow("Class II gas giant", "Class II gas giant")]
        [DataRow("Class III gas giant", "Class III gas giant")]
        [DataRow("Class IV gas giant", "Class IV gas giant")]
        [DataRow("Class V gas giant", "Class V gas giant")]
        [DataRow("Helium-rich gas giant", "Helium-rich gas giant")]
        [DataRow("Helium gas giant", "Helium gas giant")]
        public void EDSMPlanetClassAliases(string edsmName, string expectedInvariantName)
        {
            Assert.AreEqual(expectedInvariantName, PlanetClass.FromName(edsmName)?.invariantName);
        }

        [DataTestMethod]
        // From `FSSSignalDiscovered` events
        [DataRow("$Aftermath_Large:#index=1;", "Distress Call", 1, 0)]
        [DataRow("$AttackAftermath;", "Distress Call", 0, 0)]
        [DataRow("$FIXED_EVENT_CAPSHIP;", "Capital Ship", 0, 0)]
        [DataRow("$FIXED_EVENT_CHECKPOINT;", "Checkpoint", 0, 0)]
        [DataRow("$FIXED_EVENT_CONVOY;", "Convoy Beacon", 0, 0)]
        [DataRow("$FIXED_EVENT_DEBRIS;", "Debris Field", 0, 0)]
        [DataRow("$FIXED_EVENT_DISTRIBUTIONCENTRE;", "Distribution Center", 0, 0)]
        [DataRow("$FIXED_EVENT_HIGHTHREATSCENARIO_T5;", "Pirate Activity", 0, 0)]
        [DataRow("$FIXED_EVENT_HIGHTHREATSCENARIO_T6;", "Pirate Activity", 0, 0)]
        [DataRow("$FIXED_EVENT_HIGHTHREATSCENARIO_T7;", "Pirate Activity", 0, 0)]
        [DataRow("$Fixed_Event_Life_Cloud;", "Notable Stellar Phenomena", 0, 0)]
        [DataRow("$Fixed_Event_Life_Ring;", "Notable Stellar Phenomena", 0, 0)]
        [DataRow("$Gro_controlScenarioTitle;", "Armed Revolt", 0, 0)]
        [DataRow("$ListeningPost:#index=1;", "Listening Post", 1, 0)]
        [DataRow("$MULTIPLAYER_SCENARIO14_TITLE;", "Resource Extraction Site", 0, 0)]
        [DataRow("$MULTIPLAYER_SCENARIO42_TITLE;", "Nav Beacon", 0, 0)]
        [DataRow("$MULTIPLAYER_SCENARIO77_TITLE;", "Low Intensity Resource Extraction Site", 0, 0)]
        [DataRow("$MULTIPLAYER_SCENARIO78_TITLE;", "High Intensity Resource Extraction Site", 0, 0)]
        [DataRow("$MULTIPLAYER_SCENARIO79_TITLE;", "Hazardous Resource Extraction Site", 0, 0)]
        [DataRow("$MULTIPLAYER_SCENARIO80_TITLE;", "Compromised Nav Beacon", 0, 0)]
        [DataRow("$MULTIPLAYER_SCENARIO81_TITLE;", "Salvageable Wreckage", 0, 0)]
        [DataRow("$NumberStation:#index=2;", "Unregistered Comms Beacon", 2, 0)]
        [DataRow("$USS_Type_NonHuman; $USS_ThreatLevel:#threatLevel=8;", "Non-Human Signal Source", 0, 8)]
        [DataRow("$Warzone_PointRace_Low:#index=5;", "Low Intensity Conflict Zone", 5, 0)]
        [DataRow("$Warzone_PointRace_Med:#index=3;", "Medium Intensity Conflict Zone", 3, 0)]
        [DataRow("$Warzone_PointRace_High:#index=2;", "High Intensity Conflict Zone", 2, 0)]
        [DataRow("$Warzone_TG:#index=1;", "AX Conflict Zone", 1, 0)]
        // From `SaaSignalsFound` events and `FSSBodySignals` events
        [DataRow("$SAA_SignalType_Biological;", "Biological Surface Signal", 0, 0)]
        [DataRow("$SAA_SignalType_Geological;", "Geological Surface Signal", 0, 0)]
        [DataRow("$SAA_SignalType_Guardian;", "Guardian Surface Signal", 0, 0)]
        [DataRow("$SAA_SignalType_Human;", "Human Surface Signal", 0, 0)]
        [DataRow("$SAA_SignalType_Other;", "Other Surface Signal", 0, 0)]
        [DataRow("$SAA_SignalType_Thargoid;", "Thargoid Surface Signal", 0, 0)]
        // From `Touchdown` events
        [DataRow("$POI_CrashedShip:#index=1;", "Crashed Ship", 1, 0)]
        [DataRow("$POIScenario_Watson_Abandoned_Buggy_01_Easy;", "Distress Beacon", 0, 1)]
        [DataRow("$POIScenario_Watson_Abandoned_Buggy_01_Salvage_Easy;", "Distress Beacon", 0, 1)]
        [DataRow("$POIScenario_Watson_Abandoned_Buggy_01_Salvage_Medium; $USS_ThreatLevel:#threatLevel=2;", "Distress Beacon", 0, 2)]
        [DataRow("$POIScenario_Watson_Damaged_Eagle_01_Assassination_Easy;", "ENCRYPTED SIGNAL", 0, 1)]
        [DataRow("$POIScenario_Watson_Damaged_Eagle_01_Assassination_Hard; $USS_ThreatLevel:#threatLevel=3;", "ENCRYPTED SIGNAL", 0, 3)]
        [DataRow("$POIScenario_Watson_Damaged_Eagle_01_Easy;", "Distress Beacon", 0, 1)]
        [DataRow("$POIScenario_Watson_Damaged_Eagle_01_Salvage_Easy; $USS_ThreatLevel:#threatLevel=1;", "Distress Beacon", 0, 1)]
        [DataRow("$POIScenario_Watson_Damaged_Eagle_01_Salvage_Easy;", "Distress Beacon", 0, 1)]
        [DataRow("$POIScenario_Watson_Damaged_Sidewinder_01_Assassination_Easy; $USS_ThreatLevel:#threatLevel=1;", "ENCRYPTED SIGNAL", 0, 1)]
        [DataRow("$POIScenario_Watson_Damaged_Sidewinder_01_Assassination_Hard;", "ENCRYPTED SIGNAL", 0, 3)]
        [DataRow("$POIScenario_Watson_Damaged_Sidewinder_01_Assassination_Medium; $USS_ThreatLevel:#threatLevel=2;", "ENCRYPTED SIGNAL", 0, 2)]
        [DataRow("$POIScenario_Watson_Damaged_Sidewinder_01_Easy;", "Distress Beacon", 0, 1)]
        [DataRow("$POIScenario_Watson_Damaged_Sidewinder_01_Salvage_Easy;", "Distress Beacon", 0, 1)]
        [DataRow("$POIScenario_Watson_Smugglers_Cache_01_Hard;", "Irregular Markers", 0, 3)]
        [DataRow("$POIScenario_Watson_Smugglers_Cache_01_Medium;", "Irregular Markers", 0, 2)]
        [DataRow("$POIScenario_Watson_Smugglers_Cache_02_Easy;", "Irregular Markers", 0, 1)]
        [DataRow("$POIScenario_Watson_Smugglers_Cache_02_Heist_Easy; $USS_ThreatLevel:#threatLevel=1;", "Irregular Markers", 0, 1)]
        [DataRow("$POIScenario_Watson_Smugglers_Cache_02_Heist_Medium;", "Irregular Markers", 0, 2)]
        [DataRow("$POIScenario_Watson_Wreckage_Buggy_01_Easy;", "Minor Wreckage", 0, 1)]
        [DataRow("$POIScenario_Watson_Wreckage_Buggy_01_Hard;", "Minor Wreckage", 0, 3)]
        [DataRow("$POIScenario_Watson_Wreckage_Buggy_01_Medium;", "Minor Wreckage", 0, 2)]
        [DataRow("$POIScenario_Watson_Wreckage_Buggy_01_Salvage_Easy;", "Minor Wreckage", 0, 1)]
        [DataRow("$POIScenario_Watson_Wreckage_Buggy_01_Salvage_Medium; $USS_ThreatLevel:#threatLevel=2;", "Minor Wreckage", 0, 2)]
        [DataRow("$POIScenario_Watson_Wreckage_Probe_01_Hard;", "Impact Site", 0, 3)]
        [DataRow("$POIScenario_Watson_Wreckage_Probe_01_Medium;", "Impact Site", 0, 2)]
        [DataRow("$POIScenario_Watson_Wreckage_Probe_01_Salvage_Easy; $USS_ThreatLevel:#threatLevel=1;", "Impact Site", 0, 1)]
        [DataRow("$POIScenario_Watson_Wreckage_Satellite_01_Hard;", "Impact Site", 0, 3)]
        [DataRow("$POIScenario_Watson_Wreckage_Satellite_01_Medium;", "Impact Site", 0, 2)]
        [DataRow("$POIScenario_Watson_Wreckage_Satellite_01_Salvage_Easy; $USS_ThreatLevel:#threatLevel=1;", "Impact Site", 0, 1)]
        [DataRow("$POIScenario_Watson_Wreckage_Satellite_01_Salvage_Medium;", "Impact Site", 0, 2)]
        [DataRow("$POIScenario_Watson_Wrecks_Eagle_01_Hard;", "Crash Site", 0, 3)]
        [DataRow("$POIScenario_Watson_Wrecks_Eagle_01_Salvage_Easy;", "Crash Site", 0, 1)]
        [DataRow("$POIScenario_Watson_Wrecks_Sidewinder_01_Salvage_Easy; $USS_ThreatLevel:#threatLevel=1;", "Crash Site", 0, 1)]
        [DataRow("$POIScenario_Watson_Wrecks_Sidewinder_01_Salvage_Medium;", "Crash Site", 0, 2)]
        [DataRow("$POIScene_Perimeter_02;", "Active Power Source", 0, 0)]
        [DataRow("$POIScene_Trap_Cargo_01;", "Irregular Markers", 0, 0)]
        [DataRow("$POIScene_Trap_Data_02;", "Irregular Markers", 0, 0)]
        [DataRow("$POIScene_Wreckage_Cargo_03;", "Minor Wreckage", 0, 0)]
        // From `USSDrop` events
        [DataRow("$USS_Type_Aftermath;", "Combat Aftermath", 0, 0)]
        [DataRow("$USS_Type_Anomaly;", "Anomaly", 0, 0)]
        [DataRow("$USS_Type_Ceremonial;", "Ceremonial Comms", 0, 0)]
        [DataRow("$USS_Type_Convoy;", "Convoy Dispersal Pattern", 0, 0)]
        [DataRow("$USS_Type_DistressSignal;", "Distress Call", 0, 0)]
        [DataRow("$USS_Type_MissionTarget;", "Mission Target", 0, 0)]
        [DataRow("$USS_Type_NonHuman;", "Non-Human Signal Source", 0, 0)]
        [DataRow("$USS_Type_Salvage;", "Degraded Emissions", 0, 0)]
        [DataRow("$USS_Type_TradingBeacon;", "Trading Beacon", 0, 0)]
        [DataRow("$USS_Type_ValuableSalvage;", "Encoded Emissions", 0, 0)]
        [DataRow("$USS_Type_VeryValuableSalvage;", "High Grade Emissions", 0, 0)]
        [DataRow("$USS_Type_WeaponsFire;", "Weapons Fire", 0, 0)]
        public void SignalSourceParsingTest(string edName, string expectedInvariantName, int? expectedIndex, int? expectedThreatLevel)
        {
            var source = SignalSource.FromEDName(edName);
            Assert.AreEqual(expectedInvariantName, source?.invariantName);
            Assert.AreEqual(expectedIndex, source?.index ?? 0);
            Assert.AreEqual(expectedThreatLevel, source?.threatLevel ?? 0);
        }

        [TestMethod]
        public void BodyNameSerializationTest()
        {
            // `bodyname` should be considered a required serialization attribute
            // except in the case of a main star (where the `bodyname` might still be unknown)

            var starsystem = new StarSystem() { systemname = "Test System" };
            var body = new Body();
            starsystem.AddOrUpdateBody(body);
            Assert.AreEqual(1, starsystem.bodies.Count);
            try
            {
                _ = JsonConvert.SerializeObject(starsystem);
                Assert.Fail("Serialization should fail - body has neither `bodyname` nor `distance` properties set.");
            }
            catch (Exception)
            {
                // ignored
            }

            starsystem.bodies[0].bodyname = "Test body";
            try
            {
                _ = JsonConvert.SerializeObject(starsystem);
            }
            catch (Exception)
            {
                Assert.Fail("Serialization should not fail - body has `bodyname` property set.");
            }

            starsystem.bodies[0].bodyname = null;
            starsystem.bodies[0].distance = 0M;
            try
            {
                _ = JsonConvert.SerializeObject(starsystem);
            }
            catch (Exception)
            {
                Assert.Fail("Serialization should not fail - body has `distance` property set.");
            }
        }
    }
}
