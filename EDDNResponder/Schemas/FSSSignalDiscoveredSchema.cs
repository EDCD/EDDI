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
                if (!edTypes.Contains(edType))
                {
                    // This marks the end of a batch of signals.
                    if (eddnState.GameVersion.inOdyssey ?? false)
                    {
                        // In Odyssey, Location/FSDJump/CarrierJump events are written prior to `FSSSignalDiscovered` events
                        latestSignalState = eddnState;
                    }
                    else
                    {
                        // In Horizons, Location/FSDJump/CarrierJump events are written after `FSSSignalDiscovered` events
                        // so we use the prior signal state
                    }
                    if (signals.Any())
                    {
                        try
                        {
                            LockManager.GetLock(nameof(latestSignalState), () =>
                            {
                                if (latestSignalState?.Location.StarSystemLocationIsSet() ?? false)
                                {
                                    var retrievedSignals = signals
                                        .Where(s => JsonParsing.getULong(s, "SystemAddress") == latestSignalState.Location.systemAddress)
                                        .ToList();
                                    if (retrievedSignals.Any())
                                    {
                                        var handledData = PrepareSignalsData(retrievedSignals, latestSignalState);
                                        handledData = latestSignalState.GameVersion.AugmentVersion(handledData);
                                        EDDNSender.SendToEDDN("https://eddn.edcd.io/schemas/fsssignaldiscovered/1", handledData, latestSignalState);
                                        latestSignalState = null;
                                        signals.RemoveAll(s => retrievedSignals.Contains(s));
                                    }
                                }
                            });
                        }
                        catch (NullReferenceException nre)
                        {
                            Logging.Error("Failed to prepare signals for sending to EDDN", nre);
                        }
                    }
                }

                if (edTypes.Contains(edType) && eddnState.Location.StarSystemLocationIsSet())
                {
                    try
                    {
                        // Remove redundant, personal, or time sensitive data
                        var ussSignalType = data?.ContainsKey("USSType") ?? false 
                            ? data["USSType"]?.ToString() 
                            : string.Empty;
                        if (string.IsNullOrEmpty(ussSignalType) || ussSignalType != "$USS_Type_MissionTarget;")
                        {
                            // This is a signal that we need to add to our signal batch
                            latestSignalState = eddnState;
                            signals.Add(data);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Error($"{GetType().Name} failed to handle {edType} journal data.", e);
                    }
                    return true;
                }
            }
            return false;
        }

        private IDictionary<string, object> PrepareSignalsData(List<IDictionary<string, object>> retrievedSignals, EDDNState eddnState)
        {
            var handledSignals = new List<IDictionary<string, object>>();

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
                    Logging.Warn($"Failed to prepare signal to send to EDDN: {retrievedSignal}", e);
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