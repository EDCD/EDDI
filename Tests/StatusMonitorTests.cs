using EddiCore;
using EddiDataDefinitions;
using EddiStatusMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class StatusMonitorTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestParseStatusFlagsDocked()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":0, \"GuiFocus\":0 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

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
        public void TestParseStatusFlagsDockedOdyssey()
        {
            string line = "{ \"timestamp\":\"2021-05-01T21:04:13Z\", \"event\":\"Status\", \"Flags\":151060493, \"Flags2\":0, \"Pips\":[4,8,0], \"FireGroup\":0, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":32.000000, \"FuelReservoir\":0.630000 }, \"Cargo\":0.000000, \"LegalState\":\"Clean\" }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2021, 5, 1, 21, 4, 13, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual((Status.Flags)151060493, status.flags);
            Assert.AreEqual((Status.Flags2)0, status.flags2);
            Assert.AreEqual(Constants.VEHICLE_SHIP, status.vehicle);
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsFalse(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual("masslock", status.fsd_status);
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.srv_high_beams);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsFalse(status.hyperspace);
            Assert.IsTrue(status.shields_up);
            Assert.IsTrue(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsTrue(status.docked);
            Assert.IsTrue(status.analysis_mode);
            Assert.IsFalse(status.night_vision);
            Assert.IsFalse(status.altitude_from_average_radius);
            Assert.IsFalse(status.on_foot_in_station);
            Assert.IsFalse(status.on_foot_on_planet);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsFalse(status.low_oxygen);
            Assert.IsFalse(status.low_health);
            Assert.AreEqual("temperate", status.on_foot_temperature);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.AreEqual(2M, status.pips_sys);
            Assert.AreEqual(4M, status.pips_eng);
            Assert.AreEqual(0M, status.pips_wea);
            Assert.AreEqual(0, status.firegroup);
            Assert.AreEqual("none", status.gui_focus);
            Assert.AreEqual(32M, status.fuelInTanks);
            Assert.AreEqual(0.63M, status.fuelInReservoir);
            Assert.AreEqual(0, status.cargo_carried);
            Assert.AreEqual("Clean", status.legalstatus);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsNull(status.latitude);
            Assert.IsNull(status.longitude);
            Assert.IsNull(status.heading);
            Assert.IsNull(status.altitude);
            Assert.IsNull(status.bodyname);
            Assert.IsNull(status.planetradius);
        }

        [TestMethod]
        public void TestParseStatusFlagsDockedDropshipOdyssey()
        {
            string line = "{ \"timestamp\":\"2021-05-01T23:10:06Z\", \"event\":\"Status\", \"Flags\":16842761, \"Flags2\":2, \"Pips\":[4,4,4], \"FireGroup\":0, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":8.000000, \"FuelReservoir\":0.570000 }, \"Cargo\":0.000000, \"LegalState\":\"Clean\" }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2021, 5, 1, 23, 10, 06, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual((Status.Flags)16842761, status.flags);
            Assert.AreEqual((Status.Flags2)2, status.flags2);
            Assert.AreEqual(Constants.VEHICLE_TAXI, status.vehicle);
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsFalse(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual("masslock", status.fsd_status);
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.srv_high_beams);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsFalse(status.hyperspace);
            Assert.IsTrue(status.shields_up);
            Assert.IsFalse(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsTrue(status.docked);
            Assert.IsFalse(status.analysis_mode);
            Assert.IsFalse(status.night_vision);
            Assert.IsFalse(status.altitude_from_average_radius);
            Assert.IsFalse(status.on_foot_in_station);
            Assert.IsFalse(status.on_foot_on_planet);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsFalse(status.low_oxygen);
            Assert.IsFalse(status.low_health);
            Assert.AreEqual("temperate", status.on_foot_temperature);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.AreEqual(2M, status.pips_sys);
            Assert.AreEqual(2M, status.pips_eng);
            Assert.AreEqual(2M, status.pips_wea);
            Assert.AreEqual(0, status.firegroup);
            Assert.AreEqual("none", status.gui_focus);
            Assert.AreEqual(8M, status.fuelInTanks);
            Assert.AreEqual(0.57M, status.fuelInReservoir);
            Assert.AreEqual(0, status.cargo_carried);
            Assert.AreEqual("Clean", status.legalstatus);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsNull(status.latitude);
            Assert.IsNull(status.longitude);
            Assert.IsNull(status.heading);
            Assert.IsNull(status.altitude);
            Assert.IsNull(status.bodyname);
            Assert.IsNull(status.planetradius);
        }

        [TestMethod]
        public void TestParseStatusFlagsSupercruiseTaxiOdyssey()
        {
            string line = "{ \"timestamp\":\"2021-05-01T22:30:27Z\", \"event\":\"Status\", \"Flags\":16777240, \"Flags2\":2, \"Pips\":[4,4,4], \"FireGroup\":0, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":8.000000, \"FuelReservoir\":0.360000 }, \"Cargo\":0.000000, \"LegalState\":\"Clean\" }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2021, 5, 1, 22, 30, 27, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual((Status.Flags)16777240, status.flags);
            Assert.AreEqual((Status.Flags2)2, status.flags2);
            Assert.AreEqual(Constants.VEHICLE_TAXI, status.vehicle);
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsFalse(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual("ready", status.fsd_status);
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.srv_high_beams);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsTrue(status.supercruise);
            Assert.IsFalse(status.hyperspace);
            Assert.IsTrue(status.shields_up);
            Assert.IsFalse(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsFalse(status.docked);
            Assert.IsFalse(status.analysis_mode);
            Assert.IsFalse(status.night_vision);
            Assert.IsFalse(status.altitude_from_average_radius);
            Assert.IsFalse(status.on_foot_in_station);
            Assert.IsFalse(status.on_foot_on_planet);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsFalse(status.low_oxygen);
            Assert.IsFalse(status.low_health);
            Assert.AreEqual("temperate", status.on_foot_temperature);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.AreEqual(2M, status.pips_sys);
            Assert.AreEqual(2M, status.pips_eng);
            Assert.AreEqual(2M, status.pips_wea);
            Assert.AreEqual(0, status.firegroup);
            Assert.AreEqual("none", status.gui_focus);
            Assert.AreEqual(8M, status.fuelInTanks);
            Assert.AreEqual(0.36M, status.fuelInReservoir);
            Assert.AreEqual(0, status.cargo_carried);
            Assert.AreEqual("Clean", status.legalstatus);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsNull(status.latitude);
            Assert.IsNull(status.longitude);
            Assert.IsNull(status.heading);
            Assert.IsNull(status.altitude);
            Assert.IsNull(status.bodyname);
            Assert.IsNull(status.planetradius);
        }

        [TestMethod]
        public void TestParseStatusFlagsInFighterOdyssey()
        {
            string line = "{ \"timestamp\":\"2021-05-01T21:15:30Z\", \"event\":\"Status\", \"Flags\":34078792, \"Flags2\":0, \"Pips\":[2,8,2], \"FireGroup\":0, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":0.000000, \"FuelReservoir\":0.240000 }, \"Cargo\":0.000000, \"LegalState\":\"Clean\" }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2021, 5, 1, 21, 15, 30, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual((Status.Flags)34078792, status.flags);
            Assert.AreEqual((Status.Flags2)0, status.flags2);
            Assert.AreEqual(Constants.VEHICLE_FIGHTER, status.vehicle);
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsFalse(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsTrue(status.low_fuel); // Always true in a fighter since the fighter has no main fuel tank
            Assert.AreEqual("ready", status.fsd_status);
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.srv_high_beams);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsTrue(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsFalse(status.hyperspace);
            Assert.IsTrue(status.shields_up);
            Assert.IsFalse(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsFalse(status.docked);
            Assert.IsFalse(status.analysis_mode);
            Assert.IsFalse(status.night_vision);
            Assert.IsFalse(status.altitude_from_average_radius);
            Assert.IsFalse(status.on_foot_in_station);
            Assert.IsFalse(status.on_foot_on_planet);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsFalse(status.low_oxygen);
            Assert.IsFalse(status.low_health);
            Assert.AreEqual("temperate", status.on_foot_temperature);
            Assert.IsTrue(status.hardpoints_deployed);
            Assert.AreEqual(1M, status.pips_sys);
            Assert.AreEqual(4M, status.pips_eng);
            Assert.AreEqual(1M, status.pips_wea);
            Assert.AreEqual(0, status.firegroup);
            Assert.AreEqual("none", status.gui_focus);
            Assert.AreEqual(0M, status.fuelInTanks);
            Assert.AreEqual(0.24M, status.fuelInReservoir);
            Assert.AreEqual(0, status.cargo_carried);
            Assert.AreEqual("Clean", status.legalstatus);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsNull(status.latitude);
            Assert.IsNull(status.longitude);
            Assert.IsNull(status.heading);
            Assert.IsNull(status.altitude);
            Assert.IsNull(status.bodyname);
            Assert.IsNull(status.planetradius);
        }

        [TestMethod]
        public void TestParseStatusFlagsLandedInShipOdyssey()
        {
            string line = "{ \"timestamp\":\"2021-05-01T21:24:57Z\", \"event\":\"Status\", \"Flags\":153157646, \"Flags2\":0, \"Pips\":[4,8,0], \"FireGroup\":0, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":32.000000, \"FuelReservoir\":0.384769 }, \"Cargo\":0.000000, \"LegalState\":\"Clean\", \"Latitude\":40.761524, \"Longitude\":65.103111, \"Heading\":32, \"Altitude\":0, \"BodyName\":\"Nervi 2 a\", \"PlanetRadius\":866740.562500 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2021, 5, 1, 21, 24, 57, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual((Status.Flags)153157646, status.flags);
            Assert.AreEqual((Status.Flags2)0, status.flags2);
            Assert.AreEqual(Constants.VEHICLE_SHIP, status.vehicle);
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsTrue(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual("masslock", status.fsd_status);
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.srv_high_beams);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsFalse(status.hyperspace);
            Assert.IsTrue(status.shields_up);
            Assert.IsTrue(status.landing_gear_down);
            Assert.IsTrue(status.landed);
            Assert.IsFalse(status.docked);
            Assert.IsTrue(status.analysis_mode);
            Assert.IsFalse(status.night_vision);
            Assert.IsFalse(status.altitude_from_average_radius);
            Assert.IsFalse(status.on_foot_in_station);
            Assert.IsFalse(status.on_foot_on_planet);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsFalse(status.low_oxygen);
            Assert.IsFalse(status.low_health);
            Assert.AreEqual("temperate", status.on_foot_temperature);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.AreEqual(2M, status.pips_sys);
            Assert.AreEqual(4M, status.pips_eng);
            Assert.AreEqual(0M, status.pips_wea);
            Assert.AreEqual(0, status.firegroup);
            Assert.AreEqual("none", status.gui_focus);
            Assert.AreEqual(32M, status.fuelInTanks);
            Assert.AreEqual(0.384769M, status.fuelInReservoir);
            Assert.AreEqual(0, status.cargo_carried);
            Assert.AreEqual("Clean", status.legalstatus);
            Assert.IsFalse(status.aim_down_sight);
            Assert.AreEqual(40.761524M, status.latitude);
            Assert.AreEqual(65.103111M, status.longitude);
            Assert.AreEqual(32M, status.heading);
            Assert.AreEqual(0M, status.altitude);
            Assert.AreEqual("Nervi 2 a", status.bodyname);
            Assert.AreEqual(866740.562500M, status.planetradius);
        }

        [TestMethod]
        public void TestParseStatusFlagsInSRVOdyssey()
        {
            string line = "{ \"timestamp\":\"2021-05-01T21:26:19Z\", \"event\":\"Status\", \"Flags\":203423752, \"Flags2\":0, \"Pips\":[7,4,1], \"FireGroup\":0, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":0.000000, \"FuelReservoir\":0.447595 }, \"Cargo\":0.000000, \"LegalState\":\"Clean\", \"Latitude\":40.745747, \"Longitude\":65.096542, \"Heading\":159, \"Altitude\":0, \"BodyName\":\"Nervi 2 a\", \"PlanetRadius\":866740.562500 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2021, 5, 1, 21, 26, 19, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual((Status.Flags)203423752, status.flags);
            Assert.AreEqual((Status.Flags2)0, status.flags2);
            Assert.AreEqual(Constants.VEHICLE_SRV, status.vehicle);
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsTrue(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual("ready", status.fsd_status);
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.srv_high_beams);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsFalse(status.hyperspace);
            Assert.IsTrue(status.shields_up);
            Assert.IsFalse(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsFalse(status.docked);
            Assert.IsTrue(status.analysis_mode);
            Assert.IsFalse(status.night_vision);
            Assert.IsFalse(status.altitude_from_average_radius);
            Assert.IsFalse(status.on_foot_in_station);
            Assert.IsFalse(status.on_foot_on_planet);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsFalse(status.low_oxygen);
            Assert.IsFalse(status.low_health);
            Assert.AreEqual("temperate", status.on_foot_temperature);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.AreEqual(3.5M, status.pips_sys);
            Assert.AreEqual(2M, status.pips_eng);
            Assert.AreEqual(0.5M, status.pips_wea);
            Assert.AreEqual(0, status.firegroup);
            Assert.AreEqual("none", status.gui_focus);
            Assert.AreEqual(0M, status.fuelInTanks);
            Assert.AreEqual(0.447595M, status.fuelInReservoir);
            Assert.AreEqual(0, status.cargo_carried);
            Assert.AreEqual("Clean", status.legalstatus);
            Assert.IsFalse(status.aim_down_sight);
            Assert.AreEqual(40.745747M, status.latitude);
            Assert.AreEqual(65.096542M, status.longitude);
            Assert.AreEqual(159M, status.heading);
            Assert.AreEqual(0M, status.altitude);
            Assert.AreEqual("Nervi 2 a", status.bodyname);
            Assert.AreEqual(866740.562500M, status.planetradius);
        }

        [TestMethod]
        public void TestParseStatusFlagsOnFootOnPlanetOdyssey()
        {
            string line = "{ \"timestamp\":\"2021-05-01T21:30:38Z\", \"event\":\"Status\", \"Flags\":2097152, \"Flags2\":273, \"Oxygen\":1.000000, \"Health\":1.000000, \"Temperature\":163.527100, \"SelectedWeapon\":\"$humanoid_fists_name;\", \"SelectedWeapon_Localised\":\"Unarmed\", \"Gravity\":0.101595, \"LegalState\":\"Clean\", \"Latitude\":40.741016, \"Longitude\":65.076881, \"Heading\":-165, \"BodyName\":\"Nervi 2 a\" }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2021, 5, 1, 21, 30, 38, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual((Status.Flags)2097152, status.flags);
            Assert.AreEqual((Status.Flags2)273, status.flags2);
            Assert.AreEqual(Constants.VEHICLE_LEGS, status.vehicle);
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsTrue(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual("ready", status.fsd_status);
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.srv_high_beams);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsFalse(status.hyperspace);
            Assert.IsFalse(status.shields_up);
            Assert.IsFalse(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsFalse(status.docked);
            Assert.IsFalse(status.analysis_mode);
            Assert.IsFalse(status.night_vision);
            Assert.IsFalse(status.altitude_from_average_radius);
            Assert.IsFalse(status.on_foot_in_station);
            Assert.IsTrue(status.on_foot_on_planet);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsFalse(status.low_oxygen);
            Assert.IsFalse(status.low_health);
            Assert.AreEqual("cold", status.on_foot_temperature);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsNull(status.pips_sys);
            Assert.IsNull(status.pips_eng);
            Assert.IsNull(status.pips_wea);
            Assert.IsNull(status.firegroup);
            Assert.AreEqual("none", status.gui_focus);
            Assert.IsNull(status.fuelInTanks);
            Assert.IsNull(status.fuelInReservoir);
            Assert.IsNull(status.cargo_carried);
            Assert.AreEqual("Clean", status.legalstatus);
            Assert.IsFalse(status.aim_down_sight);
            Assert.AreEqual(40.741016M, status.latitude);
            Assert.AreEqual(65.076881M, status.longitude);
            Assert.AreEqual(-165M, status.heading);
            Assert.IsNull(status.altitude);
            Assert.AreEqual("Nervi 2 a", status.bodyname);
            Assert.IsNull(status.planetradius);
            Assert.AreEqual(100M, status.oxygen);
            Assert.AreEqual(100M, status.health);
            Assert.AreEqual(163.527100M, status.temperature);
            Assert.AreEqual("Unarmed", status.selected_weapon);
            Assert.AreEqual(0.101595M, status.gravity);
        }

        [TestMethod]
        public void TestParseStatusFlagsOnFootOnStationOdyssey()
        {
            string line = "{ \"timestamp\":\"2021-05-01T21:00:10Z\", \"event\":\"Status\", \"Flags\":0, \"Flags2\":9, \"Oxygen\":1.000000, \"Health\":1.000000, \"Temperature\":293.000000, \"SelectedWeapon\":\"\", \"LegalState\":\"Clean\", \"BodyName\":\"Savitskaya Vision\" }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            DateTime expectedTimestamp = new DateTime(2021, 5, 1, 21, 00, 10, DateTimeKind.Utc);
            Assert.AreEqual(expectedTimestamp, status.timestamp);
            Assert.AreEqual((Status.Flags)0, status.flags);
            Assert.AreEqual((Status.Flags2)9, status.flags2);
            Assert.AreEqual(Constants.VEHICLE_LEGS, status.vehicle);
            Assert.IsFalse(status.being_interdicted);
            Assert.IsFalse(status.in_danger);
            Assert.IsFalse(status.near_surface);
            Assert.IsFalse(status.overheating);
            Assert.IsFalse(status.low_fuel);
            Assert.AreEqual("ready", status.fsd_status);
            Assert.IsFalse(status.srv_drive_assist);
            Assert.IsFalse(status.srv_under_ship);
            Assert.IsFalse(status.srv_turret_deployed);
            Assert.IsFalse(status.srv_handbrake_activated);
            Assert.IsFalse(status.srv_high_beams);
            Assert.IsFalse(status.scooping_fuel);
            Assert.IsFalse(status.silent_running);
            Assert.IsFalse(status.cargo_scoop_deployed);
            Assert.IsFalse(status.lights_on);
            Assert.IsFalse(status.in_wing);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsFalse(status.flight_assist_off);
            Assert.IsFalse(status.supercruise);
            Assert.IsFalse(status.hyperspace);
            Assert.IsFalse(status.shields_up);
            Assert.IsFalse(status.landing_gear_down);
            Assert.IsFalse(status.landed);
            Assert.IsFalse(status.docked);
            Assert.IsFalse(status.analysis_mode);
            Assert.IsFalse(status.night_vision);
            Assert.IsFalse(status.altitude_from_average_radius);
            Assert.IsTrue(status.on_foot_in_station);
            Assert.IsFalse(status.on_foot_on_planet);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsFalse(status.low_oxygen);
            Assert.IsFalse(status.low_health);
            Assert.AreEqual("temperate", status.on_foot_temperature);
            Assert.IsFalse(status.hardpoints_deployed);
            Assert.IsNull(status.pips_sys);
            Assert.IsNull(status.pips_eng);
            Assert.IsNull(status.pips_wea);
            Assert.IsNull(status.firegroup);
            Assert.AreEqual("none", status.gui_focus);
            Assert.IsNull(status.fuelInTanks);
            Assert.IsNull(status.fuelInReservoir);
            Assert.IsNull(status.cargo_carried);
            Assert.AreEqual("Clean", status.legalstatus);
            Assert.IsFalse(status.aim_down_sight);
            Assert.IsNull(status.latitude);
            Assert.IsNull(status.longitude);
            Assert.IsNull(status.heading);
            Assert.IsNull(status.altitude);
            Assert.AreEqual("Savitskaya Vision", status.bodyname);
            Assert.IsNull(status.planetradius);
            Assert.AreEqual(100M, status.oxygen);
            Assert.AreEqual(100M, status.health);
            Assert.AreEqual(293M, status.temperature);
            Assert.AreEqual("", status.selected_weapon);
            Assert.IsNull(status.gravity);
        }

        [TestMethod]
        public void TestParseStatusFlagsNormalSpace()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16777320, \"Pips\":[7,1,4], \"FireGroup\":0, \"GuiFocus\":0 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

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
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":69255432, \"Pips\":[2,8,2], \"FireGroup\":0, \"GuiFocus\":0, \"Latitude\":-5.683115, \"Longitude\":-10.957623, \"Heading\":249, \"Altitude\":0, \"BodyName\":\"Myeia Thaa QI - B d13 - 1 1\", \"PlanetRadius\":2140275.000000}";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

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
            Assert.AreEqual("Myeia Thaa QI - B d13 - 1 1", status.bodyname);
            Assert.AreEqual(2140275.000000M, status.planetradius);
        }

        [TestMethod]
        public void TestParseStatusFlagsSupercruise()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16777240, \"Pips\":[7,1,4], \"FireGroup\":0, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":26.589718, \"FuelReservoir\":0.484983 }, \"Cargo\":3.000000, \"LegalState\":\"Clean\" }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            // Variables set from status flags (when not signed in, flags are set to '0')
            Assert.AreEqual((Status.Flags)16777240, status.flags);
            Assert.AreEqual("Ship", status.vehicle);
            Assert.AreEqual(26.589718M, status.fuelInTanks);
            Assert.AreEqual(0.484983M, status.fuelInReservoir);
            Assert.AreEqual(26.589718M + 0.484983M, status.fuel);
            Assert.AreEqual(3, status.cargo_carried);
            Assert.AreEqual(LegalStatus.Clean, status.legalStatus);
            Assert.IsTrue(status.supercruise);
        }

        [TestMethod]
        public void TestParseStatusPips()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":0, \"GuiFocus\":0 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            Assert.AreEqual(2.5M, status.pips_sys);
            Assert.AreEqual(1M, status.pips_eng);
            Assert.AreEqual(2.5M, status.pips_wea);
        }

        [TestMethod]
        public void TestParseStatusFiregroup()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":1, \"GuiFocus\":0 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            Assert.AreEqual(1, status.firegroup);
        }

        [TestMethod]
        public void TestParseStatusGuiFocus()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":1, \"GuiFocus\":5 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            Assert.AreEqual("station services", status.gui_focus);
        }

        [TestMethod]
        public void TestParseStatusGps1()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":16842765, \"Pips\":[5,2,5], \"FireGroup\":1, \"GuiFocus\":0 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            Assert.IsNull(status.latitude);
            Assert.IsNull(status.longitude);
            Assert.IsNull(status.altitude);
            Assert.IsNull(status.heading);
        }

        [TestMethod]
        public void TestParseStatusGps2()
        {
            string line = "{ \"timestamp\":\"2018-03-25T00:39:48Z\", \"event\":\"Status\", \"Flags\":69255432, \"Pips\":[2,8,2], \"FireGroup\":0, \"GuiFocus\":0, \"Latitude\":-5.683115, \"Longitude\":-10.957623, \"Heading\":249, \"Altitude\":0}";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            Assert.AreEqual(-5.683115M, status.latitude);
            Assert.AreEqual(-10.957623M, status.longitude);
            Assert.AreEqual(0M, status.altitude);
            Assert.AreEqual(249M, status.heading);
        }

        [TestMethod]
        public void TestParseStatusFlagsAnalysisFssMode()
        {
            string line = "{ \"timestamp\":\"2018 - 11 - 15T04: 41:06Z\", \"event\":\"Status\", \"Flags\":151519320, \"Pips\":[4,4,4], \"FireGroup\":2, \"GuiFocus\":9, \"Fuel\":{ \"FuelMain\":15.260000, \"FuelReservoir\":0.444812 }, \"Cargo\":39.000000 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            Assert.AreEqual(true, status.analysis_mode);
            Assert.AreEqual("fss mode", status.gui_focus);
        }

        [TestMethod]
        public void TestParseStatusFlagsAnalysisSaaMode()
        {
            string line = "{ \"timestamp\":\"2018 - 11 - 15T04: 47:51Z\", \"event\":\"Status\", \"Flags\":150995032, \"Pips\":[4,4,4], \"FireGroup\":2, \"GuiFocus\":10, \"Fuel\":{ \"FuelMain\":15.260000, \"FuelReservoir\":0.444812 }, \"Cargo\":39.000000 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            Assert.AreEqual(true, status.analysis_mode);
            Assert.AreEqual("saa mode", status.gui_focus);
        }

        [TestMethod]
        public void TestParseStatusFlagsNightMode()
        {
            string line = "{ \"timestamp\":\"2018 - 11 - 15T04: 58:37Z\", \"event\":\"Status\", \"Flags\":422117640, \"Pips\":[4,4,4], \"FireGroup\":2, \"GuiFocus\":0, \"Fuel\":{ \"FuelMain\":29.0, \"FuelReservoir\":0.564209 }, \"Cargo\":39.000000, \"Latitude\":88.365417, \"Longitude\":99.356514, \"Heading\":29, \"Altitude\":36 }";
            Status status = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).ParseStatusEntry(line);

            Assert.AreEqual(true, status.night_vision);
            Assert.AreEqual(true, status.lights_on);
            Assert.AreEqual("none", status.gui_focus);
            Assert.AreEqual(29.564209M, status.fuel);
            Assert.AreEqual(39, status.cargo_carried);
        }
    }
}
