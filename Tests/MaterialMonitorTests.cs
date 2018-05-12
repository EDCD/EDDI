using System.Resources;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiDataDefinitions;
using EddiMaterialMonitor;
using Rollbar;

namespace UnitTests
{
    [TestClass]
    public class MaterialMonitorTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        string json = @"{
            ""materials"": [
            {
                ""edname"": ""shieldpatternanalysis"",
                ""amount"": 7,
                ""minimum"": null,
                ""desired"": null,
                ""maximum"": null,
            },

            {
                ""edname"": ""zirconium"",
                ""amount"": 13,
                ""minimum"": null,
                ""desired"": 50,
                ""maximum"": 100,
            }
            ]
            }";

        [TestMethod]
        public void TestMaterialMonitor()
        {
            MaterialMonitorConfiguration config = MaterialMonitorConfiguration.FromJsonString(json);
            Assert.AreEqual(2, config.materials.Count);

            MaterialAmount zirconiumAmount = config.materials[1];
            Assert.AreEqual("zirconium", zirconiumAmount.edname);
            Assert.AreEqual(13, zirconiumAmount.amount);
            Assert.AreEqual(EddiDataDefinitions.Properties.Materials.zirconium, Material.FromEDName(config.materials[1].edname).localizedName);
            Assert.AreEqual(100, config.materials[1].maximum);
            Assert.AreEqual(50, config.materials[1].desired);
            Assert.IsNull(config.materials[1].minimum);
            Assert.AreEqual(EddiDataDefinitions.Properties.MaterialCategories.Element, Material.FromEDName(config.materials[1].edname).category.localizedName);
        }
    }
}
