using EddiCompanionAppService;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Tests.Properties;
using Utilities;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class CapiTests : TestBase
    {
        [TestMethod]
        public void TestCommoditiesFromProfile()
        {
            // Test commodities data
            var incompleteExpectedCommodities = new List<MarketInfoItem>()
            {
                new MarketInfoItem(128924334, "AgronomicTreatment", "Chemicals", 0, 3336, 3155, CommodityBracket.None, CommodityBracket.Medium, 0, 43, new HashSet<string>() ),
                new MarketInfoItem(128049204, "Explosives", "Chemicals", 224, 203, 419, CommodityBracket.High, CommodityBracket.None, 52135, 1, new HashSet<string>() ),
                new MarketInfoItem(128049202, "HydrogenFuel", "Chemicals", 84, 80, 108, CommodityBracket.High, CommodityBracket.None, 90728, 1, new HashSet<string>() ),
                new MarketInfoItem(128673850, "HydrogenPeroxide", "Chemicals", 0, 1198, 1209, CommodityBracket.None, CommodityBracket.High, 0, 116055, new HashSet<string>() ),
                new MarketInfoItem(128673851, "LiquidOxygen", "Chemicals", 0, 467, 434, CommodityBracket.None, CommodityBracket.Medium, 0, 18513, new HashSet<string>() ),
            };

            JObject json = DeserializeJsonResource<JObject>(Resources.Libby_Horizons);
            var actualCommodities = CompanionAppService.CommodityQuotesFromProfile(json);

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
        }

        [TestMethod]
        public void TestProhibitedCommoditiesFromProfile()
        {
            // Test commodities data
            var expectedProhibitedCommodities = new List<KeyValuePair<long, string>>()
            {
                new KeyValuePair<long, string>(128049670, "CombatStabilisers"),
                new KeyValuePair<long, string>(128049212, "BasicNarcotics"),
                new KeyValuePair<long, string>(128049213, "Tobacco"),
                new KeyValuePair<long, string>(128049234, "BattleWeapons"),
                new KeyValuePair<long, string>(128667728, "ImperialSlaves"),
                new KeyValuePair<long, string>(128049243, "Slaves")
            };

            JObject json = DeserializeJsonResource<JObject>(Resources.Libby_Horizons);
            var actualProhibitedCommodities = CompanionAppService.ProhibitedCommoditiesFromProfile(json);

            Assert.AreEqual(6, actualProhibitedCommodities.Count);
            foreach (var expectedCommodity in expectedProhibitedCommodities)
            {
                foreach (var actualCommodity in actualProhibitedCommodities)
                {
                    if (expectedCommodity.Key == actualCommodity.Key)
                    {
                        Assert.IsTrue(expectedCommodity.Value == actualCommodity.Value);
                    }
                }
            }
        }

        [TestMethod]
        public void TestEconomiesFromProfile()
        {
            // Test economies data
            var expectedEconomies = new List<ProfileEconomyShare>()
            {
                new ProfileEconomyShare("Military", 1)
            };

            JObject json = DeserializeJsonResource<JObject>(Resources.Abasheli_Barracks);
            var actualEconomies = CompanionAppService.EconomiesFromProfile(json);

            Assert.AreEqual(1, actualEconomies.Count);
            Assert.IsTrue(expectedEconomies[0].DeepEquals(actualEconomies[0]));
        }

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
    }
}
