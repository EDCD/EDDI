using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEddnResponder;
using EddiEddnResponder.Schemas;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Properties;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class EddnTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        class MockStarService : StarSystemRepository
        {
            public StarSystem GetOrCreateStarSystem(string name, bool fetchIfMissing = true, bool refreshIfOutdated = true)
            {
                return GetStarSystem(name, refreshIfOutdated);
            }

            public List<StarSystem> GetOrCreateStarSystems(string[] names, bool fetchIfMissing = true, bool refreshIfOutdated = true)
            {
                throw new NotImplementedException();
            }

            public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true, bool refreshIfOutdated = true)
            {
                return GetStarSystem(name, fetchIfMissing);
            }

            public List<StarSystem> GetOrFetchStarSystems(string[] names, bool fetchIfMissing = true, bool refreshIfOutdated = true)
            {
                throw new NotImplementedException();
            }

            public StarSystem GetStarSystem(string name, bool refreshIfOutdated = true)
            {
                switch (name)
                {
                    case "Artemis":
                        {
                            StarSystem result = new StarSystem();
                            result.systemname = "Artemis";
                            result.systemAddress = 3107509474002;
                            result.x = 14.28125M;
                            result.y = -63.1875M;
                            result.z = -24.875M;
                            return result;
                        }
                    case "Diaguandri":
                        {
                            StarSystem result = new StarSystem();
                            result.systemname = "Diaguandri";
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
                            result.systemname = "Sol";
                            result.systemAddress = 10477373803;
                            result.x = 0.0M;
                            result.y = 0.0M;
                            result.z = 0.0M;
                            return result;
                        }
                    case "Pleiades Sector HR-W d1-79":
                        {
                            StarSystem result = new StarSystem();
                            result.systemname = "Pleiades Sector HR-W d1-79";
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

        private EDDNResponder makeTestEDDNResponder()
        {
            EDDNResponder responder = new EDDNResponder(new MockStarService(), true);
            return responder;
        }

        [TestMethod]
        public void TestEddnSchemaInitialization()
        {
            EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            var schemas = (IEnumerable<object>)privateObject.GetField("schemas");
            var capiSchemas = (IEnumerable<object>)privateObject.GetField("capiSchemas");

            Assert.IsTrue(schemas.Any());
            Assert.IsTrue(capiSchemas.Any());
        }

        [TestMethod()]
        public void TestEDDNResponderGoodMatch()
        {
            EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder.eddnState.Location);
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemAddress", (ulong)10477373803);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmAddressAndCoordinates");

            Assert.IsTrue(confirmed);
            Assert.AreEqual("Sol", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)10477373803, responder.eddnState.Location.systemAddress);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemX);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemY);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderBadStarPos()
        {
            EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder.eddnState.Location);
            // Intentionally place our EDDN responder in a state with incorrect coordinates (from Sol).
            // The 'Docked' event does include systemName and systemAddress, so we set those here.
            privateObject.SetFieldOrProperty("systemName", "Artemis");
            privateObject.SetFieldOrProperty("systemAddress", (ulong)3107509474002);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmAddressAndCoordinates");

            Assert.IsFalse(confirmed);
            Assert.AreEqual("Artemis", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)3107509474002, responder.eddnState.Location.systemAddress);
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderBadSystemAddress()
        {
            EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder.eddnState.Location);
            // Intentionally place our EDDN responder in a state with incorrect SystemAddress (from Artemis).
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemAddress", (ulong)3107509474002);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmAddressAndCoordinates");

            Assert.IsFalse(confirmed);
            Assert.AreEqual("Sol", responder.eddnState.Location.systemName);
            Assert.IsNull(responder.eddnState.Location.systemAddress);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemX);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemY);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderBadName()
        {
            // Tests that procedurally generated body names match the procedurally generated system name
            EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder.eddnState.Location);
            privateObject.SetFieldOrProperty("systemName", "Pleiades Sector HR-W d1-79");
            privateObject.SetFieldOrProperty("systemAddress", (ulong)2724879894859);
            privateObject.SetFieldOrProperty("systemX", -80.62500M);
            privateObject.SetFieldOrProperty("systemY", -146.65625M);
            privateObject.SetFieldOrProperty("systemZ", -343.25000M);

            bool confirmed = (bool)privateObject.Invoke("ConfirmScan", new object[] { "Hyades Sector DL-X b1-2 A 1" });

            Assert.IsFalse(confirmed);
            Assert.IsNull(responder.eddnState.Location.systemName);
            Assert.IsNull(responder.eddnState.Location.systemAddress);
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);
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
                ""timestamp"": ""2018-07-30T06:07:47Z"",
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

            EDDNResponder responder = makeTestEDDNResponder();
            responder.Handle(@jumpedEvent);
            responder.Handle(@dockedEvent);

            // Test that data available from the event is set correctly
            Assert.AreEqual("Diaguandri", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)670417429889, responder.eddnState.Location.systemAddress);
            Assert.AreEqual("Ray Gateway", responder.eddnState.Location.stationName);
            Assert.AreEqual(3223343616, responder.eddnState.Location.marketId);

            // Test metadata not in the event itself but retrieved from memory and confirmed by our local database
            Assert.AreEqual(-41.06250M, responder.eddnState.Location.systemX);
            Assert.AreEqual(-62.15625M, responder.eddnState.Location.systemY);
            Assert.AreEqual(-103.25000M, responder.eddnState.Location.systemZ);
        }

        [TestMethod()]
        public void TestMyReputationDataStripping()
        {
            string line = @"{
                ""timestamp"": ""2018-11-19T01:06:17Z"",
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

            IDictionary<string, object> data = Deserializtion.DeserializeData(line);

            EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder.eddnState.PersonalData);
            data = (IDictionary<string, object>)privateObject.Invoke("Strip", new object[] { data, "Location" });

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
        public void TestStripPersonalData()
        {
            // we fail if any key with the value "bad" survives the test
            IDictionary<string, object> data = new Dictionary<string, object>()
            {
                { "good key", "good" },
                { "ActiveFine", "bad" },
                { "CockpitBreach", "bad" },
                { "BoostUsed", "bad" },
                { "FuelLevel", "bad" },
                { "FuelUsed", "bad" },
                { "JumpDist", "bad" },
                { "Wanted", "bad" },
                { "Latitude", "bad" },
                { "Longitude", "bad" },
                {
                    "Factions", new List<object>()
                    {
                        new Dictionary<string, object>()
                        {
                            { "good key", "good"},
                            { "MyReputation", "bad" },
                            { "SquadronFaction", "bad" },
                            { "HappiestSystem", "bad" },
                            { "HomeSystem", "bad" },
                            { "blah_Localised", "bad" },
                        }
                    }
                }
            };

            EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder.eddnState.PersonalData);
            data = (IDictionary<string, object>)privateObject.Invoke("Strip", new object[] { data, null });

            void testKeyValuePair(KeyValuePair<string, object> kvp)
            {
                if (kvp.Value as string == "bad")
                {
                    Assert.Fail($"key '{kvp.Key}' should have been stripped.");
                }
            }

            void testDictionary(IDictionary<string, object> dict)
            {
                foreach (KeyValuePair<string, object> kvp in dict)
                {
                    var value = kvp.Value;
                    if (value is string)
                    {
                        testKeyValuePair(kvp);
                    }
                    if (value is IList<object>)
                    {
                        IList<object> list = value as List<object>;
                        Assert.IsNotNull(list);
                        foreach (object item in list)
                        {
                            testDictionary((IDictionary<string, object>)item);
                        }
                    }
                }
            }

            testDictionary(data);
        }

        [TestMethod]
        public void TestRawJournalEventLocationData()
        {
            string location = @"{ ""timestamp"":""2018-12-16T20:08:31Z"", ""event"":""Location"", ""Docked"":false, ""StarSystem"":""Pleiades Sector GW-W c1-4"", ""SystemAddress"":1183229809290, ""StarPos"":[-81.62500,-151.31250,-383.53125], ""SystemAllegiance"":"""", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""Body"":""Pleiades Sector GW-W c1-4"", ""BodyID"":0, ""BodyType"":""Star"" }";
            string jump = @"{ ""timestamp"":""2018-12-16T20:10:15Z"", ""event"":""FSDJump"", ""StarSystem"":""Pleiades Sector HR-W d1-79"", ""SystemAddress"":2724879894859, ""StarPos"":[-80.62500,-146.65625,-343.25000], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_Extraction;"", ""SystemEconomy_Localised"":""Extraction"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_Prison;"", ""SystemGovernment_Localised"":""Detention Centre"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Population"":0, ""JumpDist"":40.562, ""FuelUsed"":2.827265, ""FuelLevel"":11.702736, ""Factions"":[ { ""Name"":""Independent Detention Foundation"", ""FactionState"":""None"", ""Government"":""Prison"", ""Influence"":0.000000, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Pilots Federation Local Branch"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.000000, ""Allegiance"":""PilotsFederation"", ""Happiness"":"""", ""MyReputation"":100.000000 } ], ""SystemFaction"":""Independent Detention Foundation"" }";
            string scan = @"{ ""timestamp"":""2018-12-16T20:10:21Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Pleiades Sector HR-W d1-79"", ""BodyID"":0, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""F"", ""StellarMass"":1.437500, ""Radius"":855515008.000000, ""AbsoluteMagnitude"":3.808395, ""Age_MY"":1216, ""SurfaceTemperature"":6591.000000, ""Luminosity"":""Vab"", ""RotationPeriod"":261918.156250, ""AxialTilt"":0.000000 }";
            string scan2 = @"{ ""timestamp"":""2018-12-16T20:28:02Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Hyades Sector DL-X b1-2"", ""BodyID"":0, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""M"", ""StellarMass"":0.367188, ""Radius"":370672928.000000, ""AbsoluteMagnitude"":9.054306, ""Age_MY"":586, ""SurfaceTemperature"":2993.000000, ""Luminosity"":""Va"", ""RotationPeriod"":167608.859375, ""AxialTilt"":0.000000, ""Rings"":[ { ""Name"":""Hyades Sector DL-X b1-2 A Belt"", ""RingClass"":""eRingClass_MetalRich"", ""MassMT"":5.4671e+13, ""InnerRad"":7.1727e+08, ""OuterRad"":1.728e+09 }, { ""Name"":""Hyades Sector DL-X b1-2 B Belt"", ""RingClass"":""eRingClass_Icy"", ""MassMT"":8.7822e+14, ""InnerRad"":6.3166e+10, ""OuterRad"":1.5917e+11 } ] }";
            string jump2 = @"{ ""timestamp"":""2019-01-27T07:23:38Z"", ""event"":""FSDJump"", ""StarSystem"":""Omega Sector OO-G a11-0"", ""SystemAddress"":5213552532472, ""StarPos"":[-1433.53125,-94.15625,5326.34375], ""SystemAllegiance"":"""", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""JumpDist"":56.848, ""FuelUsed"":4.741170, ""FuelLevel"":21.947533 }";
            string scan3 = @"{""timestamp"":""2019-01-27T07:07:46Z"",""event"":""Scan"",""ScanType"":""AutoScan"",""BodyName"":""Omega Sector DM-M b7-16 A B Belt Cluster 8"",""BodyID"":23,""Parents"":[{""Ring"":15},{""Star"":1},{""Null"":0}],""DistanceFromArrivalLS"":646.57074}";

            EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder.eddnState.Location);
            object[] arguments;

            var unhandledLocation = Deserializtion.DeserializeData(location);
            arguments = new object[] { "Location", unhandledLocation };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsTrue(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);
            Assert.AreEqual("Pleiades Sector GW-W c1-4", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)1183229809290, responder.eddnState.Location.systemAddress);
            Assert.AreEqual(-81.62500M, responder.eddnState.Location.systemX);
            Assert.AreEqual(-151.31250M, responder.eddnState.Location.systemY);
            Assert.AreEqual(-383.53125M, responder.eddnState.Location.systemZ);
            Assert.IsNull(responder.eddnState.Location.stationName);
            Assert.IsNull(responder.eddnState.Location.marketId);

            var unhandledJump = Deserializtion.DeserializeData(jump);
            arguments = new object[] { "FSDJump", unhandledJump };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsTrue(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);
            Assert.AreEqual("Pleiades Sector HR-W d1-79", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)2724879894859, responder.eddnState.Location.systemAddress);
            Assert.AreEqual(-80.62500M, responder.eddnState.Location.systemX);
            Assert.AreEqual(-146.65625M, responder.eddnState.Location.systemY);
            Assert.AreEqual(-343.25000M, responder.eddnState.Location.systemZ);
            Assert.IsNull(responder.eddnState.Location.stationName);
            Assert.IsNull(responder.eddnState.Location.marketId);

            var unhandledScan = Deserializtion.DeserializeData(scan);
            arguments = new object[] { "Scan", unhandledScan };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsTrue(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);
            Assert.AreEqual("Pleiades Sector HR-W d1-79", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)2724879894859, responder.eddnState.Location.systemAddress);
            Assert.AreEqual(-80.62500M, responder.eddnState.Location.systemX);
            Assert.AreEqual(-146.65625M, responder.eddnState.Location.systemY);
            Assert.AreEqual(-343.25000M, responder.eddnState.Location.systemZ);
            Assert.IsNull(responder.eddnState.Location.stationName);
            Assert.IsNull(responder.eddnState.Location.marketId);

            // Deliberately scan a procedurally generated body that doesn't match our last known location & verify heuristics catch it
            var unhandledScan2 = Deserializtion.DeserializeData(scan2);
            arguments = new object[] { "Scan", unhandledScan2 };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsFalse(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);
            Assert.IsNull(responder.eddnState.Location.systemName);
            Assert.IsNull(responder.eddnState.Location.systemAddress);
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);
            Assert.IsNull(responder.eddnState.Location.stationName);
            Assert.IsNull(responder.eddnState.Location.marketId);

            // Reset our position by re-stating the `FSDJump` event
            arguments = new object[] { "FSDJump", unhandledJump };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsTrue(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);

            // Deliberately create a mismatch between the system and coordinates, 
            // using the coordinates from our Location event and other data from our FSDJump event
            privateObject.SetFieldOrProperty("systemX", -81.62500M);
            privateObject.SetFieldOrProperty("systemY", -151.31250M);
            privateObject.SetFieldOrProperty("systemZ", -383.53125M);

            // Deliberately scan a body while our coordinates are in a bad state
            arguments = new object[] { "Scan", unhandledScan };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsFalse(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);

            // Reset our position by re-stating the `FSDJump` event
            arguments = new object[] { "FSDJump", unhandledJump };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsTrue(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);

            // Deliberately create a mismatch between the system name and address, 
            // using the address from our Location event and other data from our FSDJump event
            privateObject.SetFieldOrProperty("systemAddress", (ulong)1183229809290);

            // Deliberately scan a body while our system address is in a bad state
            arguments = new object[] { "Scan", unhandledScan };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsFalse(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);
            Assert.IsNull(responder.eddnState.Location.systemAddress);

            // Reset our position by stating another `FSDJump` event
            var unhandledJump2 = Deserializtion.DeserializeData(jump2);
            arguments = new object[] { "FSDJump", unhandledJump2 };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsTrue(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);

            // Scan a belt cluster from a different star system
            var unhandledScan3 = Deserializtion.DeserializeData(scan3);
            arguments = new object[] { "Scan", unhandledScan3 };
            privateObject.Invoke("GetLocationInfo", arguments);
            Assert.IsFalse(privateObject.Invoke("CheckLocationData", arguments) as bool? ?? false);
            Assert.IsNull(responder.eddnState.Location.systemName);
            Assert.IsNull(responder.eddnState.Location.systemAddress);
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);
        }

        [TestMethod]
        public void commoditySchemaJournalTest()
        {
            // Set up our schema
            var commoditySchema = makeTestEDDNResponder()
                .schemas.FirstOrDefault(s => s.GetType() == typeof(CommoditySchema));

            // Set up our initial conditions
            var marketData = Deserializtion.
                DeserializeData(DeserializeJsonResource<string>(Resources.market));
            var eddnState = new EDDNState(StarSystemSqLiteRepository.Instance);

            // Check a few items on our initial data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(marketData["timestamp"] as DateTime?));
            Assert.AreEqual(3702012928, marketData["MarketID"] as long?);
            Assert.AreEqual("all", marketData["CarrierDockingAccess"] as string);
            Assert.AreEqual(6, (marketData["Items"] as IEnumerable<object>)?.Count());
            if (marketData["Items"] is List<object> items)
            {
                if (items[0] is IDictionary<string, object> item)
                {
                    Assert.AreEqual(15, item.Keys.Count);
                    Assert.AreEqual("Painite", item["Name_Localised"] as string);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            Assert.IsTrue(commoditySchema?.Handle("Market", ref marketData, eddnState));

            // Validate the final data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(marketData["timestamp"] as DateTime?));
            Assert.AreEqual(3702012928, marketData["marketId"] as long?);
            Assert.IsFalse(marketData.ContainsKey("CarrierDockingAccess"));
            Assert.AreEqual(6, (marketData["commodities"] as IEnumerable<object>)?.Count());
            if (marketData["commodities"] is List<JObject> handledItems)
            {
                if (handledItems[0] is JObject item)
                {
                    Assert.IsFalse(item.ContainsKey("id"));
                    Assert.IsFalse(item.ContainsKey("Name_Localised"));
                    Assert.IsFalse(item.ContainsKey("Category"));
                    Assert.IsFalse(item.ContainsKey("Category_Localised"));
                    Assert.IsFalse(item.ContainsKey("Consumer"));
                    Assert.IsFalse(item.ContainsKey("Producer"));
                    Assert.IsFalse(item.ContainsKey("Rare"));

                    Assert.AreEqual(9, item.Count);
                    Assert.AreEqual("painite", item["name"]?.ToString());
                    Assert.AreEqual(0, item["buyPrice"]?.ToObject<int?>());
                    Assert.AreEqual(500096, item["sellPrice"]?.ToObject<int?>());
                    Assert.AreEqual(0, item["meanPrice"]?.ToObject<int?>());
                    Assert.AreEqual(0, item["stockBracket"]?.ToObject<int?>());
                    Assert.AreEqual(2, item["demandBracket"]?.ToObject<int?>());
                    Assert.AreEqual(0, item["stock"]?.ToObject<int?>());
                    Assert.AreEqual(200, item["demand"]?.ToObject<int?>());
                    Assert.AreEqual(1, item["statusFlags"]?.ToObject<JArray>()?.Count);
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void commoditySchemaCapiTest()
        {
            // Set up our schema
            var commoditySchema = makeTestEDDNResponder()
                .capiSchemas.FirstOrDefault(s => s.GetType() == typeof(CommoditySchema));

            // Set up our initial conditions
            var profileJson = JObject.Parse(@"{""lastSystem"":{""id"":99999,""name"":""Oresqu"",""faction"":""independent""}}");
            var marketJson = DeserializeJsonResource<JObject>(Resources.capi_market_Libby_Horizons);
            var eddnState = new EDDNState(StarSystemSqLiteRepository.Instance);

            // Check a few items on our initial data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(marketJson["timestamp"]?.ToObject<DateTime?>()));
            Assert.AreEqual(3228854528, marketJson["id"]?.ToObject<long?>());
            Assert.AreEqual("starport", marketJson["outpostType"]?.ToString());
            Assert.AreEqual(117, (marketJson["commodities"] as IEnumerable<object>)?.Count());
            if (marketJson["commodities"]?.ToObject<JArray>() is JArray items)
            {
                if (items[0] is JToken item)
                {
                    Assert.AreEqual(13, item.Count());
                    Assert.AreEqual("Agronomic Treatment", (string)item["locName"]);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            var handledData = commoditySchema?.Handle(profileJson, marketJson, new JObject(), new JObject(), false, eddnState);
            Assert.IsNotNull(handledData);

            // Validate the final data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString((DateTime)handledData["timestamp"]));
            Assert.AreEqual(3228854528, handledData["marketId"] as long?);
            Assert.IsFalse(handledData.ContainsKey("outpostType"));
            Assert.IsTrue(handledData["economies"] is IEnumerable<object>);
            Assert.AreEqual(116, (handledData["commodities"] as List<object>)?.Count);
            if (handledData["commodities"] is List<object> handledItems)
            {
                if (handledItems[0] is Dictionary<string, object> item)
                {
                    Assert.IsFalse(item.ContainsKey("id"));
                    Assert.IsFalse(item.ContainsKey("locName"));
                    Assert.IsFalse(item.ContainsKey("categoryname"));
                    Assert.IsFalse(item.ContainsKey("legality"));

                    Assert.AreEqual(8, item.Count);
                    Assert.AreEqual("AgronomicTreatment", item["name"].ToString());
                    Assert.AreEqual(0, Convert.ToInt32(item["buyPrice"]));
                    Assert.AreEqual(3336, Convert.ToInt32(item["sellPrice"]));
                    Assert.AreEqual(3155, Convert.ToInt32(item["meanPrice"]));
                    Assert.AreEqual(0, Convert.ToInt32(item["stockBracket"]));
                    Assert.AreEqual(2, Convert.ToInt32(item["demandBracket"]));
                    Assert.AreEqual(0, Convert.ToInt32(item["stock"]));
                    Assert.AreEqual(43, Convert.ToInt32(item["demand"]));
                    Assert.IsFalse(item.ContainsKey("statusFlags"));
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void fcmaterialsSchemaJournalTest()
        {
            // Set up our schema
            var fcmaterialsSchema = makeTestEDDNResponder()
                .schemas.FirstOrDefault(s => s.GetType() == typeof(FCMaterialsSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState(StarSystemSqLiteRepository.Instance);
            var fcmaterialsData = Deserializtion.
                DeserializeData(DeserializeJsonResource<string>(Resources.FCMaterials));

            // Check a few items on our initial data
            Assert.AreEqual("2022-11-08T03:15:30Z", Dates.FromDateTimeToString(fcmaterialsData["timestamp"] as DateTime?));
            Assert.AreEqual(3709999999, fcmaterialsData["MarketID"] as long?);
            Assert.AreEqual("Station 42", fcmaterialsData["CarrierName"] as string);
            Assert.AreEqual("X9X-9XX", fcmaterialsData["CarrierID"] as string);
            Assert.AreEqual(2, (fcmaterialsData["Items"] as IEnumerable<object>)?.Count());
            if (fcmaterialsData["Items"] is List<object> items)
            {
                if (items[0] is IDictionary<string, object> item)
                {
                    Assert.AreEqual(6, item.Keys.Count);
                    Assert.AreEqual("Chemical Superbase", item["Name_Localised"] as string);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            Assert.IsTrue(fcmaterialsSchema?.Handle("FCMaterials", ref fcmaterialsData, eddnState));

            // Validate the final data
            Assert.AreEqual("2022-11-08T03:15:30Z", Dates.FromDateTimeToString(fcmaterialsData["timestamp"] as DateTime?));
            Assert.AreEqual(3709999999, fcmaterialsData["MarketID"] as long?);
            Assert.AreEqual("Station 42", fcmaterialsData["CarrierName"] as string);
            Assert.AreEqual("X9X-9XX", fcmaterialsData["CarrierID"] as string);
            Assert.AreEqual(2, (fcmaterialsData["Items"] as IEnumerable<object>)?.Count());
            if (fcmaterialsData["Items"] is IEnumerable<object> handledItems)
            {
                if (handledItems.FirstOrDefault() is IDictionary<string, object> item)
                {
                    Assert.IsFalse(item.ContainsKey("Name_Localised"));

                    Assert.AreEqual(5, item.Count);
                    Assert.AreEqual(128961528, Convert.ToInt64(item["id"]));
                    Assert.AreEqual("$chemicalsuperbase_name;", item["Name"].ToString());
                    Assert.AreEqual(500, Convert.ToInt32(item["Price"]));
                    Assert.AreEqual(50, Convert.ToInt32(item["Stock"]));
                    Assert.AreEqual(0, Convert.ToInt32(item["Demand"]));
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void fcmaterialsSchemaCapiTest()
        {
            // Set up our schema
            var fcmaterialsSchema = makeTestEDDNResponder()
                .capiSchemas.FirstOrDefault(s => s.GetType() == typeof(FCMaterialsSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState(StarSystemSqLiteRepository.Instance);
            var fcMarketJson = DeserializeJsonResource<JObject>(Resources.capi_market_fleet_carrier);

            // Check a few items on our initial data
            Assert.AreEqual("2022-11-08T03:15:30Z", Dates.FromDateTimeToString(fcMarketJson["timestamp"]?.ToObject<DateTime>()));
            Assert.AreEqual(3709999999, fcMarketJson["id"]?.ToObject<long>());
            Assert.AreEqual("X9X-9XX", fcMarketJson["name"]?.ToString());
            Assert.AreEqual("fleetcarrier", fcMarketJson["outpostType"]?.ToString());
            var onFootMicroResources = fcMarketJson["orders"]?["onfootmicroresources"];
            Assert.AreEqual(1, onFootMicroResources?["sales"]?.Children().Count());
            Assert.AreEqual(16, onFootMicroResources?["purchases"]?.Children().Count());
            if (onFootMicroResources?["sales"]?.Children().Values().FirstOrDefault() is JObject item)
            {
                Assert.AreEqual(5, item.Count);
                Assert.AreEqual("Graphene", item["locName"]?.ToString());
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            var handledData = fcmaterialsSchema?.Handle(null, fcMarketJson, null, null, false, eddnState);
            Assert.IsNotNull(handledData);

            // Validate the final data
            Assert.AreEqual("2022-11-08T03:15:30Z", Dates.FromDateTimeToString((DateTime)handledData["timestamp"]));
            Assert.AreEqual(3709999999, handledData["MarketID"] as long?);
            Assert.AreEqual("X9X-9XX", handledData["CarrierID"] as string);
            if (handledData.TryGetValue("Items", out object handledItemsObj) &&
                handledItemsObj is Dictionary<string, object> handledItems)
            {
                var sales = handledItems["sales"] as Dictionary<string, object> ?? new Dictionary<string, object>();
                var purchases = handledItems["purchases"] as Dictionary<string, object> ?? new Dictionary<string, object>();
                Assert.AreEqual(1, sales.Count());
                Assert.AreEqual(0, purchases.Count());
                if (sales["128064021"] is Dictionary<string, object> handledItem)
                {
                    Assert.IsFalse(handledItem.ContainsKey("locName"));

                    Assert.AreEqual(4, handledItem.Count);
                    Assert.AreEqual(128064021, Convert.ToInt64(handledItem["id"]));
                    Assert.AreEqual("graphene", handledItem["name"].ToString());
                    Assert.AreEqual(1300, Convert.ToInt32(handledItem["price"]));
                    Assert.AreEqual(112, Convert.ToInt32(handledItem["stock"]));
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void outfittingSchemaJournalTest()
        {
            // Set up our schema
            var outfittingSchema = makeTestEDDNResponder()
                .schemas.FirstOrDefault(s => s.GetType() == typeof(OutfittingSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState(StarSystemSqLiteRepository.Instance);
            var outfittingData = Deserializtion.
                DeserializeData(DeserializeJsonResource<string>(Resources.Outfitting));

            // Check a few items on our initial data
            Assert.AreEqual("2022-11-21T00:05:07Z", Dates.FromDateTimeToString(outfittingData["timestamp"] as DateTime?));
            Assert.AreEqual(3227934976, outfittingData["MarketID"] as long?);
            Assert.AreEqual("Walker Ring", outfittingData["StationName"] as string);
            Assert.AreEqual("Gertrud", outfittingData["StarSystem"] as string);
            Assert.AreEqual(767, (outfittingData["Items"] as IEnumerable<object>)?.Count());
            if (outfittingData["Items"] is List<object> items)
            {
                if (items[0] is IDictionary<string, object> item)
                {
                    Assert.AreEqual(3, item.Keys.Count);
                    Assert.AreEqual("hpt_cannon_gimbal_huge", item["Name"] as string);
                    Assert.AreEqual(128049444, item["id"] as long?);
                    Assert.AreEqual(4476576, item["BuyPrice"] as long?);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            Assert.IsTrue(outfittingSchema?.Handle("Outfitting", ref outfittingData, eddnState));

            // Validate the final data
            Assert.AreEqual("2022-11-21T00:05:07Z", Dates.FromDateTimeToString(outfittingData["timestamp"] as DateTime?));
            Assert.AreEqual(3227934976, outfittingData["marketId"] as long?);
            Assert.AreEqual("Walker Ring", outfittingData["stationName"] as string);
            Assert.AreEqual("Gertrud", outfittingData["systemName"] as string);
            Assert.AreEqual(767, (outfittingData["modules"] as IEnumerable<object>)?.Count());
            if (outfittingData["modules"] is List<string> modules)
            {
                Assert.AreEqual("hpt_cannon_gimbal_huge", modules[0].ToString());
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void outfittingSchemaCapiTest()
        {
            // Set up our schema
            var outfittingSchema = makeTestEDDNResponder()
                .capiSchemas.FirstOrDefault(s => s.GetType() == typeof(OutfittingSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState(StarSystemSqLiteRepository.Instance);
            var shipyardJson = DeserializeJsonResource<JObject>(Resources.capi_shipyard_Abasheli_Barracks);

            // Check a few items on our initial data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(shipyardJson["timestamp"]?.ToObject<DateTime?>()));
            Assert.AreEqual(3544236032, shipyardJson["id"]?.ToObject<long?>());
            Assert.AreEqual("Abasheli Barracks", shipyardJson["name"]?.ToString());
            Assert.IsFalse(shipyardJson.ContainsKey("StarSystem"));
            Assert.AreEqual(165, (shipyardJson["modules"] as IEnumerable<object>)?.Count());
            if (shipyardJson["modules"]?.Children().Values().FirstOrDefault() is JObject item)
            {
                Assert.AreEqual(5, item.Count);
                Assert.AreEqual("Hpt_ATDumbfireMissile_Fixed_Large", item["name"]?.ToString());
                Assert.AreEqual("weapon", item["category"]?.ToString());
                Assert.AreEqual(128788700, item["id"]?.ToObject<long?>());
                Assert.AreEqual(1352250, item["cost"]?.ToObject<long?>());
                Assert.AreEqual("ELITE_HORIZONS_V_PLANETARY_LANDINGS", item["sku"]?.ToString());
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            var profileJson = JObject.Parse(@"{""lastSystem"":{""id"":99999,""name"":""Kurigosages"",""faction"":""independent""}}");
            var outfittingData = outfittingSchema?.Handle(profileJson, null, shipyardJson, null, false, eddnState);
            Assert.IsNotNull(outfittingData);

            // Validate the final data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(outfittingData["timestamp"] as DateTime?));
            Assert.AreEqual(3544236032, outfittingData["marketId"] as long?);
            Assert.AreEqual("Abasheli Barracks", outfittingData["stationName"] as string);
            Assert.AreEqual("Kurigosages", outfittingData["systemName"] as string);
            Assert.AreEqual(164, (outfittingData["modules"] as IEnumerable<object>)?.Count());
            if (outfittingData["modules"] is List<string> modules)
            {
                Assert.AreEqual("Hpt_ATDumbfireMissile_Fixed_Large", modules[0].ToString());
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void shipyardSchemaJournalTest()
        {
            // Set up our schema
            var shipyardSchema = makeTestEDDNResponder()
                .schemas.FirstOrDefault(s => s.GetType() == typeof(ShipyardSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState(StarSystemSqLiteRepository.Instance);
            var shipyardData = Deserializtion.
                DeserializeData(DeserializeJsonResource<string>(Resources.Shipyard));

            // Check a few items on our initial data
            Assert.AreEqual("2022-11-21T00:04:40Z", Dates.FromDateTimeToString(shipyardData["timestamp"] as DateTime?));
            Assert.AreEqual(3227934976, shipyardData["MarketID"] as long?);
            Assert.AreEqual("Walker Ring", shipyardData["StationName"] as string);
            Assert.AreEqual("Gertrud", shipyardData["StarSystem"] as string);
            Assert.AreEqual(18, (shipyardData["PriceList"] as IEnumerable<object>)?.Count());
            if (shipyardData["PriceList"] is List<object> items)
            {
                if (items[0] is IDictionary<string, object> item)
                {
                    Assert.AreEqual(3, item.Keys.Count);
                    Assert.AreEqual("sidewinder", item["ShipType"] as string);
                    Assert.AreEqual(128049249, item["id"] as long?);
                    Assert.AreEqual(26520, item["ShipPrice"] as long?);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            Assert.IsTrue(shipyardSchema?.Handle("Shipyard", ref shipyardData, eddnState));

            // Validate the final data
            Assert.AreEqual("2022-11-21T00:04:40Z", Dates.FromDateTimeToString(shipyardData["timestamp"] as DateTime?));
            Assert.AreEqual(3227934976, shipyardData["marketId"] as long?);
            Assert.AreEqual("Walker Ring", shipyardData["stationName"] as string);
            Assert.AreEqual("Gertrud", shipyardData["systemName"] as string);
            Assert.AreEqual(18, (shipyardData["ships"] as IEnumerable<object>)?.Count());
            if (shipyardData["ships"] is List<string> ships)
            {
                Assert.AreEqual("sidewinder", ships[0].ToString());
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void shipyardSchemaCapiTest()
        {
            // Set up our schema
            var shipyardSchema = makeTestEDDNResponder()
                .capiSchemas.FirstOrDefault(s => s.GetType() == typeof(ShipyardSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState(StarSystemSqLiteRepository.Instance);
            var shipyardJson = DeserializeJsonResource<JObject>(Resources.capi_shipyard_Abasheli_Barracks);

            // Check a few items on our initial data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(shipyardJson["timestamp"]?.ToObject<DateTime?>()));
            Assert.AreEqual(3544236032, shipyardJson["id"]?.ToObject<long?>());
            Assert.AreEqual("Abasheli Barracks", shipyardJson["name"]?.ToString());
            Assert.IsFalse(shipyardJson.ContainsKey("StarSystem"));
            Assert.AreEqual(5, (shipyardJson["ships"]?["shipyard_list"] as IEnumerable<object>)?.Count());
            Assert.AreEqual(3, (shipyardJson["ships"]?["unavailable_list"] as IEnumerable<object>)?.Count());
            if (shipyardJson["ships"]?["shipyard_list"]?.Children().Values().FirstOrDefault() is JObject item)
            {
                Assert.AreEqual(4, item.Count);
                Assert.AreEqual("Eagle", item["name"]?.ToString());
                Assert.AreEqual(128049255, item["id"]?.ToObject<long?>());
                Assert.AreEqual(44800, item["basevalue"]?.ToObject<long?>());
                Assert.AreEqual("", item["sku"]?.ToString());
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            var profileJson = JObject.Parse(@"{""lastSystem"":{""id"":99999,""name"":""Kurigosages"",""faction"":""independent""}}");
            var shipyardData = shipyardSchema?.Handle(profileJson, null, shipyardJson, null, false, eddnState);
            Assert.IsNotNull(shipyardData);

            // Validate the final data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(shipyardData["timestamp"] as DateTime?));
            Assert.AreEqual(3544236032, shipyardData["marketId"] as long?);
            Assert.AreEqual("Abasheli Barracks", shipyardData["stationName"] as string);
            Assert.AreEqual("Kurigosages", shipyardData["systemName"] as string);
            Assert.IsFalse(shipyardData["allowCobraMkIV"] as bool? ?? false);
            Assert.AreEqual(8, (shipyardData["ships"] as IEnumerable<object>)?.Count());
            if (shipyardData["ships"] is List<string> ships)
            {
                Assert.AreEqual("Eagle", ships[0].ToString());
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}

