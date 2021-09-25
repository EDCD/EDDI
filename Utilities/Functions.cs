using System;

namespace Utilities
{
    // A collection of common functions used in code
    public class Functions
    {
        /// <summary>The direct distance in light years from one point in space to another.</summary>
        public static decimal? StellarDistanceLy(decimal? x1, decimal? y1, decimal? z1, decimal? x2, decimal? y2, decimal? z2)
        {
            var diffX = x1 - x2;
            var diffY = y1 - y2;
            var diffZ = z1 - z2;

            if (diffX != null && diffY != null && diffZ != null)
            {
                double square(double x) => x * x;
                var distance = Math.Sqrt(square((double)diffX) + square((double)diffY) + square((double)diffZ));
                return (decimal)Math.Round(distance, 2);
            }
            else
            {
                return null;
            }
        }

        /// <summary>The constant bearing in degrees required to travel from one point on the surface of a body to another (results in a longer, straight path).
        /// Follows a ‘rhumb line’ (or loxodrome) path of constant bearing, which crosses all meridians at the same angle.</summary>
        public static decimal? SurfaceConstantHeadingDegrees(decimal? planetRadiusMeters, decimal? currentLatitude, decimal? currentLongitude, decimal? destinationLatitude, decimal? destinationLongitude)
        {
            if (planetRadiusMeters != null && currentLatitude != null && currentLongitude != null && destinationLatitude != null && destinationLongitude != null)
            {
                // Convert latitude & longitude to radians
                double lat1 = (double)currentLatitude * Math.PI / 180;
                double lat2 = (double)destinationLatitude * Math.PI / 180;
                double deltaLong = (double)(destinationLongitude - currentLongitude) * Math.PI / 180;

                // if deltaLong is over 180°, take the shorter rhumb line across the anti-meridian
                if (Math.Abs(deltaLong) > Math.PI)
                {
                    deltaLong = deltaLong > 0 ? -(2*Math.PI - deltaLong) : (2*Math.PI + deltaLong);
                }

                // Calculate heading using Law of Haversines
                double projectedDeltaLat = Math.Log(Math.Tan(Math.PI/4 + lat2/2) / Math.Tan(Math.PI/4 + lat1/2));
                var headingRadians = Math.Atan2(deltaLong, projectedDeltaLat);
                var headingDegrees = (decimal)(headingRadians * 180 / Math.PI);
                while (headingDegrees < 0) { headingDegrees += 360; }
                while (headingDegrees > 360) { headingDegrees -= 360; }
                return headingDegrees;
            }
            else
            {
                return null;
            }
        }

        /// <summary>The distance traveled when following a constant bearing from one point on the surface of a body to another (results in a longer, straight path).
        /// Follows a ‘rhumb line’ (or loxodrome) path of constant bearing, which crosses all meridians at the same angle.</summary>
        public static decimal? SurfaceConstantHeadingDistanceKm(decimal? planetRadiusMeters, decimal? currentLatitude, decimal? currentLongitude, decimal? destinationLatitude, decimal? destinationLongitude)
        {
            if (planetRadiusMeters != null && currentLatitude != null && currentLongitude != null && destinationLatitude != null && destinationLongitude != null)
            {
                double square(double x) => x * x;

                // Convert latitude & longitude to radians
                double lat1 = (double)currentLatitude * Math.PI / 180;
                double lat2 = (double)destinationLatitude * Math.PI / 180;
                double deltaLat = lat2 - lat1;
                double deltaLong = (double)(destinationLongitude - currentLongitude) * Math.PI / 180;

                // if deltaLong is over 180°, take the shorter rhumb line across the anti-meridian
                if (Math.Abs(deltaLong) > Math.PI)
                {
                    deltaLong = deltaLong > 0 ? -(2 * Math.PI - deltaLong) : (2 * Math.PI + deltaLong);
                }

                // Calculate straight path distance using Law of Haversines
                double projectedDeltaLat = Math.Log(Math.Tan(Math.PI / 4 + lat2 / 2) / Math.Tan(Math.PI / 4 + lat1 / 2));
                double q = Math.Abs(projectedDeltaLat) > 10E-12 ? deltaLat / projectedDeltaLat : Math.Cos(lat1); // // E-W course becomes ill-conditioned with 0/0
                var distanceKm = (decimal)Math.Sqrt(square(deltaLat) + square(q) * square(deltaLong)) * planetRadiusMeters / 1000;
                return distanceKm;
            }
            else
            {
                return null;
            }
        }

