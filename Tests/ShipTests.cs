using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using Utilities;
using Eddi;

namespace Tests
{
    [TestClass]
    public class ShipTests
    {
        [TestMethod]
        public void TestShipSpokenName1()
        {
            Ship ship = new Ship();
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your ship", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName2()
        {
            Ship ship = new Ship()
            {
                name = ""
            };
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your ship", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName3()
        {
            Ship ship = ShipDefinitions.FromModel("Anaconda");
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your Anaconda", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName4()
        {
            Ship ship = ShipDefinitions.FromModel("Anaconda");
            ship.name = "Testy";
            string spokenName = ship.SpokenName();
            Assert.AreEqual("Testy", spokenName);
        }

        [TestMethod]
        public void TestShipScenario1()
        {
            int sidewinderId = 901;
            int courierId = 902;

            Ship sidewinder;
            Ship courier;

            // Set ourselves as in beta to stop sending data to remote systems
            EDDI.Instance.eventHandler(new FileHeaderEvent(DateTime.Now, "JournalBeta.txt", "beta", "beta"));
            Logging.Verbose = true;

            // Start a ship monitor
            ShipMonitor shipMonitor = new ShipMonitor();

            // Log in
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:21Z"", ""event"":""LoadGame"", ""Commander"":""McDonald"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""FuelLevel"":2.000000, ""FuelCapacity"":2.000000, ""GameMode"":""Solo"", ""Credits"":1637243231, ""Loan"":0 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:24Z"", ""event"":""Location"", ""Docked"":true, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""StarPos"":[55.719,17.594,27.156], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""High Tech"", ""SystemGovernment"":""$government_Democracy;"", ""SystemGovernment_Localised"":""Democracy"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Body"":""Jameson Memorial"", ""BodyType"":""Station"", ""Factions"":[ { ""Name"":""Lori Jameson"", ""FactionState"":""None"", ""Government"":""Engineer"", ""Influence"":0.040307, ""Allegiance"":""Independent"" }, { ""Name"":""LTT 4487 Industry"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.191939, ""Allegiance"":""Federation"" }, { ""Name"":""The Pilots Federation"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.447217, ""Allegiance"":""Independent"" }, { ""Name"":""Future of Arro Naga"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.128599, ""Allegiance"":""Federation"" }, { ""Name"":""The Dark Wheel"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.092131, ""Allegiance"":""Independent"" }, { ""Name"":""Los Chupacabras"", ""FactionState"":""None"", ""Government"":""PrisonColony"", ""Influence"":0.099808, ""Allegiance"":""Independent"" } ], ""SystemFaction"":""The Pilots Federation"", ""FactionState"":""Boom"" }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:25Z"", ""event"":""Docked"", ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.124878 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");
            Assert.AreEqual(100, sidewinder.health);

            // Purchase a Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:37Z"", ""event"":""ShipyardBuy"", ""ShipType"":""empire_courier"", ""ShipPrice"":2231423, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:38Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:38Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":"""", ""ShipIdent"":"""", ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:38Z"", ""event"":""ShipyardNew"", ""ShipType"":""empire_courier"", ""NewShipID"":902 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:48Z"", ""event"":""Docked"", ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.122131 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder.model, "Sidewinder");

            courier = shipMonitor.GetShip(courierId);
            Assert.AreEqual(courier, shipMonitor.GetCurrentShip());
            Assert.AreEqual(courier.model, "Imperial Courier");
            Assert.AreEqual(100, courier.health);

            // Swap back to the SideWinder
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:15Z"", ""event"":""ShipyardSwap"", ""ShipType"":""sidewinder"", ""ShipID"":901, ""StoreOldShip"":""Empire_Courier"", ""StoreShipID"":902 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:16Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:16Z"", ""event"":""Loadout"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""Modules"":[ { ""Slot"":""SmallHardpoint1"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""Armour"", ""Item"":""SideWinder_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size1_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot01_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot02_Size2"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Sidewinder_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:25Z"", ""event"":""Docked"", ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.120514 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");

            // Swap back to the Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:35Z"", ""event"":""ShipyardSwap"", ""ShipType"":""empire_courier"", ""ShipID"":902, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:35Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:36Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":"""", ""ShipIdent"":"""", ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:45Z"", ""event"":""Docked"", ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.119690 }", shipMonitor);

            courier = shipMonitor.GetShip(courierId);
            Assert.AreEqual(courier, shipMonitor.GetCurrentShip());
            Assert.AreEqual(courier.model, "Imperial Courier");

            // Name the Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:19:55Z"", ""event"":""SetUserShipName"", ""Ship"":""empire_courier"", ""ShipID"":902, ""UserShipName"":""Scunthorpe Bound"", ""UserShipId"":""MC-24E"" }", shipMonitor);

            courier = shipMonitor.GetShip(courierId);
            Assert.AreEqual(courier, shipMonitor.GetCurrentShip());
            Assert.AreEqual(courier.model, "Imperial Courier");
            Assert.AreEqual(courier.name, "Scunthorpe Bound");

            // Swap back to the SideWinder
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:03Z"", ""event"":""ShipyardSwap"", ""ShipType"":""sidewinder"", ""ShipID"":901, ""StoreOldShip"":""Empire_Courier"", ""StoreShipID"":902 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:04Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:04Z"", ""event"":""Loadout"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""Modules"":[ { ""Slot"":""SmallHardpoint1"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""Armour"", ""Item"":""SideWinder_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size1_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot01_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot02_Size2"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Sidewinder_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:13Z"", ""event"":""Docked"", ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.120514 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");

            // Swap back to the Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:47Z"", ""event"":""ShipyardSwap"", ""ShipType"":""empire_courier"", ""ShipID"":902, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:48Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:48Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":""Scunthorpe Bound"", ""ShipIdent"":""MC-24E"", ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:57Z"", ""event"":""Docked"", ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.117706 }", shipMonitor);

            courier = shipMonitor.GetShip(courierId);
            Assert.AreEqual(courier, shipMonitor.GetCurrentShip());
            Assert.AreEqual(courier.model, "Imperial Courier");
            Assert.AreEqual(courier.name, "Scunthorpe Bound");

            // Sell the Sidewinder
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:27:51Z"", ""event"":""ShipyardSell"", ""ShipType"":""sidewinder"", ""SellShipID"":901, ""ShipPrice"":25272 }", shipMonitor);

            // Sell the Courier.  Note that this isn't strictly legal, as it involves selling our active ship, but we can get away with it in our test harness
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:27:51Z"", ""event"":""ShipyardSell"", ""ShipType"":""Empire_Courier"", ""SellShipID"":902, ""ShipPrice"":2008281 }", shipMonitor);
        }

        private void SendEvents(string line, ShipMonitor monitor)
        {
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            foreach (Event @event in events)
            {
                monitor.PreHandle(@event);
            }
        }
    }
}
