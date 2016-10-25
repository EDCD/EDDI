namespace EddiDataDefinitions
{
    /// <summary>An external mounting on a ship</summary>
    public class Hardpoint
    {
        /// <summary>The size of the hardpoint</summary
        public int size { get; set; }
        /// <summary>The module residing on the hardpoint (can be null)</summary>
        public Module module { get; set; }
    }
}
