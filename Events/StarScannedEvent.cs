using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class StarScannedEvent : Event
    {
        public const string NAME = "Star scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a stellar body";
        public static string SAMPLE = "{ \"timestamp\":\"2018-12-01T08:04:24Z\", \"event\":\"Scan\", \"ScanType\":\"AutoScan\", \"BodyName\":\"Arietis Sector UJ-Q b5-2\", \"BodyID\":0, \"DistanceFromArrivalLS\":0.000000, \"StarType\":\"L\", \"StellarMass\":0.218750, \"Radius\":249075072.000000, \"AbsoluteMagnitude\":11.808075, \"Age_MY\":10020, \"SurfaceTemperature\":1937.000000, \"Luminosity\":\"V\", \"RotationPeriod\":119097.164063, \"AxialTilt\":0.000000 }";

        // Variable names for this event should match the class property names for maximum compatibility with the BodyDetails() function in Cottle

        [PublicAPI("The absolute (bolometric) magnitude of the star that has been scanned")]
        public decimal? absolutemagnitude => star.absolutemagnitude;

        [PublicAPI("The cumulative probability describing the star's age, relative to other stars of the same stellar class")]
        public decimal? absolutemagnitudeprobability => star.absolutemagnitudeprobability;

        [PublicAPI("The age of the star that has been scanned, in millions of years")]
        public long? age => star.age;

        [PublicAPI("The probablility of finding a star of this class with this age")]
        public decimal? ageprobability => star.ageprobability;

        [PublicAPI("Whether this star's scan data has already been registered with Universal Cartographics")]
        public bool? alreadydiscovered => star.alreadydiscovered;

        [PublicAPI("The name of the star that has been scanned")]
        public string bodyname => star.bodyname;

        [PublicAPI("The apparent colour of the star that has been scanned")]
        public string chromaticity => star.chromaticity;

        [PublicAPI("The average density of the star, in kg per cubic meter")]
        public decimal? density => star.density;

        [PublicAPI("The cumulative probability describing the star's density, relative to other stars of the same stellarclass")]
        public decimal? densityprobability => star.densityprobability;

        [PublicAPI("The distance in LS from the main star")]
        public decimal? distance => star.distance;

        [PublicAPI("The orbital eccentricity of the star")]
        public decimal? eccentricity => star.eccentricity;

        [PublicAPI("The cumulative probability describing the star's orbital eccentricity, relative to other stars of the same stellar class")]
        public decimal? eccentricityprobability => star.eccentricityprobability;

        [PublicAPI("The estimated inner radius of the habitable zone of the scanned star, in light seconds, not considering other stars in the system")]
        public decimal? estimatedhabzoneinner => star.estimatedhabzoneinner;

        [PublicAPI("The estimated outer radius of the habitable zone of the scanned star, in light seconds, not considering other stars in the system")]
        public decimal? estimatedhabzoneouter => star.estimatedhabzoneouter;

        [PublicAPI("The estimated value of the current scan")]
        public long? estimatedvalue => star.estimatedvalue;

        [PublicAPI("The orbital inclination of the star, in degrees")]
        public decimal? inclination => star.inclination;

        [PublicAPI("The cumulative probability describing the star's orbital inclination, relative to other stars of the same stellar class")]
        public decimal? inclinationprobability => star.inclinationprobability;

        [PublicAPI("The luminosity of the star that has been scanned")]
        public decimal? luminosity => star.luminosity;

        [PublicAPI("The luminosity class of the star that has been scanned")]
        public string luminosityclass => star.luminosityclass;

        [PublicAPI("True if the star is the main / primary star in the star system")]
        public bool? mainstar => star.mainstar;

        [PublicAPI("The probablility of finding a star of this class with this mass")]
        public decimal? massprobability => star.massprobability;

        [PublicAPI("The number of seconds taken for a full orbit of the main star")]
        public decimal? orbitalperiod => star.orbitalperiod;

        [PublicAPI("The cumulative probability describing the star's orbital period about the main star, relative to other stars of the same stellar class")]
        public decimal? orbitalperiodprobability => star.orbitalperiodprobability;

        [PublicAPI("The argument of periapsis of the star, in degrees")]
        public decimal? periapsis => star.periapsis;

        [PublicAPI("The cumulative probability describing the stars's argument of periapsis, relative to other stars of the same stellar class")]
        public decimal? periapsisprobability => star.periapsisprobability;

        [PublicAPI("The radius of the star that has been scanned, in metres")]
        public decimal? radius => star.radius;

        [PublicAPI("The probablility of finding a star of this class with this radius")]
        public decimal? radiusprobability => star.radiusprobability;

        [PublicAPI("The star's rings")]
        public List<Ring> rings => star.rings;

        [PublicAPI("The number of seconds taken for a full rotation")]
        public decimal? rotationalperiod => star.rotationalperiod;

        [PublicAPI("The cumulative probability describing the stars's rotational period, relative to other stars of the same stellar class")]
        public decimal? rotationalperiodprobability => star.rotationalperiodprobability;

        [PublicAPI("The type of scan event (AutoScan, Basic, Detailed, NavBeacon, NavBeaconDetail)")]
        public string scantype { get; private set; } // One of AutoScan, Basic, Detailed, NavBeacon, NavBeaconDetail
                                                     // AutoScan events are detailed scans triggered via proximity. 

        [PublicAPI("True if the star is scoopable (K, G, B, F, O, A, M)")]
        public bool scoopable => star.scoopable;

        [PublicAPI("The semi major axis of the star's orbit, in light seconds")]
        public decimal? semimajoraxis => star.semimajoraxis;

        [PublicAPI("The cumulative probability describing the semi-major axis of the orbit of the star, relative to other stars of the same stellar class")]
        public decimal? semimajoraxisprobability => star.semimajoraxisprobability;

        [PublicAPI("The mass of the star that has been scanned, relative to Sol's mass")]
        public decimal? solarmass => star.solarmass;

        [PublicAPI("The radius of the star that has been scanned, compared to Sol")]
        public decimal? solarradius => star.solarradius;

        [PublicAPI("The stellar class of the star that has been scanned (O, G, etc)")]
        public string stellarclass => star.stellarclass;

        [PublicAPI("The stellar sub class of the star that has been scanned (0 - 9, with 0 being hotter and 9 being cooler)")]
        public int? stellarsubclass => star.stellarsubclass;

        [PublicAPI("The temperature of the star that has been scanned")]
        public decimal? temperature => star.temperature;

        [PublicAPI("The probablility of finding a star of this class with this temperature")]
        public decimal? tempprobability => star.tempprobability;

        [PublicAPI("Axial tilt for the star, in degrees (only available if DSS equipped)")]
        public decimal? tilt => star.tilt;

        [PublicAPI("The cumulative probability describing the star's orbital tilt, relative to other stars of the same stellar class")]
        public decimal? tiltprobability => star.tiltprobability;

        // Deprecated, maintained for compatibility with user scripts

        [Obsolete("Use distance instead")]
        public decimal? distancefromarrival => distance;

        [Obsolete("Use bodyname instead")]
        public string name => bodyname;

        [Obsolete("Use inclination instead")]
        public decimal? orbitalinclination => inclination;

        [Obsolete("Use rotationalperiod instead")]
        public decimal? rotationperiod => rotationalperiod;

        // Variables below are not intended to be user facing

        public bool? alreadymapped => star.alreadymapped;

        public long? bodyId => star.bodyId;

        public DateTime? mapped => star.mapped;

        public List<IDictionary<string, object>> parents => star.parents;

        public DateTime? scanned => star.scanned;

        public Body star { get; private set; }

        public StarScannedEvent(DateTime timestamp, string scantype, Body star) : base(timestamp, NAME)
        {
            this.star = star;
            this.scantype = scantype;
        }
    }
}
