using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiEvents;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class BodyScanTests
    {
        [TestMethod]
        public void TestBodyScanDagutiiABC1b()
        {
            string data = @"{ ""timestamp"":""2016-10-05T10:28:04Z"", ""event"":""Scan"", ""BodyName"":""Dagutii ABC 1 b"", ""DistanceFromArrivalLS"":644.074463, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":"""", ""Volcanism"":""carbon dioxide geysers volcanism"", ""MassEM"":0.001305, ""Radius"":964000.375000, ""SurfaceGravity"":0.559799, ""SurfaceTemperature"":89.839241, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":{ ""sulphur"":26.8, ""carbon"":22.5, ""phosphorus"":14.4, ""iron"":12.1, ""nickel"":9.2, ""chromium"":5.4, ""selenium"":4.2, ""vanadium"":3.0, ""niobium"":0.8, ""molybdenum"":0.8, ""ruthenium"":0.7 }, ""SemiMajorAxis"":739982912.000000, ""Eccentricity"":0.000102, ""OrbitalInclination"":-0.614765, ""Periapsis"":233.420425, ""OrbitalPeriod"":242733.156250, ""RotationPeriod"":242735.265625 }";

            List<Event> events = EddiJournalMonitor.JournalMonitor.ParseJournalEntry(data);

            Assert.IsTrue(events.Count == 1);

            Assert.IsTrue(events[0] is BodyScannedEvent);
            BodyScannedEvent bsEvent = (BodyScannedEvent)events[0];

            Assert.IsNotNull(bsEvent.volcanism);
            Assert.AreEqual("Geysers", bsEvent.volcanism.type);
            Assert.AreEqual("Carbon dioxide", bsEvent.volcanism.composition);
            Assert.IsNull(bsEvent.volcanism.amount);
        }

        [TestMethod]
        public void TestBodyScanPruEurkTXBc135A1()
        {
            string data = @"{ ""TerraformState"": """", ""MassEM"": 1.852702, ""PlanetClass"": ""High metal content body"", ""SurfacePressure"": 28794888.0, ""RotationPeriod"": 14164107.0, ""event"": ""Scan"", ""OrbitalPeriod"": 14146797.0, ""Eccentricity"": 4e-06, ""AtmosphereType"": ""CarbonDioxide"", ""SurfaceTemperature"": 1045.624512, ""TidalLock"": true, ""Periapsis"": 347.254364, ""BodyName"": ""Pru Eurk TX - B c13 - 5 A 1"", ""SemiMajorAxis"": 84379811840.0, ""timestamp"": ""2017 - 02 - 22T10: 53:44Z"", ""Volcanism"": ""minor silicate vapour geysers volcanism"", ""Atmosphere"": ""hot thick carbon dioxide atmosphere"", ""OrbitalInclination"": 0.143376, ""Landable"": false, ""Radius"": 7275987.0, ""DistanceFromArrivalLS"": 281.461151, ""SurfaceGravity"": 13.948622 }";

            List<Event> events = EddiJournalMonitor.JournalMonitor.ParseJournalEntry(data);

            Assert.IsTrue(events.Count == 1);

            Assert.IsTrue(events[0] is BodyScannedEvent);
            BodyScannedEvent bsEvent = (BodyScannedEvent)events[0];

            Assert.IsNotNull(bsEvent.volcanism);
            Assert.AreEqual("Geysers", bsEvent.volcanism.type);
            Assert.AreEqual("Silicate vapour", bsEvent.volcanism.composition);
            Assert.AreEqual("Minor", bsEvent.volcanism.amount);
        }

        [TestMethod]
        public void TestBodyScanPyramoeBBUd426A3()
        {
            string data = @"{ ""TerraformState"": ""Terraformable"", ""MassEM"": 0.445882, ""PlanetClass"": ""High metal content body"", ""SurfacePressure"": 1436615.5, ""RotationPeriod"": 82427.578125, ""event"": ""Scan"", ""OrbitalPeriod"": 88090320.0, ""Eccentricity"": 0.001264, ""AtmosphereType"": ""CarbonDioxide"", ""SurfaceTemperature"": 642.15802, ""TidalLock"": false, ""Periapsis"": 287.768616, ""BodyName"": ""Pyramoe BB-U d4-26 A 3"", ""SemiMajorAxis"": 338928762880.0, ""timestamp"": ""2017-02-22T10:53:46Z"", ""Volcanism"": """", ""Atmosphere"": ""hot thick carbon dioxide atmosphere"", ""OrbitalInclination"": -1.800459, ""Landable"": false, ""Radius"": 4759092.0, ""DistanceFromArrivalLS"": 1130.366821, ""SurfaceGravity"": 7.84659 }";

            List<Event> events = EddiJournalMonitor.JournalMonitor.ParseJournalEntry(data);

            Assert.IsTrue(events.Count == 1);

            Assert.IsTrue(events[0] is BodyScannedEvent);
            BodyScannedEvent bsEvent = (BodyScannedEvent)events[0];

            Assert.IsNull(bsEvent.volcanism);
        }

        [TestMethod]
        public void TestBodyScanKareneriA1()
        {
            string data = @"{ ""TerraformState"": """", ""MassEM"": 0.669502, ""PlanetClass"": ""High metal content body"", ""SurfacePressure"": 600184.6875, ""RotationPeriod"": 158551.375, ""event"": ""Scan"", ""OrbitalPeriod"": 158550.609375, ""Eccentricity"": 0.75061, ""AtmosphereType"": ""SulphurDioxide"", ""SurfaceTemperature"": 517.645813, ""TidalLock"": true, ""Periapsis"": 87.2164, ""BodyName"": ""Kareneri A 1"", ""SemiMajorAxis"": 2612434432, ""timestamp"": ""2017-02-22T10:53:18Z"", ""Volcanism"": ""metallic magma volcanism"", ""Atmosphere"": ""hot thick sulphur dioxide atmosphere"", ""OrbitalInclination"": -15.958412, ""Landable"": false, ""Radius"": 5442029, ""DistanceFromArrivalLS"": 6.102605, ""SurfaceGravity"": 9.010311 }";

            List<Event> events = EddiJournalMonitor.JournalMonitor.ParseJournalEntry(data);

            Assert.IsTrue(events.Count == 1);

            Assert.IsTrue(events[0] is BodyScannedEvent);
            BodyScannedEvent bsEvent = (BodyScannedEvent)events[0];

            Assert.IsNotNull(bsEvent.volcanism);
            Assert.AreEqual("Magma", bsEvent.volcanism.type);
            Assert.AreEqual("Iron", bsEvent.volcanism.composition);
            Assert.IsNull(bsEvent.volcanism.amount);
        }

        [TestMethod]
        public void TestBodyScanVegnaaNLRc194B2c()
        {
            string data = @"{ ""TerraformState"": """", ""MassEM"": 0.00021, ""PlanetClass"": ""Rocky body"", ""SurfacePressure"": 0.0, ""RotationPeriod"": 175479.578125, ""event"": ""Scan"", ""OrbitalPeriod"": 119920.4375, ""Eccentricity"": 0.064521, ""AtmosphereType"": ""SulphurDioxide"", ""SurfaceTemperature"": 155.416016, ""TidalLock"": true, ""Periapsis"": 142.903015, ""BodyName"": ""Vegnaa NL-R c19-4 B 2 c"", ""SemiMajorAxis"": 1799156.125, ""timestamp"": ""2017-02-22T10:53:18Z"", ""Volcanism"": ""major rocky magma volcanism"", ""Atmosphere"": """", ""OrbitalInclination"": -3.066051, ""Landable"": false, ""Radius"": 419237.34375, ""DistanceFromArrivalLS"": 141767.375, ""SurfaceGravity"": 0.475953 }";

            List<Event> events = EddiJournalMonitor.JournalMonitor.ParseJournalEntry(data);

            Assert.IsTrue(events.Count == 1);

            Assert.IsTrue(events[0] is BodyScannedEvent);
            BodyScannedEvent bsEvent = (BodyScannedEvent)events[0];

            Assert.IsNotNull(bsEvent.volcanism);
            Assert.AreEqual("Magma", bsEvent.volcanism.type);
            Assert.AreEqual("Silicate", bsEvent.volcanism.composition);
            Assert.AreEqual("Major", bsEvent.volcanism.amount);
        }
    }
}
