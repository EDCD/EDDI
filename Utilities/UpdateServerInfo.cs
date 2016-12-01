using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// Information obtained from the update server
    /// </summary>
    public class UpdateServerInfo
    {
        /// <summary>
        /// The production release version of the product
        /// </summary>
        public string prodversion { get; private set; }

        /// <summary>
        /// The URL for the production release version of the product
        /// </summary>
        public string produrl { get; private set; }

        /// <summary>
        /// The beta release version of the product
        /// </summary>
        public string betaversion { get; private set; }

        /// <summary>
        /// The URL for the beta release version of the product
        /// </summary>
        public string betaurl { get; private set; }

        /// <summary>
        /// The minimum supported version of the product
        /// </summary>
        public string minversion { get; private set; }

        /// <summary>
        /// The message of the day
        /// </summary>
        public string motd { get; private set; }

        /// <summary>
        /// Obtain information from the update server.  Note that this throws exceptions if it fails
        /// </summary>
        public static UpdateServerInfo FromServer(string baseUri)
        {
            return JsonConvert.DeserializeObject<UpdateServerInfo>(Net.DownloadString(baseUri + "_updateinfo"));
        }
    }
}
