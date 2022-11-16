using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class FSSSignalDiscoveredSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "FSSSignalDiscovered" };

        private string lastEdType;
        private EDDNState latestSignalState;

        private readonly List<IDictionary<string, object>> signals =
            new List<IDictionary<string, object>>();

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            if (edType is null || data is null || eddnState?.Location is null || eddnState.GameVersion is null) { return false; }

            if (edTypes.Contains(lastEdType) && !edTypes.Contains(edType))
            {
                // This marks the end of a batch of signals.
                if (signals.Any())
                {
                    data = PrepareSignalsData(latestSignalState);
                    return true;
                }
            }

            if (edTypes.Contains(edType))
            {
                // This is a signal that we need to add to our signal batch

                // Make sure the location data is valid
                if (!eddnState.Location.CheckLocationData(edType, data))
                {
                    return false;
                }

                if (latestSignalState is null)
                {
                    latestSignalState = eddnState;
                }
                else
                {
                    // Make sure that our signal location data is consistent across our batch by testing it here
                    var loc = eddnState.Location;
                    var lastLoc = latestSignalState.Location;
                    if (loc.systemName != lastLoc.systemName ||
                        loc.systemAddress != lastLoc.systemAddress ||
                        loc.systemX != lastLoc.systemX || loc.systemY != lastLoc.systemY ||
                        loc.systemZ != lastLoc.systemZ)
                    {
                        var ex = new ArgumentException("Tracked signal locations are not aligned.");
                        ex.Data.Add("Last tracked Location", lastLoc);
                        ex.Data.Add("Current tracked location", loc);
                        throw ex;
                    }
                }

                // Remove redundant, personal, or time sensitive data
                var ussSignalType = data.ContainsKey("USSType") ? data["USSType"]?.ToString() : string.Empty;
                if (!string.IsNullOrEmpty(ussSignalType) && ussSignalType == "$USS_Type_MissionTarget;")
                {
                    return false;
                }
                data.Remove("event");
                data.Remove("SystemAddress");
                data.Remove("TimeRemaining");
                data = eddnState.PersonalData.Strip(data);

                // Update our signal data
                signals.Add(data);
                latestSignalState = eddnState;
            }

            // We always save the edType so that we can identify the end of a signal batch.
            lastEdType = edType;
            return false;
        }

        private IDictionary<string, object> PrepareSignalsData(EDDNState eddnState)
        {
            // Create our top level data structure
            var data = new Dictionary<string, object>()
            {
                { "timestamp", Dates.FromDateTimeToString(signals?.Copy()?[0]?["timestamp"] as DateTime? ?? DateTime.MinValue) },
                { "event", "FSSSignalDiscovered" },
                { "signals", signals?.Copy() }
            } as IDictionary<string, object>;
            signals?.Clear();

            // Apply data augments
            data = eddnState.Location.AugmentStarSystemName(data);
            data = eddnState.Location.AugmentSystemAddress(data);
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            return data;
        }

        public void Send(IDictionary<string, object> data)
        {
            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fsssignaldiscovered/1", data);
            latestSignalState = null;
        }
    }
}