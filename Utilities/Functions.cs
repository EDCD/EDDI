using System;

namespace Utilities
{
    // A collection of common functions used in code
    public class Functions
    {
        public static decimal DistanceFromCoordinates(decimal x1, decimal y1, decimal z1, decimal x2, decimal y2, decimal z2)
        {
            double square(double x) => x * x;
            var diffX = (double)(x1 - x2);
            var diffY = (double)(y1 - y2);
            var diffZ = (double)(z1 - z2);
            var distance = Math.Sqrt(square(diffX) + square(diffY) + square(diffZ));
            return (decimal)Math.Round(distance, 2);
        }
    }
}
