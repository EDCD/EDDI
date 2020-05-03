using EddiInaraService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using UnitTests;

// Note: Tests work best if the tester has a valid API key configured for Inara.

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

        [TestMethod]
        public void TestGetCmdrProfiles()
        {
            List<InaraCmdr> inaraCmdrs = inaraService.GetCommanderProfiles(new string[] { "No such name", "Artie" });
            Assert.AreEqual(1, inaraCmdrs?.Count);

            if (inaraCmdrs?.Count == 1)
            {
                Assert.AreEqual("Artie", inaraCmdrs[0]?.username);
                Assert.AreEqual(1, inaraCmdrs[0]?.id);
                Assert.AreEqual("Artie", inaraCmdrs[0]?.commandername);
                Assert.AreEqual(@"https://inara.cz/cmdr/1/", inaraCmdrs[0]?.url);
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
    }
}