        /// <summary>The projected latitude and longitude of a point on the surface of a body, when pointing your ship to that location.</summary>
        public static void SurfaceCoordinates(decimal? altitudeMeters, decimal? planetRadiusMeters, decimal? slopeDegrees, decimal? headingDegrees, decimal? currentLatitude, decimal? currentLongitude, out decimal? outputLatitude, out decimal? outputLongitude)
        {
            outputLatitude = null;
            outputLongitude = null;
            if (altitudeMeters != null && planetRadiusMeters != null && slopeDegrees != null && headingDegrees != null && currentLatitude != null && currentLongitude != null)
            {
                // Normalize slope to between 0 and 360°
                while (slopeDegrees < 0) { slopeDegrees += 360; }
                while (slopeDegrees > 360) { slopeDegrees -= 360; }
                
                // Convert latitude, longitude & slope to radians
                double currLat = (double)currentLatitude * Math.PI / 180;
                double currLong = (double)currentLongitude * Math.PI / 180;
                double slopeRadians = (double)slopeDegrees * Math.PI / 180;
                double altitudeKm = (double)altitudeMeters / 1000;

                // Determine minimum slope
                double radiusKm = (double)planetRadiusMeters / 1000;
                double minSlopeRadians = Math.Acos(radiusKm / (altitudeKm + radiusKm));
                if (slopeRadians > minSlopeRadians)
                {
                    // Calculate the orbital cruise 'point to' position using Laws of Sines & Haversines 
                    double a = Math.PI / 2 - slopeRadians;
                    double path = altitudeKm / Math.Cos(a);
                    double c = Math.Asin(path * Math.Sin(a) / radiusKm);
                    double heading = (double)headingDegrees * Math.PI / 180;
                    double Lat = Math.Asin(Math.Sin(currLat) * Math.Cos(c) + Math.Cos(currLat) * Math.Sin(c) * Math.Cos(heading));
                    double Lon = currLong + Math.Atan2(Math.Sin(heading) * Math.Sin(c) * Math.Cos(Lat),
                        Math.Cos(c) - Math.Sin(currLat) * Math.Sin(Lat));

                    // Convert position to degrees
                    outputLatitude = (decimal)Math.Round(Lat * 180 / Math.PI, 4);
                    outputLongitude = (decimal)Math.Round(Lon * 180 / Math.PI, 4);
                }
            }
        }

        /// <summary>The spherical distance from one point on the surface of a body to another. Altitude is not considered in this calculation.</summary>
        public static decimal? SurfaceDistanceKm(decimal? planetRadiusMeters, decimal? currentLatitude, decimal? currentLongitude, decimal? destinationLatitude, decimal? destinationLongitude)
        {
            if (planetRadiusMeters != null && currentLatitude != null && currentLongitude != null && destinationLatitude != null && destinationLongitude != null)
            {
                double square(double x) => x * x;
                double radiusKm = (double)planetRadiusMeters / 1000;

                // Convert latitude & longitude to radians
                double lat1 = (double)currentLatitude * Math.PI / 180;
                double lat2 = (double)destinationLatitude * Math.PI / 180;
                double deltaLat = lat2 - lat1;
                double deltaLong = (double)(destinationLongitude - currentLongitude) * Math.PI / 180;

                // Calculate shortest path distance using Law of Haversines
                double a = square(Math.Sin(deltaLat / 2)) + Math.Cos(lat2) * Math.Cos(lat1) * square(Math.Sin(deltaLong / 2));
                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distanceKm = (decimal)(c * radiusKm);
                return distanceKm;
            }
            else
            {
                return null;
            }
        }

        /// <summary>The initial bearing in degrees required to travel from one point on the surface of a body to another.</summary>
        public static decimal? SurfaceHeadingDegrees(decimal? currentLatitude, decimal? currentLongitude, decimal? destinationLatitude, decimal? destinationLongitude)
        {
            if (currentLatitude != null && currentLongitude != null && destinationLatitude != null && destinationLongitude != null)
            {
                // Convert latitude & longitude to radians
                double lat1 = (double)currentLatitude * Math.PI / 180;
                double lat2 = (double)destinationLatitude * Math.PI / 180;
                double deltaLong = (double)(destinationLongitude - currentLongitude) * Math.PI / 180;

                // Calculate heading using Law of Haversines
                double y = Math.Sin(deltaLong) * Math.Cos(lat2);
                double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLong);
                var headingRadians = Math.Atan2(y, x);
                var headingDegrees = (decimal)(headingRadians * 180 / Math.PI);
                while (headingDegrees < 0) { headingDegrees += 360; }
                while (headingDegrees > 360) { headingDegrees -= 360; }
                return headingDegrees;
            }
            else
            {
                return null;
            }
        }
    }
}
