using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiEvents
{
    public class EventParsing
    {
        public static void GetStationNameAndType ( IDictionary<string, object> data, out string stationName, out string stationLocalizedName,
    out StationModel stationModel )
        {
            stationName = JsonParsing.getString( data, "StationName" );
            stationLocalizedName = JsonParsing.getString( data, "StationName_Localised" );

            // Normalize Powerplay Stronghold Carrier names
            stationName = Regex.Replace( stationName, @"/^(Stronghold Carrier|Porte-vaisseaux de forteresse|Transportadora da potência|Носитель-база|Hochburg-Carrier|Portanaves bastión|\$ShipName_StrongholdCarrier(.*?))$/i", "Stronghold Carrier" );

            // Fix known incorrectly reported StationType values.
            string stationTypeEdName;
            if ( stationName == "Stronghold Carrier" || stationName.StartsWith( "$EXT_PANEL_ColonisationShip" ) )
            {
                stationTypeEdName = StationModel.Megaship.edname;
            }
            else
            {
                stationTypeEdName = JsonParsing.getString( data, "StationType" );
            }

            stationModel = StationModel.FromEDName( stationTypeEdName ) ?? StationModel.None;
        }

        public static StationLandingPads GetLandingPads ( IDictionary<string, object> data )
        {
            var landingPads = new StationLandingPads();
            if ( data.TryGetValue( "LandingPads", out var landingPadsObj ) && landingPadsObj is Dictionary<string, object> landingPadsDict )
            {
                if ( landingPadsDict.TryGetValue( "Small", out var smallPads ) )
                {
                    landingPads.Small = Convert.ToInt32( smallPads );
                }
                if ( landingPadsDict.TryGetValue( "Medium", out var mediumPads ) )
                {
                    landingPads.Medium = Convert.ToInt32( mediumPads );
                }
                if ( landingPadsDict.TryGetValue( "Large", out var largePads ) )
                {
                    landingPads.Large = Convert.ToInt32( largePads );
                }
            }
            return landingPads;
        }

        public static void GetThargoidWarData ( IDictionary<string, object> data, out ThargoidWar thargoidWar )
        {
            thargoidWar = null;

            if ( data.TryGetValue( "ThargoidWar", out object thargoidWarObj ) &&
                 thargoidWarObj is IDictionary<string, object> thargoidWarData )
            {
                try
                {
                    var currentState = JsonParsing.getString( thargoidWarData, "CurrentState" );
                    if ( string.IsNullOrEmpty( currentState ) )
                    {
                        return;
                    }
                    var successState = JsonParsing.getString( thargoidWarData, "NextStateSuccess" );
                    var failureState = JsonParsing.getString( thargoidWarData, "NextStateFailure" );

                    thargoidWar = new ThargoidWar()
                    {
                        CurrentState = FactionState.FromEDName( currentState ) ?? FactionState.None,
                        SuccessState = FactionState.FromEDName( successState ) ?? FactionState.None,
                        FailureState = FactionState.FromEDName( failureState ) ?? FactionState.None,
                        succeeded = JsonParsing.getBool( thargoidWarData, "SuccessStateReached" ),
                        progress =
                            Math.Min( JsonParsing.getDecimal( thargoidWarData, "WarProgress" ), 1 ) *
                            100, // Multiply by 100 to convert from 0-1 scale to percentage, don't report values greater than 100%
                    };

                    // Remaining days is not always present
                    var remainingDaysString = JsonParsing.getString( thargoidWarData, "EstimatedRemainingTime" );
                    if ( !string.IsNullOrEmpty( remainingDaysString ) )
                    {
                        thargoidWar.remainingDays = Convert.ToInt32( remainingDaysString
                            .Replace( " Days", "" )
                            .Replace( " Day", "" ) );
                    }

                    thargoidWar.remainingPorts = ( thargoidWar.CurrentState == FactionState.ThargoidProbing ||
                                                   thargoidWar.CurrentState == FactionState.ThargoidRecovery )
                        ? null
                        : JsonParsing.getOptionalInt( thargoidWarData, "RemainingPorts" );
                }
                catch ( Exception e )
                {
                    Logging.Error(
                        $"Failed to parse Thargoid war data: {JsonConvert.SerializeObject( thargoidWarData )}", e );
                }
            }
        }

        public static void GetPowerplayData ( IDictionary<string, object> data, ulong systemAddress,
            out Power controllingPower, out List<Power> powersInAcquisitionRange, out PowerplayState powerplayState,
            out List<PowerAcquisitionProgress> powerAcquisitionProgress, out decimal controlProgress, out int reinforcementControlPoints, out int underminingControlPoints )
        {
            controllingPower = Power.FromEDName( JsonParsing.getString( data, "ControllingPower" ) );

            // This is a list of all powers within range to acquire this star system
            powersInAcquisitionRange = new List<Power>();
            data.TryGetValue( "Powers", out object powersVal );
            if ( powersVal is List<object> powerNames )
            {
                foreach ( var powerName in powerNames )
                {
                    powersInAcquisitionRange.Add( Power.FromEDName( (string)powerName ) );
                }
            }

            // While the system is not controlled by any power there is a `PowerplayConflictProgress` key which measures the progress of each power towards control of that star system
            powerAcquisitionProgress = new List<PowerAcquisitionProgress>();
            if ( data.TryGetValue( "PowerplayConflictProgress", out var conflictProgressData ) &&
                 conflictProgressData is Dictionary<string, decimal> conflictProgress )
            {
                foreach ( var kvp in conflictProgress )
                {
                    // Convert percent progress values from a 0-1 scale to 0-100.
                    // Values over 100% are permitted and indicate that the power has exceeded the threshold to take control of the system.
                    powerAcquisitionProgress.Add( new PowerAcquisitionProgress( kvp.Key, kvp.Value * 100 ) );
                }
            }
            powerAcquisitionProgress = powerAcquisitionProgress
                .OrderByDescending( p => p.progress )
                .ToList();

            powerplayState = PowerplayState.FromEDName( JsonParsing.getString( data, "PowerplayState" ) );

            // There is a special 'Contested' powerplay state which applies when more than one power has achieved at least 30% progress towards control
            if ( powerplayState == PowerplayState.Unoccupied && powerAcquisitionProgress.Count( p => p.progress >= 30 ) > 1 )
            {
                powerplayState = PowerplayState.Contested;
            }

            // There is a special 'Headquarters' powerplay state which applies to the power's headquarters system. This system cannot be undermined.
            if ( powerplayState == PowerplayState.Stronghold && systemAddress == controllingPower?.hqSystemAddress )
            {
                powerplayState = PowerplayState.Headquarters;
            }

            // While the system is controlled by a power there is a `PowerplayStateControlProgress` key which measures progress towards or away from control of that star system
            controlProgress = JsonParsing.getOptionalDecimal( data, "PowerplayStateControlProgress" ) ?? 0;
            // Control progress seems to be affected by an overflow error which needs correction.
            if ( controlProgress > 4000 )
            {
                // Negative values indicate a reduction to the next lowest level of control of the system at the end of the cycle.
                // Positive values over 100% indicate an increase to the level of control of the system at the end of the cycle.
                var scale = 120000;
                if ( powerplayState == PowerplayState.Exploited )
                {
                    scale = 350000;
                }
                else if ( powerplayState == PowerplayState.Fortified )
                {
                    scale = 650000;
                }
                else if ( powerplayState == PowerplayState.Stronghold )
                {
                    scale = 1000000;
                }
                controlProgress -= ( (decimal)uint.MaxValue + 1 ) / scale;
            }
            // Convert percent progress values from a 0-1 scale to 0-100.
            controlProgress *= 100;

            // While the system is controlled by a power there are `PowerplayStateReinforcement` and `PowerplayStateUndermining` keys which record control points contributed by agents either reinforcing or undermining system control in the current cycle.
            reinforcementControlPoints = JsonParsing.getOptionalInt( data, "PowerplayStateReinforcement" ) ?? 0;
            underminingControlPoints = JsonParsing.getOptionalInt( data, "PowerplayStateUndermining" ) ?? 0;
        }

        public static Superpower GetAllegiance ( IDictionary<string, object> data, string key )
        {
            data.TryGetValue( key, out object val );
            // FD may send "" rather than null;
            return Superpower.FromNameOrEdName( Convert.ToString( val ) );
        }

        public static List<Conflict> GetConflicts ( object conflictsVal, List<Faction> factions )
        {
            if ( conflictsVal is null || factions is null )
            { return null; }

            List<Conflict> conflicts = new List<Conflict>();
            if ( conflictsVal is List<object> conflictsList )
            {
                foreach ( var conflictDetail in conflictsList.Cast<IDictionary<string, object>>() )
                {
                    FactionState conflictType =
                        FactionState.FromEDName( JsonParsing.getString( conflictDetail, "WarType" ) ) ??
                        FactionState.None;
                    string status = JsonParsing.getString( conflictDetail, "Status" );

                    // Faction 1
                    conflictDetail.TryGetValue( "Faction1", out object faction1Val );
                    Dictionary<string, object> faction1Detail = (Dictionary<string, object>)faction1Val;
                    string faction1Name = JsonParsing.getString( faction1Detail, "Name" );
                    Faction faction1 = factions.Find( f => f.name == faction1Name );
                    string faction1Stake = JsonParsing.getString( faction1Detail, "Stake" );
                    int faction1DaysWon = JsonParsing.getInt( faction1Detail, "WonDays" );

                    // Faction 2
                    conflictDetail.TryGetValue( "Faction2", out object faction2Val );
                    Dictionary<string, object> faction2Detail = (Dictionary<string, object>)faction2Val;
                    string faction2Name = JsonParsing.getString( faction2Detail, "Name" );
                    Faction faction2 = factions.Find( f => f.name == faction2Name );
                    string faction2Stake = JsonParsing.getString( faction2Detail, "Stake" );
                    int faction2DaysWon = JsonParsing.getInt( faction2Detail, "WonDays" );

                    conflicts.Add( new Conflict( conflictType, status, faction1, faction1Stake, faction1DaysWon,
                        faction2, faction2Stake, faction2DaysWon ) );
                }
            }

            return conflicts;
        }

        public static string GetFactionName ( IDictionary<string, object> data, string key )
        {
            string faction = JsonParsing.getString(data, key);
            // Might be a superpower...
            Superpower superpowerFaction = Superpower.FromNameOrEdName(faction);
            return superpowerFaction?.invariantName ?? faction;
        }

        public static Faction GetFaction ( IDictionary<string, object> data, string type, string systemName,
            ulong systemAddress, List<Faction> systemFactions = null )
        {
            Faction faction = null;

            // Get the faction name and state
            if ( data.TryGetValue( type + "Faction", out object factionVal ) )
            {
                var factionName = string.Empty;
                var factionState = FactionState.None;

                if ( factionVal is Dictionary<string, object> factionData ) // 3.3.03 or later journal
                {
                    factionName = JsonParsing.getString( factionData, "Name" );
                    factionState = FactionState.FromEDName( JsonParsing.getString( factionData, "FactionState" ) ) ??
                                   FactionState.None;
                }
                else if ( factionVal is string s ) // per-3.3.03 journal
                {
                    factionName = s;
                }

                if ( string.IsNullOrEmpty( factionName ) )
                {
                    throw new ArgumentException( "Faction information could not be parsed" );
                }

                faction = systemFactions?.FirstOrDefault( f => f.name == factionName ) ??
                          new Faction { name = factionName };

                // Get the faction information specific to the star system
                var factionPresense = faction.presences.FirstOrDefault( p => p.systemAddress == systemAddress );
                if ( factionPresense is null && systemAddress > 0 )
                {
                    factionPresense = new FactionPresence()
                    {
                        systemName = systemName,
                        systemAddress = systemAddress,
                        FactionState = factionState
                    };
                    faction.presences.Add( factionPresense );
                }

                // Get the controlling faction (system or station) government
                if ( data.TryGetValue( type + "Government", out _ ) )
                {
                    faction.Government = Government.FromEDName( JsonParsing.getString( data, type + "Government" ) ) ?? Government.None;
                }

                // Get the controlling faction (system or station) allegiance
                if ( data.TryGetValue( type + "Allegiance", out _ ) )
                {
                    faction.Allegiance = GetAllegiance( data, type + "Allegiance" );
                }
                else if ( data.TryGetValue( "Factions", out var val ) && val is List<object> factionsList )
                {
                    // Station controlling faction government is not directly available in 'Location' event
                    // so we have to find and match the faction name through the factions list.
                    foreach ( var factionDetail in factionsList.Cast<IDictionary<string, object>>() )
                    {
                        var fName = JsonParsing.getString( factionDetail, "Name" );
                        if ( fName == faction.name )
                        {
                            faction.Allegiance = GetAllegiance( factionDetail, "Allegiance" );
                            break;
                        }
                    }
                }
            }

            // Since systems can have Guardian or Thargoid allegiance, treat these like factions for the purpose of
            // allegiance and government (even when no human faction exists).
            if ( data.TryGetValue( type + "Allegiance", out var allegianceVal ) )
            {
                if ( allegianceVal is string allegiance )
                {
                    if ( allegiance.Equals( Superpower.Thargoid.invariantName ) )
                    {
                        faction = new Faction
                        {
                            name = Superpower.Thargoid.localizedName,
                            Allegiance = Superpower.Thargoid,
                            Government = Government.None
                        };
                    }
                    else if ( allegiance.Equals( Superpower.Guardian.invariantName ) )
                    {
                        faction = new Faction
                        {
                            name = Superpower.Guardian.localizedName,
                            Allegiance = Superpower.Guardian,
                            Government = Government.None
                        };
                    }
                }
            }

            return faction;
        }

        public static List<Faction> GetFactions ( object factionsVal, string systemName, ulong systemAddress )
        {
            List<Faction> factions = new List<Faction>();
            if ( factionsVal is List<object> factionsList )
            {
                foreach ( var factionDetail in factionsList.Cast<IDictionary<string, object>>() )
                {
                    // Core data
                    string fName = JsonParsing.getString( factionDetail, "Name" );
                    FactionState fState =
                        FactionState.FromEDName( JsonParsing.getString( factionDetail, "FactionState" ) ) ??
                        FactionState.None;
                    Government fGov =
                        Government.FromEDName( JsonParsing.getString( factionDetail, "SystemGovernment" ) ) ??
                        Government.None;
                    decimal influence =
                        JsonParsing.getDecimal( factionDetail, "Influence" ) * 100; // Convert from a 0-1 scale to 0-100
                    Superpower fAllegiance = GetAllegiance( factionDetail, "Allegiance" );
                    Happiness happiness =
                        Happiness.FromEDName( JsonParsing.getString( factionDetail, "Happiness" ) ?? string.Empty );
                    decimal myReputation = JsonParsing.getOptionalDecimal( factionDetail, "MyReputation" ) ?? 0;

                    Faction fFaction = new Faction()
                    {
                        name = fName,
                        Government = fGov,
                        Allegiance = fAllegiance,
                        myreputation = myReputation
                    };

                    FactionPresence factionPresense = new FactionPresence()
                    {
                        systemName = systemName,
                        systemAddress = systemAddress,
                        FactionState = fState,
                        influence = influence,
                        Happiness = happiness,
                    };

                    // Active states
                    factionDetail.TryGetValue( "ActiveStates", out object activeStatesVal );
                    if ( activeStatesVal != null )
                    {
                        var activeStatesList = (List<object>)activeStatesVal;
                        foreach ( var activeState in activeStatesList.Cast<IDictionary<string, object>>() )
                        {
                            factionPresense.ActiveStates.Add(
                                FactionState.FromEDName( JsonParsing.getString( activeState, "State" ) ) ??
                                FactionState.None );
                        }
                    }

                    // Pending states
                    factionDetail.TryGetValue( "PendingStates", out object pendingStatesVal );
                    if ( pendingStatesVal != null )
                    {
                        var pendingStatesList = (List<object>)pendingStatesVal;
                        foreach ( var pendingState in pendingStatesList.Cast<IDictionary<string, object>>() )
                        {
                            FactionTrendingState pTrendingState = new FactionTrendingState(
                                FactionState.FromEDName( JsonParsing.getString( pendingState, "State" ) ) ??
                                FactionState.None,
                                JsonParsing.getOptionalInt( pendingState, "Trend" )
                            );
                            factionPresense.PendingStates.Add( pTrendingState );
                        }
                    }

                    // Recovering states
                    factionDetail.TryGetValue( "RecoveringStates", out object recoveringStatesVal );
                    if ( recoveringStatesVal != null )
                    {
                        var recoveringStatesList = (List<object>)recoveringStatesVal;
                        foreach ( var recoveringState in recoveringStatesList.Cast<IDictionary<string, object>>() )
                        {
                            FactionTrendingState rTrendingState = new FactionTrendingState(
                                FactionState.FromEDName( JsonParsing.getString( recoveringState, "State" ) ) ??
                                FactionState.None,
                                JsonParsing.getOptionalInt( recoveringState, "Trend" )
                            );
                            factionPresense.RecoveringStates.Add( rTrendingState );
                        }
                    }

                    // Squadron data
                    fFaction.squadronfaction = JsonParsing.getOptionalBool( factionDetail, "SquadronFaction" ) ?? false;
                    factionPresense.squadronhappiestsystem =
                        JsonParsing.getOptionalBool( factionDetail, "HappiestSystem" ) ?? false;
                    factionPresense.squadronhomesystem =
                        JsonParsing.getOptionalBool( factionDetail, "HomeSystem" ) ?? false;

                    fFaction.presences.Add( factionPresense );
                    factions.Add( fFaction );
                }
            }

            return factions;
        }

        public static SignalSource GetSignalSourceName ( IDictionary<string, object> data )
        {
            var signalTypeEdName = JsonParsing.getString( data, "SignalType" );

            // The source may be a direct source or a USS. If a USS, we want the USS type.

            SignalSource source;
            if ( JsonParsing.getString( data, "USSType" ) != null )
            {
                var signalSource = JsonParsing.getString(data, "USSType");
                source = SignalSource.FromEDName( signalSource ) ?? new SignalSource();
                var localizedName = JsonParsing.getString(data, "USSType_Localised");
                if ( !string.IsNullOrEmpty( localizedName ) && !localizedName.Contains( "$" ) )
                {
                    source.fallbackLocalizedName = localizedName;
                }
            }
            else
            {
                var signalSource = JsonParsing.getString(data, "SignalName");
                var isStation = JsonParsing.getOptionalBool(data, "IsStation") ?? false;
                if ( isStation )
                {
                    source = SignalSource.FromStationEDName( signalSource, signalTypeEdName );
                }
                else
                {
                    source = SignalSource.FromEDName( signalSource ) ?? new SignalSource();
                    var localizedName = JsonParsing.getString(data, "SignalName_Localised");
                    if ( !string.IsNullOrEmpty( localizedName ) && !localizedName.Contains( "$" ) )
                    {
                        source.fallbackLocalizedName = localizedName;
                    }
                }
                source.isStation = isStation;
            }

            source.signalType = signalTypeEdName is null ? null : SignalType.FromEDName( signalTypeEdName );

            return source;
        }

        public static MicroResource GetMicroResource ( IDictionary<string, object> resourceData )
        {
            var edname = JsonParsing.getString(resourceData, "Name");
            var microResource = MicroResource.FromEDName(edname);
            if ( microResource is null )
            { return null; }

            var fallbackName = JsonParsing.getString(resourceData, "Name_Localised");
            microResource.fallbackLocalizedName = fallbackName;

            var category = JsonParsing.getString(resourceData, "Category");
            var fallbackCategoryName = JsonParsing.getString(resourceData, "Category_Localised");
            if ( microResource.Category is null )
            { microResource.Category = MicroResourceCategory.FromEDName( category ); }
            if ( microResource.Category != null )
            {
                microResource.Category.fallbackLocalizedName = fallbackCategoryName;
            }
            return microResource;
        }
        
        public static string ParseCrewRole ( IDictionary<string, object> data, string key )
        {
            string role = JsonParsing.getString(data, key);
            if ( role == "FireCon" )
            {
                role = "Gunner";
            }
            else if ( role == "FighterCon" )
            {
                role = "Fighter";
            }
            return role;
        }

        public static Engineer ParseEngineer ( IDictionary<string, object> data )
        {
            string engineer = JsonParsing.getString(data, "Engineer");
            long engineerId = JsonParsing.getLong(data, "EngineerID");
            data.TryGetValue( "Rank", out object rankVal );
            int? rank = (int?)(long?)rankVal;
            data.TryGetValue( "RankProgress", out object rankProgressVal );
            int? rankProgress = (int?)(long?)rankProgressVal;
            string stage = JsonParsing.getString(data, "Progress");
            return new Engineer( engineer, engineerId, stage, rankProgress, rank );
        }

        public static Compartment ParseShipCompartment ( string shipEDName, string slot )
        {
            Compartment compartment = new Compartment() { name = slot };

            // Compartment slots are in the form of "Slotnn_Sizen" or "Militarynn"
            if ( slot.Contains( "Slot" ) )
            {
                Match matches = Regex.Match(compartment.name, @"Size([0-9]+)");
                if ( matches.Success )
                {
                    compartment.size = Int32.Parse( matches.Groups[ 1 ].Value );
                }
            }
            else if ( slot.Contains( "Military" ) )
            {
                var slotSize = ShipDefinitions.FromEDModel(shipEDName, false)?.militarysize;
                if ( slotSize is null )
                {
                    // We didn't expect to have a military slot on this ship.
                    var data = new Dictionary<string, object>() { { "ShipEDName", shipEDName }, { "Slot", slot }, { "Exception", new ArgumentException() } };
                    Logging.Error( $"Unexpected military slot found in ship edName {shipEDName}.", data );
                    return compartment;
                }
                compartment.size = (int)slotSize;
            }
            return compartment;
        }

        // Be sensible with health - round it unless it's very low
        public static decimal sensibleHealth ( decimal health )
        {
            return ( health < 10 ? Math.Round( health, 1 ) : Math.Round( health ) );
        }
    }
}
