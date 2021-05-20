﻿using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tests.Properties;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class ShipTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

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
        public void TestShipModel()
        {
            string line = "{ \"timestamp\":\"2016-09-20T18:14:26Z\", \"event\":\"ShipyardBuy\", \"ShipType\":\"Empire_Eagle\", \"ShipPrice\":10000, \"SellOldShip\":\"CobraMkIII\", \"SellShipID\":42, \"SellPrice\":950787, \"MarketID\":128132856 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);

            ShipPurchasedEvent @event = (ShipPurchasedEvent)events[0];
            Assert.AreEqual("Imperial Eagle", @event.ship);
            Assert.AreEqual(128132856, @event.marketId);
        }

        [TestMethod]
        public void TestLoadoutParsing()
        {
            string data = DeserializeJsonResource<string>(Resources.loadout);

            List<Event> events = JournalMonitor.ParseJournalEntry(data);
            Assert.AreEqual(1, events.Count);
            ShipLoadoutEvent loadoutEvent = events[0] as ShipLoadoutEvent;
            Assert.AreEqual("Peppermint", loadoutEvent.shipname);
            Assert.AreEqual(18, loadoutEvent.compartments.Count);
            Assert.AreEqual(7, loadoutEvent.hardpoints.Count);

            ShipMonitor shipMonitor = new ShipMonitor();
            var privateObject = new PrivateObject(shipMonitor);
            object[] args = new object[] { loadoutEvent };
            Ship ship = privateObject.Invoke("ParseShipLoadoutEvent", args) as Ship;
            Assert.AreEqual("Peppermint", ship.name);
            Assert.AreEqual("Int_FuelScoop_Size7_Class5", ship.compartments[0].module.edname);
            Assert.AreEqual("Fuel Scoop", ship.compartments[0].module.invariantName);
            Assert.AreEqual(16, ship.fueltankcapacity);
            Assert.AreEqual(24, ship.fueltanktotalcapacity);
        }

        [TestMethod]
        public void TestShipScenario1()
        {
            int sidewinderId = 901;
            int courierId = 902;

            Ship sidewinder;
            Ship courier;

            // Start a ship monitor
            ShipMonitor shipMonitor = new ShipMonitor();
            PrivateObject privateObject = new PrivateObject(shipMonitor);
            privateObject.SetFieldOrProperty("updatedAt", DateTime.MinValue);

            // Log in
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:21Z"", ""event"":""LoadGame"", ""Commander"":""McDonald"", ""Horizons"":true,""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""FuelLevel"":2.000000, ""FuelCapacity"":2.000000, ""GameMode"":""Solo"", ""Credits"":1637243231, ""Loan"":0 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:24Z"", ""event"":""Location"", ""Docked"":true, ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StarPos"":[55.719,17.594,27.156], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""High Tech"", ""SystemGovernment"":""$government_Democracy;"", ""SystemGovernment_Localised"":""Democracy"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Body"":""Jameson Memorial"", ""BodyType"":""Station"", ""Factions"":[ { ""Name"":""Lori Jameson"", ""FactionState"":""None"", ""Government"":""Engineer"", ""Influence"":0.040307, ""Allegiance"":""Independent"" }, { ""Name"":""LTT 4487 Industry"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.191939, ""Allegiance"":""Federation"" }, { ""Name"":""The Pilots Federation"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.447217, ""Allegiance"":""Independent"" }, { ""Name"":""Future of Arro Naga"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.128599, ""Allegiance"":""Federation"" }, { ""Name"":""The Dark Wheel"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.092131, ""Allegiance"":""Independent"" }, { ""Name"":""Los Chupacabras"", ""FactionState"":""None"", ""Government"":""PrisonColony"", ""Influence"":0.099808, ""Allegiance"":""Independent"" } ], ""SystemFaction"":""The Pilots Federation"", ""FactionState"":""Boom"" }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:25Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.124878 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");
            Assert.AreEqual(100, sidewinder.health);

            // Purchase a Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:37Z"", ""event"":""ShipyardBuy"", ""ShipType"":""empire_courier"", ""ShipPrice"":2231423, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128132856 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:38Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:40Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":"""", ""ShipIdent"":"""", ""HullHealth"":1, ""Hot"":false, ""UnladenMass"": 1200, ""MaxJumpRange"":20, ""Rebuy"":9797719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:42Z"", ""event"":""ShipyardNew"", ""ShipType"":""empire_courier"", ""NewShipID"":902 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:48Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.122131 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder.model, "Sidewinder");

            courier = shipMonitor.GetShip(courierId);
            Assert.AreEqual(courier, shipMonitor.GetCurrentShip());
            Assert.AreEqual(courier.model, "Imperial Courier");
            Assert.AreEqual(100, courier.health);

            // Swap back to the SideWinder
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:15Z"", ""event"":""ShipyardSwap"", ""ShipType"":""sidewinder"", ""ShipID"":901, ""StoreOldShip"":""Empire_Courier"", ""StoreShipID"":902, ""MarketID"":128666762 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:16Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:18Z"", ""event"":""Loadout"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""HullHealth"":1, ""Hot"":false, ""UnladenMass"": 1000, ""MaxJumpRange"":12, ""Rebuy"":979771, ""Modules"":[ { ""Slot"":""SmallHardpoint1"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""Armour"", ""Item"":""SideWinder_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size1_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot01_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot02_Size2"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Sidewinder_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:25Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.120514 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");

            // Swap back to the Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:35Z"", ""event"":""ShipyardSwap"", ""ShipType"":""empire_courier"", ""ShipID"":902, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128666762 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:36Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:38Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":"""", ""ShipIdent"":"""", ""HullHealth"":1, ""Hot"":false, ""UnladenMass"": 1200, ""MaxJumpRange"":20, ""Rebuy"":9797719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:45Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.119690 }", shipMonitor);

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
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:03Z"", ""event"":""ShipyardSwap"", ""ShipType"":""sidewinder"", ""ShipID"":901, ""StoreOldShip"":""Empire_Courier"", ""StoreShipID"":902, ""MarketID"":128666762 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:04Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:08Z"", ""event"":""Loadout"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""HullHealth"":1, ""Hot"":false, ""UnladenMass"": 1000, ""MaxJumpRange"":12, ""Rebuy"":979771, ""Modules"":[ { ""Slot"":""SmallHardpoint1"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""Armour"", ""Item"":""SideWinder_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size1_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot01_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot02_Size2"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Sidewinder_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:13Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.120514 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");

            // Swap back to the Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:47Z"", ""event"":""ShipyardSwap"", ""ShipType"":""empire_courier"", ""ShipID"":902, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128666762 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:48Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:50Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":""Scunthorpe Bound"", ""ShipIdent"":""MC-24E"", ""HullHealth"": 1, ""Hot"":false, ""UnladenMass"": 1200, ""MaxJumpRange"":20, ""Rebuy"":9797719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);

            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:57Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.117706 }", shipMonitor);

            courier = shipMonitor.GetShip(courierId);
            Assert.AreEqual(courier, shipMonitor.GetCurrentShip());
            Assert.AreEqual(courier.model, "Imperial Courier");
            Assert.AreEqual(courier.name, "Scunthorpe Bound");
            Assert.AreEqual("Int_CargoRack_Size2_Class1", courier.compartments[0].module.EDName);
            Assert.AreEqual("cargo rack", courier.compartments[0].module.invariantName.ToLowerInvariant());

            // Sell the Sidewinder
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:27:51Z"", ""event"":""ShipyardSell"", ""ShipType"":""sidewinder"", ""SellShipID"":901, ""ShipPrice"":25272, ""MarketID"":128666762 }", shipMonitor);

            // Sell the Courier.  Note that this isn't strictly legal, as it involves selling our active ship, but we can get away with it in our test harness
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:27:52Z"", ""event"":""ShipyardSell"", ""ShipType"":""Empire_Courier"", ""SellShipID"":902, ""ShipPrice"":2008281, ""MarketID"":128666762 }", shipMonitor);
        }

        [TestMethod]
        public void TestModulePurchasedEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 07 - 24T05: 49:15Z"", ""event"":""ModuleBuy"", ""Slot"":""TinyHardpoint1"", ""BuyItem"":""$hpt_crimescanner_size0_class3_name;"", ""BuyItem_Localised"":""K-Warrant Scanner"", ""MarketID"":3223343616, ""BuyPrice"":101025, ""Ship"":""krait_mkii"", ""ShipID"":81 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ModulePurchasedEvent @event = (ModulePurchasedEvent)events[0];

            Assert.AreEqual("hpt_crimescanner_size0_class3", @event.buymodule.EDName.ToLowerInvariant());
            Assert.AreEqual(3223343616, @event.marketId);
            Assert.AreEqual(101025, @event.buyprice);
            Assert.IsNull(@event.sellmodule);
            Assert.IsNull(@event.sellprice);
            Assert.AreEqual("Krait Mk. II", @event.ship);
            Assert.AreEqual(81, @event.shipid);
        }

        [TestMethod]
        public void TestModuleRetrievedEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 06 - 29T04: 45:21Z"", ""event"":""ModuleRetrieve"", ""MarketID"":3223343616, ""Slot"":""PowerPlant"", ""RetrievedItem"":""$int_powerplant_size6_class5_name;"", ""RetrievedItem_Localised"":""Power Plant"", ""Ship"":""krait_mkii"", ""ShipID"":81, ""Hot"":false, ""EngineerModifications"":""PowerPlant_Boosted"", ""Level"":4, ""Quality"":0.000000, ""SwapOutItem"":""$int_powerplant_size6_class5_name;"", ""SwapOutItem_Localised"":""Power Plant"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ModuleRetrievedEvent @event = (ModuleRetrievedEvent)events[0];

            Assert.AreEqual("int_powerplant_size6_class5", @event.module.edname.ToLowerInvariant());
            Assert.AreEqual(3223343616, @event.marketId);
            Assert.AreEqual("PowerPlant", @event.slot);
            Assert.AreEqual("Krait Mk. II", @event.ship);
            Assert.AreEqual(81, @event.shipid);
            Assert.AreEqual("int_powerplant_size6_class5", @event.swapoutmodule.edname.ToLowerInvariant());
        }

        [TestMethod]
        public void TestModuleSoldEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 06 - 29T02: 37:33Z"", ""event"":""ModuleSell"", ""MarketID"":128132856, ""Slot"":""Slot04_Size5"", ""SellItem"":""$int_fighterbay_size5_class1_name;"", ""SellItem_Localised"":""Fighter Hangar"", ""SellPrice"":561252, ""Ship"":""krait_mkii"", ""ShipID"":81 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ModuleSoldEvent @event = (ModuleSoldEvent)events[0];

            Assert.AreEqual("int_fighterbay_size5_class1", @event.module.edname.ToLowerInvariant());
            Assert.AreEqual(561252, @event.price);
            Assert.AreEqual(128132856, @event.marketId);
            Assert.AreEqual("Slot04_Size5", @event.slot);
            Assert.AreEqual("Krait Mk. II", @event.ship);
            Assert.AreEqual(81, @event.shipid);
        }

        [TestMethod]
        public void TestModuleStoredEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 07 - 22T23: 51:31Z"", ""event"":""ModuleStore"", ""MarketID"":3223343616, ""Slot"":""Slot06_Size3"", ""StoredItem"":""$int_dronecontrol_collection_size3_class5_name;"", ""StoredItem_Localised"":""Collector"", ""Ship"":""krait_mkii"", ""ShipID"":81, ""Hot"":false, ""EngineerModifications"":""Misc_LightWeight"", ""Level"":5, ""Quality"":1.000000 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ModuleStoredEvent @event = (ModuleStoredEvent)events[0];

            Assert.AreEqual(3223343616, @event.marketId);
            Assert.AreEqual("Slot06_Size3", @event.slot);
            Assert.AreEqual("int_dronecontrol_collection_size3_class5", @event.module.edname.ToLowerInvariant());
            Assert.AreEqual("Krait Mk. II", @event.ship);
            Assert.AreEqual(81, @event.shipid);
            Assert.AreEqual(true, @event.engineermodifications.Length > 0);
        }

        [TestMethod]
        public void TestModuleSwappedEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 06 - 29T02: 38:30Z"", ""event"":""ModuleSwap"", ""MarketID"":128132856, ""FromSlot"":""Slot06_Size3"", ""ToSlot"":""Slot07_Size3"", ""FromItem"":""$int_stellarbodydiscoveryscanner_advanced_name;"", ""FromItem_Localised"":""D - Scanner"", ""ToItem"":""Null"", ""Ship"":""krait_mkii"", ""ShipID"":81 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ModuleSwappedEvent @event = (ModuleSwappedEvent)events[0];

            Assert.AreEqual(128132856, @event.marketId);
            Assert.AreEqual("Slot06_Size3", @event.fromslot);
            Assert.AreEqual("Slot07_Size3", @event.toslot);
            Assert.AreEqual("int_stellarbodydiscoveryscanner_advanced", @event.frommodule.edname.ToLowerInvariant());
            Assert.AreEqual("advanced discovery scanner", @event.frommodule.invariantName.ToLowerInvariant());
            Assert.IsNull(@event.tomodule);
            Assert.AreEqual("Krait Mk. II", @event.ship);
            Assert.AreEqual(81, @event.shipid);

            // Obtain our ship monitor and save its state
            ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
            ObservableCollection<Ship> shipyard = shipMonitor.shipyard;
            PrivateObject privateObject = new PrivateObject(shipMonitor);
            privateObject.SetFieldOrProperty("updatedAt", DateTime.MinValue);

            // Set up our ship
            Ship ship = new Ship() { model = @event.ship, LocalId = (int)@event.shipid };
            ship.compartments.Add(new Compartment() { name = @event.fromslot, size = 3, module = @event.frommodule });
            ship.compartments.Add(new Compartment() { name = @event.toslot, size = 3, module = @event.tomodule });
            privateObject.Invoke("RemoveShip", new object[] { (int)@event.shipid });
            privateObject.Invoke("AddShip", new object[] { ship });

            // Test the event handler
            Assert.AreEqual(@event.frommodule, ship.compartments.FirstOrDefault(c => c.name == @event.fromslot)?.module);
            Assert.AreEqual(@event.tomodule, ship.compartments.FirstOrDefault(c => c.name == @event.toslot)?.module);
            privateObject.Invoke("handleModuleSwappedEvent", new object[] { @event });
            Assert.AreEqual(@event.frommodule, ship.compartments.FirstOrDefault(c => c.name == @event.toslot)?.module);
            Assert.AreEqual(@event.tomodule, ship.compartments.FirstOrDefault(c => c.name == @event.fromslot)?.module);

            // Restore our ship monitor to its original state
            privateObject.SetFieldOrProperty("shipyard", shipyard);
        }

        [TestMethod]
        public void TestStoredModulesEvent()
        {
            string line = "{ \"timestamp\":\"2019-01-25T00:06:36Z\", \"event\":\"StoredModules\", \"MarketID\":128928173, \"StationName\":\"Rock of Isolation\", \"StarSystem\":\"Omega Sector OD-S b4-0\", \"Items\":[ { \"Name\":\"$int_shieldgenerator_size7_class3_fast_name;\", \"Name_Localised\":\"Bi-Weave Shield\", \"StorageSlot\":52, \"StarSystem\":\"Omega Sector VE-Q b5-15\", \"MarketID\":128757071, \"TransferCost\":9729, \"TransferTime\":480, \"BuyPrice\":7501033, \"Hot\":false, \"EngineerModifications\":\"ShieldGenerator_Thermic\", \"Level\":5, \"Quality\":0.000000 }, { \"Name\":\"$int_cargorack_size7_class1_name;\", \"Name_Localised\":\"Cargo Rack\", \"StorageSlot\":101, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":976616, \"Hot\":false }, { \"Name\":\"$int_hyperdrive_size6_class5_name;\", \"Name_Localised\":\"FSD\", \"StorageSlot\":53, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":4535529, \"TransferTime\":55231, \"BuyPrice\":13752602, \"Hot\":false, \"EngineerModifications\":\"FSD_LongRange\", \"Level\":5, \"Quality\":0.000000 }, { \"Name\":\"$int_shieldgenerator_size5_class3_fast_name;\", \"Name_Localised\":\"Bi-Weave Shield\", \"StorageSlot\":116, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":280636, \"TransferTime\":55231, \"BuyPrice\":850659, \"Hot\":false, \"EngineerModifications\":\"ShieldGenerator_Optimised\", \"Level\":3, \"Quality\":0.000000 }, { \"Name\":\"$hpt_multicannon_gimbal_huge_name;\", \"Name_Localised\":\"Multi-Cannon\", \"StorageSlot\":107, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":1787862, \"TransferTime\":55231, \"BuyPrice\":5420960, \"Hot\":false, \"EngineerModifications\":\"Weapon_Overcharged\", \"Level\":4, \"Quality\":0.838000 }, { \"Name\":\"$int_repairer_size4_class5_name;\", \"Name_Localised\":\"AFM Unit\", \"StorageSlot\":114, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":1367146, \"TransferTime\":55231, \"BuyPrice\":4145240, \"Hot\":false, \"EngineerModifications\":\"Misc_Shielded\", \"Level\":3, \"Quality\":1.000000 }, { \"Name\":\"$int_refinery_size4_class5_name;\", \"Name_Localised\":\"Refinery\", \"StorageSlot\":102, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":3730077, \"Hot\":false }, { \"Name\":\"$int_fsdinterdictor_size4_class3_name;\", \"Name_Localised\":\"FSD Interdictor\", \"StorageSlot\":110, \"InTransit\":true, \"BuyPrice\":2311546, \"Hot\":false, \"EngineerModifications\":\"FSDinterdictor_Expanded\", \"Level\":4, \"Quality\":0.979500 }, { \"Name\":\"$int_hullreinforcement_size4_class2_name;\", \"Name_Localised\":\"Hull Reinforcement\", \"StorageSlot\":105, \"InTransit\":true, \"BuyPrice\":171113, \"Hot\":false, \"EngineerModifications\":\"HullReinforcement_HeavyDuty\", \"Level\":4, \"Quality\":0.000000 }, { \"Name\":\"$int_shieldgenerator_size3_class3_fast_name;\", \"Name_Localised\":\"Bi-Weave Shield\", \"StorageSlot\":113, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":24597, \"TransferTime\":55231, \"BuyPrice\":74284, \"Hot\":false, \"EngineerModifications\":\"ShieldGenerator_Optimised\", \"Level\":5, \"Quality\":0.956700 }, { \"Name\":\"$int_modulereinforcement_size3_class2_name;\", \"Name_Localised\":\"Module Reinforcement\", \"StorageSlot\":120, \"StarSystem\":\"HIP 21066\", \"MarketID\":3221959680, \"TransferCost\":27804, \"TransferTime\":56644, \"BuyPrice\":81900, \"Hot\":false }, { \"Name\":\"$int_hullreinforcement_size3_class2_name;\", \"Name_Localised\":\"Hull Reinforcement\", \"StorageSlot\":115, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":24408, \"TransferTime\":55231, \"BuyPrice\":73710, \"Hot\":false, \"EngineerModifications\":\"HullReinforcement_HeavyDuty\", \"Level\":4, \"Quality\":0.605000 }, { \"Name\":\"$int_dronecontrol_collection_size3_class2_name;\", \"Name_Localised\":\"Collector\", \"StorageSlot\":118, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":10530, \"Hot\":false, \"EngineerModifications\":\"CollectionLimpet_LightWeight\", \"Level\":5, \"Quality\":0.000000 }, { \"Name\":\"$int_dronecontrol_fueltransfer_size3_class2_name;\", \"Name_Localised\":\"Fuel Transfer\", \"StorageSlot\":57, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":3225, \"TransferTime\":55231, \"BuyPrice\":9477, \"Hot\":false, \"EngineerModifications\":\"FuelTransferLimpet_LightWeight\", \"Level\":4, \"Quality\":0.000000 }, { \"Name\":\"$hpt_mining_seismchrgwarhd_turret_medium_name;\", \"Name_Localised\":\"Seismic Charge\", \"StorageSlot\":111, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":390988, \"Hot\":false }, { \"Name\":\"$hpt_mining_subsurfdispmisle_turret_medium_name;\", \"Name_Localised\":\"Disp. Missile\", \"StorageSlot\":109, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":334986, \"Hot\":false }, { \"Name\":\"$int_modulereinforcement_size2_class2_name;\", \"Name_Localised\":\"Module Reinforcement\", \"StorageSlot\":112, \"StarSystem\":\"Wayutabal\", \"MarketID\":3224777984, \"TransferCost\":11773, \"TransferTime\":55696, \"BuyPrice\":35100, \"Hot\":false }, { \"Name\":\"$hpt_mininglaser_turret_medium_name;\", \"Name_Localised\":\"Mining Laser\", \"StorageSlot\":108, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":28587, \"Hot\":false }, { \"Name\":\"$int_dronecontrol_prospector_size1_class5_name;\", \"Name_Localised\":\"Prospector\", \"StorageSlot\":104, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":8424, \"Hot\":false }, { \"Name\":\"$hpt_mining_abrblstr_turret_small_name;\", \"Name_Localised\":\"Abrasion Blaster\", \"StorageSlot\":106, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":24114, \"Hot\":false }, { \"Name\":\"$int_corrosionproofcargorack_size1_class2_name;\", \"Name_Localised\":\"Corrosion Resistant Cargo Rack\", \"StorageSlot\":59, \"StarSystem\":\"Maia\", \"MarketID\":128679559, \"TransferCost\":4376, \"TransferTime\":58455, \"BuyPrice\":12249, \"Hot\":false }, { \"Name\":\"$int_corrosionproofcargorack_size1_class2_name;\", \"Name_Localised\":\"Corrosion Resistant Cargo Rack\", \"StorageSlot\":51, \"StarSystem\":\"Maia\", \"MarketID\":128679559, \"TransferCost\":4376, \"TransferTime\":58455, \"BuyPrice\":12249, \"Hot\":false }, { \"Name\":\"$int_corrosionproofcargorack_size1_class2_name;\", \"Name_Localised\":\"Corrosion Resistant Cargo Rack\", \"StorageSlot\":58, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":3621, \"TransferTime\":55231, \"BuyPrice\":10679, \"Hot\":false }, { \"Name\":\"$hpt_mrascanner_size0_class5_name;\", \"Name_Localised\":\"Pulse Wave\", \"StorageSlot\":100, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":909217, \"Hot\":false }, { \"Name\":\"$hpt_heatsinklauncher_turret_tiny_name;\", \"Name_Localised\":\"Heatsink\", \"StorageSlot\":119, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":1254, \"TransferTime\":55231, \"BuyPrice\":3500, \"Hot\":false, \"EngineerModifications\":\"HeatSinkLauncher_HeatSinkCapacity\", \"Level\":3, \"Quality\":0.000000 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            StoredModulesEvent @event = (StoredModulesEvent)events[0];

            Assert.AreEqual("Omega Sector OD-S b4-0", @event.system);
            Assert.AreEqual("Rock of Isolation", @event.station);
            Assert.AreEqual(128928173, @event.marketId);
            StoredModule storedModule = @event.storedmodules.FirstOrDefault(m => m.module.EDName.ToLowerInvariant() == "int_hyperdrive_size6_class5");
            Assert.AreEqual("Shinrarta Dezhra", storedModule.system);
            Assert.AreEqual(128666762, storedModule.marketid);
            Assert.AreEqual("Jameson Memorial", storedModule.station);
        }

        [TestMethod]
        public void TestShipTransferEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 07 - 30T04: 57:09Z"", ""event"":""ShipyardTransfer"", ""ShipType"":""TypeX"", ""ShipType_Localised"":""Alliance Chieftain"", ""ShipID"":76, ""System"":""Balante"", ""ShipMarketID"":3223259392, ""Distance"":8.017741, ""TransferPrice"":70213, ""TransferTime"":380, ""MarketID"":3223343616 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ShipTransferInitiatedEvent @event = (ShipTransferInitiatedEvent)events[0];

            Assert.AreEqual("Alliance Chieftain", @event.ship);
            Assert.AreEqual(76, @event.shipid);
            Assert.AreEqual(3223259392, @event.fromMarketId);
            Assert.AreEqual(8.017741M, @event.distance);
            Assert.AreEqual(70213, @event.price);
            Assert.AreEqual(380, @event.time);
            Assert.AreEqual(3223343616, @event.toMarketId);
        }

        private void SendEvents(string line, ShipMonitor monitor)
        {
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            foreach (Event @event in events)
            {
                monitor.PreHandle(@event);
            }
        }

        [TestMethod]
        public void TestShipMonitorDeserialization()
        {
            // Read from our test item "shipMonitor.json"
            ShipMonitorConfiguration configuration = new ShipMonitorConfiguration();
            try
            {
                configuration = DeserializeJsonResource<ShipMonitorConfiguration>(Resources.shipMonitor);
            }
            catch (Exception)
            {
                Assert.Fail("Failed to read ship configuration");
            }

            // Start a ship monitor
            ShipMonitor shipMonitor = new ShipMonitor();
            var privateObject = new PrivateObject(shipMonitor);

            // Build a new shipyard
            List<Ship> newShiplist = configuration.shipyard.OrderBy(s => s.model).ToList();

            // Update the shipyard
            privateObject.SetFieldOrProperty("shipyard", new ObservableCollection<Ship>(newShiplist));
            privateObject.SetFieldOrProperty("updatedAt", DateTime.MinValue);

            shipMonitor.SetCurrentShip(configuration.currentshipid);
            Assert.AreEqual(81, shipMonitor.GetCurrentShip().LocalId);

            Ship ship1 = shipMonitor.GetShip(0);
            Ship ship2 = shipMonitor.GetShip(81);

            Assert.IsNotNull(ship1);
            Assert.AreEqual("Cobra Mk. III", ship1.model);
            Assert.AreEqual(0, ship1.LocalId);
            Assert.AreEqual("The Dynamo", ship1.name);
            Assert.AreEqual("Laksak", ship1.starsystem);
            Assert.AreEqual("Stjepan Seljan Hub", ship1.station);
            Assert.AreEqual(8605684, ship1.value);

            Assert.IsNotNull(ship2);
            Assert.AreEqual("Krait Mk. II", ship2.model);
            Assert.AreEqual(81, ship2.LocalId);
            Assert.AreEqual("The Impact Kraiter", ship2.name);
            Assert.AreEqual(16, ship2.cargocapacity);
            Assert.AreEqual(8, ship2.compartments.Count());
            Assert.AreEqual("Slot01_Size6", ship2.compartments[0].name);
            Assert.AreEqual(6, ship2.compartments[0].size);
            Assert.IsNotNull(ship2.compartments[0].module);
            Assert.AreEqual("Int_ShieldGenerator_Size6_Class3_Fast", ship2.compartments[0].module.EDName);
            Assert.AreEqual("Bi-Weave Shield Generator", ship2.compartments[0].module.invariantName);
            Assert.AreEqual("SRV", ship2.launchbays[0].type);
            Assert.AreEqual(2, ship2.launchbays[0].vehicles.Count());
            Assert.AreEqual("TestBuggy", ship2.launchbays[0].vehicles[0].vehicleDefinition);
            Assert.AreEqual("Starter", ship2.launchbays[0].vehicles[0].loadoutDescription);
            Assert.AreEqual("dual plasma repeaters", ship2.launchbays[0].vehicles[0].localizedDescription);
        }

        [TestMethod]
        public void TestShipMonitorDeserializationDoesntMutateStatics()
        {
            // Read from our test item "shipMonitor.json"
            ShipMonitorConfiguration configuration = new ShipMonitorConfiguration();
            try
            {
                configuration = DeserializeJsonResource<ShipMonitorConfiguration>(Resources.shipMonitor);
            }
            catch (Exception)
            {
                Assert.Fail("Failed to read ship configuration");
            }

            Assert.AreEqual("Multipurpose", Role.MultiPurpose.edname);
        }

        [TestMethod]
        public void TestShipMonitorDeserializationMatchesSerialization()
        {
            var privateObject = new PrivateObject(new ShipMonitor());
            privateObject.SetFieldOrProperty("shipyard", new ObservableCollection<Ship>());
            privateObject.SetFieldOrProperty("updatedAt", DateTime.MinValue);

            string data = DeserializeJsonResource<string>(Resources.loadout);
            List<Event> events = JournalMonitor.ParseJournalEntry(data);
            ShipLoadoutEvent loadoutEvent = events[0] as ShipLoadoutEvent;
            object[] loadoutArgs = new object[] { loadoutEvent };
            privateObject.Invoke("handleShipLoadoutEvent", loadoutArgs);

            Ship originalShip = EDDI.Instance.CurrentShip;

            if (originalShip != null)
            {
                string originalShipString = JsonConvert.SerializeObject(originalShip);
                Ship deserializedShip = JsonConvert.DeserializeObject<Ship>(originalShipString);
                if (originalShip != null && deserializedShip != null)
                {
                    Assert.IsTrue(JsonParsing.compareJsonEquality(originalShip, deserializedShip, true, out string mutatedProperty, Array.Empty<string>()));
                    if (!string.IsNullOrEmpty(mutatedProperty))
                    {
                        Assert.Fail("Deserialized ship doesn't match original ship for property " + mutatedProperty);
                    }
                }
            }
            else
            {
                Assert.Fail("Failed to get ship");
            }
        }

        [TestMethod]
        public void TestFighterLoadoutEvent()
        {
            string data = DeserializeJsonResource<string>(Resources.loadout);
            List<Event> events = JournalMonitor.ParseJournalEntry(data);
            ShipLoadoutEvent loadoutEvent = events[0] as ShipLoadoutEvent;
            object[] loadoutArgs = new object[] { loadoutEvent };

            string data2 = DeserializeJsonResource<string>(Resources.fighterLoadout);
            events = JournalMonitor.ParseJournalEntry(data2);
            ShipLoadoutEvent fighterLoadoutEvent = events[0] as ShipLoadoutEvent;
            object[] fighterLoadoutArgs = new object[] { fighterLoadoutEvent };

            var privateObject = new PrivateObject(new ShipMonitor());
            privateObject.SetFieldOrProperty("shipyard", new ObservableCollection<Ship>());
            privateObject.SetFieldOrProperty("updatedAt", DateTime.MinValue);
            privateObject.Invoke("handleShipLoadoutEvent", loadoutArgs);
            privateObject.Invoke("handleShipLoadoutEvent", fighterLoadoutArgs);

            var currentShip = (Ship)privateObject.Invoke("GetCurrentShip", null);

            // After a loadout event generated from a fighter, 
            // we still want to track the ship we launched from as our current ship.
            Assert.AreEqual(loadoutEvent.shipid, currentShip.LocalId);
            Assert.AreNotEqual(fighterLoadoutEvent.shipid, currentShip.LocalId);
        }


        [TestMethod]
        public void TestJournalModulePurchasedHandlingMinimalShip()
        {
            string line = "{ \"timestamp\":\"2018-12-25T22:55:11Z\", \"event\":\"ModuleBuy\", \"Slot\":\"Military01\", \"BuyItem\":\"$int_guardianshieldreinforcement_size5_class2_name;\", \"BuyItem_Localised\":\"Guardian Shield Reinforcement\", \"MarketID\":128666762, \"BuyPrice\":873402, \"Ship\":\"federation_corvette\", \"ShipID\":119 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            ModulePurchasedEvent @event = (ModulePurchasedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(ModulePurchasedEvent));

            Assert.AreEqual(119, (int)@event.shipid);
            Assert.IsNotNull(@event.slot);
            Assert.IsNotNull(@event.buymodule);

            PrivateObject privateObject = new PrivateObject(new ShipMonitor());

            Ship ship = ShipDefinitions.FromModel(@event.ship);
            ship.LocalId = (int)@event.shipid;
            object[] moduleArgs = new object[] { ship, @event.slot, @event.buymodule };
            privateObject.Invoke("AddModule", moduleArgs);

            foreach (Compartment compartment in ship.compartments)
            {
                if (compartment?.name == "Military01")
                {
                    Assert.AreEqual("Guardian Shield Reinforcement", compartment.module?.invariantName);
                }
            }
        }

        [TestMethod]
        public void TestJournalModuleSoldHandlingMinimalShip()
        {
            string line = "{ \"timestamp\":\"2018-12-25T22:55:11Z\", \"event\":\"ModuleBuy\", \"Slot\":\"Slot01_Size7\", \"BuyItem\":\"$int_guardianshieldreinforcement_size5_class2_name;\", \"BuyItem_Localised\":\"Guardian Shield Reinforcement\", \"MarketID\":128666762, \"BuyPrice\":873402, \"Ship\":\"federation_corvette\", \"ShipID\":119 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ModulePurchasedEvent @event = (ModulePurchasedEvent)events[0];

            PrivateObject privateObject = new PrivateObject(new ShipMonitor());

            Ship ship = ShipDefinitions.FromModel(@event.ship);
            ship.LocalId = (int)@event.shipid;
            string slot = @event.slot;
            Module module = @event.buymodule;
            object[] moduleArgs = new object[] { ship, slot, module };
            privateObject.Invoke("AddModule", moduleArgs);

            // now sell the module
            moduleArgs = new object[] { ship, slot, null };
            privateObject.Invoke("RemoveModule", moduleArgs);
            foreach (Compartment compartment in ship.compartments)
            {
                if (compartment?.name == "Military01")
                {
                    Assert.IsNull(compartment.module);
                }
            }
        }

        [TestMethod]
        public void TestShipRefuelledEvent_Scooping()
        {
            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            privateObject.SetFieldOrProperty("CurrentShip", ShipDefinitions.FromEDModel("Asp"));
            Ship currentShip = (Ship)privateObject.GetFieldOrProperty("CurrentShip");
            currentShip.fueltanktotalcapacity = 32M;

            string line1 = "{ \"timestamp\":\"2019 - 07 - 21T16: 28:35Z\", \"event\":\"FuelScoop\", \"Scooped\":5.001066, \"Total\":31.552881 }";
            string line2 = "{ \"timestamp\":\"2019 - 07 - 21T16: 28:35Z\", \"event\":\"FuelScoop\", \"Scooped\":0.447121, \"Total\":32.000000 }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line1);
            ShipRefuelledEvent @event1 = (ShipRefuelledEvent)events[0];
            Assert.AreEqual(5.001066M, @event1.amount);
            Assert.AreEqual(31.552881M, @event1.total);
            Assert.IsFalse(@event1.full);
            Assert.AreEqual("Ship refuelled", @event1.type);

            events = JournalMonitor.ParseJournalEntry(line2);
            ShipRefuelledEvent @event2 = (ShipRefuelledEvent)events[0];
            Assert.AreEqual(0.447121M, @event2.amount);
            Assert.AreEqual(32.000000M, @event2.total);
            Assert.IsTrue(event2.full);
        }

        [TestMethod]
        public void TestShipJumpedEvent()
        {
            // Obtain our ship monitor and save its state
            ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
            ObservableCollection<Ship> shipyard = shipMonitor.shipyard;
            PrivateObject privateObject = new PrivateObject(shipMonitor);
            privateObject.SetFieldOrProperty("updatedAt", DateTime.MinValue);

            // Set up our ship
            Ship ship = new Ship() { LocalId = 9999, x = 0, y = 0, z = 0 };
            privateObject.Invoke("RemoveShip", new object[] { 9999 });
            privateObject.Invoke("AddShip", new object[] { ship });

            // Set up our event
            string line = @"{ ""timestamp"":""2019-06-30T05:38:53Z"", ""event"":""FSDJump"", ""StarSystem"":""Ogmar"", ""SystemAddress"":84180519395914, ""StarPos"":[-9534.00000,-905.28125,19802.03125], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""High Tech"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_Confederacy;"", ""SystemGovernment_Localised"":""Confederacy"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":133000, ""Body"":""Ogmar A"", ""BodyID"":1, ""BodyType"":""Star"", ""JumpDist"":8.625, ""FuelUsed"":0.151982, ""FuelLevel"":31.695932, ""Factions"":[ { ""Name"":""Jaques"", ""FactionState"":""Election"", ""Government"":""Cooperative"", ""Influence"":0.104895, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand1;"", ""Happiness_Localised"":""Elated"", ""MyReputation"":100.000000, ""ActiveStates"":[ { ""State"":""Election"" } ] }, { ""Name"":""Colonia Research Department"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.078921, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":21.639999, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""Pilots' Federation Local Branch"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.000000, ""Allegiance"":""PilotsFederation"", ""Happiness"":"""", ""MyReputation"":100.000000 }, { ""Name"":""Colonia Mining Enterprise"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.052947, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000 }, { ""Name"":""Colonia Co-operative"", ""FactionState"":""Election"", ""Government"":""Cooperative"", ""Influence"":0.104895, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":71.470001, ""PendingStates"":[ { ""State"":""Expansion"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Outbreak"" }, { ""State"":""Election"" } ] }, { ""Name"":""Colonia Agricultural Co-operative"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.076923, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":6.640000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""GalCop Colonial Defence Commission"", ""FactionState"":""Boom"", ""Government"":""Confederacy"", ""Influence"":0.449550, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000, ""ActiveStates"":[ { ""State"":""Boom"" } ] }, { ""Name"":""Colonia Tech Combine"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.090909, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""Milanov's Reavers"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.040959, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] } ], ""SystemFaction"":{ ""Name"":""GalCop Colonial Defence Commission"", ""FactionState"":""Boom"" }, ""Conflicts"":[ { ""WarType"":""election"", ""Status"":""active"", ""Faction1"":{ ""Name"":""Jaques"", ""Stake"":""Crockett Gateway"", ""WonDays"":1 }, ""Faction2"":{ ""Name"":""Colonia Co-operative"", ""Stake"":"""", ""WonDays"":2 } } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            JumpedEvent @event = (JumpedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(JumpedEvent));

            // Handle the event
            shipMonitor.PreHandle(@event);

            // Test the result to verify that the distance is calculated relative to the jump coordinates
            Assert.AreEqual(21996.3M, shipMonitor.GetShip(9999)?.distance);
        }

        [TestMethod]
        public void TestShipCommanderContinuedEventInSRV()
        {
            // Set up our `Ship monitor` private object
            ShipMonitor shipMonitor = new ShipMonitor();
            PrivateObject privateObject = new PrivateObject(shipMonitor);
            privateObject.SetFieldOrProperty("updatedAt", DateTime.MinValue);

            // Set up our `currentShipId` property with a value of "9999"
            privateObject.SetFieldOrProperty("currentShipId", 9999);

            // Set up our event (which reports that we are in an SRV with a ship id of "9998")
            string line = @"{ ""timestamp"":""2020-07-20T15:40:45Z"", ""event"":""LoadGame"", ""FID"":""F0000000"", ""Commander"":""TestCommander"", ""Horizons"":true, ""Ship"":""TestBuggy"", ""Ship_Localised"":""SRV Scarab"", ""ShipID"":9998, ""ShipName"":"""", ""ShipIdent"":"""", ""FuelLevel"":0.000000, ""FuelCapacity"":0.000000, ""GameMode"":""Group"", ""Group"":""Children of Raxxla"", ""Credits"":5065687467, ""Loan"":0 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            CommanderContinuedEvent @event = (CommanderContinuedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(CommanderContinuedEvent));

            // Test the result to verify that the ship monitor's `currentShipId` property remains "9999" rather than changing to "9998"
            shipMonitor.PreHandle(@event);
            Assert.AreEqual("TestBuggy", @event.shipEDModel);
            Assert.AreEqual(9999, (int?)privateObject.GetFieldOrProperty("currentShipId"), @"Because the ""ship"" reported by the event is an SRV, the `currentShipId` property of the ship monitor should be unchanged");

            // Re-test the event, except exchange "TestBuggy" for "SRV" in the `shipEDModel` property
            // Verify once again that the ship monitor's `currentShipId` property remains "9999" rather than changing to "9998"
            var privateEvent = new PrivateObject(@event);
            privateEvent.SetFieldOrProperty("shipEDModel", "SRV");
            shipMonitor.PreHandle(@event);
            Assert.AreEqual("SRV", @event.shipEDModel);
            Assert.AreEqual(9999, (int?)privateObject.GetFieldOrProperty("currentShipId"), @"Because the ""ship"" reported by the event is an SRV, the `currentShipId` property of the ship monitor should be unchanged");
        }
    }
}
