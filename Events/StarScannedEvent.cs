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
        public static string SAMPLE = "{ \"timestamp\":\"2017-08-28T10:56:04Z\", \"event\":\"Scan\", \"BodyName\":\"Crucis Sector PC-V a2-0 A\", \"DistanceFromArrivalLS\":0.000000, \"StarType\":\"M\", \"StellarMass\":0.183594, \"Radius\":239969568.000000, \"AbsoluteMagnitude\":11.577454, \"Age_MY\":6322, \"SurfaceTemperature\":2081.000000, \"Luminosity\":\"VI\", \"SemiMajorAxis\":18455684186112.000000, \"Eccentricity\":0.024318, \"OrbitalInclination\":80.848442, \"Periapsis\":183.522415, \"OrbitalPeriod\":981915533312.000000, \"RotationPeriod\":120296.101563, \"AxialTilt\":0.000000 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StarScannedEvent()
        {
            VARIABLES.Add("name", "The name of the star that has been scanned");
            VARIABLES.Add("chromaticity", "The apparent colour of the star that has been scanned");
            VARIABLES.Add("stellarclass", "The stellar class of the star that has been scanned (O, G, etc)");
            VARIABLES.Add("solarmass", "The mass of the star that has been scanned, relative to Sol's mass");
            VARIABLES.Add("massprobability", "The probablility of finding a star of this class with this mass");
            VARIABLES.Add("radius", "The radius of the star that has been scanned, in metres");
            VARIABLES.Add("solarradius", "The radius of the star that has been scanned, compared to Sol");
            VARIABLES.Add("radiusprobability", "The probablility of finding a star of this class with this radius");
            VARIABLES.Add("absolutemagnitude", "The absolute magnitude of the star that has been scanned");
            VARIABLES.Add("luminosity", "The luminosity of the star that has been scanned");
            VARIABLES.Add("luminosityClass", "The luminosity class of the star that has been scanned");            
            VARIABLES.Add("tempprobability", "The probablility of finding a star of this class with this temperature");
            VARIABLES.Add("age", "The age of the star that has been scanned, in years (rounded to millions of years)");
            VARIABLES.Add("ageprobability", "The probablility of finding a star of this class with this age");
            VARIABLES.Add("temperature", "The temperature of the star that has been scanned");
            VARIABLES.Add("distancefromarrival", "The distance in LS from the main star");
            VARIABLES.Add("orbitalperiod", "The number of seconds taken for a full orbit of the main star");
            VARIABLES.Add("rotationperiod", "The number of seconds taken for a full rotation");
            VARIABLES.Add("semimajoraxis", "");
            VARIABLES.Add("eccentricity", "");
            VARIABLES.Add("orbitalinclination", "");
            VARIABLES.Add("periapsis", "");
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
        
        public string luminosityClass { get; private set; }

        public decimal tempprobability { get; private set; }

        public long age { get; private set; }

        public decimal ageprobability { get; private set; }

        public decimal temperature { get; private set; }

        public string chromaticity { get; private set; }

        public decimal distancefromarrival { get; private set; }

        public decimal? orbitalperiod { get; private set; }

        public decimal rotationperiod { get; private set; }

        public decimal? semimajoraxis { get; private set; }

        public decimal? eccentricity { get; private set; }

        public decimal? orbitalinclination { get; private set; }

        public decimal? periapsis { get; private set; }

        public List<Ring> rings { get; private set; }

        public StarScannedEvent(DateTime timestamp, string name, string stellarclass, decimal solarmass, decimal radius, decimal absolutemagnitude, string luminosityClass, long age, decimal temperature, decimal distancefromarrival, decimal? orbitalperiod, decimal rotationperiod, decimal? semimajoraxis, decimal? eccentricity, decimal? orbitalinclination, decimal? periapsis, List<Ring> rings) : base(timestamp, NAME)
        {
            this.name = name;
            this.stellarclass = stellarclass;
            this.solarmass = solarmass;
            this.radius = radius;
            this.absolutemagnitude = absolutemagnitude;
            this.luminosityClass = luminosityClass;         
            this.age = age;
            this.temperature = temperature;
            this.distancefromarrival = distancefromarrival;
            this.orbitalperiod = orbitalperiod;
            this.rotationperiod = rotationperiod;
            this.semimajoraxis = semimajoraxis;
            this.eccentricity = eccentricity;
            this.orbitalinclination = orbitalinclination;
            this.periapsis = periapsis;
            this.rings = rings;
            solarradius = StarClass.solarradius(radius);
            luminosity = StarClass.luminosity(absolutemagnitude);        
            StarClass starClass = StarClass.FromName(this.stellarclass);
            if (starClass != null)
            {
                massprobability = StarClass.sanitiseCP(starClass.stellarMassCP(solarmass));
                radiusprobability = StarClass.sanitiseCP(starClass.stellarRadiusCP(this.solarradius));
                tempprobability = StarClass.sanitiseCP(starClass.tempCP(this.temperature));
                ageprobability = StarClass.sanitiseCP(starClass.ageCP(this.age));
                chromaticity = starClass.chromaticity;
            }
        }
    }
}
