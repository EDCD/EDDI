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
            try
            {
                if (!(eddnState?.Location is null) && !(eddnState.GameVersion is null))
                {
                    if (edTypes.Contains(lastEdType) && !edTypes.Contains(edType))
                    {
                        // This marks the end of a batch of signals.
                        if (signals.Any())
                        {
                            data = PrepareSignalsData(latestSignalState);
                            lastEdType = edType;
                            EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fsssignaldiscovered/1", data, eddnState);
                            latestSignalState = null;
                            return true;
                        }
                    }

                    if (edTypes.Contains(edType))
                    {
                        // This is a signal that we need to add to our signal batch

                        // Make sure the location data is valid
                        if (eddnState.Location.CheckLocationData(edType, data))
                        {
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
                            if (string.IsNullOrEmpty(ussSignalType) || ussSignalType != "$USS_Type_MissionTarget;")
                            {
                                var handledSignal = new Dictionary<string, object>();
                                if (data["timestamp"] != null) { handledSignal["timestamp"] = data["timestamp"];}
                                if (data["SignalName"] != null) { handledSignal["SignalName"] = data["SignalName"]; }
                                if (data["IsStation"] != null) { handledSignal["IsStation"] = data["IsStation"]; }
                                if (data["USSType"] != null) { handledSignal["USSType"] = data["USSType"]; }
                                if (data["SpawningState"] != null) { handledSignal["SpawningState"] = data["SpawningState"]; }
                                if (data["SpawningFaction"] != null) { handledSignal["SpawningFaction"] = data["SpawningFaction"]; }
                                if (data["ThreatLevel"] != null) { handledSignal["ThreatLevel"] = data["ThreatLevel"]; }

                                // Update our signal data
                                signals.Add(handledSignal);
                                latestSignalState = eddnState;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.Data.Add("edType", edType);
                e.Data.Add("Data", data);
                e.Data.Add("EDDN State", eddnState);
                Logging.Error($"{GetType().Name} failed to handle journal data.");
            }

            // We always save the edType so that we can identify the end of a signal batch.
            lastEdType = edType;
            return false;
        }

        private IDictionary<string, object> PrepareSignalsData(EDDNState eddnState)
        {
            // Create our top level data structure
            var retrievedSignals = signals?.Copy();
            var data = new Dictionary<string, object>()
            {
                { "timestamp", retrievedSignals?[0]?["timestamp"] },
                { "event", "FSSSignalDiscovered" },
                { "signals", retrievedSignals }
            } as IDictionary<string, object>;
            signals?.Clear();

            // Apply data augments
            data = eddnState.Location.AugmentStarSystemName(data);
            data = eddnState.Location.AugmentSystemAddress(data);
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            return data;
        }
    }
}