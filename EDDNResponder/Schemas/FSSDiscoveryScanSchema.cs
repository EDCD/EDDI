using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FSSDiscoveryScanSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "FSSDiscoveryScan" };

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            try
            {
                if (!edTypes.Contains(edType)) { return false; }
                if (eddnState?.Location is null || eddnState.GameVersion is null) { return false; }
                if (!eddnState.Location.CheckLocationData(edType, data)) { return false; }

                // Remove personal data
                data.Remove("Progress");

                // Apply data augments
                // Note: This event contains a `SystemName` property so we
                // do not need to enrich it with the conventional `StarSystem` property
                data = eddnState.Location.AugmentStarPos(data);
                data = eddnState.GameVersion.AugmentVersion(data);

                EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fssdiscoveryscan/1", data, eddnState);
                return true;
            }
            catch (Exception e)
            {
                Logging.Error($"{GetType().Name} failed to handle journal data.");
                return false;
            }
        }
    }
}