using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details of a star class
    /// </summary>
    public class StarClass
    {
        private static readonly List<StarClass> CLASSES = new List<StarClass>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public string chromaticity { get; private set; }

        public decimal percentage { get; private set; }

        public IUnivariateDistribution massdistribution { get; private set; }

        public IUnivariateDistribution radiusdistribution { get; private set; }

        public IUnivariateDistribution tempdistribution { get; private set; }

        public IUnivariateDistribution agedistribution { get; private set; }

        private StarClass(string edname, string name, string chromaticity, decimal percentage, IUnivariateDistribution massdistribution, IUnivariateDistribution radiusdistribution, IUnivariateDistribution tempdistribution, IUnivariateDistribution agedistribution)
        {
            this.edname = edname;
            this.name = name;
            this.chromaticity = chromaticity;
            this.percentage = percentage;
            this.massdistribution = massdistribution;
            this.radiusdistribution = radiusdistribution;
            this.tempdistribution = tempdistribution;
            this.agedistribution = agedistribution;

            CLASSES.Add(this);
        }

        // Percentages are obtained from a combination of https://en.wikipedia.org/wiki/Stellar_classification#Harvard_spectral_classification
        // and http://physics.stackexchange.com/questions/153150/what-does-this-stellar-mass-distribution-mean

        public static readonly StarClass O = new StarClass("O", "O", "blue", 0.0000009M, new Normal(37.57, 27.50), new Normal(14.52, 29.67), new Normal(49698, 21338), new Normal(138, 262));
        public static readonly StarClass B = new StarClass("B", "B", "blue-white", 0.039M, new Normal(5.81, 4.52), new Normal(3.36, 13.42), new Normal(16478, 6044), new Normal(237, 289));
        public static readonly StarClass A = new StarClass("A", "A", "blue-white", 0.18M, new Normal(1.82, 2.78), new Normal(2.29, 16.63), new Normal(8208, 1179), new Normal(1809, 20152));
        public static readonly StarClass F = new StarClass("F", "F", "white", 0.9M, new Normal(1.30, 0.20), new Normal(1.30, 3.86), new Normal(6743, 531), new Normal(2141, 1662));
        public static readonly StarClass G = new StarClass("G", "G", "yellow-white", 2.28M, new Normal(0.94, 0.13), new Normal(1.01, 0.76), new Normal(5653, 7672), new Normal(4713, 3892));
        public static readonly StarClass K = new StarClass("K", "K", "yellow-orange", 3.63M, new Normal(1.04, 45.55), new Normal(0.94, 2.11), new Normal(4452, 3284), new Normal(6291, 4144));
        public static readonly StarClass M = new StarClass("M", "M", "orange-red", 22.935M, new Normal(0.66, 33.23), new Normal(1.58, 47.11), new Normal(2835, 481), new Normal(6609, 8645));

        /// <summary>
        /// Provide the cumulative probability that a star of this class will have a temp equal to or lower than that supplied
        /// </summary>
        public decimal tempCP(decimal l)
        {
            return (decimal)tempdistribution.CumulativeDistribution((double)l);
        }

        /// <summary>
        /// Provide the cumulative probability that a star of this class will have a temp equal to or lower than that supplied
        /// </summary>
        public decimal ageCP(decimal l)
        {
            return (decimal)agedistribution.CumulativeDistribution((double)l);
        }

        /// <summary>
        /// Provide the cumulative probability that a star of this class will have a stellar radius equal to or lower than that supplied
        /// </summary>
        public decimal stellarRadiusCP(decimal l)
        {
            return (decimal)radiusdistribution.CumulativeDistribution((double)l);
        }

        /// <summary>
        /// Provide the cumulative probability that a star of this class will have a stellar mass equal to or lower than that supplied
        /// </summary>
        public decimal stellarMassCP(decimal l)
        {
            return (decimal)massdistribution.CumulativeDistribution((double)l);
        }

        public static StarClass FromName(string from)
        {
            return CLASSES.FirstOrDefault(v => v.name == from);
        }

        public static StarClass FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }
            return CLASSES.FirstOrDefault(v => v.edname == from);
        }

        /// <summary>
        /// Convert radius in m in to stellar radius
        /// </summary>
        public static decimal solarradius(decimal radius)
        {
            return radius / Constants.solarRadiusMeters;
        }

        /// <summary>
        /// Convert absolute magnitude in to luminosity
        /// </summary>
        public static decimal luminosity(decimal absoluteMagnitude)
        {
            return (decimal)Math.Pow(Math.Pow(100, 0.2), (Constants.solAbsoluteMagnitude - (double)absoluteMagnitude));
        }

        public static decimal temperature(decimal luminosity, decimal radius)
        {
            return (decimal)Math.Pow(((double)luminosity * Constants.solLuminosity) / 
                (4 * Math.PI * Math.Pow((double)radius, 2) * Constants.stefanBoltzmann), 0.25);
        }

        public static decimal DistanceFromStarForTemperature(double targetTempKelvin, double stellarRadiusMeters, double stellarTemperatureKelvin)
        {
            // Derived from Jackie Silver's Habitable Zone Calculator (https://forums.frontier.co.uk/showthread.php?t=127522&highlight=), used with permission
            double top = Math.Pow(stellarRadiusMeters, 2.0) * Math.Pow(stellarTemperatureKelvin, 4.0);
            double bottom = 4.0 * Math.Pow(targetTempKelvin, 4.0);
            double distanceMeters = Math.Pow(top / bottom, 0.5);
            double distancels = ( distanceMeters ) / Constants.lightSpeedMetersPerSecond; 
            return Convert.ToDecimal(distancels);
        }

        public static decimal sanitiseCP(decimal cp)
        {
            // Trim decimal places appropriately
            if (cp < .00001M || cp > .9999M)
            {
                return Math.Round(cp * 100, 4);
            }
            else if (cp < .0001M || cp > .999M)
            {
                return Math.Round(cp * 100, 3);
            }
            else if (cp < .001M || cp > .99M)
            {
                return Math.Round(cp * 100, 2);
            }
            else
            {
                return Math.Round(cp * 100);
            }
        }
    }
}
