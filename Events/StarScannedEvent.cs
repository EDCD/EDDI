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
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"":""2024-07-07T00:48:43Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Jarnun A"", ""BodyID"":1, ""Parents"":[ {""Null"":0} ], ""StarSystem"":""Jarnun"", ""SystemAddress"":670685472153, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""M"", ""Subclass"":2, ""StellarMass"":0.421875, ""Radius"":433159264.000000, ""AbsoluteMagnitude"":8.418655, ""Age_MY"":7088, ""SurfaceTemperature"":3205.000000, ""Luminosity"":""Va"", ""SemiMajorAxis"":11117656230926.513672, ""Eccentricity"":0.483479, ""OrbitalInclination"":-4.994122, ""Periapsis"":313.132410, ""OrbitalPeriod"":159647506475.448608, ""AscendingNode"":-123.046538, ""MeanAnomaly"":107.495399, ""RotationPeriod"":194834.763470, ""AxialTilt"":0.000000, ""WasDiscovered"":true, ""WasMapped"":false }",
            @"{ ""timestamp"":""2024-07-06T23:20:47Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Clota 5"", ""BodyID"":25, ""Parents"":[ {""Star"":0} ], ""StarSystem"":""Clota"", ""SystemAddress"":3657265189586, ""DistanceFromArrivalLS"":2795.784839, ""StarType"":""Y"", ""Subclass"":3, ""StellarMass"":0.015625, ""Radius"":47013316.000000, ""AbsoluteMagnitude"":21.613907, ""Age_MY"":8306, ""SurfaceTemperature"":468.000000, ""Luminosity"":""V"", ""SemiMajorAxis"":815024501085.281372, ""Eccentricity"":0.029346, ""OrbitalInclination"":-0.085222, ""Periapsis"":229.707352, ""OrbitalPeriod"":460980045.795441, ""AscendingNode"":-48.364805, ""MeanAnomaly"":164.833198, ""RotationPeriod"":297177.801695, ""AxialTilt"":-1.145456, ""Rings"":[ { ""Name"":""Clota 5 A Ring"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":9.0255e+12, ""InnerRad"":9.7362e+07, ""OuterRad"":6.48e+08 } ], ""WasDiscovered"":true, ""WasMapped"":false }",
            @"{ ""timestamp"":""2024-07-06T23:20:45Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""Clota"", ""BodyID"":0, ""StarSystem"":""Clota"", ""SystemAddress"":3657265189586, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""K"", ""Subclass"":4, ""StellarMass"":0.738281, ""Radius"":597291968.000000, ""AbsoluteMagnitude"":6.286362, ""Age_MY"":8306, ""SurfaceTemperature"":4459.000000, ""Luminosity"":""Va"", ""RotationPeriod"":302171.893089, ""AxialTilt"":0.000000, ""Rings"":[ { ""Name"":""Clota A Belt"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":1.229e+14, ""InnerRad"":9.1571e+08, ""OuterRad"":2.1809e+09 }, { ""Name"":""Clota B Belt"", ""RingClass"":""eRingClass_Rocky"", ""MassMT"":1.4223e+15, ""InnerRad"":2.0215e+10, ""OuterRad"":5.1343e+11 } ], ""WasDiscovered"":true, ""WasMapped"":false }",
            @"{ ""timestamp"":""2024-07-05T00:52:35Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""HIP 20567"", ""BodyID"":0, ""StarSystem"":""HIP 20567"", ""SystemAddress"":1659744799067, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""F"", ""Subclass"":6, ""StellarMass"":1.500000, ""Radius"":900681792.000000, ""AbsoluteMagnitude"":3.517029, ""Age_MY"":2008, ""SurfaceTemperature"":7607.000000, ""Luminosity"":""V"", ""RotationPeriod"":113131.067938, ""AxialTilt"":0.000000, ""WasDiscovered"":true, ""WasMapped"":false }"
        };

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

        [PublicAPI("The radius of the star that has been scanned, in kilometres")]
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

        public DateTime? mapped => star.mappedDateTime;

        public List<IDictionary<string, object>> parents => star.parents;

        public DateTime? scanned => star.scannedDateTime;

        public Body star { get; private set; }

        public StarScannedEvent(DateTime timestamp, string scantype, Body star) : base(timestamp, NAME)
        {
            this.star = star;
            this.scantype = scantype;
        }
    }
}
