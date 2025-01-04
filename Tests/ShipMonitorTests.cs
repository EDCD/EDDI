using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Tests.Properties;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class ShipMonitorTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestLoadoutParsingEmpireTrader()
        {
            var data = DeserializeJsonResource<string>(Resources.loadout_empire_trader);

            var events = JournalMonitor.ParseJournalEntry(data);
            Assert.AreEqual(1, events.Count);
            var loadoutEvent = events[0] as ShipLoadoutEvent;
            Assert.IsNotNull(loadoutEvent);
            Assert.AreEqual("Peppermint", loadoutEvent.shipname);
            Assert.AreEqual(18, loadoutEvent.compartments.Count);
            Assert.AreEqual(7, loadoutEvent.hardpoints.Count);

            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };
            var ship = shipMonitor.ParseShipLoadoutEvent( loadoutEvent );
            Assert.IsNotNull(ship);
            Assert.AreEqual( "Peppermint", ship.name );
            Assert.AreEqual( "VT-E23", ship.ident );
            Assert.AreEqual( 6, ship.LocalId );
            Assert.AreEqual( 16, ship.fueltankcapacity );
            Assert.AreEqual( 24, ship.fueltanktotalcapacity );
            Assert.AreEqual( 13943105, ship.hullvalue );
            Assert.AreEqual( 111211684, ship.modulesvalue );
            Assert.AreEqual( 6257742, ship.rebuy );
            Assert.AreEqual( 100, ship.health );
            Assert.AreEqual( false, ship.hot );
            Assert.AreEqual( 530.8M, ship.unladenmass );
            Assert.AreEqual( 64, ship.cargocapacity );
            Assert.AreEqual( 2, ship.hardpoints.Count( h => h.module.edname == "Hpt_MultiCannon_Gimbal_Large" ) );
            Assert.AreEqual( "Empire_Trader_Armour_Grade1", ship.bulkheads.edname );
            Assert.AreEqual( 4478720, ship.powerplant.price );
            Assert.AreEqual( 100, ship.powerplant.health );
            Assert.AreEqual( 5, ship.thrusters.modifiers.Count );
            Assert.AreEqual( 3, ship.thrusters.engineerlevel );
            Assert.AreEqual( 0, ship.thrusters.engineerquality );
            Assert.AreEqual( "Int_FuelScoop_Size7_Class5", ship.compartments[ 0 ].module.edname );
            Assert.AreEqual( "Fuel Scoop", ship.compartments[ 0 ].module.invariantName );

            // Test fuel calculations
            Assert.AreEqual( 1547.139526, ship.frameshiftdrive.GetFsdOptimalMass() );
            Assert.AreEqual( 5.199397M, ship.maxfuelperjump );
            // With zero fuel and zero cargo
            Assert.AreEqual( 15.654M, Math.Round( ship.JumpDetails( "next" ).distance, 3 ) );
            Assert.AreEqual( 34.403M, Math.Round (ship.JumpDetails( "max" ).distance, 3) );
            Assert.AreEqual( 15.654M, Math.Round( ship.JumpDetails( "total" ).distance, 3 ) );
            Assert.AreEqual( 168.413M, Math.Round(ship.JumpDetails( "full" ).distance, 3) );
            // With with max fuel and zero cargo
            Assert.AreEqual( 33.720M, Math.Round(ship.JumpDetails( "next", 16 ).distance), 3 );
            Assert.AreEqual( 34.403M, Math.Round( ship.JumpDetails( "max", 16 ).distance, 3 ) );
            Assert.AreEqual( 121.881M, Math.Round( ship.JumpDetails( "total", 16 ).distance, 3 ) );
            Assert.AreEqual( 168.413M, Math.Round( ship.JumpDetails( "full", 16 ).distance, 3 ) );
            // With with max fuel and max cargo
            Assert.AreEqual( 30.734M, Math.Round( ship.JumpDetails( "next", 16, 64 ).distance ), 3 );
            Assert.AreEqual( 30.734M, Math.Round( ship.JumpDetails( "max", 16, 64 ).distance, 3 ) );
            Assert.AreEqual( 108.867M, Math.Round( ship.JumpDetails( "total", 16, 64 ).distance, 3 ) );
            Assert.AreEqual( 150.442M, Math.Round( ship.JumpDetails( "full", 16, 64 ).distance, 3 ) );
        }

        [TestMethod]
        public void TestLoadoutParsingPythonNX ()
        {
            var data = DeserializeJsonResource<string>(Resources.loadout_python_nx);

            var events = JournalMonitor.ParseJournalEntry(data);
            Assert.AreEqual( 1, events.Count );
            var loadoutEvent = events[0] as ShipLoadoutEvent;
            Assert.IsNotNull( loadoutEvent );
            Assert.AreEqual( "", loadoutEvent.shipname );
            Assert.AreEqual( 16, loadoutEvent.compartments.Count );
            Assert.AreEqual( 12, loadoutEvent.hardpoints.Count );

            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };
            var ship = shipMonitor.ParseShipLoadoutEvent( loadoutEvent );
            Assert.IsNotNull( ship );
            Assert.AreEqual( null, ship.name );
            Assert.AreEqual( "FL-01P", ship.ident );
            Assert.AreEqual( 64, ship.LocalId );
            Assert.AreEqual( 16, ship.fueltankcapacity );
            Assert.AreEqual( 16, ship.fueltanktotalcapacity );
            Assert.AreEqual( 0, ship.hullvalue );
            Assert.AreEqual( 0, ship.modulesvalue );
            Assert.AreEqual( 0, ship.rebuy );
            Assert.AreEqual( 100, ship.health );
            Assert.AreEqual( false, ship.hot );
            Assert.AreEqual( 693.099976M, ship.unladenmass );
            Assert.AreEqual( 8, ship.cargocapacity );
            Assert.AreEqual( 4, ship.hardpoints.Count( h => h.module.edname == "Hpt_MultiCannon_Gimbal_Large" ) );
            Assert.AreEqual( "python_nx_armour_grade1", ship.bulkheads.edname );
            Assert.AreEqual( 0, ship.powerplant.price );
            Assert.AreEqual( 100, ship.powerplant.health );
            Assert.AreEqual( 0, ship.thrusters.modifiers.Count );
            Assert.AreEqual( 0, ship.thrusters.engineerlevel );
            Assert.AreEqual( 0, ship.thrusters.engineerquality );
            Assert.AreEqual( "Int_ShieldGenerator_Size6_Class3_Fast", ship.compartments[ 0 ].module.edname );
            Assert.AreEqual( "Bi-Weave Shield Generator", ship.compartments[ 0 ].module.invariantName );

            // Test fuel calculations
            Assert.AreEqual( 1175, ship.frameshiftdrive.GetFsdOptimalMass() );
            Assert.AreEqual( 5.2M, ship.maxfuelperjump );
            // With zero fuel and zero cargo
            Assert.AreEqual( 9.237M, Math.Round( ship.JumpDetails( "next" ).distance, 3 ) );
            Assert.AreEqual( 19.412M, Math.Round( ship.JumpDetails( "max" ).distance, 3 ) );
            Assert.AreEqual( 9.237M, Math.Round( ship.JumpDetails( "total" ).distance, 3 ) );
            Assert.AreEqual( 69.074M, Math.Round( ship.JumpDetails( "full" ).distance, 3 ) );
            // With with max fuel and zero cargo
            Assert.AreEqual( 19.094M, Math.Round( ship.JumpDetails( "next", 16 ).distance, 3 ) );
            Assert.AreEqual( 19.412M, Math.Round( ship.JumpDetails( "max", 16 ).distance, 3 ) );
            Assert.AreEqual( 69.074M, Math.Round( ship.JumpDetails( "total", 16 ).distance, 3 ) );
            Assert.AreEqual( 69.074M, Math.Round( ship.JumpDetails( "full", 16 ).distance, 3 ) );
            // With with max fuel and max cargo
            Assert.AreEqual( 18.881M, Math.Round( ship.JumpDetails( "next", 16, 8 ).distance, 3 ) );
            Assert.AreEqual( 19.192M, Math.Round( ship.JumpDetails( "max", 16, 8 ).distance, 3 ) );
            Assert.AreEqual( 68.291M, Math.Round( ship.JumpDetails( "total", 16, 8 ).distance, 3 ) );
            Assert.AreEqual( 68.291M, Math.Round( ship.JumpDetails( "full", 16, 8 ).distance, 3 ) );
        }

        [ TestMethod ]
        public void TestShipScenario1 ()
        {
            int sidewinderId = 901;
            int courierId = 902;

            // Start a ship monitor
            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };
            shipMonitor.shipyard.Clear();

            // Log in
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:10:21Z"", ""event"":""LoadGame"", ""Commander"":""McDonald"", ""Horizons"":true,""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""FuelLevel"":2.000000, ""FuelCapacity"":2.000000, ""GameMode"":""Solo"", ""Credits"":1637243231, ""Loan"":0 }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:10:24Z"", ""event"":""Location"", ""Docked"":true, ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StarPos"":[55.719,17.594,27.156], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""High Tech"", ""SystemGovernment"":""$government_Democracy;"", ""SystemGovernment_Localised"":""Democracy"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Body"":""Jameson Memorial"", ""BodyType"":""Station"", ""Factions"":[ { ""Name"":""Lori Jameson"", ""FactionState"":""None"", ""Government"":""Engineer"", ""Influence"":0.040307, ""Allegiance"":""Independent"" }, { ""Name"":""LTT 4487 Industry"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.191939, ""Allegiance"":""Federation"" }, { ""Name"":""The Pilots Federation"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.447217, ""Allegiance"":""Independent"" }, { ""Name"":""Future of Arro Naga"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.128599, ""Allegiance"":""Federation"" }, { ""Name"":""The Dark Wheel"", ""FactionState"":""Boom"", ""Government"":""Democracy"", ""Influence"":0.092131, ""Allegiance"":""Independent"" }, { ""Name"":""Los Chupacabras"", ""FactionState"":""None"", ""Government"":""PrisonColony"", ""Influence"":0.099808, ""Allegiance"":""Independent"" } ], ""SystemFaction"":""The Pilots Federation"", ""FactionState"":""Boom"" }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:10:25Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.124878 }",
                shipMonitor );

            var sidewinder = shipMonitor.GetShip( sidewinderId );
            Assert.AreEqual( sidewinder, shipMonitor.GetCurrentShip() );
            Assert.AreEqual( sidewinder.model, "Sidewinder" );
            Assert.AreEqual( 100, sidewinder.health );

            // Purchase a Courier
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:14:37Z"", ""event"":""ShipyardBuy"", ""ShipType"":""empire_courier"", ""ShipPrice"":2231423, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128132856 }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:14:38Z"", ""event"":""Cargo"", ""Inventory"":[ ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:14:40Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":"""", ""ShipIdent"":"""", ""HullHealth"":1, ""Hot"":false, ""UnladenMass"": 1200, ""MaxJumpRange"":20, ""Rebuy"":9797719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:14:42Z"", ""event"":""ShipyardNew"", ""ShipType"":""empire_courier"", ""NewShipID"":902 }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:14:48Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.122131 }",
                shipMonitor );

            sidewinder = shipMonitor.GetShip( sidewinderId );
            Assert.AreEqual( sidewinder.model, "Sidewinder" );

            var courier = shipMonitor.GetShip( courierId );
            Assert.AreEqual( courier, shipMonitor.GetCurrentShip() );
            Assert.AreEqual( courier.model, "Imperial Courier" );
            Assert.AreEqual( 100, courier.health );

            // Swap back to the SideWinder
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:17:15Z"", ""event"":""ShipyardSwap"", ""ShipType"":""sidewinder"", ""ShipID"":901, ""StoreOldShip"":""Empire_Courier"", ""StoreShipID"":902, ""MarketID"":128666762 }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:17:16Z"", ""event"":""Cargo"", ""Inventory"":[ ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:17:18Z"", ""event"":""Loadout"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""HullHealth"":1, ""Hot"":false, ""UnladenMass"": 1000, ""MaxJumpRange"":12, ""Rebuy"":979771, ""Modules"":[ { ""Slot"":""SmallHardpoint1"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""Armour"", ""Item"":""SideWinder_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size1_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot01_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot02_Size2"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Sidewinder_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:17:25Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.120514 }",
                shipMonitor );

            sidewinder = shipMonitor.GetShip( sidewinderId );
            Assert.AreEqual( sidewinder, shipMonitor.GetCurrentShip() );
            Assert.AreEqual( sidewinder.model, "Sidewinder" );

            // Swap back to the Courier
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:18:35Z"", ""event"":""ShipyardSwap"", ""ShipType"":""empire_courier"", ""ShipID"":902, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128666762 }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:18:36Z"", ""event"":""Cargo"", ""Inventory"":[ ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:18:38Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":"""", ""ShipIdent"":"""", ""HullHealth"":1, ""Hot"":false, ""UnladenMass"": 1200, ""MaxJumpRange"":20, ""Rebuy"":9797719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:18:45Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.119690 }",
                shipMonitor );

            courier = shipMonitor.GetShip( courierId );
            Assert.AreEqual( courier, shipMonitor.GetCurrentShip() );
            Assert.AreEqual( courier.model, "Imperial Courier" );

            // Name the Courier
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:19:55Z"", ""event"":""SetUserShipName"", ""Ship"":""empire_courier"", ""ShipID"":902, ""UserShipName"":""Scunthorpe Bound"", ""UserShipId"":""MC-24E"" }",
                shipMonitor );

            courier = shipMonitor.GetShip( courierId );
            Assert.AreEqual( courier, shipMonitor.GetCurrentShip() );
            Assert.AreEqual( courier.model, "Imperial Courier" );
            Assert.AreEqual( courier.name, "Scunthorpe Bound" );

            // Swap back to the SideWinder
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:21:03Z"", ""event"":""ShipyardSwap"", ""ShipType"":""sidewinder"", ""ShipID"":901, ""StoreOldShip"":""Empire_Courier"", ""StoreShipID"":902, ""MarketID"":128666762 }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:21:04Z"", ""event"":""Cargo"", ""Inventory"":[ ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:21:08Z"", ""event"":""Loadout"", ""Ship"":""SideWinder"", ""ShipID"":901, ""ShipName"":"""", ""ShipIdent"":"""", ""HullHealth"":1, ""Hot"":false, ""UnladenMass"": 1000, ""MaxJumpRange"":12, ""Rebuy"":979771, ""Modules"":[ { ""Slot"":""SmallHardpoint1"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""Hpt_PulseLaser_Gimbal_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5791 }, { ""Slot"":""Armour"", ""Item"":""SideWinder_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size1_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot01_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot02_Size2"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Sidewinder_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:21:13Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.120514 }",
                shipMonitor );

            sidewinder = shipMonitor.GetShip( sidewinderId );
            Assert.AreEqual( sidewinder, shipMonitor.GetCurrentShip() );
            Assert.AreEqual( sidewinder.model, "Sidewinder" );

            // Swap back to the Courier
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:21:47Z"", ""event"":""ShipyardSwap"", ""ShipType"":""empire_courier"", ""ShipID"":902, ""StoreOldShip"":""SideWinder"", ""StoreShipID"":901, ""MarketID"":128666762 }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:21:48Z"", ""event"":""Cargo"", ""Inventory"":[ ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:21:50Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":902, ""ShipName"":""Scunthorpe Bound"", ""ShipIdent"":""MC-24E"", ""HullHealth"": 1, ""Hot"":false, ""UnladenMass"": 1200, ""MaxJumpRange"":20, ""Rebuy"":9797719, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_PulseLaser_Fixed_Small"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1930 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_PowerPlant_Size4_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":17442 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":5502 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":453 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":3556 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1270 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size3_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":6197 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_CargoRack_Size2_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":2851 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_ShieldGenerator_Size2_Class1"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":1735 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_CargoRack_Size1_Class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_StellarBodyDiscoveryScanner_Standard"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":877 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }",
                shipMonitor );
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:21:57Z"", ""event"":""Docked"", ""MarketID"":128666762, ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""SystemAddress"":3932277478106, ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Boom"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High Tech"", ""DistFromStarLS"":325.117706 }",
                shipMonitor );

            courier = shipMonitor.GetShip( courierId );
            Assert.AreEqual( courier, shipMonitor.GetCurrentShip() );
            Assert.AreEqual( courier.model, "Imperial Courier" );
            Assert.AreEqual( courier.name, "Scunthorpe Bound" );
            Assert.AreEqual( "Int_CargoRack_Size2_Class1", courier.compartments[ 0 ].module.edname );
            Assert.AreEqual( "cargo rack", courier.compartments[ 0 ].module.invariantName.ToLowerInvariant() );

            // Sell the Sidewinder
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:27:51Z"", ""event"":""ShipyardSell"", ""ShipType"":""sidewinder"", ""SellShipID"":901, ""ShipPrice"":25272, ""MarketID"":128666762 }",
                shipMonitor );

            // Sell the Courier.  Note that this isn't strictly legal, as it involves selling our active ship, but we can get away with it in our test harness
            SendEvents( @"{ ""timestamp"":""2017-04-24T08:27:52Z"", ""event"":""ShipyardSell"", ""ShipType"":""Empire_Courier"", ""SellShipID"":902, ""ShipPrice"":2008281, ""MarketID"":128666762 }",
                shipMonitor );

            void SendEvents( string line, ShipMonitor monitor )
            {
                var events = JournalMonitor.ParseJournalEntry( line );
                foreach ( var @event in events )
                {
                    monitor.PreHandle( @event );
                }
            }
        }

        [TestMethod, DoNotParallelize]
        public void TestModuleSwappedEvent()
        {
            var line = @"{ ""timestamp"":""2018 - 06 - 29T02: 38:30Z"", ""event"":""ModuleSwap"", ""MarketID"":128132856, ""FromSlot"":""Slot06_Size3"", ""ToSlot"":""Slot07_Size3"", ""FromItem"":""$int_stellarbodydiscoveryscanner_advanced_name;"", ""FromItem_Localised"":""D - Scanner"", ""ToItem"":""Null"", ""Ship"":""krait_mkii"", ""ShipID"":81 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            var @event = (ModuleSwappedEvent)events[0];

            Assert.AreEqual(128132856, @event.marketId);
            Assert.AreEqual("Slot06_Size3", @event.fromslot);
            Assert.AreEqual("Slot07_Size3", @event.toslot);
            Assert.AreEqual("int_stellarbodydiscoveryscanner_advanced", @event.frommodule.edname.ToLowerInvariant());
            Assert.AreEqual("advanced discovery scanner", @event.frommodule.invariantName.ToLowerInvariant());
            Assert.IsNull(@event.tomodule);
            Assert.AreEqual("Krait Mk. II", @event.ship);
            Assert.AreEqual(81, @event.shipid);

            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };
            shipMonitor.shipyard.Clear();

            // Set up our ship
            Assert.IsNotNull(@event.shipid);
            var ship = new Ship { model = @event.ship, LocalId = (int)@event.shipid };
            ship.compartments.Add(new Compartment() { name = @event.fromslot, size = 3, module = @event.frommodule });
            ship.compartments.Add(new Compartment() { name = @event.toslot, size = 3, module = @event.tomodule });
            shipMonitor.RemoveShip( (int)@event.shipid );
            shipMonitor.AddShip( ship );

            // Test the event handler
            Assert.AreEqual(@event.frommodule, ship.compartments.FirstOrDefault(c => c.name == @event.fromslot)?.module);
            Assert.AreEqual(@event.tomodule, ship.compartments.FirstOrDefault(c => c.name == @event.toslot)?.module);
            shipMonitor.handleModuleSwappedEvent( @event );
            Assert.AreEqual(@event.frommodule, ship.compartments.FirstOrDefault(c => c.name == @event.toslot)?.module);
            Assert.AreEqual(@event.tomodule, ship.compartments.FirstOrDefault(c => c.name == @event.fromslot)?.module);
        }

        [TestMethod, DoNotParallelize]
        public void TestShipMonitorDeserialization()
        {
            // Read from our test item "shipMonitor.json"
            var configuration = new ShipMonitorConfiguration();
            try
            {
                configuration = DeserializeJsonResource<ShipMonitorConfiguration>(Resources.shipMonitor);
            }
            catch (Exception ex)
            {
                Logging.Warn( "Failed to read ship configuration", ex );
                Assert.Fail();
            }

            // Build a new shipyard
            var newShiplist = configuration.shipyard.OrderBy(s => s.model).ToList();

            // Start a ship monitor & set the new shipyard
            var shipMonitor = new ShipMonitor
            {
                updatedAt = DateTime.MinValue, 
                shipyard = new ObservableCollection<Ship>( newShiplist )
            };

            shipMonitor.SetCurrentShip(configuration.currentshipid);
            Assert.AreEqual(81, shipMonitor.GetCurrentShip().LocalId);

            var ship1 = shipMonitor.GetShip(0);
            var ship2 = shipMonitor.GetShip(81);

            Assert.IsNotNull(ship1);
            Assert.AreEqual("Cobra Mk. III", ship1.model);
            Assert.AreEqual(0, ship1.LocalId);
            Assert.AreEqual("The Dynamo", ship1.name);
            Assert.AreEqual("La Rochelle", ship1.starsystem);
            Assert.AreEqual( "J9J-9KT", ship1.station);
            Assert.AreEqual(8605684, ship1.value);

            Assert.IsNotNull(ship2);
            Assert.AreEqual("Krait Mk. II", ship2.model);
            Assert.AreEqual(81, ship2.LocalId);
            Assert.AreEqual("The Impact Kraiter", ship2.name);
            Assert.AreEqual(16, ship2.cargocapacity);
            Assert.AreEqual(8, ship2.compartments.Count);
            Assert.AreEqual("Slot01_Size6", ship2.compartments[0].name);
            Assert.AreEqual(6, ship2.compartments[0].size);
            Assert.IsNotNull(ship2.compartments[0].module);
            Assert.AreEqual("Int_ShieldGenerator_Size6_Class3_Fast", ship2.compartments[0].module.edname);
            Assert.AreEqual("Bi-Weave Shield Generator", ship2.compartments[0].module.invariantName);
            Assert.AreEqual("SRV", ship2.launchbays[0].type);
            Assert.AreEqual(2, ship2.launchbays[0].vehicles.Count);
            Assert.AreEqual("TestBuggy", ship2.launchbays[0].vehicles[0].vehicleDefinition);
            Assert.AreEqual("Starter", ship2.launchbays[0].vehicles[0].loadoutDescription);
            Assert.AreEqual("dual plasma repeaters", ship2.launchbays[0].vehicles[0].localizedDescription);
        }

        [TestMethod]
        public void TestShipMonitorDeserializationDoesntMutateStatics()
        {
            // Read from our test item "shipMonitor.json"
            try
            {
                DeserializeJsonResource<ShipMonitorConfiguration>(Resources.shipMonitor);
            }
            catch (Exception ex)
            {
                Logging.Warn( "Failed to read ship configuration", ex );
                Assert.Fail();
            }

            Assert.AreEqual("Multipurpose", Role.MultiPurpose.edname);
        }

        [TestMethod]
        public void TestShipMonitorDeserializationMatchesSerialization()
        {
            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };
            shipMonitor.shipyard.Clear();

            var data = DeserializeJsonResource<string>(Resources.loadout_empire_trader);
            var events = JournalMonitor.ParseJournalEntry(data);
            var loadoutEvent = events[0] as ShipLoadoutEvent;
            shipMonitor.handleShipLoadoutEvent( loadoutEvent );

            var originalShip = shipMonitor.GetCurrentShip();

            if (originalShip != null)
            {
                var originalShipString = JsonConvert.SerializeObject(originalShip);
                var deserializedShip = JsonConvert.DeserializeObject<Ship>(originalShipString);
                if (deserializedShip != null)
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

        [TestMethod, DoNotParallelize]
        public void TestFighterLoadoutEvent()
        {
            var data = DeserializeJsonResource<string>(Resources.loadout_empire_trader);
            var events = JournalMonitor.ParseJournalEntry(data);
            var loadoutEvent = events[0] as ShipLoadoutEvent;

            var data2 = DeserializeJsonResource<string>(Resources.fighterLoadout);
            events = JournalMonitor.ParseJournalEntry(data2);
            var fighterLoadoutEvent = events[0] as ShipLoadoutEvent;

            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };
            shipMonitor.shipyard.Clear();

            shipMonitor.handleShipLoadoutEvent( loadoutEvent );
            shipMonitor.handleShipLoadoutEvent( fighterLoadoutEvent );

            var currentShip = shipMonitor.GetCurrentShip();

            // After a loadout event generated from a fighter, 
            // we still want to track the ship we launched from as our current ship.
            Assert.IsNotNull(loadoutEvent);
            Assert.IsNotNull(fighterLoadoutEvent);
            Assert.AreEqual(loadoutEvent.shipid, currentShip?.LocalId);
            Assert.AreNotEqual(fighterLoadoutEvent.shipid, currentShip?.LocalId);
        }

        [TestMethod]
        public void TestJournalModulePurchasedHandlingMinimalShip()
        {
            var line = "{ \"timestamp\":\"2018-12-25T22:55:11Z\", \"event\":\"ModuleBuy\", \"Slot\":\"Military01\", \"BuyItem\":\"$int_guardianshieldreinforcement_size5_class2_name;\", \"BuyItem_Localised\":\"Guardian Shield Reinforcement\", \"MarketID\":128666762, \"BuyPrice\":873402, \"Ship\":\"federation_corvette\", \"ShipID\":119 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (ModulePurchasedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(ModulePurchasedEvent));

            Assert.IsNotNull(@event.shipid);
            Assert.AreEqual(119, @event.shipid);
            Assert.IsNotNull(@event.slot);
            Assert.IsNotNull(@event.buymodule);

            var ship = ShipDefinitions.FromModel(@event.ship);
            ship.LocalId = @event.shipid;

            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };
            shipMonitor.AddModule( ship, @event.slot, @event.buymodule );
            
            foreach ( var compartment in ship.compartments)
            {
                if (compartment.name == "Military01")
                {
                    Assert.AreEqual("Guardian Shield Reinforcement", compartment.module?.invariantName);
                }
            }
        }

        [TestMethod]
        public void TestJournalModuleSoldHandlingMinimalShip()
        {
            var line = "{ \"timestamp\":\"2018-12-25T22:55:11Z\", \"event\":\"ModuleBuy\", \"Slot\":\"Slot01_Size7\", \"BuyItem\":\"$int_guardianshieldreinforcement_size5_class2_name;\", \"BuyItem_Localised\":\"Guardian Shield Reinforcement\", \"MarketID\":128666762, \"BuyPrice\":873402, \"Ship\":\"federation_corvette\", \"ShipID\":119 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            var @event = (ModulePurchasedEvent)events[0];

            var ship = ShipDefinitions.FromModel(@event.ship);
            Assert.IsNotNull(@event.shipid);
            ship.LocalId = @event.shipid;
            var slot = @event.slot;
            var module = @event.buymodule;

            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };
            shipMonitor.AddModule( ship, slot, module );

            // now sell the module
            shipMonitor.RemoveModule( ship, slot, null );
            foreach (var compartment in ship.compartments)
            {
                if (compartment.name == "Military01")
                {
                    Assert.IsNull(compartment.module);
                }
            }
        }

        [TestMethod]
        public void TestShipRefuelledEvent_Scooping()
        {
            var line1 = "{ \"timestamp\":\"2019 - 07 - 21T16: 28:35Z\", \"event\":\"FuelScoop\", \"Scooped\":5.001066, \"Total\":31.552881 }";
            var line2 = "{ \"timestamp\":\"2019 - 07 - 21T16: 28:35Z\", \"event\":\"FuelScoop\", \"Scooped\":0.447121, \"Total\":32.000000 }";

            var event1 = JournalMonitor.ParseJournalEntry(line1)[0] as ShipRefuelledEvent;
            var event2 = JournalMonitor.ParseJournalEntry(line2)[0] as ShipRefuelledEvent;

            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };

            // Set up our ship
            var ship = new Ship { LocalId = 9999, fueltank = new Module() {@class = 5} };
            shipMonitor.RemoveShip( 9999 );
            shipMonitor.AddShip( ship );
            shipMonitor.currentShipId = 9999;

            // Evaluate the results of our events
            Assert.IsNotNull(event1);
            shipMonitor.PreHandle(event1);
            Assert.AreEqual(5.001066M, event1.amount);
            Assert.AreEqual(31.552881M, event1.total);
            Assert.IsFalse(event1.full);
            Assert.AreEqual("Ship refuelled", event1.type);

            Assert.IsNotNull(event2);
            shipMonitor.PreHandle(event2);
            Assert.AreEqual(0.447121M, event2.amount);
            Assert.AreEqual(32.000000M, event2.total);
            Assert.IsTrue(event2.full);
        }

        [TestMethod, DoNotParallelize]
        public void TestShipJumpedEvent()
        {
            // Set up our `Ship monitor`
            var shipMonitor = new ShipMonitor { updatedAt = DateTime.MinValue };

            // Set up our ship
            var ship = new Ship
            {
                LocalId = 9999,
                StoredLocation = new Ship.Location( DeserializeJsonResource<StarSystem>( Resources.sqlStarSystem6 ), "Furukawa Enterprise", 3534391808 )
            };
            shipMonitor.RemoveShip(9999);
            shipMonitor.AddShip(ship);

            // Set up our event
            var line = @"{ ""timestamp"":""2019-06-30T05:38:53Z"", ""event"":""FSDJump"", ""StarSystem"":""Ogmar"", ""SystemAddress"":84180519395914, ""StarPos"":[-9534.00000,-905.28125,19802.03125], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""High Tech"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_Confederacy;"", ""SystemGovernment_Localised"":""Confederacy"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":133000, ""Body"":""Ogmar A"", ""BodyID"":1, ""BodyType"":""Star"", ""JumpDist"":8.625, ""FuelUsed"":0.151982, ""FuelLevel"":31.695932, ""Factions"":[ { ""Name"":""Jaques"", ""FactionState"":""Election"", ""Government"":""Cooperative"", ""Influence"":0.104895, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand1;"", ""Happiness_Localised"":""Elated"", ""MyReputation"":100.000000, ""ActiveStates"":[ { ""State"":""Election"" } ] }, { ""Name"":""Colonia Research Department"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.078921, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":21.639999, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""Pilots' Federation Local Branch"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.000000, ""Allegiance"":""PilotsFederation"", ""Happiness"":"""", ""MyReputation"":100.000000 }, { ""Name"":""Colonia Mining Enterprise"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.052947, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000 }, { ""Name"":""Colonia Co-operative"", ""FactionState"":""Election"", ""Government"":""Cooperative"", ""Influence"":0.104895, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":71.470001, ""PendingStates"":[ { ""State"":""Expansion"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Outbreak"" }, { ""State"":""Election"" } ] }, { ""Name"":""Colonia Agricultural Co-operative"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.076923, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":6.640000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""GalCop Colonial Defence Commission"", ""FactionState"":""Boom"", ""Government"":""Confederacy"", ""Influence"":0.449550, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000, ""ActiveStates"":[ { ""State"":""Boom"" } ] }, { ""Name"":""Colonia Tech Combine"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.090909, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""Milanov's Reavers"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.040959, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] } ], ""SystemFaction"":{ ""Name"":""GalCop Colonial Defence Commission"", ""FactionState"":""Boom"" }, ""Conflicts"":[ { ""WarType"":""election"", ""Status"":""active"", ""Faction1"":{ ""Name"":""Jaques"", ""Stake"":""Crockett Gateway"", ""WonDays"":1 }, ""Faction2"":{ ""Name"":""Colonia Co-operative"", ""Stake"":"""", ""WonDays"":2 } } ] }";
            var events = JournalMonitor.ParseJournalEntry(line);
            var @event = (JumpedEvent)events[0];
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
            // Set up our `Ship monitor`
            var shipMonitor = new ShipMonitor
            {
                updatedAt = DateTime.MinValue,
                currentShipId = 9999 // Set up our `currentShipId` property with a value of "9999"
            };

            // Set up our event (which reports that we are in an SRV with a ship id of "9998")
            var line = @"{ ""timestamp"":""2020-07-20T15:40:45Z"", ""event"":""LoadGame"", ""FID"":""F0000000"", ""Commander"":""TestCommander"", ""Horizons"":true, ""Ship"":""TestBuggy"", ""Ship_Localised"":""SRV Scarab"", ""ShipID"":9998, ""ShipName"":"""", ""ShipIdent"":"""", ""FuelLevel"":0.000000, ""FuelCapacity"":0.000000, ""GameMode"":""Group"", ""Group"":""Children of Raxxla"", ""Credits"":5065687467, ""Loan"":0 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            var @event = (CommanderContinuedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(CommanderContinuedEvent));

            // Test the result to verify that the ship monitor's `currentShipId` property remains "9999" rather than changing to "9998"
            shipMonitor.PreHandle(@event);
            Assert.AreEqual("TestBuggy", @event.shipEDModel);
            Assert.AreEqual(9999, shipMonitor.currentShipId, @"Because the ""ship"" reported by the event is an SRV, the `currentShipId` property of the ship monitor should be unchanged");

            // Re-test the event, except exchange "TestBuggy" for "SRV" in the `shipEDModel` property
            // Verify once again that the ship monitor's `currentShipId` property remains "9999" rather than changing to "9998"
            line = @"{ ""timestamp"":""2020-07-20T15:40:45Z"", ""event"":""LoadGame"", ""FID"":""F0000000"", ""Commander"":""TestCommander"", ""Horizons"":true, ""Ship"":""SRV"", ""Ship_Localised"":""SRV Scarab"", ""ShipID"":9998, ""ShipName"":"""", ""ShipIdent"":"""", ""FuelLevel"":0.000000, ""FuelCapacity"":0.000000, ""GameMode"":""Group"", ""Group"":""Children of Raxxla"", ""Credits"":5065687467, ""Loan"":0 }";
            events = JournalMonitor.ParseJournalEntry(line);
            @event = (CommanderContinuedEvent)events[0];
            shipMonitor.PreHandle(@event);
            Assert.AreEqual("SRV", @event.shipEDModel);
            Assert.AreEqual(9999, shipMonitor.currentShipId, @"Because the ""ship"" reported by the event is an SRV, the `currentShipId` property of the ship monitor should be unchanged");
        }

        [ TestMethod ]
        public void TestShipRebootedEvent ()
        {
            // Set up our `Ship monitor`
            var shipMonitor = new ShipMonitor
            {
                updatedAt = DateTime.MinValue,
                currentShipId = 9999 // Set up our `currentShipId` property with a value of "9999"
            };

            // Set up our Loadout event
            var line = @"{ ""timestamp"":""2024-08-20T01:22:19Z"", ""event"":""Loadout"", ""Ship"":""viper_mkiv"", ""ShipID"":949, ""ShipName"":""Aisling guard"", ""ShipIdent"":""HN-74"", ""HullValue"":437931, ""ModulesValue"":10685968, ""HullHealth"":0.058691, ""Hot"":true, ""UnladenMass"":304.600006, ""CargoCapacity"":6, ""MaxJumpRange"":25.832956, ""FuelCapacity"":{ ""Main"":16.000000, ""Reserve"":0.460000 }, ""Rebuy"":556196, ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""hpt_beamlaser_gimbal_medium"", ""On"":true, ""Priority"":2, ""Health"":0.000000, ""Value"":500600 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""hpt_beamlaser_gimbal_medium"", ""On"":true, ""Priority"":3, ""Health"":0.000000, ""Value"":500600 }, { ""Slot"":""SmallHardpoint1"", ""Item"":""hpt_basicmissilerack_fixed_small"", ""On"":true, ""Priority"":3, ""AmmoInClip"":6, ""AmmoInHopper"":6, ""Health"":0.491816, ""Value"":70785 }, { ""Slot"":""SmallHardpoint2"", ""Item"":""hpt_basicmissilerack_fixed_small"", ""On"":true, ""Priority"":2, ""AmmoInClip"":6, ""AmmoInHopper"":6, ""Health"":0.614118, ""Value"":72600 }, { ""Slot"":""TinyHardpoint1"", ""Item"":""hpt_electroniccountermeasure_tiny"", ""On"":true, ""Priority"":0, ""Health"":0.000000, ""Value"":12188 }, { ""Slot"":""TinyHardpoint2"", ""Item"":""hpt_cloudscanner_size0_class4"", ""On"":false, ""Priority"":0, ""Health"":0.083574, ""Value"":356556 }, { ""Slot"":""Armour"", ""Item"":""viper_mkiv_armour_reactive"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":1032203 }, { ""Slot"":""PaintJob"", ""Item"":""paintjob_viper_mkiv_tactical_blue"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""PowerPlant"", ""Item"":""int_powerplant_size4_class5"", ""On"":true, ""Priority"":1, ""Health"":0.526949, ""Value"":1441233, ""Engineering"":{ ""Engineer"":""Felicity Farseer"", ""EngineerID"":300100, ""BlueprintID"":128673765, ""BlueprintName"":""PowerPlant_Boosted"", ""Level"":1, ""Quality"":1.000000, ""Modifiers"":[ { ""Label"":""Integrity"", ""Value"":83.599998, ""OriginalValue"":88.000000, ""LessIsGood"":0 }, { ""Label"":""PowerCapacity"", ""Value"":17.472000, ""OriginalValue"":15.600000, ""LessIsGood"":0 }, { ""Label"":""HeatEfficiency"", ""Value"":0.420000, ""OriginalValue"":0.400000, ""LessIsGood"":1 } ] } }, { ""Slot"":""MainEngines"", ""Item"":""int_engine_size4_class5"", ""On"":true, ""Priority"":0, ""Health"":0.000000, ""Value"":1610080, ""Engineering"":{ ""Engineer"":""Felicity Farseer"", ""EngineerID"":300100, ""BlueprintID"":128673656, ""BlueprintName"":""Engine_Dirty"", ""Level"":2, ""Quality"":1.000000, ""Modifiers"":[ { ""Label"":""Integrity"", ""Value"":82.720001, ""OriginalValue"":88.000000, ""LessIsGood"":0 }, { ""Label"":""PowerDraw"", ""Value"":5.215200, ""OriginalValue"":4.920000, ""LessIsGood"":1 }, { ""Label"":""EngineOptimalMass"", ""Value"":399.000000, ""OriginalValue"":420.000000, ""LessIsGood"":0 }, { ""Label"":""EngineOptPerformance"", ""Value"":119.000008, ""OriginalValue"":100.000000, ""LessIsGood"":0 }, { ""Label"":""EngineHeatRate"", ""Value"":1.690000, ""OriginalValue"":1.300000, ""LessIsGood"":1 } ] } }, { ""Slot"":""FrameShiftDrive"", ""Item"":""int_hyperdrive_overcharge_size4_class5"", ""On"":true, ""Priority"":0, ""Health"":0.666094, ""Value"":1883794, ""Engineering"":{ ""Engineer"":""Felicity Farseer"", ""EngineerID"":300100, ""BlueprintID"":128673691, ""BlueprintName"":""FSD_LongRange"", ""Level"":2, ""Quality"":0.906000, ""Modifiers"":[ { ""Label"":""Mass"", ""Value"":11.500000, ""OriginalValue"":10.000000, ""LessIsGood"":1 }, { ""Label"":""Integrity"", ""Value"":94.000000, ""OriginalValue"":100.000000, ""LessIsGood"":0 }, { ""Label"":""PowerDraw"", ""Value"":0.477000, ""OriginalValue"":0.450000, ""LessIsGood"":1 }, { ""Label"":""FSDOptimalMass"", ""Value"":725.750977, ""OriginalValue"":585.000000, ""LessIsGood"":0 } ] } }, { ""Slot"":""LifeSupport"", ""Item"":""int_lifesupport_size2_class5"", ""On"":true, ""Priority"":3, ""Health"":0.977148, ""Value"":56547 }, { ""Slot"":""PowerDistributor"", ""Item"":""int_powerdistributor_size3_class5"", ""On"":true, ""Priority"":1, ""Health"":0.914809, ""Value"":158331 }, { ""Slot"":""Radar"", ""Item"":""int_sensors_size3_class5"", ""On"":true, ""Priority"":2, ""Health"":0.837076, ""Value"":158331, ""Engineering"":{ ""Engineer"":""Felicity Farseer"", ""EngineerID"":300100, ""BlueprintID"":128740133, ""BlueprintName"":""Sensor_LongRange"", ""Level"":2, ""Quality"":1.000000, ""Modifiers"":[ { ""Label"":""Mass"", ""Value"":7.000000, ""OriginalValue"":5.000000, ""LessIsGood"":1 }, { ""Label"":""SensorTargetScanAngle"", ""Value"":25.500000, ""OriginalValue"":30.000000, ""LessIsGood"":0 }, { ""Label"":""Range"", ""Value"":8424.000000, ""OriginalValue"":6480.000000, ""LessIsGood"":0 } ] } }, { ""Slot"":""FuelTank"", ""Item"":""int_fueltank_size4_class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""Decal2"", ""Item"":""decal_powerplay_aislingduval"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""Decal3"", ""Item"":""decal_powerplay_aislingduval"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""Slot01_Size4"", ""Item"":""int_shieldgenerator_size4_class5_strong"", ""On"":true, ""Priority"":1, ""Health"":0.825511, ""Value"":2415120 }, { ""Slot"":""Slot02_Size4"", ""Item"":""int_hullreinforcement_size4_class2"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":195000 }, { ""Slot"":""Slot03_Size3"", ""Item"":""int_hullreinforcement_size3_class2"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":84000 }, { ""Slot"":""Slot04_Size2"", ""Item"":""int_modulereinforcement_size2_class2"", ""On"":true, ""Priority"":1, ""Health"":0.000000, ""Value"":36000 }, { ""Slot"":""Slot05_Size2"", ""Item"":""int_buggybay_size2_class1"", ""On"":false, ""Priority"":0, ""Health"":1.000000, ""Value"":18000 }, { ""Slot"":""Slot06_Size1"", ""Item"":""int_cargorack_size1_class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""Slot07_Size1"", ""Item"":""int_cargorack_size1_class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""Slot08_Size1"", ""Item"":""int_cargorack_size1_class1"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""Military01"", ""Item"":""int_hullreinforcement_size3_class2"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":84000 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""int_planetapproachsuite_advanced"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""VesselVoice"", ""Item"":""voicepack_amelie"", ""On"":true, ""Priority"":1, ""Health"":1.000000 }, { ""Slot"":""ShipCockpit"", ""Item"":""viper_mkiv_cockpit"", ""On"":true, ""Priority"":1, ""Health"":0.856739 }, { ""Slot"":""CargoHatch"", ""Item"":""modularcargobaydoor"", ""On"":false, ""Priority"":4, ""Health"":0.359405 } ] }";
            var events = JournalMonitor.ParseJournalEntry( line );
            var loadoutEvent = (ShipLoadoutEvent)events[ 0 ];
            Assert.IsNotNull( loadoutEvent );
            Assert.IsInstanceOfType( loadoutEvent, typeof( ShipLoadoutEvent ) );
            shipMonitor.PreHandle( loadoutEvent );

            var ship = shipMonitor.GetCurrentShip();
            Assert.AreEqual( 949, ship.LocalId );
            Assert.AreEqual( 0, ship.thrusters.health );
            Assert.AreEqual( 53, ship.powerplant.health );

            // Set up our ShipRebooted event
            line = @"{ ""timestamp"":""2024-08-20T01:24:48Z"", ""event"":""RebootRepair"", ""Modules"":[ ""MainEngines"", ""MediumHardpoint1"", ""MediumHardpoint2"", ""TinyHardpoint1"", ""Slot04_Size2"" ] }";
            events = JournalMonitor.ParseJournalEntry( line );
            var shipRebootedEvent = (ShipRebootedEvent)events[ 0 ];
            Assert.IsNotNull( shipRebootedEvent );
            Assert.IsInstanceOfType<ShipRebootedEvent>( shipRebootedEvent );
            shipMonitor.PreHandle( shipRebootedEvent );

            Assert.AreEqual( 5, shipRebootedEvent.compartments.Count );
            Assert.IsTrue( shipRebootedEvent.compartments.Contains( "MainEngines" ) );
            Assert.IsTrue( shipRebootedEvent.compartments.Contains( "MediumHardpoint1" ) );
            Assert.IsTrue( shipRebootedEvent.compartments.Contains( "MediumHardpoint2" ) );
            Assert.IsTrue( shipRebootedEvent.compartments.Contains( "TinyHardpoint1" ) );
            Assert.IsTrue( shipRebootedEvent.compartments.Contains( "Slot04_Size2" ) );

            ship = shipMonitor.GetCurrentShip();
            Assert.AreEqual( 949, ship.LocalId );
            Assert.AreEqual( 1, ship.thrusters.health );
            Assert.AreEqual( 53, ship.powerplant.health );
        }
    }
}
