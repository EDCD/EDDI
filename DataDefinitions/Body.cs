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
        public long EDDBID { get; set; }

        /// <summary>The type of the body (Star or Planet)</summary>
        public string type => Type?.localizedName;

        /// <summary>The type of the body (Star or Planet)</summary>
        public BodyType Type { get; set; }

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
        public decimal? temperature;

        /// <summary>The body's rings</summary>
        [JsonConverter(typeof(RingConverter))]
        public List<Ring> rings;

        // Star-specific items

        /// <summary>If this body is the main star</summary>
        public bool? mainstar { get; set; }

        /// <summary>The stellar class of the star</summary>
        public string stellarclass;

        /// <summary>The Luminosity Class of the Star (since 2.4)</summary>
        public string luminosityclass { get; set; }

        /// <summary>The solar mass of the star</summary>
        public decimal? solarmass;

        /// <summary>The solar radius of the star, compared to Sol</summary>
        public decimal? solarradius;

        /// <summary>The absolute magnitude of the star</summary> 
        public decimal? absoluteMagnitude;

        // Additional information
        public string chromaticity;
        public decimal? radiusprobability;
        public decimal? massprobability;
        public decimal? tempprobability;
        public decimal? ageprobability;

        // Body-specific items

        /// <summary>The argument of periapsis, in degrees</summary>
        public decimal? periapsis;

        /// <summary>The atmosphere</summary>
        public string atmosphere => atmosphereclass?.localizedName;

        /// <summary>The atmosphere class</summary>
        public AtmosphereClass atmosphereclass;

        /// <summary>The atmosphere's composition</summary>
        public List<AtmosphereComposition> atmosphereCompositions;

        /// <summary>The axial tilt, in degrees</summary>
        public decimal? tilt;

        /// <summary>The earth mass of the planet</summary>
        public decimal? earthmass;

        /// <summary>The gravity of the planet, in G's</summary>
        public decimal? gravity;

        /// <summary>The orbital eccentricity of the planet</summary>
        public decimal? eccentricity;

        /// <summary>The orbital inclination of the planet, in degrees</summary>
        public decimal? inclination;

        /// <summary>The orbital period of the planet, in days</summary>
        public decimal? orbitalperiod;

        /// <summary>The radius of the planet, in km</summary>
        public decimal? radius;

        /// <summary>The rotational period of the planet, in days</summary>
        public decimal? rotationalperiod;

        /// <summary>The semi-major axis of the planet, in astronomical units (AU)</summary>
        public decimal? semimajoraxis;

        /// <summary>The pressure at the surface of the planet, in Earth atmospheres</summary>
        public decimal? pressure;

        /// <summary>The terraform state</summary>
        public string terraformstate => terraformState?.localizedName;

        /// <summary>The terraform state</summary>
        public TerraformState terraformState;

        /// <summary>The planet type</summary>
        public string planettype => planetClass?.localizedName;

        /// <summary>The planet type</summary>
        public PlanetClass planetClass;

        /// <summary>The volcanism</summary>
        [JsonConverter(typeof(VolcanismConverter))]
        public Volcanism volcanism;

        /// <summary>The solid body composition of the body</summary>
        public List<BodySolidComposition> solidComposition;

        /// <summary>The materials present at the surface of the body</summary>
        public List<MaterialPresence> materials;

        /// <summary>The reserve level</summary>
        public string reserves;

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

            var Planet = new BodyType("Planet");
            var Star = new BodyType("Star");
            var Belt = new BodyType("Belt");
        }

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

            var AmmoniaWorld = new PlanetClass("AmmoniaWorld");
            var EarthLikeBody = new PlanetClass("EarthLikeBody");
            var GasGiantWithAmmoniaBasedLife = new PlanetClass("GasGiantWithAmmoniaBasedLife");
            var GasGiantWithWaterBasedLife = new PlanetClass("GasGiantWithWaterBasedLife");
            var HeliumGasGiant = new PlanetClass("HeliumGasGiant");
            var HeliumRichGasGiant = new PlanetClass("HeliumRichGasGiant");
            var HighMetalContentBody = new PlanetClass("HighMetalContentBody");
            var IcyBody = new PlanetClass("IcyBody");
            var MetalRichBody = new PlanetClass("MetalRichBody");
            var RockyBody = new PlanetClass("RockyBody");
            var RockyIceBody = new PlanetClass("RockyIceBody");
            var ClassIGasGiant = new PlanetClass("ClassIGasGiant");
            var ClassIIGasGiant = new PlanetClass("ClassIIGasGiant");
            var ClassIIIGasGiant = new PlanetClass("ClassIIIGasGiant");
            var ClassIVGasGiant = new PlanetClass("ClassIVGasGiant");
            var ClassVGasGiant = new PlanetClass("ClassVGasGiant");
            var WaterGiant = new PlanetClass("WaterGiant");
            var WaterGiantWithLife = new PlanetClass("WaterGiantWithLife");
            var WaterWorld = new PlanetClass("WaterWorld");
        }

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
            normalizedEDName = normalizedEDName.Replace("earthlikeworld", "earthlikebody"); // EDDB uses "earth-like world" while the journal uses "Earthlike body". Fix that here.
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
        public string invariantName => GetInvariantString(edName);
        public string localizedName => GetLocalizedString(edName);
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
