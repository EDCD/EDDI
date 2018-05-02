using System;
using EddiCargoMonitor;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CargoMonitorTests
    {
        string legacyJSON = @"{
            ""cargo"": [
            {
                ""name"": ""Hydrogen Fuel"",
                ""stolen"": 0,
                ""haulage"": 0,
                ""other"": 2,
                ""total"": 2,
                ""ejected"": 0,
                ""price"": 110,
                ""category"": ""Chemicals"",
                ""commodity"": {
                    ""name"": ""Hydrogen Fuel"",
                    ""category"": ""Chemicals"",
                    ""avgprice"": 110,
                    ""rare"": false,
                    ""buyprice"": null,
                    ""stock"": null,
                    ""stockbracket"": null,
                    ""sellprice"": null,
                    ""demand"": null,
                    ""demandbracket"": null,
                    ""StatusFlags"": null,
                    ""EDDBID"": 2,
                    ""EDName"": ""HydrogenFuel""
                },
                ""haulageamounts"": []
            },
            {
                ""name"": ""Limpet"",
                ""stolen"": 0,
                ""haulage"": 0,
                ""other"": 6,
                ""total"": 6,
                ""ejected"": 0,
                ""price"": 101,
                ""category"": ""NonMarketable"",
                ""commodity"": {
                    ""name"": ""Limpet"",
                    ""category"": ""NonMarketable"",
                    ""avgprice"": 101,
                    ""rare"": false,
                    ""buyprice"": null,
                    ""stock"": null,
                    ""stockbracket"": null,
                    ""sellprice"": null,
                    ""demand"": null,
                    ""demandbracket"": null,
                    ""StatusFlags"": null,
                    ""EDDBID"": 84,
                    ""EDName"": ""Drones""
                },
                ""haulageamounts"": []
            }
            ],
            ""cargocarried"": 8
            }";

        string json_v3 = @"
            {
              ""cargo"": [
                {
                  ""edname"": ""HydrogenFuel"",
                  ""stolen"": 0,
                  ""haulage"": 0,
                  ""other"": 2,
                  ""total"": 2,
                  ""ejected"": 0,
                  ""price"": 110,
                  ""haulageamounts"": []
                },
                {
                  ""edname"": ""Drones"",
                  ""stolen"": 0,
                  ""haulage"": 0,
                  ""other"": 6,
                  ""total"": 6,
                  ""ejected"": 0,
                  ""price"": 101,
                  ""haulageamounts"": []
            }
              ],
              ""cargocarried"": 8
            }";

        [TestMethod]
        public void TestLoadCAPI()
        {
            CargoMonitorConfiguration config = CargoMonitorConfiguration.FromJsonString(legacyJSON);
            Assert.AreEqual(2, config.cargo.Count);
            Cargo cargo = config.cargo[1];
            Assert.AreEqual("Drones", cargo.commodityDef.edname);
            Assert.AreEqual(6, cargo.total);
        }

        [TestMethod]
        public void TestLoadJsonv3()
        {
            CargoMonitorConfiguration config = CargoMonitorConfiguration.FromJsonString(json_v3);
            Assert.AreEqual(2, config.cargo.Count);
            Cargo cargo = config.cargo[1];
            Assert.AreEqual("Drones", cargo.commodityDef.edname);
            Assert.AreEqual(6, cargo.total);
        }
    }
}
