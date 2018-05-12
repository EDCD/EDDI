using EddiDataDefinitions;
using EddiDataProviderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetMQ;
using NetMQ.Sockets;
using Rollbar;
using System;
using System.IO;
using System.IO.Compression;

namespace UnitTests
{
    [TestClass]
    public class EddnTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        class MockStarService : StarSystemRepository
        {
            public StarSystem GetOrCreateStarSystem(string name, bool refreshIfOutdated = true)
            {
                throw new NotImplementedException();
            }

            public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true)
            {
                throw new NotImplementedException();
            }

            public StarSystem GetStarSystem(string name, bool refreshIfOutdated = true)
            {
                switch(name)
                {
                    case "Artemis":
                        {
                            StarSystem result = new StarSystem();
                            result.name = "Artemis";
                            result.x = 14.28125M;
                            result.y = -63.1875M;
                            result.z = -24.875M;
                            return result;
                        }

                    default:
                        break;
                }
                return null;
            }

            public void LeaveStarSystem(StarSystem starSystem)
            {
                throw new NotImplementedException();
            }

            public void SaveStarSystem(StarSystem starSystem)
            {
                throw new NotImplementedException();
            }
        }

        private EDDNResponder.EDDNResponder makeTestEDDNResponder()
        {
            EDDNResponder.EDDNResponder responder = new EDDNResponder.EDDNResponder(new MockStarService());
            return responder;
        }

        [TestMethod()]
        public void TestEDDNResponderGoodMatch()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool matched = responder.eventSystemNameMatches("Sol");

            Assert.IsTrue(matched);
        }

        [TestMethod()]
        public void TestEDDNResponderBadInitialGoodFinal()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            // Intentionally place our EDDN responder in a state with no coordinates available.
            privateObject.SetFieldOrProperty("systemName", "Not in this galaxy");
            privateObject.SetFieldOrProperty("systemX", null);
            privateObject.SetFieldOrProperty("systemY", null);
            privateObject.SetFieldOrProperty("systemZ", null);

            bool matched = responder.eventSystemNameMatches("Artemis");

            Assert.IsTrue(matched);
            Assert.AreEqual("Artemis", responder.systemName);
            Assert.AreEqual(14.28125M, (decimal)responder.systemX);
            Assert.AreEqual(-63.1875M, (decimal)responder.systemY);
            Assert.AreEqual(-24.875M, (decimal)responder.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderGoodInitialBadFinal()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool matched = responder.eventSystemNameMatches("Not in this galaxy");

            Assert.IsFalse(matched);
            Assert.IsNull(responder.systemX);
            Assert.IsNull(responder.systemY);
            Assert.IsNull(responder.systemZ);
        }
    }
}

