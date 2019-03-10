using Eddi;
using EddiEvents;
using Rollbar;
using System;

namespace UnitTests
{
    public class TestBase
    {
        internal void MakeSafe()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;

            // Set ourselves as in beta to stop sending data to remote systems
            EDDI.Instance.enqueueEvent(new FileHeaderEvent(DateTime.Now, "JournalBeta.txt", "beta", "beta"));

            // Don't write to permanent storage
            Utilities.Files.unitTesting = true;
        }
    }
}
