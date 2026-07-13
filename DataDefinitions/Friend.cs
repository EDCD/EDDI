using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A friend
    /// </summary>
    public class Friend
    {
        /// <summary>The name of the friend</summary>
        [PublicAPI( "the name of the friend" )]
        public string name { get; set; } = string.Empty;

        /// <summary>The status of the friend</summary>
        [PublicAPI( "the status of the friend (one of the following: Requested, Declined, Added, Lost, Offline, Online)" )]
        public string status { get; set; } = string.Empty;
    }
}
