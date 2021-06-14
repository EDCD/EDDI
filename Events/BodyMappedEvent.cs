using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class BodyMappedEvent : Event
    {
        public const string NAME = "Body mapped";
        public const string DESCRIPTION = "Triggered after mapping a body with the Surface Area Analysis scanner";
        public const string SAMPLE = @"{ ""timestamp"":""2018-10-05T15:06:12Z"", ""event"":""SAAScanComplete"", ""BodyName"":""Eranin 5"", ""BodyID"":5, ""SystemAddress"":2832631632594, ""ProbesUsed"":6, ""EfficiencyTarget"":9 }";

        // Variable names for this event should match the class property names for maximum compatibility with the BodyDetails() function in Cottle

        [PublicAPI("The name of the body that has been scanned")]
        public string bodyname => body?.bodyname ?? bodyName;

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

        [PublicAPI("The number of probes used to map the body")]
        public int probesused { get; private set; }

        [PublicAPI("The efficiency target for the number of probes used to map the body")]
        public int efficiencytarget { get; private set; }

        // Not intended to be user facing

        public DateTime? scanned => body?.scanned;

        public DateTime? mapped => body?.mapped;

        [Obsolete("Use 'bodyname' instead")]
        public string name => body?.bodyname;

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
