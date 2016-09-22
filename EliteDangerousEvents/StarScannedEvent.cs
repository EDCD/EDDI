using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousEvents
{
    public class StarScannedEvent : Event
    {
        public const string NAME = "Star scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a stellar body";
        //public static StarScannedEvent SAMPLE = new StarScannedEvent(DateTime.Now, "Alnitak", "O", 26.621094M, 2305180672M, -5.027969M, 80000000, 30725);
        public static StarScannedEvent SAMPLE = new StarScannedEvent(DateTime.Now, "Col 285 Sector ZK-E c12-8", "K", 0.867188M, 687434496, 5.697311M, 8868000000, 4760);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StarScannedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-07-25T10:02:38Z\",\"event\":\"Scan\",\"BodyName\":\"Alnitak\",\"DistanceFromArrivalLS\":0.000000,\"StarType\":\"O\",\"StellarMass\":26.621094,\"Radius\":2305180672.000000,\"AbsoluteMagnitude\":-5.027969,\"OrbitalPeriod\":5755731.500000,\"RotationPeriod\":90114.937500,\"Age\":800000000,\"Temperature\":26000}";

            VARIABLES.Add("name", "The name of the star that has been scanned");
            VARIABLES.Add("chromaticity", "The apparent colour of the star that has been scanned");
            VARIABLES.Add("stellarclass", "The stellar class of the star that has been scanned (O, G, etc)");
            VARIABLES.Add("solarmass", "The mass of the star that has been scanned, relative to Sol's mass");
            VARIABLES.Add("massprobability", "The probablility of finding a star of this class and at least this mass");
            VARIABLES.Add("radius", "The radius of the star that has been scanned, in metres");
            VARIABLES.Add("solarradius", "The radius of the star that has been scanned, compared to Sol");
            VARIABLES.Add("radiusprobability", "The probablility of finding a star of this class and at least this radius");
            VARIABLES.Add("absolutemagnitude", "The absolute magnitude of the star that has been scanned");
            VARIABLES.Add("luminosity", "The luminosity of the star that has been scanned");
            VARIABLES.Add("luminosityprobability", "The probablility of finding a star of this class and at least this luminosity");
            VARIABLES.Add("age", "The age of the star that has been scanned, in years");
            VARIABLES.Add("temperature", "The temperature of the star that has been scanned");
        }

        public string name { get; private set; }

        public string stellarclass { get; private set; }

        public decimal solarmass{ get; private set; }

        public decimal massprobability { get; private set; }

        public decimal radius { get; private set; }

        public decimal solarradius { get; private set; }

        public decimal radiusprobability { get; private set; }

        public decimal absolutemagnitude { get; private set; }

        public decimal luminosity { get; private set; }

        public decimal luminosityprobability { get; private set; }

        public long age { get; private set; }

        public decimal temperature { get; private set; }

        public string chromaticity { get; private set; }

        public StarScannedEvent(DateTime timestamp, string name, string stellarclass, decimal solarmass, decimal radius, decimal absolutemagnitude, long age, decimal temperature) : base(timestamp, NAME)
        {
            this.name = name;
            this.stellarclass = stellarclass;
            this.solarmass = solarmass;
            this.radius = radius;
            this.absolutemagnitude = absolutemagnitude;
            this.age = age;
            this.temperature = temperature;
            StarClass starClass = StarClass.FromName(this.stellarclass);
            if (starClass != null)
            {
                this.massprobability = sanitiseCP(starClass.stellarMassCP(solarmass));
                this.solarradius = StarClass.solarradius(radius);
                this.radiusprobability = sanitiseCP(starClass.stellarRadiusCP(this.solarradius));
                this.luminosity = StarClass.luminosity(absolutemagnitude);
                Logging.Info("*******************************************************************************");
                Logging.Info("Luminosity is " + this.luminosity);
                this.luminosityprobability = sanitiseCP(starClass.luminosityCP(this.luminosity));
                Logging.Info("Luminosity probability is " + this.luminosityprobability);
                Logging.Info("Temperature is " + StarClass.temperature(luminosity, this.radius));
                // TODO remove when temperature is in journal
                this.temperature = StarClass.temperature(luminosity, this.radius);
                this.chromaticity = starClass.chromaticity;
            }

        }

        private decimal sanitiseCP(decimal cp)
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
