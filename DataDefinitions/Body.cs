using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A star or planet
    /// </summary>
    public class Body
    {
        /// <summary>The ID of this body in EDDB</summary>
        public long? EDDBID { get; set; }

        /// <summary>The type of the body (Star or Planet)</summary>
        [Obsolete("Please use BodyType instead")]
        public string type => (Type ?? BodyType.None).localizedName;

        /// <summary>The type of the body (Star or Planet)</summary>
        public BodyType Type { get; set; } = BodyType.None;

        /// <summary>The name of the body</summary>
        public string name { get; set; }

        /// <summary>The name of the system in which the body resides</summary>
        public string systemname { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        public long? systemAddress { get; set; }

        /// <summary>The ID of the system associated with this body in EDDB</summary>
        public long? systemEDDBID { get; set; }

        /// <summary>The age of the body, in millions of years</summary>
        public long? age { get; set; }

        /// <summary>The distance of the body from the arrival star, in light seconds </summary>
        public long? distance { get; set; }

        /// <summary>If this body can be landed upon</summary>
        public bool? landable { get; set; }

        /// <summary>If this body is tidally locked</summary>
        public bool? tidallylocked { get; set; }

        /// <summary>The surface temperature of the body, in Kelvin</summary>
        public decimal? temperature { get; set; }

        /// <summary>The body's rings</summary>
        [JsonConverter(typeof(RingConverter))]
        public List<Ring> rings { get; set; } = new List<Ring>();

        // Star-specific items

        /// <summary>If this body is the main star</summary>
        public bool? mainstar { get; set; }

        /// <summary>The stellar class of the star</summary>
        public string stellarclass { get; set; }

        /// <summary>The Luminosity Class of the Star (since 2.4)</summary>
        public string luminosityclass { get; set; }

        /// <summary>The solar mass of the star</summary>
        public decimal? solarmass { get; set; }

        /// <summary>The solar radius of the star, compared to Sol</summary>
        public decimal? solarradius { get; set; }

        /// <summary>The absolute magnitude of the star</summary> 
        public decimal? absoluteMagnitude { get; set; }

        // Additional information
        public string chromaticity { get; set; }
        public decimal? radiusprobability { get; set; }
        public decimal? massprobability { get; set; }
        public decimal? tempprobability { get; set; }
        public decimal? ageprobability { get; set; }

        // Body-specific items

        /// <summary>The argument of periapsis, in degrees</summary>
        public decimal? periapsis { get; set; }

        /// <summary>The atmosphere class</summary>
        public AtmosphereClass atmosphereclass { get; set; } = AtmosphereClass.NoAtmosphere;

        /// <summary>The atmosphere</summary>
        [Obsolete("Please use AtmosphereClass instead")]
        public string atmosphere => (atmosphereclass ?? AtmosphereClass.NoAtmosphere).localizedName;

        /// <summary>The atmosphere's composition</summary>
        public List<AtmosphereComposition> atmosphereCompositions { get; set; } = new List<AtmosphereComposition>();

        /// <summary>The axial tilt, in degrees</summary>
        public decimal? tilt { get; set; }

        /// <summary>The earth mass of the planet</summary>
        public decimal? earthmass { get; set; }

        /// <summary>The gravity of the planet, in G's</summary>
        public decimal? gravity { get; set; }

        /// <summary>The orbital eccentricity of the planet</summary>
        public decimal? eccentricity { get; set; }

        /// <summary>The orbital inclination of the planet, in degrees</summary>
        public decimal? inclination { get; set; }

        /// <summary>The orbital period of the planet, in days</summary>
        public decimal? orbitalperiod { get; set; }

        /// <summary>The radius of the planet, in km</summary>
        public decimal? radius { get; set; }

        /// <summary>The rotational period of the planet, in days</summary>
        public decimal? rotationalperiod { get; set; }

        /// <summary>The semi-major axis of the planet, in astronomical units (AU)</summary>
        public decimal? semimajoraxis { get; set; }

        /// <summary>The pressure at the surface of the planet, in Earth atmospheres</summary>
        public decimal? pressure { get; set; }

        /// <summary>The terraform state (localized name)</summary>
        [Obsolete("Please use TerraformState instead")]
        public string terraformstate => (terraformState ?? TerraformState.None).localizedName;

        /// <summary>The terraform state</summary>
        public TerraformState terraformState { get; set; } = TerraformState.None;

        /// <summary>The planet type (localized name)</summary>
        [Obsolete("Please use PlanetClass instead")]
        public string planettype => (planetClass ?? PlanetClass.None).localizedName;

        /// <summary>The planet type</summary>
        public PlanetClass planetClass { get; set; } = PlanetClass.None;

        /// <summary>The volcanism</summary>
        [JsonConverter(typeof(VolcanismConverter))]
        public Volcanism volcanism { get; set; }

        /// <summary>The solid body composition of the body</summary>
        public List<BodySolidComposition> solidComposition { get; set; } = new List<BodySolidComposition>();

        /// <summary>The materials present at the surface of the body</summary>
        public List<MaterialPresence> materials { get; set; } = new List<MaterialPresence>();

        /// <summary>The reserve level (localized name)</summary>
        [Obsolete("Please use SystemReserveLevel instead")]
        public string reserves => (reserveLevel ?? SystemReserveLevel.None).localizedName;
        /// <summary>The reserve level</summary>
        public SystemReserveLevel reserveLevel { get; set; } = SystemReserveLevel.None;

        /// <summary> the last time the information present changed (in the data source) </summary>
        public long? updatedat { get; set; }

        /// <summary>
        /// Calculate additonal information for the star
        /// </summary>
        public void setStellarExtras()
        {
            StarClass starClass = StarClass.FromName(stellarclass);
            if (starClass != null)
            {
                if (solarmass != null) massprobability = StarClass.sanitiseCP(starClass.stellarMassCP((decimal)solarmass));
                if (solarradius != null) radiusprobability = StarClass.sanitiseCP(starClass.stellarRadiusCP((decimal)solarradius));
                if (temperature != null) tempprobability = StarClass.sanitiseCP(starClass.tempCP((decimal)temperature));
                if (age != null) ageprobability = StarClass.sanitiseCP(starClass.ageCP((decimal)age));
                chromaticity = starClass.chromaticity.localizedName;
            }
        }
    }

    public class BodyType : ResourceBasedLocalizedEDName<BodyType>
    {
        static BodyType()
        {
            resourceManager = Properties.Body.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new BodyType(edname);

            None = new BodyType("None");
            var Planet = new BodyType("Planet");
            var Star = new BodyType("Star");
            var Belt = new BodyType("Belt");
        }

        public static readonly BodyType None;

        // dummy used to ensure that the static constructor has run
        public BodyType() : this("")
        { }

        private BodyType(string edname) : base(edname, edname)
        { }
    }

    public class PlanetClass : ResourceBasedLocalizedEDName<PlanetClass>
    {
        static PlanetClass()
        {
            resourceManager = Properties.PlanetClass.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new PlanetClass(edname);

            None = new PlanetClass("None");
            var Ammonia = new PlanetClass("Ammonia");
            var EarthLike = new PlanetClass("EarthLike");
            var GasGiantWithAmmoniaBasedLife = new PlanetClass("GasGiantWithAmmoniaBasedLife");
            var GasGiantWithWaterBasedLife = new PlanetClass("GasGiantWithWaterBasedLife");
            var HeliumGasGiant = new PlanetClass("HeliumGasGiant");
            var HeliumRichGasGiant = new PlanetClass("HeliumRichGasGiant");
            var HighMetalContent = new PlanetClass("HighMetalContent");
            var Icy = new PlanetClass("Icy");
            var MetalRich = new PlanetClass("MetalRich");
            var Rock = new PlanetClass("Rocky");
            var RockyIce = new PlanetClass("RockyIce");
            var ClassIGasGiant = new PlanetClass("ClassIGasGiant");
            var ClassIIGasGiant = new PlanetClass("ClassIIGasGiant");
            var ClassIIIGasGiant = new PlanetClass("ClassIIIGasGiant");
            var ClassIVGasGiant = new PlanetClass("ClassIVGasGiant");
            var ClassVGasGiant = new PlanetClass("ClassVGasGiant");
            var WaterGiant = new PlanetClass("WaterGiant");
            var WaterGiantWithLife = new PlanetClass("WaterGiantWithLife");
            var Water = new PlanetClass("Water");
        }

        public static readonly PlanetClass None;

        // dummy used to ensure that the static constructor has run
        public PlanetClass() : this("")
        { }

        private PlanetClass(string edname) : base(edname, edname)
        { }

        new public static PlanetClass FromEDName(string edname)
        {
            if (edname == null)
            {
                return null;
            }

            string normalizedEDName = edname.Replace(" ", "").Replace("-", "");
            normalizedEDName = normalizedEDName.Replace("world", ""); // In some cases, EDDB uses "world" while the journal uses "body". Fix that here.
            normalizedEDName = normalizedEDName.Replace("body", ""); // In some cases, EDDB uses "world" while the journal uses "body". Fix that here.
            normalizedEDName = normalizedEDName.Replace("sudarsky", ""); // EDDB uses "class iv gas giant" while the journal uses "Sudarsky class IV gas giant". Fix that here.
            return ResourceBasedLocalizedEDName<PlanetClass>.FromEDName(normalizedEDName);
        }
    }

    /// <summary> Body Solid Composition </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BodySolidComposition
    {
        static BodySolidComposition()
        {
            resourceManager = Properties.Body.ResourceManager;
            resourceManager.IgnoreCase = true;

            COMPOSITIONS.Add("Ice", "Ice");
            COMPOSITIONS.Add("Rock", "Rock");
            COMPOSITIONS.Add("Metal", "Metal");
        }

        public static readonly ResourceManager resourceManager;

        // Translation of composition of atmosphere 
        private static readonly IDictionary<string, string> COMPOSITIONS = new Dictionary<string, string>();

        [JsonProperty("composition")]
        public string edName { get; set; } // Ice, Rock, etc.
        public string invariantName => GetInvariantString(edName ?? "None");
        public string localizedName => GetLocalizedString(edName ?? "None");
        [JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string name => localizedName;

        [JsonProperty("share")]
        public decimal percent { get; set; } // Percent share of the body

        private string GetInvariantString(string name)
        {
            if (name == null) { return null; }
            name = name.Replace(" ", "_");
            return resourceManager.GetString(name, CultureInfo.InvariantCulture);
        }

        private string GetLocalizedString(string name)
        {
            if (name == null) { return null; }
            name = name.Replace(" ", "_");
            return resourceManager.GetString(name);
        }

        public BodySolidComposition(string composition, decimal percent)
        {
            this.edName = composition;
            this.percent = percent;
        }
    }
}
