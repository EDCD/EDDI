﻿using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Details of a star class
    /// </summary>
    public class StarClass
    {
        private static readonly List<StarClass> STARS = new List<StarClass>();

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
        }

        // Percentages are obtained from a combination of https://en.wikipedia.org/wiki/Stellar_classification#Harvard_spectral_classification
        // and http://physics.stackexchange.com/questions/153150/what-does-this-stellar-mass-distribution-mean

        // Mass distributions

        // Radius distributions

        // Luminosity distributions

        public static readonly StarClass O = new StarClass("O", "O", "blue", 0.0000009M, new Gamma(3, 11), new Gamma(3, 20), new Gamma(12, 5000));
        public static readonly StarClass B = new StarClass("B", "B", "blue-white", 0.039M, new Normal(9.05, 2.32), new Normal(4.2, 0.8), new Normal(15012, 4995));
        public static readonly StarClass A = new StarClass("A", "A", "blue-white", 0.18M, new Normal(1.75, 0.12), new Normal(1.6, 0.667), new Normal(15, 3.333));
        public static readonly StarClass F = new StarClass("F", "F", "white", 0.9M, new Normal(1.22, 0.06), new Normal(1.275, 0.042), new Normal(3.25, 0.583));
        public static readonly StarClass G = new StarClass("G", "G", "yellow-white", 2.28M, new Normal(0.92, 0.04), new Normal(1.055, 0.032), new Normal(1.05, 0.15));
        public static readonly StarClass K = new StarClass("K", "K", "yellow-orange", 3.63M, new Normal(0.625, 0.06), new Normal(0.83, 0.043), new Normal(0.34, 0.087));
        public static readonly StarClass M = new StarClass("M", "M", "orange-red", 22.935M, new Normal(0.265, 0.06), new Gamma(1, 0.25), new Gamma(1, 0.04));

        public double luminosityLikelihood(double l)
        {
            return luminositydistribution.CumulativeDistribution(l);
        }

        public static StarClass FromName(string from)
        {
            StarClass result = STARS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown star name " + from);
            }
            return result;
        }

        public static StarClass FromEDName(string from)
        {
            StarClass result = STARS.FirstOrDefault(v => v.edname == from);
            if (result == null)
            {
                Logging.Report("Unknown star ED name " + from);
            }
            return result;
        }

        /// <summary>
        /// Convert radius in m in to stellar radius
        /// </summary>
        public static decimal stellarradius(decimal radius)
        {
            return radius / 695500000;
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
