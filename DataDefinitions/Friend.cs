using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A friend
    /// </summary>
    public class Friend
    {
        /// <summary>The name of the friend</summary>
        public string name { get; set; } = string.Empty;

        /// <summary>The status of the friend</summary>
        public string status { get; set; } = string.Empty;
    }
}
