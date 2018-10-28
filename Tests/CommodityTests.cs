using EddiDataDefinitions;
using EDDNResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            Assert.AreEqual("", commodity.stockbracket);
            Assert.AreEqual(7279, commodity.sellprice);
            Assert.AreEqual(56, commodity.demand);
            Assert.AreEqual(1, commodity.demandbracket);
            Assert.AreEqual(128049669, commodity.EliteID);
            Assert.AreEqual(36, commodity.EDDBID);
            Assert.AreEqual("Medicines", commodity.definition.category.invariantName);
            Assert.AreEqual(6779, commodity.avgprice);
            Assert.IsFalse(commodity.rare);
            Assert.IsFalse(commodity.fromFDev);
        }

        private static CommodityMarketQuote CannedCAPIQuote()
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
                ""statusFlags"": [],
                ""categoryname"": ""Chemicals"",
                ""locName"": ""Explosives""
            }";
            JObject jObject = JObject.Parse(json);
            CommodityMarketQuote quote = CommodityMarketQuote.FromCapiJson(jObject);
            quote.fromFDev = true;
            return quote;
        }

        [TestMethod]
        public void TestParseCAPICommodityQuote()
        {
            CommodityMarketQuote quote = CannedCAPIQuote();
            Assert.AreEqual(313, quote.buyprice);
            Assert.AreEqual(281, quote.sellprice);
            Assert.AreEqual(294, quote.avgprice);
            Assert.AreEqual("", quote.demandbracket);
            Assert.AreEqual(2, quote.stockbracket);
            Assert.AreEqual(31881, quote.stock);
            Assert.AreEqual(1, quote.demand);
            Assert.AreEqual(0, quote.StatusFlags.Count);
            Assert.IsTrue(quote.fromFDev);
        }

        [TestMethod]
        public void TestParseCAPICommodityQuoteNotOverwritten()
        {
            CommodityMarketQuote quote = CannedCAPIQuote();
            CommodityMarketQuote oldQuote = new CommodityMarketQuote(quote.definition)
            {
                avgprice = 99999999
            };
            Assert.AreEqual(313, quote.buyprice);
            Assert.AreEqual(281, quote.sellprice);
            Assert.AreEqual(294, quote.avgprice);
            Assert.AreEqual("", quote.demandbracket);
            Assert.AreEqual(2, quote.stockbracket);
            Assert.AreEqual(31881, quote.stock);
            Assert.AreEqual(1, quote.demand);
            Assert.AreEqual(0, quote.StatusFlags.Count);
            Assert.IsTrue(quote.fromFDev);
        }

        [TestMethod]
        public void TestEddnCommodityQuote()
        {
            CommodityMarketQuote quote = CannedCAPIQuote();
            EDDNCommodity eddnCommodity = new EDDNCommodity(quote);
            Assert.AreEqual(quote.buyprice, eddnCommodity.buyPrice);
            Assert.AreEqual(quote.sellprice, eddnCommodity.sellPrice);
            Assert.AreEqual(quote.avgprice, eddnCommodity.meanPrice);
            Assert.AreEqual(quote.demandbracket, eddnCommodity.demandBracket);
            Assert.AreEqual(quote.stockbracket, eddnCommodity.stockBracket);
            Assert.AreEqual(quote.stock, eddnCommodity.stock);
            Assert.AreEqual(quote.demand, eddnCommodity.demand);
            Assert.AreEqual(quote.StatusFlags.Count, eddnCommodity.statusFlags.Count);
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
