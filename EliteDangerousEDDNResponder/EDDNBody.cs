using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEDDNResponder
{
    /// <summary>
    /// The body for an EDDN request
    /// </summary>
    class EDDNBody
    {
        public EDDNHeader header;
        [JsonProperty("$schemaRef")]
        public string schemaRef;
        public IDictionary<string, object> message;
    }
}
