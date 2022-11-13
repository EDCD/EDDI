using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FSSBodySignalsSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "FSSBodySignals" };

        public IDictionary<string, object> Handle(string edType, IDictionary<string, object> data, EDDNState eddnState, out bool handled)
        {
            handled = false;
            if (edType is null || !edTypes.Contains(edType)) { return null; }
            if (data is null || eddnState?.Location is null || eddnState.GameVersion is null) { return null; }
            if (!eddnState.Location.CheckLocationData(edType, data)) { return null; }

            // Strip localized values
            data = eddnState.PersonalData.Strip(data);

            // Apply data augments
            data = eddnState.Location.AugmentStarSystemName(data);
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            handled = true;
            return data;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fssbodysignals/1", data);
        }
    }
}