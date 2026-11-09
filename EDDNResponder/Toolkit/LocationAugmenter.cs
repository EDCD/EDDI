using EddiCore;
using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
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
        private enum StationPosture { Unknown, DockedShip, OnFoot, OtherVessel }
        private StationPosture stationPosture;
        internal bool ShipDocked => stationPosture == StationPosture.DockedShip;
        internal bool AboardCarrier { get; private set; }
        internal bool OnFootAtCarrier => AboardCarrier && stationPosture == StationPosture.OnFoot;

        // Journal Body Info
        public string journalBodyName { get; private set; }
        public int? journalBodyId { get; private set; }
        private ulong journalBodySystemAddress;
        private DateTime? journalBodyTimeStamp;
        private DateTime? bodyContextTimeStamp;

        // Status Info
        public string statusBodyName { get; private set; }
        public bool statusOnFootOnPlanet { get; private set; }
        public decimal? statusLatitude { get; private set; }
        public decimal? statusLongitude { get; private set; }
        public DateTime? statusTimeStamp { get; private set; }
        private ulong statusSystemAddress;

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
            if ( statusTimeStamp != null && status.timestamp < statusTimeStamp ) { return; }
            statusBodyName = !string.IsNullOrEmpty(status.bodyname) ? status.bodyname : null;
            statusOnFootOnPlanet = status.on_foot_on_planet;
            statusLatitude = status.latitude;
            statusLongitude = status.longitude;
            statusTimeStamp = status.timestamp;
            statusSystemAddress = StarSystemLocationIsSet() && bodyContextTimeStamp != null &&
                                  status.timestamp >= bodyContextTimeStamp ? systemAddress : 0;
        }

        internal void GetLocationInfo(string edType, IDictionary<string, object> data)
        {
            // We always start location data fresh when handling events containing complete star system location data
            if (IsFullStarSystemLocationEvent(edType))
            {
                // On-foot CarrierJump events omit station identity. Retain only a carrier
                // already identified by journal evidence, with no intervening departure.
                var retainCarrier = edType == "CarrierJump" && AboardCarrier && !StationIdentityConflicts( data );
                ClearStarSystemLocation();
                if ( !retainCarrier ) { ClearStationLocation(); }
            }

            GetStarSystemLocation(edType, data);
            if ( IsFullStarSystemLocationEvent( edType ) && StarSystemLocationIsSet() )
            {
                bodyContextTimeStamp = EventTimeStamp( data );
            }
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
                    ClearLocation();
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
                    {
                        SetStationLocation( data, StationPosture.DockedShip, IsCarrier( data ) );
                        break;
                    }
                    case "CarrierJump":
                    {
                        if ( JsonParsing.getOptionalBool( data, "OnFoot" ) == true )
                        {
                            if ( HasStationIdentity( data ) ) { SetStationLocation( data, StationPosture.OnFoot, true ); }
                            else if ( AboardCarrier ) { stationPosture = StationPosture.OnFoot; }
                            else { ClearStationLocation(); }
                        }
                        else if ( JsonParsing.getOptionalBool( data, "Docked" ) == true )
                        {
                            // A taxi or another commander's vessel can be docked without
                            // establishing that the commander is back in their own ship.
                            var posture = stationPosture == StationPosture.OtherVessel
                                ? StationPosture.OtherVessel : StationPosture.DockedShip;
                            if ( HasStationIdentity( data ) ) { SetStationLocation( data, posture, true ); }
                            else if ( AboardCarrier ) { stationPosture = posture; }
                            else { ClearStationLocation(); }
                        }
                        else if ( AboardCarrier ) { stationPosture = StationPosture.OtherVessel; }
                        else { ClearStationLocation(); }
                        break;
                    }
                    case "Location":
                    {
                        var docked = JsonParsing.getOptionalBool( data, "Docked" ) == true;
                        if ( docked || ( JsonParsing.getOptionalBool( data, "OnFoot" ) == true && IsCarrier( data ) ) )
                        {
                            SetStationLocation( data, docked ? StationPosture.DockedShip : StationPosture.OnFoot, IsCarrier( data ) );
                        }
                        else { ClearStationLocation(); }
                        break;
                    }
                    case "Disembark":
                    case "Embark":
                    {
                        var ownShip = JsonParsing.getOptionalBool( data, "SRV" ) != true &&
                                      JsonParsing.getOptionalBool( data, "Taxi" ) != true &&
                                      JsonParsing.getOptionalBool( data, "Multicrew" ) != true;
                        var posture = edType == "Disembark" ? StationPosture.OnFoot
                            : ownShip ? StationPosture.DockedShip : StationPosture.OtherVessel;

                        if ( JsonParsing.getOptionalBool( data, "OnStation" ) == true && HasStationIdentity( data ) )
                        {
                            var carrier = IsCarrier( data ) ||
                                          ( !data.ContainsKey( "StationType" ) && AboardCarrier && !StationIdentityConflicts( data ) );
                            SetStationLocation( data, posture, carrier );
                        }
                        else if ( AboardCarrier && !StationIdentityConflicts( data ) )
                        {
                            // Disembark can report OnStation:false / OnPlanet:true while aboard
                            // a carrier, without names or IDs. Those flags alone do not prove departure.
                            // Boarding any vessel changes posture, not carrier presence.
                            stationPosture = posture;
                        }
                        else { ClearStationLocation(); }
                        break;
                    }
                    case "FSDJump":
                    case "StartJump":
                    case "SupercruiseEntry":
                    case "Undocked":
                    case "LoadGame":
                    case "Shutdown":
                    {
                        ClearStationLocation();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse market location data for EDDN", ex);
            }
        }

        private static bool IsCarrier ( IDictionary<string, object> data ) =>
            string.Equals( JsonParsing.getString( data, "StationType" ), "FleetCarrier", StringComparison.OrdinalIgnoreCase ) ||
            string.Equals( JsonParsing.getString( data, "StationType" ), "SquadronCarrier", StringComparison.OrdinalIgnoreCase );

        private static bool HasStationIdentity ( IDictionary<string, object> data ) =>
            JsonParsing.getOptionalLong( data, "MarketID" ) is > 0 &&
            !string.IsNullOrWhiteSpace( JsonParsing.getString( data, "StationName" )?.TrimEnd( '+', ' ' ) );

        private bool StationIdentityConflicts ( IDictionary<string, object> data ) =>
            ( data.ContainsKey( "MarketID" ) && JsonParsing.getOptionalLong( data, "MarketID" ) != marketId ) ||
            ( data.ContainsKey( "StationName" ) &&
              !string.Equals( stationName, JsonParsing.getString( data, "StationName" )?.TrimEnd( '+', ' ' ), StringComparison.OrdinalIgnoreCase ) );

        private void SetStationLocation ( IDictionary<string, object> data, StationPosture posture, bool carrier )
        {
            marketId = JsonParsing.getOptionalLong( data, "MarketID" );
            stationName = JsonParsing.getString( data, "StationName" )?.TrimEnd( '+', ' ' );
            stationPosture = posture;
            AboardCarrier = carrier && HasStationIdentity( data );
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
                        journalBodySystemAddress = systemAddress;
                        journalBodyTimeStamp = EventTimeStamp( data );
                        if ( edType == "Location" && JsonParsing.getString( data, "BodyType" ) == "Station" )
                        {
                            journalBodyName = null;
                            journalBodyId = null;
                        }
                        break;
                    }
                    case "LeaveBody":
                    {
                        var hadBodyContext = bodyContextTimeStamp != null;
                        ClearBodyLocation();
                        if ( hadBodyContext ) { bodyContextTimeStamp = EventTimeStamp( data ); }
                        break;
                    }
                    case "LoadGame":
                    case "Shutdown":
                    {
                        ClearBodyLocation();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse body location data for EDDN", ex);
            }
        }

        internal void Reset ()
        {
            ClearLocation();
            invalidState = false;
        }

        private void ClearBodyLocation ()
        {
            journalBodyName = null;
            journalBodyId = null;
            journalBodySystemAddress = 0;
            journalBodyTimeStamp = null;
            bodyContextTimeStamp = null;
            statusBodyName = null;
            statusOnFootOnPlanet = false;
            statusLatitude = null;
            statusLongitude = null;
            statusTimeStamp = null;
            statusSystemAddress = 0;
        }

        private void ClearLocation()
        {
            ClearStarSystemLocation();
            ClearStationLocation();
        }

        private void ClearStationLocation ()
        {
            stationName = null;
            marketId = null;
            stationPosture = StationPosture.Unknown;
            AboardCarrier = false;
        }

        internal bool CheckStationData ( JObject profileJson, JObject marketJson, JObject shipyardJson )
        {
            // CAPI's lastStarport can remain populated after departure; journal evidence
            // must establish either ship docking or on-foot presence aboard this carrier.
            if ( string.IsNullOrWhiteSpace( systemName ) || string.IsNullOrWhiteSpace( stationName ) ||
                 marketId is not > 0 ) { return false; }

            var commander = profileJson?["commander"] as JObject;
            var system = profileJson?["lastSystem"] as JObject;
            var station = profileJson?["lastStarport"] as JObject;
            var profileDocked = commander?["docked"]?.Type == JTokenType.Boolean && commander["docked"].Value<bool>();
            var profileOnFoot = commander?["onfoot"]?.Type == JTokenType.Boolean && commander["onfoot"].Value<bool>();
            if ( !( ( ShipDocked && profileDocked ) || ( OnFootAtCarrier && profileOnFoot ) ) ||
                 !string.Equals( systemName, system?["name"]?.ToString(), StringComparison.Ordinal ) ||
                 !MatchesStation( station ) ) { return false; }

            // A combined callback must describe one station throughout. Missing endpoints are
            // allowed, but every supplied station endpoint must agree before any schema runs.
            return ( marketJson != null || shipyardJson != null ) &&
                   ( marketJson == null || MatchesStation( marketJson ) ) &&
                   ( shipyardJson == null || MatchesStation( shipyardJson ) );
        }

        private bool MatchesStation ( JObject station )
        {
            return station != null &&
                   long.TryParse( station["id"]?.ToString(), out var id ) &&
                   id == marketId &&
                   string.Equals( stationName, station["name"]?.ToString()?.TrimEnd( '+', ' ' ), StringComparison.OrdinalIgnoreCase );
        }

        private void ClearStarSystemLocation ()
        {
            ClearBodyLocation();
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
            if ( !HasCurrentBodyStatus( data, 60 ) || !EventBodyMatchesStatus( data ) ) { return data; }

            if ( !data.ContainsKey( "BodyName" ) )
            {
                data.Add( "BodyName", statusBodyName );
            }
            if ( !data.ContainsKey( "BodyID" ) && journalBodyId != null && JournalBodyMatchesStatus( data ) )
            {
                data.Add( "BodyID", journalBodyId.Value );
            }

            return data;
        }

        internal IDictionary<string, object> AugmentBodyLatLong(IDictionary<string, object> data, int timestampThresholdSeconds = 60, bool onlyIfOnFoot = false)
        {
            // Ref. https://github.com/EDCD/EDDN/blob/master/schemas/scanorganic-README.md
            if ( !data.ContainsKey( "Latitude" ) && statusLatitude != null &&
                 !data.ContainsKey( "Longitude" ) && statusLongitude != null &&
                 ( !onlyIfOnFoot || statusOnFootOnPlanet ) && 
                 HasCurrentBodyStatus( data, timestampThresholdSeconds ) &&
                 JournalBodyMatchesStatus( data ) && EventBodyMatchesStatus( data ) )
            {
                data.Add( "Latitude", statusLatitude );
                data.Add( "Longitude", statusLongitude );
            }
            return data;
        }

        private static DateTime? EventTimeStamp ( IDictionary<string, object> data )
        {
            if ( !data.ContainsKey( "timestamp" ) ) { return null; }
            try { return JsonParsing.getDateTime( "timestamp", data ); }
            catch ( ArgumentException ) { return null; }
            catch ( FormatException ) { return null; }
        }

        private bool MatchesBodySystem ( IDictionary<string, object> data )
        {
            if ( data.ContainsKey( "SystemAddress" ) && JsonParsing.getULong( data, "SystemAddress" ) != systemAddress ) { return false; }
            foreach ( var key in new[] { "StarSystem", "System" } )
            {
                var name = JsonParsing.getString( data, key );
                if ( !string.IsNullOrEmpty( name ) && !string.Equals( name, systemName, StringComparison.Ordinal ) ) { return false; }
            }
            return true;
        }

        private bool HasCurrentBodyStatus ( IDictionary<string, object> data, int timestampThresholdSeconds )
        {
            if ( bodyContextTimeStamp is not DateTime contextTimestamp || statusTimeStamp is not DateTime statusTimestamp ||
                 EventTimeStamp( data ) is not DateTime timestamp ) { return false; }
            return StarSystemLocationIsSet() && timestamp >= contextTimestamp &&
                   statusSystemAddress == systemAddress && statusTimestamp >= contextTimestamp &&
                   !string.IsNullOrWhiteSpace( statusBodyName ) &&
                   !string.Equals( statusBodyName, stationName, StringComparison.OrdinalIgnoreCase ) &&
                   Math.Abs( ( statusTimestamp - timestamp ).TotalSeconds ) <= timestampThresholdSeconds &&
                   MatchesBodySystem( data );
        }

        private bool JournalBodyMatchesStatus ( IDictionary<string, object> data ) =>
            journalBodySystemAddress == systemAddress && journalBodyTimeStamp != null && journalBodyTimeStamp <= EventTimeStamp( data ) &&
            !string.IsNullOrWhiteSpace( journalBodyName ) && string.Equals( journalBodyName, statusBodyName, StringComparison.OrdinalIgnoreCase );

        private bool EventBodyMatchesStatus ( IDictionary<string, object> data )
        {
            if ( data.ContainsKey( "BodyName" ) && !string.Equals( JsonParsing.getString( data, "BodyName" ), statusBodyName, StringComparison.OrdinalIgnoreCase ) ) { return false; }
            return !data.ContainsKey( "BodyID" ) ||
                   ( journalBodyId != null && JournalBodyMatchesStatus( data ) && JsonParsing.getOptionalInt( data, "BodyID" ) == journalBodyId );
        }

        internal bool CheckLocationData ( string edType, IDictionary<string, object> data )
        {
            // ScanOrganic does not update cached location, and CodexEntry uses System rather
            // than StarSystem. Neither may borrow mandatory coordinates from another system.
            if ( ( edType is "ScanOrganic" or "CodexEntry" ) && !MatchesBodySystem( data ) ) { return false; }

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
