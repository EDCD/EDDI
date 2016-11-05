using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public IUnivariateDistribution luminositydistribution { get; private set; }

        private StarClass(string edname, string name, string chromaticity, decimal percentage, IUnivariateDistribution massdistribution, IUnivariateDistribution radiusdistribution, IUnivariateDistribution luminositydistribution)
        {
            this.edname = edname;
            this.name = name;
            this.chromaticity = chromaticity;
            this.percentage = percentage;
            this.massdistribution = massdistribution;
            this.radiusdistribution = radiusdistribution;
            this.luminositydistribution = luminositydistribution;

            CLASSES.Add(this);
        }

        // Percentages are obtained from a combination of https://en.wikipedia.org/wiki/Stellar_classification#Harvard_spectral_classification
        // and http://physics.stackexchange.com/questions/153150/what-does-this-stellar-mass-distribution-mean

        public static readonly StarClass O = new StarClass("O", "O", "blue", 0.0000009M, new Gamma(3, 1/11.0), new Gamma(3, 1/20.0), new Gamma(9, 1/5000.0));
        public static readonly StarClass B = new StarClass("B", "B", "blue-white", 0.039M, new Normal(9.05, 6.96), new Normal(4.2, 2.4), new Normal(15012, 4995));
        public static readonly StarClass A = new StarClass("A", "A", "blue-white", 0.18M, new Normal(1.75, 0.36), new Normal(1.6, 2), new Normal(15, 19));
        public static readonly StarClass F = new StarClass("F", "F", "white", 0.9M, new Normal(1.22, 0.18), new Normal(1.275, 0.126), new Normal(3.25, 1.749));
        public static readonly StarClass G = new StarClass("G", "G", "yellow-white", 2.28M, new Normal(0.92, 0.12), new Normal(1.055, 0.15), new Normal(1.05, 0.45));
        public static readonly StarClass K = new StarClass("K", "K", "yellow-orange", 3.63M, new Normal(0.625, 0.18), new Normal(0.83, 0.129), new Normal(0.34, 0.261));
        public static readonly StarClass M = new StarClass("M", "M", "orange-red", 22.935M, new Normal(0.265, 0.18), new Gamma(1, 1/0.25), new Gamma(1, 1/0.04));

        /// <summary>
        /// Provide the cumulative probability that a star of this class will have a luminosity equal to or lower than that supplied
        /// </summary>
        public decimal luminosityCP(decimal l)
        {
            return (decimal)luminositydistribution.CumulativeDistribution((double)l);
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
            StarClass result = CLASSES.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown star name " + from);
            }
            return result;
        }

        public static StarClass FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }
            StarClass result = CLASSES.FirstOrDefault(v => v.edname == from);
            if (result == null)
            {
                Logging.Report("Unknown star ED name " + from);
            }
            return result;
        }

        /// <summary>
        /// Convert radius in m in to stellar radius
        /// </summary>
        public static decimal solarradius(decimal radius)
        {
            return radius / 695700000;
        }

        /// <summary>
        /// Convert absolute magnitude in to luminosity
        /// </summary>
        public static decimal luminosity(decimal absoluteMagnitude)
        {
            double solAbsoluteMagnitude = 4.83;

            return (decimal)Math.Pow(Math.Pow(100, 0.2), (solAbsoluteMagnitude - (double)absoluteMagnitude));
        }

        public static decimal temperature(decimal luminosity, decimal radius)
        {
            double solLuminosity = 3.846e26;
            double stefanBoltzmann = 5.670367e-8;

            return (decimal)Math.Pow(((double)luminosity * solLuminosity) / (4 * Math.PI * Math.Pow((double)radius, 2) * stefanBoltzmann), 0.25);
        }
    }
}
