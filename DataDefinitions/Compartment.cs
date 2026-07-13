using Utilities;

namespace EddiDataDefinitions
{
    ///<summary>A compartment is an internal slot within a ship</summary>
    public class Compartment
    {
        // The name of the compartment
        public string name { get; set; }

        /// <summary>The size of the compartment</summary>
        [PublicAPI( "the numeric size of the compartment, from 1 to 8" )]
        public int size { get; set; }

        /// <summary>The module residing in the compartment (can be null)</summary>
        [PublicAPI( "the module in the compartment, as an object" )]
        public Module module { get; set; }
    }
}
