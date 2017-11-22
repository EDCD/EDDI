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
        public string composition { get; set; }

        /// <summary>The composition of the ring translated in chosen language</summary>
        [JsonIgnore]
        public string LocalComposition
        {
            get
            {
                return I18N.GetString(composition) ?? composition;
            }
        }


        /// <summary>The mass of the ring, in megatonnes</summary>
        public decimal mass { get; set; }

        /// <summary>The inner radius of the ring, in metres</summary>
        public decimal innerradius { get; set; }

        /// <summary>The outer radius of the ring, in metres</summary>
        public decimal outerradius { get; set; }

        public Ring(string name, string composition, decimal mass, decimal innerradius, decimal outerradius)
        {
            this.name = name;
            this.composition = composition;
            this.mass = mass;
            this.innerradius = innerradius;
            this.outerradius = outerradius;
        }
    }
}
