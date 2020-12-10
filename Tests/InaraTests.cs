using EddiInaraService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Tests.Properties;
using Utilities;

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
            PrivateObject privateInaraService = new PrivateObject(typeof(InaraService));
            var results = (List<InaraAPIEvent>)privateInaraService.Invoke("IndexAndFilterAPIEvents", new object[] { inaraAPIEvents, InaraConfiguration.FromFile() });

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
            try
            {
                var expectedCmdrs = new List<InaraCmdr>()
                {
                    new InaraCmdr()
                    {
                        id = 1,
                        username = "Artie",
                        commandername = "Artie",
                        commanderranks = new List<InaraCmdrRanks>()
                        {
                            new InaraCmdrRanks() { rank = "combat", rankvalue = 7, progress = 0.31 },
                            new InaraCmdrRanks() { rank = "trade", rankvalue = 8, progress = 1.0 },
                            new InaraCmdrRanks() { rank = "exploration", rankvalue = 6, progress = 0.65 },
                            new InaraCmdrRanks() { rank = "cqc", rankvalue = 3, progress = 0.11 },
                            new InaraCmdrRanks() { rank = "empire", rankvalue = 12, progress = 0.34 },
                            new InaraCmdrRanks() { rank = "federation", rankvalue = 12, progress = 0.94 }
                        },
                        preferredallegiance = "Independent",
                        preferredpower = null,
                        squadron = new InaraCmdrSquadron()
                        {
                            id = 5,
                            name = "Inara Dojo",
                            memberscount = 9,
                            squadronrank = "Squadron commander",
                            url = "https://inara.cz/squadron/5/"
                        },
                        preferredrole = "Freelancer",
                        imageurl = "https://inara.cz/data/users/0/1x1673.jpg",
                        url = "https://inara.cz/cmdr/1/"
                    }
                };

                var inaraCmdrs = DeserializeJsonResource<List<InaraCmdr>>(Resources.inaraCmdrs);
                Assert.IsTrue(expectedCmdrs.DeepEquals(inaraCmdrs));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
