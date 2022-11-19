using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class NavRouteSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "NavRoute" };

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            try
            {
                if (!edTypes.Contains(edType)) { return false; }
                if (eddnState?.GameVersion == null) { return false; }
                data = eddnState.GameVersion.AugmentVersion(data);
                EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/navroute/1", data, eddnState);
                return true;
            }
            catch (Exception e)
            {
                e.Data.Add("edType", edType);
                e.Data.Add("Data", data);
                e.Data.Add("EDDN State", eddnState);
                Logging.Error($"{GetType().Name} failed to handle journal data.");
                return false;
            }
        }
    }
}