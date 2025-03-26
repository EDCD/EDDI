using EddiCompanionAppService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiNavigationService;
using EddiSpeechService;
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
        private static dynamic VaProxy => VoiceAttackPlugin.VaProxy;

        // The following variables notify changes via `PropertyChanged`
        private static readonly Dictionary<string, Action> StandardValues = new Dictionary<string, Action>
        {
            { nameof(EDDI.Instance.CurrentStarSystem), () => setStarSystemValues(EDDI.Instance.CurrentStarSystem, "System") },
            { nameof(EDDI.Instance.LastStarSystem), () => setStarSystemValues(EDDI.Instance.LastStarSystem, "Last system") },
            { nameof(EDDI.Instance.NextStarSystem), () => setStarSystemValues(EDDI.Instance.NextStarSystem, "Next system") },
            { nameof(EDDI.Instance.DestinationStarSystem), () => setStarSystemValues(EDDI.Instance.DestinationStarSystem, "Destination system") },
            { nameof(EDDI.Instance.DestinationDistanceLy), () => VaProxy.SetDecimal("Destination system distance", EDDI.Instance.DestinationDistanceLy) },
            { nameof(EDDI.Instance.HomeStarSystem), () =>
                {
                    setStarSystemValues(EDDI.Instance.HomeStarSystem, "Home system");

                    // Backwards-compatibility with 1.x documented variables
                    try
                    {
                        VaProxy.SetText("Home system", EDDI.Instance.HomeStarSystem?.systemname);
                        VaProxy.SetText("Home system (spoken)", Translations.getPhoneticStarSystem(EDDI.Instance.HomeStarSystem?.systemname));
                        if (EDDI.Instance.HomeStation != null)
                        {
                                VaProxy.SetText("Home station", EDDI.Instance.HomeStation?.name);
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
            { nameof(EDDI.Instance.Environment), () => VaProxy.SetText("Environment", EDDI.Instance.Environment) },
            { nameof(EDDI.Instance.Vehicle), () => VaProxy.SetText("Vehicle", EDDI.Instance.Vehicle) },
            { nameof(EDDI.Instance.inHorizons), () => VaProxy.SetBoolean("horizons", EDDI.Instance.inHorizons) },
            { nameof(EDDI.Instance.inOdyssey), () => VaProxy.SetBoolean("odyssey", EDDI.Instance.inOdyssey) },
        };

        protected internal static void updateStandardValues(PropertyChangedEventArgs eventArgs, dynamic vaProxy)
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
            vaProxy.SetBoolean("cAPI active", CompanionAppService.Instance.active);
            vaProxy.SetBoolean("ipa active", !(SpeechService.Instance.Configuration.DisableIpa));
            vaProxy.SetBoolean("icao active", SpeechService.Instance.Configuration.EnableIcao);
            vaProxy.SetDecimal("Search system distance", NavigationService.Instance.SearchDistanceLy);
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
            VaProxy.SetText("EDDI version", Constants.EDDI_VERSION.ToString());
        }

        protected internal static void updateConfigurationValues ( object sender, PropertyChangedEventArgs e )
        {
            if ( sender is ConfigService configService )
            {
                if ( e.PropertyName.Equals( nameof( CargoMonitorConfiguration ), StringComparison.InvariantCultureIgnoreCase ) )
                {
                    var cargoConfig = configService.cargoMonitorConfiguration;
                    setCargo( cargoConfig, VaProxy );
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
        public static void setDictionaryValues ( IDictionary<string, object> dict, string prefix, dynamic vaProxy )
        {
            foreach ( var key in dict.Keys )
            {
                var varname = "EDDI " + prefix + " " + key;

                vaProxy.SetText( varname, null );
                vaProxy.SetInt( varname, null );
                vaProxy.SetSmallInt( varname, null );
                vaProxy.SetDecimal( varname, null );
                vaProxy.SetBoolean( varname, null );

                var value = dict[ key ];
                if ( value is null ) { continue; }

                var s = value.ToString();
                vaProxy.SetText( varname, s );
                if ( value is decimal d || decimal.TryParse( s, out d ) )
                {
                    vaProxy.SetDecimal( varname, d );
                    vaProxy.SetBoolean( varname, d != 0 );
                    if ( d <= int.MaxValue )
                    {
                        vaProxy.SetInt( varname, Convert.ToInt32( Math.Round( d, MidpointRounding.AwayFromZero ) ) );
                    }

                    if ( d <= short.MaxValue )
                    {
                        vaProxy.SetSmallInt( varname, Convert.ToInt16( Math.Round( d, MidpointRounding.AwayFromZero ) ) );
                    }
                }
                else
                {
                    vaProxy.SetDecimal( varname, null );
                }

                if ( value is int i || int.TryParse( s, out i ) )
                {
                    vaProxy.SetInt( varname, i );
                }

                if ( value is short sh || short.TryParse( s, out sh ) )
                {
                    vaProxy.SetSmallInt( varname, sh );
                }

                if ( value is bool b || bool.TryParse( s, out b ) )
                {
                    vaProxy.SetBoolean( varname, b );
                    vaProxy.SetDecimal( varname, (decimal?)( b ? 1 : 0 ) );
                    vaProxy.SetInt( varname, (int?)( b ? 1 : 0 ) );
                    vaProxy.SetSmallInt( varname, (short?)( b ? 1 : 0 ) );
                }
                else if ( !decimal.TryParse( s, out _ ) )
                {
                    b = !string.IsNullOrEmpty( s );
                    vaProxy.SetBoolean( varname, b );
                    vaProxy.SetDecimal( varname, (decimal?)(b ? 1 : 0) );
                    vaProxy.SetInt( varname, (int?)(b ? 1 : 0) );
                    vaProxy.SetSmallInt( varname, (short?)(b ? 1 : 0) );
                }
            }
        }

        /// <summary>Set values for a station</summary>
        protected static void setStationValues(Station station, string prefix)
        {
            Logging.Debug("Setting station information");

            VaProxy.SetText(prefix + " name", station?.name);
            VaProxy.SetDecimal(prefix + " distance from star", station?.distancefromstar);
            VaProxy.SetText(prefix + " government", (station?.Faction?.Government ?? Government.None).localizedName);
            VaProxy.SetText(prefix + " allegiance", (station?.Faction?.Allegiance ?? Superpower.None).localizedName);
            VaProxy.SetText(prefix + " faction", station?.Faction?.name);
            VaProxy.SetText(prefix + " state", (station?.Faction?.presences
                .FirstOrDefault(p => p.systemAddress == station.systemAddress)?.FactionState ?? FactionState.None).localizedName);
            VaProxy.SetText(prefix + " primary economy", station?.primaryeconomy);
            VaProxy.SetText(prefix + " secondary economy", station?.secondaryeconomy);
            // Services
            VaProxy.SetBoolean(prefix + " has refuel", station?.hasrefuel);
            VaProxy.SetBoolean(prefix + " has repair", station?.hasrepair);
            VaProxy.SetBoolean(prefix + " has rearm", station?.hasrearm);
            VaProxy.SetBoolean(prefix + " has market", station?.hasmarket);
            VaProxy.SetBoolean(prefix + " has black market", station?.hasblackmarket);
            VaProxy.SetBoolean(prefix + " has outfitting", station?.hasoutfitting);
            VaProxy.SetBoolean(prefix + " has shipyard", station?.hasshipyard);

            Logging.Debug("Set station information");
        }

        protected static void setCommanderValues(Commander cmdr)
        {
            try
            {
                VaProxy.SetText("Name", cmdr?.name);
                VaProxy.SetInt("Combat rating", cmdr?.combatrating?.rank);
                VaProxy.SetText("Combat rank", cmdr?.combatrating?.localizedName);
                VaProxy.SetInt("Trade rating", cmdr?.traderating?.rank);
                VaProxy.SetText("Trade rank", cmdr?.traderating?.localizedName);
                VaProxy.SetInt("Explore rating", cmdr?.explorationrating?.rank);
                VaProxy.SetText("Explore rank", cmdr?.explorationrating?.localizedName);
                VaProxy.SetInt("Empire rating", cmdr?.empirerating?.rank);
                VaProxy.SetText("Empire rank", cmdr?.empirerating?.maleRank.localizedName);
                VaProxy.SetInt("Federation rating", cmdr?.federationrating?.rank);
                VaProxy.SetText("Federation rank", cmdr?.federationrating?.localizedName);
                VaProxy.SetInt("Mercenary rating", cmdr?.mercenaryrating?.rank);
                VaProxy.SetText("Mercenary rank", cmdr?.mercenaryrating?.localizedName);
                VaProxy.SetInt("Exobiologist rating", cmdr?.exobiologistrating?.rank);
                VaProxy.SetText("Exobiologist rank", cmdr?.exobiologistrating?.localizedName);
                VaProxy.SetDecimal("Credits", cmdr?.credits);
                VaProxy.SetText("Credits (spoken)", Translations.Humanize(cmdr?.credits));
                VaProxy.SetDecimal("Debt", cmdr?.debt);
                VaProxy.SetText("Debt (spoken)", Translations.Humanize(cmdr?.debt));
                VaProxy.SetText("Title", cmdr?.title ?? EddiCore.Properties.Resources.Commander);
                VaProxy.SetText("Gender", cmdr?.gender ?? EddiCore.Properties.Resources.commander_gender_n);
                VaProxy.SetText("Squadron name", cmdr?.squadronname);
                VaProxy.SetText("Squadron id", cmdr?.squadronid);
                VaProxy.SetInt("Squadron rating", cmdr?.squadronrank?.rank);
                VaProxy.SetText("Squadron rank", cmdr?.squadronrank?.localizedName);
                VaProxy.SetText("Squadron allegiance", cmdr?.squadronallegiance?.localizedName);
                VaProxy.SetText("Squadron power", cmdr?.squadronpower?.localizedName);
                VaProxy.SetText("Squadron faction", cmdr?.squadronfaction);
                VaProxy.SetText("Power", cmdr?.Power?.localizedName);

                // Backwards-compatibility with 1.x
                VaProxy.SetText("System rank", cmdr?.title);

                setStatus(VaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(VaProxy, "Failed to set commander information", e);
            }

            Logging.Debug("Set commander information");
        }

        public static void setShipValues(Ship ship, string prefix)
        {
            Logging.Debug("Setting ship information (" + prefix + ")");
            try
            {
                VaProxy.SetText(prefix + " manufacturer", ship?.manufacturer);
                VaProxy.SetText(prefix + " model", ship?.model);
                VaProxy.SetText(prefix + " model (spoken)", ship?.SpokenModel());

                var cmdrName = ConfigService.Instance.commanderConfiguration.commanderName;
                if ( cmdrName != null )
                {
                    VaProxy.SetText( prefix + " callsign",
                        ship?.manufacturer + " " + cmdrName.Substring( 0, 3 ).ToUpperInvariant() );
                    VaProxy.SetText( prefix + " callsign (spoken)",
                        ship?.SpokenManufacturer() + " " +
                        Translations.ICAO( cmdrName.Substring( 0, 3 ).ToUpperInvariant() ) );
                }

                VaProxy.SetText(prefix + " name", ship?.name);
                VaProxy.SetText(prefix + " name (spoken)", ship?.phoneticName);
                VaProxy.SetText(prefix + " ident", ship?.ident);
                VaProxy.SetText(prefix + " ident (spoken)", Translations.ICAO(ship?.ident, false));
                VaProxy.SetText(prefix + " role", ship?.Role?.localizedName);
                VaProxy.SetText(prefix + " size", ship?.Size?.localizedName);
                VaProxy.SetDecimal(prefix + " value", ship?.value);
                VaProxy.SetText(prefix + " value (spoken)", Translations.Humanize(ship?.value));
                VaProxy.SetDecimal(prefix + " hull value", ship?.hullvalue);
                VaProxy.SetText(prefix + " hull value (spoken)", Translations.Humanize(ship?.hullvalue));
                VaProxy.SetDecimal(prefix + " modules value", ship?.modulesvalue);
                VaProxy.SetText(prefix + " modules value (spoken)", Translations.Humanize(ship?.modulesvalue));
                VaProxy.SetDecimal(prefix + " rebuy", ship?.rebuy);
                VaProxy.SetText(prefix + " rebuy (spoken)", Translations.Humanize(ship?.rebuy));
                VaProxy.SetDecimal(prefix + " health", ship?.health);
                VaProxy.SetInt(prefix + " cargo capacity", ship?.cargocapacity);
                VaProxy.SetBoolean(prefix + " hot", ship?.hot);

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
                VaProxy.SetDecimal(prefix + " fuel tank capacity", ship?.fueltankcapacity);
                VaProxy.SetDecimal(prefix + " total fuel tank capacity", ship?.fueltanktotalcapacity);

                // Special for max jump range and max fuel per jump
                VaProxy.SetDecimal(prefix + " max jump range", ship?.maxjumprange);
                VaProxy.SetDecimal(prefix + " max fuel per jump", ship?.maxfuelperjump);

                // Hardpoints
                SetShipHardpoints( ship, prefix );
                
                // Compartments
                SetShipCompartments( ship, prefix );

                // Fetch the star system in which the ship is stored
                if ( ship?.starsystem != null)
                {
                    VaProxy.SetText(prefix + " system", ship.starsystem);
                    VaProxy.SetText(prefix + " station", ship.station);
                    VaProxy.SetDecimal(prefix + " distance", ship.distance);
                }

                setStatus(VaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(VaProxy, "Failed to set ship information", e);
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
                VaProxy.SetInt( baseCompartmentName + " size", Compartment?.size );
                VaProxy.SetBoolean( baseCompartmentName + " occupied", Compartment?.module != null );
                setShipModuleValues( Compartment?.module, baseCompartmentName + " module" );
                setShipModuleOutfittingValues( Compartment?.module, EDDI.Instance.CurrentStation?.outfitting,
                    baseCompartmentName + " module" );
            }
            VaProxy.SetInt( prefix + " compartments", filledCompartments );
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
                    VaProxy.SetBoolean( baseHardpointName + " occupied", Hardpoint?.module != null );
                    setShipModuleValues( Hardpoint?.module, baseHardpointName + " module" );
                    setShipModuleOutfittingValues( Hardpoint?.module, EDDI.Instance.CurrentStation?.outfitting,
                        baseHardpointName + " module" );
                }
                VaProxy.SetInt( $"{prefix} {invariantSizeNames[ i ]} hardpoints", hardpointsAtSize.Count );
                totalHardpointsCount += hardpointsAtSize.Count;
            }
            VaProxy.SetInt( prefix + " hardpoints", totalHardpointsCount );
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleValues(Module module, string name )
        {
            VaProxy.SetText(name, module?.localizedName);
            VaProxy.SetInt(name + " class", module?.@class);
            VaProxy.SetText(name + " grade", module?.grade);
            VaProxy.SetDecimal(name + " health", module?.health);
            VaProxy.SetDecimal(name + " cost", module?.price);
            VaProxy.SetDecimal(name + " value", module?.value);
            if (module != null && module.price < module.value)
            {
                decimal discount = Math.Round((1 - (module.price / ((decimal)module.value))) * 100, 1);
                VaProxy.SetDecimal(name + " discount", discount > 0.01M ? discount : (decimal?)null);
            }
            else
            {
                VaProxy.SetDecimal(name + " discount", null);
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
                        VaProxy.SetDecimal(name + " station cost", (decimal?)Module?.price);
                        if (Module?.price < existing.price)
                        {
                            // And it's cheaper
                            VaProxy.SetDecimal(name + " station discount", existing.price - Module.price);
                            VaProxy.SetText(name + " station discount (spoken)", Translations.Humanize(existing.price - Module.price));
                        }
                        return;
                    }
                }
            }
            // Not found so remove any existing
            VaProxy.SetDecimal(name + " station cost", (decimal?)null);
            VaProxy.SetDecimal(name + " station discount", (decimal?)null);
            VaProxy.SetText(name + " station discount (spoken)", (string)null);
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

                VaProxy.SetInt("Stored ship entries", shipyard.Count);
            }
        }

        protected internal static void setStarSystemValues(StarSystem system, string prefix)
        {
            Logging.Debug("Setting system information (" + prefix + ")");
            try
            {
                VaProxy.SetText(prefix + " name", system?.systemname);
                VaProxy.SetText(prefix + " name (spoken)", Translations.getPhoneticStarSystem(system?.systemname));
                VaProxy.SetDecimal(prefix + " population", system?.population);
                VaProxy.SetText(prefix + " population (spoken)", Translations.Humanize(system?.population));
                VaProxy.SetText(prefix + " allegiance", (system?.Faction?.Allegiance ?? Superpower.None).localizedName);
                VaProxy.SetText(prefix + " government", (system?.Faction?.Government ?? Government.None).localizedName);
                VaProxy.SetText(prefix + " faction", system?.Faction?.name);
                VaProxy.SetText(prefix + " primary economy", system?.primaryeconomy);
                VaProxy.SetText(prefix + " state", (system?.Faction?.presences
                    .FirstOrDefault(p => p.systemAddress == system.systemAddress)?.FactionState ?? FactionState.None).localizedName);
                VaProxy.SetText(prefix + " security", system?.security);
                VaProxy.SetText(prefix + " power", system?.power);
                VaProxy.SetText(prefix + " power (spoken)", Translations.getPhoneticPower(EDDI.Instance.CurrentStarSystem?.power));
                VaProxy.SetText(prefix + " power state", system?.powerstate);
                VaProxy.SetBoolean(prefix + " requires permit", system?.requirespermit);
                VaProxy.SetDecimal(prefix + " X", system?.x);
                VaProxy.SetDecimal(prefix + " Y", system?.y);
                VaProxy.SetDecimal(prefix + " Z", system?.z);
                VaProxy.SetInt(prefix + " visits", system?.visits);
                VaProxy.SetDate(prefix + " previous visit", system?.visits > 1 ? system.lastvisit : null);
                VaProxy.SetDecimal(prefix + " minutes since previous visit", system?.visits > 1 && system?.lastvisit.HasValue == true ? (long)(DateTime.UtcNow - system.lastvisit.Value).TotalMinutes : (decimal?)null);
                VaProxy.SetText(prefix + " comment", system?.comment);
                VaProxy.SetDecimal(prefix + " distance from home", system?.distancefromhome);
                VaProxy.SetBoolean(prefix + " scoopable", system?.scoopable);
                VaProxy.SetInt(prefix + " total bodies", system?.totalbodies);
                VaProxy.SetInt(prefix + " scanned bodies", system?.scannedbodies);
                VaProxy.SetInt(prefix + " mapped bodies", system?.mappedbodies);

                if (system != null)
                {
                    foreach (Station Station in system.stations)
                    {
                        VaProxy.SetText(prefix + " station name", Station.name);
                    }
                    VaProxy.SetInt(prefix + " stations", system.stations.Count);
                    VaProxy.SetInt(prefix + " orbital stations", system.stations.Count(s => !s.IsPlanetary()));
                    VaProxy.SetInt(prefix + " starports", system.stations.Count(s => s.IsStarport()));
                    VaProxy.SetInt(prefix + " outposts", system.stations.Count(s => s.IsOutpost()));
                    VaProxy.SetInt(prefix + " planetary stations", system.stations.Count(s => s.IsPlanetary()));
                    VaProxy.SetInt(prefix + " planetary settlements", system.stations.Count(s => s.IsPlanetarySettlement()));

                    Body primaryBody = null;
                    if (system.bodies != null && system.bodies.Count > 0)
                    {
                        primaryBody = (system.bodies[0].distance == 0 ? system.bodies[0] : null);
                    }
                    setBodyValues(primaryBody, prefix + " main star", VaProxy);
                }
                setStatus(VaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(VaProxy, "Failed to set system information", e);
            }
            Logging.Debug("Set system information (" + prefix + ")");
        }

        private static void setBodyValues(Body body, string prefix, dynamic vaProxy)
        {
            Logging.Debug("Setting body information (" + prefix + ")");
            vaProxy.SetText(prefix + " name", body?.bodyname);
            vaProxy.SetText(prefix + " stellar class", body?.stellarclass);
            vaProxy.SetDecimal(prefix + " age", body?.age);
            Logging.Debug("Set body information (" + prefix + ")");
        }

        protected static void setDetailedBodyValues(Body body, string prefix)
        {
            Logging.Debug("Setting current stellar body information");
            VaProxy.SetText(prefix + " type", (body?.bodyType ?? BodyType.None).localizedName);
            VaProxy.SetText(prefix + " name", body?.bodyname);
            VaProxy.SetText(prefix + " short name", body?.shortname);
            VaProxy.SetText(prefix + " system name", body?.systemname);
            if (body?.age == null)
            {
                VaProxy.SetDecimal(prefix + " age", null);
            }
            else
            {
                VaProxy.SetDecimal(prefix + " age", (decimal)(long)body.age);
            }
            VaProxy.SetDecimal(prefix + " distance", body?.distance);
            VaProxy.SetDecimal(prefix + " temperature", body?.temperature);
            // Orbital characteristics
            VaProxy.SetDecimal(prefix + " eccentricity", body?.eccentricity);
            VaProxy.SetDecimal(prefix + " inclination", body?.inclination);
            VaProxy.SetDecimal(prefix + " orbital period", body?.orbitalperiod);
            VaProxy.SetDecimal(prefix + " radius", body?.radius);
            VaProxy.SetDecimal(prefix + " rotational period", body?.rotationalperiod);
            VaProxy.SetDecimal(prefix + " semi major axis", body?.semimajoraxis);
            // Star specific items 
            if (body?.bodyType?.invariantName == "Star")
            {
                VaProxy.SetBoolean(prefix + " main star", body?.mainstar);
                VaProxy.SetText(prefix + " stellar class", body?.stellarclass);
                VaProxy.SetText(prefix + " luminosity class", body?.luminosityclass);
                VaProxy.SetDecimal(prefix + " solar mass", body?.solarmass);
                VaProxy.SetDecimal(prefix + " solar radius", body?.solarradius);
                VaProxy.SetText(prefix + " chromaticity", body?.chromaticity);
                VaProxy.SetDecimal(prefix + " radius probability", body?.radiusprobability);
                VaProxy.SetDecimal(prefix + " mass probability", body?.massprobability);
                VaProxy.SetDecimal(prefix + " temp probability", body?.tempprobability);
                VaProxy.SetDecimal(prefix + " age probability", body?.ageprobability);
                VaProxy.SetDecimal(prefix + " estimated inner hab zone", body?.estimatedhabzoneinner);
                VaProxy.SetDecimal(prefix + " estimated outer hab zone", body?.estimatedhabzoneouter);
                VaProxy.SetBoolean(prefix + " scoopable", body?.scoopable);
            }
            // Body specific items 
            if (body?.bodyType?.invariantName == "Planet")
            {
                VaProxy.SetDecimal(prefix + " periapsis", body?.periapsis);
                VaProxy.SetText(prefix + " atmosphere", (body?.atmosphereclass ?? AtmosphereClass.None).localizedName);
                VaProxy.SetDecimal(prefix + " tilt", body?.tilt);
                VaProxy.SetDecimal(prefix + " earth mass", body?.earthmass);
                VaProxy.SetDecimal(prefix + " gravity", body?.gravity);
                VaProxy.SetDecimal(prefix + " pressure", body?.pressure);
                VaProxy.SetText(prefix + " terraform state", (body?.terraformState ?? TerraformState.NotTerraformable).localizedName);
                VaProxy.SetText(prefix + " planet type", (body?.planetClass ?? PlanetClass.None).localizedName);
                VaProxy.SetText(prefix + " reserves", (body?.reserveLevel ?? ReserveLevel.None).localizedName);
                VaProxy.SetBoolean(prefix + " landable", body?.landable);
                VaProxy.SetBoolean(prefix + " tidally locked", body?.tidallylocked);
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
                Logging.Error( "Failed to set real-time status information" );
            }
        }

        private static bool TrySetFromMetaVariables ( string prefix, MetaVariables variables )
        {
            var va_vars = variables.Results.AsVoiceAttackVariables( prefix );
            try
            {
                foreach ( var variable in va_vars )
                {
                    if ( variable.variableType == typeof(string) )
                    {
                        VaProxy.SetText( variable.key, variable.value as string );
                    }
                    else if ( variable.variableType == typeof(int) )
                    {
                        VaProxy.SetInt( variable.key, variable.value as int? );
                    }
                    else if ( variable.variableType == typeof(bool) )
                    {
                        VaProxy.SetBoolean( variable.key, variable.value as bool? );
                    }
                    else if ( variable.variableType == typeof(decimal) )
                    {
                        VaProxy.SetDecimal( variable.key, variable.value as decimal? );
                    }
                    else if ( variable.variableType == typeof(DateTime) )
                    {
                        VaProxy.SetDateTime( variable.key, variable.value as DateTime? );
                    }
                }

                return true;
            }
            catch ( Exception ex )
            {
                Logging.Warn( ex.Message, ex );
                return false;
            }
        }

        protected internal static void setCAPIState(bool caPIactive, dynamic vaProxy)
        {
            vaProxy.SetBoolean("cAPI active", caPIactive);
        }

        protected internal static void setSpeechState(PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(SpeechService.Instance.eddiSpeaking).Split('.').Last())
            {
                LockManager.GetLock(nameof(SpeechService.Instance.eddiSpeaking), () => 
                {
                    VaProxy.SetBoolean("EDDI speaking", SpeechService.Instance.eddiSpeaking);
                });
            }
            if (eventArgs.PropertyName == nameof(SpeechService.Instance.Configuration).Split('.').Last())
            {
                LockManager.GetLock(nameof(SpeechService.Instance.Configuration), () => 
                {
                    VaProxy.SetBoolean("ipa active", !(SpeechService.Instance.Configuration.DisableIpa));
                    VaProxy.SetBoolean("icao active", SpeechService.Instance.Configuration.EnableIcao);
                });
            }
        }

        protected internal static void setStatus(dynamic vaProxy, string status, Exception exception = null)
        {
            vaProxy.SetText("EDDI status", status);
            if (exception is null)
            {
                vaProxy.SetText("EDDI exception", null);
            }
            else
            {
                Logging.Error(status, exception);
                vaProxy.WriteToLog("EDDI exception (see EDDI's log for details)", "red");
                vaProxy.SetText("EDDI exception", exception.ToString());
            }
        }

        protected static void setCargo( CargoMonitorConfiguration cargoConfig, dynamic vaProxy )
        {
            try
            {
                vaProxy.SetInt("Ship cargo carried", cargoConfig?.cargo.Sum( c => c.total ) ?? 0);
                vaProxy.SetInt("Ship limpets carried", cargoConfig?.cargo.Where(c => c.edname.Equals( "Drones" ) ).Sum(c => c.total) ?? 0);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set ship cargo values", ex);
            }
        }
    }
}
