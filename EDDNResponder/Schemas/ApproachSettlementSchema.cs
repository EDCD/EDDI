using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class ApproachSettlementSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "ApproachSettlement" };

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            if (edType is null || !edTypes.Contains(edType)) { return false; }
            if (data is null || eddnState?.Location is null || eddnState.GameVersion is null) { return false; }
            if (!eddnState.Location.CheckLocationData(edType, data)) { return false; }

            // No personal data to remove

            // Apply data augments
            data = eddnState.Location.AugmentStarSystemName(data);
            data = eddnState.Location.AugmentSystemAddress(data); // Later version journal events have this but we'll double check before sending to EDDN.
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            return true;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/approachsettlement/1", data);
        }
    }
}