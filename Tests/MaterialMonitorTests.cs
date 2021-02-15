using EddiDataDefinitions;
using EddiMaterialMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UnitTests
{
    [TestClass]
    public class MaterialMonitorTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
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

        [TestMethod]
        public void TestMaterialThresholds()
        {
            var privateObject = new PrivateObject(new MaterialMonitor());

            bool TestThreshold(int previous, int amount, int? target)
            {
                return (bool)privateObject.Invoke("materialThreshold", previous, amount, target);
            }

            // If no threshold target is specified, the result must be false
            Assert.IsFalse(TestThreshold(149, 150, null));

            // If we haven't reached our target, the result must be false
            Assert.IsFalse(TestThreshold(149, 150, 200));

            // If we have reached our target, the result must be true
            Assert.IsTrue(TestThreshold(149, 150, 150));

            // If we're already beyond our target, the result must be false
            Assert.IsFalse(TestThreshold(149, 150, 100));
        }
    }
}
