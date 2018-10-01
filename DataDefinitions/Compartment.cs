namespace EddiDataDefinitions
{
    ///<summary>A compartment is an internal slot within a ship</summary>
    public class Compartment
    {
        // The name of the compartment
        public string name { get; set; }
        /// <summary>The size of the compartment</summary>
        public int size { get; set; }
        /// <summary>The position of the compartment in power consumption</summary>
        public int position { get; set; }
        /// <summary>The module residing in the compartment (can be null)</summary>
        public Module module { get; set; }
    }
}
