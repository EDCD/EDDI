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
    public class ServerInfo
    {
        /// <summary>
        /// Instance information for production
        /// </summary>
        public InstanceInfo production { get;  set; }

        /// <summary>
        /// Instance information for beta
        /// </summary>
        public InstanceInfo beta { get;  set; }

        /// <summary>
        /// Obtain information from the update server.  Note that this throws exceptions if it fails
        /// </summary>
        public static ServerInfo FromServer(string baseUri)
        {
            string data = Net.DownloadString(baseUri + "_info");
            return data == null ? null : JsonConvert.DeserializeObject<ServerInfo>(data);
        }
    }
}
