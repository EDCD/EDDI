using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Newtonsoft.Json;
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
        public void TestJournalStarScan1()
        {
            string line = @"{ ""timestamp"":""2016-10-27T08:51:23Z"", ""event"":""Scan"", ""BodyName"":""Vela Dark Region FG-Y d3"", ""DistanceFromArrivalLS"":0.000000, ""StarType"":""K"", ""StellarMass"":0.960938, ""Radius"":692146368.000000, ""AbsoluteMagnitude"":5.375961, ""Age_MY"":230, ""SurfaceTemperature"":5108.000000, ""RotationPeriod"":393121.093750, ""Rings"":[ { ""Name"":""Vela Dark Region FG-Y d3 A Belt"", ""RingClass"":""eRingClass_MetalRich"", ""MassMT"":1.2262e+10, ""InnerRad"":1.2288e+09, ""OuterRad"":2.3812e+09 } ] }";
            Event theEvent = JournalMonitor.ParseJournalEntry(line);
            Assert.IsNotNull(theEvent);
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
