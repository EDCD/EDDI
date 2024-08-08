using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class BodyScannedEvent : Event
    {
        public const string NAME = "Body scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a planetary body";
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"":""2024-07-07T20:16:16Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""HIP 4310 A 5"", ""BodyID"":19, ""Parents"":[ {""Star"":1}, {""Null"":0} ], ""StarSystem"":""HIP 4310"", ""SystemAddress"":1183364027090, ""DistanceFromArrivalLS"":123.338674, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":"""", ""AtmosphereType"":""None"", ""Volcanism"":"""", ""MassEM"":0.267556, ""Radius"":4041058.500000, ""SurfaceGravity"":6.530316, ""SurfaceTemperature"":533.916748, ""SurfacePressure"":0.000000, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":21.206753 }, { ""Name"":""nickel"", ""Percent"":16.039900 }, { ""Name"":""sulphur"", ""Percent"":14.641438 }, { ""Name"":""carbon"", ""Percent"":12.311934 }, { ""Name"":""chromium"", ""Percent"":9.537381 }, { ""Name"":""manganese"", ""Percent"":8.758172 }, { ""Name"":""phosphorus"", ""Percent"":7.882310 }, { ""Name"":""zinc"", ""Percent"":5.763201 }, { ""Name"":""molybdenum"", ""Percent"":1.384789 }, { ""Name"":""ruthenium"", ""Percent"":1.309652 }, { ""Name"":""tungsten"", ""Percent"":1.164464 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.651117, ""Metal"":0.348883 }, ""SemiMajorAxis"":36955666542.053223, ""Eccentricity"":0.000841, ""OrbitalInclination"":0.000549, ""Periapsis"":69.475598, ""OrbitalPeriod"":4462032.735348, ""AscendingNode"":-18.946473, ""MeanAnomaly"":130.848763, ""RotationPeriod"":-4439612.419307, ""AxialTilt"":2.118238, ""WasDiscovered"":true, ""WasMapped"":true }",
            @"{ ""timestamp"":""2024-07-07T20:16:24Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""HIP 4310 A 5 a"", ""BodyID"":20, ""Parents"":[ {""Planet"":19}, {""Star"":1}, {""Null"":0} ], ""StarSystem"":""HIP 4310"", ""SystemAddress"":1183364027090, ""DistanceFromArrivalLS"":123.435824, ""TidalLock"":true, ""TerraformState"":"""", ""PlanetClass"":""Rocky body"", ""Atmosphere"":"""", ""AtmosphereType"":""SulphurDioxide"", ""AtmosphereComposition"":[ { ""Name"":""SulphurDioxide"", ""Percent"":90.000000 }, { ""Name"":""Silicates"", ""Percent"":5.000000 }, { ""Name"":""Oxygen"", ""Percent"":2.999999 } ], ""Volcanism"":""rocky magma volcanism"", ""MassEM"":0.015201, ""Radius"":1700712.500000, ""SurfaceGravity"":2.094735, ""SurfaceTemperature"":533.916748, ""SurfacePressure"":0.000000, ""Landable"":false, ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.855797, ""Metal"":0.144203 }, ""SemiMajorAxis"":43915281.295776, ""Eccentricity"":0.000000, ""OrbitalInclination"":-24.170971, ""Periapsis"":298.154963, ""OrbitalPeriod"":172241.181135, ""AscendingNode"":175.191316, ""MeanAnomaly"":20.023749, ""RotationPeriod"":177066.570578, ""AxialTilt"":-0.388167, ""WasDiscovered"":true, ""WasMapped"":true }",
            @"{ ""timestamp"":""2024-07-07T20:12:56Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""HIP 4310 B 9 i"", ""BodyID"":62, ""Parents"":[ {""Planet"":51}, {""Star"":2}, {""Null"":0} ], ""StarSystem"":""HIP 4310"", ""SystemAddress"":1183364027090, ""DistanceFromArrivalLS"":145566.468532, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""Icy body"", ""Atmosphere"":""argon rich atmosphere"", ""AtmosphereType"":""ArgonRich"", ""AtmosphereComposition"":[ { ""Name"":""Nitrogen"", ""Percent"":87.174294 }, { ""Name"":""Argon"", ""Percent"":12.825711 } ], ""Volcanism"":"""", ""MassEM"":0.110502, ""Radius"":4166162.250000, ""SurfaceGravity"":2.537507, ""SurfaceTemperature"":102.514107, ""SurfacePressure"":26999.443359, ""Landable"":false, ""Composition"":{ ""Ice"":0.881750, ""Rock"":0.101573, ""Metal"":0.016678 }, ""SemiMajorAxis"":16625593900.680542, ""Eccentricity"":0.000000, ""OrbitalInclination"":20.370366, ""Periapsis"":269.670219, ""OrbitalPeriod"":15734075.307846, ""AscendingNode"":-77.713332, ""MeanAnomaly"":145.225609, ""RotationPeriod"":253730.209613, ""AxialTilt"":-0.217062, ""Rings"":[ { ""Name"":""HIP 4310 B 9 i A Ring"", ""RingClass"":""eRingClass_Icy"", ""MassMT"":19874, ""InnerRad"":6.2747e+06, ""OuterRad"":1.6706e+07 } ], ""ReserveLevel"":""MajorResources"", ""WasDiscovered"":true, ""WasMapped"":true }"
        };

        // Variable names for this event should match the class property names for maximum compatibility with the BodyDetails() function in Cottle

        [PublicAPI("The type of scan event (AutoScan, Basic, Detailed, NavBeacon, NavBeaconDetail)")]
        public string scantype { get; private set; } // One of AutoScan, Basic, Detailed, NavBeacon, NavBeaconDetail
                                                     // AutoScan events are detailed scans triggered via proximity. 

        [PublicAPI("The name of the body that has been scanned")]
        public string bodyname => body?.bodyname;

        [PublicAPI("The name of the system containing the scanned body")]
        public string systemname => body?.systemname;

        [PublicAPI("The short name of the body, less the system name")]
        public string shortname => body?.shortname;

        [PublicAPI("The body type of the body that has been scanned (e.g. 'Planet', 'Moon', 'Star' etc.)")]
        public string bodytype => (body?.bodyType ?? BodyType.None).localizedName;

        [PublicAPI("The type of body that has been scanned (High metal content body etc)")]
        public string planettype => (body?.planetClass ?? PlanetClass.None).localizedName;  // This matches the object property reported from the BodyDetails() function

        [PublicAPI("The mass of the body that has been scanned, relative to Earth's mass")]
        public decimal? earthmass => body?.earthmass;

        [PublicAPI("The cumulative probability describing the body's mass, relative to other bodies of the same planet type")]
        public decimal? massprobability => body?.massprobability;

        [PublicAPI("The radius of the body that has been scanned, in kilometres")]
        public decimal? radius => body?.radius;

        [PublicAPI("The cumulative probability describing the body's radius, relative to other bodies of the same planet type")]
        public decimal? radiusprobability => body?.radiusprobability;

        [PublicAPI("The surface gravity of the body that has been scanned, relative to Earth's gravity")]
        public decimal? gravity => body?.gravity;

        [PublicAPI("The cumulative probability describing the body's gravity, relative to other bodies of the same planet type")]
        public decimal? gravityprobability => body?.gravityprobability;

        [PublicAPI("The surface temperature of the body that has been scanned, in Kelvin (only available if DSS equipped)")]
        public decimal? temperature => body?.temperature;

        [PublicAPI("The cumulative probability describing the body's temperature, relative to other bodies of the same planet type")]
        public decimal? tempprobability => body?.tempprobability;

        [PublicAPI("The surface pressure of the body that has been scanned, in Earth atmospheres (only available if DSS equipped)")]
        public decimal? pressure => body?.pressure;

        [PublicAPI("The cumulative probability describing the body's atmospheric pressure, relative to other bodies of the same planet type")]
        public decimal? pressureprobability => body?.pressureprobability;

        [PublicAPI("True if the body is tidally locked (only available if DSS equipped)")]
        public bool? tidallylocked => body?.tidallylocked;

        [PublicAPI("True if the body is landable (only available if DSS equipped)")]
        public bool? landable => body?.landable;

        [PublicAPI("The atmosphere of the body that has been scanned (only available if DSS equipped)")]
        public string atmosphere => (body?.atmosphereclass ?? AtmosphereClass.None).localizedName; // This matches the object property reported from the BodyDetails() function

        [PublicAPI("The composition of the atmosphere of the body that has been scanned (array of objects) (only available if DSS equipped)")]
        public List<AtmosphereComposition> atmospherecompositions => body?.atmospherecompositions;

        [PublicAPI("The composition of the body's solids that has been scanned (array of objects) (only available if DSS equipped)")]
        public List<SolidComposition> solidcompositions => body?.solidcompositions;

        [PublicAPI("The volcanism of the body that has been scanned (only available if DSS equipped)")]
        public Volcanism volcanism => body?.volcanism;

        [PublicAPI("The distance in LS from the main star")]
        public decimal? distance => body?.distance;

        [PublicAPI("The number of days taken for a full orbit of the main star")]
        public decimal? orbitalperiod => body?.orbitalperiod;

        [PublicAPI("The cumulative probability describing the body's orbital period, relative to other bodies of the same planettype")]
        public decimal? orbitalperiodprobability => body?.orbitalperiodprobability;

        [PublicAPI("The number of days taken for a full rotation")]
        public decimal? rotationalperiod => body?.rotationalperiod;

        [PublicAPI("The cumulative probability describing the body's rotational period, relative to other bodies of the same planettype")]
        public decimal? rotationalperiodprobability => body?.rotationalperiodprobability;

        [PublicAPI("The semi major axis of the body's orbit, in light seconds")]
        public decimal? semimajoraxis => body?.semimajoraxis;

        [PublicAPI("The cumulative probability describing the body's semi-major axis, relative to other bodies of the same planet type")]
        public decimal? semimajoraxisprobability => body?.semimajoraxisprobability;

        [PublicAPI("The orbital eccentricity of the body")]
        public decimal? eccentricity => body?.eccentricity;

        [PublicAPI("The cumulative probability describing the body's orbital eccentricity, relative to other bodies of the same planet type")]
        public decimal? eccentricityprobability => body?.eccentricityprobability;

        [PublicAPI("The orbital inclination of the body, in degrees")]
        public decimal? inclination => body?.inclination;

        [PublicAPI("The cumulative probability describing the body's orbital inclination, relative to other bodies of the same planet type")]
        public decimal? inclinationprobability => body?.inclinationprobability;

        [PublicAPI("The argument of periapsis of the body, in degrees")]
        public decimal? periapsis => body?.periapsis;

        [PublicAPI("The cumulative probability describing the body's argument of periapsis, relative to other bodies of the same planet type")]
        public decimal? periapsisprobability => body?.periapsisprobability;

        [PublicAPI("A list of the body's rings (as ring objects)")]
        public List<Ring> rings => body?.rings;

        [PublicAPI("The level of reserves in the rings if applicable (Pristine/Major/Common/Low/Depleted)")]
        public string reserves => (body?.reserveLevel ?? ReserveLevel.None).localizedName; // This matches the object property reported from the BodyDetails() function

        [PublicAPI("A list of materials present on the body that has been scanned")]
        public List<MaterialPresence> materials => body?.materials;

        [PublicAPI("Whether the body can be, is in the process of, or has been terraformed (only available if DSS equipped)")]
        public string terraformstate => (body?.terraformState ?? TerraformState.NotTerraformable).localizedName; // This matches the object property reported from the BodyDetails() function

        [PublicAPI("Axial tilt for the body, in degrees (only available if DSS equipped)")]
        public decimal? tilt => body?.tilt;

        [PublicAPI("The cumulative probability describing the body's orbital tilt, relative to other bodies of the same planet type")]
        public decimal? tiltprobability => body?.tiltprobability;

        [PublicAPI("The average density of the body, in kg per cubic meter")]
        public decimal? density => body?.density;

        [PublicAPI("The cumulative probability describing the body's density, relative to other bodies of the same planet type")]
        public decimal? densityprobability => body?.densityprobability;

        [PublicAPI("The estimated value of the current scan")]
        public long? estimatedvalue => body?.estimatedvalue;

        [PublicAPI("Whether this body's scan data has already been registered with Universal Cartographics")]
        public bool? alreadydiscovered => body?.alreadydiscovered;

        [PublicAPI("Whether this body's map data has already been registered with Universal Cartographics")]
        public bool? alreadymapped => body?.alreadymapped;

        // Variables below are not intended to be user facing

        public Body body { get; private set; }

        public long? bodyId => body.bodyId;
        
        public List<IDictionary<string, object>> parents => body.parents;

        public AtmosphereClass atmosphereclass => body.atmosphereclass;
        
        public PlanetClass planetClass => body.planetClass;
        
        public TerraformState terraformState => body.terraformState;

        public ulong? systemAddress => body.systemAddress;

        // Deprecated, maintained for compatibility with user scripts

        [Obsolete("Use bodyname instead")]
        public string name => bodyname;

        [Obsolete("Use planetClass instead")]
        public string bodyclass => (body.planetClass ?? PlanetClass.None).localizedName;
        
        [Obsolete("Use distance instead")]
        public decimal? distancefromarrival => distance;  // This is the object property reported from the BodyDetails() function

        [Obsolete("Use inclination instead")]
        public decimal? orbitalinclination => inclination;  // This is the object property reported from the BodyDetails() function

        [Obsolete("Use rotationalperiod instead")]
        public decimal? rotationperiod => rotationalperiod;  // This is the object property reported from the BodyDetails() function

        [Obsolete("Use tilt instead")]
        public decimal? axialtilt => tilt;  // This is the object property reported from the BodyDetails() function

        public BodyScannedEvent(DateTime timestamp, string scantype, Body body) : base(timestamp, NAME)
        {
            this.body = body;
            this.scantype = scantype;
        }
    }
}
