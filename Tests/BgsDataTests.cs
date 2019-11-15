using EddiBgsService;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Tests.Properties;

namespace UnitTests
{
    // Tests for the EliteBGS Service
    internal class FakeBgsRestClient : IBgsRestClient
    {
        public Dictionary<string, string> CannedContent = new Dictionary<string, string>();
        public Dictionary<string, object> CannedData = new Dictionary<string, object>();

        public Uri BuildUri(IRestRequest request)
        {
            return new Uri("fakeBGS://" + request.Resource);
        }

        public IRestResponse<T> Execute<T>(IRestRequest request) where T : new()
        {
            // this will throw if given a resource not in the canned dictionaries: that's OK
            string content = CannedContent[request.Resource];
            T data = (T)CannedData[request.Resource];
            IRestResponse<T> restResponse = new RestResponse<T>
            {
                Content = content,
                Data = data,
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = HttpStatusCode.OK,
            };
            return restResponse;
        }

        public void Expect(string resource, string content, object data)
        {
            CannedContent[resource] = content;
            CannedData[resource] = data;
        }
    }

    [TestClass]
    public class BgsDataTests : TestBase
    {
        FakeBgsRestClient fakeBgsRestClient;
        BgsService fakeBgsService;

        [TestInitialize]
        public void start()
        {
            fakeBgsRestClient = new FakeBgsRestClient();
            fakeBgsService = new BgsService(fakeBgsRestClient);
            MakeSafe();
        }

        [TestMethod]
        public void TestFaction()
        {
            string dateTimeStringFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'";

            // Test faction data
            JObject response = DeserializeJsonResource<JObject>(Resources.bgsFaction);
            Faction faction = fakeBgsService.ParseFaction(response);

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

        [TestMethod]
        public void TestBgsFactionFromName()
        {
            // Setup
            string resource = "v4/factions?";
            string json = Encoding.UTF8.GetString(Resources.bgsFactionResponse);
            RestRequest data = new RestRequest();
            fakeBgsRestClient.Expect(resource, json, data);

            // Act
            Faction faction1 = fakeBgsService.GetFactionByName("The Dark Wheel");

            // Assert
            Assert.IsNotNull(faction1);
            Assert.AreEqual("The Dark Wheel", faction1.name);
            Assert.AreEqual("Democracy", faction1.Government.invariantName);
            Assert.AreEqual("Independent", faction1.Allegiance.invariantName);
            Assert.AreEqual(41917, faction1.EDDBID);
            Assert.AreNotEqual(DateTime.MinValue, faction1.updatedAt);

            // Even if the faction does not exist, we should return a basic object
            Faction faction2 = fakeBgsService.GetFactionByName("No such faction");
            Assert.IsNotNull(faction2);
            Assert.AreEqual("No such faction", faction2.name);
            Assert.AreEqual(Government.None, faction2.Government);
            Assert.AreEqual(Superpower.None, faction2.Allegiance);
            Assert.AreEqual(DateTime.MinValue, faction2.updatedAt);
        }
    }
}