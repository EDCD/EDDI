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
        // public static string SAMPLE = "{ \"timestamp\":\"2016-10-05T10:13:55Z\", \"event\":\"Scan\", \"BodyName\":\"Col 285 Sector RS-K c8-5 A\", \"DistanceFromArrivalLS\":0.000000, \"StarType\":\"TTS\", \"StellarMass\":0.449219, \"Radius\":458926400.000000, \"AbsoluteMagnitude\":8.287720, \"Age_MY\":51, \"SurfaceTemperature\":3209.000000, \"Luminosity\":\"Va\", \"SemiMajorAxis\":352032544.000000, \"Eccentricity\":0.027010, \"OrbitalInclination\":74.195038, \"Periapsis\":330.750244, \"OrbitalPeriod\":36441.519531, \"RotationPeriod\":203102.843750 }";
        public static string SAMPLE = "{ \"timestamp\":\"2017-08-28T01:06:03Z\", \"event\":\"Scan\", \"BodyName\":\"LFT 926 B\", \"DistanceFromArrivalLS\":353.886200, \"StarType\":\"L\", \"StellarMass\":0.121094, \"Radius\":202889536.000000, \"AbsoluteMagnitude\":12.913437, \"Age_MY\":9828, \"SurfaceTemperature\":1664.000000, \"Luminosity\":\"V\", \"SemiMajorAxis\":78877065216.000000, \"Eccentricity\":0.037499, \"OrbitalInclination\":33.005280, \"Periapsis\":338.539429, \"OrbitalPeriod\":30585052.000000, \"RotationPeriod\":91694.914063, \"AxialTilt\":0.000000, \"Rings\":[ { \"Name\":\"LFT 926 B A Belt\", \"RingClass\":\"eRingClass_MetalRich\", \"MassMT\":1.4034e+13, \"InnerRad\":3.24e+08, \"OuterRad\":1.1938e+09 } ] }";

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
            VARIABLES.Add("luminosityclass", "The luminosity class of the star that has been scanned");            
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
        
        public string luminosityclass { get; private set; }

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

        public StarScannedEvent(DateTime timestamp, string name, string stellarclass, decimal solarmass, decimal radius, decimal absolutemagnitude, string luminosityclass, long age, decimal temperature, decimal distancefromarrival, decimal? orbitalperiod, decimal rotationperiod, decimal? semimajoraxis, decimal? eccentricity, decimal? orbitalinclination, decimal? periapsis, List<Ring> rings) : base(timestamp, NAME)
        {
            this.name = name;
            this.stellarclass = stellarclass;
            this.solarmass = solarmass;
            this.radius = radius;
            this.absolutemagnitude = absolutemagnitude;
            this.luminosityclass = luminosityclass;         
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
