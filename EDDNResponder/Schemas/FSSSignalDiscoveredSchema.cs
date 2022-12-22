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

        private EDDNState latestSignalState;

        private readonly List<IDictionary<string, object>> signals =
            new List<IDictionary<string, object>>();

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState)
        {
            if (!(eddnState?.Location is null) && !(eddnState.GameVersion is null))
            {
                if (!edTypes.Contains(edType) && eddnState.Location.StarSystemLocationIsSet())
                {
                    try
                    {
                        // This marks the end of a batch of signals.
                        LockManager.GetLock(nameof(FSSSignalDiscoveredSchema), () =>
                        {
                            var retrievedSignals = signals?
                                .Where(s => JsonParsing.getULong(s, "SystemAddress") == eddnState.Location.systemAddress)
                                .ToList();
                            if (retrievedSignals?.Any() ?? false)
                            {
                                var handledData = PrepareSignalsData(retrievedSignals, latestSignalState);
                                handledData = eddnState.GameVersion.AugmentVersion(handledData);
                                latestSignalState = null;
                                signals.RemoveAll(s => retrievedSignals.Contains(s));
                                EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fsssignaldiscovered/1", handledData, eddnState);
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        e.Data.Add("edType", edType);
                        e.Data.Add("Signals", signals);
                        e.Data.Add("EDDN State", eddnState);
                        Logging.Error($"{GetType().Name} failed to compile and send journal signals data.");
                    }
                }

                if (edTypes.Contains(edType))
                {
                    try
                    {
                        // Remove redundant, personal, or time sensitive data
                        var ussSignalType = data.ContainsKey("USSType") ? data["USSType"]?.ToString() : string.Empty;
                        if (string.IsNullOrEmpty(ussSignalType) || ussSignalType != "$USS_Type_MissionTarget;")
                        {
                            // This is a signal that we need to add to our signal batch
                            signals.Add(data);
                            latestSignalState = eddnState;
                        }
                    }
                    catch (Exception e)
                    {
                        e.Data.Add("edType", edType);
                        e.Data.Add("Data", data);
                        e.Data.Add("EDDN State", eddnState);
                        Logging.Error($"{GetType().Name} failed to handle journal data.");
                    }
                    return true;
                }
            }
            return false;
        }

        private IDictionary<string, object> PrepareSignalsData(List<IDictionary<string, object>> retrievedSignals, EDDNState eddnState)
        {
            List<IDictionary<string, object>> handledSignals = new List<IDictionary<string, object>>();
            foreach (var retrievedSignal in retrievedSignals)
            {
                var handledSignal = new Dictionary<string, object>();
                try
                {
                    if (retrievedSignal.ContainsKey("timestamp"))
                    {
                        handledSignal["timestamp"] = retrievedSignal["timestamp"];
                    }
                    if (retrievedSignal.ContainsKey("SignalName"))
                    {
                        handledSignal["SignalName"] = retrievedSignal["SignalName"];
                    }
                    if (retrievedSignal.ContainsKey("IsStation"))
                    {
                        handledSignal["IsStation"] = retrievedSignal["IsStation"];
                    }
                    if (retrievedSignal.ContainsKey("USSType"))
                    {
                        handledSignal["USSType"] = retrievedSignal["USSType"];
                    }
                    if (retrievedSignal.ContainsKey("SpawningState"))
                    {
                        handledSignal["SpawningState"] = retrievedSignal["SpawningState"];
                    }
                    if (retrievedSignal.ContainsKey("SpawningFaction"))
                    {
                        handledSignal["SpawningFaction"] = retrievedSignal["SpawningFaction"];
                    }
                    if (retrievedSignal.ContainsKey("ThreatLevel"))
                    {
                        handledSignal["ThreatLevel"] = retrievedSignal["ThreatLevel"];
                    }
                }
                catch (Exception e)
                {
                    e.Data.Add("Signal", retrievedSignal);
                    Logging.Warn("Failed to prepare signal to send to EDDN", e);
                }

                // The signal must at minimum contain a timestamp and SignalName.
                if (handledSignal.ContainsKey("timestamp") && handledSignal.ContainsKey("SignalName"))
                {
                    handledSignals.Add(handledSignal);
                }
            }

            // Create our top level data structure
            var data = new Dictionary<string, object>
            {
                { "timestamp", handledSignals[0]?["timestamp"] },
                { "event", "FSSSignalDiscovered" },
                { "signals", handledSignals }
            } as IDictionary<string, object>;

            // Apply data augments
            data = eddnState.Location.AugmentStarSystemName(data);
            data = eddnState.Location.AugmentSystemAddress(data);
            data = eddnState.Location.AugmentStarPos(data);
            data = eddnState.GameVersion.AugmentVersion(data);

            return data;
        }
    }
}