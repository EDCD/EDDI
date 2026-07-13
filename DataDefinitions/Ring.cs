using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A stellar or planetary ring
    /// </summary>
    public class Ring (
        string name,
        RingComposition composition,
        decimal mass,
        decimal innerradius,
        decimal outerradius )
    {
        /// <summary>The name of the ring</summary>
        [PublicAPI( "the name of the ring" )]
        public string name { get; set; } = name;

        /// <summary>The composition of the ring</summary>
        [PublicAPI( "the composition of the ring" ), JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string composition => Composition?.localizedName;
        
        /// <summary>The mass of the ring, in megatonnes</summary>
        [PublicAPI( "the mass of the ring, in megatons" )]
        public decimal mass { get; set; } = mass;

        /// <summary>The inner radius of the ring, in kilometres</summary>
        [PublicAPI( "the inner radius of the ring, in km" )]
        public decimal innerradius { get; set; } = innerradius;

        /// <summary>The outer radius of the ring, in kilometres</summary>
        [PublicAPI( "the outer radius of the ring, in km" )]
        public decimal outerradius { get; set; } = outerradius;

        /// <summary>When we mapped this object, if we have (DateTime)</summary>
        [PublicAPI( "a DateTime value that is set when the ring is mapped and unset otherwise." )]
        public DateTime? mapped { get; set; }

        /// <summary>The hotpots we found when we scanned this object, if any</summary>
        [PublicAPI( "a list of ring hotspots (as objects with properties 'commodity' and 'amount'), available for mapped rings only" )]
        public List<CommodityAmount> hotspots { get; set; } = [ ];

        // Not intended to be user facing

        public RingComposition Composition { get; set; } = composition;

        [JsonIgnore]
        public string localizedComposition => Composition?.localizedName;

        [JsonIgnore]
        public string invariantComposition => Composition?.invariantName;
    }
}
