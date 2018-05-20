using System;
using System.Collections.Generic;
using Eddi;
using EddiCargoMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rollbar;

namespace UnitTests
{
    [TestClass]
    public class CargoMonitorTests
    {
        CargoMonitor cargoMonitor = new CargoMonitor();
        Cargo cargo;
        string line;
        List<Event> events;

        [TestInitialize]
        private void StartTestCargoMonitor()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
            
            // Set ourselves as in beta to stop sending data to remote systems
            EDDI.Instance.eventHandler(new FileHeaderEvent(DateTime.Now, "JournalBeta.txt", "beta", "beta"));
        }

        [TestMethod]
        public void TestCargoConfig()
        {
            string cargoConfigJson = @"{
	            ""cargo"": [{
		            ""edname"": ""DamagedEscapePod"",
		            ""stolen"": 0,
		            ""haulage"": 0,
		            ""owned"": 4,
		            ""need"": 0,
		            ""total"": 4,
		            ""ejected"": 0,
		            ""price"": 11912,
		            ""haulageamounts"": []
	            },
	            {
		            ""edname"": ""USSCargoBlackBox"",
		            ""stolen"": 4,
		            ""haulage"": 0,
		            ""owned"": 0,
		            ""need"": 0,
		            ""total"": 4,
		            ""ejected"": 0,
		            ""price"": 6995,
		            ""haulageamounts"": []
	            },
	            {
		            ""edname"": ""Drones"",
		            ""stolen"": 0,
		            ""haulage"": 0,
		            ""owned"": 21,
		            ""need"": 0,
		            ""total"": 21,
		            ""ejected"": 0,
		            ""price"": 101,
		            ""haulageamounts"": []
	            }],
	            ""cargocarried"": 29
            }";
            CargoMonitorConfiguration config = CargoMonitorConfiguration.FromJsonString(cargoConfigJson);

