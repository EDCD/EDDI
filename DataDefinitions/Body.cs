﻿using Newtonsoft.Json;
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
    public class Body : INotifyPropertyChanged
    {
        /// <summary>Information for Surface Signals (biology/geology)</summary>
        public SurfaceSignals surfaceSignals { get; set; }

        /// <summary>The ID of this body in the star system</summary>
        public long? bodyId { get; set; }

        /// <summary>The ID of this body in EDSM</summary>
        public long? EDSMID { get; set; }

        /// <summary>The localized type of the body </summary>
        [PublicAPI, JsonIgnore, Obsolete("For use with Cottle. Please use bodyType instead.")]
        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        /// <summary>The body type of the body (e.g. Star or Planet)</summary>
        [JsonProperty("Type")]
        public BodyType bodyType { get; set; } = BodyType.None;

        /// <summary>The name of the body</summary>
        [PublicAPI, JsonProperty("name")]
        public string bodyname { get; set; }

        /// <summary>The short name of the body</summary>
        [PublicAPI, JsonIgnore]
        public string shortname => GetShortName(bodyname, systemname);

        /// <summary>The name of the system in which the body resides</summary>
        [PublicAPI]
        public string systemname { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        public ulong? systemAddress { get; set; }

        /// <summary>The distance of the body from the arrival star, in light seconds </summary>
        [PublicAPI]
        public decimal? distance { get; set; }

        /// <summary>The surface temperature of the body, in Kelvin</summary>
        [PublicAPI]
        public decimal? temperature { get; set; }

        /// <summary>The radius of the body, in km</summary>
        [PublicAPI]
        public decimal? radius { get; set; }

        /// <summary>The body's rings</summary>
        [PublicAPI]
        public List<Ring> rings { get; set; }

        // Scan data

        /// <summary>Whether we're the first commander to discover this body</summary>
        [PublicAPI]
        public bool? alreadydiscovered { get; set; }

        /// <summary>When we scanned this object, if we have</summary>
        [PublicAPI, JsonIgnore]
        public long? scanned => scannedDateTime is null 
            ? null 
            : (long?)Dates.fromDateTimeToSeconds((DateTime)scannedDateTime);

        [JsonProperty(nameof(scanned))]
        public DateTime? scannedDateTime
        {
            get => _scannedDateTime;
            set { _scannedDateTime = value; OnPropertyChanged(); }
        }
        [JsonIgnore] private DateTime? _scannedDateTime;

        /// <summary>Whether we're the first commander to map this body</summary>
        [PublicAPI]
        public bool? alreadymapped { get; set; }

        /// <summary>When we mapped this object, if we have</summary>
        [PublicAPI, JsonIgnore]
        public long? mapped => mappedDateTime is null
            ? null
            : (long?)Dates.fromDateTimeToSeconds((DateTime)mappedDateTime);

        [JsonProperty(nameof(mapped))]
        public DateTime? mappedDateTime
        {
            get => _mappedDateTime;
            set { _mappedDateTime = value; OnPropertyChanged(); }
        }
        [JsonIgnore] private DateTime? _mappedDateTime;

        /// <summary>Whether we received an efficiency bonus when mapping this body</summary>
        public bool mappedEfficiently
        {
            get => _mappedEfficiently;
            set { _mappedEfficiently = value; OnPropertyChanged(); }
        }
        [JsonIgnore] private bool _mappedEfficiently;

        /// <summary>The estimated value of the body</summary>
        [PublicAPI, JsonIgnore]
        public long estimatedvalue => scannedDateTime == null 
            ? 0 
            : solarmass == null 
                ? estimateBodyValue(mappedDateTime != null, mappedEfficiently) 
                : estimateStarValue();

        /// <summary>The estimated maximum value of the body</summary>
        [PublicAPI, JsonIgnore]
        public long maxestimatedvalue => scannedDateTime == null 
            ? 0 
            : solarmass == null
                ? estimateBodyValue(true, true)
                : estimateStarValue();

        // Orbital characteristics

        /// <summary>The argument of periapsis, in degrees</summary>
        [PublicAPI]
        public decimal? periapsis { get; set; }

        /// <summary>The axial tilt, in degrees</summary>
        [PublicAPI]
        public decimal? tilt { get; set; }

        /// <summary>The orbital eccentricity of the planet</summary>
        [PublicAPI]
        public decimal? eccentricity { get; set; }

        /// <summary>The orbital inclination of the body, in degrees</summary>
        [PublicAPI]
        public decimal? inclination { get; set; }

        /// <summary>The orbital period of the body, in days</summary>
        [PublicAPI]
        public decimal? orbitalperiod { get; set; }

        /// <summary>The rotational period of the body, in days</summary>
        [PublicAPI]
        public decimal? rotationalperiod { get; set; }

        /// <summary>The semi-major axis of the body, in light seconds</summary>
        [PublicAPI]
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
                            bodyType = BodyType.Moon;
                        }
                        else if (value.Exists(p => p.ContainsKey("Star")))
                        {
                            bodyType = BodyType.Planet;
                        }
                    }
                    else
                    {
                        bodyType = BodyType.Star;
                    }
                }
                _parents = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] private List<IDictionary<string, object>> _parents;

        /// <summary> Density in Kg per cubic meter </summary>
        [PublicAPI, JsonIgnore]
        public decimal? density => GetDensity();

        public Body()
        {
            // TODO:#2212........[Temporary initialization of SurfaceSignals]
            surfaceSignals = new SurfaceSignals();
        }

        // Additional calculated statistics

        [PublicAPI, JsonIgnore]
        public decimal? massprobability => Probability.CumulativeP(starClass == null ? planetClass.massdistribution : starClass.massdistribution, starClass == null ? earthmass : solarmass);
        
        [PublicAPI, JsonIgnore]
        public decimal? radiusprobability => Probability.CumulativeP(starClass == null ? planetClass.radiusdistribution : starClass.radiusdistribution, starClass == null ? radius : solarradius);
        
        [PublicAPI, JsonIgnore]
        public decimal? tempprobability => Probability.CumulativeP(starClass == null ? planetClass.tempdistribution : starClass.tempdistribution, temperature);
        
        [PublicAPI, JsonIgnore]
        public decimal? orbitalperiodprobability => Probability.CumulativeP(starClass == null ? planetClass.orbitalperioddistribution : starClass.orbitalperioddistribution, orbitalperiod);
        
        [PublicAPI, JsonIgnore]
        public decimal? semimajoraxisprobability => Probability.CumulativeP(starClass == null ? planetClass.semimajoraxisdistribution : starClass.semimajoraxisdistribution, semimajoraxis);
        
        [PublicAPI, JsonIgnore]
        public decimal? eccentricityprobability => Probability.CumulativeP(starClass == null ? planetClass.eccentricitydistribution : starClass.eccentricitydistribution, eccentricity);
        
        [PublicAPI, JsonIgnore]
        public decimal? inclinationprobability => Probability.CumulativeP(starClass == null ? planetClass.inclinationdistribution : starClass.inclinationdistribution, inclination);
        
        [PublicAPI, JsonIgnore]
        public decimal? periapsisprobability => Probability.CumulativeP(starClass == null ? planetClass.periapsisdistribution : starClass.periapsisdistribution, periapsis);
        
        [PublicAPI, JsonIgnore]
        public decimal? rotationalperiodprobability => Probability.CumulativeP(starClass == null ? planetClass.rotationalperioddistribution : starClass.rotationalperioddistribution, rotationalperiod);
        
        [PublicAPI, JsonIgnore]
        public decimal? tiltprobability => Probability.CumulativeP(starClass == null ? planetClass.tiltdistribution : starClass.tiltdistribution, tilt);
        
        [PublicAPI, JsonIgnore]
        public decimal? densityprobability => Probability.CumulativeP(starClass == null ? planetClass.densitydistribution : starClass.densitydistribution, density);

        // Star-specific items

        /// <summary>The age of the body, in millions of years</summary>
        [PublicAPI]
        public long? age { get; set; }

        /// <summary>If this body is the main star</summary>
        [PublicAPI, JsonIgnore]
        public bool? mainstar => distance == 0;

        /// <summary>The stellar class of the star</summary>
        [PublicAPI]
        public string stellarclass { get; set; }

        /// <summary>The stellar subclass of the star (0-9)</summary>
        [PublicAPI]
        public int? stellarsubclass { get; set; }

        /// <summary>The Luminosity Class of the Star (since 2.4)</summary>
        public string luminosityclass { get; set; }

        /// <summary>The solar mass of the star</summary>
        [PublicAPI]
        public decimal? solarmass { get; set; }

        /// <summary>The absolute magnitude of the star</summary> 
        [PublicAPI]
        public decimal? absolutemagnitude { get; set; }

        /// <summary>Class information about the star</summary> 
        public StarClass starClass => StarClass.FromEDName(stellarclass);

        // Additional calculated star information

        [PublicAPI, JsonIgnore]
        public bool scoopable => !string.IsNullOrEmpty(stellarclass) 
                                 && "KGBFOAM".Contains(stellarclass.Split('_')[0]);

        [PublicAPI, JsonIgnore]
        public string chromaticity => starClass?.chromaticity?.localizedName; // For use with Cottle

        [PublicAPI, JsonIgnore]
        public decimal? luminosity => StarClass.luminosity(absolutemagnitude);

        /// <summary>The solar radius of the star, compared to Sol</summary>
        [PublicAPI, JsonIgnore]
        public decimal? solarradius => StarClass.solarradius(radius);

        /// <summary>Minimum estimated single-star habitable zone (target black body temperature of 315°K / 42°C / 107°F or less, radius in km)</summary>
        [PublicAPI, JsonIgnore]
        public decimal? estimatedhabzoneinner => solarmass > 0 && radius > 0 && temperature > 0 ?
            (decimal?)StarClass.DistanceFromStarForTemperature(StarClass.maxHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature)) : null;

        /// <summary>Maximum estimated single-star habitable zone (target black body temperature of 223.15°K / -50°C / -58°F or more, radius in km)</summary>
        [PublicAPI, JsonIgnore]
        public decimal? estimatedhabzoneouter => solarmass > 0 && radius > 0 && temperature > 0 ?
            (decimal?)StarClass.DistanceFromStarForTemperature(StarClass.minHabitableTempKelvin, Convert.ToDouble(radius), Convert.ToDouble(temperature)) : null;

        /// <summary> Star definition </summary>
        public Body(string bodyName, long? bodyId, string systemName, ulong systemAddress, List<IDictionary<string, object>> parents, decimal? distanceLs, string stellarclass, int? stellarsubclass, decimal? solarmass, decimal radiusKm, decimal? absolutemagnitude, long? ageMegaYears, decimal? temperatureKelvin, string luminosityclass, decimal? semimajoraxisLs, decimal? eccentricity, decimal? orbitalinclinationDegrees, decimal? periapsisDegrees, decimal? orbitalPeriodDays, decimal? rotationPeriodDays, decimal? axialTiltDegrees, List<Ring> rings, bool? alreadydiscovered, bool? alreadymapped)
        {
            // TODO:#2212........[temporary initialization of SurfaceSignals]
            surfaceSignals = new SurfaceSignals();

            this.bodyname = bodyName;
            this.radius = radiusKm;
            this.bodyType = BodyType.Star;
            this.rings = rings ?? new List<Ring>();
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

        // Additional calculated star statistics
        [PublicAPI, JsonIgnore]
        public decimal? ageprobability => starClass == null ? null : Probability.CumulativeP(starClass.agedistribution, age);

        [PublicAPI, JsonIgnore]
        public decimal? absolutemagnitudeprobability => starClass == null ? null : Probability.CumulativeP(starClass.absolutemagnitudedistribution, absolutemagnitude);

        private long estimateStarValue()
        {
            // Credit to MattG's thread at https://forums.frontier.co.uk/threads/exploration-value-formulae.232000/ for scan value formulas

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
        [PublicAPI, JsonIgnore, Obsolete("Please use AtmosphereClass instead")]
        public string atmosphere => (atmosphereclass ?? AtmosphereClass.None).localizedName;

        /// <summary>The atmosphere's composition</summary>
        [PublicAPI]
        public List<AtmosphereComposition> atmospherecompositions { get; set; } = new List<AtmosphereComposition>();

        /// <summary>If this body can be landed upon</summary>
        [PublicAPI]
        public bool? landable { get; set; }

        /// <summary>If this body is tidally locked</summary>
        [PublicAPI]
        public bool? tidallylocked { get; set; }

        /// <summary>The earth mass of the planet</summary>
        [PublicAPI]
        public decimal? earthmass { get; set; }

        /// <summary>The gravity of the planet, in G's</summary>
        [PublicAPI]
        public decimal? gravity { get; set; }

        /// <summary>The pressure at the surface of the planet, in Earth atmospheres</summary>
        [PublicAPI]
        public decimal? pressure { get; set; }

        /// <summary>The terraform state (localized name)</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use TerraformState instead")]
        public string terraformstate => (terraformState ?? TerraformState.NotTerraformable).localizedName;

        /// <summary>The terraform state</summary>
        public TerraformState terraformState { get; set; } = TerraformState.NotTerraformable;

        /// <summary>The planet type (localized name)</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use PlanetClass instead")]
        public string planettype => (planetClass ?? PlanetClass.None).localizedName;

        /// <summary>The planet type</summary>
        public PlanetClass planetClass { get; set; } = PlanetClass.None;

        /// <summary>The volcanism</summary>
        [PublicAPI, JsonConverter(typeof(VolcanismConverter))]
        public Volcanism volcanism { get; set; }

        /// <summary>The solid body composition of the body</summary>
        [PublicAPI]
        public List<SolidComposition> solidcompositions { get; set; } = new List<SolidComposition>();

        /// <summary>The materials present at the surface of the body</summary>
        [PublicAPI]
        public List<MaterialPresence> materials { get; set; } = new List<MaterialPresence>();

        /// <summary>The reserve level (localized name)</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use reserveLevel instead")]
        public string reserves => (reserveLevel ?? ReserveLevel.None).localizedName;

        /// <summary>The reserve level</summary>
        public ReserveLevel reserveLevel { get; set; } = ReserveLevel.None;

        /// <summary> Planet or Moon definition </summary>
        public Body(string bodyName, long? bodyId, string systemName, ulong systemAddress, List<IDictionary<string, object>> parents, decimal? distanceLs, bool? tidallylocked, TerraformState terraformstate, PlanetClass planetClass, AtmosphereClass atmosphereClass, List<AtmosphereComposition> atmosphereCompositions, Volcanism volcanism, decimal? earthmass, decimal? radiusKm, decimal gravity, decimal? temperatureKelvin, decimal? pressureAtm, bool? landable, List<MaterialPresence> materials, List<SolidComposition> solidCompositions, decimal? semimajoraxisLs, decimal? eccentricity, decimal? orbitalinclinationDegrees, decimal? periapsisDegrees, decimal? orbitalPeriodDays, decimal? rotationPeriodDays, decimal? axialtiltDegrees, List<Ring> rings, ReserveLevel reserveLevel, bool? alreadydiscovered, bool? alreadymapped)
        {
            this.bodyname = bodyName;
            this.bodyType = (bool)parents?.Exists(p => p.ContainsKey("Planet"))
                        ? BodyType.Moon : BodyType.Planet;
            this.rings = rings ?? new List<Ring>();
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

        // Additional calculated planet and moon statistics
        [PublicAPI, JsonIgnore]
        public decimal? gravityprobability => Probability.CumulativeP(starClass == null ? planetClass.gravitydistribution : null, gravity);

        [PublicAPI, JsonIgnore]
        public decimal? pressureprobability => Probability.CumulativeP(starClass == null ? planetClass.pressuredistribution : null, pressure);

        [PublicAPI, JsonIgnore] // The duration of a solar day on the body, in Earth days
        public decimal? solarday => orbitalperiod - rotationalperiod == 0 ? null : orbitalperiod * rotationalperiod / (orbitalperiod - rotationalperiod);

        [PublicAPI, JsonIgnore] // The ground speed of the parent body's shadow on the surface of the body in meters per second
        public decimal? solarsurfacevelocity => 2 * (decimal)Math.PI * radius * 1000 / (solarday * 86400);

        private long estimateBodyValue(bool isMapped, bool isMappedEfficiently)
        {
            // Credit to MattG's thread at https://forums.frontier.co.uk/threads/exploration-value-formulae.232000/ for scan value formulas
            
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

            int k = 300; // base value
            int k_terraformable = 93328;

            var alreadyDiscovered = (alreadydiscovered ?? true);
            var alreadyMapped = (alreadymapped ?? true); // If we don't know then we'll assume true to underestimate rather than overestimate the value

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
            k = terraformState.edname == "Terraformable" || terraformState.edname == "Terraformed" 
                ? (k + k_terraformable) 
                : k;

            double mappingMultiplier = 1;
            if (isMapped)
            {
                if (!alreadyDiscovered && !alreadyMapped) // First to discover and first to map
                {
                    mappingMultiplier = 3.699622554;
                }
                else if (!alreadyMapped) // Not first to discover but first to map
                {
                    mappingMultiplier = 8.0956;
                }
                else // Not first to discover or first to map
                {
                    mappingMultiplier = 3.3333333333;
                }
            }

            // Calculate exploration scan values
            double value = Math.Max(scanMinValue, (k + (k * q * Math.Pow((double)earthmass, scanPower))) * mappingMultiplier);
            if (isMapped)
            {
                value += ((value * 0.3) > 555) ? value * 0.3 : 555;
                if (isMappedEfficiently)
                {
                    value *= efficientMappingMultiplier;
                }
            }
            value *= !alreadyDiscovered ? firstDiscoveryMultiplier : 1;
            return (long)Math.Round(value);
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
                double cubicMeters = 4.0 / 3 * Math.PI * Math.Pow(radiusM, 3);
                return (decimal?)(massKg / cubicMeters);
            }
            else { return null; }
        }

        // Orbital Velocity required to maintain orbit at a given altitude
        public decimal? GetOrbitalVelocityMetersPerSecond(decimal? altitudeMeters)
        {
            if (earthmass != null && radius != null && altitudeMeters != null)
            {
                var orbitalRadiusMeters = ((radius * 1000) + altitudeMeters);
                return (decimal)Math.Round(Math.Sqrt(Constants.gravitationalConstant * (double)(earthmass * (decimal)Constants.earthMassKg) / (double)orbitalRadiusMeters));
            }
            return null;
        }

        public static string GetShortName(string bodyname, string systemname)
        {
            if (bodyname is null) { return null; }
            return (systemname == null || bodyname == systemname || !bodyname.StartsWith(systemname)) 
                ? bodyname 
                : bodyname?.Replace(systemname, "").Trim();
        }

        public static int CompareById(Body lhs, Body rhs) => Math.Sign((lhs.bodyId - rhs.bodyId) ?? 0);

        #region Legacy data conversions
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (bodyType == null)
            {
                _additionalData.TryGetValue("Type", out JToken var);
                BodyType typ = var?.ToObject<BodyType>();
                bodyType = typ;
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
        #endregion

        #region Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberNameAttribute] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region Newtonsoft ShouldSerialize logic

        // Allow `bodyname` to be omitted when distance is set
        // (so that we can create a temporary main star with a stellar class when
        // the `bodyname` is still unknown)
        public bool ShouldSerializebodyname() => distance is null || !string.IsNullOrEmpty(bodyname);

        #endregion
    }
}
