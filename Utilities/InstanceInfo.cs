namespace Utilities
{
    /// <summary>
    /// Information obtained from the update server
    /// </summary>
    public class InstanceInfo
    {
        /// <summary>
        /// The current version of the product
        /// </summary>
        public string version { get;  set; }

        /// <summary>
        /// The URL for the current version of the product
        /// </summary>
        public string url { get;  set; }

        /// <summary>
        /// The minimum supported version of the product
        /// </summary>
        public string minversion { get;  set; }

        /// <summary>
        /// The message of the day
        /// </summary>
        public string motd { get;  set; }
    }
}