            Assert.AreEqual(3, config.cargo.Count);
            cargo = config.cargo.ToList().FirstOrDefault(c => c.edname == "DamagedEscapePod");
            Assert.AreEqual("Damaged Escape Pod", cargo.commodityDef.invariantName);
            Assert.AreEqual(4, cargo.total);
            Assert.AreEqual(4, cargo.owned);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(0, cargo.haulage);
        }

        [TestMethod]
        public void TestCargoEventsScenario()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());

            // CargoEvent
            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""damagedescapepod"", ""Name_Localised"": ""Damaged Escape Pod"", ""Count"": 4, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCargoInventoryEvent((CargoInventoryEvent)events[0]);
            Assert.AreEqual(3, cargoMonitor.inventory.Count);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(21, cargo.total);
            Assert.AreEqual(21, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoCollectedEvent
            line = @"{""timestamp"":""2016-06-10T14:32:03Z"",""event"":""CollectCargo"",""Type"":""agriculturalmedicines"",""Stolen"":true}";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCommodityCollectedEvent((CommodityCollectedEvent)events[0]);
            Assert.AreEqual(4, cargoMonitor.inventory.Count);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "AgriculturalMedicines");
            Assert.AreEqual("Agri-Medicines", cargo.invariantName);
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.stolen);
            Assert.AreEqual(0, cargo.owned + cargo.need + cargo.haulage);

            // CargoEjectedEvent
            line = @"{""timestamp"": ""2016-06-10T14:32:03Z"", ""event"": ""EjectCargo"", ""Type"":""drones"", ""Count"":2, ""Abandoned"":true}";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCommodityEjectedEvent((CommodityEjectedEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual(19, cargo.total);
            Assert.AreEqual(19, cargo.owned);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(0, cargo.haulage);

            // CargoPurchasedEvent
            line = @"{ ""timestamp"":""2018-04-07T16:29:39Z"", ""event"":""MarketBuy"", ""MarketID"":3224801280, ""Type"":""coffee"", ""Count"":1, ""BuyPrice"":1198, ""TotalCost"":1198 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCommodityPurchasedEvent((CommodityPurchasedEvent)events[0]);

            Assert.AreEqual(5, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Coffee");
            Assert.AreEqual("Coffee", cargo.invariantName);
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoRefinedEvent
            line = @"{ ""timestamp"":""2016-09-30T18:00:22Z"", ""event"":""MiningRefined"", ""Type"":""$hydrogenperoxide_name;"", ""Type_Localised"":""Hydrogen Peroxide"" }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCommodityRefinedEvent((CommodityRefinedEvent)events[0]);

            Assert.AreEqual(6, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "HydrogenPeroxide");
            Assert.AreEqual("Hydrogen Peroxide", cargo.invariantName);
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoSoldEvent
            line = @"{ ""timestamp"":""2018-04-07T16:29:44Z"", ""event"":""MarketSell"", ""MarketID"":3224801280, ""Type"":""coffee"", ""Count"":1, ""SellPrice"":1138, ""TotalSale"":1138, ""AvgPricePaid"":1198 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCommoditySoldEvent((CommoditySoldEvent)events[0]);

            Assert.AreEqual(5, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Coffee");
            Assert.IsNull(cargo);

            // CargoPowerCommodityObtainedEvent
            line = @"{ ""timestamp"":""2016-12-02T16:10:26Z"", ""event"":""PowerplayCollect"", ""Power"":""Aisling Duval"", ""Type"":""$aislingmediamaterials_name;"", ""Type_Localised"":""Aisling Media Materials"", ""Count"":3 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handlePowerCommodityObtainedEvent((PowerCommodityObtainedEvent)events[0]);

            Assert.AreEqual(6, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "aislingmediamaterials");
            Assert.AreEqual("Aisling Media Materials", cargo.invariantName);
            Assert.AreEqual(3, cargo.total);
            Assert.AreEqual(3, cargo.owned);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(0, cargo.haulage);

            // CargoPowerCommodityDeliveredEvent
            line = @"{ ""timestamp"":""2016-12-02T16:10:26Z"", ""event"":""PowerplayDeliver"", ""Power"":""Aisling Duval"", ""Type"":""$aislingmediamaterials_name;"", ""Type_Localised"":""Aisling Media Materials"", ""Count"":3 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handlePowerCommodityDeliveredEvent((PowerCommodityDeliveredEvent)events[0]);

            Assert.AreEqual(5, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "aislingmediamaterials");
            Assert.IsNull(cargo);
        }

        [TestMethod]
        public void TestCargoLimpetScenario()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());

            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""damagedescapepod"", ""Name_Localised"": ""Damaged Escape Pod"", ""Count"": 4, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCargoInventoryEvent((CargoInventoryEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(21, cargo.total);
            Assert.AreEqual(21, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoLimpetPurchasedEvent
            line = @"{ ""timestamp"":""2016-09-21T06:53:53Z"", ""event"":""BuyDrones"", ""Type"":""Drones"", ""Count"":19, ""BuyPrice"":101, ""TotalCost"":1919 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleLimpetPurchasedEvent((LimpetPurchasedEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(40, cargo.total);
            Assert.AreEqual(40, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoLimpetSoldEvent
            line = @"{ ""timestamp"":""2016-09-24T00:03:25Z"", ""event"":""SellDrones"", ""Type"":""Drones"", ""Count"":8, ""SellPrice"":101, ""TotalSale"":808 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleLimpetSoldEvent((LimpetSoldEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(32, cargo.total);
            Assert.AreEqual(32, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoLimpetLaunchedEvent
            line = @"{ ""timestamp"":""2018-04-07T20:05:07Z"", ""event"":""LaunchDrone"", ""Type"":""Collection"" }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleLimpetLaunchedEvent();

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(31, cargo.total);
            Assert.AreEqual(31, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);
        }

        [TestMethod]
        public void TestCargoMissionScenario()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());

            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""damagedescapepod"", ""Name_Localised"": ""Damaged Escape Pod"", ""Count"": 4, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCargoInventoryEvent((CargoInventoryEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(21, cargo.total);
            Assert.AreEqual(21, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoMissionAcceptedEvent - Check to see if this is a cargo mission and update our inventory accordingly
            line = @"{ ""timestamp"": ""2018-05-05T19:42:20Z"", ""event"": ""MissionAccepted"", ""Faction"": ""Elite Knights"", ""Name"": ""Mission_Salvage_Planet"", ""LocalisedName"": ""Salvage 3 Structural Regulators"", ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 3, ""DestinationSystem"": ""Merope"", ""Expiry"": ""2018-05-12T15:20:27Z"", ""Wing"": false, ""Influence"": ""Med"", ""Reputation"": ""Med"", ""Reward"": 557296, ""MissionID"": 375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            line = @"{ ""timestamp"": ""2018-05-05T19:42:20Z"", ""event"": ""MissionAccepted"", ""Faction"": ""Merope Expeditionary Fleet"", ""Name"": ""Mission_Salvage_Planet"", ""LocalisedName"": ""Salvage 4 Structural Regulators"", ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 4, ""DestinationSystem"": ""HIP 17692"", ""Expiry"": ""2018-05-12T15:20:27Z"", ""Wing"": false, ""Influence"": ""Med"", ""Reputation"": ""Med"", ""Reward"": 557296, ""MissionID"": 375660729 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual("Structural Regulators", cargo.invariantName);
            Assert.AreEqual(0, cargo.total);
            Assert.AreEqual(7, cargo.need);

            HaulageAmount haulage = cargo.haulageamounts.FirstOrDefault(h => h.id == 375682327);
            Assert.AreEqual(0, cargo.haulage + cargo.stolen + cargo.owned);
            Assert.AreEqual(3, haulage.amount);
            Assert.AreEqual("Mission_Salvage_Planet", haulage.name);
            Assert.AreEqual(DateTime.Parse("2018-05-12T15:20:27Z").ToUniversalTime(), haulage.expiry);

            // CargoCollectedEvent
            line = @"{""timestamp"":""2018-05-05T19:42:20Z"",""event"":""CollectCargo"",""Type"":""$StructuralRegulators_Name;"",""Stolen"":false}";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCommodityCollectedEvent((CommodityCollectedEvent)events[0]);
            line = @"{""timestamp"":""2018-05-05T19:42:20Z"",""event"":""CollectCargo"",""Type"":""$StructuralRegulators_Name;"",""Stolen"":false}";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCommodityCollectedEvent((CommodityCollectedEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.haulage);
            Assert.AreEqual(5, cargo.need);
            Assert.AreEqual(0, cargo.stolen + cargo.owned);
            Assert.AreEqual(3, haulage.amount);

            // CargoMissionAbandonedEvent - If we abandon a mission with cargo it becomes stolen
            line = @"{ ""timestamp"":""2018-05-05T19:42:20Z"", ""event"":""MissionAbandoned"", ""Name"":""Mission_Salvage_Planet"", ""MissionID"":375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleMissionAbandonedEvent((MissionAbandonedEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.stolen);
            Assert.AreEqual(4, cargo.need);
            Assert.AreEqual(0, cargo.haulage + cargo.owned);

            // CargoMissionCompletedEvent - Check to see if this is a cargo mission and update our inventory accordingly
            line = @"{ ""timestamp"": ""2018-05-05T22:27:58Z"", ""event"": ""MissionCompleted"", ""Faction"": ""Merope Expeditionary Fleet"", ""Name"": ""Mission_Salvage_Planet_name"", ""MissionID"": 375660729, ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 4, ""DestinationSystem"": ""HIP 17692"", ""Reward"": 624016, ""FactionEffects"": [ { ""Faction"": ""Merope Expeditionary Fleet"", ""Effects"": [ { ""Effect"": ""$MISSIONUTIL_Interaction_Summary_civilUnrest_down;"", ""Effect_Localised"": ""$#MinorFaction; are happy to report improved civil contentment, making a period of civil unrest unlikely."", ""Trend"": ""DownGood"" } ], ""Influence"": [ { ""SystemAddress"": 224644818084, ""Trend"": ""UpGood"" } ], ""Reputation"": ""UpGood"" } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleMissionCompletedEvent((MissionCompletedEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.IsNull(cargo);

            // CargoMissionFailedEvent - If we fail a mission with cargo it becomes stolen
            line = @"{ ""timestamp"": ""2018-05-05T19:42:20Z"", ""event"": ""MissionAccepted"", ""Faction"": ""Elite Knights"", ""Name"": ""Mission_Salvage_Planet"", ""LocalisedName"": ""Salvage 3 Structural Regulators"", ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 3, ""DestinationSystem"": ""Merope"", ""Expiry"": ""2018-05-12T15:20:27Z"", ""Wing"": false, ""Influence"": ""Med"", ""Reputation"": ""Med"", ""Reward"": 557296, ""MissionID"": 375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            line = @"{""timestamp"":""2018-05-05T19:42:20Z"",""event"":""CollectCargo"",""Type"":""$StructuralRegulators_Name;"",""Stolen"":false}";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCommodityCollectedEvent((CommodityCollectedEvent)events[0]);
            line = @"{ ""timestamp"":""2018-05-05T19:42:20Z"", ""event"":""MissionFailed"", ""Name"":""Mission_Salvage_Planet"", ""MissionID"":375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleMissionFailedEvent((MissionFailedEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.stolen);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(0, cargo.haulage + cargo.owned);
        }

        [TestMethod]
        public void TestCargoSearchAndRescue()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());

            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""damagedescapepod"", ""Name_Localised"": ""Damaged Escape Pod"", ""Count"": 4, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCargoInventoryEvent((CargoInventoryEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "DamagedEscapePod");
            Assert.AreEqual(4, cargo.total);
            Assert.AreEqual(4, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoSearchAndRescueEvent
            line = @"{ ""timestamp"":""2017-08-26T01:58:24Z"", ""event"":""SearchAndRescue"", ""Name"":""damagedescapepod"", ""Count"":2, ""Reward"":5310 }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleSearchAndRescueEvent((SearchAndRescueEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "DamagedEscapePod");
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);
        }


        [TestMethod]
        public void TestCargoSynthesis()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());

            // CargoSynthesisedEvent
            line = @"{ ""timestamp"": ""2018-05-05T21:08:41Z"", ""event"": ""Synthesis"", ""Name"": ""Limpet Basic"", ""Materials"": [ { ""Name"": ""iron"", ""Count"": 10 }, { ""Name"": ""nickel"", ""Count"": 10 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleSynthesisedEvent();

            Assert.AreEqual(1, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual(4, cargo.total);
            Assert.AreEqual(4, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);
        }

        [TestMethod]
        public void TestCargoTechnologyBroker()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());

            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""iondistributor"", ""Name_Localised"": ""Ion Distributor"", ""Count"": 10, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleCargoInventoryEvent((CargoInventoryEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "IonDistributor");
            Assert.AreEqual(10, cargo.total);
            Assert.AreEqual(10, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoTechnologyBrokerEvent
            line = @"{ ""timestamp"":""2018-03-02T11:28:44Z"", ""event"":""TechnologyBroker"", ""BrokerType"":""Human"", ""MarketID"":128151032, ""ItemsUnlocked"":[{ ""Name"":""Hpt_PlasmaShockCannon_Fixed_Medium"", ""Name_Localised"":""Shock Cannon"" }], ""Commodities"":[{ ""Name"":""iondistributor"", ""Name_Localised"":""Ion Distributor"", ""Count"":6 }], ""Materials"":[ { ""Name"":""vanadium"", ""Count"":30, ""Category"":""Raw"" }, { ""Name"":""tungsten"", ""Count"":30, ""Category"":""Raw"" }, { ""Name"":""rhenium"", ""Count"":36, ""Category"":""Raw"" }, { ""Name"":""technetium"", ""Count"":30, ""Category"":""Raw""}]}";
            events = JournalMonitor.ParseJournalEntry(line);
            cargoMonitor._handleTechnologyBrokerEvent((TechnologyBrokerEvent)events[0]);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "IonDistributor");
            Assert.AreEqual(4, cargo.total);
            Assert.AreEqual(4, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);
        }

        [TestCleanup]
        private void StopTestCargoMonitor()
        {
        }
    }
}
