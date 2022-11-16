using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FSSBodySignalsSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "FSSBodySignals" };

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            if (edType is null || !edTypes.Contains(edType)) { return false; }
            if (data is null || eddnState?.Location is null || eddnState.GameVersion is null) { return false; }
            if (!eddnState.Location.CheckLocationData(edType, data)) { return false; }

            // Strip localized values
            data = eddnState.PersonalData.Strip(data);

            // Apply data augments
            data = eddnState.Location.AugmentStarSystemName(data);
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            return true;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fssbodysignals/1", data);
        }
    }
}