using System;
using EddiCargoMonitor;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CargoMonitorTests
    {
        string json = @"{
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

        [TestMethod]
        public void TestLoadCAPI()
        {
            CargoMonitorConfiguration config = CargoMonitorConfiguration.FromJsonString(json);
            Assert.AreEqual(2, config.cargo.Count);
            Cargo cargo = config.cargo[1];
            Assert.AreEqual("Drones", cargo.commodity.EDName);
            Assert.AreEqual(6, cargo.total);
        }
    }
}
