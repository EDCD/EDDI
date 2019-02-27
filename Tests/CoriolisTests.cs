using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Rollbar;
using Eddi;
using EddiEvents;

namespace UnitTests
{
    [TestClass]
    public class CoriolisTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;

            // Set ourselves as in beta to stop sending data to remote systems
            EDDI.Instance.enqueueEvent(new FileHeaderEvent(DateTime.Now, "JournalBeta.txt", "beta", "beta"));

            // Don't write to permanent storage
            Utilities.Files.unitTesting = true;
        }

        [TestMethod]
        public void TestUri()
        {
            string data = "BZ+24 123";
            string uriData = Uri.EscapeDataString(data);
            Assert.AreEqual("BZ%2B24%20123", uriData);
        }
    }
}
