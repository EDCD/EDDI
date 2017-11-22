using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class StarScannedEvent : Event
    {
        public const string NAME = "Star scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a stellar body";
        public static string SAMPLE = "{ \"timestamp\":\"2017-08-28T01:06:03Z\", \"event\":\"Scan\", \"BodyName\":\"LFT 926 B\", \"DistanceFromArrivalLS\":353.886200, \"StarType\":\"L\", \"StellarMass\":0.121094, \"Radius\":202889536.000000, \"AbsoluteMagnitude\":12.913437, \"Age_MY\":9828, \"SurfaceTemperature\":1664.000000, \"Luminosity\":\"V\", \"SemiMajorAxis\":78877065216.000000, \"Eccentricity\":0.037499, \"OrbitalInclination\":33.005280, \"Periapsis\":338.539429, \"OrbitalPeriod\":30585052.000000, \"RotationPeriod\":91694.914063, \"AxialTilt\":0.000000, \"Rings\":[ { \"Name\":\"LFT 926 B A Belt\", \"RingClass\":\"eRingClass_MetalRich\", \"MassMT\":1.4034e+13, \"InnerRad\":3.24e+08, \"OuterRad\":1.1938e+09 } ] }";

        // Scan value calculation constants
        public const double dssDivider = 2.4;
        public const double scanDivider = 66.25;

        // Scan habitable zone constants
        public const double maxHabitableTempKelvin = 315;
        public const double minHabitableTempKelvin = 223.15;

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
            VARIABLES.Add("absolutemagnitude", "The absolute (bolometric) magnitude of the star that has been scanned");
            VARIABLES.Add("luminosity", "The luminosity of the star that has been scanned");
            VARIABLES.Add("luminosityclass", "The luminosity class of the star that has been scanned");            
            VARIABLES.Add("tempprobability", "The probablility of finding a star of this class with this temperature");
            VARIABLES.Add("age", "The age of the star that has been scanned, in millions of years");
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
            VARIABLES.Add("estimatedvalue", "The estimated value of the current scan");
            VARIABLES.Add("estimatedhabzoneinner", "The estimated inner radius of the habitable zone of the scanned star, in light seconds, not considering other stars in the system");
            VARIABLES.Add("estimatedhabzoneouter", "The estimated outer radius of the habitable zone of the scanned star, in light seconds, not considering other stars in the system");
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

        public decimal? estimatedhabzoneinner { get; private set; }

        public decimal? estimatedhabzoneouter { get; private set; }

        public long? estimatedvalue { get; private set; }

        public StarScannedEvent(DateTime timestamp, string name, string stellarclass, decimal solarmass, decimal radius, decimal absolutemagnitude, string luminosityclass, long age, decimal temperature, decimal distancefromarrival, decimal? orbitalperiod, decimal rotationperiod, decimal? semimajoraxis, decimal? eccentricity, decimal? orbitalinclination, decimal? periapsis, List<Ring> rings, bool dssEquipped) : base(timestamp, NAME)
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
                if (radius != 0 && temperature != 0)
                {
                    // Minimum estimated single-star habitable zone (target black body temperature of 315°K / 42°C / 107°F or less)
                    estimatedhabzoneinner = StarClass.DistanceFromStarForTemperature(maxHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature));
                    this.estimatedhabzoneinner = estimatedhabzoneinner;

                    // Maximum estimated single-star habitable zone (target black body temperature of 223.15°K / -50°C / -58°F or more)
                    estimatedhabzoneouter = StarClass.DistanceFromStarForTemperature(minHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature));
                    this.estimatedhabzoneouter = estimatedhabzoneouter;
                }
            }
            this.estimatedvalue = estimateValue(dssEquipped);
        }

        private long? estimateValue(bool dssEquipped)
        {
            // Credit to MattG's thread at https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae for scan value formulas
            // 'bodyDataConstant' is a derived constant from MattG's thread for calculating scan values.
            int baseValue = 2880;
            double value;

            // Override constants for specific types of bodies
            if ((stellarclass == "H") || (stellarclass == "N"))
            {
                // Black holes and Neutron stars
                baseValue = 54309;
            }
            else if (stellarclass.StartsWith("D") && (stellarclass.Length <= 3))
            {
                // White dwarves
                baseValue = 33737;
            }

            // Calculate exploration scan values
            value = baseValue + ((double)solarmass * baseValue / scanDivider);

            if (dssEquipped == false)
            {
                value = value / dssDivider;
            }
            
            return (long?)Math.Round(value, 0);
        }
    }
}
