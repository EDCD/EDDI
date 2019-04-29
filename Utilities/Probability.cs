using MathNet.Numerics.Distributions;
using System;

namespace Utilities
{
    public class Probability
    {
        /// <summary> Provide the cumulative probability that a value will be equal to or lower than that supplied </summary>
        public static decimal? CumulativeP(IUnivariateDistribution distribution, decimal? val)
        {
            return val == null || distribution == null ? null : sanitiseCumulativeP((decimal?)distribution.CumulativeDistribution((double)val));
        }

        /// <summary> Sanitizes cumulative probability & trim decimal places </summary>
        private static decimal? sanitiseCumulativeP(decimal? cp)
        {
            if (cp == null)
            {
                return null;
            }

            // Trim decimal places appropriately
            if (cp < .00001M || cp > .9999M)
            {
                return Math.Round((decimal)cp * 100, 4);
            }
            else if (cp < .0001M || cp > .999M)
            {
                return Math.Round((decimal)cp * 100, 3);
            }
            else if (cp < .001M || cp > .99M)
            {
                return Math.Round((decimal)cp * 100, 2);
            }
            else
            {
                return Math.Round((decimal)cp * 100);
            }
        }
    }
}
