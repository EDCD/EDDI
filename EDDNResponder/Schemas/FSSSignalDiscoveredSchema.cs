using EddiEddnResponder.Sender;
using EddiEddnResponder.Toolkit;
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
        public List<string> edTypes => [ "FSSSignalDiscovered" ];

        private readonly List<IDictionary<string, object>> signals =
            [ ];

        private readonly Dictionary<ulong, EDDNState> eddnStates = [ ];

        private static readonly object signalsLock = new();
        private static readonly object stateLock = new();

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState, EDDNSender eddnSender )
        {
            if (eddnState?.Location is null || eddnState.GameVersion is null) { return false; }

            UpdateLocationSnapshot( edType, eddnState );

            if ( !edTypes.Contains( edType ) )
            {
                SendQueuedSignals( eddnSender );
                return false;
            }

            try
            {
                // Remove redundant, personal, or time sensitive data
                var ussSignalType = data?.ContainsKey("USSType") ?? false
                    ? data["USSType"]?.ToString()
                    : string.Empty;
                if (string.IsNullOrEmpty(ussSignalType) || ussSignalType != "$USS_Type_MissionTarget;")
                {
                    // This is a signal that we need to add to our signal batch
                    lock (signalsLock)
                    {
                        signals.Add(data);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Error($"{GetType().Name} failed to handle {edType} journal data.", e);
            }
            return true;
        }

        private void UpdateLocationSnapshot ( string edType, EDDNState eddnState )
        {
            if ( !LocationAugmenter.IsFullStarSystemLocationEvent( edType ) ||
                 !eddnState.Location.StarSystemLocationIsSet() )
            {
                return;
            }

            lock ( stateLock )
            {
                eddnStates[ eddnState.Location.systemAddress ] = eddnState.Copy();
            }
        }

        private void SendQueuedSignals ( EDDNSender eddnSender )
        {

            lock ( signalsLock )
            {
                if ( signals.Count == 0 ) { return; }

                var signalGroups = signals
                    .GroupBy( s => JsonParsing.getULong( s, "SystemAddress" ) )
                    .Where( g => g.Key > 1 )
                    .ToList();

                foreach ( var signalGroup in signalGroups )
                {
                    EDDNState state;
                    lock ( stateLock )
                    {
                        if ( !eddnStates.TryGetValue( signalGroup.Key, out state ) ||
                             !state.Location.StarSystemLocationIsSet() )
                        {
                            continue;
                        }
                    }

                    try
                    {
                        var retrievedSignals = signalGroup.ToList();
                        var handledData = PrepareSignalsData( retrievedSignals, state );
                        handledData = state.GameVersion.AugmentVersion( handledData );
                        eddnSender.SendToEDDN( "https://eddn.edcd.io/schemas/fsssignaldiscovered/1",
                            handledData, state );
                        signals.RemoveAll( s => retrievedSignals.Contains( s ) );
                    }
                    catch ( NullReferenceException nre )
                    {
                        Logging.Error( "Failed to prepare signals for sending to EDDN", nre );
                    }
                    catch ( ArgumentOutOfRangeException aore )
                    {
                        Logging.Error( "Failed to prepare signals for sending to EDDN", aore );
                    }
                }
            }
        }

        private static IDictionary<string, object> PrepareSignalsData(List<IDictionary<string, object>> retrievedSignals, EDDNState eddnState)
        {
            var handledSignals = new List<IDictionary<string, object>>();

            foreach (var retrievedSignal in retrievedSignals)
            {
                var handledSignal = new Dictionary<string, object>();
                try
                {
                    if (retrievedSignal.TryGetValue("timestamp", out var timestamp))
                    {
                        handledSignal["timestamp"] = timestamp;
                    }
                    if (retrievedSignal.TryGetValue("SignalName", out var signalName))
                    {
                        handledSignal["SignalName"] = signalName;
                    }
                    if ( retrievedSignal.TryGetValue( "SignalType", out var signalType ) )
                    {
                        handledSignal[ "SignalType" ] = signalType;
                    }
                    if (retrievedSignal.TryGetValue("IsStation", out var isStation))
                    {
                        handledSignal["IsStation"] = isStation;
                    }
                    if (retrievedSignal.TryGetValue("USSType", out var ussType))
                    {
                        handledSignal["USSType"] = ussType;
                    }
                    if (retrievedSignal.TryGetValue("SpawningState", out var spawningState))
                    {
                        handledSignal["SpawningState"] = spawningState;
                    }
                    if (retrievedSignal.TryGetValue("SpawningFaction", out var spawningFaction))
                    {
                        handledSignal["SpawningFaction"] = spawningFaction;
                    }
                    if (retrievedSignal.TryGetValue("ThreatLevel", out var threatLevel))
                    {
                        handledSignal["ThreatLevel"] = threatLevel;
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
