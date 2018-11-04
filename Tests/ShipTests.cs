using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using Utilities;
using Eddi;
using Rollbar;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;

namespace UnitTests
{
    [TestClass]
    public class ShipTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;

            // Set ourselves as in beta to stop sending data to remote systems
            EDDI.Instance.eventHandler(new FileHeaderEvent(DateTime.UtcNow, "JournalBeta.txt", "beta", "beta"));
            Logging.Verbose = true;
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
        [DeploymentItem("eddi.json")]
        [DeploymentItem("loadout.json")]
        [DeploymentItem(@"x86\SQLite.Interop.dll", "x86")]
        [DeploymentItem(@"x64\SQLite.Interop.dll", "x64")]
        public void TestLoadoutParsing()
        {
            string data = System.IO.File.ReadAllText("loadout.json");

            List<Event> events = JournalMonitor.ParseJournalEntry(data);
            Assert.AreEqual(1, events.Count);
            ShipLoadoutEvent loadoutEvent = events[0] as ShipLoadoutEvent;
            Assert.AreEqual("Peppermint", loadoutEvent.shipname);
            Assert.AreEqual(18, loadoutEvent.compartments.Count);
            Assert.AreEqual(7, loadoutEvent.hardpoints.Count);

            ShipMonitor shipMonitor = new ShipMonitor();
            var privateObject = new PrivateObject(shipMonitor);
            object[] args = new object[]{loadoutEvent};
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

            // Log in
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:21Z"", ""event"":""LoadGame"", ""Commander"":""McDonald"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""FuelLevel"":2.000000, ""FuelCapacity"":2.000000, ""GameMode"":""Solo"", ""Credits"":1637243231, ""Loan"":0 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:24Z"", ""event"":""Location"", ""Docked"":true, ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StarPos"":[55.719,17.594,27.156], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""High Tech"", ""SystemGovernment"":""$government_Democracy;"", ""SystemGovernment_Localised"":""Democracy"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Body"":""Jameson Memorial"", ""BodyType"":""Station"", ""Factions"":[ { ""Name"":""Lori Jameson"", ""FactionState"":""None"", ""Government"":""Engineer"", ""Influence"":0.040307, ""Allegiance"":""Independent"" }, { ""Name"":""LTT 4487 Industry"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.191939, ""Allegiance"":""Federation"" }, { ""Name"":""The Pilots Federation"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.447217, ""Allegiance"":""Independent"" }, { ""Name"":""Future of Arro Naga"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.128599, ""Allegiance"":""Federation"" }, { ""Name"":""The Dark Wheel"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.092131, ""Allegiance"":""Independent"" }, { ""Name"":""Los Chupacabras"", ""FactionState"":""None"", ""Government"":""PrisonColony"", ""Influence"":0.099808, ""Allegiance"":""Independent"" } ], ""SystemFaction"":""The Pilots Federation"", ""FactionState"":""Boom"" }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:10:25Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.124878 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");
            Assert.AreEqual(100, sidewinder.health);

            // Purchase a Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:37Z"", ""event"":""ShipyardBuy"", ""ShipType"":""empire_courier"", ""ShipPrice"":2231423, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128132856 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:38Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:36Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":"""", ""ShipIdent"":"""", ""Rebuy"":9797719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:14:38Z"", ""event"":""ShipyardNew"", ""ShipType"":""empire_courier"", ""NewShipID"":902 }", shipMonitor);
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
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:16Z"", ""event"":""Loadout"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""Rebuy"":979771, ""Modules"":[ { ""Slot"":""SmallHardpoint1"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""Armour"", ""Item"":""SideWinder_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size1_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot01_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot02_Size2"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Sidewinder_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:25Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.120514 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");

            // Swap back to the Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:35Z"", ""event"":""ShipyardSwap"", ""ShipType"":""empire_courier"", ""ShipID"":902, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128666762 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:35Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:18:36Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":"""", ""ShipIdent"":"""", ""Rebuy"":9797719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
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
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:17:16Z"", ""event"":""Loadout"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""Rebuy"":979771, ""Modules"":[ { ""Slot"":""SmallHardpoint1"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""Armour"", ""Item"":""SideWinder_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size1_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot01_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot02_Size2"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Sidewinder_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:13Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.120514 }", shipMonitor);

            sidewinder = shipMonitor.GetShip(sidewinderId);
            Assert.AreEqual(sidewinder, shipMonitor.GetCurrentShip());
            Assert.AreEqual(sidewinder.model, "Sidewinder");

            // Swap back to the Courier
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:47Z"", ""event"":""ShipyardSwap"", ""ShipType"":""empire_courier"", ""ShipID"":902, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128666762 }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:48Z"", ""event"":""Cargo"", ""Inventory"":[ ] }", shipMonitor);
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:21:48Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":""Scunthorpe Bound"", ""ShipIdent"":""MC-24E"", ""Rebuy"":9798719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }", shipMonitor);
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
            SendEvents(@"{ ""timestamp"":""2017-04-24T08:27:51Z"", ""event"":""ShipyardSell"", ""ShipType"":""Empire_Courier"", ""SellShipID"":902, ""ShipPrice"":2008281, ""MarketID"":128666762 }", shipMonitor);
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
        [DeploymentItem("shipMonitor.json")]
        public void TestShipMonitorDeserialization()
        {
            // Read from our test item "shipMonitor.json"
            ShipMonitorConfiguration configuration = new ShipMonitorConfiguration();
            try
            {
                string data = System.IO.File.ReadAllText("shipMonitor.json");
                if (data != null)
                {
                    configuration = JsonConvert.DeserializeObject<ShipMonitorConfiguration>(data);
                }
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
        }

        [TestMethod]
        [DeploymentItem("shipMonitor.json")]
        public void TestShipMonitorDeserializationDoesntMutateStatics()
        {
            // Read from our test item "shipMonitor.json"
            ShipMonitorConfiguration configuration = new ShipMonitorConfiguration();
            try
            {
                string data = System.IO.File.ReadAllText("shipMonitor.json");
                if (data != null)
                {
                    configuration = JsonConvert.DeserializeObject<ShipMonitorConfiguration>(data);
                }
            }
            catch (Exception)
            {
                Assert.Fail("Failed to read ship configuration");
            }

            Assert.AreEqual("Multipurpose", Role.MultiPurpose.edname);
        }
    }
}
