using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiEddnResponder.Toolkit
{
    public class LocationAugmenter
    {
        // We keep track of location information locally to minimize influence from other EDDI systems

        // Star System
        public string systemName { get; private set; }
        public ulong? systemAddress { get; private set; }
        public decimal? systemX { get; private set; }
        public decimal? systemY { get; private set; }
        public decimal? systemZ { get; private set; }

        // Station (Market)
        public string stationName { get; private set; }
        public long? marketId { get; private set; }

        // Body
        public string journalBodyName { get; private set; }
        public int? journalBodyId { get; private set; }
        public string statusBodyName { get; private set; }

        public LocationAugmenter(IStarSystemRepository starSystemRepository)
        {
            StarSystemRepository = starSystemRepository;
        }

        internal IStarSystemRepository StarSystemRepository;

        public bool invalidState { get; private set; } // Are we in an invalid state?

        // These events contain full star system location data. 
        private static readonly List<string> fullStarSystemLocationEvents = new List<string>
        {
            "FSDJump",
            "Location",
            "CarrierJump"
        };

        // These events must be ignored to prevent enriching events with incorrect location data
        private static readonly List<string> starSystemIgnoredEvents = new List<string>
        {
            "CarrierJumpRequest", // CarrierJumpRequest events describing the system the carrier is jumping too rather than the system we are in
            "FSDTarget", // FSDTarget events describing the system we are targeting rather than the system we are in
            "FSSSignalDiscovered"  // Scan events from the destination system can register after StartJump and before we actually leave the originating system
        };

        internal void GetLocationInfo(Status status)
        {
            statusBodyName = !string.IsNullOrEmpty(status?.bodyname) ? status.bodyname : null;
        }

        internal void GetLocationInfo(string edType, IDictionary<string, object> data)
        {
            // We always start location data fresh when handling events containing complete star system location data
            if (fullStarSystemLocationEvents.Contains(edType))
            {
                ClearLocation();
            }

            GetStarSystemLocation(edType, data);
            GetStationLocation(edType, data);
            GetBodyLocation(edType, data);
        }

        private void GetStarSystemLocation(string edType, IDictionary<string, object> data)
        {
            try
            {
                Logging.Debug($"Extracting star system location data from {edType} event for EDDN", data);

                // Ignore any events that we've blacklisted for contaminating our location data
                if (!starSystemIgnoredEvents.Contains(edType))
                {
                    systemName = JsonParsing.getString(data, "StarSystem") ?? systemName;

                    // Some events are bugged and return a SystemAddress of 1, regardless of the system we are in.
                    // We need to ignore data that matches this pattern.
                    ulong SystemAddress = JsonParsing.getULong(data, "SystemAddress");
                    systemAddress = (SystemAddress > 1 ? SystemAddress : systemAddress);

                    data.TryGetValue("StarPos", out object starpos);
                    if (starpos != null)
                    {
                        List<object> starPos = (List<object>)starpos;
                        systemX = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32M) / 32M;
                        systemY = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32M) / 32M;
                        systemZ = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32M) / 32M;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse star system location data for EDDN", ex);
            }
        }

        private void GetStationLocation(string edType, IDictionary<string, object> data)
        {
            try
            {
                Logging.Debug($"Extracting market location data from {edType} event for EDDN", data);

                switch (edType)
                {
                    case "Docked":
                    case "Location":
                    {
                        marketId = JsonParsing.getOptionalLong(data, "MarketID");
                        stationName = JsonParsing.getString(data, "StationName");
                        break;
                    }
                    case "FSDJump":
                    case "Undock":
                    {
                        marketId = null;
                        stationName = null;
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse market location data for EDDN", ex);
            }
        }

        private void GetBodyLocation(string edType, IDictionary<string, object> data)
        {
            try
            {
                Logging.Debug($"Extracting body location data from {edType} event for EDDN", data);

                switch (edType)
                {
                    case "ApproachBody":
                    case "Location":
                    {
                        journalBodyId = JsonParsing.getOptionalInt(data, "BodyID");
                        journalBodyName = JsonParsing.getString(data, "Body");
                        break;
                    }
                    case "LeaveBody":
                    case "FSDJump":
                    {
                        journalBodyId = null;
                        journalBodyName = null;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse body location data for EDDN", ex);
            }
        }

        private void ClearLocation()
        {
            systemName = null;
            systemAddress = null;
            systemX = null;
            systemY = null;
            systemZ = null;
            stationName = null;
            marketId = null;
        }

        internal bool StarSystemLocationIsSet()
        {
            return systemName != null &&
                   systemAddress != null &&
                   systemX != null &&
                   systemY != null &&
                   systemZ != null;
        }

        internal IDictionary<string, object> AugmentStarSystemName(IDictionary<string, object> data)
        {
            if (!data.ContainsKey("StarSystem"))
            {
                data.Add("StarSystem", systemName);
            }
            return data;
        }

        internal IDictionary<string, object> AugmentSystemAddress(IDictionary<string, object> data)
        {
            if (!data.ContainsKey("SystemAddress"))
            {
                data.Add("SystemAddress", systemAddress);
            }
            return data;
        }

        internal IDictionary<string, object> AugmentStarPos(IDictionary<string, object> data)
        {
            if (!data.ContainsKey("StarPos") && systemX != null && systemY != null && systemZ != null)
            {
                IList<decimal> starpos = new List<decimal>
                {
                    systemX.Value,
                    systemY.Value,
                    systemZ.Value
                };
                data.Add("StarPos", starpos);
            }
            return data;
        }

        internal IDictionary<string, object> AugmentBody(IDictionary<string, object> data)
        {
            // Ref. https://github.com/EDCD/EDDN/blob/master/schemas/codexentry-README.md#bodyid-and-bodyname
            if (!data.ContainsKey("BodyName") && !string.IsNullOrEmpty(statusBodyName))
            {
                data.Add("BodyName", statusBodyName);
                if (!data.ContainsKey("BodyID") && statusBodyName == journalBodyName)
                {
                    data.Add("BodyID", journalBodyId);
                }
            }
            return data;
        }

        internal bool CheckLocationData(string edType, IDictionary<string, object> data)
        {
            // Confirm the location data in memory is as accurate as possible when handling an event with partial location data
            if (fullStarSystemLocationEvents.Contains(edType) && StarSystemLocationIsSet()) { return true; }

            // Can only send journal data if we know our current location data is correct
            // If any location data is null, data shall not be sent to EDDN.
            if (StarSystemLocationIsSet())
            {
                // The `Docked` event doesn't provide system coordinates, and the `Scan`event doesn't provide any system location data.
                // The EDDN journal schema requires that we enrich the journal event data with coordinates and system name (and system address if possible).
                if (data.ContainsKey("BodyName") && !data.ContainsKey("SystemName"))
                {
                    // Apply heuristics to weed out mismatched systems and bodies
                    ConfirmScan(JsonParsing.getString(data, "BodyName"));
                }
                if (!data.ContainsKey("SystemAddress") || !data.ContainsKey("StarPos"))
                {
                    // Out of an overabundance of caution, we do not use data from our saved star systems to enrich the data we send to EDDN, 
                    // but we do use it as an independent check to make sure our system address and coordinates are accurate
                    ConfirmAddressAndCoordinates();
                }

                if (StarSystemLocationIsSet())
                {
                    invalidState = false;
                    return true;
                }

                if (!invalidState)
                {
                    invalidState = true;
                    Logging.Warn("The EDDN responder is in an invalid state and is unable to send messages.", JsonConvert.SerializeObject(this) + " Event: " + JsonConvert.SerializeObject(data));
                }
            }
            return false;
        }

        private bool ConfirmAddressAndCoordinates()
        {
            if (systemName != null)
            {
                StarSystem system;
                if (systemName == EDDI.Instance.CurrentStarSystem?.systemname)
                {
                    system = EDDI.Instance.CurrentStarSystem;
                }
                else
                {
                    system = StarSystemRepository.GetOrCreateStarSystem(systemName);
                }
                if (system != null)
                {
                    if (systemAddress != system.systemAddress)
                    {
                        systemAddress = null;
                    }
                    if (systemX != system.x || systemY != system.y || systemZ != system.z)
                    {
                        systemX = null;
                        systemY = null;
                        systemZ = null;
                    }
                }
                else
                {
                    ClearLocation();
                }
            }
            return systemAddress != null && systemX != null && systemY != null && systemZ != null;
        }

        private bool ConfirmScan(string scannedBodyName)
        {
            if (scannedBodyName != null && systemName != null)
            {
                if (scannedBodyName.StartsWith(systemName))
                {
                    // If the system name is a subset of the body name, we're probably in the right place.
                    return true;
                }
                else
                {
                    // If the body doesn't start with the system name, it should also 
                    // not match a naming pattern for a procedurally generated name.
                    // If it does, it's (probably) in the wrong place.
                    Regex isProcGenName = new Regex(@"[A-Z][A-Z]-[A-Z] [a-h][0-9]");
                    if (!isProcGenName.IsMatch(scannedBodyName))
                    {
                        return true;
                    }
                }
            }
            // Set values to null if data can't be confirmed. 
            ClearLocation();
            return false;
        }
    }
}