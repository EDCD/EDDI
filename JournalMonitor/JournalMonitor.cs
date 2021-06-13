using EddiCargoMonitor;
using EddiCore;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiMissionMonitor;
using EddiShipMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
    public class JournalMonitor : LogMonitor, EDDIMonitor
    {
        private static readonly Regex JsonRegex = new Regex(@"^{.*}$", RegexOptions.Singleline);
        public JournalMonitor() : base(GetSavedGamesDir(), @"^Journal.*\.[0-9\.]+\.log$", (result, isLogLoadEvent) =>
        ForwardJournalEntry(result, EDDI.Instance.enqueueEvent, isLogLoadEvent))
        { }

        private enum ShipyardType { ShipsHere, ShipsRemote }

        private static Dictionary<long, CancellationTokenSource> carrierJumpCancellationTokenSources = new Dictionary<long, CancellationTokenSource>();

        public static void ForwardJournalEntry(string line, Action<Event> callback, bool isLogLoadEvent)
        {
            if (line == null)
            {
                return;
            }

            List<Event> events = ParseJournalEntry(line, isLogLoadEvent);
            foreach (Event @event in events)
            {
                // The DiscoveryScanEvent and SystemScanComplete events may be written before all applicable scans have been queued.
                // We will wait a short period of time after these events take place so that all scans generated in tandem 
                // with these events are enqueued before these events are enqueued.
                if ((@event is DiscoveryScanEvent || @event is SystemScanComplete) && !@event.fromLoad)
                {
                    Task.Run(async () =>
                    {
                        int timeout = 0;
                        do
                        {
                            await Task.Delay(1500);
                            timeout++;
                        }
                        while (EDDI.Instance.CurrentStarSystem.bodies.Count == 0 && timeout < 3);
                        callback(@event);
                    });
                    continue;
                }

                callback(@event);
            }
        }

        public static List<Event> ParseJournalEntry(string line, bool fromLogLoad = false)
        {
            List<Event> events = new List<Event>();
            try
            {
                Match match = JsonRegex.Match(line);
                if (match.Success)
                {
                    IDictionary<string, object> data = Deserializtion.DeserializeData(line);

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
                        Logging.Warn("Event without event field!");
                        return null;
                    }

                    bool handled = false;

                    string edType = JsonParsing.getString(data, "event");
                    if (edType == "Fileheader")
                    {
                        EDDI.Instance.JournalTimeStamp = DateTime.MinValue;
                    }
                    else
                    {
                        EDDI.Instance.JournalTimeStamp = timestamp;
                    }

                    try
                    {
                        switch (edType)
                        {
                            case "Docked":
                                {
                                    string systemName = JsonParsing.getString(data, "StarSystem");
                                    long? systemAddress = JsonParsing.getOptionalLong(data, "SystemAddress");
                                    long? marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    string stationState = JsonParsing.getString(data, "StationState") ?? string.Empty;
                                    StationModel stationModel = StationModel.FromEDName(JsonParsing.getString(data, "StationType")) ?? StationModel.None;
                                    Faction controllingfaction = getFaction(data, "Station", systemName);
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
                                    var systemAddress = JsonParsing.getOptionalLong(data, "SystemAddress");
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
                                    nearestDestination.fallbackLocalizedName = JsonParsing.getString(data, "SignalName_Localised") ?? nearestdestination;

                                    events.Add(new TouchdownEvent(timestamp, longitude, latitude, system, systemAddress, body, bodyId, onStation, onPlanet, taxi, multicrew, playercontrolled, nearestDestination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Liftoff":
                                {
                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getOptionalLong(data, "SystemAddress");
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
                                    nearestDestination.fallbackLocalizedName = JsonParsing.getString(data, "SignalName_Localised") ?? nearestdestination;

                                    events.Add(new LiftoffEvent(timestamp, longitude, latitude, system, systemAddress, body, bodyId, onStation, onPlanet, taxi, multicrew, playercontrolled, nearestDestination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SupercruiseEntry":
                                {
                                    string system = JsonParsing.getString(data, "StarySystem");
                                    long? systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    bool? taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    bool? multicrew = JsonParsing.getOptionalBool(data, "Multicrew");
                                    events.Add(new EnteredSupercruiseEvent(timestamp, system, systemAddress, taxi, multicrew) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SupercruiseExit":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    BodyType bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType")) ?? BodyType.None;
                                    bool? taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    bool? multicrew = JsonParsing.getOptionalBool(data, "Multicrew");
                                    events.Add(new EnteredNormalSpaceEvent(timestamp, system, systemAddress, body, bodyId, bodyType, taxi, multicrew) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSDJump":
                                {
                                    string systemName = JsonParsing.getString(data, "StarSystem");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
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
                                    Faction controllingfaction = getFaction(data, "System", systemName);
                                    Economy economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy")) ?? Economy.None;
                                    Economy economy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy")) ?? Economy.None;
                                    SecurityLevel security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity")) ?? SecurityLevel.None;
                                    long? population = JsonParsing.getOptionalLong(data, "Population");

                                    // Parse factions array data
                                    List<Faction> factions = new List<Faction>();
                                    data.TryGetValue("Factions", out object factionsVal);
                                    if (factionsVal != null)
                                    {
                                        factions = getFactions(factionsVal, systemName);
                                    }

                                    // Parse conflicts array data
                                    List<Conflict> conflicts = new List<Conflict>();
                                    data.TryGetValue("Conflicts", out object conflictsVal);
                                    if (conflictsVal != null)
                                    {
                                        conflicts = getConflicts(conflictsVal, factions);
                                    }

                                    // Powerplay data (if pledged)
                                    Power powerplayPower = new Power();
                                    getPowerplayData(data, out powerplayPower, out PowerplayState powerplayState);

                                    bool? taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    bool? multicrew = JsonParsing.getOptionalBool(data, "Multicrew");

                                    events.Add(new JumpedEvent(timestamp, systemName, systemAddress, x, y, z, starName, distance, fuelUsed, fuelRemaining, boostUsed, controllingfaction, factions, conflicts, economy, economy2, security, population, powerplayPower, powerplayState, taxi, multicrew) { raw = line, fromLoad = fromLogLoad });
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
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    decimal? distFromStarLs = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

                                    string body = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    BodyType bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType"));
                                    bool docked = JsonParsing.getBool(data, "Docked");
                                    Faction systemfaction = getFaction(data, "System", systemName);
                                    Faction stationfaction = getFaction(data, "Station", systemName);
                                    Economy economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy"));
                                    Economy economy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy"));
                                    SecurityLevel security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity"));
                                    long? population = JsonParsing.getOptionalLong(data, "Population");

                                    // If docked
                                    string station = JsonParsing.getString(data, "StationName");
                                    StationModel stationtype = StationModel.FromEDName(JsonParsing.getString(data, "StationType"));
                                    long? marketId = JsonParsing.getOptionalLong(data, "MarketID");

                                    // If landed
                                    decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    // Parse factions array data
                                    List<Faction> factions = new List<Faction>();
                                    data.TryGetValue("Factions", out object factionsVal);
                                    if (factionsVal != null)
                                    {
                                        factions = getFactions(factionsVal, systemName);
                                    }

                                    // Powerplay data (if pledged)
                                    Power powerplayPower = new Power();
                                    getPowerplayData(data, out powerplayPower, out PowerplayState powerplayState);

                                    bool taxi = JsonParsing.getOptionalBool(data, "Taxi") ?? false;
                                    bool multicrew = JsonParsing.getOptionalBool(data, "Multicrew") ?? false;
                                    bool inSRV = JsonParsing.getOptionalBool(data, "InSRV") ?? false;
                                    bool onFoot = JsonParsing.getOptionalBool(data, "OnFoot") ?? false;
                                    
                                    events.Add(new LocationEvent(timestamp, systemName, x, y, z, systemAddress, distFromStarLs, body, bodyId, bodyType, docked, station, stationtype, marketId, systemfaction, stationfaction, economy, economy2, security, population, longitude, latitude, factions, powerplayPower, powerplayState, taxi, multicrew, inSRV, onFoot) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Bounty":
                                {
                                    string target = JsonParsing.getString(data, "Target");
                                    if (target != null)
                                    {
                                        // Might be a ship
                                        var targetShip = ShipDefinitions.FromEDModel(target, false);

                                        // Might be a SRV or Fighter
                                        var targetVehicle = VehicleDefinition.EDNameExists(target) ? VehicleDefinition.FromEDName(target) : null;

                                        // Might be an on foot commander
                                        var targetCmdrSuit = Suit.EDNameExists(target) ? Suit.FromEDName(target) : null;

                                        // Might be an on foot NPC
                                        var targetNpcSuitLoadout = NpcSuitLoadout.EDNameExists(target) ? NpcSuitLoadout.FromEDName(target) : null;

                                        target = targetShip?.SpokenModel()
                                            ?? targetCmdrSuit?.localizedName
                                            ?? targetVehicle?.localizedName
                                            ?? targetNpcSuitLoadout?.localizedName
                                            ?? JsonParsing.getString(data, "Target_Localised")
                                            ;
                                    }

                                    string victimFaction = getFactionName(data, "VictimFaction");

                                    data.TryGetValue("SharedWithOthers", out object val);
                                    bool shared = false;
                                    if (val != null && (long)val == 1)
                                    {
                                        shared = true;
                                    }

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
                                        string factionName = getFactionName(data, "Faction");
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
                                                string factionName = getFactionName(rewardData, "Faction");
                                                rewardData.TryGetValue("Reward", out val);
                                                long factionReward = (long)val;

                                                rewards.Add(new Reward(factionName, factionReward));
                                            }
                                        }
                                    }

                                    events.Add(new BountyAwardedEvent(timestamp, target, victimFaction, reward, rewards, shared) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CapShipBond":
                            case "DatalinkVoucher":
                            case "FactionKillBond":
                                {
                                    data.TryGetValue("Reward", out object val);
                                    long reward = (long)val;
                                    string victimFaction = getFactionName(data, "VictimFaction");

                                    if (data.ContainsKey("AwardingFaction"))
                                    {
                                        string awardingFaction = getFactionName(data, "AwardingFaction");
                                        events.Add(new BondAwardedEvent(timestamp, awardingFaction, victimFaction, reward) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (data.ContainsKey("PayeeFaction"))
                                    {
                                        string payeeFaction = getFactionName(data, "PayeeFaction");
                                        events.Add(new DataVoucherAwardedEvent(timestamp, payeeFaction, victimFaction, reward) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "CommitCrime":
                                {
                                    object val;
                                    string crimetype = JsonParsing.getString(data, "CrimeType");
                                    string faction = getFactionName(data, "Faction");
                                    string victim = JsonParsing.getString(data, "Victim");
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
                                    int shipId = (int)(long)val;
                                    string ship = JsonParsing.getString(data, "Ship");
                                    string shipName = JsonParsing.getString(data, "ShipName");
                                    string shipIdent = JsonParsing.getString(data, "ShipIdent");

                                    long? hullValue = JsonParsing.getOptionalLong(data, "HullValue");
                                    long? modulesValue = JsonParsing.getOptionalLong(data, "ModulesValue");
                                    decimal hullHealth = sensibleHealth(JsonParsing.getDecimal(data, "HullHealth") * 100);
                                    decimal unladenMass = JsonParsing.getOptionalDecimal(data, "UnladenMass") ?? 0;
                                    decimal maxJumpRange = JsonParsing.getOptionalDecimal(data, "MaxJumpRange") ?? 0;
                                    decimal optimalMass = 0;

                                    long rebuy = JsonParsing.getLong(data, "Rebuy");

                                    // If ship is 'hot', then modules are also 'hot'
                                    bool hot = JsonParsing.getOptionalBool(data, "Hot") ?? false;

                                    data.TryGetValue("Modules", out val);
                                    List<object> modulesData = (List<object>)val;

                                    string paintjob = null;
                                    List<Hardpoint> hardpoints = new List<Hardpoint>();
                                    List<Compartment> compartments = new List<Compartment>();
                                    if (modulesData != null)
                                    {
                                        foreach (Dictionary<string, object> moduleData in modulesData)
                                        {
                                            // Common items
                                            string slot = JsonParsing.getString(moduleData, "Slot");
                                            string item = JsonParsing.getString(moduleData, "Item");
                                            bool enabled = JsonParsing.getBool(moduleData, "On");
                                            int priority = JsonParsing.getInt(moduleData, "Priority");
                                            // Health is as 0->1 but we want 0->100, and to a sensible number of decimal places
                                            decimal health = JsonParsing.getDecimal(moduleData, "Health") * 100;
                                            if (health < 5)
                                            {
                                                health = Math.Round(health, 1);
                                            }
                                            else
                                            {
                                                health = Math.Round(health);
                                            }

                                            // Some built-in modules don't give "Value" keys in the Loadout event. We'll set them to zero to match the Frontier API.
                                            long price = JsonParsing.getOptionalLong(moduleData, "Value") ?? 0;

                                            // Ammunition
                                            int? clip = JsonParsing.getOptionalInt(moduleData, "AmmoInClip");
                                            int? hopper = JsonParsing.getOptionalInt(moduleData, "AmmoInHopper");

                                            // Engineering modifications
                                            moduleData.TryGetValue("Engineering", out object engineeringVal);
                                            bool modified = engineeringVal != null ? true : false;
                                            Dictionary<string, object> engineeringData = (Dictionary<string, object>)engineeringVal;
                                            string blueprint = modified ? JsonParsing.getString(engineeringData, "BlueprintName") : null;
                                            long blueprintId = modified ? JsonParsing.getLong(engineeringData, "BlueprintID") : 0;
                                            int level = modified ? JsonParsing.getInt(engineeringData, "Level") : 0;
                                            Blueprint modification = Blueprint.FromEliteID(blueprintId, engineeringData)
                                                ?? Blueprint.FromEDNameAndGrade(blueprint, level) ?? Blueprint.None;
                                            decimal quality = modified ? JsonParsing.getDecimal(engineeringData, "Quality") : 0;
                                            string experimentalEffect = modified ? JsonParsing.getString(engineeringData, "ExperimentalEffect") : null;
                                            List<EngineeringModifier> modifiers = new List<EngineeringModifier>();
                                            if (modified)
                                            {
                                                engineeringData.TryGetValue("Modifiers", out object modifiersVal);
                                                List<object> modifiersData = (List<object>)modifiersVal;
                                                foreach (Dictionary<string, object> modifier in modifiersData)
                                                {
                                                    try
                                                    {
                                                        string edname = JsonParsing.getString(modifier, "Label");
                                                        decimal? currentValue = JsonParsing.getOptionalDecimal(modifier, "Value");
                                                        decimal? originalValue = JsonParsing.getOptionalDecimal(modifier, "OriginalValue");
                                                        bool lessIsGood = JsonParsing.getOptionalInt(modifier, "LessIsGood") == 1;
                                                        string valueStr = JsonParsing.getString(modifier, "ValueStr");
                                                        modifiers.Add(new EngineeringModifier()
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
                                                        Dictionary<string, object> modVal = new Dictionary<string, object>()
                                                            {
                                                                { "Exception", e },
                                                                { "Module", item },
                                                                { "Engineering", engineeringData }
                                                            };
                                                        Logging.Error("Failed to parse engineering modification", modVal);
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

                                                Module module = new Module(Module.FromEDName(item) ?? new Module());
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

                                                Module module = new Module(Module.FromEDName(item) ?? new Module());
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

                                                // Get the optimal mass for the Frame Shift Drive
                                                if (slot.Equals("FrameShiftDrive", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    string fsd = module.@class + module.grade;
                                                    Constants.baseOptimalMass.TryGetValue(fsd, out optimalMass);
                                                    if (modified)
                                                    {
                                                        foreach (EngineeringModifier modifier in modifiers)
                                                        {
                                                            if (modifier.EDName.Equals("FSDOptimalMass", StringComparison.InvariantCultureIgnoreCase))
                                                            {
                                                                optimalMass = (decimal)modifier.currentValue;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    events.Add(new ShipLoadoutEvent(timestamp, ship, shipId, shipName, shipIdent, hullValue, modulesValue, hullHealth, unladenMass, maxJumpRange, optimalMass, rebuy, hot, compartments, hardpoints, paintjob) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleInfo":
                                events.Add(new ModuleInfoEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                handled = true;
                                break;
                            case "CockpitBreached":
                                events.Add(new CockpitBreachedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                handled = true;
                                break;
                            case "ApproachBody":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    events.Add(new NearSurfaceEvent(timestamp, true, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LeaveBody":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    events.Add(new NearSurfaceEvent(timestamp, false, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ApproachSettlement":
                                {
                                    string settlementname = JsonParsing.getString(data, "Name");
                                    long? marketId = JsonParsing.getOptionalLong(data, "MarketID"); // Tourist beacons are reported as settlements without MarketID
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    string bodyName = JsonParsing.getString(data, "BodyName");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");

                                    // The settlement name may be a proper name or a generic signal type.
                                    SignalSource settlementName = SignalSource.FromEDName(settlementname) ?? new SignalSource();
                                    settlementName.fallbackLocalizedName = JsonParsing.getString(data, "Name_Localised") ?? settlementname;

                                    decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    events.Add(new SettlementApproachedEvent(timestamp, settlementName, marketId, systemAddress, bodyName, bodyId, latitude, longitude) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Scan":
                                {
                                    string name = JsonParsing.getString(data, "BodyName");
                                    string scantype = JsonParsing.getString(data, "ScanType");

                                    string systemName = JsonParsing.getString(data, "StarSystem");
                                    long? systemAddress = JsonParsing.getOptionalLong(data, "SystemAddress");

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
                                    bool? alreadydiscovered = JsonParsing.getOptionalBool(data, "WasDiscovered");
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

                                        Body star = new Body(name, bodyId, parents, distanceLs, stellarclass, stellarsubclass, stellarMass, radiusKm, absoluteMagnitude, ageMegaYears, temperatureKelvin, luminosityClass, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, alreadydiscovered, alreadymapped, systemName, systemAddress)
                                        {
                                            scanned = (DateTime?)timestamp
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

                                        Body body = new Body(name, bodyId, parents, distanceLs, tidallyLocked, terraformState, planetClass, atmosphereClass, atmosphereCompositions, volcanism, earthMass, radiusKm, gravity, temperatureKelvin, pressureAtm, landable, materials, solidCompositions, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, reserveLevel, alreadydiscovered, alreadymapped, systemName, systemAddress)
                                        {
                                            scanned = (DateTime?)timestamp
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
                                    var info = ShipyardInfo.FromFile();
                                    if (info.PriceList != null && info.MarketID == marketId
                                        && info.StarSystem == system
                                        && info.StationName == station
                                        && info.Horizons == EDDI.Instance.inHorizons)
                                    {
                                        events.Add(new ShipyardEvent(timestamp, marketId, station, system, info) { raw = line, fromLoad = fromLogLoad });
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

                                    var ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetShip(shipId);
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
                                            module.engineermodification = Blueprint.FromEDName(engineerModifications) ?? Blueprint.None;
                                            module.engineerquality = JsonParsing.getOptionalDecimal(item, "Quality") ?? 0;
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
                                    var info = OutfittingInfo.FromFile();
                                    if (info.Items != null && info.MarketID == marketId
                                        && info.StarSystem == system
                                        && info.StationName == station
                                        && info.Horizons == EDDI.Instance.inHorizons)
                                    {
                                        events.Add(new OutfittingEvent(timestamp, marketId, station, system, info) { raw = line, fromLoad = fromLogLoad });
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

                                    events.Add(new SRVLaunchedEvent(timestamp, loadout, playercontrolled, id) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockSRV":
                                {
                                    int srvId = JsonParsing.getInt(data, "ID");
                                    events.Add(new SRVDockedEvent(timestamp, srvId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SRVDestroyed":
                                {
                                    string vehicle = "srv";
                                    int srvId = JsonParsing.getInt(data, "ID");
                                    events.Add(new VehicleDestroyedEvent(timestamp, vehicle, srvId) { raw = line, fromLoad = fromLogLoad });
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
                                    events.Add(new VehicleDestroyedEvent(timestamp, vehicle, fighterId) { raw = line, fromLoad = fromLogLoad });
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
                                    bool iscommander = JsonParsing.getBool(data, "IsPlayer");
                                    data.TryGetValue("CombatRank", out object val);
                                    CombatRating rating = (val == null ? null : CombatRating.FromRank(Convert.ToInt32(val)));
                                    string faction = getFactionName(data, "Faction");
                                    string power = JsonParsing.getString(data, "Power");

                                    events.Add(new ShipInterdictedEvent(timestamp, true, submitted, iscommander, interdictor, rating, faction, power) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "EscapeInterdiction":
                                {
                                    string interdictor = JsonParsing.getString(data, "Interdictor");
                                    bool iscommander = JsonParsing.getBool(data, "IsPlayer");

                                    events.Add(new ShipInterdictedEvent(timestamp, false, false, iscommander, interdictor, null, null, null) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "Interdiction":
                                {
                                    bool success = JsonParsing.getBool(data, "Success");
                                    string interdictee = JsonParsing.getString(data, "Interdicted");
                                    bool iscommander = JsonParsing.getBool(data, "IsPlayer");
                                    data.TryGetValue("CombatRank", out object val);
                                    CombatRating rating = (val == null ? null : CombatRating.FromRank((int)val));
                                    string faction = getFactionName(data, "Faction");
                                    string power = JsonParsing.getString(data, "Power");

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
                                    events.Add(new FSDEngagedEvent(timestamp, target, system, stellarclass) { raw = line, fromLoad = fromLogLoad });
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
                                            from = JsonParsing.getString(data, "From_Localised");
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
                                        events.Add(new MessageReceivedEvent(timestamp, from, source, false, messageChannel, JsonParsing.getString(data, "Message_Localised")) { raw = line, fromLoad = fromLogLoad });

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
                                        else if (message.Contains("_StartInterdiction"))
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
                                    if (fighter == true && piloted == false)
                                    {
                                        vehicle = Constants.VEHICLE_FIGHTER;
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
                                    bool targetlocked = JsonParsing.getBool(data, "TargetLocked");

                                    // Target locked
                                    int? scanstage = JsonParsing.getOptionalInt(data, "ScanStage");
                                    string ship = JsonParsing.getString(data, "Ship");
                                    if (ship != null)
                                    {
                                        if (ship.Contains("fighter"))
                                        {
                                            string shipLocalised = JsonParsing.getString(data, "Ship_Localised");
                                            if (shipLocalised != null)
                                            {
                                                ship = shipLocalised + " Fighter";
                                            }
                                        }
                                        else
                                        {
                                            Ship shipDef = ShipDefinitions.FromEDModel(ship, false);
                                            if (shipDef != null)
                                            {
                                                ship = shipDef.model;
                                            }
                                        }
                                    }

                                    // Scan stage >= 1
                                    string name = JsonParsing.getString(data, "PilotName_Localised");
                                    CombatRating rank = CombatRating.FromEDName(JsonParsing.getString(data, "PilotRank"));

                                    // Scan stage >= 2
                                    decimal? shieldHealth = JsonParsing.getOptionalDecimal(data, "ShieldHealth");
                                    decimal? hullHealth = JsonParsing.getOptionalDecimal(data, "HullHealth");

                                    // Scan stage >= 3
                                    string faction = JsonParsing.getString(data, "Faction");
                                    LegalStatus legalStatus = LegalStatus.FromEDName(JsonParsing.getString(data, "LegalStatus")) ?? LegalStatus.None;
                                    Power power = Power.FromEDName(JsonParsing.getString(data, "Power")) ?? Power.None;
                                    int? bounty = JsonParsing.getOptionalInt(data, "Bounty");
                                    string subSystem = JsonParsing.getString(data, "Subsystem_Localised");
                                    decimal? subSystemHealth = JsonParsing.getOptionalDecimal(data, "SubsystemHealth");

                                    events.Add(new ShipTargetedEvent(timestamp, targetlocked, ship, scanstage, name, rank, faction, power, legalStatus, bounty, shieldHealth, hullHealth, subSystem, subSystemHealth) { raw = line, fromLoad = fromLogLoad });
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
                                    string option = JsonParsing.getString(data, "Option");
                                    long price = JsonParsing.getLong(data, "Cost");

                                    if (option == "rebuy")
                                    {
                                        events.Add(new ShipRepurchasedEvent(timestamp, price) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "NavBeaconScan":
                                {
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    data.TryGetValue("NumBodies", out object val);
                                    int numbodies = (int)(long)val;
                                    events.Add(new NavBeaconScanEvent(timestamp, systemAddress, numbodies) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSSDiscoveryScan":
                                {
                                    decimal progress = JsonParsing.getDecimal(data, "Progress"); // value from 0-1
                                    int bodyCount = JsonParsing.getInt(data, "BodyCount"); // total number of stellar bodies in system
                                    int nonBodyCount = JsonParsing.getInt(data, "NonBodyCount"); // total number of non-body signals found
                                    events.Add(new DiscoveryScanEvent(timestamp, progress, bodyCount, nonBodyCount) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSSSignalDiscovered":
                                {
                                    long? systemAddress = JsonParsing.getLong(data, "SystemAddress");

                                    SignalSource source = GetSignalSourceName(data);
                                    source.spawningFaction = getFactionName(data, "SpawningFaction") ?? Superpower.None.localizedName; // the minor faction, if relevant
                                    var secondsRemaining = JsonParsing.getOptionalDecimal(data, "TimeRemaining"); // remaining lifetime in seconds, if relevant
                                    source.expiry = secondsRemaining is null ? (DateTime?)null : timestamp.AddSeconds((double)(secondsRemaining));

                                    string spawningstate = JsonParsing.getString(data, "SpawningState");
                                    string normalizedSpawningState = spawningstate?.Replace("$FactionState_", "")?.Replace("_desc;", "");
                                    source.spawningState = FactionState.FromEDName(normalizedSpawningState) ?? new FactionState();
                                    source.spawningState.fallbackLocalizedName = JsonParsing.getString(data, "SpawningState_Localised");

                                    source.threatLevel = JsonParsing.getOptionalInt(data, "ThreatLevel") ?? 0;
                                    source.isStation = JsonParsing.getOptionalBool(data, "IsStation") ?? false;

                                    bool unique = false;
                                    if (EDDI.Instance.CurrentStarSystem != null && EDDI.Instance.CurrentStarSystem.systemAddress == systemAddress)
                                    {
                                        unique = !EDDI.Instance.CurrentStarSystem.signalsources.Contains(source.localizedName);
                                        EDDI.Instance.CurrentStarSystem.AddOrUpdateSignalSource(source);
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
                                    long? systemAddress = JsonParsing.getOptionalLong(data, "SystemAddress");
                                    int probesUsed = JsonParsing.getInt(data, "ProbesUsed");
                                    int efficiencyTarget = JsonParsing.getInt(data, "EfficiencyTarget");

                                    // Target may be either a ring or a body
                                    StarSystem system = EDDI.Instance?.CurrentStarSystem;
                                    Body body = null;

                                    if (bodyName.EndsWith(" Ring"))
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
                                            body.scanned = body.scanned ?? timestamp;
                                            body.mapped = timestamp;
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
                            case "USSDrop":
                                {
                                    SignalSource source = GetSignalSourceName(data);
                                    data.TryGetValue("USSThreat", out object val);
                                    int threat = (int)(long)val;
                                    events.Add(new EnteredSignalSourceEvent(timestamp, source, threat) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Market":
                                {
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string station = JsonParsing.getString(data, "StationName");
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    var info = MarketInfo.FromFile();
                                    if (info != null && info.MarketID == marketId
                                        && info.StarSystem == system
                                        && info.StationName == station)
                                    {
                                        events.Add(new MarketEvent(timestamp, marketId, station, system, info) { raw = line, fromLoad = fromLogLoad });
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

                                    string contributionType = JsonParsing.getString(data, "Type"); // (Commodity, materials, Credits, Bond, Bounty)
                                    switch (contributionType)
                                    {
                                        case "Commodity":
                                            {
                                                string edname = JsonParsing.getString(data, "Commodity");
                                                int amount = JsonParsing.getInt(data, "Quantity");
                                                int total = JsonParsing.getInt(data, "TotalQuantity");
                                                CommodityAmount commodity = new CommodityAmount(CommodityDefinition.FromEDName(edname), amount);
                                                events.Add(new EngineerContributedEvent(timestamp, engineer, commodity, null, contributionType, amount, total) { raw = line, fromLoad = fromLogLoad });
                                            }
                                            break;
                                        case "Materials":
                                            {
                                                string edname = JsonParsing.getString(data, "Material");
                                                int amount = JsonParsing.getInt(data, "Quantity");
                                                int total = JsonParsing.getInt(data, "TotalQuantity");
                                                MaterialAmount material = new MaterialAmount(Material.FromEDName(edname), amount);
                                                events.Add(new EngineerContributedEvent(timestamp, engineer, null, material, contributionType, amount, total) { raw = line, fromLoad = fromLogLoad });
                                            }
                                            break;
                                        case "Credits":
                                        case "Bond":
                                        case "Bounty":
                                            { } // We don't currently handle credit changes from these types.
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

                                    string ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip().EDName;
                                    Compartment compartment = parseShipCompartment(ship, JsonParsing.getString(data, "Slot")); //
                                    compartment.module = Module.FromEDName(JsonParsing.getString(data, "Module"));
                                    List<CommodityAmount> commodities = new List<CommodityAmount>();
                                    List<MaterialAmount> materials = new List<MaterialAmount>();
                                    if (data.TryGetValue("Ingredients", out val))
                                    {
                                        // 2.2 style
                                        if (val is Dictionary<string, object> usedData)
                                        {
                                            foreach (KeyValuePair<string, object> used in usedData)
                                            {
                                                // Used could be a material or a commodity
                                                CommodityDefinition commodity = CommodityDefinition.FromEDName(used.Key);
                                                if (commodity.category != null)
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

                                    GameMode mode = GameMode.FromEDName(JsonParsing.getString(data, "GameMode"));
                                    string group = JsonParsing.getString(data, "Group");
                                    long credits = (long)JsonParsing.getOptionalLong(data, "Credits");
                                    long loan = (long)JsonParsing.getOptionalLong(data, "Loan");
                                    decimal? fuel = JsonParsing.getOptionalDecimal(data, "FuelLevel");
                                    decimal? fuelCapacity = JsonParsing.getOptionalDecimal(data, "FuelCapacity");

                                    events.Add(new CommanderContinuedEvent(timestamp, commander, frontierID, horizons, odyssey, shipId, shipEDModel, shipName, shipIdent, startedLanded, startDead, mode, group, credits, loan, fuel, fuelCapacity) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewHire":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    long crewid = JsonParsing.getLong(data, "CrewID");
                                    string faction = getFactionName(data, "Faction");
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

                                    events.Add(new CrewJoinedEvent(timestamp, captain) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "QuitACrew":
                                {
                                    string captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewLeftEvent(timestamp, captain) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ChangeCrewRole":
                                {
                                    string role = getRole(data, "Role");
                                    events.Add(new CrewRoleChangedEvent(timestamp, role) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberJoins":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewMemberJoinedEvent(timestamp, member) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberQuits":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewMemberLeftEvent(timestamp, member) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewLaunchFighter":
                                {
                                    string name = JsonParsing.getString(data, "Crew");
                                    int fighterId = JsonParsing.getInt(data, "ID");
                                    events.Add(new CrewMemberLaunchedEvent(timestamp, name, fighterId) { raw = line, fromLoad = fromLogLoad });

                                }
                                handled = true;
                                break;
                            case "CrewMemberRoleChange":
                                {
                                    string name = JsonParsing.getString(data, "Crew");
                                    string role = getRole(data, "Role");
                                    events.Add(new CrewMemberRoleChangedEvent(timestamp, name, role) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "KickCrewMember":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewMemberRemovedEvent(timestamp, member) { raw = line, fromLoad = fromLogLoad });
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
                                    decimal combat = (long)val;
                                    data.TryGetValue("Trade", out val);
                                    decimal trade = (long)val;
                                    data.TryGetValue("Explore", out val);
                                    decimal exploration = (long)val;
                                    data.TryGetValue("CQC", out val);
                                    decimal cqc = (long)val;
                                    data.TryGetValue("Empire", out val);
                                    decimal empire = (long)val;
                                    data.TryGetValue("Federation", out val);
                                    decimal federation = (long)val;
                                    data.TryGetValue("Soldier", out val);
                                    decimal soldier = (long)val;
                                    data.TryGetValue("Exobiologist", out val);
                                    decimal exobiologist = (long)val;

                                    events.Add(new CommanderProgressEvent(timestamp, combat, trade, exploration, cqc, empire, federation, soldier, exobiologist) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Rank":
                                {
                                    data.TryGetValue("Combat", out object val);
                                    CombatRating combat = CombatRating.FromRank((int)((long)val));
                                    data.TryGetValue("Trade", out val);
                                    TradeRating trade = TradeRating.FromRank((int)((long)val));
                                    data.TryGetValue("Explore", out val);
                                    ExplorationRating exploration = ExplorationRating.FromRank((int)((long)val));
                                    data.TryGetValue("CQC", out val);
                                    CQCRating cqc = CQCRating.FromRank((int)((long)val));
                                    data.TryGetValue("Empire", out val);
                                    EmpireRating empire = EmpireRating.FromRank((int)((long)val));
                                    data.TryGetValue("Federation", out val);
                                    FederationRating federation = FederationRating.FromRank((int)((long)val));
                                    data.TryGetValue("Soldier", out val);
                                    MercenaryRating mercenary = MercenaryRating.FromRank((int)((long)val));
                                    data.TryGetValue("Exobiologist", out val);
                                    ExobiologistRating exobiologist = ExobiologistRating.FromRank((int)((long)val));

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
                                    string faction = getFactionName(data, "Faction");
                                    data.TryGetValue("ShipID", out val);
                                    int shipId = (int)(long)val;

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
                                    string faction = getFactionName(data, "Faction");
                                    data.TryGetValue("ShipID", out val);
                                    int shipId = (int)(long)val;

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
                                    Ship currentShip = EDDI.Instance.CurrentShip;
                                    bool full = currentShip?.fueltanktotalcapacity == null
                                        ? false
                                        : total == (currentShip?.fueltanktotalcapacity ?? 0M);

                                    events.Add(new ShipRefuelledEvent(timestamp, "Scoop", null, amount, total, full) { raw = line, fromLoad = fromLogLoad });
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
                                            string factionName = getFactionName(rewardData, "Faction");
                                            rewardData.TryGetValue("Amount", out val);
                                            long factionReward = (long)val;

                                            rewards.Add(new Reward(factionName, factionReward));
                                        }
                                    }
                                    else
                                    {
                                        string factionName = getFactionName(data, "Faction");
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

                                    events.Add(new MissionAcceptedEvent(timestamp, cgid, "MISSION_CommunityGoal", name, null, system, null, null, null, null, null, null, null, null, null, null, true, null, null, null, null, false) { raw = line, fromLoad = fromLogLoad });
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

                                    events.Add(new MissionCompletedEvent(timestamp, cgid, "MISSION_CommunityGoal", name, null, null, null, true, reward, null, null, null, null, 0) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CargoDepot":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    long missionid = (long)val;
                                    string updatetype = JsonParsing.getString(data, "UpdateType");

                                    // Not available in 'WingUpdate'
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "CargoType"));
                                    data.TryGetValue("Count", out val);
                                    int? amount = (int?)(long?)val;

                                    long startmarketid = JsonParsing.getLong(data, "StartMarketID");
                                    long endmarketid = JsonParsing.getLong(data, "EndMarketID");
                                    int collected = JsonParsing.getInt(data, "ItemsCollected");
                                    int delivered = JsonParsing.getInt(data, "ItemsDelivered");
                                    int totaltodeliver = JsonParsing.getInt(data, "TotalItemsToDeliver");

                                    events.Add(new CargoDepotEvent(timestamp, missionid, updatetype, commodity, amount, startmarketid, endmarketid, collected, delivered, totaltodeliver) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Missions":
                                {
                                    List<Mission> missions = new List<Mission>();

                                    for (int i = 0; i < 3; i++)
                                    {
                                        MissionStatus status = MissionStatus.FromStatus(i);
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
                                            MissionStatus missionStatus = i == 0 && expires == 0 ? MissionStatus.FromStatus(3) : status;
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
                                    var faction = getFactionName(data, "Faction");
                                    var reward = JsonParsing.getOptionalInt(data, "Reward");
                                    var wing = JsonParsing.getBool(data, "Wing");

                                    // Missions with destinations
                                    var destinationsystem = JsonParsing.getString(data, "DestinationSystem");
                                    var destinationstation = JsonParsing.getString(data, "DestinationStation");

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
                                    var targetfaction = getFactionName(data, "TargetFaction");
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

                                    events.Add(new MissionAcceptedEvent(timestamp, missionid, name, localisedname, faction, destinationsystem, destinationstation, microResource, commodity, amount, passengerswanted, passengertype, passengervips, target, targettype, targetfaction, false, expiry, influence, reputation, reward, wing) { raw = line, fromLoad = fromLogLoad });
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
                                    var faction = getFactionName(data, "Faction");

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
                                                    var rewardMicroResource = MicroResource.FromEDName(m);
                                                    rewardMicroResource.fallbackLocalizedName = fallbackM;
                                                    microResourceRewards.Add(new MicroResourceAmount(rewardMicroResource, null, null, count));
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

                                    events.Add(new MissionCompletedEvent(timestamp, missionid, name, faction, microResource, commodity, amount, false, reward, permitsAwarded, commodityrewards, materialsrewards, microResourceRewards, donation) { raw = line, fromLoad = fromLogLoad });
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
                                    string name = JsonParsing.getString(data, "MissionName");
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
                                            if (module.Mount == Module.ModuleMount.Fixed)
                                            {
                                                mount = "fixed";
                                            }
                                            else if (module.Mount == Module.ModuleMount.Gimballed)
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
                                    data.TryGetValue("Modules", out object val);
                                    List<object> slotsJson = (List<object>)val;

                                    var ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip();
                                    List<Module> modules = new List<Module>();
                                    foreach (string slot in slotsJson)
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
                                    bool update = false;
                                    List<CargoInfo> inventory = new List<CargoInfo>();

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
                                            CargoInfo info = new CargoInfo(name, missionid, count, stolen);
                                            inventory.Add(info);
                                        }
                                    }
                                    else
                                    {
                                        inventory = CargoInfoReader.FromFile().Inventory;
                                        update = true;
                                    }

                                    // Protect against out of date Cargo.json files during 'LogLoad'
                                    if (cargocarried == inventory.Sum(i => i.count))
                                    {
                                        events.Add(new CargoEvent(timestamp, update, vessel, inventory, cargocarried) { raw = line, fromLoad = fromLogLoad });

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
                                    events.Add(new ShipShutdownEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Fileheader":
                                {
                                    string filename = journalFileName;
                                    string version = JsonParsing.getString(data, "gameversion");
                                    string build = JsonParsing.getString(data, "build").Replace(" ", "");
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
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
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
                            case "SAASignalsFound":
                                {
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
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
                                            source.fallbackLocalizedName = JsonParsing.getString(signal, "Type_Localised") ?? signalSource;
                                            int amount = JsonParsing.getInt(signal, "Count");
                                            surfaceSignals.Add(new SignalAmount(source, amount));
                                        }
                                        surfaceSignals = surfaceSignals.OrderByDescending(s => s.amount).ToList();
                                        events.Add(new SurfaceSignalsEvent(timestamp, systemAddress, bodyName, bodyId, surfaceSignals) { raw = line, fromLoad = fromLogLoad });
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
                                    decimal x = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                    decimal y = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                    decimal z = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    Economy systemEconomy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy"));
                                    Economy systemEconomy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy"));
                                    Faction systemfaction = getFaction(data, "System", systemName);
                                    SecurityLevel systemSecurity = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity"));
                                    systemSecurity.fallbackLocalizedName = JsonParsing.getString(data, "SystemSecurity_Localised");
                                    long? systemPopulation = JsonParsing.getOptionalLong(data, "Population");

                                    // Get destination body data (if any)
                                    string bodyName = JsonParsing.getString(data, "Body");
                                    long? bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    BodyType bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType"));

                                    // Get carrier data
                                    bool docked = JsonParsing.getBool(data, "Docked");
                                    string carrierName = JsonParsing.getString(data, "StationName");
                                    StationModel carrierType = StationModel.FromEDName(JsonParsing.getString(data, "StationType"));
                                    long carrierId = JsonParsing.getLong(data, "MarketID");
                                    Faction stationFaction = getFaction(data, "Station", systemName);

                                    // Get carrier services data
                                    List<StationService> stationServices = new List<StationService>();
                                    data.TryGetValue("StationServices", out object stationserviceVal);
                                    List<string> stationservices = (stationserviceVal as List<object>)?.Cast<string>()?.ToList() ?? new List<string>();
                                    foreach (string service in stationservices)
                                    {
                                        stationServices.Add(StationService.FromEDName(service));
                                    }

                                    // Get carrier economies and their shares
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
                                        factions = getFactions(factionsVal, systemName);
                                    }

                                    // Parse conflicts array data
                                    List<Conflict> conflicts = new List<Conflict>();
                                    data.TryGetValue("Conflicts", out object conflictsVal);
                                    if (conflictsVal != null)
                                    {
                                        conflicts = getConflicts(conflictsVal, factions);
                                    }

                                    // Powerplay data (if pledged)
                                    getPowerplayData(data, out Power powerplayPower, out PowerplayState powerplayState);

                                    events.Add(new CarrierJumpedEvent(timestamp, systemName, systemAddress, x, y, z, bodyName, bodyId, bodyType, systemfaction, factions, conflicts, systemEconomy, systemEconomy2, systemSecurity, systemPopulation, powerplayPower, powerplayState, docked, carrierName, carrierType, carrierId, stationFaction, stationServices, stationEconomies) { raw = line, fromLoad = fromLogLoad });

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
                                            EDDI.Instance.enqueueEvent(new CarrierCooldownEvent(timestamp.AddMilliseconds(timeMs), systemName, systemAddress, bodyName, bodyId, bodyType, carrierName, carrierType, carrierId) { fromLoad = fromLogLoad });
                                        }).ConfigureAwait(false);
                                    }
                                }
                                handled = true;
                                break;
                            case "CarrierJumpRequest":
                                {
                                    long carrierId = JsonParsing.getLong(data, "CarrierID");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    string systemName = JsonParsing.getString(data, "SystemName");
                                    string bodyName = JsonParsing.getString(data, "Body");
                                    long bodyId = JsonParsing.getLong(data, "BodyID");

                                    // There is a bug in the journal output where "Body" can be missing but "BodyID" can be present. Try to Work around that here.
                                    if (string.IsNullOrEmpty(bodyName) && !string.IsNullOrEmpty(systemName))
                                    {
                                        StarSystem starSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systemName);
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

                                        // Jumps seem to be scheduled for 10 seconds after the minute, between 15:10 and 16:10 after the request
                                        int varSeconds = (60 + 10) - timestamp.Second;
                                        var tasks = new List<Task>();

                                        tasks.Add(Task.Run(async () =>
                                        {
                                            int timeMs = (Constants.carrierPreJumpSeconds + varSeconds - Constants.carrierLandingPadLockdownSeconds) * 1000;
                                            await Task.Delay(timeMs, carrierJumpCancellationTS.Token);
                                            EDDI.Instance.enqueueEvent(new CarrierPadsLockedEvent(timestamp.AddMilliseconds(timeMs), carrierId) { fromLoad = fromLogLoad });
                                        }, carrierJumpCancellationTS.Token));

                                        tasks.Add(Task.Run(async () =>
                                        {
                                            int timeMs = (Constants.carrierPreJumpSeconds + varSeconds) * 1000;
                                            await Task.Delay(timeMs, carrierJumpCancellationTS.Token);
                                            string originStarSystem = EDDI.Instance.CurrentStarSystem?.systemname;
                                            long? originSystemAddress = EDDI.Instance.CurrentStarSystem?.systemAddress;
                                            EDDI.Instance.enqueueEvent(new CarrierJumpEngagedEvent(timestamp.AddMilliseconds(timeMs), systemName, systemAddress, originStarSystem, originSystemAddress, bodyName, bodyId, carrierId) { fromLoad = fromLogLoad });
                                        }, carrierJumpCancellationTS.Token));

                                        tasks.Add(Task.Run(async () =>
                                        {
                                            // This event will be canceled and replaced by an updated `CarrierCooldownEvent` if the owner is aboard the fleet carrier and sees the `CarrierJumpedEvent`.
                                            int timeMs = (Constants.carrierPreJumpSeconds + varSeconds + Constants.carrierPostJumpSeconds) * 1000; // Cooldown timer starts when the carrier jump is engaged, not when the jump ends
                                            await Task.Delay(timeMs, carrierJumpCancellationTS.Token);
                                            EDDI.Instance.enqueueEvent(new CarrierCooldownEvent(timestamp.AddMilliseconds(timeMs), systemName, systemAddress, bodyName, bodyId, null, null, null, carrierId) { fromLoad = fromLogLoad });
                                        }, carrierJumpCancellationTS.Token));

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
                                            EDDI.Instance.enqueueEvent(new CarrierCooldownEvent(timestamp.AddMilliseconds(timeMs), null, null, null, null, null, null, null, carrierId) { fromLoad = fromLogLoad });
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
                                    AsteroidMaterialContent materialContent = new AsteroidMaterialContent(content);
                                    materialContent.fallbackLocalizedName = JsonParsing.getString(data, "Content_Localised")?.Replace("Material Content: ", "");
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
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
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
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
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
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
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
                                    var info = new MicroResourceInfo().FromFile($"{edType}.json");

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
                                    var edname = JsonParsing.getString(data, "Name");
                                    var fallbackName = JsonParsing.getString(data, "Name_Localised");
                                    var category = JsonParsing.getString(data, "Category");
                                    var fallbackCategoryName = JsonParsing.getString(data, "Category_Localised");
                                    var amount = JsonParsing.getInt(data, "Count");
                                    var price = JsonParsing.getInt(data, "Price");
                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID");

                                    switch (category)
                                    {
                                        case "Item":
                                        case "Component":
                                        case "Data":
                                        case "Consumable":
                                            {
                                                var microResource = MicroResource.FromEDName(edname);
                                                microResource.fallbackLocalizedName = fallbackName;
                                                microResource.Category.fallbackLocalizedName = fallbackCategoryName;
                                                events.Add(new MicroResourcesPurchasedEvent(timestamp, microResource, amount, price, marketId) { raw = line, fromLoad = fromLogLoad });
                                                handled = true;
                                            }
                                            break;
                                        default:
                                            // Unhandled category
                                            break;
                                    }
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
                            case "BuyWeapon":
                            case "CargoTransfer": // Not needed for updating the cargo monitor, the `Cargo` event keeps us up to date.
                            case "CarrierBuy":
                            case "CarrierStats":
                            case "CarrierBankTransfer":
                            case "CarrierCancelDecommission":
                            case "CarrierCrewServices":
                            case "CarrierDecommission":
                            case "CarrierDepositFuel":
                            case "CarrierDockingPermission":
                            case "CarrierFinance":
                            case "CarrierModulePack":
                            case "CarrierNameChange":
                            case "CarrierShipPack":
                            case "CarrierTradeOrder":
                            case "CodexDiscovery":
                            case "CodexEntry":
                            case "CollectItems":
                            case "CreateSuitLoadout":
                            case "CrimeVictim":
                            case "DeleteSuitLoadout":
                            case "DiscoveryScan":
                            case "DropItems":
                            case "EngineerLegacyConvert":
                            case "LoadoutEquipModule":
                            case "LoadoutRemoveModule":
                            case "NavRoute":
                            case "RenameSuitLoadout":
                            case "ReservoirReplenished":
                            case "RestockVehicle":
                            case "Scanned":
                            case "ScanOrganic":
                            case "SellMicroResources":
                            case "SellOrganicData":
                            case "SellSuit":
                            case "SellWeapon":
                            case "SharedBookmarkToSquadron":
                            case "SuitLoadout":
                            case "SwitchSuitLoadout":
                            case "TradeMicroResources": // This is always followed by `ShipLockerMaterials`, which we can use to keep our inventory up to date
                            case "TransferMicroResources":
                            case "UpgradeSuit":
                            case "UpgradeWeapon":
                            case "UseConsumable": // Seems to include only medkits and energy cells (grenades not included) and it's not needed. The `BackpackChange` event keeps us up to date.
                            case "WingAdd":
                            case "WingInvite":
                            case "WingJoin":
                            case "WingLeave":
                            case "WonATrophyForSquadron":
                                // we silently ignore these, but forward them to the responders
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
            catch (Exception ex)
            {
                Logging.Warn("Failed to parse line: " + ex.ToString());
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    { "event", line },
                    { "exception", ex.Message },
                    { "stacktrace", ex.StackTrace }
                };
                Logging.Error("Exception whilst parsing journal line", data);
            }
            return events;
        }

        private static void getPowerplayData(IDictionary<string, object> data, out Power powerplayPower, out PowerplayState powerplayState)
        {
            powerplayPower = new Power();
            data.TryGetValue("Powers", out object powersVal);
            // There can be more than one power listed for a system when the system is being contested
            // If so, the power state will be `Contested` and the power name will be null.
            if (powersVal is List<object> powerNames)
            {
                if (powerNames.Count == 1)
                {
                    powerplayPower = Power.FromEDName((string)powerNames[0]);
                }
            }
            powerplayState = PowerplayState.FromEDName(JsonParsing.getString(data, "PowerplayState")) ?? PowerplayState.None;
        }

        private static Superpower getAllegiance(IDictionary<string, object> data, string key)
        {
            data.TryGetValue(key, out object val);
            // FD sends "" rather than null; fix that here
            if (((string)val) == "") { val = null; }
            return Superpower.FromNameOrEdName((string)val);
        }

        private static List<Conflict> getConflicts(object conflictsVal, List<Faction> factions)
        {
            if (conflictsVal is null || factions is null) { return null; }

            List<Conflict> conflicts = new List<Conflict>();
            var conflictsList = conflictsVal as List<object>;
            foreach (IDictionary<string, object> conflictDetail in conflictsList)
            {
                FactionState conflictType = FactionState.FromEDName(JsonParsing.getString(conflictDetail, "WarType")) ?? FactionState.None;
                string status = JsonParsing.getString(conflictDetail, "Status");

                // Faction 1
                conflictDetail.TryGetValue("Faction1", out object faction1Val);
                Dictionary<string, object> faction1Detail = (Dictionary<string, object>)faction1Val;
                string faction1Name = JsonParsing.getString(faction1Detail, "Name");
                Faction faction1 = factions.Find(f => f.name == faction1Name);
                string faction1Stake = JsonParsing.getString(faction1Detail, "Stake");
                int faction1DaysWon = JsonParsing.getInt(faction1Detail, "WonDays");

                // Faction 2
                conflictDetail.TryGetValue("Faction2", out object faction2Val);
                Dictionary<string, object> faction2Detail = (Dictionary<string, object>)faction2Val;
                string faction2Name = JsonParsing.getString(faction2Detail, "Name");
                Faction faction2 = factions.Find(f => f.name == faction2Name);
                string faction2Stake = JsonParsing.getString(faction2Detail, "Stake");
                int faction2DaysWon = JsonParsing.getInt(faction2Detail, "WonDays");

                conflicts.Add(new Conflict(conflictType, status, faction1, faction1Stake, faction1DaysWon, faction2, faction2Stake, faction2DaysWon));
            }
            return conflicts;
        }

        private static string getFactionName(IDictionary<string, object> data, string key)
        {
            string faction = JsonParsing.getString(data, key);
            // Might be a superpower...
            Superpower superpowerFaction = Superpower.FromNameOrEdName(faction);
            return superpowerFaction?.invariantName ?? faction;
        }

        private static Faction getFaction(IDictionary<string, object> data, string type, string systemName)
        {
            Faction faction = new Faction();

            // Get the faction name and state
            if (data.TryGetValue(type + "Faction", out object factionVal))
            {
                if (factionVal is Dictionary<string, object> factionData) // 3.3.03 or later journal
                {
                    faction.name = JsonParsing.getString(factionData, "Name");

                    // Get the faction information specific to the star system
                    FactionPresence factionPresense = new FactionPresence()
                    {
                        systemName = systemName,
                        FactionState = FactionState.FromEDName(JsonParsing.getString(factionData, "FactionState")) ?? FactionState.None,
                    };
                    faction.presences.Add(factionPresense);
                }
                else // per-3.3.03 journal
                {
                    faction.name = factionVal as string;
                }
            }

            // Get the faction allegiance
            if (data.TryGetValue(type + "Allegiance", out _))
            {
                faction.Allegiance = getAllegiance(data, type + "Allegiance");
            }
            else if (data.TryGetValue("Factions", out object val))
            {
                // Station controlling faction government not discretely available in 'Location' event
                var factionsList = val as List<object>;
                foreach (IDictionary<string, object> factionDetail in factionsList)
                {
                    string fName = JsonParsing.getString(factionDetail, "Name");
                    if (fName == faction.name)
                    {
                        faction.Allegiance = getAllegiance(factionDetail, "Allegiance");
                        break;
                    }
                }
            }

            // Get the controlling faction (system or station) government
            faction.Government = Government.FromEDName(JsonParsing.getString(data, type + "Government")) ?? Government.None;

            return faction;
        }

        private static List<Faction> getFactions(object factionsVal, string systemName)
        {
            List<Faction> factions = new List<Faction>();
            var factionsList = factionsVal as List<object>;
            foreach (IDictionary<string, object> factionDetail in factionsList)
            {
                // Core data
                string fName = JsonParsing.getString(factionDetail, "Name");
                FactionState fState = FactionState.FromEDName(JsonParsing.getString(factionDetail, "FactionState")) ?? FactionState.None;
                Government fGov = Government.FromEDName(JsonParsing.getString(factionDetail, "SystemGovernment")) ?? Government.None;
                decimal influence = JsonParsing.getDecimal(factionDetail, "Influence");
                Superpower fAllegiance = getAllegiance(factionDetail, "Allegiance");
                Happiness happiness = Happiness.FromEDName(JsonParsing.getString(factionDetail, "Happiness") ?? string.Empty);
                decimal myReputation = JsonParsing.getOptionalDecimal(factionDetail, "MyReputation") ?? 0;

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
                    FactionState = fState,
                    influence = influence,
                    Happiness = happiness,
                };

                // Active states
                factionDetail.TryGetValue("ActiveStates", out object activeStatesVal);
                if (activeStatesVal != null)
                {
                    var activeStatesList = (List<object>)activeStatesVal;
                    foreach (IDictionary<string, object> activeState in activeStatesList)
                    {
                        factionPresense.ActiveStates.Add(FactionState.FromEDName(JsonParsing.getString(activeState, "State")) ?? FactionState.None);
                    }
                }

                // Pending states
                factionDetail.TryGetValue("PendingStates", out object pendingStatesVal);
                if (pendingStatesVal != null)
                {
                    var pendingStatesList = (List<object>)pendingStatesVal;
                    foreach (IDictionary<string, object> pendingState in pendingStatesList)
                    {
                        FactionTrendingState pTrendingState = new FactionTrendingState(
                            FactionState.FromEDName(JsonParsing.getString(pendingState, "State")) ?? FactionState.None,
                            JsonParsing.getInt(pendingState, "Trend")
                        );
                        factionPresense.PendingStates.Add(pTrendingState);
                    }
                }

                // Recovering states
                factionDetail.TryGetValue("RecoveringStates", out object recoveringStatesVal);
                if (recoveringStatesVal != null)
                {
                    var recoveringStatesList = (List<object>)recoveringStatesVal;
                    foreach (IDictionary<string, object> recoveringState in recoveringStatesList)
                    {
                        FactionTrendingState rTrendingState = new FactionTrendingState(
                            FactionState.FromEDName(JsonParsing.getString(recoveringState, "State")) ?? FactionState.None,
                            JsonParsing.getInt(recoveringState, "Trend")
                        );
                        factionPresense.RecoveringStates.Add(rTrendingState);
                    }
                }

                // Squadron data
                fFaction.squadronfaction = JsonParsing.getOptionalBool(factionDetail, "SquadronFaction") ?? false;
                factionPresense.squadronhappiestsystem = JsonParsing.getOptionalBool(factionDetail, "HappiestSystem") ?? false;
                factionPresense.squadronhomesystem = JsonParsing.getOptionalBool(factionDetail, "HomeSystem") ?? false;

                if (fFaction != null)
                {
                    fFaction.presences.Add(factionPresense);
                    factions.Add(fFaction);
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
                source.fallbackLocalizedName = JsonParsing.getString(data, "USSType_Localised") ?? signalSource;
            }
            else
            {
                string signalSource = JsonParsing.getString(data, "SignalName");
                source = SignalSource.FromEDName(signalSource) ?? new SignalSource();
                source.fallbackLocalizedName = JsonParsing.getString(data, "SignalName_Localised") ?? signalSource;
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
        {
        }

        public void PostHandle(Event @event)
        {
        }

        public void HandleProfile(JObject profile)
        {
        }

        public IDictionary<string, object> GetVariables()
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

        private static Compartment parseShipCompartment(string ship, string slot)
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
                var slotSize = ShipDefinitions.FromEDModel(ship, false)?.militarysize;
                if (slotSize is null)
                {
                    // We didn't expect to have a military slot on this ship.
                    var data = new Dictionary<string, object>() { { "ShipEDName", ship }, { "Slot", slot }, { "Exception", new ArgumentException() } };
                    Logging.Error($"Unexpected military slot found in ship edName {ship}.", data);
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
