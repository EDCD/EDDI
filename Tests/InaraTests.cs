using EddiInaraService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Tests.Properties;
using UnitTests;

namespace IntegrationTests
{
    [TestClass]
    public class InaraTests : TestBase
    {
        private IInaraService inaraService;

        [TestInitialize]
        public void start()
        {
            MakeSafe();
            inaraService = new InaraService();
        }

        [TestMethod]
        public void TestSendEventBatch()
        {
            // This integration test will work best using a valid API key. While it can work with the read-only API key, if the read-only API has been over-utilized then the test can fail due to too many requests.
            List<InaraAPIEvent> inaraAPIEvents = new List<InaraAPIEvent>()
            {
                { new InaraAPIEvent(DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>() { { "searchName", "No such name" } })},
                { new InaraAPIEvent(DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>() { { "searchName", "Artie" } })}
            };
            List<InaraResponse> responses = inaraService.SendEventBatch(inaraAPIEvents, InaraConfiguration.FromFile(), true);

            // Check that only valid responses were returned
            if (responses.Count == 1)
            {
                Assert.AreEqual(200, responses[0].eventStatus);
                Assert.AreEqual(1, responses[0].eventCustomID);
                Assert.AreEqual(@"https://inara.cz/cmdr/1/", ((Dictionary<string, object>)responses[0].eventData)["inaraURL"]);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}

namespace UnitTests
{
    [TestClass]
    public class InaraTests : TestBase
    {
        private IInaraService inaraService;

        [TestInitialize]
        public void start()
        {
            MakeSafe();
            inaraService = new InaraService();
        }

        [TestMethod]
        public void TestIndexAndFilterAPIEvents()
        {
            List<InaraAPIEvent> inaraAPIEvents = new List<InaraAPIEvent>()
            {
                { new InaraAPIEvent(DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>() { { "searchName", "No such name" } })},
                { new InaraAPIEvent(DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>() { { "searchName", "Artie" } })}
            };
            PrivateType privateInaraService = new PrivateType(typeof(InaraService));
            var results = (List<InaraAPIEvent>)privateInaraService.InvokeStatic("IndexAndFilterAPIEvents", new object[] { inaraAPIEvents, InaraConfiguration.FromFile() });

            if (results.Count == 2)
            {
                // Check that appropriate response IDs were assigned to each API event
                Assert.AreEqual(0, results[0].eventCustomID);
                Assert.AreEqual(1, results[1].eventCustomID);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestInvalidAPIkey()
        {
            InaraService.invalidAPIkey += OnInvalidAPIkey;
            InaraConfiguration inaraConfiguration = new InaraConfiguration() { apiKey = "invalidAPIkey!@#", isAPIkeyValid = false };
            PrivateObject privateInaraService = new PrivateObject(inaraService);
            privateInaraService.Invoke("checkAPIcredentialsOk", new object[] { inaraConfiguration });
            System.Threading.Thread.Sleep(50);
            Assert.IsTrue(invalidAPIkeyTestPassed);
        }
        private bool invalidAPIkeyTestPassed;
        private void OnInvalidAPIkey(object sender, EventArgs e) 
        {
            invalidAPIkeyTestPassed = true;
        }

        [TestMethod]
        public void TestCmdrProfiles()
        {
            List<InaraCmdr> inaraCmdrs = DeserializeJsonResource<List<InaraCmdr>>(Resources.inaraCmdrs);
            Assert.AreEqual(1, inaraCmdrs?.Count);
            Assert.AreEqual("Artie", inaraCmdrs[0]?.username);
            Assert.AreEqual(1, inaraCmdrs[0]?.id);
            Assert.AreEqual("Artie", inaraCmdrs[0]?.commandername);
            Assert.AreEqual(@"https://inara.cz/cmdr/1/", inaraCmdrs[0]?.url);
            Assert.AreEqual("Independent", inaraCmdrs[0]?.preferredallegiance);
            Assert.IsNull(inaraCmdrs[0]?.preferredpower);
            Assert.AreEqual("Freelancer", inaraCmdrs[0]?.preferredrole);
            Assert.AreEqual("https://inara.cz/data/users/0/1x1673.jpg", inaraCmdrs[0]?.imageurl);
            Assert.AreEqual("https://inara.cz/cmdr/1/", inaraCmdrs[0]?.url);
            Assert.AreEqual(6, inaraCmdrs[0]?.commanderranks.Count);
            Assert.AreEqual("trade", inaraCmdrs[0]?.commanderranks[1]?.rank);
            Assert.AreEqual(8, inaraCmdrs[0]?.commanderranks[1]?.rankvalue);
            Assert.AreEqual(1.0, inaraCmdrs[0]?.commanderranks[1]?.progress);
            Assert.AreEqual("cqc", inaraCmdrs[0]?.commanderranks[3]?.rank);
            Assert.AreEqual(3, inaraCmdrs[0]?.commanderranks[3]?.rankvalue);
            Assert.AreEqual(0.11, inaraCmdrs[0]?.commanderranks[3]?.progress);
            Assert.AreEqual(5, inaraCmdrs[0]?.squadron?.id);
            Assert.AreEqual("Inara Dojo", inaraCmdrs[0]?.squadron?.name);
            Assert.AreEqual(9, inaraCmdrs[0]?.squadron?.memberscount);
            Assert.AreEqual("Squadron commander", inaraCmdrs[0]?.squadron?.squadronrank);
            Assert.AreEqual("https://inara.cz/squadron/5/", inaraCmdrs[0]?.squadron?.url);
        }
    }
}
