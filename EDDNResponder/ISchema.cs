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
        IDictionary<string, object> Handle(string edType, IDictionary<string, object> data, EDDNState eddnState, out bool handled);

        void Send(IDictionary<string, object> data);
    }

    public interface ICapiSchema
    {
        IDictionary<string, object> Handle(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson, EDDNState eddnState, out bool handled);

        void SendCapi(IDictionary<string, object> data);
    }
}