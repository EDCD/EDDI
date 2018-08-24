using System;
using System.Collections.Generic;
using Eddi;
using EddiCargoMonitor;
using EddiMissionMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rollbar;

namespace UnitTests
{
    [TestClass]
    public class MissionMonitorTests
    {
        MissionMonitor missionMonitor = new MissionMonitor();
        Mission mission;
        string line;
        List<Event> events;

        [TestInitialize]
        private void StartTestMissionMonitor()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;

            // Set ourselves as in beta to stop sending data to remote systems
            EDDI.Instance.eventHandler(new FileHeaderEvent(DateTime.Now, "JournalBeta.txt", "beta", "beta"));
        }

        [TestCleanup]
        private void StopTestMissionMonitor()
        {
        }
    }
}
