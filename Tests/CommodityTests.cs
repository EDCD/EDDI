using System;
using System.Collections.Generic;
using System.Linq;
using EddiDataDefinitions;
using EDDNResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Tests.Properties;
using Utilities;

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
            Assert.AreEqual("{\"name\":\"explosives\",\"buyPrice\":313,\"meanPrice\":294,\"sellPrice\":281,\"stock\":31881,\"stockBracket\":2,\"demand\":1,\"demandBracket\":\"\",\"statusFlags\":[\"Producer\"]}", JsonConvert.SerializeObject(edQuote, new JsonSerializerSettings { ContractResolver = new EDDNContractResolver() }));
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

        [TestMethod]
        public void TestDeserializeMarketJson()
        {
            var info = TestBase.DeserializeJsonResource<MarketInfo>(Resources.market);
            Assert.AreEqual(3702012928, info.MarketID);
            Assert.AreEqual("X9K-WTW", info.StationName);
            Assert.AreEqual("Gateway", info.StarSystem);

            var expectedItems = new List<MarketInfoItem>()
            {
                new MarketInfoItem(128668550, "painite", "minerals", 0, 500096, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 200),
                new MarketInfoItem(128673846, "bromellite", "minerals", 0, 10009, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 100),
                new MarketInfoItem(128673848, "lowtemperaturediamond", "minerals", 0, 500553, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 150),
                new MarketInfoItem(128924330, "grandidierite", "minerals", 0, 424204, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 100),
                new MarketInfoItem(128924331, "alexandrite", "minerals", 0, 348192, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 97),
                new MarketInfoItem(128924332, "opal", "minerals", 0, 1014218, 0, CommodityBracket.None, CommodityBracket.Medium, 0, 300)
            };

            Assert.AreEqual(expectedItems.Count, info.Items.Count);
            foreach (var actualItem in info.Items)
            {
                foreach (var expectedItem in expectedItems)
                {
                    if (actualItem.EliteID == expectedItem.EliteID)
                    {
                        Assert.IsTrue(actualItem.DeepEquals(expectedItem));
                    }
                }
            }
        }
    }
}
