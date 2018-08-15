﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
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
            Assert.AreEqual(ev.name, "Grea Bloae HH-T d4-44 4");
            Assert.AreEqual(ev.distancefromarrival, (decimal)703.763611);
            Assert.IsTrue(ev.tidallylocked.HasValue);
            Assert.IsFalse((bool)ev.tidallylocked);
            Assert.AreEqual(ev.terraformstate, "Terraformable");
            Assert.AreEqual(ev.bodyclass, "High metal content body");
            Assert.AreEqual(ev.atmosphere, "hot thick carbon dioxide atmosphere");
            Assert.IsNotNull(ev.volcanism);
            Assert.AreEqual("Magma", ev.volcanism.invariantType);
            Assert.AreEqual("Iron", ev.volcanism.invariantComposition);
            Assert.AreEqual("Minor", ev.volcanism.invariantAmount);
            Assert.IsTrue(ev.earthmass == (decimal)2.171783);
            Assert.IsTrue(ev.radius  == (decimal)7622170.500000);
            Assert.AreEqual(ev.gravity, Body.ms2g((decimal)14.899396));
            Assert.AreEqual(ev.temperature, (decimal)836.165466);
            Assert.AreEqual(ev.pressure, (decimal)33000114.000000);
            Assert.IsTrue(ev.landable.HasValue);
            Assert.IsFalse((bool)ev.landable);
            Assert.AreEqual(ev.semimajoraxis, (decimal)210957926400.000000);
            Assert.AreEqual(ev.eccentricity, (decimal)0.000248);
            Assert.AreEqual(ev.orbitalinclination, (decimal)0.015659);
            Assert.AreEqual(ev.periapsis, (decimal)104.416656);
            Assert.AreEqual(ev.orbitalperiod, (decimal)48801056.000000);
            Assert.AreEqual(ev.rotationperiod, (decimal)79442.242188);
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
            Assert.AreEqual(692146368.000000M, theEvent.radius);
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
        }

        [TestMethod]
        public void TestJournalStarScan2()
        {
            string line = @"{ ""timestamp"":""2016-10-28T12:07:09Z"", ""event"":""Scan"", ""BodyName"":""Col 285 Sector CG-X d1-44"", ""DistanceFromArrivalLS"":0.000000, ""StarType"":""TTS"", ""StellarMass"":0.808594, ""Radius"":659162816.000000, ""AbsoluteMagnitude"":6.411560, ""Age_MY"":154, ""SurfaceTemperature"":4124.000000, ""RotationPeriod"":341417.281250, ""Rings"":[ { ""Name"":""Col 285 Sector CG-X d1-44 A Belt"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":1.1625e+13, ""InnerRad"":1.0876e+09, ""OuterRad"":2.4192e+09 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            StarScannedEvent theEvent = (StarScannedEvent)events[0];
            Assert.AreEqual(theEvent.radius, (decimal)659162816.0);
            Assert.AreEqual(theEvent.solarradius, StarClass.solarradius((decimal)659162816.000000));
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
            string line = @"{ ""timestamp"":""2017-04-14T19:34:32Z"",""event"":""Docked"",""StationName"":""Freeholm"",""StationType"":""AsteroidBase"",""StarSystem"":""Artemis"",""StationFaction"":""Artemis Empire Assembly"",""FactionState"":""Boom"",""StationGovernment"":""$government_Patronage;"",""StationGovernment_Localised"":""Patronage"",""StationAllegiance"":""Empire"",""StationEconomy"":""$economy_Industrial;"",""StationEconomy_Localised"":""Industrial"",""DistFromStarLS"":2527.211914,""StationServices"":[""Refuel""], ""MarketID"": 128169720, ""SystemAddress"": 3107509474002}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockedEvent theEvent = (DockedEvent)events[0];

            Assert.AreEqual("AsteroidBase", theEvent.model);

            Assert.AreEqual(128169720, theEvent.marketId);
            Assert.AreEqual(3107509474002, theEvent.systemAddress);
        }

        [TestMethod]
        public void TestJournalDocked2()
        {
            string line = @"{ ""timestamp"":""2017-04-14T19:34:32Z"",""event"":""Docked"",""StationName"":""Freeholm"",""StationType"":""AsteroidBase"",""StarSystem"":""Artemis"",""StationFaction"":""Artemis Empire Assembly"",""FactionState"":""Boom"",""StationGovernment"":""$government_Patronage;"",""StationGovernment_Localised"":""Patronage"",""StationAllegiance"":""Empire"",""StationEconomy"":""$economy_Industrial;"",""StationEconomy_Localised"":""Industrial"",""DistFromStarLS"":2527.211914,""StationServices"":[""Refuel""], ""MarketID"": 128169720, ""SystemAddress"": 3107509474002}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockedEvent theEvent = (DockedEvent)events[0];

            Assert.AreEqual("AsteroidBase", theEvent.model);
            Assert.AreEqual(1, theEvent.stationservices.Count);
            Assert.AreEqual("Refuel", theEvent.stationservices[0]);
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
            string line = @"{ ""timestamp"":""2017-05-05T16:07:37Z"", ""event"":""MissionAccepted"", ""Faction"":""Chick Ek Partnership"", ""Name"":""Mission_Sightseeing_Criminal_BOOM"", ""Commodity"":""$Wine_Name;"", ""Commodity_Localised"":""Wine"", ""Count"":3, ""DestinationSystem"":""HR 7221$MISSIONUTIL_MULTIPLE_FINAL_SEPARATOR;Tupa"", ""Expiry"":""2017-05-06T04:31:24Z"", ""Influence"":""Low"", ""Reputation"":""Med"", ""PassengerCount"":7, ""PassengerVIPs"":true, ""PassengerWanted"":true, ""PassengerType"":""Criminal"", ""MissionID"":134724902 }";

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
            Friend testFriend = new Friend();
            testFriend.name = @event.name;
            testFriend.status = @event.status;

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
		""RecoveringStates"": [{
			""State"": ""CivilWar"",
			""Trend"": 0
		}]
	},
	{
		""Name"": ""CartelofDiaguandri"",
		""FactionState"": ""None"",
		""Government"": ""Anarchy"",
		""Influence"": 0.009940,
		""Allegiance"": ""Independent"",
		""PendingStates"": [{
			""State"": ""Bust"",
			""Trend"": 0
		},
		{
			""State"": ""CivilUnrest"",
			""Trend"": 1
		}],
		""RecoveringStates"": [{
			""State"": ""CivilWar"",
			""Trend"": 0
		}]
	},
	{
		""Name"": ""RevolutionaryPartyofDiaguandri"",
		""FactionState"": ""None"",
		""Government"": ""Democracy"",
		""Influence"": 0.124254,
		""Allegiance"": ""Federation"",
		""PendingStates"": [{
			""State"": ""Boom"",
			""Trend"": 1
		},
		{
			""State"": ""Bust"",
			""Trend"": 1
		}]
	},
	{
		""Name"": ""TheBrotherhoodoftheDarkCircle"",
		""FactionState"": ""None"",
		""Government"": ""Corporate"",
		""Influence"": 0.093439,
		""Allegiance"": ""Independent"",
		""RecoveringStates"": [{
			""State"": ""CivilUnrest"",
			""Trend"": 1
		}]
	},
	{
		""Name"": ""EXO"",
		""FactionState"": ""Expansion"",
		""Government"": ""Democracy"",
		""Influence"": 0.527833,
		""Allegiance"": ""Independent"",
		""PendingStates"": [{
			""State"": ""Boom"",
			""Trend"": 1
		}]
	}],
	""SystemFaction"": ""EXO"",
	""FactionState"": ""Expansion""
}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            JumpedEvent jumpedEvent = (JumpedEvent)events[0];
            Assert.AreEqual("Diaguandri", jumpedEvent.system);
            Assert.AreEqual(670417429889, jumpedEvent.systemAddress);
            Assert.AreEqual(-41.06250M, jumpedEvent.x);
            Assert.AreEqual(-62.15625M, jumpedEvent.y);
            Assert.AreEqual(-103.25000M, jumpedEvent.z);
            Assert.AreEqual("Independent", jumpedEvent.allegiance);
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
    }
}
