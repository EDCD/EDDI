using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A friend
    /// </summary>
    public class Friend
    {
        /// <summary>The name of the friend</summary>
        [PublicAPI]
        public string name { get; set; } = string.Empty;

        /// <summary>The status of the friend</summary>
        [PublicAPI]
        public string status { get; set; } = string.Empty;
    }
}
