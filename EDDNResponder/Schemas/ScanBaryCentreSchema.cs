using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class ScanBaryCentreSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "ScanBaryCentre" };

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            if (edType is null || !edTypes.Contains(edType)) { return false; }
            if (eddnState?.Location is null || eddnState.GameVersion is null) { return false; }
            if (!eddnState.Location.CheckLocationData(edType, data)) { return false; }

            // No personal data to remove

            // Apply data augments
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            return true;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/scanbarycentre/1", data);
        }
    }
}