using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiJournalMonitor;
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

            List<string> setKeys = new List<string>();
            EddiVoiceAttackResponder.VoiceAttackVariables.setEventValues(vaProxy, ev, setKeys);
            EddiVoiceAttackResponder.VoiceAttackVariables.setEventExtendedValues(vaProxy, "EDDI " + ev.type.ToLowerInvariant(), JsonConvert.DeserializeObject(JsonConvert.SerializeObject(ev)), setKeys);

            Assert.AreEqual(7, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan totalbodies").Value);
            Assert.AreEqual(3, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan nonbodies").Value);
            Assert.AreEqual(44M, vaProxy.vaVars.FirstOrDefault(k => k.Key == "EDDI discovery scan progress").Value);
        }
    }
}
