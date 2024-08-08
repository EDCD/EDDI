using EddiCargoMonitor;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class CargoMonitorTests : TestBase
    {
        readonly CargoMonitor cargoMonitor = new CargoMonitor(new CargoMonitorConfiguration());

        private const string cargoConfigJson = @"{
                    ""cargo"": [{
                        ""edname"": ""DamagedEscapePod"",
                        ""stolen"": 0,
                        ""haulage"": 0,
                        ""owned"": 4,
                        ""price"": 11912.0,
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
                        ""price"": 6995.0,
                        ""haulageData"": []
                        }, 
                        {
                        ""edname"": ""Drones"",
                        ""stolen"": 0,
                        ""haulage"": 0,
                        ""owned"": 21,
                        ""price"": 101.0,
                        ""haulageData"": []
                        }],
                    ""cargocarried"": 29,
                    ""updatedat"": ""2022-10-02T10:31:52Z""
            }";

        [TestInitialize]
        public void StartTestCargoMonitor()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestCargoConfig()
        {
            var config = ConfigService.FromJson<CargoMonitorConfiguration>(cargoConfigJson);

            Assert.AreEqual(3, config.cargo.Count);
            var cargo = config.cargo.FirstOrDefault(c => c.edname.Equals("DamagedEscapePod", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(cargo);
            Assert.AreEqual("Damaged Escape Pod", cargo.commodityDef.invariantName);
            Assert.AreEqual(4, cargo.total);
            Assert.AreEqual(4, cargo.owned);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(0, cargo.haulage);
            Assert.AreEqual(11912, cargo.price);
        }

        [TestMethod]
        public void TestCargoEventsScenario()
        {
            var privateObject = new PrivateObject(cargoMonitor);

            // 'Startup' CargoEvent
            var line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":52, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"drones\", \"Name_Localised\":\"Limpet\", \"Count\":20, \"Stolen\":0 } ] }";
            var events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", events[0] );
            Assert.AreEqual(4, cargoMonitor.inventory.Count);
            Assert.AreEqual(52, cargoMonitor.cargoCarried);

            var cargo = cargoMonitor.inventory.FirstOrDefault(c => c.edname.Equals("Drones", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(cargo);
            Assert.AreEqual("Limpet", cargo.localizedName);
            Assert.AreEqual(20, cargo.total);
            Assert.AreEqual(20, cargo.owned);
            Assert.AreEqual(0, cargo.stolen + cargo.haulage);

            // Drone count reduced with subsequent startup CargoEvent
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":42, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"drones\", \"Name_Localised\":\"Limpet\", \"Count\":10, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", events[0] );
            Assert.AreEqual(4, cargoMonitor.inventory.Count);
            Assert.AreEqual(42, cargoMonitor.cargoCarried);
            cargo = cargoMonitor.inventory.FirstOrDefault(c => c.edname.Equals("Drones", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(cargo);
            Assert.AreEqual(10, cargo.total);

            // Drones removed from inventory with subsequent startup CargoEvent
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", events[0] );
            Assert.AreEqual(3, cargoMonitor.inventory.Count);
            Assert.AreEqual(32, cargoMonitor.cargoCarried);
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "Drones", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNull(cargo);
        }

        [ TestMethod ]
        public void TestCommodityPurchasedEvent ()
        {
            var privateObject = new PrivateObject(cargoMonitor);

            var line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":52, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"drones\", \"Name_Localised\":\"Limpet\", \"Count\":20, \"Stolen\":0 } ] }";
            var events = JournalMonitor.ParseJournalEntry( line );
            privateObject.Invoke( "_handleCargoEvent", events[ 0 ] );
            Assert.AreEqual( 4, cargoMonitor.inventory.Count );
            Assert.AreEqual( 52, cargoMonitor.cargoCarried );

            var cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals("AnimalMeat", StringComparison.InvariantCultureIgnoreCase) );
            Assert.IsNotNull( cargo );
            Assert.AreEqual( "Animal Meat", cargo.localizedName );
            Assert.AreEqual( 1, cargo.total );
            Assert.AreEqual( 1, cargo.owned );
            Assert.AreEqual( 0, cargo.stolen + cargo.haulage );

            line = @"{ ""timestamp"":""2021-01-17T05:56:13Z"", ""event"":""MarketBuy"", ""MarketID"":3222000896, ""Type"":""animalmeat"", ""Type_Localised"":""Animal Meat"", ""Count"":105, ""BuyPrice"":1503, ""TotalCost"":157815 }";
            events = JournalMonitor.ParseJournalEntry( line );
            privateObject.Invoke( "_handleCommodityPurchasedEvent", events[ 0 ] );
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals("AnimalMeat", StringComparison.InvariantCultureIgnoreCase) );
            Assert.IsNotNull( cargo );
            Assert.AreEqual( 106, cargo.total );
            Assert.AreEqual( 106, cargo.owned );
            Assert.AreEqual( 0, cargo.stolen + cargo.haulage );
        }

        [ TestMethod ]
        public void TestCommodityEjectedEvent ()
        {
            var privateObject = new PrivateObject(cargoMonitor);

            var line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":52, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"drones\", \"Name_Localised\":\"Limpet\", \"Count\":20, \"Stolen\":0 } ] }";
            var events = JournalMonitor.ParseJournalEntry( line );
            privateObject.Invoke( "_handleCargoEvent", events[ 0 ] );
            Assert.AreEqual( 4, cargoMonitor.inventory.Count );
            Assert.AreEqual( 52, cargoMonitor.cargoCarried );

            var cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals("Biowaste", StringComparison.InvariantCultureIgnoreCase) );
            Assert.IsNotNull( cargo );
            Assert.AreEqual( 30, cargo.total );
            Assert.AreEqual( 30, cargo.haulage );

            line = @"{""timestamp"": ""2016-06-10T14:32:03Z"", ""event"": ""EjectCargo"", ""Type"":""biowaste"", ""Count"":2, ""MissionID"":426282789, ""Abandoned"":true}";
            events = JournalMonitor.ParseJournalEntry( line );
            privateObject.Invoke( "_handleCommodityEjectedEvent", events[ 0 ] );

            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals("Biowaste", StringComparison.InvariantCultureIgnoreCase) );
            Assert.IsNotNull( cargo );
            Assert.AreEqual( 28, cargo.total );
            Assert.AreEqual( 28, cargo.haulage );
        }

        [ TestMethod ]
        public void TestCommoditySoldEvent ()
        {
            var privateObject = new PrivateObject(cargoMonitor);

            var line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":52, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"drones\", \"Name_Localised\":\"Limpet\", \"Count\":20, \"Stolen\":0 } ] }";
            var events = JournalMonitor.ParseJournalEntry( line );
            privateObject.Invoke( "_handleCargoEvent", events[ 0 ] );
            Assert.AreEqual( 4, cargoMonitor.inventory.Count );
            Assert.AreEqual( 52, cargoMonitor.cargoCarried );

            var cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "HydrogenFuel", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNotNull( cargo );
            Assert.AreEqual( "Hydrogen Fuel", cargo.localizedName );
            Assert.AreEqual( 1, cargo.total );
            Assert.AreEqual( 1, cargo.owned );
            Assert.AreEqual( 0, cargo.stolen + cargo.haulage );

            line = @"{ ""timestamp"":""2022-02-06T04:35:37Z"", ""event"":""MarketSell"", ""MarketID"":3502759680, ""Type"":""hydrogenfuel"", ""Type_Localised"":""Hydrogen Fuel"", ""Count"":1, ""SellPrice"":100, ""TotalSale"":100, ""AvgPricePaid"":84 }";
            events = JournalMonitor.ParseJournalEntry( line );
            privateObject.Invoke( "_handleCommoditySoldEvent", events[ 0 ] );
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "HydrogenFuel", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNull( cargo );
        }

        [TestMethod]
        public void TestCargoMissionScenario()
        {
            var privateObject = new PrivateObject(cargoMonitor);

            // CargoEvent
            var line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 } ] }";
            var events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });

            // CargoEvent - Collected 2 Structural Regulators for mission ID 375682327.
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":34, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"structuralregulators\", \"MissionID\":375682327, \"Count\":2, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });

            var cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "StructuralRegulators", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNotNull(cargo);
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.haulage);
            Assert.AreEqual(0, cargo.stolen + cargo.owned);

            // Cargo MissionAbandonedEvent - Verify haulage data for for mission ID 375682327 has been removed
            line = @"{ ""timestamp"":""2018-05-05T19:42:20Z"", ""event"":""MissionAbandoned"", ""Name"":""Mission_Salvage_Planet"", ""MissionID"":375682327 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleMissionAbandonedEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "StructuralRegulators", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNotNull( cargo );
            Assert.AreEqual( 2, cargo.total );
            Assert.AreEqual( 2, cargo.stolen );
            Assert.AreEqual( 0, cargo.haulage + cargo.owned );

            // CargoEvent - Verify 2 stolen Structural Regulators
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":34, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"structuralregulators\", \"Count\":2, \"Stolen\":2 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "StructuralRegulators", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNotNull(cargo);
            Assert.AreEqual(2, cargo.total);
            Assert.AreEqual(2, cargo.stolen);
            Assert.AreEqual(0, cargo.haulage + cargo.owned);

            // Verify 2 stolen Structural Regulators are removed at a `Cargo` event.
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":32, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "StructuralRegulators", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNull(cargo);

            // CargoDepot - verify cargo now includes 60 units of silver
            line = @"{ ""timestamp"":""2018-08-26T02:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748339, ""UpdateType"":""Collect"", ""CargoType"":""Silver"", ""Count"":60, ""StartMarketID"":3225297216, ""EndMarketID"":3224777216, ""ItemsCollected"":60, ""ItemsDelivered"":0, ""TotalItemsToDeliver"":60, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoDepotEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "Silver", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNotNull( cargo );
            Assert.AreEqual( 60, cargo.total );
            Assert.AreEqual( 60, cargo.haulage );
            Assert.AreEqual( 0, cargo.stolen );
            Assert.AreEqual( 0, cargo.owned );

            // Cargo - verify cargo still includes 60 units of silver
            line = "{ \"timestamp\":\"2018-10-31T03:39:10Z\", \"event\":\"Cargo\", \"Count\":92, \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"biowaste\", \"MissionID\":426282789, \"Count\":30, \"Stolen\":0 }, { \"Name\":\"animalmeat\", \"Name_Localised\":\"Animal Meat\", \"Count\":1, \"Stolen\":0 }, { \"Name\":\"silver\", \"MissionID\":413748339, \"Count\":60, \"Stolen\":0 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "Silver", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNotNull( cargo );
            Assert.AreEqual(60, cargo.total);
            Assert.AreEqual(60, cargo.haulage);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(0, cargo.owned);

            // CargoDepot - verify 60 units of silver and cargo entry have been removed
            line = @"{ ""timestamp"":""2018-08-26T03:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748339, ""UpdateType"":""Deliver"", ""CargoType"":""Silver"", ""Count"":60, ""StartMarketID"":3225297216, ""EndMarketID"":3224777216, ""ItemsCollected"":60, ""ItemsDelivered"":60, ""TotalItemsToDeliver"":60, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleCargoDepotEvent", new object[] { events[0] });
            cargo = cargoMonitor.inventory.FirstOrDefault( c => c.edname.Equals( "Silver", StringComparison.InvariantCultureIgnoreCase ) );
            Assert.IsNull( cargo );
        }

        [TestMethod]
        public void TestCargoPriceScenario()
        {
            // Test that average cargo price dynamically updates based on the aquisition prices and quantities
            var privateObject = new PrivateObject(cargoMonitor);

            // Synthesise 4 drones
            var line = @"{ ""timestamp"":""2020-10-26T04:05:27Z"", ""event"":""Synthesis"", ""Name"":""Limpet Basic"", ""Materials"":[ { ""Name"":""iron"", ""Count"":10 }, { ""Name"":""nickel"", ""Count"":10 } ] }";
            var events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleSynthesisedEvent", new object[] { events[0] });
            var cargo = cargoMonitor.inventory.ToList().FirstOrDefault(c => c.edname == "Drones");
            Assert.IsNotNull(cargo);
            Assert.AreEqual(4, cargo.total);
            Assert.AreEqual(0, cargo.haulage);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(4, cargo.owned);
            Assert.AreEqual(0, cargo.price); // weighted price: 0

            // Buy one drone
            line = @"{ ""timestamp"":""2020-10-26T04:10:27Z"", ""event"":""BuyDrones"", ""Type"":""Drones"", ""Count"":1, ""BuyPrice"":127, ""TotalCost"":127 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleLimpetPurchasedEvent", new object[] { events[0] });
            Assert.AreEqual(5, cargo.total);
            Assert.AreEqual(0, cargo.haulage);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(5, cargo.owned);
            Assert.AreEqual(25, cargo.price); // weighted price: 25.4

            // Buy 5 drones
            line = @"{ ""timestamp"":""2020-10-26T04:15:27Z"", ""event"":""BuyDrones"", ""Type"":""Drones"", ""Count"":5, ""BuyPrice"":127, ""TotalCost"":635 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleLimpetPurchasedEvent", new object[] { events[0] });
            Assert.AreEqual(10, cargo.total);
            Assert.AreEqual(0, cargo.haulage);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(10, cargo.owned);
            Assert.AreEqual(76, cargo.price); // weighted price: 76.2

            // Buy another 5 drones, except these are on sale
            line = @"{ ""timestamp"":""2020-10-26T04:15:27Z"", ""event"":""BuyDrones"", ""Type"":""Drones"", ""Count"":5, ""BuyPrice"":1, ""TotalCost"":5 }";
            events = JournalMonitor.ParseJournalEntry(line);
            privateObject.Invoke("_handleLimpetPurchasedEvent", new object[] { events[0] });
            Assert.AreEqual(15, cargo.total);
            Assert.AreEqual(0, cargo.haulage);
            Assert.AreEqual(0, cargo.stolen);
            Assert.AreEqual(15, cargo.owned);
            Assert.AreEqual(51, cargo.price); // weighted price: 51.13
        }
    }
}
