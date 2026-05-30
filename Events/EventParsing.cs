using EddiDataDefinitions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    public class EventParsing
    {
        private static Superpower Allegiance ( IDictionary<string, object> data, string key )
        {
            data.TryGetValue( key, out var val );
            // FD may send "" rather than null;
            return Superpower.FromNameOrEdName( Convert.ToString( val ) );
        }

        public static string CrewRole ( IDictionary<string, object> data, string key )
        {
            var role = JsonParsing.getString(data, key);
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

        public static Engineer Engineer ( IDictionary<string, object> data )
        {
            try
            { 
                var engineer = JsonParsing.getString(data, "Engineer");
                var engineerId = JsonParsing.getLong(data, "EngineerID");
                var rank = JsonParsing.getOptionalInt(data, "Rank");
                var rankProgress = JsonParsing.getOptionalInt( data, "RankProgress" );
                var stage = JsonParsing.getString(data, "Progress");
                return new Engineer( engineer, engineerId, stage, rankProgress, rank );
            }
            catch ( Exception e )
            {
                var dict = new Dictionary<string, object> { { "data", data }, { "exception", e } };
                Logging.Warn("Failed to parse malformed journal Engineer data", dict);
            }

            return null;
        }

        public static Faction Faction ( IDictionary<string, object> data, string type, string systemName,
            ulong systemAddress, List<Faction> systemFactions = null )
        {
            Faction faction = null;

            // Get the faction name and state
            if ( data.TryGetValue( type + "Faction", out var factionVal ) )
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
                    faction.Allegiance = Allegiance( data, type + "Allegiance" );
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
                            faction.Allegiance = Allegiance( factionDetail, "Allegiance" );
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

        public static List<Conflict> FactionConflicts ( object conflictsVal, List<Faction> factions )
        {
            if ( conflictsVal is null || factions is null )
            { return null; }

            var conflicts = new List<Conflict>();
            if ( conflictsVal is List<object> conflictsList )
            {
                foreach ( var conflictDetail in conflictsList.Cast<IDictionary<string, object>>() )
                {
                    var conflictType =
                        FactionState.FromEDName( JsonParsing.getString( conflictDetail, "WarType" ) ) ??
                        FactionState.None;
                    var status = JsonParsing.getString( conflictDetail, "Status" );

                    // Faction 1
                    conflictDetail.TryGetValue( "Faction1", out var faction1Val );
                    var faction1Detail = (Dictionary<string, object>)faction1Val;
                    var faction1Name = JsonParsing.getString( faction1Detail, "Name" );
                    var faction1 = factions.Find( f => f.name == faction1Name );
                    var faction1Stake = JsonParsing.getString( faction1Detail, "Stake_Localised" ) 
                                        ?? JsonParsing.getString( faction1Detail, "Stake" );
                    var faction1DaysWon = JsonParsing.getInt( faction1Detail, "WonDays" );

                    // Faction 2
                    conflictDetail.TryGetValue( "Faction2", out var faction2Val );
                    var faction2Detail = (Dictionary<string, object>)faction2Val;
                    var faction2Name = JsonParsing.getString( faction2Detail, "Name" );
                    var faction2 = factions.Find( f => f.name == faction2Name );
                    var faction2Stake = JsonParsing.getString( faction2Detail, "Stake_Localised" ) 
                                        ?? JsonParsing.getString( faction2Detail, "Stake" );
                    var faction2DaysWon = JsonParsing.getInt( faction2Detail, "WonDays" );

                    conflicts.Add( new Conflict( conflictType, status, faction1, faction1Stake, faction1DaysWon,
                        faction2, faction2Stake, faction2DaysWon ) );
                }
            }

            return conflicts;
        }

        public static string FactionName ( IDictionary<string, object> data, string key )
        {
            var faction = JsonParsing.getString(data, key);
            // Might be a superpower...
            var superpowerFaction = Superpower.FromNameOrEdName(faction);
            return superpowerFaction?.invariantName ?? faction;
        }

        public static List<Faction> Factions ( object factionsVal, string systemName, ulong systemAddress )
        {
            var factions = new List<Faction>();
            if ( factionsVal is List<object> factionsList )
            {
                foreach ( var factionDetail in factionsList.Cast<IDictionary<string, object>>() )
                {
                    // Core data
                    var fName = JsonParsing.getString( factionDetail, "Name" );
                    var fState =
                        FactionState.FromEDName( JsonParsing.getString( factionDetail, "FactionState" ) ) ??
                        FactionState.None;
                    var fGov =
                        Government.FromEDName( JsonParsing.getString( factionDetail, "Government" ) ) ??
                        Government.FromEDName( JsonParsing.getString( factionDetail, "SystemGovernment" ) ) ??
                        Government.None;
                    var influence =
                        JsonParsing.getDecimal( factionDetail, "Influence" ) * 100; // Convert from a 0-1 scale to 0-100
                    var fAllegiance = Allegiance( factionDetail, "Allegiance" );
                    var happiness =
                        Happiness.FromEDName( JsonParsing.getString( factionDetail, "Happiness" ) ?? string.Empty );
                    var myReputation = JsonParsing.getOptionalDecimal( factionDetail, "MyReputation" ) ?? 0;

                    var fFaction = new Faction()
                    {
                        name = fName,
                        Government = fGov,
                        Allegiance = fAllegiance,
                        myreputation = myReputation
                    };

                    var factionPresense = new FactionPresence()
                    {
                        systemName = systemName,
                        systemAddress = systemAddress,
                        FactionState = fState,
                        influence = influence,
                        Happiness = happiness,
                    };

                    // Active states
                    factionDetail.TryGetValue( "ActiveStates", out var activeStatesVal );
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
                    factionDetail.TryGetValue( "PendingStates", out var pendingStatesVal );
                    if ( pendingStatesVal != null )
                    {
                        var pendingStatesList = (List<object>)pendingStatesVal;
                        foreach ( var pendingState in pendingStatesList.Cast<IDictionary<string, object>>() )
                        {
                            var pTrendingState = new FactionTrendingState(
                                FactionState.FromEDName( JsonParsing.getString( pendingState, "State" ) ) ??
                                FactionState.None,
                                JsonParsing.getOptionalInt( pendingState, "Trend" )
                            );
                            factionPresense.PendingStates.Add( pTrendingState );
                        }
                    }

                    // Recovering states
                    factionDetail.TryGetValue( "RecoveringStates", out var recoveringStatesVal );
                    if ( recoveringStatesVal != null )
                    {
                        var recoveringStatesList = (List<object>)recoveringStatesVal;
                        foreach ( var recoveringState in recoveringStatesList.Cast<IDictionary<string, object>>() )
                        {
                            var rTrendingState = new FactionTrendingState(
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

        [CanBeNull]
        public static OrganicVariant GetOrganicVariant ( IDictionary<string, object> data )
        {
            // i.e. Green
            var variantEDName = JsonParsing.getString(data, "Variant");
            var localizedVariant = JsonParsing.getString(data, "Variant_Localised");
            var variant = OrganicVariant.FromEDName(variantEDName);
            if ( variant != null && !string.IsNullOrEmpty( localizedVariant ) )
            {
                variant.fallbackLocalizedName = localizedVariant;
            }
            if ( variant is null && data.ContainsKey( "Variant" ) )
            {
                Logging.Error( "Unable to parse organic variant data.", data );
            }
            return variant;
        }

        [CanBeNull]
        public static OrganicSpecies GetOrganicSpecies ( IDictionary<string, object> data )
        {
            // i.e. Flabellum
            var speciesEDName = JsonParsing.getString(data, "Species");
            var localizedSpecies = JsonParsing.getString(data, "Species_Localised");
            var species = OrganicSpecies.FromEDName(speciesEDName);
            if ( species != null && !string.IsNullOrEmpty( localizedSpecies ) )
            {
                species.fallbackLocalizedName = localizedSpecies;
            }
            if ( species is null && data.ContainsKey( "Species" ) )
            {
                Logging.Warn( "Unable to parse organic species data.", data );
            }
            return species;
        }

        [CanBeNull]
        public static OrganicGenus GetOrganicGenus ( IDictionary<string, object> data )
        {
            // i.e. Frutexa
            var genusEDName = JsonParsing.getString(data, "Genus");
            var localizedGenus = JsonParsing.getString(data, "Genus_Localised");
            var genus = OrganicGenus.FromEDName(genusEDName);
            if ( genus != null && !string.IsNullOrEmpty( localizedGenus ) )
            {
                genus.fallbackLocalizedName = localizedGenus;
            }
            if ( genus is null && data.ContainsKey( "Genus" ) )
            {
                Logging.Warn( "Unable to parse organic genus data.", data );
            }
            return genus;
        }

        public static StationLandingPads LandingPadsCollection ( IDictionary<string, object> data )
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

        public static MicroResource MicroResource ( IDictionary<string, object> resourceData )
        {
            var edname = JsonParsing.getString(resourceData, "Name");
            var microResource = EddiDataDefinitions.MicroResource.FromEDName(edname);
            if ( microResource is null )
            { return null; }

            var fallbackName = JsonParsing.getString(resourceData, "Name_Localised");
            microResource.fallbackLocalizedName = fallbackName;

            var category = JsonParsing.getString(resourceData, "Category");
            var fallbackCategoryName = JsonParsing.getString(resourceData, "Category_Localised");
            microResource.Category ??= MicroResourceCategory.FromEDName( category );
            if ( microResource.Category != null )
            {
                microResource.Category.fallbackLocalizedName = fallbackCategoryName;
            }
            return microResource;
        }
        
        public static void PowerplayDetails ( IDictionary<string, object> data, ulong systemAddress,
            out Power controllingPower, out List<Power> powersInAcquisitionRange, out PowerplayState powerplayState,
            out List<PowerAcquisitionProgress> powerAcquisitionProgress, out decimal controlProgress, out int reinforcementControlPoints, out int underminingControlPoints )
        {
            controllingPower = Power.FromEDName( JsonParsing.getString( data, "ControllingPower" ) );

            // This is a list of all powers within range to acquire this star system
            powersInAcquisitionRange = [ ];
            data.TryGetValue( "Powers", out var powersVal );
            if ( powersVal is List<object> powerNames )
            {
                foreach ( var powerName in powerNames )
                {
                    powersInAcquisitionRange.Add( Power.FromEDName( (string)powerName ) );
                }
            }

            // While the system is not controlled by any power there is a `PowerplayConflictProgress` key which measures the progress of each power towards control of that star system
            powerAcquisitionProgress = [ ];
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

        // Be sensible with health - round it unless it's very low
        public static decimal sensibleHealth ( decimal health )
        {
            return ( health < 10 ? Math.Round( health, 1 ) : Math.Round( health ) );
        }

        public static SignalSource SignalSource ( IDictionary<string, object> data )
        {
            var signalTypeEdName = JsonParsing.getString( data, "SignalType" );

            // The source may be a direct source or a USS. If a USS, we want the USS type.

            SignalSource source;
            if ( JsonParsing.getString( data, "USSType" ) != null )
            {
                var signalSource = JsonParsing.getString(data, "USSType");
                source = EddiDataDefinitions.SignalSource.FromEDName( signalSource ) ?? new SignalSource();
                var localizedName = JsonParsing.getString(data, "USSType_Localised");
                if ( !string.IsNullOrEmpty( localizedName ) && !localizedName.Contains( '$' ) )
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
                    source = EddiDataDefinitions.SignalSource.FromStationEDName( signalSource, signalTypeEdName );
                }
                else
                {
                    source = EddiDataDefinitions.SignalSource.FromEDName( signalSource ) ?? new SignalSource();
                    var localizedName = JsonParsing.getString(data, "SignalName_Localised");
                    if ( !string.IsNullOrEmpty( localizedName ) && !localizedName.Contains( '$' ) )
                    {
                        source.fallbackLocalizedName = localizedName;
                    }
                }
                source.isStation = isStation;
            }

            source.signalType = signalTypeEdName is null ? null : SignalType.FromEDName( signalTypeEdName );

            return source;
        }

        public static Compartment ShipCompartment ( string shipEDName, string slot )
        {
            var compartment = new Compartment() { name = slot };

            // Standard compartment slots are in the form of "SlotNN_SizeN"
            var matches = GeneratedRegex.ShipSlotSizeRegex().Match(compartment.name);
            if ( matches.Success )
            {
                compartment.size = int.Parse( matches.Groups[ 2 ].Value );
            }

            // Specialty slots are in the form of "CargoNN", "MilitaryNN", or "PassengerNN"
            else if ( GeneratedRegex.ShipSpecialtySlotRegex().IsMatch( compartment.name ) )
            {
                var specialtySlotSizes = ShipDefinitions.FromEDModel(shipEDName, false)?.specialtySlotSizes;
                if ( specialtySlotSizes != null && specialtySlotSizes.TryGetValue( slot, out var slotSize ) )
                {
                    compartment.size = slotSize;
                }
                else
                {
                    Logging.Error( $"Unexpected specialty slot '{slot}' observed in ship edName '{shipEDName}'." );
                }
            }

            // Additional slots are reserved for core modules and various cosmetics.

            return compartment;
        }

        public static void StationNameAndType ( IDictionary<string, object> data, out string stationName, out string stationLocalizedName, out StationModel stationModel )
        {
            stationName = JsonParsing.getString( data, "StationName" );
            stationLocalizedName = JsonParsing.getString( data, "StationName_Localised" );
            var stationTypeEdName = JsonParsing.getString( data, "StationType" );
            stationModel = null;

            if ( string.IsNullOrEmpty( stationName ) || string.IsNullOrEmpty( stationTypeEdName ) ) { return; }
            
            // Normalize Powerplay Stronghold Carrier names
            stationName = GeneratedRegex.StrongholdCarrierRegex().Replace( stationName, "Stronghold Carrier" );

            // Fix known incorrectly reported StationType values.
            if ( stationName == "Stronghold Carrier" || stationName.StartsWith( "$EXT_PANEL_ColonisationShip" ) )
            {
                stationTypeEdName = StationModel.Megaship.edname;
            }
            else if ( !string.IsNullOrEmpty(stationTypeEdName) && 
                      StationModel.FleetCarrier.edname.Equals( stationTypeEdName, StringComparison.OrdinalIgnoreCase) &&
                      data.TryGetValue( "StationServices", out var services) && 
                      JArray.FromObject( services ).ToString().Contains("squadronBank", StringComparison.OrdinalIgnoreCase) )
            {
                // Fix instances of Squadron Carriers being reported as Fleet Carriers.
                stationTypeEdName = StationModel.SquadronCarrier.edname;
            }
            else
            {
                stationTypeEdName = JsonParsing.getString( data, "StationType" );
            }

            stationModel = StationModel.FromEDName( stationTypeEdName ) ?? StationModel.None;
        }
        
        public static void ThargoidWarData ( IDictionary<string, object> data, out ThargoidWar thargoidWar )
        {
            thargoidWar = null;

            if ( data.TryGetValue( "ThargoidWar", out var thargoidWarObj ) &&
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
    }
}
