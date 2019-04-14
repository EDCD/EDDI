using EddiBgsService;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    // Tests for the EliteBGS Service

    [TestClass]
    public class BgsDataTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        [DeploymentItem("bgsFaction.json")]
        public void TestFaction()
        {
            string dateTimeStringFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'";

            // Test factions data
            string jsonString = System.IO.File.ReadAllText("bgsFaction.json");
            JObject response = JsonConvert.DeserializeObject<JObject>(jsonString);

            PrivateType bgsService = new PrivateType(typeof(BgsService));
            Faction faction = (Faction)bgsService.InvokeStatic("ParseFaction", new object[] { response });

            Assert.IsNotNull(faction);

            // Test The Dark Wheel core data
            Assert.AreEqual(41917, faction.EDDBID);
            Assert.AreEqual("Independent", faction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", faction.Government.invariantName);
            Assert.IsNull(faction.isplayer);
            Assert.AreEqual("2019-04-13T03:37:17.000Z", faction.updatedAt.ToString(dateTimeStringFormat));

            // Test The Dark Wheel faction presence data
            string systemName = "Shinrarta Dezhra";
            FactionPresence factionPresence = faction.presences.FirstOrDefault(p => p.systemName == systemName);

            Assert.AreEqual(28.9M, factionPresence?.influence);
            Assert.AreEqual("Boom", factionPresence?.FactionState?.invariantName);
            Assert.AreEqual("Happy", factionPresence?.Happiness.invariantName);
            Assert.AreEqual(1, factionPresence.ActiveStates.Count());
            Assert.AreEqual("Boom", factionPresence.ActiveStates[0].invariantName);
            Assert.AreEqual(0, factionPresence.PendingStates.Count());
            Assert.AreEqual(0, factionPresence.RecoveringStates.Count());
            Assert.AreEqual("2019-04-13T03:37:17.000Z", factionPresence.updatedAt.ToString(dateTimeStringFormat));

            systemName = "LFT 926";
            factionPresence = faction.presences.FirstOrDefault(p => p.systemName == systemName);

            Assert.AreEqual(11.2983M, factionPresence?.influence);
            Assert.AreEqual("Boom", factionPresence?.FactionState?.invariantName);
            Assert.AreEqual("Happy", factionPresence?.Happiness.invariantName);
            Assert.AreEqual(0, factionPresence.ActiveStates.Count());
            Assert.AreEqual(0, factionPresence.PendingStates.Count());
            Assert.AreEqual(1, factionPresence.RecoveringStates.Count());
            Assert.AreEqual("War", factionPresence.RecoveringStates[0].factionState.invariantName);
            Assert.AreEqual(0, factionPresence.RecoveringStates[0].trend);
            Assert.AreEqual("2019-04-13T03:27:28.000Z", factionPresence.updatedAt.ToString(dateTimeStringFormat));
        }
    }
}