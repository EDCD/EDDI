using Eddi;
using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiEvents;
using EddiShipMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Utilities;

namespace EddiJournalMonitor
{
    public class JournalMonitor : LogMonitor, EDDIMonitor
    {
        private static Regex JsonRegex = new Regex(@"^{.*}$");

        public JournalMonitor() : base(GetSavedGamesDir(), @"^Journal.*\.[0-9\.]+\.log$", result =>
        ForwardJournalEntry(result, EDDI.Instance.eventHandler)) { }

        public static void ForwardJournalEntry(string line, Action<Event> callback)
        {
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
                            timestamp = DateTime.Parse(getString(data, "timestamp")).ToUniversalTime();
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

                    string edType = getString(data, "event");
                    switch (edType)
                    {
                        case "Docked":
                            {
                                string systemName = getString(data, "StarSystem");
                                string stationName = getString(data, "StationName");
                                string stationModel = getString(data, "StationType");
                                Superpower allegiance = getAllegiance(data, "StationAllegiance");
                                string faction = getFaction(data, "StationFaction");
                                State factionState = State.FromEDName(getString(data, "FactionState"));
                                Economy economy = Economy.FromEDName(getString(data, "StationEconomy"));
                                Government government = Government.FromEDName(getString(data, "StationGovernment"));
                                decimal? distancefromstar = getOptionalDecimal(data, "DistFromStarLS");

                                // Get station services data
                                object val;
                                data.TryGetValue("StationServices", out val);
                                List<string> stationservices = (val as List<object>)?.Cast<string>()?.ToList();

                                events.Add(new DockedEvent(timestamp, systemName, stationName, stationModel, faction, factionState, economy, government, distancefromstar, stationservices) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Undocked":
                            {
                                string stationName = getString(data, "StationName");
                                events.Add(new UndockedEvent(timestamp, stationName) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Touchdown":
                            {
                                decimal? latitude = getOptionalDecimal(data, "Latitude");
                                decimal? longitude = getOptionalDecimal(data, "Longitude");
                                bool playercontrolled = getOptionalBool(data, "PlayerControlled") ?? true;
                                events.Add(new TouchdownEvent(timestamp, longitude, latitude, playercontrolled) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Liftoff":
                            {
                                decimal? latitude = getOptionalDecimal(data, "Latitude");
                                decimal? longitude = getOptionalDecimal(data, "Longitude");
                                bool playercontrolled = getOptionalBool(data, "PlayerControlled") ?? true;
                                events.Add(new LiftoffEvent(timestamp, longitude, latitude, playercontrolled) { raw = line });
                            }
                            handled = true;
                            break;
                        case "SupercruiseEntry":
                            {
                                string system = getString(data, "StarySystem");
                                events.Add(new EnteredSupercruiseEvent(timestamp, system) { raw = line });
                            }
                            handled = true;
                            break;
                        case "SupercruiseExit":
                            {
                                string system = getString(data, "StarSystem");
                                string body = getString(data, "Body");
                                string bodyType = getString(data, "BodyType");
                                events.Add(new EnteredNormalSpaceEvent(timestamp, system, body, bodyType) { raw = line });
                            }
                            handled = true;
                            break;
                        case "FSDJump":
                            {
                                object val;

                                string systemName = getString(data, "StarSystem");
                                data.TryGetValue("StarPos", out val);
                                List<object> starPos = (List<object>)val;
                                decimal x = Math.Round(getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                decimal y = Math.Round(getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                decimal z = Math.Round(getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;

                                decimal fuelUsed = getDecimal(data, "FuelUsed");
                                decimal fuelRemaining = getDecimal(data, "FuelLevel");
                                decimal distance = getDecimal(data, "JumpDist");
                                Superpower allegiance = getAllegiance(data, "SystemAllegiance");
                                string faction = getFaction(data, "SystemFaction");
                                State factionState = State.FromEDName(getString(data, "FactionState"));
                                Economy economy = Economy.FromEDName(getString(data, "SystemEconomy"));
                                Government government = Government.FromEDName(getString(data, "SystemGovernment"));
                                SecurityLevel security = SecurityLevel.FromEDName(getString(data, "SystemSecurity"));
                                long? population = getOptionalLong(data, "Population");

                                events.Add(new JumpedEvent(timestamp, systemName, x, y, z, distance, fuelUsed, fuelRemaining, allegiance, faction, factionState, economy, government, security, population) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Location":
                            {
                                object val;

                                string systemName = getString(data, "StarSystem");

                                if (systemName == "Training")
                                {
                                    // Training system; ignore
                                    break;
                                }

                                data.TryGetValue("StarPos", out val);
                                List<object> starPos = (List<object>)val;
                                decimal x = Math.Round(getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                decimal y = Math.Round(getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                decimal z = Math.Round(getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;

                                string body = getString(data, "Body");
                                string bodyType = getString(data, "BodyType");
                                bool docked = getBool(data, "Docked");
                                Superpower allegiance = getAllegiance(data, "SystemAllegiance");
                                string faction = getFaction(data, "SystemFaction");
                                Economy economy = Economy.FromEDName(getString(data, "SystemEconomy"));
                                Government government = Government.FromEDName(getString(data, "SystemGovernment"));
                                SecurityLevel security = SecurityLevel.FromEDName(getString(data, "SystemSecurity"));
                                long? population = getOptionalLong(data, "Population");

                                string station = getString(data, "StationName");
                                string stationtype = getString(data, "StationType");

                                decimal? latitude = getOptionalDecimal(data, "Latitude");
                                decimal? longitude = getOptionalDecimal(data, "Longitude");

                                events.Add(new LocationEvent(timestamp, systemName, x, y, z, body, bodyType, docked, station, stationtype, allegiance, faction, economy, government, security, population, longitude, latitude) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Bounty":
                            {
                                object val;

                                string target = getString(data, "Target");
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
                                string victimFaction = getString(data, "VictimFaction");

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
                                string crimetype = getString(data, "CrimeType");
                                string faction = getFaction(data, "Faction");
                                string victim = getString(data, "Victim");
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
                                    CombatRating rating = CombatRating.FromRank((int)(long)val);
                                    events.Add(new CombatPromotionEvent(timestamp, rating) { raw = line });
                                    handled = true;
                                }
                                else if (data.ContainsKey("Trade"))
                                {
                                    data.TryGetValue("Trade", out val);
                                    TradeRating rating = TradeRating.FromRank((int)(long)val);
                                    events.Add(new TradePromotionEvent(timestamp, rating) { raw = line });
                                    handled = true;
                                }
                                else if (data.ContainsKey("Explore"))
                                {
                                    data.TryGetValue("Explore", out val);
                                    ExplorationRating rating = ExplorationRating.FromRank((int)(long)val);
                                    events.Add(new ExplorationPromotionEvent(timestamp, rating) { raw = line });
                                    handled = true;
                                }
                            }
                            break;
                        case "CollectCargo":
                            {
                                string commodityName = getString(data, "Type");
                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map collectcargo type " + commodityName + " to commodity");
                                }
                                bool stolen = getBool(data, "Stolen");
                                events.Add(new CommodityCollectedEvent(timestamp, commodity, stolen) { raw = line });
                                handled = true;
                            }
                            handled = true;
                            break;
                        case "EjectCargo":
                            {
                                object val;
                                string commodityName = getString(data, "Type");
                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map ejectcargo type " + commodityName + " to commodity");
                                }
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                bool abandoned = getBool(data, "Abandoned");
                                events.Add(new CommodityEjectedEvent(timestamp, commodity, amount, abandoned) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Loadout":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "Ship");
                                string shipName = getString(data, "ShipName");
                                string shipIdent = getString(data, "ShipIdent");

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
                                        string slot = getString(moduleData, "Slot");
                                        string item = getString(moduleData, "Item");
                                        bool enabled = getBool(moduleData, "On");
                                        int priority = getInt(moduleData, "Priority");
                                        // Health is as 0->1 but we want 0->100, and to a sensible number of decimal places
                                        decimal health = getDecimal(moduleData, "Health") * 100;
                                        if (health < 5)
                                        {
                                            health = Math.Round(health, 1);
                                        }
                                        else
                                        {
                                            health = Math.Round(health);
                                        }
                                        long price = getLong(moduleData, "Value");

                                        // Ammunition
                                        int? clip = getOptionalInt(moduleData, "AmmoInClip");
                                        int? hopper = getOptionalInt(moduleData, "AmmoInHopper");

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

                                            Module module = ModuleDefinitions.fromEDName(item);
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
                                        else
                                        {
                                            // This is a compartment
                                            Compartment compartment = new Compartment() { name = slot };
                                            Module module = ModuleDefinitions.fromEDName(item);
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
                        case "ApproachSettlement":
                            {
                                object val;
                                string name = getString(data, "Name");
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
                                string name = getString(data, "BodyName");
                                decimal distancefromarrival = getDecimal(data, "DistanceFromArrivalLS");

                                // Belt
                                if (name.Contains("Belt Cluster"))
                                {

                                    events.Add(new BeltScannedEvent(timestamp, name, distancefromarrival) { raw = line });
                                    handled = true;
                                    break;
                                }

                                // Common items
                                decimal radius = getDecimal(data, "Radius");
                                decimal? orbitalperiod = getOptionalDecimal(data, "OrbitalPeriod");
                                decimal rotationperiod = getDecimal(data, "RotationPeriod");
                                decimal? semimajoraxis = getOptionalDecimal(data, "SemiMajorAxis");
                                decimal? eccentricity = getOptionalDecimal(data, "Eccentricity");
                                decimal? orbitalinclination = getOptionalDecimal(data, "OrbitalInclination");
                                decimal? periapsis = getOptionalDecimal(data, "Periapsis");

                                data.TryGetValue("Rings", out val);
                                List<object> ringsData = (List<object>)val;
                                List<Ring> rings = new List<Ring>();
                                if (ringsData != null)
                                {
                                    foreach (Dictionary<string, object> ringData in ringsData)
                                    {
                                        string ringName = getString(ringData, "Name");
                                        string ringComposition = Composition.FromEDName(getString(ringData, "RingClass")).name;
                                        decimal ringMass = getDecimal(ringData, "MassMT");
                                        decimal ringInnerRadius = getDecimal(ringData, "InnerRad");
                                        decimal ringOuterRadius = getDecimal(ringData, "OuterRad");

                                        rings.Add(new Ring(ringName, ringComposition, ringMass, ringInnerRadius, ringOuterRadius));
                                    }
                                }

                                if (data.ContainsKey("StarType"))
                                {
                                    // Star
                                    string starType = getString(data, "StarType");
                                    decimal stellarMass = getDecimal(data, "StellarMass");
                                    decimal absoluteMagnitude = getDecimal(data, "AbsoluteMagnitude");
                                    string luminosityClass = getString(data, "Luminosity");
                                    data.TryGetValue("Age_MY", out val);
                                    long age = (long)val * 1000000;
                                    decimal temperature = getDecimal(data, "SurfaceTemperature");

                                    events.Add(new StarScannedEvent(timestamp, name, starType, stellarMass, radius, absoluteMagnitude, luminosityClass, age, temperature, distancefromarrival, orbitalperiod, rotationperiod, semimajoraxis, eccentricity, orbitalinclination, periapsis, rings) { raw = line });
                                    handled = true;
                                }
                                else
                                {
                                    // Body
                                    bool? tidallyLocked = getOptionalBool(data, "TidalLock");

                                    string bodyClass = getString(data, "PlanetClass");
                                    decimal? earthMass = getOptionalDecimal(data, "MassEM");

                                    // MKW: Gravity in the Journal is in m/s; must convert it to G
                                    decimal gravity = Body.ms2g(getDecimal(data, "SurfaceGravity"));

                                    decimal? temperature = getOptionalDecimal(data, "SurfaceTemperature");

                                    decimal? pressure = getOptionalDecimal(data, "SurfacePressure");

                                    bool? landable = getOptionalBool(data, "Landable");

                                    string reserves = getString(data, "ReserveLevel");

                                    decimal? axialTilt = getOptionalDecimal(data, "AxialTilt");

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
                                                    materials.Add(new MaterialPresence(material, getDecimal("Amount", kv.Value)));
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
                                                materials.Add(new MaterialPresence(material, getDecimal(materialJson, "Percent")));
                                            }
                                        }
                                    }

                                    string terraformState = getString(data, "TerraformState");
                                    string atmosphere = getString(data, "Atmosphere");
                                    Volcanism volcanism = Volcanism.FromName(getString(data, "Volcanism"));

                                    events.Add(new BodyScannedEvent(timestamp, name, bodyClass, earthMass, radius, gravity, temperature, pressure, tidallyLocked, landable, atmosphere, volcanism, distancefromarrival, (decimal)orbitalperiod, rotationperiod, semimajoraxis, eccentricity, orbitalinclination, periapsis, rings, reserves, materials, terraformState, axialTilt) { raw = line });
                                    handled = true;
                                }
                            }
                            break;
                        case "DatalinkScan":
                            {
                                string message = getString(data, "Message");
                                events.Add(new DatalinkMessageEvent(timestamp, message) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DataScanned":
                            {
                                DataScan datalinktype = DataScan.FromEDName(getString(data, "Type"));
                                events.Add(new DataScannedEvent(timestamp, datalinktype) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardBuy":
                            {
                                object val;
                                // We don't have a ship ID at this point so use the ship type
                                string ship = getString(data, "ShipType");

                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;

                                data.TryGetValue("StoreShipID", out val);
                                int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                string storedShip = getString(data, "StoreOldShip");

                                data.TryGetValue("SellShipID", out val);
                                int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                string soldShip = getString(data, "SellOldShip");

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
                                string ship = getString(data, "ShipType");

                                events.Add(new ShipDeliveredEvent(timestamp, ship, shipId) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardSell":
                            {
                                object val;
                                data.TryGetValue("SellShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "ShipType");
                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;
                                string system = getString(data, "System");                                
                                events.Add(new ShipSoldEvent(timestamp, ship, shipId, price, system) { raw = line });
                            }
                            handled = true;
                            break;
                        case "SellShipOnRebuy":
                            {
                                object val;
                                data.TryGetValue("SellShipId", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "ShipType");
                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;
                                string system = getString(data, "System");
                                events.Add(new ShipSoldOnRebuyEvent(timestamp, ship, shipId, price, system) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardSwap":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "ShipType");

                                data.TryGetValue("StoreShipID", out val);
                                int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                string storedShip = getString(data, "StoreOldShip");

                                data.TryGetValue("SellShipID", out val);
                                int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                string soldShip = getString(data, "SellOldShip");

                                events.Add(new ShipSwappedEvent(timestamp, ship, shipId, soldShip, soldShipId, storedShip, storedShipId) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ShipyardTransfer":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "ShipType");

                                string system = getString(data, "System");
                                decimal distance = getDecimal(data, "Distance");
                                data.TryGetValue("TransferPrice", out val);
                                long price = (long)val;
                                data.TryGetValue("TransferTime", out val);
                                long time = (long)val;

                                events.Add(new ShipTransferInitiatedEvent(timestamp, ship, shipId, system, distance, price, time) { raw = line });
                            }
                            handled = true;
                            break;
                        case "FetchRemoteModule":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "Ship");

                                Module module = ModuleDefinitions.fromEDName(getString(data, "StoredItem"));
                                data.TryGetValue("TransferCost", out val);
                                long transferCost = (long)val;
                                long? transferTime = getOptionalLong(data, "TransferTime");

                                // Probably not useful. We'll get these but we won't tell the end user about them
                                data.TryGetValue("StorageSlot", out val);
                                int storageSlot = (int)(long)val;
                                data.TryGetValue("ServerId", out val);
                                long serverId = (long)val;

                                events.Add(new ModuleTransferEvent(timestamp, ship, shipId, storageSlot, serverId, module, transferCost, transferTime) { raw = line });
                            }
                            handled = true;
                            break;
                        case "MassModuleStore":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "Ship");

                                data.TryGetValue("Items", out val);
                                List<object> items = (List<object>)val;

                                List<string> slots = new List<string>();
                                List<Module> modules = new List<Module>();

                                Module module = new Module();
                                if (items != null)
                                {

                                    foreach (Dictionary<string, object> item in items)
                                    {
                                        string slot = getString(item, "Slot");
                                        slots.Add(slot);

                                        module = ModuleDefinitions.fromEDName(getString(item, "Name"));
                                        module.modified = getString(item, "EngineerModifications") != null;
                                        modules.Add(module);
                                    }
                                }

                                events.Add(new ModulesStoredEvent(timestamp, ship, shipId, slots, modules) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleBuy":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "Ship");

                                string slot = getString(data, "Slot");
                                Module buyModule = ModuleDefinitions.fromEDName(getString(data, "BuyItem"));
                                data.TryGetValue("BuyPrice", out val);
                                long buyPrice = (long)val;
                                buyModule.price = buyPrice;

                                // Set retrieved module defaults
                                buyModule.enabled = true;
                                buyModule.priority = 1;
                                buyModule.health = 100;
                                buyModule.modified = false;

                                Module sellModule = ModuleDefinitions.fromEDName(getString(data, "SellItem"));
                                long? sellPrice = getOptionalLong(data, "SellPrice");
                                Module storedModule = ModuleDefinitions.fromEDName(getString(data, "StoredItem"));

                                events.Add(new ModulePurchasedEvent(timestamp, ship, shipId, slot, buyModule, buyPrice, sellModule, sellPrice, storedModule) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleRetrieve":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "Ship");

                                string slot = getString(data, "Slot");
                                Module module = ModuleDefinitions.fromEDName(getString(data, "RetrievedItem"));
                                data.TryGetValue("Cost", out val);
                                long? cost = getOptionalLong(data, "Cost");
                                string engineerModifications = getString(data, "EngineerModifications");
                                module.modified = engineerModifications != null;

                                // Set retrieved module defaults
                                module.price = module.value;
                                module.enabled = true;
                                module.priority = 1;
                                module.health = 100;

                                Module swapoutModule = ModuleDefinitions.fromEDName(getString(data, "SwapOutItem"));

                                events.Add(new ModuleRetrievedEvent(timestamp, ship, shipId, slot, module, cost, engineerModifications, swapoutModule) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleSell":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "Ship");

                                string slot = getString(data, "Slot");
                                Module module = ModuleDefinitions.fromEDName(getString(data, "SellItem"));
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
                                string ship = getString(data, "Ship");

                                Module module = ModuleDefinitions.fromEDName(getString(data, "SellItem"));
                                data.TryGetValue("SellPrice", out val);
                                long price = (long)val;

                                // Probably not useful. We'll get these but we won't tell the end user about them
                                data.TryGetValue("StorageSlot", out val);
                                int storageSlot = (int)(long)val;
                                data.TryGetValue("ServerId", out val);
                                long serverId = (long)val;

                                events.Add(new ModuleSoldFromStorage(timestamp, ship, shipId, storageSlot, serverId, module, price) { raw = line });
                            }
                            handled = true;
                            break;
                        case "ModuleStore":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "Ship");

                                string slot = getString(data, "Slot");
                                Module module = ModuleDefinitions.fromEDName(getString(data, "StoredItem"));
                                string engineerModifications = getString(data, "EngineerModifications");
                                module.modified = engineerModifications != null;
                                data.TryGetValue("Cost", out val);
                                long? cost = getOptionalLong(data, "Cost");


                                Module replacementModule = ModuleDefinitions.fromEDName(getString(data, "ReplacementItem"));
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
                                string ship = getString(data, "Ship");

                                string fromSlot = getString(data, "FromSlot");
                                Module fromModule = ModuleDefinitions.fromEDName(getString(data, "FromItem"));
                                string toSlot = getString(data, "ToSlot");
                                Module toModule = ModuleDefinitions.fromEDName(getString(data, "ToItem"));

                                events.Add(new ModuleSwappedEvent(timestamp, ship, shipId, fromSlot, fromModule, toSlot, toModule) { raw = line });
                            }
                            handled = true;
                            break;
                        case "SetUserShipName":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string ship = getString(data, "Ship");
                                string name = getString(data, "UserShipName");
                                string ident = getString(data, "UserShipId");

                                events.Add(new ShipRenamedEvent(timestamp, ship, shipId, name, ident) { raw = line });
                            }
                            handled = true;
                            break;
                        case "LaunchSRV":
                            {
                                string loadout = getString(data, "Loadout");
                                bool playercontrolled = getBool(data, "PlayerControlled");

                                events.Add(new SRVLaunchedEvent(timestamp, loadout, playercontrolled) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Music":
                            {
                                string musicTrack = getString(data, "MusicTrack");
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
                                string loadout = getString(data, "Loadout");
                                bool playerControlled = getBool(data, "PlayerControlled");
                                events.Add(new FighterLaunchedEvent(timestamp, loadout, playerControlled) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockFighter":
                            events.Add(new FighterDockedEvent(timestamp) { raw = line });
                            handled = true;
                            break;
                        case "VehicleSwitch":
                            {
                                string to = getString(data, "To");
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
                                bool submitted = getBool(data, "Submitted");
                                string interdictor = getString(data, "Interdictor");
                                bool iscommander = getBool(data, "IsPlayer");
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));
                                string faction = getFaction(data, "Faction");
                                string power = getString(data, "Power");

                                events.Add(new ShipInterdictedEvent(timestamp, true, submitted, iscommander, interdictor, rating, faction, power) { raw = line });
                                handled = true;
                            }
                            break;
                        case "EscapeInterdiction":
                            {
                                string interdictor = getString(data, "Interdictor");
                                bool iscommander = getBool(data, "IsPlayer");

                                events.Add(new ShipInterdictedEvent(timestamp, false, false, iscommander, interdictor, null, null, null) { raw = line });
                                handled = true;
                            }
                            break;
                        case "Interdiction":
                            {
                                object val;
                                bool success = getBool(data, "Success");
                                string interdictee = getString(data, "Interdicted");
                                bool iscommander = getBool(data, "IsPlayer");
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));
                                string faction = getFaction(data, "Faction");
                                string power = getString(data, "Power");

                                events.Add(new ShipInterdictionEvent(timestamp, success, iscommander, interdictee, rating, faction, power) { raw = line });
                                handled = true;
                            }
                            break;
                        case "PVPKill":
                            {
                                object val;
                                string victim = getString(data, "Victim");
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));

                                events.Add(new KilledEvent(timestamp, victim, rating) { raw = line });
                                handled = true;
                            }
                            break;
                        case "MaterialCollected":
                            {
                                object val;
                                Material material = Material.FromEDName(getString(data, "Name"));
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                events.Add(new MaterialCollectedEvent(timestamp, material, amount) { raw = line });
                                handled = true;
                            }
                            break;
                        case "MaterialDiscarded":
                            {
                                object val;
                                Material material = Material.FromEDName(getString(data, "Name"));
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                events.Add(new MaterialDiscardedEvent(timestamp, material, amount) { raw = line });
                                handled = true;
                            }
                            break;
                        case "MaterialDiscovered":
                            {
                                Material material = Material.FromEDName(getString(data, "Name"));
                                events.Add(new MaterialDiscoveredEvent(timestamp, material) { raw = line });
                                handled = true;
                            }
                            break;
                        case "ScientificResearch":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                Material material = Material.FromEDName(getString(data, "Name"));
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                events.Add(new MaterialDonatedEvent(timestamp, material, amount) { raw = line });
                                handled = true;
                            }
                            break;
                        case "StartJump":
                            {
                                string target = getString(data, "JumpType");
                                string stellarclass = getString(data, "StarClass");
                                string system = getString(data, "StarSystem");
                                events.Add(new FSDEngagedEvent(timestamp, target, system, stellarclass) { raw = line });
                                handled = true;
                            }
                            break;
                        case "ReceiveText":
                            {
                                string from = getString(data, "From");
                                string channel = getString(data, "Channel");
                                string message = getString(data, "Message");

                                if (!(from.StartsWith("$cmdr") || from.StartsWith("&")))
                                {
                                    // This is NPC speech.  What's the source?
                                    string source;
                                    if (from.Contains("npc_name_decorate"))
                                    {
                                        source = npcSpeechBy(from, message);
                                        from = from.Replace("$npc_name_decorate:#name=", "").Replace(";", "");
                                    }
                                    else if (from.Contains("ShipName_"))
                                    {
                                        source = npcSpeechBy(from, message);
                                        from = getString(data, "From_Localised");
                                    }
                                    else if ((message.Contains("STATION_")) || message.Contains("$Docking"))
                                    {
                                        source = "Station";
                                    }
                                    else
                                    {
                                        source = "NPC";
                                    }
                                    events.Add(new MessageReceivedEvent(timestamp, from, source, false, channel, getString(data, "Message_Localised")));
                                    // See if we want to spawn a specific event as well
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
                                else
                                {
                                    // Various sources
                                    string source;
                                    if (from.Contains("$RolePanel"))
                                    {
                                        source = "Crew member";
                                        from = from.Replace("$RolePanel1_crew; $cmdr_decorate:#name=", "Crew member ");
                                        from = from.Replace("$RolePanel1_unmanned; $cmdr_decorate:#name=", "Crew member ");
                                        from = from.Replace("$RolePanel2_crew; $cmdr_decorate:#name=", "Crew member ");
                                        from = from.Replace("$RolePanel2_unmanned; $cmdr_decorate:#name=", "Crew member ");
                                    }
                                    else
                                    {
                                        source = "Commander";
                                        from = from.Replace("$cmdr_decorate:#name=", "").Replace("&", "");
                                    }
                                    from = from.Replace(";", "");
                                    events.Add(new MessageReceivedEvent(timestamp, from, source, true, channel, message) { raw = line });
                                }
                            }
                            handled = true;
                            break;
                        case "SendText":
                            {
                                string to = getString(data, "To");
                                to = to.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                string message = getString(data, "Message");
                                events.Add(new MessageSentEvent(timestamp, to, message) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingRequested":
                            {
                                string stationName = getString(data, "StationName");
                                events.Add(new DockingRequestedEvent(timestamp, stationName) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingGranted":
                            {
                                object val;
                                string stationName = getString(data, "StationName");
                                data.TryGetValue("LandingPad", out val);
                                int landingPad = (int)(long)val;
                                events.Add(new DockingGrantedEvent(timestamp, stationName, landingPad) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingDenied":
                            {
                                string stationName = getString(data, "StationName");
                                string reason = getString(data, "Reason");
                                events.Add(new DockingDeniedEvent(timestamp, stationName, reason) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingCancelled":
                            {
                                string stationName = getString(data, "StationName");
                                events.Add(new DockingCancelledEvent(timestamp, stationName) { raw = line });
                            }
                            handled = true;
                            break;
                        case "DockingTimeout":
                            {
                                string stationName = getString(data, "StationName");
                                events.Add(new DockingTimedOutEvent(timestamp, stationName) { raw = line });
                            }
                            handled = true;
                            break;
                        case "MiningRefined":
                            {
                                string commodityName = getString(data, "Type");

                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
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
                                decimal health = sensibleHealth(getDecimal(data, "Health") * 100);
                                bool? piloted = getOptionalBool(data, "PlayerPilot");
                                bool? fighter = getOptionalBool(data, "Fighter");

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
                                bool shieldsUp = getBool(data, "ShieldsUp");
                                if (shieldsUp == true)
                                {
                                    events.Add(new ShieldsUpEvent(timestamp) { raw = line });
                                }
                                else
                                {
                                    events.Add(new ShieldsDownEvent(timestamp) { raw = line });
                                }
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
                                    names.Add(getString(data, "KillerName"));
                                    ships.Add(getString(data, "KillerShip"));
                                    ratings.Add(CombatRating.FromEDName(getString(data, "KillerRank")));
                                }
                                if (data.ContainsKey("killers"))
                                {
                                    // Multiple killers
                                    data.TryGetValue("Killers", out val);
                                    List<object> killers = (List<object>)val;
                                    foreach (IDictionary<string, object> killer in killers)
                                    {
                                        names.Add(getString(killer, "Name"));
                                        ships.Add(getString(killer, "Ship"));
                                        ratings.Add(CombatRating.FromEDName(getString(killer, "Rank")));
                                    }
                                }
                                events.Add(new DiedEvent(timestamp, names, ships, ratings) { raw = line });
                                handled = true;
                            }
                            break;
                        case "Resurrect":
                            {
                                string option = getString(data, "Option");
                                long price = getLong(data, "Cost");

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
                                string system = getString(data, "System");
                                long price = getLong(data, "Cost");
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
                                string source = getString(data, "USSType");
                                data.TryGetValue("USSThreat", out val);
                                int threat = (int)(long)val;
                                events.Add(new EnteredSignalSourceEvent(timestamp, source, threat) { raw = line });
                            }
                            handled = true;
                            break;
                        case "MarketBuy":
                            {
                                object val;
                                string commodityName = getString(data, "Type");
                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map marketbuy type " + commodityName + " to commodity");
                                }
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                data.TryGetValue("BuyPrice", out val);
                                long price = (long)val;
                                events.Add(new CommodityPurchasedEvent(timestamp, commodity, amount, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MarketSell":
                            {
                                object val;
                                string commodityName = getString(data, "Type");
                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
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
                                bool? tmp = getOptionalBool(data, "IllegalGoods");
                                bool illegal = tmp.HasValue ? (bool)tmp : false;
                                tmp = getOptionalBool(data, "StolenGoods");
                                bool stolen = tmp.HasValue ? (bool)tmp : false;
                                tmp = getOptionalBool(data, "BlackMarket");
                                bool blackmarket = tmp.HasValue ? (bool)tmp : false;
                                events.Add(new CommoditySoldEvent(timestamp, commodity, amount, price, profit, illegal, stolen, blackmarket) { raw = line });
                                handled = true;
                                break;
                            }
                        case "EngineerCraft":
                            {
                                object val;
                                string engineer = getString(data, "Engineer");
                                string blueprint = getString(data, "Blueprint");
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
                                            Commodity commodity = CommodityDefinitions.FromName(used.Key);
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
                                            Material material = Material.FromEDName(getString(materialJson, "Name"));
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
                                string engineer = getString(data, "Engineer");
                                string blueprint = getString(data, "Blueprint");
                                data.TryGetValue("Level", out val);
                                int level = (int)(long)val;

                                events.Add(new ModificationAppliedEvent(timestamp, engineer, blueprint, level) { raw = line });
                                handled = true;
                                break;
                            }
                        case "EngineerProgress":
                            {
                                object val;
                                string engineer = getString(data, "Engineer");
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
                                string commander = getString(data, "Commander");

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

                                string ship = getString(data, "Ship");
                                string shipName = getString(data, "ShipName");
                                string shipIdent = getString(data, "ShipIdent");

                                GameMode mode = GameMode.FromEDName(getString(data, "GameMode"));
                                string group = getString(data, "Group");
                                data.TryGetValue("Credits", out val);
                                decimal credits = (long)val;
                                data.TryGetValue("Loan", out val);
                                decimal loan = (long)val;
                                decimal? fuel = getOptionalDecimal(data, "FuelLevel");
                                decimal? fuelCapacity = getOptionalDecimal(data, "FuelCapacity");

                                events.Add(new CommanderContinuedEvent(timestamp, commander, (int)shipId, ship, shipName, shipIdent, mode, group, credits, loan, fuel, fuelCapacity) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewHire":
                            {
                                object val;
                                string name = getString(data, "Name");
                                string faction = getFaction(data, "Faction");
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = CombatRating.FromRank((int)(long)val);
                                events.Add(new CrewHiredEvent(timestamp, name, faction, price, rating) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewFire":
                            {
                                string name = getString(data, "Name");
                                events.Add(new CrewFiredEvent(timestamp, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewAssign":
                            {
                                string name = getString(data, "Name");
                                string role = getRole(data, "Role");
                                events.Add(new CrewAssignedEvent(timestamp, name, role) { raw = line });
                                handled = true;
                                break;
                            }
                        case "JoinACrew":
                            {
                                string captain = getString(data, "Captain");
                                captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new CrewJoinedEvent(timestamp, captain) { raw = line });
                                handled = true;
                                break;
                            }
                        case "QuitACrew":
                            {
                                string captain = getString(data, "Captain");
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
                                string member = getString(data, "Crew");
                                member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new CrewMemberJoinedEvent(timestamp, member) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewMemberQuits":
                            {
                                string member = getString(data, "Crew");
                                member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new CrewMemberLeftEvent(timestamp, member) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewLaunchFighter":
                            {
                                string name = getString(data, "Crew");
                                events.Add(new CrewMemberLaunchedEvent(timestamp, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CrewMemberRoleChange":
                            {
                                string name = getString(data, "Crew");
                                string role = getRole(data, "Role");
                                events.Add(new CrewMemberRoleChangedEvent(timestamp, name, role) { raw = line });
                                handled = true;
                                break;
                            }
                        case "KickCrewMember":
                            {
                                string member = getString(data, "Crew");
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
                                long price = (long)val;
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
                                long price = (long)val;
                                events.Add(new LimpetSoldEvent(timestamp, amount, price) { raw = line });
                                handled = true;
                                break;
                            }
                        case "ClearSavedGame":
                            {
                                string name = getString(data, "Name");
                                events.Add(new ClearedSaveEvent(timestamp, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "NewCommander":
                            {
                                string name = getString(data, "Name");
                                string package = getString(data, "Package");
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
                                string filename = getString(data, "Filename");
                                data.TryGetValue("Width", out val);
                                int width = (int)(long)val;
                                data.TryGetValue("Height", out val);
                                int height = (int)(long)val;
                                string system = getString(data, "System");
                                string body = getString(data, "Body");
                                decimal? latitude = getOptionalDecimal(data, "Latitude");
                                decimal? longitude = getOptionalDecimal(data, "Longitude");                                

                                events.Add(new ScreenshotEvent(timestamp, filename, width, height, system, body, longitude, latitude) { raw = line });
                                handled = true;
                                break;
                            }
                        case "BuyTradeData":
                            {
                                object val;
                                string system = getString(data, "System");
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
                                decimal? brokerpercentage = getOptionalDecimal(data, "BrokerPercentage");

                                events.Add(new FinePaidEvent(timestamp, amount, brokerpercentage, false) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PayLegacyFines":
                            {
                                object val;
                                data.TryGetValue("Amount", out val);
                                long amount = (long)val;
                                decimal? brokerpercentage = getOptionalDecimal(data, "BrokerPercentage");

                                events.Add(new FinePaidEvent(timestamp, amount, brokerpercentage, true) { raw = line });
                                handled = true;
                                break;
                            }
                        case "RefuelPartial":
                            {
                                object val;
                                decimal amount = getDecimal(data, "Amount");
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null) { raw = line });
                                handled = true;
                                break;
                            }
                        case "RefuelAll":
                            {
                                object val;
                                decimal amount = getDecimal(data, "Amount");
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null) { raw = line });
                                handled = true;
                                break;
                            }
                        case "FuelScoop":
                            {
                                decimal amount = getDecimal(data, "Scooped");
                                decimal total = getDecimal(data, "Total");

                                events.Add(new ShipRefuelledEvent(timestamp, "Scoop", null, amount, total) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Friends":
                            {
                                string status = getString(data, "Status");                            
                                string friend = getString(data, "Name");
                                friend = friend.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                events.Add(new FriendsEvent(timestamp, status, friend) { raw = line });
                                handled = true;
                                break;
                            }                            
                        case "RedeemVoucher":
                            {
                                object val;

                                string type = getString(data, "Type");
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
                                data.TryGetValue("Amount", out val);
                                long amount = (long)val;

                                decimal? brokerpercentage = getOptionalDecimal(data, "BrokerPercentage");

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
                                List<DateTime> expiry = new List<DateTime>();
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
                                    cgid.Add(getLong(goaldata, "CGID"));
                                    name.Add(getString(goaldata, "Title"));
                                    system.Add(getString(goaldata, "SystemName"));
                                    station.Add(getString(goaldata, "MarketName"));
                                    goaldata.TryGetValue("Expiry", out val);
                                    expiry.Add((DateTime)val);
                                    iscomplete.Add(getBool(goaldata, "IsComplete"));
                                    total.Add(getInt(goaldata, "CurrentTotal"));
                                    contribution.Add(getInt(goaldata, "PlayerContribution"));
                                    contributors.Add(getInt(goaldata, "NumContributors"));
                                    percentileband.Add(getDecimal(goaldata, "PlayerPercentileBand"));

                                    // If the community goal is constructed with a fixed-size top rank (ie max reward for top 10 players)

                                    topranksize.Add(getOptionalInt(goaldata, "TopRankSize"));
                                    toprank.Add(getOptionalBool(goaldata, "PlayerInTopRank"));

                                    // If the community goal has reached the first success tier

                                    goaldata.TryGetValue("TierReached", out val);
                                    tier.Add((string)val);
                                    tierreward.Add(getOptionalLong(goaldata, "Bonus"));

                                    events.Add(new CommunityGoalEvent(timestamp, cgid, name, system, station, expiry, iscomplete, total, contribution, contributors, percentileband, topranksize, toprank, tier, tierreward) { raw = line });
                                }
                                handled = true;
                                break;
                            }
                        case "CommunityGoalJoin":
                            {
                                string name = getString(data, "Name");
                                string system = getString(data, "System");

                                events.Add(new MissionAcceptedEvent(timestamp, null, name, system, null, null, null, null, null, null, null, null, null, true, null, null, null) { raw = line });
                                handled = true;
                                break;
                            }
                        case "CommunityGoalReward":
                            {
                                object val;
                                string name = getString(data, "Name");
                                string system = getString(data, "System");
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
                                string name = getString(data, "Name");
                                string faction = getFaction(data, "Faction");

                                // Missions with destinations
                                string destinationsystem = getString(data, "DestinationSystem");
                                string destinationstation = getString(data, "DestinationStation");

                                // Missions with commodities
                                Commodity commodity = CommodityDefinitions.FromName(getString(data, "Commodity"));
                                data.TryGetValue("Count", out val);
                                int? amount = (int?)(long?)val;

                                // Missions with targets
                                string target = getString(data, "Target");
                                string targettype = getString(data, "TargetType");
                                string targetfaction = getFaction(data, "TargetFaction");
                                data.TryGetValue("KillCount", out val);
                                if (val != null)
                                {
                                    amount = (int?)(long?)val;
                                }

                                // Missions with passengers
                                string passengertype = getString(data, "PassengerType");
                                bool? passengerswanted = getOptionalBool(data, "PassengerWanted");
                                data.TryGetValue("PassengerCount", out val);
                                if (val != null)
                                {
                                    amount = (int?)(long?)val;
                                }

                                // Impact on influence and reputation
                                string influence = getString(data, "Influence");
                                string reputation = getString(data, "Reputation");

                                events.Add(new MissionAcceptedEvent(timestamp, missionid, name, faction, destinationsystem, destinationstation, commodity, amount, passengertype, passengerswanted, target, targettype, targetfaction, false, expiry, influence, reputation) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MissionCompleted":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = getString(data, "Name");
                                data.TryGetValue("Reward", out val);
                                long reward = (val == null ? 0 : (long)val);
                                data.TryGetValue("Donation", out val);
                                long donation = (val == null ? 0 : (long)val);
                                string faction = getFaction(data, "Faction");

                                // Missions with commodities
                                Commodity commodity = CommodityDefinitions.FromName(getString(data, "Commodity"));
                                data.TryGetValue("Count", out val);
                                int? amount = (int?)(long?)val;

                                List<CommodityAmount> commodityrewards = new List<CommodityAmount>();
                                data.TryGetValue("CommodityReward", out val);
                                List<object> commodityRewardsData = (List<object>)val;
                                if (commodityRewardsData != null)
                                {
                                    foreach (Dictionary<string, object> commodityRewardData in commodityRewardsData)
                                    {
                                        Commodity rewardCommodity = CommodityDefinitions.FromName(getString(commodityRewardData, "Name"));
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
                                string name = getString(data, "Name");
                                events.Add(new MissionAbandonedEvent(timestamp, missionid, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MissionRedirected":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = getString(data, "MissionName");
                                string newdestinationstation = getString(data, "NewDestinationStation");  
                                string olddestinationstation = getString(data, "OldDestinationStation");
                                string newdestinationsystem = getString(data, "NewDestinationSystem");
                                string olddestinationsystem = getString(data, "OldDestinationSystem");
                                events.Add(new MissionRedirectedEvent(timestamp, missionid, name, newdestinationstation, olddestinationstation, newdestinationsystem, olddestinationsystem) { raw = line });
                                handled = true;
                                break;
                            }
                        case "MissionFailed":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = getString(data, "Name");
                                events.Add(new MissionFailedEvent(timestamp, missionid, name) { raw = line });
                                handled = true;
                                break;
                            }
                        case "SearchAndRescue":
                            {
                                object val;
                                string name = getString(data, "Name");
                                data.TryGetValue("Count", out val);
                                int? amount = (int?)(long?)val;
                                data.TryGetValue("Reward", out val);
                                long reward = (val == null ? 0 : (long)val);                                
                                events.Add(new SearchAndRescueEvent(timestamp, name, amount, reward) { raw = line });
                                handled = true;
                                break;
                            }
                        case "AfmuRepairs":
                            {
                                string item = getString(data, "Module");
                                // Item might be a module
                                Module module = ModuleDefinitions.fromEDName(item);
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
                                        item = "" + module.@class.ToString() + module.grade + " " + mount + " " + module.name;
                                    }
                                    else
                                    {
                                        item = module.name;
                                    }
                                }

                                bool repairedfully = getBool(data, "FullyRepaired");
                                decimal health = getDecimal(data, "Health");

                                events.Add(new ShipAfmuRepairedEvent(timestamp, item, repairedfully, health) { raw = line });
                                handled = true;
                                break;
                            }
                        case "Repair":
                            {
                                object val;
                                string item = getString(data, "Item");
                                // Item might be a module
                                Module module = ModuleDefinitions.fromEDName(item);
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
                                        item = "" + module.@class.ToString() + module.grade + " " + mount + " " + module.name;
                                    }
                                    else
                                    {
                                        item = module.name;
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
                                decimal? hull = getOptionalDecimal(data, "HullRepaired");
                                decimal? cockpit = getOptionalDecimal(data, "CockpitRepaired");
                                decimal? corrosion = getOptionalDecimal(data, "CorrosionRepaired");

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
                                string synthesis = getString(data, "Name");

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
                                        Material material = Material.FromEDName(getString(materialJson, "Name"));
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
                                        Material material = Material.FromEDName(getString(materialJson, "Name"));
                                        material.category = "Element";
                                        materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                    }
                                }

                                data.TryGetValue("Manufactured", out val);
                                if (val != null)
                                {
                                    List<object> materialsJson = (List<object>)val;
                                    foreach (Dictionary<string, object> materialJson in materialsJson)
                                    {
                                        Material material = Material.FromEDName(getString(materialJson, "Name"));
                                        material.category = "Manufactured";
                                        materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                    }
                                }

                                data.TryGetValue("Encoded", out val);
                                if (val != null)
                                {
                                    List<object> materialsJson = (List<object>)val;
                                    foreach (Dictionary<string, object> materialJson in materialsJson)
                                    {
                                        Material material = Material.FromEDName(getString(materialJson, "Name"));
                                        material.category = "Data";
                                        materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                    }
                                }

                                events.Add(new MaterialInventoryEvent(DateTime.Now, materials) { raw = line });
                            }
                            handled = true;
                            break;
                        case "Cargo":
                            {
                                object val;
                                List<Cargo> inventory = new List<Cargo>();

                                data.TryGetValue("Inventory", out val);
                                if (val != null)
                                {
                                    List<object> inventoryJson = (List<object>)val;
                                    foreach (Dictionary<string, object> cargoJson in inventoryJson)
                                    {
                                        Cargo cargo = new Cargo();
                                        cargo.commodity = CommodityDefinitions.FromName(getString(cargoJson, "Name"));
                                        cargo.amount = getInt(cargoJson, "Count");
                                        inventory.Add(cargo);
                                    }
                                }

                                events.Add(new CargoInventoryEvent(DateTime.Now, inventory) { raw = line });
                            }
                            handled = true;
                            break;
                        case "PowerplayJoin":
                            {
                                string power = getString(data, "Power");

                                events.Add(new PowerJoinedEvent(timestamp, power) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayLeave":
                            {
                                string power = getString(data, "Power");

                                events.Add(new PowerLeftEvent(timestamp, power) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayDefect":
                            {
                                string frompower = getString(data, "FromPower");
                                string topower = getString(data, "ToPower");

                                events.Add(new PowerDefectedEvent(timestamp, frompower, topower) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayVote":
                            {
                                object val;
                                string power = getString(data, "Power");
                                string system = getString(data, "System");
                                data.TryGetValue("Votes", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerPreparationVoteCast(timestamp, power, system, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplaySalary":
                            {
                                object val;
                                string power = getString(data, "Power");
                                data.TryGetValue("Amount", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerSalaryClaimedEvent(timestamp, power, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayCollect":
                            {
                                object val;
                                string power = getString(data, "Power");
                                // Currently using localised information as we don't have commodity definitions for all powerplay commodities
                                string commodity = getString(data, "Type_Localised");
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerCommodityObtainedEvent(timestamp, power, commodity, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayDeliver":
                            {
                                object val;
                                string power = getString(data, "Power");
                                // Currently using localised information as we don't have commodity definitions for all powerplay commodities
                                string commodity = getString(data, "Type_Localised");
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerCommodityDeliveredEvent(timestamp, power, commodity, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayFastTrack":
                            {
                                object val;
                                string power = getString(data, "Power");
                                data.TryGetValue("Cost", out val);
                                int amount = (int)(long)val;

                                events.Add(new PowerCommodityFastTrackedEvent(timestamp, power, amount) { raw = line });
                                handled = true;
                                break;
                            }
                        case "PowerplayVoucher":
                            {
                                object val;
                                string power = getString(data, "Power");
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
                                string version = getString(data, "gameversion");
                                string build = getString(data, "build").Replace(" ", "");

                                events.Add(new FileHeaderEvent(timestamp, filename, version, build) { raw = line });
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
                Logging.Error("Exception whilst parsing line", line);
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

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return "Monitor Elite: Dangerous' journal.log for many common events.  This should not be disabled unless you are sure you know what you are doing, as it will result in many functions inside EDDI no longer working";
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

        // Helpers for parsing json
        private static decimal getDecimal(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getDecimal(key, val);
        }

        private static decimal getDecimal(string key, object val)
        {
            if (val == null)
            {
                throw new ArgumentNullException("Expected value for " + key + " not present");
            }
            if (val is long)
            {
                return (long)val;
            }
            else if (val is double)
            {
                return (decimal)(double)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        private static decimal? getOptionalDecimal(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getOptionalDecimal(key, val);
        }

        private static decimal? getOptionalDecimal(string key, object val)
        {
            if (val == null)
            {
                return null;
            }
            else if (val is long)
            {
                return (long?)val;
            }
            else if (val is double)
            {
                return (decimal?)(double?)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        private static int getInt(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getInt(key, val);
        }

        private static int getInt(string key, object val)
        {
            if (val is long)
            {
                return (int)(long)val;
            }
            else if (val is int)
            {
                return (int)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        private static int? getOptionalInt(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getOptionalInt(key, val);
        }

        private static int? getOptionalInt(string key, object val)
        {
            if (val == null)
            {
                return null;
            }
            else if (val is long)
            {
                return (int?)(long?)val;
            }
            else if (val is int)
            {
                return (int?)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        private static long getLong(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getLong(key, val);
        }

        private static long getLong(string key, object val)
        {
            if (val is long)
            {
                return (long)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        private static long? getOptionalLong(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getOptionalLong(key, val);
        }

        private static long? getOptionalLong(string key, object val)
        {
            if (val == null)
            {
                return null;
            }
            else if (val is long)
            {
                return (int?)(long?)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        private static bool getBool(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getBool(key, val);
        }

        private static bool getBool(string key, object val)
        {
            if (val == null)
            {
                throw new ArgumentNullException("Expected value for " + key + " not present");
            }
            return (bool)val;
        }

        private static bool? getOptionalBool(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getOptionalBool(key, val);
        }

        private static bool? getOptionalBool(string key, object val)
        {
            return (bool?)val;
        }

        private static string getString(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return (string)val;
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
            string faction = getString(data, key);
            // Might be a superpower...
            Superpower superpowerFaction = Superpower.From(faction);
            return superpowerFaction != null ? superpowerFaction.name : faction;
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
            string role = getString(data, key);
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
