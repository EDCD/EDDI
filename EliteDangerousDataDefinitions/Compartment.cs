namespace EliteDangerousDataDefinitions
{
    ///<summary>A compartment is an internal slot within a ship</summary>
    public class Compartment
    {
        /// <summary>The size of the compartment</summary>
        public int size { get; set; }
        /// <summary>The module residing in the compartment (can be null)</summary>
        public Module module { get; set; }
    }
}
