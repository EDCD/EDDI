using EddiCore;
using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder.Toolkit
{
    public class LocationAugmenter
    {
        // We keep track of location information locally to minimize influence from other EDDI systems

        // Star System
        public string systemName { get; internal set; }
        public ulong systemAddress { get; internal set; }
        public decimal? systemX { get; internal set; }
        public decimal? systemY { get; internal set; }
        public decimal? systemZ { get; internal set; }

        // Station (Market)
        public string stationName { get; private set; }
        public long? marketId { get; private set; }

        // Journal Body Info
        public string journalBodyName { get; private set; }
        public int? journalBodyId { get; private set; }

        // Status Info
        public string statusBodyName { get; private set; }
        public bool statusOnFootOnPlanet { get; private set; }
        public decimal? statusLatitude { get; private set; }
        public decimal? statusLongitude { get; private set; }
        public DateTime? statusTimeStamp { get; private set; }

        private bool invalidState { get; set; } // Are we in an invalid state?

        // These events contain full star system location data. 
        private static readonly List<string> fullStarSystemLocationEvents =
        [
            "FSDJump",
            "Location",
            "CarrierJump"
        ];

        internal static bool IsFullStarSystemLocationEvent ( string edType )
        {
            return fullStarSystemLocationEvents.Contains( edType );
        }

        // These events must be ignored to prevent enriching events with incorrect location data
        private static readonly List<string> starSystemIgnoredEvents =
        [
            "CarrierJumpRequest", // CarrierJumpRequest events describing the system the carrier is jumping too rather than the system we are in
            "FSDTarget", // FSDTarget events describing the system we are targeting rather than the system we are in
            "FSSSignalDiscovered", // Scan events from the destination system can register after StartJump and before we actually leave the originating system
            "Outfitting", // Relies on an exterior file and contains `StarSystem` field without `SystemAddress` field. Safer to ignore.
            "Market", // Relies on an exterior file and contains `StarSystem` field without `SystemAddress` field. Safer to ignore.
            "ScanOrganic", // May report incorrect location info if the player does not allow the animation to complete before boarding a ship.
            "Shipyard", // Relies on an exterior file and contains `StarSystem` field without `SystemAddress` field. Safer to ignore.
            "StartJump", // `StartJump` events list the destination star system name.
            "StoredModules", // Contains `StarSystem` field without `SystemAddress` field. Safer to ignore.
            "StoredShips"
        ];

        internal void GetLocationInfo(Status status)
        {
            if ( status is null ) { return; }
            statusBodyName = !string.IsNullOrEmpty(status.bodyname) ? status.bodyname : null;
            statusOnFootOnPlanet = status.on_foot_on_planet;
            statusLatitude = status.latitude;
            statusLongitude = status.longitude;
            statusTimeStamp = status.timestamp;
        }

        internal void GetLocationInfo(string edType, IDictionary<string, object> data)
        {
            // We always start location data fresh when handling events containing complete star system location data
            if (IsFullStarSystemLocationEvent(edType))
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
                if ( starSystemIgnoredEvents.Contains( edType ) )
                {
                    return;
                }

                var incomingSystemName = data.ContainsKey( "StarSystem" )
                    ? JsonParsing.getString( data, "StarSystem" )
                    : null;
                var incomingSystemAddress = data.ContainsKey( "SystemAddress" )
                    ? JsonParsing.getULong( data, "SystemAddress" )
                    : 0UL;
                // Some events are bugged and return a SystemAddress of 1, regardless of the system we are in.
                // We need to ignore data that matches this pattern.
                var incomingSystemAddressIsValid = incomingSystemAddress > 1;

                if ( IsFullStarSystemLocationEvent( edType ) )
                {
                    systemName = incomingSystemName ?? systemName;
                    systemAddress = incomingSystemAddressIsValid ? incomingSystemAddress : systemAddress;

                    data.TryGetValue( "StarPos", out var starpos );
                    if ( starpos != null )
                    {
                        SetStarPos( starpos );
                    }
                    return;
                }

                var nameWouldChange = !string.IsNullOrEmpty( incomingSystemName ) && incomingSystemName != systemName;
                var addressWouldChange = incomingSystemAddressIsValid && incomingSystemAddress != systemAddress;
                var nameConflicts = !string.IsNullOrEmpty( incomingSystemName ) && systemName != null && incomingSystemName != systemName;
                var addressConflicts = incomingSystemAddressIsValid && systemAddress != 0 && incomingSystemAddress != systemAddress;
                var hasCoordinates = systemX != null || systemY != null || systemZ != null;

                // Partial events can confirm the current location but cannot combine a new system identity with old coordinates.
                if ( nameConflicts || addressConflicts || ( hasCoordinates && ( nameWouldChange || addressWouldChange ) ) )
                {
                    ClearStarSystemLocation();
                }

                if ( !string.IsNullOrEmpty( incomingSystemName ) )
                {
                    systemName = incomingSystemName;
                }

                if ( incomingSystemAddressIsValid )
                {
                    systemAddress = incomingSystemAddress;
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse star system location data for EDDN", ex);
            }
        }

        private void SetStarPos ( object starpos )
        {
            if ( starpos is IList<object> starPos && starPos.Count >= 3 )
            {
                systemX = Math.Round( JsonParsing.getDecimal( "X", starPos[ 0 ] ) * 32M ) / 32M;
                systemY = Math.Round( JsonParsing.getDecimal( "Y", starPos[ 1 ] ) * 32M ) / 32M;
                systemZ = Math.Round( JsonParsing.getDecimal( "Z", starPos[ 2 ] ) * 32M ) / 32M;
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
                        stationName = JsonParsing.getString(data, "StationName")?.TrimEnd( '+', ' ' ); // Remove any +++ at the end of the station name
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
            ClearStarSystemLocation();
            stationName = null;
            marketId = null;
        }

        private void ClearStarSystemLocation ()
        {
            systemName = null;
            systemAddress = 0;
            systemX = null;
            systemY = null;
            systemZ = null;
        }

        internal bool StarSystemLocationIsSet()
        {
            return systemName != null &&
                   systemAddress != 0 &&
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

        internal IDictionary<string, object> AugmentBodyNameID(IDictionary<string, object> data)
        {
            // Ref. https://github.com/EDCD/EDDN/blob/master/schemas/codexentry-README.md#bodyid-and-bodyname
            if ( !data.ContainsKey( "BodyName" ) && !string.IsNullOrEmpty( statusBodyName ) )
            {
                data.Add( "BodyName", statusBodyName );

                if ( !data.ContainsKey( "BodyID" ) && statusBodyName.Equals( journalBodyName, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    data.Add( "BodyID", journalBodyId );
                }
            }

            return data;
        }

        internal IDictionary<string, object> AugmentBodyLatLong(IDictionary<string, object> data, int timestampThresholdSeconds = 60, bool onlyIfOnFoot = false)
        {
            // Ref. https://github.com/EDCD/EDDN/blob/master/schemas/scanorganic-README.md
            var timestamp = JsonParsing.getDateTime( "timestamp", data );
            if ( !data.ContainsKey( "Latitude" ) && statusLatitude != null &&
                 !data.ContainsKey( "Longitude" ) && statusLongitude != null &&
                 ( !onlyIfOnFoot || statusOnFootOnPlanet ) && 
                 statusTimeStamp != null && Math.Abs(((DateTime)statusTimeStamp - timestamp).TotalSeconds) <= timestampThresholdSeconds &&
                 journalBodyName.Equals(statusBodyName, StringComparison.InvariantCultureIgnoreCase))
            {
                data.Add( "Latitude", statusLatitude );
                data.Add( "Longitude", statusLongitude );
            }
            return data;
        }

        internal bool CheckLocationData ( string edType, IDictionary<string, object> data )
        {
            // Confirm the location data in memory is as accurate as possible when handling an event with partial location data
            if ( IsFullStarSystemLocationEvent( edType ) && StarSystemLocationIsSet() ) { return true; }

            // Can only send journal data if we know our current location data is correct
            // If any location data is null, data shall not be sent to EDDN.
            if ( StarSystemLocationIsSet() )
            {
                // The `Docked` event doesn't provide system coordinates, and the `Scan`event doesn't provide any system location data.
                // The EDDN journal schema requires that we enrich the journal event data with coordinates and system name (and system address if possible).
                if ( data.ContainsKey( "BodyName" ) && !data.ContainsKey( "SystemName" ) )
                {
                    // Apply heuristics to weed out mismatched systems and bodies
                    ConfirmScan( JsonParsing.getString( data, "BodyName" ) );
                }

                if ( !data.ContainsKey( "SystemName" ) )
                {
                    // Out of an overabundance of caution, we do not use data from our saved star systems to enrich the data we send to EDDN, 
                    // but we do use it as an independent check to make sure our system name and coordinates are accurate
                    ConfirmName();
                }

                if ( !data.ContainsKey( "StarPos" ) )
                {
                    ConfirmCoordinates();
                }

                if ( StarSystemLocationIsSet() )
                {
                    invalidState = false;
                    return true;
                }

                if ( !invalidState )
                {
                    invalidState = true;
                    Logging.Warn( "The EDDN responder is in an invalid state and is unable to send messages.",
                        new Dictionary<string, object> { { "EDDN State", this }, { "Event", data } } );
                }
            }

            return false;
        }

        internal bool ConfirmName()
        {
            if ( systemAddress > 0 )
            {
                StarSystem system;
                if (systemAddress == EDDI.Instance.GameState.CurrentStarSystem?.systemAddress)
                {
                    system = EDDI.Instance.GameState.CurrentStarSystem;
                }
                else
                {
                    system = EDDI.Instance.DataProvider.GetOrFetchQuickStarSystemAsync( systemAddress, true )
                        .GetResultOrTimeout( TimeSpan.FromSeconds( 10 ) );
                }

                if ( systemName == system?.systemname )
                {
                    return true;
                }
            }

            // If the name and system address are inconsistent then reset our location
            ClearLocation();
            return false;
        }

        internal bool ConfirmCoordinates ()
        {
            if ( systemAddress > 0 && systemX != null && systemY != null && systemZ != null )
            {
                var massCode = (int)(systemAddress & 7);
                var boxelSize = 10 * (1 << massCode);
                var calcX = ((((systemAddress >> (30 - (massCode * 2))) & (ulong)(0x3FFF >> massCode)) << massCode) * 10.0) - 49985;
                var calcY = ((((systemAddress >> (17 - massCode)) & (ulong)(0x1FFF >> massCode)) << massCode) * 10.0) - 40985;
                var calcZ = ((((systemAddress >> 3) & (ulong)(0x3FFF >> massCode)) << massCode) * 10.0) - 24105;
                var xDiff = (double)systemX - calcX;
                var yDiff = (double)systemY - calcY;
                var zDiff = (double)systemZ - calcZ;
                if ( !( xDiff < 0 ) && !( xDiff > boxelSize ) && !( yDiff < 0 ) && !( yDiff > boxelSize ) && !( zDiff < 0 ) && !( zDiff > boxelSize ) )
                {
                    return true;
                }

                Logging.Warn( $"Cached coordinates for system address ({systemAddress}) appear to be incorrect ({systemX},{systemY},{systemZ}) should be within {boxelSize}LY of ({calcX},{calcY},{calcZ}). Actual differences ({xDiff},{yDiff},{zDiff})" );

                // Set values to null if data can't be confirmed. 
                systemX = null;
                systemY = null;
                systemZ = null;
            }

            return false;
        }

        internal bool ConfirmScan(string scannedBodyName)
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
                    if (!GeneratedRegex.PROC_GEN_SYSTEM_BODY().IsMatch(scannedBodyName))
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
