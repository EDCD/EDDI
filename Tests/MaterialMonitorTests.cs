using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiDataDefinitions;
using EddiMaterialMonitor;

namespace UnitTests
{
    [TestClass]
    public class MaterialMonitorTests
    {
        string json = @"{
            ""materials"": [
            {
                ""edname"": ""shieldpatternanalysis"",
                ""material"": ""Aberrant Shield Pattern Analysis"",
                ""amount"": 7,
                ""minimum"": null,
                ""desired"": null,
                ""maximum"": null,
                ""Category"": ""Encoded""
            },

            {
                ""edname"": ""zirconium"",
                ""material"": ""Zirconium"",
                ""amount"": 13,
                ""minimum"": null,
                ""desired"": null,
                ""maximum"": null,
                ""Category"": ""Elements""
            }
            ]
            }";

        [TestMethod]
        public void TestLoadCAPI()
        {
            
            MaterialMonitorConfiguration config = MaterialMonitorConfiguration.FromJsonString(json);
            Assert.AreEqual(2, config.materials.Count);
            MaterialAmount zirconiumAmount = config.materials[1];
            Assert.AreEqual("zirconium", zirconiumAmount.edname);
            Assert.AreEqual(13, zirconiumAmount.amount);
        }
    }
}
