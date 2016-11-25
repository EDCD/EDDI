using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string type { get; set; }

        /// <summary>The name of the body</summary>
        public string name { get; set;  }

        /// <summary>The name of the system in which the body resides</summary>
        public string systemname { get; set; }

        /// <summary>The age of the body, in millions of years</summary>
        public long? age { get; set; }

        /// <summary>The distance of the body from the arrival star, in light seconds </summary>
        public long? distance { get; set; }

        /// <summary>If this body can be landed upon</summary>
        public bool? landable { get; set; }

        /// <summary>If this body is tidally locked</summary>
        public bool? tidallylocked { get; set; }

        /// <summary>The surface temperature of the body</summary>
        public long? temperature;

        // Star-specific items

        /// <summary>If this body is the main star</summary>
        public bool? mainstar { get; set; }

        /// <summary>The stellar class of the star</summary>
        public string stellarclass;

        /// <summary>The solar mass of the star</summary>
        public decimal? solarmass;

        /// <summary>The solar radius of the star</summary>
        public decimal? solarradius;

        // Body-specific items

        /// <summary>The argument of periapsis</summary>
        public decimal? periapsis;

        /// <summary>The atmosphere</summary>
        public string atmosphere;

        /// <summary>The axial tilt</summary>
        public decimal? tilt;

        /// <summary>The earth mass of the planet</summary>
        public decimal? earthmass;

        /// <summary>The gravity of the planet</summary>
        public decimal? gravity;

        /// <summary>The orbital eccentricity of the planet</summary>
        public decimal? eccentricity;

        /// <summary>The orbital inclination of the planet</summary>
        public decimal? inclination;

        /// <summary>The orbital period of the planet, in days</summary>
        public decimal? orbitalperiod;

        /// <summary>The radius of the planet, in km</summary>
        public long? radius;

        /// <summary>The rotational period of the planet, in days</summary>
        public decimal? rotationalperiod;

        /// <summary>The semi-major axis of the planet, in km</summary>
        public decimal? semimajoraxis;

        /// <summary>The pressure of the planet, in days</summary>
        public decimal? pressure;

        /// <summary>The terraform state</summary>
        public string terraformstate;

        /// <summary>The planet type</summary>
        public string planettype;

        /// <summary>The volcanism</summary>
        public string volcanism;

        // materials
        public List<MaterialPercentage> materials;
        
        /// <summary>
        /// Convert gravity in m/s to g
        /// </summary>
        public static decimal ms2g(decimal gravity)
        {
            return gravity / (decimal)9.80665;
        }
    }

    public class MaterialPercentage
    {
        public string material { get; private set; }
        public decimal percentage { get; private set; }

        [JsonConstructor]
        public MaterialPercentage(string material, decimal percentage)
        {
            this.material = material;
            this.percentage = percentage;
        }

        public MaterialPercentage(Material material, decimal percentage)
        {
            this.material = material.name;
            this.percentage = percentage;
        }
    }
}
