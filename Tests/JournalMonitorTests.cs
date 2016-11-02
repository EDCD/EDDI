using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Newtonsoft.Json;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;

namespace Tests
{
    [TestClass]
    public class JournalMonitorTests
    {
        [TestMethod]
        public void TestJournalPlanetScan()
        {
            string line = @"{ ""timestamp"":""2016-09-22T21:34:30Z"", ""event"":""Scan"", ""BodyName"":""Nemehim 4"", ""DistanceFromArrivalLS"":1115.837646, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Rocky ice body"", ""Atmosphere"":"""", ""Volcanism"":"""", ""MassEM"":0.013448, ""Radius"":1688803.625000, ""SurfaceGravity"":1.879402, ""SurfaceTemperature"":103.615654, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":{ ""iron"":18.8, ""sulphur"":17.7, ""carbon"":14.9, ""nickel"":14.3, ""phosphorus"":9.6, ""chromium"":8.5, ""manganese"":7.8, ""zinc"":5.1, ""molybdenum"":1.2, ""tungsten"":1.0, ""tellurium"":1.0 }, ""OrbitalPeriod"":122165280.000000, ""RotationPeriod"":112645.117188 }";
            Event theEvent = JournalMonitor.ParseJournalEntry(line);
            Console.WriteLine(JsonConvert.SerializeObject(theEvent));
        }

        [TestMethod]
        public void TestJournalPlanetScan1()
        {
            string line = @"{ ""timestamp"":""2016 - 11 - 01T18: 49:07Z"", ""event"":""Scan"", ""BodyName"":""Grea Bloae HH-T d4-44 4"", ""DistanceFromArrivalLS"":703.763611, ""TidalLock"":false, ""TerraformState"":""Terraformable"", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""hot thick carbon dioxide atmosphere"", ""Volcanism"":""minor metallic magma volcanism"", ""MassEM"":2.171783, ""Radius"":7622170.500000, ""SurfaceGravity"":14.899396, ""SurfaceTemperature"":836.165466, ""SurfacePressure"":33000114.000000, ""Landable"":false, ""SemiMajorAxis"":210957926400.000000, ""Eccentricity"":0.000248, ""OrbitalInclination"":0.015659, ""Periapsis"":104.416656, ""OrbitalPeriod"":48801056.000000, ""RotationPeriod"":79442.242188 }";
            Event theEvent = JournalMonitor.ParseJournalEntry(line);
            Assert.IsNotNull(theEvent);
            //Assert.IsInstanceOfType(theEvent, BodyScannedEvent);
            BodyScannedEvent ev = theEvent as BodyScannedEvent;
            Assert.IsNotNull(ev);
            Assert.AreEqual(ev.name, "Grea Bloae HH-T d4-44 4");
            Assert.AreEqual(ev.distancefromarrival, (decimal)703.763611);
            Assert.IsFalse(ev.tidallylocked);
            Assert.AreEqual(ev.terraformstate, "Terraformable");
            Assert.AreEqual(ev.bodyclass, "High metal content body");
            Assert.AreEqual(ev.atmosphere, "hot thick carbon dioxide atmosphere");
            Assert.AreEqual(ev.volcanism, "minor metallic magma volcanism");
            //Assert.IsTrue(ev.massEM == (decimal)2.171783)
            //Assert.IsTrue(ev.radius  == (decimal)7622170.500000);
            Assert.AreEqual(ev.gravity, Body.ms2g((decimal)14.899396));
            Assert.AreEqual(ev.temperature, (decimal)836.165466);
            Assert.AreEqual(ev.pressure, (decimal)33000114.000000);
            Assert.IsFalse(ev.landable);
            Assert.AreEqual(ev.semimajoraxis, (decimal)210957926400.000000);
            Assert.AreEqual(ev.eccentricity, (decimal)0.000248);
            Assert.AreEqual(ev.orbitalinclination, (decimal)0.015659);
            Assert.AreEqual(ev.periapsis, (decimal)104.416656);
            Assert.AreEqual(ev.orbitalperiod, (decimal)48801056.000000);
            Assert.AreEqual(ev.rotationperiod, (decimal)79442.242188);
            Console.WriteLine(JsonConvert.SerializeObject(theEvent));
        }

        [TestMethod]
        public void TestJournalStarScan1()
        {
            string line = @"{ ""timestamp"":""2016-10-27T08:51:23Z"", ""event"":""Scan"", ""BodyName"":""Vela Dark Region FG-Y d3"", ""DistanceFromArrivalLS"":0.000000, ""StarType"":""K"", ""StellarMass"":0.960938, ""Radius"":692146368.000000, ""AbsoluteMagnitude"":5.375961, ""Age_MY"":230, ""SurfaceTemperature"":5108.000000, ""RotationPeriod"":393121.093750, ""Rings"":[ { ""Name"":""Vela Dark Region FG-Y d3 A Belt"", ""RingClass"":""eRingClass_MetalRich"", ""MassMT"":1.2262e+10, ""InnerRad"":1.2288e+09, ""OuterRad"":2.3812e+09 } ] }";
            Event theEvent = JournalMonitor.ParseJournalEntry(line);
            Assert.IsNotNull(theEvent);
        }

        [TestMethod]
        public void TestJournalStarScan2()
        {
            string line = @"{ ""timestamp"":""2016-10-28T12:07:09Z"", ""event"":""Scan"", ""BodyName"":""Col 285 Sector CG-X d1-44"", ""DistanceFromArrivalLS"":0.000000, ""StarType"":""TTS"", ""StellarMass"":0.808594, ""Radius"":659162816.000000, ""AbsoluteMagnitude"":6.411560, ""Age_MY"":154, ""SurfaceTemperature"":4124.000000, ""RotationPeriod"":341417.281250, ""Rings"":[ { ""Name"":""Col 285 Sector CG-X d1-44 A Belt"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":1.1625e+13, ""InnerRad"":1.0876e+09, ""OuterRad"":2.4192e+09 } ] }";
            StarScannedEvent theEvent = (StarScannedEvent)JournalMonitor.ParseJournalEntry(line);
            Assert.IsNotNull(theEvent);
            Assert.AreEqual(theEvent.radius, (decimal)659162816.0);
            Assert.AreEqual(theEvent.solarradius, StarClass.solarradius((decimal)659162816.000000));
        }

        [TestMethod]
        public void TestJournalShipyardNew1()
        {
            string line = @"{ ""timestamp"":""2016-10-27T08:49:08Z"", ""event"":""ShipyardNew"", ""ShipType"":""belugaliner"", ""NewShipID"":56 }";
            Event theEvent = JournalMonitor.ParseJournalEntry(line);
            Assert.IsNotNull(theEvent);
        }
    }
}
