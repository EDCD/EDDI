using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A star or planet
    /// </summary>
    public class Body
    {
        /// <summary>The ID of this body in the star system</summary>
        public long? bodyId { get; set; }
        
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

        /// <summary>The estimated value of the body</summary>
        public long? estimatedvalue => estimateValue();

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

        public Body()
        { }

        [JsonIgnore]
        private List<IDictionary<string, object>> _parents;

        // Star-specific items

        /// <summary>The age of the body, in millions of years</summary>
        public long? age { get; set; }

        /// <summary>If this body is the main star</summary>
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
        public string chromaticity => starClass?.chromaticity?.localizedName; // For use with Cottle
        public decimal? luminosity => StarClass.luminosity(absolutemagnitude);
        public decimal? radiusprobability => starClass == null ? null : StarClass.sanitiseCP(starClass.stellarRadiusCP(solarradius));
        public decimal? massprobability => starClass == null ? null : StarClass.sanitiseCP(starClass.stellarMassCP(solarmass));
        public decimal? tempprobability => starClass == null ? null : StarClass.sanitiseCP(starClass.tempCP(temperature));
        public decimal? ageprobability => starClass == null ? null : StarClass.sanitiseCP(starClass.ageCP(age));
        /// <summary>The solar radius of the star, compared to Sol</summary>
        public decimal? solarradius => StarClass.solarradius(radius);
        /// <summary>Minimum estimated single-star habitable zone (target black body temperature of 315°K / 42°C / 107°F or less, radius in km)</summary>
        public decimal? estimatedhabzoneinner => solarmass > 0 && radius > 0 && temperature > 0 ? 
            (decimal?)StarClass.DistanceFromStarForTemperature(StarClass.maxHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature)) : null;
        /// <summary>Maximum estimated single-star habitable zone (target black body temperature of 223.15°K / -50°C / -58°F or more, radius in km)</summary>
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
        }

        // Estimated exploration value calculations
        private long? estimateValue()
        {
            return 0;
            // throw new NotImplementedException();
        }

        private long? estimateStarValue(bool detailedScan)
        {
            // Scan value calculation constants
            const double dssDivider = 2.4;
            const double scanDivider = 66.25;

            // Credit to MattG's thread at https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae for scan value formulas
            // 'bodyDataConstant' is a derived constant from MattG's thread for calculating scan values.
            int baseValue = 2880;
            double value;

            // Override constants for specific types of bodies
            if ((stellarclass == "H") || (stellarclass == "N"))
            {
                // Black holes and Neutron stars
                baseValue = 54309;
            }
            else if (stellarclass.StartsWith("D") && (stellarclass.Length <= 3))
            {
                // White dwarves
                baseValue = 33737;
            }

            // Calculate exploration scan values
            value = baseValue + ((double)solarmass * baseValue / scanDivider);

            if (detailedScan == false)
            {
                value = value / dssDivider;
            }

            return (long?)Math.Round(value, 0);
        }

        private long? estimateBodyValue(bool detailedScan)
        {
            // Scan value calculation constants
            const double dssDivider = 2.4;
            const double scanDivider = 5.3;
            const double scanMultiplier = 3;
            const double scanPower = 0.199977;

            // Credit to MattG's thread at https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae for scan value formulas
            int baseTypeValue = 720;
            int terraValue = 0;
            bool terraformable = false;

            // Override constants for specific types of bodies
            if (terraformState.edname == "Terraformable")
            {
                terraformable = true;
            }
            if (planetClass.edname == "AmmoniaWorld")
            {
                // Ammonia worlds
                baseTypeValue = 232619;
            }
            else if (planetClass.edname == "EarthLikeBody")
            {
                // Earth-like worlds
                baseTypeValue = 155581;
                terraValue = 279088;
            }
            else if (planetClass.edname == "WaterWorld")
            {
                // Water worlds
                baseTypeValue = 155581;
                if (terraformable)
                {
                    terraValue = 279088;
                }
            }
            else if (planetClass.edname == "MetalRichBody")
            {
                // Metal rich worlds
                baseTypeValue = 52292;
            }
            else if (planetClass.edname == "HighMetalContentBody")
            {
                // High metal content worlds
                baseTypeValue = 23168;
                if (terraformable)
                {
                    terraValue = 241607;
                }
            }
            else if (planetClass.edname == "RockyBody")
            {
                // Rocky worlds
                if (terraformable)
                {
                    terraValue = 223971;
                }
            }
            else if (planetClass.edname == "ClassIGasGiant")
            {
                // Class I gas giants
                baseTypeValue = 3974;
            }
            else if (planetClass.edname == "ClassIIGasGiant")
            {
                // Class II gas giants
                baseTypeValue = 23168;
            }

            // Calculate exploration scan values
            double baseValue = baseTypeValue + (scanMultiplier * baseTypeValue * Math.Pow((double)earthmass, scanPower) / scanDivider);
            double terraBonusValue = terraValue + (scanMultiplier * terraValue * Math.Pow((double)earthmass, scanPower) / scanDivider);
            double value = baseValue + terraBonusValue;

            if (detailedScan == false)
            {
                value = value / dssDivider;
            }
            return (long?)Math.Round(value, 0);
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
            _additionalData = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
