using EddiCargoMonitor;
using EddiEvents;
using GalnetMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class MetaVariablesTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestGalnetNewsPublishedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Galnet news published", typeof(GalnetNewsPublishedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(7, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].category"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].content"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].id"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].published"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].read"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].title"));
            Assert.IsNotNull(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(7, vaVars.Count);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == @"EDDI galnet news published items \<index\> category")?.variableType);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == @"EDDI galnet news published items \<index\> content")?.variableType);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == @"EDDI galnet news published items \<index\> id")?.variableType);
            Assert.AreEqual(typeof(DateTime), vaVars.FirstOrDefault(k => k.key == @"EDDI galnet news published items \<index\> published")?.variableType);
            Assert.AreEqual(typeof(bool), vaVars.FirstOrDefault(k => k.key == @"EDDI galnet news published items \<index\> read")?.variableType);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == @"EDDI galnet news published items \<index\> title")?.variableType);
            Assert.AreEqual(typeof(int), vaVars.FirstOrDefault(k => k.key == @"EDDI galnet news published items")?.variableType);
            Assert.IsTrue(vaVars.TrueForAll(v => v.value == null));
        }

        [TestMethod]
        public void TestSRVTurretDeployableEvent()
        {
            var entry = new KeyValuePair<string, Type>("SRV turret deployable", typeof(SRVTurretDeployableEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(1, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"deployable")?.key);
            Assert.IsTrue(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(1, vaVars.Count);
            Assert.AreEqual(typeof(bool), vaVars.FirstOrDefault(k => k.key == @"EDDI srv turret deployable")?.variableType);
            Assert.IsTrue(vaVars.TrueForAll(v => v.value == null));
        }

        [TestMethod]
        public void TestExplorationDataSoldEvent()
        {
            var entry = new KeyValuePair<string, Type>("Exploration data sold", typeof(ExplorationDataSoldEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(5, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"systems"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"systems[\<index\>]"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "reward"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "bonus"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "total"));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(5, vaVars.Count);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == "EDDI exploration data sold systems \\<index\\>").variableType);
            Assert.AreEqual(typeof(int), vaVars.FirstOrDefault(k => k.key == "EDDI exploration data sold systems").variableType);
            Assert.AreEqual(typeof(decimal), vaVars.FirstOrDefault(k => k.key == "EDDI exploration data sold reward").variableType);
            Assert.AreEqual(typeof(decimal), vaVars.FirstOrDefault(k => k.key == "EDDI exploration data sold bonus").variableType);
            Assert.AreEqual(typeof(decimal), vaVars.FirstOrDefault(k => k.key == "EDDI exploration data sold total").variableType);
        }

        [TestMethod]
        public void TestDiscoveryScanEvent()
        {
            var entry = new KeyValuePair<string, Type>("Discovery scan", typeof(DiscoveryScanEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(2, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "totalbodies"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "nonbodies"));
            Assert.IsNull(cottleVars.FirstOrDefault(k => k.key == "progress"));
            Assert.IsTrue(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(2, vaVars.Count);
            Assert.AreEqual(typeof(int), vaVars.FirstOrDefault(k => k.key == "EDDI discovery scan totalbodies").variableType);
            Assert.AreEqual(typeof(int), vaVars.FirstOrDefault(k => k.key == "EDDI discovery scan nonbodies").variableType);
            Assert.IsNull(vaVars.FirstOrDefault(k => k.key == "EDDI discovery scan progress")?.variableType);
        }

        [TestMethod]
        public void TestAsteroidProspectedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Asteroid prospected", typeof(AsteroidProspectedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(6, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities[\\<index\\>].commodity"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities[\\<index\\>].percentage"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "materialcontent"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "remaining"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "motherlode"));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(6, vaVars.Count);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == "EDDI asteroid prospected commodities \\<index\\> commodity").variableType);
            Assert.AreEqual(typeof(decimal), vaVars.FirstOrDefault(k => k.key == "EDDI asteroid prospected commodities \\<index\\> percentage").variableType);
            Assert.AreEqual(typeof(int), vaVars.FirstOrDefault(k => k.key == "EDDI asteroid prospected commodities").variableType);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == "EDDI asteroid prospected materialcontent").variableType);
            Assert.AreEqual(typeof(decimal), vaVars.FirstOrDefault(k => k.key == "EDDI asteroid prospected remaining").variableType);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == "EDDI asteroid prospected motherlode").variableType);
        }

        [TestMethod]
        public void TestShipFSDEvent()
        {
            var entry = new KeyValuePair<string, Type>("Ship fsd", typeof(ShipFsdEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(1, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "fsd_status"));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == "EDDI ship fsd status").variableType);
        }

        [TestMethod]
        public void TestCommodityEjectedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Commodity ejected", typeof(CommodityEjectedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            Assert.AreEqual(4, vars.Count);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "commodity").description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "amount").description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "missionid").description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "abandoned").description);

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(4, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodity"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "amount"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "missionid"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "abandoned"));

            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodity").description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "amount").description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "missionid").description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "abandoned").description);

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(4, vaVars.Count);
            Assert.AreEqual(typeof(string), vaVars.FirstOrDefault(k => k.key == "EDDI commodity ejected commodity").variableType);
            Assert.AreEqual(typeof(int), vaVars.FirstOrDefault(k => k.key == "EDDI commodity ejected amount").variableType);
            Assert.AreEqual(typeof(decimal), vaVars.FirstOrDefault(k => k.key == "EDDI commodity ejected missionid").variableType);
            Assert.AreEqual(typeof(bool), vaVars.FirstOrDefault(k => k.key == "EDDI commodity ejected abandoned").variableType);

            Assert.AreEqual("The name of the commodity ejected", vaVars.FirstOrDefault(k => k.key == "EDDI commodity ejected commodity").description);
            Assert.AreEqual("The amount of commodity ejected", vaVars.FirstOrDefault(k => k.key == "EDDI commodity ejected amount").description);
            Assert.AreEqual("ID of the mission-related commodity, if applicable", vaVars.FirstOrDefault(k => k.key == "EDDI commodity ejected missionid").description);
            Assert.AreEqual("True if the cargo has been abandoned", vaVars.FirstOrDefault(k => k.key == "EDDI commodity ejected abandoned").description);
        }
    }
}
