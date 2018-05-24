using EddiDataDefinitions;
using EddiMaterialMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rollbar;
using System.Collections.Generic;

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

        [TestMethod]
        public void TestBlueprintMaterialsFromJson()
        {
            string json = @"{
	""Thermal resistant Bulkheads grade 5"": {

        ""materials"": [{
				""amount"": 1,
				""material"": ""Proto Heat Radiators""

            }, {
				""amount"": 1,
				""material"": ""Molybdenum""
			}, {
				""amount"": 1,
				""material"": ""Phase Alloys""
			}
		]
	}
}";
            Dictionary<string, BlueprintMaterials> blueprints = JsonConvert.DeserializeObject<Dictionary<string, BlueprintMaterials>>(json);
            Assert.AreEqual(1, blueprints.Count);
        }

        [TestMethod]
        public void TestMaterialAmountFromJson()
        {
            string json = @"{
				""amount"": 1,
                ""material"": ""Molybdenum""
            }";
            MaterialAmount materialAmount = JsonConvert.DeserializeObject<MaterialAmount>(json);
            Assert.AreEqual(1, materialAmount.amount);
            Assert.AreEqual("Molybdenum", materialAmount.material);
        }
    }
}
