﻿using EddiDataDefinitions;
using EddiStatusMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rollbar;
using System;

namespace UnitTests
{
    [TestClass]
    public class StatusMonitorTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        [TestMethod]
        public void TestParseStatusFlagsDocked()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":0, \"GuiFocus\":0 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2018, 3, 25, 0, 39, 48, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual(status.flags, (Status.Flags)16842765);
            Assert.AreEqual(status.vehicle, "Ship");
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsFalse(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual(status.fsd_status, "masslock");
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsTrue(status.shields_up);
            Assert.IsTrue(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsTrue(status.docked);
        }

        [TestMethod]
        public void TestParseStatusFlagsNormalSpace()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16777320, \"Pips\":[7,1,4], \"FireGroup\":0, \"GuiFocus\":0 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            Assert.AreEqual(status.flags, (Status.Flags)16777320);
            Assert.AreEqual(status.vehicle, "Ship");
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsFalse(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual(status.fsd_status, "ready");
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsTrue(status.hardpoints_deployed);
            Assert.IsTrue(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsTrue(status.shields_up);
            Assert.IsFalse(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsFalse(status.docked);
        }

        [TestMethod]
        public void TestParseStatusFlagsSrv()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":69255432, \"Pips\":[2,8,2], \"FireGroup\":0, \"GuiFocus\":0, \"Latitude\":-5.683115, \"Longitude\":-10.957623, \"Heading\":249, \"Altitude\":0}";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            Assert.AreEqual(status.flags, (Status.Flags)69255432);
            Assert.AreEqual(status.vehicle, "SRV");
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsTrue(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual(status.fsd_status, "ready");
            Assert.IsTrue(status.srv_drive_assist);
            Assert.IsTrue(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsTrue(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsTrue(status.shields_up);
            Assert.IsFalse(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsFalse(status.docked);
        }

        [TestMethod]
        public void TestParseStatusFlagsSupercruise()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16777240, \"Pips\":[7,1,4], \"FireGroup\":0, \"GuiFocus\":0 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            Assert.AreEqual(status.flags, (Status.Flags)16777240);
            Assert.AreEqual(status.vehicle, "Ship");
            Assert.IsTrue(status.supercruise);
        }

        [TestMethod]
        public void TestParseStatusPips()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":0, \"GuiFocus\":0 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            Assert.AreEqual(status.pips_sys, (decimal)2.5);
            Assert.AreEqual(status.pips_eng, (decimal)1);
            Assert.AreEqual(status.pips_wea, (decimal)2.5);
        }

        [TestMethod]
        public void TestParseStatusFiregroup()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":1, \"GuiFocus\":0 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            Assert.AreEqual(status.firegroup, 1);
        }

        [TestMethod]
        public void TestParseStatusGuiFocus()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":1, \"GuiFocus\":5 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            Assert.AreEqual(status.gui_focus, "station services");
        }

        [TestMethod]
        public void TestParseStatusGps1()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":1, \"GuiFocus\":0 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            Assert.IsNull(status.latitude);
            Assert.IsNull(status.longitude);
            Assert.IsNull(status.altitude);
            Assert.IsNull(status.heading);
        }

        [TestMethod]
        public void TestParseStatusGps2()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":69255432, \"Pips\":[2,8,2], \"FireGroup\":0, \"GuiFocus\":0, \"Latitude\":-5.683115, \"Longitude\":-10.957623, \"Heading\":249, \"Altitude\":0}";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            Assert.AreEqual(status.latitude, (decimal)-5.683115);
            Assert.AreEqual(status.longitude, (decimal)-10.957623);
            Assert.AreEqual(status.altitude, (decimal)0);
            Assert.AreEqual(status.heading, (decimal)249);
        }

        [TestMethod]
        public void TestParseStatusFlagsAnalysisFssMode()
        {
            string line = "{ \"timestamp\":\"2018 - 11 - 15T04: 41:06Z\", \"event\":\"Status\", \"Flags\":151519320, \"Pips\":[4,4,4], \"FireGroup\":2, \"GuiFocus\":9, \"Fuel\":{ \"FuelMain\":15.260000, \"FuelReservoir\":0.444812 }, \"Cargo\":39.000000 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            Assert.AreEqual(true, status.analysis_mode);
            Assert.AreEqual("fss mode", status.gui_focus);
        }

        [TestMethod]
        public void TestParseStatusFlagsAnalysisSaaMode()
        {
            string line = "{ \"timestamp\":\"2018 - 11 - 15T04: 47:51Z\", \"event\":\"Status\", \"Flags\":150995032, \"Pips\":[4,4,4], \"FireGroup\":2, \"GuiFocus\":10, \"Fuel\":{ \"FuelMain\":15.260000, \"FuelReservoir\":0.444812 }, \"Cargo\":39.000000 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            Assert.AreEqual(true, status.analysis_mode);
            Assert.AreEqual("saa mode", status.gui_focus);
        }
        
        [TestMethod]
        public void TestParseStatusFlagsNightMode()
        {
            string line = "{ \"timestamp\":\"2018 - 11 - 15T04: 58:37Z\", \"event\":\"Status\", \"Flags\":422117640, \"Pips\":[4,4,4], \"FireGroup\":2, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":29.0, \"FuelReservoir\":0.564209 }, \"Cargo\":39.000000, \"Latitude\":88.365417, \"Longitude\":99.356514, \"Heading\":29, \"Altitude\":36 }";
            Status status = StatusMonitor.Instance.ParseStatusEntry(line);

            Assert.AreEqual(true, status.night_vision);
            Assert.AreEqual(true, status.lights_on);
            Assert.AreEqual("none", status.gui_focus);
            Assert.AreEqual(29.564209M, status.fuel);
            Assert.AreEqual(39, status.cargo_carried);
        }
    }
}
