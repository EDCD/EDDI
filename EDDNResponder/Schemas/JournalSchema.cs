using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class JournalSchema : ISchema
    {
        public List<string> edTypes => new List<string>
        {
            "Docked", 
            "FSDJump", 
            "Scan", 
            "Location", 
            "SAASignalsFound",
            "Scan"
        };

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            if (edType is null || !edTypes.Contains(edType)) { return false; }
            if (data is null || eddnState?.Location is null || eddnState.GameVersion is null) { return false; }
            if (!eddnState.Location.CheckLocationData(edType, data) || !CheckSanity(edType, data)) return false;

            // Remove personal data
            data = eddnState.PersonalData.Strip(data);

            // Apply data augments
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.Location.AugmentBody(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            return true;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/journal/1", data);
        }

        private bool CheckSanity(string edType, IDictionary<string, object> data)
        {
            // We've already vetted location data via the CheckLocationData method.
            // Perform any additional quality checks we think we need here.
            var passed = true;
            switch (edType)
            {
                case "Docked":
                    // Identify and catch a possible FDev bug that can allow incomplete `Docked` messages
                    // missing a MarketID and many other properties.
                    if (!data.ContainsKey("MarketID")) { passed = false; }

                    // Don't allow messages with a missing StationName.
                    if (data.ContainsKey("StationName") && string.IsNullOrEmpty(JsonParsing.getString(data, "StationName"))) { passed = false; }

                    break;
                case "SAASignalsFound":
                    if (!data.ContainsKey("Signals")) { passed = false; }
                    break;
                case "Scan":
                    if (!data.ContainsKey("ScanType")) { passed = false; }
                    break;
            }
            return passed;
        }
    }
}