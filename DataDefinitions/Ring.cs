using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A stellar or planetary ring
    /// </summary>
    public class Ring
    {
        /// <summary>The name of the ring</summary>
        [PublicAPI]
        public string name { get; set; }

        /// <summary>The composition of the ring</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string composition => Composition?.localizedName;
        
        /// <summary>The mass of the ring, in megatonnes</summary>
        [PublicAPI]
        public decimal mass { get; set; }

        /// <summary>The inner radius of the ring, in kilometres</summary>
        [PublicAPI]
        public decimal innerradius { get; set; }

        /// <summary>The outer radius of the ring, in kilometres</summary>
        [PublicAPI]
        public decimal outerradius { get; set; }

        // Not intended to be user facing

        public RingComposition Composition { get; set; }

        [JsonIgnore]
        public string localizedComposition => Composition?.localizedName;

        [JsonIgnore]
        public string invariantComposition => Composition?.invariantName;

        public Ring(string name, RingComposition composition, decimal mass, decimal innerradius, decimal outerradius)
        {
            this.name = name;
            this.Composition = composition;
            this.mass = mass;
            this.innerradius = innerradius;
            this.outerradius = outerradius;
        }
    }
}
