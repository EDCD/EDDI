using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiCargoMonitor;
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

        public JournalMonitor() : base(GetSavedGamesDir(), @"^Journal.*\.[0-9\.]+\.log$", result =>
        ForwardJournalEntry(result, EDDI.Instance.eventHandler)) { }

        public static void ForwardJournalEntry(string line, Action<Event> callback)
        {
            if (line == null)
            {
                return;
            }

            List<Event> events = ParseJournalEntry(line);
            foreach (Event @event in events)
            {
                callback(@event);
            }
        }

        public static List<Event> ParseJournalEntry(string line)
        {
            List<Event> events = new List<Event>();
            try
            {
                Match match = JsonRegex.Match(line);
                if (match.Success)
                {
                    IDictionary<string, object> data = Deserializtion.DeserializeData(line);

                    // Every event has a timestamp field
                    DateTime timestamp = DateTime.Now;
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

                    switch (edType)
                    {
                        case "Docked":
                            {
                                string systemName = JsonParsing.getString(data, "StarSystem");
                                string stationName = JsonParsing.getString(data, "StationName");
                                string stationState = JsonParsing.getString(data, "StationState") ?? string.Empty;
                                string stationModel = JsonParsing.getString(data, "StationType");
                                Superpower allegiance = getAllegiance(data, "StationAllegiance");
                                string faction = getFaction(data, "StationFaction");
                                SystemState factionState = SystemState.FromEDName(JsonParsing.getString(data, "FactionState"));
                                Economy economy = Economy.FromEDName(JsonParsing.getString(data, "StationEconomy"));
                                Government government = Government.FromEDName(JsonParsing.getString(data, "StationGovernment"));
                                decimal? distancefromstar = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

                                // Get station services data
                                object val;
                                data.TryGetValue("StationServices", out val);
                                List<string> stationservices = (val as List<object>)?.Cast<string>()?.ToList();

                                events.Add(new DockedEvent(timestamp, systemName, stationName, stationState, stationModel, allegiance, faction, factionState, economy, government, distancefromstar, stationservices) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Undocked":
                            {
                                string stationName = JsonParsing.getString(data, "StationName");
                                events.Add(new UndockedEvent(timestamp, stationName) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Touchdown":
                            {
                                decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                bool playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;
                                events.Add(new TouchdownEvent(timestamp, longitude, latitude, playercontrolled) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Liftoff":
                            {
                                decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                bool playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;
                                events.Add(new LiftoffEvent(timestamp, longitude, latitude, playercontrolled) { raw = line });
                            }
                            handled = true;
                            break;
                        case "SupercruiseEntry":
                            {
                                string system = JsonParsing.getString(data, "StarySystem");
                                events.Add(new EnteredSupercruiseEvent(timestamp, system) { raw = line });
                            }
                            handled = true;
                            break;
                        case "SupercruiseExit":
                            {
                                string system = JsonParsing.getString(data, "StarSystem");
                                string body = JsonParsing.getString(data, "Body");
                                string bodyType = JsonParsing.getString(data, "BodyType");
                                events.Add(new EnteredNormalSpaceEvent(timestamp, system, body, bodyType) { raw = line });
                            }
                            handled = true;
                            break;
                        case "FSDJump":
                            {
                                object val;

                                string systemName = JsonParsing.getString(data, "StarSystem");
                                data.TryGetValue("StarPos", out val);
                                List<object> starPos = (List<object>)val;
                                decimal x = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                decimal y = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                decimal z = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;

                                decimal fuelUsed = JsonParsing.getDecimal(data, "FuelUsed");
                                decimal fuelRemaining = JsonParsing.getDecimal(data, "FuelLevel");
                                decimal distance = JsonParsing.getDecimal(data, "JumpDist");
                                Superpower allegiance = getAllegiance(data, "SystemAllegiance");
                                string faction = getFaction(data, "SystemFaction");
                                SystemState factionState = SystemState.FromEDName(JsonParsing.getString(data, "FactionState"));
                                Economy economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy"));
                                Government government = Government.FromEDName(JsonParsing.getString(data, "SystemGovernment"));
                                SecurityLevel security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity"));
                                long? population = JsonParsing.getOptionalLong(data, "Population");

                                events.Add(new JumpedEvent(timestamp, systemName, x, y, z, distance, fuelUsed, fuelRemaining, allegiance, faction, factionState, economy, government, security, population) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Location":
                            {
                                object val;

                                string systemName = JsonParsing.getString(data, "StarSystem");

                                if (systemName == "Training")
                                {
                                    // Training system; ignore
                                    break;
                                }

                                data.TryGetValue("StarPos", out val);
                                List<object> starPos = (List<object>)val;
                                decimal x = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                decimal y = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                decimal z = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;

                                string body = JsonParsing.getString(data, "Body");
                                string bodyType = JsonParsing.getString(data, "BodyType");
                                bool docked = JsonParsing.getBool(data, "Docked");
                                Superpower allegiance = getAllegiance(data, "SystemAllegiance");
                                string faction = getFaction(data, "SystemFaction");
                                Economy economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy"));
                                Government government = Government.FromEDName(JsonParsing.getString(data, "SystemGovernment"));
                                SecurityLevel security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity"));
                                long? population = JsonParsing.getOptionalLong(data, "Population");

                                string station = JsonParsing.getString(data, "StationName");
                                string stationtype = JsonParsing.getString(data, "StationType");

                                decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                events.Add(new LocationEvent(timestamp, systemName, x, y, z, body, bodyType, docked, station, stationtype, allegiance, faction, economy, government, security, population, longitude, latitude) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Bounty":
                            {
                                object val;

                                string target = JsonParsing.getString(data, "Target");
                                if (target != null)
                                {
                                    // Target might be a ship, but if not then the string we provide is repopulated in ship.model so use it regardless
                                    Ship ship = ShipDefinitions.FromEDModel(target);
                                    target = ship.model;
                                }

                                string victimFaction = getFaction(data, "VictimFaction");

                                data.TryGetValue("SharedWithOthers", out val);
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
                                    string factionName = getFaction(data, "Faction");
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
                                            string factionName = getFaction(rewardData, "Faction");
                                            rewardData.TryGetValue("Reward", out val);
                                            long factionReward = (long)val;

                                            rewards.Add(new Reward(factionName, factionReward));
                                        }
                                    }
                                }

                                events.Add(new BountyAwardedEvent(timestamp, target, victimFaction, reward, rewards, shared) { raw = line });
                            }
                            handled = true;
                            break;
                        case "CapShipBond":
                        case "DatalinkVoucher":
                        case "FactionKillBond":
                            {
                                object val;
                                data.TryGetValue("Reward", out val);
                                long reward = (long)val;
                                string victimFaction = JsonParsing.getString(data, "VictimFaction");

                                if (data.ContainsKey("AwardingFaction"))
                                {
                                    string awardingFaction = getFaction(data, "AwardingFaction");
                                    events.Add(new BondAwardedEvent(timestamp, awardingFaction, victimFaction, reward) { raw = line });
                                }
                                else if (data.ContainsKey("PayeeFaction"))
                                {
                                    string payeeFaction = getFaction(data, "PayeeFaction");
                                    events.Add(new DataVoucherAwardedEvent(timestamp, payeeFaction, victimFaction, reward) { raw = line });
                                }
                            }
                            handled = true;
                            break;
                        case "CommitCrime":
                            {
                                object val;
                                string crimetype = JsonParsing.getString(data, "CrimeType");
                                string faction = getFaction(data, "Faction");
                                string victim = JsonParsing.getString(data, "Victim");
                                // Might be a fine or a bounty
                                if (data.ContainsKey("Fine"))
                                {
                                    data.TryGetValue("Fine", out val);
                                    long fine = (long)val;
                                    events.Add(new FineIncurredEvent(timestamp, crimetype, faction, victim, fine) { raw = line });
                                }
                                else
                                {
                                    data.TryGetValue("Bounty", out val);
                                    long bounty = (long)val;
                                    events.Add(new BountyIncurredEvent(timestamp, crimetype, faction, victim, bounty) { raw = line });
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
                                    events.Add(new CombatPromotionEvent(timestamp, rating) { raw = line });
                                    handled = true;
                                }
                                else if (data.ContainsKey("Trade"))
                                {
                                    data.TryGetValue("Trade", out val);
                                    TradeRating rating = TradeRating.FromRank(Convert.ToInt32(val));
                                    events.Add(new TradePromotionEvent(timestamp, rating) { raw = line });
                                    handled = true;
                                }
                                else if (data.ContainsKey("Explore"))
                                {
                                    data.TryGetValue("Explore", out val);
                                    ExplorationRating rating = ExplorationRating.FromRank(Convert.ToInt32(val));
                                    events.Add(new ExplorationPromotionEvent(timestamp, rating) { raw = line });
                                    handled = true;
                                }
                                else if (data.ContainsKey("Federation"))
                                {
                                    Superpower superpower = Superpower.FromName("Federation");
                                    data.TryGetValue("Federation", out val);
                                    FederationRating rating = FederationRating.FromRank(Convert.ToInt32(val));
                                    events.Add(new FederationPromotionEvent(timestamp, rating) { raw = line });
                                    handled = true;
                                }
                                else if (data.ContainsKey("Empire"))
                                {
                                    data.TryGetValue("Empire", out val);
                                    EmpireRating rating = EmpireRating.FromRank(Convert.ToInt32(val));
                                    events.Add(new EmpirePromotionEvent(timestamp, rating) { raw = line });
                                    handled = true;
                                }
                            }
                            break;
                        case "CollectCargo":
                            {
                                string commodityName = JsonParsing.getString(data, "Type");
                                CommodityDefinition commodity = CommodityDefinition.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map collectcargo type " + commodityName + " to commodity");
                                }
                                bool stolen = JsonParsing.getBool(data, "Stolen");
                                events.Add(new CommodityCollectedEvent(timestamp, commodity, stolen) { raw = line });
                                handled = true;
                            }
                            handled = true;
                            break;
                        case "EjectCargo":
                            {
                                object val;
                                string commodityName = JsonParsing.getString(data, "Type");
                                CommodityDefinition commodity = CommodityDefinition.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map ejectcargo type " + commodityName + " to commodity");
                                }
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                bool abandoned = JsonParsing.getBool(data, "Abandoned");
                                events.Add(new CommodityEjectedEvent(timestamp, commodity, amount, abandoned) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Loadout":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "Ship");
                                string shipName = JsonParsing.getString(data, "ShipName");
                                string shipIdent = JsonParsing.getString(data, "ShipIdent");

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

                                            Module module = Module.FromEDName(item);
                                            if (module == null)
                                            {
                                                Logging.Info("Unknown module " + item);
                                                Logging.Report("Unknown module " + item, JsonConvert.SerializeObject(moduleData));
                                            }
                                            else
                                            {
                                                module.enabled = enabled;
                                                module.priority = priority;
                                                module.health = health;
                                                module.price = price;
                                                module.clipcapacity = clip;
                                                module.hoppercapacity = hopper;
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
                                        else
                                        {
                                            // This is a compartment
                                            Compartment compartment = new Compartment() { name = slot };
                                            Module module = Module.FromEDName(item);
                                            if (module == null)
                                            {
                                                Logging.Info("Unknown module " + item);
                                                Logging.Report("Unknown module " + item, JsonConvert.SerializeObject(moduleData));
                                            }
                                            else
                                            {
                                                module.enabled = enabled;
                                                module.priority = priority;
                                                module.health = health;
                                                module.price = price;
                                                compartment.module = module;
                                                compartments.Add(compartment);
                                            }
                                        }
                                    }
                                }
                                events.Add(new ShipLoadoutEvent(timestamp, ship, shipId, shipName, shipIdent, compartments, hardpoints, paintjob) { raw = line });
                            }
                            handled = true;
                            break;
                        case "CockpitBreached":
                            events.Add(new CockpitBreachedEvent(timestamp) { raw = line });
                            handled = true;
                            break;
                        case "ApproachBody":
                            {
                                string system = JsonParsing.getString(data, "StarSystem");
                                string body = JsonParsing.getString(data, "Body");
                                events.Add(new NearSurfaceEvent(timestamp, true, system, body) { raw = line });
                            }
                            handled = true;
                            break;
                        case "LeaveBody":
                            {
                                string system = JsonParsing.getString(data, "StarSystem");
                                string body = JsonParsing.getString(data, "Body");
                                events.Add(new NearSurfaceEvent(timestamp, false, system, body) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ApproachSettlement":
                            {
                                object val;
                                string name = JsonParsing.getString(data, "Name");
                                // Replace with localised name if available
                                if (data.TryGetValue("Name_Localised", out val))
                                {
                                    name = (string)val;
                                }
                                events.Add(new SettlementApproachedEvent(timestamp, name) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Scan":
                            {
                                object val;
                                string name = JsonParsing.getString(data, "BodyName");
                                decimal distancefromarrival = JsonParsing.getDecimal(data, "DistanceFromArrivalLS");

                                // Belt
                                if (name.Contains("Belt Cluster"))
                                {

                                    events.Add(new BeltScannedEvent(timestamp, name, distancefromarrival) { raw = line });
                                    handled = true;
                                    break;
                                }

                                // Common items
                                decimal radius = JsonParsing.getDecimal(data, "Radius");
                                decimal? orbitalperiod = JsonParsing.getOptionalDecimal(data, "OrbitalPeriod");
                                decimal rotationperiod = JsonParsing.getDecimal(data, "RotationPeriod");
                                decimal? semimajoraxis = JsonParsing.getOptionalDecimal(data, "SemiMajorAxis");
                                decimal? eccentricity = JsonParsing.getOptionalDecimal(data, "Eccentricity");
                                decimal? orbitalinclination = JsonParsing.getOptionalDecimal(data, "OrbitalInclination");
                                decimal? periapsis = JsonParsing.getOptionalDecimal(data, "Periapsis");

                                // Check whether we have a detailed discovery scanner on board the current ship
                                bool dssEquipped = false;
                                Ship ship = EDDI.Instance.CurrentShip;
                                if (ship != null)
                                {
                                    foreach (Compartment compartment in ship.compartments)
                                    {
                                        if ((compartment.module.basename == "DetailedSurfaceScanner") && (compartment.module.enabled))
                                        {
                                            dssEquipped = true;
                                        }
                                    }
                                }

                                // Rings
                                data.TryGetValue("Rings", out val);
                                List<object> ringsData = (List<object>)val;
                                List<Ring> rings = new List<Ring>();
                                if (ringsData != null)
                                {
                                    foreach (Dictionary<string, object> ringData in ringsData)
                                    {
                                        string ringName = JsonParsing.getString(ringData, "Name");
                                        string ringComposition = Composition.FromEDName(JsonParsing.getString(ringData, "RingClass")).localizedName;
                                        decimal ringMass = JsonParsing.getDecimal(ringData, "MassMT");
                                        decimal ringInnerRadius = JsonParsing.getDecimal(ringData, "InnerRad");
                                        decimal ringOuterRadius = JsonParsing.getDecimal(ringData, "OuterRad");

                                        rings.Add(new Ring(ringName, ringComposition, ringMass, ringInnerRadius, ringOuterRadius));
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
                                    decimal temperature = JsonParsing.getDecimal(data, "SurfaceTemperature");

                                    events.Add(new StarScannedEvent(timestamp, name, starType, stellarMass, radius, absoluteMagnitude, luminosityClass, ageMegaYears, temperature, distancefromarrival, orbitalperiod, rotationperiod, semimajoraxis, eccentricity, orbitalinclination, periapsis, rings, dssEquipped) { raw = line });
                                    handled = true;
                                }
                                else
                                {
                                    // Body
                                    bool? tidallyLocked = JsonParsing.getOptionalBool(data, "TidalLock");

                                    string bodyClass = JsonParsing.getString(data, "PlanetClass");
                                    decimal? earthMass = JsonParsing.getOptionalDecimal(data, "MassEM");

                                    // MKW: Gravity in the Journal is in m/s; must convert it to G
                                    decimal gravity = Body.ms2g(JsonParsing.getDecimal(data, "SurfaceGravity"));

                                    decimal? temperature = JsonParsing.getOptionalDecimal(data, "SurfaceTemperature");

                                    decimal? pressure = JsonParsing.getOptionalDecimal(data, "SurfacePressure");

                                    bool? landable = JsonParsing.getOptionalBool(data, "Landable");

                                    string reserves = JsonParsing.getString(data, "ReserveLevel");

                                    decimal? axialTilt = JsonParsing.getOptionalDecimal(data, "AxialTilt");

                                    // TODO atmosphere composition

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
                                        else if (val is List<object>)
                                        {
                                            // 2.3 style
                                            List<object> materialsJson = (List<object>)val;

                                            foreach (Dictionary<string, object> materialJson in materialsJson)
                                            {
                                                Material material = Material.FromEDName((string)materialJson["Name"]);
                                                materials.Add(new MaterialPresence(material, JsonParsing.getDecimal(materialJson, "Percent")));
                                            }
                                        }
                                    }

                                    string terraformState = JsonParsing.getString(data, "TerraformState");
                                    string atmosphere = JsonParsing.getString(data, "Atmosphere");
                                    Volcanism volcanism = Volcanism.FromName(JsonParsing.getString(data, "Volcanism"));

                                    events.Add(new BodyScannedEvent(timestamp, name, bodyClass, earthMass, radius, gravity, temperature, pressure, tidallyLocked, landable, atmosphere, volcanism, distancefromarrival, (decimal)orbitalperiod, rotationperiod, semimajoraxis, eccentricity, orbitalinclination, periapsis, rings, reserves, materials, terraformState, axialTilt, dssEquipped) { raw = line });
                                    handled = true;
                                }
                            }
                            break;
                        case "DatalinkScan":
                            {
                                string message = JsonParsing.getString(data, "Message");
                                events.Add(new DatalinkMessageEvent(timestamp, message) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DataScanned":
                            {
                                DataScan datalinktype = DataScan.FromEDName(JsonParsing.getString(data, "Type"));
                                events.Add(new DataScannedEvent(timestamp, datalinktype) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardBuy":
                            {
                                object val;
                                // We don't have a ship ID at this point so use the ship type
                                string ship = JsonParsing.getString(data, "ShipType");

                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;

                                data.TryGetValue("StoreShipID", out val);
                                int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                string storedShip = JsonParsing.getString(data, "StoreOldShip");

                                data.TryGetValue("SellShipID", out val);
                                int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                string soldShip = JsonParsing.getString(data, "SellOldShip");

                                data.TryGetValue("SellPrice", out val);
                                long? soldPrice = (long?)val;
                                events.Add(new ShipPurchasedEvent(timestamp, ship, price, soldShip, soldShipId, soldPrice, storedShip, storedShipId) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardNew":
                            {
                                object val;
                                data.TryGetValue("NewShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "ShipType");

                                events.Add(new ShipDeliveredEvent(timestamp, ship, shipId) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardSell":
                            {
                                object val;
                                data.TryGetValue("SellShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "ShipType");
                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;
                                string system = JsonParsing.getString(data, "System");                                
                                events.Add(new ShipSoldEvent(timestamp, ship, shipId, price, system) { raw = line });
                            }
                            handled = true;
                            break;
                        case "SellShipOnRebuy":
                            {
                                object val;
                                data.TryGetValue("SellShipId", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "ShipType");
                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;
                                string system = JsonParsing.getString(data, "System");
                                events.Add(new ShipSoldOnRebuyEvent(timestamp, ship, shipId, price, system) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardArrived":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "ShipType");
                                string system = JsonParsing.getString(data, "System");
                                decimal distance = JsonParsing.getDecimal(data, "Distance");
                                long? price = JsonParsing.getOptionalLong(data, "TransferPrice");
                                long? time = JsonParsing.getOptionalLong(data, "TransferTime");
                                string station = JsonParsing.getString(data, "Station");
                                events.Add(new ShipArrivedEvent(timestamp, ship, shipId, system, distance, price, time, station) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardSwap":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "ShipType");

                                data.TryGetValue("StoreShipID", out val);
                                int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                string storedShip = JsonParsing.getString(data, "StoreOldShip");

                                data.TryGetValue("SellShipID", out val);
                                int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                string soldShip = JsonParsing.getString(data, "SellOldShip");

                                events.Add(new ShipSwappedEvent(timestamp, ship, shipId, soldShip, soldShipId, storedShip, storedShipId) { raw = line });
                            }
                            handled = true;
                            break;
                        case "TechnologyBroker":
                            {
                                object val;

                                string brokerType = JsonParsing.getString(data, "BrokerType");
                                long marketId = JsonParsing.getLong(data, "MarketID");

                                data.TryGetValue("ItemsUnlocked", out val);
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
                                        Logging.Info("Unknown module " + moduleEdName);
                                        Logging.Report("Unknown module " + moduleEdName, JsonConvert.SerializeObject(item));
                                    }
                                    items.Add(module);
                                }

                                data.TryGetValue("Commodities", out val);
                                List<object> commodities = (List<object>)val;
                                List<CommodityAmount> Commodities = new List<CommodityAmount>();
                                foreach (Dictionary<string, object> _commodity in commodities)
                                {
                                    string commodityEdName = JsonParsing.getString(_commodity, "Name");
                                    CommodityDefinition commodity = CommodityDefinition.FromName(commodityEdName);
                                    int count = JsonParsing.getInt(_commodity, "Count");
                                    if (commodity == null)
                                    {
                                        Logging.Info("Unknown commodity " + commodityEdName);
                                        Logging.Report("Unknown commodity " + commodityEdName, JsonConvert.SerializeObject(_commodity));
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

                                events.Add(new TechnologyBrokerEvent(timestamp, brokerType, marketId, items, Commodities, Materials) { raw = line });
                                handled = true;
                            }
                            break;
                        case "ShipyardTransfer":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "ShipType");

                                string system = JsonParsing.getString(data, "System");
                                decimal distance = JsonParsing.getDecimal(data, "Distance");
                                long? price = JsonParsing.getOptionalLong(data, "TransferPrice");
                                long? time = JsonParsing.getOptionalLong(data, "TransferTime");

                                events.Add(new ShipTransferInitiatedEvent(timestamp, ship, shipId, system, distance, price, time) { raw = line });

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
                                        EDDI.Instance.eventHandler(new ShipArrivedEvent(DateTime.UtcNow, ship, shipId, arrivalSystem, distance, price, time, arrivalStation));
                                    }
                                }
                            }
                            handled = true;
                            break;
                        case "FetchRemoteModule":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
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

                                events.Add(new ModuleTransferEvent(timestamp, ship, shipId, storageSlot, serverId, module, transferCost, transferTime) { raw = line });

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
                                        EDDI.Instance.eventHandler(new ModuleArrivedEvent(DateTime.UtcNow, ship, shipId, storageSlot, serverId, module, transferCost, transferTime, arrivalSystem, arrivalStation));
                                    }
                                }
                            }
                            handled = true;
                            break;
                        case "MassModuleStore":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
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
                                        module.modified = JsonParsing.getString(item, "EngineerModifications") != null;
                                        modules.Add(module);
                                    }
                                }

                                events.Add(new ModulesStoredEvent(timestamp, ship, shipId, slots, modules) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleArrived":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
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

                                string system = JsonParsing.getString(data, "System");
                                string station = JsonParsing.getString(data, "Station");

                                events.Add(new ModuleArrivedEvent(timestamp, ship, shipId, storageSlot, serverId, module, transferCost, transferTime, system, station) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleBuy":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
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

                                events.Add(new ModulePurchasedEvent(timestamp, ship, shipId, slot, buyModule, buyPrice, sellModule, sellPrice, storedModule) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleRetrieve":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "Ship");

                                string slot = JsonParsing.getString(data, "Slot");
                                Module module = Module.FromEDName(JsonParsing.getString(data, "RetrievedItem"));
                                data.TryGetValue("Cost", out val);
                                long? cost = JsonParsing.getOptionalLong(data, "Cost");
                                string engineerModifications = JsonParsing.getString(data, "EngineerModifications");
                                module.modified = engineerModifications != null;

                                // Set retrieved module defaults
                                module.price = module.value;
                                module.enabled = true;
                                module.priority = 1;
                                module.health = 100;

                                Module swapoutModule = Module.FromEDName(JsonParsing.getString(data, "SwapOutItem"));

                                events.Add(new ModuleRetrievedEvent(timestamp, ship, shipId, slot, module, cost, engineerModifications, swapoutModule) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleSell":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "Ship");

                                string slot = JsonParsing.getString(data, "Slot");
                                Module module = Module.FromEDName(JsonParsing.getString(data, "SellItem"));
                                data.TryGetValue("SellPrice", out val);
                                long price = (long)val;


                                events.Add(new ModuleSoldEvent(timestamp, ship, shipId, slot, module, price ) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleSellRemote":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
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

                                events.Add(new ModuleSoldFromStorageEvent(timestamp, ship, shipId, storageSlot, serverId, module, price) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleStore":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "Ship");

                                string slot = JsonParsing.getString(data, "Slot");
                                Module module = Module.FromEDName(JsonParsing.getString(data, "StoredItem"));
                                string engineerModifications = JsonParsing.getString(data, "EngineerModifications");
                                module.modified = engineerModifications != null;
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

                                events.Add(new ModuleStoredEvent(timestamp, ship, shipId, slot, module, cost, engineerModifications, replacementModule) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleSwap":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "Ship");

                                string fromSlot = JsonParsing.getString(data, "FromSlot");
                                Module fromModule = Module.FromEDName(JsonParsing.getString(data, "FromItem"));
                                string toSlot = JsonParsing.getString(data, "ToSlot");
                                Module toModule = Module.FromEDName(JsonParsing.getString(data, "ToItem"));

                                events.Add(new ModuleSwappedEvent(timestamp, ship, shipId, fromSlot, fromModule, toSlot, toModule) { raw = line });
                            }
                            handled = true;
                            break;
                        case "SetUserShipName":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = JsonParsing.getString(data, "Ship");
                                string name = JsonParsing.getString(data, "UserShipName");
                                string ident = JsonParsing.getString(data, "UserShipId");

                                events.Add(new ShipRenamedEvent(timestamp, ship, shipId, name, ident) { raw = line });
                            }
                            handled = true;
                            break;
                        case "LaunchSRV":
                            {
                                string loadout = JsonParsing.getString(data, "Loadout");
                                bool playercontrolled = JsonParsing.getBool(data, "PlayerControlled");

                                events.Add(new SRVLaunchedEvent(timestamp, loadout, playercontrolled) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Music":
                            {
                                string musicTrack = JsonParsing.getString(data, "MusicTrack");
                                events.Add(new MusicEvent(timestamp, musicTrack) { raw = line });
                            }
                            handled = true;
                            break;                            
                        case "DockSRV":
                            events.Add(new SRVDockedEvent(timestamp) { raw = line });
                            handled = true;
                            break;
                        case "LaunchFighter":
                            {
                                string loadout = JsonParsing.getString(data, "Loadout");
                                bool playerControlled = JsonParsing.getBool(data, "PlayerControlled");
                                events.Add(new FighterLaunchedEvent(timestamp, loadout, playerControlled) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockFighter":
                            events.Add(new FighterDockedEvent(timestamp) { raw = line });
                            handled = true;
                            break;
                        case "SRVDestroyed":
                            {
                                string vehicle = "srv";
                                events.Add(new VehicleDestroyedEvent(timestamp, vehicle) { raw = line });
                                handled = true;
                            }
                            break;
                        case "FighterDestroyed":
                            {
                                string vehicle = "fighter";
                                events.Add(new VehicleDestroyedEvent(timestamp, vehicle) { raw = line });
                                handled = true;
                            }
                            break;
                        case "FighterRebuilt":
                            {
                                string loadout = JsonParsing.getString(data, "Loadout");
                                events.Add(new FighterRebuiltEvent(timestamp, loadout) { raw = line });
                                handled = true;
                            }
                            break;
                        case "VehicleSwitch":
                            {
                                string to = JsonParsing.getString(data, "To");
                                if (to == "Fighter")
                                {
                                    events.Add(new ControllingFighterEvent(timestamp) { raw = line });
                                    handled = true;
                                }
                                else if (to == "Mothership")
                                {
                                    events.Add(new ControllingShipEvent(timestamp) { raw = line });
                                    handled = true;
                                }
                            }
                            break;
                        case "Interdicted":
                            {
                                object val;
                                bool submitted = JsonParsing.getBool(data, "Submitted");
                                string interdictor = JsonParsing.getString(data, "Interdictor");
                                bool iscommander = JsonParsing.getBool(data, "IsPlayer");
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)val));
                                string faction = getFaction(data, "Faction");
                                string power = JsonParsing.getString(data, "Power");

                                events.Add(new ShipInterdictedEvent(timestamp, true, submitted, iscommander, interdictor, rating, faction, power) { raw = line });
                                handled = true;
                            }
                            break;
                        case "EscapeInterdiction":
                            {
                                string interdictor = JsonParsing.getString(data, "Interdictor");
                                bool iscommander = JsonParsing.getBool(data, "IsPlayer");

                                events.Add(new ShipInterdictedEvent(timestamp, false, false, iscommander, interdictor, null, null, null) { raw = line });
                                handled = true;
                            }
                            break;
                        case "Interdiction":
                            {
                                object val;
                                bool success = JsonParsing.getBool(data, "Success");
                                string interdictee = JsonParsing.getString(data, "Interdicted");
                                bool iscommander = JsonParsing.getBool(data, "IsPlayer");
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)val));
                                string faction = getFaction(data, "Faction");
                                string power = JsonParsing.getString(data, "Power");

                                events.Add(new ShipInterdictionEvent(timestamp, success, iscommander, interdictee, rating, faction, power) { raw = line });
                                handled = true;
                            }
                            break;
                        case "PVPKill":
                            {
                                object val;
                                string victim = JsonParsing.getString(data, "Victim");
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)val));

                                events.Add(new KilledEvent(timestamp, victim, rating) { raw = line });
                                handled = true;
                            }
                            break;
                        case "MaterialCollected":
                            {
                                object val;
                                Material material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                events.Add(new MaterialCollectedEvent(timestamp, material, amount) { raw = line });
                                handled = true;
                            }
                            break;
                        case "MaterialDiscarded":
                            {
                                object val;
                                Material material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                events.Add(new MaterialDiscardedEvent(timestamp, material, amount) { raw = line });
                                handled = true;
                            }
                            break;
                        case "MaterialDiscovered":
                            {
                                Material material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                events.Add(new MaterialDiscoveredEvent(timestamp, material) { raw = line });
                                handled = true;
                            }
                            break;
                        case "MaterialTrade":
                            {
                                object val;

                                long marketId = JsonParsing.getLong(data, "MarketID");
                                string traderType = JsonParsing.getString(data, "TraderType");

                                data.TryGetValue("Paid", out val);
                                Dictionary<string, object> paid = (Dictionary<string, object>)val;

                                string materialEdName = JsonParsing.getString(paid, "Material");
                                Material materialPaid = Material.FromEDName(materialEdName);
                                int materialPaidQty = JsonParsing.getInt(paid, "Quantity");

                                if (materialPaid == null)
                                {
                                    Logging.Info("Unknown material " + materialEdName);
                                    Logging.Report("Unknown material " + materialEdName, JsonConvert.SerializeObject(paid));
                                }

                                data.TryGetValue("Received", out val);
                                Dictionary<string, object> received = (Dictionary<string, object>)val;

                                Material materialReceived = Material.FromEDName(JsonParsing.getString(received, "Material"));
                                int materialReceivedQty = JsonParsing.getInt(received, "Quantity");

                                if (materialReceived == null)
                                {
                                    Logging.Info("Unknown material " + materialEdName);
                                    Logging.Report("Unknown material " + materialEdName, JsonConvert.SerializeObject(received));
                                }

                                events.Add(new MaterialTradedEvent(timestamp, marketId, traderType, materialPaid, materialPaidQty, materialReceived, materialReceivedQty) { raw = line });
                                handled = true;

                                break;
                            }
                        case "ScientificResearch":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                Material material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                events.Add(new MaterialDonatedEvent(timestamp, material, amount) { raw = line });
                                handled = true;
                            }
                            break;
                        case "StartJump":
                            {
                                string target = JsonParsing.getString(data, "JumpType");
                                string stellarclass = JsonParsing.getString(data, "StarClass");
                                string system = JsonParsing.getString(data, "StarSystem");
                                events.Add(new FSDEngagedEvent(timestamp, target, system, stellarclass) { raw = line });
                                handled = true;
                            }
                            break;
                        case "ReceiveText":
                            {
                                string from = JsonParsing.getString(data, "From");
                                string channel = JsonParsing.getString(data, "Channel");
                                string message = JsonParsing.getString(data, "Message");
                                string source = "";

                                if (
                                    channel == "player" ||
                                    channel == "wing" ||
                                    channel == "friend" ||
                                    channel == "voicechat" ||
                                    channel == "local" ||
                                    channel == null
                                )
                                {
                                    // Give priority to player messages
                                    source = channel == "wing" ? "Wing mate" : (channel == null ? "Crew mate" : "Commander");
                                    channel = channel == null ? "multicrew" : channel;
                                    events.Add(new MessageReceivedEvent(timestamp, from, source, true, channel, message) { raw = line });
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
                                    events.Add(new MessageReceivedEvent(timestamp, from, source, false, channel, JsonParsing.getString(data, "Message_Localised")));

                                    // See if we also want to spawn a specific event as well?
                                    if (message == "$STATION_NoFireZone_entered;")
                                    {
                                        events.Add(new StationNoFireZoneEnteredEvent(timestamp, false) { raw = line });
                                    }
                                    else if (message == "$STATION_NoFireZone_entered_deployed;")
                                    {
                                        events.Add(new StationNoFireZoneEnteredEvent(timestamp, true) { raw = line });
                                    }
                                    else if (message == "$STATION_NoFireZone_exited;")
                                    {
                                        events.Add(new StationNoFireZoneExitedEvent(timestamp) { raw = line });
                                    }
                                    else if (message.Contains("_StartInterdiction"))
                                    {
                                        // Find out who is doing the interdicting
                                        string by = npcSpeechBy(from, message);

                                        events.Add(new NPCInterdictionCommencedEvent(timestamp, by) { raw = line });
                                    }
                                    else if (message.Contains("_Attack") || message.Contains("_OnAttackStart") || message.Contains("AttackRun") || message.Contains("OnDeclarePiracyAttack"))
                                    {
                                        // Find out who is doing the attacking
                                        string by = npcSpeechBy(from, message);
                                        events.Add(new NPCAttackCommencedEvent(timestamp, by) { raw = line });
                                    }
                                    else if (message.Contains("_OnStartScanCargo"))
                                    {
                                        // Find out who is doing the scanning
                                        string by = npcSpeechBy(from, message);
                                        events.Add(new NPCCargoScanCommencedEvent(timestamp, by) { raw = line });
                                    }
                                }
                            }
                            handled = true;
                            break;
                        case "SendText":
                            {
                                string to = JsonParsing.getString(data, "To");
                                string message = JsonParsing.getString(data, "Message");
                                events.Add(new MessageSentEvent(timestamp, to, message) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingRequested":
                            {
                                string stationName = JsonParsing.getString(data, "StationName");
                                events.Add(new DockingRequestedEvent(timestamp, stationName) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingGranted":
                            {
                                object val;
                                string stationName = JsonParsing.getString(data, "StationName");
                                data.TryGetValue("LandingPad", out val);
                                int landingPad = (int)(long)val;
                                events.Add(new DockingGrantedEvent(timestamp, stationName, landingPad) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingDenied":
                            {
                                string stationName = JsonParsing.getString(data, "StationName");
                                string reason = JsonParsing.getString(data, "Reason");
                                events.Add(new DockingDeniedEvent(timestamp, stationName, reason) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingCancelled":
                            {
                                string stationName = JsonParsing.getString(data, "StationName");
                                events.Add(new DockingCancelledEvent(timestamp, stationName) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingTimeout":
                            {
                                string stationName = JsonParsing.getString(data, "StationName");
                                events.Add(new DockingTimedOutEvent(timestamp, stationName) { raw = line });
                            }
                            handled = true;
                            break;
                        case "MiningRefined":
                            {
                                string commodityName = JsonParsing.getString(data, "Type");

                                CommodityDefinition commodity = CommodityDefinition.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map commodityrefined type " + commodityName + " to commodity");
                                }
                                events.Add(new CommodityRefinedEvent(timestamp, commodity) { raw = line });
                            }
                            handled = true;
                            break;
                        case "HeatWarning":
                            events.Add(new HeatWarningEvent(timestamp) { raw = line });
                            handled = true;
                            break;
                        case "HeatDamage":
                            events.Add(new HeatDamageEvent(timestamp) { raw = line });
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

                                events.Add(new HullDamagedEvent(timestamp, vehicle, piloted, health) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShieldState":
                            {
                                bool shieldsUp = JsonParsing.getBool(data, "ShieldsUp");
                                if (shieldsUp == true)
                                {
                                    events.Add(new ShieldsUpEvent(timestamp) { raw = line });
                                }
                                else
                                {
                                    events.Add(new ShieldsDownEvent(timestamp) { raw = line });
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
                                events.Add(new ShipTargetedEvent(timestamp, targetlocked, ship, scanstage, name, rank, faction, legalStatus, bounty, shieldHealth, hullHealth, subSystem, subSystemHealth) { raw = line });
                            }
                            handled = true;
                            break;
                        case "UnderAttack":
                            {
                                string target = JsonParsing.getString(data, "Target");
                                events.Add(new UnderAttackEvent(timestamp, target) { raw = line });
                                handled = true;
                                break;
                            }
                        case "SelfDestruct":
                            events.Add(new SelfDestructEvent(timestamp) { raw = line });
                            handled = true;
                            break;
                        case "Died":
                            {
                                object val;

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
                                    data.TryGetValue("Killers", out val);
                                    List<object> killers = (List<object>)val;
                                    foreach (IDictionary<string, object> killer in killers)
                                    {
                                        names.Add(JsonParsing.getString(killer, "Name"));
                                        ships.Add(JsonParsing.getString(killer, "Ship"));
                                        ratings.Add(CombatRating.FromEDName(JsonParsing.getString(killer, "Rank")));
                                    }
                                }
                                events.Add(new DiedEvent(timestamp, names, ships, ratings) { raw = line });
                                handled = true;
                            }
                            break;
                        case "Resurrect":
                            {
                                string option = JsonParsing.getString(data, "Option");
                                long price = JsonParsing.getLong(data, "Cost");

                                if (option == "rebuy")
                                {
                                    events.Add(new ShipRepurchasedEvent(timestamp, price) { raw = line });
                                    handled = true;
                                }
                            }
                            break;
                        case "NavBeaconScan":
                            {
                                object val;
                                data.TryGetValue("NumBodies", out val);
                                int numbodies = (int)(long)val;
                                events.Add(new NavBeaconScanEvent(timestamp, numbodies) { raw = line });
                            }
                            handled = true;
                            break;
                        case "BuyExplorationData":
                            {
                                string system = JsonParsing.getString(data, "System");
                                long price = JsonParsing.getLong(data, "Cost");
                                events.Add(new ExplorationDataPurchasedEvent(timestamp, system, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "SellExplorationData":
                            {
                                object val;
                                data.TryGetValue("Systems", out val);
                                List<string> systems = ((List<object>)val).Cast<string>().ToList();
                                data.TryGetValue("Discovered", out val);
                                List<string> firsts = ((List<object>)val).Cast<string>().ToList();
                                data.TryGetValue("BaseValue", out val);
                                decimal reward = (long)val;
                                data.TryGetValue("Bonus", out val);
                                decimal bonus = (long)val;
                                events.Add(new ExplorationDataSoldEvent(timestamp, systems, firsts, reward, bonus) { raw = line });
                                handled = true;
                                break;
                            }
                        case "USSDrop":
                            {
                                object val;
                                string source = JsonParsing.getString(data, "USSType");
                                data.TryGetValue("USSThreat", out val);
                                int threat = (int)(long)val;
                                events.Add(new EnteredSignalSourceEvent(timestamp, source, threat) { raw = line });
                            }
                            handled = true;
                            break;
                        case "MarketBuy":
                            {
                                object val;
                                data.TryGetValue("MarketID", out val);
                                long marketid = (long)val;
                                string commodityName = JsonParsing.getString(data, "Type");
                                CommodityDefinition commodity = CommodityDefinition.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map marketbuy type " + commodityName + " to commodity");
                                }
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                data.TryGetValue("BuyPrice", out val);
                                int price = (int)(long)val;
                                events.Add(new CommodityPurchasedEvent(timestamp, marketid, commodity, amount, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MarketSell":
                            {
                                object val;
                                data.TryGetValue("MarketID", out val);
                                long marketid = (long)val;
                                string commodityName = JsonParsing.getString(data, "Type");
                                CommodityDefinition commodity = CommodityDefinition.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map marketsell type " + commodityName + " to commodity");
                                }
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                data.TryGetValue("SellPrice", out val);
                                long price = (long)val;
                                data.TryGetValue("AvgPricePaid", out val);
                                long buyPrice = (long)val;
                                // We don't care about buy price, we care about profit per unit
                                long profit = price - buyPrice;
                                bool? tmp = JsonParsing.getOptionalBool(data, "IllegalGoods");
                                bool illegal = tmp.HasValue ? (bool)tmp : false;
                                tmp = JsonParsing.getOptionalBool(data, "StolenGoods");
                                bool stolen = tmp.HasValue ? (bool)tmp : false;
                                tmp = JsonParsing.getOptionalBool(data, "BlackMarket");
                                bool blackmarket = tmp.HasValue ? (bool)tmp : false;
                                events.Add(new CommoditySoldEvent(timestamp, marketid, commodity, amount, price, profit, illegal, stolen, blackmarket) { raw = line });
                                handled = true;
                                break;
                            }
                        case "EngineerCraft":
                            {
                                object val;
                                string engineer = JsonParsing.getString(data, "Engineer");
                                string blueprint = JsonParsing.getString(data, "Blueprint");
                                data.TryGetValue("Level", out val);
                                int level = (int)(long)val;

                                List<CommodityAmount> commodities = new List<CommodityAmount>();
                                List<MaterialAmount> materials = new List<MaterialAmount>();
                                if (data.TryGetValue("Ingredients", out val))
                                {
                                    if (val is Dictionary<string, object>)
                                    {
                                        // 2.2 style
                                        Dictionary<string, object> usedData = (Dictionary<string, object>)val;
                                        foreach (KeyValuePair<string, object> used in usedData)
                                        {
                                            // Used could be a material or a commodity
                                            CommodityDefinition commodity = CommodityDefinition.FromName(used.Key);
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
                                    else if (val is List<object>)
                                    {
                                        // 2.3 style
                                        List<object> materialsJson = (List<object>)val;

                                        foreach (Dictionary<string, object> materialJson in materialsJson)
                                        {
                                            Material material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }
                                }
                                events.Add(new ModificationCraftedEvent(timestamp, engineer, blueprint, level, materials, commodities) { raw = line });
                                handled = true;
                                break;
                            }
                        case "EngineerApply":
                            {
                                object val;
                                string engineer = JsonParsing.getString(data, "Engineer");
                                string blueprint = JsonParsing.getString(data, "Blueprint");
                                data.TryGetValue("Level", out val);
                                int level = (int)(long)val;

                                events.Add(new ModificationAppliedEvent(timestamp, engineer, blueprint, level) { raw = line });
                                handled = true;
                                break;
                            }
                        case "EngineerProgress":
                            {
                                object val;
                                string engineer = JsonParsing.getString(data, "Engineer");
                                data.TryGetValue("Rank", out val);
                                if (val == null)
                                {
                                    // There are other non-rank events for engineers but we don't pay attention to them
                                    break;
                                }
                                int rank = (int)(long)val;

                                events.Add(new EngineerProgressedEvent(timestamp, engineer, rank) { raw = line });
                                handled = true;
                                break;
                            }
                        case "LoadGame":
                            {
                                object val;
                                string commander = JsonParsing.getString(data, "Commander");

                                data.TryGetValue("ShipID", out val);
                                int? shipId = (int?)(long?)val;

                                if (shipId == null)
                                {
                                    // This happens if we are in CQC.  Flag it back to EDDI so that it ignores everything that happens until
                                    // we're out of CQC again
                                    events.Add(new EnteredCQCEvent(timestamp, commander) { raw = line });
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

                                events.Add(new CommanderContinuedEvent(timestamp, commander, (int)shipId, ship, shipName, shipIdent, mode, group, credits, loan, fuel, fuelCapacity) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewHire":
                            {
                                string name = JsonParsing.getString(data, "Name");
                                string faction = getFaction(data, "Faction");
                                long price = JsonParsing.getLong(data, "Cost");
                                CombatRating rating = CombatRating.FromRank(JsonParsing.getInt(data, "CombatRank"));
                                events.Add(new CrewHiredEvent(timestamp, name, faction, price, rating) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewFire":
                            {
                                string name = JsonParsing.getString(data, "Name");
                                events.Add(new CrewFiredEvent(timestamp, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewAssign":
                            {
                                string name = JsonParsing.getString(data, "Name");
                                string role = getRole(data, "Role");
                                events.Add(new CrewAssignedEvent(timestamp, name, role) { raw = line });
                                handled = true;
                                break;
                            }
                        case "JoinACrew":
                            {
                                string captain = JsonParsing.getString(data, "Captain");
                                captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new CrewJoinedEvent(timestamp, captain) { raw = line });
                                handled = true;
                                break;
                            }
                        case "QuitACrew":
                            {
                                string captain = JsonParsing.getString(data, "Captain");
                                captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new CrewLeftEvent(timestamp, captain) { raw = line });
                                handled = true;
                                break;
                            }
                        case "ChangeCrewRole":
                            {
                                string role = getRole(data, "Role");
                                events.Add(new CrewRoleChangedEvent(timestamp, role) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewMemberJoins":
                            {
                                string member = JsonParsing.getString(data, "Crew");
                                member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new CrewMemberJoinedEvent(timestamp, member) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewMemberQuits":
                            {
                                string member = JsonParsing.getString(data, "Crew");
                                member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new CrewMemberLeftEvent(timestamp, member) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewLaunchFighter":
                            {
                                string name = JsonParsing.getString(data, "Crew");
                                events.Add(new CrewMemberLaunchedEvent(timestamp, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewMemberRoleChange":
                            {
                                string name = JsonParsing.getString(data, "Crew");
                                string role = getRole(data, "Role");
                                events.Add(new CrewMemberRoleChangedEvent(timestamp, name, role) { raw = line });
                                handled = true;
                                break;
                            }
                        case "KickCrewMember":
                            {
                                string member = JsonParsing.getString(data, "Crew");
                                member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new CrewMemberRemovedEvent(timestamp, member) { raw = line });
                                handled = true;
                                break;
                            }
                        case "BuyAmmo":
                            {
                                object val;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;
                                events.Add(new ShipRestockedEvent(timestamp, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "BuyDrones":
                            {
                                object val;
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                data.TryGetValue("BuyPrice", out val);
                                int price = (int)(long)val;
                                events.Add(new LimpetPurchasedEvent(timestamp, amount, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "SellDrones":
                            {
                                object val;
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                data.TryGetValue("SellPrice", out val);
                                int price = (int)(long)val;
                                events.Add(new LimpetSoldEvent(timestamp, amount, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "LaunchDrone":
                            {
                                string kind = JsonParsing.getString(data, "Type");
                                events.Add(new LimpetLaunchedEvent(timestamp, kind) { raw = line });
                                handled = true;
                                break;
                            }
                        case "ClearSavedGame":
                            {
                                string name = JsonParsing.getString(data, "Name");
                                events.Add(new ClearedSaveEvent(timestamp, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "NewCommander":
                            {
                                string name = JsonParsing.getString(data, "Name");
                                string package = JsonParsing.getString(data, "Package");
                                events.Add(new CommanderStartedEvent(timestamp, name, package) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Progress":
                            {
                                object val;
                                data.TryGetValue("Combat", out val);
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

                                events.Add(new CommanderProgressEvent(timestamp, combat, trade, exploration, cqc, empire, federation) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Rank":
                            {
                                object val;
                                data.TryGetValue("Combat", out val);
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

                                events.Add(new CommanderRatingsEvent(timestamp, combat, trade, exploration, cqc, empire, federation) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Screenshot":
                            {
                                object val;
                                string filename = JsonParsing.getString(data, "Filename");
                                data.TryGetValue("Width", out val);
                                int width = (int)(long)val;
                                data.TryGetValue("Height", out val);
                                int height = (int)(long)val;
                                string system = JsonParsing.getString(data, "System");
                                string body = JsonParsing.getString(data, "Body");
                                decimal? latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                decimal? longitude = JsonParsing.getOptionalDecimal(data, "Longitude");                                

                                events.Add(new ScreenshotEvent(timestamp, filename, width, height, system, body, longitude, latitude) { raw = line });
                                handled = true;
                                break;
                            }
                        case "BuyTradeData":
                            {
                                object val;
                                string system = JsonParsing.getString(data, "System");
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                events.Add(new TradeDataPurchasedEvent(timestamp, system, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PayFines":
                            {
                                object val;
                                data.TryGetValue("Amount", out val);
                                long amount = (long)val;
                                decimal? brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");

                                events.Add(new FinePaidEvent(timestamp, amount, brokerpercentage, false) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PayLegacyFines":
                            {
                                object val;
                                data.TryGetValue("Amount", out val);
                                long amount = (long)val;
                                decimal? brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");

                                events.Add(new FinePaidEvent(timestamp, amount, brokerpercentage, true) { raw = line });
                                handled = true;
                                break;
                            }
                        case "RefuelPartial":
                            {
                                object val;
                                decimal amount = JsonParsing.getDecimal(data, "Amount");
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null) { raw = line });
                                handled = true;
                                break;
                            }
                        case "RefuelAll":
                            {
                                object val;
                                decimal amount = JsonParsing.getDecimal(data, "Amount");
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null) { raw = line });
                                handled = true;
                                break;
                            }
                        case "FuelScoop":
                            {
                                decimal amount = JsonParsing.getDecimal(data, "Scooped");
                                decimal total = JsonParsing.getDecimal(data, "Total");

                                events.Add(new ShipRefuelledEvent(timestamp, "Scoop", null, amount, total) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Friends":
                            {
                                string status = JsonParsing.getString(data, "Status");                            
                                string name = JsonParsing.getString(data, "Name");
                                name = name.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                Friend cmdr = new Friend();
                                cmdr.name = name;
                                cmdr.status = status;

                                /// Does this friend exist in our friends list?
                                List<Friend> friends = EDDI.Instance.Cmdr.friends;
                                int index = friends.FindIndex(friend => friend.name == name);
                                if (index >= 0)
                                {
                                    if (friends[index].status != cmdr.status)
                                    {
                                        /// This is a known friend with a revised status: replace in situ (this is more efficient than removing and re-adding).
                                        friends[index] = cmdr;
                                        events.Add(new FriendsEvent(timestamp, name, status) { raw = line });
                                    }
                                }
                                else
                                {
                                    /// This is a new friend, add them to the list
                                    friends.Add(cmdr);
                                }

                                handled = true;
                                break;
                            }
                        case "JetConeBoost":
                            {
                                decimal boost = JsonParsing.getDecimal(data, "BoostValue");

                                events.Add(new JetConeBoostEvent(timestamp, boost) { raw = line });
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

                                events.Add(new JetConeDamageEvent(timestamp, modulename, module) { raw = line });
                                handled = true;
                                break;
                            }
                        case "RedeemVoucher":
                            {
                                object val;

                                string type = JsonParsing.getString(data, "Type");
                                List<Reward> rewards = new List<Reward>();

                                // Obtain list of factions
                                data.TryGetValue("Factions", out val);
                                List<object> factionsData = (List<object>)val;
                                if (factionsData != null)
                                {
                                    foreach (Dictionary<string, object> rewardData in factionsData)
                                    {
                                        string factionName = getFaction(rewardData, "Faction");
                                        rewardData.TryGetValue("Amount", out val);
                                        long factionReward = (long)val;

                                        rewards.Add(new Reward(factionName, factionReward));
                                    }
                                }
                                else
                                {
                                    string factionName = getFaction(data, "Faction");
                                    data.TryGetValue("Amount", out val);
                                    long factionReward = (long)val;

                                    rewards.Add(new Reward(factionName, factionReward));
                                }
                                data.TryGetValue("Amount", out val);
                                long amount = (long)val;

                                decimal? brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");

                                if (type == "bounty")
                                {
                                    events.Add(new BountyRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line });
                                }
                                else if (type == "CombatBond")
                                {
                                    events.Add(new BondRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line });
                                }
                                else if (type == "trade")
                                {
                                    events.Add(new TradeVoucherRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line });
                                }
                                else if (type == "settlement" || type == "scannable")
                                {
                                    events.Add(new DataVoucherRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line });
                                }
                                else
                                {
                                    Logging.Warn("Unhandled voucher type " + type);
                                    Logging.Report("Unhandled voucher type " + type);
                                }
                                handled = true;
                                break;
                            }
                        case "CommunityGoal":
                            {
                                object val;

                                // There may be multiple goals in each event. We add them all to lists
                                data.TryGetValue("CurrentGoals", out val);
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

                                events.Add(new CommunityGoalEvent(timestamp, cgid, name, system, station, expiry, iscomplete, total, contribution, contributors, percentileband, topranksize, toprank, tier, tierreward) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CommunityGoalJoin":
                            {
                                string name = JsonParsing.getString(data, "Name");
                                string system = JsonParsing.getString(data, "System");

                                events.Add(new MissionAcceptedEvent(timestamp, null, name, system, null, null, null, null, null, null, null, null, null, true, null, null, null) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CommunityGoalReward":
                            {
                                object val;
                                string name = JsonParsing.getString(data, "Name");
                                string system = JsonParsing.getString(data, "System");
                                data.TryGetValue("Reward", out val);
                                long reward = (val == null ? 0 : (long)val);

                                events.Add(new MissionCompletedEvent(timestamp, null, name, null, null, null, true, reward, null, 0) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MissionAccepted":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                data.TryGetValue("Expiry", out val);
                                DateTime? expiry = (val == null ? (DateTime?)null : (DateTime)val);
                                string name = JsonParsing.getString(data, "Name");
                                string faction = getFaction(data, "Faction");

                                // Missions with destinations
                                string destinationsystem = JsonParsing.getString(data, "DestinationSystem");
                                string destinationstation = JsonParsing.getString(data, "DestinationStation");

                                // Missions with commodities
                                CommodityDefinition commodity = CommodityDefinition.FromName(JsonParsing.getString(data, "Commodity"));
                                data.TryGetValue("Count", out val);
                                int? amount = (int?)(long?)val;

                                // Missions with targets
                                string target = JsonParsing.getString(data, "Target");
                                string targettype = JsonParsing.getString(data, "TargetType");
                                string targetfaction = getFaction(data, "TargetFaction");
                                data.TryGetValue("KillCount", out val);
                                if (val != null)
                                {
                                    amount = (int?)(long?)val;
                                }

                                // Missions with passengers
                                string passengertype = JsonParsing.getString(data, "PassengerType");
                                bool? passengerswanted = JsonParsing.getOptionalBool(data, "PassengerWanted");
                                data.TryGetValue("PassengerCount", out val);
                                if (val != null)
                                {
                                    amount = (int?)(long?)val;
                                }

                                // Impact on influence and reputation
                                string influence = JsonParsing.getString(data, "Influence");
                                string reputation = JsonParsing.getString(data, "Reputation");

                                events.Add(new MissionAcceptedEvent(timestamp, missionid, name, faction, destinationsystem, destinationstation, commodity, amount, passengertype, passengerswanted, target, targettype, targetfaction, false, expiry, influence, reputation) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MissionCompleted":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = JsonParsing.getString(data, "Name");
                                data.TryGetValue("Reward", out val);
                                long reward = (val == null ? 0 : (long)val);
                                data.TryGetValue("Donation", out val);
                                long donation = (val == null ? 0 : (long)val);
                                string faction = getFaction(data, "Faction");

                                // Missions with commodities
                                CommodityDefinition commodity = CommodityDefinition.FromName(JsonParsing.getString(data, "Commodity"));
                                data.TryGetValue("Count", out val);
                                int? amount = (int?)(long?)val;

                                List<CommodityAmount> commodityrewards = new List<CommodityAmount>();
                                data.TryGetValue("CommodityReward", out val);
                                List<object> commodityRewardsData = (List<object>)val;
                                if (commodityRewardsData != null)
                                {
                                    foreach (Dictionary<string, object> commodityRewardData in commodityRewardsData)
                                    {
                                        CommodityDefinition rewardCommodity = CommodityDefinition.FromName(JsonParsing.getString(commodityRewardData, "Name"));
                                        commodityRewardData.TryGetValue("Count", out val);
                                        int count = (int)(long)val;
                                        commodityrewards.Add(new CommodityAmount(rewardCommodity, count));
                                    }
                                }

                                events.Add(new MissionCompletedEvent(timestamp, missionid, name, faction, commodity, amount, false, reward, commodityrewards, donation) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MissionAbandoned":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = JsonParsing.getString(data, "Name");
                                events.Add(new MissionAbandonedEvent(timestamp, missionid, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MissionRedirected":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = JsonParsing.getString(data, "MissionName");
                                string newdestinationstation = JsonParsing.getString(data, "NewDestinationStation");  
                                string olddestinationstation = JsonParsing.getString(data, "OldDestinationStation");
                                string newdestinationsystem = JsonParsing.getString(data, "NewDestinationSystem");
                                string olddestinationsystem = JsonParsing.getString(data, "OldDestinationSystem");
                                events.Add(new MissionRedirectedEvent(timestamp, missionid, name, newdestinationstation, olddestinationstation, newdestinationsystem, olddestinationsystem) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MissionFailed":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = JsonParsing.getString(data, "Name");
                                events.Add(new MissionFailedEvent(timestamp, missionid, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "SearchAndRescue":
                            {
                                object val;
                                string commodityName = JsonParsing.getString(data, "Name");
                                CommodityDefinition commodity = CommodityDefinition.FromName(JsonParsing.getString(data, "Name"));
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map SearchAndRescue commodity type " + commodityName + " to commodity");
                                }
                                data.TryGetValue("Count", out val);
                                int? amount = (int?)(long?)val;
                                data.TryGetValue("Reward", out val);
                                long reward = (val == null ? 0 : (long)val);                                
                                events.Add(new SearchAndRescueEvent(timestamp, commodity, amount, reward) { raw = line });
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

                                events.Add(new ShipAfmuRepairedEvent(timestamp, item, repairedfully, health) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Repair":
                            {
                                object val;
                                string item = JsonParsing.getString(data, "Item");
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
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;
                                events.Add(new ShipRepairedEvent(timestamp, item, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "RepairDrone":
                            {
                                decimal? hull = JsonParsing.getOptionalDecimal(data, "HullRepaired");
                                decimal? cockpit = JsonParsing.getOptionalDecimal(data, "CockpitRepaired");
                                decimal? corrosion = JsonParsing.getOptionalDecimal(data, "CorrosionRepaired");

                                events.Add(new ShipRepairDroneEvent(timestamp, hull, cockpit, corrosion) { raw = line });
                                handled = true;
                                break;
                            }
                        case "RepairAll":
                            {
                                object val;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;
                                events.Add(new ShipRepairedEvent(timestamp, null, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "RebootRepair":
                            {
                                object val;
                                data.TryGetValue("Modules", out val);
                                List<object> modulesJson = (List<object>)val;

                                List<string> modules = new List<string>();
                                foreach (string module in modulesJson)
                                {
                                    modules.Add(module);
                                }
                                events.Add(new ShipRebootedEvent(timestamp, modules) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Synthesis":
                            {
                                object val;
                                string synthesis = JsonParsing.getString(data, "Name");

                                data.TryGetValue("Materials", out val);
                                List<MaterialAmount> materials = new List<MaterialAmount>();
                                if (val is Dictionary<string, object>)
                                {
                                    // 2.2 style
                                    Dictionary<string, object> materialsData = (Dictionary<string, object>)val;
                                    if (materialsData != null)
                                    {
                                        foreach (KeyValuePair<string, object> materialData in materialsData)
                                        {
                                            Material material = Material.FromEDName(materialData.Key);
                                            materials.Add(new MaterialAmount(material, (int)(long)materialData.Value));
                                        }
                                    }
                                }
                                else if (val is List<object>)
                                {
                                    // 2.3 style
                                    List<object> materialsJson = (List<object>)val;

                                    foreach (Dictionary<string, object> materialJson in materialsJson)
                                    {
                                        Material material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                        materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                    }
                                }

                                events.Add(new SynthesisedEvent(timestamp, synthesis, materials) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Materials":
                            {
                                object val;
                                List<MaterialAmount> materials = new List<MaterialAmount>();

                                data.TryGetValue("Raw", out val);
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

                                events.Add(new MaterialInventoryEvent(DateTime.Now, materials) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Cargo":
                            {
                                int cargocarried = 0;
                                object val;
                                List<Cargo> inventory = new List<Cargo>();

                                data.TryGetValue("Inventory", out val);
                                if (val != null)
                                {
                                    List<object> inventoryJson = (List<object>)val;
                                    foreach (Dictionary<string, object> cargoJson in inventoryJson)
                                    {
                                        string name = JsonParsing.getString(cargoJson, "Name");
                                        int amount = JsonParsing.getInt(cargoJson, "Count");
                                        cargocarried += amount;
                                        Cargo cargo = new Cargo(name, amount);
                                        cargo.haulage = 0;
                                        cargo.stolen = JsonParsing.getInt(cargoJson, "Stolen");
                                        cargo.owned = amount - cargo.stolen;
                                        inventory.Add(cargo);
                                    }
                                }

                                events.Add(new CargoInventoryEvent(DateTime.Now, inventory, cargocarried) { raw = line });
                            }
                            handled = true;
                            break;

                        case "PowerplayJoin":
                            {
                                string power = JsonParsing.getString(data, "Power");

                                events.Add(new PowerJoinedEvent(timestamp, power) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayLeave":
                            {
                                string power = JsonParsing.getString(data, "Power");

                                events.Add(new PowerLeftEvent(timestamp, power) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayDefect":
                            {
                                string frompower = JsonParsing.getString(data, "FromPower");
                                string topower = JsonParsing.getString(data, "ToPower");

                                events.Add(new PowerDefectedEvent(timestamp, frompower, topower) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayVote":
                            {
                                object val;
                                string power = JsonParsing.getString(data, "Power");
                                string system = JsonParsing.getString(data, "System");
                                data.TryGetValue("Votes", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerPreparationVoteCast(timestamp, power, system, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplaySalary":
                            {
                                object val;
                                string power = JsonParsing.getString(data, "Power");
                                data.TryGetValue("Amount", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerSalaryClaimedEvent(timestamp, power, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayCollect":
                            {
                                object val;
                                string power = JsonParsing.getString(data, "Power");
                                CommodityDefinition commodity = CommodityDefinition.FromName(JsonParsing.getString(data, "Type"));
                                commodity.fallbackLocalizedName = JsonParsing.getString(data, "Type_Localised");
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerCommodityObtainedEvent(timestamp, power, commodity, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayDeliver":
                            {
                                object val;
                                string power = JsonParsing.getString(data, "Power");
                                CommodityDefinition commodity = CommodityDefinition.FromName(JsonParsing.getString(data, "Type"));
                                commodity.fallbackLocalizedName = JsonParsing.getString(data, "Type_Localised");
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerCommodityDeliveredEvent(timestamp, power, commodity, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayFastTrack":
                            {
                                object val;
                                string power = JsonParsing.getString(data, "Power");
                                data.TryGetValue("Cost", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerCommodityFastTrackedEvent(timestamp, power, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayVoucher":
                            {
                                object val;
                                string power = JsonParsing.getString(data, "Power");
                                data.TryGetValue("Systems", out val);
                                List<string> systems = ((List<object>)val).Cast<string>().ToList();

                                events.Add(new PowerVoucherReceivedEvent(timestamp, power, systems) { raw = line });
                                handled = true;
                                break;
                            }
                        case "SystemsShutdown":
                            {
                                events.Add(new ShipShutdownEvent(timestamp) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Fileheader":
                            {
                                string filename = journalFileName;
                                string version = JsonParsing.getString(data, "gameversion");
                                string build = JsonParsing.getString(data, "build").Replace(" ", "");

                                events.Add(new FileHeaderEvent(timestamp, filename, version, build) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Shutdown":
                            {
                                events.Add(new ShutdownEvent(timestamp) { raw = line });
                                handled = true;
                                break;
                            }
                    }

                    if (!handled)
                    {
                        Logging.Debug("Unhandled event: " + line);

                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to parse line: " + ex.ToString());
                Logging.Error("Exception whilst parsing journal line", ex);
            }
            return events;
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
            return EddiJournalMonitor.Properties.JournalMonitor.name;
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return EddiJournalMonitor.Properties.JournalMonitor.desc;
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
            IntPtr path;
            int result = NativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out path);
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

        private static Superpower getAllegiance(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            // FD sends "" rather than null; fix that here
            if (((string)val) == "") { val = null; }
            return Superpower.From((string)val);
        }

        private static string getFaction(IDictionary<string, object> data, string key)
        {
            string faction = JsonParsing.getString(data, key);
            // Might be a superpower...
            Superpower superpowerFaction = Superpower.AllOfThem.FirstOrDefault(x => x.basename == faction);
            return superpowerFaction?.invariantName ?? faction;
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
    }
}
