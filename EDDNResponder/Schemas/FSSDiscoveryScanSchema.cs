using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FSSDiscoveryScanSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "FSSDiscoveryScan" };

        public IDictionary<string, object> Handle(string edType, IDictionary<string, object> data, EDDNState eddnState, out bool handled)
        {
            handled = false;
            if (edType is null || !edTypes.Contains(edType)) { return null; }
            if (data is null || eddnState?.Location is null || eddnState.GameVersion is null) { return null; }
            if (!eddnState.Location.CheckLocationData(edType, data)) { return null; }

            // Remove personal data
            data.Remove("Progress");

            // Apply data augments
            // Note: This event contains a `SystemName` property so we
            // do not need to enrich it with the conventional `StarSystem` property
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            handled = true;
            return data;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fssdiscoveryscan/1", data);
        }
    }
}