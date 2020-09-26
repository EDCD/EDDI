using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class UtilityTests : TestBase
    {
        const decimal latitudeNewYork = 40.7128M;
        const decimal longitudeNewYork = -74.0060M;
        const decimal latitudeLondon = 51.5074M;
        const decimal longitudeLondon = -0.1278M;
        const decimal radiusEarthMeters = 6.371009E6M;

        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestDistanceFunctionFromCoordinates()
        {
            // Colonia
            const decimal x1 = -9530.5M;
            const decimal y1 = -910.28125M;
            const decimal z1 = 19808.125M;

            // Hephaestus
            const decimal x2 = -9521.40625M;
            const decimal y2 = -907.3125M;
            const decimal z2 = 19801.65625M;

            var result = Functions.StellarDistanceLy(x1, y1, z1, x2, y2, z2);
            Assert.AreEqual(11.55M, result);
        }

        [TestMethod]
        public void TestDistanceFunctionFromNullCoordinates()
        {
            // Colonia
            const decimal x1 = -9530.5M;
            const decimal y1 = -910.28125M;
            const decimal z1 = 19808.125M;

            var result = Functions.StellarDistanceLy(x1, y1, z1, null, null, null);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestDistanceFunctionFromStarSystems()
        {
            var curr = new StarSystem() { systemname = "Sol", x = 0, y = 0, z = 0 };
            var dest = new StarSystem() { systemname = "Alpha Centauri", x = 3.03125M, y = -0.09375M, z = 3.15625M };

            var result = dest.DistanceFromStarSystem(curr);
            Assert.AreEqual(4.38M, result);
        }

        [TestMethod]
        public void TestDistanceFunctionFromUnknownStarSystems()
        {
            var curr = new StarSystem() { systemname = "Hephaestus", x = -9521.40625M, y = -907.3125M, z = 19801.65625M };
            var dest = new StarSystem() { systemname = "UnknownStar", x = null, y = null, z = null };

            var result = dest.DistanceFromStarSystem(curr);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestDistanceFunctionFromNullStarSystems()
        {
            StarSystem curr = null;
            var dest = new StarSystem() { systemname = "Hephaestus", x = -9521.40625M, y = -907.3125M, z = 19801.65625M };

            var result = dest?.DistanceFromStarSystem(curr);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestSurfaceCoordinatesFunction()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestSurfaceDistanceFunction()
        { 
            // Result from https://www.movable-type.co.uk/scripts/latlong.html: 5570 km
            var surfaceDistanceKm = Functions.SurfaceDistanceKm(radiusEarthMeters, latitudeNewYork, longitudeNewYork, latitudeLondon, longitudeLondon);
            Assert.AreEqual(5570.23004851831M, surfaceDistanceKm);
        }

        [TestMethod]
        public void TestSurfaceConstantHeadingDistanceFunction()
        {
            // Result from https://www.movable-type.co.uk/scripts/latlong.html: 5794 km
            var surfaceDistanceKm = Functions.SurfaceConstantHeadingDistanceKm(radiusEarthMeters, latitudeNewYork, longitudeNewYork, latitudeLondon, longitudeLondon);
            Assert.AreEqual(5794.128935806150181553M, surfaceDistanceKm);
        }

        [TestMethod]
        public void TestSurfaceHeadingFunction()
        {
            // Result from https://www.movable-type.co.uk/scripts/latlong.html: 51.2125 degrees
            var headingDegrees = Functions.SurfaceHeadingDegrees(latitudeNewYork, longitudeNewYork, latitudeLondon, longitudeLondon);
            Assert.AreEqual(51.2126168241972M, headingDegrees);
        }

        [TestMethod]
        public void TestSurfaceConstantHeadingFunction()
        {
            // Result from https://www.movable-type.co.uk/scripts/latlong.html: 78.0441 degrees
            var headingDegrees = Functions.SurfaceConstantHeadingDegrees(radiusEarthMeters, latitudeNewYork, longitudeNewYork, latitudeLondon, longitudeLondon);
            Assert.AreEqual(78.0440808681045M, headingDegrees);
        }

        [TestMethod]
        public void TestDateTimeToString()
        {
            // Parse the DateTime value and test 
            if (DateTime.TryParseExact("2020-07-30T19:16:42Z", "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("fi");
                Assert.AreEqual("2020-07-30T19.16.42Z", date.ToString("yyyy-MM-ddTHH:mm:ssZ"), @"Should yield ""."" rather than "":"" as a time separator, which is invalid for EDDN and other 3rd party services");
                Assert.AreEqual("2020-07-30T19:16:42Z", Dates.FromDateTimeToString(date), @"Should yield a string using the invariant culture, including the invariant "":"" time separator");
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            }
            else
            {
                Assert.Fail("Failed to parse our DateTime string");
            }
        }
    }
}
