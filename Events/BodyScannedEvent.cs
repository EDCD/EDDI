using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class BodyScannedEvent : Event
    {
        public const string NAME = "Body scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a planetary body";
        public const string SAMPLE = @"{ ""timestamp"":""2018-12-03T06:14:45Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""Maia A 2 a"", ""BodyID"":6, ""Parents"":[ {""Star"":5}, {""Star"":1}, {""Null"":0} ], ""DistanceFromArrivalLS"":634.646851, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Metal rich body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":""major metallic magma volcanism"", ""MassEM"":0.007395, ""Radius"":1032003.750000, ""SurfaceGravity"":2.767483, ""SurfaceTemperature"":1088.117188, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":36.187508 }, { ""Name"":""nickel"", ""Percent"":27.370716 }, { ""Name"":""chromium"", ""Percent"":16.274725 }, { ""Name"":""zinc"", ""Percent"":9.834411 }, { ""Name"":""zirconium"", ""Percent"":4.202116 }, { ""Name"":""niobium"", ""Percent"":2.473223 }, { ""Name"":""molybdenum"", ""Percent"":2.363023 }, { ""Name"":""technetium"", ""Percent"":1.294281 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.000000, ""Metal"":1.000000 }, ""SemiMajorAxis"":1139188608.000000, ""Eccentricity"":0.003271, ""OrbitalInclination"":-0.020626, ""Periapsis"":49.808826, ""OrbitalPeriod"":104294.210938, ""RotationPeriod"":104295.164063, ""AxialTilt"":-0.087406 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BodyScannedEvent()
        {
            VARIABLES.Add("bodyname", "The name of the body that has been scanned");
            VARIABLES.Add("systemname", "The name of the system containing the scanned body");
            VARIABLES.Add("shortname", "The short name of the body, less the system name");
            VARIABLES.Add("planettype", "The type of body that has been scanned (High metal content body etc)");
            VARIABLES.Add("gravity", "The surface gravity of the body that has been scanned, relative to Earth's gravity");
            VARIABLES.Add("earthmass", "The mass of the body that has been scanned, relative to Earth's mass");
            VARIABLES.Add("radius", "The radius of the body that has been scanned, in kilometres");
            VARIABLES.Add("temperature", "The surface temperature of the body that has been scanned, in Kelvin (only available if DSS equipped)");
            VARIABLES.Add("pressure", "The surface pressure of the body that has been scanned, in Earth atmospheres (only available if DSS equipped)");
            VARIABLES.Add("tidallylocked", "True if the body is tidally locked (only available if DSS equipped)");
            VARIABLES.Add("landable", "True if the body is landable (only available if DSS equipped)");
            VARIABLES.Add("atmosphere", "The atmosphere of the body that has been scanned (only available if DSS equipped)");
            VARIABLES.Add("atmospherecompositions", "The composition of the atmosphere of the body that has been scanned (array of AtmosphereComposition objects) (only available if DSS equipped)");
            VARIABLES.Add("solidcompositions", "The composition of the body's solids that has been scanned (array of SolidComposition objects) (only available if DSS equipped)");
            VARIABLES.Add("volcanism", "The volcanism of the body that has been scanned (only available if DSS equipped)");
            VARIABLES.Add("distance", "The distance in LS from the main star");
            VARIABLES.Add("orbitalperiod", "The number of days taken for a full orbit of the main star");
            VARIABLES.Add("rotationalperiod", "The number of days taken for a full rotation");
            VARIABLES.Add("semimajoraxis", "The semi major axis of the body's orbit, in light seconds");
            VARIABLES.Add("eccentricity", "The orbital eccentricity of the body");
            VARIABLES.Add("inclination", "The orbital inclination of the body, in degrees");
            VARIABLES.Add("periapsis", "The argument of periapsis of the body, in degrees");
            VARIABLES.Add("rings", "A list of the body's rings (as ring objects)");
            VARIABLES.Add("reserves", "The level of reserves in the rings if applicable (Pristine/Major/Common/Low/Depleted)");
            VARIABLES.Add("materials", "A list of materials present on the body that has been scanned");
            VARIABLES.Add("terraformstate", "Whether the body can be, is in the process of, or has been terraformed (only available if DSS equipped)");
            VARIABLES.Add("tilt", "Axial tilt for the body, in degrees (only available if DSS equipped)");
            VARIABLES.Add("estimatedvalue", "The estimated value of the current scan");
        }

        // Variable names for this event should match the class property names for maximum compatibility with the BodyDetails() function in Cottle

        public string bodyname => body.bodyname;

        public string systemname { get; private set; }

        public string shortname => body.shortname;

        public string planettype => (body.planetClass ?? PlanetClass.None).localizedName;  // This matches the object property reported from the BodyDetails() function

        public decimal? earthmass => body.earthmass;

        public decimal? radius => body.radius;

        public decimal gravity => (decimal)body.gravity;

        public decimal? temperature => body.temperature;

        public decimal? pressure => body.pressure;

        public bool? tidallylocked => body.tidallylocked;

        public bool? landable => body.landable;

        public string atmosphere => (body.atmosphereclass ?? AtmosphereClass.None).localizedName; // This matches the object property reported from the BodyDetails() function

        public List<AtmosphereComposition> atmospherecompositions => body.atmospherecompositions;

        public List<SolidComposition> solidcompositions => body.solidcompositions;

        public Volcanism volcanism => body.volcanism;

        public decimal distance => (decimal)body.distance;

        public decimal? orbitalperiod => body.orbitalperiod;

        public decimal? rotationalperiod => body.rotationalperiod;

        public decimal? semimajoraxis => body.semimajoraxis;

        public decimal? eccentricity => body.eccentricity;

        public decimal? inclination => body.inclination;

        public decimal? periapsis => body.periapsis;

        public List<Ring> rings => body.rings;

        public string reserves => (body.reserveLevel ?? ReserveLevel.None).localizedName; // This matches the object property reported from the BodyDetails() function

        public List<MaterialPresence> materials => body.materials;

        public string terraformstate => (terraformState ?? TerraformState.NotTerraformable).localizedName; // This matches the object property reported from the BodyDetails() function

        public decimal? tilt => body.tilt;

        public long? estimatedvalue => body.estimatedvalue;

        // Variables below are not intended to be user facing
        public Body body { get; private set; }
        public long? bodyId => body.bodyId;
        public List<IDictionary<string, object>> parents => body.parents;
        public AtmosphereClass atmosphereclass => body.atmosphereclass;
        public PlanetClass planetClass => body.planetClass;
        public TerraformState terraformState => body.terraformState;
        public string scantype { get; private set; } // One of AutoScan, Basic, Detailed, NavBeacon, NavBeaconDetail
                                                     // AutoScan events are detailed scans triggered via proximity. 

        // Deprecated, maintained for compatibility with user scripts
        [JsonIgnore, Obsolete("Use bodyname instead")]
        public string name => bodyname;
        [JsonIgnore, Obsolete("Use planetClass instead")]
        public string bodyclass => (body.planetClass ?? PlanetClass.None).localizedName;
        [JsonIgnore, Obsolete("Use distance instead")]
        public decimal distancefromarrival => distance;  // This is the object property reported from the BodyDetails() function
        [JsonIgnore, Obsolete("Use inclination instead")]
        public decimal? orbitalinclination => inclination;  // This is the object property reported from the BodyDetails() function
        [JsonIgnore, Obsolete("Use rotationalperiod instead")]
        public decimal? rotationperiod => rotationalperiod;  // This is the object property reported from the BodyDetails() function
        [JsonIgnore, Obsolete("Use tilt instead")]
        public decimal? axialtilt => tilt;  // This is the object property reported from the BodyDetails() function

        public BodyScannedEvent(DateTime timestamp, string scantype, Body body) : base(timestamp, NAME)
        {
            this.body = body;
            this.scantype = scantype;
        }
    }
}
