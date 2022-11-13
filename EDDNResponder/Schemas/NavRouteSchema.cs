using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class NavRouteSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "NavRoute" };

        public IDictionary<string, object> Handle(string edType, IDictionary<string, object> data, EDDNState eddnState, out bool handled)
        {
            handled = false;
            if (edType is null || !edTypes.Contains(edType)) { return null; }
            if (data == null || eddnState?.GameVersion == null) { return null; }

            data = eddnState.GameVersion.AugmentVersion(data);

            handled = true;
            return data;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/navroute/1", data);
        }
    }
}