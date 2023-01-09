using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FSSBodySignalsSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "FSSBodySignals" };

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            try
            {
                if (!edTypes.Contains(edType)) { return false; }
                if (eddnState?.Location is null || eddnState.GameVersion is null) { return false; }
                if (!eddnState.Location.CheckLocationData(edType, data)) { return false; }

                // Strip localized values
                data = eddnState.PersonalData.Strip(data, edType);

                // Apply data augments
                data = eddnState.Location.AugmentStarSystemName(data);
                data = eddnState.Location.AugmentStarPos(data);
                data = eddnState.GameVersion.AugmentVersion(data);

                EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fssbodysignals/1", data, eddnState);
                return true;
            }
            catch (Exception e)
            {
                Logging.Error($"{GetType().Name} failed to handle journal data.", e);
                return false;
            }
        }
    }
}