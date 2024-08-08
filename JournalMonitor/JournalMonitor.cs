using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiJournalMonitor
{
    public class JournalMonitor : LogMonitor, IEddiMonitor
    {
        public JournalMonitor () : base( GetSavedGamesDir(), @"^Journal.*\.[0-9\.]+\.log$",
            ( result, isLogLoadEvent ) =>
                ForwardJournalEntries( result.ToList(), EDDI.Instance.enqueueEvent, isLogLoadEvent ) )
        { }

        private static readonly Regex JsonRegex = new Regex( @"^{.*}$", RegexOptions.Singleline );

        /// <summary>
        /// Holds a delayed event until we see an event of the type specified
        /// </summary>
        private static readonly ConcurrentDictionary<string, Event> DelayedEventHolder = new ConcurrentDictionary<string, Event>();

        private enum ShipyardType { [UsedImplicitly] ShipsHere, [UsedImplicitly] ShipsRemote }

        private static readonly Dictionary<long, CancellationTokenSource> carrierJumpCancellationTokenSources =
            new Dictionary<long, CancellationTokenSource>();

        private static CancellationTokenSource ShipShutdownCancellationTokenSource;

        private static void ForwardJournalEntries ( IList<string> lines, Action<Event> callback, bool isLogLoadEventBatch )
        {
            if ( !lines.Any() ) { return; }

            var events = ParseJournalEntries(lines, isLogLoadEventBatch);

            // Append any delayed events
            foreach ( var @event in events.ToList() )
            {
                if ( DelayedEventHolder.TryRemove( @event.type, out var delayedEvent ) )
                {
                    events.Add( delayedEvent );
                }
            }

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
            var discoveryScanEvents = events.OfType<DiscoveryScanEvent>().ToList();
            events = events.Except( discoveryScanEvents ).Concat( discoveryScanEvents ).ToList();

            // Reorder SystemScanComplete to occur after other events in the batch including body and star scans
            var SystemScanCompleteEvents = events.OfType<SystemScanComplete>().ToList();
            events = events.Except( SystemScanCompleteEvents ).Concat( SystemScanCompleteEvents ).ToList();

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
                    if ( ( i + 1 ) <= ( events.Count - 1 ) && events[ i + 1 ] is MaterialCollectedEvent mce && mce.edname is "tg_shutdowndata" )
                    {
                        // If a ShipShutdown event is followed by a material collection event for Massive Energy Surge Analytics
                        // (available from Thargoid Titan energy pulses), ship systems are only momentarily impacted /
                        // flickering for a few seconds. Simulate a partial ship system shutdown.
                        shipShutdownEvent.partialshutdown = true;
                    }
                    else
                    {
                        // Simulate a full ship system shutdown and reboot.
                        // Suppress additional shutdown events during this time.
                        ShipShutdownCancellationTokenSource = new CancellationTokenSource();
                        var startTime = DateTime.UtcNow;
                        Task.Run( async () =>
                        {
                            // The ship reboots about 30 seconds after the shutdown occurs unless canceled early.
                            await Task.Delay( TimeSpan.FromSeconds( 30 ), ShipShutdownCancellationTokenSource.Token );
                            EDDI.Instance.enqueueEvent( new ShipShutdownRebootEvent( shipShutdownEvent.timestamp + ( DateTime.UtcNow - startTime ) ) );
                            ShipShutdownCancellationTokenSource?.Dispose();
                            ShipShutdownCancellationTokenSource = null;
                        } ).ConfigureAwait( false );
                    }
                }
            }

            return events;
        }

        public static List<Event> ParseJournalEntry(string line, bool fromLogLoad = false, bool fromSpeechResponderTest = false )
        {
            List<Event> events = new List<Event>();
            try
            {
                Match match = JsonRegex.Match(line);
                if (match.Success)
                {
                    Logging.Debug($"Received event: {line}");
                    IDictionary<string, object> data = Deserializtion.DeserializeData(line);

                    // Ignore specified log load events
                    if (fromLogLoad && ignoredLogLoadEvents.Contains(JsonParsing.getString(data, "event")))
                    {
                        return events;
                    }

                    // Every event has a timestamp field
                    DateTime timestamp = DateTime.UtcNow;
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
                    string edType = JsonParsing.getString(data, "event");
                    if (edType == "Fileheader")
                    {
                        EDDI.Instance.JournalTimeStamp = DateTime.MinValue;
                    }
                    else
                    {
                        EDDI.Instance.JournalTimeStamp = timestamp;
                    }

                    bool handled = false;
                    try
                    {
                        switch (edType)
                        {
                            case "Docked":
                                {
                                    string systemName = JsonParsing.getString(data, "StarSystem");
                                    ulong systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    long? marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    StationModel stationModel = StationModel.FromEDName(JsonParsing.getString(data, "StationType")) ?? StationModel.None;
                                    Faction controllingfaction = GetFaction(data, "Station", systemName);
                                    decimal? distancefromstar = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

                                    // Get station services data
                                    data.TryGetValue("StationServices", out object val);
                                    List<string> stationservices = (val as List<object>)?.Cast<string>()?.ToList() ?? new List<string>();
                                    List<StationService> stationServices = new List<StationService>();
                                    foreach (string service in stationservices)
                                    {
                                        stationServices.Add(StationService.FromEDName(service));
                                    }

                                    // Get station economies and their shares
                                    data.TryGetValue("StationEconomies", out object val2);
                                    List<object> economies = val2 as List<object> ?? new List<object>();
                                    List<EconomyShare> Economies = new List<EconomyShare>();
                                    foreach (Dictionary<string, object> economyshare in economies)
                                    {
                                        Economy economy = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                                        economy.fallbackLocalizedName = JsonParsing.getString(economyshare, "Name_Localised");
                                        decimal share = JsonParsing.getDecimal(economyshare, "Proportion");
                                        if (economy != Economy.None && share > 0)
                                        {
                                            Economies.Add(new EconomyShare(economy, share));
                                        }
                                    }

                                    bool cockpitBreach = JsonParsing.getOptionalBool(data, "CockpitBreach") ?? false;
                                    bool wanted = JsonParsing.getOptionalBool(data, "Wanted") ?? false;
                                    bool activeFine = JsonParsing.getOptionalBool(data, "ActiveFine") ?? false;

                                    var stationStateEdName = JsonParsing.getString( data, "StationState" );
                                    var stationState = string.IsNullOrEmpty( stationStateEdName )
                                        ? StationState.NormalOperation
                                        : StationState.FromEDName( stationStateEdName );

                                    events.Add(new DockedEvent(timestamp, systemName, systemAddress, marketId, stationName, stationState, stationModel, controllingfaction, Economies, distancefromstar, stationServices, cockpitBreach, wanted, activeFine) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Undocked":
                                {
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    long? marketId = JsonParsing.getLong(data, "MarketID");
                                    events.Add(new UndockedEvent(timestamp, stationName, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Touchdown":
                                {
                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var onStation = JsonParsing.getOptionalBool(data, "OnStation");
                                    var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");
                                    var playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;

                                    var taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");

                                    // The nearest destination may be a specific destination name or a generic signal source.
                                    // Per the journal manual, the NearestDestination is included if within 50km of a location listed in the nav panel
                                    var nearestdestination = JsonParsing.getString(data, "NearestDestination");
                                    var nearestDestination = SignalSource.FromEDName(nearestdestination) ?? new SignalSource();
                                    var localizedName = JsonParsing.getString(data, "SignalName_Localised");
                                    if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                                    {
                                        nearestDestination.fallbackLocalizedName = localizedName;
                                    }
                                    events.Add(new TouchdownEvent(timestamp, longitude, latitude, system, systemAddress, body, bodyId, onStation, onPlanet, taxi, multicrew, playercontrolled, nearestDestination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Liftoff":
                                {
                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var onStation = JsonParsing.getOptionalBool(data, "OnStation");
                                    var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");
                                    var playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;

                                    var taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");

                                    // The nearest destination may be a specific destination name or a generic signal source.
                                    // Per the journal manual, the NearestDestination is included if within 50km of a location listed in the nav panel
                                    var nearestdestination = JsonParsing.getString(data, "NearestDestination");
                                    var nearestDestination = SignalSource.FromEDName(nearestdestination) ?? new SignalSource();
                                    var localizedName = JsonParsing.getString(data, "SignalName_Localised");
                                    if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                                    {
                                        nearestDestination.fallbackLocalizedName = localizedName;
                                    }
                                    events.Add(new LiftoffEvent(timestamp, longitude, latitude, system, systemAddress, body, bodyId, onStation, onPlanet, taxi, multicrew, playercontrolled, nearestDestination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SupercruiseEntry":
                                {
                                    string system = JsonParsing.getString(data, "StarySystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    bool? taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    bool? multicrew = JsonParsing.getOptionalBool(data, "Multicrew");
                                    events.Add(new EnteredSupercruiseEvent(timestamp, system, systemAddress, taxi, multicrew) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SupercruiseExit":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType")) ?? BodyType.None;
                                    if ( bodyType == BodyType.Planet )
                                    {
                                        bodyType = EDDI.Instance.CurrentStarSystem?.bodies.FirstOrDefault( b => b.bodyId != null && b.bodyId == bodyId )?.bodyType ??
                                                   bodyType;
                                    }
                                    bool? taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    bool? multicrew = JsonParsing.getOptionalBool(data, "Multicrew");
                                    events.Add(new EnteredNormalSpaceEvent(timestamp, system, systemAddress, body, bodyId, bodyType, taxi, multicrew) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSDJump":
                                {
                                    string systemName = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    data.TryGetValue("StarPos", out object val);
                                    List<object> starPos = (List<object>)val;
                                    decimal x = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                    decimal y = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                    decimal z = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;
                                    string starName = JsonParsing.getString(data, "Body"); // Documented by the journal, but apparently never written. We can't rely on this being set.
                                    decimal fuelUsed = JsonParsing.getDecimal(data, "FuelUsed");
                                    decimal fuelRemaining = JsonParsing.getDecimal(data, "FuelLevel");
                                    int? boostUsed = JsonParsing.getOptionalInt(data, "BoostUsed"); // 1-3 are synthesis, 4 is any supercharge (white dwarf or neutron star)
                                    decimal distance = JsonParsing.getDecimal(data, "JumpDist");
                                    Faction controllingfaction = GetFaction(data, "System", systemName);
                                    Economy economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy")) ?? Economy.None;
                                    Economy economy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy")) ?? Economy.None;
                                    SecurityLevel security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity")) ?? SecurityLevel.None;
                                    long? population = JsonParsing.getOptionalLong(data, "Population");

                                    // Parse factions array data
                                    List<Faction> factions = new List<Faction>();
                                    data.TryGetValue("Factions", out object factionsVal);
                                    if (factionsVal != null)
                                    {
                                        factions = GetFactions(factionsVal, systemName);
                                    }

                                    // Parse conflicts array data
                                    List<Conflict> conflicts = new List<Conflict>();
                                    data.TryGetValue("Conflicts", out object conflictsVal);
                                    if (conflictsVal != null)
                                    {
                                        conflicts = GetConflicts(conflictsVal, factions);
                                    }

                                    // Powerplay data (if pledged)
                                    GetPowerplayData(data, out List<Power> powerplayPowers, out PowerplayState powerplayState);

                                    // Thargoid war data (if any)
                                    GetThargoidWarData( data, out ThargoidWar thargoidWar );

                                    bool? taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    bool? multicrew = JsonParsing.getOptionalBool(data, "Multicrew");

                                    events.Add(new JumpedEvent(timestamp, systemName, systemAddress, x, y, z, starName, distance, fuelUsed, fuelRemaining, boostUsed, controllingfaction, factions, conflicts, economy, economy2, security, population, powerplayPowers, powerplayState, taxi, multicrew, thargoidWar) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Location":
                                {
                                    string systemName = JsonParsing.getString(data, "StarSystem");

                                    if (systemName == "Training")
                                    {
                                        // Training system; ignore
                                        break;
                                    }

                                    data.TryGetValue("StarPos", out object val);
                                    List<object> starPos = (List<object>)val;
                                    decimal x = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                    decimal y = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                    decimal z = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    decimal? distFromStarLs = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

                                    string body = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType")) ?? BodyType.None;
                                    if ( bodyType == BodyType.Planet )
                                    {
                                        bodyType = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem( systemName )?
                                                       .bodies.FirstOrDefault( b => b.bodyId != null && b.bodyId == bodyId )?.bodyType ??
                                                   bodyType;
                                    }

                                    bool docked = JsonParsing.getBool(data, "Docked");
                                    Faction systemfaction = GetFaction(data, "System", systemName);
                                    Faction stationfaction = GetFaction(data, "Station", systemName);
                                    Economy economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy"));
                                    Economy economy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy"));
                                    SecurityLevel security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity"));
                                    long? population = JsonParsing.getOptionalLong(data, "Population");

                                    // If docked
                                    string station = JsonParsing.getString(data, "StationName");
                                    StationModel stationtype = StationModel.FromEDName(JsonParsing.getString(data, "StationType"));
                                    long? marketId = JsonParsing.getOptionalLong(data, "MarketID");

                                    // Get station services data
                                    data.TryGetValue("StationServices", out val);
                                    List<string> stationservices = (val as List<object>)?.Cast<string>()?.ToList() ?? new List<string>();
                                    List<StationService> stationServices = new List<StationService>();
                                    foreach (string service in stationservices)
                                    {
                                        stationServices.Add(StationService.FromEDName(service));
                                    }

                                    // Get station economies and their shares
                                    data.TryGetValue("StationEconomies", out object val2);
                                    List<object> economies = val2 as List<object> ?? new List<object>();
                                    List<EconomyShare> Economies = new List<EconomyShare>();
                                    foreach (Dictionary<string, object> economyshare in economies)
                                    {
                                        var economyShare = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                                        economyShare.fallbackLocalizedName = JsonParsing.getString(economyshare, "Name_Localised");
                                        decimal share = JsonParsing.getDecimal(economyshare, "Proportion");
                                        if (economyShare != Economy.None && share > 0)
                                        {
                                            Economies.Add(new EconomyShare(economyShare, share));
                                        }
                                    }

                                    // If landed
                                    decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    // Parse factions array data
                                    List<Faction> factions = new List<Faction>();
                                    data.TryGetValue("Factions", out object factionsVal);
                                    if (factionsVal != null)
                                    {
                                        factions = GetFactions(factionsVal, systemName);
                                    }

                                    // Parse conflicts array data
                                    List<Conflict> conflicts = new List<Conflict>();
                                    data.TryGetValue("Conflicts", out object conflictsVal);
                                    if (conflictsVal != null)
                                    {
                                        conflicts = GetConflicts(conflictsVal, factions);
                                    }

                                    // Powerplay data (if pledged)
                                    GetPowerplayData( data, out List<Power> powerplayPowers, out PowerplayState powerplayState );

                                    bool taxi = JsonParsing.getOptionalBool(data, "Taxi") ?? false;
                                    bool multicrew = JsonParsing.getOptionalBool(data, "Multicrew") ?? false;
                                    bool inSRV = JsonParsing.getOptionalBool(data, "InSRV") ?? false;
                                    bool onFoot = JsonParsing.getOptionalBool(data, "OnFoot") ?? false;

                                    // Thargoid war data (if any)
                                    GetThargoidWarData( data, out ThargoidWar thargoidWar );

                                    // There is a bug in Odyssey where a `Location` event may be written instead of a `CarrierJump` event.
                                    // Per Journal Manual v37, this should be fixed in Odyssey Update 15.
                                    if (docked && carrierJumpCancellationTokenSources.ContainsKey(marketId ?? 0))
                                    {
                                        events.Add(new CarrierJumpedEvent(timestamp, systemName, systemAddress, x, y, z, body, bodyId, bodyType, docked, onFoot, station, stationtype, marketId, stationServices, systemfaction, stationfaction, factions, conflicts, Economies, economy, economy2, security, population, powerplayPowers, powerplayState, thargoidWar ) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        events.Add(new LocationEvent(timestamp, systemName, systemAddress, x, y, z, distFromStarLs, body, bodyId, bodyType, longitude, latitude, docked, station, stationtype, marketId, stationServices, systemfaction, stationfaction, factions, conflicts, Economies, economy, economy2, security, population, powerplayPowers, powerplayState, taxi, multicrew, inSRV, onFoot, thargoidWar ) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "Bounty":
                                {
                                    var target = JsonParsing.getString(data, "Target");
                                    var target_localised = JsonParsing.getString( data, "Target_Localised" );
                                    var victimName = JsonParsing.getString(data, "PilotName_Localised");
                                    var victimFaction = GetFactionName(data, "VictimFaction");

                                    data.TryGetValue("SharedWithOthers", out object val);
                                    var shared = val != null && (long)val == 1;

                                    long reward;
                                    List<Reward> rewards = new List<Reward>();

                                    if (data.ContainsKey("Reward"))
                                    {
                                        // Old-style
                                        data.TryGetValue("Reward", out val);
                                        reward = (long)val;
                                        if (reward == 0)
                                        {
                                            // 0-credit reward; ignore
                                            break;
                                        }
                                        string factionName = GetFactionName(data, "Faction");
                                        rewards.Add(new Reward(factionName, reward));
                                    }
                                    else
                                    {
                                        data.TryGetValue("TotalReward", out val);
                                        reward = (long)val;
                                        if (reward == 0)
                                        {
                                            // 0-credit reward; ignore
                                            break;
                                        }
                                        // Obtain list of rewards
                                        data.TryGetValue("Rewards", out val);
                                        List<object> rewardsData = (List<object>)val;
                                        if (rewardsData != null)
                                        {
                                            foreach (Dictionary<string, object> rewardData in rewardsData)
                                            {
                                                string factionName = GetFactionName(rewardData, "Faction");
                                                rewardData.TryGetValue("Reward", out val);
                                                long factionReward = (long)val;

                                                rewards.Add(new Reward(factionName, factionReward));
                                            }
                                        }
                                    }

                                    events.Add(new BountyAwardedEvent(timestamp, target, target_localised, victimName, victimFaction, reward, rewards, shared) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CapShipBond":
                            case "DatalinkVoucher":
                            case "FactionKillBond":
                                {
                                    data.TryGetValue("Reward", out object val);
                                    long reward = (long)val;
                                    string victimFaction = GetFactionName(data, "VictimFaction");

                                    if (data.ContainsKey("AwardingFaction"))
                                    {
                                        string awardingFaction = GetFactionName(data, "AwardingFaction");
                                        events.Add(new BondAwardedEvent(timestamp, awardingFaction, victimFaction, reward) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (data.ContainsKey("PayeeFaction"))
                                    {
                                        string payeeFaction = GetFactionName(data, "PayeeFaction");
                                        events.Add(new DataVoucherAwardedEvent(timestamp, payeeFaction, victimFaction, reward) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "CommitCrime":
                                {
                                    object val;
                                    string crimetype = JsonParsing.getString(data, "CrimeType");
                                    string faction = GetFactionName(data, "Faction");
                                    string victim = JsonParsing.getString(data, "Victim");

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
                                        long fine = (long)val;
                                        events.Add(new FineIncurredEvent(timestamp, crimetype, faction, victim, fine) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        data.TryGetValue("Bounty", out val);
                                        long bounty = (long)val;
                                        events.Add(new BountyIncurredEvent(timestamp, crimetype, faction, victim, bounty) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "Promotion":
                                {
                                    object rating = null;
                                    if (data.TryGetValue("Combat", out object val))
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
                                        events.Add(new CommanderPromotionEvent(timestamp, rating, EDDI.Instance.Cmdr.gender) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "CollectCargo":
                                {
                                    string commodityName = JsonParsing.getString(data, "Type");
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    long? missionid = JsonParsing.getOptionalLong(data, "MissionID");
                                    bool stolen = JsonParsing.getBool(data, "Stolen");
                                    events.Add(new CommodityCollectedEvent(timestamp, commodity, missionid, stolen) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EjectCargo":
                                {
                                    string commodityName = JsonParsing.getString(data, "Type");
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    long? missionid = JsonParsing.getOptionalLong(data, "MissionID");
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;
                                    bool abandoned = JsonParsing.getBool(data, "Abandoned");
                                    events.Add(new CommodityEjectedEvent(timestamp, commodity, amount, missionid, abandoned) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Loadout":
                                {
                                    data.TryGetValue("ShipID", out object val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");
                                    var shipName = JsonParsing.getString(data, "ShipName");
                                    var shipIdent = JsonParsing.getString(data, "ShipIdent");

                                    var hullValue = JsonParsing.getOptionalLong(data, "HullValue");
                                    var modulesValue = JsonParsing.getOptionalLong(data, "ModulesValue");
                                    var hullHealth = sensibleHealth((JsonParsing.getOptionalDecimal(data, "HullHealth") ?? 1) * 100);
                                    var unladenMass = JsonParsing.getOptionalDecimal(data, "UnladenMass") ?? 0;
                                    var maxJumpRange = JsonParsing.getOptionalDecimal(data, "MaxJumpRange") ?? 0;

                                    var rebuy = JsonParsing.getLong(data, "Rebuy");

                                    // If ship is 'hot', then modules are also 'hot'
                                    var hot = JsonParsing.getOptionalBool(data, "Hot") ?? false;

                                    data.TryGetValue("Modules", out val);
                                    var modulesData = (List<object>)val;

                                    string paintjob = null;
                                    var hardpoints = new List<Hardpoint>();
                                    var compartments = new List<Compartment>();
                                    if (modulesData != null)
                                    {
                                        foreach (Dictionary<string, object> moduleData in modulesData)
                                        {
                                            // Common items
                                            var slot = JsonParsing.getString(moduleData, "Slot");
                                            var item = JsonParsing.getString(moduleData, "Item");
                                            var enabled = JsonParsing.getBool(moduleData, "On");
                                            var priority = JsonParsing.getInt(moduleData, "Priority");
                                            // Health is as 0->1 but we want 0->100, and to a sensible number of decimal places
                                            var health = JsonParsing.getDecimal(moduleData, "Health") * 100;
                                            if (health < 5)
                                            {
                                                health = Math.Round(health, 1);
                                            }
                                            else
                                            {
                                                health = Math.Round(health);
                                            }

                                            // Some built-in modules don't give "Value" keys in the Loadout event. We'll set them to zero to match the Frontier API.
                                            var price = JsonParsing.getOptionalLong(moduleData, "Value") ?? 0;

                                            // Ammunition
                                            var clip = JsonParsing.getOptionalInt(moduleData, "AmmoInClip");
                                            var hopper = JsonParsing.getOptionalInt(moduleData, "AmmoInHopper");

                                            // Engineering modifications
                                            moduleData.TryGetValue("Engineering", out object engineeringVal);
                                            var modified = engineeringVal != null;
                                            var engineeringData = (Dictionary<string, object>)engineeringVal;
                                            var blueprint = modified ? JsonParsing.getString(engineeringData, "BlueprintName") : null;
                                            var blueprintId = modified ? JsonParsing.getLong(engineeringData, "BlueprintID") : 0;
                                            var level = modified ? JsonParsing.getInt(engineeringData, "Level") : 0;
                                            var modification = Blueprint.FromEliteID(blueprintId, engineeringData)
                                                ?? Blueprint.FromEDNameAndGrade(blueprint, level) ?? Blueprint.None;
                                            var quality = modified ? JsonParsing.getDecimal(engineeringData, "Quality") : 0;
                                            var experimentalEffect = modified ? JsonParsing.getString(engineeringData, "ExperimentalEffect") : null;
                                            var modifiers = new List<EngineeringModifier>();
                                            if (modified)
                                            {
                                                engineeringData.TryGetValue("Modifiers", out object modifiersVal);
                                                var modifiersData = (List<object>)modifiersVal;
                                                foreach (Dictionary<string, object> modifier in modifiersData)
                                                {
                                                    try
                                                    {
                                                        var edname = JsonParsing.getString(modifier, "Label");
                                                        var currentValue = JsonParsing.getOptionalDecimal(modifier, "Value");
                                                        var originalValue = JsonParsing.getOptionalDecimal(modifier, "OriginalValue");
                                                        var lessIsGood = JsonParsing.getOptionalInt(modifier, "LessIsGood") == 1;
                                                        var valueStr = JsonParsing.getString(modifier, "ValueStr");
                                                        modifiers.Add(new EngineeringModifier
                                                        {
                                                            EDName = edname,
                                                            currentValue = currentValue,
                                                            originalValue = originalValue,
                                                            lessIsGood = lessIsGood,
                                                            valueStr = valueStr
                                                        });
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Logging.Error($"Failed to parse engineering modification for item {JsonConvert.SerializeObject(item)}", e);
                                                    }
                                                }
                                            }
                                            if (slot.Contains("Hardpoint"))
                                            {
                                                // This is a hardpoint
                                                Hardpoint hardpoint = new Hardpoint() { name = slot };
                                                if (hardpoint.name.StartsWith("Tiny"))
                                                {
                                                    hardpoint.size = 0;
                                                }
                                                else if (hardpoint.name.StartsWith("Small", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    hardpoint.size = 1;
                                                }
                                                else if (hardpoint.name.StartsWith("Medium", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    hardpoint.size = 2;
                                                }
                                                else if (hardpoint.name.StartsWith("Large", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    hardpoint.size = 3;
                                                }
                                                else if (hardpoint.name.StartsWith("Huge", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    hardpoint.size = 4;
                                                }

                                                Module module = new Module(Module.FromEDName(item, moduleData) ?? new Module());
                                                if (module.edname == null)
                                                {
                                                    Logging.Info("Unknown module " + item, JsonConvert.SerializeObject(moduleData));
                                                }
                                                else
                                                {
                                                    module.hot = hot;
                                                    module.enabled = enabled;
                                                    module.priority = priority;
                                                    module.health = health;
                                                    module.price = price;
                                                    module.ammoinclip = clip;
                                                    module.ammoinhopper = hopper;
                                                    module.modified = modified;
                                                    module.modificationEDName = blueprint;
                                                    module.engineermodification = modification;
                                                    module.engineerlevel = level;
                                                    module.engineerquality = quality;
                                                    module.engineerExperimentalEffectEDName = experimentalEffect;
                                                    module.modifiers = modifiers;
                                                    hardpoint.module = module;
                                                    hardpoints.Add(hardpoint);
                                                }
                                            }
                                            else if (slot.Equals("PaintJob", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // This is a paintjob
                                                paintjob = item;
                                            }
                                            else if (slot.Equals("PlanetaryApproachSuite", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore planetary approach suite for now
                                            }
                                            else if (slot.StartsWith("Bobble", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore bobbles
                                            }
                                            else if (slot.StartsWith("Decal", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore decals
                                            }
                                            else if (slot.StartsWith("StringLights", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore string lights
                                            }
                                            else if (slot.Equals("WeaponColour", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore weapon colour
                                            }
                                            else if (slot.Equals("EngineColour", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore engine colour
                                            }
                                            else if (slot.StartsWith("ShipKit", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore ship kits
                                            }
                                            else if (slot.StartsWith("ShipName", StringComparison.InvariantCultureIgnoreCase) || slot.StartsWith("ShipID", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore nameplates
                                            }
                                            else if (slot.Equals("VesselVoice", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore the chosen voice
                                            }
                                            else if (slot.Equals("DataLinkScanner", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore the data link scanner
                                            }
                                            else if (slot.Equals("CodexScanner", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore the codex scanner
                                            }
                                            else
                                            {
                                                // This is a compartment
                                                Compartment compartment = parseShipCompartment(ship, slot);
                                                // Compartment slots are in the form of "Slotnn_Sizen" or "Militarynn"

                                                Module module = new Module(Module.FromEDName(item, moduleData) ?? new Module());
                                                if (module.edname == null)
                                                {
                                                    Logging.Info("Unknown module " + item, JsonConvert.SerializeObject(moduleData));
                                                }
                                                else
                                                {
                                                    module.hot = hot;
                                                    module.enabled = enabled;
                                                    module.priority = priority;
                                                    module.health = health;
                                                    module.price = price;
                                                    module.ammoinclip = clip;
                                                    module.ammoinhopper = hopper;
                                                    module.modified = modified;
                                                    module.modificationEDName = blueprint;
                                                    module.engineermodification = modification;
                                                    module.engineerlevel = level;
                                                    module.engineerquality = quality;
                                                    module.engineerExperimentalEffectEDName = experimentalEffect;
                                                    module.modifiers = modifiers;
                                                    compartment.module = module;
                                                    compartments.Add(compartment);
                                                }
                                            }
                                        }
                                    }
                                    events.Add(new ShipLoadoutEvent(timestamp, ship, shipId, shipName, shipIdent, hullValue, modulesValue, hullHealth, unladenMass, maxJumpRange, rebuy, hot, compartments, hardpoints, paintjob) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleInfo":
                                {
                                    if (ModuleInfo.TryFromFile(timestamp, out var info, out line))
                                    {
                                        events.Add(new ModuleInfoEvent(timestamp, info.Modules)
                                            { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "CockpitBreached":
                                {
                                    events.Add(new CockpitBreachedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ApproachBody":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    events.Add(new NearSurfaceEvent(timestamp, true, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LeaveBody":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    events.Add(new NearSurfaceEvent(timestamp, false, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ApproachSettlement":
                                {
                                    // The settlement name may be a proper name or a symbolic name.
                                    var settlementName = JsonParsing.getString(data, "Name").ReplaceEnd('+');

                                    // Symbolic names may include a localized name which may have an number appended to it (e.g. "Ancient Ruins (3)".
                                    // If so, remove the appended number.
                                    var localizedName = JsonParsing.getString(data, "Name_Localised");
                                    if ( !string.IsNullOrEmpty(localizedName) )
                                    {
                                        localizedName = Regex.Replace( localizedName, @"\s\(\d\)", "" ).Trim();
                                    }

                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID"); // Tourist beacons and guardian structures are reported as settlements without MarketID
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var bodyName = JsonParsing.getString(data, "BodyName");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");

                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    var controllingFaction = GetFaction(data, "Station", EDDI.Instance.CurrentStarSystem?.systemname);

                                    // Get station services data
                                    var stationServices = new List<StationService>();
                                    if ( data.TryGetValue( "StationServices", out var val ) )
                                    {
                                        stationServices = ( val as List<object> )
                                            .Cast<string>()
                                            .Select( StationService.FromEDName )
                                            .ToList();
                                    }

                                    // Get station economies and their shares
                                    var Economies = new List<EconomyShare>();
                                    if ( data.TryGetValue( "StationEconomies", out var val2 ) )
                                    {
                                        var economies = ( val2 as List<object> )
                                            .Cast<Dictionary<string, object>>();
                                        foreach ( var economyshare in economies )
                                        {
                                            var economy = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                                            economy.fallbackLocalizedName = JsonParsing.getString( economyshare, "Name_Localised" );
                                            var share = JsonParsing.getDecimal(economyshare, "Proportion");
                                            if ( economy != Economy.None && share > 0 )
                                            {
                                                Economies.Add( new EconomyShare( economy, share ) );
                                            }
                                        }
                                    }

                                    events.Add(new SettlementApproachedEvent(timestamp, settlementName, localizedName, marketId, controllingFaction, stationServices, Economies, systemAddress, bodyName, bodyId, latitude, longitude) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Scan":
                                {
                                    string name = JsonParsing.getString(data, "BodyName");
                                    string scantype = JsonParsing.getString(data, "ScanType");

                                    string systemName = JsonParsing.getString(data, "StarSystem");
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
                                    decimal distanceLs = JsonParsing.getDecimal(data, "DistanceFromArrivalLS");
                                    // Need to convert radius from meters (per journal) to kilometers
                                    decimal radiusKm = JsonParsing.getDecimal(data, "Radius") / 1000;
                                    // Need to convert orbital period from seconds (per journal) to days
                                    decimal? orbitalPeriodDays = ConstantConverters.seconds2days(JsonParsing.getOptionalDecimal(data, "OrbitalPeriod"));
                                    // Need to convert rotation period from seconds (per journal) to days
                                    decimal? rotationPeriodDays = ConstantConverters.seconds2days(JsonParsing.getOptionalDecimal(data, "RotationPeriod"));
                                    // Need to convert meters to light seconds
                                    decimal? semimajoraxisLs = ConstantConverters.meters2ls(JsonParsing.getOptionalDecimal(data, "SemiMajorAxis"));
                                    decimal? eccentricity = JsonParsing.getOptionalDecimal(data, "Eccentricity");
                                    decimal? orbitalinclinationDegrees = JsonParsing.getOptionalDecimal(data, "OrbitalInclination");
                                    decimal? periapsisDegrees = JsonParsing.getOptionalDecimal(data, "Periapsis");
                                    decimal? axialTiltDegrees = JsonParsing.getOptionalDecimal(data, "AxialTilt");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    decimal? temperatureKelvin = JsonParsing.getOptionalDecimal(data, "SurfaceTemperature");

                                    // Parent body types and IDs
                                    data.TryGetValue("Parents", out object parentsVal);
                                    List<IDictionary<string, object>> parents = new List<IDictionary<string, object>>();
                                    if (parentsVal != null)
                                    {
                                        foreach (IDictionary<string, object> parent in (List<object>)parentsVal)
                                        {
                                            parents.Add(parent);
                                        }
                                    }

                                    // Scan status
                                    bool? alreadydiscovered = scantype == "NavBeaconDetail" ? true : JsonParsing.getOptionalBool(data, "WasDiscovered");
                                    bool? alreadymapped = JsonParsing.getOptionalBool(data, "WasMapped");

                                    // Rings
                                    data.TryGetValue("Rings", out object val);
                                    List<object> ringsData = (List<object>)val;
                                    List<Ring> rings = new List<Ring>();
                                    if (ringsData != null)
                                    {
                                        foreach (Dictionary<string, object> ringData in ringsData)
                                        {
                                            string ringName = JsonParsing.getString(ringData, "Name");
                                            RingComposition ringComposition = RingComposition.FromEDName(JsonParsing.getString(ringData, "RingClass"));
                                            decimal ringMassMegaTons = JsonParsing.getDecimal(ringData, "MassMT");
                                            decimal ringInnerRadiusKm = JsonParsing.getDecimal(ringData, "InnerRad") / 1000;
                                            decimal ringOuterRadiusKm = JsonParsing.getDecimal(ringData, "OuterRad") / 1000;

                                            rings.Add(new Ring(ringName, ringComposition, ringMassMegaTons, ringInnerRadiusKm, ringOuterRadiusKm));
                                        }
                                    }

                                    if (data.ContainsKey("StarType"))
                                    {
                                        // Star
                                        string stellarclass = JsonParsing.getString(data, "StarType");
                                        int? stellarsubclass = JsonParsing.getOptionalInt(data, "Subclass");
                                        decimal stellarMass = JsonParsing.getDecimal(data, "StellarMass");
                                        decimal absoluteMagnitude = JsonParsing.getDecimal(data, "AbsoluteMagnitude");
                                        string luminosityClass = JsonParsing.getString(data, "Luminosity");
                                        data.TryGetValue("Age_MY", out val);
                                        long ageMegaYears = (long)val;

                                        Body star = new Body(name, bodyId, systemName, systemAddress, parents, distanceLs, stellarclass, stellarsubclass, stellarMass, radiusKm, absoluteMagnitude, ageMegaYears, temperatureKelvin, luminosityClass, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, alreadydiscovered, alreadymapped)
                                        {
                                            scannedDateTime = (DateTime?)timestamp
                                        };

                                        events.Add(new StarScannedEvent(timestamp, scantype, star) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (data.ContainsKey("PlanetClass"))
                                    {
                                        // Body
                                        bool? tidallyLocked = JsonParsing.getOptionalBool(data, "TidalLock") ?? false;

                                        PlanetClass planetClass = PlanetClass.FromEDName(JsonParsing.getString(data, "PlanetClass")) ?? PlanetClass.None;
                                        decimal? earthMass = JsonParsing.getOptionalDecimal(data, "MassEM");

                                        // MKW: Gravity in the Journal is in m/s; must convert it to G
                                        decimal gravity = ConstantConverters.ms2g(JsonParsing.getDecimal(data, "SurfaceGravity"));

                                        decimal? pressureAtm = ConstantConverters.pascals2atm(JsonParsing.getOptionalDecimal(data, "SurfacePressure"));

                                        bool? landable = JsonParsing.getOptionalBool(data, "Landable") ?? false;

                                        ReserveLevel reserveLevel = ReserveLevel.FromEDName(JsonParsing.getString(data, "ReserveLevel"));

                                        // The "Atmosphere" is most accurately described through the "AtmosphereType" and "AtmosphereComposition" 
                                        // properties, so we use them in preference to "Atmosphere"

                                        // Gas giants may receive an empty string in place of an atmosphere class string. Fix it, since gas giants definitely have atmospheres. 
                                        AtmosphereClass atmosphereClass = planetClass.invariantName.Contains("gas giant") && JsonParsing.getString(data, "AtmosphereType") == string.Empty
                                            ? AtmosphereClass.FromEDName("GasGiant")
                                            : AtmosphereClass.FromEDName(JsonParsing.getString(data, "AtmosphereType")) ?? AtmosphereClass.None;

                                        data.TryGetValue("AtmosphereComposition", out val);
                                        List<AtmosphereComposition> atmosphereCompositions = new List<AtmosphereComposition>();
                                        if (val != null)
                                        {
                                            if (val is List<object> atmosJson)
                                            {
                                                foreach (Dictionary<string, object> atmoJson in atmosJson)
                                                {
                                                    string edComposition = JsonParsing.getString(atmoJson, "Name");
                                                    decimal? percent = JsonParsing.getOptionalDecimal(atmoJson, "Percent");
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
                                        }

                                        data.TryGetValue("Composition", out val);
                                        List<SolidComposition> solidCompositions = new List<SolidComposition>();
                                        if (val != null)
                                        {
                                            if (val is Dictionary<string, object> bodyCompsJson)
                                            {
                                                IDictionary<string, object> compositionData = (IDictionary<string, object>)val;
                                                foreach (KeyValuePair<string, object> kv in compositionData)
                                                {
                                                    string edComposition = kv.Key;
                                                    // The journal gives solid composition as a fraction of 1. Multiply by 100 to convert to a true percentage.
                                                    decimal percent = ((decimal)(double)kv.Value) * 100;
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
                                        }

                                        data.TryGetValue("Materials", out val);
                                        List<MaterialPresence> materials = new List<MaterialPresence>();
                                        if (val != null)
                                        {
                                            if (val is Dictionary<string, object>)
                                            {
                                                // 2.2 style
                                                IDictionary<string, object> materialsData = (IDictionary<string, object>)val;
                                                foreach (KeyValuePair<string, object> kv in materialsData)
                                                {
                                                    Material material = Material.FromEDName(kv.Key);
                                                    if (material != null)
                                                    {
                                                        materials.Add(new MaterialPresence(material, JsonParsing.getDecimal("Amount", kv.Value)));
                                                    }
                                                }
                                            }
                                            else if (val is List<object> materialsJson) // 2.3 style
                                            {
                                                foreach (Dictionary<string, object> materialJson in materialsJson)
                                                {
                                                    Material material = Material.FromEDName((string)materialJson["Name"]);
                                                    materials.Add(new MaterialPresence(material, JsonParsing.getDecimal(materialJson, "Percent")));
                                                }
                                            }
                                        }

                                        TerraformState terraformState = TerraformState.FromEDName(JsonParsing.getString(data, "TerraformState")) ?? TerraformState.NotTerraformable;
                                        Volcanism volcanism = Volcanism.FromName(JsonParsing.getString(data, "Volcanism"));

                                        Body body = new Body(name, bodyId, systemName, systemAddress, parents, distanceLs, tidallyLocked, terraformState, planetClass, atmosphereClass, atmosphereCompositions, volcanism, earthMass, radiusKm, gravity, temperatureKelvin, pressureAtm, landable, materials, solidCompositions, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, reserveLevel, alreadydiscovered, alreadymapped)
                                        {
                                            scannedDateTime = (DateTime?)timestamp
                                        };

                                        events.Add(new BodyScannedEvent(timestamp, scantype, body) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "DatalinkScan":
                                {
                                    string message = JsonParsing.getString(data, "Message");
                                    events.Add(new DatalinkMessageEvent(timestamp, message) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DataScanned":
                                {
                                    DataScan datalinktype = DataScan.FromEDName(JsonParsing.getString(data, "Type"));
                                    events.Add(new DataScannedEvent(timestamp, datalinktype) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Shipyard":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string station = JsonParsing.getString(data, "StationName");
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    if (ShipyardInfo.TryFromFile(timestamp, system, station, marketId, out var info, out var raw))
                                    {
                                        events.Add(new ShipyardEvent(timestamp, marketId, station, system, info) { raw = raw, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "ShipyardBuy":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");

                                    // We don't have a ship ID at this point so use the ship type
                                    string ship = JsonParsing.getString(data, "ShipType");

                                    data.TryGetValue("ShipPrice", out object val);
                                    long price = (long)val;

                                    data.TryGetValue("StoreShipID", out val);
                                    int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                    string storedShip = JsonParsing.getString(data, "StoreOldShip");

                                    data.TryGetValue("SellShipID", out val);
                                    int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                    string soldShip = JsonParsing.getString(data, "SellOldShip");

                                    data.TryGetValue("SellPrice", out val);
                                    long? soldPrice = (long?)val;
                                    events.Add(new ShipPurchasedEvent(timestamp, ship, price, soldShip, soldShipId, soldPrice, storedShip, storedShipId, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardNew":
                            case "ShipRedeemed":
                                {
                                    data.TryGetValue("NewShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "ShipType");

                                    events.Add(new ShipDeliveredEvent(timestamp, ship, shipId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardSell":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("SellShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "ShipType");
                                    data.TryGetValue("ShipPrice", out val);
                                    long price = (long)val;
                                    string system = JsonParsing.getString(data, "System"); // Only written when the ship is in a different star system
                                    events.Add(new ShipSoldEvent(timestamp, ship, shipId, price, system, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SellShipOnRebuy":
                                {
                                    data.TryGetValue("SellShipId", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "ShipType");
                                    data.TryGetValue("ShipPrice", out val);
                                    long price = (long)val;
                                    string system = JsonParsing.getString(data, "System");
                                    events.Add(new ShipSoldOnRebuyEvent(timestamp, ship, shipId, price, system) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardSwap":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "ShipType");

                                    data.TryGetValue("StoreShipID", out val);
                                    int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                    string storedShip = JsonParsing.getString(data, "StoreOldShip");

                                    data.TryGetValue("SellShipID", out val);
                                    int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                    string soldShip = JsonParsing.getString(data, "SellOldShip");

                                    events.Add(new ShipSwappedEvent(timestamp, ship, shipId, soldShip, soldShipId, storedShip, storedShipId, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "StoredShips":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    string station = JsonParsing.getString(data, "StationName");

                                    List<Ship> shipyard = new List<Ship>();
                                    foreach (var type in Enum.GetNames(typeof(ShipyardType)))
                                    {
                                        data.TryGetValue(type, out object val);
                                        List<object> shipsData = (List<object>)val;
                                        if (shipsData != null)
                                        {
                                            foreach (Dictionary<string, object> shipData in shipsData)
                                            {
                                                string shipType = JsonParsing.getString(shipData, "ShipType");
                                                Ship ship = ShipDefinitions.FromEDModel(shipType);
                                                if (ship != null)
                                                {
                                                    ship.LocalId = JsonParsing.getInt(shipData, "ShipID");
                                                    ship.name = JsonParsing.getString(shipData, "Name");
                                                    ship.value = JsonParsing.getLong(shipData, "Value");
                                                    ship.hot = JsonParsing.getOptionalBool(shipData, "Hot") ?? false;
                                                    ship.intransit = JsonParsing.getOptionalBool(shipData, "InTransit") ?? false;
                                                    ship.marketid = JsonParsing.getOptionalLong(shipData, "ShipMarketID") ?? marketId;
                                                    ship.transferprice = JsonParsing.getOptionalLong(shipData, "TransferPrice");
                                                    ship.transfertime = JsonParsing.getOptionalLong(shipData, "TransferTime");

                                                    string starSystem = JsonParsing.getString(shipData, "StarSystem");
                                                    ship.starsystem = starSystem ?? system;
                                                    if (starSystem != null)
                                                    {
                                                        StarSystem systemData = StarSystemSqLiteRepository.Instance.GetStarSystem(starSystem, true);
                                                        ship.station = systemData?.stations?.FirstOrDefault(s => s.marketId == ship.marketid)?.name;
                                                        ship.x = systemData?.x;
                                                        ship.y = systemData?.y;
                                                        ship.z = systemData?.z;
                                                        ship.distance = ship.Distance(EDDI.Instance?.CurrentStarSystem?.x, EDDI.Instance?.CurrentStarSystem?.y, EDDI.Instance?.CurrentStarSystem?.z);
                                                    }
                                                    else
                                                    {
                                                        ship.station = station;
                                                        ship.x = EDDI.Instance?.CurrentStarSystem?.x;
                                                        ship.y = EDDI.Instance?.CurrentStarSystem?.y;
                                                        ship.z = EDDI.Instance?.CurrentStarSystem?.z;
                                                        ship.distance = 0;
                                                    }
                                                    shipyard.Add(ship);
                                                }
                                            }
                                        }
                                    }
                                    events.Add(new StoredShipsEvent(timestamp, marketId, station, system, shipyard) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "StoredModules":
                                {
                                    List<StoredModule> storedModules = new List<StoredModule>();

                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    string station = JsonParsing.getString(data, "StationName");

                                    data.TryGetValue("Items", out object val);
                                    List<object> items = (List<object>)val;
                                    if (items != null)
                                    {
                                        foreach (Dictionary<string, object> item in items)
                                        {
                                            string name = JsonParsing.getString(item, "Name");
                                            Module module = new Module(Module.FromEDName(name))
                                            {
                                                hot = JsonParsing.getOptionalBool(item, "Hot") ?? false
                                            };
                                            item.TryGetValue("EngineerModifications", out val);
                                            module.modificationEDName = JsonParsing.getString(item, "EngineerModifications");
                                            module.modified = !string.IsNullOrEmpty(module.modificationEDName);
                                            module.engineerlevel = module.modified ? JsonParsing.getInt(item, "Level") : 0;
                                            module.engineermodification = Blueprint.FromEDNameAndGrade(module.modificationEDName, module.engineerlevel) ?? Blueprint.None;
                                            module.engineerquality = module.modified ? JsonParsing.getDecimal(item, "Quality") : 0;

                                            StoredModule storedModule = new StoredModule
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

                                        string[] systemNames = storedModules.Where(s => !string.IsNullOrEmpty(s.system)).Select(s => s.system).Distinct().ToArray();
                                        List<StarSystem> systemsData = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystems(systemNames);
                                        if (systemsData?.Any() ?? false)
                                        {
                                            List<StoredModule> storedModulesHolder = new List<StoredModule>();
                                            foreach (StoredModule storedModule in storedModules)
                                            {
                                                if (!storedModule.intransit)
                                                {
                                                    StarSystem systemData = systemsData.FirstOrDefault(s => s.systemname == storedModule.system);
                                                    Station stationData = systemData?.stations?.FirstOrDefault(s => s.marketId == storedModule.marketid);
                                                    storedModule.station = stationData?.name;
                                                }
                                                storedModulesHolder.Add(storedModule);
                                            }
                                            storedModules = storedModulesHolder;
                                        }
                                    }
                                    events.Add(new StoredModulesEvent(timestamp, marketId, station, system, storedModules) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "TechnologyBroker":
                                {
                                    string brokerType = JsonParsing.getString(data, "BrokerType");
                                    long marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("ItemsUnlocked", out object val);
                                    List<object> itemsUnlocked = (List<object>)val;
                                    List<Module> items = new List<Module>();
                                    foreach (object item in itemsUnlocked)
                                    {
                                        Dictionary<string, object> itemProperties = (Dictionary<string, object>)item;
                                        string moduleEdName = JsonParsing.getString(itemProperties, "Name");
                                        Module module = Module.FromEDName(moduleEdName);
                                        if (module == null)
                                        {
                                            // Unknown module
                                            Logging.Info("Unknown module " + moduleEdName, JsonConvert.SerializeObject(item));
                                        }
                                        items.Add(module);
                                    }

                                    data.TryGetValue("Commodities", out val);
                                    List<object> commodities = (List<object>)val;
                                    List<CommodityAmount> Commodities = new List<CommodityAmount>();
                                    foreach (Dictionary<string, object> _commodity in commodities)
                                    {
                                        string commodityEdName = JsonParsing.getString(_commodity, "Name");
                                        CommodityDefinition commodity = CommodityDefinition.FromEDName(commodityEdName);
                                        int count = JsonParsing.getInt(_commodity, "Count");
                                        if (commodity == null)
                                        {
                                            Logging.Info("Unknown commodity " + commodityEdName);
                                            Logging.Info("Unknown commodity " + commodityEdName, JsonConvert.SerializeObject(_commodity));
                                        }
                                        Commodities.Add(new CommodityAmount(commodity, count));
                                    }

                                    data.TryGetValue("Materials", out val);
                                    List<object> materials = (List<object>)val;
                                    List<MaterialAmount> Materials = new List<MaterialAmount>();
                                    foreach (Dictionary<string, object> _material in materials)
                                    {
                                        string materialEdName = JsonParsing.getString(_material, "Name");
                                        Material material = Material.FromEDName(materialEdName);
                                        int count = JsonParsing.getInt(_material, "Count");
                                        Materials.Add(new MaterialAmount(material, count));
                                    }

                                    events.Add(new TechnologyBrokerEvent(timestamp, brokerType, marketId, items, Commodities, Materials) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardTransfer":
                                {
                                    long toMarketId = JsonParsing.getLong(data, "MarketID");
                                    long fromMarketId = JsonParsing.getLong(data, "ShipMarketID");

                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;

                                    string system = JsonParsing.getString(data, "System");
                                    decimal distance = JsonParsing.getDecimal(data, "Distance");
                                    long? price = JsonParsing.getOptionalLong(data, "TransferPrice");
                                    long? time = JsonParsing.getOptionalLong(data, "TransferTime");

                                    var ship = ConfigService.Instance.shipMonitorConfiguration?.shipyard.FirstOrDefault(s => s.LocalId == shipId);
                                    if (ship is null)
                                    {
                                        string shipEDModel = JsonParsing.getString(data, "ShipType");
                                        ship = ShipDefinitions.FromEDModel(shipEDModel);
                                        ship.LocalId = shipId;
                                    }

                                    events.Add(new ShipTransferInitiatedEvent(timestamp, ship, system, distance, price, time, fromMarketId, toMarketId) { raw = line, fromLoad = fromLogLoad });

                                    // Generate secondary event when the ship is arriving
                                    if (time.HasValue)
                                    {
                                        ShipArrived();
                                        async void ShipArrived()
                                        {
                                            // Include the station and system at which the transfer will arrive
                                            string arrivalStation = EDDI.Instance.CurrentStation?.name ?? string.Empty;
                                            string arrivalSystem = EDDI.Instance.CurrentStarSystem?.systemname ?? string.Empty;
                                            await Task.Delay((int)time * 1000);
                                            EDDI.Instance.enqueueEvent(new ShipArrivedEvent(DateTime.UtcNow, ship, arrivalSystem, distance, price, time, arrivalStation, fromMarketId, toMarketId) { fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "FetchRemoteModule":
                                {

                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");

                                    Module module = Module.FromEDName(JsonParsing.getString(data, "StoredItem"));
                                    data.TryGetValue("TransferCost", out val);
                                    long transferCost = (long)val;
                                    long? transferTime = JsonParsing.getOptionalLong(data, "TransferTime");

                                    // Probably not useful. We'll get these but we won't tell the end user about them
                                    data.TryGetValue("StorageSlot", out val);
                                    int storageSlot = (int)(long)val;
                                    data.TryGetValue("ServerId", out val);
                                    long serverId = (long)val;

                                    events.Add(new ModuleTransferEvent(timestamp, ship, shipId, storageSlot, serverId, module, transferCost, transferTime) { raw = line, fromLoad = fromLogLoad });

                                    // Generate a secondary event when the module is arriving

                                    if (transferTime.HasValue)
                                    {
                                        ModuleArrived();
                                        async void ModuleArrived()
                                        {
                                            // Include the station and system at which the transfer will arrive
                                            string arrivalStation = EDDI.Instance.CurrentStation?.name ?? string.Empty;
                                            string arrivalSystem = EDDI.Instance.CurrentStarSystem?.systemname ?? string.Empty;
                                            await Task.Delay((int)transferTime * 1000);
                                            EDDI.Instance.enqueueEvent(new ModuleArrivedEvent(DateTime.UtcNow, ship, shipId, storageSlot, serverId, module, transferCost, transferTime, arrivalSystem, arrivalStation) { fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "MassModuleStore":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");

                                    data.TryGetValue("Items", out val);
                                    List<object> items = (List<object>)val;

                                    List<string> slots = new List<string>();
                                    List<Module> modules = new List<Module>();

                                    Module module = new Module();
                                    if (items != null)
                                    {

                                        foreach (Dictionary<string, object> item in items)
                                        {
                                            string slot = JsonParsing.getString(item, "Slot");
                                            slots.Add(slot);

                                            module = Module.FromEDName(JsonParsing.getString(item, "Name"));
                                            module.hot = JsonParsing.getBool(item, "Hot");
                                            string engineerModifications = JsonParsing.getString(item, "EngineerModifications");
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
                            case "ModuleBuy":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");

                                    string slot = JsonParsing.getString(data, "Slot");
                                    Module buyModule = Module.FromEDName(JsonParsing.getString(data, "BuyItem"));
                                    data.TryGetValue("BuyPrice", out val);
                                    long buyPrice = (long)val;
                                    buyModule.price = buyPrice;

                                    // Set retrieved module defaults
                                    buyModule.enabled = true;
                                    buyModule.priority = 1;
                                    buyModule.health = 100;
                                    buyModule.modified = false;

                                    Module sellModule = Module.FromEDName(JsonParsing.getString(data, "SellItem"));
                                    long? sellPrice = JsonParsing.getOptionalLong(data, "SellPrice");
                                    Module storedModule = Module.FromEDName(JsonParsing.getString(data, "StoredItem"));

                                    events.Add(new ModulePurchasedEvent(timestamp, ship, shipId, slot, buyModule, buyPrice, sellModule, sellPrice, storedModule, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleRetrieve":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");

                                    string slot = JsonParsing.getString(data, "Slot");
                                    Module module = Module.FromEDName(JsonParsing.getString(data, "RetrievedItem"));
                                    module.hot = JsonParsing.getBool(data, "Hot");
                                    string engineerModifications = JsonParsing.getString(data, "EngineerModifications");
                                    module.modified = engineerModifications != null;
                                    module.engineerlevel = JsonParsing.getOptionalInt(data, "Level") ?? 0;
                                    module.engineermodification = Blueprint.FromEDNameAndGrade(engineerModifications, module.engineerlevel) ?? Blueprint.None;
                                    module.engineerquality = JsonParsing.getOptionalDecimal(data, "Quality") ?? 0;

                                    // Set retrieved module defaults
                                    module.price = module.value;
                                    module.enabled = true;
                                    module.priority = 1;
                                    module.health = 100;

                                    // Set module cost
                                    data.TryGetValue("Cost", out val);
                                    long? cost = JsonParsing.getOptionalLong(data, "Cost");

                                    Module swapoutModule = Module.FromEDName(JsonParsing.getString(data, "SwapOutItem"));

                                    events.Add(new ModuleRetrievedEvent(timestamp, ship, shipId, slot, module, cost, engineerModifications, swapoutModule, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleSell":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");

                                    string slot = JsonParsing.getString(data, "Slot");
                                    Module module = Module.FromEDName(JsonParsing.getString(data, "SellItem"));
                                    data.TryGetValue("SellPrice", out val);
                                    long price = (long)val;

                                    events.Add(new ModuleSoldEvent(timestamp, ship, shipId, slot, module, price, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleSellRemote":
                                {
                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");

                                    Module module = Module.FromEDName(JsonParsing.getString(data, "SellItem"));
                                    data.TryGetValue("SellPrice", out val);
                                    long price = (long)val;

                                    // Probably not useful. We'll get these but we won't tell the end user about them
                                    data.TryGetValue("StorageSlot", out val);
                                    int storageSlot = (int)(long)val;
                                    data.TryGetValue("ServerId", out val);
                                    long serverId = (long)val;

                                    events.Add(new ModuleSoldFromStorageEvent(timestamp, ship, shipId, storageSlot, serverId, module, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleStore":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");

                                    string slot = JsonParsing.getString(data, "Slot");
                                    Module module = Module.FromEDName(JsonParsing.getString(data, "StoredItem"));
                                    module.hot = JsonParsing.getBool(data, "Hot");
                                    string engineerModifications = JsonParsing.getString(data, "EngineerModifications");
                                    module.modified = engineerModifications != null;
                                    module.engineerlevel = JsonParsing.getOptionalInt(data, "Level") ?? 0;
                                    module.engineermodification = Blueprint.FromEDNameAndGrade(engineerModifications, module.engineerlevel) ?? Blueprint.None;
                                    module.engineerquality = JsonParsing.getOptionalDecimal(data, "Quality") ?? 0;

                                    data.TryGetValue("Cost", out val);
                                    long? cost = JsonParsing.getOptionalLong(data, "Cost");

                                    Module replacementModule = Module.FromEDName(JsonParsing.getString(data, "ReplacementItem"));
                                    if (replacementModule != null)
                                    {
                                        replacementModule.price = replacementModule.value;
                                        replacementModule.enabled = true;
                                        replacementModule.priority = 1;
                                        replacementModule.health = 100;
                                        replacementModule.modified = false;
                                    }

                                    events.Add(new ModuleStoredEvent(timestamp, ship, shipId, slot, module, cost, engineerModifications, replacementModule, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleSwap":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");

                                    string fromSlot = JsonParsing.getString(data, "FromSlot");
                                    Module fromModule = Module.FromEDName(JsonParsing.getString(data, "FromItem"));
                                    string toSlot = JsonParsing.getString(data, "ToSlot");
                                    Module toModule = Module.FromEDName(JsonParsing.getString(data, "ToItem"));

                                    events.Add(new ModuleSwappedEvent(timestamp, ship, shipId, fromSlot, fromModule, toSlot, toModule, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Outfitting":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string station = JsonParsing.getString(data, "StationName");
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    if (OutfittingInfo.TryFromFile(timestamp, system, station, marketId, out var info, out var raw))
                                    {
                                        events.Add(new OutfittingEvent(timestamp, marketId, station, system, info) { raw = raw, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "SetUserShipName":
                                {
                                    data.TryGetValue("ShipID", out object val);
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");
                                    string name = JsonParsing.getString(data, "UserShipName");
                                    string ident = JsonParsing.getString(data, "UserShipId");

                                    events.Add(new ShipRenamedEvent(timestamp, ship, shipId, name, ident) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Music":
                                {
                                    string musicTrack = JsonParsing.getString(data, "MusicTrack");
                                    events.Add(new MusicEvent(timestamp, musicTrack) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LaunchSRV":
                                {
                                    string loadout = JsonParsing.getString(data, "Loadout");
                                    bool playercontrolled = JsonParsing.getBool(data, "PlayerControlled");
                                    int? id = JsonParsing.getOptionalInt(data, "ID");
                                    var vehicleDefinition = VehicleDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                                    vehicleDefinition.fallbackLocalizedName = JsonParsing.getString(data, "SRVType_Localised");
                                    events.Add(new SRVLaunchedEvent(timestamp, loadout, playercontrolled, vehicleDefinition, id) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockSRV":
                                {
                                    int? srvId = JsonParsing.getOptionalInt(data, "ID");
                                    var vehicleDefinition = VehicleDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                                    vehicleDefinition.fallbackLocalizedName = JsonParsing.getString(data, "SRVType_Localised");
                                    events.Add(new SRVDockedEvent(timestamp, vehicleDefinition, srvId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SRVDestroyed":
                                {
                                    string vehicle = "srv";
                                    int? srvId = JsonParsing.getOptionalInt(data, "ID");
                                    var vehicleDefinition = VehicleDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                                    vehicleDefinition.fallbackLocalizedName = JsonParsing.getString(data, "SRVType_Localised");
                                    events.Add(new VehicleDestroyedEvent(timestamp, vehicle, vehicleDefinition, srvId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "LaunchFighter":
                                {
                                    string loadout = JsonParsing.getString(data, "Loadout");
                                    int fighterId = JsonParsing.getInt(data, "ID");
                                    bool playerControlled = JsonParsing.getBool(data, "PlayerControlled");
                                    events.Add(new FighterLaunchedEvent(timestamp, loadout, fighterId, playerControlled) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockFighter":
                                {
                                    int fighterId = JsonParsing.getInt(data, "ID");
                                    events.Add(new FighterDockedEvent(timestamp, fighterId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FighterDestroyed":
                                {
                                    string vehicle = "fighter";
                                    int fighterId = JsonParsing.getInt(data, "ID");
                                    events.Add(new VehicleDestroyedEvent(timestamp, vehicle, null, fighterId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "FighterRebuilt":
                                {
                                    string loadout = JsonParsing.getString(data, "Loadout");
                                    int fighterId = JsonParsing.getInt(data, "ID");
                                    events.Add(new FighterRebuiltEvent(timestamp, loadout, fighterId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "VehicleSwitch":
                                {
                                    string to = JsonParsing.getString(data, "To");
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
                            case "Interdicted":
                                {
                                    bool submitted = JsonParsing.getBool(data, "Submitted");
                                    string interdictor = JsonParsing.getString(data, "Interdictor");
                                    var iscommander = JsonParsing.getOptionalBool(data, "IsPlayer") ?? false;
                                    var isThargoid = JsonParsing.getOptionalBool(data, "IsThargoid") ?? false;
                                    data.TryGetValue("CombatRank", out object val);
                                    CombatRating rating = (val == null ? null : CombatRating.FromRank(Convert.ToInt32(val)));
                                    string faction = GetFactionName(data, "Faction");
                                    string power = JsonParsing.getString(data, "Power");

                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "Interdictor_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        interdictor = NpcAuthorityShip.EDNameExists(interdictor)
                                            ? NpcAuthorityShip.FromEDName(interdictor)?.localizedName
                                            : JsonParsing.getString(data, "Interdictor_Localised");
                                    }
                                    else if ( isThargoid )
                                    {
                                        interdictor = NpcAuthorityShip.Thargoid.localizedName;
                                    }
                                    else if (string.IsNullOrEmpty(interdictor) && !data.ContainsKey("Interdictor") && string.IsNullOrEmpty(faction))
                                    {
                                        // This matches the pattern for an unknown ship interdiction attempt
                                        interdictor = NpcAuthorityShip.UNKNOWN.localizedName;
                                    }

                                    events.Add(new ShipInterdictedEvent(timestamp, true, submitted, iscommander, isThargoid, interdictor, rating, faction, power) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "EscapeInterdiction":
                                {
                                    string interdictor = JsonParsing.getString(data, "Interdictor");
                                    var iscommander = JsonParsing.getOptionalBool(data, "IsPlayer") ?? false;
                                    var isThargoid = JsonParsing.getOptionalBool(data, "isThargoid") ?? false;

                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "Interdictor_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        interdictor = NpcAuthorityShip.EDNameExists(interdictor)
                                            ? NpcAuthorityShip.FromEDName(interdictor)?.localizedName
                                            : JsonParsing.getString(data, "Interdictor_Localised");
                                    }
                                    else if ( isThargoid )
                                    {
                                        interdictor = NpcAuthorityShip.Thargoid.localizedName;
                                    }
                                    else if (string.IsNullOrEmpty(interdictor) && !data.ContainsKey("Interdictor"))
                                    {
                                        // This matches the pattern for an unknown ship interdiction attempt
                                        interdictor = NpcAuthorityShip.UNKNOWN.localizedName;
                                    }

                                    events.Add(new ShipInterdictedEvent(timestamp, false, false, iscommander, isThargoid, interdictor, null, null, null) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "Interdiction":
                                {
                                    bool success = JsonParsing.getBool(data, "Success");
                                    string interdictee = JsonParsing.getString(data, "Interdicted");
                                    bool iscommander = JsonParsing.getBool(data, "IsPlayer");
                                    data.TryGetValue("CombatRank", out object val);
                                    var rating = ( val == null ? null : CombatRating.FromRank( Convert.ToInt32( val ) ) );
                                    string faction = GetFactionName(data, "Faction");
                                    string power = JsonParsing.getString(data, "Power");

                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "Interdicted_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        interdictee = NpcAuthorityShip.EDNameExists(interdictee) 
                                            ? NpcAuthorityShip.FromEDName(interdictee)?.localizedName 
                                            : JsonParsing.getString(data, "Interdicted_Localised");
                                    }

                                    events.Add(new ShipInterdictionEvent(timestamp, success, iscommander, interdictee, rating, faction, power) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "PVPKill":
                                {
                                    string victim = JsonParsing.getString(data, "Victim");
                                    data.TryGetValue("CombatRank", out object val);
                                    CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));

                                    events.Add(new KilledEvent(timestamp, victim, rating) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "MaterialCollected":
                                {
                                    Material material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;
                                    events.Add(new MaterialCollectedEvent(timestamp, material, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "MaterialDiscarded":
                                {
                                    Material material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;
                                    events.Add(new MaterialDiscardedEvent(timestamp, material, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "MaterialDiscovered":
                                {
                                    Material material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    events.Add(new MaterialDiscoveredEvent(timestamp, material) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "MaterialTrade":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string traderType = JsonParsing.getString(data, "TraderType");

                                    data.TryGetValue("Paid", out object val);
                                    Dictionary<string, object> paid = (Dictionary<string, object>)val;

                                    string materialEdName = JsonParsing.getString(paid, "Material");
                                    Material materialPaid = Material.FromEDName(materialEdName);
                                    int materialPaidQty = JsonParsing.getInt(paid, "Quantity");

                                    if (materialPaid == null)
                                    {
                                        Logging.Info("Unknown material " + materialEdName);
                                        Logging.Info("Unknown material " + materialEdName, JsonConvert.SerializeObject(paid));
                                    }

                                    data.TryGetValue("Received", out val);
                                    Dictionary<string, object> received = (Dictionary<string, object>)val;

                                    Material materialReceived = Material.FromEDName(JsonParsing.getString(received, "Material"));
                                    int materialReceivedQty = JsonParsing.getInt(received, "Quantity");

                                    if (materialReceived == null)
                                    {
                                        Logging.Info("Unknown material " + materialEdName, JsonConvert.SerializeObject(received));
                                    }

                                    events.Add(new MaterialTradedEvent(timestamp, marketId, traderType, materialPaid, materialPaidQty, materialReceived, materialReceivedQty) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;

                                    break;
                                }
                            case "ScientificResearch":
                                {
                                    data.TryGetValue("Name", out object val);
                                    Material material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    int amount = JsonParsing.getInt(data, "Count");
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    events.Add(new MaterialDonatedEvent(timestamp, material, amount, marketId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "StartJump":
                                {
                                    string target = JsonParsing.getString(data, "JumpType");
                                    string stellarclass = JsonParsing.getString(data, "StarClass");
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getOptionalULong(data, "SystemAddress");
                                    var isTaxi = JsonParsing.getOptionalBool( data, "Taxi" ) ?? false;
                                    events.Add(new FSDEngagedEvent(timestamp, target, system, systemAddress, stellarclass, isTaxi) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "ReceiveText":
                                {
                                    string from = JsonParsing.getString(data, "From");
                                    string channel = JsonParsing.getString(data, "Channel");
                                    string message = JsonParsing.getString(data, "Message");
                                    MessageChannel messageChannel;
                                    MessageSource source;

                                    if (from == string.Empty && channel == "npc" && (message.StartsWith("$COMMS_entered") || message.StartsWith("$CHAT_Intro")))
                                    {
                                        // We can safely ignore system messages that initialize the chat system or announce that we entered a channel - no event is needed. 
                                        handled = true;
                                        break;
                                    }

                                    if (
                                        channel == "player" ||
                                        channel == "wing" ||
                                        channel == "friend" ||
                                        channel == "voicechat" ||
                                        channel == "local" ||
                                        channel == "squadron" ||
                                        channel == "starsystem" ||
                                        channel == null
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
                                        events.Add(new MessageReceivedEvent(timestamp, from, source, true, messageChannel, message) { raw = line, fromLoad = fromLogLoad });
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
                                            from = JsonParsing.getString(data, "From");
                                            if (!string.IsNullOrEmpty(JsonParsing.getString(data, "From_Localised")))
                                            {
                                                // This is an NPC with a symbolic name
                                                from = NpcAuthorityShip.EDNameExists(from) 
                                                    ? NpcAuthorityShip.FromEDName(from)?.localizedName 
                                                    : JsonParsing.getString(data, "From_Localised");
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
                                        events.Add(new MessageReceivedEvent(timestamp, from, source, false, messageChannel, JsonParsing.getString(data, "Message_Localised"), EDDI.Instance.CurrentStarSystem, EDDI.Instance.CurrentStellarBody, EDDI.Instance.CurrentStation) { raw = line, fromLoad = fromLogLoad });

                                        // See if we also want to spawn a specific event as well?
                                        if (message == "$STATION_NoFireZone_entered;" && EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                                        {
                                            events.Add(new StationNoFireZoneEnteredEvent(timestamp, false) { raw = line, fromLoad = fromLogLoad });
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
                                            MessageSource by = MessageSource.FromMessage(from, message);

                                            events.Add(new NPCInterdictionCommencedEvent(timestamp, by) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_Attack") || message.Contains("_OnAttackStart") || message.Contains("AttackRun") || message.Contains("OnDeclarePiracyAttack"))
                                        {
                                            // Find out who is doing the attacking
                                            MessageSource by = MessageSource.FromMessage(from, message);
                                            events.Add(new NPCAttackCommencedEvent(timestamp, by) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_OnStartScanCargo"))
                                        {
                                            // Find out who is doing the scanning
                                            MessageSource by = MessageSource.FromMessage(from, message);
                                            events.Add(new NPCCargoScanCommencedEvent(timestamp, by) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "SendText":
                                {
                                    string to = JsonParsing.getString(data, "To");
                                    string message = JsonParsing.getString(data, "Message");
                                    events.Add(new MessageSentEvent(timestamp, to, message) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingRequested":
                                {
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    string stationType = JsonParsing.getString(data, "StationType");
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    events.Add(new DockingRequestedEvent(timestamp, stationName, stationType, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingGranted":
                                {
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    string stationType = JsonParsing.getString(data, "StationType");
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("LandingPad", out object val);
                                    int landingPad = (int)(long)val;
                                    events.Add(new DockingGrantedEvent(timestamp, stationName, stationType, marketId, landingPad) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingDenied":
                                {
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    string stationType = JsonParsing.getString(data, "StationType");
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string reason = JsonParsing.getString(data, "Reason");
                                    events.Add(new DockingDeniedEvent(timestamp, stationName, stationType, marketId, reason) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingCancelled":
                                {
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    string stationType = JsonParsing.getString(data, "StationType");
                                    long? marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    events.Add(new DockingCancelledEvent(timestamp, stationName, stationType, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingTimeout":
                                {
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    string stationType = JsonParsing.getString(data, "StationType");
                                    events.Add(new DockingTimedOutEvent(timestamp, stationName, stationType) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MiningRefined":
                                {
                                    string commodityName = JsonParsing.getString(data, "Type");

                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    events.Add(new CommodityRefinedEvent(timestamp, commodity) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "HeatWarning":
                                events.Add(new HeatWarningEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                handled = true;
                                break;
                            case "HeatDamage":
                                events.Add(new HeatDamageEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                handled = true;
                                break;
                            case "HullDamage":
                                {
                                    decimal health = sensibleHealth(JsonParsing.getDecimal(data, "Health") * 100);
                                    bool? piloted = JsonParsing.getOptionalBool(data, "PlayerPilot");
                                    bool? fighter = JsonParsing.getOptionalBool(data, "Fighter");

                                    string vehicle = EDDI.Instance.Vehicle;
                                    if ( piloted == false )
                                    {
                                        if ( fighter == true )
                                        {
                                            vehicle = Constants.VEHICLE_FIGHTER;
                                        }
                                        else if ( EDDI.Instance.Vehicle == Constants.VEHICLE_SRV )
                                        {
                                            vehicle = Constants.VEHICLE_SHIP;
                                        }
                                        else if ( EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP )
                                        {
                                            vehicle = Constants.VEHICLE_SRV;
                                        }
                                    }

                                    events.Add(new HullDamagedEvent(timestamp, vehicle, piloted, health) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShieldState":
                                // As of September 2019, this event no longer appears to be written to the Player Journal.
                                // We still generate an event via the Status Monitor.
                                break;
                            case "ShipTargeted":
                                {
                                    var targetlocked = JsonParsing.getBool(data, "TargetLocked");

                                    // Target locked
                                    var scanstage = JsonParsing.getOptionalInt(data, "ScanStage");
                                    VehicleDefinition fighterDef = null;
                                    Ship shipDef = null;
                                    var vehicleEDName = JsonParsing.getString(data, "Ship");
                                    if ( vehicleEDName != null)
                                    {
                                        if (vehicleEDName.Contains("fighter", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            fighterDef = VehicleDefinition.FromEDName( vehicleEDName );
                                            fighterDef.fallbackLocalizedName = JsonParsing.getString( data, "Ship_Localised" );
                                        }
                                        else
                                        {
                                            shipDef = ShipDefinitions.FromEDModel(vehicleEDName, false);
                                            if ( shipDef is null )
                                            {
                                                shipDef = ShipDefinitions.FromEDModel( vehicleEDName, true );
                                                shipDef.model = JsonParsing.getString( data, "Ship_Localised" );
                                            }
                                        }
                                    }

                                    // Scan stage >= 1
                                    var name = JsonParsing.getString(data, "PilotName");
                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "PilotName_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        name = NpcAuthorityShip.EDNameExists(name) 
                                            ? NpcAuthorityShip.FromEDName(name)?.localizedName 
                                            : JsonParsing.getString(data, "PilotName_Localised");
                                    }

                                    // Sometimes we don't get a localized name when we ought to.
                                    // Strip out any remaining unlocalized content in the name.
                                    const string unlocalizedNameRegex = @"\$.+;";
                                    if ( !string.IsNullOrEmpty(name) && Regex.IsMatch( name, unlocalizedNameRegex ) )
                                    {
                                        var tidiedName = Regex.Replace( name, unlocalizedNameRegex, "" ).Trim();
                                        if ( !string.IsNullOrEmpty(tidiedName) )
                                        {
                                            name = tidiedName;
                                        }
                                    }

                                    var rank = CombatRating.FromEDName(JsonParsing.getString(data, "PilotRank"));

                                    // Scan stage >= 2
                                    var shieldHealth = JsonParsing.getOptionalDecimal(data, "ShieldHealth");
                                    var hullHealth = JsonParsing.getOptionalDecimal(data, "HullHealth");

                                    // Scan stage >= 3
                                    var faction = JsonParsing.getString(data, "Faction");
                                    var legalStatus = LegalStatus.FromEDName(JsonParsing.getString(data, "LegalStatus"));
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    var bounty = JsonParsing.getOptionalInt(data, "Bounty");
                                    string subsystemName = null;
                                    decimal? subSystemHealth = null;
                                    if ( data.ContainsKey( "Subsystem" ) )
                                    {
                                        var subsystemEDName = JsonParsing.getString( data, "Subsystem" );
                                        subsystemName = subsystemEDName.StartsWith( "$ext_drive" ) 
                                            ? EddiDataDefinitions.Properties.Modules.Thrusters // The `ShipTargeted` event uses non-standard drive names
                                            : Module.FromEDName( subsystemEDName )?.localizedName;
                                        if ( string.IsNullOrEmpty(subsystemName) )
                                        {
                                            subsystemName = JsonParsing.getString( data, "Subsystem_Localised" );
                                        }
                                        subSystemHealth = JsonParsing.getOptionalDecimal( data, "SubsystemHealth" );
                                    }

                                    events.Add(new ShipTargetedEvent(timestamp, targetlocked, shipDef, fighterDef, scanstage, name, rank, faction, power, legalStatus, bounty, shieldHealth, hullHealth, subsystemName, subSystemHealth) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "UnderAttack":
                                {
                                    string target = JsonParsing.getString(data, "Target");
                                    events.Add(new UnderAttackEvent(timestamp, target) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "SelfDestruct":
                                events.Add(new SelfDestructEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                handled = true;
                                break;
                            case "Died":
                                {
                                    Killer parseKiller(IDictionary<string, object> killerData, bool singleKiller)
                                    {
                                        // Property names differ if there is a single killer vs. multiple killers
                                        var name = JsonParsing.getString(killerData, singleKiller ? "KillerName" : "Name");
                                        if (!string.IsNullOrEmpty(JsonParsing.getString(data, singleKiller ? "KillerName_Localised" : "Name_Localised")))
                                        {
                                            // This is an NPC with a symbolic name
                                            name = NpcAuthorityShip.EDNameExists(name)
                                                ? NpcAuthorityShip.FromEDName(name)?.localizedName
                                                : JsonParsing.getString(data, singleKiller ? "KillerName_Localised" : "Name_Localised");
                                        }

                                        var equipment = JsonParsing.getString(killerData, singleKiller ? "KillerShip" : "Ship"); // May be a ship, a suit, etc.
                                        var rating = CombatRating.FromEDName(JsonParsing.getString(killerData, singleKiller ? "KillerRank" : "Rank"));
                                        return new Killer(name, equipment, rating);
                                    }

                                    var killers = new List<Killer>();
                                    if (data.ContainsKey("KillerName"))
                                    {
                                        // Single killer
                                        killers.Add(parseKiller(data, true));
                                    }
                                    if (data.ContainsKey("killers"))
                                    {
                                        // Multiple killers
                                        data.TryGetValue("Killers", out object val);
                                        List<object> killersData = (List<object>)val;
                                        foreach (IDictionary<string, object> killerData in killersData)
                                        {
                                            killers.Add(parseKiller(killerData, false));
                                        }
                                    }
                                    events.Add(new DiedEvent(timestamp, killers) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Resurrect":
                                {
                                    var option = JsonParsing.getString(data, "Option");
                                    var price = JsonParsing.getLong(data, "Cost");
                                    events.Add(new RespawnedEvent(timestamp, option, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "NavBeaconScan":
                                {
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    data.TryGetValue("NumBodies", out object val);
                                    int numbodies = (int)(long)val;
                                    events.Add(new NavBeaconScanEvent(timestamp, systemAddress, numbodies) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSSDiscoveryScan":
                                {
                                    // A journal error was introduced in Odyssey 4.0 Update 17
                                    // where FSSDiscoveryScan would report zero bodies and infinite progress.
                                    // Don't parse `Progress` unless `BodyCount` is greater than zero.

                                    int bodyCount = JsonParsing.getInt(data, "BodyCount"); // total number of stellar bodies in system
                                    int nonBodyCount = JsonParsing.getInt(data, "NonBodyCount"); // total number of non-body signals found
                                    decimal progress = bodyCount > 0 ? JsonParsing.getDecimal(data, "Progress") : 0; // value from 0-1
                                    events.Add( new DiscoveryScanEvent( timestamp, progress, bodyCount, nonBodyCount ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "FSSSignalDiscovered":
                                {
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");

                                    SignalSource source = GetSignalSourceName(data);

                                    var signalTypeEdName = JsonParsing.getString( data, "SignalType" );
                                    source.signalType = signalTypeEdName is null ? null : SignalType.FromEDName( signalTypeEdName );

                                    source.spawningFaction = GetFactionName(data, "SpawningFaction") ?? Superpower.None.localizedName; // the minor faction, if relevant
                                    var secondsRemaining = JsonParsing.getOptionalDecimal(data, "TimeRemaining"); // remaining lifetime in seconds, if relevant
                                    source.expiry = secondsRemaining is null ? (DateTime?)null : timestamp.AddSeconds((double)(secondsRemaining));

                                    string spawningstate = JsonParsing.getString(data, "SpawningState");
                                    string normalizedSpawningState = spawningstate?.Replace("$FactionState_", "")?.Replace("_desc;", "");
                                    source.spawningState = FactionState.FromEDName(normalizedSpawningState) ?? new FactionState();
                                    source.spawningState.fallbackLocalizedName = JsonParsing.getString(data, "SpawningState_Localised");

                                    source.threatLevel = JsonParsing.getOptionalInt(data, "ThreatLevel") ?? 0;

                                    bool unique = false;
                                    if (EDDI.Instance.CurrentStarSystem != null && EDDI.Instance.CurrentStarSystem.systemAddress == systemAddress)
                                    {
                                        unique = !EDDI.Instance.CurrentStarSystem.signalsources.Contains(source.localizedName);
                                        EDDI.Instance.CurrentStarSystem.AddOrUpdateSignalSource(source);

                                        if (source.isStation ?? false)
                                        {
                                            // Add station signals to the current star system if they are not already present.
                                            if (EDDI.Instance.CurrentStarSystem.stations.All(s => s.name != source.edname))
                                            {
                                                var station = new Station { name = source.edname, systemAddress = systemAddress };
                                                if (!string.IsNullOrEmpty(source.localizedName) && source.edname != source.localizedName)
                                                {
                                                    // At present, fleet carriers are the only station model which may have a localized signal name
                                                    station.Model = StationModel.FleetCarrier;
                                                }
                                                EDDI.Instance.CurrentStarSystem.stations.Add(station);
                                            }
                                        }
                                    }
                                    
                                    events.Add(new SignalDetectedEvent(timestamp, systemAddress, source, unique) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BuyExplorationData":
                                {
                                    string system = JsonParsing.getString(data, "System");
                                    long price = JsonParsing.getLong(data, "Cost");
                                    events.Add(new ExplorationDataPurchasedEvent(timestamp, system, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SAAScanComplete":
                                {
                                    string bodyName = JsonParsing.getString(data, "BodyName");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    int probesUsed = JsonParsing.getInt(data, "ProbesUsed");
                                    int efficiencyTarget = JsonParsing.getInt(data, "EfficiencyTarget");

                                    // Target may be either a ring or a body
                                    StarSystem system = EDDI.Instance?.CurrentStarSystem;
                                    Body body = null;

                                    if (system != null && bodyName.EndsWith(" Ring"))
                                    {
                                        // We've mapped a ring. 
                                        Ring ring = null;
                                        List<Body> ringedBodies = system.bodies?.Where(b => b?.rings?.Count > 0).ToList();
                                        foreach (Body ringedBody in ringedBodies)
                                        {
                                            ring = ringedBody.rings.FirstOrDefault(r => r.name == bodyName);
                                            if (ring != null)
                                            {
                                                body = ringedBody;
                                                break;
                                            }
                                        }
                                        events.Add(new RingMappedEvent(timestamp, bodyName, ring, body, systemAddress, probesUsed, efficiencyTarget) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // Prepare updated map details to update the body in our star system
                                        body = system?.BodyWithID(bodyId);
                                        if (!(body is null))
                                        {
                                            body.scannedDateTime = body.scannedDateTime ?? timestamp;
                                            body.mappedDateTime = timestamp;
                                            body.mappedEfficiently = probesUsed <= efficiencyTarget;
                                            events.Add(new BodyMappedEvent(timestamp, bodyName, body, systemAddress, probesUsed, efficiencyTarget) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "SellExplorationData":
                                {
                                    data.TryGetValue("Systems", out object val);
                                    List<string> systems = ((List<object>)val).Cast<string>().ToList();
                                    data.TryGetValue("Discovered", out val);
                                    List<string> firsts = ((List<object>)val).Cast<string>().ToList();
                                    data.TryGetValue("BaseValue", out val);
                                    decimal reward = (long)val;
                                    data.TryGetValue("Bonus", out val);
                                    decimal bonus = (long)val;
                                    data.TryGetValue("TotalEarnings", out val);
                                    decimal total = (long)val;
                                    events.Add(new ExplorationDataSoldEvent(timestamp, systems, reward, bonus, total) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MultiSellExplorationData":
                                {
                                    List<string> systems = new List<string>();
                                    data.TryGetValue("Discovered", out object val);
                                    List<object> discovered = (List<object>)val;
                                    foreach (Dictionary<string, object> discoveredSystem in discovered)
                                    {
                                        string system = JsonParsing.getString(discoveredSystem, "SystemName");
                                        if (!string.IsNullOrEmpty(system))
                                        {
                                            systems.Add(system);
                                        }
                                    }
                                    data.TryGetValue("BaseValue", out val);
                                    decimal reward = (long)val;
                                    data.TryGetValue("Bonus", out val);
                                    decimal bonus = (long)val;
                                    data.TryGetValue("TotalEarnings", out val);
                                    decimal total = (long)val;
                                    events.Add(new ExplorationDataSoldEvent(timestamp, systems, reward, bonus, total) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Market":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string station = JsonParsing.getString(data, "StationName");
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    if (MarketInfo.TryFromFile(timestamp, system, station, marketId, out var info, out var raw))
                                    {
                                        events.Add(new MarketEvent(timestamp, marketId, station, system, info) { raw = raw, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "MarketBuy":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string commodityName = JsonParsing.getString(data, "Type");
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    int amount = JsonParsing.getInt(data, "Count");
                                    int price = JsonParsing.getInt(data, "BuyPrice");
                                    events.Add(new CommodityPurchasedEvent(timestamp, marketId, commodity, amount, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MarketSell":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string commodityName = JsonParsing.getString(data, "Type");
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    int amount = JsonParsing.getInt(data, "Count");
                                    int sellPrice = JsonParsing.getInt(data, "SellPrice");

                                    long buyPrice = JsonParsing.getLong(data, "AvgPricePaid");
                                    // We don't care about buy price, we care about profit per unit
                                    long profit = sellPrice - buyPrice;

                                    bool illegal = JsonParsing.getOptionalBool(data, "IllegalGoods") ?? false;
                                    bool stolen = JsonParsing.getOptionalBool(data, "StolenGoods") ?? false;
                                    bool blackmarket = JsonParsing.getOptionalBool(data, "BlackMarket") ?? false;

                                    events.Add(new CommoditySoldEvent(timestamp, marketId, commodity, amount, sellPrice, profit, illegal, stolen, blackmarket) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EngineerContribution":
                                {
                                    string name = JsonParsing.getString(data, "Engineer");
                                    long engineerId = JsonParsing.getLong(data, "EngineerID");
                                    Engineer engineer = Engineer.FromNameOrId(name, engineerId);

                                    string contributionType =
                                        JsonParsing.getString(data,
                                            "Type"); // (Commodity, materials, Credits, Bond, Bounty)
                                    int amount = JsonParsing.getInt(data, "Quantity");
                                    int total = JsonParsing.getInt(data, "TotalQuantity");
                                    switch (contributionType)
                                    {
                                        case "Commodity":
                                            {
                                                string edname = JsonParsing.getString(data, "Commodity");
                                                CommodityAmount commodity = new CommodityAmount(CommodityDefinition.FromEDName(edname), amount);
                                                events.Add(new EngineerContributedEvent(timestamp, engineer, contributionType, amount, total, commodity, null) { raw = line, fromLoad = fromLogLoad });
                                            }
                                            break;
                                        case "Materials":
                                            {
                                                string edname = JsonParsing.getString(data, "Material");
                                                MaterialAmount material = new MaterialAmount(Material.FromEDName(edname), amount);
                                                events.Add(new EngineerContributedEvent(timestamp, engineer, contributionType, amount, total, null, material) { raw = line, fromLoad = fromLogLoad });
                                            }
                                            break;
                                        case "Credits":
                                        case "Bond":
                                        case "Bounty":
                                            {
                                                // We don't currently handle these types.
                                            }
                                            break;
                                    }
                                }
                                handled = true;
                                break;
                            case "EngineerCraft":
                                {
                                    string engineer = JsonParsing.getString(data, "Engineer");
                                    long engineerId = JsonParsing.getLong(data, "EngineerID");
                                    string blueprintpEdName = JsonParsing.getString(data, "BlueprintName");
                                    long blueprintId = JsonParsing.getLong(data, "BlueprintID");

                                    data.TryGetValue("Level", out object val);
                                    int level = (int)(long)val;

                                    decimal? quality = JsonParsing.getOptionalDecimal(data, "Quality"); //
                                    string experimentalEffect = JsonParsing.getString(data, "ApplyExperimentalEffect"); //

                                    var ship = EDDI.Instance.CurrentShip?.EDName;
                                    Compartment compartment = null;
                                    if (!string.IsNullOrEmpty(ship))
                                    {
                                        compartment = parseShipCompartment(ship, JsonParsing.getString(data, "Slot"));
                                        compartment.module = Module.FromEDName(JsonParsing.getString(data, "Module"));
                                    }
                                    var commodities = new List<CommodityAmount>();
                                    var materials = new List<MaterialAmount>();
                                    if (data.TryGetValue("Ingredients", out val))
                                    {
                                        // 2.2 style
                                        if (val is Dictionary<string, object> usedData)
                                        {
                                            foreach (KeyValuePair<string, object> used in usedData)
                                            {
                                                // Used could be a material or a commodity
                                                CommodityDefinition commodity = CommodityDefinition.FromEDName(used.Key);
                                                if (commodity.Category != null)
                                                {
                                                    // This is a real commodity
                                                    commodities.Add(new CommodityAmount(commodity, (int)(long)used.Value));
                                                }
                                                else
                                                {
                                                    // Probably a material then
                                                    Material material = Material.FromEDName(used.Key);
                                                    materials.Add(new MaterialAmount(material, (int)(long)used.Value));
                                                }
                                            }
                                        }
                                        else if (val is List<object> materialsJson) // 2.3 style
                                        {
                                            foreach (Dictionary<string, object> materialJson in materialsJson)
                                            {
                                                Material material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                                materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                            }
                                        }
                                    }
                                    events.Add(new ModificationCraftedEvent(timestamp, engineer, engineerId, blueprintpEdName, blueprintId, level, quality, experimentalEffect, materials, commodities, compartment) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EngineerProgress":
                                {
                                    data.TryGetValue("Engineers", out object val);
                                    if (val != null)
                                    {
                                        // This is a startup entry. 
                                        // Update engineer progress / status data
                                        List<object> engineers = (List<object>)val;
                                        foreach (IDictionary<string, object> engineerData in engineers)
                                        {
                                            Engineer engineer = parseEngineer(engineerData);
                                            if (!string.IsNullOrEmpty(engineer.name))
                                            {
                                                Engineer.AddOrUpdate(engineer);
                                            }
                                        }
                                        // Generate an event to pass the data
                                        events.Add(new EngineerProgressedEvent(timestamp, null, null) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // This is a progress entry.
                                        Engineer engineer = parseEngineer(data);
                                        Engineer lastEngineer = Engineer.FromNameOrId(engineer.name, engineer.id).Copy();
                                        Engineer.AddOrUpdate(engineer);
                                        if (engineer.stage != null && engineer.stage != lastEngineer?.stage)
                                        {
                                            events.Add(new EngineerProgressedEvent(timestamp, engineer, "Stage") { raw = line, fromLoad = fromLogLoad });
                                        }
                                        if (engineer.rank != null && engineer.rank != lastEngineer?.rank)
                                        {
                                            events.Add(new EngineerProgressedEvent(timestamp, engineer, "Rank") { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "LoadGame":
                                {
                                    string commander = JsonParsing.getString(data, "Commander");
                                    string frontierID = JsonParsing.getString(data, "FID");

                                    // Active expansions
                                    bool horizons = JsonParsing.getOptionalBool(data, "Horizons") ?? false; // Whether the account has the Horizons DLC
                                    bool odyssey = JsonParsing.getOptionalBool(data, "Odyssey") ?? false; // Whether the account has the Odyssey DLC
                                    Logging.Info($"Active expansions... Horizons: {horizons}, Odyssey: {odyssey}.");

                                    string shipEDModel = JsonParsing.getString(data, "Ship"); // This describes a vehicle, whether ship or otherwise.
                                                                                       // If on foot this may be a suit & if in an SRV then this may be an SRV.
                                    string shipName = JsonParsing.getString(data, "ShipName");
                                    string shipIdent = JsonParsing.getString(data, "ShipIdent");
                                    long? shipId = JsonParsing.getOptionalLong(data, "ShipID"); // If on foot we'll get a suit ID here, which we need to treat as a long

                                    // shipId may be null either if we're logging into CQC or if we're logging in while in an Apex taxi service
                                    if (shipId == null)
                                    {
                                        if (!string.IsNullOrEmpty(shipEDModel) && shipEDModel.ToLowerInvariant().Contains("taxi"))
                                        {
                                            // This is a taxi
                                        }
                                        else
                                        {
                                            // The LoadGame event for entering CQC contains no ship details.
                                            // We are entering CQC. Flag it back to EDDI so we can ignore everything that happens until
                                            // we're out of CQC again
                                            events.Add(new EnteredCQCEvent(timestamp, commander) { raw = line, fromLoad = fromLogLoad });
                                            handled = true;
                                            break;
                                        }
                                    }
                                    
                                    bool? startedLanded = JsonParsing.getOptionalBool(data, "StartedLanded");
                                    bool? startDead = JsonParsing.getOptionalBool(data, "StartDead");

                                    long credits = (long)JsonParsing.getOptionalLong(data, "Credits");
                                    long loan = (long)JsonParsing.getOptionalLong(data, "Loan");

                                    decimal? fuel = JsonParsing.getOptionalDecimal(data, "FuelLevel");
                                    decimal? fuelCapacity = JsonParsing.getOptionalDecimal(data, "FuelCapacity");

                                    string version = JsonParsing.getString(data, "gameversion")?.Trim();
                                    string build = JsonParsing.getString(data, "build")?.Trim();

                                    GameMode mode = GameMode.FromEDName(JsonParsing.getString(data, "GameMode"));
                                    string group = JsonParsing.getString(data, "Group"); // The name of the group, only if the mode is "Group" 

                                    events.Add(new CommanderContinuedEvent(timestamp, commander, frontierID, horizons, odyssey, shipId, shipEDModel, shipName, shipIdent, startedLanded, startDead, mode, group, credits, loan, fuel, fuelCapacity, version, build) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewHire":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    long crewid = JsonParsing.getLong(data, "CrewID");
                                    string faction = GetFactionName(data, "Faction");
                                    long price = JsonParsing.getLong(data, "Cost");
                                    CombatRating rating = CombatRating.FromRank(JsonParsing.getInt(data, "CombatRank"));
                                    events.Add(new CrewHiredEvent(timestamp, name, crewid, faction, price, rating) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewFire":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    long crewid = JsonParsing.getLong(data, "CrewID");
                                    events.Add(new CrewFiredEvent(timestamp, name, crewid) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewAssign":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    long crewid = JsonParsing.getLong(data, "CrewID");
                                    string role = getRole(data, "Role");
                                    events.Add(new CrewAssignedEvent(timestamp, name, crewid, role) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "NpcCrewPaidWage":
                                {
                                    string name = JsonParsing.getString(data, "NpcCrewName");
                                    long crewid = JsonParsing.getLong(data, "NpcCrewId");
                                    long amount = JsonParsing.getLong(data, "Amount");
                                    events.Add(new CrewPaidWageEvent(timestamp, name, crewid, amount) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "NpcCrewRank":
                                {
                                    string name = JsonParsing.getString(data, "NpcCrewName");
                                    long crewid = JsonParsing.getLong(data, "NpcCrewId");
                                    data.TryGetValue("RankCombat", out object val);
                                    CombatRating rating = CombatRating.FromRank(Convert.ToInt32(val));
                                    events.Add(new CrewPromotionEvent(timestamp, name, crewid, rating) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "JoinACrew":
                                {
                                    string captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewJoinedEvent(timestamp, captain, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "QuitACrew":
                                {
                                    string captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewLeftEvent(timestamp, captain, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ChangeCrewRole":
                                {
                                    string role = getRole(data, "Role");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewRoleChangedEvent(timestamp, role, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberJoins":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberJoinedEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberQuits":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberLeftEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewLaunchFighter":
                                {
                                    string name = JsonParsing.getString(data, "Crew");
                                    int fighterId = JsonParsing.getInt(data, "ID");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberLaunchedEvent(timestamp, name, fighterId, telepresence) { raw = line, fromLoad = fromLogLoad });

                                }
                                handled = true;
                                break;
                            case "CrewMemberRoleChange":
                                {
                                    string name = JsonParsing.getString(data, "Crew");
                                    string role = getRole(data, "Role");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberRoleChangedEvent(timestamp, name, role, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "KickCrewMember":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberRemovedEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EndCrewSession":
                                {
                                    var onCrime = JsonParsing.getOptionalBool(data, "OnCrime");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewSessionEndedEvent(timestamp, onCrime, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BuyAmmo":
                                {
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;
                                    events.Add(new ShipRestockedEvent(timestamp, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BuyDrones":
                                {
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;
                                    data.TryGetValue("BuyPrice", out val);
                                    int price = (int)(long)val;
                                    events.Add(new LimpetPurchasedEvent(timestamp, amount, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SellDrones":
                                {
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;
                                    data.TryGetValue("SellPrice", out val);
                                    int price = (int)(long)val;
                                    events.Add(new LimpetSoldEvent(timestamp, amount, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LaunchDrone":
                                {
                                    string kind = JsonParsing.getString(data, "Type");
                                    events.Add(new LimpetLaunchedEvent(timestamp, kind) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ClearSavedGame":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    string frontierID = JsonParsing.getString(data, "FID");
                                    events.Add(new ClearedSaveEvent(timestamp, name, frontierID) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "NewCommander":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    string frontierID = JsonParsing.getString(data, "FID");
                                    string package = JsonParsing.getString(data, "Package");
                                    events.Add(new CommanderStartedEvent(timestamp, name, frontierID, package) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Progress":
                                {
                                    data.TryGetValue("Combat", out object val);
                                    decimal combat = (long?)val ?? 0;
                                    data.TryGetValue("Trade", out val);
                                    decimal trade = (long?)val ?? 0;
                                    data.TryGetValue("Explore", out val);
                                    decimal exploration = (long?)val ?? 0;
                                    data.TryGetValue("CQC", out val);
                                    decimal cqc = (long?)val ?? 0;
                                    data.TryGetValue("Empire", out val);
                                    decimal empire = (long?)val ?? 0;
                                    data.TryGetValue("Federation", out val);
                                    decimal federation = (long?)val ?? 0;
                                    data.TryGetValue("Soldier", out val);
                                    decimal soldier = (long?)val ?? 0;
                                    data.TryGetValue("Exobiologist", out val);
                                    decimal exobiologist = (long?)val ?? 0;

                                    events.Add(new CommanderProgressEvent(timestamp, combat, trade, exploration, cqc, empire, federation, soldier, exobiologist) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Rank":
                                {
                                    data.TryGetValue("Combat", out object val);
                                    CombatRating combat = CombatRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Trade", out val);
                                    TradeRating trade = TradeRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Explore", out val);
                                    ExplorationRating exploration = ExplorationRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("CQC", out val);
                                    CQCRating cqc = CQCRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Empire", out val);
                                    EmpireRating empire = EmpireRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Federation", out val);
                                    FederationRating federation = FederationRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Soldier", out val);
                                    MercenaryRating mercenary = MercenaryRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Exobiologist", out val);
                                    ExobiologistRating exobiologist = ExobiologistRating.FromRank((int)((long?)val ?? 0));

                                    events.Add(new CommanderRatingsEvent(timestamp, combat, trade, exploration, cqc, empire, federation, mercenary, exobiologist) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Screenshot":
                                {
                                    string filename = JsonParsing.getString(data, "Filename");
                                    data.TryGetValue("Width", out object val);
                                    int width = (int)(long)val;
                                    data.TryGetValue("Height", out val);
                                    int height = (int)(long)val;
                                    string system = JsonParsing.getString(data, "System");
                                    string body = JsonParsing.getString(data, "Body");
                                    decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    events.Add(new ScreenshotEvent(timestamp, filename, width, height, system, body, longitude, latitude) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BuyTradeData":
                                {
                                    string system = JsonParsing.getString(data, "System");
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;

                                    events.Add(new TradeDataPurchasedEvent(timestamp, system, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PayBounties":
                                {
                                    data.TryGetValue("Amount", out object val);
                                    long amount = (long)val;
                                    decimal? brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");
                                    bool allBounties = JsonParsing.getOptionalBool(data, "AllFines") ?? false;
                                    string faction = GetFactionName(data, "Faction");
                                    int shipId = 0;
                                    var shipIdLong = JsonParsing.getLong(data, "ShipID");
                                    if (shipIdLong > 4293000000)
                                    {
                                        // This is a suit loadout ID. Use a null value since bounties associated with the commander, rather than the ship, are being paid.
                                        shipIdLong = -1;
                                    }
                                    else
                                    {
                                        shipId = (int)shipIdLong;
                                    }

                                    events.Add(new BountyPaidEvent(timestamp, amount, brokerpercentage, allBounties, faction, shipId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PayFines":
                                {
                                    data.TryGetValue("Amount", out object val);
                                    long amount = (long)val;
                                    decimal? brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");
                                    bool allFines = JsonParsing.getOptionalBool(data, "AllFines") ?? false;
                                    string faction = GetFactionName(data, "Faction");
                                    int shipId = 0;
                                    var shipIdLong = JsonParsing.getLong(data, "ShipID");
                                    if (shipIdLong >= 4293000000)
                                    {
                                        // This is a suit loadout ID. Use a -1 value to signal that fines associated with the commander, rather than the ship, are being paid.
                                        shipIdLong = -1;
                                    }
                                    else
                                    {
                                        shipId = (int)shipIdLong;
                                    }

                                    events.Add(new FinePaidEvent(timestamp, amount, brokerpercentage, allFines, faction, shipId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RefuelPartial":
                                {
                                    decimal amount = JsonParsing.getDecimal(data, "Amount");
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null, false) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RefuelAll":
                                {
                                    decimal amount = JsonParsing.getDecimal(data, "Amount");
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null, true) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FuelScoop":
                                {
                                    decimal amount = JsonParsing.getDecimal(data, "Scooped");
                                    decimal total = JsonParsing.getDecimal(data, "Total");
                                    events.Add(new ShipRefuelledEvent(timestamp, "Scoop", null, amount, total) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Friends":
                                {
                                    string status = JsonParsing.getString(data, "Status");
                                    string name = JsonParsing.getString(data, "Name");
                                    name = name.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    events.Add(new FriendsEvent(timestamp, name, status) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "JetConeBoost":
                                {
                                    decimal boost = JsonParsing.getDecimal(data, "BoostValue");
                                    events.Add(new JetConeBoostEvent(timestamp, boost) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "JetConeDamage":
                                {
                                    string modulename = JsonParsing.getString(data, "Module");
                                    Module module = Module.FromEDName(modulename);
                                    if (module != null)
                                    {
                                        if (module.Mount != null)
                                        {
                                            // This is a weapon so provide a bit more information
                                            string mount = module.mount;
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
                            case "RedeemVoucher":
                                {

                                    string type = JsonParsing.getString(data, "Type");
                                    List<Reward> rewards = new List<Reward>();

                                    // Obtain list of factions
                                    data.TryGetValue("Factions", out object val);
                                    List<object> factionsData = (List<object>)val;
                                    if (factionsData != null)
                                    {
                                        foreach (Dictionary<string, object> rewardData in factionsData)
                                        {
                                            string factionName = GetFactionName(rewardData, "Faction");
                                            rewardData.TryGetValue("Amount", out val);
                                            long factionReward = (long)val;

                                            rewards.Add(new Reward(factionName, factionReward));
                                        }
                                    }
                                    else
                                    {
                                        string factionName = GetFactionName(data, "Faction");
                                        data.TryGetValue("Amount", out val);
                                        long factionReward = (long)val;

                                        rewards.Add(new Reward(factionName, factionReward));
                                    }
                                    data.TryGetValue("Amount", out val);
                                    long amount = (long)val;

                                    decimal? brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");

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
                                    else if (type == "codex" || type == "settlement" || type == "scannable")
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
                            case "CommunityGoal":
                                {
                                    // There may be multiple goals in each event.
                                    data.TryGetValue("CurrentGoals", out object goalsVal);
                                    string goalsJson = JsonConvert.SerializeObject(goalsVal);
                                    List<CommunityGoal> goals = JsonConvert.DeserializeObject<List<CommunityGoal>>(goalsJson);
                                    events.Add(new CommunityGoalsEvent(timestamp, goals) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommunityGoalJoin":
                                {
                                    long cgid = JsonParsing.getLong(data, "CGID");
                                    string name = JsonParsing.getString(data, "Name");
                                    string system = JsonParsing.getString(data, "System");

                                    var mission = new Mission(cgid, "MISSION_CommunityGoal", null, MissionStatus.Active)
                                    {
                                        localisedname = name,
                                        destinationsystem = system,
                                        originsystem = system,
                                        communal = true
                                    };

                                    events.Add(new MissionAcceptedEvent(timestamp, mission) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommunityGoalDiscard":
                                {
                                    long cgid = JsonParsing.getLong(data, "CGID");

                                    events.Add(new MissionAbandonedEvent(timestamp, cgid, "MISSION_CommunityGoal", 0) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommunityGoalReward":
                                {
                                    long cgid = JsonParsing.getLong(data, "CGID");
                                    string name = JsonParsing.getString(data, "Name");
                                    string system = JsonParsing.getString(data, "System");
                                    data.TryGetValue("Reward", out object val);
                                    long reward = (val == null ? 0 : (long)val);

                                    events.Add(new MissionCompletedEvent(timestamp, cgid, "MISSION_CommunityGoal", name, null, null, null, true, reward, null, null, null, null, null, 0) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CargoDepot":
                                {
                                    var missionid = JsonParsing.getLong(data, "MissionID");
                                    var updatetype = JsonParsing.getString(data, "UpdateType");
                                    var startmarketid = JsonParsing.getLong(data, "StartMarketID");
                                    var endmarketid = JsonParsing.getLong(data, "EndMarketID");
                                    var collected = JsonParsing.getInt(data, "ItemsCollected");
                                    var delivered = JsonParsing.getInt(data, "ItemsDelivered");
                                    var totaltodeliver = JsonParsing.getInt(data, "TotalItemsToDeliver");

                                    // Not available in 'WingUpdate'
                                    var commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "CargoType"));
                                    var amount = JsonParsing.getOptionalInt(data, "Count");

                                    // The Progress value represents pending progress for goods in transit: (ItemsCollected-ItemsDelivered)/TotalItemsToDeliver

                                    events.Add(new CargoDepotEvent(timestamp, missionid, updatetype, commodity, amount, startmarketid, endmarketid, collected, delivered, totaltodeliver) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Missions":
                                {
                                    var possibleStatuses = new [] 
                                    {
                                        MissionStatus.Active, 
                                        MissionStatus.Failed, 
                                        MissionStatus.Complete
                                    };

                                    List<Mission> missions = new List<Mission>();
                                    foreach (var status in possibleStatuses)
                                    {
                                        data.TryGetValue(status.invariantName, out object val);
                                        List<object> missionLog = (List<object>)val;

                                        foreach (object mission in missionLog)
                                        {
                                            Dictionary<string, object> missionProperties = (Dictionary<string, object>)mission;
                                            long missionId = JsonParsing.getLong(missionProperties, "MissionID");
                                            string name = JsonParsing.getString(missionProperties, "Name");
                                            decimal expires = JsonParsing.getDecimal(missionProperties, "Expires");
                                            DateTime expiry = DateTime.UtcNow.AddSeconds((double)expires);

                                            // If mission is 'Active' and expires = 0, then set status to 'Claim'
                                            MissionStatus missionStatus = status == MissionStatus.Active && expires == 0 
                                                ? MissionStatus.Claim: 
                                                status;
                                            Mission newMission = new Mission(missionId, name, expiry, missionStatus);
                                            if (newMission == null)
                                            {
                                                // Mal-formed mission
                                                Logging.Error("Bad mission entry", mission);
                                            }
                                            else
                                            {
                                                missions.Add(newMission);
                                            }
                                        }
                                    }
                                    events.Add(new MissionsEvent(timestamp, missions) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Passengers":
                                {
                                    List<Passenger> passengers = new List<Passenger>();
                                    data.TryGetValue("Manifest", out object val);
                                    List<object> passengerManifest = (List<object>)val;

                                    foreach (object passenger in passengerManifest)
                                    {
                                        Dictionary<string, object> passengerProperties = (Dictionary<string, object>)passenger;
                                        long missionid = JsonParsing.getLong(passengerProperties, "MissionID");
                                        string type = JsonParsing.getString(passengerProperties, "Type");
                                        bool vip = JsonParsing.getBool(passengerProperties, "VIP");
                                        bool wanted = JsonParsing.getBool(passengerProperties, "Wanted");
                                        int amount = JsonParsing.getInt(passengerProperties, "Count");

                                        Passenger newPassenger = new Passenger(missionid, type, vip, wanted, amount);
                                        if (newPassenger == null)
                                        {
                                            // Mal-formed mission
                                            Logging.Error("Bad mission entry", passenger);
                                        }
                                        else
                                        {
                                            passengers.Add(newPassenger);
                                        }
                                    }
                                    events.Add(new PassengersEvent(timestamp, passengers) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MissionAccepted":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    var missionid = (long)val;
                                    data.TryGetValue("Expiry", out val);
                                    DateTime? expiry = (val == null ? (DateTime?)null : (DateTime)val);
                                    var name = JsonParsing.getString(data, "Name");
                                    var localisedname = JsonParsing.getString(data, "LocalisedName");
                                    if (!string.IsNullOrEmpty(localisedname))
                                    {
                                        localisedname = Regex.Replace(localisedname, @"<.*?>", ""); // Mission localized names may have embedded HTML tags. If so then remove them.
                                    }
                                    var faction = GetFactionName(data, "Faction");
                                    var reward = JsonParsing.getOptionalInt(data, "Reward");
                                    var wing = JsonParsing.getBool(data, "Wing");

                                    if (name == "MISSION_genericPermit1")
                                    {
                                        // This is a permit mission where the permit is granted immediately once it is accepted.
                                        // There are no other mission related events generated from this (no mission completion event).
                                        events.Add(new PermitAcquiredEvent(timestamp, faction) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // Missions with destinations
                                        var destinationsystem = JsonParsing.getString(data, "DestinationSystem");
                                        var destinationstation = JsonParsing.getString(data, "DestinationStation");
                                        var destinationsettlement = JsonParsing.getString(data, "DestinationSettlement");

                                        // Missions with commodities (which may include on-foot micro-resources)
                                        var c = JsonParsing.getString(data, "Commodity");
                                        var fallbackC = JsonParsing.getString(data, "Commodity_Localised");
                                        CommodityDefinition commodity = null;
                                        MicroResource microResource = null;

                                        if (!string.IsNullOrEmpty(c))
                                        {
                                            if (MicroResource.EDNameExists(c))
                                            {
                                                // This is an on-foot micro-resource
                                                microResource = MicroResource.FromEDName(c);
                                                microResource.fallbackLocalizedName = fallbackC;
                                            }
                                            else
                                            {
                                                // This is (probably) a traditional ship commodity
                                                commodity = CommodityDefinition.FromEDName(c);
                                                commodity.fallbackLocalizedName = fallbackC;
                                            }
                                        }
                                        data.TryGetValue("Count", out val);
                                        var amount = (int?)(long?)val;

                                        // Missions with targets
                                        var target = JsonParsing.getString(data, "Target");
                                        var targettype = JsonParsing.getString(data, "TargetType");
                                        var targetfaction = GetFactionName(data, "TargetFaction");
                                        data.TryGetValue("KillCount", out val);
                                        if (val != null)
                                        {
                                            amount = (int?)(long?)val;
                                        }

                                        // Missions with passengers
                                        var passengercount = JsonParsing.getOptionalInt(data, "PassengerCount");
                                        var passengertype = JsonParsing.getString(data, "PassengerType");
                                        var passengerswanted = JsonParsing.getOptionalBool(data, "PassengerWanted");
                                        var passengervips = JsonParsing.getOptionalBool(data, "PassengerVIPs");
                                        data.TryGetValue("PassengerCount", out val);
                                        if (val != null)
                                        {
                                            amount = (int?)(long?)val;
                                        }

                                        // Impact on influence and reputation
                                        var influence = JsonParsing.getString(data, "Influence");
                                        var reputation = JsonParsing.getString(data, "Reputation");

                                        var mission = new Mission(missionid, name, expiry, MissionStatus.Active)
                                        {
                                            // Common parameters
                                            localisedname = localisedname,
                                            amount = amount ?? 0,
                                            influence = influence,
                                            reputation = reputation,
                                            reward = reward ?? 0,
                                            communal = false,

                                            // Get the minor faction stuff
                                            faction = faction,

                                            // Set mission origin to to the current system & station
                                            originsystem = EDDI.Instance.CurrentStarSystem?.systemname,
                                            originstation = EDDI.Instance?.CurrentStation?.name,

                                            // Missions with engineering rewards
                                            CommodityDefinition = commodity,
                                            MicroResourceDefinition = microResource,

                                            // Missions with targets
                                            targetTypeEDName = targettype?.Split('_')?
                                                .ElementAtOrDefault(2)?.Replace(";", ""),
                                            target = target,
                                            targetfaction = targetfaction,

                                            // Missions with passengers
                                            passengertypeEDName = passengertype,
                                            passengervips = passengervips,
                                            passengerwanted = passengerswanted
                                        };

                                        // Missions with multiple destinations
                                        if ( destinationsystem != null && destinationsystem.Contains( "$MISSIONUTIL_MULTIPLE" ) )
                                        {
                                            // If 'chained' mission, get the destination systems
                                            string[] systems = destinationsystem
                                                .Replace("$MISSIONUTIL_MULTIPLE_INNER_SEPARATOR;", "#")
                                                .Replace("$MISSIONUTIL_MULTIPLE_FINAL_SEPARATOR;", "#")
                                                .Split('#');

                                            var starSystems = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystems(systems, true, false);
                                            foreach ( var system in starSystems )
                                            {
                                                if ( !string.IsNullOrEmpty( system.systemname ) &&
                                                     system.x is decimal sx &&
                                                     system.y is decimal sy &&
                                                     system.z is decimal sz )
                                                {
                                                    var dest = new NavWaypoint( system.systemname, sx, sy, sz );
                                                    dest.missionids.Add( mission.missionid );
                                                    mission.destinationsystems.Add( dest );
                                                }
                                            }

                                            // Load the first destination system.
                                            mission.destinationsystem = mission.destinationsystems.ElementAtOrDefault( 0 ).systemName;
                                        }
                                        else
                                        {
                                            // Populate destination system and station, depending on mission type
                                            if ( mission.tagsList.Contains( MissionType.Altruism ) )
                                            {
                                                mission.destinationsystem = mission.originsystem;
                                                mission.destinationstation = mission.originstation;
                                            }
                                            else
                                            {
                                                mission.destinationsystem = destinationsystem;
                                                mission.destinationstation = destinationstation ?? destinationsettlement;
                                            }
                                        }

                                        events.Add(new MissionAcceptedEvent(timestamp, mission) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "MissionCompleted":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    var missionid = (long)val;
                                    var name = JsonParsing.getString(data, "Name");
                                    data.TryGetValue("Reward", out val);
                                    var reward = (val == null ? 0 : (long)val);
                                    var donation = JsonParsing.getOptionalLong(data, "Donated") ?? 0;
                                    var faction = GetFactionName(data, "Faction");

                                    // Missions with commodities (which may include on-foot micro-resources)
                                    var c = JsonParsing.getString(data, "Commodity");
                                    var fallbackC = JsonParsing.getString(data, "Commodity_Localised");
                                    CommodityDefinition commodity = null;
                                    MicroResource microResource = null;

                                    if (!string.IsNullOrEmpty(c))
                                    {
                                        if (MicroResource.EDNameExists(c))
                                        {
                                            // This is an on-foot micro-resource
                                            microResource = MicroResource.FromEDName(c);
                                            microResource.fallbackLocalizedName = fallbackC;
                                        }
                                        else
                                        {
                                            // This is (probably) a traditional ship commodity
                                            commodity = CommodityDefinition.FromEDName(c);
                                            commodity.fallbackLocalizedName = fallbackC;
                                        }
                                    }
                                    data.TryGetValue("Count", out val);
                                    var amount = (int?)(long?)val;

                                    var permitsAwarded = new List<string>();
                                    data.TryGetValue("PermitsAwarded", out val);
                                    var permitsAwardedData = (List<object>)val;
                                    if (permitsAwardedData != null)
                                    {
                                        foreach (Dictionary<string, object> permitAwardedData in permitsAwardedData)
                                        {
                                            string permitAwarded = JsonParsing.getString(permitAwardedData, "Name");
                                            permitsAwarded.Add(permitAwarded);
                                        }
                                    }

                                    var commodityrewards = new List<CommodityAmount>();
                                    data.TryGetValue("CommodityReward", out val);
                                    var commodityRewardsData = (List<object>)val;
                                    if (commodityRewardsData != null)
                                    {
                                        foreach (Dictionary<string, object> commodityRewardData in commodityRewardsData)
                                        {
                                            CommodityDefinition rewardCommodity = CommodityDefinition.FromEDName(JsonParsing.getString(commodityRewardData, "Name"));
                                            commodityRewardData.TryGetValue("Count", out val);
                                            int count = (int)(long)val;
                                            commodityrewards.Add(new CommodityAmount(rewardCommodity, count));
                                        }
                                    }

                                    var materialsrewards = new List<MaterialAmount>();
                                    var microResourceRewards = new List<MicroResourceAmount>();
                                    data.TryGetValue("MaterialsReward", out val);
                                    var materialsRewardsData = (List<object>)val;
                                    if (materialsRewardsData != null)
                                    {
                                        foreach (Dictionary<string, object> materialsRewardData in materialsRewardsData)
                                        {
                                            var m = JsonParsing.getString(materialsRewardData, "Name");
                                            var fallbackM = JsonParsing.getString(materialsRewardData, "Name_Localised");
                                            materialsRewardData.TryGetValue("Count", out val);
                                            int count = (int)(long)val;

                                            if (!string.IsNullOrEmpty(m))
                                            {
                                                if (MicroResource.EDNameExists(m))
                                                {
                                                    // This is an on-foot micro-resource
                                                    microResourceRewards.Add(new MicroResourceAmount(m, null, count, null, fallbackM));
                                                }
                                                else
                                                {
                                                    // This is (probably) a traditional ship material
                                                    var rewardMaterial = Material.FromEDName(m);
                                                    rewardMaterial.fallbackLocalizedName = fallbackM;
                                                    materialsrewards.Add(new MaterialAmount(rewardMaterial, count));
                                                }
                                            }
                                        }
                                    }

                                    var missionFactionEffects = new List<MissionFactionEffect>();
                                    data.TryGetValue("FactionEffects", out val);
                                    var missionFactionEffectsData = (List<object>)val;
                                    if (missionFactionEffectsData != null)
                                    {
                                        foreach (Dictionary<string, object> missionFactionEffectData in missionFactionEffectsData)
                                        {
                                            var effectFaction = JsonParsing.getString(missionFactionEffectData, "Faction");
                                            var reputationPlusses = JsonParsing.getString(missionFactionEffectData, "Reputation");

                                            var effects = new List<MissionEffect>();
                                            missionFactionEffectData.TryGetValue("Effects", out val);
                                            var effectsData = (List<object>)val;
                                            if (effectsData != null)
                                            {
                                                foreach (Dictionary<string, object> effectData in effectsData)
                                                {
                                                    var edEffect = JsonParsing.getString(effectData, "Effect");
                                                    var localizedEffect = JsonParsing.getString(effectData, "Effect_Localised");
                                                    effects.Add(new MissionEffect(edEffect, localizedEffect));
                                                }
                                            }

                                            var influences = new List<MissionInfluence>();
                                            missionFactionEffectData.TryGetValue("Influence", out val);
                                            var influencesData = (List<object>)val;
                                            if (influencesData != null)
                                            {
                                                foreach (Dictionary<string, object> influenceData in influencesData)
                                                {
                                                    var influencedSystemAddress = JsonParsing.getULong(influenceData, "SystemAddress");
                                                    var influencePlusses = JsonParsing.getString(influenceData, "Influence");
                                                    influences.Add(new MissionInfluence(influencedSystemAddress, influencePlusses));
                                                }
                                            }
                                            
                                            missionFactionEffects.Add(new MissionFactionEffect(effectFaction, effects, influences, reputationPlusses));
                                        }
                                    }

                                    events.Add(new MissionCompletedEvent(timestamp, missionid, name, faction, microResource, commodity, amount, false, reward, permitsAwarded, commodityrewards, materialsrewards, microResourceRewards, missionFactionEffects, donation) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MissionAbandoned":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    long missionid = (long)val;
                                    string name = JsonParsing.getString(data, "Name");
                                    data.TryGetValue("Fine", out val);
                                    long fine = val == null ? 0 : (long)val;
                                    events.Add(new MissionAbandonedEvent(timestamp, missionid, name, fine) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MissionRedirected":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    long missionid = (long)val;
                                    string name = JsonParsing.getString(data, "Name");
                                    string newdestinationstation = JsonParsing.getString(data, "NewDestinationStation");
                                    string olddestinationstation = JsonParsing.getString(data, "OldDestinationStation");
                                    string newdestinationsystem = JsonParsing.getString(data, "NewDestinationSystem");
                                    string olddestinationsystem = JsonParsing.getString(data, "OldDestinationSystem");
                                    events.Add(new MissionRedirectedEvent(timestamp, missionid, name, newdestinationstation, olddestinationstation, newdestinationsystem, olddestinationsystem) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MissionFailed":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    long missionid = (long)val;
                                    string name = JsonParsing.getString(data, "Name");
                                    data.TryGetValue("Fine", out val);
                                    long fine = val == null ? 0 : (long)val;
                                    events.Add(new MissionFailedEvent(timestamp, missionid, name, fine) { raw = line, fromLoad = fromLogLoad });

                                }
                                handled = true;
                                break;
                            case "SearchAndRescue":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string commodityName = JsonParsing.getString(data, "Name");
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Name"));
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    data.TryGetValue("Count", out object val);
                                    int? amount = (int?)(long?)val;
                                    data.TryGetValue("Reward", out val);
                                    long reward = (val == null ? 0 : (long)val);
                                    events.Add(new SearchAndRescueEvent(timestamp, commodity, amount, reward, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "AfmuRepairs":
                                {
                                    string item = JsonParsing.getString(data, "Module");
                                    // Item might be a module
                                    Module module = Module.FromEDName(item);
                                    if (module != null)
                                    {
                                        if (module.Mount != null)
                                        {
                                            // This is a weapon so provide a bit more information
                                            string mount;
                                            if (module.Mount == ModuleMount.Fixed)
                                            {
                                                mount = "fixed";
                                            }
                                            else if (module.Mount == ModuleMount.Gimballed)
                                            {
                                                mount = "gimballed";
                                            }
                                            else
                                            {
                                                mount = "turreted";
                                            }
                                            item = "" + module.@class.ToString() + module.grade + " " + mount + " " + module.localizedName;
                                        }
                                        else
                                        {
                                            item = module.localizedName;
                                        }
                                    }

                                    // There is an FDev bug that can set `FullyRepaired` to false even when the module health is full,
                                    // so we work around this by relying on the `Health` property rather than the `FullyRepaired` property.
                                    // This appears to be a unique problem with Module Reinforcement Packages.

                                    decimal health = JsonParsing.getDecimal(data, "Health");
                                    bool repairedfully = health == 1M;

                                    events.Add(new ShipAfmuRepairedEvent(timestamp, item, repairedfully, health) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Repair":
                                {
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;

                                    // Starting with version 3.7, the "Repair" event may contain one item or multiple items
                                    // Each item is either a description (e.g. all, wear, hull, paint) or the name of a module
                                    data.TryGetValue("Items", out object itemsVal);
                                    if (itemsVal != null)
                                    {
                                        if (itemsVal is List<object> itemEDNames)
                                        {
                                            events.Add(new ShipRepairedEvent(timestamp, itemEDNames.ConvertAll(o => o.ToString()).ToList(), price) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                    else
                                    {
                                        // We have a single "item"
                                        string itemEDName = JsonParsing.getString(data, "Item");
                                        if (!string.IsNullOrEmpty(itemEDName))
                                        {
                                            events.Add(new ShipRepairedEvent(timestamp, itemEDName, price) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "RepairDrone":
                                {
                                    decimal? hull = JsonParsing.getOptionalDecimal(data, "HullRepaired");
                                    decimal? cockpit = JsonParsing.getOptionalDecimal(data, "CockpitRepaired");
                                    decimal? corrosion = JsonParsing.getOptionalDecimal(data, "CorrosionRepaired");

                                    events.Add(new ShipRepairDroneEvent(timestamp, hull, cockpit, corrosion) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RepairAll":
                                {
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;
                                    events.Add(new ShipRepairedEvent(timestamp, "All", price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RebootRepair":
                                {
                                    // This event returns a list of slots rather than actual module ednames.
                                    List<string> slotsJson = null;
                                    data.TryGetValue( "Modules", out var val );
                                    if ( val is List<string> ls )
                                    {
                                        slotsJson = ls;
                                    }
                                    else if ( val is List<object> lo )
                                    {
                                        slotsJson = lo.Select( o => o.ToString() ).ToList();
                                    }

                                    var ship = EDDI.Instance.CurrentShip;
                                    List<Module> modules = new List<Module>();
                                    foreach (var slot in slotsJson)
                                    {
                                        Module module = null;
                                        if (slot.Contains("CargoHatch"))
                                        {
                                            module = ship.cargohatch;
                                        }
                                        else if (slot.Contains("FrameShiftDrive"))
                                        {
                                            module = ship.frameshiftdrive;
                                        }
                                        else if (slot.Contains("LifeSupport"))
                                        {
                                            module = ship.lifesupport;
                                        }
                                        else if (slot.Contains("MainEngines"))
                                        {
                                            module = ship.thrusters;
                                        }
                                        else if (slot.Contains("PowerDistributor"))
                                        {
                                            module = ship.powerdistributor;
                                        }
                                        else if (slot.Contains("PowerPlant"))
                                        {
                                            module = ship.powerplant;
                                        }
                                        else if (slot.Contains("Radar"))
                                        {
                                            module = ship.sensors;
                                        }
                                        else if (slot.Contains("Hardpoint"))
                                        {
                                            module = ship.hardpoints.SingleOrDefault(h => h.name == slot)?.module;
                                        }
                                        else if (slot.Contains("Slot") || slot.Contains("Military"))
                                        {
                                            module = ship.compartments.SingleOrDefault(c => c.name == slot)?.module;
                                        }

                                        if (module != null) { modules.Add(module); }
                                    }

                                    events.Add(new ShipRebootedEvent(timestamp, modules) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Synthesis":
                                {
                                    string synthesis = JsonParsing.getString(data, "Name");

                                    data.TryGetValue("Materials", out object val);
                                    List<MaterialAmount> materials = new List<MaterialAmount>();
                                    // 2.2 style
                                    if (val is Dictionary<string, object> materialsData)
                                    {
                                        if (materialsData != null)
                                        {
                                            foreach (KeyValuePair<string, object> materialData in materialsData)
                                            {
                                                Material material = Material.FromEDName(materialData.Key);
                                                materials.Add(new MaterialAmount(material, (int)(long)materialData.Value));
                                            }
                                        }
                                    }
                                    else if (val is List<object> materialsJson) // 2.3 style
                                    {
                                        foreach (Dictionary<string, object> materialJson in materialsJson)
                                        {
                                            Material material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }

                                    events.Add(new SynthesisedEvent(timestamp, synthesis, materials) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Materials":
                                {
                                    List<MaterialAmount> materials = new List<MaterialAmount>();

                                    data.TryGetValue("Raw", out object val);
                                    if (val != null)
                                    {
                                        List<object> materialsJson = (List<object>)val;
                                        foreach (Dictionary<string, object> materialJson in materialsJson)
                                        {
                                            Material material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }

                                    data.TryGetValue("Manufactured", out val);
                                    if (val != null)
                                    {
                                        List<object> materialsJson = (List<object>)val;
                                        foreach (Dictionary<string, object> materialJson in materialsJson)
                                        {
                                            Material material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }

                                    data.TryGetValue("Encoded", out val);
                                    if (val != null)
                                    {
                                        List<object> materialsJson = (List<object>)val;
                                        foreach (Dictionary<string, object> materialJson in materialsJson)
                                        {
                                            Material material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }

                                    events.Add(new MaterialInventoryEvent(DateTime.UtcNow, materials) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Cargo":
                                {
                                    var inventory = new List<CargoInfoItem>();

                                    string vessel = JsonParsing.getString(data, "Vessel") ?? EDDI.Instance?.Vehicle;
                                    int cargocarried = JsonParsing.getOptionalInt(data, "Count") ?? 0;
                                    data.TryGetValue("Inventory", out object val);
                                    if (val != null)
                                    {
                                        List<object> inventoryJson = (List<object>)val;
                                        foreach (Dictionary<string, object> cargoJson in inventoryJson)
                                        {
                                            string name = JsonParsing.getString(cargoJson, "Name");
                                            long? missionid = JsonParsing.getOptionalLong(cargoJson, "MissionID");
                                            int count = JsonParsing.getInt(cargoJson, "Count");
                                            int stolen = JsonParsing.getInt(cargoJson, "Stolen");
                                            var info = new CargoInfoItem(name, missionid, count, stolen);
                                            inventory.Add(info);
                                        }
                                        events.Add(new CargoEvent(timestamp, false, vessel, inventory, cargocarried) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (CargoInfo.TryFromFile(timestamp, vessel, cargocarried, out var info, out line))
                                    {
                                        events.Add(new CargoEvent(timestamp, true, vessel, info.Inventory, cargocarried) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "NavRoute":
                                {
                                    if (NavRouteInfo.TryFromFile(timestamp, true, out NavRouteInfo navRoute, out string rawRoute))
                                    {
                                        events.Add(new NavRouteEvent(timestamp, navRoute.Route) { raw = rawRoute, fromLoad = fromLogLoad });
                                    }
                                    else if (!fromLogLoad)
                                    {
                                        Logging.Warn("NavRoute.json was not updated. The read operation timed out.");
                                    }
                                }
                                handled = true;
                                break;
                            case "NavRouteClear":
                                {
                                    if (NavRouteInfo.TryFromFile(timestamp, false, out NavRouteInfo navRoute, out string rawRoute))
                                    {
                                        events.Add(new NavRouteEvent(timestamp, navRoute.Route) { raw = rawRoute, fromLoad = fromLogLoad });
                                    }
                                    else if (!fromLogLoad)
                                    {
                                        Logging.Warn("NavRoute.json was not updated. The read operation timed out.");
                                    }
                                }
                                handled = true;
                                break;
                            case "PowerplayJoin":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));

                                    events.Add(new PowerJoinedEvent(timestamp, power) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayLeave":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));

                                    events.Add(new PowerLeftEvent(timestamp, power) { raw = line, fromLoad = fromLogLoad });

                                }
                                handled = true;
                                break;
                            case "PowerplayDefect":
                                {
                                    Power fromPower = Power.FromEDName(JsonParsing.getString(data, "FromPower"));
                                    Power toPower = Power.FromEDName(JsonParsing.getString(data, "ToPower"));

                                    events.Add(new PowerDefectedEvent(timestamp, fromPower, toPower) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayVote":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    string system = JsonParsing.getString(data, "System");
                                    data.TryGetValue("Votes", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerPreparationVoteCast(timestamp, power, system, amount) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplaySalary":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue("Amount", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerSalaryClaimedEvent(timestamp, power, amount) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayCollect":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Type"));
                                    commodity.fallbackLocalizedName = JsonParsing.getString(data, "Type_Localised");
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerCommodityObtainedEvent(timestamp, power, commodity, amount) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayDeliver":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Type"));
                                    commodity.fallbackLocalizedName = JsonParsing.getString(data, "Type_Localised");
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerCommodityDeliveredEvent(timestamp, power, commodity, amount) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayFastTrack":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue("Cost", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerCommodityFastTrackedEvent(timestamp, power, amount) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayVoucher":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue("Systems", out object val);
                                    List<string> systems = ((List<object>)val).Cast<string>().ToList();

                                    events.Add(new PowerVoucherReceivedEvent(timestamp, power, systems) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SquadronStartup":
                                {
                                    string name = JsonParsing.getString(data, "SquadronName");
                                    int rank = JsonParsing.getInt(data, "CurrentRank");

                                    events.Add(new SquadronStartupEvent(timestamp, name, rank) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "AppliedToSquadron":
                            case "DisbandedSquadron":
                            case "InvitedToSquadron":
                            case "JoinedSquadron":
                            case "KickedFromSquadron":
                            case "LeftSquadron":
                            case "SquadronCreated":
                                {
                                    string name = JsonParsing.getString(data, "SquadronName");
                                    string status = edType.Replace("Squadron", "")
                                        .Replace("To", "")
                                        .Replace("From", "")
                                        .ToLowerInvariant();

                                    events.Add(new SquadronStatusEvent(timestamp, name, status) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SquadronDemotion":
                            case "SquadronPromotion":
                                {
                                    string name = JsonParsing.getString(data, "SquadronName");
                                    int oldrank = JsonParsing.getInt(data, "OldRank");
                                    int newrank = JsonParsing.getInt(data, "NewRank");

                                    events.Add(new SquadronRankEvent(timestamp, name, oldrank, newrank) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SystemsShutdown":
                                {
                                    if ( fromLogLoad || ShipShutdownCancellationTokenSource != null )
                                    {
                                        // Ignore events triggered by loag loads.
                                        // Ignore repetitions when the ship is already in a shut-down state. 
                                    }
                                    else
                                    {
                                        events.Add( new ShipShutdownEvent( timestamp ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                }
                                handled = true;
                                break;
                            case "Fileheader":
                                {
                                    string filename = journalFileName;
                                    string version = JsonParsing.getString(data, "gameversion")?.Trim();
                                    string build = JsonParsing.getString(data, "build")?.Trim();
                                    Logging.Info($"GameVersion: {version}, Build {build}.");
                                    events.Add(new FileHeaderEvent(timestamp, filename, version, build) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Shutdown":
                                {
                                    events.Add(new ShutdownEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSDTarget":
                                {
                                    string systemName = JsonParsing.getString(data, "Name");
                                    ulong systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    int remainingJumpsInRoute = JsonParsing.getOptionalInt(data, "RemainingJumpsInRoute") ?? 0;
                                    string starclass = JsonParsing.getString(data, "StarClass");
                                    events.Add(new FSDTargetEvent(timestamp, systemName, systemAddress, remainingJumpsInRoute, starclass) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSSAllBodiesFound":
                                {
                                    string systemName = JsonParsing.getString(data, "SystemName");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    int count = JsonParsing.getInt(data, "Count");
                                    events.Add(new SystemScanComplete(timestamp, systemName, systemAddress, count) { raw = line, fromLoad = fromLogLoad });

                                }
                                handled = true;
                                break;
                            case "FSSBodySignals":
                                {
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string bodyName = JsonParsing.getString(data, "BodyName");
                                    long bodyId = JsonParsing.getLong(data, "BodyID");
                                    data.TryGetValue("Signals", out object signalsVal);

                                    // These are surface signal sources from a body that we've scanned
                                    List<SignalAmount> surfaceSignals = new List<SignalAmount>();
                                    foreach (Dictionary<string, object> signal in (List<object>)signalsVal)
                                    {
                                        SignalSource source;
                                        string signalSource = JsonParsing.getString(signal, "Type");
                                        source = SignalSource.FromEDName(signalSource) ?? new SignalSource();
                                        var localizedName = JsonParsing.getString(data, "Type_Localised");
                                        if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                                        {
                                            source.fallbackLocalizedName = localizedName;
                                        }
                                        int amount = JsonParsing.getInt(signal, "Count");
                                        surfaceSignals.Add(new SignalAmount(source, amount));
                                    }
                                    surfaceSignals = surfaceSignals.OrderByDescending(s => s.amount).ToList();
                                    events.Add(new SurfaceSignalsEvent(timestamp, "FSS", systemAddress, bodyName, bodyId, surfaceSignals) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SAASignalsFound":
                                {
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string bodyName = JsonParsing.getString(data, "BodyName");
                                    long bodyId = JsonParsing.getLong(data, "BodyID");
                                    data.TryGetValue("Signals", out object signalsVal);

                                    if (bodyName.EndsWith(" Ring"))
                                    {
                                        // This is the mining hotspots from a ring that we've mapped
                                        List<CommodityAmount> hotspots = new List<CommodityAmount>();
                                        foreach (Dictionary<string, object> signal in (List<object>)signalsVal)
                                        {
                                            string commodityEdName = JsonParsing.getString(signal, "Type");
                                            CommodityDefinition type = CommodityDefinition.FromEDName(commodityEdName);
                                            type.fallbackLocalizedName = JsonParsing.getString(signal, "Type_Localised");
                                            int amount = JsonParsing.getInt(signal, "Count");
                                            hotspots.Add(new CommodityAmount(type, amount));
                                        }
                                        hotspots = hotspots.OrderByDescending(h => h.amount).ToList();

                                        var ring = EDDI.Instance?.CurrentStarSystem?.bodies?
                                            .Where(b => b.rings.Any())
                                            .SelectMany(b => b?.rings)?
                                            .FirstOrDefault(r => r.name == bodyName);
                                        if (ring != null)
                                        {
                                            ring.mapped = timestamp;
                                            ring.hotspots = hotspots;
                                            StarSystemSqLiteRepository.Instance.SaveStarSystem(EDDI.Instance.CurrentStarSystem);
                                        }

                                        events.Add(new RingHotspotsEvent(timestamp, systemAddress, bodyName, bodyId, hotspots) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // This is surface signal sources from a body that we've mapped
                                        List<SignalAmount> surfaceSignals = new List<SignalAmount>();
                                        foreach (Dictionary<string, object> signal in (List<object>)signalsVal)
                                        {
                                            SignalSource source;
                                            string signalSource = JsonParsing.getString(signal, "Type");
                                            source = SignalSource.FromEDName(signalSource) ?? new SignalSource();
                                            var localizedName = JsonParsing.getString(data, "Type_Localised");
                                            if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                                            {
                                                source.fallbackLocalizedName = localizedName;
                                            }
                                            int amount = JsonParsing.getInt(signal, "Count");
                                            surfaceSignals.Add(new SignalAmount(source, amount));
                                        }
                                        surfaceSignals = surfaceSignals.OrderByDescending(s => s.amount).ToList();
                                        events.Add(new SurfaceSignalsEvent(timestamp, "SAA", systemAddress, bodyName, bodyId, surfaceSignals) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "Commander":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    string frontierID = JsonParsing.getString(data, "FID");
                                    events.Add(new CommanderLoadingEvent(timestamp, name, frontierID) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Statistics":
                                {
                                    Statistics statistics = new Statistics();

                                    data.TryGetValue("Bank_Account", out object bankAccountVal);
                                    Dictionary<string, object> bankaccount = (Dictionary<string, object>)bankAccountVal;
                                    if (bankaccount?.Count > 0)
                                    {
                                        statistics.bankaccount.wealth = JsonParsing.getOptionalLong(bankaccount, "Current_Wealth");
                                        statistics.bankaccount.spentonships = JsonParsing.getOptionalLong(bankaccount, "Spent_On_Ships");
                                        statistics.bankaccount.spentonoutfitting = JsonParsing.getOptionalLong(bankaccount, "Spent_On_Outfitting");
                                        statistics.bankaccount.spentonrepairs = JsonParsing.getOptionalLong(bankaccount, "Spent_On_Repairs");
                                        statistics.bankaccount.spentonfuel = JsonParsing.getOptionalLong(bankaccount, "Spent_On_Fuel");
                                        statistics.bankaccount.spentonammoconsumables = JsonParsing.getOptionalLong(bankaccount, "Spent_On_Ammo_Consumables");
                                        statistics.bankaccount.spentoninsurance = JsonParsing.getOptionalLong(bankaccount, "Spent_On_Insurance");
                                        statistics.bankaccount.insuranceclaims = JsonParsing.getOptionalLong(bankaccount, "Insurance_Claims");
                                        statistics.bankaccount.ownedshipcount = JsonParsing.getOptionalLong(bankaccount, "Owned_Ship_Count");
                                    }

                                    data.TryGetValue("Combat", out object combatVal);
                                    Dictionary<string, object> combat = (Dictionary<string, object>)combatVal;
                                    if (combat?.Count > 0)
                                    {
                                        statistics.combat.bountiesclaimed = JsonParsing.getOptionalLong(combat, "Bounties_Claimed");
                                        statistics.combat.bountyhuntingprofit = JsonParsing.getOptionalDecimal(combat, "Bounty_Hunting_Profit");
                                        statistics.combat.combatbonds = JsonParsing.getOptionalLong(combat, "Combat_Bonds");
                                        statistics.combat.combatbondprofits = JsonParsing.getOptionalLong(combat, "Combat_Bond_Profits");
                                        statistics.combat.assassinations = JsonParsing.getOptionalLong(combat, "Assassinations");
                                        statistics.combat.assassinationprofits = JsonParsing.getOptionalLong(combat, "Assassination_Profits");
                                        statistics.combat.highestsinglereward = JsonParsing.getOptionalLong(combat, "Highest_Single_Reward");
                                        statistics.combat.skimmerskilled = JsonParsing.getOptionalLong(combat, "Skimmers_Killed");
                                    }

                                    data.TryGetValue("Crime", out object crimeVal);
                                    Dictionary<string, object> crime = (Dictionary<string, object>)crimeVal;
                                    if (crime?.Count > 0)
                                    {
                                        statistics.crime.notoriety = JsonParsing.getOptionalInt(crime, "Notoriety");
                                        statistics.crime.fines = JsonParsing.getOptionalLong(crime, "Fines");
                                        statistics.crime.totalfines = JsonParsing.getOptionalLong(crime, "Total_Fines");
                                        statistics.crime.bountiesreceived = JsonParsing.getOptionalLong(crime, "Bounties_Received");
                                        statistics.crime.totalbounties = JsonParsing.getOptionalLong(crime, "Total_Bounties");
                                        statistics.crime.highestbounty = JsonParsing.getOptionalLong(crime, "Highest_Bounty");
                                    }

                                    data.TryGetValue("Smuggling", out object smugglingVal);
                                    Dictionary<string, object> smuggling = (Dictionary<string, object>)smugglingVal;
                                    if (smuggling?.Count > 0)
                                    {
                                        statistics.smuggling.blackmarketstradedwith = JsonParsing.getOptionalLong(smuggling, "Black_Markets_Traded_With");
                                        statistics.smuggling.blackmarketprofits = JsonParsing.getOptionalLong(smuggling, "Black_Markets_Profits");
                                        statistics.smuggling.resourcessmuggled = JsonParsing.getOptionalLong(smuggling, "Resources_Smuggled");
                                        statistics.smuggling.averageprofit = JsonParsing.getOptionalDecimal(smuggling, "Average_Profit");
                                        statistics.smuggling.highestsingletransaction = JsonParsing.getOptionalLong(smuggling, "Highest_Single_Transaction");
                                    }

                                    data.TryGetValue("Trading", out object tradingVal);
                                    Dictionary<string, object> trading = (Dictionary<string, object>)tradingVal;
                                    if (trading?.Count > 0)
                                    {
                                        statistics.trading.marketstradedwith = JsonParsing.getOptionalLong(trading, "Markets_Traded_With");
                                        statistics.trading.marketprofits = JsonParsing.getOptionalLong(trading, "Market_Profits");
                                        statistics.trading.resourcestraded = JsonParsing.getOptionalLong(trading, "Resources_Traded");
                                        statistics.trading.averageprofit = JsonParsing.getOptionalDecimal(trading, "Average_Profit");
                                        statistics.trading.highestsingletransaction = JsonParsing.getOptionalLong(trading, "Highest_Single_Transaction");
                                    }

                                    data.TryGetValue("Mining", out object miningVal);
                                    Dictionary<string, object> mining = (Dictionary<string, object>)miningVal;
                                    if (mining?.Count > 0)
                                    {
                                        statistics.mining.profits = JsonParsing.getOptionalLong(mining, "Mining_Profits");
                                        statistics.mining.quantitymined = JsonParsing.getOptionalLong(mining, "Quantity_Mined");
                                        statistics.mining.materialscollected = JsonParsing.getOptionalLong(mining, "Materials_Collected");
                                    }

                                    data.TryGetValue("Exploration", out object explorationVal);
                                    Dictionary<string, object> exploration = (Dictionary<string, object>)explorationVal;
                                    if (exploration?.Count > 0)
                                    {
                                        statistics.exploration.systemsvisited = JsonParsing.getOptionalLong(exploration, "Systems_Visited");
                                        statistics.exploration.profits = JsonParsing.getOptionalLong(exploration, "Exploration_Profits");
                                        statistics.exploration.planetsscannedlevel2 = JsonParsing.getOptionalLong(exploration, "Planets_Scanned_To_Level_2");
                                        statistics.exploration.planetsscannedlevel3 = JsonParsing.getOptionalLong(exploration, "Planets_Scanned_To_Level_3");
                                        statistics.exploration.highestpayout = JsonParsing.getOptionalLong(exploration, "Highest_Payout");
                                        statistics.exploration.totalhyperspacedistance = JsonParsing.getOptionalDecimal(exploration, "Total_Hyperspace_Distance");
                                        statistics.exploration.totalhyperspacejumps = JsonParsing.getOptionalLong(exploration, "Total_Hyperspace_Jumps");
                                        statistics.exploration.greatestdistancefromstart = JsonParsing.getOptionalDecimal(exploration, "Greatest_Distance_From_Start");
                                        statistics.exploration.timeplayedseconds = JsonParsing.getOptionalLong(exploration, "Time_Played");
                                    }

                                    data.TryGetValue("Passengers", out object passengersVal);
                                    Dictionary<string, object> passengers = (Dictionary<string, object>)passengersVal;
                                    if (passengers?.Count > 0)
                                    {
                                        statistics.passengers.accepted = JsonParsing.getOptionalLong(passengers, "Passengers_Missions_Accepted");
                                        statistics.passengers.disgruntled = JsonParsing.getOptionalLong(passengers, "Passengers_Missions_Disgruntled");
                                        statistics.passengers.bulk = JsonParsing.getOptionalLong(passengers, "Passengers_Missions_Bulk");
                                        statistics.passengers.vip = JsonParsing.getOptionalLong(passengers, "Passengers_Missions_VIP");
                                        statistics.passengers.delivered = JsonParsing.getOptionalLong(passengers, "Passengers_Missions_Delivered");
                                        statistics.passengers.ejected = JsonParsing.getOptionalLong(passengers, "Passengers_Missions_Ejected");
                                    }

                                    data.TryGetValue("Search_And_Rescue", out object searchAndRescueVal);
                                    Dictionary<string, object> searchAndRescue = (Dictionary<string, object>)searchAndRescueVal;
                                    if (searchAndRescue?.Count > 0)
                                    {
                                        statistics.searchandrescue.traded = JsonParsing.getOptionalLong(searchAndRescue, "SearchRescue_Traded");
                                        statistics.searchandrescue.profit = JsonParsing.getOptionalLong(searchAndRescue, "SearchRescue_Profit");
                                        statistics.searchandrescue.count = JsonParsing.getOptionalLong(searchAndRescue, "SearchRescue_Count");
                                    }

                                    data.TryGetValue("TG_ENCOUNTERS", out object thargoidVal);
                                    Dictionary<string, object> thargoid = (Dictionary<string, object>)thargoidVal;
                                    if (thargoid?.Count > 0)
                                    {
                                        statistics.thargoidencounters.wakesscanned = JsonParsing.getOptionalLong(thargoid, "TG_ENCOUNTER_WAKES");
                                        statistics.thargoidencounters.imprints = JsonParsing.getOptionalLong(thargoid, "TG_ENCOUNTER_IMPRINT");
                                        statistics.thargoidencounters.totalencounters = JsonParsing.getOptionalLong(thargoid, "TG_ENCOUNTER_TOTAL");
                                        statistics.thargoidencounters.lastsystem = JsonParsing.getString(thargoid, "TG_ENCOUNTER_TOTAL_LAST_SYSTEM");
                                        statistics.thargoidencounters.lastshipmodel = JsonParsing.getString(thargoid, "TG_ENCOUNTER_TOTAL_LAST_SHIP");
                                        var lastTimeStampString = JsonParsing.getString(thargoid, "TG_ENCOUNTER_TOTAL_LAST_TIMESTAMP");
                                        statistics.thargoidencounters.lasttimestamp = !string.IsNullOrEmpty(lastTimeStampString) ? DateTime.Parse(lastTimeStampString) : (DateTime?)null;
                                    }

                                    data.TryGetValue("Crafting", out object craftingVal);
                                    Dictionary<string, object> crafting = (Dictionary<string, object>)craftingVal;
                                    if (crafting?.Count > 0)
                                    {
                                        statistics.crafting.countofusedengineers = JsonParsing.getOptionalLong(crafting, "Count_Of_Used_Engineers");
                                        statistics.crafting.recipesgenerated = JsonParsing.getOptionalLong(crafting, "Recipes_Generated");
                                        statistics.crafting.recipesgeneratedrank1 = JsonParsing.getOptionalLong(crafting, "Recipes_Generated_Rank_1");
                                        statistics.crafting.recipesgeneratedrank2 = JsonParsing.getOptionalLong(crafting, "Recipes_Generated_Rank_2");
                                        statistics.crafting.recipesgeneratedrank3 = JsonParsing.getOptionalLong(crafting, "Recipes_Generated_Rank_3");
                                        statistics.crafting.recipesgeneratedrank4 = JsonParsing.getOptionalLong(crafting, "Recipes_Generated_Rank_4");
                                        statistics.crafting.recipesgeneratedrank5 = JsonParsing.getOptionalLong(crafting, "Recipes_Generated_Rank_5");
                                    }

                                    data.TryGetValue("Crew", out object crewVal);
                                    Dictionary<string, object> crew = (Dictionary<string, object>)crewVal;
                                    if (crew?.Count > 0)
                                    {
                                        statistics.npccrew.totalwages = JsonParsing.getOptionalLong(crew, "NpcCrew_TotalWages");
                                        statistics.npccrew.hired = JsonParsing.getOptionalLong(crew, "NpcCrew_Hired");
                                        statistics.npccrew.fired = JsonParsing.getOptionalLong(crew, "NpcCrew_Fired");
                                        statistics.npccrew.died = JsonParsing.getOptionalLong(crew, "NpcCrew_Died");
                                    }

                                    data.TryGetValue("Multicrew", out object multicrewVal);
                                    Dictionary<string, object> multicrew = (Dictionary<string, object>)multicrewVal;
                                    if (multicrew?.Count > 0)
                                    {
                                        statistics.multicrew.timetotalseconds = JsonParsing.getOptionalLong(multicrew, "Multicrew_Time_Total");
                                        statistics.multicrew.gunnertimetotalseconds = JsonParsing.getOptionalLong(multicrew, "Multicrew_Gunner_Time_Total");
                                        statistics.multicrew.fightertimetotalseconds = JsonParsing.getOptionalLong(multicrew, "Multicrew_Fighter_Time_Total");
                                        statistics.multicrew.multicrewcreditstotal = JsonParsing.getOptionalLong(multicrew, "Multicrew_Credits_Total");
                                        statistics.multicrew.multicrewfinestotal = JsonParsing.getOptionalLong(multicrew, "Multicrew_Fines_Total");
                                    }

                                    data.TryGetValue("Material_Trader_Stats", out object materialTraderVal);
                                    Dictionary<string, object> materialtrader = (Dictionary<string, object>)materialTraderVal;
                                    if (materialtrader?.Count > 0)
                                    {
                                        statistics.materialtrader.tradescompleted = JsonParsing.getOptionalLong(materialtrader, "Trades_Completed");
                                        statistics.materialtrader.materialstraded = JsonParsing.getOptionalLong(materialtrader, "Materials_Traded");
                                        statistics.materialtrader.encodedmaterialstraded = JsonParsing.getOptionalLong(materialtrader, "Encoded_Materials_Traded");
                                        statistics.materialtrader.rawmaterialstraded = JsonParsing.getOptionalLong(materialtrader, "Raw_Materials_Traded");
                                        statistics.materialtrader.grade1materialstraded = JsonParsing.getOptionalLong(materialtrader, "Grade_1_Materials_Traded");
                                        statistics.materialtrader.grade2materialstraded = JsonParsing.getOptionalLong(materialtrader, "Grade_2_Materials_Traded");
                                        statistics.materialtrader.grade3materialstraded = JsonParsing.getOptionalLong(materialtrader, "Grade_3_Materials_Traded");
                                        statistics.materialtrader.grade4materialstraded = JsonParsing.getOptionalLong(materialtrader, "Grade_4_Materials_Traded");
                                        statistics.materialtrader.grade5materialstraded = JsonParsing.getOptionalLong(materialtrader, "Grade_5_Materials_Traded");
                                    }

                                    data.TryGetValue("CQC", out object cqcVal);
                                    Dictionary<string, object> cqc = (Dictionary<string, object>)cqcVal;
                                    if (cqc?.Count > 0)
                                    {
                                        statistics.cqc.creditsearned = JsonParsing.getOptionalLong(cqc, "CQC_Credits_Earned");
                                        statistics.cqc.timeplayedseconds = JsonParsing.getOptionalLong(cqc, "CQC_Time_Played");
                                        statistics.cqc.killdeathratio = JsonParsing.getOptionalDecimal(cqc, "CQC_KD");
                                        statistics.cqc.kills = JsonParsing.getOptionalLong(cqc, "CQC_Kills");
                                        statistics.cqc.winlossratio = JsonParsing.getOptionalDecimal(cqc, "CQC_WL");
                                    }

                                    events.Add(new StatisticsEvent(timestamp, statistics) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Powerplay":
                                {
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    int rank = JsonParsing.getInt(data, "Rank") + 1; // This is zero based in the journal but not in the Frontier API. Adding +1 here synchronizes the two.
                                    int merits = JsonParsing.getInt(data, "Merits");
                                    int votes = JsonParsing.getInt(data, "Votes");
                                    TimeSpan timePledged = TimeSpan.FromSeconds(JsonParsing.getLong(data, "TimePledged"));
                                    events.Add(new PowerplayEvent(timestamp, power, rank, merits, votes, timePledged) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Reputation":
                                {
                                    decimal empire = JsonParsing.getOptionalDecimal(data, "Empire") ?? 0;
                                    decimal federation = JsonParsing.getOptionalDecimal(data, "Federation") ?? 0;
                                    decimal independent = JsonParsing.getOptionalDecimal(data, "Independent") ?? 0;
                                    decimal alliance = JsonParsing.getOptionalDecimal(data, "Alliance") ?? 0;
                                    events.Add(new CommanderReputationEvent(timestamp, empire, federation, independent, alliance) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierJump":
                                {
                                    // Get destination star system data
                                    string systemName = JsonParsing.getString(data, "StarSystem");
                                    data.TryGetValue("StarPos", out object starposVal);
                                    List<object> starPos = (List<object>)starposVal;
                                    decimal x = Math.Round(JsonParsing.getDecimal("X", starPos?[0]) * 32) / (decimal)32.0;
                                    decimal y = Math.Round(JsonParsing.getDecimal("Y", starPos?[1]) * 32) / (decimal)32.0;
                                    decimal z = Math.Round(JsonParsing.getDecimal("Z", starPos?[2]) * 32) / (decimal)32.0;
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    Economy systemEconomy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy"));
                                    Economy systemEconomy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy"));
                                    Faction systemfaction = GetFaction(data, "System", systemName);
                                    SecurityLevel systemSecurity = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity"));
                                    systemSecurity.fallbackLocalizedName = JsonParsing.getString(data, "SystemSecurity_Localised");
                                    long? systemPopulation = JsonParsing.getOptionalLong(data, "Population");

                                    // Get destination body data (if any)
                                    string bodyName = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType")) ?? BodyType.None;
                                    if ( bodyType == BodyType.Planet )
                                    {
                                        bodyType = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem( systemName )?
                                                       .bodies.FirstOrDefault( b => b.bodyId != null && b.bodyId == bodyId )?.bodyType ??
                                                   bodyType;
                                    }

                                    // Get carrier data (may not be present when on-foot at a fleet carrier but not docked)
                                    string carrierName = JsonParsing.getString(data, "StationName");
                                    StationModel carrierType = StationModel.FromEDName(JsonParsing.getString(data, "StationType"));
                                    long carrierId = JsonParsing.getLong(data, "MarketID");
                                    Faction stationFaction = GetFaction(data, "Station", systemName);

                                    // Get carrier services data (may not be present when on-foot at a fleet carrier but not docked)
                                    List<StationService> stationServices = new List<StationService>();
                                    data.TryGetValue("StationServices", out object stationserviceVal);
                                    List<string> stationservices = (stationserviceVal as List<object>)?.Cast<string>()?.ToList() ?? new List<string>();
                                    foreach (string service in stationservices)
                                    {
                                        stationServices.Add(StationService.FromEDName(service));
                                    }

                                    // Get carrier economies and their shares (may not be present when on-foot at a fleet carrier but not docked)
                                    data.TryGetValue("StationEconomies", out object economiesVal);
                                    List<object> economies = economiesVal as List<object> ?? new List<object>();
                                    List<EconomyShare> stationEconomies = new List<EconomyShare>();
                                    foreach (Dictionary<string, object> economyshare in economies)
                                    {
                                        Economy economy = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                                        economy.fallbackLocalizedName = JsonParsing.getString(economyshare, "Name_Localised");
                                        decimal share = JsonParsing.getDecimal(economyshare, "Proportion");
                                        if (economy != Economy.None && share > 0)
                                        {
                                            stationEconomies.Add(new EconomyShare(economy, share));
                                        }
                                    }

                                    // Parse factions array data
                                    List<Faction> factions = new List<Faction>();
                                    data.TryGetValue("Factions", out object factionsVal);
                                    if (factionsVal != null)
                                    {
                                        factions = GetFactions(factionsVal, systemName);
                                    }

                                    // Parse conflicts array data
                                    List<Conflict> conflicts = new List<Conflict>();
                                    data.TryGetValue("Conflicts", out object conflictsVal);
                                    if (conflictsVal != null)
                                    {
                                        conflicts = GetConflicts(conflictsVal, factions);
                                    }

                                    // Powerplay data (if pledged)
                                    GetPowerplayData( data, out List<Power> powerplayPowers, out PowerplayState powerplayState );

                                    // Thargoid war data (if any)
                                    GetThargoidWarData( data, out ThargoidWar thargoidWar );

                                    bool docked = JsonParsing.getBool(data, "Docked");
                                    bool onFoot = JsonParsing.getOptionalBool(data, "OnFoot") ?? false;

                                    events.Add(new CarrierJumpedEvent(timestamp, systemName, systemAddress, x, y, z, bodyName, bodyId, bodyType, docked, onFoot, carrierName, carrierType, carrierId, stationServices, systemfaction, stationFaction, factions, conflicts, stationEconomies, systemEconomy, systemEconomy2, systemSecurity, systemPopulation, powerplayPowers, powerplayState, thargoidWar ) { raw = line, fromLoad = fromLogLoad });

                                    // Generate secondary event when the carrier jump cooldown completes
                                    if (carrierJumpCancellationTokenSources.TryGetValue(carrierId, out var carrierJumpCancellationTS))
                                    {
                                        // Cancel any pending cooldown event (to prevent doubling events if the commander is the fleet carrier owner)
                                        carrierJumpCancellationTS.Cancel();
                                    }
                                    if (!fromLogLoad)
                                    {
                                        Task.Run(async () =>
                                        {
                                            int timeMs = (Constants.carrierPostJumpSeconds - Constants.carrierJumpSeconds) * 1000; // Cooldown timer starts when the carrier jump is engaged, not when the jump ends
                                            await Task.Delay(timeMs);
                                            EDDI.Instance.enqueueEvent(new CarrierCooldownEvent(timestamp.AddMilliseconds(timeMs), carrierId, systemName, systemAddress, bodyName, bodyId, bodyType, carrierName, carrierType) { fromLoad = fromLogLoad });
                                        }).ConfigureAwait(false);
                                    }
                                }
                                handled = true;
                                break;
                            case "CarrierJumpRequest":
                                {
                                    long carrierId = JsonParsing.getLong(data, "CarrierID");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string systemName = JsonParsing.getString(data, "SystemName");
                                    string bodyName = JsonParsing.getString(data, "Body");
                                    long bodyId = JsonParsing.getLong(data, "BodyID");
                                    var departureTime = JsonParsing.getDateTime( "DepartureTime", data );

                                    // There is a bug in the journal output where "Body" can be missing but "BodyID" can be present. Try to Work around that here.
                                    if (string.IsNullOrEmpty(bodyName) && systemAddress > 0)
                                    {
                                        var starSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systemName, true, true, true, false, false);
                                        bodyName = starSystem?.bodies?.FirstOrDefault(b => b?.bodyId == bodyId)?.bodyname;
                                    }

                                    events.Add(new CarrierJumpRequestEvent(timestamp, systemName, systemAddress, bodyName, bodyId, carrierId) { raw = line, fromLoad = fromLogLoad });

                                    // Cancel any pending carrier jump related events
                                    if (carrierJumpCancellationTokenSources.TryGetValue(carrierId, out var carrierJumpCancellationTS))
                                    {
                                        carrierJumpCancellationTS.Cancel();
                                    }

                                    if (!fromLogLoad)
                                    {
                                        // Generate a new cancellation token source
                                        carrierJumpCancellationTS = new CancellationTokenSource();
                                        carrierJumpCancellationTokenSources[carrierId] = carrierJumpCancellationTS;

                                        // Generate secondary tasks to spawn events when the carrier locks down landing pads and when it begins jumping.
                                        // These may be cancelled via the cancellation token source above.

                                        var departureSeconds = ( departureTime - timestamp ).TotalSeconds;
                                        var tasks = new List<Task>
                                        {
                                            Task.Run(async () =>
                                            {
                                                var timespan = TimeSpan.FromSeconds(departureSeconds - Constants.carrierLandingPadLockdownSeconds);
                                                await Task.Delay(timespan, carrierJumpCancellationTS.Token);
                                                EDDI.Instance.enqueueEvent(new CarrierPadsLockedEvent(timestamp.Add(timespan), carrierId) { fromLoad = fromLogLoad });
                                            }, carrierJumpCancellationTS.Token),
                                            Task.Run(async () =>
                                            {
                                                var timespan = TimeSpan.FromSeconds(departureSeconds);
                                                await Task.Delay(timespan, carrierJumpCancellationTS.Token);
                                                if ( EDDI.Instance.CurrentStarSystem != null )
                                                {
                                                    string originStarSystem = EDDI.Instance.CurrentStarSystem.systemname;
                                                    var originSystemAddress = EDDI.Instance.CurrentStarSystem.systemAddress;
                                                EDDI.Instance.enqueueEvent(new CarrierJumpEngagedEvent(timestamp.Add(timespan), systemName, systemAddress, originStarSystem, originSystemAddress, bodyName, bodyId, carrierId) { fromLoad = fromLogLoad });
                                                }
                                            }, carrierJumpCancellationTS.Token),
                                            Task.Run(async () =>
                                            {
                                                // This event will be canceled and replaced by an updated `CarrierCooldownEvent` if the owner is aboard the fleet carrier and sees the `CarrierJumpedEvent`.
                                                var timespan = TimeSpan.FromSeconds(departureSeconds + Constants.carrierPostJumpSeconds); // Cooldown timer starts when the carrier jump is engaged, not when the jump ends
                                                await Task.Delay(timespan, carrierJumpCancellationTS.Token);
                                                EDDI.Instance.enqueueEvent(new CarrierCooldownEvent(timestamp.Add(timespan), carrierId, systemName, systemAddress, bodyName, bodyId, null, null, null) { fromLoad = fromLogLoad });
                                            }, carrierJumpCancellationTS.Token)
                                        };

                                        Task.Run(async () =>
                                        {
                                            try
                                            {
                                                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
                                            }
                                            catch (OperationCanceledException)
                                            {
                                                // Tasks were cancelled. Nothing to do here.
                                            }
                                            finally
                                            {
                                                carrierJumpCancellationTokenSources.Remove(carrierId);
                                                carrierJumpCancellationTS.Dispose();
                                            }
                                        });
                                    }
                                }
                                handled = true;
                                break;
                            case "CarrierJumpCancelled":
                                {
                                    long carrierId = JsonParsing.getLong(data, "CarrierID");
                                    // Cancel any pending carrier jump related events
                                    if (carrierJumpCancellationTokenSources.TryGetValue(carrierId, out var carrierJumpCancellationTS))
                                    {
                                        carrierJumpCancellationTS.Cancel();
                                    }
                                    events.Add(new CarrierJumpCancelledEvent(timestamp, carrierId) { raw = line, fromLoad = fromLogLoad });
                                    if (!fromLogLoad)
                                    {
                                        Task.Run(async () =>
                                        {
                                            int timeMs = 60000; // Cooldown timer starts when the carrier jump is cancelled and lasts for one minute
                                            await Task.Delay(timeMs);
                                            EDDI.Instance.enqueueEvent(new CarrierCooldownEvent(timestamp.AddMilliseconds(timeMs), carrierId, EDDI.Instance.FleetCarrier?.currentStarSystem, 0, null, null, null, EDDI.Instance.FleetCarrier?.callsign, StationModel.FleetCarrier ) { fromLoad = fromLogLoad });
                                        }).ConfigureAwait(false);
                                    }
                                }
                                handled = true;
                                break;
                            case "AsteroidCracked":
                                {
                                    string bodyName = JsonParsing.getString(data, "Body");
                                    events.Add(new AsteroidCrackedEvent(timestamp, bodyName) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ProspectedAsteroid":
                                {
                                    data.TryGetValue("Materials", out object val); // (array of Name and Proportion)
                                    List<CommodityPresence> commodities = new List<CommodityPresence>();
                                    if (val is List<object> listVal)
                                    {
                                        foreach (var commodityVal in listVal)
                                        {
                                            if (commodityVal is Dictionary<string, object> commodityData)
                                            {
                                                string commodityEdName = JsonParsing.getString(commodityData, "Name");
                                                CommodityDefinition commodity = CommodityDefinition.FromEDName(commodityEdName);
                                                decimal proportion = JsonParsing.getDecimal(commodityData, "Proportion"); // Out of 100
                                                if (commodity != null)
                                                {
                                                    commodity.fallbackLocalizedName = JsonParsing.getString(commodityData, "Name_Localised");
                                                    commodities.Add(new CommodityPresence(commodity, proportion));
                                                }
                                            }
                                        }
                                    }
                                    string content = JsonParsing.getString(data, "Content"); // (a string representing High/Medium/Low material content)
                                    var materialContent = new AsteroidMaterialContent(content)
                                    {
                                        fallbackLocalizedName = JsonParsing.getString(data, "Content_Localised")?.Replace("Material Content: ", "")
                                    };
                                    decimal remaining = JsonParsing.getDecimal(data, "Remaining"); // Out of 100

                                    // If a motherlode commodity is present
                                    CommodityDefinition motherlodeCommodityDefinition = null;
                                    string motherlodeEDName = JsonParsing.getString(data, "MotherlodeMaterial");
                                    if (!string.IsNullOrEmpty(motherlodeEDName))
                                    {
                                        motherlodeCommodityDefinition = CommodityDefinition.FromEDName(motherlodeEDName);
                                    }

                                    events.Add(new AsteroidProspectedEvent(timestamp, commodities, materialContent, remaining, motherlodeCommodityDefinition) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Disembark":
                                {
                                    bool fromSRV = JsonParsing.getBool(data, "SRV"); // true if getting out of SRV, false if getting out of a ship 
                                    bool fromTaxi = JsonParsing.getBool(data, "Taxi"); //  true when getting out of a transport ship (e.g. Apex Taxi or Frontline Solutions dropship)
                                    bool fromMultiCrew = JsonParsing.getBool(data, "Multicrew"); //  true when getting out of another player’s vessel
                                    int? fromLocalId = JsonParsing.getOptionalInt(data, "ID"); // player’s ship ID (if player's own vessel)

                                    string system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    int? bodyId = JsonParsing.getOptionalInt(data, "BodyID");
                                    bool? onStation = JsonParsing.getOptionalBool(data, "OnStation");
                                    bool? onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");

                                    string station = JsonParsing.getString(data, "StationName"); // if at a station
                                    long? marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    StationModel stationModel = StationModel.FromEDName(JsonParsing.getString(data, "StationType"));

                                    events.Add(new DisembarkEvent(timestamp, fromSRV, fromTaxi, fromMultiCrew, fromLocalId, system, systemAddress, body, bodyId, onStation, onPlanet, station, marketId, stationModel) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Embark":
                                {
                                    bool toSRV = JsonParsing.getBool(data, "SRV"); // true if getting out of SRV, false if getting out of a ship 
                                    bool toTaxi = JsonParsing.getBool(data, "Taxi"); //  true when getting out of a transport ship (e.g. Apex Taxi or Frontline Solutions dropship)
                                    bool toMultiCrew = JsonParsing.getBool(data, "Multicrew"); //  true when getting out of another player’s vessel
                                    int? toLocalId = JsonParsing.getOptionalInt(data, "ID"); // player’s ship ID (if player's own vessel)

                                    string system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    int? bodyId = JsonParsing.getOptionalInt(data, "BodyID");
                                    bool? onStation = JsonParsing.getOptionalBool(data, "OnStation");
                                    bool? onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");

                                    string station = JsonParsing.getString(data, "StationName"); // if at a station
                                    long? marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    StationModel stationModel = StationModel.FromEDName(JsonParsing.getString(data, "StationType"));

                                    events.Add(new EmbarkEvent(timestamp, toSRV, toTaxi, toMultiCrew, toLocalId, system, systemAddress, body, bodyId, onStation, onPlanet, station, marketId, stationModel) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BookDropship":
                            case "BookTaxi":
                                {
                                    var type = edType.Replace("Book", "");
                                    var price = JsonParsing.getOptionalInt(data, "Cost");
                                    var system = JsonParsing.getString(data, "DestinationSystem");
                                    var destination = JsonParsing.getString(data, "DestinationLocation");
                                    events.Add(new BookTransportEvent(timestamp, type, price, system, destination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CancelDropship":
                            case "CancelTaxi":
                                {
                                    var type = edType.Replace("Cancel", "");
                                    var refund = JsonParsing.getOptionalInt(data, "Refund");
                                    events.Add(new CancelTransportEvent(timestamp, type, refund) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DropshipDeploy":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    int? bodyId = JsonParsing.getOptionalInt(data, "BodyID");
                                    // There are `OnStation` and `OnPlanet` properties, but these are
                                    // always false and always true so we won't bother parsing them.

                                    events.Add(new DropshipDeploymentEvent(timestamp, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Backpack":
                            case "ShipLocker":
                                {
                                    if (MicroResourceInfo.TryFromFile(timestamp, out var info, out line, $"{edType}.json"))
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
                                            handled = true;
                                        }
                                        else if (edType == "ShipLocker")
                                        {
                                            events.Add(new ShipLockerEvent(timestamp, inventory) { raw = line, fromLoad = fromLogLoad });
                                            handled = true;
                                        }
                                    }
                                }
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
                            case "BuyMicroResources":
                                {
                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID");

                                    MicroResource GetResource( IDictionary<string, object> resourceData )
                                    {
                                        var edname = JsonParsing.getString(resourceData, "Name");
                                        var microResource = MicroResource.FromEDName(edname);
                                        if ( microResource is null ) { return null; }

                                        var fallbackName = JsonParsing.getString(resourceData, "Name_Localised");
                                        microResource.fallbackLocalizedName = fallbackName;

                                        var category = JsonParsing.getString(resourceData, "Category");
                                        var fallbackCategoryName = JsonParsing.getString(resourceData, "Category_Localised");
                                        if ( microResource.Category is null ) { microResource.Category = MicroResourceCategory.FromEDName( category ); }
                                        microResource.Category.fallbackLocalizedName = fallbackCategoryName;
                                        return microResource;
                                    }

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
                                                        var microResource = GetResource( microVal );
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
                                        var microResource = GetResource( data );
                                        var amount = JsonParsing.getInt( data, "Count" );
                                        var price = JsonParsing.getInt(data, "Price"); // Total price
                                        var resourceAmounts = new List<MicroResourceAmount> { new MicroResourceAmount( microResource, amount ) };
                                        events.Add( new MicroResourcesPurchasedEvent( timestamp, resourceAmounts, price, marketId ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                    handled = true;
                                }
                                break;
                            case "BuySuit":
                                {
                                    var edname = JsonParsing.getString(data, "Name");
                                    var fallbackName = JsonParsing.getString(data, "Name_Localised");
                                    var suitId = JsonParsing.getOptionalLong(data, "SuitID");
                                    var price = JsonParsing.getOptionalInt(data, "Price");
                                    var suit = Suit.FromEDName(edname, suitId);
                                    suit.fallbackLocalizedName = fallbackName;
                                    events.Add(new SuitPurchasedEvent(timestamp, suit, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierBuy":
                                {
                                    var carrierID = JsonParsing.getOptionalLong(data, "CarrierID");
                                    var carrierCallsign = JsonParsing.getString(data, "Callsign");
                                    var carrierStarSystem = JsonParsing.getString(data, "Location");
                                    var carrierSystemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var price = JsonParsing.getOptionalLong(data, "Price");
                                    events.Add(new CarrierPurchasedEvent(timestamp, carrierID, carrierCallsign, carrierStarSystem, carrierSystemAddress, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierStats":
                                {
                                    var carrierID = JsonParsing.getOptionalLong(data, "CarrierID");
                                    var carrierCallsign = JsonParsing.getString(data, "Callsign");
                                    var carrierName = JsonParsing.getString(data, "Name");

                                    var dockingAccess = JsonParsing.getString(data, "DockingAccess");
                                    var notoriousAccess = JsonParsing.getBool(data, "AllowNotorious");
                                    var fuelLevel = JsonParsing.getInt(data, "FuelLevel");

                                    int crewSpace = 0;
                                    int cargoSpace = 0;
                                    int cargoSpaceReserved = 0;
                                    int shipPacks = 0;
                                    int modulePacks = 0;
                                    int freeSpace = 0;
                                    if (data.TryGetValue("SpaceUsage", out object spaceUsage) && spaceUsage is Dictionary<string, object> space)
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
                                    if (data.TryGetValue("Finance", out object finances) && finances is Dictionary<string, object> finance)
                                    {
                                        bankBalance = JsonParsing.getLong(finance, "CarrierBalance");
                                        bankReservedBalance = JsonParsing.getLong(finance, "ReserveBalance");
                                        bankAvailableBalance = JsonParsing.getLong(finance, "AvailableBalance");
                                    }

                                    events.Add(new CarrierStatsEvent(timestamp, carrierID, carrierCallsign, carrierName, dockingAccess, notoriousAccess, fuelLevel, usedSpace, freeSpace, bankBalance, bankReservedBalance, bankAvailableBalance ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierBankTransfer":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var deposit = JsonParsing.getOptionalULong(data, "Deposit");
                                    var withdrawal = JsonParsing.getOptionalULong(data, "Withdraw");
                                    var cmdrBalance = JsonParsing.getULong(data, "PlayerBalance");
                                    var carrierBalance = JsonParsing.getLong(data, "CarrierBalance");
                                    events.Add(new CarrierBankTransferEvent(timestamp, carrierID, deposit, withdrawal, cmdrBalance, carrierBalance) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierCancelDecommission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    events.Add(new CarrierDecommissionCancelledEvent(timestamp, carrierID) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDecommission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var refund = JsonParsing.getULong(data, "ScrapRefund");
                                    var decommissionTimespan = Dates.fromTimestamp(JsonParsing.getOptionalLong(data, "ScrapTime")) - timestamp;
                                    events.Add(new CarrierDecommissionScheduledEvent(timestamp, carrierID, refund, decommissionTimespan) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierCrewServices":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var operation = JsonParsing.getString(data, "Operation");
                                    var crewRole = StationService.FromEDName(JsonParsing.getString(data, "CrewRole"));
                                    var crewName = JsonParsing.getString(data, "CrewName");
                                    events.Add(new CarrierServiceChangedEvent(timestamp, carrierID, operation, crewRole, crewName) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDepositFuel":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var amount = JsonParsing.getInt(data, "Amount");
                                    var total = JsonParsing.getInt(data, "Total");
                                    events.Add(new CarrierFuelDepositEvent(timestamp, carrierID, amount, total) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDockingPermission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var dockingAccess = JsonParsing.getString(data, "DockingAccess");
                                    var allowNotorious = JsonParsing.getBool(data, "AllowNotorious");
                                    events.Add(new CarrierDockingPermissionEvent(timestamp, carrierID, dockingAccess, allowNotorious) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierFinance":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var taxRate = JsonParsing.getInt(data, "TaxRate");
                                    var reservePercent = JsonParsing.getInt(data, "ReservePercent");
                                    var carrierBalance = JsonParsing.getLong(data, "CarrierBalance");
                                    var carrierReserveBalance = JsonParsing.getLong(data, "ReserveBalance");
                                    var carrierAvailableBalance = JsonParsing.getLong(data, "CarrierAvailableBalance");
                                    events.Add(new CarrierFinanceEvent(timestamp, carrierID, taxRate, reservePercent, carrierBalance, carrierReserveBalance, carrierAvailableBalance) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierNameChange":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var callsign = JsonParsing.getString(data, "Callsign");
                                    var name = JsonParsing.getString(data, "Name");
                                    events.Add(new CarrierNameChangeEvent(timestamp, carrierID, callsign, name) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FCMaterials":
                                {
                                    var carrierID = JsonParsing.getLong(data, "MarketID");
                                    var carrierName = JsonParsing.getString(data, "CarrierName");
                                    var callsign = JsonParsing.getString(data, "CarrierID");

                                    var raw = Files.FromSavedGames("FCMaterials.json");
                                    if (raw != null)
                                    {
                                        var info = JsonConvert.DeserializeObject<FCMaterialsInfo>(raw);
                                        if (info != null && info.CarrierID == carrierID
                                                         && info.CarrierName == carrierName
                                                         && info.callsign == callsign)
                                        {
                                            events.Add(new FleetCarrierMaterialsEvent(timestamp, carrierID, carrierName, callsign, info) { raw = raw, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "SupercruiseDestinationDrop":
                                {
                                    // "Type" may be either a signal source or proper name.
                                    // If proper name, it might be a fleet carrier with both name and ID.
                                    var type = JsonParsing.getString( data, "Type" );
                                    var typeLocalized = JsonParsing.getString( data, "Type_Localised" );
                                    var threat = JsonParsing.getOptionalInt( data, "Threat" ) ?? 0; // Typically 0 except for USS drops.
                                    var marketID = JsonParsing.getOptionalLong( data, "MarketID" );

                                    if ( type.StartsWith("$") ) 
                                    {
                                        // Symbolic signal source name. Prefer our own localization and fallback using the provided localization string if needed.
                                        var signalSource = SignalSource.FromEDName( type );
                                        signalSource.fallbackLocalizedName = typeLocalized;
                                        type = signalSource.invariantName;
                                        typeLocalized = signalSource.localizedName;
                                    }
                                    else
                                    {
                                        // Destination might be a fleet carrier with name and carrier ID in a single string.
                                        // Check and break apart if needed.
                                        var fleetCarrierRegex = new Regex( "^(.+)(?> )([A-Za-z0-9]{3}-[A-Za-z0-9]{3})$" );
                                        if ( string.IsNullOrEmpty( typeLocalized ) && fleetCarrierRegex.IsMatch( type ) )
                                        {
                                            // Fleet carrier names include both the carrier name and carrier ID, we need to separate them
                                            var fleetCarrierParts = fleetCarrierRegex.Matches( type )[ 0 ].Groups;
                                            if ( fleetCarrierParts.Count == 3 )
                                            {
                                                type = fleetCarrierParts[ 2 ].Value;
                                                typeLocalized = fleetCarrierParts[ 1 ].Value;
                                            }
                                        }
                                    }

                                    events.Add( new DestinationArrivedEvent( timestamp, type, typeLocalized, threat, marketID ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "CargoTransfer":
                                {
                                    var toShip = new List<CommodityAmount>();
                                    var toSRV = new List<CommodityAmount>();
                                    var toCarrier = new List<CommodityAmount>();
                                    if ( data.TryGetValue("Transfers", out var transfersVal) )
                                    {
                                        var transfersArray = JArray.FromObject( transfersVal );
                                        foreach ( var transfer in transfersArray )
                                        {
                                            var direction = transfer[ "Direction" ].ToString();
                                            var count = (int)transfer[ "Count" ];
                                            var commodity = CommodityDefinition.FromEDName( transfer[ "Type" ].ToString() );
                                            commodity.fallbackLocalizedName = transfer[ "Type_Localised" ]?.ToString();

                                            // Objects may have a `MissionID` but the legalstatus is not identified so we rtat these items
                                            // as CommodityAmount objects and use the `Cargo` event to update the CargoMonitor.

                                            var commodityAmount = new CommodityAmount( commodity, count );
                                            if ( direction.Equals( "toship", StringComparison.InvariantCultureIgnoreCase ) )
                                            {
                                                toShip.Add( commodityAmount );
                                            }
                                            else if ( direction.Equals( "tosrv", StringComparison.InvariantCultureIgnoreCase ) )
                                            {
                                                toSRV.Add( commodityAmount );
                                            }
                                            else if ( direction.Equals( "tocarrier", StringComparison.InvariantCultureIgnoreCase ) )
                                            {
                                                toCarrier.Add( commodityAmount );
                                            }
                                            else
                                            {
                                                throw new ArgumentException( "Unhandled CargoTransfer `Direction`." );
                                            }
                                        }
                                    }

                                    var cargoTransferEvent = new CargoTransferEvent( timestamp, toShip, toSRV, toCarrier ) { raw = line, fromLoad = fromLogLoad };
                                    if ( fromSpeechResponderTest )
                                    {
                                        events.Add( cargoTransferEvent );
                                    }
                                    else
                                    {
                                        DelayedEventHolder[ CargoEvent.NAME ] = cargoTransferEvent;
                                    }
                                }
                                handled = true;
                                break;

                            // we silently ignore these, but forward them to the responders
                            case "CodexDiscovery":
                            case "CodexEntry":
                            case "ModuleBuyAndStore":
                            case "RestockVehicle":
                            case "ScanOrganic":
                            case "SellMicroResources":
                            case "SellOrganicData":

                            // Low priority (for now)
                            case "BuyWeapon":
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
                            case "SellSuit":  
                            case "SellWeapon": 
                            case "SuitLoadout":
                            case "SwitchSuitLoadout":
                            case "UpgradeSuit":
                            case "UpgradeWeapon":
                            case "WingAdd":
                            case "WingInvite":
                            case "WingJoin":
                            case "WingLeave":

                            // No plans to support
                            case "CollectItems": // The `BackpackChange` event keeps us up to date.
                            case "CrimeVictim": // No need to track crimes committed by other cmdrs. If added, filter out events where the current player is listed as the offender.
                            case "DiscoveryScan": // Probably deprecated / replaced by `FSSDiscoveryScan`
                            case "DropItems": // The `BackpackChange` event keeps us up to date.
                            case "EngineerLegacyConvert": // Unnecessary.
                            case "Resupply": // Seems to be related to resupplying Odyssey backpack items
                            case "ScanBaryCentre": // We do not do anything with scanned barycentres at this time (though the raw event is still passed to the EDDN responder)
                            case "Scanned": // Written at the end of a successful scan, too late to react to this.
                            case "SharedBookmarkToSquadron": // Unnecessary.
                            case "ShipyardRedeem": // Unnecessary, all of the necessary information is already given in the `ShipRedeem` event.
                            case "TradeMicroResources": // This is always followed by `ShipLocker`, which we can use to keep our inventory up to date
                            case "TransferMicroResources": // Removed, no longer written
                            case "UseConsumable": // Seems to include only medkits and energy cells (grenades not included) and it's not needed. The `BackpackChange` event keeps us up to date.
                            case "USSDrop": // Superseded by / redundant with the `SupercruiseDestinationDrop` event.
                            case "WonATrophyForSquadron": // No interesting data here so no reason to add this.
                                break;
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
                Logging.Debug(jre.Message, jre);
                try
                {
                    if (line.Contains("\"event\":\"BackpackChange\"") && line.Contains("] \"Removed\""))
                    {
                        // We've observed a missing comma in the `BackpackChange` event, fix that here.
                        line = line.Replace("] \"Removed\"", "], \"Removed\"");
                        return ParseJournalEntry(line, fromLogLoad);
                    }
                }
                catch
                {
                    // Unable to recover so re-throw.
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Exception whilst parsing journal line {line}", ex);
            }
            return events;
        }

        private static void GetThargoidWarData ( IDictionary<string, object> data, out ThargoidWar thargoidWar )
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
                    if ( !string.IsNullOrEmpty(remainingDaysString) )
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

        private static void GetPowerplayData(IDictionary<string, object> data, out List<Power> powerplayPowers, out PowerplayState powerplayState)
        {
            powerplayPowers = new List<Power>();
            data.TryGetValue("Powers", out object powersVal);
            // There can be more than one power listed for a system when the system is being contested
            if (powersVal is List<object> powerNames)
            {
                foreach ( var powerName in powerNames )
                {
                    powerplayPowers.Add(Power.FromEDName((string)powerName));
                }
            }
            powerplayState = PowerplayState.FromEDName(JsonParsing.getString(data, "PowerplayState")) ?? PowerplayState.None;
        }

        private static Superpower GetAllegiance(IDictionary<string, object> data, string key)
        {
            data.TryGetValue(key, out object val);
            // FD may send "" rather than null;
            return Superpower.FromNameOrEdName(Convert.ToString(val));
        }

        private static List<Conflict> GetConflicts(object conflictsVal, List<Faction> factions)
        {
            if (conflictsVal is null || factions is null) { return null; }

            List<Conflict> conflicts = new List<Conflict>();
            if ( conflictsVal is List<object> conflictsList )
            {
                foreach ( IDictionary<string, object> conflictDetail in conflictsList )
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

        private static string GetFactionName(IDictionary<string, object> data, string key)
        {
            string faction = JsonParsing.getString(data, key);
            // Might be a superpower...
            Superpower superpowerFaction = Superpower.FromNameOrEdName(faction);
            return superpowerFaction?.invariantName ?? faction;
        }

        private static Faction GetFaction(IDictionary<string, object> data, string type, string systemName)
        {
            Faction faction = null;

            // Get the faction name and state
            if (data.TryGetValue(type + "Faction", out object factionVal))
            {
                var factionName = string.Empty;
                var factionState = FactionState.None;

                if ( factionVal is Dictionary<string, object> factionData) // 3.3.03 or later journal
                {
                    factionName = JsonParsing.getString( factionData, "Name" );
                    factionState = FactionState.FromEDName( JsonParsing.getString( factionData, "FactionState" ) ) ??
                                   FactionState.None;
                }
                else if (factionVal is string s) // per-3.3.03 journal
                {
                    factionName = s;
                }

                if ( string.IsNullOrEmpty(factionName) )
                {
                    throw new ArgumentException( "Faction information could not be parsed" );
                }

                faction = EDDI.Instance.CurrentStarSystem?.factions.FirstOrDefault( f => f.name == factionName ) ??
                          new Faction { name = factionName };

                // Get the faction information specific to the star system
                var factionPresense = faction.presences.FirstOrDefault( p => p.systemName == systemName );
                if ( factionPresense is null && !string.IsNullOrEmpty( systemName ) )
                {
                    factionPresense = new FactionPresence()
                    {
                        systemName = systemName,
                        FactionState = factionState
                    };
                    faction.presences.Add( factionPresense );
                }
            }

            // Since systems can have Thargoid allegiance, treat Thargoids like a faction for the purpose of
            // allegiance and government (even when no human faction exists).

            // Get the faction allegiance
            if ( data.TryGetValue( type + "Allegiance", out _ ) )
            {
                if ( faction is null ) { faction = new Faction(); }
                faction.Allegiance = GetAllegiance( data, type + "Allegiance" );
                if ( string.IsNullOrEmpty( faction.name ) && faction.Allegiance != null )
                {
                    faction.name = faction.Allegiance.localizedName;
                }
            }
            else if ( data.TryGetValue( "Factions", out var val ) && val is List<object> factionsList )
            {
                // Station controlling faction government is not discretely available in 'Location' event
                if ( faction is null ) { faction = new Faction(); }
                foreach ( IDictionary<string, object> factionDetail in factionsList )
                {
                    string fName = JsonParsing.getString( factionDetail, "Name" );
                    if ( fName == faction.name )
                    {
                        faction.Allegiance = GetAllegiance( factionDetail, "Allegiance" );
                        break;
                    }
                }
            }

            // Get the controlling faction (system or station) government
            if ( data.TryGetValue( type + "Government", out _ ) )
            {
                if ( faction is null ) { faction = new Faction(); }
                faction.Government = Government.FromEDName( JsonParsing.getString( data, type + "Government" ) ) ?? Government.None;
            }

            return faction;
        }

        private static List<Faction> GetFactions(object factionsVal, string systemName)
        {
            List<Faction> factions = new List<Faction>();
            if ( factionsVal is List<object> factionsList )
            {
                foreach ( IDictionary<string, object> factionDetail in factionsList )
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
                        name = fName, Government = fGov, Allegiance = fAllegiance, myreputation = myReputation
                    };

                    FactionPresence factionPresense = new FactionPresence()
                    {
                        systemName = systemName, FactionState = fState, influence = influence, Happiness = happiness,
                    };

                    // Active states
                    factionDetail.TryGetValue( "ActiveStates", out object activeStatesVal );
                    if ( activeStatesVal != null )
                    {
                        var activeStatesList = (List<object>)activeStatesVal;
                        foreach ( IDictionary<string, object> activeState in activeStatesList )
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
                        foreach ( IDictionary<string, object> pendingState in pendingStatesList )
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
                        foreach ( IDictionary<string, object> recoveringState in recoveringStatesList )
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

        private static SignalSource GetSignalSourceName(IDictionary<string, object> data)
        {
            // The source may be a direct source or a USS. If a USS, we want the USS type.
            SignalSource source;
            if (JsonParsing.getString(data, "USSType") != null)
            {
                string signalSource = JsonParsing.getString(data, "USSType");
                source = SignalSource.FromEDName(signalSource) ?? new SignalSource();
                var localizedName = JsonParsing.getString(data, "USSType_Localised");
                if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                {
                    source.fallbackLocalizedName = localizedName;
                }
            }
            else
            {
                string signalSource = JsonParsing.getString(data, "SignalName");
                var isStation = JsonParsing.getOptionalBool(data, "IsStation") ?? false;
                if (isStation)
                {
                    source = SignalSource.FromStationEDName(signalSource);
                }
                else
                {
                    source = SignalSource.FromEDName(signalSource) ?? new SignalSource();
                    var localizedName = JsonParsing.getString(data, "SignalName_Localised");
                    if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                    {
                        source.fallbackLocalizedName = localizedName;
                    }
                }
                source.isStation = isStation;
            }
            return source;
        }

        // Be sensible with health - round it unless it's very low
        private static decimal sensibleHealth(decimal health)
        {
            return (health < 10 ? Math.Round(health, 1) : Math.Round(health));
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
            foreach (var carrierJumpCancellationTS in carrierJumpCancellationTokenSources.Values) { carrierJumpCancellationTS.Cancel(); }
            stop();
        }

        public void Reload() { }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        private static string GetSavedGamesDir()
        {
            int result = NativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out IntPtr path);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous";
            }
            else
            {
                throw new ExternalException("Failed to find the saved games directory.", result);
            }
        }

        internal class NativeMethods
        {
            [DllImport("Shell32.dll")]
            internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        }

        public void PreHandle(Event @event)
        { }

        public void PostHandle(Event @event)
        { }

        public void HandleProfile(JObject profile)
        { }

        public void HandleStatus ( Status status )
        {
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            return null;
        }

        private static string getRole(IDictionary<string, object> data, string key)
        {
            string role = JsonParsing.getString(data, key);
            if (role == "FireCon")
            {
                role = "Gunner";
            }
            else if (role == "FighterCon")
            {
                role = "Fighter";
            }
            return role;
        }

        private static Engineer parseEngineer(IDictionary<string, object> data)
        {
            string engineer = JsonParsing.getString(data, "Engineer");
            long engineerId = JsonParsing.getLong(data, "EngineerID");
            data.TryGetValue("Rank", out object rankVal);
            int? rank = (int?)(long?)rankVal;
            data.TryGetValue("RankProgress", out object rankProgressVal);
            int? rankProgress = (int?)(long?)rankProgressVal;
            string stage = JsonParsing.getString(data, "Progress");
            return new Engineer(engineer, engineerId, stage, rankProgress, rank);
        }

        private static Compartment parseShipCompartment(string shipEDName, string slot)
        {
            Compartment compartment = new Compartment() { name = slot };

            // Compartment slots are in the form of "Slotnn_Sizen" or "Militarynn"
            if (slot.Contains("Slot"))
            {
                Match matches = Regex.Match(compartment.name, @"Size([0-9]+)");
                if (matches.Success)
                {
                    compartment.size = Int32.Parse(matches.Groups[1].Value);
                }
            }
            else if (slot.Contains("Military"))
            {
                var slotSize = ShipDefinitions.FromEDModel(shipEDName, false)?.militarysize;
                if (slotSize is null)
                {
                    // We didn't expect to have a military slot on this ship.
                    var data = new Dictionary<string, object>() { { "ShipEDName", shipEDName }, { "Slot", slot }, { "Exception", new ArgumentException() } };
                    Logging.Error($"Unexpected military slot found in ship edName {shipEDName}.", data);
                    return compartment;
                }
                compartment.size = (int)slotSize;
            }
            return compartment;
        }

        private static readonly string[] ignoredLogLoadEvents = new string[]
        {
            // We ignore these events when parsing / loading a log for a game session already in process.
            "AfmuRepairs",
            "ChangeCrewRole",
            "ClearSavedGame",
            "CockpitBreached",
            "Continued",
            "CrewFire",
            "CrewLaunchFighter",
            "CrewMemberJoins",
            "CrewMemberQuits",
            "CrewMemberRoleChange",
            "DataScanned",
            "DatalinkScan",
            "DockingCancelled",
            "DockingDenied",
            "DockingGranted",
            "DockingRequested",
            "DockingTimeout",
            "EndCrewSession",
            "EscapeInterdiction",
            "FSDTarget",
            "FuelScoop",
            "HeatDamage",
            "HeatWarning",
            "JetConeBoost",
            "JetConeDamage",
            "KickCrewMember",
            "MaterialDiscovered",
            "Music",
            "NpcCrewRank",
            "Powerplay",
            "ReceiveText",
            "Scanned",
            "SendText",
            "ShieldState",
            "Shutdown",
            "SystemsShutdown",
            "UnderAttack",
            "WingAdd",
            "WingInvite",
            "WingJoin",
            "WingLeave"
        };
    }
}
