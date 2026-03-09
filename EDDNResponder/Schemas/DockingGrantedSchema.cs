using EddiEddnResponder.Sender;
using EddiEddnResponder.Toolkit;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class DockingGrantedSchema : ISchema
    {
        public List<string> edTypes => [ "DockingGranted" ];

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            try
            {
                if (!edTypes.Contains(edType)) { return false; }
                if (eddnState.GameVersion is null) { return false; }

                // No personal data to remove
                data = PersonalDataStripper.Strip(data, edType);

                // Apply data augments
                data = eddnState.GameVersion.AugmentVersion(data);

                EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/dockinggranted/1", data, eddnState);
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