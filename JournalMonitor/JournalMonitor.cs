using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiCargoMonitor;
using EddiCrimeMonitor;
using EddiMissionMonitor;
using EddiShipMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiJournalMonitor
{
    public class JournalMonitor : LogMonitor, EDDIMonitor
    {
        private static Regex JsonRegex = new Regex(@"^{.*}$", RegexOptions.Singleline);
        public JournalMonitor() : base(GetSavedGamesDir(), @"^Journal.*\.[0-9\.]+\.log$", (result, isLogLoadEvent) =>
        ForwardJournalEntry(result, EDDI.Instance.enqueueEvent, isLogLoadEvent))
        { }

        private enum ShipyardType { ShipsHere, ShipsRemote }

        public static void ForwardJournalEntry(string line, Action<Event> callback, bool isLogLoadEvent)
        {
            if (line == null)
            {
                return;
            }

            List<Event> events = ParseJournalEntry(line, isLogLoadEvent);
            foreach (Event @event in events)
            {
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
                    if (data.ContainsKey("timestamp"))
                    {
                        if (data["timestamp"] is DateTime)
                        {
                            timestamp = ((DateTime)data["timestamp"]).ToUniversalTime();
                        }
                        else
                        {
                            timestamp = DateTime.Parse(JsonParsing.getString(data, "timestamp")).ToUniversalTime();
                        }
                    }
                    else
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
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    string stationName = JsonParsing.getString(data, "StationName");
                                    string stationState = JsonParsing.getString(data, "StationState") ?? string.Empty;
                                    StationModel stationModel = StationModel.FromEDName(JsonParsing.getString(data, "StationType") ?? "None");
                                    Faction controllingfaction = getFaction(data, "Station", systemName);
                                    decimal? distancefromstar = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

                                    // Get station services data
                                    data.TryGetValue("StationServices", out object val);
                                    List<string> stationservices = (val as List<object>)?.Cast<string>()?.ToList();
                                    List<StationService> stationServices = new List<StationService>();
                                    foreach (string service in stationservices)
                                    {
                                        stationServices.Add(StationService.FromEDName(service));
                                    }

                                    // Get station economies and their shares
                                    data.TryGetValue("StationEconomies", out object val2);
                                    List<object> economies = val2 as List<object>;
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
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    events.Add(new UndockedEvent(timestamp, stationName, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Touchdown":
                                {
                                    decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                    bool playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;
                                    events.Add(new TouchdownEvent(timestamp, longitude, latitude, playercontrolled) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Liftoff":
                                {
                                    decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                    bool playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;
                                    events.Add(new LiftoffEvent(timestamp, longitude, latitude, playercontrolled) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SupercruiseEntry":
                                {
                                    string system = JsonParsing.getString(data, "StarySystem");
                                    events.Add(new EnteredSupercruiseEvent(timestamp, system) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SupercruiseExit":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    BodyType bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType") ?? "None");
                                    events.Add(new EnteredNormalSpaceEvent(timestamp, system, systemAddress, body, bodyType) { raw = line, fromLoad = fromLogLoad });
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
                                    Economy economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy") ?? "$economy_None");
                                    Economy economy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy") ?? "$economy_None"); ;
                                    SecurityLevel security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity") ?? "None");
                                    long? population = JsonParsing.getOptionalLong(data, "Population");

                                    // Parse factions array data
                                    List<Faction> factions = new List<Faction>();
                                    data.TryGetValue("Factions", out object factionsVal);
                                    if (factionsVal != null)
                                    {
                                        factions = getFactions(factionsVal, systemName);
                                    }

                                    // Calculate remaining distance to route destination (if it exists)
                                    decimal destDistance = 0;
                                    string destination = EDDI.Instance.DestinationStarSystem?.name;
                                    if (!string.IsNullOrEmpty(destination))
                                    {
                                        destDistance = EDDI.Instance.getSystemDistanceFromDestination(systemName);
                                    }

                                    events.Add(new JumpedEvent(timestamp, systemName, systemAddress, x, y, z, starName, distance, fuelUsed, fuelRemaining, boostUsed, controllingfaction, factions, economy, economy2, security, population, destination, destDistance) { raw = line, fromLoad = fromLogLoad });
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

                                    string body = JsonParsing.getString(data, "Body");
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

                                    events.Add(new LocationEvent(timestamp, systemName, x, y, z, systemAddress, body, bodyType, docked, station, stationtype, marketId, systemfaction, stationfaction, economy, economy2, security, population, longitude, latitude, factions) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Bounty":
                                {

                                    string target = JsonParsing.getString(data, "Target");
                                    if (target != null)
                                    {
                                        // Target might be a ship, but if not then the string we provide is repopulated in ship.model so use it regardless
                                        Ship ship = ShipDefinitions.FromEDModel(target);
                                        target = ship.model;
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
                                    object val;
                                    if (data.ContainsKey("Combat"))
                                    {
                                        data.TryGetValue("Combat", out val);
                                        CombatRating rating = CombatRating.FromRank(Convert.ToInt32(val));
                                        events.Add(new CombatPromotionEvent(timestamp, rating) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (data.ContainsKey("Trade"))
                                    {
                                        data.TryGetValue("Trade", out val);
                                        TradeRating rating = TradeRating.FromRank(Convert.ToInt32(val));
                                        events.Add(new TradePromotionEvent(timestamp, rating) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (data.ContainsKey("Explore"))
                                    {
                                        data.TryGetValue("Explore", out val);
                                        ExplorationRating rating = ExplorationRating.FromRank(Convert.ToInt32(val));
                                        events.Add(new ExplorationPromotionEvent(timestamp, rating) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (data.ContainsKey("Federation"))
                                    {
                                        data.TryGetValue("Federation", out val);
                                        FederationRating rating = FederationRating.FromRank(Convert.ToInt32(val));
                                        events.Add(new FederationPromotionEvent(timestamp, rating) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (data.ContainsKey("Empire"))
                                    {
                                        data.TryGetValue("Empire", out val);
                                        EmpireRating rating = EmpireRating.FromRank(Convert.ToInt32(val));
                                        events.Add(new EmpirePromotionEvent(timestamp, rating) { raw = line, fromLoad = fromLogLoad });
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
                                    handled = true;
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
                                            moduleData.TryGetValue("Engineering", out val);
                                            bool modified = val != null ? true : false;
                                            Dictionary<string, object> engineeringData = (Dictionary<string, object>)val;
                                            string blueprint = modified ? JsonParsing.getString(engineeringData, "BlueprintName") : null;
                                            Modifications modification = Modifications.FromEDName(blueprint) ?? Modifications.None;
                                            int level = modified ? JsonParsing.getInt(engineeringData, "Level") : 0;
                                            decimal quality = modified ? JsonParsing.getDecimal(engineeringData, "Quality") : 0;

                                            if (slot.Contains("Hardpoint"))
                                            {
                                                // This is a hardpoint
                                                Hardpoint hardpoint = new Hardpoint() { name = slot };
                                                if (hardpoint.name.StartsWith("Tiny"))
                                                {
                                                    hardpoint.size = 0;
                                                }
                                                else if (hardpoint.name.StartsWith("Small"))
                                                {
                                                    hardpoint.size = 1;
                                                }
                                                else if (hardpoint.name.StartsWith("Medium"))
                                                {
                                                    hardpoint.size = 2;
                                                }
                                                else if (hardpoint.name.StartsWith("Large"))
                                                {
                                                    hardpoint.size = 3;
                                                }
                                                else if (hardpoint.name.StartsWith("Huge"))
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
                                                    module.engineermodification = modification;
                                                    module.engineerlevel = level;
                                                    module.engineerquality = quality;
                                                    hardpoint.module = module;
                                                    hardpoints.Add(hardpoint);
                                                }
                                            }
                                            else if (slot == "PaintJob")
                                            {
                                                // This is a paintjob
                                                paintjob = item;
                                            }
                                            else if (slot == "PlanetaryApproachSuite")
                                            {
                                                // Ignore planetary approach suite for now
                                            }
                                            else if (slot.StartsWith("Bobble"))
                                            {
                                                // Ignore bobbles
                                            }
                                            else if (slot.StartsWith("Decal"))
                                            {
                                                // Ignore decals
                                            }
                                            else if (slot.Contains("String_Lights"))
                                            {
                                                // Ignore string lights
                                            }
                                            else if (slot == "WeaponColour")
                                            {
                                                // Ignore weapon colour
                                            }
                                            else if (slot == "EngineColour")
                                            {
                                                // Ignore engine colour
                                            }
                                            else if (slot.StartsWith("ShipKit"))
                                            {
                                                // Ignore ship kits
                                            }
                                            else if (slot.StartsWith("ShipName") || slot.StartsWith("ShipID"))
                                            {
                                                // Ignore nameplates
                                            }
                                            else if (slot == "VesselVoice")
                                            {
                                                // Ignore the chosen voice
                                            }
                                            else if (slot == "DataLinkScanner")
                                            {
                                                // Ignore the data link scanner
                                            }
                                            else if (slot == "CodexScanner")
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
                                                    module.engineermodification = modification;
                                                    module.engineerlevel = level;
                                                    module.engineerquality = quality;
                                                    compartment.module = module;
                                                    compartments.Add(compartment);
                                                }
                                            }
                                        }
                                    }
                                    events.Add(new ShipLoadoutEvent(timestamp, ship, shipId, shipName, shipIdent, hullValue, modulesValue, hullHealth, rebuy, hot, compartments, hardpoints, paintjob) { raw = line, fromLoad = fromLogLoad });
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
                                    events.Add(new NearSurfaceEvent(timestamp, true, system, systemAddress, body) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LeaveBody":
                                {
                                    string system = JsonParsing.getString(data, "StarSystem");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    string body = JsonParsing.getString(data, "Body");
                                    events.Add(new NearSurfaceEvent(timestamp, false, system, systemAddress, body) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ApproachSettlement":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    long marketId = JsonParsing.getLong(data, "MarketID");
                                    // Replace with localised name if available
                                    if (data.TryGetValue("Name_Localised", out object val))
                                    {
                                        name = (string)val;
                                    }
                                    decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    events.Add(new SettlementApproachedEvent(timestamp, name, marketId, latitude, longitude) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Scan":
                                {
                                    string name = JsonParsing.getString(data, "BodyName");
                                    string systemName = EDDI.Instance?.CurrentStarSystem?.name;
                                    string scantype = JsonParsing.getString(data, "ScanType");
                                    decimal distancefromarrival = JsonParsing.getDecimal(data, "DistanceFromArrivalLS");

                                    // Belt
                                    if (name.Contains("Belt Cluster"))
                                    {
                                        events.Add(new BeltScannedEvent(timestamp, scantype, name, distancefromarrival) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                        break;
                                    }

                                    // Common items
                                    // Need to convert radius from meters (per journal) to kilometers
                                    decimal radiusKm = JsonParsing.getDecimal(data, "Radius") / 1000;
                                    // Need to convert orbital period from seconds (per journal) to days
                                    decimal? orbitalPeriodDays = ConstantConverters.seconds2days(JsonParsing.getOptionalDecimal(data, "OrbitalPeriod"));
                                    // Need to convert rotation period from seconds (per journal) to days
                                    decimal rotationPeriodDays = (decimal)ConstantConverters.seconds2days(JsonParsing.getDecimal(data, "RotationPeriod"));
                                    // Need to convert meters to light seconds
                                    decimal? semimajoraxisLs = ConstantConverters.meters2ls(JsonParsing.getOptionalDecimal(data, "SemiMajorAxis"));
                                    decimal? eccentricity = JsonParsing.getOptionalDecimal(data, "Eccentricity");
                                    decimal? orbitalinclinationDegrees = JsonParsing.getOptionalDecimal(data, "OrbitalInclination");
                                    decimal? periapsisDegrees = JsonParsing.getOptionalDecimal(data, "Periapsis");
                                    decimal? axialTiltDegrees = JsonParsing.getOptionalDecimal(data, "AxialTilt");

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
                                        string starType = JsonParsing.getString(data, "StarType");
                                        decimal stellarMass = JsonParsing.getDecimal(data, "StellarMass");
                                        decimal absoluteMagnitude = JsonParsing.getDecimal(data, "AbsoluteMagnitude");
                                        string luminosityClass = JsonParsing.getString(data, "Luminosity");
                                        data.TryGetValue("Age_MY", out val);
                                        long ageMegaYears = (long)val;
                                        decimal temperatureKelvin = JsonParsing.getDecimal(data, "SurfaceTemperature");
                                        bool mainstar = distancefromarrival == 0 ? true : false;

                                        events.Add(new StarScannedEvent(timestamp, scantype, name, starType, stellarMass, radiusKm, absoluteMagnitude, luminosityClass, ageMegaYears, temperatureKelvin, distancefromarrival, orbitalPeriodDays, rotationPeriodDays, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, rings, mainstar) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else
                                    {
                                        // Body
                                        bool? tidallyLocked = JsonParsing.getOptionalBool(data, "TidalLock") ?? false;

                                        PlanetClass planetClass = PlanetClass.FromEDName(JsonParsing.getString(data, "PlanetClass")) ?? PlanetClass.None;
                                        decimal? earthMass = JsonParsing.getOptionalDecimal(data, "MassEM");

                                        // MKW: Gravity in the Journal is in m/s; must convert it to G
                                        decimal gravity = ConstantConverters.ms2g(JsonParsing.getDecimal(data, "SurfaceGravity"));

                                        decimal? temperatureKelvin = JsonParsing.getOptionalDecimal(data, "SurfaceTemperature");

                                        decimal? pressureAtm = ConstantConverters.pascals2atm(JsonParsing.getOptionalDecimal(data, "SurfacePressure"));

                                        bool? landable = JsonParsing.getOptionalBool(data, "Landable") ?? false;

                                        string reserves = JsonParsing.getString(data, "ReserveLevel");

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

                                        events.Add(new BodyScannedEvent(timestamp, scantype, name, systemName, planetClass, earthMass, radiusKm, gravity, temperatureKelvin, pressureAtm, tidallyLocked, landable, atmosphereClass, atmosphereCompositions, solidCompositions, volcanism, distancefromarrival, (decimal)orbitalPeriodDays, rotationPeriodDays, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, rings, reserves, materials, terraformState, axialTiltDegrees) { raw = line, fromLoad = fromLogLoad });
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
                                    events.Add(new ShipyardEvent(timestamp, marketId, station, system) { raw = line, fromLoad = fromLogLoad });
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
                                    string system = JsonParsing.getString(data, "System");
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
                                                        ship.station = systemData?.stations?.FirstOrDefault(s => s.marketId == ship.marketid).name;
                                                    }
                                                    else
                                                    {
                                                        ship.station = station;
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
                                            bool modified = val != null ? true : false;
                                            module.modified = modified;
                                            module.engineermodification = Modifications.FromEDName((string)val) ?? Modifications.None;
                                            module.engineerlevel = modified ? JsonParsing.getInt(item, "Level") : 0;
                                            module.engineerquality = modified? JsonParsing.getDecimal(item, "Quality") : 0;

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
                                        List<StarSystem> systemsData = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystems(systemNames);
                                        List<StoredModule> storedModulesHolder = new List<StoredModule>();
                                        foreach (StoredModule storedModule in storedModules)
                                        {
                                            if (!storedModule.intransit)
                                            {
                                                StarSystem systemData = systemsData.FirstOrDefault(s => s.name == storedModule.system);
                                                Station stationData = systemData?.stations?.FirstOrDefault(s => s.marketId == storedModule.marketid);
                                                storedModule.station = stationData?.name;
                                            }
                                            storedModulesHolder.Add(storedModule);
                                        }
                                        storedModules = storedModulesHolder;
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
                                    string ship = JsonParsing.getString(data, "ShipType");

                                    string system = JsonParsing.getString(data, "System");
                                    decimal distance = JsonParsing.getDecimal(data, "Distance");
                                    long? price = JsonParsing.getOptionalLong(data, "TransferPrice");
                                    long? time = JsonParsing.getOptionalLong(data, "TransferTime");

                                    events.Add(new ShipTransferInitiatedEvent(timestamp, ship, shipId, system, distance, price, time, fromMarketId, toMarketId) { raw = line, fromLoad = fromLogLoad });

                                    // Generate secondary event when the ship is arriving
                                    if (time.HasValue)
                                    {
                                        ShipArrived();
                                        async void ShipArrived()
                                        {
                                            // Include the station and system at which the transfer will arrive
                                            string arrivalStation = EDDI.Instance.CurrentStation?.name ?? string.Empty;
                                            string arrivalSystem = EDDI.Instance.CurrentStarSystem?.name ?? string.Empty;
                                            await Task.Delay((int)time * 1000);
                                            EDDI.Instance.enqueueEvent(new ShipArrivedEvent(DateTime.UtcNow, ship, shipId, arrivalSystem, distance, price, time, arrivalStation, fromMarketId, toMarketId));
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
                                            string arrivalSystem = EDDI.Instance.CurrentStarSystem?.name ?? string.Empty;
                                            await Task.Delay((int)transferTime * 1000);
                                            EDDI.Instance.enqueueEvent(new ModuleArrivedEvent(DateTime.UtcNow, ship, shipId, storageSlot, serverId, module, transferCost, transferTime, arrivalSystem, arrivalStation));
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
                                            module.hot = JsonParsing.getBool(data, "Hot");
                                            string engineerModifications = JsonParsing.getString(data, "EngineerModifications");
                                            module.modified = engineerModifications != null;
                                            module.engineermodification = Modifications.FromEDName(engineerModifications) ?? Modifications.None;
                                            module.engineerlevel = JsonParsing.getOptionalInt(data, "Level") ?? 0;
                                            module.engineerquality = JsonParsing.getOptionalDecimal(data, "Quality") ?? 0;
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
                                    module.engineermodification = Modifications.FromEDName(engineerModifications) ?? Modifications.None;
                                    module.engineerlevel = JsonParsing.getOptionalInt(data, "Level") ?? 0;
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
                                    module.engineermodification = Modifications.FromEDName(engineerModifications) ?? Modifications.None;
                                    module.engineerlevel = JsonParsing.getOptionalInt(data, "Level") ?? 0;
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
                                    events.Add(new OutfittingEvent(timestamp, marketId, station, system) { raw = line, fromLoad = fromLogLoad });
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
                            case "LaunchSRV":
                                {
                                    string loadout = JsonParsing.getString(data, "Loadout");
                                    bool playercontrolled = JsonParsing.getBool(data, "PlayerControlled");

                                    events.Add(new SRVLaunchedEvent(timestamp, loadout, playercontrolled) { raw = line, fromLoad = fromLogLoad });
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
                            case "DockSRV":
                                events.Add(new SRVDockedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                handled = true;
                                break;
                            case "LaunchFighter":
                                {
                                    string loadout = JsonParsing.getString(data, "Loadout");
                                    bool playerControlled = JsonParsing.getBool(data, "PlayerControlled");
                                    events.Add(new FighterLaunchedEvent(timestamp, loadout, playerControlled) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockFighter":
                                events.Add(new FighterDockedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                handled = true;
                                break;
                            case "SRVDestroyed":
                                {
                                    string vehicle = "srv";
                                    events.Add(new VehicleDestroyedEvent(timestamp, vehicle) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "FighterDestroyed":
                                {
                                    string vehicle = "fighter";
                                    events.Add(new VehicleDestroyedEvent(timestamp, vehicle) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "FighterRebuilt":
                                {
                                    string loadout = JsonParsing.getString(data, "Loadout");
                                    events.Add(new FighterRebuiltEvent(timestamp, loadout) { raw = line, fromLoad = fromLogLoad });
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
                                    CombatRating rating = (val == null ? null : CombatRating.FromRank((int)val));
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
                                    string source = "";

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
                                        source = channel == "squadron" ? "Squadron mate" : channel == "wing" ? "Wing mate" : channel == null ? "Crew mate" : "Commander";
                                        channel = channel ?? "multicrew";
                                        events.Add(new MessageReceivedEvent(timestamp, from, source, true, channel, message) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // This is NPC speech.  What's the source?
                                        if (from.Contains("npc_name_decorate"))
                                        {
                                            source = npcSpeechBy(from, message);
                                            from = from.Replace("$npc_name_decorate:#name=", "").Replace(";", "");
                                        }
                                        else if (from.Contains("ShipName_"))
                                        {
                                            source = npcSpeechBy(from, message);
                                            from = JsonParsing.getString(data, "From_Localised");
                                        }
                                        else if ((message.StartsWith("$STATION_")) || message.Contains("$Docking"))
                                        {
                                            source = "Station";
                                        }
                                        else
                                        {
                                            source = "NPC";
                                        }
                                        events.Add(new MessageReceivedEvent(timestamp, from, source, false, channel, JsonParsing.getString(data, "Message_Localised")) { raw = line, fromLoad = fromLogLoad });

                                        // See if we also want to spawn a specific event as well?
                                        if (message == "$STATION_NoFireZone_entered;")
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
                                            string by = npcSpeechBy(from, message);

                                            events.Add(new NPCInterdictionCommencedEvent(timestamp, by) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_Attack") || message.Contains("_OnAttackStart") || message.Contains("AttackRun") || message.Contains("OnDeclarePiracyAttack"))
                                        {
                                            // Find out who is doing the attacking
                                            string by = npcSpeechBy(from, message);
                                            events.Add(new NPCAttackCommencedEvent(timestamp, by) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_OnStartScanCargo"))
                                        {
                                            // Find out who is doing the scanning
                                            string by = npcSpeechBy(from, message);
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
                                    long marketId = JsonParsing.getLong(data, "MarketID");
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
                                {
                                    bool shieldsUp = JsonParsing.getBool(data, "ShieldsUp");
                                    if (shieldsUp == true)
                                    {
                                        events.Add(new ShieldsUpEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        events.Add(new ShieldsDownEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "ShipTargeted":
                                {
                                    bool targetlocked = JsonParsing.getBool(data, "TargetLocked");
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
                                            Ship shipDef = ShipDefinitions.FromEDModel(ship);
                                            if (shipDef != null)
                                            {
                                                ship = shipDef.model;
                                            }
                                        }
                                    }
                                    string name = JsonParsing.getString(data, "PilotName_Localised");
                                    CombatRating rank = new CombatRating();
                                    string pilotRank = JsonParsing.getString(data, "PilotRank");
                                    if (pilotRank != null)
                                    {
                                        rank = CombatRating.FromEDName(pilotRank);
                                    }
                                    decimal? shieldHealth = JsonParsing.getOptionalDecimal(data, "ShieldHealth");
                                    decimal? hullHealth = JsonParsing.getOptionalDecimal(data, "HullHealth");
                                    string faction = JsonParsing.getString(data, "Faction");
                                    LegalStatus legalStatus = new LegalStatus();
                                    string pilotLegalStatus = JsonParsing.getString(data, "LegalStatus");
                                    if (pilotLegalStatus != null)
                                    {
                                        legalStatus = LegalStatus.FromEDName(pilotLegalStatus);
                                    }
                                    int? bounty = JsonParsing.getOptionalInt(data, "Bounty");
                                    string subSystem = JsonParsing.getString(data, "Subsystem_Localised");
                                    decimal? subSystemHealth = JsonParsing.getOptionalDecimal(data, "SubsystemHealth");
                                    events.Add(new ShipTargetedEvent(timestamp, targetlocked, ship, scanstage, name, rank, faction, legalStatus, bounty, shieldHealth, hullHealth, subSystem, subSystemHealth) { raw = line, fromLoad = fromLogLoad });
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

                                    List<string> names = new List<string>();
                                    List<string> ships = new List<string>();
                                    List<CombatRating> ratings = new List<CombatRating>();

                                    if (data.ContainsKey("KillerName"))
                                    {
                                        // Single killer
                                        names.Add(JsonParsing.getString(data, "KillerName"));
                                        ships.Add(JsonParsing.getString(data, "KillerShip"));
                                        ratings.Add(CombatRating.FromEDName(JsonParsing.getString(data, "KillerRank")));
                                    }
                                    if (data.ContainsKey("killers"))
                                    {
                                        // Multiple killers
                                        data.TryGetValue("Killers", out object val);
                                        List<object> killers = (List<object>)val;
                                        foreach (IDictionary<string, object> killer in killers)
                                        {
                                            names.Add(JsonParsing.getString(killer, "Name"));
                                            ships.Add(JsonParsing.getString(killer, "Ship"));
                                            ratings.Add(CombatRating.FromEDName(JsonParsing.getString(killer, "Rank")));
                                        }
                                    }
                                    events.Add(new DiedEvent(timestamp, names, ships, ratings) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
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
                                    handled = true;
                                }
                                break;
                            case "FSSDiscoveryScan":
                                {
                                    decimal progress = JsonParsing.getDecimal(data, "Progress"); // value from 0-1
                                    int bodyCount = JsonParsing.getInt(data, "BodyCount"); // number of stellar bodies in system
                                    int nonBodyCount = JsonParsing.getInt(data, "NonBodyCount"); // Number of non-body signals found
                                    events.Add(new DiscoveryScanEvent(timestamp, progress, bodyCount, nonBodyCount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "FSSSignalDiscovered":
                                {
                                    SignalSource source = GetSignalSource(data);
                                    string spawningFaction = getFactionName(data, "SpawningFaction") ?? Superpower.None.localizedName; // the minor faction, if relevant
                                    decimal? secondsRemaining = JsonParsing.getOptionalDecimal(data, "TimeRemaining"); // remaining lifetime in seconds, if relevant

                                    string spawningstate = JsonParsing.getString(data, "SpawningState");
                                    string normalizedSpawningState = spawningstate?.Replace("$FactionState_", "")?.Replace("_desc;", "");
                                    FactionState spawningState = FactionState.FromEDName(normalizedSpawningState) ?? new FactionState();
                                    spawningState.fallbackLocalizedName = JsonParsing.getString(data, "SpawningState_Localised");

                                    int? threatLevel = JsonParsing.getOptionalInt(data, "ThreatLevel") ?? 0;
                                    bool? isStation = JsonParsing.getOptionalBool(data, "IsStation") ?? false;

                                    events.Add(new SignalDetectedEvent(timestamp, source, spawningState, spawningFaction, secondsRemaining, threatLevel, isStation) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "BuyExplorationData":
                                {
                                    string system = JsonParsing.getString(data, "System");
                                    long price = JsonParsing.getLong(data, "Cost");
                                    events.Add(new ExplorationDataPurchasedEvent(timestamp, system, price) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "SAAScanComplete":
                                {
                                    string body = JsonParsing.getString(data, "BodyName");
                                    int probesUsed = JsonParsing.getInt(data, "ProbesUsed");
                                    int efficiencyTarget = JsonParsing.getInt(data, "EfficiencyTarget");
                                    events.Add(new BodyMappedEvent(timestamp, body, probesUsed, efficiencyTarget) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    events.Add(new ExplorationDataSoldEvent(timestamp, systems, firsts, reward, bonus) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "USSDrop":
                                {
                                    SignalSource source = GetSignalSource(data);
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
                                    events.Add(new MarketEvent(timestamp, marketId, station, system) { raw = line, fromLoad = fromLogLoad });
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
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
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

                                    string ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip().model;
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
                                    handled = true;
                                    break;
                                }
                            case "EngineerProgress":
                                {
                                    data.TryGetValue("Engineers", out object val);
                                    if (val != null)
                                    {
                                        // This is a startup entry. 
                                        // Update engineer progress / status data but do not generate events.
                                        List<object> engineers = (List<object>)val;
                                        foreach (IDictionary<string, object> engineerData in engineers)
                                        {
                                            Engineer engineer = parseEngineer(engineerData);
                                            Engineer.AddOrUpdate(engineer);
                                        }
                                    }
                                    else
                                    {
                                        // This is a progress entry.
                                        Engineer engineer = parseEngineer(data);
                                        Engineer lastEngineer = Engineer.FromNameOrId(engineer.name, engineer.id);

                                        if (engineer.rank != null && engineer.rank != lastEngineer?.rank)
                                        {
                                            events.Add(new EngineerProgressedEvent(timestamp, engineer, "Rank") { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (engineer.stage != null && engineer.stage != lastEngineer?.stage)
                                        {
                                            events.Add(new EngineerProgressedEvent(timestamp, engineer, "Stage") { raw = line, fromLoad = fromLogLoad });
                                        }
                                        Engineer.AddOrUpdate(engineer);
                                    }
                                    handled = true;
                                    break;
                                }
                            case "LoadGame":
                                {
                                    string commander = JsonParsing.getString(data, "Commander");
                                    bool horizons = JsonParsing.getBool(data, "Horizons");

                                    data.TryGetValue("ShipID", out object val);
                                    int? shipId = (int?)(long?)val;

                                    if (shipId == null)
                                    {
                                        // This happens if we are in CQC.  Flag it back to EDDI so that it ignores everything that happens until
                                        // we're out of CQC again
                                        events.Add(new EnteredCQCEvent(timestamp, commander) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                        break;
                                    }

                                    string ship = JsonParsing.getString(data, "Ship");
                                    string shipName = JsonParsing.getString(data, "ShipName");
                                    string shipIdent = JsonParsing.getString(data, "ShipIdent");

                                    GameMode mode = GameMode.FromEDName(JsonParsing.getString(data, "GameMode"));
                                    string group = JsonParsing.getString(data, "Group");
                                    data.TryGetValue("Credits", out val);
                                    decimal credits = (long)val;
                                    data.TryGetValue("Loan", out val);
                                    decimal loan = (long)val;
                                    decimal? fuel = JsonParsing.getOptionalDecimal(data, "FuelLevel");
                                    decimal? fuelCapacity = JsonParsing.getOptionalDecimal(data, "FuelCapacity");

                                    events.Add(new CommanderContinuedEvent(timestamp, commander, horizons, (int)shipId, ship, shipName, shipIdent, mode, group, credits, loan, fuel, fuelCapacity) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CrewHire":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    long crewid = JsonParsing.getLong(data, "CrewID");
                                    string faction = getFactionName(data, "Faction");
                                    long price = JsonParsing.getLong(data, "Cost");
                                    CombatRating rating = CombatRating.FromRank(JsonParsing.getInt(data, "CombatRank"));
                                    events.Add(new CrewHiredEvent(timestamp, name, crewid, faction, price, rating) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CrewFire":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    long crewid = JsonParsing.getLong(data, "CrewID");
                                    events.Add(new CrewFiredEvent(timestamp, name, crewid) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CrewAssign":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    long crewid = JsonParsing.getLong(data, "CrewID");
                                    string role = getRole(data, "Role");
                                    events.Add(new CrewAssignedEvent(timestamp, name, crewid, role) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "NpcCrewPaidWage":
                                {
                                    string name = JsonParsing.getString(data, "NpcCrewName");
                                    long crewid = JsonParsing.getLong(data, "NpcCrewId");
                                    long amount = JsonParsing.getLong(data, "Amount");
                                    events.Add(new CrewPaidWageEvent(timestamp, name, crewid, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "NpcCrewRank":
                                {
                                    string name = JsonParsing.getString(data, "NpcCrewName");
                                    long crewid = JsonParsing.getLong(data, "NpcCrewId");
                                    data.TryGetValue("RankCombat", out object val);
                                    CombatRating rating = CombatRating.FromRank(Convert.ToInt32(val));
                                    events.Add(new CrewPromotionEvent(timestamp, name, crewid, rating) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "JoinACrew":
                                {
                                    string captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewJoinedEvent(timestamp, captain) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "QuitACrew":
                                {
                                    string captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewLeftEvent(timestamp, captain) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "ChangeCrewRole":
                                {
                                    string role = getRole(data, "Role");
                                    events.Add(new CrewRoleChangedEvent(timestamp, role) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CrewMemberJoins":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewMemberJoinedEvent(timestamp, member) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CrewMemberQuits":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewMemberLeftEvent(timestamp, member) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CrewLaunchFighter":
                                {
                                    string name = JsonParsing.getString(data, "Crew");
                                    events.Add(new CrewMemberLaunchedEvent(timestamp, name) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CrewMemberRoleChange":
                                {
                                    string name = JsonParsing.getString(data, "Crew");
                                    string role = getRole(data, "Role");
                                    events.Add(new CrewMemberRoleChangedEvent(timestamp, name, role) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "KickCrewMember":
                                {
                                    string member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                    events.Add(new CrewMemberRemovedEvent(timestamp, member) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "BuyAmmo":
                                {
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;
                                    events.Add(new ShipRestockedEvent(timestamp, price) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "BuyDrones":
                                {
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;
                                    data.TryGetValue("BuyPrice", out val);
                                    int price = (int)(long)val;
                                    events.Add(new LimpetPurchasedEvent(timestamp, amount, price) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "SellDrones":
                                {
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;
                                    data.TryGetValue("SellPrice", out val);
                                    int price = (int)(long)val;
                                    events.Add(new LimpetSoldEvent(timestamp, amount, price) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "LaunchDrone":
                                {
                                    string kind = JsonParsing.getString(data, "Type");
                                    events.Add(new LimpetLaunchedEvent(timestamp, kind) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "ClearSavedGame":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    events.Add(new ClearedSaveEvent(timestamp, name) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "NewCommander":
                                {
                                    string name = JsonParsing.getString(data, "Name");
                                    string package = JsonParsing.getString(data, "Package");
                                    events.Add(new CommanderStartedEvent(timestamp, name, package) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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

                                    events.Add(new CommanderProgressEvent(timestamp, combat, trade, exploration, cqc, empire, federation) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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

                                    events.Add(new CommanderRatingsEvent(timestamp, combat, trade, exploration, cqc, empire, federation) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
                            case "BuyTradeData":
                                {
                                    string system = JsonParsing.getString(data, "System");
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;

                                    events.Add(new TradeDataPurchasedEvent(timestamp, system, price) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
                            case "RefuelPartial":
                                {
                                    decimal amount = JsonParsing.getDecimal(data, "Amount");
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null, false) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "RefuelAll":
                                {
                                    decimal amount = JsonParsing.getDecimal(data, "Amount");
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null, true) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "FuelScoop":
                                {
                                    decimal amount = JsonParsing.getDecimal(data, "Scooped");
                                    decimal total = JsonParsing.getDecimal(data, "Total");
                                    bool full = EDDI.Instance.CurrentShip?.fueltanktotalcapacity == null
                                        ? false
                                        : Math.Round(total) == EDDI.Instance.CurrentShip.fueltanktotalcapacity;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Scoop", null, amount, total, full) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "Friends":
                                {
                                    string status = JsonParsing.getString(data, "Status");
                                    string name = JsonParsing.getString(data, "Name");
                                    name = name.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    events.Add(new FriendsEvent(timestamp, name, status) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "JetConeBoost":
                                {
                                    decimal boost = JsonParsing.getDecimal(data, "BoostValue");
                                    events.Add(new JetConeBoostEvent(timestamp, boost) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "JetConeDamage":
                                {
                                    string modulename = JsonParsing.getString(data, "Module");
                                    Module module = Module.FromEDName(modulename);
                                    if (module != null)
                                    {
                                        if (module.mount != null)
                                        {
                                            // This is a weapon so provide a bit more information
                                            string mount = module.LocalizedMountName();
                                            modulename = "" + module.@class.ToString() + module.grade + " " + mount + " " + module.localizedName;
                                        }
                                        else
                                        {
                                            modulename = module.localizedName;
                                        }
                                    }

                                    events.Add(new JetConeDamageEvent(timestamp, modulename, module) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    else if (type == "settlement" || type == "scannable")
                                    {
                                        events.Add(new DataVoucherRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        Logging.Warn("Unhandled voucher type " + type, line);
                                    }
                                    handled = true;
                                    break;
                                }
                            case "CommunityGoal":
                                {

                                    // There may be multiple goals in each event. We add them all to lists
                                    data.TryGetValue("CurrentGoals", out object val);
                                    List<object> goalsdata = (List<object>)val;

                                    // Create empty lists
                                    List<long> cgid = new List<long>();
                                    List<string> name = new List<string>();
                                    List<string> system = new List<string>();
                                    List<string> station = new List<string>();
                                    List<long> expiry = new List<long>();
                                    List<bool> iscomplete = new List<bool>();
                                    List<int> total = new List<int>();
                                    List<int> contribution = new List<int>();
                                    List<int> contributors = new List<int>();
                                    List<decimal> percentileband = new List<decimal>();

                                    List<int?> topranksize = new List<int?>();
                                    List<bool?> toprank = new List<bool?>();

                                    List<string> tier = new List<string>();
                                    List<long?> tierreward = new List<long?>();

                                    // Fill the lists
                                    foreach (IDictionary<string, object> goaldata in goalsdata)
                                    {
                                        cgid.Add(JsonParsing.getLong(goaldata, "CGID"));
                                        name.Add(JsonParsing.getString(goaldata, "Title"));
                                        system.Add(JsonParsing.getString(goaldata, "SystemName"));
                                        station.Add(JsonParsing.getString(goaldata, "MarketName"));
                                        DateTime expiryDateTime = ((DateTime)goaldata["Expiry"]).ToUniversalTime();
                                        long expiryseconds = (long)(expiryDateTime - timestamp).TotalSeconds;
                                        expiry.Add(expiryseconds);
                                        iscomplete.Add(JsonParsing.getBool(goaldata, "IsComplete"));
                                        total.Add(JsonParsing.getInt(goaldata, "CurrentTotal"));
                                        contribution.Add(JsonParsing.getInt(goaldata, "PlayerContribution"));
                                        contributors.Add(JsonParsing.getInt(goaldata, "NumContributors"));
                                        percentileband.Add(JsonParsing.getDecimal(goaldata, "PlayerPercentileBand"));

                                        // If the community goal is constructed with a fixed-size top rank (ie max reward for top 10 players)

                                        topranksize.Add(JsonParsing.getOptionalInt(goaldata, "TopRankSize"));
                                        toprank.Add(JsonParsing.getOptionalBool(goaldata, "PlayerInTopRank"));

                                        // If the community goal has reached the first success tier

                                        goaldata.TryGetValue("TierReached", out val);
                                        tier.Add((string)val);
                                        tierreward.Add(JsonParsing.getOptionalLong(goaldata, "Bonus"));
                                    }

                                    events.Add(new CommunityGoalEvent(timestamp, cgid, name, system, station, expiry, iscomplete, total, contribution, contributors, percentileband, topranksize, toprank, tier, tierreward) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CommunityGoalJoin":
                                {
                                    long cgid = JsonParsing.getLong(data, "CGID");
                                    string name = JsonParsing.getString(data, "Name");
                                    string system = JsonParsing.getString(data, "System");

                                    events.Add(new MissionAcceptedEvent(timestamp, cgid, "MISSION_CommunityGoal", name, null, system, null, null, null, null, null, null, null, null, null, true, null, null, null, null, false) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "CommunityGoalDiscard":
                                {
                                    long cgid = JsonParsing.getLong(data, "CGID");

	                                events.Add(new MissionAbandonedEvent(timestamp, cgid, "MISSION_CommunityGoal", 0));
                                    handled = true;
                                    break;
                                }
                            case "CommunityGoalReward":
                                {
                                    long cgid = JsonParsing.getLong(data, "CGID");
                                    string name = JsonParsing.getString(data, "Name");
                                    string system = JsonParsing.getString(data, "System");
                                    data.TryGetValue("Reward", out object val);
                                    long reward = (val == null ? 0 : (long)val);

                                    events.Add(new MissionCompletedEvent(timestamp, cgid, "MISSION_CommunityGoal", name, null, null, true, reward, null, null, null, 0) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
                            case "Missions":
                                {
                                    List<Mission> missions = new List<Mission>();

                                    for (int i = 0; i < 3; i++)
                                    {
                                        MissionStatus missionStatus = MissionStatus.FromStatus(i);
                                        string status = missionStatus.invariantName;
                                        data.TryGetValue(status, out object val);
                                        List<object> missionLog = (List<object>)val;

                                        foreach (object mission in missionLog)
                                        {
                                            Dictionary<string, object> missionProperties = (Dictionary<string, object>)mission;
                                            long missionId = JsonParsing.getLong(missionProperties, "MissionID");
                                            string name = JsonParsing.getString(missionProperties, "Name");
                                            decimal expires = JsonParsing.getDecimal(missionProperties, "Expires");
                                            DateTime expiry = DateTime.Now.AddSeconds((double)expires);
                                            if (i == 0 && expires == 0)
                                            {
                                                // If mission is 'Active' and 'expires' = 0, add 24 hours to expiry
                                                expiry = DateTime.Now.AddSeconds((double)expires).AddDays(1);
                                            }

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
                                    handled = true;
                                }
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
                                    handled = true;
                                }
                                break;
                            case "MissionAccepted":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    long missionid = (long)val;
                                    data.TryGetValue("Expiry", out val);
                                    DateTime? expiry = (val == null ? (DateTime?)null : (DateTime)val);
                                    string name = JsonParsing.getString(data, "Name");
                                    string localisedname = JsonParsing.getString(data, "LocalisedName");
                                    string faction = getFactionName(data, "Faction");
                                    int? reward = JsonParsing.getOptionalInt(data, "Reward");
                                    bool wing = JsonParsing.getBool(data, "Wing");

                                    // Missions with destinations
                                    string destinationsystem = JsonParsing.getString(data, "DestinationSystem");
                                    string destinationstation = JsonParsing.getString(data, "DestinationStation");

                                    // Missions with commodities
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Commodity"));
                                    data.TryGetValue("Count", out val);
                                    int? amount = (int?)(long?)val;

                                    // Missions with targets
                                    string target = JsonParsing.getString(data, "Target");
                                    string targettype = JsonParsing.getString(data, "TargetType");
                                    string targetfaction = getFactionName(data, "TargetFaction");
                                    data.TryGetValue("KillCount", out val);
                                    if (val != null)
                                    {
                                        amount = (int?)(long?)val;
                                    }

                                    // Missions with passengers
                                    int? passengercount = JsonParsing.getOptionalInt(data, "PassengerCount");
                                    string passengertype = JsonParsing.getString(data, "PassengerType");
                                    bool? passengerswanted = JsonParsing.getOptionalBool(data, "PassengerWanted");
                                    bool? passengervips = JsonParsing.getOptionalBool(data, "PassengerVIPs");
                                    data.TryGetValue("PassengerCount", out val);
                                    if (val != null)
                                    {
                                        amount = (int?)(long?)val;
                                    }

                                    // Impact on influence and reputation
                                    string influence = JsonParsing.getString(data, "Influence");
                                    string reputation = JsonParsing.getString(data, "Reputation");

                                    events.Add(new MissionAcceptedEvent(timestamp, missionid, name, localisedname, faction, destinationsystem, destinationstation, commodity, amount, passengerswanted, passengertype, passengervips, target, targettype, targetfaction, false, expiry, influence, reputation, reward, wing) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "MissionCompleted":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    long missionid = (long)val;
                                    string name = JsonParsing.getString(data, "Name");
                                    data.TryGetValue("Reward", out val);
                                    long reward = (val == null ? 0 : (long)val);
                                    long donation = JsonParsing.getOptionalLong(data, "Donated") ?? 0;
                                    string faction = getFactionName(data, "Faction");

                                    // Missions with commodities
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Commodity"));
                                    data.TryGetValue("Count", out val);
                                    int? amount = (int?)(long?)val;

                                    List<string> permitsAwarded = new List<string>();
                                    data.TryGetValue("PermitsAwarded", out val);
                                    List<object> permitsAwardedData = (List<object>)val;
                                    if (permitsAwardedData != null)
                                    {
                                        foreach (Dictionary<string, object> permitAwardedData in permitsAwardedData)
                                        {
                                            string permitAwarded = JsonParsing.getString(permitAwardedData, "Name");
                                            permitsAwarded.Add(permitAwarded);
                                        }
                                    }

                                    List<CommodityAmount> commodityrewards = new List<CommodityAmount>();
                                    data.TryGetValue("CommodityReward", out val);
                                    List<object> commodityRewardsData = (List<object>)val;
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

                                    List<MaterialAmount> materialsrewards = new List<MaterialAmount>();
                                    data.TryGetValue("MaterialsReward", out val);
                                    List<object> materialsRewardsData = (List<object>)val;
                                    if (materialsRewardsData != null)
                                    {
                                        foreach (Dictionary<string, object> materialsRewardData in materialsRewardsData)
                                        {
                                            Material rewardMaterial = Material.FromEDName(JsonParsing.getString(materialsRewardData, "Name"));
                                            materialsRewardData.TryGetValue("Count", out val);
                                            int count = (int)(long)val;
                                            materialsrewards.Add(new MaterialAmount(rewardMaterial, count));
                                        }
                                    }

                                    events.Add(new MissionCompletedEvent(timestamp, missionid, name, faction, commodity, amount, false, reward, permitsAwarded, commodityrewards, materialsrewards, donation) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "MissionAbandoned":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    long missionid = (long)val;
                                    string name = JsonParsing.getString(data, "Name");
	                                data.TryGetValue("Fine", out val);
	                                long fine = val == null ? 0 : (long)val;
	                                events.Add(new MissionAbandonedEvent(timestamp, missionid, name, fine) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
                            case "MissionFailed":
                                {
                                    data.TryGetValue("MissionID", out object val);
                                    long missionid = (long)val;
                                    string name = JsonParsing.getString(data, "Name");
                                    events.Add(new MissionFailedEvent(timestamp, missionid, name) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
                            case "AfmuRepairs":
                                {
                                    string item = JsonParsing.getString(data, "Module");
                                    // Item might be a module
                                    Module module = Module.FromEDName(item);
                                    if (module != null)
                                    {
                                        if (module.mount != null)
                                        {
                                            // This is a weapon so provide a bit more information
                                            string mount;
                                            if (module.mount == Module.ModuleMount.Fixed)
                                            {
                                                mount = "fixed";
                                            }
                                            else if (module.mount == Module.ModuleMount.Gimballed)
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

                                    bool repairedfully = JsonParsing.getBool(data, "FullyRepaired");
                                    decimal health = JsonParsing.getDecimal(data, "Health");

                                    events.Add(new ShipAfmuRepairedEvent(timestamp, item, repairedfully, health) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "Repair":
                                {
                                    string item = JsonParsing.getString(data, "Item");
                                    if (item == "Wear")
                                    {
                                        item = EddiDataDefinitions.Properties.Modules.ShipIntegrity;
                                    }
                                    else if (item != "All" && item != "Paint")
                                    {
                                        // Item might be a module
                                        Module module = Module.FromEDName(item);
                                        if (module != null)
                                        {
                                            if (module.mount != null)
                                            {
                                                // This is a weapon so provide a bit more information
                                                string mount;
                                                switch (module.mount)
                                                {
                                                    case Module.ModuleMount.Fixed:
                                                        mount = "fixed";
                                                        break;
                                                    case Module.ModuleMount.Gimballed:
                                                        mount = "gimballed";
                                                        break;
                                                    default:
                                                        mount = "turreted";
                                                        break;
                                                }
                                                item = $"{module.@class}{module.grade} {mount} {module.localizedName}";
                                            }
                                            else
                                            {
                                                item = module.localizedName;
                                            }
                                        }
                                    }
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;
                                    events.Add(new ShipRepairedEvent(timestamp, item, price) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "RepairDrone":
                                {
                                    decimal? hull = JsonParsing.getOptionalDecimal(data, "HullRepaired");
                                    decimal? cockpit = JsonParsing.getOptionalDecimal(data, "CockpitRepaired");
                                    decimal? corrosion = JsonParsing.getOptionalDecimal(data, "CorrosionRepaired");

                                    events.Add(new ShipRepairDroneEvent(timestamp, hull, cockpit, corrosion) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "RepairAll":
                                {
                                    data.TryGetValue("Cost", out object val);
                                    long price = (long)val;
                                    events.Add(new ShipRepairedEvent(timestamp, null, price) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "RebootRepair":
                                {
                                    data.TryGetValue("Modules", out object val);
                                    List<object> modulesJson = (List<object>)val;

                                    List<string> modules = new List<string>();
                                    foreach (string module in modulesJson)
                                    {
                                        modules.Add(module);
                                    }
                                    events.Add(new ShipRebootedEvent(timestamp, modules) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
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
	                                int cargocarried = JsonParsing.getInt(data, "Count");
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
                                    string power = JsonParsing.getString(data, "Power");

                                    events.Add(new PowerJoinedEvent(timestamp, power) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "PowerplayLeave":
                                {
                                    string power = JsonParsing.getString(data, "Power");

                                    events.Add(new PowerLeftEvent(timestamp, power) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "PowerplayDefect":
                                {
                                    string frompower = JsonParsing.getString(data, "FromPower");
                                    string topower = JsonParsing.getString(data, "ToPower");

                                    events.Add(new PowerDefectedEvent(timestamp, frompower, topower) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "PowerplayVote":
                                {
                                    string power = JsonParsing.getString(data, "Power");
                                    string system = JsonParsing.getString(data, "System");
                                    data.TryGetValue("Votes", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerPreparationVoteCast(timestamp, power, system, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "PowerplaySalary":
                                {
                                    string power = JsonParsing.getString(data, "Power");
                                    data.TryGetValue("Amount", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerSalaryClaimedEvent(timestamp, power, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "PowerplayCollect":
                                {
                                    string power = JsonParsing.getString(data, "Power");
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Type"));
                                    commodity.fallbackLocalizedName = JsonParsing.getString(data, "Type_Localised");
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerCommodityObtainedEvent(timestamp, power, commodity, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "PowerplayDeliver":
                                {
                                    string power = JsonParsing.getString(data, "Power");
                                    CommodityDefinition commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Type"));
                                    commodity.fallbackLocalizedName = JsonParsing.getString(data, "Type_Localised");
                                    data.TryGetValue("Count", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerCommodityDeliveredEvent(timestamp, power, commodity, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "PowerplayFastTrack":
                                {
                                    string power = JsonParsing.getString(data, "Power");
                                    data.TryGetValue("Cost", out object val);
                                    int amount = (int)(long)val;

                                    events.Add(new PowerCommodityFastTrackedEvent(timestamp, power, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "PowerplayVoucher":
                                {
                                    string power = JsonParsing.getString(data, "Power");
                                    data.TryGetValue("Systems", out object val);
                                    List<string> systems = ((List<object>)val).Cast<string>().ToList();

                                    events.Add(new PowerVoucherReceivedEvent(timestamp, power, systems) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "SquadronStartup":
                                {
                                    string name = JsonParsing.getString(data, "SquadronName");
                                    int rank = JsonParsing.getInt(data, "CurrentRank");

                                    events.Add(new SquadronStartupEvent(timestamp, name, rank) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
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
                                    handled = true;
                                    break;
                                }
                            case "SquadronDemotion":
                            case "SquadronPromotion":
                                {
                                    string name = JsonParsing.getString(data, "SquadronName");
                                    int oldrank = JsonParsing.getInt(data, "OldRank");
                                    int newrank = JsonParsing.getInt(data, "NewRank");

                                    events.Add(new SquadronRankEvent(timestamp, name, oldrank, newrank) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "SystemsShutdown":
                                {
                                    events.Add(new ShipShutdownEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "Fileheader":
                                {
                                    string filename = journalFileName;
                                    string version = JsonParsing.getString(data, "gameversion");
                                    string build = JsonParsing.getString(data, "build").Replace(" ", "");

                                    events.Add(new FileHeaderEvent(timestamp, filename, version, build) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "Shutdown":
                                {
                                    events.Add(new ShutdownEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "FSDTarget":
                                {
                                    string systemName = JsonParsing.getString(data, "Name");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    events.Add(new FSDTargetEvent(timestamp, systemName, systemAddress) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }
                            case "FSSAllBodiesFound":
                                {
                                    string systemName = JsonParsing.getString(data, "SystemName");
                                    long systemAddress = JsonParsing.getLong(data, "SystemAddress");
                                    int count = JsonParsing.getInt(data, "Count");
                                    events.Add(new SystemScanComplete(timestamp, systemName, systemAddress, count) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }

                            case "Commander":
                            case "Reputation":
                            case "Statistics":
                            case "CodexEntry":
                            case "ReservoirReplenished":
                            case "ProspectedAsteroid":
                            case "CrimeVictim":
                            case "Scanned":
                                // we silently ignore these, but forward them to the responders
                                break;
                            default:
                                throw new NotImplementedException($"EDDI has no handler for event type '{edType}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Something went wrong, but an unhandled event will still be passed to the responders.
                        Logging.Warn($"{ex.Message}/r/nRaw event:/r/n{line}");
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

        private static SignalSource GetSignalSource(IDictionary<string, object> data)
        {
            // The source may be a direct source or a USS. If a USS, we want the USS type.
            SignalSource source;
            if (JsonParsing.getString(data, "USSType") != null)
            {
                source = SignalSource.FromEDName(JsonParsing.getString(data, "USSType"));
                source.fallbackLocalizedName = JsonParsing.getString(data, "USSType_Localised");
            }
            else
            {
                source = SignalSource.FromEDName(JsonParsing.getString(data, "SignalName"));
                source.fallbackLocalizedName = JsonParsing.getString(data, "SignalName_Localised");
            }

            return source;
        }

        private static Superpower getAllegiance(IDictionary<string, object> data, string key)
        {
            data.TryGetValue(key, out object val);
            // FD sends "" rather than null; fix that here
            if (((string)val) == "") { val = null; }
            return Superpower.FromNameOrEdName((string)val);
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
            if (data.TryGetValue(type + "Faction", out object val))
            {
                if (val is Dictionary<string, object> factionData) // 3.3.03 or later journal
                {
                    faction.name = JsonParsing.getString(factionData, "Name");

                    // Get the faction information specific to the star system
                    FactionPresence factionPresense = new FactionPresence()
                    {
                        systemName = systemName,
                        FactionState = FactionState.FromEDName(JsonParsing.getString(factionData, "FactionState") ?? "None"),
                    };
                    faction.presences.Add(factionPresense);
                }
                else // per-3.3.03 journal
                {
                    faction.name = val as string;
                }
            }

            // Get the faction allegiance
            if (data.TryGetValue(type + "Allegiance", out val))
            {
                faction.Allegiance = getAllegiance(data, type + "Allegiance");
            }

            // Station controlling faction government not discretely available in 'Location' event
            else if (data.TryGetValue("Factions", out val))
            {
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
                FactionState fState = FactionState.FromEDName(JsonParsing.getString(factionDetail, "FactionState") ?? "None");
                Government fGov = Government.FromEDName(JsonParsing.getString(factionDetail, "SystemGovernment") ?? "$government_None;");
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
                        factionPresense.ActiveStates.Add(FactionState.FromEDName(JsonParsing.getString(activeState, "State") ?? "None"));
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
                            FactionState.FromEDName(JsonParsing.getString(pendingState, "State") ?? "None"),
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
                            FactionState.FromEDName(JsonParsing.getString(recoveringState, "State") ?? "None"),
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

        private static string npcSpeechBy(string from, string message)
        {
            string by = null;
            if (message.StartsWith("$AmbushedPilot_"))
            {
                by = "Ambushed pilot";
            }
            else if (message.StartsWith("$BountyHunter"))
            {
                by = "Bounty hunter";
            }
            else if (message.StartsWith("$CapShip") || message.StartsWith("$FEDCapShip"))
            {
                by = "Capital ship";
            }
            else if (message.StartsWith("$CargoHunter"))
            {
                by = "Cargo hunter"; // Mission specific
            }
            else if (message.StartsWith("$Commuter"))
            {
                by = "Civilian pilot";
            }
            else if (message.StartsWith("$ConvoyExplorers"))
            {
                by = "Exploration convoy";
            }
            else if (message.StartsWith("$ConvoyWedding"))
            {
                by = "Wedding convoy";
            }
            else if (message.StartsWith("$CruiseLiner"))
            {
                by = "Cruise liner";
            }
            else if (message.StartsWith("$Escort"))
            {
                by = "Escort";
            }
            else if (message.StartsWith("$Hitman"))
            {
                by = "Hitman";
            }
            else if (message.StartsWith("$Messenger"))
            {
                by = "Messenger";
            }
            else if (message.StartsWith("$Military"))
            {
                by = "Military";
            }
            else if (message.StartsWith("$Miner"))
            {
                by = "Miner";
            }
            else if (message.StartsWith("$PassengerHunter"))
            {
                by = "Passenger hunter"; // Mission specific
            }
            else if (message.StartsWith("$PassengerLiner"))
            {
                by = "Passenger liner";
            }
            else if (message.StartsWith("$Pirate"))
            {
                by = "Pirate";
            }
            else if (message.StartsWith("$Police"))
            {
                // Police messages appear to be re-used by bounty hunters.  Check from to see if it really is police
                if (from.Contains("Police"))
                {
                    by = "Police";
                }
                else
                {
                    by = "Bounty hunter";
                }
            }
            else if (message.StartsWith("$PowersAssassin"))
            {
                by = "Rival power's agent";  // Power play specific
            }
            else if (message.StartsWith("$PowersPirate"))
            {
                by = "Rival power's agent"; // Power play specific
            }
            else if (message.StartsWith("$PowersSecurity"))
            {
                by = "Rival power's agent"; // Power play specific
            }
            else if (message.StartsWith("$Propagandist"))
            {
                by = "Propagandist";
            }
            else if (message.StartsWith("$Protester"))
            {
                by = "Protester";
            }
            else if (message.StartsWith("$Refugee"))
            {
                by = "Refugee";
            }
            else if (message.StartsWith("$Smuggler"))
            {
                by = "Civilian pilot";  // We shouldn't recognize a smuggler without a cargo scan
            }
            else if (message.StartsWith("$StarshipOne"))
            {
                by = "Starship One";
            }
            else if (message.Contains("_SearchandRescue_"))
            {
                by = "Search and rescue";
            }
            else
            {
                by = "NPC";
            }
            return by;
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

        public string MonitorVersion()
        {
            return "1.0.0";
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
            internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
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
                compartment.size = (int)ShipDefinitions.FromEDModel(ship)?.militarysize;
            }

            return compartment;
        }

        private static string[] ignoredLogLoadEvents = new string[]
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
            "ReceiveText",
            "Scanned",
            "SendText",
            "ShieldState",
            "ShipTargeted",
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
