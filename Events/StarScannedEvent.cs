using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class StarScannedEvent : Event
    {
        public const string NAME = "Star scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a stellar body";
        public static string SAMPLE = "{ \"timestamp\":\"2018-12-01T08:04:24Z\", \"event\":\"Scan\", \"ScanType\":\"AutoScan\", \"BodyName\":\"Arietis Sector UJ-Q b5-2\", \"BodyID\":0, \"DistanceFromArrivalLS\":0.000000, \"StarType\":\"L\", \"StellarMass\":0.218750, \"Radius\":249075072.000000, \"AbsoluteMagnitude\":11.808075, \"Age_MY\":10020, \"SurfaceTemperature\":1937.000000, \"Luminosity\":\"V\", \"RotationPeriod\":119097.164063, \"AxialTilt\":0.000000 }";

        // Scan value calculation constants
        public const double dssDivider = 2.4;
        public const double scanDivider = 66.25;

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
            VARIABLES.Add("distance", "The distance in LS from the main star");
            VARIABLES.Add("distancefromarrival", "The distance in LS from the main star (old - do not use... preserved for compatibility)");
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
            VARIABLES.Add("mainstar", "True if the star is the main / primary star in the star system");
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

        public decimal distance { get; private set; }  // Object property matches the BodyDetails() function

        [Obsolete("Preserved for compatibility with older Cottle scripts only. Use `distance` instead.")]
        public decimal distancefromarrival => distance;

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

        public string scantype { get; private set; } // One of AutoScan, Basic, Detailed, NavBeacon, NavBeaconDetail
        // AutoScan events are detailed scans triggered via proximity. 

        public bool mainstar { get; private set; }

        public StarScannedEvent(DateTime timestamp, string scantype, string name, string stellarclass, decimal solarmass, decimal radiusKm, decimal absolutemagnitude, string luminosityclass, long ageMegayears, decimal temperatureKelvin, decimal distanceLs, decimal? orbitalperiod, decimal rotationperiod, decimal? semimajoraxis, decimal? eccentricity, decimal? orbitalinclination, decimal? periapsis, List<Ring> rings, bool mainstar) : base(timestamp, NAME)
        {
            this.scantype = scantype;
            this.name = name;
            this.stellarclass = stellarclass;
            this.solarmass = solarmass;
            this.radius = radiusKm;
            this.absolutemagnitude = absolutemagnitude;
            this.luminosityclass = luminosityclass;         
            this.age = ageMegayears;
            this.temperature = temperatureKelvin;
            this.distance = distanceLs;
            this.orbitalperiod = orbitalperiod;
            this.rotationperiod = rotationperiod;
            this.semimajoraxis = semimajoraxis;
            this.eccentricity = eccentricity;
            this.orbitalinclination = orbitalinclination;
            this.periapsis = periapsis;
            this.rings = rings;
            solarradius = StarClass.solarradius(radiusKm);
            luminosity = StarClass.luminosity(absolutemagnitude);  
            StarClass starClass = StarClass.FromName(this.stellarclass);
            if (starClass != null)
            {
                massprobability = StarClass.sanitiseCP(starClass.stellarMassCP(solarmass));
                radiusprobability = StarClass.sanitiseCP(starClass.stellarRadiusCP(this.solarradius));
                tempprobability = StarClass.sanitiseCP(starClass.tempCP(this.temperature));
                ageprobability = StarClass.sanitiseCP(starClass.ageCP(this.age));
                chromaticity = starClass.chromaticity.localizedName;
            }
            if (radiusKm != 0 && temperatureKelvin != 0)
            {
                // Minimum estimated single-star habitable zone (target black body temperature of 315°K / 42°C / 107°F or less)
                estimatedhabzoneinner = StarClass.DistanceFromStarForTemperature(StarClass.maxHabitableTempKelvin, Convert.ToDouble(radiusKm), Convert.ToDouble(temperatureKelvin));

                // Maximum estimated single-star habitable zone (target black body temperature of 223.15°K / -50°C / -58°F or more)
                estimatedhabzoneouter = StarClass.DistanceFromStarForTemperature(StarClass.minHabitableTempKelvin, Convert.ToDouble(radiusKm), Convert.ToDouble(temperatureKelvin));
            }
            estimatedvalue = estimateValue(scantype != null ? scantype.Contains("Detail") : false);
            this.mainstar = mainstar;
        }

        private long? estimateValue(bool detailedScan)
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

            if (detailedScan == false)
            {
                value = value / dssDivider;
            }

            return (long?)Math.Round(value, 0);
        }
    }
}
