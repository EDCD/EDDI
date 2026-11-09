using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiJournalMonitor
{
    [UsedImplicitly]
    public class JournalMonitor () : LogMonitor( Files.GetEliteSavedGamesDir(), @"^Journal.*\.[0-9\.]+\.log$",
        ( result, isLogLoadEvent ) =>
            ForwardJournalEntries( result.ToList(), EDDI.Instance.enqueueEvent, isLogLoadEvent ) ), IEddiMonitor, IJournalEntryParser
    {
        private static void ForwardJournalEntries ( IList<string> lines, Action<Event> callback, bool isLogLoadEventBatch )
        {
            if ( !lines.Any() ) { return; }

            var events = ParseJournalEntries(lines, isLogLoadEventBatch);

            // Enqueue events for processing
            events.ForEach(callback);
        }

        public static List<Event> ParseJournalEntries(IList<string> lines, bool fromLogLoad = false)
        {
            var events = lines
                .Where( line => !string.IsNullOrEmpty( line ) )
                .SelectMany( line => ParseJournalEntry( line, fromLogLoad ) )
                .ToList();

            if ( fromLogLoad ) { return events; }

            // Reorder DiscoveryScanEvent to occur after other events in the batch including body and star scans
            if ( events.Any( e => e is DiscoveryScanEvent ) )
            {
                var discoveryScanEvents = events.OfType<DiscoveryScanEvent>().ToList();
                events = events.Except( discoveryScanEvents ).Concat( discoveryScanEvents ).ToList();
            }

            // Reorder FSSSignalDiscovered to occur after other events in the batch including FSDJump events
            if ( events.Any( e => e is JumpedEvent ) )
            {
                var SignalDetectedEvents = events.OfType<SignalDetectedEvent>().ToList();
                events = events.Except( SignalDetectedEvents ).Concat( SignalDetectedEvents ).ToList();
            }

            // Reorder SystemScanComplete to occur after other events in the batch including body and star scans
            if ( events.Any( e => e is SystemScanComplete ) )
            {
                var SystemScanCompleteEvents = events.OfType<SystemScanComplete>().ToList();
                events = events.Except( SystemScanCompleteEvents ).Concat( SystemScanCompleteEvents ).ToList();
            }

            // We will ignore `USSDrop` journal events since they are redundant with `SupercruiseDestinationDrop` but we shall 
            // supplement our response to `SupercruiseDestinationDrop` with a signal source identifier.
            if ( lines.Any( l => l.Contains( "USSDrop" ) ) )
            {
                foreach ( var destinationArrivedEvent in events.OfType<DestinationArrivedEvent>() )
                {
                    destinationArrivedEvent.isSignalSource = true;
                }
            }

            // Handle any other sequential event patterns we wish to handle
            for ( var i = 0; i < events.Count; i++ )
            {
                if ( events[ i ] is ShipShutdownEvent shipShutdownEvent )
                {
                    if ( ( i + 1 ) <= ( events.Count - 1 ) && events[ i + 1 ] is MaterialCollectedEvent @event && @event.edname == "tg_shutdowndata" )
                    {
                        // If a ShipShutdown event is followed by a material collection event for Massive Energy Surge Analytics
                        // (available from Thargoid Titan energy pulses), ship systems are only momentarily impacted /
                        // flickering for a few seconds. Simulate a partial ship system shutdown.
                        shipShutdownEvent.partialshutdown = true;
                    }
                }
            }

            return events;
        }

        public static List<Event> ParseJournalEntry(string line, bool fromLogLoad = false )
        {
            var events = new List<Event>();
            try
            {
                var match = GeneratedRegex.JsonWrappedRegex().Match(line);
                if (match.Success)
                {
                    Logging.Debug($"Received event: {line}");
                    var data = Deserializtion.DeserializeData(line);

                    // Every event has a timestamp field
                    var timestamp = DateTime.UtcNow;
                    try
                    {
                        timestamp = JsonParsing.getDateTime("timestamp", data);
                    }
                    catch
                    {
                        Logging.Warn("Event without timestamp; using current time");
                    }

                    // Every event has an event field
                    if (!data.ContainsKey("event"))
                    {
                        Logging.Warn("Event without event field!", line);
                        return events;
                    }

                    // Get the `edType`
                    var edType = JsonParsing.getString(data, "event");
                    
                    var handled = false;
                    try
                    {
                        switch (edType)
                        {
                            #region Startup Events
                              
                            case "Cargo":
                                handled = CargoEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ClearSavedGame":
                                handled = ClearedSaveEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;       
                            case "Commander":
                                handled = CommanderLoadingEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Fileheader":
                                handled = FileHeaderEvent.Handle( timestamp, journalFileName, line, data, ref events, fromLogLoad );
                                break;
                            case "Loadout":
                                handled = ShipLoadoutEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;      
                            case "Materials":
                                handled = MaterialInventoryEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Missions":
                                handled = MissionsEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "NewCommander":
                                handled = CommanderStartedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "LoadGame":
                                handled = CommanderContinuedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Passengers":
                                handled = PassengersEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Powerplay":
                                handled = PowerplayEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Progress":
                                handled = CommanderProgressEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Rank":
                                handled = CommanderRatingsEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Reputation":
                                handled = CommanderReputationEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SquadronStartup":
                                handled = SquadronStartupEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Statistics":
                                handled = StatisticsEvent.Handle( timestamp, line, ref events, fromLogLoad );
                                break;

                            #endregion

                            #region Travel Events

                            case "ApproachBody":
                            case "LeaveBody":
                                handled = NearSurfaceEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "Docked":
                                handled = DockedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "DockingCancelled":
                                handled = DockingCancelledEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "DockingDenied":
                                handled = DockingDeniedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "DockingGranted":
                                handled = DockingGrantedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "DockingRequested":
                                handled = DockingRequestedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "DockingTimeout":
                                handled = DockingTimedOutEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;                                
                            case "FSDJump":
                                handled = JumpedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "FSDTarget":
                                handled = FSDTargetEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Liftoff":
                                handled = LiftoffEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Location":
                                handled = LocationEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "NavRoute":
                            case "NavRouteClear":
                                handled = NavRouteEvent.Handle( timestamp, edType, ref events, fromLogLoad );
                                break;
                            case "StartJump":
                                handled = FSDEngagedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SupercruiseEntry":
                                handled = EnteredSupercruiseEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SupercruiseExit":
                                handled = EnteredNormalSpaceEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Touchdown":
                                handled = TouchdownEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Undocked":
                                handled = UndockedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;

                            #endregion

                            #region Combat (and Combat Reward) Events

                            case "Bounty":
                                handled = BountyAwardedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CapShipBond":
                            case "FactionKillBond":
                                handled = BondAwardedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Died":
                                handled = DiedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "EscapeInterdiction":
                            case "Interdicted":
                                handled = ShipInterdictedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "FighterDestroyed":
                                handled = VesselDestroyedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "HeatDamage":
                                handled = HeatDamageEvent.Handle( timestamp, line, ref events, fromLogLoad );
                                break;
                            case "HeatWarning":
                                handled = HeatWarningEvent.Handle( timestamp, line, ref events, fromLogLoad );
                                break;
                            case "HullDamage":
                                handled = HullDamagedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Interdiction":
                                handled = ShipInterdictionEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "PVPKill":
                                handled = KilledEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ShieldState":
                                // We generate this event via the Status Monitor.
                                handled = true;
                                break;
                            case "ShipTargeted":
                                handled = ShipTargetedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SRVDestroyed":
                                handled = VesselDestroyedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "UnderAttack":
                                handled = UnderAttackEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            
                            #endregion

                            #region Exploration

                            case "BuyExplorationData":
                                handled = ExplorationDataPurchasedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CodexEntry":
                                handled = CodexEntryEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "FSSAllBodiesFound":
                                handled = SystemScanComplete.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "FSSBodySignals":
                            case "SAASignalsFound":
                                handled = SurfaceSignalsEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "FSSDiscoveryScan":
                                handled = DiscoveryScanEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "FSSSignalDiscovered":
                                handled = SignalDetectedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "MaterialCollected":
                                handled = MaterialCollectedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "MaterialDiscarded":
                                handled = MaterialDiscardedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "MaterialDiscovered":
                                handled = MaterialDiscoveredEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "MultiSellExplorationData":
                                handled = ExplorationDataSoldEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "NavBeaconScan":
                                handled = NavBeaconScanEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SAAScanComplete": // Body mapped
                                handled = BodyMappedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Scan":
                                {
                                    var name = JsonParsing.getString(data, "BodyName");
                                    var scantype = JsonParsing.getString(data, "ScanType");

                                    var systemName = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");

                                    // Belt
                                    if (name.Contains("Belt Cluster"))
                                    {
                                        // We don't do anything with belt cluster scans at this time.
                                        handled = true;
                                        break;
                                    }

                                    if (name.Contains(" Ring"))
                                    {
                                        // We don't do anything with ring scans at this time.
                                        handled = true;
                                        break;
                                    }

                                    // Common items
                                    var distanceLs = JsonParsing.getDecimal(data, "DistanceFromArrivalLS");
                                    // Need to convert radius from meters (per journal) to kilometers
                                    var radiusKm = JsonParsing.getDecimal(data, "Radius") / 1000;
                                    // Need to convert orbital period from seconds (per journal) to days
                                    var orbitalPeriodDays = ConstantConverters.seconds2days(JsonParsing.getOptionalDecimal(data, "OrbitalPeriod"));
                                    // Need to convert rotation period from seconds (per journal) to days
                                    var rotationPeriodDays = ConstantConverters.seconds2days(JsonParsing.getOptionalDecimal(data, "RotationPeriod"));
                                    // Need to convert meters to light seconds
                                    var semimajoraxisLs = ConstantConverters.meters2ls(JsonParsing.getOptionalDecimal(data, "SemiMajorAxis"));
                                    var eccentricity = JsonParsing.getOptionalDecimal(data, "Eccentricity");
                                    var orbitalinclinationDegrees = JsonParsing.getOptionalDecimal(data, "OrbitalInclination");
                                    var periapsisDegrees = JsonParsing.getOptionalDecimal(data, "Periapsis");
                                    var axialTiltDegrees = JsonParsing.getOptionalDecimal(data, "AxialTilt");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var temperatureKelvin = JsonParsing.getOptionalDecimal(data, "SurfaceTemperature");

                                    // Parent body types and IDs
                                    var parents = new List<IDictionary<string, int>>();
                                    if ( data.TryGetValue( "Parents", out var parentsVal ) && parentsVal is IEnumerable<object> pVal )
                                    {
                                        foreach ( var parentObj in pVal.Cast<IDictionary<string, object>>() )
                                        {
                                            try
                                            {
                                                var intDict = parentObj.ToDictionary(kv => kv.Key, kv => Convert.ToInt32(kv.Value));
                                                parents.Add( intDict );
                                            }
                                            catch
                                            {
                                                // handle non-numeric values or ignore
                                            }
                                        }
                                    }

                                    // Scan status
                                    var alreadydiscovered = scantype == "NavBeaconDetail" ? true : JsonParsing.getOptionalBool(data, "WasDiscovered");
                                    var alreadymapped = JsonParsing.getOptionalBool(data, "WasMapped");
                                    var alreadyfirstfootfalled = JsonParsing.getOptionalBool(data, "WasFootfalled");

                                    // Rings
                                    data.TryGetValue("Rings", out var val);
                                    var ringsData = (List<object>)val;
                                    var rings = new List<Ring>();
                                    if (ringsData != null)
                                    {
                                        foreach (var ringData in ringsData.Cast<IDictionary<string, object>>() )
                                        {
                                            var ringName = JsonParsing.getString(ringData, "Name");
                                            var ringComposition = RingComposition.FromEDName(JsonParsing.getString(ringData, "RingClass"));
                                            var ringMassMegaTons = JsonParsing.getDecimal(ringData, "MassMT");
                                            var ringInnerRadiusKm = JsonParsing.getDecimal(ringData, "InnerRad") / 1000;
                                            var ringOuterRadiusKm = JsonParsing.getDecimal(ringData, "OuterRad") / 1000;

                                            rings.Add(new Ring(ringName, ringComposition, ringMassMegaTons, ringInnerRadiusKm, ringOuterRadiusKm));
                                        }
                                    }

                                    if (data.ContainsKey("StarType"))
                                    {
                                        // Star
                                        var stellarclass = JsonParsing.getString(data, "StarType");
                                        var stellarsubclass = JsonParsing.getOptionalInt(data, "Subclass");
                                        var stellarMass = JsonParsing.getDecimal(data, "StellarMass");
                                        var absoluteMagnitude = JsonParsing.getDecimal(data, "AbsoluteMagnitude");
                                        var luminosityClass = JsonParsing.getString(data, "Luminosity");
                                        data.TryGetValue("Age_MY", out val);
                                        var ageMegaYears = (long)val;

                                        var star = new Body(name, bodyId, systemName, systemAddress, parents, distanceLs, stellarclass, stellarsubclass, stellarMass, radiusKm, absoluteMagnitude, ageMegaYears, temperatureKelvin, luminosityClass, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, alreadydiscovered, alreadymapped)
                                        {
                                            scannedDateTime = timestamp
                                        };

                                        events.Add(new StarScannedEvent(timestamp, scantype, star) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (data.ContainsKey("PlanetClass"))
                                    {
                                        // Body
                                       var tidallyLocked = JsonParsing.getOptionalBool(data, "TidalLock") ?? false;

                                        var planetClass = PlanetClass.FromEDName(JsonParsing.getString(data, "PlanetClass")) ?? PlanetClass.None;
                                        var earthMass = JsonParsing.getOptionalDecimal(data, "MassEM");

                                        // MKW: Gravity in the Journal is in m/s; must convert it to G
                                        var gravity = ConstantConverters.ms2g(JsonParsing.getDecimal(data, "SurfaceGravity"));

                                        var pressureAtm = ConstantConverters.pascals2atm(JsonParsing.getOptionalDecimal(data, "SurfacePressure"));

                                        var landable = JsonParsing.getOptionalBool(data, "Landable") ?? false;

                                        var reserveLevel = ReserveLevel.FromEDName(JsonParsing.getString(data, "ReserveLevel"));

                                        // The "Atmosphere" is most accurately described through the "AtmosphereType" and "AtmosphereComposition" 
                                        // properties, so we use them in preference to "Atmosphere"

                                        // Gas giants may receive an empty string in place of an atmosphere class string. Fix it, since gas giants definitely have atmospheres. 
                                        var atmosphereClass = planetClass.invariantName.Contains("gas giant") && JsonParsing.getString(data, "AtmosphereType") == string.Empty
                                            ? AtmosphereClass.FromEDName("GasGiant")
                                            : AtmosphereClass.FromEDName(JsonParsing.getString(data, "AtmosphereType")) ?? AtmosphereClass.None;

                                        data.TryGetValue("AtmosphereComposition", out val);
                                        var atmosphereCompositions = new List<AtmosphereComposition>();
                                        if (val is List<object> atmosJson )
                                        {
                                            foreach (var atmoJson in atmosJson.Cast<IDictionary<string, object>>() )
                                            {
                                                var edComposition = JsonParsing.getString(atmoJson, "Name");
                                                var percent = JsonParsing.getOptionalDecimal(atmoJson, "Percent");
                                                if (edComposition != null && percent != null)
                                                {
                                                    atmosphereCompositions.Add(new AtmosphereComposition(edComposition, (decimal)percent));
                                                }
                                            }
                                            if (atmosphereCompositions.Count > 0)
                                            {
                                                atmosphereCompositions = atmosphereCompositions.OrderByDescending(x => x.percent).ToList();
                                            }
                                        }

                                        data.TryGetValue("Composition", out val);
                                        var solidCompositions = new List<SolidComposition>();
                                        if (val is Dictionary<string, object> bodyCompsJson )
                                        {
                                            foreach (var kv in bodyCompsJson )
                                            {
                                                var edComposition = kv.Key;
                                                // The journal gives solid composition as a fraction of 1. Multiply by 100 to convert to a true percentage.
                                                var percent = (decimal)(double)kv.Value * 100;
                                                if (edComposition != null)
                                                {
                                                    solidCompositions.Add(new SolidComposition(edComposition, percent));
                                                }
                                            }
                                            if (solidCompositions.Count > 0)
                                            {
                                                solidCompositions = solidCompositions.OrderByDescending(x => x.percent).ToList();
                                            }
                                        }

                                        data.TryGetValue("Materials", out val);
                                       var materials = new List<MaterialPresence>();
                                        if (val != null)
                                        {
                                            if (val is Dictionary<string, object>)
                                            {
                                                // 2.2 style
                                                var materialsData = (IDictionary<string, object>)val;
                                                foreach (var kv in materialsData)
                                                {
                                                    var material = Material.FromEDName(kv.Key);
                                                    if (material != null)
                                                    {
                                                        materials.Add(new MaterialPresence(material, JsonParsing.getDecimal("Amount", kv.Value)));
                                                    }
                                                }
                                            }
                                            else if (val is List<object> materialsJson) // 2.3 style
                                            {
                                                foreach (var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                                                {
                                                    var material = Material.FromEDName((string)materialJson["Name"]);
                                                    materials.Add(new MaterialPresence(material, JsonParsing.getDecimal(materialJson, "Percent")));
                                                }
                                            }
                                        }

                                        var terraformState = TerraformState.FromEDName(JsonParsing.getString(data, "TerraformState")) ?? TerraformState.NotTerraformable;
                                        var volcanism = Volcanism.FromName(JsonParsing.getString(data, "Volcanism"));

                                        var body = new Body(name, bodyId, systemName, systemAddress, parents, distanceLs, tidallyLocked, terraformState, planetClass, atmosphereClass, atmosphereCompositions, volcanism, earthMass, radiusKm, gravity, temperatureKelvin, pressureAtm, landable, materials, solidCompositions, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, reserveLevel, alreadydiscovered, alreadymapped, alreadyfirstfootfalled)
                                        {
                                            scannedDateTime = timestamp
                                        };

                                        events.Add(new BodyScannedEvent(timestamp, scantype, body) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "ScanOrganic":
                                handled = ScanOrganicEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SellOrganicData":
                                handled = OrganicDataSoldEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Screenshot":
                                handled = ScreenshotEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SellExplorationData":
                                handled = ExplorationDataSoldEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;

                            #endregion

                            #region Trade (and Mining) Events

                            case "AsteroidCracked":
                                handled = AsteroidCrackedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "BuyTradeData":
                                handled = TradeDataPurchasedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CollectCargo":
                                handled = CommodityCollectedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "EjectCargo":
                                handled = CommodityEjectedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "MarketBuy":
                                handled = CommodityPurchasedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "MarketSell":
                                handled = CommoditySoldEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "MiningRefined":
                                handled = CommodityRefinedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;

                            #endregion

                            #region Station Services Events

                            case "BuyAmmo":
                                handled = ShipRestockedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "BuyDrones":
                                handled = LimpetPurchasedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;                                
                            case "CargoDepot":
                                handled = CargoDepotEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CommunityGoal":
                                handled = CommunityGoalsEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CommunityGoalDiscard":
                                handled = MissionAbandonedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "CommunityGoalJoin":
                                handled = MissionAcceptedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "CommunityGoalReward":
                                handled = MissionCompletedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "CrewAssign":
                                handled = CrewAssignedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CrewFire":
                                handled = CrewFiredEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CrewHire":
                                handled = CrewHiredEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "EngineerContribution":
                                handled = EngineerContributedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "EngineerCraft":
                                handled = ModificationCraftedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "EngineerProgress":
                                handled = EngineerProgressedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "FetchRemoteModule":
                                handled = ModuleTransferEvent.Handle(timestamp, line, data, ref events, fromLogLoad);
                                break;
                            case "Market":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var station = JsonParsing.getString(data, "StationName_Localised") ?? JsonParsing.getString(data, "StationName");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    if (MarketInfo.TryFromFile(timestamp, system, station, marketId, out var info, out var raw))
                                    {
                                        events.Add(new MarketEvent(timestamp, marketId, station, system, info) { raw = raw, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "MassModuleStore":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");

                                    data.TryGetValue("Items", out val);
                                    var items = (List<object>)val;

                                    var slots = new List<string>();
                                    var modules = new List<Module>();

                                    if (items != null)
                                    {
                                        foreach (var item in items.Cast<IDictionary<string, object>>() )
                                        {
                                            var slot = JsonParsing.getString(item, "Slot");
                                            slots.Add(slot);

                                            var module = Module.FromEDName(JsonParsing.getString(item, "Name"));
                                            module.hot = JsonParsing.getBool(item, "Hot");
                                            var engineerModifications = JsonParsing.getString(item, "EngineerModifications");
                                            module.modified = engineerModifications != null;
                                            module.engineerlevel = JsonParsing.getOptionalInt(item, "Level") ?? 0;
                                            module.engineerquality = JsonParsing.getOptionalDecimal(item, "Quality") ?? 0;
                                            module.engineermodification = Blueprint.FromEDNameAndGrade( engineerModifications, Convert.ToInt32(Math.Floor(module.engineerquality)) );
                                            modules.Add(module);
                                        }
                                    }

                                    events.Add(new ModulesStoredEvent(timestamp, ship, shipId, slots, modules, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MaterialTrade":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var traderType = JsonParsing.getString(data, "TraderType");

                                    data.TryGetValue("Paid", out var val);
                                    var paid = (Dictionary<string, object>)val;

                                    var materialEdName = JsonParsing.getString(paid, "Material");
                                    var materialPaid = Material.FromEDName(materialEdName);
                                    var materialPaidQty = JsonParsing.getInt(paid, "Quantity");

                                    if (materialPaid == null)
                                    {
                                        Logging.Info("Unknown material " + materialEdName);
                                        Logging.Info("Unknown material " + materialEdName, JsonConvert.SerializeObject(paid));
                                    }

                                    data.TryGetValue("Received", out val);
                                    var received = (Dictionary<string, object>)val;

                                    var materialReceived = Material.FromEDName(JsonParsing.getString(received, "Material"));
                                    var materialReceivedQty = JsonParsing.getInt(received, "Quantity");

                                    if (materialReceived == null)
                                    {
                                        Logging.Info("Unknown material " + materialEdName, JsonConvert.SerializeObject(received));
                                    }

                                    events.Add(new MaterialTradedEvent(timestamp, marketId, traderType, materialPaid, materialPaidQty, materialReceived, materialReceivedQty) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;

                                    break;
                                }
                            case "MissionAbandoned":
                                handled = MissionAbandonedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "MissionAccepted":
                                handled = MissionAcceptedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "MissionCompleted":
                                handled = MissionCompletedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "MissionFailed":
                                handled = MissionFailedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "MissionRedirected":
                                handled = MissionRedirectedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleBuy":
                                handled = ModulePurchasedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleBuyAndStore":
                                handled = ModulePurchasedToStorageEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleRetrieve":
                                handled = ModuleRetrievedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleSell":
                                handled = ModuleSoldEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleSellRemote":
                                handled = ModuleSoldFromStorageEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleStore":
                                handled = ModuleStoredEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleSwap":
                                handled = ModuleSwappedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Outfitting":
                                handled = OutfittingEvent.Handle( timestamp, data, ref events, fromLogLoad );
                                break;
                            case "PayBounties":
                                handled = BountyPaidEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "PayFines":
                                handled = FinePaidEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "RedeemVoucher":
                                {
                                    var type = JsonParsing.getString(data, "Type");
                                    var rewards = new List<Reward>();

                                    // Obtain list of factions
                                    data.TryGetValue("Factions", out var val);
                                    var factionsData = (List<object>)val;
                                    if (factionsData != null)
                                    {
                                        foreach (var rewardData in factionsData.Cast<IDictionary<string, object>>())
                                        {
                                            var factionName = EventParsing.FactionName(rewardData, "Faction");
                                            rewardData.TryGetValue("Amount", out val);
                                            var factionReward = (long)val;

                                            rewards.Add(new Reward(factionName, factionReward));
                                        }
                                    }
                                    else
                                    {
                                        var factionName = EventParsing.FactionName(data, "Faction");
                                        data.TryGetValue("Amount", out val);
                                        var factionReward = (long)val;

                                        rewards.Add(new Reward(factionName, factionReward));
                                    }
                                    data.TryGetValue("Amount", out val);
                                    var amount = (long)val;

                                    var brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");

                                    if (type == "bounty")
                                    {
                                        events.Add(new BountyRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (type == "CombatBond")
                                    {
                                        events.Add(new BondRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (type == "trade")
                                    {
                                        events.Add(new TradeVoucherRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (type is "codex" or "settlement" or "scannable")
                                    {
                                        events.Add(new DataVoucherRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        Logging.Warn("Unhandled voucher type " + type, line);
                                    }
                                }
                                handled = true;
                                break;
                            case "RefuelAll":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var amount = JsonParsing.getDecimal(data, "Amount");
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null, true) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RefuelPartial":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var amount = JsonParsing.getDecimal(data, "Amount");
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null, false) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Repair":
                                {
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;

                                    // Starting with version 3.7, the "Repair" event may contain one item or multiple items
                                    // Each item is either a description (e.g. all, wear, hull, paint) or the name of a module
                                    data.TryGetValue("Items", out var itemsVal);
                                    if (itemsVal != null)
                                    {
                                        if (itemsVal is List<object> itemEDNames)
                                        {
                                            events.Add(new ShipRepairedEvent(timestamp, itemEDNames.ConvertAll(o => o.ToString()), price) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                    else
                                    {
                                        // We have a single "item"
                                        var itemEDName = JsonParsing.getString(data, "Item");
                                        if (!string.IsNullOrEmpty(itemEDName))
                                        {
                                            events.Add(new ShipRepairedEvent(timestamp, itemEDName, price) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "RepairAll":
                                {
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;
                                    events.Add(new ShipRepairedEvent(timestamp, "All", price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ScientificResearch":
                                {
                                    var material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    var amount = JsonParsing.getInt(data, "Count");
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    events.Add(new MaterialDonatedEvent(timestamp, material, amount, marketId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "SearchAndRescue":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var commodityName = JsonParsing.getString(data, "Name");
                                    var commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Name"));
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    data.TryGetValue("Count", out var val);
                                    var amount = (int?)(long?)val;
                                    data.TryGetValue("Reward", out val);
                                    var reward = val == null ? 0 : (long)val;
                                    events.Add(new SearchAndRescueEvent(timestamp, commodity, amount, reward, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SellDrones":
                                {
                                    data.TryGetValue("Count", out var val);
                                    var amount = (int)(long)val;
                                    data.TryGetValue("SellPrice", out val);
                                    var price = (int)(long)val;
                                    events.Add(new LimpetSoldEvent(timestamp, amount, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SellShipOnRebuy":
                                {
                                    data.TryGetValue("SellShipId", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "ShipType");
                                    data.TryGetValue("ShipPrice", out val);
                                    var price = (long)val;
                                    var system = JsonParsing.getString(data, "System");
                                    events.Add(new ShipSoldOnRebuyEvent(timestamp, ship, shipId, price, system) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SetUserShipName":
                                {
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");
                                    var name = JsonParsing.getString(data, "UserShipName");
                                    var ident = JsonParsing.getString(data, "UserShipId");

                                    events.Add(new ShipRenamedEvent(timestamp, ship, shipId, name, ident) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Shipyard":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var station = JsonParsing.getString(data, "StationName");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    if (ShipyardInfo.TryFromFile(timestamp, system, station, marketId, out var info, out var raw))
                                    {
                                        events.Add(new ShipyardEvent(timestamp, marketId, station, system, info) { raw = raw, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "ShipyardBuy":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");

                                    // We don't have a ship ID at this point so use the ship type
                                    var ship = JsonParsing.getString(data, "ShipType");

                                    data.TryGetValue("ShipPrice", out var val);
                                    var price = (long)val;

                                    data.TryGetValue("StoreShipID", out val);
                                    var storedShipId = val == null ? (int?)null : (int)(long)val;
                                    var storedShip = JsonParsing.getString(data, "StoreOldShip");

                                    data.TryGetValue("SellShipID", out val);
                                    var soldShipId = val == null ? (int?)null : (int)(long)val;
                                    var soldShip = JsonParsing.getString(data, "SellOldShip");

                                    data.TryGetValue("SellPrice", out val);
                                    var soldPrice = (long?)val;
                                    events.Add(new ShipPurchasedEvent(timestamp, ship, price, soldShip, soldShipId, soldPrice, storedShip, storedShipId, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardNew":
                            case "ShipRedeemed":
                                {
                                    data.TryGetValue("NewShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "ShipType");

                                    events.Add(new ShipDeliveredEvent(timestamp, ship, shipId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardSell":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("SellShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "ShipType");
                                    data.TryGetValue("ShipPrice", out val);
                                    var price = (long)val;
                                    var system = JsonParsing.getString(data, "System"); // Only written when the ship is in a different star system
                                    events.Add(new ShipSoldEvent(timestamp, ship, shipId, price, system, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardTransfer":
                                handled = ShipTransferInitiatedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ShipyardSwap":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "ShipType");

                                    data.TryGetValue("StoreShipID", out val);
                                    var storedShipId = val == null ? (int?)null : (int)(long)val;
                                    var storedShip = JsonParsing.getString(data, "StoreOldShip");

                                    data.TryGetValue("SellShipID", out val);
                                    var soldShipId = val == null ? (int?)null : (int)(long)val;
                                    var soldShip = JsonParsing.getString(data, "SellOldShip");

                                    events.Add(new ShipSwappedEvent(timestamp, ship, shipId, soldShip, soldShipId, storedShip, storedShipId, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "StoredModules":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var storedModules = new List<StoredModule>();

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var station = JsonParsing.getString(data, "StationName");

                                    data.TryGetValue("Items", out var val);
                                    var items = (List<object>)val;
                                    if (items != null)
                                    {
                                        foreach (var item in items.Cast<IDictionary<string, object>>() )
                                        {
                                            var name = JsonParsing.getString(item, "Name");
                                            var module = new Module(Module.FromEDName(name))
                                            {
                                                hot = JsonParsing.getOptionalBool(item, "Hot") ?? false,
                                                price = JsonParsing.getOptionalLong(item, "BuyPrice") ?? 0,
                                                mercPrice = JsonParsing.getOptionalLong(item, "BuyMercCoinsPrice") ?? 0,
                                            };
                                            item.TryGetValue("EngineerModifications", out val);
                                            module.modificationEDName = JsonParsing.getString(item, "EngineerModifications");
                                            module.modified = !string.IsNullOrEmpty(module.modificationEDName);
                                            module.engineerlevel = module.modified ? JsonParsing.getInt(item, "Level") : 0;
                                            module.engineermodification = Blueprint.FromEDNameAndGrade(module.modificationEDName, module.engineerlevel) ?? Blueprint.None;
                                            module.engineerquality = module.modified ? JsonParsing.getDecimal(item, "Quality") : 0;

                                            var storedModule = new StoredModule
                                            {
                                                module = module,
                                                slot = JsonParsing.getInt(item, "StorageSlot"),
                                                intransit = JsonParsing.getOptionalBool(item, "InTransit") ?? false,
                                                system = JsonParsing.getString(item, "StarSystem"),
                                                marketid = JsonParsing.getOptionalLong(item, "MarketID"),
                                                transfercost = JsonParsing.getOptionalLong(item, "TransferCost"),
                                                transfertime = JsonParsing.getOptionalLong(item, "TransferTime")
                                            };
                                            storedModules.Add(storedModule);
                                        }
                                    }
                                    events.Add(new StoredModulesEvent(timestamp, marketId, station, system, storedModules) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "StoredShips":
                                handled = StoredShipsEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "TechnologyBroker":
                                {
                                    var brokerType = JsonParsing.getString(data, "BrokerType");
                                    var marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("ItemsUnlocked", out var val);
                                    var itemsUnlocked = (List<object>)val;
                                    var items = new List<Module>();
                                    foreach (var item in itemsUnlocked)
                                    {
                                        var itemProperties = (Dictionary<string, object>)item;
                                        var moduleEdName = JsonParsing.getString(itemProperties, "Name");
                                        var module = Module.FromEDName(moduleEdName);
                                        if (module == null)
                                        {
                                            // Unknown module
                                            Logging.Info("Unknown module " + moduleEdName, JsonConvert.SerializeObject(item));
                                        }
                                        items.Add(module);
                                    }

                                    data.TryGetValue("Commodities", out val);
                                    var commodities = (List<object>)val;
                                    var Commodities = new List<CommodityAmount>();
                                    foreach (var _commodity in commodities.Cast<IDictionary<string, object>>() )
                                    {
                                        var commodityEdName = JsonParsing.getString(_commodity, "Name");
                                        var commodity = CommodityDefinition.FromEDName(commodityEdName);
                                        var count = JsonParsing.getInt(_commodity, "Count");
                                        if (commodity == null)
                                        {
                                            Logging.Info("Unknown commodity " + commodityEdName, JsonConvert.SerializeObject(_commodity));
                                            continue;
                                        }
                                        Commodities.Add(new CommodityAmount(commodity, count));
                                    }

                                    data.TryGetValue("Materials", out val);
                                    var materials = (List<object>)val;
                                    var Materials = new List<MaterialAmount>();
                                    foreach (var _material in materials.Cast<IDictionary<string, object>>() )
                                    {
                                        var materialEdName = JsonParsing.getString(_material, "Name");
                                        var material = Material.FromEDName(materialEdName);
                                        var count = JsonParsing.getInt(_material, "Count");
                                        Materials.Add(new MaterialAmount(material, count));
                                    }

                                    events.Add(new TechnologyBrokerEvent(timestamp, brokerType, marketId, items, Commodities, Materials) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Powerplay Events

                            case "DeliverPowerMicroResources":
                                {
                                    // { "timestamp":"2024-10-19T15:01:28Z", "event":"DeliverPowerMicroResources", "TotalCount":2, "MicroResources":[ { "Name":"powerelectronics", "Name_Localised":"Electronics Package", "Category":"Item", "Count":2 } ], "MarketID":3223182848 }
                                    if ( data.ContainsKey( "TotalCount" ) )
                                    {
                                        var marketID = JsonParsing.getLong( data, "MarketID" );
                                        var resourceAmounts = new List<MicroResourceAmount>();
                                        if ( data.TryGetValue( "MicroResources", out var val ) )
                                        {
                                            if ( val is List<object> listVal )
                                            {
                                                foreach ( var res in listVal )
                                                {
                                                    if ( res is IDictionary<string, object> microVal )
                                                    {
                                                        var microResource = EventParsing.MicroResource( microVal );
                                                        var amount = JsonParsing.getInt(microVal, "Count");
                                                        if ( microResource != null )
                                                        {
                                                            resourceAmounts.Add( new MicroResourceAmount( microResource, amount ) );
                                                        }
                                                    }
                                                }
                                                events.Add( new PowerMicroResourcesDeliveredEvent( timestamp, marketID, resourceAmounts ) { raw = line, fromLoad = fromLogLoad } );
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // We haven't seen this pattern before. Break unhandled.
                                        break;
                                    }
                                }
                                handled = true;
                                break;
                            case "HoloscreenHacked":
                                {
                                    // {"timestamp":"2024-10-22T20:40:06Z","event":"HoloscreenHacked","PowerBefore":"Aisling Duval","PowerAfter":"Yuri Grom"}

                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading
                                    
                                    var powerBefore = Power.FromEDName( JsonParsing.getString( data, "PowerBefore" ) );
                                    var powerAfter = Power.FromEDName( JsonParsing.getString( data, "PowerAfter" ) );
                                    events.Add(new HoloscreenHackedEvent(timestamp, powerBefore, powerAfter) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayCollect":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue( "Count", out var val );
                                    var amount = (int)(long)val;
                                    // May be a commodity or a microresource
                                    var receivedEdName = JsonParsing.getString( data, "Type" );
                                    var commodityDef = CommodityDefinition.AllOfThem.FirstOrDefault(c => c.edname.Equals(receivedEdName, StringComparison.InvariantCultureIgnoreCase));
                                    if ( commodityDef != null )
                                    {
                                        commodityDef.fallbackLocalizedName = JsonParsing.getString( data, "Type_Localised" );
                                        events.Add( new PowerCommodityObtainedEvent( timestamp, power, commodityDef, amount ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                    var microResourceDef = MicroResource.AllOfThem.FirstOrDefault(m => m.edname.Equals(receivedEdName, StringComparison.InvariantCultureIgnoreCase));
                                    if ( microResourceDef != null )
                                    {
                                        // Microresources are already handled via the PowerMicroResourcesCollectedEvent
                                    }
                                    else
                                    {
                                        // We haven't seen this pattern before. Break unhandled.
                                        break;
                                    }
                                }
                                handled = true;
                                break;
                            case "PowerplayDeliver":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue( "Count", out var val );
                                    var amount = (int)(long)val;
                                    // May be a commodity or a microresource
                                    var deliveredEdName = JsonParsing.getString( data, "Type" );
                                    var commodityDef = CommodityDefinition.AllOfThem.FirstOrDefault(c => c.edname.Equals(deliveredEdName, StringComparison.InvariantCultureIgnoreCase));
                                    if ( commodityDef != null )
                                    {
                                        commodityDef.fallbackLocalizedName = JsonParsing.getString( data, "Type_Localised" );
                                        events.Add( new PowerCommodityDeliveredEvent( timestamp, power, commodityDef, amount ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                    var microResourceDef = MicroResource.AllOfThem.FirstOrDefault(m => m.edname.Equals(deliveredEdName, StringComparison.InvariantCultureIgnoreCase));
                                    if ( microResourceDef != null )
                                    {
                                        // Microresources are already handled via the PowerMicroResourcesDeliveredEvent
                                    }
                                    else
                                    {
                                        // We haven't seen this pattern before. Break unhandled.
                                        break;
                                    }
                                }
                                handled = true;
                                break;
                            case "PowerplayJoin":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    events.Add(new PowerJoinedEvent(timestamp, power) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayLeave":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    events.Add(new PowerLeftEvent(timestamp, power) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayMerits":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    var meritsGained = JsonParsing.getInt( data, "MeritsGained" );
                                    var meritsTotal = JsonParsing.getInt( data, "TotalMerits" );
                                    events.Add(new PowerMeritsEvent(timestamp, power, meritsGained, meritsTotal ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "PowerplayRank":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    var rank = JsonParsing.getInt( data, "Rank" );
                                    events.Add( new PowerRankEvent( timestamp, power, rank ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "PowerplayVoucher":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue("Systems", out var val);
                                    var systems = ((List<object>)val).Cast<string>().ToList();
                                    events.Add(new PowerVoucherReceivedEvent(timestamp, power, systems) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RequestPowerMicroResources":
                                {
                                    // {"timestamp":"2024-10-23T19:59:28Z","event":"RequestPowerMicroResources","TotalCount":3,"MicroResources":[{"Name":"powerspyware","Name_Localised":"Power Tracker Malware","Category":"Data","Count":3}],"MarketID":3930400257}
                                    if ( data.ContainsKey( "TotalCount" ) )
                                    {
                                        var marketID = JsonParsing.getLong( data, "MarketID" );
                                        var resourceAmounts = new List<MicroResourceAmount>();
                                        if ( data.TryGetValue( "MicroResources", out var val ) )
                                        {
                                            if ( val is List<object> listVal )
                                            {
                                                foreach ( var res in listVal )
                                                {
                                                    if ( res is IDictionary<string, object> microVal )
                                                    {
                                                        var microResource = EventParsing.MicroResource( microVal );
                                                        var amount = JsonParsing.getInt(microVal, "Count");
                                                        if ( microResource != null )
                                                        {
                                                            resourceAmounts.Add( new MicroResourceAmount( microResource, amount ) );
                                                        }
                                                    }
                                                }
                                                events.Add( new PowerMicroResourcesCollectedEvent( timestamp, marketID, resourceAmounts ) { raw = line, fromLoad = fromLogLoad } );
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // We haven't seen this pattern before. Break unhandled.
                                        break;
                                    }
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Squadron Events

                            case "AppliedToSquadron":
                            case "DisbandedSquadron":
                            case "InvitedToSquadron":
                            case "JoinedSquadron":
                            case "KickedFromSquadron":
                            case "LeftSquadron":
                            case "SquadronCreated":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "SquadronName");
                                    var squadronID = JsonParsing.getOptionalInt( data, "SquadronID" );
                                    var status = edType.Replace("Squadron", "")
                                        .Replace("To", "")
                                        .Replace("From", "")
                                        .ToLowerInvariant();

                                    events.Add(new SquadronStatusEvent(timestamp, name, squadronID, status) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SquadronDemotion":
                            case "SquadronPromotion":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "SquadronName");
                                    var squadronID = JsonParsing.getOptionalInt( data, "SquadronID" );

                                    var oldRankID = JsonParsing.getOptionalInt(data, "OldRank");
                                    var oldRankName = JsonParsing.getString(data, "OldRankName");
                                    var oldRankNameLocalized = JsonParsing.getString(data, "OldRankName_Localised");
                                    var oldRank = new SquadronRank(oldRankID, oldRankName, oldRankNameLocalized);

                                    var newRankID = JsonParsing.getOptionalInt(data, "NewRank");
                                    var newRankName = JsonParsing.getString(data, "NewRankName");
                                    var newRankNameLocalized = JsonParsing.getString(data, "NewRankName_Localised");
                                    var newRank = new SquadronRank(newRankID, newRankName, newRankNameLocalized);

                                    events.Add(new SquadronRankEvent(timestamp, name, squadronID, oldRank, newRank) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Fleet Carrier Events

                            case "CarrierBankTransfer":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var deposit = JsonParsing.getOptionalLong(data, "Deposit") ?? 0;
                                    var withdrawal = JsonParsing.getOptionalLong(data, "Withdraw") ?? 0;
                                    var cmdrBalance = JsonParsing.getOptionalLong(data, "PlayerBalance") ?? 0;
                                    var carrierBalance = JsonParsing.getOptionalLong(data, "CarrierBalance") ?? 0;
                                    events.Add(new CarrierBankTransferEvent(timestamp, carrierID, carrierType, deposit, withdrawal, cmdrBalance, carrierBalance) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierBuy":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var carrierCallsign = JsonParsing.getString(data, "Callsign");
                                    var carrierStarSystem = JsonParsing.getString(data, "Location");
                                    var carrierSystemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var price = JsonParsing.getOptionalLong(data, "Price");
                                    events.Add(new CarrierPurchasedEvent(timestamp, carrierID, carrierCallsign, carrierType, carrierStarSystem, carrierSystemAddress, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierCancelDecommission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    events.Add(new CarrierDecommissionCancelledEvent(timestamp, carrierID, carrierType ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierCrewServices":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var operation = JsonParsing.getString(data, "Operation");
                                    var crewRole = StationService.FromEDName(JsonParsing.getString(data, "CrewRole"));
                                    var crewName = JsonParsing.getString(data, "CrewName");
                                    events.Add(new CarrierServiceChangedEvent(timestamp, carrierID, carrierType, operation, crewRole, crewName) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDecommission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var refund = JsonParsing.getULong(data, "ScrapRefund");
                                    var decommissionTimespan = Dates.fromTimestamp(JsonParsing.getOptionalLong(data, "ScrapTime")) - timestamp;
                                    events.Add(new CarrierDecommissionScheduledEvent(timestamp, carrierID, carrierType, refund, decommissionTimespan ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDepositFuel":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var amount = JsonParsing.getInt(data, "Amount");
                                    var total = JsonParsing.getInt(data, "Total");
                                    events.Add(new CarrierFuelDepositEvent(timestamp, carrierID, carrierType, amount, total) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDockingPermission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var dockingAccess = JsonParsing.getString(data, "DockingAccess");
                                    var allowNotorious = JsonParsing.getBool(data, "AllowNotorious");
                                    events.Add(new CarrierDockingPermissionEvent(timestamp, carrierID, carrierType, dockingAccess, allowNotorious) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierFinance":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var taxRate = JsonParsing.getOptionalInt(data, "TaxRate") ?? 0;
                                    var reservePercent = JsonParsing.getOptionalInt(data, "ReservePercent") ?? 0;
                                    var carrierBalance = JsonParsing.getLong(data, "CarrierBalance");
                                    var carrierReserveBalance = JsonParsing.getLong(data, "ReserveBalance");
                                    var carrierAvailableBalance = JsonParsing.getLong(data, "CarrierAvailableBalance");
                                    events.Add(new CarrierFinanceEvent(timestamp, carrierID, carrierType, taxRate, reservePercent, carrierBalance, carrierReserveBalance, carrierAvailableBalance) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierJump":
                                handled = CarrierJumpedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CarrierJumpRequest":
                                handled = CarrierJumpRequestEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CarrierJumpCancelled":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierId = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );

                                    events.Add(new CarrierJumpCancelledEvent(timestamp, carrierId, carrierType ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierLocation":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var systemName = JsonParsing.getString(data, "StarSystem");
                                    var bodyId = JsonParsing.getLong(data, "BodyID");
                                    events.Add(new CarrierLocationEvent(timestamp, carrierID, carrierType, systemAddress, systemName, bodyId ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "CarrierNameChange":
                                {
                                    // We've observed a missing `CarrierType` field name in the `CarrierNameChange` event, fix that here.
                                    if ( line.Contains( @"""event"":""CarrierNameChange""" ) && line.Contains( @""""": ""SquadronCarrier""," ) )
                                    {
                                        line = line.Replace( @""""": ""SquadronCarrier"",", @"""CarrierType"": ""SquadronCarrier""," );
                                    }

                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var callsign = JsonParsing.getString(data, "Callsign");
                                    var name = JsonParsing.getString(data, "Name");
                                    events.Add(new CarrierNameChangeEvent(timestamp, carrierID, carrierType, callsign, name ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierStats":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierCallsign = JsonParsing.getString(data, "Callsign");
                                    var carrierName = JsonParsing.getString(data, "Name");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );

                                    var dockingAccess = JsonParsing.getString(data, "DockingAccess");
                                    var notoriousAccess = JsonParsing.getBool(data, "AllowNotorious");
                                    var fuelLevel = JsonParsing.getInt(data, "FuelLevel");
                                    var jumpRange = JsonParsing.getOptionalDecimal( data, "JumpRangeCurr" ) ?? 500;
                                    var jumpRangeMax = JsonParsing.getOptionalDecimal( data, "JumpRangeMax" ) ?? 500;
                                    var pendingDecommission = JsonParsing.getOptionalBool(data, "PendingDecommission") ?? false;

                                    var crewSpace = 0;
                                    var cargoSpace = 0;
                                    var cargoSpaceReserved = 0;
                                    var shipPacks = 0;
                                    var modulePacks = 0;
                                    var freeSpace = 0;
                                    if (data.TryGetValue("SpaceUsage", out var spaceUsage) && spaceUsage is Dictionary<string, object> space)
                                    {
                                        crewSpace = JsonParsing.getInt(space, "Crew");
                                        cargoSpace = JsonParsing.getInt(space, "Cargo");
                                        cargoSpaceReserved = JsonParsing.getInt(space, "CargoSpaceReserved");
                                        shipPacks = JsonParsing.getInt(space, "ShipPacks");
                                        modulePacks = JsonParsing.getInt(space, "ModulePacks");
                                        freeSpace = JsonParsing.getInt(space, "FreeSpace");
                                    }
                                    var usedSpace = crewSpace 
                                                    + cargoSpace 
                                                    + cargoSpaceReserved 
                                                    + shipPacks 
                                                    + modulePacks;

                                    long bankBalance = 0;
                                    long bankReservedBalance = 0;
                                    long bankAvailableBalance = 0;
                                    if (data.TryGetValue("Finance", out var finances) && finances is Dictionary<string, object> finance)
                                    {
                                        bankBalance = JsonParsing.getLong(finance, "CarrierBalance");
                                        bankReservedBalance = JsonParsing.getLong(finance, "ReserveBalance");
                                        bankAvailableBalance = JsonParsing.getLong(finance, "AvailableBalance");
                                    }

                                    events.Add(new CarrierStatsEvent(timestamp, carrierID, carrierType, carrierCallsign, carrierName, dockingAccess, notoriousAccess, fuelLevel, usedSpace, freeSpace, bankBalance, bankReservedBalance, bankAvailableBalance, jumpRange, jumpRangeMax, pendingDecommission ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Odyssey (On Foot) Events
                                
                            case "Backpack":
                            case "ShipLocker":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    if ( MicroResourceInfo.TryFromFile(timestamp, out var info, out line, $"{edType}.json"))
                                    {
                                        if ( info is null ) { break; }

                                        // Flatten the list
                                        var inventory = new List<MicroResourceAmount>();
                                        inventory.AddRange(info.Components);
                                        inventory.AddRange(info.Consumables);
                                        inventory.AddRange(info.Data);
                                        inventory.AddRange(info.Items);

                                        if (edType == "Backpack")
                                        {
                                            events.Add(new BackpackEvent(timestamp, inventory) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (edType == "ShipLocker")
                                        {
                                            events.Add(new ShipLockerEvent(timestamp, inventory) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "BackpackChange":
                                {
                                    // Note: Also updates backpack.json
                                    var added = MicroResourceInfo.ReadMicroResources("Added", data);
                                    var removed = MicroResourceInfo.ReadMicroResources("Removed", data);
                                    events.Add(new BackpackChangedEvent(timestamp, added, removed) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BookDropship":
                            case "BookTaxi":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var type = edType.Replace("Book", "");
                                    var price = JsonParsing.getOptionalInt(data, "Cost");
                                    var system = JsonParsing.getString(data, "DestinationSystem");
                                    var destination = JsonParsing.getString(data, "DestinationLocation");
                                    events.Add(new BookTransportEvent(timestamp, type, price, system, destination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BuyMicroResources":
                                {
                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID");

                                    // The `BuyMicroResources` event sometimes contains an array
                                    // (thought to occur primarily when buying from a fleet carrier bartender)
                                    if ( data.ContainsKey("TotalCount") )
                                    {
                                        var price = JsonParsing.getInt( data, "Price" ); // Total price
                                        var resourceAmounts = new List<MicroResourceAmount>();
                                        if ( data.TryGetValue( "MicroResources", out var val ) )
                                        {
                                            if ( val is List<object> listVal )
                                            {
                                                foreach ( var res in listVal )
                                                {
                                                    if ( res is IDictionary<string, object> microVal )
                                                    {
                                                        var microResource = EventParsing.MicroResource( microVal );
                                                        var amount = JsonParsing.getInt(microVal, "Count");
                                                        if ( microResource != null )
                                                        {
                                                            resourceAmounts.Add( new MicroResourceAmount( microResource, amount ) );
                                                        }
                                                    }
                                                }
                                                events.Add( new MicroResourcesPurchasedEvent( timestamp, resourceAmounts, price, marketId ) { raw = line, fromLoad = fromLogLoad } );
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var microResource = EventParsing.MicroResource( data );
                                        var amount = JsonParsing.getInt( data, "Count" );
                                        var price = JsonParsing.getInt(data, "Price"); // Total price
                                        var resourceAmounts = new List<MicroResourceAmount> { new( microResource, amount ) };
                                        events.Add( new MicroResourcesPurchasedEvent( timestamp, resourceAmounts, price, marketId ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                    handled = true;
                                }
                                break;
                            case "BuySuit":
                                handled = SuitPurchasedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "BuyWeapon":
                                handled = HandWeaponPurchasedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CancelDropship":
                            case "CancelTaxi":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var type = edType.Replace("Cancel", "");
                                    var refund = JsonParsing.getOptionalInt(data, "Refund");
                                    events.Add(new CancelTransportEvent(timestamp, type, refund) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Disembark":
                                handled = DisembarkEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "DropshipDeploy":
                                {
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
                                    // There are `OnStation` and `OnPlanet` properties, but these are
                                    // always false and always true so we won't bother parsing them.

                                    events.Add(new DropshipDeploymentEvent(timestamp, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Embark":
                                handled = EmbarkEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "FCMaterials":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierID = JsonParsing.getLong(data, "MarketID");
                                    var carrierName = JsonParsing.getString(data, "CarrierName");
                                    var callsign = JsonParsing.getString(data, "CarrierID");

                                    var (raw, parsed, isRecent) = Files.FromSavedGamesAsync(
                                        "FCMaterials.json",
                                        extract: json =>
                                        {
                                            var o = JsonConvert.DeserializeObject<FCMaterialsInfo>( json );
                                            return (o?.timestamp, o);
                                        },
                                        compareTo: timestamp
                                    ).GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );

                                    if ( isRecent && parsed != null
                                                  && parsed.CarrierID == carrierID
                                                  && parsed.CarrierName == carrierName
                                                  && parsed.callsign == callsign )
                                    {
                                        events.Add( new FleetCarrierMaterialsEvent( timestamp, carrierID, carrierName,
                                            callsign, parsed ) { raw = raw, fromLoad = fromLogLoad } );
                                    }
                                }
                                handled = true;
                                break;
                            case "SellSuit":
                                handled = SuitSoldEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SellWeapon":
                                handled = HandWeaponSoldEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "UpgradeWeapon":
                                handled = HandWeaponUpgraded.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "UpgradeSuit":
                                handled = SuitUpgradedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;

                            #endregion

                            #region Other (Miscellaneous) Events

                            case "AfmuRepairs":
                                handled = ShipAfmuRepairedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ApproachSettlement":
                                handled = SettlementApproachedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "CargoTransfer":
                                handled = CargoTransferEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ChangeCrewRole":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var role = EventParsing.CrewRole(data, "Role");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewRoleChangedEvent(timestamp, role, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CockpitBreached":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    events.Add(new CockpitBreachedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommitCrime":
                                {
                                    object val;
                                    var crimetype = JsonParsing.getString(data, "CrimeType");
                                    var faction = EventParsing.FactionName(data, "Faction");
                                    var victim = JsonParsing.getString(data, "Victim");

                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "Victim_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        victim = NpcAuthorityShip.EDNameExists(victim)
                                            ? NpcAuthorityShip.FromEDName(victim)?.localizedName
                                            : JsonParsing.getString(data, "Victim_Localised");
                                    }

                                    // Might be a fine or a bounty
                                    if (data.ContainsKey("Fine"))
                                    {
                                        data.TryGetValue("Fine", out val);
                                        var fine = (long)val;
                                        events.Add(new FineIncurredEvent(timestamp, crimetype, faction, victim, fine) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        data.TryGetValue("Bounty", out val);
                                        var bounty = (long)val;
                                        events.Add(new BountyIncurredEvent(timestamp, crimetype, faction, victim, bounty) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "CrewLaunchFighter":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "Crew");
                                    var fighterId = JsonParsing.getInt(data, "ID");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewMemberLaunchedEvent(timestamp, name, fighterId, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberJoins":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewMemberJoinedEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberRoleChange":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "Crew");
                                    var role = EventParsing.CrewRole(data, "Role");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewMemberRoleChangedEvent(timestamp, name, role, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberQuits":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewMemberLeftEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DatalinkScan":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var message = JsonParsing.getString(data, "Message");
                                    events.Add(new DatalinkMessageEvent(timestamp, message) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DatalinkVoucher":
                                {
                                    data.TryGetValue("Reward", out var val);
                                    var reward = (long)val;
                                    var victimFaction = EventParsing.FactionName(data, "VictimFaction");
                                    var payeeFaction = EventParsing.FactionName(data, "PayeeFaction");
                                    events.Add(new DataVoucherAwardedEvent(timestamp, payeeFaction, victimFaction, reward) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DataScanned":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var datalinktype = DataScan.FromEDName(JsonParsing.getString(data, "Type"));
                                    events.Add(new DataScannedEvent(timestamp, datalinktype) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockFighter":
                                handled = VesselDockedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "DockSRV":
                                handled = VesselDockedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "EndCrewSession":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var onCrime = JsonParsing.getOptionalBool(data, "OnCrime");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewSessionEndedEvent(timestamp, onCrime, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FighterRebuilt":
                                handled = FighterRebuiltEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Friends":
                                handled = FriendsEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "FuelScoop":
                                {
                                    // We're handling this via the Status Monitor
                                }
                                handled = true;
                                break;
                            case "GameModeChange":
                                handled = GameModeChangedEvent.Handle( timestamp, data, ref events );
                                break;
                            case "JetConeBoost":
                                {
                                    var boost = JsonParsing.getDecimal(data, "BoostValue");
                                    events.Add(new JetConeBoostEvent(timestamp, boost) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "JetConeDamage":
                                {
                                    var modulename = JsonParsing.getString(data, "Module");
                                    var module = Module.FromEDName(modulename);
                                    if (module != null)
                                    {
                                        if (module.Mount != null)
                                        {
                                            // This is a weapon so provide a bit more information
                                            var mount = module.mount;
                                            modulename = "" + module.@class.ToString() + module.grade + " " + mount + " " + module.localizedName;
                                        }
                                        else
                                        {
                                            modulename = module.localizedName;
                                        }
                                    }

                                    events.Add(new JetConeDamageEvent(timestamp, modulename, module) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "JoinACrew":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewJoinedEvent(timestamp, captain, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "KickCrewMember":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewMemberRemovedEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LaunchDrone":
                                {
                                    var kind = JsonParsing.getString(data, "Type");
                                    events.Add(new LimpetLaunchedEvent(timestamp, kind) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LaunchFighter":
                            case "LaunchSRV":
                            case "LaunchVessel":
                                handled = VesselLaunchedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleInfo":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    if ( ModuleInfo.TryFromFile(timestamp, out var info, out line))
                                    {
                                        events.Add(new ModuleInfoEvent(timestamp, info.Modules)
                                            { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "Music":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var musicTrack = JsonParsing.getString(data, "MusicTrack");
                                    events.Add(new MusicEvent(timestamp, musicTrack) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "NpcCrewPaidWage":
                                handled = CrewPaidWageEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "NpcCrewRank":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "NpcCrewName");
                                    var crewid = JsonParsing.getLong(data, "NpcCrewId");
                                    data.TryGetValue("RankCombat", out var val);
                                    var rating = CombatRating.FromRank(Convert.ToInt32(val));
                                    events.Add(new CrewPromotionEvent(timestamp, name, crewid, rating) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Promotion":
                                {
                                    object rating = null;
                                    if (data.TryGetValue("Combat", out var val))
                                    {
                                        rating = CombatRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("CQC", out val))
                                    {
                                        rating = CQCRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Trade", out val))
                                    {
                                        rating = TradeRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Explore", out val))
                                    {
                                        rating = ExplorationRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Federation", out val))
                                    {
                                        rating = FederationRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Empire", out val))
                                    {
                                        rating = EmpireRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Soldier", out val))
                                    {
                                        rating = MercenaryRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Exobiologist", out val))
                                    {
                                        rating = ExobiologistRating.FromRank(Convert.ToInt32(val));
                                    }
                                    if (rating != null)
                                    {
                                        var genderPreference = ConfigService.Instance.commanderConfiguration.gender;
                                        events.Add(new CommanderPromotionEvent(timestamp, rating, genderPreference) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "ProspectedAsteroid":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    data.TryGetValue("Materials", out var val); // (array of Name and Proportion)
                                    var commodities = new List<CommodityPresence>();
                                    if (val is List<object> listVal)
                                    {
                                        foreach (var commodityVal in listVal)
                                        {
                                            if (commodityVal is Dictionary<string, object> commodityData)
                                            {
                                                var commodityEdName = JsonParsing.getString(commodityData, "Name");
                                                var commodity = CommodityDefinition.FromEDName(commodityEdName);
                                                var proportion = JsonParsing.getDecimal(commodityData, "Proportion"); // Out of 100
                                                if (commodity != null)
                                                {
                                                    commodity.fallbackLocalizedName = JsonParsing.getString(commodityData, "Name_Localised");
                                                    commodities.Add(new CommodityPresence(commodity, proportion));
                                                }
                                            }
                                        }
                                    }
                                    var content = JsonParsing.getString(data, "Content"); // (a string representing High/Medium/Low material content)
                                    var materialContent = new AsteroidMaterialContent(content)
                                    {
                                        fallbackLocalizedName = JsonParsing.getString(data, "Content_Localised")?.Replace("Material Content: ", "")
                                    };
                                    var remaining = JsonParsing.getDecimal(data, "Remaining"); // Out of 100

                                    // If a motherlode commodity is present
                                    CommodityDefinition motherlodeCommodityDefinition = null;
                                    var motherlodeEDName = JsonParsing.getString(data, "MotherlodeMaterial");
                                    if (!string.IsNullOrEmpty(motherlodeEDName))
                                    {
                                        motherlodeCommodityDefinition = CommodityDefinition.FromEDName(motherlodeEDName);
                                    }

                                    events.Add(new AsteroidProspectedEvent(timestamp, commodities, materialContent, remaining, motherlodeCommodityDefinition) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "QuitACrew":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = JsonParsing.getOptionalBool(data, "Telepresence");
                                    events.Add(new CrewLeftEvent(timestamp, captain, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RebootRepair":
                                {
                                    // This event returns a list of slots rather than actual module ednames.
                                    List<string> compartmentsJson = null;
                                    data.TryGetValue( "Modules", out var val );
                                    if ( val is List<string> ls )
                                    {
                                        compartmentsJson = ls;
                                    }
                                    else if ( val is List<object> lo )
                                    {
                                        compartmentsJson = lo.Select( o => o.ToString() ).ToList();
                                    }
                                    events.Add(new ShipRebootedEvent( timestamp, compartmentsJson ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ReceiveText":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var from = JsonParsing.getString(data, "From");
                                    var localizedFrom = JsonParsing.getString(data, "From_Localised");
                                    var channel = JsonParsing.getString(data, "Channel");
                                    var message = JsonParsing.getString(data, "Message");
                                    MessageChannel messageChannel;
                                    MessageSource source;

                                    if (from == string.Empty && channel == "npc" && (message.StartsWith("$COMMS_entered") || message.StartsWith("$CHAT_Intro")))
                                    {
                                        // We can safely ignore system messages that initialize the chat system or announce that we entered a channel - no event is needed. 
                                        handled = true;
                                        break;
                                    }

                                    if (
                                        channel is "player" or "wing" or "friend" or "voicechat" or "local" or "squadron" or "starsystem" or null
                                    )
                                    {
                                        // Give priority to player messages
                                        if (string.IsNullOrEmpty(channel))
                                        {
                                            // Multicrew messages omit the `channel` property
                                            source = MessageSource.CrewMate;
                                        }
                                        else if (channel == "squadron")
                                        {
                                            source = MessageSource.SquadronMate;
                                        }
                                        else if (channel == "wing")
                                        {
                                            source = MessageSource.WingMate;
                                        }
                                        else
                                        {
                                            source = MessageSource.Commander;
                                        }
                                        messageChannel = MessageChannel.FromEDName(channel ?? "multicrew");
                                        events.Add(new MessageReceivedEvent(timestamp, localizedFrom ?? from, source, true, messageChannel, message) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // This is NPC speech.  What's the source?
                                        if (from.Contains("npc_name_decorate"))
                                        {
                                            source = MessageSource.FromMessage(from, message);
                                            from = from.Replace("$npc_name_decorate:#name=", "").Replace(";", "");
                                        }
                                        else if (from.Contains("ShipName_") || from.Contains("_Scenario_"))
                                        {
                                            source = MessageSource.FromMessage(from, message);
                                            if (!string.IsNullOrEmpty(localizedFrom))
                                            {
                                                // This is an NPC with a symbolic name
                                                from = NpcAuthorityShip.EDNameExists(from) 
                                                    ? NpcAuthorityShip.FromEDName(from)?.localizedName 
                                                    : localizedFrom;
                                            }
                                        }
                                        else if (from.StartsWith("$Name_AX_Military; "))
                                        {
                                            source = MessageSource.FromMessage(from, message);
                                            from = from.Replace("$Name_AX_Military; ", "");
                                        }
                                        else if (message.StartsWith("$STATION_") || message.Contains("$Docking"))
                                        {
                                            source = MessageSource.Station;
                                        }
                                        else
                                        {
                                            source = MessageSource.NPC;
                                        }
                                        messageChannel = MessageChannel.FromEDName(channel);
                                        events.Add(new MessageReceivedEvent(timestamp, localizedFrom ?? from, source, false, messageChannel, JsonParsing.getString(data, "Message_Localised") ) { raw = line, fromLoad = fromLogLoad });

                                        // See if we also want to spawn a specific event as well?
                                        if (message == "$STATION_NoFireZone_entered;")
                                        {
                                            events.Add(new StationNoFireZoneEnteredEvent(timestamp, false) { RequiresShip = true, raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message == "$STATION_NoFireZone_entered_deployed;")
                                        {
                                            events.Add(new StationNoFireZoneEnteredEvent(timestamp, true) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message == "$STATION_NoFireZone_exited;")
                                        {
                                            events.Add(new StationNoFireZoneExitedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_StartInterdiction") || message.Contains("_Hitman_Interdiction"))
                                        {
                                            // Find out who is doing the interdicting
                                            source = MessageSource.FromMessage(from, message);
                                            events.Add(new NPCInterdictionCommencedEvent(timestamp, localizedFrom ?? from, source ) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_Attack") || message.Contains("_OnAttackStart") || message.Contains("AttackRun") || message.Contains("OnDeclarePiracyAttack"))
                                        {
                                            // Find out who is doing the attacking
                                            source = MessageSource.FromMessage(from, message);
                                            events.Add(new NPCAttackCommencedEvent(timestamp, localizedFrom ?? from, source ) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_OnStartScanCargo"))
                                        {
                                            // Find out who is doing the scanning
                                            source = MessageSource.FromMessage(from, message);
                                            events.Add(new NPCCargoScanCommencedEvent(timestamp, localizedFrom ?? from, source ) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "RepairDrone":
                                handled = ShipRepairDroneEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Resurrect":
                                handled = RespawnedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SelfDestruct":
                                handled = SelfDestructEvent.Handle( timestamp, line, ref events, fromLogLoad );
                                break;
                            case "SendText":
                                handled = MessageSentEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "Shutdown":
                                handled = ShutdownEvent.Handle( timestamp, line, ref events, fromLogLoad );
                                break;
                            case "Synthesis":
                                handled = SynthesisedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "SystemsShutdown":
                                handled = ShipShutdownEvent.Handle(timestamp, line, ref events, fromLogLoad);
                                break;
                            case "VehicleSwitch":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var to = JsonParsing.getString(data, "To");
                                    if (to == "Fighter")
                                    {
                                        events.Add(new ControllingFighterEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (to == "Mothership")
                                    {
                                        events.Add(new ControllingShipEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "SupercruiseDestinationDrop":
                                handled = DestinationArrivedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;

                            #endregion

                            #region Colonisation Events
                            
                            case "ColonisationSystemClaim":
                            case "ColonisationSystemClaimRelease":
                                handled = ColonisationClaimProcessedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "ColonisationBeaconDeployed":
                                handled = ColonisationBeaconDeployedEvent.Handle( timestamp, line, ref events, fromLogLoad );
                                break;
                            case "ColonisationContribution": 
                                handled = ColonisationContributionEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ColonisationConstructionDepot":
                                handled = ColonisationConstructionDepotEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;

                            #endregion

                            #region Ignored Events

                            // we silently ignore these, but forward them to the responders

                            // Low priority (for now)
                            case "CarrierTradeOrder": // Implement when we are ready to handle fleet carrier cargo.
                            case "CarrierModulePack": 
                            case "CarrierShipPack":
                            case "ClearImpound": // Sample: { "timestamp":"2022-10-20T18:01:17Z", "event":"ClearImpound", "ShipType":"asp", "ShipType_Localised":"Asp Explorer", "ShipID":34, "ShipMarketID":3705689344, "MarketID":3705689344 }
                            case "CreateSuitLoadout": 
                            case "DeleteSuitLoadout": 
                            case "LoadoutEquipModule":
                            case "LoadoutRemoveModule":
                            case "RenameSuitLoadout":
                            case "ReservoirReplenished":
                            case "RestockVehicle": // Samples: { "timestamp":"2026-06-22T20:54:25Z", "event":"RestockVehicle", "Type":"empire_fighter", "Type_Localised":"Gu-97", "Loadout":"one", "ID":85, "Cost":13400, "Count":1 }, { "timestamp":"2026-06-22T20:54:42Z", "event":"RestockVehicle", "Type":"lander01", "Type_Localised":"Nomad", "Loadout":"base", "ID":85, "Cost":0, "Count":1 }, { "timestamp":"2026-08-29T23:40:26Z", "event":"RestockVehicle", "Type":"mev_rhino", "Type_Localised":"SRV Rhino", "Loadout":"galactic", "ID":145, "Cost":0, "Count":1 }
                            case "SellMicroResources":
                            case "ShipyardBankDeposit": // Written when depositing a ship into a squadron carrier. Sample: { "timestamp": "2025-08-20T02:09:05Z", "event": "ShipyardBankDeposit", "ShipType": "Type9", "ShipType_Localised": "Type-9 Heavy", "MarketID": 3713125120 }
                            case "SuitLoadout":
                            case "SwitchSuitLoadout":
                            case "WingAdd":
                            case "WingInvite":
                            case "WingJoin":
                            case "WingLeave":

                            // No plans to support
                            case "CancelledSquadronApplication": // Unnecessary.
                            case "CollectItems": // The `BackpackChange` event keeps us up to date.
                            case "Continued": // This indicates that the journal continues in a new file. We should pick this up automatically.
                            case "CrimeVictim": // No need to track crimes committed by other cmdrs. If added, filter out events where the current player is listed as the offender.
                            case "DiscoveryScan": // Probably deprecated / replaced by `FSSDiscoveryScan`
                            case "DropItems": // The `BackpackChange` event keeps us up to date.
                            case "EngineerLegacyConvert": // Unnecessary.
                            case "MarketID": // Unnecessary / no obvious use for this event.
                            case "Resupply": // Seems to be related to resupplying Odyssey backpack items
                            case "ScanBaryCentre": // We do not do anything with scanned barycentres at this time (though the raw event is still passed to the EDDN responder)
                            case "Scanned": // Written at the end of a successful scan, too late to react to this.
                            case "SharedBookmarkToSquadron": // Unnecessary.
                            case "ShipyardRedeem": // Unnecessary, all of the necessary information is already given in the `ShipRedeem` event.
                            case "SquadronApplicationApproved": // Unnecessary.
                            case "SquadronApplicationRejected": // Unnecessary.
                            case "TradeMicroResources": // This is always followed by `ShipLocker`, which we can use to keep our inventory up to date
                            case "TransferMicroResources": // Removed, no longer written
                            case "UseConsumable": // Seems to include only medkits and energy cells (grenades not included) and it's not needed. The `BackpackChange` event keeps us up to date.
                            case "USSDrop": // Superseded by / redundant with the `SupercruiseDestinationDrop` event.
                            case "WonATrophyForSquadron": // No interesting data here so no reason to add this.
                                break;

                            #endregion

                            default:
                                throw new NotImplementedException($"EDDI has no handler for event type '{edType}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Something went wrong, but an unhandled event will still be passed to the responders.
                        Logging.Warn($"{ex.Message}/r/nRaw event:/r/n{line}", ex);
                    }

                    if (!handled)
                    {
                        Logging.Debug("Unhandled event: " + line);

                        // Pass a basic event so that responders can react appropriately.
                        // For example, the EDSM responder will handle raw events.
                        events.Add(new UnhandledEvent(timestamp, edType) { raw = line, fromLoad = fromLogLoad });
                    }
                }
            }
            catch (JsonReaderException jre)
            {
                if ( line.Contains( @"""event"":""BackpackChange""" ) && line.Contains( @"] ""Removed""" ) )
                {
                    // We've observed a missing comma in the `BackpackChange` event, fix that here.
                    line = line.Replace( @"] ""Removed""", @"], ""Removed""" );
                    return ParseJournalEntry( line, fromLogLoad );
                }

                // Only log json reader exceptions that we haven't handled above.
                Logging.Error( $"Unable to read from json string: {line}", jre );
            }
            catch (Exception ex)
            {
                Logging.Error($"Exception whilst parsing journal line {line}", ex);
            }
            return events;
        }

        List<Event> IJournalEntryParser.ParseJournalEntry ( string line, bool fromLogLoad )
        {
            return ParseJournalEntry( line, fromLogLoad );
        }

        public string MonitorName()
        {
            return "Journal monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.JournalMonitor.name;
        }

        public string MonitorDescription()
        {
            return Properties.JournalMonitor.desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        public bool NeedsStart()
        {
            return true;
        }

        public void Start()
        {
            start();
        }

        public void Stop()
        {
            stop();
        }

        public void Reload() { }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        public Task PreHandleAsync ( Event @event )
        {
            return Task.CompletedTask;
        }

        public Task PostHandleAsync ( Event @event )
        {
            return Task.CompletedTask;
        }

        public Task HandleProfileAsync(JObject profile)
        {
            return Task.CompletedTask;
        }

        public Task HandleStatusAsync ( Status status )
        {
            return Task.CompletedTask;
        }        
    }
}
