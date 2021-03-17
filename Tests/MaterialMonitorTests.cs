using System;
using System.Linq;
using System.ServiceModel.Security;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
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
            Assert.AreEqual(EddiDataDefinitions.Properties.MaterialCategories.Element, Material.FromEDName(config.materials[1].edname).Category.localizedName);
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

            bool TestIncThreshold(int previous, int amount, int? target)
            {
                return (bool)privateObject.Invoke("incMaterialThreshold", previous, amount, target);
            }

            // If no threshold target is specified, the result must be false
            Assert.IsFalse(TestIncThreshold(149, 150, null));

            // If we haven't reached our target, the result must be false
            Assert.IsFalse(TestIncThreshold(149, 150, 200));

            // If we have reached our target, the result must be true
            Assert.IsTrue(TestIncThreshold(149, 150, 150));

            // If we're already beyond our target, the result must be false
            Assert.IsFalse(TestIncThreshold(149, 150, 100));

            bool TestDecThreshold(int previous, int amount, int? target)
            {
                return (bool)privateObject.Invoke("decMaterialThreshold", previous, amount, target);
            }

            // If no threshold target is specified, the result must be false
            Assert.IsFalse(TestDecThreshold(151, 150, null));

            // If we've already fallen below our target, the result must be false
            Assert.IsFalse(TestDecThreshold(151, 150, 200));

            // If we have fallen below our target, the result must be true
            Assert.IsTrue(TestDecThreshold(150, 149, 150));

            // If we haven't fallen below our target, the result must be false
            Assert.IsFalse(TestDecThreshold(151, 150, 150));
        }

        [TestMethod]
        public void TestMaterialInventoryEvent()
        {
            var materialMonitor = new MaterialMonitor();
            string line = @"{ ""timestamp"":""2021-01-23T02:50:29Z"", ""event"":""Materials"", ""Raw"":[ { ""Name"":""phosphorus"", ""Count"":231 }, { ""Name"":""mercury"", ""Count"":100 }, { ""Name"":""germanium"", ""Count"":244 }, { ""Name"":""manganese"", ""Count"":222 }, { ""Name"":""zirconium"", ""Count"":107 }, { ""Name"":""niobium"", ""Count"":130 }, { ""Name"":""vanadium"", ""Count"":164 }, { ""Name"":""yttrium"", ""Count"":112 }, { ""Name"":""carbon"", ""Count"":278 }, { ""Name"":""polonium"", ""Count"":65 }, { ""Name"":""nickel"", ""Count"":266 }, { ""Name"":""zinc"", ""Count"":167 }, { ""Name"":""molybdenum"", ""Count"":119 }, { ""Name"":""tungsten"", ""Count"":103 }, { ""Name"":""tin"", ""Count"":177 }, { ""Name"":""iron"", ""Count"":189 }, { ""Name"":""selenium"", ""Count"":75 }, { ""Name"":""arsenic"", ""Count"":101 }, { ""Name"":""chromium"", ""Count"":239 }, { ""Name"":""tellurium"", ""Count"":18 }, { ""Name"":""ruthenium"", ""Count"":39 }, { ""Name"":""technetium"", ""Count"":101 }, { ""Name"":""sulphur"", ""Count"":241 }, { ""Name"":""cadmium"", ""Count"":166 }, { ""Name"":""rhenium"", ""Count"":285 }, { ""Name"":""lead"", ""Count"":283 }, { ""Name"":""boron"", ""Count"":188 } ], ""Manufactured"":[ { ""Name"":""salvagedalloys"", ""Name_Localised"":""Salvaged Alloys"", ""Count"":257 }, { ""Name"":""shieldemitters"", ""Name_Localised"":""Shield Emitters"", ""Count"":227 }, { ""Name"":""conductivecomponents"", ""Name_Localised"":""Conductive Components"", ""Count"":179 }, { ""Name"":""mechanicalcomponents"", ""Name_Localised"":""Mechanical Components"", ""Count"":149 }, { ""Name"":""protolightalloys"", ""Name_Localised"":""Proto Light Alloys"", ""Count"":138 }, { ""Name"":""heatvanes"", ""Name_Localised"":""Heat Vanes"", ""Count"":122 }, { ""Name"":""hybridcapacitors"", ""Name_Localised"":""Hybrid Capacitors"", ""Count"":209 }, { ""Name"":""fedcorecomposites"", ""Name_Localised"":""Core Dynamics Composites"", ""Count"":29 }, { ""Name"":""highdensitycomposites"", ""Name_Localised"":""High Density Composites"", ""Count"":183 }, { ""Name"":""thermicalloys"", ""Name_Localised"":""Thermic Alloys"", ""Count"":150 }, { ""Name"":""heatexchangers"", ""Name_Localised"":""Heat Exchangers"", ""Count"":184 }, { ""Name"":""fedproprietarycomposites"", ""Name_Localised"":""Proprietary Composites"", ""Count"":116 }, { ""Name"":""improvisedcomponents"", ""Name_Localised"":""Improvised Components"", ""Count"":5 }, { ""Name"":""biotechconductors"", ""Name_Localised"":""Biotech Conductors"", ""Count"":71 }, { ""Name"":""gridresistors"", ""Name_Localised"":""Grid Resistors"", ""Count"":241 }, { ""Name"":""militarygradealloys"", ""Name_Localised"":""Military Grade Alloys"", ""Count"":100 }, { ""Name"":""heatdispersionplate"", ""Name_Localised"":""Heat Dispersion Plate"", ""Count"":238 }, { ""Name"":""exquisitefocuscrystals"", ""Name_Localised"":""Exquisite Focus Crystals"", ""Count"":99 }, { ""Name"":""mechanicalequipment"", ""Name_Localised"":""Mechanical Equipment"", ""Count"":231 }, { ""Name"":""conductiveceramics"", ""Name_Localised"":""Conductive Ceramics"", ""Count"":126 }, { ""Name"":""mechanicalscrap"", ""Name_Localised"":""Mechanical Scrap"", ""Count"":190 }, { ""Name"":""conductivepolymers"", ""Name_Localised"":""Conductive Polymers"", ""Count"":101 }, { ""Name"":""polymercapacitors"", ""Name_Localised"":""Polymer Capacitors"", ""Count"":93 }, { ""Name"":""compoundshielding"", ""Name_Localised"":""Compound Shielding"", ""Count"":124 }, { ""Name"":""refinedfocuscrystals"", ""Name_Localised"":""Refined Focus Crystals"", ""Count"":117 }, { ""Name"":""protoheatradiators"", ""Name_Localised"":""Proto Heat Radiators"", ""Count"":92 }, { ""Name"":""heatconductionwiring"", ""Name_Localised"":""Heat Conduction Wiring"", ""Count"":250 }, { ""Name"":""chemicalmanipulators"", ""Name_Localised"":""Chemical Manipulators"", ""Count"":135 }, { ""Name"":""configurablecomponents"", ""Name_Localised"":""Configurable Components"", ""Count"":120 }, { ""Name"":""precipitatedalloys"", ""Name_Localised"":""Precipitated Alloys"", ""Count"":195 }, { ""Name"":""unknowntechnologycomponents"", ""Name_Localised"":""Thargoid Technological Components"", ""Count"":30 }, { ""Name"":""unknownorganiccircuitry"", ""Name_Localised"":""Thargoid Organic Circuitry"", ""Count"":12 }, { ""Name"":""unknownenergycell"", ""Name_Localised"":""Thargoid Energy Cell"", ""Count"":19 }, { ""Name"":""unknownenergysource"", ""Name_Localised"":""Sensor Fragment"", ""Count"":13 }, { ""Name"":""unknowncarapace"", ""Name_Localised"":""Thargoid Carapace"", ""Count"":36 }, { ""Name"":""chemicaldistillery"", ""Name_Localised"":""Chemical Distillery"", ""Count"":200 }, { ""Name"":""shieldingsensors"", ""Name_Localised"":""Shielding Sensors"", ""Count"":188 }, { ""Name"":""focuscrystals"", ""Name_Localised"":""Focus Crystals"", ""Count"":189 }, { ""Name"":""wornshieldemitters"", ""Name_Localised"":""Worn Shield Emitters"", ""Count"":222 }, { ""Name"":""electrochemicalarrays"", ""Name_Localised"":""Electrochemical Arrays"", ""Count"":190 }, { ""Name"":""militarysupercapacitors"", ""Name_Localised"":""Military Supercapacitors"", ""Count"":3 }, { ""Name"":""uncutfocuscrystals"", ""Name_Localised"":""Flawed Focus Crystals"", ""Count"":235 }, { ""Name"":""protoradiolicalloys"", ""Name_Localised"":""Proto Radiolic Alloys"", ""Count"":44 }, { ""Name"":""phasealloys"", ""Name_Localised"":""Phase Alloys"", ""Count"":200 }, { ""Name"":""pharmaceuticalisolators"", ""Name_Localised"":""Pharmaceutical Isolators"", ""Count"":59 }, { ""Name"":""imperialshielding"", ""Name_Localised"":""Imperial Shielding"", ""Count"":97 }, { ""Name"":""chemicalprocessors"", ""Name_Localised"":""Chemical Processors"", ""Count"":230 }, { ""Name"":""galvanisingalloys"", ""Name_Localised"":""Galvanising Alloys"", ""Count"":241 }, { ""Name"":""basicconductors"", ""Name_Localised"":""Basic Conductors"", ""Count"":286 }, { ""Name"":""heatresistantceramics"", ""Name_Localised"":""Heat Resistant Ceramics"", ""Count"":231 }, { ""Name"":""temperedalloys"", ""Name_Localised"":""Tempered Alloys"", ""Count"":270 }, { ""Name"":""crystalshards"", ""Name_Localised"":""Crystal Shards"", ""Count"":232 }, { ""Name"":""guardian_powerconduit"", ""Name_Localised"":""Guardian Power Conduit"", ""Count"":219 }, { ""Name"":""guardian_powercell"", ""Name_Localised"":""Guardian Power Cell"", ""Count"":231 }, { ""Name"":""guardian_techcomponent"", ""Name_Localised"":""Guardian Technology Component"", ""Count"":30 }, { ""Name"":""guardian_sentinel_wreckagecomponents"", ""Name_Localised"":""Guardian Wreckage Components"", ""Count"":78 }, { ""Name"":""guardian_sentinel_weaponparts"", ""Name_Localised"":""Guardian Sentinel Weapon Parts"", ""Count"":162 }, { ""Name"":""compactcomposites"", ""Name_Localised"":""Compact Composites"", ""Count"":209 }, { ""Name"":""chemicalstorageunits"", ""Name_Localised"":""Chemical Storage Units"", ""Count"":269 }, { ""Name"":""filamentcomposites"", ""Name_Localised"":""Filament Composites"", ""Count"":224 }, { ""Name"":""tg_propulsionelement"", ""Name_Localised"":""Propulsion Elements"", ""Count"":66 }, { ""Name"":""tg_biomechanicalconduits"", ""Name_Localised"":""Bio-Mechanical Conduits"", ""Count"":75 }, { ""Name"":""tg_wreckagecomponents"", ""Name_Localised"":""Wreckage Components"", ""Count"":61 }, { ""Name"":""tg_weaponparts"", ""Name_Localised"":""Weapon Parts"", ""Count"":111 } ], ""Encoded"":[ { ""Name"":""shieldsoakanalysis"", ""Name_Localised"":""Inconsistent Shield Soak Analysis"", ""Count"":241 }, { ""Name"":""scrambledemissiondata"", ""Name_Localised"":""Exceptional Scrambled Emission Data"", ""Count"":263 }, { ""Name"":""encodedscandata"", ""Name_Localised"":""Divergent Scan Data"", ""Count"":73 }, { ""Name"":""hyperspacetrajectories"", ""Name_Localised"":""Eccentric Hyperspace Trajectories"", ""Count"":133 }, { ""Name"":""encryptioncodes"", ""Name_Localised"":""Tagged Encryption Codes"", ""Count"":217 }, { ""Name"":""disruptedwakeechoes"", ""Name_Localised"":""Atypical Disrupted Wake Echoes"", ""Count"":267 }, { ""Name"":""wakesolutions"", ""Name_Localised"":""Strange Wake Solutions"", ""Count"":169 }, { ""Name"":""symmetrickeys"", ""Name_Localised"":""Open Symmetric Keys"", ""Count"":158 }, { ""Name"":""securityfirmware"", ""Name_Localised"":""Security Firmware Patch"", ""Count"":85 }, { ""Name"":""decodedemissiondata"", ""Name_Localised"":""Decoded Emission Data"", ""Count"":148 }, { ""Name"":""shieldpatternanalysis"", ""Name_Localised"":""Aberrant Shield Pattern Analysis"", ""Count"":150 }, { ""Name"":""unknownshipsignature"", ""Name_Localised"":""Thargoid Ship Signature"", ""Count"":45 }, { ""Name"":""unknownwakedata"", ""Name_Localised"":""Thargoid Wake Data"", ""Count"":9 }, { ""Name"":""ancienttechnologicaldata"", ""Name_Localised"":""Pattern Epsilon Obelisk Data"", ""Count"":75 }, { ""Name"":""ancientlanguagedata"", ""Name_Localised"":""Pattern Delta Obelisk Data"", ""Count"":150 }, { ""Name"":""ancienthistoricaldata"", ""Name_Localised"":""Pattern Gamma Obelisk Data"", ""Count"":150 }, { ""Name"":""ancientbiologicaldata"", ""Name_Localised"":""Pattern Alpha Obelisk Data"", ""Count"":150 }, { ""Name"":""ancientculturaldata"", ""Name_Localised"":""Pattern Beta Obelisk Data"", ""Count"":150 }, { ""Name"":""fsdtelemetry"", ""Name_Localised"":""Anomalous FSD Telemetry"", ""Count"":249 }, { ""Name"":""bulkscandata"", ""Name_Localised"":""Anomalous Bulk Scan Data"", ""Count"":282 }, { ""Name"":""emissiondata"", ""Name_Localised"":""Unexpected Emission Data"", ""Count"":142 }, { ""Name"":""shieldcyclerecordings"", ""Name_Localised"":""Distorted Shield Cycle Recordings"", ""Count"":300 }, { ""Name"":""embeddedfirmware"", ""Name_Localised"":""Modified Embedded Firmware"", ""Count"":39 }, { ""Name"":""legacyfirmware"", ""Name_Localised"":""Specialised Legacy Firmware"", ""Count"":247 }, { ""Name"":""encryptedfiles"", ""Name_Localised"":""Unusual Encrypted Files"", ""Count"":283 }, { ""Name"":""archivedemissiondata"", ""Name_Localised"":""Irregular Emission Data"", ""Count"":213 }, { ""Name"":""consumerfirmware"", ""Name_Localised"":""Modified Consumer Firmware"", ""Count"":212 }, { ""Name"":""shieldfrequencydata"", ""Name_Localised"":""Peculiar Shield Frequency Data"", ""Count"":36 }, { ""Name"":""tg_residuedata"", ""Name_Localised"":""Thargoid Residue Data"", ""Count"":24 }, { ""Name"":""tg_structuraldata"", ""Name_Localised"":""Thargoid Structural Data"", ""Count"":33 }, { ""Name"":""tg_compositiondata"", ""Name_Localised"":""Thargoid Material Composition Data"", ""Count"":30 }, { ""Name"":""classifiedscandata"", ""Name_Localised"":""Classified Scan Fragment"", ""Count"":38 }, { ""Name"":""compactemissionsdata"", ""Name_Localised"":""Abnormal Compact Emissions Data"", ""Count"":31 }, { ""Name"":""encryptionarchives"", ""Name_Localised"":""Atypical Encryption Archives"", ""Count"":121 }, { ""Name"":""shielddensityreports"", ""Name_Localised"":""Untypical Shield Scans "", ""Count"":182 }, { ""Name"":""industrialfirmware"", ""Name_Localised"":""Cracked Industrial Firmware"", ""Count"":25 }, { ""Name"":""dataminedwake"", ""Name_Localised"":""Datamined Wake Exceptions"", ""Count"":29 }, { ""Name"":""scandatabanks"", ""Name_Localised"":""Classified Scan Databanks"", ""Count"":181 }, { ""Name"":""scanarchives"", ""Name_Localised"":""Unidentified Scan Archives"", ""Count"":234 }, { ""Name"":""adaptiveencryptors"", ""Name_Localised"":""Adaptive Encryptors Capture"", ""Count"":40 }, { ""Name"":""guardian_vesselblueprint"", ""Name_Localised"":""Guardian Vessel Blueprint Fragment"", ""Count"":5 }, { ""Name"":""guardian_moduleblueprint"", ""Name_Localised"":""Guardian Module Blueprint Fragment"", ""Count"":6 } ] }";
            var events = JournalMonitor.ParseJournalEntry(line);
            MaterialInventoryEvent @event = (MaterialInventoryEvent)events[0];

            int? InventoryAmount(string edname)
            {
                return materialMonitor.inventory
                    .SingleOrDefault(m => string.Equals(m.edname, edname, StringComparison.InvariantCultureIgnoreCase))
                    ?.amount;
            }

            // Add antimony so that we can test whether obsolete data in our inventory but not listed in the event is corrected 
            var antimony = materialMonitor.inventory.Single(m => string.Equals(m.edname, "antimony", StringComparison.InvariantCultureIgnoreCase));
            if (antimony is null)
            {
                antimony = new MaterialAmount("antimony", 5, 25, 50, 75);
                materialMonitor.inventory.Add(antimony);
            }
            else
            {
                antimony.amount = 5;
            }
            Assert.AreEqual(5, InventoryAmount("antimony"));

            // Handle our event
            materialMonitor.PreHandle(@event);

            // Test materials listed in the event
            Assert.AreEqual(231, InventoryAmount("phosphorus"));
            Assert.AreEqual(100, InventoryAmount("mercury"));
            Assert.AreEqual(244, InventoryAmount("germanium"));

            // Test materials not listed in the event
            Assert.AreEqual(0, InventoryAmount("antimony"));
            Assert.AreEqual(0, InventoryAmount("guardian_weaponblueprint"));

            // Test unknown materials
            Assert.IsNull(InventoryAmount("unobtainum"));
        }
    }
}
