using System;
using System.Linq;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class CommodityTests
    {
        [TestMethod]
        public void TestMalformedCommodityName()
        {
            string malformedCommodityName = "I gotta quote\" and a backslash\\, I'm really bad.";
            var badCommoditity = CommodityDefinition.FromNameOrEDName(malformedCommodityName);
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
                ""buyprice"": 7000,
                ""stock"": 5,
                ""stockbracket"": """",
                ""sellprice"": 7279,
                ""demand"": 56,
                ""demandbracket"": 1,
                ""StatusFlags"": [],
                ""EliteID"": 128049669,
                ""EDDBID"": 36,
                ""category"": ""Medicines"",
                ""avgprice"": 6779,
                ""rare"": false
            }";

            CommodityMarketQuote commodity = JsonConvert.DeserializeObject<CommodityMarketQuote>(commodityDefCommodity);
            Assert.IsNotNull(commodity);
            Assert.AreEqual("ProgenitorCells", commodity.definition.edname); ;
            Assert.AreEqual("Progenitor Cells", commodity.invariantName);
            Assert.AreEqual(7000, commodity.buyprice);
            Assert.AreEqual(5, commodity.stock);
            Assert.AreEqual(null, commodity.stockbracket);
            Assert.AreEqual(7279, commodity.sellprice);
            Assert.AreEqual(56, commodity.demand);
            Assert.AreEqual(CommodityBracket.Low, commodity.demandbracket);
            Assert.AreEqual(128049669, commodity.EliteID);
            Assert.AreEqual(36, commodity.EDDBID);
            Assert.AreEqual("Medicines", commodity.definition.category.invariantName);
            Assert.AreEqual(6779, commodity.avgprice);
            Assert.IsFalse(commodity.rare);
        }

        private static MarketInfoItem CannedCAPIQuote()
        {
            string json = @"{
                ""id"": 128049204,
                ""name"": ""Explosives"",
                ""legality"": """",
                ""buyPrice"": 313,
                ""sellPrice"": 281,
                ""meanPrice"": 294,
                ""demandBracket"": """",
                ""stockBracket"": 2,
                ""stock"": 31881,
                ""demand"": 1,
                ""statusFlags"": [""Producer""],
                ""categoryname"": ""Chemicals"",
                ""locName"": ""Explosives""
            }";
            return JsonConvert.DeserializeObject<MarketInfoItem>(json);
        }

        [TestMethod]
        public void TestParseCAPICommodityQuote()
        {
            var edQuote = CannedCAPIQuote();
            Assert.AreEqual(313, edQuote.buyPrice);
            Assert.AreEqual(281, edQuote.sellPrice);
            Assert.AreEqual(294, edQuote.meanPrice);
            Assert.AreEqual(null, edQuote.demandBracket);
            Assert.AreEqual(CommodityBracket.Medium, edQuote.stockBracket);
            Assert.AreEqual(31881, edQuote.stock);
            Assert.AreEqual(1, edQuote.demand);
            Assert.AreEqual(1, edQuote.statusFlags.Count);
            Assert.AreEqual("Producer", edQuote.statusFlags.First());
        }

        [TestMethod]
        public void TestParseCAPICommodityQuoteNotOverwritten()
        {
            var edQuote = CannedCAPIQuote();
            var quote = edQuote.ToCommodityMarketQuote();
            var oldDefinition = quote.definition.Copy();
            oldDefinition.avgprice = 99999999;
            var oldQuote = new CommodityMarketQuote(oldDefinition);
            Assert.AreEqual(313, quote.buyprice);
            Assert.AreEqual(281, quote.sellprice);
            Assert.AreEqual(294, quote.avgprice);
            Assert.AreEqual(null, quote.demandbracket);
            Assert.AreEqual(CommodityBracket.Medium, quote.stockbracket);
            Assert.AreEqual(31881, quote.stock);
            Assert.AreEqual(1, quote.demand);
            Assert.AreEqual(1, quote.StatusFlags.Count);
            Assert.AreEqual("Producer", quote.StatusFlags.First());
        }

        [TestMethod]
        public void TestEddnCommodityQuote()
        {
            var edQuote = CannedCAPIQuote();
            var quote = edQuote.ToCommodityMarketQuote();
            Assert.AreEqual(quote.buyprice, edQuote.buyPrice);
            Assert.AreEqual(quote.sellprice, edQuote.sellPrice);
            Assert.AreEqual(quote.avgprice, edQuote.meanPrice);
            Assert.AreEqual(quote.demandbracket, edQuote.demandBracket);
            Assert.AreEqual(quote.stockbracket, edQuote.stockBracket);
            Assert.AreEqual(quote.stock, edQuote.stock);
            Assert.AreEqual(quote.demand, edQuote.demand);
            Assert.AreEqual(quote.StatusFlags.Count, edQuote.statusFlags.Count);
            Assert.AreEqual(quote.StatusFlags.First(), edQuote.statusFlags.First());
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

            // Assert that parsing this now throws. 
            // `Assert.ThrowsException<>(...)` doesn't seem to be available?
            try
            {
                CommodityMarketQuote commodity = JsonConvert.DeserializeObject<CommodityMarketQuote>(legacyCommodity);
                Assert.Fail("Expected invalid commodity JSON to throw");
            }
            catch
            {
                // passed
            }
        }
    }
}
