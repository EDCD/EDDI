using Newtonsoft.Json;
using System;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A stellar or planetary ring
    /// </summary>
    public class Ring
    {
        /// <summary>The name of the ring</summary>
        public string name { get; set; }

        /// <summary>The composition of the ring</summary>
        [JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string composition => Composition.localizedName;
        public RingComposition Composition { get; set; }
        [JsonIgnore]
        public string localizedComposition => Composition?.localizedName;
        [JsonIgnore]
        public string invariantComposition => Composition?.invariantName;

        /// <summary>The mass of the ring, in megatonnes</summary>
        public decimal mass { get; set; }

        /// <summary>The inner radius of the ring, in kilometres</summary>
        public decimal innerradius { get; set; }

        /// <summary>The outer radius of the ring, in kilometres</summary>
        public decimal outerradius { get; set; }

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
