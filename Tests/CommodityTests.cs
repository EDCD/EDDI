using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using EddiDataDefinitions;
using Newtonsoft.Json;
using Rollbar;

namespace UnitTests
{
    [TestClass]
    public class CommodityTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        [TestMethod]
        public void TestMalformedCommodityName()
        {
            string malformedCommodityName = "I gotta quote\" and a backslash\\, I'm really bad.";
            var badCommoditity = CommodityDefinition.FromName(malformedCommodityName);
            Assert.AreEqual(malformedCommodityName.ToLowerInvariant(), badCommoditity.localizedName);
        }

        [TestMethod]
        public void TestCommodityDefinitionCommodity()
        {
            string commodityDefCommodity = @"
            {
                ""definition"": {
                    ""edname"": ""ProgenitorCells""
                },
                ""invariantName"": ""Progenitor Cells"",
                ""localizedName"": ""Progenitor Cells"",
                ""name"": ""Progenitor Cells"",
                ""buyprice"": null,
                ""stock"": null,
                ""stockbracket"": """",
                ""sellprice"": 7279,
                ""demand"": 56,
                ""demandbracket"": """",
                ""StatusFlags"": [],
                ""EliteID"": 0,
                ""EDDBID"": 36,
                ""category"": ""Medicines"",
                ""avgprice"": 6779,
                ""rare"": false
            }";

            CommodityMarketQuote commodity = JsonConvert.DeserializeObject<CommodityMarketQuote>(commodityDefCommodity);
            Assert.IsNotNull(commodity);
            Assert.AreEqual("ProgenitorCells", commodity.definition.edname); ;
            Assert.AreEqual("Progenitor Cells", commodity.invariantName);
        }

        [TestMethod]
        public void TestLegacyCommodity()
        {
            /// Test legacy data that may be stored in user's local sql databases. 
            /// Legacy data includes all data stored in user's sql databases prior to version 3.0.1-b2
            /// Note that data structures were reorganized at this time to support internationalization.
            string legacyCommodity = @"
            {
                ""name"": ""ProgenitorCells"",
                ""category"": null,
                ""avgprice"": null,
                ""rare"": null,
                ""buyprice"": null,
                ""stock"": null,
                ""stockbracket"": null,
                ""sellprice"": 6561,
                ""demand"": 17,
                ""demandbracket"": null,
                ""StatusFlags"": null,
                ""EDDBID"": -1,
                ""EDName"": null
            }";

            CommodityMarketQuote commodity = JsonConvert.DeserializeObject<CommodityMarketQuote>(legacyCommodity);
            Assert.IsNotNull(commodity);
            Assert.AreEqual("ProgenitorCells", commodity.definition.edname); ;
            Assert.AreEqual("Progenitor Cells", commodity.invariantName);
        }
    }
}
