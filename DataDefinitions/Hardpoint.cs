using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>An external mounting on a ship</summary>
    public class Hardpoint
    {
        /// <summary>The name of the hardpoint</summary>
        public string name { get; set; }

        /// <summary>The size of the hardpoint</summary
        [PublicAPI]
        public int size { get; set; }

        /// <summary>The module residing on the hardpoint (can be null)</summary>
        [PublicAPI]
        public Module module { get; set; }
    }
}
