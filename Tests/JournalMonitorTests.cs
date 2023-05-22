using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tests.Properties;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class JournalMonitorTests : TestBase
    {
        FakeEdsmRestClient fakeEdsmRestClient;

        [TestInitialize]
        public void start()
        {
            fakeEdsmRestClient = new FakeEdsmRestClient();
            MakeSafe();
        }

        [TestMethod]
        public void TestJournalPlanetScan1()
        {
            string line = @"{ ""timestamp"":""2016-09-22T21:34:30Z"", ""event"":""Scan"", ""BodyName"":""Nemehim 4"", ""StarSystem"":""Nemehim"", ""SystemAddress"":1733388440282, ""DistanceFromArrivalLS"":1115.837646, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Rocky ice body"", ""Atmosphere"":"""", ""Volcanism"":"""", ""MassEM"":0.013448, ""Radius"":1688803.625000, ""SurfaceGravity"":1.879402, ""SurfaceTemperature"":103.615654, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":{ ""iron"":18.8, ""sulphur"":17.7, ""carbon"":14.9, ""nickel"":14.3, ""phosphorus"":9.6, ""chromium"":8.5, ""manganese"":7.8, ""zinc"":5.1, ""molybdenum"":1.2, ""tungsten"":1.0, ""tellurium"":1.0 }, ""OrbitalPeriod"":122165280.000000, ""RotationPeriod"":112645.117188 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
        }

        [TestMethod]
        public void TestJournalPlanetScan2()
        {
            string line = @"{ ""timestamp"":""2016 - 11 - 01T18: 49:07Z"", ""event"":""Scan"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""StarSystem"":""Grea Bloae HH-T d4-44"", ""SystemAddress"":1520309296811, ""DistanceFromArrivalLS"":703.763611, ""TidalLock"":false, ""TerraformState"":""Terraformable"", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick carbon dioxide atmosphere"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":2.171783, ""Radius"":7622170.500000, ""SurfaceGravity"":14.899396, ""SurfaceTemperature"":836.165466, ""SurfacePressure"":33000114.000000, ""Landable"":false, ""SemiMajorAxis"":210957926400.000000, ""Eccentricity"":0.000248, ""OrbitalInclination"":0.015659, ""Periapsis"":104.416656, ""OrbitalPeriod"":48801056.000000, ""RotationPeriod"":79442.242188 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(BodyScannedEvent));
            BodyScannedEvent ev = events[0] as BodyScannedEvent;
            Assert.IsNotNull(ev);
            Assert.AreEqual("Grea Bloae HH-T d4-44 4", ev.bodyname);
            Assert.AreEqual((decimal)703.763611, ev.distance);
            Assert.IsNotNull(ev.tidallylocked);
            Assert.IsFalse(ev.tidallylocked);
            Assert.AreEqual("Candidate for terraforming", ev.terraformState.invariantName);
            Assert.AreEqual("High metal content world", ev.planetClass.invariantName);
            Assert.IsNotNull(ev.volcanism);
            Assert.AreEqual("Magma", ev.volcanism.invariantType);
            Assert.AreEqual("Iron", ev.volcanism.invariantComposition);
            Assert.AreEqual("Minor", ev.volcanism.invariantAmount);
            Assert.AreEqual((decimal)2.171783, ev.earthmass);
            Assert.AreEqual((double)7622.170500000M, (double?)ev.radius ?? 0, 0.01);
            Assert.AreEqual(ConstantConverters.ms2g((decimal)14.899396), ev.gravity);
            Assert.AreEqual((decimal)836.165466, ev.temperature);
            Assert.AreEqual(325.986, (double?)ev.pressure ?? 0, 0.01);
            Assert.IsNotNull(ev.landable);
            Assert.IsFalse(ev.landable);
            Assert.AreEqual(703.679898444943, (double?)ev.semimajoraxis ?? 0, 0.01);
            Assert.AreEqual((decimal)0.000248, ev.eccentricity);
            Assert.AreEqual((decimal)0.015659, ev.inclination);
            Assert.AreEqual((decimal)104.416656, ev.periapsis);
            Assert.AreEqual(564.827, (double?)ev.orbitalperiod ?? 0, 0.01);
            Assert.AreEqual(0.91947, (double?)ev.rotationalperiod ?? 0, 0.01);
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
                ""StarSystem"":""Sol"",
                ""SystemAddress"":10477373803, 
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
            Assert.AreEqual(96.5M, ev.atmospherecompositions[0].percent);
            Assert.AreEqual("Rock", ev.solidcompositions[0].invariantName);
            Assert.AreEqual(70M, ev.solidcompositions[0].percent);
        }

        [TestMethod]
        public void TestJournalPlanetScan4()
        {
            // Test new scan data from game version 3.4.
            string line = @"{ ""timestamp"":""2019-04-12T04:42:10Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""HR 1185 A 1"", ""BodyID"":4, ""StarSystem"":""HR 1185"", ""SystemAddress"":1774711381, ""Parents"":[ {""Null"":3}, {""Star"":1}, {""Null"":0} ], ""DistanceFromArrivalLS"":45.276505, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick silicate vapour atmosphere"", ""AtmosphereType"":""SilicateVapour"", ""AtmosphereComposition"":[ { ""Name"":""Silicates"", ""Percent"":99.989662 } ], ""Volcanism"":""major silicate vapour geysers volcanism"", ""MassEM"":2.428317, ""Radius"":8014977.000000, ""SurfaceGravity"":15.066424, ""SurfaceTemperature"":4894.569336, ""SurfacePressure"":6359968768.000000, ""Landable"":false, ""Composition"":{ ""Ice"":0.000073, ""Rock"":0.671092, ""Metal"":0.322412 }, ""SemiMajorAxis"":15315170.000000, ""Eccentricity"":0.021248, ""OrbitalInclination"":-4.599963, ""Periapsis"":144.548447, ""OrbitalPeriod"":27184.082031, ""RotationPeriod"":39590.453125, ""AxialTilt"":0.120614, ""WasDiscovered"":true, ""WasMapped"":true }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(BodyScannedEvent));
            BodyScannedEvent ev = events[0] as BodyScannedEvent;
            Assert.IsNotNull(ev);
            Assert.AreEqual("Planet", ev.body.bodyType.invariantName);
            Debug.Assert(ev.alreadydiscovered != null, "ev.alreadydiscovered != null");
            Assert.IsTrue((bool)ev.alreadydiscovered);
            Debug.Assert(ev.alreadymapped != null, "ev.alreadymapped != null");
            Assert.IsTrue((bool)ev.alreadymapped);
        }

        [TestMethod]
        public void TestJournalPlanetScan5()
        {
            // Test Nav Beacon scan data from game version 4.0.
            string line = @"{
                ""timestamp"": ""2023-01-29T13:05:14Z"",
                ""event"": ""Scan"",
                ""ScanType"": ""NavBeaconDetail"",
                ""BodyName"": ""Nakulha BC 4 a"",
                ""BodyID"": 9,
                ""Parents"": [{
                        ""Planet"": 8
                    }, {
                        ""Null"": 2
                    }, {
                        ""Null"": 0
                    }
                ],
                ""StarSystem"": ""Nakulha"",
                ""SystemAddress"": 20461895296465,
                ""DistanceFromArrivalLS"": 2506.979499,
                ""TidalLock"": true,
                ""TerraformState"": """",
                ""PlanetClass"": ""Icy body"",
                ""Atmosphere"": """",
                ""AtmosphereType"": ""None"",
                ""Volcanism"": """",
                ""MassEM"": 0.001115,
                ""Radius"": 909518.812500,
                ""SurfaceGravity"": 0.536999,
                ""SurfaceTemperature"": 114.884079,
                ""SurfacePressure"": 0.000000,
                ""Landable"": true,
                ""Materials"": [{
                        ""Name"": ""sulphur"",
                        ""Percent"": 26.425076
                    }, {
                        ""Name"": ""carbon"",
                        ""Percent"": 22.220751
                    }, {
                        ""Name"": ""phosphorus"",
                        ""Percent"": 14.226108
                    }, {
                        ""Name"": ""iron"",
                        ""Percent"": 13.131762
                    }, {
                        ""Name"": ""nickel"",
                        ""Percent"": 9.932315
                    }, {
                        ""Name"": ""manganese"",
                        ""Percent"": 5.423284
                    }, {
                        ""Name"": ""germanium"",
                        ""Percent"": 3.475405
                    }, {
                        ""Name"": ""vanadium"",
                        ""Percent"": 3.224703
                    }, {
                        ""Name"": ""niobium"",
                        ""Percent"": 0.897486
                    }, {
                        ""Name"": ""mercury"",
                        ""Percent"": 0.573442
                    }, {
                        ""Name"": ""technetium"",
                        ""Percent"": 0.469670
                    }
                ],
                ""Composition"": {
                    ""Ice"": 0.807359,
                    ""Rock"": 0.165785,
                    ""Metal"": 0.026856
                },
                ""SemiMajorAxis"": 36050819.158554,
                ""Eccentricity"": 0.000000,
                ""OrbitalInclination"": -42.302688,
                ""Periapsis"": 19.727769,
                ""OrbitalPeriod"": 420461.648703,
                ""AscendingNode"": 139.366346,
                ""MeanAnomaly"": 289.416976,
                ""RotationPeriod"": 429682.225063,
                ""AxialTilt"": 0.232535,
                ""WasDiscovered"": false,
                ""WasMapped"": true
            }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(BodyScannedEvent));
            BodyScannedEvent ev = events[0] as BodyScannedEvent;
            Assert.IsNotNull(ev);
            Assert.AreEqual("Moon", ev.body.bodyType.invariantName);
            Debug.Assert(ev.alreadydiscovered != null, "ev.alreadydiscovered != null");
            Assert.IsTrue((bool)ev.alreadydiscovered);
            Debug.Assert(ev.alreadymapped != null, "ev.alreadymapped != null");
            Assert.IsTrue((bool)ev.alreadymapped);
        }

        [TestMethod]
        public void TestJournalStarScan1()
        {
            string line = @"{ ""timestamp"":""2016-10-27T08:51:23Z"", ""event"":""Scan"", ""BodyName"":""Vela Dark Region FG-Y d3"", ""StarSystem"":""Vela Dark Region FG-Y d3"", ""SystemAddress"":113757866339, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""K"", ""StellarMass"":0.960938, ""Radius"":692146368.000000, ""AbsoluteMagnitude"":5.375961, ""Age_MY"":230, ""SurfaceTemperature"":5108.000000, ""RotationPeriod"":393121.093750, ""Rings"":[ { ""Name"":""Vela Dark Region FG-Y d3 A Belt"", ""RingClass"":""eRingClass_Metalic"", ""MassMT"":1.2262e+10, ""InnerRad"":1.2288e+09, ""OuterRad"":2.3812e+09 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            StarScannedEvent theEvent = (StarScannedEvent)events[0];

            Assert.AreEqual(230, theEvent.age);
            Assert.IsNull(theEvent.eccentricity);
            Assert.AreEqual("Vela Dark Region FG-Y d3", theEvent.bodyname);
            Assert.IsNull(theEvent.orbitalperiod);
            Assert.AreEqual(692146.368000000M, theEvent.radius);
            Assert.IsNull(theEvent.semimajoraxis);
            Assert.AreEqual(0.960938, (double?)theEvent.solarmass ?? 0, 0.001);
            Assert.AreEqual("K", theEvent.stellarclass);
            Assert.AreEqual(5108, theEvent.temperature);
            Assert.AreEqual(5.375961M, theEvent.absolutemagnitude);
            Assert.IsTrue(theEvent.mainstar ?? false);
            // Stellar extras
            Assert.AreEqual("yellow-orange", theEvent.chromaticity);
            Assert.AreEqual(99.33M, theEvent.massprobability);
            Assert.AreEqual(65, theEvent.radiusprobability);
            Assert.AreEqual(95, theEvent.tempprobability);
            Assert.AreEqual(7, theEvent.ageprobability);
            Assert.AreEqual(303.548, (double?)theEvent.estimatedhabzoneinner ?? 0, .01);
            Assert.AreEqual(604.861, (double?)theEvent.estimatedhabzoneouter ?? 0, .01);
            // Ring
            Assert.AreEqual("Metallic", theEvent.rings[0].Composition.invariantName);
        }

        [TestMethod]
        public void TestJournalStarScan2()
        {
            string line = @"{ ""timestamp"":""2016-10-28T12:07:09Z"", ""event"":""Scan"", ""BodyName"":""Col 285 Sector CG-X d1-44"", ""StarSystem"":""Col 285 Sector CG-X d1-44"", ""SystemAddress"":1522272307539, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""TTS"", ""StellarMass"":0.808594, ""Radius"":659162816.000000, ""AbsoluteMagnitude"":6.411560, ""Age_MY"":154, ""SurfaceTemperature"":4124.000000, ""RotationPeriod"":341417.281250, ""Rings"":[ { ""Name"":""Col 285 Sector CG-X d1-44 A Belt"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":1.1625e+13, ""InnerRad"":1.0876e+09, ""OuterRad"":2.4192e+09 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            StarScannedEvent theEvent = (StarScannedEvent)events[0];
            Assert.AreEqual((decimal)659162.816, theEvent.radius);
            Assert.AreEqual(StarClass.solarradius((decimal)659162.816000000), theEvent.solarradius);
            Assert.AreEqual(0.94775, (double?)theEvent.solarradius ?? 0, 0.01);
            Assert.IsTrue(theEvent.mainstar ?? false);
        }

        [TestMethod]
        public void TestJournalStarScan3()
        {
            // Gamer version 3.4
            string line = @"{ ""timestamp"":""2019-04-12T04:49:10Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Pleiades Sector MN-T c3-14 B"", ""BodyID"":2, ""StarSystem"":""Pleiades Sector MN-T c3-14"", ""SystemAddress"":3932008911514, ""Parents"":[ {""Null"":0} ], ""DistanceFromArrivalLS"":84306.257813, ""StarType"":""M"", ""Subclass"":8, ""StellarMass"":0.246094, ""Radius"":316421920.000000, ""AbsoluteMagnitude"":10.680222, ""Age_MY"":702, ""SurfaceTemperature"":2228.000000, ""Luminosity"":""Va"", ""SemiMajorAxis"":20863141281792.000000, ""Eccentricity"":0.278661, ""OrbitalInclination"":-103.465088, ""Periapsis"":32.983871, ""OrbitalPeriod"":104334450688.000000, ""RotationPeriod"":212050.531250, ""AxialTilt"":0.000000, ""WasDiscovered"":true, ""WasMapped"":false }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            StarScannedEvent theEvent = (StarScannedEvent)events[0];
            Assert.AreEqual(8, theEvent.stellarsubclass);
            Debug.Assert(theEvent.alreadydiscovered != null, "theEvent.alreadydiscovered != null");
            Assert.IsTrue((bool)theEvent.alreadydiscovered);
            Assert.AreEqual(3687.497842466M, theEvent.density);
            Assert.AreEqual(1204, theEvent.estimatedvalue);
            Assert.AreEqual(0.00456994738549848M, theEvent.luminosity);
            Assert.AreEqual("Va", theEvent.luminosityclass);
            Assert.IsTrue(!theEvent.mainstar ?? false);
            Assert.AreEqual(32.983871M, theEvent.periapsis);
            Assert.IsTrue(theEvent.scoopable);
            Assert.AreEqual(0M, theEvent.tilt);
            Debug.Assert(theEvent.alreadymapped != null, "theEvent.alreadymapped != null");
            Assert.IsFalse((bool)theEvent.alreadymapped);
            Assert.AreEqual(2, theEvent.bodyId);
            Assert.IsNull(theEvent.mapped);
            Assert.IsInstanceOfType(theEvent.scanned, typeof(System.DateTime));
            Assert.AreEqual(1, theEvent.parents.Count);
            // Stellar extras
            Assert.AreEqual(85, theEvent.absolutemagnitudeprobability);
            Assert.AreEqual(14, theEvent.densityprobability);
            Assert.AreEqual(86, theEvent.eccentricityprobability);
            Assert.AreEqual(1, theEvent.inclinationprobability);
            Assert.AreEqual(52, theEvent.orbitalperiodprobability);
            Assert.AreEqual(8, theEvent.periapsisprobability);
            Assert.AreEqual(61, theEvent.rotationalperiodprobability);
            Assert.AreEqual(100, theEvent.semimajoraxisprobability);
            Assert.IsNull(theEvent.tiltprobability);
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
            Assert.AreEqual((ulong)3107509474002, theEvent.systemAddress);
            Assert.AreEqual(1, theEvent.stationservices.Count);
            Assert.AreEqual("Refuel", theEvent.stationservices[0]);
            Assert.AreEqual(2, theEvent.economyShares.Count);
            Assert.AreEqual("Industrial", theEvent.economyShares[0].economy.invariantName);
            Assert.AreEqual(0.7M, theEvent.economyShares[0].proportion);
            Assert.AreEqual("Extraction", theEvent.economyShares[1].economy.invariantName);
            Assert.AreEqual(0.3M, theEvent.economyShares[1].proportion);
            Assert.AreEqual(StationState.NormalOperation, theEvent.stationState);
        }

        [TestMethod]
        public void TestJournalDocked2()
        {
            string line = @"{ ""timestamp"":""2018-04-01T05:21:24Z"", ""event"":""Docked"", ""StationName"":""Donaldson"", ""StationState"":""UnderRepairs"", ""StationType"":""Orbis"", ""StarSystem"":""Alioth"", ""SystemAddress"":1109989017963, ""MarketID"":128141048, ""StationFaction"":{ ""Name"":""Alioth Pro-Alliance Grou"", ""FactionState"":""Boom"" }, ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationAllegiance"":""Alliance"", ""StationServices"":[ ""Dock"", ""Autodock"", ""BlackMarket"", ""Commodities"", ""Contacts"", ""Exploration"", ""Missions"", ""Outfitting"", ""CrewLounge"", ""Rearm"", ""Refuel"", ""Repair"", ""Shipyard"", ""Tuning"", ""Workshop"", ""MissionsGenerated"", ""FlightController"", ""StationOperations"", ""Powerplay"", ""SearchAndRescue"" ], ""StationEconomy"":""$economy_Service;"", ""StationEconomy_Localised"":""Service"", ""StationEconomies"":[ { ""Name"":""$economy_Service;"", ""Name_Localised"":""Service"", ""Proportion"":1.000000 } ], ""DistFromStarLS"":4632.417480 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockedEvent theEvent = (DockedEvent)events[0];

            Assert.AreEqual("Orbis", theEvent.stationModel.edname);
            Assert.AreEqual("Donaldson", theEvent.station);
            Assert.AreEqual("Alioth", theEvent.system);
            Assert.AreEqual("Boom", theEvent.controllingfaction.presences.FirstOrDefault(p => p.systemName == theEvent.system)?.FactionState?.invariantName);
            Assert.AreEqual("Democracy", theEvent.controllingfaction.Government.invariantName);
            Assert.AreEqual("Alliance", theEvent.controllingfaction.Allegiance.invariantName);
            Assert.AreEqual(20, theEvent.stationservices.Count);
            Assert.AreEqual(1, theEvent.economyShares.Count);
            Assert.AreEqual("Service", theEvent.economyShares[0].economy.invariantName);
            Assert.AreEqual(1.0M, theEvent.economyShares[0].proportion);
            Assert.AreEqual(StationState.UnderRepairs, theEvent.stationState);
        }

        [TestMethod]
        public void TestJournalDockedDuplicateEconomies()
        {
            string line = @"{ ""timestamp"":""2018-04-01T05:21:24Z"", ""event"":""Docked"", ""StationName"":""Katzenstein Dock"", ""StationType"":""Coriolis"", ""StarSystem"":""36 Ophiuchi"", ""SystemAddress"":1865903245675, ""MarketID"":3228939264, ""StationFaction"":{ ""Name"":""36 Ophiuchi Future"", ""FactionState"":""Boom"" }, ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationAllegiance"":""Federation"", ""StationServices"":[ ""Dock"", ""Autodock"", ""BlackMarket"", ""Commodities"", ""Contacts"", ""Exploration"", ""Missions"", ""Outfitting"", ""CrewLounge"", ""Rearm"", ""Refuel"", ""Repair"", ""Shipyard"", ""Tuning"", ""Workshop"", ""MissionsGenerated"", ""FlightController"", ""StationOperations"", ""Powerplay"", ""SearchAndRescue"", ""StationMenu"" ], ""StationEconomy"":""$economy_Refinery;"", ""StationEconomy_Localised"":""Refinery"", ""StationEconomies"":[ { ""Name"":""$economy_Refinery;"", ""Name_Localised"":""Refinery"", ""Proportion"":0.84 }, { ""Name"":""$economy_Refinery;"", ""Name_Localised"":""Refinery"", ""Proportion"":0.16 } ], ""DistFromStarLS"":4217877.0 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            DockedEvent theEvent = (DockedEvent)events[0];

            Assert.AreEqual("Coriolis", theEvent.stationModel.edname);
            Assert.AreEqual("Katzenstein Dock", theEvent.station);
            Assert.AreEqual("36 Ophiuchi", theEvent.system);
            Assert.AreEqual("Boom", theEvent.controllingfaction.presences.FirstOrDefault(p => p.systemName == theEvent.system)?.FactionState?.invariantName);
            Assert.AreEqual("Democracy", theEvent.controllingfaction.Government.invariantName);
            Assert.AreEqual("Federation", theEvent.controllingfaction.Allegiance.invariantName);
            Assert.AreEqual(21, theEvent.stationservices.Count);
            Assert.AreEqual(2, theEvent.economyShares.Count);
            Assert.AreEqual("Refinery", theEvent.economyShares[0].economy.invariantName);
            Assert.AreEqual(0.84M, theEvent.economyShares[0].proportion);
            Assert.AreEqual("Refinery", theEvent.economyShares[1].economy.invariantName);
            Assert.AreEqual(0.16M, theEvent.economyShares[1].proportion);

            // The Station definition should consolidate the economy shares above. Test that now. 
            Station testStation = new Station() { name = "testStation", economyShares = theEvent.economyShares };
            Assert.AreEqual(1, testStation.economyShares.Count);
            Assert.AreEqual("Refinery", testStation.economyShares[0].economy.invariantName);
            Assert.AreEqual(1.00M, testStation.economyShares[0].proportion);
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
            Assert.AreEqual("Police", theEvent.Source.invariantName);
            Assert.AreEqual("Federal Security Service", theEvent.from);
            Assert.AreEqual("npc", theEvent.Channel.invariantName);
        }

        [TestMethod]
        public void TestJournalMessageReceived2()
        {
            string line = @"{ ""timestamp"":""2016-10-06T12:48:56Z"", ""event"":""ReceiveText"", ""From"":""$npc_name_decorate:#name=Jonathan Dallard;"", ""From_Localised"":""Jonathan Dallard"", ""Message"":""$Pirate_OnStartScanCargo07;"", ""Message_Localised"":""Do you have anything of value?"", ""Channel"":""npc"" }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 2);

            MessageReceivedEvent event1 = (MessageReceivedEvent)events[0];

            Assert.IsFalse(event1.player);
            Assert.AreEqual("Pirate", event1.Source.invariantName);
            Assert.AreEqual("Jonathan Dallard", event1.from);
            Assert.AreEqual("npc", event1.Channel.invariantName);

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
            Assert.AreEqual("Commander", event1.Source.invariantName);
            Assert.AreEqual("SlowIce", event1.from);
            Assert.AreEqual("good luck", event1.message);
            Assert.AreEqual("player", event1.Channel.invariantName);
        }

        [TestMethod]
        public void TestJournalPlayerLocalChat()
        {
            string line = @"{ ""timestamp"":""2017 - 10 - 12T20: 39:25Z"", ""event"":""ReceiveText"", ""From"":""Rebecca Lansing"", ""Message"":""Hi there"", ""Channel"":""local"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            MessageReceivedEvent event1 = (MessageReceivedEvent)events[0];

            Assert.IsTrue(event1.player);
            Assert.AreEqual("Commander", event1.Source.invariantName);
            Assert.AreEqual("Rebecca Lansing", event1.from);
            Assert.AreEqual("Hi there", event1.message);
            Assert.AreEqual("local", event1.Channel.invariantName);
        }

        [TestMethod]
        public void TestJournalPlayerWingChat()
        {
            string line = @"{ ""timestamp"":""2017-10-12T21:11:10Z"", ""event"":""ReceiveText"", ""From"":""SlowIce"", ""Message"":""hello"", ""Channel"":""wing"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            MessageReceivedEvent event1 = (MessageReceivedEvent)events[0];

            Assert.IsTrue(event1.player);
            Assert.AreEqual("Wing mate", event1.Source.invariantName);
            Assert.AreEqual("SlowIce", event1.from);
            Assert.AreEqual("hello", event1.message);
            Assert.AreEqual("wing", event1.Channel.invariantName);
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
            Assert.AreEqual("multicrew", event1.Channel.invariantName);
            Assert.AreEqual("Crew mate", event1.Source.invariantName);
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
            string line1 = "{ \"timestamp\":\"2017-08-24T17:22:03Z\", \"event\":\"Friends\", \"Status\":\"Online\", \"Name\":\"_Testy_McTest_\" }";
            string line2 = "{ \"timestamp\":\"2017-08-24T17:22:03Z\", \"event\":\"Friends\", \"Status\":\"Offline\", \"Name\":\"_Testy_McTest_\" }";

            // Setup
            EDDI eddiInstance = EDDI.Instance;
            Friend[] preexistingFriends = eddiInstance.Cmdr.friends.ToArray();
            PrivateObject privateEddiInstance = new PrivateObject(eddiInstance);
            bool eventFriends(FriendsEvent friendsEvent)
            {
                return (bool)privateEddiInstance.Invoke("eventFriends", new object[] { friendsEvent });
            }

            // Act
            List<Event> events1 = JournalMonitor.ParseJournalEntry(line1);
            List<Event> events2 = JournalMonitor.ParseJournalEntry(line2);

            // Both should generate one event
            Assert.AreEqual(1, events1.Count);
            Assert.AreEqual(1, events2.Count);
            FriendsEvent event1 = (FriendsEvent)events1[0];
            FriendsEvent event2 = (FriendsEvent)events2[0];
            Assert.AreEqual("Online", event1.status);
            Assert.AreEqual("Offline", event2.status);

            // The first event should be suppressed at the EDDI level
            bool passEvent1 = eventFriends(event1);
            bool passEvent2 = eventFriends(event2);
            Assert.IsFalse(passEvent1);
            Assert.IsTrue(passEvent2);

            // Clean up
            eddiInstance.Cmdr.friends = new List<Friend>(preexistingFriends);
        }

        [TestMethod]
        public void TestJournalSearchAndRescue()
        {
            string line = @"{""timestamp"":""2018-05-26T22:04:09Z"",""event"":""SearchAndRescue"",""MarketID"":3228973824,""Name"":""usscargoblackbox"",""Name_Localised"":""Black Box"",""Count"":1,""Reward"":21184}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            SearchAndRescueEvent sarEvent = (SearchAndRescueEvent)events[0];
            Assert.AreEqual("Black Box", sarEvent.commodity.invariantName);
            Assert.AreEqual("Salvage", sarEvent.commodity.Category.invariantName);
        }

        [TestMethod]
        public void TestJournalEnteredNormalSpaceEvent()
        {
            string line = @"{ ""timestamp"":""2018 - 02 - 07T07: 13:39Z"", ""event"":""SupercruiseExit"", ""StarSystem"":""Wyrd"", ""SystemAddress"":5031654888146, ""Body"":""Vonarburg Co-operative"", ""BodyID"":35, ""BodyType"":""Station"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            EnteredNormalSpaceEvent normalSpaceEvent = (EnteredNormalSpaceEvent)events[0];
            Assert.AreEqual("Vonarburg Co-operative", normalSpaceEvent.bodyname);
            Assert.AreEqual("Station", normalSpaceEvent.bodytype);
            Assert.AreEqual("Wyrd", normalSpaceEvent.systemname);
            Assert.AreEqual((ulong)5031654888146, normalSpaceEvent.systemAddress);
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
            Assert.AreEqual((ulong)670417429889, jumpedEvent.systemAddress);
            Assert.AreEqual(-41.06250M, jumpedEvent.x);
            Assert.AreEqual(-62.15625M, jumpedEvent.y);
            Assert.AreEqual(-103.25000M, jumpedEvent.z);
            Assert.AreEqual("Independent", jumpedEvent.controllingfaction.Allegiance.invariantName);
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
            Assert.AreEqual("Expansion", jumpedEvent.factions.FirstOrDefault(f => f.name == "EXO")?.presences.FirstOrDefault(p => p.systemName == "Diaguandri")?.FactionState?.invariantName);
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
                ""StationName"": ""Ray Gateway"",
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
                ""StationGovernment"": ""$government_Democracy;"",
                ""StationGovernment_Localised"": ""Democracy"",
                ""SystemSecurity"": ""$SYSTEM_SECURITY_medium;"",
                ""SystemSecurity_Localised"": ""MediumSecurity"",
                ""Population"": 10303479,
                ""Body"": ""Ray Gateway"",
                ""BodyID"": 32,
                ""BodyType"": ""Station"",
                ""Factions"": [{
                    ""Name"": ""Diaguandri Interstellar"",
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
                    ""Name"": ""People's MET 20 Liberals"",
                    ""FactionState"": ""Boom"",
                    ""Government"": ""Democracy"",
                    ""Influence"": 0.206000,
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
                    ""FactionState"": ""Boom"",
                    ""Government"": ""Dictatorship"",
                    ""Influence"": 0.072000,
                    ""Allegiance"": ""Independent""
                },
                {
                    ""Name"": ""Cartel of Diaguandri"",
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
                    ""Name"": ""Revolutionary Party of Diaguandri"",
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
                    ""Name"": ""The Brotherhood of the Dark Circle"",
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
                },
                ""StationFaction"": {
                    ""Name"": ""EXO"",
                    ""FactionState"": ""None""
                }
            }";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            LocationEvent @event = (LocationEvent)events[0];

            Assert.AreEqual("Ray Gateway", @event.bodyname);
            Assert.AreEqual("Station", @event.bodyType.invariantName);
            Assert.AreEqual(true, @event.docked);
            Assert.AreEqual("High Tech", @event.Economy.invariantName);
            Assert.AreEqual("Refinery", @event.Economy2.invariantName);

            Assert.AreEqual("EXO", @event.systemfaction);
            Assert.AreEqual("Independent", @event.controllingsystemfaction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", @event.controllingsystemfaction.Government.invariantName);
            Assert.AreEqual("EXO", @event.stationfaction);
            Assert.AreEqual("Independent", @event.controllingstationfaction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", @event.controllingstationfaction.Government.invariantName);

            Assert.IsNull(@event.latitude);
            Assert.IsNull(@event.longitude);
            Assert.AreEqual(3223343616, @event.marketId);
            Assert.AreEqual(10303479, @event.population);
            Assert.AreEqual("Medium", @event.securityLevel.invariantName);
            Assert.AreEqual("Ray Gateway", @event.station);
            Assert.AreEqual("Coriolis Starport", @event.stationModel.invariantName);
            Assert.AreEqual("Diaguandri", @event.systemname);
            Assert.AreEqual((ulong)670417429889, @event.systemAddress);
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

            Assert.AreEqual("Ageno", @event.systemname);
            Assert.AreEqual((ulong)18262335038849, @event.systemAddress);
            Assert.AreEqual("Ageno B 2 a", @event.bodyname);

            string line2 = @"{ ""timestamp"":""2018 - 07 - 24T07: 08:58Z"", ""event"":""LeaveBody"", ""StarSystem"":""Ageno"", ""SystemAddress"":18262335038849, ""Body"":""Ageno B 2 a"", ""BodyID"":17 }";
            events = JournalMonitor.ParseJournalEntry(line2);
            NearSurfaceEvent @event2 = (NearSurfaceEvent)events[0];

            Assert.AreEqual("Ageno", @event2.systemname);
            Assert.AreEqual((ulong)18262335038849, @event2.systemAddress);
            Assert.AreEqual("Ageno B 2 a", @event2.bodyname);
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
            string line = @"{ ""timestamp"":""2018-11-04T03:11:56Z"", ""event"":""ApproachSettlement"", ""Name"":""Bulmer Enterprise"", ""MarketID"":3510380288, ""SystemAddress"": 670417429889, ""Latitude"":-23.121552, ""Longitude"":-98.177559 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SettlementApproachedEvent @event = (SettlementApproachedEvent)events[0];

            Assert.AreEqual(3510380288, @event.marketId);
            Assert.AreEqual((ulong)670417429889, @event.systemAddress);
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

            Assert.AreEqual("Thargoid", @event.controllingfaction.Allegiance.invariantName);
        }

        [TestMethod]
        public void TestJumpedEventAllegianceGuardian()
        {
            string line = @"{ ""timestamp"":""2018-08-25T06:28:04Z"", ""event"":""FSDJump"", ""StarSystem"":""Synuefe EU-Q c21-10"", ""SystemAddress"":2833906537146, ""StarPos"":[758.65625,-176.90625,-133.21875], ""SystemAllegiance"":""Guardian"", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""JumpDist"":29.867, ""FuelUsed"":1.269155, ""FuelLevel"":18.824537 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            JumpedEvent @event = (JumpedEvent)events[0];

            Assert.AreEqual("Guardian", @event.controllingfaction.Allegiance.invariantName);
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
            Assert.AreEqual("None", @event.controllingsystemfaction.Allegiance?.invariantName);
            Assert.AreEqual("$faction_None", @event.controllingsystemfaction.Allegiance?.edname);
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

            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            var result = (bool)privateObject.Invoke("eventJumped", new object[] { @event });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestJournalStoredModulesEventHandler()
        {
            string line = "{ \"timestamp\":\"2019-01-25T00:06:36Z\", \"event\":\"StoredModules\", \"MarketID\":128928173, \"StationName\":\"Rock of Isolation\", \"StarSystem\":\"Omega Sector OD-S b4-0\", \"Items\":[ { \"Name\":\"$int_shieldgenerator_size7_class3_fast_name;\", \"Name_Localised\":\"Bi-Weave Shield\", \"StorageSlot\":52, \"StarSystem\":\"Omega Sector VE-Q b5-15\", \"MarketID\":128757071, \"TransferCost\":9729, \"TransferTime\":480, \"BuyPrice\":7501033, \"Hot\":false, \"EngineerModifications\":\"ShieldGenerator_Thermic\", \"Level\":5, \"Quality\":0.000000 }, { \"Name\":\"$int_cargorack_size7_class1_name;\", \"Name_Localised\":\"Cargo Rack\", \"StorageSlot\":101, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":976616, \"Hot\":false }, { \"Name\":\"$int_hyperdrive_size6_class5_name;\", \"Name_Localised\":\"FSD\", \"StorageSlot\":53, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":4535529, \"TransferTime\":55231, \"BuyPrice\":13752602, \"Hot\":false, \"EngineerModifications\":\"FSD_LongRange\", \"Level\":5, \"Quality\":0.000000 }, { \"Name\":\"$int_shieldgenerator_size5_class3_fast_name;\", \"Name_Localised\":\"Bi-Weave Shield\", \"StorageSlot\":116, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":280636, \"TransferTime\":55231, \"BuyPrice\":850659, \"Hot\":false, \"EngineerModifications\":\"ShieldGenerator_Optimised\", \"Level\":3, \"Quality\":0.000000 }, { \"Name\":\"$hpt_multicannon_gimbal_huge_name;\", \"Name_Localised\":\"Multi-Cannon\", \"StorageSlot\":107, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":1787862, \"TransferTime\":55231, \"BuyPrice\":5420960, \"Hot\":false, \"EngineerModifications\":\"Weapon_Overcharged\", \"Level\":4, \"Quality\":0.838000 }, { \"Name\":\"$int_repairer_size4_class5_name;\", \"Name_Localised\":\"AFM Unit\", \"StorageSlot\":114, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":1367146, \"TransferTime\":55231, \"BuyPrice\":4145240, \"Hot\":false, \"EngineerModifications\":\"Misc_Shielded\", \"Level\":3, \"Quality\":1.000000 }, { \"Name\":\"$int_refinery_size4_class5_name;\", \"Name_Localised\":\"Refinery\", \"StorageSlot\":102, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":3730077, \"Hot\":false }, { \"Name\":\"$int_fsdinterdictor_size4_class3_name;\", \"Name_Localised\":\"FSD Interdictor\", \"StorageSlot\":110, \"InTransit\":true, \"BuyPrice\":2311546, \"Hot\":false, \"EngineerModifications\":\"FSDinterdictor_Expanded\", \"Level\":4, \"Quality\":0.979500 }, { \"Name\":\"$int_hullreinforcement_size4_class2_name;\", \"Name_Localised\":\"Hull Reinforcement\", \"StorageSlot\":105, \"InTransit\":true, \"BuyPrice\":171113, \"Hot\":false, \"EngineerModifications\":\"HullReinforcement_HeavyDuty\", \"Level\":4, \"Quality\":0.000000 }, { \"Name\":\"$int_shieldgenerator_size3_class3_fast_name;\", \"Name_Localised\":\"Bi-Weave Shield\", \"StorageSlot\":113, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":24597, \"TransferTime\":55231, \"BuyPrice\":74284, \"Hot\":false, \"EngineerModifications\":\"ShieldGenerator_Optimised\", \"Level\":5, \"Quality\":0.956700 }, { \"Name\":\"$int_modulereinforcement_size3_class2_name;\", \"Name_Localised\":\"Module Reinforcement\", \"StorageSlot\":120, \"StarSystem\":\"HIP 21066\", \"MarketID\":3221959680, \"TransferCost\":27804, \"TransferTime\":56644, \"BuyPrice\":81900, \"Hot\":false }, { \"Name\":\"$int_hullreinforcement_size3_class2_name;\", \"Name_Localised\":\"Hull Reinforcement\", \"StorageSlot\":115, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":24408, \"TransferTime\":55231, \"BuyPrice\":73710, \"Hot\":false, \"EngineerModifications\":\"HullReinforcement_HeavyDuty\", \"Level\":4, \"Quality\":0.605000 }, { \"Name\":\"$int_dronecontrol_collection_size3_class2_name;\", \"Name_Localised\":\"Collector\", \"StorageSlot\":118, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":10530, \"Hot\":false, \"EngineerModifications\":\"CollectionLimpet_LightWeight\", \"Level\":5, \"Quality\":0.000000 }, { \"Name\":\"$int_dronecontrol_fueltransfer_size3_class2_name;\", \"Name_Localised\":\"Fuel Transfer\", \"StorageSlot\":57, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":3225, \"TransferTime\":55231, \"BuyPrice\":9477, \"Hot\":false, \"EngineerModifications\":\"FuelTransferLimpet_LightWeight\", \"Level\":4, \"Quality\":0.000000 }, { \"Name\":\"$hpt_mining_seismchrgwarhd_turret_medium_name;\", \"Name_Localised\":\"Seismic Charge\", \"StorageSlot\":111, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":390988, \"Hot\":false }, { \"Name\":\"$hpt_mining_subsurfdispmisle_turret_medium_name;\", \"Name_Localised\":\"Disp. Missile\", \"StorageSlot\":109, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":334986, \"Hot\":false }, { \"Name\":\"$int_modulereinforcement_size2_class2_name;\", \"Name_Localised\":\"Module Reinforcement\", \"StorageSlot\":112, \"StarSystem\":\"Wayutabal\", \"MarketID\":3224777984, \"TransferCost\":11773, \"TransferTime\":55696, \"BuyPrice\":35100, \"Hot\":false }, { \"Name\":\"$hpt_mininglaser_turret_medium_name;\", \"Name_Localised\":\"Mining Laser\", \"StorageSlot\":108, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":28587, \"Hot\":false }, { \"Name\":\"$int_dronecontrol_prospector_size1_class5_name;\", \"Name_Localised\":\"Prospector\", \"StorageSlot\":104, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":8424, \"Hot\":false }, { \"Name\":\"$hpt_mining_abrblstr_turret_small_name;\", \"Name_Localised\":\"Abrasion Blaster\", \"StorageSlot\":106, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":24114, \"Hot\":false }, { \"Name\":\"$int_corrosionproofcargorack_size1_class2_name;\", \"Name_Localised\":\"Corrosion Resistant Cargo Rack\", \"StorageSlot\":59, \"StarSystem\":\"Maia\", \"MarketID\":128679559, \"TransferCost\":4376, \"TransferTime\":58455, \"BuyPrice\":12249, \"Hot\":false }, { \"Name\":\"$int_corrosionproofcargorack_size1_class2_name;\", \"Name_Localised\":\"Corrosion Resistant Cargo Rack\", \"StorageSlot\":51, \"StarSystem\":\"Maia\", \"MarketID\":128679559, \"TransferCost\":4376, \"TransferTime\":58455, \"BuyPrice\":12249, \"Hot\":false }, { \"Name\":\"$int_corrosionproofcargorack_size1_class2_name;\", \"Name_Localised\":\"Corrosion Resistant Cargo Rack\", \"StorageSlot\":58, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":3621, \"TransferTime\":55231, \"BuyPrice\":10679, \"Hot\":false }, { \"Name\":\"$hpt_mrascanner_size0_class5_name;\", \"Name_Localised\":\"Pulse Wave\", \"StorageSlot\":100, \"StarSystem\":\"Omega Sector OD-S b4-0\", \"MarketID\":128928173, \"TransferCost\":0, \"TransferTime\":0, \"BuyPrice\":909217, \"Hot\":false }, { \"Name\":\"$hpt_heatsinklauncher_turret_tiny_name;\", \"Name_Localised\":\"Heatsink\", \"StorageSlot\":119, \"StarSystem\":\"Shinrarta Dezhra\", \"MarketID\":128666762, \"TransferCost\":1254, \"TransferTime\":55231, \"BuyPrice\":3500, \"Hot\":false, \"EngineerModifications\":\"HeatSinkLauncher_HeatSinkCapacity\", \"Level\":3, \"Quality\":0.000000 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            StoredModulesEvent @event = (StoredModulesEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(StoredModulesEvent));
        }

        [TestMethod]
        public void TestJournalSystemScanCompleteEvent()
        {
            string line = @"{""timestamp"":""2019-03-10T16:09:36Z"", ""event"":""FSSAllBodiesFound"", ""SystemName"":""Dumbae DN-I d10-6057"", ""SystemAddress"":208127228285531, ""Count"":19 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            SystemScanComplete @event = (SystemScanComplete)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(SystemScanComplete));
            Assert.AreEqual("Dumbae DN-I d10-6057", @event.systemname);
            Assert.AreEqual(208127228285531, @event.systemAddress);
            Assert.AreEqual(19, @event.count);
        }

        [TestMethod]
        public void TestMultiSellExplorationEvent()
        {
            string line = @"{ ""timestamp"":""2018-11-14T10:35:35Z"", ""event"":""MultiSellExplorationData"", ""Discovered"":[ { ""SystemName"":""HIP 84742"", ""NumBodies"":23 }, { ""SystemName"":""Col 359 Sector NY-S b20-1"", ""NumBodies"":9 } ], ""BaseValue"":2938186, ""Bonus"":291000, ""TotalEarnings"":3229186 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            ExplorationDataSoldEvent @event = (ExplorationDataSoldEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(ExplorationDataSoldEvent));
            Assert.AreEqual(2, @event.systems?.Count);
            Assert.AreEqual(2938186M, @event.reward);
            Assert.AreEqual(291000M, @event.bonus);
            Assert.AreEqual(3229186M, @event.total);
        }

        [TestMethod]
        public void TestSignalDetectedEvent()
        {
            // Test a scenario signal
            string line = @"{ ""timestamp"":""2019-02-06T07:22:27Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":1177567513979, ""SignalName"":""$MULTIPLAYER_SCENARIO42_TITLE;"", ""SignalName_Localised"":""Nav Beacon"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SignalDetectedEvent @event = (SignalDetectedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(SignalDetectedEvent));
            Assert.AreEqual("Nav Beacon", @event.signalSource.invariantName);
        }

        [TestMethod]
        public void TestSignalDetectedEvent2()
        {
            // Test a USS signal
            string line = @"{ ""timestamp"":""2019-02-17T19:39:57Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":60276065930987, ""SignalName"":""$USS;"", ""SignalName_Localised"":""Unidentified signal source"", ""USSType"":""$USS_Type_ValuableSalvage;"", ""USSType_Localised"":""Encoded emissions"", ""SpawningState"":""$FactionState_War_desc;"", ""SpawningState_Localised"":""The War state represents a conflict between the controlling faction in a system and a new faction that has expanded into the system."", ""SpawningFaction"":""Colonia Council"", ""ThreatLevel"":0, ""TimeRemaining"":2385.815674 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SignalDetectedEvent @event = (SignalDetectedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(SignalDetectedEvent));
            Assert.AreEqual("Encoded Emissions", @event.signalSource.invariantName);
            Assert.AreEqual("War", @event.signalSource.spawningState.invariantName);
            Assert.AreEqual("Colonia Council", @event.faction);
            Assert.AreEqual(0, @event.threatlevel);
            Assert.AreEqual(2385.816M, @event.secondsremaining);
        }

        [TestMethod]
        public void TestSignalDetectedEvent3()
        {
            // Test a uniquely named object signal
            string line = @"{ ""timestamp"":""2019-02-17T19:39:39Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":60276065930987, ""SignalName"":""Samson Class Bulk Cargo Ship GDZ-044"" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SignalDetectedEvent @event = (SignalDetectedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(SignalDetectedEvent));
            Assert.AreEqual("Samson Class Bulk Cargo Ship GDZ-044", @event.source);
        }

        [TestMethod]
        public void TestSignalDetectedEvent4()
        {
            // Test a carrier signal source
            string line = @"{ ""timestamp"":""2021-01-11T19:44:08Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":1733119939274, ""SignalName"":""PBSF SPACE ODDITY XBH-64Y"", ""IsStation"":true }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SignalDetectedEvent @event = (SignalDetectedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(SignalDetectedEvent));
            Assert.AreEqual("PBSF SPACE ODDITY", @event.signalSource.localizedName);
            Assert.AreEqual("XBH-64Y", @event.signalSource.invariantName);

            var testSystem = new StarSystem() { systemname = "Test System" };
            testSystem.AddOrUpdateSignalSource(@event.signalSource);
            Assert.AreEqual(1, testSystem.carriersignalsources.Count);
        }

        [TestMethod]
        public void TestSignalDetectedUnique()
        {
            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            privateObject.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "TestSystem", systemAddress = 6606892846275 });

            string line0 = @"{ ""timestamp"":""2019-02-04T02:20:28Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";
            string line1 = @"{ ""timestamp"":""2019-02-04T02:25:03Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";
            string line2 = @"{ ""timestamp"":""2019-02-04T02:28:26Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$Fixed_Event_Life_Ring;"", ""SignalName_Localised"":""Notable stellar phenomena"" }";
            string line3 = @"{ ""timestamp"":""2019-02-04T02:38:53Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$Fixed_Event_Life_Ring;"", ""SignalName_Localised"":""Notable stellar phenomena"" }";
            string line4 = @"{ ""timestamp"":""2019-02-04T02:38:53Z"", ""event"":""FSSSignalDiscovered"", ""SystemAddress"":6606892846275, ""SignalName"":""$NumberStation;"", ""SignalName_Localised"":""Unregistered Comms Beacon"" }";

            var events0 = JournalMonitor.ParseJournalEntry(line0);
            var event0 = (SignalDetectedEvent)events0[0];
            Assert.AreEqual("Unregistered Comms Beacon", event0.signalSource.invariantName);
            Assert.IsTrue(event0.unique);

            var events1 = JournalMonitor.ParseJournalEntry(line1);
            var event1 = (SignalDetectedEvent)events1[0];
            Assert.AreEqual("Unregistered Comms Beacon", event1.signalSource.invariantName);
            Assert.IsFalse(event1.unique);

            var events2 = JournalMonitor.ParseJournalEntry(line2);
            var event2 = (SignalDetectedEvent)events2[0];
            Assert.AreEqual("Notable Stellar Phenomena", event2.signalSource.invariantName);
            Assert.IsTrue(@event2.unique);

            var events3 = JournalMonitor.ParseJournalEntry(line3);
            var event3 = (SignalDetectedEvent)events3[0];
            Assert.AreEqual("Notable Stellar Phenomena", event3.signalSource.invariantName);
            Assert.IsFalse(@event3.unique);

            var events4 = JournalMonitor.ParseJournalEntry(line4);
            var event4 = (SignalDetectedEvent)events4[0];
            Assert.AreEqual("Unregistered Comms Beacon", event4.signalSource.invariantName);
            Assert.IsFalse(event4.unique);
        }

        [TestMethod]
        public void TestFSDJumpConflicts()
        {
            string line = @"{ ""timestamp"":""2019-06-30T05:38:53Z"", ""event"":""FSDJump"", ""StarSystem"":""Ogmar"", ""SystemAddress"":84180519395914, ""StarPos"":[-9534.00000,-905.28125,19802.03125], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_HighTech;"", ""SystemEconomy_Localised"":""High Tech"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_Confederacy;"", ""SystemGovernment_Localised"":""Confederacy"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":133000, ""Body"":""Ogmar A"", ""BodyID"":1, ""BodyType"":""Star"", ""JumpDist"":8.625, ""FuelUsed"":0.151982, ""FuelLevel"":31.695932, ""Factions"":[ { ""Name"":""Jaques"", ""FactionState"":""Election"", ""Government"":""Cooperative"", ""Influence"":0.104895, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand1;"", ""Happiness_Localised"":""Elated"", ""MyReputation"":100.000000, ""ActiveStates"":[ { ""State"":""Election"" } ] }, { ""Name"":""Colonia Research Department"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.078921, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":21.639999, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""Pilots' Federation Local Branch"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.000000, ""Allegiance"":""PilotsFederation"", ""Happiness"":"""", ""MyReputation"":100.000000 }, { ""Name"":""Colonia Mining Enterprise"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.052947, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000 }, { ""Name"":""Colonia Co-operative"", ""FactionState"":""Election"", ""Government"":""Cooperative"", ""Influence"":0.104895, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":71.470001, ""PendingStates"":[ { ""State"":""Expansion"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Outbreak"" }, { ""State"":""Election"" } ] }, { ""Name"":""Colonia Agricultural Co-operative"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.076923, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":6.640000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""GalCop Colonial Defence Commission"", ""FactionState"":""Boom"", ""Government"":""Confederacy"", ""Influence"":0.449550, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000, ""ActiveStates"":[ { ""State"":""Boom"" } ] }, { ""Name"":""Colonia Tech Combine"", ""FactionState"":""None"", ""Government"":""Cooperative"", ""Influence"":0.090909, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] }, { ""Name"":""Milanov's Reavers"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.040959, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""Outbreak"", ""Trend"":0 } ] } ], ""SystemFaction"":{ ""Name"":""GalCop Colonial Defence Commission"", ""FactionState"":""Boom"" }, ""Conflicts"":[ { ""WarType"":""election"", ""Status"":""active"", ""Faction1"":{ ""Name"":""Jaques"", ""Stake"":""Crockett Gateway"", ""WonDays"":1 }, ""Faction2"":{ ""Name"":""Colonia Co-operative"", ""Stake"":"""", ""WonDays"":2 } } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            JumpedEvent @event = (JumpedEvent)events[0];
            Assert.IsNotNull(@event);
            Assert.IsInstanceOfType(@event, typeof(JumpedEvent));
            Assert.IsNotNull(@event.conflicts);
            Assert.AreEqual(1, @event.conflicts.Count());
            Assert.AreEqual("Election", @event.conflicts[0].factionState.invariantName);
            Assert.AreEqual("active", @event.conflicts[0].status);
            Assert.AreEqual("Crockett Gateway", @event.conflicts[0].stake);
            Assert.AreEqual(3, @event.conflicts[0].conflictdays);
            Assert.AreEqual(1, @event.conflicts[0].margin);
            Assert.AreEqual("Jaques", @event.conflicts[0].faction1);
            Assert.AreEqual(1, @event.conflicts[0].faction1dayswon);
            Assert.AreEqual("Colonia Co-operative", @event.conflicts[0].faction2);
            Assert.AreEqual(2, @event.conflicts[0].faction2dayswon);
        }

        [TestMethod]
        public void TestStatisticsWithoutThargoidEncounters()
        {
            string line = @"{ ""timestamp"":""2019 - 09 - 13T23: 08:23Z"", ""event"":""Statistics"", ""Bank_Account"":{ ""Current_Wealth"":102745031, ""Spent_On_Ships"":63159760, ""Spent_On_Outfitting"":68503498, ""Spent_On_Repairs"":61377, ""Spent_On_Fuel"":18207, ""Spent_On_Ammo_Consumables"":26558, ""Insurance_Claims"":2, ""Spent_On_Insurance"":1981088, ""Owned_Ship_Count"":1 }, ""Combat"":{ ""Bounties_Claimed"":14, ""Bounty_Hunting_Profit"":318587, ""Combat_Bonds"":0, ""Combat_Bond_Profits"":0, ""Assassinations"":1, ""Assassination_Profits"":496428, ""Highest_Single_Reward"":96725, ""Skimmers_Killed"":0 }, ""Crime"":{ ""Notoriety"":0, ""Fines"":10, ""Total_Fines"":115301, ""Bounties_Received"":1, ""Total_Bounties"":400, ""Highest_Bounty"":400 }, ""Smuggling"":{ ""Black_Markets_Traded_With"":1, ""Black_Markets_Profits"":67, ""Resources_Smuggled"":2, ""Average_Profit"":33.5, ""Highest_Single_Transaction"":42 }, ""Trading"":{ ""Markets_Traded_With"":15, ""Market_Profits"":12890065, ""Resources_Traded"":11297, ""Average_Profit"":96917.781954887, ""Highest_Single_Transaction"":682212 }, ""Mining"":{ ""Mining_Profits"":0, ""Quantity_Mined"":0, ""Materials_Collected"":726 }, ""Exploration"":{ ""Systems_Visited"":334, ""Exploration_Profits"":6162412, ""Planets_Scanned_To_Level_2"":909, ""Planets_Scanned_To_Level_3"":909, ""Efficient_Scans"":3, ""Highest_Payout"":279383, ""Total_Hyperspace_Distance"":8890, ""Total_Hyperspace_Jumps"":482, ""Greatest_Distance_From_Start"":877.74392687704, ""Time_Played"":310680 }, ""Passengers"":{ ""Passengers_Missions_Accepted"":88, ""Passengers_Missions_Bulk"":1025, ""Passengers_Missions_VIP"":14, ""Passengers_Missions_Delivered"":1039, ""Passengers_Missions_Ejected"":0 }, ""Search_And_Rescue"":{ ""SearchRescue_Traded"":0, ""SearchRescue_Profit"":0, ""SearchRescue_Count"":0 }, ""Crafting"":{ ""Count_Of_Used_Engineers"":2, ""Recipes_Generated"":52, ""Recipes_Generated_Rank_1"":33, ""Recipes_Generated_Rank_2"":11, ""Recipes_Generated_Rank_3"":5, ""Recipes_Generated_Rank_4"":3, ""Recipes_Generated_Rank_5"":0 }, ""Crew"":{  }, ""Multicrew"":{ ""Multicrew_Time_Total"":7083, ""Multicrew_Gunner_Time_Total"":0, ""Multicrew_Fighter_Time_Total"":1769, ""Multicrew_Credits_Total"":104161, ""Multicrew_Fines_Total"":100 }, ""Material_Trader_Stats"":{ ""Trades_Completed"":3, ""Materials_Traded"":56, ""Raw_Materials_Traded"":56, ""Grade_1_Materials_Traded"":56 } }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            // Ideally we'd assert that it logs no warnings but that's not easy without mocking the logging framework.
            // Instead we test for a stanza that comes after the missing TG_ENCOUNTERS stanza.
            Assert.AreEqual(1, events.Count);
            StatisticsEvent statisticsEvent = (StatisticsEvent)events[0];
            Assert.AreEqual(7083, statisticsEvent.multicrew.timetotalseconds);
        }

        [TestMethod]
        public void TestReputationWithoutIndependent()
        {
            string line = @"{ ""timestamp"":""2019 - 09 - 13T23: 08:20Z"", ""event"":""Reputation"", ""Empire"":18.287001, ""Federation"":75.703102, ""Alliance"":1.179020 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            CommanderReputationEvent reputationEvent = (CommanderReputationEvent)events[0];
            Assert.AreEqual(18.287001M, reputationEvent.empire);
            Assert.AreEqual(75.703102M, reputationEvent.federation);
            Assert.AreEqual(1.179020M, reputationEvent.alliance);
            Assert.AreEqual(0M, reputationEvent.independent);
        }

        [TestMethod]
        public void TestTouchdownEventBio()
        {
            string line = "{ \"timestamp\":\"2019 - 09 - 26T06: 42:43Z\", \"event\":\"Touchdown\", \"PlayerControlled\":true, \"Taxi\":false, \"Multicrew\":false, \"StarSystem\":\"Nervi\", \"SystemAddress\":2518721481067, \"Body\":\"Nervi 2 a\", \"BodyID\":17, \"OnStation\":false, \"OnPlanet\":true, \"Latitude\":-44.165684, \"Longitude\":-123.219307, \"NearestDestination\":\"$SAA_Unknown_Signal:#type=$SAA_SignalType_Biological;:#index=15;\", \"NearestDestination_Localised\":\"Surface signal: Biological (15)\" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            TouchdownEvent @event = (TouchdownEvent)events[0];
            Assert.IsTrue(@event.playercontrolled);
            Assert.AreEqual(-44.165684M, @event.latitude);
            Assert.AreEqual(-123.219307M, @event.longitude);
            Assert.AreEqual("Biological Surface Signal", @event.nearestDestination.invariantName);
        }

        [TestMethod]
        public void TestTouchdownEventGuardian()
        {
            string line = "{ \"timestamp\":\"2019 - 09 - 26T04: 55:43Z\", \"event\":\"Touchdown\", \"PlayerControlled\":true, \"Taxi\":false, \"Multicrew\":false, \"StarSystem\":\"Nervi\", \"SystemAddress\":2518721481067, \"Body\":\"Nervi 2 a\", \"BodyID\":17, \"OnStation\":false, \"OnPlanet\":true, \"Latitude\":-44.464405, \"Longitude\":-95.072144, \"NearestDestination\":\"$Ancient_Tiny_003:#index=1;\", \"NearestDestination_Localised\":\"Guardian Structure\" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            TouchdownEvent @event = (TouchdownEvent)events[0];
            Assert.IsTrue(@event.playercontrolled);
            Assert.AreEqual(-44.464405M, @event.latitude);
            Assert.AreEqual(-95.072144M, @event.longitude);
            Assert.AreEqual("Guardian Structure", @event.nearestDestination.invariantName);
        }

        [TestMethod]
        public void TestEngineerProgress()
        {
            string startupLine = "{ \"timestamp\":\"2018-05-04T13:58:22Z\", \"event\":\"EngineerProgress\", \"Engineers\":[ { \"Engineer\":\"Zacariah Nemo\", \"EngineerID\":300050, \"Progress\":\"Invited\" }, { \"Engineer\":\"Marco Qwent\", \"EngineerID\":300200, \"Progress\":\"Unlocked\", \"RankProgress\":37, \"Rank\":4 }, { \"Engineer\":\"Hera Tani\", \"EngineerID\":300090, \"Progress\":\"Unlocked\", \"RankProgress\":0, \"Rank\":3 }, { \"Engineer\":\"Tod 'The Blaster' McQuinn\", \"EngineerID\":300260, \"Progress\":\"Unlocked\", \"RankProgress\":97, \"Rank\":3 }, { \"Engineer\":\"Selene Jean\", \"EngineerID\":300210, \"Progress\":\"Known\" }, { \"Engineer\":\"Lei Cheung\", \"EngineerID\":300120, \"Progress\":\"Known\" }, { \"Engineer\":\"Juri Ishmaak\", \"EngineerID\":300250, \"Progress\":\"Known\" }, { \"Engineer\":\"Felicity Farseer\", \"EngineerID\":300100, \"Progress\":\"Unlocked\", \"RankProgress\":0, \"Rank\":5 }, { \"Engineer\":\"Professor Palin\", \"EngineerID\":300220, \"Progress\":\"Invited\" }, { \"Engineer\":\"Elvira Martuuk\", \"EngineerID\":300160, \"Progress\":\"Unlocked\", \"RankProgress\":0, \"Rank\":5 }, { \"Engineer\":\"Lori Jameson\", \"EngineerID\":300230, \"Progress\":\"Known\" }, { \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"Progress\":\"Unlocked\", \"RankProgress\":0, \"Rank\":5 }, { \"Engineer\":\"Liz Ryder\", \"EngineerID\":300080, \"Progress\":\"Unlocked\", \"RankProgress\":93, \"Rank\":3 }, { \"Engineer\":\"Ram Tah\", \"EngineerID\":300110, \"Progress\":\"Unlocked\", \"RankProgress\":31, \"Rank\":3 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(startupLine);
            EngineerProgressedEvent startupEvent = (EngineerProgressedEvent)events[0];
            Assert.IsNull(startupEvent.Engineer);
            Assert.AreEqual("Invited", Engineer.FromName("Zacariah Nemo")?.stage);
            Assert.IsNull(Engineer.FromName("Zacariah Nemo")?.rank);

            // When an engineer is unlocked, there should be an event for the change in stage and a second event for the change in rank...
            string updateLine = "{ \"timestamp\":\"2018-01-16T09:34:36Z\", \"event\":\"EngineerProgress\", \"Engineer\":\"Zacariah Nemo\", \"EngineerID\":300050, \"Progress\":\"Unlocked\", \"Rank\":1, \"RankProgress\":0 }";
            events = JournalMonitor.ParseJournalEntry(updateLine);
            Assert.AreEqual(2, events.Count);
            EngineerProgressedEvent stageEvent = (EngineerProgressedEvent)events[0];
            Assert.AreEqual("Zacariah Nemo", stageEvent.Engineer?.name);
            Assert.AreEqual(300050, stageEvent.Engineer?.id);
            Assert.AreEqual("Unlocked", stageEvent.Engineer?.stage);
            EngineerProgressedEvent rankEvent = (EngineerProgressedEvent)events[1];
            Assert.AreEqual(1, rankEvent.Engineer?.rank);

            // We expect one event for the change in rank...
            string updateLine2 = "{ \"timestamp\":\"2018-01-16T09:34:36Z\", \"event\":\"EngineerProgress\", \"Engineer\":\"Zacariah Nemo\", \"EngineerID\":300050, \"Progress\":\"Unlocked\", \"Rank\":2, \"RankProgress\":0 }";
            events = JournalMonitor.ParseJournalEntry(updateLine2);
            Assert.AreEqual(1, events.Count);
            EngineerProgressedEvent rankEvent2 = (EngineerProgressedEvent)events[0];
            Assert.AreEqual(2, rankEvent2.Engineer?.rank);
        }

        [TestMethod]
        public void TestCarrierJumpedEvent()
        {
            string line = "{\"timestamp\":\"2020-05-17T14:07:24Z\",\"event\":\"CarrierJump\",\"Docked\":true,\"StationName\":\"G53-K3Q\",\"StationType\":\"FleetCarrier\",\"MarketID\":3700571136,\"StationFaction\":{\"Name\":\"FleetCarrier\"},\"StationGovernment\":\"$government_Carrier;\",\"StationGovernment_Localised\":\"Private Ownership \",\"StationServices\":[\"dock\",\"autodock\",\"blackmarket\",\"commodities\",\"contacts\",\"exploration\",\"crewlounge\",\"rearm\",\"refuel\",\"repair\",\"engineer\",\"flightcontroller\",\"stationoperations\",\"stationMenu\",\"carriermanagement\",\"carrierfuel\",\"voucherredemption\"],\"StationEconomy\":\"$economy_Carrier;\",\"StationEconomy_Localised\":\"Private Enterprise\",\"StationEconomies\":[{\"Name\":\"$economy_Carrier;\",\"Name_Localised\":\"Private Enterprise\",\"Proportion\":1}],\"StarSystem\":\"Aparctias\",\"SystemAddress\":358797513434,\"StarPos\":[25.1875,-56.375,22.90625],\"SystemAllegiance\":\"Independent\",\"SystemEconomy\":\"$economy_Colony;\",\"SystemEconomy_Localised\":\"Colony\",\"SystemSecondEconomy\":\"$economy_Refinery;\",\"SystemSecondEconomy_Localised\":\"Refinery\",\"SystemGovernment\":\"$government_Dictatorship;\",\"SystemGovernment_Localised\":\"Dictatorship\",\"SystemSecurity\":\"$SYSTEM_SECURITY_medium;\",\"SystemSecurity_Localised\":\"Medium Security\",\"Population\":80000,\"Body\":\"Aparctias\",\"BodyID\":0,\"BodyType\":\"Star\",\"Powers\":[\"Yuri Grom\"],\"PowerplayState\":\"Exploited\",\"Factions\":[{\"Name\":\"Union of Aparctias Future\",\"FactionState\":\"None\",\"Government\":\"Democracy\",\"Influence\":0.062,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0},{\"Name\":\"Monarchy of Aparctias\",\"FactionState\":\"None\",\"Government\":\"Feudal\",\"Influence\":0.035,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0},{\"Name\":\"Aparctias Purple Council\",\"FactionState\":\"Boom\",\"Government\":\"Anarchy\",\"Influence\":0.049,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"ActiveStates\":[{\"State\":\"Boom\"}]},{\"Name\":\"Beta-3 Tucani Silver Allied Net\",\"FactionState\":\"None\",\"Government\":\"Corporate\",\"Influence\":0.096,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0.32538},{\"Name\":\"Falcons' Nest\",\"FactionState\":\"None\",\"Government\":\"Confederacy\",\"Influence\":0.078,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"RecoveringStates\":[{\"State\":\"NaturalDisaster\",\"Trend\":0}]},{\"Name\":\"EG Union\",\"FactionState\":\"War\",\"Government\":\"Dictatorship\",\"Influence\":0.34,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"ActiveStates\":[{\"State\":\"Boom\"},{\"State\":\"War\"}]},{\"Name\":\"Paladin Consortium\",\"FactionState\":\"War\",\"Government\":\"Democracy\",\"Influence\":0.34,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"PendingStates\":[{\"State\":\"Boom\",\"Trend\":0},{\"State\":\"CivilLiberty\",\"Trend\":0}],\"ActiveStates\":[{\"State\":\"War\"}]}],\"SystemFaction\":{\"Name\":\"EG Union\",\"FactionState\":\"War\"},\"Conflicts\":[{\"WarType\":\"war\",\"Status\":\"active\",\"Faction1\":{\"Name\":\"EG Union\",\"Stake\":\"Hancock Refinery\",\"WonDays\":1},\"Faction2\":{\"Name\":\"Paladin Consortium\",\"Stake\":\"\",\"WonDays\":0}}]}";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            CarrierJumpedEvent @event = (CarrierJumpedEvent)events[0];
            Assert.IsTrue(@event.docked);
            Assert.AreEqual("G53-K3Q", @event.carriername);
            Assert.AreEqual("Fleet Carrier", @event.carrierType.invariantName);
            Assert.AreEqual(3700571136, @event.carrierId);
            Assert.AreEqual("FleetCarrier", @event.carrierFaction.name);
            Assert.AreEqual("Private Ownership", @event.carrierFaction.Government.invariantName);
            Assert.AreEqual(17, @event.carrierServices.Count);
            Assert.AreEqual(1, @event.carrierEconomies.Count);
            Assert.AreEqual("Private Enterprise", @event.carrierEconomies[0].economy.invariantName);
            Assert.AreEqual("Aparctias", @event.systemname);
            Assert.AreEqual((ulong)358797513434, @event.systemAddress);
            Assert.AreEqual(25.1875M, @event.x);
            Assert.AreEqual(-56.375M, @event.y);
            Assert.AreEqual(22.90625M, @event.z);
            Assert.AreEqual("Independent", @event.controllingsystemfaction.Allegiance.invariantName);
            Assert.AreEqual("Colony", @event.systemEconomy.invariantName);
            Assert.AreEqual("Refinery", @event.systemEconomy2.invariantName);
            Assert.AreEqual("Dictatorship", @event.controllingsystemfaction.Government.invariantName);
            Assert.AreEqual("Medium", @event.securityLevel.invariantName);
            Assert.AreEqual(80000, @event.population);
            Assert.AreEqual("Aparctias", @event.bodyname);
            Assert.AreEqual(0, @event.bodyId);
            Assert.AreEqual("Star", @event.bodyType.invariantName);
            Assert.AreEqual("Yuri Grom", @event.Power.invariantName);
            Assert.AreEqual("Exploited", @event.powerState.invariantName);
            Assert.AreEqual(7, @event.factions.Count);
            Assert.AreEqual("EG Union", @event.controllingsystemfaction.name);
            Assert.AreEqual("War", @event.controllingsystemfaction.presences.FirstOrDefault(p => p.systemName == "Aparctias")?.FactionState.invariantName);
            Assert.AreEqual(1, @event.conflicts.Count);
            Assert.AreEqual("EG Union", @event.conflicts[0].faction1);
            Assert.AreEqual("Paladin Consortium", @event.conflicts[0].faction2);
        }

        [TestMethod]
        public void TestShipRepairedEvent()
        {
            string line = "{ \"timestamp\":\"2016-09-25T12:31:38Z\", \"event\":\"Repair\", \"Item\":\"Wear\", \"Cost\":2824 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ShipRepairedEvent @event = (ShipRepairedEvent)events[0];
            Assert.IsInstanceOfType(@event.items, typeof(List<string>));
            Assert.IsInstanceOfType(@event.modules, typeof(List<Module>));
            Assert.AreEqual(EddiDataDefinitions.Properties.Modules.ShipIntegrity, @event.items?[0]);
            Assert.AreEqual(0, @event.modules.Count);
            Assert.AreEqual(2824, @event.price);
        }

        [TestMethod]
        public void TestShipRepairedEvent2()
        {
            string line = "{ \"timestamp\":\"2020-03-31T13:39:42Z\", \"event\":\"Repair\", \"Items\":[ \"$hpt_dumbfiremissilerack_fixed_large_name;\", \"$hpt_beamlaser_gimbal_medium_name;\", \"$hpt_railgun_fixed_medium_name;\", \"$hpt_beamlaser_gimbal_medium_name;\", \"$hpt_dumbfiremissilerack_fixed_large_name;\" ], \"Cost\":34590 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ShipRepairedEvent @event = (ShipRepairedEvent)events[0];
            Assert.IsInstanceOfType(@event.items, typeof(List<string>));
            Assert.IsInstanceOfType(@event.modules, typeof(List<Module>));
            Assert.AreEqual(0, @event.items.Count);
            Assert.AreEqual(5, @event.modules.Count);
            Assert.AreEqual(34590, @event.price);
        }

        [TestMethod]
        public void TestShipRepairedEvent3()
        {
            string line = "{ \"timestamp\":\"2020-05-13T08:45:03Z\", \"event\":\"RepairAll\", \"Cost\":104817 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ShipRepairedEvent @event = (ShipRepairedEvent)events[0];
            Assert.IsInstanceOfType(@event.items, typeof(List<string>));
            Assert.IsInstanceOfType(@event.modules, typeof(List<Module>));
            Assert.AreEqual("All", @event.items[0]);
            Assert.AreEqual(0, @event.modules.Count);
            Assert.AreEqual(104817, @event.price);
        }

        [TestMethod]
        public void TestShipRepairedEvent4()
        {
            string line = "{ \"timestamp\":\"2020-03-31T13:39:42Z\", \"event\":\"Repair\", \"Items\":[ \"Wear\" ], \"Cost\":2824 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ShipRepairedEvent @event = (ShipRepairedEvent)events[0];
            Assert.IsInstanceOfType(@event.items, typeof(List<string>));
            Assert.IsInstanceOfType(@event.modules, typeof(List<Module>));
            Assert.AreEqual(EddiDataDefinitions.Properties.Modules.ShipIntegrity, @event.items?[0]);
            Assert.AreEqual(0, @event.modules.Count);
            Assert.AreEqual(2824, @event.price);
        }

        [TestMethod]
        public void TestCarrierJumpRequestMissingBody()
        {
            // There is an FDev bug which caused the `Body` property not to be written for a `CarrierJumpRequest` event.
            // Test that we handle that scenario gracefully.

            // Set up our data resources with canned data
            string systemsResource = "api-v1/systems";
            string systemsJson = "[" + Encoding.UTF8.GetString(Resources.edsmSystem) + "]";
            List<JObject> systemsData = new List<JObject>();
            fakeEdsmRestClient.Expect(systemsResource, systemsJson, systemsData);
            string bodiesResource = "api-system-v1/bodies";
            string bodiesJson = Encoding.UTF8.GetString(Resources.edsmBodies);
            List<JObject> bodiesData = new List<JObject>();
            fakeEdsmRestClient.Expect(bodiesResource, bodiesJson, bodiesData);

            // Parse the event
            string line = "{ \"timestamp\":\"2020-06-12T11:01:40Z\", \"event\":\"CarrierJumpRequest\", \"CarrierID\":3701442048, \"SystemName\":\"Shinrarta Dezhra\", \"SystemAddress\":3932277478106, \"BodyID\":16, \"DepartureTime\":\"2023-05-22T09:09:57Z\" }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            CarrierJumpRequestEvent @event = (CarrierJumpRequestEvent)events[0];

            // Declare our expected value
            CarrierJumpRequestEvent expectedEvent = new CarrierJumpRequestEvent(new DateTime(2020, 6, 12, 11, 1, 40, DateTimeKind.Utc), "Shinrarta Dezhra", 3932277478106, "Shinrarta Dezhra B 2", 16, 3701442048) { raw = line, fromLoad = false };

            // Assert the results
            Assert.IsTrue(expectedEvent.DeepEquals(@event));
        }

        [TestMethod]
        public void TestShipInterdictedByPlayerEvent()
        {
            string line = @"{ ""timestamp"":""2021-01-14T16:15:01Z"", ""event"":""Interdicted"", ""Submitted"":true, ""Interdictor"":""Blaes"", ""IsPlayer"":true, ""CombatRank"":5 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            ShipInterdictedEvent @event = (ShipInterdictedEvent)events[0];
            Assert.AreEqual("Ship interdicted", @event.type);
            Assert.IsTrue(@event.submitted);
            Assert.AreEqual("Blaes", @event.interdictor);
            Assert.IsTrue(@event.iscommander);
            Assert.AreEqual(CombatRating.FromRank(5).localizedName, @event.rating);
            Assert.IsNull(@event.power);
        }

        [TestMethod]
        public void TestAFMURepairsEvent()
        {
            string line1 = @"{ ""timestamp"":""2021-01-30T00:28:18Z"", ""event"":""AfmuRepairs"", ""Module"":""$int_modulereinforcement_size2_class2_name;"", ""Module_Localised"":""Module Reinforcement"", ""FullyRepaired"":false, ""Health"":1.000000 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line1);
            ShipAfmuRepairedEvent event1 = (ShipAfmuRepairedEvent)events[0];
            Assert.AreEqual("Module Reinforcement Package", event1.item);
            Assert.AreEqual(1M, event1.health);
            // There is an FDev bug that can set `repairedfully` to false even when the module health is full.
            // This appears to be a unique problem with Module Reinforcement Packages. We need to work around this.
            Assert.IsTrue(event1.repairedfully);

            string line2 = @"{ ""timestamp"":""2020-05-31T16:37:08Z"", ""event"":""AfmuRepairs"", ""Module"":""$hpt_multicannon_gimbal_small_name;"", ""Module_Localised"":""Multi-Cannon"", ""FullyRepaired"":true, ""Health"":1.000000 }";
            events = JournalMonitor.ParseJournalEntry(line2);
            ShipAfmuRepairedEvent event2 = (ShipAfmuRepairedEvent)events[0];
            Assert.AreEqual("1G gimballed Multi-Cannon", event2.item);
            Assert.AreEqual(1M, event2.health);
            Assert.IsTrue(event2.repairedfully);

            string line3 = @"{ ""timestamp"":""2020-05-31T16:38:56Z"", ""event"":""AfmuRepairs"", ""Module"":""$hpt_beamlaser_gimbal_medium_name;"", ""Module_Localised"":""Beam Laser"", ""FullyRepaired"":false, ""Health"":0.993321 }";
            events = JournalMonitor.ParseJournalEntry(line3);
            ShipAfmuRepairedEvent event3 = (ShipAfmuRepairedEvent)events[0];
            Assert.AreEqual("2D gimballed Beam Laser", event3.item);
            Assert.AreEqual(0.993321M, event3.health);
            Assert.IsFalse(event3.repairedfully);
        }

        [TestMethod]
        public void TestCommunityGoalsEvent()
        {
            string line = @"{
               ""timestamp"": ""2021-02-27T15:32:42Z"",
               ""event"": ""CommunityGoal"",
               ""CurrentGoals"": [
                  {
                     ""CGID"": 641,
                     ""Title"": ""Defence of the Galactic Summit"",
                     ""SystemName"": ""Sirius"",
                     ""MarketName"": ""Spirit of Laelaps"",
                     ""Expiry"": ""2021-03-04T06:00:00Z"",
                     ""IsComplete"": false,
                     ""CurrentTotal"": 163782436330,
                     ""PlayerContribution"": 84049848,
                     ""NumContributors"": 8354,
                     ""TopTier"": {
                        ""Name"": ""Tier 8"",
                        ""Bonus"": """"
                     },
                     ""TopRankSize"": 10,
                     ""PlayerInTopRank"": false,
                     ""TierReached"": ""Tier 5"",
                     ""PlayerPercentileBand"": 10,
                     ""Bonus"": 100000000
                  }
               ]
            }";
            var events = JournalMonitor.ParseJournalEntry(line);
            var @event = (CommunityGoalsEvent)events[0];
            var goal = @event.goals[0];
            Assert.AreEqual(641, goal.cgid);
            Assert.AreEqual("Defence of the Galactic Summit", goal.name);
            Assert.AreEqual("Sirius", goal.system);
            Assert.AreEqual("Spirit of Laelaps", goal.station);
            Assert.AreEqual(DateTime.Parse("2021-03-04T06:00:00Z").ToUniversalTime(), goal.expiryDateTime);
            Assert.AreEqual(false, goal.iscomplete);
            Assert.AreEqual(163782436330, goal.total);
            Assert.AreEqual(84049848, goal.contribution);
            Assert.AreEqual(8354, goal.contributors);
            Assert.AreEqual(8, goal.toptier);
            Assert.AreEqual("", goal.toptierreward);
            Assert.AreEqual(10, goal.topranksize);
            Assert.AreEqual(false, goal.toprank);
            Assert.AreEqual(5, goal.tier);
            Assert.AreEqual(10, goal.percentileband);
            Assert.AreEqual(100000000, goal.tierreward);
        }

        [TestMethod]
        public void TestCommanderContinuedOnFoot()
        {
            string line = @"{
                ""timestamp"": ""2021-04-30T21:50:03Z"",
                ""event"": ""LoadGame"",
                ""FID"": ""F100000"",
                ""Commander"": ""John Jameson"",
                ""Horizons"": true,
                ""Odyssey"": true,
                ""Ship"": ""ExplorationSuit_Class1"",
                ""Ship_Localised"": ""Artemis Suit"",
                ""ShipID"": 4293000003,
                ""ShipName"": """",
                ""ShipIdent"": """",
                ""FuelLevel"": 1.000000,
                ""FuelCapacity"": 1.000000,
                ""GameMode"": ""Open"",
                ""Credits"": 294004749,
                ""Loan"": 0
            }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);

            var @event = (CommanderContinuedEvent)events[0];
            Assert.AreEqual("Commander continued", @event.type);
            Assert.AreEqual("F100000", @event.frontierID);
            Assert.AreEqual("John Jameson", @event.commander);
            Assert.IsTrue(@event.horizons);
            Assert.IsTrue(@event.odyssey);
            Assert.AreEqual("ExplorationSuit_Class1", @event.shipEDModel);
            Assert.AreEqual(Constants.VEHICLE_LEGS, @event.ship);
            Assert.AreEqual(4293000003, @event.shipid);
            Assert.IsTrue(string.IsNullOrEmpty(@event.shipname));
            Assert.IsTrue(string.IsNullOrEmpty(@event.shipident));
            Assert.AreEqual(1M, @event.fuel);
            Assert.AreEqual(1M, @event.fuelcapacity);
            Assert.AreEqual("Open", @event.mode);
            Assert.AreEqual(294004749, @event.credits);
            Assert.AreEqual(0, @event.loan);
        }

        [TestMethod]
        public void TestCommanderContinuedInApexTaxi()
        {
            string line = @"{
                ""timestamp"": ""2021-04-30T21:59:36Z"",
                ""event"": ""LoadGame"",
                ""FID"": ""F100000"",
                ""Commander"": ""John Jameson"",
                ""Horizons"": true,
                ""Odyssey"": true,
                ""Ship"": ""adder_taxi"",
                ""Ship_Localised"": ""$ADDER_NAME;"",
                ""GameMode"": ""Open"",
                ""Credits"": 294004649,
                ""Loan"": 0
            }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);

            var @event = (CommanderContinuedEvent)events[0];
            Assert.AreEqual("Commander continued", @event.type);
            Assert.AreEqual("F100000", @event.frontierID);
            Assert.AreEqual("John Jameson", @event.commander);
            Assert.IsTrue(@event.horizons);
            Assert.IsTrue(@event.odyssey);
            Assert.AreEqual("adder_taxi", @event.shipEDModel);
            Assert.AreEqual(Constants.VEHICLE_TAXI, @event.ship);
            Assert.IsNull(@event.shipid);
            Assert.IsTrue(string.IsNullOrEmpty(@event.shipname));
            Assert.IsTrue(string.IsNullOrEmpty(@event.shipident));
            Assert.IsNull(@event.fuel);
            Assert.IsNull(@event.fuelcapacity);
            Assert.AreEqual("Open", @event.mode);
            Assert.AreEqual(294004649, @event.credits);
            Assert.AreEqual(0, @event.loan);
        }

        [TestMethod]
        public void TestCommanderContinuedCQC()
        {
            string line = @"{
                ""timestamp"": ""2021-04-30T21:59:36Z"",
                ""event"": ""LoadGame"",
                ""FID"": ""F100000"",
                ""Commander"": ""John Jameson"",
                ""Horizons"": true,
                ""Odyssey"": true,
                ""Credits"": 594877206,
                ""Loan"": 0
            }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);

            var @event = (EnteredCQCEvent)events[0];
            Assert.AreEqual("John Jameson", @event.commander);
        }

        [TestMethod]
        public void TestCommanderContinuedShip()
        {
            string line = @"{
                ""timestamp"": ""2021-05-01T03:12:27Z"",
                ""event"": ""LoadGame"",
                ""FID"": ""F100000"",
                ""Commander"": ""John Jameson"",
                ""Horizons"": true,
                ""Odyssey"": true,
                ""Ship"": ""DiamondBackXL"",
                ""Ship_Localised"": ""Diamondback Explorer"",
                ""ShipID"": 38,
                ""ShipName"": ""Resolution"",
                ""ShipIdent"": ""TK-28D"",
                ""FuelLevel"": 32.000000,
                ""FuelCapacity"": 32.000000,
                ""GameMode"": ""Solo"",
                ""Credits"": 7795285167,
                ""Loan"": 0
            }";

            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);

            var @event = (CommanderContinuedEvent)events[0];
            Assert.AreEqual("Commander continued", @event.type);
            Assert.AreEqual("F100000", @event.frontierID);
            Assert.AreEqual("John Jameson", @event.commander);
            Assert.IsTrue(@event.horizons);
            Assert.IsTrue(@event.odyssey);
            Assert.AreEqual("DiamondBackXL", @event.shipEDModel);
            Assert.AreEqual("Diamondback Explorer", @event.ship);
            Assert.AreEqual(38, @event.shipid);
            Assert.AreEqual("Resolution", @event.shipname);
            Assert.AreEqual("TK-28D", @event.shipident);
            Assert.AreEqual(32M, @event.fuel);
            Assert.AreEqual(32M, @event.fuelcapacity);
            Assert.AreEqual("Solo", @event.mode);
            Assert.AreEqual(7795285167, @event.credits);
            Assert.AreEqual(0, @event.loan);
        }

        [DataTestMethod]
        [DataRow(@"{ ""timestamp"":""2021-01-08T13:26:17Z"", ""event"":""Died"", ""KillerName"":""Xepherous"", ""KillerShip"":""federation_dropship"", ""KillerRank"":""Master"" }", @"{""killers"":[{""name"":""Xepherous"",""rating"":""Master"",""equipment"":""Federal Dropship""}],""raw"":""{ \""timestamp\"":\""2021-01-08T13:26:17Z\"", \""event\"":\""Died\"", \""KillerName\"":\""Xepherous\"", \""KillerShip\"":\""federation_dropship\"", \""KillerRank\"":\""Master\"" }"",""timestamp"":""2021-01-08T13:26:17Z"",""type"":""Died"",""fromLoad"":false}")]
        [DataRow(@"{ ""timestamp"":""2021-05-07T11:34:12Z"", ""event"":""Died"", ""KillerName"":""Gretchen Rasmussen"", ""KillerShip"":""assaultsuitai_class1"", ""KillerRank"":""Harmless"" }", @"{""killers"":[{""name"":""Gretchen Rasmussen"",""rating"":""Harmless"",""equipment"":""Commando""}],""raw"":""{ \""timestamp\"":\""2021-05-07T11:34:12Z\"", \""event\"":\""Died\"", \""KillerName\"":\""Gretchen Rasmussen\"", \""KillerShip\"":\""assaultsuitai_class1\"", \""KillerRank\"":\""Harmless\"" }"",""timestamp"":""2021-05-07T11:34:12Z"",""type"":""Died"",""fromLoad"":false}")]
        [DataRow(@"{ ""timestamp"":""2021-05-07T12:08:29Z"", ""event"":""Died"", ""KillerShip"":""ps_turretbasesmall_3m"" }", @"{""killers"":[],""raw"":""{ \""timestamp\"":\""2021-05-07T12:08:29Z\"", \""event\"":\""Died\"", \""KillerShip\"":\""ps_turretbasesmall_3m\"" }"",""timestamp"":""2021-05-07T12:08:29Z"",""type"":""Died"",""fromLoad"":false}")]
        [DataRow(@"{ ""timestamp"":""2017-04-08T22:25:38Z"", ""event"":""Died"", ""Killers"":[ { ""Name"":""Cmdr Uno"", ""Ship"":""federation_dropship_mkii"", ""Rank"":""Elite"" }, { ""Name"":""Cmdr Dos"", ""Ship"":""cutter"", ""Rank"":""Elite"" }, { ""Name"":""Cmdr Tres"", ""Ship"":""empire_trader"", ""Rank"":""Elite"" } ] }", @"{""killers"":[],""raw"":""{ \""timestamp\"":\""2017-04-08T22:25:38Z\"", \""event\"":\""Died\"", \""Killers\"":[ { \""Name\"":\""Cmdr Uno\"", \""Ship\"":\""federation_dropship_mkii\"", \""Rank\"":\""Elite\"" }, { \""Name\"":\""Cmdr Dos\"", \""Ship\"":\""cutter\"", \""Rank\"":\""Elite\"" }, { \""Name\"":\""Cmdr Tres\"", \""Ship\"":\""empire_trader\"", \""Rank\"":\""Elite\"" } ] }"",""timestamp"":""2017-04-08T22:25:38Z"",""type"":""Died"",""fromLoad"":false}")]
        public void TestDiedEvent(string line, string expected)
        {
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (DiedEvent)events[0];
            Assert.AreEqual(expected, JsonConvert.SerializeObject(@event));
        }

        [DataTestMethod]
        [DataRow(@"{ ""timestamp"":""2021-05-03T22:22:12Z"", ""event"":""Embark"", ""SRV"":false, ""Taxi"":false, ""Multicrew"":false, ""ID"":6, ""StarSystem"":""Sumod"", ""SystemAddress"":3961847269739, ""Body"":""Sharp Dock"", ""BodyID"":56, ""OnStation"":true, ""OnPlanet"":false, ""StationName"":""Sharp Dock"", ""StationType"":""Coriolis"", ""MarketID"":32239521286 }", false, true, false, false, 6, "Sumod", 3961847269739UL, "Sharp Dock", 56, "Sharp Dock", "Coriolis Starport", 32239521286, true, false)] // Embarking from an orbital station to your ship
        [DataRow(@"{ ""timestamp"":""2021-05-02T22:51:54Z"", ""event"":""Embark"", ""SRV"":true, ""Taxi"":false, ""Multicrew"":false, ""ID"":53, ""StarSystem"":""Nervi"", ""SystemAddress"":2518721481067, ""Body"":""Nervi 2 a"", ""BodyID"":17, ""OnStation"":false, ""OnPlanet"":true }", false, false, true, false, 53, "Nervi", 2518721481067UL, "Nervi 2 a", 17, null, null, null, false, true)] // Embarking from a surface to an SRV
        [DataRow(@"{ ""timestamp"":""2021-05-03T21:51:47Z"", ""event"":""Embark"", ""SRV"":false, ""Taxi"":true, ""Multicrew"":false, ""StarSystem"":""Firenses"", ""SystemAddress"":2868635379121, ""Body"":""Roberts Gateway"", ""BodyID"":44, ""OnStation"":true, ""OnPlanet"":false, ""StationName"":""Roberts Gateway"", ""StationType"":""Coriolis"", ""MarketID"":32216360961 }", false, false, false, true, null, "Firenses", 2868635379121UL, "Roberts Gateway", 44, "Roberts Gateway", "Coriolis Starport", 32216360961, true, false)] // Embarking from an orbital station to a taxi
        public void TestEmbarkEvent(string line, bool toMulticrew, bool toShip, bool toSRV, bool toTaxi, int? toLocalId, string systemName, ulong systemAddress, string bodyName, int? bodyId, string station, string stationType, long? marketId, bool onStation, bool onPlanet)
        {
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (EmbarkEvent)events[0];
            Assert.AreEqual(toMulticrew, @event.tomulticrew);
            Assert.AreEqual(toShip, @event.toship);
            Assert.AreEqual(toSRV, @event.tosrv);
            Assert.AreEqual(toTaxi, @event.totransport);
            Assert.AreEqual(toLocalId, @event.toLocalId);
            Assert.AreEqual(systemName, @event.systemname);
            Assert.AreEqual(systemAddress, @event.systemAddress);
            Assert.AreEqual(bodyName, @event.bodyname);
            Assert.AreEqual(bodyId, @event.bodyId);
            Assert.AreEqual(station, @event.station);
            Assert.AreEqual(stationType, @event.stationModel?.invariantName);
            Assert.AreEqual(marketId, @event.marketId);
            Assert.AreEqual(onPlanet, @event.onplanet);
            Assert.AreEqual(onStation, @event.onstation);
        }

        [DataTestMethod]
        [DataRow(@"{ ""timestamp"":""2021-05-03T21:57:16Z"", ""event"":""Disembark"", ""SRV"":false, ""Taxi"":true, ""Multicrew"":false, ""StarSystem"":""Sumod"", ""SystemAddress"":3961847269739, ""Body"":""Sharp Dock"", ""BodyID"":56, ""OnStation"":true, ""OnPlanet"":false, ""StationName"":""Sharp Dock"", ""StationType"":""Coriolis"", ""MarketID"":32239521286 }", false, false, false, true, null, "Sumod", 3961847269739UL, "Sharp Dock", 56, "Sharp Dock", "Coriolis Starport", 32239521286, true, false)] // Disembarking from a taxi to an orbital station
        [DataRow(@"{ ""timestamp"":""2021-05-03T21:47:38Z"", ""event"":""Disembark"", ""SRV"":false, ""Taxi"":false, ""Multicrew"":false, ""ID"":6, ""StarSystem"":""Firenses"", ""SystemAddress"":2868635379121, ""Body"":""Roberts Gateway"", ""BodyID"":44, ""OnStation"":true, ""OnPlanet"":false, ""StationName"":""Roberts Gateway"", ""StationType"":""Coriolis"", ""MarketID"":32216360961 }", false, true, false, false, 6, "Firenses", 2868635379121UL, "Roberts Gateway", 44, "Roberts Gateway", "Coriolis Starport", 32216360961, true, false)] // Disembarking from your ship to an orbital station
        [DataRow(@"{ ""timestamp"":""2021-05-02T22:52:25Z"", ""event"":""Disembark"", ""SRV"":true, ""Taxi"":false, ""Multicrew"":false, ""ID"":53, ""StarSystem"":""Nervi"", ""SystemAddress"":2518721481067, ""Body"":""Nervi 2 a"", ""BodyID"":17, ""OnStation"":false, ""OnPlanet"":true }", false, false, true, false, 53, "Nervi", 2518721481067UL, "Nervi 2 a", 17, null, null, null, false, true)] // Disembarking to an SRV from on foot
        public void TestDisembarkEvent(string line, bool fromMulticrew, bool fromShip, bool fromSRV, bool fromTaxi, int? fromLocalId, string systemName, ulong systemAddress, string bodyName, int? bodyId, string station, string stationType, long? marketId, bool onStation, bool onPlanet)
        {
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (DisembarkEvent)events[0];
            Assert.AreEqual(fromMulticrew, @event.frommulticrew);
            Assert.AreEqual(fromShip, @event.fromship);
            Assert.AreEqual(fromSRV, @event.fromsrv);
            Assert.AreEqual(fromTaxi, @event.fromtransport);
            Assert.AreEqual(fromLocalId, @event.fromLocalId);
            Assert.AreEqual(systemName, @event.systemname);
            Assert.AreEqual(systemAddress, @event.systemAddress);
            Assert.AreEqual(bodyName, @event.bodyname);
            Assert.AreEqual(bodyId, @event.bodyId);
            Assert.AreEqual(station, @event.station);
            Assert.AreEqual(stationType, @event.stationModel?.invariantName);
            Assert.AreEqual(marketId, @event.marketId);
            Assert.AreEqual(onPlanet, @event.onplanet);
            Assert.AreEqual(onStation, @event.onstation);
        }

        [DataTestMethod]
        [DataRow("{ \"timestamp\":\"2021-05-29T21:14:30Z\", \"event\":\"BackpackChange\", \"Added\":[ { \"Name\":\"viscoelasticpolymer\", \"Name_Localised\":\"Viscoelastic Polymer\", \"OwnerID\":0, \"Count\":1, \"Type\":\"Component\" } ] }", true, "Viscoelastic Polymer", "Assets", 0, -1, 1)]
        [DataRow("{ \"timestamp\":\"2021-05-29T20:40:44Z\", \"event\":\"BackpackChange\", \"Removed\":[ { \"Name\":\"largecapacitypowerregulator\", \"Name_Localised\":\"Power Regulator\", \"OwnerID\":1642695, \"MissionID\":775573921, \"Count\":1, \"Type\":\"Item\" } ] }", false, "Power Regulator", "Goods", 1642695, 775573921, 1)]
        public void TestBackpackChanged(string line, bool isAdded, string expectedInvariantName, string expectedInvariantCategory, int expectedOwnerId, long expectedMissionId, int expectedAmount)
        {
            // DataRow attributes don't work well with nullable types so we work around that here.
            long? nullableLongConverter(long l)
            {
                if (l == -1) { return null; } else { return l; }
            }
            int? nullableIntConverter(int i)
            {
                if (i == -1) { return null; } else { return i; }
            }

            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (BackpackChangedEvent)events[0];

            if (isAdded)
            {
                var addedItem = @event.added[0];
                Assert.AreEqual(expectedInvariantName, addedItem.microResource?.invariantName);
                Assert.AreEqual(expectedInvariantCategory, addedItem.microResource?.Category?.invariantName);
                Assert.AreEqual(nullableIntConverter(expectedOwnerId), addedItem.ownerId);
                Assert.AreEqual(nullableLongConverter(expectedMissionId), addedItem.missionId);
                Assert.AreEqual(expectedAmount, addedItem.amount);
            }
            else
            {
                var removedItem = @event.removed[0];
                Assert.AreEqual(expectedInvariantName, removedItem.microResource?.invariantName);
                Assert.AreEqual(expectedInvariantCategory, removedItem.microResource?.Category?.invariantName);
                Assert.AreEqual(nullableIntConverter(expectedOwnerId), removedItem.ownerId);
                Assert.AreEqual(nullableLongConverter(expectedMissionId), removedItem.missionId);
                Assert.AreEqual(expectedAmount, removedItem.amount);
            }
        }

        [DataTestMethod]
        [DataRow(@"{ ""timestamp"":""2020-10-05T11:17:50Z"", ""event"":""BookTaxi"", ""Cost"":23200, ""DestinationSystem"":""Opala"", ""DestinationLocation"":""Onizuka's Hold"" }", "Taxi", 23200, "Opala", "Onizuka's Hold")]
        [DataRow(@"{ ""timestamp"":""2021-04-10T10:42:04Z"", ""event"":""BookDropship"", ""Cost"":0, ""DestinationSystem"":""Nervi"", ""DestinationLocation"":""Al-Kashi Terminal"" }", "Dropship", 0, "Nervi", "Al-Kashi Terminal")]
        public void TestBookTransport(string line, string expectedType, int expectedPrice, string expectedStarSystem, string expectedDestination)
        {
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (BookTransportEvent)events[0];

            Assert.AreEqual(expectedType, @event.transporttype);
            Assert.AreEqual(expectedPrice, @event.price);
            Assert.AreEqual(expectedStarSystem, @event.starsystem);
            Assert.AreEqual(expectedDestination, @event.destination);
        }

        [DataTestMethod]
        [DataRow(@"{ ""timestamp"":""2020-10-05T11:17:50Z"", ""event"":""CancelTaxi"", ""Refund"":100 }", "Taxi", 100)]
        [DataRow(@"{ ""timestamp"":""2021-04-10T10:42:04Z"", ""event"":""CancelDropship"", ""Refund"":0 }", "Dropship", 0)]
        public void TestCancelTransport(string line, string expectedType, int expectedRefund)
        {
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (CancelTransportEvent)events[0];

            Assert.AreEqual(expectedType, @event.transporttype);
            Assert.AreEqual(expectedRefund, @event.refund);
        }

        [TestMethod]
        public void TestBuyMicroResourcesEvent()
        {
            var line = @"{ ""timestamp"":""2021-04-30T21:41:34Z"", ""event"":""BuyMicroResources"", ""Name"":""healthpack"", ""Name_Localised"":""Medkit"", ""Category"":""Consumable"", ""Count"":2, ""Price"":2000, ""MarketID"":3221524992 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (MicroResourcesPurchasedEvent)events[0];

            Assert.AreEqual("Medkit", @event.microResource?.invariantName);
            Assert.AreEqual("Consumables", @event.microResource?.Category?.invariantName);
            Assert.AreEqual(2, @event.amount);
            Assert.AreEqual(2000, @event.price);
            Assert.AreEqual(3221524992, @event.marketid);
        }

        [DataTestMethod]
        [DataRow(@"{ ""timestamp"":""2021-04-30T21:37:58Z"", ""event"":""BuySuit"", ""Name"":""UtilitySuit_Class1"", ""Name_Localised"":""Maverick Suit"", ""Price"":150000, ""SuitID"":1698502991022131 }", "Maverick Suit", 1, 1698502991022131, 150000)]
        [DataRow(@"{ ""timestamp"":""2021-04-30T21:38:18Z"", ""event"":""BuySuit"", ""Name"":""ExplorationSuit_Class1"", ""Name_Localised"":""Artemis Suit"", ""Price"":150000, ""SuitID"":1698503011784221 }", "Artemis Suit", 1, 1698503011784221, 150000)]
        [DataRow(@"{ ""timestamp"":""2021-04-30T21:38:39Z"", ""event"":""BuySuit"", ""Name"":""TacticalSuit_Class2"", ""Name_Localised"":""Dominator Suit"", ""Price"":150000, ""SuitID"":1698503033928536 }", "Dominator Suit", 2, 1698503033928536, 150000)]
        public void TestBuySuitEvent(string line, string expectedInvariantName, int expectedGrade, long? expectedSuitId, int? expectedPrice)
        {
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (SuitPurchasedEvent)events[0];
            Assert.AreEqual(expectedInvariantName, @event.suit_invariant);
            Assert.AreEqual(expectedGrade, @event.Suit.grade);
            Assert.AreEqual(expectedSuitId, @event.Suit.suitId);
            Assert.AreEqual(expectedPrice, @event.price);
        }

        [DataTestMethod]
        [DataRow(@"{ ""timestamp"":""2019-06-12T05:12:24Z"", ""event"":""EngineerContribution"", ""Engineer"":""Petra Olmanova"", ""EngineerID"":300130, ""Type"":""Commodity"", ""Commodity"":""progenitorcells"", ""Commodity_Localised"":""Progenitor Cells"", ""Quantity"":168, ""TotalQuantity"":168 }", "Petra Olmanova", "Commodity", "Progenitor Cells", null, "Medicines", 168, 168)]
        [DataRow(@"{ ""timestamp"":""2022-02-02T23:13:17Z"", ""event"":""EngineerContribution"", ""Engineer"":""Chloe Sedesi"", ""EngineerID"":300300, ""Type"":""Materials"", ""Material"":""unknownenergysource"", ""Material_Localised"":""Sensor Fragment"", ""Quantity"":32, ""TotalQuantity"":200 }", "Chloe Sedesi", "Materials", null, "Sensor Fragment", "Manufactured", 32, 200)]
        public void TestEngineerContributedEvent(string line, string engineer, string contributiontype, string commodity, string material, string category, int amount, int total)
        {
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (EngineerContributedEvent)events[0];

            Assert.AreEqual(engineer, @event.Engineer.name);
            Assert.AreEqual(contributiontype, @event.contributiontype);
            var commodityDefinition = CommodityDefinition.FromEDName(@event.commodityAmount?.edname);
            if (commodityDefinition != null)
            {
                Assert.AreEqual(commodity, commodityDefinition.invariantName);
                Assert.AreEqual(category, commodityDefinition?.Category?.invariantName);
            }
            var materialDefinition = Material.FromEDName(@event.materialAmount?.edname);
            if (materialDefinition != null)
            {
                Assert.AreEqual(material, materialDefinition.invariantName);
                Assert.AreEqual(category, materialDefinition?.Category?.invariantName);
            }
            Assert.AreEqual(amount, @event.amount);
            Assert.AreEqual(total, @event.total);
        }

        [DataTestMethod]
        [DataRow(@"{ ""timestamp"":""2022-03-17T18:20:53Z"", ""event"":""FSSBodySignals"", ""BodyName"":""Phroi Blou EW-W d1-1056 2 a"", ""BodyID"":18, ""SystemAddress"":36293555558035, ""Signals"":[ { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":3 } ] }", "FSS", "Phroi Blou EW-W d1-1056 2 a", 18, (ulong)36293555558035, 0, 3, 0, 0, 0, 0)]
        [DataRow(@"{ ""timestamp"":""2019-04-17T13:40:39Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Hermitage 4 b"", ""SystemAddress"":5363877956440, ""BodyID"":13, ""Signals"":[ { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":14 } ] }", "SAA", "Hermitage 4 b", 13, (ulong)5363877956440, 0, 14, 0, 0, 0, 0)]
        [DataRow(@"{ ""timestamp"":""2022-07-01T09:14:32Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Asellus 3a"", ""SystemAddress"":1144348739947, ""BodyID"":10, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":2 }, { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":3 }, { ""Type"":""$SAA_SignalType_Human;"", ""Type_Localised"":""Human"", ""Count"":8 } ], ""Genuses"":[ { ""Genus"":""$Codex_Ent_Bacterial_Genus_Name;"", ""Genus_Localised"":""Bacterium"" }, { ""Genus"":""$Codex_Ent_Stratum_Genus_Name;"", ""Genus_Localised"":""Stratum"" } ] }", "SAA", "Asellus 3a", 10, (ulong)1144348739947, 2, 3, 0, 8, 0, 0)]
        [DataRow("{ \"timestamp\":\"2019-09-24T02:40:34Z\", \"event\":\"SAASignalsFound\", \"BodyName\":\"HIP 41908 AB 1 c a\", \"SystemAddress\":61461226668, \"BodyID\":11, \"Signals\":[ { \"Type\":\"$SAA_SignalType_Biological;\", \"Type_Localised\":\"Biological\", \"Count\":16 }, { \"Type\":\"$SAA_SignalType_Geological;\", \"Type_Localised\":\"Geological\", \"Count\":17 }, { \"Type\":\"$SAA_SignalType_Human;\", \"Type_Localised\":\"Human\", \"Count\":4 } ] }", "SAA", "HIP 41908 AB 1 c a", 11, (ulong)61461226668, 16, 17, 0, 4, 0, 0)]
        public void TestSurfaceSignalsEvent(string line, string expectedDetectionType, string expectedBodyName, int expectedBodyID,
            ulong expectedSystemAddress, int expectedBioSignals, int expectedGeoSignals, int expectedGuardianSignals, int expectedHumanSignals,
            int expectedThargoidSignals, int expectedOtherSignals)
        {
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @event = (SurfaceSignalsEvent)events[0];

            Assert.AreEqual(expectedDetectionType, @event.detectionType);
            Assert.AreEqual(expectedBodyName, @event.bodyname);
            Assert.AreEqual(expectedBodyID, @event.bodyId);
            Assert.AreEqual(expectedSystemAddress, @event.systemAddress);
            Assert.AreEqual(expectedBioSignals, @event.surfacesignals?.FirstOrDefault(s => s.signalSource.edname == "SAA_SignalType_Biological")?.amount ?? 0);
            Assert.AreEqual(expectedGeoSignals, @event.surfacesignals?.FirstOrDefault(s => s.signalSource.edname == "SAA_SignalType_Geological")?.amount ?? 0);
            Assert.AreEqual(expectedGuardianSignals, @event.surfacesignals?.FirstOrDefault(s => s.signalSource.edname == "SAA_SignalType_Guardian")?.amount ?? 0);
            Assert.AreEqual(expectedHumanSignals, @event.surfacesignals?.FirstOrDefault(s => s.signalSource.edname == "SAA_SignalType_Human")?.amount ?? 0);
            Assert.AreEqual(expectedThargoidSignals, @event.surfacesignals?.FirstOrDefault(s => s.signalSource.edname == "SAA_SignalType_Thargoid")?.amount ?? 0);
            Assert.AreEqual(expectedOtherSignals, @event.surfacesignals?.FirstOrDefault(s => s.signalSource.edname == "SAA_SignalType_Other")?.amount ?? 0);
        }

        [TestMethod]
        public void TestBountyAwardedOdyssey()
        {
            string line1 = @"{ ""timestamp"":""2023-01-28T23:36:38Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""Duwali Liberty Party"", ""Reward"":2050 } ], ""Target"":""citizensuitai_scientific"", ""Target_Localised"":""Researcher"", ""TotalReward"":2050, ""VictimFaction"":""Duwali Partnership"" }";
            string line2 = @"{ ""timestamp"":""2023-01-28T23:36:59Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""Defence Party of Duwali"", ""Reward"":14100 } ], ""Target"":""citizensuitai_industrial"", ""Target_Localised"":""Technician"", ""TotalReward"":14100, ""VictimFaction"":""Duwali Partnership"" }";

            var events1 = JournalMonitor.ParseJournalEntry(line1);
            Assert.AreEqual(1, events1.Count);
            var event1 = (BountyAwardedEvent)events1[0];
            Assert.IsNotNull(event1);
            Assert.AreEqual(1, event1.rewards.Count);
            Assert.AreEqual("Duwali Liberty Party", event1.rewards[0].faction);
            Assert.AreEqual(2050, event1.rewards[0].amount);
            Assert.AreEqual("Researcher", event1.target);
            Assert.AreEqual(2050, event1.reward);
            Assert.AreEqual("Duwali Partnership", event1.faction);

            var events2 = JournalMonitor.ParseJournalEntry(line2);
            Assert.AreEqual(1, events2.Count);
            var event2 = (BountyAwardedEvent)events2[0];
            Assert.IsNotNull(event2);
            Assert.AreEqual(1, event2.rewards.Count);
            Assert.AreEqual("Defence Party of Duwali", event2.rewards[0].faction);
            Assert.AreEqual(14100, event2.rewards[0].amount);
            Assert.AreEqual("Technician", event2.target);
            Assert.AreEqual(14100, event2.reward);
            Assert.AreEqual("Duwali Partnership", event2.faction);
        }

        [ TestMethod ]
        public void TestExplorationSequence()
        {
            var eddiPrivateObject = new PrivateObject( EDDI.Instance );
            eddiPrivateObject.SetFieldOrProperty("CurrentStarSystem", new StarSystem() { systemname = "Flyae Drye HT-X b48-1", systemAddress = 2830785848739 } );

            var startJump = @"{ ""timestamp"":""2023-03-25T15:50:33Z"", ""event"":""StartJump"", ""JumpType"":""Hyperspace"", ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""StarClass"":""M"" }";
            var fsdJump = @"{ ""timestamp"":""2023-03-25T15:50:51Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""StarPos"":[-2908.06250,-86.37500,-20.09375], ""SystemAllegiance"":"""", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""Body"":""Flyae Drye HT-X b48-1 A"", ""BodyID"":2, ""BodyType"":""Star"", ""JumpDist"":10.584, ""FuelUsed"":0.052671, ""FuelLevel"":31.947329 }";
            //var scanBaryCentre1 = @"{ ""timestamp"":""2023-03-25T15:50:57Z"", ""event"":""ScanBaryCentre"", ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""BodyID"":1, ""SemiMajorAxis"":2528809428215.026855, ""Eccentricity"":0.199627, ""OrbitalInclination"":-6.513809, ""Periapsis"":3.362396, ""OrbitalPeriod"":29986675381.660461, ""AscendingNode"":-95.656540, ""MeanAnomaly"":247.707499 }";
            var scan1 = @"{ ""timestamp"":""2023-03-25T15:50:57Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 A"", ""BodyID"":2, ""Parents"":[ {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""M"", ""Subclass"":7, ""StellarMass"":0.222656, ""Radius"":277113280.000000, ""AbsoluteMagnitude"":10.548630, ""Age_MY"":6214, ""SurfaceTemperature"":2454.000000, ""Luminosity"":""Va"", ""SemiMajorAxis"":15303524136.543274, ""Eccentricity"":0.082850, ""OrbitalInclination"":-44.888145, ""Periapsis"":100.591116, ""OrbitalPeriod"":22389916.777611, ""AscendingNode"":-40.137907, ""MeanAnomaly"":277.534435, ""RotationPeriod"":145033.644415, ""AxialTilt"":0.000000, ""Rings"":[ { ""Name"":""Flyae Drye HT-X b48-1 A A Belt"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":5.1347e+13, ""InnerRad"":4.1705e+08, ""OuterRad"":1.4626e+09 } ], ""WasDiscovered"":true, ""WasMapped"":false }";
            var scan2 = @"{ ""timestamp"":""2023-03-25T15:50:57Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 A 4"", ""BodyID"":13, ""Parents"":[ {""Star"":2}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":16.503368, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.013128, ""Radius"":1542120.750000, ""SurfaceGravity"":2.200283, ""SurfaceTemperature"":393.717072, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":21.079741 }, { ""Name"":""nickel"", ""Percent"":15.943832 }, { ""Name"":""sulphur"", ""Percent"":14.770421 }, { ""Name"":""carbon"", ""Percent"":12.420394 }, { ""Name"":""chromium"", ""Percent"":9.480260 }, { ""Name"":""manganese"", ""Percent"":8.705718 }, { ""Name"":""phosphorus"", ""Percent"":7.951749 }, { ""Name"":""zinc"", ""Percent"":5.728685 }, { ""Name"":""cadmium"", ""Percent"":1.636938 }, { ""Name"":""tin"", ""Percent"":1.352916 }, { ""Name"":""antimony"", ""Percent"":0.929354 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667369, ""Metal"":0.332631 }, ""SemiMajorAxis"":4949231207.370758, ""Eccentricity"":0.000600, ""OrbitalInclination"":-1.754364, ""Periapsis"":261.345506, ""OrbitalPeriod"":402408.725023, ""AscendingNode"":97.946791, ""MeanAnomaly"":56.306418, ""RotationPeriod"":402409.275143, ""AxialTilt"":0.077470, ""WasDiscovered"":true, ""WasMapped"":false }";
            var scan3 = @"{ ""timestamp"":""2023-03-25T15:50:57Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 A 5 a"", ""BodyID"":15, ""Parents"":[ {""Planet"":14}, {""Star"":2}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":30.658479, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Rocky body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":0.001554, ""Radius"":803703.000000, ""SurfaceGravity"":0.959035, ""SurfaceTemperature"":295.681122, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":19.299084 }, { ""Name"":""sulphur"", ""Percent"":17.066414 }, { ""Name"":""nickel"", ""Percent"":14.597018 }, { ""Name"":""carbon"", ""Percent"":14.351087 }, { ""Name"":""phosphorus"", ""Percent"":9.187811 }, { ""Name"":""chromium"", ""Percent"":8.679440 }, { ""Name"":""manganese"", ""Percent"":7.970325 }, { ""Name"":""germanium"", ""Percent"":5.077768 }, { ""Name"":""niobium"", ""Percent"":1.318990 }, { ""Name"":""molybdenum"", ""Percent"":1.260219 }, { ""Name"":""ruthenium"", ""Percent"":1.191842 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.858679, ""Metal"":0.141321 }, ""SemiMajorAxis"":15658407.211304, ""Eccentricity"":0.000000, ""OrbitalInclination"":-77.732180, ""Periapsis"":35.722467, ""OrbitalPeriod"":94737.572074, ""AscendingNode"":-47.932034, ""MeanAnomaly"":326.596322, ""RotationPeriod"":96524.569681, ""AxialTilt"":0.222691, ""WasDiscovered"":true, ""WasMapped"":false }";
            //var scanBaryCentre2 = @"{ ""timestamp"":""2023-03-25T15:50:57Z"", ""event"":""ScanBaryCentre"", ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""BodyID"":10, ""SemiMajorAxis"":3873022198.677063, ""Eccentricity"":0.005410, ""OrbitalInclination"":-0.119500, ""Periapsis"":58.219409, ""OrbitalPeriod"":278570.902348, ""AscendingNode"":-92.315317, ""MeanAnomaly"":348.647296 }";
            var scan4 = @"{ ""timestamp"":""2023-03-25T15:50:57Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 A 3"", ""BodyID"":12, ""Parents"":[ {""Null"":10}, {""Star"":2}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":12.839536, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major silicate vapour geysers volcanism"", ""MassEM"":0.013571, ""Radius"":1558919.625000, ""SurfaceGravity"":2.225672, ""SurfaceTemperature"":441.780396, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":21.745972 }, { ""Name"":""nickel"", ""Percent"":16.447742 }, { ""Name"":""sulphur"", ""Percent"":15.237244 }, { ""Name"":""carbon"", ""Percent"":12.812943 }, { ""Name"":""chromium"", ""Percent"":9.779886 }, { ""Name"":""manganese"", ""Percent"":8.980865 }, { ""Name"":""phosphorus"", ""Percent"":8.203066 }, { ""Name"":""selenium"", ""Percent"":2.384758 }, { ""Name"":""cadmium"", ""Percent"":1.688673 }, { ""Name"":""molybdenum"", ""Percent"":1.419999 }, { ""Name"":""yttrium"", ""Percent"":1.298861 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667369, ""Metal"":0.332631 }, ""SemiMajorAxis"":3281001.150608, ""Eccentricity"":0.035638, ""OrbitalInclination"":-6.868057, ""Periapsis"":45.588046, ""OrbitalPeriod"":31476.033926, ""AscendingNode"":-108.332705, ""MeanAnomaly"":100.614123, ""RotationPeriod"":44292.496661, ""AxialTilt"":0.434738, ""WasDiscovered"":true, ""WasMapped"":false }";
            var scan5 = @"{ ""timestamp"":""2023-03-25T15:50:57Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 A 2"", ""BodyID"":11, ""Parents"":[ {""Null"":10}, {""Star"":2}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":12.861255, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major silicate vapour geysers volcanism"", ""MassEM"":0.013845, ""Radius"":1569163.000000, ""SurfaceGravity"":2.241171, ""SurfaceTemperature"":441.780396, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":23.381681 }, { ""Name"":""nickel"", ""Percent"":17.684923 }, { ""Name"":""sulphur"", ""Percent"":16.383373 }, { ""Name"":""carbon"", ""Percent"":13.776720 }, { ""Name"":""chromium"", ""Percent"":10.515518 }, { ""Name"":""phosphorus"", ""Percent"":8.820092 }, { ""Name"":""zirconium"", ""Percent"":2.715095 }, { ""Name"":""arsenic"", ""Percent"":2.179962 }, { ""Name"":""niobium"", ""Percent"":1.598013 }, { ""Name"":""tin"", ""Percent"":1.500657 }, { ""Name"":""ruthenium"", ""Percent"":1.443968 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667369, ""Metal"":0.332631 }, ""SemiMajorAxis"":3215910.315514, ""Eccentricity"":0.035638, ""OrbitalInclination"":-6.868057, ""Periapsis"":225.588041, ""OrbitalPeriod"":31476.033926, ""AscendingNode"":-108.332705, ""MeanAnomaly"":100.614123, ""RotationPeriod"":44738.501964, ""AxialTilt"":0.202042, ""WasDiscovered"":true, ""WasMapped"":false }";
            var scan6 = @"{ ""timestamp"":""2023-03-25T15:50:57Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 A 1"", ""BodyID"":9, ""Parents"":[ {""Star"":2}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":5.030041, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major silicate vapour geysers volcanism"", ""MassEM"":0.025824, ""Radius"":1922685.625000, ""SurfaceGravity"":2.784270, ""SurfaceTemperature"":692.795471, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":22.948023 }, { ""Name"":""nickel"", ""Percent"":17.356924 }, { ""Name"":""sulphur"", ""Percent"":16.079517 }, { ""Name"":""carbon"", ""Percent"":13.521210 }, { ""Name"":""manganese"", ""Percent"":9.477300 }, { ""Name"":""phosphorus"", ""Percent"":8.656510 }, { ""Name"":""zinc"", ""Percent"":6.236414 }, { ""Name"":""selenium"", ""Percent"":2.516581 }, { ""Name"":""niobium"", ""Percent"":1.568375 }, { ""Name"":""mercury"", ""Percent"":1.002102 }, { ""Name"":""polonium"", ""Percent"":0.637042 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667369, ""Metal"":0.332631 }, ""SemiMajorAxis"":1508015811.443329, ""Eccentricity"":0.000032, ""OrbitalInclination"":0.003300, ""Periapsis"":343.196157, ""OrbitalPeriod"":67681.450844, ""AscendingNode"":-7.225531, ""MeanAnomaly"":354.763075, ""RotationPeriod"":67681.535529, ""AxialTilt"":-0.226950, ""WasDiscovered"":true, ""WasMapped"":false }";
            var scan7 = @"{ ""timestamp"":""2023-03-25T15:50:58Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 A 5"", ""BodyID"":14, ""Parents"":[ {""Star"":2}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":30.644567, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.040814, ""Radius"":2225768.000000, ""SurfaceGravity"":3.283657, ""SurfaceTemperature"":295.681122, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":21.258522 }, { ""Name"":""nickel"", ""Percent"":16.079054 }, { ""Name"":""sulphur"", ""Percent"":14.785975 }, { ""Name"":""carbon"", ""Percent"":12.433473 }, { ""Name"":""chromium"", ""Percent"":9.560664 }, { ""Name"":""manganese"", ""Percent"":8.779552 }, { ""Name"":""phosphorus"", ""Percent"":7.960123 }, { ""Name"":""zinc"", ""Percent"":5.777270 }, { ""Name"":""yttrium"", ""Percent"":1.269746 }, { ""Name"":""tungsten"", ""Percent"":1.167306 }, { ""Name"":""mercury"", ""Percent"":0.928325 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.660083, ""Metal"":0.339917 }, ""SemiMajorAxis"":9187461137.771606, ""Eccentricity"":0.000052, ""OrbitalInclination"":0.005540, ""Periapsis"":61.788661, ""OrbitalPeriod"":1017780.363560, ""AscendingNode"":-39.785415, ""MeanAnomaly"":340.644973, ""RotationPeriod"":1017781.651987, ""AxialTilt"":-0.185765, ""WasDiscovered"":true, ""WasMapped"":false }";
            var discoveryScan = @"{ ""timestamp"":""2023-03-25T15:51:01Z"", ""event"":""FSSDiscoveryScan"", ""Progress"":0.294365, ""BodyCount"":45, ""NonBodyCount"":6, ""SystemName"":""TestSystem"", ""SystemAddress"":9999999999999 }";
            var scan8 = @"{ ""timestamp"":""2023-03-25T15:51:01Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 C"", ""BodyID"":4, ""Parents"":[ {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":38286.768620, ""StarType"":""L"", ""Subclass"":8, ""StellarMass"":0.089844, ""Radius"":156512944.000000, ""AbsoluteMagnitude"":14.249313, ""Age_MY"":6214, ""SurfaceTemperature"":1393.000000, ""Luminosity"":""V"", ""SemiMajorAxis"":7838416099548.339844, ""Eccentricity"":0.199627, ""OrbitalInclination"":-6.513809, ""Periapsis"":183.362388, ""OrbitalPeriod"":29986675381.660461, ""AscendingNode"":-95.656540, ""MeanAnomaly"":247.707499, ""RotationPeriod"":86544.611379, ""AxialTilt"":0.000000, ""WasDiscovered"":true, ""WasMapped"":false }";
            var scan9 = @"{ ""timestamp"":""2023-03-25T15:51:01Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Flyae Drye HT-X b48-1 B"", ""BodyID"":3, ""Parents"":[ {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":257.836606, ""StarType"":""T"", ""Subclass"":7, ""StellarMass"":0.054688, ""Radius"":118732504.000000, ""AbsoluteMagnitude"":16.854416, ""Age_MY"":6214, ""SurfaceTemperature"":878.000000, ""Luminosity"":""V"", ""SemiMajorAxis"":62307288050.651550, ""Eccentricity"":0.082850, ""OrbitalInclination"":-44.888145, ""Periapsis"":280.591111, ""OrbitalPeriod"":22389916.777611, ""AscendingNode"":-40.137907, ""MeanAnomaly"":277.534499, ""RotationPeriod"":50917.861242, ""AxialTilt"":0.000000, ""Rings"":[ { ""Name"":""Flyae Drye HT-X b48-1 B A Belt"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":2.1153e+13, ""InnerRad"":1.9308e+08, ""OuterRad"":9.1594e+08 } ], ""WasDiscovered"":true, ""WasMapped"":false }";
            var scan10 = @"{ ""timestamp"":""2023-03-25T15:58:21Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 B 1"", ""BodyID"":21, ""Parents"":[ {""Star"":3}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":255.491658, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":0.002951, ""Radius"":943923.562500, ""SurfaceGravity"":1.320152, ""SurfaceTemperature"":274.034424, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":23.074236 }, { ""Name"":""nickel"", ""Percent"":17.452387 }, { ""Name"":""sulphur"", ""Percent"":16.135445 }, { ""Name"":""carbon"", ""Percent"":13.568237 }, { ""Name"":""manganese"", ""Percent"":9.529425 }, { ""Name"":""phosphorus"", ""Percent"":8.686618 }, { ""Name"":""germanium"", ""Percent"":4.800777 }, { ""Name"":""zirconium"", ""Percent"":2.679394 }, { ""Name"":""niobium"", ""Percent"":1.577001 }, { ""Name"":""tin"", ""Percent"":1.481238 }, { ""Name"":""antimony"", ""Percent"":1.015241 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.665391, ""Metal"":0.334609 }, ""SemiMajorAxis"":1168515324.592590, ""Eccentricity"":0.000374, ""OrbitalInclination"":-0.039123, ""Periapsis"":286.683680, ""OrbitalPeriod"":93150.691986, ""AscendingNode"":130.527439, ""MeanAnomaly"":197.999511, ""RotationPeriod"":93150.753032, ""AxialTilt"":0.963490, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan11 = @"{ ""timestamp"":""2023-03-25T15:58:27Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 B 3"", ""BodyID"":23, ""Parents"":[ {""Star"":3}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":263.131870, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.002351, ""Radius"":874786.562500, ""SurfaceGravity"":1.224273, ""SurfaceTemperature"":228.195648, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":23.032526 }, { ""Name"":""nickel"", ""Percent"":17.420837 }, { ""Name"":""sulphur"", ""Percent"":16.047169 }, { ""Name"":""carbon"", ""Percent"":13.494007 }, { ""Name"":""manganese"", ""Percent"":9.512197 }, { ""Name"":""phosphorus"", ""Percent"":8.639095 }, { ""Name"":""vanadium"", ""Percent"":5.655986 }, { ""Name"":""arsenic"", ""Percent"":2.135227 }, { ""Name"":""niobium"", ""Percent"":1.574150 }, { ""Name"":""tin"", ""Percent"":1.479125 }, { ""Name"":""antimony"", ""Percent"":1.009687 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.661767, ""Metal"":0.338233 }, ""SemiMajorAxis"":2123434782.028198, ""Eccentricity"":0.000254, ""OrbitalInclination"":1.666938, ""Periapsis"":276.872192, ""OrbitalPeriod"":228188.019991, ""AscendingNode"":-114.652730, ""MeanAnomaly"":28.175569, ""RotationPeriod"":228188.180139, ""AxialTilt"":-0.978731, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan12 = @"{ ""timestamp"":""2023-03-25T15:58:30Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 B 4"", ""BodyID"":24, ""Parents"":[ {""Star"":3}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":251.865911, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.001337, ""Radius"":727042.375000, ""SurfaceGravity"":1.007862, ""SurfaceTemperature"":206.282516, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":23.597265 }, { ""Name"":""nickel"", ""Percent"":17.847982 }, { ""Name"":""sulphur"", ""Percent"":16.534435 }, { ""Name"":""carbon"", ""Percent"":13.903748 }, { ""Name"":""phosphorus"", ""Percent"":8.901418 }, { ""Name"":""zinc"", ""Percent"":6.412853 }, { ""Name"":""vanadium"", ""Percent"":5.794667 }, { ""Name"":""zirconium"", ""Percent"":2.740129 }, { ""Name"":""tin"", ""Percent"":1.514493 }, { ""Name"":""ruthenium"", ""Percent"":1.457282 }, { ""Name"":""tungsten"", ""Percent"":1.295727 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667369, ""Metal"":0.332631 }, ""SemiMajorAxis"":3055635631.084442, ""Eccentricity"":0.000126, ""OrbitalInclination"":0.030937, ""Periapsis"":349.882772, ""OrbitalPeriod"":393900.746107, ""AscendingNode"":-95.196861, ""MeanAnomaly"":35.953685, ""RotationPeriod"":393901.034322, ""AxialTilt"":0.152645, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan13 = @"{ ""timestamp"":""2023-03-25T15:58:32Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 B 5"", ""BodyID"":25, ""Parents"":[ {""Star"":3}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":256.796137, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.001388, ""Radius"":736116.437500, ""SurfaceGravity"":1.020741, ""SurfaceTemperature"":196.627899, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":24.044863 }, { ""Name"":""nickel"", ""Percent"":18.186525 }, { ""Name"":""sulphur"", ""Percent"":16.848064 }, { ""Name"":""carbon"", ""Percent"":14.167476 }, { ""Name"":""phosphorus"", ""Percent"":9.070262 }, { ""Name"":""vanadium"", ""Percent"":5.904582 }, { ""Name"":""germanium"", ""Percent"":5.012803 }, { ""Name"":""zirconium"", ""Percent"":2.792104 }, { ""Name"":""molybdenum"", ""Percent"":1.570116 }, { ""Name"":""tin"", ""Percent"":1.543220 }, { ""Name"":""technetium"", ""Percent"":0.859988 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667369, ""Metal"":0.332631 }, ""SemiMajorAxis"":3673243939.876556, ""Eccentricity"":0.000220, ""OrbitalInclination"":-0.833600, ""Periapsis"":263.985845, ""OrbitalPeriod"":519169.449806, ""AscendingNode"":27.586123, ""MeanAnomaly"":103.377054, ""RotationPeriod"":519169.787540, ""AxialTilt"":0.490797, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan14 = @"{ ""timestamp"":""2023-03-25T15:58:45Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 B 2"", ""BodyID"":22, ""Parents"":[ {""Star"":3}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":252.608792, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major rocky magma volcanism"", ""MassEM"":0.001781, ""Radius"":799447.187500, ""SurfaceGravity"":1.110857, ""SurfaceTemperature"":245.905228, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":22.126280 }, { ""Name"":""nickel"", ""Percent"":16.735392 }, { ""Name"":""sulphur"", ""Percent"":15.503719 }, { ""Name"":""carbon"", ""Percent"":13.037022 }, { ""Name"":""chromium"", ""Percent"":9.950923 }, { ""Name"":""phosphorus"", ""Percent"":8.346525 }, { ""Name"":""vanadium"", ""Percent"":5.433444 }, { ""Name"":""germanium"", ""Percent"":4.612820 }, { ""Name"":""niobium"", ""Percent"":1.512213 }, { ""Name"":""tin"", ""Percent"":1.420084 }, { ""Name"":""yttrium"", ""Percent"":1.321576 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667369, ""Metal"":0.332631 }, ""SemiMajorAxis"":1649902641.773224, ""Eccentricity"":0.002584, ""OrbitalInclination"":0.462986, ""Periapsis"":342.517471, ""OrbitalPeriod"":156286.674738, ""AscendingNode"":117.035124, ""MeanAnomaly"":169.001133, ""RotationPeriod"":156286.776511, ""AxialTilt"":-0.337310, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan15 = @"{ ""timestamp"":""2023-03-25T15:58:48Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 B 6"", ""BodyID"":26, ""Parents"":[ {""Star"":3}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":275.390533, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.006234, ""Radius"":1208048.125000, ""SurfaceGravity"":1.702470, ""SurfaceTemperature"":178.231735, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":22.951380 }, { ""Name"":""nickel"", ""Percent"":17.359461 }, { ""Name"":""sulphur"", ""Percent"":16.081865 }, { ""Name"":""carbon"", ""Percent"":13.523182 }, { ""Name"":""manganese"", ""Percent"":9.478685 }, { ""Name"":""phosphorus"", ""Percent"":8.657774 }, { ""Name"":""vanadium"", ""Percent"":5.636060 }, { ""Name"":""arsenic"", ""Percent"":2.139843 }, { ""Name"":""cadmium"", ""Percent"":1.782279 }, { ""Name"":""niobium"", ""Percent"":1.568604 }, { ""Name"":""technetium"", ""Percent"":0.820878 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667369, ""Metal"":0.332631 }, ""SemiMajorAxis"":5512626230.716705, ""Eccentricity"":0.022683, ""OrbitalInclination"":3.498910, ""Periapsis"":82.179644, ""OrbitalPeriod"":954491.913319, ""AscendingNode"":-69.292163, ""MeanAnomaly"":298.896966, ""RotationPeriod"":954492.631004, ""AxialTilt"":0.398619, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan16 = @"{ ""timestamp"":""2023-03-25T15:58:54Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 A 6"", ""BodyID"":16, ""Parents"":[ {""Star"":2}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":43.385312, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot water atmosphere"", ""AtmosphereType"":""Water"", ""AtmosphereComposition"":[ { ""Name"":""Water"", ""Percent"":100.000000 } ], ""Volcanism"":"""", ""MassEM"":0.010269, ""Radius"":1423025.500000, ""SurfaceGravity"":2.021284, ""SurfaceTemperature"":549.818481, ""SurfacePressure"":108262.445313, ""Landable"":false, ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.667368, ""Metal"":0.332632 }, ""SemiMajorAxis"":13006126284.599304, ""Eccentricity"":0.000042, ""OrbitalInclination"":-7.490775, ""Periapsis"":120.759264, ""OrbitalPeriod"":1714283.943176, ""AscendingNode"":-78.248013, ""MeanAnomaly"":148.332517, ""RotationPeriod"":1714286.029000, ""AxialTilt"":0.301547, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan17 = @"{ ""timestamp"":""2023-03-25T15:59:06Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 C 2"", ""BodyID"":58, ""Parents"":[ {""Star"":4}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":38786.307625, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""helium atmosphere"", ""AtmosphereType"":""Helium"", ""AtmosphereComposition"":[ { ""Name"":""Helium"", ""Percent"":89.334976 }, { ""Name"":""Hydrogen"", ""Percent"":8.427828 }, { ""Name"":""Neon"", ""Percent"":2.237205 } ], ""Volcanism"":""major water geysers volcanism"", ""MassEM"":4.315027, ""Radius"":11859586.000000, ""SurfaceGravity"":12.227956, ""SurfaceTemperature"":33.022499, ""SurfacePressure"":81145.750000, ""Landable"":false, ""Composition"":{ ""Ice"":0.681745, ""Rock"":0.212394, ""Metal"":0.105862 }, ""SemiMajorAxis"":153162139654.159546, ""Eccentricity"":0.000036, ""OrbitalInclination"":0.078593, ""Periapsis"":323.464147, ""OrbitalPeriod"":109027290.344238, ""AscendingNode"":172.694288, ""MeanAnomaly"":17.033126, ""RotationPeriod"":164812.278766, ""AxialTilt"":0.517243, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan18 = @"{ ""timestamp"":""2023-03-25T15:59:09Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 C 1"", ""BodyID"":57, ""Parents"":[ {""Star"":4}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":37928.035581, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""helium atmosphere"", ""AtmosphereType"":""Helium"", ""AtmosphereComposition"":[ { ""Name"":""Helium"", ""Percent"":89.334976 }, { ""Name"":""Hydrogen"", ""Percent"":8.427828 }, { ""Name"":""Neon"", ""Percent"":2.237205 } ], ""Volcanism"":""major water geysers volcanism"", ""MassEM"":2.989866, ""Radius"":10705851.000000, ""SurfaceGravity"":10.397256, ""SurfaceTemperature"":37.751526, ""SurfacePressure"":58667.164063, ""Landable"":false, ""Composition"":{ ""Ice"":0.681745, ""Rock"":0.212394, ""Metal"":0.105862 }, ""SemiMajorAxis"":108225107192.993164, ""Eccentricity"":0.001579, ""OrbitalInclination"":0.290149, ""Periapsis"":116.895047, ""OrbitalPeriod"":64758996.367455, ""AscendingNode"":-15.598923, ""MeanAnomaly"":142.889749, ""RotationPeriod"":138225.146129, ""AxialTilt"":0.425118, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan19 = @"{ ""timestamp"":""2023-03-25T15:59:17Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 C 5 a"", ""BodyID"":62, ""Parents"":[ {""Planet"":61}, {""Star"":4}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":39966.956170, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.189293, ""Radius"":4937730.500000, ""SurfaceGravity"":3.094493, ""SurfaceTemperature"":20.605873, ""SurfacePressure"":1.494034, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":28.219172 }, { ""Name"":""carbon"", ""Percent"":23.729401 }, { ""Name"":""phosphorus"", ""Percent"":15.191969 }, { ""Name"":""iron"", ""Percent"":12.534502 }, { ""Name"":""nickel"", ""Percent"":9.480572 }, { ""Name"":""chromium"", ""Percent"":5.637182 }, { ""Name"":""arsenic"", ""Percent"":1.473362 }, { ""Name"":""zirconium"", ""Percent"":1.455514 }, { ""Name"":""cadmium"", ""Percent"":0.973361 }, { ""Name"":""niobium"", ""Percent"":0.856666 }, { ""Name"":""technetium"", ""Percent"":0.448309 } ], ""Composition"":{ ""Ice"":0.876036, ""Rock"":0.106363, ""Metal"":0.017601 }, ""SemiMajorAxis"":5992727398.872375, ""Eccentricity"":0.000000, ""OrbitalInclination"":69.828514, ""Periapsis"":65.566848, ""OrbitalPeriod"":75263839.960098, ""AscendingNode"":-149.683777, ""MeanAnomaly"":255.738473, ""RotationPeriod"":120734.019056, ""AxialTilt"":0.147713, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan20 = @"{ ""timestamp"":""2023-03-25T15:59:21Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 C 5"", ""BodyID"":61, ""Parents"":[ {""Star"":4}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":39956.294683, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""helium atmosphere"", ""AtmosphereType"":""Helium"", ""AtmosphereComposition"":[ { ""Name"":""Helium"", ""Percent"":91.379318 }, { ""Name"":""Hydrogen"", ""Percent"":8.620689 } ], ""Volcanism"":""major water geysers volcanism"", ""MassEM"":3.573774, ""Radius"":11203883.000000, ""SurfaceGravity"":11.347480, ""SurfaceTemperature"":21.060436, ""SurfacePressure"":66941.835938, ""Landable"":false, ""Composition"":{ ""Ice"":0.668019, ""Rock"":0.219892, ""Metal"":0.112089 }, ""SemiMajorAxis"":528741586208.343506, ""Eccentricity"":0.001145, ""OrbitalInclination"":-0.998447, ""Periapsis"":299.574426, ""OrbitalPeriod"":699315714.836121, ""AscendingNode"":-85.522130, ""MeanAnomaly"":101.890440, ""RotationPeriod"":53247.989072, ""AxialTilt"":-3.093438, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan21 = @"{ ""timestamp"":""2023-03-25T15:59:24Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 C 3"", ""BodyID"":59, ""Parents"":[ {""Star"":4}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":38011.162949, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""neon rich atmosphere"", ""AtmosphereType"":""NeonRich"", ""AtmosphereComposition"":[ { ""Name"":""Helium"", ""Percent"":97.541206 }, { ""Name"":""Neon"", ""Percent"":2.442712 } ], ""Volcanism"":""water geysers volcanism"", ""MassEM"":1.400663, ""Radius"":8594744.000000, ""SurfaceGravity"":7.557484, ""SurfaceTemperature"":29.003063, ""SurfacePressure"":28388.515625, ""Landable"":false, ""Composition"":{ ""Ice"":0.681740, ""Rock"":0.212396, ""Metal"":0.105864 }, ""SemiMajorAxis"":207860791683.197021, ""Eccentricity"":0.002018, ""OrbitalInclination"":-0.905436, ""Periapsis"":85.102982, ""OrbitalPeriod"":172372096.776962, ""AscendingNode"":117.451854, ""MeanAnomaly"":185.129836, ""RotationPeriod"":90847.038135, ""AxialTilt"":0.245058, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan22 = @"{ ""timestamp"":""2023-03-25T15:59:27Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 C 4"", ""BodyID"":60, ""Parents"":[ {""Star"":4}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":37556.398392, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""helium atmosphere"", ""AtmosphereType"":""Helium"", ""AtmosphereComposition"":[ { ""Name"":""Helium"", ""Percent"":89.334976 }, { ""Name"":""Hydrogen"", ""Percent"":8.427828 }, { ""Name"":""Neon"", ""Percent"":2.237204 } ], ""Volcanism"":""major water geysers volcanism"", ""MassEM"":1.802247, ""Radius"":9255765.000000, ""SurfaceGravity"":8.384921, ""SurfaceTemperature"":25.669958, ""SurfacePressure"":38155.128906, ""Landable"":false, ""Composition"":{ ""Ice"":0.681740, ""Rock"":0.212396, ""Metal"":0.105864 }, ""SemiMajorAxis"":293899971246.719360, ""Eccentricity"":0.000070, ""OrbitalInclination"":-0.011323, ""Periapsis"":141.558812, ""OrbitalPeriod"":289805996.417999, ""AscendingNode"":101.504630, ""MeanAnomaly"":250.423955, ""RotationPeriod"":64518.344420, ""AxialTilt"":0.156645, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan23 = @"{ ""timestamp"":""2023-03-25T15:59:32Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 C 6"", ""BodyID"":63, ""Parents"":[ {""Star"":4}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":37374.736725, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""helium atmosphere"", ""AtmosphereType"":""Helium"", ""AtmosphereComposition"":[ { ""Name"":""Helium"", ""Percent"":91.379318 }, { ""Name"":""Hydrogen"", ""Percent"":8.620689 } ], ""Volcanism"":""major water geysers volcanism"", ""MassEM"":2.077534, ""Radius"":9646336.000000, ""SurfaceGravity"":8.898827, ""SurfaceTemperature"":20.353291, ""SurfacePressure"":42014.078125, ""Landable"":false, ""Composition"":{ ""Ice"":0.681742, ""Rock"":0.212397, ""Metal"":0.105862 }, ""SemiMajorAxis"":932366126775.741577, ""Eccentricity"":0.009263, ""OrbitalInclination"":-0.088886, ""Periapsis"":325.109307, ""OrbitalPeriod"":1637524604.797363, ""AscendingNode"":55.868437, ""MeanAnomaly"":359.129816, ""RotationPeriod"":76858.446655, ""AxialTilt"":0.366048, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan24 = @"{ ""timestamp"":""2023-03-25T15:59:50Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 2"", ""BodyID"":37, ""Parents"":[ {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1159.895033, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Sudarsky class I gas giant"", ""Atmosphere"":"""", ""AtmosphereComposition"":[ { ""Name"":""Hydrogen"", ""Percent"":73.092964 }, { ""Name"":""Helium"", ""Percent"":26.907047 } ], ""Volcanism"":"""", ""MassEM"":19.758814, ""Radius"":38975880.000000, ""SurfaceGravity"":5.184161, ""SurfaceTemperature"":48.351440, ""SurfacePressure"":0.000000, ""Landable"":false, ""SemiMajorAxis"":337680923938.751221, ""Eccentricity"":0.000100, ""OrbitalInclination"":-0.480994, ""Periapsis"":239.198925, ""OrbitalPeriod"":202727085.351944, ""AscendingNode"":-5.552124, ""MeanAnomaly"":11.248881, ""RotationPeriod"":73717.084702, ""AxialTilt"":-0.156144, ""Rings"":[ { ""Name"":""Flyae Drye HT-X b48-1 AB 2 A Ring"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":1.6204e+10, ""InnerRad"":6.431e+07, ""OuterRad"":6.8807e+07 }, { ""Name"":""Flyae Drye HT-X b48-1 AB 2 B Ring"", ""RingClass"":""eRingClass_Icy"", ""MassMT"":1.6618e+11, ""InnerRad"":6.8907e+07, ""OuterRad"":1.0324e+08 } ], ""ReserveLevel"":""PristineResources"", ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan25 = @"{ ""timestamp"":""2023-03-25T15:59:58Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 2 a"", ""BodyID"":40, ""Parents"":[ {""Planet"":37}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1160.977100, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.000869, ""Radius"":841046.937500, ""SurfaceGravity"":0.489594, ""SurfaceTemperature"":42.820366, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":27.062803 }, { ""Name"":""carbon"", ""Percent"":22.757015 }, { ""Name"":""phosphorus"", ""Percent"":14.569431 }, { ""Name"":""iron"", ""Percent"":12.458669 }, { ""Name"":""nickel"", ""Percent"":9.423214 }, { ""Name"":""manganese"", ""Percent"":5.145303 }, { ""Name"":""zinc"", ""Percent"":3.385800 }, { ""Name"":""vanadium"", ""Percent"":3.059415 }, { ""Name"":""cadmium"", ""Percent"":0.967472 }, { ""Name"":""niobium"", ""Percent"":0.851484 }, { ""Name"":""polonium"", ""Percent"":0.319381 } ], ""Composition"":{ ""Ice"":0.818180, ""Rock"":0.165118, ""Metal"":0.016702 }, ""SemiMajorAxis"":339384138.584137, ""Eccentricity"":0.000033, ""OrbitalInclination"":0.019341, ""Periapsis"":245.962984, ""OrbitalPeriod"":442628.490925, ""AscendingNode"":-172.303068, ""MeanAnomaly"":65.832188, ""RotationPeriod"":-425235.238497, ""AxialTilt"":2.676049, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan26 = @"{ ""timestamp"":""2023-03-25T16:00:02Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 2 b"", ""BodyID"":41, ""Parents"":[ {""Planet"":37}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1157.535646, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.000532, ""Radius"":715629.250000, ""SurfaceGravity"":0.413802, ""SurfaceTemperature"":42.736786, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.829014 }, { ""Name"":""carbon"", ""Percent"":22.560421 }, { ""Name"":""phosphorus"", ""Percent"":14.443569 }, { ""Name"":""iron"", ""Percent"":12.215899 }, { ""Name"":""nickel"", ""Percent"":9.239594 }, { ""Name"":""manganese"", ""Percent"":5.045041 }, { ""Name"":""selenium"", ""Percent"":4.198967 }, { ""Name"":""vanadium"", ""Percent"":2.999799 }, { ""Name"":""tellurium"", ""Percent"":0.909702 }, { ""Name"":""niobium"", ""Percent"":0.834892 }, { ""Name"":""tin"", ""Percent"":0.723104 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":798842489.719391, ""Eccentricity"":0.000416, ""OrbitalInclination"":-0.008998, ""Periapsis"":282.573371, ""OrbitalPeriod"":1598430.693150, ""AscendingNode"":132.805467, ""MeanAnomaly"":216.973390, ""RotationPeriod"":1572613.357510, ""AxialTilt"":0.210436, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan27 = @"{ ""timestamp"":""2023-03-25T16:00:05Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 2 c"", ""BodyID"":42, ""Parents"":[ {""Planet"":37}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1158.367382, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.000413, ""Radius"":658257.937500, ""SurfaceGravity"":0.380176, ""SurfaceTemperature"":42.717346, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.916862 }, { ""Name"":""carbon"", ""Percent"":22.634293 }, { ""Name"":""phosphorus"", ""Percent"":14.490864 }, { ""Name"":""iron"", ""Percent"":12.255898 }, { ""Name"":""nickel"", ""Percent"":9.269848 }, { ""Name"":""chromium"", ""Percent"":5.511885 }, { ""Name"":""germanium"", ""Percent"":3.523835 }, { ""Name"":""zinc"", ""Percent"":3.330694 }, { ""Name"":""cadmium"", ""Percent"":0.951726 }, { ""Name"":""molybdenum"", ""Percent"":0.800303 }, { ""Name"":""polonium"", ""Percent"":0.313789 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":1045502424.240112, ""Eccentricity"":0.000090, ""OrbitalInclination"":0.004978, ""Periapsis"":264.042566, ""OrbitalPeriod"":2393258.631229, ""AscendingNode"":165.333704, ""MeanAnomaly"":194.669346, ""RotationPeriod"":2364317.578774, ""AxialTilt"":0.266873, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan28 = @"{ ""timestamp"":""2023-03-25T16:00:13Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 2 e"", ""BodyID"":45, ""Parents"":[ {""Null"":43}, {""Planet"":37}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1159.850480, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.000383, ""Radius"":641962.250000, ""SurfaceGravity"":0.370641, ""SurfaceTemperature"":42.696423, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":27.187386 }, { ""Name"":""carbon"", ""Percent"":22.861776 }, { ""Name"":""phosphorus"", ""Percent"":14.636501 }, { ""Name"":""iron"", ""Percent"":12.379075 }, { ""Name"":""nickel"", ""Percent"":9.363013 }, { ""Name"":""selenium"", ""Percent"":4.255055 }, { ""Name"":""germanium"", ""Percent"":3.559251 }, { ""Name"":""zinc"", ""Percent"":3.364169 }, { ""Name"":""niobium"", ""Percent"":0.846044 }, { ""Name"":""molybdenum"", ""Percent"":0.808346 }, { ""Name"":""yttrium"", ""Percent"":0.739387 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":5663158.774376, ""Eccentricity"":0.096045, ""OrbitalInclination"":-2.912683, ""Periapsis"":351.534776, ""OrbitalPeriod"":301796.704531, ""AscendingNode"":-49.463202, ""MeanAnomaly"":328.786095, ""RotationPeriod"":391652.113855, ""AxialTilt"":0.275734, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan29 = @"{ ""timestamp"":""2023-03-25T16:00:16Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 2 d"", ""BodyID"":44, ""Parents"":[ {""Null"":43}, {""Planet"":37}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1159.873248, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.000560, ""Radius"":728128.625000, ""SurfaceGravity"":0.421139, ""SurfaceTemperature"":42.696423, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":27.563694 }, { ""Name"":""carbon"", ""Percent"":23.178213 }, { ""Name"":""phosphorus"", ""Percent"":14.839090 }, { ""Name"":""iron"", ""Percent"":12.550417 }, { ""Name"":""nickel"", ""Percent"":9.492610 }, { ""Name"":""manganese"", ""Percent"":5.183194 }, { ""Name"":""vanadium"", ""Percent"":3.081946 }, { ""Name"":""arsenic"", ""Percent"":1.613777 }, { ""Name"":""tellurium"", ""Percent"":0.934613 }, { ""Name"":""molybdenum"", ""Percent"":0.819535 }, { ""Name"":""tin"", ""Percent"":0.742905 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":3874258.518219, ""Eccentricity"":0.096045, ""OrbitalInclination"":-2.912683, ""Periapsis"":171.534781, ""OrbitalPeriod"":301796.704531, ""AscendingNode"":-49.463202, ""MeanAnomaly"":328.790148, ""RotationPeriod"":355137.718630, ""AxialTilt"":0.218068, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan30 = @"{ ""timestamp"":""2023-03-25T16:00:28Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 1"", ""BodyID"":27, ""Parents"":[ {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":898.583282, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Sudarsky class I gas giant"", ""Atmosphere"":"""", ""AtmosphereComposition"":[ { ""Name"":""Hydrogen"", ""Percent"":73.092964 }, { ""Name"":""Helium"", ""Percent"":26.907045 } ], ""Volcanism"":"""", ""MassEM"":305.063843, ""Radius"":72661488.000000, ""SurfaceGravity"":23.029875, ""SurfaceTemperature"":91.071915, ""SurfacePressure"":0.000000, ""Landable"":false, ""SemiMajorAxis"":261708962917.327881, ""Eccentricity"":0.000035, ""OrbitalInclination"":0.078356, ""Periapsis"":103.277255, ""OrbitalPeriod"":138318383.693695, ""AscendingNode"":31.280706, ""MeanAnomaly"":283.773688, ""RotationPeriod"":150556.280487, ""AxialTilt"":0.447474, ""Rings"":[ { ""Name"":""Flyae Drye HT-X b48-1 AB 1 A Ring"", ""RingClass"":""eRingClass_Icy"", ""MassMT"":9.6895e+11, ""InnerRad"":1.2312e+08, ""OuterRad"":2.3435e+08 } ], ""ReserveLevel"":""PristineResources"", ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan31 = @"{ ""timestamp"":""2023-03-25T16:00:33Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 1 a"", ""BodyID"":29, ""Parents"":[ {""Planet"":27}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":897.330417, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major water geysers volcanism"", ""MassEM"":0.000301, ""Radius"":592667.187500, ""SurfaceGravity"":0.341838, ""SurfaceTemperature"":56.898434, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.878813 }, { ""Name"":""carbon"", ""Percent"":22.602299 }, { ""Name"":""phosphorus"", ""Percent"":14.470381 }, { ""Name"":""iron"", ""Percent"":12.238576 }, { ""Name"":""nickel"", ""Percent"":9.256746 }, { ""Name"":""chromium"", ""Percent"":5.504095 }, { ""Name"":""selenium"", ""Percent"":4.206762 }, { ""Name"":""vanadium"", ""Percent"":3.005368 }, { ""Name"":""molybdenum"", ""Percent"":0.799172 }, { ""Name"":""tin"", ""Percent"":0.724446 }, { ""Name"":""polonium"", ""Percent"":0.313345 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":415052515.268326, ""Eccentricity"":0.000006, ""OrbitalInclination"":0.002299, ""Periapsis"":199.586887, ""OrbitalPeriod"":152361.398935, ""AscendingNode"":-84.596855, ""MeanAnomaly"":215.947780, ""RotationPeriod"":152363.103149, ""AxialTilt"":-0.048066, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan32 = @"{ ""timestamp"":""2023-03-25T16:00:36Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 1 d"", ""BodyID"":32, ""Parents"":[ {""Planet"":27}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":895.323424, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.000413, ""Radius"":658124.000000, ""SurfaceGravity"":0.380098, ""SurfaceTemperature"":53.051006, ""SurfacePressure"":72.577599, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.508778 }, { ""Name"":""carbon"", ""Percent"":22.291136 }, { ""Name"":""phosphorus"", ""Percent"":14.271169 }, { ""Name"":""iron"", ""Percent"":12.070088 }, { ""Name"":""nickel"", ""Percent"":9.129310 }, { ""Name"":""chromium"", ""Percent"":5.428319 }, { ""Name"":""manganese"", ""Percent"":4.984823 }, { ""Name"":""zinc"", ""Percent"":3.280198 }, { ""Name"":""molybdenum"", ""Percent"":0.788170 }, { ""Name"":""yttrium"", ""Percent"":0.720932 }, { ""Name"":""mercury"", ""Percent"":0.527081 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":1295214116.573334, ""Eccentricity"":0.000279, ""OrbitalInclination"":0.394879, ""Periapsis"":354.780335, ""OrbitalPeriod"":839909.958839, ""AscendingNode"":52.529909, ""MeanAnomaly"":182.894940, ""RotationPeriod"":839919.364911, ""AxialTilt"":0.076292, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan33 = @"{ ""timestamp"":""2023-03-25T16:00:40Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 1 b"", ""BodyID"":30, ""Parents"":[ {""Planet"":27}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":900.560628, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.000474, ""Radius"":687824.812500, ""SurfaceGravity"":0.399275, ""SurfaceTemperature"":54.663204, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.688660 }, { ""Name"":""carbon"", ""Percent"":22.442398 }, { ""Name"":""phosphorus"", ""Percent"":14.368010 }, { ""Name"":""iron"", ""Percent"":12.300508 }, { ""Name"":""nickel"", ""Percent"":9.303589 }, { ""Name"":""manganese"", ""Percent"":5.079984 }, { ""Name"":""selenium"", ""Percent"":4.177001 }, { ""Name"":""germanium"", ""Percent"":3.527333 }, { ""Name"":""niobium"", ""Percent"":0.840674 }, { ""Name"":""yttrium"", ""Percent"":0.734695 }, { ""Name"":""mercury"", ""Percent"":0.537143 } ], ""Composition"":{ ""Ice"":0.817601, ""Rock"":0.165621, ""Metal"":0.016778 }, ""SemiMajorAxis"":737900143.861771, ""Eccentricity"":0.000502, ""OrbitalInclination"":0.181315, ""Periapsis"":332.511224, ""OrbitalPeriod"":361174.190044, ""AscendingNode"":134.550693, ""MeanAnomaly"":56.779724, ""RotationPeriod"":-361178.258847, ""AxialTilt"":-3.008065, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan34 = @"{ ""timestamp"":""2023-03-25T16:00:42Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 1 c"", ""BodyID"":31, ""Parents"":[ {""Planet"":27}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":900.857516, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.000330, ""Radius"":610961.187500, ""SurfaceGravity"":0.352519, ""SurfaceTemperature"":53.768112, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.466549 }, { ""Name"":""carbon"", ""Percent"":22.255629 }, { ""Name"":""phosphorus"", ""Percent"":14.248435 }, { ""Name"":""iron"", ""Percent"":12.050862 }, { ""Name"":""nickel"", ""Percent"":9.114767 }, { ""Name"":""chromium"", ""Percent"":5.419672 }, { ""Name"":""manganese"", ""Percent"":4.976883 }, { ""Name"":""vanadium"", ""Percent"":2.959272 }, { ""Name"":""tellurium"", ""Percent"":0.897411 }, { ""Name"":""niobium"", ""Percent"":0.823612 }, { ""Name"":""molybdenum"", ""Percent"":0.786914 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":982772111.892700, ""Eccentricity"":0.000687, ""OrbitalInclination"":-0.006285, ""Periapsis"":72.306346, ""OrbitalPeriod"":555136.281252, ""AscendingNode"":-49.656425, ""MeanAnomaly"":344.790417, ""RotationPeriod"":555142.481646, ""AxialTilt"":0.353356, ""WasDiscovered"":false, ""WasMapped"":false }";
            //var scanBaryCentre3 = @"{ ""timestamp"":""2023-03-25T16:00:48Z"", ""event"":""ScanBaryCentre"", ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""BodyID"":33, ""SemiMajorAxis"":1722292602.062225, ""Eccentricity"":0.002548, ""OrbitalInclination"":-0.029077, ""Periapsis"":115.885057, ""OrbitalPeriod"":1287895.977497, ""AscendingNode"":175.002099, ""MeanAnomaly"":275.719969 }";
            var scan35 = @"{ ""timestamp"":""2023-03-25T16:00:48Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 1 e"", ""BodyID"":34, ""Parents"":[ {""Null"":33}, {""Planet"":27}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":901.009239, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""water geysers volcanism"", ""MassEM"":0.000392, ""Radius"":646631.437500, ""SurfaceGravity"":0.373372, ""SurfaceTemperature"":52.346928, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.884830 }, { ""Name"":""carbon"", ""Percent"":22.607359 }, { ""Name"":""phosphorus"", ""Percent"":14.473620 }, { ""Name"":""iron"", ""Percent"":12.241315 }, { ""Name"":""nickel"", ""Percent"":9.258819 }, { ""Name"":""manganese"", ""Percent"":5.055538 }, { ""Name"":""selenium"", ""Percent"":4.207704 }, { ""Name"":""vanadium"", ""Percent"":3.006041 }, { ""Name"":""niobium"", ""Percent"":0.836629 }, { ""Name"":""ruthenium"", ""Percent"":0.755979 }, { ""Name"":""tungsten"", ""Percent"":0.672171 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":2708460.807800, ""Eccentricity"":0.033148, ""OrbitalInclination"":5.016322, ""Periapsis"":347.047174, ""OrbitalPeriod"":149660.867453, ""AscendingNode"":-12.196546, ""MeanAnomaly"":145.020294, ""RotationPeriod"":214560.242889, ""AxialTilt"":0.515014, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan36 = @"{ ""timestamp"":""2023-03-25T16:00:52Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 1 f"", ""BodyID"":35, ""Parents"":[ {""Null"":33}, {""Planet"":27}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":900.995910, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""water geysers volcanism"", ""MassEM"":0.000371, ""Radius"":635176.375000, ""SurfaceGravity"":0.366672, ""SurfaceTemperature"":52.346928, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":28.609589 }, { ""Name"":""carbon"", ""Percent"":24.057699 }, { ""Name"":""phosphorus"", ""Percent"":15.402153 }, { ""Name"":""iron"", ""Percent"":13.026638 }, { ""Name"":""nickel"", ""Percent"":9.852804 }, { ""Name"":""germanium"", ""Percent"":3.745439 }, { ""Name"":""arsenic"", ""Percent"":1.675011 }, { ""Name"":""zirconium"", ""Percent"":1.512661 }, { ""Name"":""yttrium"", ""Percent"":0.778066 }, { ""Name"":""tin"", ""Percent"":0.771094 }, { ""Name"":""mercury"", ""Percent"":0.568852 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":2858327.269554, ""Eccentricity"":0.033148, ""OrbitalInclination"":5.016322, ""Periapsis"":167.047180, ""OrbitalPeriod"":149660.867453, ""AscendingNode"":-12.196546, ""MeanAnomaly"":145.029837, ""RotationPeriod"":208859.685273, ""AxialTilt"":-0.047267, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan37 = @"{ ""timestamp"":""2023-03-25T16:00:58Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 1 g"", ""BodyID"":36, ""Parents"":[ {""Planet"":27}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":893.041736, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""thin argon atmosphere"", ""AtmosphereType"":""Argon"", ""AtmosphereComposition"":[ { ""Name"":""Argon"", ""Percent"":100.000000 } ], ""Volcanism"":"""", ""MassEM"":0.002339, ""Radius"":1168659.750000, ""SurfaceGravity"":0.682523, ""SurfaceTemperature"":51.252640, ""SurfacePressure"":234.016724, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":27.325672 }, { ""Name"":""carbon"", ""Percent"":22.978062 }, { ""Name"":""phosphorus"", ""Percent"":14.710949 }, { ""Name"":""iron"", ""Percent"":12.442041 }, { ""Name"":""nickel"", ""Percent"":9.410639 }, { ""Name"":""selenium"", ""Percent"":4.276699 }, { ""Name"":""zinc"", ""Percent"":3.381281 }, { ""Name"":""vanadium"", ""Percent"":3.055332 }, { ""Name"":""niobium"", ""Percent"":0.850347 }, { ""Name"":""molybdenum"", ""Percent"":0.812458 }, { ""Name"":""antimony"", ""Percent"":0.756518 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":3161780476.570129, ""Eccentricity"":0.001952, ""OrbitalInclination"":0.822125, ""Periapsis"":193.446202, ""OrbitalPeriod"":3203449.487686, ""AscendingNode"":73.794928, ""MeanAnomaly"":315.758955, ""RotationPeriod"":3203485.557791, ""AxialTilt"":-0.376384, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan38 = @"{ ""timestamp"":""2023-03-25T16:01:16Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 3"", ""BodyID"":46, ""Parents"":[ {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1489.718274, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Sudarsky class I gas giant"", ""Atmosphere"":"""", ""AtmosphereComposition"":[ { ""Name"":""Hydrogen"", ""Percent"":73.092957 }, { ""Name"":""Helium"", ""Percent"":26.907040 } ], ""Volcanism"":"""", ""MassEM"":70.926521, ""Radius"":52562964.000000, ""SurfaceGravity"":10.231948, ""SurfaceTemperature"":47.122860, ""SurfacePressure"":0.000000, ""Landable"":false, ""SemiMajorAxis"":455779534578.323364, ""Eccentricity"":0.000413, ""OrbitalInclination"":0.286425, ""Periapsis"":343.474302, ""OrbitalPeriod"":317895370.721817, ""AscendingNode"":-113.660283, ""MeanAnomaly"":85.517915, ""RotationPeriod"":37091.256824, ""AxialTilt"":-2.995112, ""Rings"":[ { ""Name"":""Flyae Drye HT-X b48-1 AB 3 A Ring"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":6.6402e+10, ""InnerRad"":8.4515e+07, ""OuterRad"":9.6287e+07 }, { ""Name"":""Flyae Drye HT-X b48-1 AB 3 B Ring"", ""RingClass"":""eRingClass_Icy"", ""MassMT"":3.5864e+11, ""InnerRad"":9.6387e+07, ""OuterRad"":1.4411e+08 } ], ""ReserveLevel"":""PristineResources"", ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan39 = @"{ ""timestamp"":""2023-03-25T16:01:24Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 3 b"", ""BodyID"":50, ""Parents"":[ {""Planet"":46}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1490.676597, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major water geysers volcanism"", ""MassEM"":0.001095, ""Radius"":909298.562500, ""SurfaceGravity"":0.527968, ""SurfaceTemperature"":38.534313, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.808327 }, { ""Name"":""carbon"", ""Percent"":22.543026 }, { ""Name"":""phosphorus"", ""Percent"":14.432432 }, { ""Name"":""iron"", ""Percent"":12.206478 }, { ""Name"":""nickel"", ""Percent"":9.232470 }, { ""Name"":""manganese"", ""Percent"":5.041151 }, { ""Name"":""selenium"", ""Percent"":4.195730 }, { ""Name"":""zinc"", ""Percent"":3.317264 }, { ""Name"":""cadmium"", ""Percent"":0.947888 }, { ""Name"":""antimony"", ""Percent"":0.742195 }, { ""Name"":""mercury"", ""Percent"":0.533037 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":290448123.216629, ""Eccentricity"":0.000041, ""OrbitalInclination"":0.000077, ""Periapsis"":88.064952, ""OrbitalPeriod"":184963.554144, ""AscendingNode"":-0.236724, ""MeanAnomaly"":2.415358, ""RotationPeriod"":184977.608679, ""AxialTilt"":-0.468633, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan40 = @"{ ""timestamp"":""2023-03-25T16:01:27Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 3 a"", ""BodyID"":49, ""Parents"":[ {""Planet"":46}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1490.487768, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major water geysers volcanism"", ""MassEM"":0.000709, ""Radius"":787148.625000, ""SurfaceGravity"":0.455842, ""SurfaceTemperature"":38.669968, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.784983 }, { ""Name"":""carbon"", ""Percent"":22.523397 }, { ""Name"":""phosphorus"", ""Percent"":14.419866 }, { ""Name"":""iron"", ""Percent"":12.195851 }, { ""Name"":""nickel"", ""Percent"":9.224431 }, { ""Name"":""chromium"", ""Percent"":5.484880 }, { ""Name"":""selenium"", ""Percent"":4.192076 }, { ""Name"":""vanadium"", ""Percent"":2.994877 }, { ""Name"":""cadmium"", ""Percent"":0.947063 }, { ""Name"":""molybdenum"", ""Percent"":0.796382 }, { ""Name"":""technetium"", ""Percent"":0.436196 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":243073296.546936, ""Eccentricity"":0.000015, ""OrbitalInclination"":-0.001373, ""Periapsis"":34.535462, ""OrbitalPeriod"":141608.446836, ""AscendingNode"":168.263739, ""MeanAnomaly"":102.382839, ""RotationPeriod"":141619.210518, ""AxialTilt"":-0.156814, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan41 = @"{ ""timestamp"":""2023-03-25T16:01:33Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 3 d"", ""BodyID"":52, ""Parents"":[ {""Planet"":46}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1490.652382, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.002222, ""Radius"":1147006.000000, ""SurfaceGravity"":0.673044, ""SurfaceTemperature"":38.126705, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.284918 }, { ""Name"":""carbon"", ""Percent"":22.102894 }, { ""Name"":""phosphorus"", ""Percent"":14.150654 }, { ""Name"":""iron"", ""Percent"":12.137581 }, { ""Name"":""nickel"", ""Percent"":9.180357 }, { ""Name"":""chromium"", ""Percent"":5.458673 }, { ""Name"":""manganese"", ""Percent"":5.012697 }, { ""Name"":""zinc"", ""Percent"":3.298540 }, { ""Name"":""cadmium"", ""Percent"":0.942538 }, { ""Name"":""tellurium"", ""Percent"":0.901119 }, { ""Name"":""mercury"", ""Percent"":0.530028 } ], ""Composition"":{ ""Ice"":0.816632, ""Rock"":0.166463, ""Metal"":0.016905 }, ""SemiMajorAxis"":559946155.548096, ""Eccentricity"":0.000283, ""OrbitalInclination"":0.701979, ""Periapsis"":190.211510, ""OrbitalPeriod"":495111.119747, ""AscendingNode"":30.024453, ""MeanAnomaly"":76.689927, ""RotationPeriod"":-495148.776248, ""AxialTilt"":2.314908, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan42 = @"{ ""timestamp"":""2023-03-25T16:01:36Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 3 c"", ""BodyID"":51, ""Parents"":[ {""Planet"":46}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1488.404259, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major water magma volcanism"", ""MassEM"":0.000723, ""Radius"":792361.562500, ""SurfaceGravity"":0.458912, ""SurfaceTemperature"":38.308350, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.717163 }, { ""Name"":""carbon"", ""Percent"":22.466368 }, { ""Name"":""phosphorus"", ""Percent"":14.383355 }, { ""Name"":""iron"", ""Percent"":12.164970 }, { ""Name"":""nickel"", ""Percent"":9.201075 }, { ""Name"":""manganese"", ""Percent"":5.024008 }, { ""Name"":""selenium"", ""Percent"":4.181462 }, { ""Name"":""germanium"", ""Percent"":3.497691 }, { ""Name"":""cadmium"", ""Percent"":0.944665 }, { ""Name"":""ruthenium"", ""Percent"":0.751265 }, { ""Name"":""tungsten"", ""Percent"":0.667979 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":406875264.644623, ""Eccentricity"":0.001393, ""OrbitalInclination"":-0.073572, ""Periapsis"":55.273098, ""OrbitalPeriod"":306672.781706, ""AscendingNode"":141.547502, ""MeanAnomaly"":281.300179, ""RotationPeriod"":306696.055435, ""AxialTilt"":0.314369, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan43 = @"{ ""timestamp"":""2023-03-25T16:01:41Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 3 f"", ""BodyID"":54, ""Parents"":[ {""Planet"":46}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1497.064339, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.002558, ""Radius"":1203746.500000, ""SurfaceGravity"":0.703583, ""SurfaceTemperature"":37.593781, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":26.160482 }, { ""Name"":""carbon"", ""Percent"":21.998255 }, { ""Name"":""phosphorus"", ""Percent"":14.083661 }, { ""Name"":""iron"", ""Percent"":11.911500 }, { ""Name"":""nickel"", ""Percent"":9.009359 }, { ""Name"":""chromium"", ""Percent"":5.356997 }, { ""Name"":""manganese"", ""Percent"":4.919328 }, { ""Name"":""selenium"", ""Percent"":4.094337 }, { ""Name"":""cadmium"", ""Percent"":0.924982 }, { ""Name"":""tellurium"", ""Percent"":0.887033 }, { ""Name"":""tungsten"", ""Percent"":0.654061 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":2312374591.827393, ""Eccentricity"":0.005048, ""OrbitalInclination"":-0.047748, ""Periapsis"":260.014749, ""OrbitalPeriod"":4154995.441437, ""AscendingNode"":-141.419617, ""MeanAnomaly"":49.285418, ""RotationPeriod"":4155311.363740, ""AxialTilt"":0.294899, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan44 = @"{ ""timestamp"":""2023-03-25T16:01:44Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 3 e"", ""BodyID"":53, ""Parents"":[ {""Planet"":46}, {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1493.252557, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.001466, ""Radius"":1001495.250000, ""SurfaceGravity"":0.582686, ""SurfaceTemperature"":37.690853, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""sulphur"", ""Percent"":27.379250 }, { ""Name"":""carbon"", ""Percent"":23.023113 }, { ""Name"":""phosphorus"", ""Percent"":14.739792 }, { ""Name"":""iron"", ""Percent"":12.466434 }, { ""Name"":""nickel"", ""Percent"":9.429089 }, { ""Name"":""selenium"", ""Percent"":4.285084 }, { ""Name"":""zinc"", ""Percent"":3.387910 }, { ""Name"":""vanadium"", ""Percent"":3.061322 }, { ""Name"":""cadmium"", ""Percent"":0.968075 }, { ""Name"":""molybdenum"", ""Percent"":0.814051 }, { ""Name"":""technetium"", ""Percent"":0.445874 } ], ""Composition"":{ ""Ice"":0.823637, ""Rock"":0.160376, ""Metal"":0.015987 }, ""SemiMajorAxis"":1638397634.029388, ""Eccentricity"":0.000304, ""OrbitalInclination"":0.319619, ""Periapsis"":119.177799, ""OrbitalPeriod"":2478061.974049, ""AscendingNode"":62.171947, ""MeanAnomaly"":146.689644, ""RotationPeriod"":2478250.426731, ""AxialTilt"":-0.032227, ""WasDiscovered"":false, ""WasMapped"":false }";
            var scan45 = @"{ ""timestamp"":""2023-03-25T16:02:19Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Flyae Drye HT-X b48-1 AB 4"", ""BodyID"":55, ""Parents"":[ {""Null"":1}, {""Null"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":2087.963651, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""helium atmosphere"", ""AtmosphereType"":""Helium"", ""AtmosphereComposition"":[ { ""Name"":""Helium"", ""Percent"":89.334976 }, { ""Name"":""Hydrogen"", ""Percent"":8.427828 }, { ""Name"":""Neon"", ""Percent"":2.237205 } ], ""Volcanism"":""major water geysers volcanism"", ""MassEM"":4.124320, ""Radius"":11725180.000000, ""SurfaceGravity"":11.957013, ""SurfaceTemperature"":32.570705, ""SurfacePressure"":77539.242188, ""Landable"":false, ""Composition"":{ ""Ice"":0.681303, ""Rock"":0.212257, ""Metal"":0.105794 }, ""SemiMajorAxis"":641202288866.043091, ""Eccentricity"":0.000371, ""OrbitalInclination"":0.144758, ""Periapsis"":34.285849, ""OrbitalPeriod"":530450201.034546, ""AscendingNode"":-31.508977, ""MeanAnomaly"":272.071147, ""RotationPeriod"":47389.573199, ""AxialTilt"":0.342787, ""Rings"":[ { ""Name"":""Flyae Drye HT-X b48-1 AB 4 A Ring"", ""RingClass"":""eRingClass_Icy"", ""MassMT"":1.2139e+09, ""InnerRad"":1.77e+07, ""OuterRad"":5.5829e+07 } ], ""ReserveLevel"":""PristineResources"", ""WasDiscovered"":false, ""WasMapped"":false }";
            var allBodiesFound = @"{ ""timestamp"":""2023-03-25T16:02:19Z"", ""event"":""FSSAllBodiesFound"", ""SystemName"":""TestSystem"", ""SystemAddress"":9999999999999, ""Count"":45 }";

            // Test the temporary star we create for targeted systems
            var nextSystemStar = new Body()
            {
                bodyType = BodyType.FromEDName( "Star" ),
                systemname = "TestSystem",
                systemAddress = 9999999999999,
                distance = 0M,
                stellarclass = "M"
            };
            var nextSystem = new StarSystem() { systemname = "TestSystem", systemAddress = 9999999999999 };
            nextSystem.AddOrUpdateBody(nextSystemStar);
            Assert.AreEqual(1, nextSystem.bodies.Count);
            eddiPrivateObject.SetFieldOrProperty("NextStarSystem", nextSystem);

            var events = JournalMonitor.ParseJournalEntry(startJump);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (FSDEngagedEvent)events[ 0 ] );

            events = JournalMonitor.ParseJournalEntry(fsdJump);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (JumpedEvent)events[ 0 ] );

            // Test that the current star system has been updated
            var currentStarSystem = (StarSystem)eddiPrivateObject.GetFieldOrProperty( "CurrentStarSystem" );
            Assert.AreEqual( 1, currentStarSystem.bodies.Count );

            // Scan 1
            events = JournalMonitor.ParseJournalEntry(scan1);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (StarScannedEvent)events[ 0 ] );

            // Test that the temporary star has been replaced by the main star
            Assert.AreEqual(1, currentStarSystem.bodies.Count);
            var mainStar = currentStarSystem.bodies.FirstOrDefault();
            Assert.IsNotNull( mainStar );
            Assert.AreEqual( BodyType.FromEDName( "Star" ), mainStar.bodyType );
            Assert.AreEqual( "TestSystem", mainStar.systemname );
            Assert.AreEqual( (ulong)9999999999999, mainStar.systemAddress );
            Assert.AreEqual( 0M, mainStar.distance );
            Assert.AreEqual( "M", mainStar.stellarclass );
            Assert.AreEqual( "Flyae Drye HT-X b48-1 A", mainStar.bodyname );
            Assert.AreEqual( 2, mainStar.bodyId );

            // Verify duplicate scans are not double counted
            events = JournalMonitor.ParseJournalEntry( scan1 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (StarScannedEvent)events[ 0 ] );
            Assert.AreEqual( 1, currentStarSystem.bodies.Count );

            // Scan 2
            events = JournalMonitor.ParseJournalEntry(scan2);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );
            Assert.AreEqual( 2, currentStarSystem.bodies.Count );

            // Scan 3
            events = JournalMonitor.ParseJournalEntry(scan3);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 4
            events = JournalMonitor.ParseJournalEntry(scan4);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 5
            events = JournalMonitor.ParseJournalEntry(scan5);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 6
            events = JournalMonitor.ParseJournalEntry(scan6);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 7
            events = JournalMonitor.ParseJournalEntry(scan7);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Discovery Scan
            Assert.AreEqual( 7, currentStarSystem.bodies.Count );
            events = JournalMonitor.ParseJournalEntry(discoveryScan);
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (DiscoveryScanEvent)events[ 0 ] );
            Assert.AreEqual( 45, currentStarSystem.totalbodies );

            // Scan 8
            events = JournalMonitor.ParseJournalEntry( scan8 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (StarScannedEvent)events[ 0 ] );

            // Scan 9
            events = JournalMonitor.ParseJournalEntry( scan9 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (StarScannedEvent)events[ 0 ] );

            // Scan 10
            events = JournalMonitor.ParseJournalEntry( scan10 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 11
            events = JournalMonitor.ParseJournalEntry( scan11 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 12
            events = JournalMonitor.ParseJournalEntry( scan12 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 13
            events = JournalMonitor.ParseJournalEntry( scan13 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 14
            events = JournalMonitor.ParseJournalEntry( scan14 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 15
            events = JournalMonitor.ParseJournalEntry( scan15 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 16
            events = JournalMonitor.ParseJournalEntry( scan16 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 17
            events = JournalMonitor.ParseJournalEntry( scan17 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 18
            events = JournalMonitor.ParseJournalEntry( scan18 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 19
            events = JournalMonitor.ParseJournalEntry( scan19 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 20
            events = JournalMonitor.ParseJournalEntry( scan20 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 21
            events = JournalMonitor.ParseJournalEntry( scan21 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 22
            events = JournalMonitor.ParseJournalEntry( scan22 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 23
            events = JournalMonitor.ParseJournalEntry( scan23 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 24
            events = JournalMonitor.ParseJournalEntry( scan24 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 25
            events = JournalMonitor.ParseJournalEntry( scan25 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 26
            events = JournalMonitor.ParseJournalEntry( scan26 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 27
            events = JournalMonitor.ParseJournalEntry( scan27 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 28
            events = JournalMonitor.ParseJournalEntry( scan28 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 29
            events = JournalMonitor.ParseJournalEntry( scan29 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 30
            events = JournalMonitor.ParseJournalEntry( scan30 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 31
            events = JournalMonitor.ParseJournalEntry( scan31 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 32
            events = JournalMonitor.ParseJournalEntry( scan32 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 33
            events = JournalMonitor.ParseJournalEntry( scan33 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 34
            events = JournalMonitor.ParseJournalEntry( scan34 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 35
            events = JournalMonitor.ParseJournalEntry( scan35 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 36
            events = JournalMonitor.ParseJournalEntry( scan36 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 37
            events = JournalMonitor.ParseJournalEntry( scan37 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 38
            events = JournalMonitor.ParseJournalEntry( scan38 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 39
            events = JournalMonitor.ParseJournalEntry( scan39 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 40
            events = JournalMonitor.ParseJournalEntry( scan40 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 41
            events = JournalMonitor.ParseJournalEntry( scan41 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 42
            events = JournalMonitor.ParseJournalEntry( scan42 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 43
            events = JournalMonitor.ParseJournalEntry( scan43 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 44
            events = JournalMonitor.ParseJournalEntry( scan44 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            // Scan 45
            events = JournalMonitor.ParseJournalEntry( scan45 );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (BodyScannedEvent)events[ 0 ] );

            Assert.AreEqual( 45, currentStarSystem.totalbodies );
            Assert.AreEqual( 3, currentStarSystem.bodies.Count( b => b.bodyType == BodyType.Star ));
            Assert.AreEqual( 22, currentStarSystem.bodies.Count( b => b.bodyType == BodyType.Planet ) );
            Assert.AreEqual( 20, currentStarSystem.bodies.Count( b => b.bodyType == BodyType.Moon ) );

            events = JournalMonitor.ParseJournalEntry( allBodiesFound );
            Assert.AreEqual( 1, events.Count );
            eddiPrivateObject.Invoke( "eventHandler", (SystemScanComplete)events[ 0 ] );
            Assert.AreEqual(45, ( (SystemScanComplete)events[ 0 ] ).count );
        }
    }
}
