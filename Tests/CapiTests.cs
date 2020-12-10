using EddiCompanionAppService;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class CapiTests : TestBase
    {
        [TestMethod]
        public void TestOutfittingModules()
        {
            // Test outfitting data
            var incompleteExpectedModules = new List<OutfittingInfoItem>()
            {
                new OutfittingInfoItem(128788700, "Hpt_ATDumbfireMissile_Fixed_Large", "weapon", 1352250),
                new OutfittingInfoItem(128788702, "Hpt_ATMultiCannon_Fixed_Large", "weapon", 1181500),
                new OutfittingInfoItem(128793060, "Hpt_ATMultiCannon_Turret_Large", "weapon", 3821600),
                new OutfittingInfoItem(128785626, "Hpt_FlakMortar_Fixed_Medium", "weapon", 261800),
                new OutfittingInfoItem(128788699, "Hpt_ATDumbfireMissile_Fixed_Medium", "weapon", 540900)
            };

            JObject json = DeserializeJsonResource<JObject>(Resources.Abasheli_Barracks);
            var actualModules = CompanionAppService.OutfittingFromProfile(json);

            Assert.AreEqual(165, actualModules.Count);
            foreach (var expectedModule in incompleteExpectedModules)
            {
                foreach (var actualModule in actualModules)
                {
                    if (expectedModule.EliteID == actualModule.EliteID)
                    {
                        Assert.IsTrue(expectedModule.DeepEquals(actualModule));
                    }
                }
            }
        }

        [TestMethod]
        public void TestShipyardShips()
        {
            // Test shipyard data
            var expectedShips = new List<ShipyardInfoItem>()
            { 
                new ShipyardInfoItem(128049255, "Eagle", 44800),
                new ShipyardInfoItem(128672276, "Asp_Scout", 3961154),
                new ShipyardInfoItem(128049249, "SideWinder", 32000),
                new ShipyardInfoItem(128049309, "Vulture", 4925615),
                new ShipyardInfoItem(128049363, "Anaconda", 146969451),
                new ShipyardInfoItem(128049321, "Federation_Dropship", 14314205),
                new ShipyardInfoItem(128672152, "Federation_Gunship", 35814205),
                new ShipyardInfoItem(128672145, "Federation_Dropship_MkII", 19814205)
            };

            JObject json = DeserializeJsonResource<JObject>(Resources.Abasheli_Barracks);
            var actualShips = CompanionAppService.ShipyardFromProfile(json);

            Assert.AreEqual(expectedShips.Count, actualShips.Count);
            foreach (var expectedShip in expectedShips)
            {
                foreach (var actualShip in actualShips)
                {
                    if (expectedShip.EliteID == actualShip.EliteID)
                    {
                        Assert.IsTrue(expectedShip.DeepEquals(actualShip));
                    }
                }
            }
        }

        [TestMethod]
        public void TestProfileStation()
        {
            var marketTimestamp = DateTime.UtcNow;
            JObject marketJson = DeserializeJsonResource<JObject>(Resources.Libby_Horizons);

            var expectedStation = new ProfileStation()
            {
                name = "Libby Horizons",
                marketId = 3228854528,
                economyShares = new List<ProfileEconomyShare>() 
                {
                    new ProfileEconomyShare("Refinery", 0.88M),
                    new ProfileEconomyShare("Industrial", 0.12M),
                },
                eddnCommodityMarketQuotes = new List<MarketInfoItem>()
                {
                    new MarketInfoItem(128924334, "AgronomicTreatment", "Chemicals", 0, 3336, 3155, CommodityBracket.None, CommodityBracket.Medium, 0, 43, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128049204, "Explosives", "Chemicals", 224, 203, 419, CommodityBracket.High, CommodityBracket.None, 52135, 1, false, new HashSet<string>() { "Producer" } ),
                    new MarketInfoItem(128049202, "HydrogenFuel", "Chemicals", 84, 80, 108, CommodityBracket.High, CommodityBracket.None, 90728, 1, false, new HashSet<string>() { "Producer" } ),
                    new MarketInfoItem(128673850, "HydrogenPeroxide", "Chemicals", 0, 1198, 1209, CommodityBracket.None, CommodityBracket.High, 0, 116055, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128673851, "LiquidOxygen", "Chemicals", 0, 467, 434, CommodityBracket.None, CommodityBracket.Medium, 0, 18513, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128049203, "MineralOil", "Chemicals", 0, 687, 395, CommodityBracket.None, CommodityBracket.Medium, 0, 249414, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128672305, "SurfaceStabilisers", "Chemicals", 416, 390, 663, CommodityBracket.High, CommodityBracket.None, 44411, 1, false, new HashSet<string>() { "Producer" } ),
                    new MarketInfoItem(128961249, "Tritium", "Chemicals", 41179, 40693, 42558, CommodityBracket.Medium, CommodityBracket.None, 10464, 1, false, new HashSet<string>() { "Producer" } ),
                    new MarketInfoItem(128049166, "Water", "Chemicals", 0, 457, 267, CommodityBracket.None, CommodityBracket.Medium, 0, 36527, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128049241, "Clothing", "Consumer Items", 0, 902, 459, CommodityBracket.None, CommodityBracket.Medium, 0, 38689, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128049240, "ConsumerTechnology", "Consumer Items", 0, 7658, 6809, CommodityBracket.None, CommodityBracket.High, 0, 5598, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128049238, "DomesticAppliances", "Consumer Items", 0, 1117, 659, CommodityBracket.None, CommodityBracket.Medium, 0, 14440, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128682048, "SurvivalEquipment", "Consumer Items", 500, 467, 647, CommodityBracket.High, CommodityBracket.None, 116, 1, false, new HashSet<string>() { "Producer" } ),
                    new MarketInfoItem(128049177, "Algae", "Foods", 0, 464, 321, CommodityBracket.None, CommodityBracket.Medium, 0, 7417, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128049182, "Animalmeat", "Foods", 0, 2181, 1524, CommodityBracket.None, CommodityBracket.High, 0, 14121, false, new HashSet<string>() { "Consumer", "powerplay" } ),
                    new MarketInfoItem(128049189, "Coffee", "Foods", 0, 2181, 1504, CommodityBracket.None, CommodityBracket.High, 0, 5194, false, new HashSet<string>() { "Consumer", "powerplay" } ),
                    new MarketInfoItem(128049183, "Fish", "Foods", 0, 1083, 640, CommodityBracket.None, CommodityBracket.High, 0, 45149, false, new HashSet<string>() { "Consumer", "powerplay" } ),
                    new MarketInfoItem(128049184, "FoodCartridges", "Foods", 0, 334, 225, CommodityBracket.None, CommodityBracket.Medium, 0, 1887, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128049178, "FruitAndVegetables", "Foods", 0, 970, 528, CommodityBracket.None, CommodityBracket.High, 0, 18707, false, new HashSet<string>() { "Consumer", "powerplay" } ),
                    new MarketInfoItem(128049180, "Grain", "Foods", 0, 836, 432, CommodityBracket.None, CommodityBracket.High, 0, 99831, false, new HashSet<string>() { "Consumer", "powerplay" } ),
                    new MarketInfoItem(128049185, "SyntheticMeat", "Foods", 0, 810, 487, CommodityBracket.None, CommodityBracket.High, 0, 6223, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128049188, "Tea", "Foods", 0, 2392, 1691, CommodityBracket.None, CommodityBracket.High, 0, 13184, false, new HashSet<string>() { "Consumer", "powerplay" } ),
                    new MarketInfoItem(128673856, "CMMComposite", "Industrial Materials", 0, 6779, 5984, CommodityBracket.None, CommodityBracket.High, 0, 2287, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128672302, "CeramicComposites", "Industrial Materials", 0, 712, 393, CommodityBracket.None, CommodityBracket.High, 0, 35686, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128673857, "CoolingHoses", "Industrial Materials", 0, 1896, 1886, CommodityBracket.None, CommodityBracket.High, 0, 7839, false, new HashSet<string>() { "Consumer" } ),
                    new MarketInfoItem(128673855, "InsulatingMembrane", "Industrial Materials", 0, 11386, 10691, CommodityBracket.None, CommodityBracket.Medium, 0, 1461, false, new HashSet<string>() { "Consumer" } ),
                },
                prohibitedCommodities = new List<KeyValuePair<long, string>>()
                {
                    new KeyValuePair<long, string>(128049670, "CombatStabilisers"),
                    new KeyValuePair<long, string>(128049212, "BasicNarcotics"),
                    new KeyValuePair<long, string>(128049213, "Tobacco"),
                    new KeyValuePair<long, string>(128049234, "BattleWeapons"),
                    new KeyValuePair<long, string>(128667728, "ImperialSlaves"),
                    new KeyValuePair<long, string>(128049243, "Slaves")
                },
                commoditiesupdatedat = Dates.fromDateTimeToSeconds(marketTimestamp),
                json = DeserializeJsonResource<JObject>(Resources.Libby_Horizons),
                stationServices = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("dock", "ok"),
                    new KeyValuePair<string, string>("contacts", "ok"),
                    new KeyValuePair<string, string>("exploration", "ok"),
                    new KeyValuePair<string, string>("commodities", "ok"),
                    new KeyValuePair<string, string>("refuel", "ok"),
                    new KeyValuePair<string, string>("repair", "ok"),
                    new KeyValuePair<string, string>("rearm", "ok"),
                    new KeyValuePair<string, string>("outfitting", "ok"),
                    new KeyValuePair<string, string>("shipyard", "ok"),
                    new KeyValuePair<string, string>("crewlounge", "ok"),
                    new KeyValuePair<string, string>("powerplay", "ok"),
                    new KeyValuePair<string, string>("searchrescue", "ok"),
                    new KeyValuePair<string, string>("materialtrader", "ok"),
                    new KeyValuePair<string, string>("stationmenu", "ok"),
                    new KeyValuePair<string, string>("shop", "ok"),
                    new KeyValuePair<string, string>("engineer", "ok")
                }
            };

            var actualStation = CompanionAppService.ProfileStation(marketTimestamp, marketJson);

            // Test commodities separately to minimize redundant data entry
            var incompleteExpectedCommodities = expectedStation.eddnCommodityMarketQuotes;
            var actualCommodities = actualStation.eddnCommodityMarketQuotes;
            Assert.AreEqual(116, actualCommodities.Count);
            foreach (var expectedCommodity in incompleteExpectedCommodities)
            {
                foreach (var actualCommodity in actualCommodities)
                {
                    if (expectedCommodity.EliteID == actualCommodity.EliteID)
                    {
                        Assert.IsTrue(expectedCommodity.DeepEquals(actualCommodity));
                    }
                }
            }

            // Compare actual and expected stations, less the commodities we already tested above
            expectedStation.eddnCommodityMarketQuotes = null;
            actualStation.eddnCommodityMarketQuotes = null;
            Assert.IsTrue(expectedStation.DeepEquals(actualStation));
        }

        [TestMethod]
        public void TestProfileUpdateStation()
        {
            // Set up our original station
            var originalStation = new Station()
            {
                name = "Libby Horizons",
                marketId = 3228854528,
                updatedat = 0
            };

            // Set up our profile station
            var profile = new Profile();
            var marketTimestamp = DateTime.UtcNow;
            JObject marketJson = DeserializeJsonResource<JObject>(Resources.Libby_Horizons);
            profile.LastStation = CompanionAppService.ProfileStation(marketTimestamp, marketJson);

            var updatedStation = profile.LastStation.UpdateStation(DateTime.UtcNow, originalStation);
            Assert.IsTrue(updatedStation.economyShares.DeepEquals(new List<EconomyShare>() 
            { 
                new EconomyShare("Refinery", 0.88M), 
                new EconomyShare("Industrial", 0.12M) 
            }));
            Assert.IsTrue(updatedStation.stationServices.DeepEquals(new List<StationService>() 
            { 
                StationService.FromEDName("dock"),
                StationService.FromEDName("contacts"),
                StationService.FromEDName("exploration"),
                StationService.FromEDName("commodities"),
                StationService.FromEDName("refuel"),
                StationService.FromEDName("repair"),
                StationService.FromEDName("rearm"),
                StationService.FromEDName("outfitting"),
                StationService.FromEDName("shipyard"),
                StationService.FromEDName("crewlounge"),
                StationService.FromEDName("powerplay"),
                StationService.FromEDName("searchrescue"),
                StationService.FromEDName("materialtrader"),
                StationService.FromEDName("stationmenu"),
                StationService.FromEDName("shop"),
                StationService.FromEDName("engineer"),
            }));
            Assert.AreEqual(116, updatedStation.commodities.Count);
            Assert.IsTrue(new CommodityMarketQuote(CommodityDefinition.FromEDName("Tritium")) 
            { 
                buyprice = 41179, 
                sellprice = 40693, 
                demand = 1, 
                demandbracket = CommodityBracket.None, 
                stock = 10464, 
                stockbracket = CommodityBracket.Medium, 
                StatusFlags = new HashSet<string>() { "Producer" }
            }.DeepEquals(updatedStation.commodities.FirstOrDefault(c => c.EliteID == 128961249)));
            Assert.AreEqual(42558, CommodityDefinition.FromEDName("Tritium").avgprice); 
            Assert.AreEqual(6, updatedStation.prohibited.Count);
            Assert.IsTrue(CommodityDefinition.FromEDName("Tobacco").DeepEquals(updatedStation.prohibited.FirstOrDefault(p => p.EliteID == 128049213)));
            Assert.AreEqual(Dates.fromDateTimeToSeconds(marketTimestamp), updatedStation.commoditiesupdatedat);
        }
    }
}
