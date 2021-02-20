using EddiCargoMonitor;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiJournalMonitor;
using EddiVoiceAttackResponder;
using GalnetMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

            List<VoiceAttackVariable> setVars = new List<VoiceAttackVariable>();
            VoiceAttackVariables.PrepareEventVariables(ev.type, $"EDDI {ev.type.ToLowerInvariant()}", typeof(DiscoveryScanEvent), ref setVars, true, ev);
            VoiceAttackVariables.SetEventVariables(vaProxy, setVars);

            Assert.AreEqual(2, setVars.Count);
            Assert.AreEqual(7, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan totalbodies").Value);
            Assert.AreEqual(3, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan nonbodies").Value);
            Assert.IsNull(vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan progress").Value);
            Assert.AreEqual(setVars.Count, vaProxy.vaVars.Count, "The 'setVars' list should match the keys set in the 'vaProxy.vaVars' list");
            foreach (VoiceAttackVariable variable in setVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.Key), "Unmatched key");
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

            List<VoiceAttackVariable> setVars = new List<VoiceAttackVariable>();
            VoiceAttackVariables.PrepareEventVariables(ev.type, $"EDDI {ev.type.ToLowerInvariant()}", typeof(AsteroidProspectedEvent), ref setVars, true, ev);
            VoiceAttackVariables.SetEventVariables(vaProxy, setVars);

            Assert.AreEqual(26, setVars.Count);
            Assert.AreEqual(90M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected remaining").Value);
            Assert.AreEqual("Alexandrite", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected motherlode").Value);
            Assert.AreEqual("Low Temperature Diamonds", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 commodity").Value);
            Assert.AreEqual(57445M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 commodity definition avgprice").Value);
            Assert.IsNull(vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 commodity definition category fallback localized name").Value);
            Assert.AreEqual("Minerals", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 commodity definition category invariant name").Value);
            Assert.AreEqual(128673848M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 commodity definition elite id").Value);
            Assert.AreEqual("Low Temperature Diamonds", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 commodity definition invariant name").Value);
            Assert.IsFalse((bool)vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 commodity definition rare").Value);
            Assert.AreEqual(26.078022M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 0 percentage").Value);
            Assert.AreEqual("Hydrogen Peroxide", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 1 commodity").Value);
            Assert.AreEqual(10.189009M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities 1 percentage").Value);
            Assert.AreEqual(2, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected commodities entries").Value);
            Assert.AreEqual("Low", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI asteroid prospected materialcontent").Value);
            foreach (VoiceAttackVariable variable in setVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.Key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVAShipFSDEvent()
        {
            // Test a generated variable name from overlapping strings.
            // The prefix "EDDI ship fsd" should be merged with the formatted child key "fsd status" to yield "EDDI ship fsd status".
            ShipFsdEvent ev = new ShipFsdEvent (DateTime.UtcNow, "ready");

            List<VoiceAttackVariable> setVars = new List<VoiceAttackVariable>();
            VoiceAttackVariables.PrepareEventVariables(ev.type, $"EDDI {ev.type.ToLowerInvariant()}", typeof(ShipFsdEvent), ref setVars, true, ev);
            VoiceAttackVariables.SetEventVariables(vaProxy, setVars);

            Assert.AreEqual(1, setVars.Count);
            Assert.AreEqual("ready", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI ship fsd status").Value);
            foreach (VoiceAttackVariable variable in setVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.Key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVACommodityEjectedEvent()
        {
            // Test a generated variable name from overlapping strings.
            // The prefix "EDDI ship fsd" should be merged with the formatted child key "fsd status" to yield "EDDI ship fsd status".
            CommodityEjectedEvent ev = new CommodityEjectedEvent(DateTime.UtcNow, CommodityDefinition.FromEDName("Water"), 5, null, true);

            List<VoiceAttackVariable> setVars = new List<VoiceAttackVariable>();
            VoiceAttackVariables.PrepareEventVariables(ev.type, $"EDDI {ev.type.ToLowerInvariant()}", typeof(CommodityEjectedEvent), ref setVars, true, ev);
            VoiceAttackVariables.SetEventVariables(vaProxy, setVars);

            Assert.AreEqual(4, setVars.Count);
            Assert.AreEqual("Water", vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI commodity ejected commodity").Value);
            Assert.AreEqual(5, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI commodity ejected amount").Value);
            Assert.IsNull(vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI commodity ejected missionid").Value);
            Assert.AreEqual(true, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI commodity ejected abandoned").Value);
            foreach (VoiceAttackVariable variable in setVars)
            {
                Assert.IsTrue(vaProxy.vaVars.ContainsKey(variable.Key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestGalnetNewsPublishedEvent()
        {
            var setVars = new List<VoiceAttackVariable>();
            var entry = new KeyValuePair<string, Type>("Galnet news published", typeof(GalnetNewsPublishedEvent));
            VoiceAttackVariables.PrepareEventVariables(entry.Key, $"EDDI {entry.Key.ToLowerInvariant()}", entry.Value, ref setVars);

            Assert.AreEqual(7, setVars.Count);
            Assert.AreEqual(typeof(string), setVars.FirstOrDefault(k => k.Key == @"EDDI galnet news published items *\<index\>* category")?.Type);
            Assert.AreEqual(typeof(string), setVars.FirstOrDefault(k => k.Key == @"EDDI galnet news published items *\<index\>* content")?.Type);
            Assert.AreEqual(typeof(string), setVars.FirstOrDefault(k => k.Key == @"EDDI galnet news published items *\<index\>* id")?.Type);
            Assert.AreEqual(typeof(DateTime), setVars.FirstOrDefault(k => k.Key == @"EDDI galnet news published items *\<index\>* published")?.Type);
            Assert.AreEqual(typeof(bool), setVars.FirstOrDefault(k => k.Key == @"EDDI galnet news published items *\<index\>* read")?.Type);
            Assert.AreEqual(typeof(string), setVars.FirstOrDefault(k => k.Key == @"EDDI galnet news published items *\<index\>* title")?.Type);
            Assert.AreEqual(typeof(int), setVars.FirstOrDefault(k => k.Key == @"EDDI galnet news published items entries")?.Type);
        }
    }
}
