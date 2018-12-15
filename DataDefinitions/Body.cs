using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A star or planet
    /// </summary>
    public class Body
    {
        /// <summary>The ID of this body in EDDB</summary>
        public long? EDDBID { get; set; }

        /// <summary>The ID of this body in EDSM</summary>
        public long? EDSMID { get; set; }

        /// <summary>The type of the body (Star or Planet)</summary>
        [JsonIgnore, Obsolete("Please use BodyType instead")]
        public string type => (Type ?? BodyType.None).localizedName;

        /// <summary>The type of the body (Star or Planet)</summary>
        public BodyType Type { get; set; } = BodyType.None;

        /// <summary>The name of the body</summary>
        public string name { get; set; }

        /// <summary>The short name of the body</summary>
        public string shortname => (systemname == null || name == systemname) ? name : name.Replace(systemname, "").Trim();

        /// <summary>The name of the system in which the body resides</summary>
        public string systemname { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        public long? systemAddress { get; set; }

        /// <summary>The ID of the system associated with this body in EDDB</summary>
        public long? systemEDDBID { get; set; }

        /// <summary>The age of the body, in millions of years</summary>
        public long? age { get; set; }

        /// <summary>The distance of the body from the arrival star, in light seconds </summary>
        public decimal? distance { get; set; }

        /// <summary>If this body can be landed upon</summary>
        public bool? landable { get; set; }

        /// <summary>If this body is tidally locked</summary>
        public bool? tidallylocked { get; set; }

        /// <summary>The surface temperature of the body, in Kelvin</summary>
        public decimal? temperature { get; set; }

        /// <summary>The body's rings</summary>
        public List<Ring> rings { get; set; }

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
        public decimal? estimatedhabzoneinner { get; set; }
        public decimal? estimatedhabzoneouter { get; set; }

        // Body-specific items

        /// <summary>The argument of periapsis, in degrees</summary>
        public decimal? periapsis { get; set; }

        /// <summary>The atmosphere class</summary>
        public AtmosphereClass atmosphereclass { get; set; } = AtmosphereClass.None;

        /// <summary>The atmosphere</summary>
        [JsonIgnore, Obsolete("Please use AtmosphereClass instead")]
        public string atmosphere => (atmosphereclass ?? AtmosphereClass.None).localizedName;

        /// <summary>The atmosphere's composition</summary>
        public List<AtmosphereComposition> atmospherecompositions { get; set; } = new List<AtmosphereComposition>();

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

        /// <summary>The semi-major axis of the planet, in light seconds</summary>
        public decimal? semimajoraxis { get; set; }

        /// <summary>The pressure at the surface of the planet, in Earth atmospheres</summary>
        public decimal? pressure { get; set; }

        /// <summary>The terraform state (localized name)</summary>
        [JsonIgnore, Obsolete("Please use TerraformState instead")]
        public string terraformstate => (terraformState ?? TerraformState.NotTerraformable).localizedName;

        /// <summary>The terraform state</summary>
        public TerraformState terraformState { get; set; } = TerraformState.NotTerraformable;

        /// <summary>The planet type (localized name)</summary>
        [JsonIgnore, Obsolete("Please use PlanetClass instead")]
        public string planettype => (planetClass ?? PlanetClass.None).localizedName;

        /// <summary>The planet type</summary>
        public PlanetClass planetClass { get; set; } = PlanetClass.None;

        /// <summary>The volcanism</summary>
        [JsonConverter(typeof(VolcanismConverter))]
        public Volcanism volcanism { get; set; }

        /// <summary>The solid body composition of the body</summary>
        public List<SolidComposition> solidcompositions { get; set; } = new List<SolidComposition>();

        /// <summary>The materials present at the surface of the body</summary>
        public List<MaterialPresence> materials { get; set; } = new List<MaterialPresence>();

        /// <summary>The reserve level (localized name)</summary>
        [JsonIgnore, Obsolete("Please use SystemReserveLevel instead")]
        public string reserves => (reserveLevel ?? ReserveLevel.None).localizedName;
        /// <summary>The reserve level</summary>
        public ReserveLevel reserveLevel { get; set; } = ReserveLevel.None;

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
            // `estimatedvalue` is only set during scan events.
            if (radius != 0 && radius != null && temperature != 0 && temperature != null)
            {
                // Minimum estimated single-star habitable zone (target black body temperature of 315°K / 42°C / 107°F or less, radius in km)
                estimatedhabzoneinner = StarClass.DistanceFromStarForTemperature(StarClass.maxHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature));

                // Maximum estimated single-star habitable zone (target black body temperature of 223.15°K / -50°C / -58°F or more, radius in km)
                estimatedhabzoneouter = StarClass.DistanceFromStarForTemperature(StarClass.minHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature));
            }
            if (distance != null) { mainstar = distance == 0 ? true : false; }
        }
    }
}
