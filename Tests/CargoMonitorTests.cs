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
            EDDI.Instance.eventHandler(new FileHeaderEvent(DateTime.UtcNow, "JournalBeta.txt", "beta", "beta"));
        }

        [TestMethod]
        public void TestHaulageCopyCtor()
        {
            Haulage original = new Haulage(1, "name", "Sol", 42, null, false);
            Haulage copy = new Haulage(original);
            Assert.AreEqual(original.name, copy.name);
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
		            ""haulageData"": [{
                        ""missionid"": 413563829,
                        ""name"": ""Mission_Salvage_Expansion"",
                        ""typeEDName"": ""Salvage"",
                        ""status"": ""Active"",
                        ""originsystem"": ""HIP 20277"",
                        ""sourcesystem"": ""Bunuson"",
                        ""sourcebody"": null,
                        ""amount"": 4,
                        ""remaining"": 4,
                        ""startmarketid"": 0,
                        ""endmarketid"": 0,
                        ""collected"": 0,
                        ""delivered"": 0,
                        ""expiry"": null,
                        ""shared"": false
                    }]
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
		            ""haulageData"": []
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
		            ""haulageData"": []
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

            // Verify haulage object 
            Assert.AreEqual(1, cargo.haulageData.Count());
            Haulage haulage = cargo.haulageData[0];
            Assert.AreEqual(413563829, haulage.missionid);
            Assert.AreEqual("Mission_Salvage_Expansion", haulage.name);
            Assert.AreEqual("Salvage", haulage.typeEDName);
            Assert.AreEqual(4, haulage.amount);
            Assert.AreEqual(4, haulage.remaining);
            Assert.IsFalse(haulage.shared);
        }

        [TestMethod]
        public void TestCargoEventsScenario()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());
            var privateObject = new PrivateObject(cargoMonitor);

            // CargoEvent
            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""damagedescapepod"", ""Name_Localised"": ""Damaged Escape Pod"", ""Count"": 4, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoInventoryEvent", new object[] { events[0] });
            Assert.AreEqual(3, cargoMonitor.inventory.Count);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(21, cargo.total);
            Assert.AreEqual(21, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoCollectedEvent
            line = @"{""timestamp"":""2016-06-10T14:32:03Z"",""event"":""CollectCargo"",""Type"":""agriculturalmedicines"",""Stolen"":true}";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommodityCollectedEvent", new object[] { events[0] });
            Assert.AreEqual(4, cargoMonitor.inventory.Count);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "AgriculturalMedicines");
            Assert.AreEqual("Agri-Medicines", cargo.invariantName);
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.stolen);
            Assert.AreEqual(0, cargo.owned + cargo.need + cargo.haulage);

            // CargoEjectedEvent
            line = @"{""timestamp"": ""2016-06-10T14:32:03Z"", ""event"": ""EjectCargo"", ""Type"":""drones"", ""Count"":2, ""Abandoned"":true}";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommodityEjectedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual(19, cargo.total);
            Assert.AreEqual(19, cargo.owned);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(0, cargo.haulage);

            // CargoPurchasedEvent
            line = @"{ ""timestamp"":""2018-04-07T16:29:39Z"", ""event"":""MarketBuy"", ""MarketID"":3224801280, ""Type"":""coffee"", ""Count"":1, ""BuyPrice"":1198, ""TotalCost"":1198 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommodityPurchasedEvent", new object[] { events[0] });

            Assert.AreEqual(5, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Coffee");
            Assert.AreEqual("Coffee", cargo.invariantName);
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoRefinedEvent
            line = @"{ ""timestamp"":""2016-09-30T18:00:22Z"", ""event"":""MiningRefined"", ""Type"":""$hydrogenperoxide_name;"", ""Type_Localised"":""Hydrogen Peroxide"" }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommodityRefinedEvent", new object[] { events[0] });

            Assert.AreEqual(6, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "HydrogenPeroxide");
            Assert.AreEqual("Hydrogen Peroxide", cargo.invariantName);
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoSoldEvent
            line = @"{ ""timestamp"":""2018-04-07T16:29:44Z"", ""event"":""MarketSell"", ""MarketID"":3224801280, ""Type"":""coffee"", ""Count"":1, ""SellPrice"":1138, ""TotalSale"":1138, ""AvgPricePaid"":1198 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommoditySoldEvent", new object[] { events[0] });

            Assert.AreEqual(5, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Coffee");
            Assert.IsNull(cargo);

            // CargoPowerCommodityObtainedEvent
            line = @"{ ""timestamp"":""2016-12-02T16:10:26Z"", ""event"":""PowerplayCollect"", ""Power"":""Aisling Duval"", ""Type"":""$aislingmediamaterials_name;"", ""Type_Localised"":""Aisling Media Materials"", ""Count"":3 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handlePowerCommodityObtainedEvent", new object[] { events[0] });

            Assert.AreEqual(6, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname.ToLowerInvariant() == "aislingmediamaterials");
            Assert.AreEqual("Aisling Media Materials", cargo.invariantName);
            Assert.AreEqual(3, cargo.total);
            Assert.AreEqual(3, cargo.owned);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(0, cargo.haulage);

            // CargoPowerCommodityDeliveredEvent
            line = @"{ ""timestamp"":""2016-12-02T16:10:26Z"", ""event"":""PowerplayDeliver"", ""Power"":""Aisling Duval"", ""Type"":""$aislingmediamaterials_name;"", ""Type_Localised"":""Aisling Media Materials"", ""Count"":3 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handlePowerCommodityDeliveredEvent", new object[] { events[0] });

            Assert.AreEqual(5, cargoMonitor.inventory.Count);
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname.ToLowerInvariant() == "aislingmediamaterials");
            Assert.IsNull(cargo);
        }

        [TestMethod]
        public void TestCargoLimpetScenario()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());
            var privateObject = new PrivateObject(cargoMonitor);

            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""damagedescapepod"", ""Name_Localised"": ""Damaged Escape Pod"", ""Count"": 4, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoInventoryEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(21, cargo.total);
            Assert.AreEqual(21, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoLimpetPurchasedEvent
            line = @"{ ""timestamp"":""2016-09-21T06:53:53Z"", ""event"":""BuyDrones"", ""Type"":""Drones"", ""Count"":19, ""BuyPrice"":101, ""TotalCost"":1919 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleLimpetPurchasedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(40, cargo.total);
            Assert.AreEqual(40, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoLimpetSoldEvent
            line = @"{ ""timestamp"":""2016-09-24T00:03:25Z"", ""event"":""SellDrones"", ""Type"":""Drones"", ""Count"":8, ""SellPrice"":101, ""TotalSale"":808 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleLimpetSoldEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(32, cargo.total);
            Assert.AreEqual(32, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoLimpetLaunchedEvent
            line = @"{ ""timestamp"":""2018-04-07T20:05:07Z"", ""event"":""LaunchDrone"", ""Type"":""Collection"" }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleLimpetLaunchedEvent", new object[] {});

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
            var privateObject = new PrivateObject(cargoMonitor);

            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""damagedescapepod"", ""Name_Localised"": ""Damaged Escape Pod"", ""Count"": 4, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoInventoryEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.AreEqual("Limpet", cargo.invariantName);
            Assert.AreEqual(21, cargo.total);
            Assert.AreEqual(21, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoMissionAcceptedEvent - Check to see if this is a cargo mission and update our inventory accordingly
            line = @"{ ""timestamp"": ""2018-05-05T19:42:20Z"", ""event"": ""MissionAccepted"", ""Faction"": ""Elite Knights"", ""Name"": ""Mission_Salvage_Planet"", ""LocalisedName"": ""Salvage 3 Structural Regulators"", ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 3, ""DestinationSystem"": ""Merope"", ""Expiry"": ""2018-05-12T15:20:27Z"", ""Wing"": false, ""Influence"": ""Med"", ""Reputation"": ""Med"", ""Reward"": 557296, ""MissionID"": 375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionAcceptedEvent", new object[] { events[0] });
            line = @"{ ""timestamp"": ""2018-05-05T19:42:20Z"", ""event"": ""MissionAccepted"", ""Faction"": ""Merope Expeditionary Fleet"", ""Name"": ""Mission_Salvage_Planet"", ""LocalisedName"": ""Salvage 4 Structural Regulators"", ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 4, ""DestinationSystem"": ""HIP 17692"", ""Expiry"": ""2018-05-12T15:20:27Z"", ""Wing"": false, ""Influence"": ""Med"", ""Reputation"": ""Med"", ""Reward"": 557296, ""MissionID"": 375660729 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionAcceptedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual("Structural Regulators", cargo.invariantName);
            Assert.AreEqual(0, cargo.total);
            Assert.AreEqual(7, cargo.need);

            Haulage haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == 375682327);
            Assert.AreEqual(0, cargo.haulage + cargo.stolen + cargo.owned);
            Assert.AreEqual(3, haulage.amount);
            Assert.AreEqual("Mission_Salvage_Planet", haulage.name);
            Assert.AreEqual(DateTime.Parse("2018-05-12T15:20:27Z").ToUniversalTime(), haulage.expiry);

            // CargoCollectedEvent
            line = @"{""timestamp"":""2018-05-05T19:42:20Z"",""event"":""CollectCargo"",""Type"":""$StructuralRegulators_Name;"",""Stolen"":false}";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommodityCollectedEvent", new object[] { events[0] });
            line = @"{""timestamp"":""2018-05-05T19:42:20Z"",""event"":""CollectCargo"",""Type"":""$StructuralRegulators_Name;"",""Stolen"":false}";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommodityCollectedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.haulage);
            Assert.AreEqual(5, cargo.need);
            Assert.AreEqual(0, cargo.stolen + cargo.owned);
            Assert.AreEqual(3, haulage.amount);

            // CargoMissionAbandonedEvent - If we abandon a mission with cargo it becomes stolen
            line = @"{ ""timestamp"":""2018-05-05T19:42:20Z"", ""event"":""MissionAbandoned"", ""Name"":""Mission_Salvage_Planet"", ""MissionID"":375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionAbandonedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.stolen);
            Assert.AreEqual(4, cargo.need);
            Assert.AreEqual(0, cargo.haulage + cargo.owned);

            // CargoMissionCompletedEvent - Check to see if this is a cargo mission and update our inventory accordingly
            line = @"{ ""timestamp"": ""2018-05-05T22:27:58Z"", ""event"": ""MissionCompleted"", ""Faction"": ""Merope Expeditionary Fleet"", ""Name"": ""Mission_Salvage_Planet_name"", ""MissionID"": 375660729, ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 4, ""DestinationSystem"": ""HIP 17692"", ""Reward"": 624016, ""FactionEffects"": [ { ""Faction"": ""Merope Expeditionary Fleet"", ""Effects"": [ { ""Effect"": ""$MISSIONUTIL_Interaction_Summary_civilUnrest_down;"", ""Effect_Localised"": ""$#MinorFaction; are happy to report improved civil contentment, making a period of civil unrest unlikely."", ""Trend"": ""DownGood"" } ], ""Influence"": [ { ""SystemAddress"": 224644818084, ""Trend"": ""UpGood"" } ], ""Reputation"": ""UpGood"" } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionCompletedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.IsNull(cargo);

            // CargoMissionFailedEvent - If we fail a mission with cargo it becomes stolen
            line = @"{ ""timestamp"": ""2018-05-05T19:42:20Z"", ""event"": ""MissionAccepted"", ""Faction"": ""Elite Knights"", ""Name"": ""Mission_Salvage_Planet"", ""LocalisedName"": ""Salvage 3 Structural Regulators"", ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 3, ""DestinationSystem"": ""Merope"", ""Expiry"": ""2018-05-12T15:20:27Z"", ""Wing"": false, ""Influence"": ""Med"", ""Reputation"": ""Med"", ""Reward"": 557296, ""MissionID"": 375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionAcceptedEvent", new object[] { events[0] });
            line = @"{""timestamp"":""2018-05-05T19:42:20Z"",""event"":""CollectCargo"",""Type"":""$StructuralRegulators_Name;"",""Stolen"":false}";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommodityCollectedEvent", new object[] { events[0] });
            line = @"{ ""timestamp"":""2018-05-05T19:42:20Z"", ""event"":""MissionFailed"", ""Name"":""Mission_Salvage_Planet"", ""MissionID"":375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionFailedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.stolen);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(0, cargo.haulage + cargo.owned);

            // CargoDepotEvent - Check response for missed 'Mission accepted' event. Verify both cargo and haulage are created
            line = @"{ ""timestamp"":""2018-08-26T02:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748324, ""UpdateType"":""Deliver"", ""CargoType"":""Tantalum"", ""Count"":54, ""StartMarketID"":0, ""EndMarketID"":3224777216, ""ItemsCollected"":0, ""ItemsDelivered"":54, ""TotalItemsToDeliver"":70, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoDepotEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Tantalum");
            Assert.IsNotNull(cargo);
            Assert.AreEqual(0, cargo.total);
            Assert.AreEqual(0, cargo.haulage + cargo.owned);
            Assert.AreEqual(16, cargo.need);

            haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == 413748324);
            Assert.IsNotNull(haulage);
            Assert.AreEqual(16, haulage.remaining);
            Assert.IsTrue(haulage.shared);

            // Cargo Delivery 'Mission accepted' Event with 'Cargo Depot' events
            line = @"{ ""timestamp"":""2018-08-26T00:50:48Z"", ""event"":""MissionAccepted"", ""Faction"":""Calennero State Industries"", ""Name"":""Mission_Delivery_Boom"", ""LocalisedName"":""Boom time delivery of 60 units of Silver"", ""Commodity"":""$Silver_Name;"", ""Commodity_Localised"":""Silver"", ""Count"":60, ""DestinationSystem"":""HIP 20277"", ""DestinationStation"":""Fabian City"", ""Expiry"":""2018-08-27T00:48:38Z"", ""Wing"":false, ""Influence"":""Med"", ""Reputation"":""Med"", ""Reward"":25000000, ""MissionID"":413748339 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionAcceptedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Silver");
            Assert.IsNotNull(cargo);
            Assert.AreEqual(0, cargo.total);
            Assert.AreEqual(0, cargo.haulage + cargo.owned);
            Assert.AreEqual(60, cargo.need);

            haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == 413748339);
            Assert.IsNotNull(haulage);
            Assert.AreEqual(60, haulage.remaining);
            Assert.IsFalse(haulage.shared);

            line = @"{ ""timestamp"":""2018-08-26T02:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748339, ""UpdateType"":""Collect"", ""CargoType"":""Silver"", ""Count"":60, ""StartMarketID"":3225297216, ""EndMarketID"":3224777216, ""ItemsCollected"":60, ""ItemsDelivered"":0, ""TotalItemsToDeliver"":60, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoDepotEvent", new object[] { events[0] });

            Assert.AreEqual(60, cargo.total);
            Assert.AreEqual(60, cargo.haulage);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(60, haulage.remaining);
            Assert.AreEqual(3225297216, haulage.startmarketid);
            Assert.AreEqual(3224777216, haulage.endmarketid);

            line = @"{ ""timestamp"":""2018-08-26T03:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748339, ""UpdateType"":""Deliver"", ""CargoType"":""Silver"", ""Count"":60, ""StartMarketID"":3225297216, ""EndMarketID"":3224777216, ""ItemsCollected"":60, ""ItemsDelivered"":60, ""TotalItemsToDeliver"":60, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoDepotEvent", new object[] { events[0] });

            Assert.AreEqual(0, cargo.total);
            Assert.AreEqual(0, cargo.haulage);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(0, haulage.remaining);
        }

        [TestMethod]
        public void TestCargoSearchAndRescue()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());
            var privateObject = new PrivateObject(cargoMonitor);

            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""damagedescapepod"", ""Name_Localised"": ""Damaged Escape Pod"", ""Count"": 4, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoInventoryEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "DamagedEscapePod");
            Assert.AreEqual(4, cargo.total);
            Assert.AreEqual(4, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoSearchAndRescueEvent
            line = @"{ ""timestamp"":""2017-08-26T01:58:24Z"", ""event"":""SearchAndRescue"", ""Name"":""damagedescapepod"", ""Count"":2, ""Reward"":5310, ""MarketID"":128666762 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleSearchAndRescueEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "DamagedEscapePod");
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);
        }


        [TestMethod]
        public void TestCargoSynthesis()
        {
            cargoMonitor.initializeCargoMonitor(new CargoMonitorConfiguration());
            var privateObject = new PrivateObject(cargoMonitor);

            // CargoSynthesisedEvent
            line = @"{ ""timestamp"": ""2018-05-05T21:08:41Z"", ""event"": ""Synthesis"", ""Name"": ""Limpet Basic"", ""Materials"": [ { ""Name"": ""iron"", ""Count"": 10 }, { ""Name"": ""nickel"", ""Count"": 10 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleSynthesisedEvent", new object[] {});

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
            var privateObject = new PrivateObject(cargoMonitor);

            line = @"{""timestamp"": ""2018-05-05T19:12:10Z"", ""event"": ""Cargo"", ""Inventory"": [ { ""Name"": ""iondistributor"", ""Name_Localised"": ""Ion Distributor"", ""Count"": 10, ""Stolen"": 0 }, { ""Name"": ""usscargoblackbox"", ""Name_Localised"": ""Black Box"", ""Count"": 4, ""Stolen"": 4 }, { ""Name"": ""drones"", ""Name_Localised"": ""Limpet"", ""Count"": 21, ""Stolen"": 0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoInventoryEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "IonDistributor");
            Assert.AreEqual(10, cargo.total);
            Assert.AreEqual(10, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            // CargoTechnologyBrokerEvent
            line = @"{ ""timestamp"":""2018-03-02T11:28:44Z"", ""event"":""TechnologyBroker"", ""BrokerType"":""Human"", ""MarketID"":128151032, ""ItemsUnlocked"":[{ ""Name"":""Hpt_PlasmaShockCannon_Fixed_Medium"", ""Name_Localised"":""Shock Cannon"" }], ""Commodities"":[{ ""Name"":""iondistributor"", ""Name_Localised"":""Ion Distributor"", ""Count"":6 }], ""Materials"":[ { ""Name"":""vanadium"", ""Count"":30, ""Category"":""Raw"" }, { ""Name"":""tungsten"", ""Count"":30, ""Category"":""Raw"" }, { ""Name"":""rhenium"", ""Count"":36, ""Category"":""Raw"" }, { ""Name"":""technetium"", ""Count"":30, ""Category"":""Raw""}]}";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleTechnologyBrokerEvent", new object[] { events[0] });

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
