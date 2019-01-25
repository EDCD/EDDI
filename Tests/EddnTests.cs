using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rollbar;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class EddnTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        class MockStarService : StarSystemRepository
        {
            public StarSystem GetOrCreateStarSystem(string name, bool refreshIfOutdated = true)
            {
                return GetStarSystem(name, refreshIfOutdated);
            }

            public List<StarSystem> GetOrCreateStarSystems(string[] names, bool refreshIfOutdated = true)
            {
                throw new NotImplementedException();
            }

            public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true)
            {
                return GetStarSystem(name, fetchIfMissing);
            }

            public List<StarSystem> GetOrFetchStarSystems(string[] names, bool fetchIfMissing = true)
            {
                throw new NotImplementedException();
            }

            public StarSystem GetStarSystem(string name, bool refreshIfOutdated = true)
            {
                switch(name)
                {
                    case "Artemis":
                        {
                            StarSystem result = new StarSystem();
                            result.name = "Artemis";
                            result.systemAddress = 3107509474002;
                            result.x = 14.28125M;
                            result.y = -63.1875M;
                            result.z = -24.875M;
                            return result;
                        }
                    case "Diaguandri":
                        {
                            StarSystem result = new StarSystem();
                            result.name = "Diaguandri";
                            result.systemAddress = 670417429889;
                            result.x = -41.06250M;
                            result.y = -62.15625M;
                            result.z = -103.25000M;

                            Station station = new Station();
                            station.name = "Ray Gateway";
                            station.marketId = 3223343616;
                            result.stations.Add(station);

                            return result;
                        }
                    case "Sol":
                        {
                            StarSystem result = new StarSystem();
                            result.name = "Sol";
                            result.systemAddress = 10477373803;
                            result.x = 0.0M;
                            result.y = 0.0M;
                            result.z = 0.0M;
                            return result;
                        }
                    case "Pleiades Sector HR-W d1-79":
                        {
                            StarSystem result = new StarSystem();
                            result.name = "Pleiades Sector HR-W d1-79";
                            result.systemAddress = 2724879894859;
                            result.x = -80.62500M;
                            result.y = -146.65625M;
                            result.z = -343.25000M;
                            return result;
                        }
                    default:
                        break;
                }
                return null;
            }

            public List<StarSystem> GetStarSystems(string[] names, bool refreshIfOutdated = true)
            {
                throw new NotImplementedException();
            }

            public void LeaveStarSystem(StarSystem starSystem)
            {
                throw new NotImplementedException();
            }

            public void SaveStarSystem(StarSystem starSystem)
            {
                throw new NotImplementedException();
            }

            public void SaveStarSystems(List<StarSystem> starSystems)
            {
                throw new NotImplementedException();
            }
        }

        private EDDNResponder.EDDNResponder makeTestEDDNResponder()
        {
            EDDNResponder.EDDNResponder responder = new EDDNResponder.EDDNResponder(new MockStarService());
            return responder;
        }

        [TestMethod()]
        public void TestEDDNResponderGoodMatch()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemAddress", 10477373803);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmAddressAndCoordinates", new object[] { "Sol" });

            Assert.IsTrue(confirmed);
            Assert.AreEqual("Sol", responder.systemName);
            Assert.AreEqual(10477373803, responder.systemAddress);
            Assert.AreEqual(0.0M, responder.systemX);
            Assert.AreEqual(0.0M, responder.systemY);
            Assert.AreEqual(0.0M, responder.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderBadStarPos()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            // Intentionally place our EDDN responder in a state with incorrect coordinates (from Sol).
            // The 'Docked' event does include systemName and systemAddress, so we set those here.
            privateObject.SetFieldOrProperty("systemName", "Artemis");
            privateObject.SetFieldOrProperty("systemAddress", 3107509474002);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmAddressAndCoordinates", new object[] { "Artemis" });

            Assert.IsFalse(confirmed);
            Assert.AreEqual("Artemis", responder.systemName);
            Assert.AreEqual(3107509474002, responder.systemAddress);
            Assert.IsNull(responder.systemX);
            Assert.IsNull(responder.systemY);
            Assert.IsNull(responder.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderBadSystemAddress()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            // Intentionally place our EDDN responder in a state with incorrect SystemAddress (from Artemis).
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemAddress", 3107509474002);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmAddressAndCoordinates", new object[] { "Sol" });

            Assert.IsFalse(confirmed);
            Assert.AreEqual("Sol", responder.systemName);
            Assert.IsNull(responder.systemAddress);
            Assert.AreEqual(0.0M, responder.systemX);
            Assert.AreEqual(0.0M, responder.systemY);
            Assert.AreEqual(0.0M, responder.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderUnverifiableData()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemAddress", 10477373803);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmAddressAndCoordinates", new object[] { "Not in this galaxy" });

            Assert.IsFalse(confirmed);
            Assert.IsNull(responder.systemName);
            Assert.IsNull(responder.systemAddress);
            Assert.IsNull(responder.systemX);
            Assert.IsNull(responder.systemY);
            Assert.IsNull(responder.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderBadName()
        {
            // Tests that procedurally generated body names match the procedurally generated system name
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            privateObject.SetFieldOrProperty("systemName", "Pleiades Sector HR-W d1-79");
            privateObject.SetFieldOrProperty("systemAddress", 2724879894859);
            privateObject.SetFieldOrProperty("systemX", -80.62500M);
            privateObject.SetFieldOrProperty("systemY", -146.65625M);
            privateObject.SetFieldOrProperty("systemZ", -343.25000M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmScan", new object[] { "Hyades Sector DL-X b1-2" });

            Assert.IsFalse(confirmed);
            Assert.IsNull(responder.systemName);
            Assert.IsNull(responder.systemAddress);
            Assert.IsNull(responder.systemX);
            Assert.IsNull(responder.systemY);
            Assert.IsNull(responder.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderDockedEvent()
        {
            string line = @"{
	            ""timestamp"": ""2018-07-30T04:50:32Z"",
	            ""event"": ""FSDJump"",
	            ""StarSystem"": ""Diaguandri"",
	            ""SystemAddress"": 670417429889,
	            ""StarPos"": [-41.06250, -62.15625, -103.25000],
	            ""SystemAllegiance"": ""Independent"",
	            ""SystemEconomy"": ""$economy_HighTech;"",
	            ""SystemEconomy_Localised"": ""High Tech"",
	            ""SystemSecondEconomy"": ""$economy_Refinery;"",
	            ""SystemSecondEconomy_Localised"": ""Refinery"",
	            ""SystemGovernment"": ""$government_Democracy;"",
	            ""SystemGovernment_Localised"": ""Democracy"",
	            ""SystemSecurity"": ""$SYSTEM_SECURITY_medium;"",
	            ""SystemSecurity_Localised"": ""Medium Security"",
	            ""Population"": 10303479,
	            ""JumpDist"": 8.018,
	            ""FuelUsed"": 0.917520,
	            ""FuelLevel"": 29.021893,
	            ""Factions"": [{
		            ""Name"": ""Diaguandri Interstellar"",
		            ""FactionState"": ""Election"",
		            ""Government"": ""Corporate"",
		            ""Influence"": 0.072565,
		            ""Allegiance"": ""Independent"",
		            ""RecoveringStates"": [{
			            ""State"": ""Boom"",
			            ""Trend"": 0
		            }]
	            },
	            {
		            ""Name"": ""People's MET 20 Liberals"",
		            ""FactionState"": ""Boom"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.092445,
		            ""Allegiance"": ""Federation""
	            },
	            {
		            ""Name"": ""Pilots Federation Local Branch"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.000000,
		            ""Allegiance"": ""PilotsFederation""
	            },
	            {
		            ""Name"": ""Natural Diaguandri Regulatory State"",
		            ""FactionState"": ""CivilWar"",
		            ""Government"": ""Dictatorship"",
		            ""Influence"": 0.009940,
		            ""Allegiance"": ""Independent""
	            },
	            {
		            ""Name"": ""Cartel of Diaguandri"",
		            ""FactionState"": ""CivilWar"",
		            ""Government"": ""Anarchy"",
		            ""Influence"": 0.009940,
		            ""Allegiance"": ""Independent"",
		            ""PendingStates"": [{
			            ""State"": ""Bust"",
			            ""Trend"": 0
		            }]
	            },
	            {
		            ""Name"": ""Revolutionary Party of Diaguandri"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.050696,
		            ""Allegiance"": ""Federation"",
		            ""PendingStates"": [{
			            ""State"": ""Bust"",
			            ""Trend"": 1
		            }]
	            },
	            {
		            ""Name"": ""The Brotherhood of the Dark Circle"",
		            ""FactionState"": ""Election"",
		            ""Government"": ""Corporate"",
		            ""Influence"": 0.078529,
		            ""Allegiance"": ""Independent"",
		            ""PendingStates"": [{
			            ""State"": ""CivilUnrest"",
			            ""Trend"": 0
		            }],
		            ""RecoveringStates"": [{
			            ""State"": ""Boom"",
			            ""Trend"": 0
		            }]
	            },
	            {
		            ""Name"": ""EXO"",
		            ""FactionState"": ""Boom"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.685885,
		            ""Allegiance"": ""Independent"",
		            ""PendingStates"": [{
			            ""State"": ""Expansion"",
			            ""Trend"": 0
		            }]
	            }],
	            ""SystemFaction"": {
		            ""Name"": ""EXO"",
		            ""FactionState"": ""Boom""
	            }
            }";

            string line2 = @"{
	            ""timestamp"": ""2018-07-30T06: 07: 47Z"",
	            ""event"": ""Docked"",
	            ""StationName"": ""Ray Gateway"",
	            ""StationType"": ""Coriolis"",
	            ""StarSystem"": ""Diaguandri"",
	            ""SystemAddress"": 670417429889,
	            ""MarketID"": 3223343616,
	            ""StationFaction"": {
		            ""Name"": ""EXO"",
		            ""FactionState"": ""Boom""
	            },
	            ""StationGovernment"": ""$government_Democracy;"",
	            ""StationGovernment_Localised"": ""Democracy"",
	            ""StationServices"": [""Dock"",
	            ""Autodock"",
	            ""BlackMarket"",
	            ""Commodities"",
	            ""Contacts"",
	            ""Exploration"",
	            ""Missions"",
	            ""Outfitting"",
	            ""CrewLounge"",
	            ""Rearm"",
	            ""Refuel"",
	            ""Repair"",
	            ""Shipyard"",
	            ""Tuning"",
	            ""Workshop"",
	            ""MissionsGenerated"",
	            ""FlightController"",
	            ""StationOperations"",
	            ""Powerplay"",
	            ""SearchAndRescue"",
	            ""MaterialTrader"",
	            ""TechBroker""],
	            ""StationEconomy"": ""$economy_HighTech;"",
	            ""StationEconomy_Localised"": ""HighTech"",
	            ""StationEconomies"": [{
		            ""Name"": ""$economy_HighTech;"",
		            ""Name_Localised"": ""HighTech"",
		            ""Proportion"": 0.800000
	            },
	            {
		            ""Name"": ""$economy_Refinery;"",
		            ""Name_Localised"": ""Refinery"",
		            ""Proportion"": 0.200000
	            }],
	            ""DistFromStarLS"": 566.487976
            }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            JumpedEvent @jumpedEvent = (JumpedEvent)events[0];

            events = JournalMonitor.ParseJournalEntry(line2);
            Assert.IsTrue(events.Count == 1);
            DockedEvent @dockedEvent = (DockedEvent)events[0];

            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            responder.Handle(@jumpedEvent);
            responder.Handle(@dockedEvent);

            // Test that data available from the event is set correctly
            Assert.AreEqual("Diaguandri", responder.systemName);
            Assert.AreEqual(670417429889, responder.systemAddress);
            Assert.AreEqual("Ray Gateway", responder.stationName);
            Assert.AreEqual(3223343616, responder.marketId);

            // Test metadata not in the event itself but retrieved from memory and confirmed by our local database
            Assert.AreEqual(-41.06250M, responder.systemX);
            Assert.AreEqual(-62.15625M, responder.systemY);
            Assert.AreEqual(-103.25000M, responder.systemZ);
        }

        [TestMethod()]
        public void TestMyReputationDataStripping()
        {
            string line = @"{
	            ""timestamp"": ""2018-11-19T01: 06: 17Z"",
	            ""event"": ""Location"",
	            ""Docked"": false,
	            ""StarSystem"": ""BD+48738"",
	            ""SystemAddress"": 908352033466,
	            ""StarPos"": [-93.53125, -24.46875, -114.71875],
	            ""SystemAllegiance"": ""Independent"",
	            ""SystemEconomy"": ""$economy_Extraction;"",
	            ""SystemEconomy_Localised"": ""Extraction"",
	            ""SystemSecondEconomy"": ""$economy_Industrial;"",
	            ""SystemSecondEconomy_Localised"": ""Industrial"",
	            ""SystemGovernment"": ""$government_Cooperative;"",
	            ""SystemGovernment_Localised"": ""Cooperative"",
	            ""SystemSecurity"": ""$SYSTEM_SECURITY_medium;"",
	            ""SystemSecurity_Localised"": ""MediumSecurity"",
	            ""Population"": 2530147,
	            ""Body"": ""LinnaeusEnterprise"",
	            ""BodyID"": 28,
	            ""BodyType"": ""Station"",
	            ""Factions"": [{
		            ""Name"": ""IndependentBD+48738Liberals"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.037000,
		            ""Allegiance"": ""Federation"",
		            ""Happiness"": ""$Faction_HappinessBand2;"",
		            ""Happiness_Localised"": ""Happy"",
		            ""MyReputation"": 0.000000
	            },
	            {
		            ""Name"": ""PilotsFederationLocalBranch"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Democracy"",
		            ""Influence"": 0.000000,
		            ""Allegiance"": ""PilotsFederation"",
		            ""Happiness"": """",
		            ""MyReputation"": 100.000000
	            },
	            {
		            ""Name"": ""NewBD+48738Focus"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Dictatorship"",
		            ""Influence"": 0.046000,
		            ""Allegiance"": ""Independent"",
		            ""Happiness"": ""$Faction_HappinessBand2;"",
		            ""Happiness_Localised"": ""Happy"",
		            ""MyReputation"": 0.000000
	            },
	            {
		            ""Name"": ""BD+48738CrimsonTravelCo"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Corporate"",
		            ""Influence"": 0.032000,
		            ""Allegiance"": ""Independent"",
		            ""Happiness"": ""$Faction_HappinessBand2;"",
		            ""Happiness_Localised"": ""Happy"",
		            ""MyReputation"": 0.000000
	            },
	            {
		            ""Name"": ""BD+48738CrimsonPosse"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Anarchy"",
		            ""Influence"": 0.010000,
		            ""Allegiance"": ""Independent"",
		            ""Happiness"": ""$Faction_HappinessBand2;"",
		            ""Happiness_Localised"": ""Happy"",
		            ""MyReputation"": 0.000000
	            },
	            {
		            ""Name"": ""Laniakea"",
		            ""FactionState"": ""None"",
		            ""Government"": ""Cooperative"",
		            ""Influence"": 0.875000,
		            ""Allegiance"": ""Independent"",
		            ""Happiness"": ""$Faction_HappinessBand2;"",
		            ""Happiness_Localised"": ""Happy"",
		            ""MyReputation"": 0.000000,
		            ""PendingStates"": [{
			            ""State"": ""Expansion"",
			            ""Trend"": 0
		            }]
	            }],
	            ""SystemFaction"": {
		            ""Name"": ""Laniakea"",
		            ""FactionState"": ""None""
	            }
            }";
            IDictionary<string, object> data = Utilities.Deserializtion.DeserializeData(line);

            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            data = (IDictionary<string, object>)privateObject.Invoke("StripPersonalData", new object[] { data });

            data.TryGetValue("Factions", out object factionsVal);
            if (factionsVal != null)
            {
                var factions = (List<object>)factionsVal;
                foreach (object faction in factions)
                {
                    Assert.IsFalse(((IDictionary<string, object>)faction).ContainsKey("MyReputation"));
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestUnhandledEvents()
        {
            string location = @"{ ""timestamp"":""2018-12-16T20:08:31Z"", ""event"":""Location"", ""Docked"":false, ""StarSystem"":""Pleiades Sector GW-W c1-4"", ""SystemAddress"":1183229809290, ""StarPos"":[-81.62500,-151.31250,-383.53125], ""SystemAllegiance"":"""", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""Body"":""Pleiades Sector GW-W c1-4"", ""BodyID"":0, ""BodyType"":""Star"" }";
            string jump = @"{ ""timestamp"":""2018-12-16T20:10:15Z"", ""event"":""FSDJump"", ""StarSystem"":""Pleiades Sector HR-W d1-79"", ""SystemAddress"":2724879894859, ""StarPos"":[-80.62500,-146.65625,-343.25000], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_Extraction;"", ""SystemEconomy_Localised"":""Extraction"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_Prison;"", ""SystemGovernment_Localised"":""Detention Centre"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Population"":0, ""JumpDist"":40.562, ""FuelUsed"":2.827265, ""FuelLevel"":11.702736, ""Factions"":[ { ""Name"":""Independent Detention Foundation"", ""FactionState"":""None"", ""Government"":""Prison"", ""Influence"":0.000000, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Pilots Federation Local Branch"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.000000, ""Allegiance"":""PilotsFederation"", ""Happiness"":"""", ""MyReputation"":100.000000 } ], ""SystemFaction"":""Independent Detention Foundation"" }";
            string scan = @"{ ""timestamp"":""2018-12-16T20:10:21Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Pleiades Sector HR-W d1-79"", ""BodyID"":0, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""F"", ""StellarMass"":1.437500, ""Radius"":855515008.000000, ""AbsoluteMagnitude"":3.808395, ""Age_MY"":1216, ""SurfaceTemperature"":6591.000000, ""Luminosity"":""Vab"", ""RotationPeriod"":261918.156250, ""AxialTilt"":0.000000 }";
            string scan2 = @"{ ""timestamp"":""2018-12-16T20:28:02Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Hyades Sector DL-X b1-2"", ""BodyID"":0, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""M"", ""StellarMass"":0.367188, ""Radius"":370672928.000000, ""AbsoluteMagnitude"":9.054306, ""Age_MY"":586, ""SurfaceTemperature"":2993.000000, ""Luminosity"":""Va"", ""RotationPeriod"":167608.859375, ""AxialTilt"":0.000000, ""Rings"":[ { ""Name"":""Hyades Sector DL-X b1-2 A Belt"", ""RingClass"":""eRingClass_MetalRich"", ""MassMT"":5.4671e+13, ""InnerRad"":7.1727e+08, ""OuterRad"":1.728e+09 }, { ""Name"":""Hyades Sector DL-X b1-2 B Belt"", ""RingClass"":""eRingClass_Icy"", ""MassMT"":8.7822e+14, ""InnerRad"":6.3166e+10, ""OuterRad"":1.5917e+11 } ] }";

            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            object[] arguments;

            UnhandledEvent unhandledLocation = new UnhandledEvent(DateTime.UtcNow, "Location") { raw = location };
            arguments = new object[] { unhandledLocation };
            privateObject.Invoke("handleRawEvent", arguments);
            Assert.AreEqual("Pleiades Sector GW-W c1-4", responder.systemName);
            Assert.AreEqual(1183229809290, responder.systemAddress);
            Assert.AreEqual(-81.62500M, responder.systemX);
            Assert.AreEqual(-151.31250M, responder.systemY);
            Assert.AreEqual(-383.53125M, responder.systemZ);
            Assert.IsNull(responder.stationName);
            Assert.IsNull(responder.marketId);

            UnhandledEvent unhandledJump = new UnhandledEvent(DateTime.UtcNow, "FSDJump") { raw = jump };
            arguments = new object[] { unhandledJump };
            privateObject.Invoke("handleRawEvent", arguments);
            Assert.AreEqual("Pleiades Sector HR-W d1-79", responder.systemName);
            Assert.AreEqual(2724879894859, responder.systemAddress);
            Assert.AreEqual(-80.62500M, responder.systemX);
            Assert.AreEqual(-146.65625M, responder.systemY);
            Assert.AreEqual(-343.25000M, responder.systemZ);
            Assert.IsNull(responder.stationName);
            Assert.IsNull(responder.marketId);

            UnhandledEvent unhandledScan = new UnhandledEvent(DateTime.UtcNow, "Scan") { raw = scan };
            arguments = new object[] { unhandledScan };
            privateObject.Invoke("handleRawEvent", arguments);
            Assert.AreEqual("Pleiades Sector HR-W d1-79", responder.systemName);
            Assert.AreEqual(2724879894859, responder.systemAddress);
            Assert.AreEqual(-80.62500M, responder.systemX);
            Assert.AreEqual(-146.65625M, responder.systemY);
            Assert.AreEqual(-343.25000M, responder.systemZ);
            Assert.IsNull(responder.stationName);
            Assert.IsNull(responder.marketId);

            // Deliberately scan a procedurally generated body that doesn't match our last known location & verify heuristics catch it
            UnhandledEvent unhandledScan2 = new UnhandledEvent(DateTime.UtcNow, "Scan") { raw = scan2 };
            arguments = new object[] { unhandledScan2 };
            privateObject.Invoke("handleRawEvent", arguments);
            Assert.IsNull(responder.systemName);
            Assert.IsNull(responder.systemAddress);
            Assert.IsNull(responder.systemX);
            Assert.IsNull(responder.systemY);
            Assert.IsNull(responder.systemZ);
            Assert.IsNull(responder.stationName);
            Assert.IsNull(responder.marketId);

            // Reset our position by re-stating the `FSDJump` event
            arguments = new object[] { unhandledJump };
            privateObject.Invoke("handleRawEvent", arguments);

            // Deliberately create a mismatch between the system and coordinates, 
            // using the coordinates from our Location event and other data from our FSDJump event
            privateObject.SetFieldOrProperty("systemX", -81.62500M);
            privateObject.SetFieldOrProperty("systemY", -151.31250M);
            privateObject.SetFieldOrProperty("systemZ", -383.53125M);

            // Deliberately scan a body while our coordinates are in a bad state
            arguments = new object[] { unhandledScan };
            privateObject.Invoke("handleRawEvent", arguments);
            Assert.IsNull(responder.systemX);
            Assert.IsNull(responder.systemY);
            Assert.IsNull(responder.systemZ);

            // Reset our position by re-stating the `FSDJump` event
            arguments = new object[] { unhandledJump };
            privateObject.Invoke("handleRawEvent", arguments);

            // Deliberately create a mismatch between the system name and address, 
            // using the address from our Location event and other data from our FSDJump event
            privateObject.SetFieldOrProperty("systemAddress", 1183229809290);

            // Deliberately scan a body while our system address is in a bad state
            arguments = new object[] { unhandledScan };
            privateObject.Invoke("handleRawEvent", arguments);
            Assert.IsNull(responder.systemAddress);
        }
    }
}

