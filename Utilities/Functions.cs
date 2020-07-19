using System;

namespace Utilities
{
    // A collection of common functions used in code
    public class Functions
    {
        public static decimal? DistanceFromCoordinates(decimal? x1, decimal? y1, decimal? z1, decimal? x2, decimal? y2, decimal? z2)
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
    }
}
