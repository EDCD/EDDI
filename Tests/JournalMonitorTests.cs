﻿using EddiCore;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiJournalMonitor;
using EddiMissionMonitor;
using EddiShipMonitor;
using EddiStarMapService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Tests.Properties;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class JournalMonitorTests : TestBase
    {
        FakeEdsmRestClient fakeEdsmRestClient;
        StarMapService fakeEdsmService;
        private DataProviderService dataProviderService;

        [TestInitialize]
        public void start()
        {
            fakeEdsmRestClient = new FakeEdsmRestClient();
            fakeEdsmService = new StarMapService(fakeEdsmRestClient);
            dataProviderService = new DataProviderService(fakeEdsmService);
            MakeSafe();
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
            Assert.AreEqual("Grea Bloae HH-T d4-44 4", ev.bodyname);
            Assert.AreEqual((decimal)703.763611, ev.distance);
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
            Assert.AreEqual((decimal)0.015659, ev.inclination);
            Assert.AreEqual((decimal)104.416656, ev.periapsis);
            Assert.AreEqual(564.827, (double)ev.orbitalperiod, 0.01);
            Assert.AreEqual(0.91947, (double)ev.rotationalperiod, 0.01);
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
            Assert.AreEqual(96.5M, ev.atmospherecompositions[0].percent);
            Assert.AreEqual("Rock", ev.solidcompositions[0].invariantName);
            Assert.AreEqual(70M, ev.solidcompositions[0].percent);
        }

        [TestMethod]
        public void TestJournalPlanetScan4()
        {
            // Test new scan data from game version 3.4.
            string line = @"{ ""timestamp"":""2019-04-12T04:42:10Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""HR 1185 A 1"", ""BodyID"":4, ""Parents"":[ {""Null"":3}, {""Star"":1}, {""Null"":0} ], ""DistanceFromArrivalLS"":45.276505, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick silicate vapour atmosphere"", ""AtmosphereType"":""SilicateVapour"", ""AtmosphereComposition"":[ { ""Name"":""Silicates"", ""Percent"":99.989662 } ], ""Volcanism"":""major silicate vapour geysers volcanism"", ""MassEM"":2.428317, ""Radius"":8014977.000000, ""SurfaceGravity"":15.066424, ""SurfaceTemperature"":4894.569336, ""SurfacePressure"":6359968768.000000, ""Landable"":false, ""Composition"":{ ""Ice"":0.000073, ""Rock"":0.671092, ""Metal"":0.322412 }, ""SemiMajorAxis"":15315170.000000, ""Eccentricity"":0.021248, ""OrbitalInclination"":-4.599963, ""Periapsis"":144.548447, ""OrbitalPeriod"":27184.082031, ""RotationPeriod"":39590.453125, ""AxialTilt"":0.120614, ""WasDiscovered"":true, ""WasMapped"":true }";
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
        public void TestJournalStarScan1()
        {
            string line = @"{ ""timestamp"":""2016-10-27T08:51:23Z"", ""event"":""Scan"", ""BodyName"":""Vela Dark Region FG-Y d3"", ""DistanceFromArrivalLS"":0.000000, ""StarType"":""K"", ""StellarMass"":0.960938, ""Radius"":692146368.000000, ""AbsoluteMagnitude"":5.375961, ""Age_MY"":230, ""SurfaceTemperature"":5108.000000, ""RotationPeriod"":393121.093750, ""Rings"":[ { ""Name"":""Vela Dark Region FG-Y d3 A Belt"", ""RingClass"":""eRingClass_Metalic"", ""MassMT"":1.2262e+10, ""InnerRad"":1.2288e+09, ""OuterRad"":2.3812e+09 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            StarScannedEvent theEvent = (StarScannedEvent)events[0];

            Assert.AreEqual(230, theEvent.age);
            Assert.IsNull(theEvent.eccentricity);
            Assert.AreEqual("Vela Dark Region FG-Y d3", theEvent.bodyname);
            Assert.IsNull(theEvent.orbitalperiod);
            Assert.AreEqual(692146.368000000M, theEvent.radius);
            Assert.IsNull(theEvent.semimajoraxis);
            Assert.AreEqual(0.960938, (double)theEvent.solarmass, 0.001);
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
            Assert.AreEqual(303.548, (double)theEvent.estimatedhabzoneinner, .01);
            Assert.AreEqual(604.861, (double)theEvent.estimatedhabzoneouter, .01);
            // Ring
            Assert.AreEqual("Metallic", theEvent.rings[0].Composition.invariantName);
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
            Assert.IsTrue(theEvent.mainstar ?? false);
        }

        [TestMethod]
        public void TestJournalStarScan3()
        {
            // Gamer version 3.4
            string line = @"{ ""timestamp"":""2019-04-12T04:49:10Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Pleiades Sector MN-T c3-14 B"", ""BodyID"":2, ""Parents"":[ {""Null"":0} ], ""DistanceFromArrivalLS"":84306.257813, ""StarType"":""M"", ""Subclass"":8, ""StellarMass"":0.246094, ""Radius"":316421920.000000, ""AbsoluteMagnitude"":10.680222, ""Age_MY"":702, ""SurfaceTemperature"":2228.000000, ""Luminosity"":""Va"", ""SemiMajorAxis"":20863141281792.000000, ""Eccentricity"":0.278661, ""OrbitalInclination"":-103.465088, ""Periapsis"":32.983871, ""OrbitalPeriod"":104334450688.000000, ""RotationPeriod"":212050.531250, ""AxialTilt"":0.000000, ""WasDiscovered"":true, ""WasMapped"":false }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            StarScannedEvent theEvent = (StarScannedEvent)events[0];
            Assert.AreEqual(8, theEvent.stellarsubclass);
            Debug.Assert(theEvent.alreadydiscovered != null, "theEvent.alreadydiscovered != null");
            Assert.IsTrue((bool)theEvent.alreadydiscovered);
            Assert.AreEqual(4916.66378995466M, theEvent.density);
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
            Assert.AreEqual(37, theEvent.densityprobability);
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
            Assert.AreEqual("Boom", theEvent.controllingfaction.presences.FirstOrDefault(p => p.systemName == theEvent.system)?.FactionState?.invariantName);
            Assert.AreEqual("Democracy", theEvent.controllingfaction.Government.invariantName);
            Assert.AreEqual("Alliance", theEvent.controllingfaction.Allegiance.invariantName);
            Assert.AreEqual(20, theEvent.stationservices.Count);
            Assert.AreEqual(1, theEvent.economyShares.Count);
            Assert.AreEqual("Service", theEvent.economyShares[0].economy.invariantName);
            Assert.AreEqual(1.0M, theEvent.economyShares[0].proportion);
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
            Assert.AreEqual("Salvage", sarEvent.commodity.category.invariantName);
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

            Assert.AreEqual("EXO", @event.faction);
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

            Assert.AreEqual("Ageno", @event.systemname);
            Assert.AreEqual(18262335038849, @event.systemAddress);
            Assert.AreEqual("Ageno B 2 a", @event.bodyname);

            string line2 = @"{ ""timestamp"":""2018 - 07 - 24T07: 08:58Z"", ""event"":""LeaveBody"", ""StarSystem"":""Ageno"", ""SystemAddress"":18262335038849, ""Body"":""Ageno B 2 a"", ""BodyID"":17 }";
            events = JournalMonitor.ParseJournalEntry(line2);
            NearSurfaceEvent @event2 = (NearSurfaceEvent)events[0];

            Assert.AreEqual("Ageno", @event2.systemname);
            Assert.AreEqual(18262335038849, @event2.systemAddress);
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
            Assert.AreEqual(670417429889, @event.systemAddress);
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
            Assert.AreEqual("PBSF SPACE ODDITY XBH-64Y", @event.signalSource.localizedName);

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
        public void TestSurfaceSignalsEvent()
        {
            string line = "{ \"timestamp\":\"2019-09-24T02:40:34Z\", \"event\":\"SAASignalsFound\", \"BodyName\":\"HIP 41908 AB 1 c a\", \"SystemAddress\":61461226668, \"BodyID\":11, \"Signals\":[ { \"Type\":\"$SAA_SignalType_Biological;\", \"Type_Localised\":\"Biological\", \"Count\":16 }, { \"Type\":\"$SAA_SignalType_Geological;\", \"Type_Localised\":\"Geological\", \"Count\":17 }, { \"Type\":\"$SAA_SignalType_Human;\", \"Type_Localised\":\"Human\", \"Count\":4 } ] }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            SurfaceSignalsEvent surfaceSignalEvent = (SurfaceSignalsEvent)events[0];
            Assert.AreEqual("HIP 41908 AB 1 c a", surfaceSignalEvent.bodyname);
            Assert.AreEqual(61461226668, surfaceSignalEvent.systemAddress);
            Assert.AreEqual(3, surfaceSignalEvent.surfacesignals.Count);
            // The types of surface signals are ordered by count, so we expect geo signals before bio signals.
            Assert.AreEqual("Geological Surface Signal", surfaceSignalEvent.surfacesignals[0].signalSource.invariantName);
            Assert.AreEqual(17, surfaceSignalEvent.surfacesignals[0].amount);
            Assert.AreEqual("Biological Surface Signal", surfaceSignalEvent.surfacesignals[1].signalSource.invariantName);
            Assert.AreEqual(16, surfaceSignalEvent.surfacesignals[1].amount);
            Assert.AreEqual("Human Surface Signal", surfaceSignalEvent.surfacesignals[2].signalSource.invariantName);
            Assert.AreEqual(4, surfaceSignalEvent.surfacesignals[2].amount);
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
            Assert.AreEqual(358797513434, @event.systemAddress);
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
            string line = "{ \"timestamp\":\"2020-06-12T11:01:40Z\", \"event\":\"CarrierJumpRequest\", \"CarrierID\":3701442048, \"SystemName\":\"Shinrarta Dezhra\", \"SystemAddress\":3932277478106, \"BodyID\":16 }";
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
        [DataRow(@"{ ""timestamp"":""2021-05-03T22:22:12Z"", ""event"":""Embark"", ""SRV"":false, ""Taxi"":false, ""Multicrew"":false, ""ID"":6, ""StarSystem"":""Sumod"", ""SystemAddress"":3961847269739, ""Body"":""Sharp Dock"", ""BodyID"":56, ""OnStation"":true, ""OnPlanet"":false, ""StationName"":""Sharp Dock"", ""StationType"":""Coriolis"", ""MarketID"":32239521286 }", false, true, false, false, 6, "Sumod", 3961847269739, "Sharp Dock", 56, "Sharp Dock", "Coriolis Starport", 32239521286, true, false)] // Embarking from an orbital station to your ship
        [DataRow(@"{ ""timestamp"":""2021-05-02T22:51:54Z"", ""event"":""Embark"", ""SRV"":true, ""Taxi"":false, ""Multicrew"":false, ""ID"":53, ""StarSystem"":""Nervi"", ""SystemAddress"":2518721481067, ""Body"":""Nervi 2 a"", ""BodyID"":17, ""OnStation"":false, ""OnPlanet"":true }", false, false, true, false, 53, "Nervi", 2518721481067, "Nervi 2 a", 17, null, null, null, false, true)] // Embarking from a surface to an SRV
        [DataRow(@"{ ""timestamp"":""2021-05-03T21:51:47Z"", ""event"":""Embark"", ""SRV"":false, ""Taxi"":true, ""Multicrew"":false, ""StarSystem"":""Firenses"", ""SystemAddress"":2868635379121, ""Body"":""Roberts Gateway"", ""BodyID"":44, ""OnStation"":true, ""OnPlanet"":false, ""StationName"":""Roberts Gateway"", ""StationType"":""Coriolis"", ""MarketID"":32216360961 }", false, false, false, true, null, "Firenses", 2868635379121, "Roberts Gateway", 44, "Roberts Gateway", "Coriolis Starport", 32216360961, true, false)] // Embarking from an orbital station to a taxi
        public void TestEmbarkEvent(string line, bool toMulticrew, bool toShip, bool toSRV, bool toTaxi, int? toLocalId, string systemName, long systemAddress, string bodyName, int? bodyId, string station, string stationType, long? marketId, bool onStation, bool onPlanet)
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
        [DataRow(@"{ ""timestamp"":""2021-05-03T21:57:16Z"", ""event"":""Disembark"", ""SRV"":false, ""Taxi"":true, ""Multicrew"":false, ""StarSystem"":""Sumod"", ""SystemAddress"":3961847269739, ""Body"":""Sharp Dock"", ""BodyID"":56, ""OnStation"":true, ""OnPlanet"":false, ""StationName"":""Sharp Dock"", ""StationType"":""Coriolis"", ""MarketID"":32239521286 }", false, false, false, true, null, "Sumod", 3961847269739, "Sharp Dock", 56, "Sharp Dock", "Coriolis Starport", 32239521286, true, false)] // Disembarking from a taxi to an orbital station
        [DataRow(@"{ ""timestamp"":""2021-05-03T21:47:38Z"", ""event"":""Disembark"", ""SRV"":false, ""Taxi"":false, ""Multicrew"":false, ""ID"":6, ""StarSystem"":""Firenses"", ""SystemAddress"":2868635379121, ""Body"":""Roberts Gateway"", ""BodyID"":44, ""OnStation"":true, ""OnPlanet"":false, ""StationName"":""Roberts Gateway"", ""StationType"":""Coriolis"", ""MarketID"":32216360961 }", false, true, false, false, 6, "Firenses", 2868635379121, "Roberts Gateway", 44, "Roberts Gateway", "Coriolis Starport", 32216360961, true, false)] // Disembarking from your ship to an orbital station
        [DataRow(@"{ ""timestamp"":""2021-05-02T22:52:25Z"", ""event"":""Disembark"", ""SRV"":true, ""Taxi"":false, ""Multicrew"":false, ""ID"":53, ""StarSystem"":""Nervi"", ""SystemAddress"":2518721481067, ""Body"":""Nervi 2 a"", ""BodyID"":17, ""OnStation"":false, ""OnPlanet"":true }", false, false, true, false, 53, "Nervi", 2518721481067, "Nervi 2 a", 17, null, null, null, false, true)] // Disembarking to an SRV from on foot
        public void TestDisembarkEvent(string line, bool fromMulticrew, bool fromShip, bool fromSRV, bool fromTaxi, int? fromLocalId, string systemName, long systemAddress, string bodyName, int? bodyId, string station, string stationType, long? marketId, bool onStation, bool onPlanet)
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
    }
}
