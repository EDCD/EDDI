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
            var privateObject = new PrivateObject(cargoMonitor);
            Haulage haulage = new Haulage();

            // CargoEvent
            line = "{ \"timestamp\":\"2018-10-31T01:54:40Z\", \"event\":\"Missions\", \"Active\":[  ], \"Failed\":[  ], \"Complete\":[  ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionsEvent", new object[] { events[0] });
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });
            Assert.AreEqual(3, cargoMonitor.inventory.Count);
            Assert.AreEqual(32, cargoMonitor.cargoCarried);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "HydrogenFuel");
            Assert.AreEqual("Hydrogen Fuel", cargo.localizedName);
            Assert.AreEqual(1, cargo.total);
            Assert.AreEqual(1, cargo.owned);
            Assert.AreEqual(0, cargo.need + cargo.stolen + cargo.haulage);

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Biowaste");
            Assert.AreEqual(30, cargo.total);
            Assert.AreEqual(30, cargo.haulage);
            haulage = cargo.haulageData.First();
            Assert.AreEqual(426282789, haulage.missionid);
            Assert.AreEqual("Unknown", haulage.name);
            Assert.AreEqual(30, haulage.amount);
            Assert.AreEqual("Active", haulage.status);

            // CargoEjectedEvent
            haulage.typeEDName = "delivery";
            line = @"{""timestamp"": ""2016-06-10T14:32:03Z"", ""event"": ""EjectCargo"", ""Type"":""biowaste"", ""Count"":2, ""MissionID"":426282789, ""Abandoned"":true}";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCommodityEjectedEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Biowaste");
            haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == 426282789);
            Assert.AreEqual("Failed", haulage.status);
        }

        [TestMethod]
        public void TestCargoMissionScenario()
        {
            var privateObject = new PrivateObject(cargoMonitor);
            Haulage haulage = new Haulage();

            // CargoEvent
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });

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
            Assert.AreEqual(0, cargo.haulage + cargo.stolen + cargo.owned);
            Assert.AreEqual(7, cargo.need);
            haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == 375682327);
            Assert.AreEqual(3, haulage.amount);
            Assert.AreEqual("Mission_Salvage_Planet", haulage.name);
            Assert.AreEqual(DateTime.Parse("2018-05-12T15:20:27Z").ToUniversalTime(), haulage.expiry);

            // CargoEvent - Collected 2 Structural Regulators for mission ID 375682327. Verify haulage & need changed
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"structuralregulators\", \"MissionID\":375682327, \"Count\":2, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });

            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.haulage);
            Assert.AreEqual(5, cargo.need);
            Assert.AreEqual(0, cargo.stolen + cargo.owned);

            // Cargo MissionAbandonedEvent - Verify haulage data for for mission ID 375682327 has been removed
            line = @"{ ""timestamp"":""2018-05-05T19:42:20Z"", ""event"":""MissionAbandoned"", ""Name"":""Mission_Salvage_Planet"", ""MissionID"":375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionAbandonedEvent", new object[] { events[0] });
            haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == 375682327);
            Assert.IsNull(haulage);

            // CargoEvent - Verify 2 stolen Structural Regulators and 4 still needed for mission ID 37566072
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"structuralregulators\", \"Count\":2, \"Stolen\":2 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.stolen);
            Assert.AreEqual(4, cargo.need);
            Assert.AreEqual(0, cargo.haulage + cargo.owned);

            // CargoMissionCompletedEvent - Verify haulage data & cargo has been removed
            line = @"{ ""timestamp"": ""2018-05-05T22:27:58Z"", ""event"": ""MissionCompleted"", ""Faction"": ""Merope Expeditionary Fleet"", ""Name"": ""Mission_Salvage_Planet_name"", ""MissionID"": 375660729, ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 4, ""DestinationSystem"": ""HIP 17692"", ""Reward"": 624016, ""FactionEffects"": [ { ""Faction"": ""Merope Expeditionary Fleet"", ""Effects"": [ { ""Effect"": ""$MISSIONUTIL_Interaction_Summary_civilUnrest_down;"", ""Effect_Localised"": ""$#MinorFaction; are happy to report improved civil contentment, making a period of civil unrest unlikely."", ""Trend"": ""DownGood"" } ], ""Influence"": [ { ""SystemAddress"": 224644818084, ""Trend"": ""UpGood"" } ], ""Reputation"": ""UpGood"" } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionCompletedEvent", new object[] { events[0] });
            haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == 375660729);
            Assert.IsNull(haulage);

            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.IsNull(cargo);

            // CargoMissionFailedEvent
            line = @"{ ""timestamp"": ""2018-05-05T19:42:20Z"", ""event"": ""MissionAccepted"", ""Faction"": ""Elite Knights"", ""Name"": ""Mission_Salvage_Planet"", ""LocalisedName"": ""Salvage 3 Structural Regulators"", ""Commodity"": ""$StructuralRegulators_Name;"", ""Commodity_Localised"": ""Structural Regulators"", ""Count"": 3, ""DestinationSystem"": ""Merope"", ""Expiry"": ""2018-05-12T15:20:27Z"", ""Wing"": false, ""Influence"": ""Med"", ""Reputation"": ""Med"", ""Reward"": 557296, ""MissionID"": 375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionAcceptedEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.IsNotNull(cargo);

            line = @"{ ""timestamp"":""2018-05-05T19:42:20Z"", ""event"":""MissionFailed"", ""Name"":""Mission_Salvage_Planet"", ""MissionID"":375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionFailedEvent", new object[] { events[0] });
            haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == 375682327);
            Assert.IsNull(haulage);

            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "StructuralRegulators");
            Assert.IsNull(cargo);

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

            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"silver\", \"MissionID\":413748339, \"Count\":60, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });
            Assert.AreEqual(60, cargo.total);
            Assert.AreEqual(60, cargo.haulage);
            Assert.AreEqual(0, cargo.need);
            Assert.AreEqual(60, haulage.remaining);
            Assert.AreEqual(3225297216, haulage.startmarketid);
            Assert.AreEqual(3224777216, haulage.endmarketid);

            line = @"{ ""timestamp"":""2018-08-26T03:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748339, ""UpdateType"":""Deliver"", ""CargoType"":""Silver"", ""Count"":60, ""StartMarketID"":3225297216, ""EndMarketID"":3224777216, ""ItemsCollected"":60, ""ItemsDelivered"":60, ""TotalItemsToDeliver"":60, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoDepotEvent", new object[] { events[0] });
            Assert.AreEqual(0, haulage.remaining);
        }

        [TestCleanup]
        private void StopTestCargoMonitor()
        {
        }
    }
}
