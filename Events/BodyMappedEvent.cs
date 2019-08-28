using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class BodyMappedEvent : Event
    {
        public const string NAME = "Body mapped";
        public const string DESCRIPTION = "Triggered after mapping a body with the Surface Area Analysis scanner";
        public const string SAMPLE = @"{ ""timestamp"":""2018-10-05T15:06:12Z"", ""event"":""SAAScanComplete"", ""BodyName"":""Eranin 5"", ""BodyID"":5, ""SystemAddress"":2832631632594, ""ProbesUsed"":6, ""EfficiencyTarget"":9 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BodyMappedEvent()
        {
            VARIABLES.Add("bodyname", "The name of the body that has been scanned");
            VARIABLES.Add("systemname", "The name of the system containing the scanned body");
            VARIABLES.Add("shortname", "The short name of the body, less the system name");
            VARIABLES.Add("planettype", "The type of body that has been scanned (High metal content body etc)");
            VARIABLES.Add("gravity", "The surface gravity of the body that has been scanned, relative to Earth's gravity");
            VARIABLES.Add("gravityprobability", "The cumulative probability describing the body's gravity, relative to other bodies of the same planet type");
            VARIABLES.Add("earthmass", "The mass of the body that has been scanned, relative to Earth's mass");
            VARIABLES.Add("massprobability", "The cumulative probability describing the body's mass, relative to other bodies of the same planet type");
            VARIABLES.Add("radius", "The radius of the body that has been scanned, in kilometres");
            VARIABLES.Add("radiusprobability", "The cumulative probability describing the body's radius, relative to other bodies of the same planet type");
            VARIABLES.Add("temperature", "The surface temperature of the body that has been scanned, in Kelvin (only available if DSS equipped)");
            VARIABLES.Add("tempprobability", "The cumulative probability describing the body's temperature, relative to other bodies of the same planet type");
            VARIABLES.Add("pressure", "The surface pressure of the body that has been scanned, in Earth atmospheres (only available if DSS equipped)");
            VARIABLES.Add("pressureprobability", "The cumulative probability describing the body's atmospheric pressure, relative to other bodies of the same planet type");
            VARIABLES.Add("tidallylocked", "True if the body is tidally locked (only available if DSS equipped)");
            VARIABLES.Add("landable", "True if the body is landable (only available if DSS equipped)");
            VARIABLES.Add("atmosphere", "The atmosphere of the body that has been scanned (only available if DSS equipped)");
            VARIABLES.Add("atmospherecompositions", "The composition of the atmosphere of the body that has been scanned (array of AtmosphereComposition objects) (only available if DSS equipped)");
            VARIABLES.Add("solidcompositions", "The composition of the body's solids that has been scanned (array of SolidComposition objects) (only available if DSS equipped)");
            VARIABLES.Add("volcanism", "The volcanism of the body that has been scanned (only available if DSS equipped)");
            VARIABLES.Add("density", "The average density of the body, in kg per cubic meter");
            VARIABLES.Add("densityprobability", "The cumulative probability describing the body's density, relative to other bodies of the same planet type");
            VARIABLES.Add("distance", "The distance in LS from the main star");
            VARIABLES.Add("orbitalperiod", "The number of days taken for a full orbit of the main star");
            VARIABLES.Add("orbitalperiodprobability", "The cumulative probability describing the body's orbital period, relative to other bodies of the same planettype");
            VARIABLES.Add("rotationalperiod", "The number of days taken for a full rotation");
            VARIABLES.Add("rotationalperiodprobability", "The cumulative probability describing the body's rotational period, relative to other bodies of the same planettype");
            VARIABLES.Add("semimajoraxis", "The semi major axis of the body's orbit, in light seconds");
            VARIABLES.Add("semimajoraxisprobability", "The cumulative probability describing the body's semi-major axis, relative to other bodies of the same planet type");
            VARIABLES.Add("eccentricity", "The orbital eccentricity of the body");
            VARIABLES.Add("eccentricityprobability", "The cumulative probability describing the body's orbital eccentricity, relative to other bodies of the same planet type");
            VARIABLES.Add("inclination", "The orbital inclination of the body, in degrees");
            VARIABLES.Add("inclinationprobability", "The cumulative probability describing the body's orbital inclination, relative to other bodies of the same planet type");
            VARIABLES.Add("periapsis", "The argument of periapsis of the body, in degrees");
            VARIABLES.Add("periapsisprobability", "The cumulative probability describing the body's argument of periapsis, relative to other bodies of the same planet type");
            VARIABLES.Add("rings", "A list of the body's rings (as ring objects)");
            VARIABLES.Add("reserves", "The level of reserves in the rings if applicable (Pristine/Major/Common/Low/Depleted)");
            VARIABLES.Add("materials", "A list of materials present on the body that has been scanned");
            VARIABLES.Add("terraformstate", "Whether the body can be, is in the process of, or has been terraformed (only available if DSS equipped)");
            VARIABLES.Add("tilt", "Axial tilt for the body, in degrees (only available if DSS equipped)");
            VARIABLES.Add("tiltprobability", "The cumulative probability describing the body's orbital tilt, relative to other bodies of the same planet type");
            VARIABLES.Add("estimatedvalue", "The estimated value of the current scan");
            VARIABLES.Add("probesused", "The number of probes used to map the body");
            VARIABLES.Add("efficiencytarget", "The efficiency target for the number of probes used to map the body");
        }

        // Variable names for this event should match the class property names for maximum compatibility with the BodyDetails() function in Cottle

        public string name => body?.bodyname;

        public string bodyname => body?.bodyname ?? bodyName;

        public string systemname { get; private set; }

        public string shortname => body?.shortname;

        public string bodytype => (body?.bodyType ?? BodyType.None).localizedName;

        public string planettype => (body?.planetClass ?? PlanetClass.None).localizedName;  // This matches the object property reported from the BodyDetails() function

        public decimal? earthmass => body?.earthmass;

        public decimal? massprobability => body?.massprobability;

        public decimal? radius => body?.radius;

        public decimal? radiusprobability => body?.radiusprobability;

        public decimal? gravity => body?.gravity;

        public decimal? gravityprobability => body?.gravityprobability;

        public decimal? temperature => body?.temperature;

        public decimal? tempprobability => body?.tempprobability;

        public decimal? pressure => body?.pressure;

        public decimal? pressureprobability => body?.pressureprobability;

        public bool? tidallylocked => body?.tidallylocked;

        public bool? landable => body?.landable;

        public string atmosphere => (body?.atmosphereclass ?? AtmosphereClass.None).localizedName; // This matches the object property reported from the BodyDetails() function

        public List<AtmosphereComposition> atmospherecompositions => body?.atmospherecompositions;

        public List<SolidComposition> solidcompositions => body?.solidcompositions;

        public Volcanism volcanism => body?.volcanism;

        public decimal? distance => body?.distance;

        public decimal? orbitalperiod => body?.orbitalperiod;

        public decimal? orbitalperiodprobability => body?.orbitalperiodprobability;

        public decimal? rotationalperiod => body?.rotationalperiod;

        public decimal? rotationalperiodprobability => body?.rotationalperiodprobability;

        public decimal? semimajoraxis => body?.semimajoraxis;

        public decimal? eccentricityprobability => body?.eccentricityprobability;

        public decimal? eccentricity => body?.eccentricity;

        public decimal? semimajoraxisprobability => body?.semimajoraxisprobability;

        public decimal? inclination => body?.inclination;

        public decimal? inclinationprobability => body?.inclinationprobability;

        public decimal? periapsis => body?.periapsis;

        public decimal? periapsisprobability => body?.periapsisprobability;

        public List<Ring> rings => body?.rings;

        public string reserves => (body?.reserveLevel ?? ReserveLevel.None).localizedName; // This matches the object property reported from the BodyDetails() function

        public List<MaterialPresence> materials => body?.materials;

        public string terraformstate => (body?.terraformState ?? TerraformState.NotTerraformable).localizedName; // This matches the object property reported from the BodyDetails() function

        public decimal? tilt => body?.tilt;

        public decimal? tiltyprobability => body?.tiltprobability;

        public decimal? density => body?.density;

        public decimal? densityprobability => body?.densityprobability;

        public long? estimatedvalue => body?.estimatedvalue;

        public DateTime? scanned => body?.scanned;

        public DateTime? mapped => body?.mapped;

        public bool alreadydiscovered => body?.alreadydiscovered ?? false;

        public bool alreadymapped => body?.alreadymapped ?? false;

        public int probesused { get; private set; }

        public int efficiencytarget { get; private set; }

        // Not intended to be user facing
        public string bodyName { get; private set; }

        public Body body { get; private set; }
        public long? systemAddress { get; private set; }

        public BodyMappedEvent(DateTime timestamp, string bodyName, Body body, long? systemAddress, int probesUsed, int efficiencyTarget) : base(timestamp, NAME)
        {
            this.bodyName = bodyName;
            this.body = body;
            this.systemAddress = systemAddress;
            this.probesused = probesUsed;
            this.efficiencytarget = efficiencyTarget;
        }
    }
}
