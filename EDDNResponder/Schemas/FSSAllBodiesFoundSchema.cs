using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FSSAllBodiesFoundSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "FSSAllBodiesFound" };

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            if (edType is null || !edTypes.Contains(edType)) { return false; }
            if (data is null || eddnState?.Location is null || eddnState.GameVersion is null) { return false; }
            if (!eddnState.Location.CheckLocationData(edType, data)) { return false; }

            // No personal data to remove

            // Apply data augments
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            return true;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fssallbodiesfound/1", data);
        }
    }
}