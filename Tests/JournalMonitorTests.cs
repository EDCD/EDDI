using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiMissionMonitor;
using System.Collections.Generic;
using Rollbar;

namespace UnitTests
{
    [TestClass]
    public class JournalMonitorTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        [TestMethod]
        public void TestJournalPlanetScan1()
        {
            string line = @"{ ""timestamp"":""2016-09-22T21:34:30Z"", ""event"":""Scan"", ""BodyName"":""Nemehim 4"", ""DistanceFromArrivalLS"":1115.837646, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Rocky ice body"", ""Atmosphere"":"""", ""Volcanism"":"""", ""MassEM"":0.013448, ""Radius"":1688803.625000, ""SurfaceGravity"":1.879402, ""SurfaceTemperature"":103.615654, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":{ ""iron"":18.8, ""sulphur"":17.7, ""carbon"":14.9, ""nickel"":14.3, ""phosphorus"":9.6, ""chromium"":8.5, ""manganese"":7.8, ""zinc"":5.1, ""molybdenum"":1.2, ""tungsten"":1.0, ""tellurium"":1.0 }, ""OrbitalPeriod"":122165280.000000, ""RotationPeriod"":112645.117188 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
        }

        [TestMethod]
        public void TestJournalPlanetScan2()
        {
            string line = @"{ ""timestamp"":""2016 - 11 - 01T18: 49:07Z"", ""event"":""Scan"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""DistanceFromArrivalLS"":703.763611, ""TidalLock"":false, ""TerraformState"":""Terraformable"", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick carbon dioxide atmosphere"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":2.171783, ""Radius"":7622170.500000, ""SurfaceGravity"":14.899396, ""SurfaceTemperature"":836.165466, ""SurfacePressure"":33000114.000000, ""Landable"":false, ""SemiMajorAxis"":210957926400.000000, ""Eccentricity"":0.000248, ""OrbitalInclination"":0.015659, ""Periapsis"":104.416656, ""OrbitalPeriod"":48801056.000000, ""RotationPeriod"":79442.242188 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(BodyScannedEvent));
            BodyScannedEvent ev = events[0] as BodyScannedEvent;
            Assert.IsNotNull(ev);
            Assert.AreEqual("Grea Bloae HH-T d4-44 4", ev.name);
            Assert.AreEqual((decimal)703.763611, ev.distancefromarrival);
            Assert.IsNotNull(ev.tidallylocked);
            Assert.IsFalse((bool)ev.tidallylocked);
            Assert.AreEqual("Candidate for terraforming", ev.terraformState.invariantName);
            Assert.AreEqual("High metal content world", ev.planetClass.invariantName);
            Assert.IsNotNull(ev.volcanism);
            Assert.AreEqual("Magma", ev.volcanism.invariantType);
            Assert.AreEqual("Iron", ev.volcanism.invariantComposition);
            Assert.AreEqual("Minor", ev.volcanism.invariantAmount);
            Assert.AreEqual((decimal)2.171783, ev.earthmass);
            Assert.AreEqual((double)7622.170500000M, (double)ev.radius, 0.01);
            Assert.AreEqual(Utilities.ConstantConverters.ms2g((decimal)14.899396), ev.gravity);
            Assert.AreEqual((decimal)836.165466, ev.temperature);
            Assert.AreEqual(325.986, (double)ev.pressure, 0.01);
            Assert.IsNotNull(ev.landable);
            Assert.IsFalse((bool)ev.landable);
            Assert.AreEqual(703.679898444943, (double)ev.semimajoraxis, 0.01);
            Assert.AreEqual((decimal)0.000248, ev.eccentricity);
            Assert.AreEqual((decimal)0.015659, ev.orbitalinclination);
            Assert.AreEqual((decimal)104.416656, ev.periapsis);
            Assert.AreEqual(564.827, (double)ev.orbitalperiod, 0.01);
            Assert.AreEqual(0.91947, (double)ev.rotationperiod, 0.01);
        }

        [TestMethod]
        public void TestJournalPlanetScan3()
        {
            string line = @"{
        ""Atmosphere"": ""hot thick carbon dioxide atmosphere"",
         ""AtmosphereComposition"": [
            {
                ""Name"": ""CarbonDioxide"",
                 ""Percent"": 96.5
            },
             {
                ""Name"": ""Nitrogen"",
                 ""Percent"": 3.499999
            }
        ],
         ""AtmosphereType"": ""CarbonDioxide"",
         ""AxialTilt"": 3.094469,
         ""BodyID"": 2,
         ""BodyName"": ""Venus"",
         ""Composition"": {
            ""Ice"": 0.0,
             ""Metal"": 0.3,
             ""Rock"": 0.7
        },
         ""DistanceFromArrivalLS"": 360.959534,
         ""Eccentricity"": 0.0067,
         ""Landable"": false,
         ""MassEM"": 0.815,
         ""OrbitalInclination"": 3.395,
         ""OrbitalPeriod"": 19414166.0,
         ""Parents"": [
            {
                ""Star"": 0
            }
        ],
         ""Periapsis"": 55.186001,
         ""PlanetClass"": ""High metal content body"",
         ""Radius"": 6051800.0,
         ""RotationPeriod"": 20996798.0,
         ""ScanType"": ""NavBeaconDetail"",
         ""SemiMajorAxis"": 108207980544.0,
         ""SurfaceGravity"": 8.869474,
         ""SurfacePressure"": 9442427.0,
         ""SurfaceTemperature"": 735.0,
         ""SystemAddress"": 10477373803,
         ""TerraformState"": """",
         ""TidalLock"": true,
         ""Volcanism"": ""minor rocky magma volcanism"",
         ""event"": ""Scan"",
         ""timestamp"": ""2018-09-03T19:07:54Z""
    }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(BodyScannedEvent));
            BodyScannedEvent ev = events[0] as BodyScannedEvent;
            Assert.IsNotNull(ev);
            Assert.AreEqual("Carbon dioxide", ev.atmosphereclass.invariantName);
            Assert.AreEqual(96.5M, ev.atmospherecomposition[0].percent);
            Assert.AreEqual("Rock", ev.solidcomposition[0].invariantName);
            Assert.AreEqual(70M, ev.solidcomposition[0].percent);
        }

        [TestMethod]
        public void TestJournalStarScan1()
        {
            string line = @"{ ""timestamp"":""2016-10-27T08:51:23Z"", ""event"":""Scan"", ""BodyName"":""Vela Dark Region FG-Y d3"", ""DistanceFromArrivalLS"":0.000000, ""StarType"":""K"", ""StellarMass"":0.960938, ""Radius"":692146368.000000, ""AbsoluteMagnitude"":5.375961, ""Age_MY"":230, ""SurfaceTemperature"":5108.000000, ""RotationPeriod"":393121.093750, ""Rings"":[ { ""Name"":""Vela Dark Region FG-Y d3 A Belt"", ""RingClass"":""eRingClass_MetalRich"", ""MassMT"":1.2262e+10, ""InnerRad"":1.2288e+09, ""OuterRad"":2.3812e+09 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            StarScannedEvent theEvent = (StarScannedEvent)events[0];

            Assert.AreEqual(230, theEvent.age);
            Assert.IsNull(theEvent.eccentricity);
            Assert.AreEqual("Vela Dark Region FG-Y d3", theEvent.name);
            Assert.IsNull(theEvent.orbitalperiod);
            Assert.AreEqual(692146.368000000M, theEvent.radius);
            Assert.IsNull(theEvent.semimajoraxis);
            Assert.AreEqual(0.960938, (double)theEvent.solarmass, 0.001);
            Assert.AreEqual("K", theEvent.stellarclass);
            Assert.AreEqual(5108, theEvent.temperature);
            // Stellar extras
            Assert.AreEqual("yellow-orange", theEvent.chromaticity);
            Assert.AreEqual(50, theEvent.massprobability);
            Assert.AreEqual(51, theEvent.radiusprobability);
            Assert.AreEqual(58, theEvent.tempprobability);
            Assert.AreEqual(7, theEvent.ageprobability);
            Assert.AreEqual(303.548, (double)theEvent.estimatedhabzoneinner, .01);
            Assert.AreEqual(604.861, (double)theEvent.estimatedhabzoneouter, .01);
        }

        [TestMethod]
        public void TestJournalStarScan2()
        {
            string line = @"{ ""timestamp"":""2016-10-28T12:07:09Z"", ""event"":""Scan"", ""BodyName"":""Col 285 Sector CG-X d1-44"", ""DistanceFromArrivalLS"":0.000000, ""StarType"":""TTS"", ""StellarMass"":0.808594, ""Radius"":659162816.000000, ""AbsoluteMagnitude"":6.411560, ""Age_MY"":154, ""SurfaceTemperature"":4124.000000, ""RotationPeriod"":341417.281250, ""Rings"":[ { ""Name"":""Col 285 Sector CG-X d1-44 A Belt"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":1.1625e+13, ""InnerRad"":1.0876e+09, ""OuterRad"":2.4192e+09 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            StarScannedEvent theEvent = (StarScannedEvent)events[0];
            Assert.AreEqual((decimal)659162.816, theEvent.radius);
            Assert.AreEqual(StarClass.solarradius((decimal)659162.816000000), theEvent.solarradius);
            Assert.AreEqual(0.94775, (double)theEvent.solarradius, 0.01);
        }

        [TestMethod]
        public void TestJournalShipyardNew1()
        {
            string line = @"{ ""timestamp"":""2016-10-27T08:49:08Z"", ""event"":""ShipyardNew"", ""ShipType"":""belugaliner"", ""NewShipID"":56 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
        }

        [TestMethod]
        public void TestJournalInterdiction1()
        {
            string line = @"{ ""timestamp"":""2016-09-21T07:00:17Z"",""event"":""Interdiction"",""Success"":true,""Interdicted"":""Torval's Shield"",""IsPlayer"":false,""Faction"":""Zemina Torval"",""Power"":""Empire""}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
        }

        [TestMethod]
        public void TestJournalFileheader1()
        {
            string line = @"{""timestamp"":""2016-06-10T14:31:00Z"", ""event"":""Fileheader"", ""part"":1, ""gameversion"":""2.2"", ""build"":""r131487/r0 "" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            FileHeaderEvent theEvent = (FileHeaderEvent)events[0];

            Assert.AreEqual("r131487/r0", theEvent.build);
        }

        [TestMethod]
        public void TestJournalDocked1()
        {
            string line = @"{ ""timestamp"":""2017-04-14T19:34:32Z"", ""event"":""Docked"", ""StationName"":""Freeholm"", ""StationType"":""AsteroidBase"", ""StarSystem"":""Artemis"", ""StationFaction"":{ ""Name"":""Artemis Empire Assembly"", ""FactionState"":""Boom"" }, ""StationGovernment"":""$government_Patronage;"", ""StationGovernment_Localised"":""Patronage"", ""StationAllegiance"":""Empire"", ""StationEconomy"":""$economy_Industrial;"", ""StationEconomy_Localised"":""Industrial"", ""StationEconomies"": [ { ""Name"": ""$economy_Industrial;"", ""Proportion"": 0.7 }, { ""Name"": ""$economy_Extraction;"", ""Proportion"": 0.3 } ], ""DistFromStarLS"":2527.211914, ""StationServices"":[""Refuel""], ""MarketID"": 128169720, ""SystemAddress"": 3107509474002 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockedEvent theEvent = (DockedEvent)events[0];

            Assert.AreEqual("AsteroidBase", theEvent.stationModel.edname);

            Assert.AreEqual(128169720, theEvent.marketId);
            Assert.AreEqual(3107509474002, theEvent.systemAddress);
            Assert.AreEqual(1, theEvent.stationservices.Count);
            Assert.AreEqual("Refuel", theEvent.stationservices[0]);
            Assert.AreEqual(2, theEvent.economyShares.Count);
            Assert.AreEqual("Industrial", theEvent.economyShares[0].economy.invariantName);
            Assert.AreEqual(0.7M, theEvent.economyShares[0].proportion);
            Assert.AreEqual("Extraction", theEvent.economyShares[1].economy.invariantName);
            Assert.AreEqual(0.3M, theEvent.economyShares[1].proportion);
        }

        [TestMethod]
        public void TestJournalDocked2()
        {
            string line = @"{ ""timestamp"":""2018-04-01T05:21:24Z"", ""event"":""Docked"", ""StationName"":""Donaldson"", ""StationType"":""Orbis"", ""StarSystem"":""Alioth"", ""SystemAddress"":1109989017963, ""MarketID"":128141048, ""StationFaction"":{ ""Name"":""Alioth Pro-Alliance Grou"", ""FactionState"":""Boom"" }, ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationAllegiance"":""Alliance"", ""StationServices"":[ ""Dock"", ""Autodock"", ""BlackMarket"", ""Commodities"", ""Contacts"", ""Exploration"", ""Missions"", ""Outfitting"", ""CrewLounge"", ""Rearm"", ""Refuel"", ""Repair"", ""Shipyard"", ""Tuning"", ""Workshop"", ""MissionsGenerated"", ""FlightController"", ""StationOperations"", ""Powerplay"", ""SearchAndRescue"" ], ""StationEconomy"":""$economy_Service;"", ""StationEconomy_Localised"":""Service"", ""StationEconomies"":[ { ""Name"":""$economy_Service;"", ""Name_Localised"":""Service"", ""Proportion"":1.000000 } ], ""DistFromStarLS"":4632.417480 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockedEvent theEvent = (DockedEvent)events[0];

            Assert.AreEqual("Orbis", theEvent.stationModel.edname);
            Assert.AreEqual("Donaldson", theEvent.station);
            Assert.AreEqual("Alioth", theEvent.system);
            Assert.AreEqual("Boom", theEvent.factionState.invariantName);
            Assert.AreEqual("Democracy", theEvent.Government.invariantName);
            Assert.AreEqual("Alliance", theEvent.Allegiance.invariantName);
            Assert.AreEqual(20, theEvent.stationservices.Count);
            Assert.AreEqual(1, theEvent.economyShares.Count);
            Assert.AreEqual("Service", theEvent.economyShares[0].economy.invariantName);
            Assert.AreEqual(1.0M, theEvent.economyShares[0].proportion);
        }

        [TestMethod]
        public void TestJournalDockingCancelled()
        {
            string line = @"{ ""timestamp"":""2018-06-04T07:43:11Z"", ""event"":""DockingCancelled"", ""MarketID"":3227840768, ""StationName"":""Laval Terminal"", ""StationType"":""Orbis"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockingCancelledEvent theEvent = (DockingCancelledEvent)events[0];

            Assert.AreEqual("Orbis", theEvent.stationDefinition.basename);
            Assert.AreEqual("Laval Terminal", theEvent.station);
            Assert.AreEqual(3227840768, theEvent.marketId);
        }

        [TestMethod]
        public void TestJournalDockingDenied()
        {
            string line = @"{ ""timestamp"":""2018-06-04T01:53:29Z"", ""event"":""DockingDenied"", ""Reason"":""Offences"", ""MarketID"":3223343616, ""StationName"":""Ray Gateway"", ""StationType"":""Coriolis"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockingDeniedEvent theEvent = (DockingDeniedEvent)events[0];

            Assert.AreEqual("Coriolis", theEvent.stationDefinition.basename);
            Assert.AreEqual("Ray Gateway", theEvent.station);
            Assert.AreEqual(3223343616, theEvent.marketId);
        }

        [TestMethod]
        public void TestJournalDockingRequested()
        {
            string line = @"{ ""timestamp"":""2018-06-04T07:34:07Z"", ""event"":""DockingRequested"", ""MarketID"":3222020352, ""StationName"":""Morris Enterprise"", ""StationType"":""Bernal"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockingRequestedEvent theEvent = (DockingRequestedEvent)events[0];

            Assert.AreEqual("Bernal", theEvent.stationDefinition.basename);
            Assert.AreEqual("Morris Enterprise", theEvent.station);
            Assert.AreEqual(3222020352, theEvent.marketId);
        }

        [TestMethod]
        public void TestJournalDockingGranted()
        {
            string line = @"{ ""timestamp"":""2018-06-04T07:53:34Z"", ""event"":""DockingGranted"", ""LandingPad"":17, ""MarketID"":128850247, ""StationName"":""Simbad's Refuge"", ""StationType"":""AsteroidBase"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockingGrantedEvent theEvent = (DockingGrantedEvent)events[0];

            Assert.AreEqual("AsteroidBase", theEvent.stationDefinition.basename);
            Assert.AreEqual(17, theEvent.landingpad);
            Assert.AreEqual("Simbad's Refuge", theEvent.station);
            Assert.AreEqual(128850247, theEvent.marketId);
        }

        [TestMethod]
        public void TestJournalMessageReceived1()
        {
            string line = @"{ ""timestamp"":""2016-10-07T03:02:44Z"", ""event"":""ReceiveText"", ""From"":""$ShipName_Police_Federation;"", ""From_Localised"":""Federal Security Service"", ""Message"":""$Police_StartPatrol03;"", ""Message_Localised"":""Receiving five by five, I'm in the air now, joining patrol."", ""Channel"":""npc"" }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            MessageReceivedEvent theEvent = (MessageReceivedEvent)events[0];

            Assert.IsFalse(theEvent.player);
            Assert.AreEqual("Police", theEvent.source);
            Assert.AreEqual("Federal Security Service", theEvent.from);
        }

        [TestMethod]
        public void TestJournalMessageReceived2()
        {
            string line = @"{ ""timestamp"":""2016-10-06T12:48:56Z"", ""event"":""ReceiveText"", ""From"":""$npc_name_decorate:#name=Jonathan Dallard;"", ""From_Localised"":""Jonathan Dallard"", ""Message"":""$Pirate_OnStartScanCargo07;"", ""Message_Localised"":""Do you have anything of value?"", ""Channel"":""npc"" }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 2);

            MessageReceivedEvent event1 = (MessageReceivedEvent)events[0];

            Assert.IsFalse(event1.player);
            Assert.AreEqual("Pirate", event1.source);
            Assert.AreEqual("Jonathan Dallard", event1.from);

            NPCCargoScanCommencedEvent event2 = (NPCCargoScanCommencedEvent)events[1];

            Assert.AreEqual("Pirate", event2.by);
        }

        [TestMethod]
        public void TestJournalCommsSystemMessage()
        {
            string line = @"{ ""timestamp"":""2018-11-15T06:16:23Z"", ""event"":""ReceiveText"", ""From"":"""", ""Message"":""$COMMS_entered:#name=ICZ JH-V c2-7;"", ""Message_Localised"":""Entered Channel: ICZ JH-V c2-7"", ""Channel"":""npc"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 0);
        }

        [TestMethod]
        public void TestJournalPlayerDirectMessage()
        {
            string line = "{ \"timestamp\":\"2017-10-12T19:58:46Z\", \"event\":\"ReceiveText\", \"From\":\"SlowIce\", \"Message\":\"good luck\", \"Channel\":\"player\" }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            MessageReceivedEvent event1 = (MessageReceivedEvent)events[0];

            Assert.IsTrue(event1.player);
            Assert.AreEqual("Commander", event1.source);
            Assert.AreEqual("SlowIce", event1.from);
            Assert.AreEqual("good luck", event1.message);
        }

        [TestMethod]
        public void TestJournalPlayerLocalChat()
        {
           string line = @"{ ""timestamp"":""2017 - 10 - 12T20: 39:25Z"", ""event"":""ReceiveText"", ""From"":""Rebecca Lansing"", ""Message"":""Hi there"", ""Channel"":""local"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            MessageReceivedEvent event1 = (MessageReceivedEvent)events[0];

            Assert.IsTrue(event1.player);
            Assert.AreEqual("Commander", event1.source);
            Assert.AreEqual("Rebecca Lansing", event1.from);
            Assert.AreEqual("Hi there", event1.message);
        }

        [TestMethod]
        public void TestJournalPlayerWingChat()
        {
            string line = @"{ ""timestamp"":""2017-10-12T21:11:10Z"", ""event"":""ReceiveText"", ""From"":""SlowIce"", ""Message"":""hello"", ""Channel"":""wing"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            MessageReceivedEvent event1 = (MessageReceivedEvent)events[0];

            Assert.IsTrue(event1.player);
            Assert.AreEqual("Wing mate", event1.source);
            Assert.AreEqual("SlowIce", event1.from);
            Assert.AreEqual("hello", event1.message);
        }

        [TestMethod]
        public void TestJournalPlayerMulticrewChat()
        {
            // Test for messages received from multicrew. These are received without a defined key for 'Channel' in the player journal.
            string line = @"{ ""timestamp"":""2017 - 12 - 06T22: 40:54Z"", ""event"":""ReceiveText"", ""From"":""Nexonoid"", ""Message"":""whats up"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            MessageReceivedEvent event1 = (MessageReceivedEvent)events[0];

            Assert.IsTrue(event1.player);
            Assert.AreEqual("multicrew", event1.channel);
            Assert.AreEqual("Crew mate", event1.source);
            Assert.AreEqual("Nexonoid", event1.from);
            Assert.AreEqual("whats up", event1.message);
        }

        [TestMethod]
        public void TestJournalMissionAccepted1()
        {
            string line = @"{ ""timestamp"":""2017-05-05T16:07:37Z"", ""event"":""MissionAccepted"", ""Faction"":""Chick Ek Partnership"", ""Name"":""Mission_Sightseeing_Criminal_BOOM"", ""Commodity"":""$Wine_Name;"", ""Commodity_Localised"":""Wine"", ""Count"":3, ""DestinationSystem"":""HR 7221$MISSIONUTIL_MULTIPLE_FINAL_SEPARATOR;Tupa"", ""Expiry"":""2017-05-06T04:31:24Z"", ""Wing"":false, ""Influence"":""Low"", ""Reputation"":""Med"", ""PassengerCount"":7, ""PassengerVIPs"":true, ""PassengerWanted"":true, ""PassengerType"":""Criminal"", ""MissionID"":134724902 }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            MissionAcceptedEvent event1 = (MissionAcceptedEvent)events[0];

            Assert.AreEqual("Wine", event1.commodity);
        }

        [TestMethod]
        public void TestFriends()
        {
            string line = "{ \"timestamp\":\"2017-08-24T17:22:03Z\", \"event\":\"Friends\", \"Status\":\"Online\", \"Name\":\"_Testy_McTest_\" }";
            string line2 = "{ \"timestamp\":\"2017-08-24T17:22:03Z\", \"event\":\"Friends\", \"Status\":\"Offline\", \"Name\":\"_Testy_McTest_\" }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            events = JournalMonitor.ParseJournalEntry(line2);

            /// Since this friend is unknown to us, the first time we see this friend no event should trigger. 
            /// Only the second line, registering the status as offline, should be registered as an event.
            Assert.IsTrue(events.Count == 1);

            FriendsEvent @event = (FriendsEvent)events[0];
            Friend testFriend = new Friend
            {
                name = @event.name,
                status = @event.status
            };

            Assert.AreEqual("Offline", @event.status);

            // Clean up
            Eddi.EDDI.Instance.Cmdr.friends.Remove(testFriend);
        }

        [TestMethod]
        public void TestJournalSearchAndRescue()
        {
            string line = @"{""timestamp"":""2018-05-26T22:04:09Z"",""event"":""SearchAndRescue"",""MarketID"":3228973824,""Name"":""usscargoblackbox"",""Name_Localised"":""Black Box"",""Count"":1,""Reward"":21184}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            SearchAndRescueEvent sarEvent = (SearchAndRescueEvent)events[0];
            Assert.AreEqual("Black Box", sarEvent.commodity.invariantName);
            Assert.AreEqual("Salvage", sarEvent.commodity.category.invariantName);
        }

        [TestMethod]
        public void TestJournalEnteredNormalSpaceEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 02 - 07T07: 13:39Z"", ""event"":""SupercruiseExit"", ""StarSystem"":""Wyrd"", ""SystemAddress"":5031654888146, ""Body"":""Vonarburg Co-operative"", ""BodyID"":35, ""BodyType"":""Station"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            EnteredNormalSpaceEvent normalSpaceEvent = (EnteredNormalSpaceEvent)events[0];
            Assert.AreEqual("Vonarburg Co-operative", normalSpaceEvent.body);
            Assert.AreEqual("Station", normalSpaceEvent.bodytype);
            Assert.AreEqual("Wyrd", normalSpaceEvent.system);
            Assert.AreEqual(5031654888146, normalSpaceEvent.systemAddress);
        }

        [TestMethod]
        public void TestJournalJumpedEvent()
        {
            string line = @"{
	        ""timestamp"": ""2018-08-08T06: 56: 20Z"",
	        ""event"": ""FSDJump"",
	        ""StarSystem"": ""Diaguandri"",
        	""SystemAddress"": 670417429889,
	        ""StarPos"": [-41.06250, -62.15625, -103.25000],
	        ""SystemAllegiance"": ""Independent"",
	        ""SystemEconomy"": ""$economy_HighTech;"",
	        ""SystemEconomy_Localised"": ""HighTech"",
	        ""SystemSecondEconomy"": ""$economy_Refinery;"",
	        ""SystemSecondEconomy_Localised"": ""Refinery"",
	        ""SystemGovernment"": ""$government_Democracy;"",
	        ""SystemGovernment_Localised"": ""Democracy"",
	        ""SystemSecurity"": ""$SYSTEM_SECURITY_medium;"",
	        ""SystemSecurity_Localised"": ""MediumSecurity"",
	        ""Population"": 10303479,
	        ""JumpDist"": 19.340,
	        ""FuelUsed"": 2.218082,
	        ""FuelLevel"": 23.899260,
	        ""Factions"": [{
		        ""Name"": ""DiaguandriInterstellar"",
		        ""FactionState"": ""Boom"",
		        ""Government"": ""Corporate"",
		        ""Influence"": 0.100398,
		        ""Allegiance"": ""Independent""
	        },
	        {
		        ""Name"": ""People'sMET20Liberals"",
		        ""FactionState"": ""Boom"",
		        ""Government"": ""Democracy"",
		        ""Influence"": 0.123260,
		        ""Allegiance"": ""Federation""
	        },
	        {
		        ""Name"": ""PilotsFederationLocalBranch"",
		        ""FactionState"": ""None"",
		        ""Government"": ""Democracy"",
		        ""Influence"": 0.000000,
		        ""Allegiance"": ""PilotsFederation""
	        },
	        {
		        ""Name"": ""NaturalDiaguandriRegulatoryState"",
		        ""FactionState"": ""None"",
		        ""Government"": ""Dictatorship"",
		        ""Influence"": 0.020875,
		        ""Allegiance"": ""Independent"",
		        ""RecoveringStates"": [{""State"": ""CivilWar"", ""Trend"": 0}]
	        },
	        {
		        ""Name"": ""CartelofDiaguandri"",
		        ""FactionState"": ""None"",
		        ""Government"": ""Anarchy"",
		        ""Influence"": 0.009940,
		        ""Allegiance"": ""Independent"",
		        ""PendingStates"": [{""State"": ""Bust"", ""Trend"": 0}, {""State"": ""CivilUnrest"", ""Trend"": 1}],
		        ""RecoveringStates"": [{""State"": ""CivilWar"", ""Trend"": 0}]
	        },
	        {
		        ""Name"": ""RevolutionaryPartyofDiaguandri"",
		        ""FactionState"": ""None"",
		        ""Government"": ""Democracy"",
		        ""Influence"": 0.124254,
		        ""Allegiance"": ""Federation"",
		        ""PendingStates"": [{""State"": ""Boom"", ""Trend"": 1}, {""State"": ""Bust"", ""Trend"": 1}]
	        },
	        {
		        ""Name"": ""TheBrotherhoodoftheDarkCircle"",
		        ""FactionState"": ""None"",
		        ""Government"": ""Corporate"",
		        ""Influence"": 0.093439,
		        ""Allegiance"": ""Independent"",
		        ""RecoveringStates"": [{""State"": ""CivilUnrest"", ""Trend"": 1}]
            },
            {
		        ""Name"": ""EXO"",
		        ""FactionState"": ""Expansion"",
		        ""Government"": ""Democracy"",
		        ""Influence"": 0.527833,
		        ""Allegiance"": ""Independent"",
		        ""PendingStates"": [{""State"": ""Boom"", ""Trend"": 1}]
	        }],
	        ""SystemFaction"": {
		        ""Name"": ""EXO"",
		        ""FactionState"": ""Expansion""
	        }
            }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            JumpedEvent jumpedEvent = (JumpedEvent)events[0];
            Assert.AreEqual("Diaguandri", jumpedEvent.system);
            Assert.AreEqual(670417429889, jumpedEvent.systemAddress);
            Assert.AreEqual(-41.06250M, jumpedEvent.x);
            Assert.AreEqual(-62.15625M, jumpedEvent.y);
            Assert.AreEqual(-103.25000M, jumpedEvent.z);
            Assert.AreEqual("Independent", jumpedEvent.Allegiance.invariantName);
            Assert.AreEqual("High Tech", jumpedEvent.economy);
            Assert.AreEqual("Refinery", jumpedEvent.economy2);
            Assert.AreEqual("Democracy", jumpedEvent.government);
            Assert.AreEqual("Medium", jumpedEvent.security);
            Assert.AreEqual(10303479, jumpedEvent.population);
            Assert.AreEqual(19.340M, jumpedEvent.distance);
            Assert.AreEqual(2.218082M, jumpedEvent.fuelused);
            Assert.AreEqual(23.899260M, jumpedEvent.fuelremaining);
            Assert.AreEqual("EXO", jumpedEvent.faction);
            Assert.AreEqual("Expansion", jumpedEvent.factionstate);
        }


        [TestMethod]
        public void TestJournalJumpedEvent2()
        {
            // Test for unpopulated system
            string line = @"{
                ""timestamp"": ""2018-10-17T00:40:45Z"",
                ""event"": ""FSDJump"",
                ""StarSystem"": ""Wredguia WD-K d8-65"",
                ""SystemAddress"": 2243793258827,
                ""StarPos"": [-319.15625,10.37500,-332.31250],
                ""SystemAllegiance"": """",
                ""SystemEconomy"": ""$economy_None;"",
                ""SystemEconomy_Localised"": ""None"",
                ""SystemSecondEconomy"": ""$economy_None;"",
                ""SystemSecondEconomy_Localised"": ""None"",
                ""SystemGovernment"": ""$government_None;"",
                ""SystemGovernment_Localised"": ""None"",
                ""SystemSecurity"": ""$GAlAXY_MAP_INFO_state_anarchy;"",
                ""SystemSecurity_Localised"": ""Anarchy"",
                ""Population"": 0,
                ""JumpDist"": 24.230,
                ""FuelUsed"": 4.271171,
                ""FuelLevel"": 27.728828
            }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            JumpedEvent jumpedEvent = (JumpedEvent)events[0];
            Assert.AreEqual("None", jumpedEvent.factionstate);
        }

        [TestMethod]
        public void TestJournalJumpedEventFactionStateNull()
        {
            // Test for unpopulated system
            string line = @"{
                ""timestamp"": ""2018-10-17T00:40:45Z"",
                ""event"": ""FSDJump"",
                ""StarSystem"": ""Wredguia WD-K d8-65"",
                ""SystemAddress"": 2243793258827,
                ""StarPos"": [-319.15625,10.37500,-332.31250],
                ""SystemAllegiance"": """",
                ""SystemEconomy"": ""$economy_None;"",
                ""SystemEconomy_Localised"": ""None"",
                ""SystemSecondEconomy"": ""$economy_None;"",
                ""SystemSecondEconomy_Localised"": ""None"",
                ""SystemGovernment"": ""$government_None;"",
                ""SystemGovernment_Localised"": ""None"",
                ""SystemSecurity"": ""$GAlAXY_MAP_INFO_state_anarchy;"",
                ""SystemSecurity_Localised"": ""Anarchy"",
                ""Population"": 0,
                ""JumpDist"": 24.230,
                ""FuelUsed"": 4.271171,
                ""FuelLevel"": 27.728828
            }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            JumpedEvent jumpedEvent = (JumpedEvent)events[0];
            Assert.AreEqual("None", jumpedEvent.factionstate);
        }

        [TestMethod]
        public void TestJournalLocationEvent()
        {
            string line = @"{
	            ""timestamp"": ""2018-08-12T02: 52: 13Z"",
	                ""event"": ""Location"",
	        ""Docked"": true,
	            ""MarketID"": 3223343616,
	            ""StationName"": ""RayGateway"",
	            ""StationType"": ""Coriolis"",
	            ""StarSystem"": ""Diaguandri"",
	            ""SystemAddress"": 670417429889,
	            ""StarPos"": [-41.06250,
	            -62.15625,
	            -103.25000],
	            ""SystemAllegiance"": ""Independent"",
	            ""SystemEconomy"": ""$economy_HighTech;"",
    	        ""SystemEconomy_Localised"": ""HighTech"",
    	        ""SystemSecondEconomy"": ""$economy_Refinery;"",
	            ""SystemSecondEconomy_Localised"": ""Refinery"",
	            ""SystemGovernment"": ""$government_Democracy;"",
	            ""SystemGovernment_Localised"": ""Democracy"",
	            ""SystemSecurity"": ""$SYSTEM_SECURITY_medium;"",
	            ""SystemSecurity_Localised"": ""MediumSecurity"",
	            ""Population"": 10303479,
	            ""Body"": ""RayGateway"",
	            ""BodyID"": 32,
	            ""BodyType"": ""Station"",
	            ""Factions"": [{
		            ""Name"": ""DiaguandriInterstellar"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Corporate"",
		            ""Influence"": 0.090000,
		            ""Allegiance"": ""Independent"",
		            ""RecoveringStates"": [{
			            ""State"": ""Boom"",
			            ""Trend"": 0
		            }]
	            },
	            {
		            ""Name"": ""People'sMET20Liberals"",
		            ""FactionState"": ""Boom"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.206000,
		         ""Allegiance"": ""Federation""
	            },
	            {
		            ""Name"": ""PilotsFederationLocalBranch"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.000000,
		            ""Allegiance"": ""PilotsFederation""
	            },
	            {
		            ""Name"": ""NaturalDiaguandriRegulatoryState"",
		            ""FactionState"": ""Boom"",
		            ""Government"": ""Dictatorship"",
		            ""Influence"": 0.072000,
		            ""Allegiance"": ""Independent""
	            },
	            {
		            ""Name"": ""CartelofDiaguandri"",
		            ""FactionState"": ""Bust"",
		            ""Government"": ""Anarchy"",
		            ""Influence"": 0.121000,
		            ""Allegiance"": ""Independent"",
		            ""PendingStates"": [{
			            ""State"": ""Boom"",
			            ""Trend"": 1
		            },
		            {
			            ""State"": ""CivilUnrest"",
			            ""Trend"": -1
		            }]
	            },
	            {
		            ""Name"": ""RevolutionaryPartyofDiaguandri"",
		            ""FactionState"": ""Boom"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.181000,
		            ""Allegiance"": ""Federation"",
		            ""PendingStates"": [{
			            ""State"": ""Bust"",
			            ""Trend"": 0
		            }]
	            },
	            {
		            ""Name"": ""TheBrotherhoodoftheDarkCircle"",
		            ""FactionState"": ""Boom"",
		            ""Government"": ""Corporate"",
		            ""Influence"": 0.086000,
		            ""Allegiance"": ""Independent""
	            },
	            {
		            ""Name"": ""EXO"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.244000,
		            ""Allegiance"": ""Independent"",
		            ""PendingStates"": [{
			            ""State"": ""Boom"",
			            ""Trend"": 1
		            }]
	            }],
	            ""SystemFaction"": {
		            ""Name"": ""EXO"",
		            ""FactionState"": ""None""
	            }
            }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            LocationEvent @event = (LocationEvent)events[0];

            Assert.AreEqual("Independent", @event.Allegiance.invariantName);
            Assert.AreEqual("RayGateway", @event.body);
            Assert.AreEqual("Station", @event.bodyType.invariantName);
            Assert.AreEqual(true, @event.docked);
            Assert.AreEqual("High Tech", @event.Economy.invariantName);
            Assert.AreEqual("Refinery", @event.Economy2.invariantName);
            Assert.AreEqual("EXO", @event.faction);
            Assert.AreEqual("Democracy", @event.Government.invariantName);
            Assert.IsNull(@event.latitude);
            Assert.IsNull(@event.longitude);
            Assert.AreEqual(3223343616, @event.marketId);
            Assert.AreEqual(10303479, @event.population);
            Assert.AreEqual("Medium", @event.securityLevel.invariantName);
            Assert.AreEqual("RayGateway", @event.station);
            Assert.AreEqual("Coriolis Starport", @event.stationModel.invariantName);
            Assert.AreEqual("Diaguandri", @event.system);
            Assert.AreEqual(670417429889, @event.systemAddress);
            Assert.AreEqual(-41.06250M, @event.x);
            Assert.AreEqual(-62.15625M, @event.y);
            Assert.AreEqual(-103.25000M, @event.z);
        }

        [TestMethod]
        public void TestNearSurfaceEvent()
        {
            string line = @"{ ""timestamp"":""2018-07-24T07:08:37Z"", ""event"":""ApproachBody"", ""StarSystem"":""Ageno"", ""SystemAddress"":18262335038849, ""Body"":""Ageno B 2 a"", ""BodyID"":17 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            NearSurfaceEvent @event = (NearSurfaceEvent)events[0];

            Assert.AreEqual("Ageno", @event.system);
            Assert.AreEqual(18262335038849, @event.systemAddress);
            Assert.AreEqual("Ageno B 2 a", @event.body);

            string line2 = @"{ ""timestamp"":""2018 - 07 - 24T07: 08:58Z"", ""event"":""LeaveBody"", ""StarSystem"":""Ageno"", ""SystemAddress"":18262335038849, ""Body"":""Ageno B 2 a"", ""BodyID"":17 }";
            events = JournalMonitor.ParseJournalEntry(line2);
            NearSurfaceEvent @event2 = (NearSurfaceEvent)events[0];

            Assert.AreEqual("Ageno", @event2.system);
            Assert.AreEqual(18262335038849, @event2.systemAddress);
            Assert.AreEqual("Ageno B 2 a", @event2.body);
        }

        [TestMethod]
        public void TestSearchAndRescueEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 06 - 17T05: 32:32Z"", ""event"":""SearchAndRescue"", ""MarketID"":3222633216, ""Name"":""occupiedcryopod"", ""Name_Localised"":""Occupied Escape Pod"", ""Count"":2, ""Reward"":48593 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SearchAndRescueEvent @event = (SearchAndRescueEvent)events[0];

            Assert.AreEqual(3222633216, @event.marketId);
            Assert.AreEqual("occupiedcryopod", @event.commodity.edname.ToLowerInvariant());
            Assert.AreEqual(2, @event.amount);
            Assert.AreEqual(48593, @event.reward);
        }

        [TestMethod]
        public void TestSettlementApproachedEvent()
        {
            string line = @"{ ""timestamp"":""2018-11-04T03:11:56Z"", ""event"":""ApproachSettlement"", ""Name"":""Bulmer Enterprise"", ""MarketID"":3510380288, ""Latitude"":-23.121552, ""Longitude"":-98.177559 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SettlementApproachedEvent @event = (SettlementApproachedEvent)events[0];

            Assert.AreEqual(3510380288, @event.marketId);
            Assert.AreEqual("Bulmer Enterprise", @event.name);
            Assert.AreEqual(-23.121552M, @event.latitude);
            Assert.AreEqual(-98.177559M, @event.longitude);
        }

        [TestMethod]
        public void TestUndockedEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 08 - 12T02: 53:41Z"", ""event"":""Undocked"", ""StationName"":""Ray Gateway"", ""StationType"":""Coriolis"", ""MarketID"":3223343616 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            UndockedEvent @event = (UndockedEvent)events[0];

            Assert.AreEqual(3223343616, @event.marketId);
            Assert.AreEqual("Ray Gateway", @event.station);
        }

        [TestMethod]
        public void TestJumpedEventAllegianceThargoid()
        {
            string line = @"{ ""timestamp"":""2018 - 03 - 25T02: 59:48Z"", ""event"":""FSDJump"", ""StarSystem"":""Pleiades Sector OY-R c4-19"", ""SystemAddress"":5306398479010, ""StarPos"":[-73.81250,-98.62500,-262.31250], ""SystemAllegiance"":""Thargoid"", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""JumpDist"":13.936, ""FuelUsed"":3.833808, ""FuelLevel"":28.166193 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            JumpedEvent @event = (JumpedEvent)events[0];

            Assert.AreEqual("Thargoid", @event.Allegiance.invariantName);
        }

        [TestMethod]
        public void TestJumpedEventAllegianceGuardian()
        {
            string line = @"{ ""timestamp"":""2018-08-25T06:28:04Z"", ""event"":""FSDJump"", ""StarSystem"":""Synuefe EU-Q c21-10"", ""SystemAddress"":2833906537146, ""StarPos"":[758.65625,-176.90625,-133.21875], ""SystemAllegiance"":""Guardian"", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""JumpDist"":29.867, ""FuelUsed"":1.269155, ""FuelLevel"":18.824537 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            JumpedEvent @event = (JumpedEvent)events[0];

            Assert.AreEqual("Guardian", @event.Allegiance.invariantName);
        }

        [TestMethod]
        public void TestFactionKillBondThargoid()
        {
            string line = @"{""timestamp"":""2018 - 07 - 28T07: 39:54Z"",""event"":""FactionKillBond"",""Reward"":10000,""AwardingFaction"":""$faction_PilotsFederation;"",""AwardingFaction_Localised"":""Pilots Federation"",""VictimFaction"":""$faction_Thargoid;"",""VictimFaction_Localised"":""Thargoids""}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            BondAwardedEvent @event = (BondAwardedEvent)events[0];

            Assert.AreEqual("Pilots Federation", @event.awardingfaction);
            Assert.AreEqual("Thargoid", @event.victimfaction);
        }

        [TestMethod]
        public void TestUnhandledEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 10 - 30T20: 45:07Z"", ""event"":""AnyUnhandledEvent""}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            UnhandledEvent @event = (UnhandledEvent)events[0];

            Assert.AreEqual("AnyUnhandledEvent", @event.edType);
            Assert.IsNotNull(@event.raw);
        }

        [TestMethod]
        public void TestSquadronStatusEvent()
        {
            string line = @"{ ""timestamp"":""2018-10-17T16:17:55Z"", ""event"":""SquadronCreated"", ""SquadronName"":""TestSquadron"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            SquadronStatusEvent event1 = (SquadronStatusEvent)events[0];
            Assert.AreEqual("TestSquadron", event1.name);
            Assert.AreEqual("created", event1.status);
        }

        [TestMethod]
        public void TestSquadronRankEvent()
        {
            string line = @"{ ""timestamp"":""2018-10-17T16:17:55Z"", ""event"":""SquadronDemotion"", ""SquadronName"":""TestSquadron"", ""OldRank"":3, ""NewRank"":2 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            SquadronRankEvent event1 = (SquadronRankEvent)events[0];
            Assert.AreEqual("TestSquadron", event1.name);
            Assert.AreEqual(3, event1.oldrank);
            Assert.AreEqual(2, event1.newrank);
        }

        [TestMethod]
        public void TestLocationAllegiance()
        {
            string line = @"{ ""timestamp"":""2018-12-09T18:39:32Z"", ""event"":""Location"", ""Docked"":false, ""StarSystem"":""Col 285 Sector SP-K b23-7"", ""SystemAddress"":16065459463649, ""StarPos"":[112.31250,13.46875,153.06250], ""SystemAllegiance"":"""", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""Body"":""Col 285 Sector SP-K b23-7 A"", ""BodyID"":1, ""BodyType"":""Star"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            LocationEvent @event = (LocationEvent)events[0];

            Assert.IsNotNull(@event.raw);
            Assert.AreEqual("None", @event.Allegiance?.invariantName);
            Assert.AreEqual("$faction_None", @event.Allegiance?.edname);
        }

        [TestMethod]
        public void TestJumpEvent3()
        {
            string line = "{ \"timestamp\":\"2018-12-25T20:07:06Z\", \"event\":\"FSDJump\", \"StarSystem\":\"LHS 20\", \"SystemAddress\":33656303199641, \"StarPos\":[11.18750,-37.37500,-31.84375], \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_HighTech;\", \"SystemEconomy_Localised\":\"High Tech\", \"SystemSecondEconomy\":\"$economy_Refinery;\", \"SystemSecondEconomy_Localised\":\"Refinery\", \"SystemGovernment\":\"$government_Democracy;\", \"SystemGovernment_Localised\":\"Democracy\", \"SystemSecurity\":\"$SYSTEM_SECURITY_medium;\", \"SystemSecurity_Localised\":\"Medium Security\", \"Population\":9500553, \"JumpDist\":20.361, \"FuelUsed\":3.065896, \"FuelLevel\":19.762932, \"Factions\":[ { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"\", \"MyReputation\":6.106290 }, { \"Name\":\"Shenetserii Confederation\", \"FactionState\":\"None\", \"Government\":\"Confederacy\", \"Influence\":0.127000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":18.809999, \"PendingStates\":[ { \"State\":\"War\", \"Trend\":0 } ] }, { \"Name\":\"LHS 20 Company\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.127000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":4.950000, \"PendingStates\":[ { \"State\":\"War\", \"Trend\":0 } ] }, { \"Name\":\"Traditional LHS 20 Defence Party\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.087000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":2.640000 }, { \"Name\":\"Movement for LHS 20 Liberals\", \"FactionState\":\"CivilWar\", \"Government\":\"Democracy\", \"Influence\":0.226000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"SquadronFaction\":true, \"HomeSystem\":true, \"MyReputation\":100.000000, \"ActiveStates\":[ { \"State\":\"CivilLiberty\" }, { \"State\":\"Investment\" }, { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Nationalists of LHS 20\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.105000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000 }, { \"Name\":\"LHS 20 Organisation\", \"FactionState\":\"CivilWar\", \"Government\":\"Anarchy\", \"Influence\":0.166000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":5.940000, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"LHS 20 Engineers\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.162000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":15.000000 } ], \"SystemFaction\":{ \"Name\":\"Movement for LHS 20 Liberals\", \"FactionState\":\"CivilWar\" } }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            JumpedEvent @event = (JumpedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(JumpedEvent));

            PrivateObject privateObject = new PrivateObject(Eddi.EDDI.Instance);
            var result = (bool)privateObject.Invoke("eventJumped", new object[] { @event });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestJournalJumpEventHandler()
        {
            string line = "{ \"timestamp\":\"2018-12-25T20:07:06Z\", \"event\":\"FSDJump\", \"StarSystem\":\"LHS 20\", \"SystemAddress\":33656303199641, \"StarPos\":[11.18750,-37.37500,-31.84375], \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_HighTech;\", \"SystemEconomy_Localised\":\"High Tech\", \"SystemSecondEconomy\":\"$economy_Refinery;\", \"SystemSecondEconomy_Localised\":\"Refinery\", \"SystemGovernment\":\"$government_Democracy;\", \"SystemGovernment_Localised\":\"Democracy\", \"SystemSecurity\":\"$SYSTEM_SECURITY_medium;\", \"SystemSecurity_Localised\":\"Medium Security\", \"Population\":9500553, \"JumpDist\":20.361, \"FuelUsed\":3.065896, \"FuelLevel\":19.762932, \"Factions\":[ { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"\", \"MyReputation\":6.106290 }, { \"Name\":\"Shenetserii Confederation\", \"FactionState\":\"None\", \"Government\":\"Confederacy\", \"Influence\":0.127000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":18.809999, \"PendingStates\":[ { \"State\":\"War\", \"Trend\":0 } ] }, { \"Name\":\"LHS 20 Company\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.127000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":4.950000, \"PendingStates\":[ { \"State\":\"War\", \"Trend\":0 } ] }, { \"Name\":\"Traditional LHS 20 Defence Party\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.087000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":2.640000 }, { \"Name\":\"Movement for LHS 20 Liberals\", \"FactionState\":\"CivilWar\", \"Government\":\"Democracy\", \"Influence\":0.226000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"SquadronFaction\":true, \"HomeSystem\":true, \"MyReputation\":100.000000, \"ActiveStates\":[ { \"State\":\"CivilLiberty\" }, { \"State\":\"Investment\" }, { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Nationalists of LHS 20\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.105000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000 }, { \"Name\":\"LHS 20 Organisation\", \"FactionState\":\"CivilWar\", \"Government\":\"Anarchy\", \"Influence\":0.166000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":5.940000, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"LHS 20 Engineers\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.162000, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":15.000000 } ], \"SystemFaction\":{ \"Name\":\"Movement for LHS 20 Liberals\", \"FactionState\":\"CivilWar\" } }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            JumpedEvent @event = (JumpedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(JumpedEvent));

            PrivateObject privateObject = new PrivateObject(Eddi.EDDI.Instance);
            var result = (bool)privateObject.Invoke("eventJumped", new object[] { @event });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestJournalLocationEventHandler()
        {
            string line = "{ \"timestamp\":\"2018-12-27T08:05:23Z\", \"event\":\"Location\", \"Docked\":true, \"MarketID\":3230448384, \"StationName\":\"Cleve Hub\", \"StationType\":\"Orbis\", \"StarSystem\":\"Eravate\", \"SystemAddress\":5856221467362, \"StarPos\":[-42.43750,-3.15625,59.65625], \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemSecondEconomy\":\"$economy_Industrial;\", \"SystemSecondEconomy_Localised\":\"Industrial\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":740380179, \"Body\":\"Cleve Hub\", \"BodyID\":48, \"BodyType\":\"Station\", \"Powers\":[ \"Zachary Hudson\" ], \"PowerplayState\":\"Exploited\", \"Factions\":[ { \"Name\":\"Eravate School of Commerce\", \"FactionState\":\"None\", \"Government\":\"Cooperative\", \"Influence\":0.086913, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":91.840103 }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":42.790199 }, { \"Name\":\"Independent Eravate Free\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.123876, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Eravate Network\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.036963, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Traditional Eravate Autocracy\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.064935, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Eravate Life Services\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.095904, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Official Eravate Flag\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.179820, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000 }, { \"Name\":\"Adle's Armada\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.411588, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"SquadronFaction\":true, \"HappiestSystem\":true, \"HomeSystem\":true, \"MyReputation\":100.000000, \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":0 } ] } ], \"SystemFaction\":{ \"Name\":\"Adle's Armada\", \"FactionState\":\"None\" } }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            LocationEvent @event = (LocationEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(LocationEvent));

            PrivateObject privateObject = new PrivateObject(Eddi.EDDI.Instance);
            var result = (bool)privateObject.Invoke("eventLocation", new object[] { @event });
            Assert.IsTrue(result);
        }
    }
}
