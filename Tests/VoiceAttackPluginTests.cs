using EddiCargoMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace UnitTests
{
    public class MockVAProxy
    {
        public List<string> vaLog = new List<string>();
        public Dictionary<string, object> vaVars = new Dictionary<string, object>();

        public void WriteToLog(string msg, string color = null)
        {
            vaLog.Add(msg);
        }

        public void SetText(string varName, string value)
        {
            vaVars.Add(varName, value);
        }

        public void SetInt(string varName, int? value)
        {
            vaVars.Add(varName, value);
        }

        public void SetBoolean(string varName, bool? value)
        {
            vaVars.Add(varName, value);
        }

        public void SetDecimal(string varName, decimal? value)
        {
            vaVars.Add(varName, value);
        }

        public void SetDate(string varName, DateTime? value)
        {
            vaVars.Add(varName, value);
        }
    }

    [TestClass]
    public class VoiceAttackPluginTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        private MockVAProxy vaProxy = new MockVAProxy();

        [TestMethod]
        public void TestVAExplorationDataSoldEvent()
        {
            string line = @"{ ""timestamp"":""2016-09-23T18:57:55Z"", ""event"":""SellExplorationData"", ""Systems"":[ ""Gamma Tucanae"", ""Rho Capricorni"", ""Dain"", ""Col 285 Sector BR-S b18-0"", ""LP 571-80"", ""Kawilocidi"", ""Irulachan"", ""Alrai Sector MC-M a7-0"", ""Col 285 Sector FX-Q b19-5"", ""Col 285 Sector EX-Q b19-7"", ""Alrai Sector FB-O a6-3"" ], ""Discovered"":[ ""Irulachan"" ], ""BaseValue"":63573, ""Bonus"":1445, ""TotalEarnings"":65018 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(ExplorationDataSoldEvent));
            var ev = events[0] as ExplorationDataSoldEvent;

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = vars.AsVoiceAttackVariables(ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(15, vaVars.Count);
            Assert.AreEqual("Gamma Tucanae", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 0").Value);
            Assert.AreEqual("Rho Capricorni", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 1").Value);
            Assert.AreEqual("Dain", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 2").Value);
            Assert.AreEqual("Col 285 Sector BR-S b18-0", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 3").Value);
            Assert.AreEqual("LP 571-80", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 4").Value);
            Assert.AreEqual("Kawilocidi", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 5").Value);
            Assert.AreEqual("Irulachan", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 6").Value);
            Assert.AreEqual("Alrai Sector MC-M a7-0", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 7").Value);
            Assert.AreEqual("Col 285 Sector FX-Q b19-5", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 8").Value);
            Assert.AreEqual("Col 285 Sector EX-Q b19-7", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 9").Value);
            Assert.AreEqual("Alrai Sector FB-O a6-3", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems 10").Value);
            Assert.AreEqual(11, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold systems entries").Value);
            Assert.AreEqual(63573M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold reward").Value);
            Assert.AreEqual(1445M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold bonus").Value);
            Assert.AreEqual(65018M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI exploration data sold total").Value);
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVADiscoveryScanEvent()
        {
            string line = @"{ ""timestamp"":""2019-10-26T02:15:49Z"", ""event"":""FSSDiscoveryScan"", ""Progress"":0.439435, ""BodyCount"":7, ""NonBodyCount"":3, ""SystemName"":""Outotz WO-A d1"", ""SystemAddress"":44870715523 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(DiscoveryScanEvent));
            DiscoveryScanEvent ev = events[0] as DiscoveryScanEvent;

            Assert.AreEqual(7, ev.totalbodies);
            Assert.AreEqual(3, ev.nonbodies);
            Assert.AreEqual(44, ev.progress);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = vars.AsVoiceAttackVariables(ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(2, vaVars.Count);
            Assert.AreEqual(7, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan totalbodies").Value);
            Assert.AreEqual(3, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan nonbodies").Value);
            Assert.IsNull(vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan progress").Value);
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVAAsteroidProspectedEvent()
        {
            string line = "{ \"timestamp\":\"2020-04-10T02:32:21Z\", \"event\":\"ProspectedAsteroid\", \"Materials\":[ { \"Name\":\"LowTemperatureDiamond\", \"Name_Localised\":\"Low Temperature Diamonds\", \"Proportion\":26.078022 }, { \"Name\":\"HydrogenPeroxide\", \"Name_Localised\":\"Hydrogen Peroxide\", \"Proportion\":10.189009 } ], \"MotherlodeMaterial\":\"Alexandrite\", \"Content\":\"$AsteroidMaterialContent_Low;\", \"Content_Localised\":\"Material Content: Low\", \"Remaining\":90.000000 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(AsteroidProspectedEvent));
            AsteroidProspectedEvent ev = events[0] as AsteroidProspectedEvent;

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = vars.AsVoiceAttackVariables(ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(8, vaVars.Count);
            Assert.AreEqual(90M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected remaining").Value);
            Assert.AreEqual("Alexandrite", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected motherlode").Value);
            Assert.AreEqual("Low Temperature Diamonds", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 commodity").Value);
            Assert.AreEqual(26.078022M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 percentage").Value);
            Assert.AreEqual("Hydrogen Peroxide", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 1 commodity").Value);
            Assert.AreEqual(10.189009M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 1 percentage").Value);
            Assert.AreEqual(2, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities entries").Value);
            Assert.AreEqual("Low", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected materialcontent").Value);
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVAShipFSDEvent()
        {
            // Test a generated variable name from overlapping strings.
            // The prefix "EDDI ship fsd" should be merged with the formatted child key "fsd status" to yield "EDDI ship fsd status".
            ShipFsdEvent ev = new ShipFsdEvent (DateTime.UtcNow, "ready");
            var vars = new MetaVariables(ev.GetType(), ev).Results;
            
            var vaVars = vars.AsVoiceAttackVariables(ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(1, vaVars.Count);
            Assert.AreEqual("ready", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI ship fsd status").Value);
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVACommodityEjectedEvent()
        {
            // Test a generated variable name from overlapping strings.
            // The prefix "EDDI ship fsd" should be merged with the formatted child key "fsd status" to yield "EDDI ship fsd status".
            CommodityEjectedEvent ev = new CommodityEjectedEvent(DateTime.UtcNow, CommodityDefinition.FromEDName("Water"), 5, null, true);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(4, cottleVars.Count);
            Assert.AreEqual("Water", cottleVars.FirstOrDefault(k => k.key == "commodity").value);
            Assert.AreEqual(5, cottleVars.FirstOrDefault(k => k.key == "amount").value);
            Assert.IsNull(cottleVars.FirstOrDefault(k => k.key == "missionid").value);
            Assert.AreEqual(true, cottleVars.FirstOrDefault(k => k.key == "abandoned").value);

            var vaVars = vars.AsVoiceAttackVariables(ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(4, vaVars.Count);
            Assert.AreEqual("Water", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI commodity ejected commodity").Value);
            Assert.AreEqual(5, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI commodity ejected amount").Value);
            Assert.IsNull(vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI commodity ejected missionid").Value);
            Assert.AreEqual(true, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI commodity ejected abandoned").Value);
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.key), "Unmatched key");
            }
        }
    }
}
