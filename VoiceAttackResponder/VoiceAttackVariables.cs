using EddiCompanionAppService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiNavigationService;
using EddiSpeechService;
using EddiSpeechService.SpeechConversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackResponder
{
    public class VoiceAttackVariables
    {
        // The following variables notify changes via `PropertyChanged`
        private static readonly Dictionary<string, Action> StandardValues = new Dictionary<string, Action>
        {
            { nameof(EDDI.Instance.CurrentStarSystem), () => setStarSystemValues(EDDI.Instance.CurrentStarSystem, "System") },
            { nameof(EDDI.Instance.LastStarSystem), () => setStarSystemValues(EDDI.Instance.LastStarSystem, "Last system") },
            { nameof(EDDI.Instance.NextStarSystem), () => setStarSystemValues(EDDI.Instance.NextStarSystem, "Next system") },
            { nameof(EDDI.Instance.DestinationStarSystem), () => setStarSystemValues(EDDI.Instance.DestinationStarSystem, "Destination system") },
            { nameof(EDDI.Instance.DestinationDistanceLy), () => VoiceAttackPlugin.SetDecimal("Destination system distance", EDDI.Instance.DestinationDistanceLy) },
            { nameof(EDDI.Instance.HomeStarSystem), () =>
                {
                    setStarSystemValues(EDDI.Instance.HomeStarSystem, "Home system");

                    // Backwards-compatibility with 1.x documented variables
                    try
                    {
                        VoiceAttackPlugin.SetText("Home system", EDDI.Instance.HomeStarSystem?.systemname);
                        VoiceAttackPlugin.SetText("Home system (spoken)", SpeechConversions.getPhoneticStarSystem(EDDI.Instance.HomeStarSystem?.systemname));
                        if (EDDI.Instance.HomeStation != null)
                        {
                                VoiceAttackPlugin.SetText("Home station", EDDI.Instance.HomeStation?.name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Error("Failed to set 1.x home system values", ex);
                    }
                } },
            { nameof(EDDI.Instance.CurrentStellarBody), () => setDetailedBodyValues(EDDI.Instance.CurrentStellarBody, "Body") },
            { nameof(EDDI.Instance.CurrentStation), () => setStationValues(EDDI.Instance.CurrentStation, "Current station") },
            { nameof(EDDI.Instance.HomeStation), () => setStationValues(EDDI.Instance.HomeStation, "Home station") },
            { nameof(EDDI.Instance.Cmdr), () => setCommanderValues(EDDI.Instance.Cmdr) },
            { nameof(EDDI.Instance.FleetCarrier), () => setFleetCarrierValues(EDDI.Instance.FleetCarrier, "Carrier") },
            { nameof(EDDI.Instance.CurrentShip), () => setShipValues(EDDI.Instance.CurrentShip, "Ship") },
            { nameof(EDDI.Instance.Environment), () => VoiceAttackPlugin.SetText("Environment", EDDI.Instance.Environment) },
            { nameof(EDDI.Instance.Vehicle), () => VoiceAttackPlugin.SetText("Vehicle", EDDI.Instance.Vehicle) },
            { nameof(EDDI.Instance.inHorizons), () => VoiceAttackPlugin.SetBoolean("horizons", EDDI.Instance.inHorizons) },
            { nameof(EDDI.Instance.inOdyssey), () => VoiceAttackPlugin.SetBoolean("odyssey", EDDI.Instance.inOdyssey) },
        };

        protected internal static void updateStandardValues(PropertyChangedEventArgs eventArgs)
        {
            // Update select values when triggered by a `PropertyChanged` event
            foreach (var standardValue in StandardValues)
            {
                if (eventArgs.PropertyName == standardValue.Key.Split('.').Last())
                {
                    try
                    {
                        LockManager.GetLock(standardValue.Key, standardValue.Value);
                    }
                    catch (Exception ex)
                    {
                        Logging.Error($"Failed to set {standardValue.Key}", ex);
                    }
                }
            }

            // Update values not notified by `PropertyChanged` events
            VoiceAttackPlugin.SetBoolean("cAPI active", CompanionAppService.Instance.active);
            VoiceAttackPlugin.SetBoolean("ipa active", !(SpeechService.Instance.Configuration.DisableIpa));
            VoiceAttackPlugin.SetBoolean("icao active", SpeechService.Instance.Configuration.EnableIcao);
            VoiceAttackPlugin.SetDecimal("Search system distance", NavigationService.Instance.SearchDistanceLy);
            setStarSystemValues(NavigationService.Instance.SearchStarSystem, "Search system" );
            setStationValues(NavigationService.Instance.SearchStation, "Search station" );
        }

        protected internal static void initializeStandardValues()
        {
            foreach (var standardValue in StandardValues)
            {
                try
                {
                    LockManager.GetLock(standardValue.Key, standardValue.Value);
                }
                catch (Exception ex)
                {
                    Logging.Error($"Failed to initialize {standardValue.Key}", ex);
                }
            }
            VoiceAttackPlugin.SetText("EDDI version", Constants.EDDI_VERSION.ToString());
        }

        protected internal static void updateConfigurationValues ( object sender, PropertyChangedEventArgs e )
        {
            if ( sender is ConfigService configService )
            {
                if ( e.PropertyName.Equals( nameof( CargoMonitorConfiguration ), StringComparison.InvariantCultureIgnoreCase ) )
                {
                    var cargoConfig = configService.cargoMonitorConfiguration;
                    setCargo( cargoConfig );
                    return;
                }

                if ( e.PropertyName.Equals( nameof( ShipMonitorConfiguration ), StringComparison.InvariantCultureIgnoreCase ) )
                {
                    var shipConfig = configService.shipMonitorConfiguration;
                    var currentShip = shipConfig.shipyard.FirstOrDefault( s => s.LocalId == shipConfig.currentshipid );
                    setShipValues( currentShip, "Ship" );
                    Task.Run( () =>
                    {
                        setShipyardValues( shipConfig.shipyard?.ToList() );
                    } );
                    return;
                }
            }
        }

        // Set values from a dictionary
        public static void setDictionaryValues ( IDictionary<string, object> dict, string prefix )
        {
            foreach ( var key in dict.Keys )
            {
                var varname = "EDDI " + prefix + " " + key;

                VoiceAttackPlugin.SetText( varname, null );
                VoiceAttackPlugin.SetInt( varname, null );
                VoiceAttackPlugin.SetSmallInt( varname, null );
                VoiceAttackPlugin.SetDecimal( varname, null );
                VoiceAttackPlugin.SetBoolean( varname, null );

                var value = dict[ key ];
                if ( value is null ) { continue; }

                var s = value.ToString();
                VoiceAttackPlugin.SetText( varname, s );
                if ( value is decimal d || decimal.TryParse( s, out d ) )
                {
                    VoiceAttackPlugin.SetDecimal( varname, d );
                    VoiceAttackPlugin.SetBoolean( varname, d != 0 );
                    if ( d <= int.MaxValue )
                    {
                        VoiceAttackPlugin.SetInt( varname, System.Convert.ToInt32( Math.Round( d, MidpointRounding.AwayFromZero ) ) );
                    }

                    if ( d <= short.MaxValue )
                    {
                        VoiceAttackPlugin.SetSmallInt( varname, System.Convert.ToInt16( Math.Round( d, MidpointRounding.AwayFromZero ) ) );
                    }
                }
                else
                {
                    VoiceAttackPlugin.SetDecimal( varname, null );
                }

                if ( value is int i || int.TryParse( s, out i ) )
                {
                    VoiceAttackPlugin.SetInt( varname, i );
                }

                if ( value is short sh || short.TryParse( s, out sh ) )
                {
                    VoiceAttackPlugin.SetSmallInt( varname, sh );
                }

                if ( value is bool b || bool.TryParse( s, out b ) )
                {
                    VoiceAttackPlugin.SetBoolean( varname, b );
                    VoiceAttackPlugin.SetDecimal( varname, (decimal?)( b ? 1 : 0 ) );
                    VoiceAttackPlugin.SetInt( varname, (int?)( b ? 1 : 0 ) );
                    VoiceAttackPlugin.SetSmallInt( varname, (short?)( b ? 1 : 0 ) );
                }
                else if ( !decimal.TryParse( s, out _ ) )
                {
                    b = !string.IsNullOrEmpty( s );
                    VoiceAttackPlugin.SetBoolean( varname, b );
                    VoiceAttackPlugin.SetDecimal( varname, (decimal?)(b ? 1 : 0) );
                    VoiceAttackPlugin.SetInt( varname, (int?)(b ? 1 : 0) );
                    VoiceAttackPlugin.SetSmallInt( varname, (short?)(b ? 1 : 0) );
                }
            }
        }

        /// <summary>Set values for a station</summary>
        protected static void setStationValues(Station station, string prefix)
        {
            Logging.Debug("Setting station information");

            VoiceAttackPlugin.SetText(prefix + " name", station?.name);
            VoiceAttackPlugin.SetDecimal(prefix + " distance from star", station?.distancefromstar);
            VoiceAttackPlugin.SetText(prefix + " government", (station?.Faction?.Government ?? Government.None).localizedName);
            VoiceAttackPlugin.SetText(prefix + " allegiance", (station?.Faction?.Allegiance ?? Superpower.None).localizedName);
            VoiceAttackPlugin.SetText(prefix + " faction", station?.Faction?.name);
            VoiceAttackPlugin.SetText(prefix + " state", (station?.Faction?.presences
                .FirstOrDefault(p => p.systemAddress == station.systemAddress)?.FactionState ?? FactionState.None).localizedName);
            VoiceAttackPlugin.SetText(prefix + " primary economy", station?.primaryeconomy);
            VoiceAttackPlugin.SetText(prefix + " secondary economy", station?.secondaryeconomy);
            // Services
            VoiceAttackPlugin.SetBoolean(prefix + " has refuel", station?.hasrefuel);
            VoiceAttackPlugin.SetBoolean(prefix + " has repair", station?.hasrepair);
            VoiceAttackPlugin.SetBoolean(prefix + " has rearm", station?.hasrearm);
            VoiceAttackPlugin.SetBoolean(prefix + " has market", station?.hasmarket);
            VoiceAttackPlugin.SetBoolean(prefix + " has black market", station?.hasblackmarket);
            VoiceAttackPlugin.SetBoolean(prefix + " has outfitting", station?.hasoutfitting);
            VoiceAttackPlugin.SetBoolean(prefix + " has shipyard", station?.hasshipyard);

            Logging.Debug("Set station information");
        }

        protected static void setCommanderValues(Commander cmdr)
        {
            try
            {
                VoiceAttackPlugin.SetText("Name", cmdr?.name);
                VoiceAttackPlugin.SetInt("Combat rating", cmdr?.combatrating?.rank);
                VoiceAttackPlugin.SetText("Combat rank", cmdr?.combatrating?.localizedName);
                VoiceAttackPlugin.SetInt("Trade rating", cmdr?.traderating?.rank);
                VoiceAttackPlugin.SetText("Trade rank", cmdr?.traderating?.localizedName);
                VoiceAttackPlugin.SetInt("Explore rating", cmdr?.explorationrating?.rank);
                VoiceAttackPlugin.SetText("Explore rank", cmdr?.explorationrating?.localizedName);
                VoiceAttackPlugin.SetInt("Empire rating", cmdr?.empirerating?.rank);
                VoiceAttackPlugin.SetText("Empire rank", cmdr?.empirerating?.maleRank.localizedName);
                VoiceAttackPlugin.SetInt("Federation rating", cmdr?.federationrating?.rank);
                VoiceAttackPlugin.SetText("Federation rank", cmdr?.federationrating?.localizedName);
                VoiceAttackPlugin.SetInt("Mercenary rating", cmdr?.mercenaryrating?.rank);
                VoiceAttackPlugin.SetText("Mercenary rank", cmdr?.mercenaryrating?.localizedName);
                VoiceAttackPlugin.SetInt("Exobiologist rating", cmdr?.exobiologistrating?.rank);
                VoiceAttackPlugin.SetText("Exobiologist rank", cmdr?.exobiologistrating?.localizedName);
                VoiceAttackPlugin.SetDecimal("Credits", cmdr?.credits);
                VoiceAttackPlugin.SetText("Credits (spoken)", SpeechConversions.Humanize(cmdr?.credits));
                VoiceAttackPlugin.SetDecimal("Debt", cmdr?.debt);
                VoiceAttackPlugin.SetText("Debt (spoken)", SpeechConversions.Humanize(cmdr?.debt));
                VoiceAttackPlugin.SetText("Title", cmdr?.title ?? EddiCore.Properties.Resources.Commander);
                VoiceAttackPlugin.SetText("Gender", cmdr?.gender ?? EddiCore.Properties.Resources.commander_gender_n);
                VoiceAttackPlugin.SetText("Squadron name", cmdr?.squadronname);
                VoiceAttackPlugin.SetText("Squadron id", cmdr?.squadronid);
                VoiceAttackPlugin.SetInt("Squadron rating", cmdr?.squadronrank?.rank);
                VoiceAttackPlugin.SetText("Squadron rank", cmdr?.squadronrank?.localizedName);
                VoiceAttackPlugin.SetText("Squadron allegiance", cmdr?.squadronallegiance?.localizedName);
                VoiceAttackPlugin.SetText("Squadron power", cmdr?.squadronpower?.localizedName);
                VoiceAttackPlugin.SetText("Squadron faction", cmdr?.squadronfaction);
                VoiceAttackPlugin.SetText("Power", cmdr?.Power?.localizedName);

                // Backwards-compatibility with 1.x
                VoiceAttackPlugin.SetText("System rank", cmdr?.title);

                setStatus( "Operational");
            }
            catch (Exception e)
            {
                setStatus( "Failed to set commander information", e);
            }

            Logging.Debug("Set commander information");
        }

        public static void setShipValues(Ship ship, string prefix)
        {
            Logging.Debug("Setting ship information (" + prefix + ")");
            try
            {
                VoiceAttackPlugin.SetText(prefix + " manufacturer", ship?.manufacturer);
                VoiceAttackPlugin.SetText(prefix + " model", ship?.model);
                VoiceAttackPlugin.SetText(prefix + " model (spoken)", ship?.SpokenModel());

                var cmdrName = ConfigService.Instance.commanderConfiguration.commanderName;
                if ( cmdrName != null )
                {
                    var cmdrNamePrefix = cmdrName.Length >= 3 ? cmdrName.Substring( 0, 3 ).ToUpperInvariant() : cmdrName.ToUpperInvariant();
                    VoiceAttackPlugin.SetText( prefix + " callsign", ship?.manufacturer + " " + cmdrNamePrefix );
                    VoiceAttackPlugin.SetText( prefix + " callsign (spoken)", ship?.SpokenManufacturer() + " " + SpeechConversions.ICAO( cmdrNamePrefix ) );
                }

                VoiceAttackPlugin.SetText(prefix + " name", ship?.name);
                VoiceAttackPlugin.SetText(prefix + " name (spoken)", ship?.phoneticName);
                VoiceAttackPlugin.SetText(prefix + " ident", ship?.ident);
                VoiceAttackPlugin.SetText(prefix + " ident (spoken)", SpeechConversions.ICAO(ship?.ident, false));
                VoiceAttackPlugin.SetText(prefix + " role", ship?.Role?.localizedName);
                VoiceAttackPlugin.SetText(prefix + " size", ship?.Size?.localizedName);
                VoiceAttackPlugin.SetDecimal(prefix + " value", ship?.value);
                VoiceAttackPlugin.SetText(prefix + " value (spoken)", SpeechConversions.Humanize(ship?.value));
                VoiceAttackPlugin.SetDecimal(prefix + " hull value", ship?.hullvalue);
                VoiceAttackPlugin.SetText(prefix + " hull value (spoken)", SpeechConversions.Humanize(ship?.hullvalue));
                VoiceAttackPlugin.SetDecimal(prefix + " modules value", ship?.modulesvalue);
                VoiceAttackPlugin.SetText(prefix + " modules value (spoken)", SpeechConversions.Humanize(ship?.modulesvalue));
                VoiceAttackPlugin.SetDecimal(prefix + " rebuy", ship?.rebuy);
                VoiceAttackPlugin.SetText(prefix + " rebuy (spoken)", SpeechConversions.Humanize(ship?.rebuy));
                VoiceAttackPlugin.SetDecimal(prefix + " health", ship?.health);
                VoiceAttackPlugin.SetInt(prefix + " cargo capacity", ship?.cargocapacity);
                VoiceAttackPlugin.SetBoolean(prefix + " hot", ship?.hot);

                setShipModuleValues(ship?.bulkheads, prefix + " bulkheads" );
                setShipModuleValues(ship?.powerplant, prefix + " power plant" );
                setShipModuleValues(ship?.thrusters, prefix + " thrusters" );
                setShipModuleValues(ship?.frameshiftdrive, prefix + " frame shift drive" );
                setShipModuleValues(ship?.powerdistributor, prefix + " power distributor" );
                setShipModuleValues(ship?.sensors, prefix + " sensors" );
                setShipModuleValues(ship?.fueltank, prefix + " fuel tank" );

                if (EDDI.Instance.CurrentStation?.outfitting?.Any() ?? false)
                {
                    var stationOutfitting = EDDI.Instance.CurrentStation?.outfitting.ToList();
                    setShipModuleOutfittingValues(ship?.lifesupport, stationOutfitting, prefix + " life support" );
                    setShipModuleOutfittingValues(ship?.bulkheads, stationOutfitting, prefix + " bulkheads" );
                    setShipModuleOutfittingValues(ship?.powerplant, stationOutfitting, prefix + " power plant" );
                    setShipModuleOutfittingValues(ship?.thrusters, stationOutfitting, prefix + " thrusters" );
                    setShipModuleOutfittingValues(ship?.frameshiftdrive, stationOutfitting, prefix + " frame shift drive" );
                    setShipModuleOutfittingValues(ship?.lifesupport, stationOutfitting, prefix + " life support" );
                    setShipModuleOutfittingValues(ship?.powerdistributor, stationOutfitting, prefix + " power distributor" );
                    setShipModuleOutfittingValues(ship?.sensors, stationOutfitting, prefix + " sensors" );
                    setShipModuleOutfittingValues(ship?.fueltank, stationOutfitting, prefix + " fuel tank" );
                }

                // Special for fuel tank - capacity and total capacity
                VoiceAttackPlugin.SetDecimal(prefix + " fuel tank capacity", ship?.fueltankcapacity);
                VoiceAttackPlugin.SetDecimal(prefix + " total fuel tank capacity", ship?.fueltanktotalcapacity);

                // Special for max jump range and max fuel per jump
                VoiceAttackPlugin.SetDecimal(prefix + " max jump range", ship?.maxjumprange);
                VoiceAttackPlugin.SetDecimal(prefix + " max fuel per jump", ship?.maxfuelperjump);

                // Hardpoints
                SetShipHardpoints( ship, prefix );
                
                // Compartments
                SetShipCompartments( ship, prefix );

                // Fetch the star system in which the ship is stored
                if ( ship?.starsystem != null)
                {
                    VoiceAttackPlugin.SetText(prefix + " system", ship.starsystem);
                    VoiceAttackPlugin.SetText(prefix + " station", ship.station);
                    VoiceAttackPlugin.SetDecimal(prefix + " distance", ship.distance);
                }

                setStatus( "Operational");
            }
            catch (Exception e)
            {
                setStatus( "Failed to set ship information", e);
            }

            Logging.Debug("Set ship information");
        }

        private static void SetShipCompartments ( Ship ship, string prefix )
        {
            var filledCompartments = ship?.compartments.Count ?? 0;
            // We want to overshoot the maximum number of compartments for any ship in the game
            // and overwrite any previously written values with null values
            for ( int i = 0; i < 16; i++ ) 
            {
                var Compartment = i <= (filledCompartments - 1) ? ship?.compartments[i] : null;
                string baseCompartmentName = $"{prefix} compartment {i}";
                VoiceAttackPlugin.SetInt( baseCompartmentName + " size", Compartment?.size );
                VoiceAttackPlugin.SetBoolean( baseCompartmentName + " occupied", Compartment?.module != null );
                setShipModuleValues( Compartment?.module, baseCompartmentName + " module" );
                setShipModuleOutfittingValues( Compartment?.module, EDDI.Instance.CurrentStation?.outfitting,
                    baseCompartmentName + " module" );
            }
            VoiceAttackPlugin.SetInt( prefix + " compartments", filledCompartments );
        }

        private static void SetShipHardpoints ( Ship ship, string prefix )
        {
            var invariantSizeNames = new List<string> { "tiny", "small", "medium", "large", "huge" };
            var totalHardpointsCount = 0;
            for ( int i = 0; i < (invariantSizeNames.Count - 1); i++ ) // Hardpoint Size
            {
                var hardpointsAtSize = ship?.hardpoints.Where( h => h.size == i ).ToList() ?? new List<Hardpoint>();
                for ( int j = 0; j < 12; j++ ) // Hardpoint Slots at Size
                    // We want to overshoot the maximum number of hardpoints for each hardpoint size
                    // and overwrite any previously written values with null values
                {
                    var baseHardpointName = $"{prefix} {invariantSizeNames[i]} hardpoint {j}";
                    var Hardpoint = j <= (hardpointsAtSize.Count - 1) ? hardpointsAtSize[j] : null;
                    VoiceAttackPlugin.SetBoolean( baseHardpointName + " occupied", Hardpoint?.module != null );
                    setShipModuleValues( Hardpoint?.module, baseHardpointName + " module" );
                    setShipModuleOutfittingValues( Hardpoint?.module, EDDI.Instance.CurrentStation?.outfitting,
                        baseHardpointName + " module" );
                }
                VoiceAttackPlugin.SetInt( $"{prefix} {invariantSizeNames[ i ]} hardpoints", hardpointsAtSize.Count );
                totalHardpointsCount += hardpointsAtSize.Count;
            }
            VoiceAttackPlugin.SetInt( prefix + " hardpoints", totalHardpointsCount );
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleValues(Module module, string name )
        {
            VoiceAttackPlugin.SetText(name, module?.localizedName);
            VoiceAttackPlugin.SetInt(name + " class", module?.@class);
            VoiceAttackPlugin.SetText(name + " grade", module?.grade);
            VoiceAttackPlugin.SetDecimal(name + " health", module?.health);
            VoiceAttackPlugin.SetDecimal(name + " cost", module?.price);
            VoiceAttackPlugin.SetDecimal(name + " value", module?.value);
            if (module != null && module.price < module.value)
            {
                decimal discount = Math.Round((1 - (module.price / ((decimal)module.value))) * 100, 1);
                VoiceAttackPlugin.SetDecimal(name + " discount", discount > 0.01M ? discount : (decimal?)null);
            }
            else
            {
                VoiceAttackPlugin.SetDecimal(name + " discount", null);
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
                        VoiceAttackPlugin.SetDecimal(name + " station cost", (decimal?)Module?.price);
                        if (Module?.price < existing.price)
                        {
                            // And it's cheaper
                            VoiceAttackPlugin.SetDecimal(name + " station discount", existing.price - Module.price);
                            VoiceAttackPlugin.SetText(name + " station discount (spoken)", SpeechConversions.Humanize(existing.price - Module.price));
                        }
                        return;
                    }
                }
            }
            // Not found so remove any existing
            VoiceAttackPlugin.SetDecimal(name + " station cost", (decimal?)null);
            VoiceAttackPlugin.SetDecimal(name + " station discount", (decimal?)null);
            VoiceAttackPlugin.SetText(name + " station discount (spoken)", (string)null);
        }

        protected static void setShipyardValues(List<Ship> shipyard)
        {
            if (shipyard != null)
            {
                int currentStoredShip = 1;
                foreach (var StoredShip in shipyard)
                {
                    setShipValues(StoredShip, "Stored ship " + currentStoredShip);
                    currentStoredShip++;
                }

                VoiceAttackPlugin.SetInt("Stored ship entries", shipyard.Count);
            }
        }

        protected internal static void setStarSystemValues(StarSystem system, string prefix)
        {
            Logging.Debug("Setting system information (" + prefix + ")");
            try
            {
                VoiceAttackPlugin.SetText(prefix + " name", system?.systemname);
                VoiceAttackPlugin.SetText(prefix + " name (spoken)", SpeechConversions.getPhoneticStarSystem(system?.systemname));
                VoiceAttackPlugin.SetDecimal(prefix + " population", system?.population);
                VoiceAttackPlugin.SetText(prefix + " population (spoken)", SpeechConversions.Humanize(system?.population));
                VoiceAttackPlugin.SetText(prefix + " allegiance", (system?.Faction?.Allegiance ?? Superpower.None).localizedName);
                VoiceAttackPlugin.SetText(prefix + " government", (system?.Faction?.Government ?? Government.None).localizedName);
                VoiceAttackPlugin.SetText(prefix + " faction", system?.Faction?.name);
                VoiceAttackPlugin.SetText(prefix + " primary economy", system?.primaryeconomy);
                VoiceAttackPlugin.SetText(prefix + " state", (system?.Faction?.presences
                    .FirstOrDefault(p => p.systemAddress == system.systemAddress)?.FactionState ?? FactionState.None).localizedName);
                VoiceAttackPlugin.SetText(prefix + " security", system?.security);
                VoiceAttackPlugin.SetText(prefix + " power", system?.power);
                VoiceAttackPlugin.SetText(prefix + " power (spoken)", SpeechConversions.getPhoneticPower(EDDI.Instance.CurrentStarSystem?.power));
                VoiceAttackPlugin.SetText(prefix + " power state", system?.powerstate);
                VoiceAttackPlugin.SetBoolean(prefix + " requires permit", system?.requirespermit);
                VoiceAttackPlugin.SetDecimal(prefix + " X", system?.x);
                VoiceAttackPlugin.SetDecimal(prefix + " Y", system?.y);
                VoiceAttackPlugin.SetDecimal(prefix + " Z", system?.z);
                VoiceAttackPlugin.SetInt(prefix + " visits", system?.visits);
                VoiceAttackPlugin.SetDate(prefix + " previous visit", system?.visits > 1 ? system.lastvisit : null);
                VoiceAttackPlugin.SetDecimal(prefix + " minutes since previous visit", system?.visits > 1 && system?.lastvisit.HasValue == true ? (long)(DateTime.UtcNow - system.lastvisit.Value).TotalMinutes : (decimal?)null);
                VoiceAttackPlugin.SetText(prefix + " comment", system?.comment);
                VoiceAttackPlugin.SetDecimal(prefix + " distance from home", system?.distancefromhome);
                VoiceAttackPlugin.SetBoolean(prefix + " scoopable", system?.scoopable);
                VoiceAttackPlugin.SetInt(prefix + " total bodies", system?.totalbodies);
                VoiceAttackPlugin.SetInt(prefix + " scanned bodies", system?.scannedbodies);
                VoiceAttackPlugin.SetInt(prefix + " mapped bodies", system?.mappedbodies);

                if (system != null)
                {
                    foreach (Station Station in system.stations)
                    {
                        VoiceAttackPlugin.SetText(prefix + " station name", Station.name);
                    }
                    VoiceAttackPlugin.SetInt(prefix + " stations", system.stations.Count);
                    VoiceAttackPlugin.SetInt(prefix + " orbital stations", system.stations.Count(s => !s.IsPlanetary()));
                    VoiceAttackPlugin.SetInt(prefix + " starports", system.stations.Count(s => s.IsStarport()));
                    VoiceAttackPlugin.SetInt(prefix + " outposts", system.stations.Count(s => s.IsOutpost()));
                    VoiceAttackPlugin.SetInt(prefix + " planetary stations", system.stations.Count(s => s.IsPlanetary()));
                    VoiceAttackPlugin.SetInt(prefix + " planetary settlements", system.stations.Count(s => s.IsPlanetarySettlement()));

                    Body primaryBody = null;
                    if (system.bodies != null && system.bodies.Count > 0)
                    {
                        primaryBody = (system.bodies[0].distance == 0 ? system.bodies[0] : null);
                    }
                    setBodyValues(primaryBody, prefix + " main star");
                }
                setStatus( "Operational");
            }
            catch (Exception e)
            {
                setStatus( "Failed to set system information", e);
            }
            Logging.Debug("Set system information (" + prefix + ")");
        }

        private static void setBodyValues(Body body, string prefix)
        {
            Logging.Debug("Setting body information (" + prefix + ")");
            VoiceAttackPlugin.SetText(prefix + " name", body?.bodyname);
            VoiceAttackPlugin.SetText(prefix + " stellar class", body?.stellarclass);
            VoiceAttackPlugin.SetDecimal(prefix + " age", body?.age);
            Logging.Debug("Set body information (" + prefix + ")");
        }

        protected static void setDetailedBodyValues(Body body, string prefix)
        {
            Logging.Debug("Setting current stellar body information");
            VoiceAttackPlugin.SetText(prefix + " type", (body?.bodyType ?? BodyType.None).localizedName);
            VoiceAttackPlugin.SetText(prefix + " name", body?.bodyname);
            VoiceAttackPlugin.SetText(prefix + " short name", body?.shortname);
            VoiceAttackPlugin.SetText(prefix + " system name", body?.systemname);
            if (body?.age == null)
            {
                VoiceAttackPlugin.SetDecimal(prefix + " age", null);
            }
            else
            {
                VoiceAttackPlugin.SetDecimal(prefix + " age", (decimal)(long)body.age);
            }
            VoiceAttackPlugin.SetDecimal(prefix + " distance", body?.distance);
            VoiceAttackPlugin.SetDecimal(prefix + " temperature", body?.temperature);
            // Orbital characteristics
            VoiceAttackPlugin.SetDecimal(prefix + " eccentricity", body?.eccentricity);
            VoiceAttackPlugin.SetDecimal(prefix + " inclination", body?.inclination);
            VoiceAttackPlugin.SetDecimal(prefix + " orbital period", body?.orbitalperiod);
            VoiceAttackPlugin.SetDecimal(prefix + " radius", body?.radius);
            VoiceAttackPlugin.SetDecimal(prefix + " rotational period", body?.rotationalperiod);
            VoiceAttackPlugin.SetDecimal(prefix + " semi major axis", body?.semimajoraxis);
            // Star specific items 
            if (body?.bodyType?.invariantName == "Star")
            {
                VoiceAttackPlugin.SetBoolean(prefix + " main star", body?.mainstar);
                VoiceAttackPlugin.SetText(prefix + " stellar class", body?.stellarclass);
                VoiceAttackPlugin.SetText(prefix + " luminosity class", body?.luminosityclass);
                VoiceAttackPlugin.SetDecimal(prefix + " solar mass", body?.solarmass);
                VoiceAttackPlugin.SetDecimal(prefix + " solar radius", body?.solarradius);
                VoiceAttackPlugin.SetText(prefix + " chromaticity", body?.chromaticity);
                VoiceAttackPlugin.SetDecimal(prefix + " radius probability", body?.radiusprobability);
                VoiceAttackPlugin.SetDecimal(prefix + " mass probability", body?.massprobability);
                VoiceAttackPlugin.SetDecimal(prefix + " temp probability", body?.tempprobability);
                VoiceAttackPlugin.SetDecimal(prefix + " age probability", body?.ageprobability);
                VoiceAttackPlugin.SetDecimal(prefix + " estimated inner hab zone", body?.estimatedhabzoneinner);
                VoiceAttackPlugin.SetDecimal(prefix + " estimated outer hab zone", body?.estimatedhabzoneouter);
                VoiceAttackPlugin.SetBoolean(prefix + " scoopable", body?.scoopable);
            }
            // Body specific items 
            if (body?.bodyType?.invariantName == "Planet")
            {
                VoiceAttackPlugin.SetDecimal(prefix + " periapsis", body?.periapsis);
                VoiceAttackPlugin.SetText(prefix + " atmosphere", (body?.atmosphereclass ?? AtmosphereClass.None).localizedName);
                VoiceAttackPlugin.SetDecimal(prefix + " tilt", body?.tilt);
                VoiceAttackPlugin.SetDecimal(prefix + " earth mass", body?.earthmass);
                VoiceAttackPlugin.SetDecimal(prefix + " gravity", body?.gravity);
                VoiceAttackPlugin.SetDecimal(prefix + " pressure", body?.pressure);
                VoiceAttackPlugin.SetText(prefix + " terraform state", (body?.terraformState ?? TerraformState.NotTerraformable).localizedName);
                VoiceAttackPlugin.SetText(prefix + " planet type", (body?.planetClass ?? PlanetClass.None).localizedName);
                VoiceAttackPlugin.SetText(prefix + " reserves", (body?.reserveLevel ?? ReserveLevel.None).localizedName);
                VoiceAttackPlugin.SetBoolean(prefix + " landable", body?.landable);
                VoiceAttackPlugin.SetBoolean(prefix + " tidally locked", body?.tidallylocked);
            }

            Logging.Debug("Set body information (" + prefix + ")");
        }

        private static void setFleetCarrierValues(FleetCarrier fleetCarrier, string prefix)
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
        }

        public static void setStatusValues ( Status status, string prefix )
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
                            VoiceAttackPlugin.SetText( variable.key, variable.value as string );
                        }
                        else if ( variable.variableType == typeof(int) )
                        {
                            VoiceAttackPlugin.SetInt( variable.key, variable.value as int? );
                        }
                        else if ( variable.variableType == typeof(bool) )
                        {
                            VoiceAttackPlugin.SetBoolean( variable.key, variable.value as bool? );
                        }
                        else if ( variable.variableType == typeof(decimal) )
                        {
                            VoiceAttackPlugin.SetDecimal( variable.key, variable.value as decimal? );
                        }
                        else if ( variable.variableType == typeof(DateTime) )
                        {
                            VoiceAttackPlugin.SetDate( variable.key, variable.value as DateTime? );
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

        protected internal static void setCAPIState(bool caPIactive)
        {
            VoiceAttackPlugin.SetBoolean("cAPI active", caPIactive);
        }

        protected internal static void setSpeechState(PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(SpeechService.Instance.eddiSpeaking).Split('.').Last())
            {
                LockManager.GetLock(nameof(SpeechService.Instance.eddiSpeaking), () => 
                {
                    VoiceAttackPlugin.SetBoolean("EDDI speaking", SpeechService.Instance.eddiSpeaking);
                });
            }
            if (eventArgs.PropertyName == nameof(SpeechService.Instance.Configuration).Split('.').Last())
            {
                LockManager.GetLock(nameof(SpeechService.Instance.Configuration), () => 
                {
                    VoiceAttackPlugin.SetBoolean("ipa active", !(SpeechService.Instance.Configuration.DisableIpa));
                    VoiceAttackPlugin.SetBoolean("icao active", SpeechService.Instance.Configuration.EnableIcao);
                });
            }
        }

        protected internal static void setStatus( string status, Exception exception = null)
        {
            VoiceAttackPlugin.SetText("EDDI status", status);
            if (exception is null)
            {
                VoiceAttackPlugin.SetText("EDDI exception", null);
            }
            else
            {
                Logging.Error(status, exception);
                VoiceAttackPlugin.WriteToLog("EDDI exception (see EDDI's log for details)", "red");
                VoiceAttackPlugin.SetText("EDDI exception", exception.ToString());
            }
        }

        protected static void setCargo( CargoMonitorConfiguration cargoConfig )
        {
            try
            {
                VoiceAttackPlugin.SetInt("Ship cargo carried", cargoConfig?.cargo.Sum( c => c.total ) ?? 0);
                VoiceAttackPlugin.SetInt("Ship limpets carried", cargoConfig?.cargo.Where(c => c.edname.Equals( "Drones" ) ).Sum(c => c.total) ?? 0);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set ship cargo values", ex);
            }
        }

        public static List<VoiceAttackVariable> Convert ( List<MetaVariable> source, string startingPrefix, string eventType = null )
        {
            return source
                .Where( v => ( v.type != typeof( object ) ) )
                .Select( v => new VoiceAttackVariable( startingPrefix, eventType, v.keysPath, v.type, v.description, v.value ) )
                .ToList();
        }
    }
}
