namespace EliteDangerousDataDefinitions
{
    ///<summary>A compartment is an internal slot within a ship</summary>
    public class Compartment
    {
        /// <summary>The size of the compartment</summary>
        public int Size { get; set; }
        /// <summary>The module residing in the compartment (can be null)</summary>
        public Module Module { get; set; }
    }
}
