using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiEddnResponder.Sender
{
    /// <summary>
    /// The body for an EDDN request
    /// </summary>
    public class EDDNBody
    {
        [UsedImplicitly] public EDDNHeader header;
        [JsonProperty("$schemaRef")]
        public string schemaRef;
        public IDictionary<string, object> message;
    }
}
