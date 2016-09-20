using MathNet.Numerics.Distributions;
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

        public decimal percentage { get; private set; }

        public IDistribution massdistribution { get; private set; }

        public IDistribution radiusdistribution { get; private set; }

        public IDistribution luminositydistribution { get; private set; }

        private StarClass(string edname, string name, decimal percentage, IDistribution massdistribution, IDistribution radiusdistribution, IDistribution luminositydistribution)
        {
            this.edname = edname;
            this.name = name;
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

        public static readonly StarClass O = new StarClass("O", "O", 0.0000009M, new Normal(10, 10), new Normal(10, 10), new Normal(10, 10));
        public static readonly StarClass B = new StarClass("B", "B", 0.039M, new Normal(10, 10), new Normal(10, 10), new Normal(10, 10));
        public static readonly StarClass A = new StarClass("A", "A", 0.18M, new Normal(10, 10), new Normal(10, 10), new Normal(10, 10));
        public static readonly StarClass F = new StarClass("F", "F", 0.9M, new Normal(10, 10), new Normal(10, 10), new Normal(10, 10));
        public static readonly StarClass G = new StarClass("G", "G", 2.28M, new Normal(10, 10), new Normal(10, 10), new Normal(10, 10));
        public static readonly StarClass K = new StarClass("K", "K", 3.63M, new Normal(10, 10), new Normal(10, 10), new Normal(10, 10));
        public static readonly StarClass M = new StarClass("M", "M", 22.935M, new Normal(10, 10), new Normal(10, 10), new Normal(10, 10));

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
    }
}
