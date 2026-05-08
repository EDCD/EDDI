using EddiCompanionAppService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using EddiNavigationService;
using EddiSpeechService;
using EddiSpeechService.SpeechConversions;
using EddiVoiceAttackAdapter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackResponder
{
    public static class VoiceAttackVariables
    {
        private sealed class RuntimeActionBatchState
        {
            public List<PendingRuntimeAction> Actions { get; } = [];

            public int Depth { get; set; }
        }

        private static readonly AsyncLocal<RuntimeActionBatchState> runtimeActionBatch = new();

        // Cache for tracking last-dispatched values to prevent redundant IPC updates
        // Key format: ""'{action}:{key}'"" (e.g., ""'set_text:Ship name'"") â†’ value
        private static readonly ConcurrentDictionary<string, object> lastDispatchedValues = new();
        
        // The following variables notify changes via `PropertyChanged`
        private static readonly Dictionary<string, Action> StandardValues = new()
        {
            { nameof(EDDI.Instance.CurrentStarSystem), () => setStarSystemValues(EDDI.Instance.CurrentStarSystem, "System") },
            { nameof(EDDI.Instance.LastStarSystem), () => setStarSystemValues(EDDI.Instance.LastStarSystem, "Last system") },
            { nameof(EDDI.Instance.NextStarSystem), () => setStarSystemValues(EDDI.Instance.NextStarSystem, "Next system") },
            { nameof(EDDI.Instance.DestinationStarSystem), () => setStarSystemValues(EDDI.Instance.DestinationStarSystem, "Destination system") },
            { nameof(EDDI.Instance.DestinationDistanceLy), () => RuntimeSetDecimal("Destination system distance", EDDI.Instance.DestinationDistanceLy) },
            { nameof(EDDI.Instance.CurrentStellarBody), () => setDetailedBodyValues(EDDI.Instance.CurrentStellarBody, "Body") },
            { nameof(EDDI.Instance.CurrentStation), () => setStationValues(EDDI.Instance.CurrentStation, "Current station") },
            { nameof(EDDI.Instance.CurrentShip), () => setShipValues(ResolveCurrentShip(), "Ship") },
            { nameof(EDDI.Instance.Environment), () => RuntimeSetText("Environment", EDDI.Instance.Environment) },
            { nameof(EDDI.Instance.Vehicle), () => RuntimeSetText("Vehicle", EDDI.Instance.Vehicle) },
            { nameof(EDDI.Instance.inHorizons), () => RuntimeSetBoolean("horizons", EDDI.Instance.inHorizons) },
            { nameof(EDDI.Instance.inOdyssey), () => RuntimeSetBoolean("odyssey", EDDI.Instance.inOdyssey) },
        };

        // Cache property names extracted from fully-qualified keys for lookups during PropertyChanged events
        private static readonly Dictionary<string, Action> StandardValuesByPropertyName = BuildPropertyNameLookup();

        /// <summary>Build a lookup dictionary mapping property names to their actions </summary>
        private static Dictionary<string, Action> BuildPropertyNameLookup()
        {
            var lookup = new Dictionary<string, Action>();
            foreach (var kvp in StandardValues)
            {
                var lastDotIndex = kvp.Key.LastIndexOf('.');
                var propertyName = lastDotIndex >= 0 
                    ? kvp.Key.Substring(lastDotIndex + 1) 
                    : kvp.Key;
                lookup[propertyName] = kvp.Value;
            }
            return lookup;
        }

        internal static void updateStandardValues(PropertyChangedEventArgs eventArgs)
        {
            RunWithRuntimeActionBatch( () =>
            {
                // Update select values when triggered by a `PropertyChanged` event
                // Use pre-built lookup dictionary property name resolution
                if (eventArgs.PropertyName != null && StandardValuesByPropertyName.TryGetValue(eventArgs.PropertyName, out var action))
                {
                    var fullKey = StandardValues.FirstOrDefault(kvp => kvp.Value == action).Key;
                    if (fullKey != null)
                    {
                        try
                        {
                            LockManager.GetLock(fullKey, action);
                        }
                        catch (Exception ex)
                        {
                            Logging.Error($"Failed to set {fullKey}", ex);
                        }
                    }
                }

                // Update values not notified by `PropertyChanged` events
                RuntimeSetBoolean("cAPI active", CompanionAppService.Instance.active);
                RuntimeSetBoolean("ipa active", !ConfigService.Instance.speechServiceConfiguration.DisableIpa);
                RuntimeSetBoolean("icao active", ConfigService.Instance.speechServiceConfiguration.EnableIcao);
                RuntimeSetDecimal("Search system distance", NavigationService.Instance.SearchDistanceLy);
                setStarSystemValues(NavigationService.Instance.SearchStarSystem, "Search system" );
                setStationValues(NavigationService.Instance.SearchStation, "Search station" );
            } );
        }

        internal static void initializeStandardValues()
        {
            RunWithRuntimeActionBatch( () =>
            {
                setStatus( "Initializing" );
                
                foreach (var standardValue in StandardValues)
                {
                    try
                    {
                        // Wrap the standard value action to only execute if the underlying EDDI property has data
                        // This prevents caching of null/empty values during early startup, allowing proper
                        // initialization when monitors populate their data and PropertyChanged events fire
                        LockManager.GetLock(standardValue.Key, () =>
                        {
                            // Determine if the property has data before executing the action
                            var hasData = standardValue.Key switch
                            {
                                var k when k.Contains(nameof(EDDI.Instance.CurrentStarSystem)) => EDDI.Instance.CurrentStarSystem != null,
                                var k when k.Contains(nameof(EDDI.Instance.LastStarSystem)) => EDDI.Instance.LastStarSystem != null,
                                var k when k.Contains(nameof(EDDI.Instance.NextStarSystem)) => EDDI.Instance.NextStarSystem != null,
                                var k when k.Contains(nameof(EDDI.Instance.DestinationStarSystem)) => EDDI.Instance.DestinationStarSystem != null,
                                var k when k.Contains(nameof(EDDI.Instance.CurrentStellarBody)) => EDDI.Instance.CurrentStellarBody != null,
                                var k when k.Contains(nameof(EDDI.Instance.CurrentStation)) => EDDI.Instance.CurrentStation != null,
                                var k when k.Contains(nameof(EDDI.Instance.CurrentShip)) => ResolveCurrentShip() != null,
                                _ => true, // Other properties are assumed to always have data
                            };

                            if (hasData)
                            {
                                standardValue.Value();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Logging.Error($"Failed to initialize {standardValue.Key}", ex);
                    }
                }

                // Initialize ship information
                var shipConfig = ConfigService.Instance.shipMonitorConfiguration;
                var currentShip = ResolveCurrentShip();

                if ( currentShip != null )
                {
                    setShipValues( currentShip, "Ship" );
                }

                // Initialize shipyard information
                if ( shipConfig?.shipyard != null )
                {
                    setShipyardValues( shipConfig.shipyard.ToList() );
                }

                RuntimeSetText("EDDI version", Constants.EDDI_VERSION.ToString());
            } );
        }

        internal static void updateConfigurationValues ( object sender, PropertyChangedEventArgs e )
        {
            RunWithRuntimeActionBatch( () =>
            {
                if ( sender is ConfigService configService && e.PropertyName != null )
                {
                    if ( e.PropertyName.Equals( nameof( CargoMonitorConfiguration ), StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        var cargoConfig = configService.cargoMonitorConfiguration;
                        setCargo( cargoConfig );
                        return;
                    }

                    if ( e.PropertyName.Equals( nameof( CommanderConfiguration ), StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        var commanderMonitorVariables = EDDI.Instance.ObtainMonitor( "Commander Monitor" ).GetVariables();
                        if ( commanderMonitorVariables.TryGetValue( "cmdr", out var cmdrTuple ) &&
                             cmdrTuple.Item2 is Commander Cmdr )
                        {
                            setCommanderValues( Cmdr );
                        }

                        if ( commanderMonitorVariables.TryGetValue( "homesystem", out var homeSystemTuple ) &&
                             homeSystemTuple.Item2 is StarSystem homeSystem )
                        {
                            setStarSystemValues( homeSystem, "Home system" );
                        }

                        if ( commanderMonitorVariables.TryGetValue( "homestation", out var homeStationTuple ) &&
                             homeStationTuple.Item2 is Station homeStation )
                        {
                            setStationValues( homeStation, "Home station" );
                        }

                        if ( commanderMonitorVariables.TryGetValue( "squadronsystem", out var squadronSystemTuple ) &&
                             squadronSystemTuple.Item2 is StarSystem squadronSystem )
                        {
                            setStarSystemValues( squadronSystem, "Squadron system" );
                        }

                        return;
                    }

                    if ( e.PropertyName.Equals( nameof( FleetCarrierConfiguration ), StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        setFleetCarrierValues( ConfigService.Instance.fleetCarrierConfiguration.fleetCarrier, "Carrier" );
                        setFleetCarrierValues( ConfigService.Instance.fleetCarrierConfiguration.squadronCarrier, "Squadron carrier" );
                        return;
                    }

                    if ( e.PropertyName.Equals( nameof( ShipMonitorConfiguration ), StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        setShipValues( ResolveCurrentShip(), "Ship" );
                        Task.Run( () =>
                        {
                            var shipConfig = configService.shipMonitorConfiguration;
                            setShipyardValues( shipConfig.shipyard?.ToList() );
                        } );
                        return;
                    }

                    if ( e.PropertyName.Equals(nameof(SpeechServiceConfiguration), StringComparison.InvariantCultureIgnoreCase) )
                    {
                        RuntimeSetBoolean( "ipa active", !ConfigService.Instance.speechServiceConfiguration.DisableIpa );
                        RuntimeSetBoolean( "icao active", ConfigService.Instance.speechServiceConfiguration.EnableIcao );
                    }
                }
            } );
        }

        // Set values from a dictionary
        public static void setDictionaryValues ( IDictionary<string, object> dict, string prefix )
        {
            RunWithRuntimeActionBatch( () =>
            {
                foreach ( var key in dict.Keys )
                {
                    var varname = $"EDDI {prefix} {key}";
                    var value = dict[ key ];

                    // If value is null, clear all type variants
                    if ( value is null ) 
                    { 
                        RuntimeSetText( varname, null );
                        RuntimeSetInt( varname, null );
                        RuntimeSetDecimal( varname, null );
                        RuntimeSetBoolean( varname, null );
                        continue;
                    }

                    var s = value.ToString();
                    RuntimeSetText( varname, s );

                    // Try parsing as decimal (covers numeric types and booleans)
                    if ( value is decimal d || decimal.TryParse( s, out d ) )
                    {
                        RuntimeSetDecimal( varname, d );
                        RuntimeSetBoolean( varname, d != 0 );
                        if ( d <= int.MaxValue )
                        {
                            RuntimeSetInt( varname, System.Convert.ToInt32( Math.Round( d, MidpointRounding.AwayFromZero ) ) );
                        }
                    }
                    else
                    {
                        RuntimeSetDecimal( varname, null );
                    }

                    // Try parsing as int
                    if ( value is int i || int.TryParse( s, out i ) )
                    {
                        RuntimeSetInt( varname, i );
                    }

                    // Try parsing as bool
                    if ( value is bool b || bool.TryParse( s, out b ) )
                    {
                        RuntimeSetBoolean( varname, b );
                        RuntimeSetDecimal( varname, b ? 1 : 0 );
                        RuntimeSetInt( varname, b ? 1 : 0 );
                    }
                    else if ( !decimal.TryParse( s, out _ ) )
                    {
                        b = !string.IsNullOrEmpty( s );
                        RuntimeSetBoolean( varname, b );
                        RuntimeSetDecimal( varname, b ? 1 : 0 );
                        RuntimeSetInt( varname, b ? 1 : 0 );
                    }
                }
            } );
        }

        /// <summary>Set values for a station</summary>
        static void setStationValues(Station station, string prefix)
        {
            RunWithRuntimeActionBatch( () =>
            {
                Logging.Debug("Setting station information");

                RuntimeSetText( $"{prefix} name", station?.name);
                RuntimeSetDecimal( $"{prefix} distance from star", station?.distancefromstar);
                RuntimeSetText( $"{prefix} government", (station?.Faction?.Government ?? Government.None).localizedName);
                RuntimeSetText( $"{prefix} allegiance", (station?.Faction?.Allegiance ?? Superpower.None).localizedName);
                RuntimeSetText( $"{prefix} faction", station?.Faction?.name);
                RuntimeSetText( $"{prefix} state", (station?.Faction?.presences
                    .FirstOrDefault(p => p.systemAddress == station.systemAddress)?.FactionState ?? FactionState.None).localizedName);
                RuntimeSetText( $"{prefix} primary economy", station?.primaryeconomy);
                RuntimeSetText( $"{prefix} secondary economy", station?.secondaryeconomy);
                // Services
                RuntimeSetBoolean( $"{prefix} has refuel", station?.hasrefuel);
                RuntimeSetBoolean( $"{prefix} has repair", station?.hasrepair);
                RuntimeSetBoolean( $"{prefix} has rearm", station?.hasrearm);
                RuntimeSetBoolean( $"{prefix} has market", station?.hasmarket);
                RuntimeSetBoolean( $"{prefix} has black market", station?.hasblackmarket);
                RuntimeSetBoolean( $"{prefix} has outfitting", station?.hasoutfitting);
                RuntimeSetBoolean( $"{prefix} has shipyard", station?.hasshipyard);

                // Backwards-compatibility with 1.x documented variables
                RuntimeSetText( prefix, station?.name );

                Logging.Debug("Set station information");
            } );
        }

        static void setCommanderValues(Commander cmdr)
        {
            RunWithRuntimeActionBatch( () =>
            {
                try
                {
                    RuntimeSetText("Name", cmdr?.name);
                    RuntimeSetInt("Combat rating", cmdr?.combatrating?.rank);
                    RuntimeSetText("Combat rank", cmdr?.combatrating?.localizedName);
                    RuntimeSetInt("Trade rating", cmdr?.traderating?.rank);
                    RuntimeSetText("Trade rank", cmdr?.traderating?.localizedName);
                    RuntimeSetInt("Explore rating", cmdr?.explorationrating?.rank);
                    RuntimeSetText("Explore rank", cmdr?.explorationrating?.localizedName);
                    RuntimeSetInt("Empire rating", cmdr?.empirerating?.rank);
                    RuntimeSetText("Empire rank", cmdr?.empirerating?.maleRank.localizedName);
                    RuntimeSetInt("Federation rating", cmdr?.federationrating?.rank);
                    RuntimeSetText("Federation rank", cmdr?.federationrating?.localizedName);
                    RuntimeSetInt("Mercenary rating", cmdr?.mercenaryrating?.rank);
                    RuntimeSetText("Mercenary rank", cmdr?.mercenaryrating?.localizedName);
                    RuntimeSetInt("Exobiologist rating", cmdr?.exobiologistrating?.rank);
                    RuntimeSetText("Exobiologist rank", cmdr?.exobiologistrating?.localizedName);
                    RuntimeSetDecimal("Credits", cmdr?.credits);
                    RuntimeSetText("Credits (spoken)", SpeechConversions.Humanize(cmdr?.credits));
                    RuntimeSetDecimal("Debt", cmdr?.debt);
                    RuntimeSetText("Debt (spoken)", SpeechConversions.Humanize(cmdr?.debt));
                    RuntimeSetText("Title", cmdr?.title ?? EddiCore.Properties.Resources.Commander);
                    RuntimeSetText("Gender", cmdr?.gender ?? EddiCore.Properties.Resources.commander_gender_n);
                    RuntimeSetText("Squadron name", cmdr?.squadronname);
                    RuntimeSetText("Squadron id", cmdr?.squadrontag);
                    RuntimeSetInt("Squadron rating", cmdr?.squadronrank?.rankID);
                    RuntimeSetText("Squadron rank", cmdr?.squadronrank?.localizedName);
                    RuntimeSetText("Squadron allegiance", cmdr?.squadronallegiance?.localizedName);
                    RuntimeSetText("Squadron power", cmdr?.squadronpower?.localizedName);
                    RuntimeSetText("Squadron faction", cmdr?.squadronfaction);
                    RuntimeSetText("Power", cmdr?.Power?.localizedName);

                    // Backwards-compatibility with 1.x
                    RuntimeSetText("System rank", cmdr?.title);
                }
                catch (Exception e)
                {
                    setStatus( "Failed to set commander information", e);
                }

                Logging.Debug("Set commander information");
            } );
        }

        private static Ship ResolveCurrentShip ()
        {
            var currentShip = EDDI.Instance.CurrentShip;

            if ( currentShip != null )
            {
                return currentShip;
            }

            var shipConfig = ConfigService.Instance.shipMonitorConfiguration;

            return shipConfig?.shipyard?
                .FirstOrDefault( ship => ship.LocalId == shipConfig.currentshipid );
        }

        public static void setShipValues ( Ship ship, string prefix )
        {
            RunWithRuntimeActionBatch( () =>
            {
                Logging.Debug( $"Setting ship information ({prefix})" );
                try
                {
                    RuntimeSetText( $"{prefix} manufacturer", ship?.manufacturer );
                    RuntimeSetText( $"{prefix} model", ship?.model );
                    RuntimeSetText( $"{prefix} model (spoken)", ship?.SpokenModel() );

                    var cmdrName = ConfigService.Instance.commanderConfiguration.commanderName;
                    if ( cmdrName != null )
                    {
                        var cmdrNamePrefix = cmdrName.Length >= 3
                            ? cmdrName.Substring( 0, 3 ).ToUpperInvariant()
                            : cmdrName.ToUpperInvariant();
                        RuntimeSetText( $"{prefix} callsign", $"{ship?.manufacturer} {cmdrNamePrefix}" );
                        RuntimeSetText( $"{prefix} callsign (spoken)",
                            $"{ship?.SpokenManufacturer()} {SpeechConversions.ICAO( cmdrNamePrefix )}" );
                    }

                    RuntimeSetText( $"{prefix} name", ship?.name );
                    RuntimeSetText( $"{prefix} name (spoken)", ship?.phoneticName );
                    RuntimeSetText( $"{prefix} ident", ship?.ident );
                    RuntimeSetText( $"{prefix} ident (spoken)", SpeechConversions.ICAO( ship?.ident, false ) );
                    RuntimeSetText( $"{prefix} role", ship?.Role?.localizedName );
                    RuntimeSetText( $"{prefix} size", ship?.Size?.localizedName );
                    RuntimeSetDecimal( $"{prefix} value", ship?.value );
                    RuntimeSetText( $"{prefix} value (spoken)", SpeechConversions.Humanize( ship?.value ) );
                    RuntimeSetDecimal( $"{prefix} hull value", ship?.hullvalue );
                    RuntimeSetText( $"{prefix} hull value (spoken)", SpeechConversions.Humanize( ship?.hullvalue ) );
                    RuntimeSetDecimal( $"{prefix} modules value", ship?.modulesvalue );
                    RuntimeSetText( $"{prefix} modules value (spoken)", SpeechConversions.Humanize( ship?.modulesvalue ) );
                    RuntimeSetDecimal( $"{prefix} rebuy", ship?.rebuy );
                    RuntimeSetText( $"{prefix} rebuy (spoken)", SpeechConversions.Humanize( ship?.rebuy ) );
                    RuntimeSetDecimal( $"{prefix} health", ship?.health );
                    RuntimeSetInt( $"{prefix} cargo capacity", ship?.cargocapacity );
                    RuntimeSetBoolean( $"{prefix} hot", ship?.hot );

                    setShipModuleValues( ship?.bulkheads, $"{prefix} bulkheads" );
                    setShipModuleValues( ship?.powerplant, $"{prefix} power plant" );
                    setShipModuleValues( ship?.thrusters, $"{prefix} thrusters" );
                    setShipModuleValues( ship?.frameshiftdrive, $"{prefix} frame shift drive" );
                    setShipModuleValues( ship?.lifesupport, $"{prefix} life support" );
                    setShipModuleValues( ship?.powerdistributor, $"{prefix} power distributor" );
                    setShipModuleValues( ship?.sensors, $"{prefix} sensors" );
                    setShipModuleValues( ship?.fueltank, $"{prefix} fuel tank" );

                    if ( EDDI.Instance.CurrentStation is not null && EDDI.Instance.CurrentStation.outfitting.Count > 0 )
                    {
                        var stationOutfitting = EDDI.Instance.CurrentStation?.outfitting.ToList();
                        setShipModuleOutfittingValues( ship?.bulkheads, stationOutfitting, $"{prefix} bulkheads" );
                        setShipModuleOutfittingValues( ship?.powerplant, stationOutfitting, $"{prefix} power plant" );
                        setShipModuleOutfittingValues( ship?.thrusters, stationOutfitting, $"{prefix} thrusters" );
                        setShipModuleOutfittingValues( ship?.frameshiftdrive, stationOutfitting, $"{prefix} frame shift drive" );
                        setShipModuleOutfittingValues( ship?.lifesupport, stationOutfitting, $"{prefix} life support" );
                        setShipModuleOutfittingValues( ship?.powerdistributor, stationOutfitting, $"{prefix} power distributor" );
                        setShipModuleOutfittingValues( ship?.sensors, stationOutfitting, $"{prefix} sensors" );
                        setShipModuleOutfittingValues( ship?.fueltank, stationOutfitting, $"{prefix} fuel tank" );
                    }

                    // Special for fuel tank - capacity and total capacity
                    RuntimeSetDecimal( $"{prefix} fuel tank capacity", ship?.fueltankcapacity );
                    RuntimeSetDecimal( $"{prefix} total fuel tank capacity", ship?.fueltanktotalcapacity );

                    // Special for max jump range and max fuel per jump
                    RuntimeSetDecimal( $"{prefix} max jump range", ship?.maxjumprange );
                    RuntimeSetDecimal( $"{prefix} max fuel per jump", ship?.maxfuelperjump );

                    // Hardpoints
                    SetShipHardpoints( ship, prefix );

                    // Compartments
                    SetShipCompartments( ship, prefix );

                    // Fetch the star system in which the ship is stored
                    if ( ship?.starsystem != null )
                    {
                        RuntimeSetText( $"{prefix} system", ship.starsystem );
                        RuntimeSetText( $"{prefix} station", ship.station );
                        RuntimeSetDecimal( $"{prefix} distance", ship.distance );
                    }
                }
                catch ( Exception e )
                {
                    setStatus( "Failed to set ship information", e );
                }

                Logging.Debug( "Set ship information" );
            } );
        }

        private static void SetShipCompartments ( Ship ship, string prefix )
        {
            var filledCompartments = ship?.compartments.Count ?? 0;
            // We want to overshoot the maximum number of compartments for any ship in the game
            // and overwrite any previously written values with null values
            for ( var i = 0; i < 16; i++ ) 
            {
                var Compartment = i <= (filledCompartments - 1) ? ship?.compartments[i] : null;
                var baseCompartmentName = $"{prefix} compartment {i}";
                RuntimeSetInt( $"{baseCompartmentName} size", Compartment?.size );
                RuntimeSetBoolean( $"{baseCompartmentName} occupied", Compartment?.module != null );
                setShipModuleValues( Compartment?.module, $"{baseCompartmentName} module" );
                setShipModuleOutfittingValues( Compartment?.module, EDDI.Instance.CurrentStation?.outfitting,
                    $"{baseCompartmentName} module" );
            }
            RuntimeSetInt( $"{prefix} compartments", filledCompartments );
        }

        private static void SetShipHardpoints ( Ship ship, string prefix )
        {
            var invariantSizeNames = new List<string> { "tiny", "small", "medium", "large", "huge" };
            var totalHardpointsCount = 0;
            for ( var i = 0; i < invariantSizeNames.Count; i++ ) // Hardpoint Size
            {
                var hardpointsAtSize = ship?.hardpoints?.Where( h => h.size == i )?.ToList() ?? new List<Hardpoint>();
                for ( var j = 0; j < 12; j++ ) // Hardpoint Slots at Size
                    // We want to overshoot the maximum number of hardpoints for each hardpoint size
                    // and overwrite any previously written values with null values
                {
                    var baseHardpointName = $"{prefix} {invariantSizeNames[i]} hardpoint {j}";
                    var Hardpoint = j <= (hardpointsAtSize.Count - 1) ? hardpointsAtSize[j] : null;
                    RuntimeSetBoolean( $"{baseHardpointName} occupied", Hardpoint?.module != null );
                    setShipModuleValues( Hardpoint?.module, $"{baseHardpointName} module" );
                    setShipModuleOutfittingValues( Hardpoint?.module, EDDI.Instance.CurrentStation?.outfitting,
                        $"{baseHardpointName} module" );
                }
                RuntimeSetInt( $"{prefix} {invariantSizeNames[ i ]} hardpoints", hardpointsAtSize.Count );
                totalHardpointsCount += hardpointsAtSize.Count;
            }
            RuntimeSetInt( $"{prefix} hardpoints", totalHardpointsCount );
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleValues(Module module, string name )
        {
            RuntimeSetText(name, module?.localizedName);
            RuntimeSetInt( $"{name} class", module?.@class);
            RuntimeSetText( $"{name} grade", module?.grade);
            RuntimeSetDecimal( $"{name} health", module?.health);
            RuntimeSetDecimal( $"{name} cost", module?.price);
            RuntimeSetDecimal( $"{name} value", module?.value);
            if (module != null && module.price < module.value)
            {
                var discount = Math.Round((1 - (module.price / (decimal)module.value)) * 100, 1);
                RuntimeSetDecimal( $"{name} discount", discount > 0.01M ? discount : null);
            }
            else
            {
                RuntimeSetDecimal( $"{name} discount", null);
            }
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleOutfittingValues(Module existing, List<Module> outfittingModules, string name)
        {
            if (existing != null && outfittingModules != null)
            {
                foreach (var Module in outfittingModules)
                {
                    if (existing.edname == Module?.edname)
                    {
                        // Found it
                        RuntimeSetDecimal( $"{name} station cost", Module?.price);
                        if (Module?.price < existing.price)
                        {
                            // And it's cheaper
                            RuntimeSetDecimal( $"{name} station discount", existing.price - Module.price);
                            RuntimeSetText( $"{name} station discount (spoken)", SpeechConversions.Humanize(existing.price - Module.price));
                        }
                        return;
                    }
                }
            }
            // Not found so remove any existing
            RuntimeSetDecimal( $"{name} station cost", null);
            RuntimeSetDecimal( $"{name} station discount", null);
            RuntimeSetText( $"{name} station discount (spoken)", null);
        }

        static void setShipyardValues(List<Ship> shipyard)
        {
            RunWithRuntimeActionBatch( () =>
            {
                if (shipyard != null)
                {
                    var currentStoredShip = 1;
                    foreach (var StoredShip in shipyard)
                    {
                        setShipValues(StoredShip, $"Stored ship {currentStoredShip}" );
                        currentStoredShip++;
                    }

                    RuntimeSetInt("Stored ship entries", shipyard.Count);
                }
            } );
        }

        internal static void setStarSystemValues(StarSystem system, string prefix)
        {
            RunWithRuntimeActionBatch( () =>
            {
                Logging.Debug( $"Setting system information ({prefix})" );
                try
                {
                    var phoneticStarSystem = SpeechConversions.getPhoneticStarSystem( system?.systemname );
                    var phoneticPopulation = SpeechConversions.Humanize( system?.population );
                    var phoneticPower = SpeechConversions.getPhoneticPower( system?.power );

                    RuntimeSetText( $"{prefix} name", system?.systemname);
                    RuntimeSetText( $"{prefix} name (spoken)", phoneticStarSystem );
                    RuntimeSetDecimal( $"{prefix} distance from home", system?.distancefromhome);
                    RuntimeSetInt( $"{prefix} visits", system?.visits);
                    RuntimeSetDate( $"{prefix} previous visit", system?.visits > 1 ? system.lastvisit : null);
                    RuntimeSetDecimal( $"{prefix} minutes since previous visit", system != null && system.visits > 1 && system.lastvisit is not null 
                        ? (long)(DateTime.UtcNow - system.lastvisit.Value).TotalMinutes 
                        : null);
                    RuntimeSetDecimal( $"{prefix} population", system?.population);
                    RuntimeSetText( $"{prefix} population (spoken)", phoneticPopulation );
                    RuntimeSetText( $"{prefix} allegiance", (system?.Faction?.Allegiance ?? Superpower.None).localizedName);
                    RuntimeSetText( $"{prefix} government", (system?.Faction?.Government ?? Government.None).localizedName);
                    RuntimeSetText( $"{prefix} faction", system?.Faction?.name);
                    RuntimeSetText( $"{prefix} primary economy", system?.primaryeconomy);
                    RuntimeSetText( $"{prefix} state", (system?.Faction?.presences
                        .FirstOrDefault(p => p.systemAddress == system.systemAddress)?.FactionState ?? FactionState.None).localizedName);
                    RuntimeSetText( $"{prefix} security", system?.security);
                    RuntimeSetText( $"{prefix} power", system?.power);
                    RuntimeSetText( $"{prefix} power (spoken)", phoneticPower );
                    RuntimeSetText( $"{prefix} power state", system?.powerstate);
                    RuntimeSetBoolean( $"{prefix} requires permit", system?.requirespermit);
                    RuntimeSetDecimal( $"{prefix} X", system?.x);
                    RuntimeSetDecimal( $"{prefix} Y", system?.y);
                    RuntimeSetDecimal( $"{prefix} Z", system?.z);
                    RuntimeSetText( $"{prefix} comment", system?.comment);
                    RuntimeSetBoolean( $"{prefix} scoopable", system?.scoopable);
                    RuntimeSetInt( $"{prefix} total bodies", system?.totalbodies);
                    RuntimeSetInt( $"{prefix} scanned bodies", system?.scannedbodies);
                    RuntimeSetInt( $"{prefix} mapped bodies", system?.mappedbodies);

                    if (system != null)
                    {
                        foreach (var Station in system.stations)
                        {
                            RuntimeSetText( $"{prefix} station name", Station.name);
                        }
                        RuntimeSetInt( $"{prefix} stations", system.stations.Count);
                        RuntimeSetInt( $"{prefix} orbital stations", system.stations.Count(s => !s.IsPlanetary()));
                        RuntimeSetInt( $"{prefix} starports", system.stations.Count(s => s.IsStarport()));
                        RuntimeSetInt( $"{prefix} outposts", system.stations.Count(s => s.IsOutpost()));
                        RuntimeSetInt( $"{prefix} planetary stations", system.stations.Count(s => s.IsPlanetary()));
                        RuntimeSetInt( $"{prefix} planetary settlements", system.stations.Count(s => s.IsPlanetarySettlement()));

                        Body primaryBody = null;
                        if (system.bodies is { } list && list.Count > 0 )
                        {
                            primaryBody = system.bodies[0].distance == 0 ? system.bodies[0] : null;
                        }
                        setBodyValues(primaryBody, $"{prefix} main star" );
                    }
                    // Backwards-compatibility with 1.x documented variables
                    RuntimeSetText( prefix, system?.systemname );
                    RuntimeSetText( $"{prefix} (spoken)", phoneticStarSystem );

                    if ( system?.id64 != null )
                    {
                        TrySetFromMetaVariables( $"{prefix} id64",
                            new MetaVariables( system.id64.GetType(), system.id64 ) );
                    }
                }
                catch (Exception e)
                {
                    setStatus( "Failed to set system information", e);
                }
                Logging.Debug( $"Set system information ({prefix})" );
            } );
        }

        private static void setBodyValues(Body body, string prefix)
        {
            Logging.Debug( $"Setting body information ({prefix})" );
            RuntimeSetText( $"{prefix} name", body?.bodyname);
            RuntimeSetText( $"{prefix} stellar class", body?.stellarclass);
            RuntimeSetDecimal( $"{prefix} age", body?.age);
            Logging.Debug( $"Set body information ({prefix})" );
        }

        static void setDetailedBodyValues(Body body, string prefix)
        {
            RunWithRuntimeActionBatch( () =>
            {
                Logging.Debug("Setting current stellar body information");
                RuntimeSetText( $"{prefix} type", (body?.bodyType ?? BodyType.None).localizedName);
                RuntimeSetText( $"{prefix} system name", body?.systemname );
                RuntimeSetText( $"{prefix} name", body?.bodyname);
                RuntimeSetText( $"{prefix} short name", body?.shortname);
                RuntimeSetDecimal( $"{prefix} distance", body?.distance );
                RuntimeSetDecimal( $"{prefix} temperature", body?.temperature );
                RuntimeSetDecimal( $"{prefix} radius", body?.radius );
                TrySetFromMetaVariables( $"{prefix} rings", new MetaVariables( body?.rings?.GetType(), body?.rings ) );

                // Orbital characteristics
                RuntimeSetDecimal( $"{prefix} eccentricity", body?.eccentricity);
                RuntimeSetDecimal( $"{prefix} inclination", body?.inclination);
                RuntimeSetDecimal( $"{prefix} orbital period", body?.orbitalperiod);
                RuntimeSetDecimal( $"{prefix} rotational period", body?.rotationalperiod);
                RuntimeSetDecimal( $"{prefix} semi major axis", body?.semimajoraxis);

                // Star specific items
                if (body?.bodyType?.invariantName == "Star")
                {
                    RuntimeSetBoolean( $"{prefix} main star", body.mainstar);
                    RuntimeSetText( $"{prefix} stellar class", body.stellarclass);
                    RuntimeSetText( $"{prefix} luminosity class", body.luminosityclass);
                    RuntimeSetDecimal( $"{prefix} solar mass", body.solarmass);
                    RuntimeSetDecimal( $"{prefix} solar radius", body.solarradius);
                    RuntimeSetText( $"{prefix} chromaticity", body.chromaticity);
                    RuntimeSetDecimal( $"{prefix} radius probability", body.radiusprobability);
                    RuntimeSetDecimal( $"{prefix} mass probability", body.massprobability);
                    RuntimeSetDecimal( $"{prefix} temp probability", body.tempprobability);
                    RuntimeSetDecimal( $"{prefix} age", body.age );
                    RuntimeSetDecimal( $"{prefix} age probability", body.ageprobability);
                    RuntimeSetDecimal( $"{prefix} estimated inner hab zone", body.estimatedhabzoneinner);
                    RuntimeSetDecimal( $"{prefix} estimated outer hab zone", body.estimatedhabzoneouter);
                    RuntimeSetBoolean( $"{prefix} scoopable", body.scoopable);
                }

                // Body specific items
                if (body?.bodyType?.invariantName == "Planet")
                {
                    RuntimeSetDecimal( $"{prefix} periapsis", body.periapsis);
                    RuntimeSetText( $"{prefix} atmosphere", (body.atmosphereclass ?? AtmosphereClass.None).localizedName);
                    RuntimeSetDecimal( $"{prefix} tilt", body.tilt);
                    RuntimeSetDecimal( $"{prefix} earth mass", body.earthmass);
                    RuntimeSetDecimal( $"{prefix} gravity", body.gravity);
                    RuntimeSetDecimal( $"{prefix} pressure", body.pressure);
                    RuntimeSetText( $"{prefix} terraform state", (body.terraformState ?? TerraformState.NotTerraformable).localizedName);
                    RuntimeSetText( $"{prefix} planet type", (body.planetClass ?? PlanetClass.None).localizedName);
                    RuntimeSetText( $"{prefix} reserves", (body.reserveLevel ?? ReserveLevel.None).localizedName);
                    RuntimeSetBoolean( $"{prefix} landable", body.landable);
                    RuntimeSetBoolean( $"{prefix} tidally locked", body.tidallylocked);
                }

                Logging.Debug( $"Set body information ({prefix})" );
            } );
        }

        private static void setFleetCarrierValues(FleetCarrier fleetCarrier, string prefix)
        {
            RunWithRuntimeActionBatch( () =>
            {
                if (fleetCarrier is null) { return; }
                var variables = new MetaVariables(fleetCarrier.GetType(), fleetCarrier);
                if ( TrySetFromMetaVariables( prefix, variables ) )
                {
                    Logging.Debug( "Set fleet carrier information" );
                }
                else
                {
                    Logging.Error( "Failed to set fleet carrier information" );
                }
            } );
        }

        public static void setStatusValues ( Status status, string prefix )
        {
            RunWithRuntimeActionBatch( () =>
            {
                if ( status == null ) { return; }
                var variables = new MetaVariables(status.GetType(), status);
                if ( TrySetFromMetaVariables( prefix, variables ) )
                {
                    Logging.Debug( "Set real-time status information" );
                }
                else
                {
                    Logging.Warn( "Failed to set real-time status information" );
                }
            } );
        }

        private static bool TrySetFromMetaVariables ( string prefix, MetaVariables variables )
        {
            List<VoiceAttackVariable> va_vars = null;
            try
            {
                va_vars = Convert( variables.Results, prefix );
            }
            catch ( Exception e )
            {
                var exDict = new Dictionary<string, object>
                {
                    { "MetaVariables", variables },
                    { "Exception", e }
                };
                Logging.Error( "Failed to convert MetaVariables to VoiceAttack variables", exDict );
            }

            if ( va_vars != null )
            {
                foreach ( var variable in va_vars )
                {
                    try
                    {
                        if ( variable.variableType == typeof(string) )
                        {
                            RuntimeSetText( variable.key, variable.value as string );
                        }
                        else if ( variable.variableType == typeof(int) )
                        {
                            RuntimeSetInt( variable.key, variable.value as int? );
                        }
                        else if ( variable.variableType == typeof(bool) )
                        {
                            RuntimeSetBoolean( variable.key, variable.value as bool? );
                        }
                        else if ( variable.variableType == typeof(decimal) )
                        {
                            RuntimeSetDecimal( variable.key, variable.value as decimal? );
                        }
                        else if ( variable.variableType == typeof(DateTime) )
                        {
                            RuntimeSetDate( variable.key, variable.value as DateTime? );
                        }
                    }
                    catch ( Exception ex )
                    {
                        var exDict = new Dictionary<string, object>
                        {
                            { "VoiceAttackVariable", variable },
                            { "Exception", ex }
                        };
                        Logging.Error( ex.Message, exDict );
                        return false;
                    }
                }
            }

            return true;
        }

        internal static void setCAPIState(bool caPIactive)
        {
            RuntimeSetBoolean("cAPI active", caPIactive);
        }

        internal static void setSpeechState(PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(SpeechService.Instance.eddiSpeaking).Split('.').Last())
            {
                LockManager.GetLock(nameof(SpeechService.Instance.eddiSpeaking), () =>
                {
                    RuntimeSetBoolean("EDDI speaking", SpeechService.Instance.eddiSpeaking);
                });
            }
        }

        internal static void setStatus( string status, Exception exception = null )
        {
            RunWithRuntimeActionBatch( () =>
            {
                RuntimeSetText("EDDI status", status);
                if (exception is null)
                {
                    RuntimeSetText("EDDI exception", null);
                }
                else
                {
                    Logging.Error(status, exception);
                    RuntimeWriteToLog("EDDI exception (see EDDI's log for details)", "red");
                    RuntimeSetText("EDDI exception", exception.ToString());
                }
            } );
        }

        static void setCargo( CargoMonitorConfiguration cargoConfig )
        {
            RunWithRuntimeActionBatch( () =>
            {
                try
                {
                    RuntimeSetInt("Ship cargo carried", cargoConfig?.cargo.Sum( c => c.total ) ?? 0);
                    RuntimeSetInt("Ship limpets carried", cargoConfig?.cargo.Where(c => c.edname.Equals( "Drones" ) ).Sum(c => c.total) ?? 0);
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to set ship cargo values", ex);
                }
            } );
        }

        internal static void RuntimeSetText( string key, string value )
            => DispatchRuntimeAction( "set_text", key, value );

        private static void RuntimeSetInt( string key, int? value )
            => DispatchRuntimeAction( "set_int", key, value );

        private static void RuntimeSetBoolean( string key, bool? value )
            => DispatchRuntimeAction( "set_boolean", key, value );

        private static void RuntimeSetDecimal( string key, decimal? value )
            => DispatchRuntimeAction( "set_decimal", key, value );

        private static void RuntimeSetDate( string key, DateTime? value )
            => DispatchRuntimeAction( "set_date", key, value?.ToString( "O" ) );

        private static void RuntimeWriteToLog( string message, string color )
        {
            var payload = new Dictionary<string, object>
            {
                { RuntimePayloadKeys.CommandActionPayload.Action, "write_log" },
                { RuntimePayloadKeys.CommandActionPayload.Message, message ?? string.Empty },
                { RuntimePayloadKeys.CommandActionPayload.Color, color ?? "white" }
            };
            var pendingAction = new PendingRuntimeAction(payload);

            var batch = runtimeActionBatch.Value;
            if ( batch != null )
            {
                batch.Actions.Add( pendingAction );
                return;
            }

            DispatchRuntimeEventPayload( payload );
        }

        internal static void WriteRuntimeLog ( string message, string color = "white" )
        {
            RuntimeWriteToLog( message, color );
        }

        private static void DispatchRuntimeAction ( string action, string key, object value )
        {
            var pendingAction = BuildRuntimeVariableAction(action, key, value);

            var batch = runtimeActionBatch.Value;
            if ( batch != null )
            {
                batch.Actions.Add( pendingAction );
                return;
            }

            DispatchRuntimeActions( new[] { pendingAction } );
        }

        private static void DispatchRuntimeActions ( IEnumerable<PendingRuntimeAction> actions, bool force = false )
        {
            var actionList = actions.ToList();

            if ( actionList.Count == 0 )
            {
                return;
            }

            var nonCacheableActions = actionList
                .Where(action => !action.Cacheable)
                .ToList();

            var cacheableActions = actionList
                .Where(action => action.Cacheable)
                .GroupBy(action => action.CacheKey)
                .Select(group => group.Last())
                .Where(action => force || !IsCached(action))
                .ToList();

            var actionsToSend = nonCacheableActions
                .Concat(cacheableActions)
                .ToList();

            if ( actionsToSend.Count == 0 )
            {
                return;
            }

            Dictionary<string, object> payload;

            if ( actionsToSend.Count == 1 )
            {
                payload = actionsToSend[ 0 ].Payload;
            }
            else
            {
                payload = new Dictionary<string, object>
                {
                    {
                        RuntimePayloadKeys.CommandActionPayload.Actions,
                        actionsToSend.Select(action => action.Payload).ToList()
                    }
                };
            }

            var delivered = DispatchRuntimeEventPayload(payload);

            if ( !delivered )
            {
                return;
            }

            foreach ( var action in cacheableActions )
            {
                lastDispatchedValues[ action.CacheKey ] = action.CacheValue;
            }
        }

        /// <summary>Clear the dispatch value cache (for testing purposes)</summary>
        internal static void ClearDispatchCache()
        {
            lastDispatchedValues.Clear();
        }

        /// <summary>Null-safe equality comparison for cached values</summary>
        private static bool ValueEquals( object cached, object current )
        {
            // Both null = equal
            if ( cached == null && current == null ) return true;
            // One null, one not = not equal
            if ( cached == null || current == null ) return false;
            // Both present: use standard equality
            return cached.Equals( current );
        }

        private static bool DispatchRuntimeEventPayload ( Dictionary<string, object> payload )
        {
            try
            {
                // Create a snapshot of the payload to ensure it's not modified during serialization/transmission
                // This prevents "Collection was modified" exceptions that can occur when background threads
                // update the variable dictionary while we're serializing the message
                var payloadSnapshot = new Dictionary<string, object>( payload );

                var eventData = new EventData
                {
                    EventType = "va_runtime",
                    EventName = "command_action",
                    EventPayload = payloadSnapshot
                };

                return RuntimeEventDispatcher.DispatchAsync( eventData )
                    .GetAwaiter()
                    .GetResult();
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Failed to dispatch runtime variable action payload", ex );
                return false;
            }
        }

        private static readonly object NullCacheValue = new();

        private static bool IsCached ( PendingRuntimeAction action )
        {
            return action.Cacheable
                   && lastDispatchedValues.TryGetValue( action.CacheKey, out var priorValue )
                   && ValueEquals( priorValue, action.CacheValue );
        }

        private sealed class PendingRuntimeAction
        {
            public PendingRuntimeAction ( Dictionary<string, object> payload )
            {
                Payload = payload;
                Cacheable = false;
                CacheKey = string.Empty;
                CacheValue = NullCacheValue;
            }

            public PendingRuntimeAction (
                Dictionary<string, object> payload,
                string cacheKey,
                object cacheValue )
            {
                Payload = payload;
                Cacheable = true;
                CacheKey = cacheKey;
                CacheValue = cacheValue ?? NullCacheValue;
            }

            public Dictionary<string, object> Payload { get; }
            public bool Cacheable { get; }
            public string CacheKey { get; }
            public object CacheValue { get; }
        }

        private static PendingRuntimeAction BuildRuntimeVariableAction (
            string action,
            string key,
            object value )
        {
            var normalizedKey = key ?? string.Empty;

            var payload = new Dictionary<string, object>
            {
                { RuntimePayloadKeys.CommandActionPayload.Action, action },
                { RuntimePayloadKeys.CommandActionPayload.Key, normalizedKey },

                // Preserve null as null. Do not convert null to empty string.
                { RuntimePayloadKeys.CommandActionPayload.Value, value ?? JValue.CreateNull() }
            };

            return new PendingRuntimeAction(
                payload,
                $"{action}:{normalizedKey}",
                value );
        }

        private static void RunWithRuntimeActionBatch ( Action action, bool forceDispatch = false )
        {
            ArgumentNullException.ThrowIfNull( action );

            var batch = runtimeActionBatch.Value;
            var ownsBatch = batch == null;

            if ( ownsBatch )
            {
                batch = new RuntimeActionBatchState();
                runtimeActionBatch.Value = batch;
            }

            batch.Depth++;

            try
            {
                action();
            }
            finally
            {
                batch.Depth--;

                if ( ownsBatch )
                {
                    runtimeActionBatch.Value = null;

                    if ( batch.Actions.Count > 0 )
                    {
                        DispatchRuntimeActions( batch.Actions, forceDispatch );
                    }
                }
            }
        }

        internal static void NotifyVoiceAttackRuntimeSessionReady ()
        {
            ClearDispatchCache();
        }

        public static List<VoiceAttackVariable> Convert ( List<MetaVariable> source, string startingPrefix, string eventType = null )
        {
            return source
                .Where( v => v.type != typeof( object ) )
                .Select( v => new VoiceAttackVariable( startingPrefix, eventType, v.keysPath, v.type, v.description, v.value ) )
                .ToList();
        }
    }
}
