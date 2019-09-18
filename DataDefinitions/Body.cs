using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A star or planet
    /// </summary>
    public class Body
    {
        /// <summary>The ID of this body in the star system</summary>
        public long? bodyId { get; set; }

        public static int CompareById(Body lhs, Body rhs) => Math.Sign((lhs.bodyId - rhs.bodyId) ?? 0);

        /// <summary>The ID of this body in EDDB</summary>
        public long? EDDBID { get; set; }

        /// <summary>The ID of this body in EDSM</summary>
        public long? EDSMID { get; set; }

        /// <summary>The localized type of the body </summary>
        [JsonIgnore, Obsolete("For use with Cottle. Please use bodyType instead.")]
        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        /// <summary>The body type of the body (e.g. Star or Planet)</summary>
        [JsonProperty("Type")]
        public BodyType bodyType { get; set; } = BodyType.None;

        /// <summary>The name of the body</summary>
        [JsonProperty("name"), JsonRequired]
        public string bodyname { get; set; }

        /// <summary>The short name of the body</summary>
        [JsonIgnore]
        public string shortname => (systemname == null || bodyname == systemname) ? bodyname : bodyname.Replace(systemname, "").Trim();

        /// <summary>The name of the system in which the body resides</summary>
        public string systemname { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        public long? systemAddress { get; set; }

        /// <summary>The ID of the system associated with this body in EDDB</summary>
        public long? systemEDDBID { get; set; }

        /// <summary>The distance of the body from the arrival star, in light seconds </summary>
        public decimal? distance { get; set; }

        /// <summary>The surface temperature of the body, in Kelvin</summary>
        public decimal? temperature { get; set; }

        /// <summary>The radius of the body, in km</summary>
        public decimal? radius { get; set; }

        /// <summary>The body's rings</summary>
        public List<Ring> rings { get; set; }

        // Scan data

        /// <summary>Whether we're the first commander to discover this body</summary>
        public bool alreadydiscovered { get; set; }

        /// <summary>When we scanned this object, if we have (DateTime)</summary>
        public DateTime? scanned { get; set; }

        /// <summary>Whether we're the first commander to map this body</summary>
        public bool alreadymapped { get; set; }

        /// <summary>When we mapped this object, if we have (DateTime)</summary>
        public DateTime? mapped { get; set; }

        /// <summary>Whether we received an efficiency bonus when mapping this body</summary>
        public bool mappedEfficiently { get; set; }

        /// <summary>The estimated value of the body</summary>
        [JsonIgnore]
        public long estimatedvalue => scanned == null ? 0 :
            solarmass == null ? estimateBodyValue() : estimateStarValue();

        // Orbital characteristics

        /// <summary>The argument of periapsis, in degrees</summary>
        public decimal? periapsis { get; set; }

        /// <summary>The axial tilt, in degrees</summary>
        public decimal? tilt { get; set; }

        /// <summary>The orbital eccentricity of the planet</summary>
        public decimal? eccentricity { get; set; }

        /// <summary>The orbital inclination of the body, in degrees</summary>
        public decimal? inclination { get; set; }

        /// <summary>The orbital period of the body, in days</summary>
        public decimal? orbitalperiod { get; set; }

        /// <summary>The rotational period of the body, in days</summary>
        public decimal? rotationalperiod { get; set; }

        /// <summary>The semi-major axis of the body, in light seconds</summary>
        public decimal? semimajoraxis { get; set; }

        /// <summary>The parent bodies to this body, if any</summary>
        public List<IDictionary<string, object>> parents
        {
            get
            {
                return _parents;
            }
            set
            {
                if (bodyType == null)
                {
                    if (planetClass != null)
                    {
                        if (value.Exists(p => p.ContainsKey("Planet")))
                        {
                            bodyType = BodyType.FromEDName("Moon");
                        }
                        else if (value.Exists(p => p.ContainsKey("Star")))
                        {
                            bodyType = BodyType.FromEDName("Planet");
                        }
                    }
                    else
                    {
                        bodyType = BodyType.FromEDName("Star");
                    }
                }
                _parents = value;
            }
        }

        /// <summary> Density in Kg per cubic meter </summary>
        [JsonIgnore]
        public decimal? density
        {
            get { return GetDensity(); }
            set { _density = value; }
        }
        [JsonIgnore]
        private decimal? _density;

        public Body()
        { }

        [JsonIgnore]
        private List<IDictionary<string, object>> _parents;

        // Additional calculated statistics
        [JsonIgnore]
        public decimal? massprobability => Probability.CumulativeP(starClass == null ? planetClass.massdistribution : starClass.massdistribution, starClass == null ? earthmass : solarmass);
        [JsonIgnore]
        public decimal? radiusprobability => Probability.CumulativeP(starClass == null ? planetClass.radiusdistribution : starClass.radiusdistribution, starClass == null ? radius : solarradius);
        [JsonIgnore]
        public decimal? tempprobability => Probability.CumulativeP(starClass == null ? planetClass.tempdistribution : starClass.tempdistribution, temperature);
        [JsonIgnore]
        public decimal? orbitalperiodprobability => Probability.CumulativeP(starClass == null ? planetClass.orbitalperioddistribution : starClass.orbitalperioddistribution, orbitalperiod);
        [JsonIgnore]
        public decimal? semimajoraxisprobability => Probability.CumulativeP(starClass == null ? planetClass.semimajoraxisdistribution : starClass.semimajoraxisdistribution, semimajoraxis);
        [JsonIgnore]
        public decimal? eccentricityprobability => Probability.CumulativeP(starClass == null ? planetClass.eccentricitydistribution : starClass.eccentricitydistribution, eccentricity);
        [JsonIgnore]
        public decimal? inclinationprobability => Probability.CumulativeP(starClass == null ? planetClass.inclinationdistribution : starClass.inclinationdistribution, inclination);
        [JsonIgnore]
        public decimal? periapsisprobability => Probability.CumulativeP(starClass == null ? planetClass.periapsisdistribution : starClass.periapsisdistribution, periapsis);
        [JsonIgnore]
        public decimal? rotationalperiodprobability => Probability.CumulativeP(starClass == null ? planetClass.rotationalperioddistribution : starClass.rotationalperioddistribution, rotationalperiod);
        [JsonIgnore]
        public decimal? tiltprobability => Probability.CumulativeP(starClass == null ? planetClass.tiltdistribution : starClass.tiltdistribution, tilt);
        [JsonIgnore]
        public decimal? densityprobability => Probability.CumulativeP(starClass == null ? planetClass.densitydistribution : starClass.densitydistribution, density);

        // Star-specific items

        /// <summary>The age of the body, in millions of years</summary>
        public long? age { get; set; }

        /// <summary>If this body is the main star</summary>
        [JsonIgnore]
        public bool? mainstar => distance == 0 ? true : false;

        /// <summary>The stellar class of the star</summary>
        public string stellarclass { get; set; }

        /// <summary>The stellar subclass of the star (0-9)</summary>
        public int? stellarsubclass { get; set; }

        /// <summary>The Luminosity Class of the Star (since 2.4)</summary>
        public string luminosityclass { get; set; }

        /// <summary>The solar mass of the star</summary>
        public decimal? solarmass { get; set; }

        /// <summary>The absolute magnitude of the star</summary> 
        public decimal? absolutemagnitude { get; set; }

        /// <summary>Class information about the star</summary> 
        public StarClass starClass => StarClass.FromEDName(stellarclass);

        // Additional calculated star information
        [JsonIgnore]
        public bool scoopable => "KGBFOAM".Contains(stellarclass ?? String.Empty);
        [JsonIgnore]
        public string chromaticity => starClass?.chromaticity?.localizedName; // For use with Cottle
        [JsonIgnore]
        public decimal? luminosity => StarClass.luminosity(absolutemagnitude);
        /// <summary>The solar radius of the star, compared to Sol</summary>
        [JsonIgnore]
        public decimal? solarradius => StarClass.solarradius(radius);
        /// <summary>Minimum estimated single-star habitable zone (target black body temperature of 315°K / 42°C / 107°F or less, radius in km)</summary>
        [JsonIgnore]
        public decimal? estimatedhabzoneinner => solarmass > 0 && radius > 0 && temperature > 0 ?
            (decimal?)StarClass.DistanceFromStarForTemperature(StarClass.maxHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature)) : null;
        /// <summary>Maximum estimated single-star habitable zone (target black body temperature of 223.15°K / -50°C / -58°F or more, radius in km)</summary>
        [JsonIgnore]
        public decimal? estimatedhabzoneouter => solarmass > 0 && radius > 0 && temperature > 0 ?
            (decimal?)StarClass.DistanceFromStarForTemperature(StarClass.minHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature)) : null;

        /// <summary> Star definition </summary>
        public Body(string bodyName, long? bodyId, List<IDictionary<string, object>> parents, decimal? distanceLs, string stellarclass, int? stellarsubclass, decimal? solarmass, decimal radiusKm, decimal? absolutemagnitude, long? ageMegaYears, decimal? temperatureKelvin, string luminosityclass, decimal? semimajoraxisLs, decimal? eccentricity, decimal? orbitalinclinationDegrees, decimal? periapsisDegrees, decimal? orbitalPeriodDays, decimal? rotationPeriodDays, decimal? axialTiltDegrees, List<Ring> rings, bool alreadydiscovered, bool alreadymapped, string systemName = null, long? systemAddress = null)
        {
            this.bodyname = bodyName;
            this.radius = radiusKm;
            this.bodyType = BodyType.FromEDName("Star");
            this.rings = rings;
            this.temperature = temperatureKelvin;
            this.bodyId = bodyId;

            // Star specific items
            this.stellarclass = stellarclass;
            this.stellarsubclass = stellarsubclass;
            this.solarmass = solarmass;
            this.absolutemagnitude = absolutemagnitude;
            this.luminosityclass = luminosityclass;
            this.age = ageMegaYears;
            this.landable = false;
            this.tidallylocked = false;

            // Orbital characteristics
            this.distance = distanceLs;
            this.parents = parents;
            this.orbitalperiod = orbitalPeriodDays;
            this.rotationalperiod = rotationPeriodDays;
            this.semimajoraxis = semimajoraxisLs;
            this.eccentricity = eccentricity;
            this.inclination = orbitalinclinationDegrees;
            this.periapsis = periapsisDegrees;
            this.tilt = axialTiltDegrees;

            // System details
            this.systemname = systemName;
            this.systemAddress = systemAddress;

            // Scan details
            this.alreadydiscovered = alreadydiscovered;
            this.alreadymapped = alreadymapped;

            // Other calculations
            this.density = GetDensity();
        }

        // Additional calculated star statistics
        [JsonIgnore]
        public decimal? ageprobability => starClass == null ? null : Probability.CumulativeP(starClass.agedistribution, age);

        [JsonIgnore]
        public decimal? absolutemagnitudeprobability => starClass == null ? null : Probability.CumulativeP(starClass.absolutemagnitudedistribution, absolutemagnitude);

        private long estimateStarValue()
        {
            // Credit to MattG's thread at https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae for scan value formulas

            if (stellarclass is null || solarmass is null)
            {
                return 0;
            }

            // Scan value calculation constants
            const double scanDivider = 66.25;

            double k = 1200; // base value
            double result;

            // Override constants for specific types of bodies
            if ((stellarclass == "H") || (stellarclass == "N"))
            {
                // Black holes and Neutron stars
                k = 22628;
            }
            else if (stellarclass == "SuperMassiveBlackHole")
            {
                // Supermassive black hole
                // this is applying the same scaling to the 3.2 value as a normal black hole, not confirmed in game
                k = 33.5678;
            }
            else if (stellarclass.StartsWith("D") && (stellarclass.Length <= 3))
            {
                // White dwarves
                k = 14057;
            }

            // Calculate exploration scan values - (k + (m * k / 66.25))
            result = k + ((double)solarmass * k / scanDivider);
            return (long)Math.Round(result);
        }

        // Body-specific items

        /// <summary>The atmosphere class</summary>
        public AtmosphereClass atmosphereclass { get; set; } = AtmosphereClass.None;

        /// <summary>The atmosphere</summary>
        [JsonIgnore, Obsolete("Please use AtmosphereClass instead")]
        public string atmosphere => (atmosphereclass ?? AtmosphereClass.None).localizedName;

        /// <summary>The atmosphere's composition</summary>
        public List<AtmosphereComposition> atmospherecompositions { get; set; } = new List<AtmosphereComposition>();

        /// <summary>If this body can be landed upon</summary>
        public bool? landable { get; set; }

        /// <summary>If this body is tidally locked</summary>
        public bool? tidallylocked { get; set; }

        /// <summary>The earth mass of the planet</summary>
        public decimal? earthmass { get; set; }

        /// <summary>The gravity of the planet, in G's</summary>
        public decimal? gravity { get; set; }

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

        /// <summary> Planet or Moon definition </summary>
        public Body(string bodyName, long? bodyId, List<IDictionary<string, object>> parents, decimal? distanceLs, bool? tidallylocked, TerraformState terraformstate, PlanetClass planetClass, AtmosphereClass atmosphereClass, List<AtmosphereComposition> atmosphereCompositions, Volcanism volcanism, decimal? earthmass, decimal? radiusKm, decimal gravity, decimal? temperatureKelvin, decimal? pressureAtm, bool? landable, List<MaterialPresence> materials, List<SolidComposition> solidCompositions, decimal? semimajoraxisLs, decimal? eccentricity, decimal? orbitalinclinationDegrees, decimal? periapsisDegrees, decimal? orbitalPeriodDays, decimal? rotationPeriodDays, decimal? axialtiltDegrees, List<Ring> rings, ReserveLevel reserveLevel, bool alreadydiscovered, bool alreadymapped, string systemName = null, long? systemAddress = null)
        {
            this.bodyname = bodyName;
            this.bodyType = (bool)parents?.Exists(p => p.ContainsKey("Planet"))
                        ? BodyType.FromEDName("Moon") : BodyType.FromEDName("Planet");
            this.rings = rings;
            this.temperature = temperature;
            this.bodyId = bodyId;

            // Planet or Moon specific items
            this.planetClass = planetClass;
            this.earthmass = earthmass;
            this.radius = radiusKm;
            this.gravity = gravity;
            this.temperature = temperatureKelvin;
            this.pressure = pressureAtm;
            this.tidallylocked = tidallylocked;
            this.landable = landable;
            this.atmosphereclass = atmosphereClass;
            this.atmospherecompositions = atmosphereCompositions;
            this.solidcompositions = solidCompositions;
            this.volcanism = volcanism;
            this.reserveLevel = reserveLevel;
            this.materials = materials;
            this.terraformState = terraformstate;

            // Orbital characteristics
            this.distance = distanceLs;
            this.parents = parents;
            this.orbitalperiod = orbitalPeriodDays;
            this.rotationalperiod = rotationPeriodDays;
            this.semimajoraxis = semimajoraxisLs;
            this.eccentricity = eccentricity;
            this.inclination = orbitalinclinationDegrees;
            this.periapsis = periapsisDegrees;
            this.tilt = axialtiltDegrees;

            // System details
            this.systemname = systemName; // This is needed to derive the "shortname" property
            this.systemAddress = systemAddress;

            // Scan details
            this.alreadydiscovered = alreadydiscovered;
            this.alreadymapped = alreadymapped;

            // Other calculations
            this.density = GetDensity();
        }

        // Additional calculated planet and moon statistics
        [JsonIgnore]
        public decimal? gravityprobability => Probability.CumulativeP(starClass == null ? planetClass.gravitydistribution : null, gravity);
        [JsonIgnore]
        public decimal? pressureprobability => Probability.CumulativeP(starClass == null ? planetClass.pressuredistribution : null, pressure);

        private long estimateBodyValue()
        {
            // Credit to MattG's thread at https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae for scan value formulas

            if (earthmass is null || terraformState is null || planetClass is null)
            {
                return 0;
            }

            // Scan value calculation constants
            const double q = 0.56591828;
            const double scanPower = 0.2;
            const double scanMinValue = 500;
            const double firstDiscoveryMultiplier = 2.6;
            const double efficientMappingMultiplier = 1.25;

            bool terraformable = terraformState.edname == "Terraformable" || terraformState.edname == "Terraformed";
            int k = 300; // base value
            int k_terraformable = 93328;
            double mappingMultiplier = 1;

            // Override constants for specific types of bodies
            if (planetClass.edname == "AmmoniaWorld")
            {
                // Ammonia worlds
                k = 96932;
            }
            else if (planetClass.edname == "EarthLikeBody" || planetClass.edname == "WaterWorld")
            {
                // Earth-like & water worlds
                k = 64831;
                k_terraformable = 116295;
            }
            else if (planetClass.edname == "MetalRichBody")
            {
                // Metal rich worlds
                k = 21790;
            }
            else if (planetClass.edname == "HighMetalContentBody")
            {
                // High metal content worlds
                k = 9654;
                k_terraformable = 100677;
            }
            else if (planetClass.edname == "SudarskyClassIGasGiant")
            {
                // Class I gas giants
                k = 1656;
            }
            else if (planetClass.edname == "SudarskyClassIIGasGiant")
            {
                // Class II gas giants
                k = 9654;
            }

            // Terraformability is a scale from 0-100%, but since we don't know the % we'll assume 100% for the time being.
            k = terraformable ? (k + k_terraformable) : k;

            if (mapped != null)
            {
                if (!alreadydiscovered && !alreadymapped) // First to discover and first to map
                {
                    mappingMultiplier = 3.699622554;
                }
                else if (!alreadymapped) // Not first to discover but first to map
                {
                    mappingMultiplier = 8.0956;
                }
                else // Not first to discover or first to map
                {
                    mappingMultiplier = 3.3333333333;
                }
                mappingMultiplier *= (mappedEfficiently) ? efficientMappingMultiplier : 1;
            }

            // Calculate exploration scan values
            double result = Math.Max(scanMinValue, (k + (k * q * Math.Pow((double)earthmass, scanPower))) * mappingMultiplier);
            result *= (!alreadydiscovered) ? firstDiscoveryMultiplier : 1;
            return (long)Math.Round(result);
        }

        // Miscellaneous and legacy properties and methods

        /// <summary> the last time the information present changed (in the data source) </summary>
        public long? updatedat { get; set; }

        // Deprecated properties (preserved for backwards compatibility with Cottle and database stored values)

        // This is a key for legacy json files that cannot be changed without breaking backwards compatibility. 
        [JsonIgnore, Obsolete("Please use bodyname instead.")]
        public string name => bodyname;

        [JsonIgnore, Obsolete("Please use bodytype instead - type creates a collision with Event.type.")]
        public string type => bodytype;

        [JsonIgnore, Obsolete("Please use bodyType instead")]
        public BodyType Type => bodyType;

        // Density
        private decimal? GetDensity()
        {
            double massKg = 0;
            double radiusM = 0;

            if (bodyType != null && bodyType?.invariantName != "Star")
            {
                double radiusKm = Convert.ToDouble(radius);
                radiusM = (radiusKm * 1000);

                double earthmasses = Convert.ToDouble(earthmass);
                massKg = earthmasses * Constants.earthMassKg;
            }
            else if (bodyType?.invariantName == "Star")
            {
                double radiusKm = Convert.ToDouble(radius);
                radiusM = (radiusKm * 1000);

                double solarMasses = Convert.ToDouble(solarmass);
                massKg = solarMasses * Constants.solMassKg;
            }
            if (massKg > 0 && radiusM > 0)
            {
                double cubicMeters = 4 / 3 * Math.PI * Math.Pow(radiusM, 3);
                return (decimal?)(massKg / cubicMeters);
            }
            else { return null; }
        }

        // Convert legacy data

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (bodyType == null)
            {
                _additionalData.TryGetValue("Type", out JToken var);
                BodyType type = var.ToObject<BodyType>();
                bodyType = type;
            }
            if (absolutemagnitude == null)
            {
                _additionalData.TryGetValue("absoluteMagnitude", out JToken val);
                if (val != null)
                {
                    decimal? absoluteMagnitude = val.ToObject<decimal?>();
                    absolutemagnitude = absoluteMagnitude;
                }
            }

            // Calculate our density if possible to do so.
            density = GetDensity();

            _additionalData = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
