using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace EddiEddnResponder
{
    public interface ISchema
    {
        [UsedImplicitly]
        List<string> edTypes { get; }

        [UsedImplicitly]
        bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState);
    }

    public interface ICapiSchema
    {
        IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, bool fromLegacyServer, EDDNState eddnState);
    }
}