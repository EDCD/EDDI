using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiEvents
{
    public class StarScannedEvent : Event
    {
        public const string NAME = "Star scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a stellar body";
        public static string SAMPLE = "{\"timestamp\":\"2016-09-22T20:39:26Z\",\"event\":\"Scan\",\"BodyName\":\"Col 285 Sector ZK-E c12-8\",\"DistanceFromArrivalLS\":0.000000,\"StarType\":\"K\",\"StellarMass\":0.867188,\"Radius\":687434496.000000,\"AbsoluteMagnitude\":5.697311,\"OrbitalPeriod\":0.000000, \"RotationPeriod\":406112.093750, \"Rings\":[ {\"Name\":\"Col 285 Sector ZK-E c12-8 A Belt\",\"RingClass\":\"eRingClass_MetalRich\", \"MassMT\":1.18e+14, \"InnerRad\":1.24e+09, \"OuterRad\":2.3e+09 }, { \"Name\":\"Col 285 Sector ZK-E c12-8 B Belt\", \"RingClass\":\"eRingClass_MetalRich\", \"MassMT\":4.45e+15, \"InnerRad\":1.25e+10, \"OuterRad\":5.04e+11 } ] }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StarScannedEvent()
        {
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
            VARIABLES.Add("distancefromarrival", "The distance in LS from the main star");
            VARIABLES.Add("orbitalperiod", "The number of seconds taken for a full orbit of the main star");
            VARIABLES.Add("rotationperiod", "The number of seconds taken for a full rotation");
            VARIABLES.Add("rings", "The star's rings");
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

        public decimal distancefromarrival { get; private set; }

        public decimal orbitalperiod { get; private set; }

        public decimal rotationperiod { get; private set; }

        public List<Ring> rings { get; private set; }

        public StarScannedEvent(DateTime timestamp, string name, string stellarclass, decimal solarmass, decimal radius, decimal absolutemagnitude, long age, decimal temperature, decimal distancefromarrival, decimal orbitalperiod, decimal rotationperiod, List<Ring> rings) : base(timestamp, NAME)
        {
            this.name = name;
            this.stellarclass = stellarclass;
            this.solarmass = solarmass;
            this.radius = radius;
            this.absolutemagnitude = absolutemagnitude;
            this.age = age;
            this.temperature = temperature;
            this.distancefromarrival = distancefromarrival;
            this.orbitalperiod = orbitalperiod;
            this.rotationperiod = rotationperiod;
            this.rings = rings;
            StarClass starClass = StarClass.FromName(this.stellarclass);
            if (starClass != null)
            {
                this.massprobability = sanitiseCP(starClass.stellarMassCP(solarmass));
                this.solarradius = StarClass.solarradius(radius);
                this.radiusprobability = sanitiseCP(starClass.stellarRadiusCP(this.solarradius));
                this.luminosity = StarClass.luminosity(absolutemagnitude);
                this.luminosityprobability = sanitiseCP(starClass.luminosityCP(this.luminosity));
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
