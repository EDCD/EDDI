using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiNetLogMonitor;
using Newtonsoft.Json;
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

        public JournalMonitor() : base(GetSavedGamesDir(), @"^Journal\.[0-9\.]+\.log$", result => ForwardJournalEntry(result, EDDI.Instance.eventHandler)) { }

        public static void ForwardJournalEntry(string line, Action<Event> callback)
        {
            Event theEvent = ParseJournalEntry(line);
            if (theEvent != null)
            {
                callback(theEvent);
            }
        }

        public static Event ParseJournalEntry(string line)
        {
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
                            timestamp = DateTime.Parse((string)data["timestamp"]).ToUniversalTime();
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

                    Event journalEvent = null;
                    string edType = (string)data["event"];
                    switch (edType)
                    {
                        case "Docked":
                            {
                                object val;
                                data.TryGetValue("StarSystem", out val);
                                string systemName = (string)val;
                                data.TryGetValue("StationName", out val);
                                string stationName = (string)val;
                                data.TryGetValue("StationType", out val);
                                string stationModel = (string)val;
                                data.TryGetValue("StationAllegiance", out val);
                                // FD sends "" rather than null; fix that here
                                if (((string)val) == "") { val = null; }
                                Superpower allegiance = Superpower.From((string)val);
                                data.TryGetValue("StationFaction", out val);
                                string faction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.From(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("FactionState", out val);
                                State factionState = State.FromEDName((string)val);
                                data.TryGetValue("StationEconomy", out val);
                                Economy economy = Economy.FromEDName((string)val);
                                data.TryGetValue("StationGovernment", out val);
                                Government government = Government.FromEDName((string)val);
                                //data.TryGetValue("Security", out val);
                                //SecurityLevel securityLevel = SecurityLevel.FromEDName((string)val);
                                journalEvent = new DockedEvent(timestamp, systemName, stationName, stationModel, faction, factionState, economy, government);
                            }
                            handled = true;
                            break;
                        case "Undocked":
                            {
                                object val;
                                data.TryGetValue("StationName", out val);
                                string stationName = (string)val;
                                journalEvent = new UndockedEvent(timestamp, stationName);
                            }
                            handled = true;
                            break;
                        case "Touchdown":
                            {
                                object val;
                                data.TryGetValue("Latitude", out val);
                                decimal latitude = (decimal)(double)val;
                                data.TryGetValue("Longitude", out val);
                                decimal longitude = (decimal)(double)val;
                                journalEvent = new TouchdownEvent(timestamp, longitude, latitude);
                            }
                            handled = true;
                            break;
                        case "Liftoff":
                            {
                                object val;
                                data.TryGetValue("Latitude", out val);
                                decimal latitude = (decimal)(double)val;
                                data.TryGetValue("Longitude", out val);
                                decimal longitude = (decimal)(double)val;
                                journalEvent = new LiftoffEvent(timestamp, longitude, latitude);
                            }
                            handled = true;
                            break;
                        case "SupercruiseEntry":
                            {
                                object val;
                                data.TryGetValue("StarSystem", out val);
                                string system = (string)val;
                                journalEvent = new EnteredSupercruiseEvent(timestamp, system);
                            }
                            handled = true;
                            break;
                        case "SupercruiseExit":
                            {
                                object val;
                                data.TryGetValue("StarSystem", out val);
                                string system = (string)val;
                                data.TryGetValue("Body", out val);
                                string body = (string)val;
                                data.TryGetValue("BodyType", out val);
                                string bodyType = (string)val;
                                journalEvent = new EnteredNormalSpaceEvent(timestamp, system, body, bodyType);
                            }
                            handled = true;
                            break;
                        case "FSDJump":
                            {
                                object val;

                                data.TryGetValue("StarSystem", out val);
                                string systemName = (string)val;
                                data.TryGetValue("StarPos", out val);
                                List<object> starPos = (List<object>)val;
                                decimal x = Math.Round((decimal)((double)starPos[0]) * 32) / (decimal)32.0;
                                decimal y = Math.Round((decimal)((double)starPos[1]) * 32) / (decimal)32.0;
                                decimal z = Math.Round((decimal)((double)starPos[2]) * 32) / (decimal)32.0;

                                data.TryGetValue("FuelUsed", out val);
                                decimal fuelUsed = (decimal)(double)val;

                                data.TryGetValue("FuelLevel", out val);
                                decimal fuelRemaining = (decimal)(double)val;

                                data.TryGetValue("JumpDist", out val);
                                decimal distance = (decimal)(double)val;

                                data.TryGetValue("SystemAllegiance", out val);
                                // FD sends "" rather than null; fix that here
                                if (((string)val) == "") { val = null; }
                                Superpower allegiance = Superpower.From((string)val);
                                data.TryGetValue("SystemFaction", out val);
                                string faction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.From(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("FactionState", out val);
                                State factionState = State.FromEDName((string)val);
                                data.TryGetValue("SystemEconomy", out val);
                                Economy economy = Economy.FromEDName((string)val);
                                data.TryGetValue("SystemGovernment", out val);
                                Government government = Government.FromEDName((string)val);
                                data.TryGetValue("SystemSecurity", out val);
                                SecurityLevel security = SecurityLevel.FromEDName((string)val);

                                journalEvent = new JumpedEvent(timestamp, systemName, x, y, z, distance, fuelUsed, fuelRemaining, allegiance, faction, factionState, economy, government, security);
                            }
                            handled = true;
                            break;
                        case "Location":
                            {
                                object val;

                                data.TryGetValue("StarSystem", out val);
                                string systemName = (string)val;

                                if (systemName == "Training")
                                {
                                    // Training system; ignore
                                    break;
                                }

                                data.TryGetValue("StarPos", out val);
                                List<object> starPos = (List<object>)val;
                                decimal x = Math.Round((decimal)((double)starPos[0]) * 32) / (decimal)32.0;
                                decimal y = Math.Round((decimal)((double)starPos[1]) * 32) / (decimal)32.0;
                                decimal z = Math.Round((decimal)((double)starPos[2]) * 32) / (decimal)32.0;

                                data.TryGetValue("Body", out val);
                                string body = (string)val;
                                data.TryGetValue("BodyType", out val);
                                string bodyType = (string)val;
                                data.TryGetValue("Docked", out val);
                                bool docked = (bool)val;
                                data.TryGetValue("SystemAllegiance", out val);
                                // FD sends "" rather than null; fix that here
                                if (((string)val) == "") { val = null; }
                                Superpower allegiance = Superpower.From((string)val);
                                data.TryGetValue("SystemFaction", out val);
                                string faction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.From(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("SystemEconomy", out val);
                                Economy economy = Economy.FromEDName((string)val);
                                data.TryGetValue("SystemGovernment", out val);
                                Government government = Government.FromEDName((string)val);
                                data.TryGetValue("SystemSecurity", out val);
                                SecurityLevel security = SecurityLevel.FromEDName((string)val);

                                data.TryGetValue("StationName", out val);
                                string station = (string)val;
                                data.TryGetValue("StationType", out val);
                                string stationtype = (string)val;

                                journalEvent = new LocationEvent(timestamp, systemName, x, y, z, body, bodyType, docked, station, stationtype, allegiance, faction, economy, government, security);
                            }
                            handled = true;
                            break;
                        case "Bounty":
                            {
                                object val;

                                data.TryGetValue("Target", out val);
                                string target = (string)val;
                                if (target != null)
                                {
                                    // Target might be a ship, but if not then the string we provide is repopulated in ship.model so use it regardless
                                    Ship ship = ShipDefinitions.FromEDModel(target);
                                    target = ship.model;
                                }

                                data.TryGetValue("VictimFaction", out val);
                                string victimFaction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.From(victimFaction);
                                victimFaction = superpowerFaction != null ? superpowerFaction.name : victimFaction;

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
                                    data.TryGetValue("Faction", out val);
                                    string factionName = (string)val;
                                    // Might be a superpower...
                                    superpowerFaction = Superpower.From(factionName);
                                    factionName = superpowerFaction != null ? superpowerFaction.name : factionName;
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
                                            rewardData.TryGetValue("Faction", out val);
                                            string factionName = (string)val;
                                            // Might be a superpower...
                                            superpowerFaction = Superpower.From(factionName);
                                            factionName = superpowerFaction != null ? superpowerFaction.name : factionName;

                                            rewardData.TryGetValue("Reward", out val);
                                            long factionReward = (long)val;

                                            rewards.Add(new Reward(factionName, factionReward));
                                        }
                                    }
                                }

                                journalEvent = new BountyAwardedEvent(timestamp, target, victimFaction, reward, rewards, shared);
                            }
                            handled = true;
                            break;
                        case "CapShipBond":
                        case "FactionKillBond":
                            {
                                object val;
                                data.TryGetValue("Faction", out val);
                                string awardingFaction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.From(awardingFaction);
                                awardingFaction = superpowerFaction != null ? superpowerFaction.name : awardingFaction;
                                data.TryGetValue("Reward", out val);
                                long reward = (long)val;
                                data.TryGetValue("VictimFaction", out val);
                                string victimFaction = (string)val;

                                journalEvent = new BondAwardedEvent(timestamp, awardingFaction, victimFaction, reward);
                            }
                            handled = true;
                            break;
                        case "CommitCrime":
                            {
                                object val;
                                data.TryGetValue("CrimeType", out val);
                                string crimetype = (string)val;
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.From(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("Victim", out val);
                                string victim = (string)val;
                                // Might be a fine or a bounty
                                if (data.ContainsKey("Fine"))
                                {
                                    data.TryGetValue("Fine", out val);
                                    long fine = (long)val;
                                    journalEvent = new FineIncurredEvent(timestamp, crimetype, faction, victim, fine);
                                }
                                else
                                {
                                    data.TryGetValue("Bounty", out val);
                                    long bounty = (long)val;
                                    journalEvent = new BountyIncurredEvent(timestamp, crimetype, faction, victim, bounty);
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
                                    journalEvent = new CombatPromotionEvent(timestamp, rating);
                                    handled = true;
                                }
                                else if (data.ContainsKey("Trade"))
                                {
                                    data.TryGetValue("Trade", out val);
                                    TradeRating rating = TradeRating.FromRank((int)(long)val);
                                    journalEvent = new TradePromotionEvent(timestamp, rating);
                                    handled = true;
                                }
                                else if (data.ContainsKey("Explore"))
                                {
                                    data.TryGetValue("Explore", out val);
                                    ExplorationRating rating = ExplorationRating.FromRank((int)(long)val);
                                    journalEvent = new ExplorationPromotionEvent(timestamp, rating);
                                    handled = true;
                                }
                            }
                            break;
                        case "CollectCargo":
                            {
                                object val;
                                data.TryGetValue("Type", out val);
                                string commodityName = (string)val;
                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map collectcargo type " + commodityName + " to commodity");
                                }
                                data.TryGetValue("Stolen", out val);
                                bool stolen = (bool)val;
                                journalEvent = new CommodityCollectedEvent(timestamp, commodity, stolen);
                                handled = true;
                            }
                            handled = true;
                            break;
                        case "EjectCargo":
                            {
                                object val;
                                data.TryGetValue("Type", out val);
                                string commodityName = (string)val;
                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map ejectcargo type " + commodityName + " to commodity");
                                }
                                string cargo = (string)val;
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                data.TryGetValue("Abandoned", out val);
                                bool abandoned = (bool)val;
                                journalEvent = new CommodityEjectedEvent(timestamp, commodity, amount, abandoned);
                                handled = true;
                            }
                            handled = true;
                            break;
                        case "CockpitBreached":
                            journalEvent = new CockpitBreachedEvent(timestamp);
                            handled = true;
                            break;
                        case "Scan":
                            {
                                object val;
                                // Common items
                                data.TryGetValue("BodyName", out val);
                                string name = (string)val;

                                data.TryGetValue("DistanceFromArrivalLS", out val);
                                decimal distancefromarrival = (decimal)(double)val;

                                data.TryGetValue("Radius", out val);
                                decimal radius = (decimal)(double)val;

                                data.TryGetValue("OrbitalPeriod", out val);
                                decimal? orbitalperiod = (decimal?)(double?)val;

                                data.TryGetValue("RotationPeriod", out val);
                                decimal rotationperiod = (decimal)(double)val;

                                data.TryGetValue("SemiMajorAxis", out val);
                                decimal? semimajoraxis = (decimal?)(double?)val;

                                data.TryGetValue("Eccentricity", out val);
                                decimal? eccentricity = (decimal?)(double?)val;

                                data.TryGetValue("OrbitalInclination", out val);
                                decimal? orbitalinclination = (decimal?)(double?)val;

                                data.TryGetValue("Periapsis", out val);
                                decimal? periapsis = (decimal?)(double?)val;

                                data.TryGetValue("Rings", out val);
                                List<object> ringsData = (List<object>)val;
                                List<Ring> rings = new List<Ring>();
                                if (ringsData != null)
                                {
                                    foreach (Dictionary<string, object> ringData in ringsData)
                                    {
                                        ringData.TryGetValue("Name", out val);
                                        string ringName = (string)val;

                                        ringData.TryGetValue("RingClass", out val);
                                        Composition ringComposition = Composition.FromEDName((string)val);

                                        ringData.TryGetValue("MassMT", out val);
                                        decimal ringMass = (decimal)(double)val;

                                        ringData.TryGetValue("InnerRad", out val);
                                        decimal ringInnerRadius = (decimal)(double)val;

                                        ringData.TryGetValue("OuterRad", out val);
                                        decimal ringOuterRadius = (decimal)(double)val;

                                        rings.Add(new Ring(ringName, ringComposition, ringMass, ringInnerRadius, ringOuterRadius));
                                    }
                                }


                                if (data.ContainsKey("StarType"))
                                {
                                    // Star
                                    data.TryGetValue("StarType", out val);
                                    string starType = (string)val;

                                    data.TryGetValue("StellarMass", out val);
                                    decimal stellarMass = (decimal)(double)val;

                                    data.TryGetValue("AbsoluteMagnitude", out val);
                                    decimal absoluteMagnitude = (decimal)(double)val;

                                    data.TryGetValue("Age_MY", out val);
                                    long age = (long)val * 1000000;

                                    data.TryGetValue("SurfaceTemperature", out val);
                                    decimal temperature = (decimal)(double)val;

                                    journalEvent = new StarScannedEvent(timestamp, name, starType, stellarMass, radius, absoluteMagnitude, age, temperature, distancefromarrival, orbitalperiod, rotationperiod, semimajoraxis, eccentricity, orbitalinclination, periapsis, rings);
                                    handled = true;
                                }
                                else
                                {
                                    // Body
                                    data.TryGetValue("TidalLock", out val);
                                    bool tidallyLocked = (bool)val;

                                    data.TryGetValue("PlanetClass", out val);
                                    string bodyClass = (string)val;

                                    // MKW: Gravity in the Journal is in m/s; must convert it to G
                                    data.TryGetValue("SurfaceGravity", out val);
                                    decimal gravity = Body.ms2g((decimal)(double)val);

                                    data.TryGetValue("SurfaceTemperature", out val);
                                    decimal temperature = (decimal)(double)val;

                                    data.TryGetValue("SurfacePressure", out val);
                                    decimal pressure = (decimal)(double)val;

                                    data.TryGetValue("Landable", out val);
                                    bool landable = (bool)val;

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
                                                    materials.Add(new MaterialPresence(material, (decimal)(double)kv.Value));
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
                                                materials.Add(new MaterialPresence(material, (decimal)(double)materialJson["Percent"]));
                                            }
                                        }
                                    }

                                    data.TryGetValue("TerraformState", out val);
                                    string terraformState = (string)val;

                                    // Atmosphere
                                    data.TryGetValue("Atmosphere", out val);
                                    string atmosphere = (string)val;

                                    // Volcanism
                                    data.TryGetValue("Volcanism", out val);
                                    string volcanism = (string)val;

                                    journalEvent = new BodyScannedEvent(timestamp, name, bodyClass, gravity, temperature, pressure, tidallyLocked, landable, atmosphere, volcanism, distancefromarrival, (decimal)orbitalperiod, rotationperiod, semimajoraxis, eccentricity, orbitalinclination, periapsis, rings, materials, terraformState);
                                    handled = true;
                                }
                            }
                            break;
                        case "ShipyardBuy":
                            {
                                object val;
                                // We don't have a ship ID at this point so use the ship type
                                data.TryGetValue("ShipType", out val);
                                string shipModel = (string)val;
                                Ship ship = findShip(null, shipModel);

                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;

                                data.TryGetValue("StoreShipID", out val);
                                int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                data.TryGetValue("StoreOldShip", out val);
                                string storedShipModel = (string)val;
                                Ship storedShip = storedShipId == null ? null : findShip(storedShipId, storedShipModel);

                                data.TryGetValue("SellShipID", out val);
                                int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                data.TryGetValue("SellOldShip", out val);
                                string soldShipModel = (string)val;
                                Ship soldShip = soldShipId == null ? null : findShip(soldShipId, soldShipModel);

                                data.TryGetValue("SellPrice", out val);
                                long? soldPrice = (long?)val;
                                journalEvent = new ShipPurchasedEvent(timestamp, ship, price, soldShip, soldPrice, storedShip);
                            }
                            handled = true;
                            break;
                        case "ShipyardNew":
                            {
                                object val;
                                data.TryGetValue("NewShipID", out val);
                                int shipId = (int)(long)val;
                                data.TryGetValue("ShipType", out val);
                                string shipModel = (string)val;
                                Ship ship = findShip(shipId, shipModel);

                                journalEvent = new ShipDeliveredEvent(timestamp, ship);
                            }
                            handled = true;
                            break;
                        case "ShipyardSell":
                            {
                                object val;
                                data.TryGetValue("SellShipID", out val);
                                int shipId = (int)(long)val;
                                data.TryGetValue("ShipType", out val);
                                string shipModel = (string)val;
                                Ship ship = findShip(shipId, shipModel);
                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;
                                journalEvent = new ShipSoldEvent(timestamp, ship, price);
                            }
                            handled = true;
                            break;
                        case "ShipyardSwap":
                            {
                                object val;

                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                data.TryGetValue("ShipType", out val);
                                string shipModel = (string)val;
                                Ship ship = findShip(shipId, shipModel);

                                data.TryGetValue("StoreShipID", out val);
                                int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                data.TryGetValue("StoreOldShip", out val);
                                string storedShipModel = (string)val;
                                Ship storedShip = storedShipId == null ? null : findShip(storedShipId, storedShipModel);

                                data.TryGetValue("SellShipID", out val);
                                int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                data.TryGetValue("SellOldShip", out val);
                                string soldShipModel = (string)val;
                                Ship soldShip = soldShipId == null ? null : findShip(soldShipId, soldShipModel);

                                journalEvent = new ShipSwappedEvent(timestamp, ship, soldShip, storedShip);
                            }
                            handled = true;
                            break;
                        case "ShipyardTransfer":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                data.TryGetValue("ShipType", out val);
                                string shipModel = (string)val;
                                Ship ship = findShip(shipId, shipModel);

                                data.TryGetValue("System", out val);
                                string system = (string)val;

                                data.TryGetValue("Distance", out val);
                                decimal distance = (decimal)(double)val;

                                data.TryGetValue("TransferPrice", out val);
                                long price = (long)val;

                                journalEvent = new ShipTransferInitiatedEvent(timestamp, ship, system, distance, price);

                                handled = true;
                            }
                            break;
                        case "LaunchSRV":
                            {
                                object val;
                                data.TryGetValue("Loadout", out val);
                                string loadout = (string)val;

                                data.TryGetValue("PlayerControlled", out val);
                                bool playercontrolled = (bool)val;

                                journalEvent = new SRVLaunchedEvent(timestamp, loadout, playercontrolled);
                            }
                            handled = true;
                            break;
                        case "DockSRV":
                            journalEvent = new SRVDockedEvent(timestamp);
                            handled = true;
                            break;
                        case "LaunchFighter":
                            {
                                object val;
                                data.TryGetValue("Loadout", out val);
                                string loadout = (string)val;
                                data.TryGetValue("PlayerControlled", out val);
                                bool playerControlled = (bool)val;
                                journalEvent = new FighterLaunchedEvent(timestamp, loadout, playerControlled);
                            }
                            handled = true;
                            break;
                        case "DockFighter":
                            journalEvent = new FighterDockedEvent(timestamp);
                            handled = true;
                            break;
                        case "VehicleSwitch":
                            {
                                object val;
                                data.TryGetValue("To", out val);
                                string to = (string)val;
                                if (to == "Fighter")
                                {
                                    journalEvent = new ControllingFighterEvent(timestamp);
                                    handled = true;
                                }
                                else if (to == "Mothership")
                                {
                                    journalEvent = new ControllingShipEvent(timestamp);
                                    handled = true;
                                }
                            }
                            break;
                        case "Interdicted":
                            {
                                object val;
                                data.TryGetValue("Submitted", out val);
                                bool submitted = (bool)val;
                                data.TryGetValue("Interdictor", out val);
                                string interdictor = (string)val;
                                data.TryGetValue("IsPlayer", out val);
                                bool iscommander = (bool)val;
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;

                                journalEvent = new ShipInterdictedEvent(timestamp, true, submitted, iscommander, interdictor, rating, faction, power);
                                handled = true;
                            }
                            break;
                        case "EscapeInterdiction":
                            {
                                object val;
                                data.TryGetValue("Interdictor", out val);
                                string interdictor = (string)val;
                                data.TryGetValue("IsPlayer", out val);
                                bool iscommander = (bool)val;

                                journalEvent = new ShipInterdictedEvent(timestamp, false, false, iscommander, interdictor, null, null, null);
                                handled = true;
                            }
                            break;
                        case "Interdiction":
                            {
                                object val;
                                data.TryGetValue("Success", out val);
                                bool success = (bool)val;
                                data.TryGetValue("Interdicted", out val);
                                string interdictee = (string)val;
                                data.TryGetValue("IsPlayer", out val);
                                bool iscommander = (bool)val;
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;

                                journalEvent = new ShipInterdictionEvent(timestamp, success, iscommander, interdictee, rating, faction, power);
                                handled = true;
                            }
                            break;
                        case "PVPKill":
                            {
                                object val;
                                data.TryGetValue("Victim", out val);
                                string victim = (string)val;
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));

                                journalEvent = new KilledEvent(timestamp, victim, rating);
                                handled = true;
                            }
                            break;
                        case "MaterialCollected":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                Material material = Material.FromEDName((string)val);
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                journalEvent = new MaterialCollectedEvent(timestamp, material, amount);
                                handled = true;
                            }
                            break;
                        case "MaterialDiscarded":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                Material material = Material.FromEDName((string)val);
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                journalEvent = new MaterialDiscardedEvent(timestamp, material, amount);
                                handled = true;
                            }
                            break;
                        case "MaterialDiscovered":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                Material material = Material.FromEDName((string)val);
                                journalEvent = new MaterialDiscoveredEvent(timestamp, material);
                                handled = true;
                            }
                            break;
                        case "ScientificResearch":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                Material material = Material.FromEDName((string)val);
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                journalEvent = new MaterialDonatedEvent(timestamp, material, amount);
                                handled = true;
                            }
                            break;
                        case "ReceiveText":
                            {
                                object val;
                                data.TryGetValue("From", out val);
                                string from = (string)val;

                                data.TryGetValue("Channel", out val);
                                string channel = (string)val;

                                data.TryGetValue("Message", out val);
                                string message = (string)val;

                                if (!(from.StartsWith("$cmdr") || from.StartsWith("&")))
                                {
                                    // This is NPC speech; see if it's something that we can use
                                    if (message == "$STATION_NoFireZone_entered;")
                                    {
                                        journalEvent = new StationNoFireZoneEnteredEvent(timestamp, false);
                                    }
                                    else if (message == "$STATION_NoFireZone_entered_deployed;")
                                    {
                                        journalEvent = new StationNoFireZoneEnteredEvent(timestamp, true);
                                    }
                                    else if (message == "$STATION_NoFireZone_exited;")
                                    {
                                        journalEvent = new StationNoFireZoneExitedEvent(timestamp);
                                    }
                                    else if (message.Contains("_StartInterdiction"))
                                    {
                                        // Find out who is doing the interdicting
                                        string by = npcSpeechBy(from, message);

                                        journalEvent = new NPCInterdictionCommencedEvent(timestamp, by);
                                    }
                                    else if (message.Contains("_Attack") || message.Contains("_OnAttackStart") || message.Contains("AttackRun") || message.Contains("OnDeclarePiracyAttack"))
                                    {
                                        // Find out who is doing the attacking
                                        string by = npcSpeechBy(from, message);
                                        journalEvent = new NPCAttackCommencedEvent(timestamp, by);
                                    }
                                    else if (message.Contains("_OnStartScanCargo"))
                                    {
                                        // Find out who is doing the scanning
                                        string by = npcSpeechBy(from, message);
                                        journalEvent = new NPCCargoScanCommencedEvent(timestamp, by);
                                    }
                                }
                                else
                                {
                                    from = from.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    journalEvent = new MessageReceivedEvent(timestamp, from, true, channel, message);
                                }
                            }
                            handled = true;
                            break;
                        case "SendText":
                            {
                                object val;
                                data.TryGetValue("To", out val);
                                string to = (string)val;
                                to = to.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                data.TryGetValue("Message", out val);
                                string message = (string)val;
                                journalEvent = new MessageSentEvent(timestamp, to, message);
                            }
                            handled = true;
                            break;
                        case "DockingRequested":
                            {
                                object val;
                                data.TryGetValue("StationName", out val);
                                string stationName = (string)val;
                                journalEvent = new DockingRequestedEvent(timestamp, stationName);
                            }
                            handled = true;
                            break;
                        case "DockingGranted":
                            {
                                object val;
                                data.TryGetValue("StationName", out val);
                                string stationName = (string)val;
                                data.TryGetValue("LandingPad", out val);
                                int landingPad = (int)(long)val;
                                journalEvent = new DockingGrantedEvent(timestamp, stationName, landingPad);
                            }
                            handled = true;
                            break;
                        case "DockingDenied":
                            {
                                object val;
                                data.TryGetValue("StationName", out val);
                                string stationName = (string)val;
                                data.TryGetValue("Reason", out val);
                                string reason = (string)val;
                                journalEvent = new DockingDeniedEvent(timestamp, stationName, reason);
                            }
                            handled = true;
                            break;
                        case "DockingCancelled":
                            {
                                object val;
                                data.TryGetValue("StationName", out val);
                                string stationName = (string)val;
                                journalEvent = new DockingCancelledEvent(timestamp, stationName);
                            }
                            handled = true;
                            break;
                        case "DockingTimeout":
                            {
                                object val;
                                data.TryGetValue("StationName", out val);
                                string stationName = (string)val;
                                journalEvent = new DockingTimedOutEvent(timestamp, stationName);
                            }
                            handled = true;
                            break;
                        case "MiningRefined":
                            {
                                object val;
                                data.TryGetValue("Type", out val);
                                string commodityName = (string)val;

                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map commodityrefined type " + commodityName + " to commodity");
                                }
                                journalEvent = new CommodityRefinedEvent(timestamp, commodity);
                            }
                            handled = true;
                            break;
                        case "HeatWarning":
                            journalEvent = new HeatWarningEvent(timestamp);
                            handled = true;
                            break;
                        case "HeatDamage":
                            journalEvent = new HeatDamageEvent(timestamp);
                            handled = true;
                            break;
                        case "HullDamage":
                            {
                                object val;
                                data.TryGetValue("Health", out val);
                                decimal health = sensibleHealth((decimal)(((double)val) * 100));

                                data.TryGetValue("PlayerPilot", out val);
                                bool? piloted = (bool?)val;

                                data.TryGetValue("Fighter", out val);
                                bool? fighter = (bool?)val;

                                string vehicle = EDDI.Instance.Vehicle;
                                if (fighter == true && piloted == false)
                                {
                                    vehicle = Constants.VEHICLE_FIGHTER;
                                }

                                journalEvent = new HullDamagedEvent(timestamp, vehicle, piloted, health);
                            }
                            handled = true;
                            break;
                        case "ShieldState":
                            {
                                object val;
                                data.TryGetValue("ShieldsUp", out val);
                                bool shieldsUp = (bool)val;
                                if (shieldsUp == true)
                                {
                                    journalEvent = new ShieldsUpEvent(timestamp);
                                }
                                else
                                {
                                    journalEvent = new ShieldsDownEvent(timestamp);
                                }
                                handled = true;
                                break;
                            }
                        case "SelfDestruct":
                            journalEvent = new SelfDestructEvent(timestamp);
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
                                    data.TryGetValue("KillerName", out val);
                                    names.Add((string)val);
                                    data.TryGetValue("KillerShip", out val);
                                    ships.Add((string)val);
                                    data.TryGetValue("KillerRank", out val);
                                    ratings.Add(CombatRating.FromEDName((string)val));
                                }
                                if (data.ContainsKey("killers"))
                                {
                                    // Multiple killers
                                    data.TryGetValue("Killers", out val);
                                    List<object> killers = (List<object>)val;
                                    foreach (IDictionary<string, object> killer in killers)
                                    {
                                        killer.TryGetValue("Name", out val);
                                        names.Add((string)val);
                                        killer.TryGetValue("Ship", out val);
                                        ships.Add((string)val);
                                        killer.TryGetValue("Rank", out val);
                                        ratings.Add(CombatRating.FromEDName((string)val));
                                    }
                                }
                                journalEvent = new DiedEvent(timestamp, names, ships, ratings);
                                handled = true;
                            }
                            break;
                        case "BuyExplorationData":
                            {
                                object val;
                                data.TryGetValue("System", out val);
                                string system = (string)val;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;
                                journalEvent = new ExplorationDataPurchasedEvent(timestamp, system, price);
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
                                journalEvent = new ExplorationDataSoldEvent(timestamp, systems, firsts, reward, bonus);
                                handled = true;
                                break;
                            }
                        case "USSDrop":
                            {
                                object val;
                                data.TryGetValue("USSType", out val);
                                string source = (string)val;
                                data.TryGetValue("USSThreat", out val);
                                int threat = (int)(long)val;
                                journalEvent = new EnteredSignalSourceEvent(timestamp, source, threat);
                            }
                            handled = true;
                            break;
                        case "MarketBuy":
                            {
                                object val;
                                data.TryGetValue("Type", out val);
                                string commodityName = (string)val;
                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map marketbuy type " + commodityName + " to commodity");
                                }
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                data.TryGetValue("BuyPrice", out val);
                                long price = (long)val;
                                journalEvent = new CommodityPurchasedEvent(timestamp, commodity, amount, price);
                                handled = true;
                                break;
                            }
                        case "MarketSell":
                            {
                                object val;
                                data.TryGetValue("Type", out val);
                                string commodityName = (string)val;
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
                                data.TryGetValue("IllegalGoods", out val);
                                bool illegal = (val == null ? false : (bool)val);
                                data.TryGetValue("StolenGoods", out val);
                                bool stolen = (val == null ? false : (bool)val);
                                data.TryGetValue("BlackMarket", out val);
                                bool blackmarket = (val == null ? false : (bool)val);
                                journalEvent = new CommoditySoldEvent(timestamp, commodity, amount, price, profit, illegal, stolen, blackmarket);
                                handled = true;
                                break;
                            }
                        case "EngineerCraft":
                            {
                                object val;
                                data.TryGetValue("Engineer", out val);
                                string engineer = (string)val;
                                data.TryGetValue("Blueprint", out val);
                                string blueprint = (string)val;
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
                                            Material material = Material.FromEDName((string)materialJson["Name"]);
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }
                                }
                                journalEvent = new ModificationCraftedEvent(timestamp, engineer, blueprint, level, materials, commodities);
                                handled = true;
                                break;
                            }
                        case "EngineerApply":
                            {
                                object val;
                                data.TryGetValue("Engineer", out val);
                                string engineer = (string)val;
                                data.TryGetValue("Blueprint", out val);
                                string blueprint = (string)val;
                                data.TryGetValue("Level", out val);
                                int level = (int)(long)val;

                                journalEvent = new ModificationAppliedEvent(timestamp, engineer, blueprint, level);
                                handled = true;
                                break;
                            }
                        case "EngineerProgress":
                            {
                                object val;
                                data.TryGetValue("Engineer", out val);
                                string engineer = (string)val;
                                data.TryGetValue("Rank", out val);
                                if (val == null)
                                {
                                    // There are other non-rank events for engineers but we don't pay attention to them
                                    break;
                                }
                                int rank = (int)(long)val;

                                journalEvent = new EngineerProgressedEvent(timestamp, engineer, rank);
                                handled = true;
                                break;
                            }
                        case "LoadGame":
                            {
                                object val;
                                data.TryGetValue("Commander", out val);
                                string commander = (string)val;

                                data.TryGetValue("ShipID", out val);
                                int? shipId = (int?)(long?)val;

                                if (shipId == null)
                                {
                                    // This happens if we are in CQC.  Flag it back to EDDI so that it ignores everything that happens until
                                    // we're out of CQC again
                                    journalEvent = new EnteredCQCEvent(timestamp, commander);
                                    handled = true;
                                    break;
                                }

                                data.TryGetValue("Ship", out val);
                                string shipModel = (string)val;
                                Ship ship = findShip(shipId, shipModel);
                                data.TryGetValue("GameMode", out val);
                                GameMode mode = GameMode.FromEDName((string)val);
                                data.TryGetValue("Group", out val);
                                string group = (string)val;
                                data.TryGetValue("Credits", out val);
                                decimal credits = (long)val;
                                data.TryGetValue("Loan", out val);
                                decimal loan = (long)val;
                                journalEvent = new CommanderContinuedEvent(timestamp, commander, ship, mode, group, credits, loan);
                                handled = true;
                                break;
                            }
                        case "CrewHire":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.From(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = CombatRating.FromRank((int)(long)val);
                                journalEvent = new CrewHiredEvent(timestamp, name, faction, price, rating);
                                handled = true;
                                break;
                            }
                        case "CrewFire":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                journalEvent = new CrewFiredEvent(timestamp, name);
                                handled = true;
                                break;
                            }
                        case "CrewAssign":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                data.TryGetValue("Role", out val);
                                string role = (string)val;
                                journalEvent = new CrewAssignedEvent(timestamp, name, role);
                                handled = true;
                                break;
                            }
                        case "BuyAmmo":
                            {
                                object val;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;
                                journalEvent = new ShipRestockedEvent(timestamp, price);
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
                                journalEvent = new LimpetPurchasedEvent(timestamp, amount, price);
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
                                journalEvent = new LimpetSoldEvent(timestamp, amount, price);
                                handled = true;
                                break;
                            }
                        case "ClearSavedGame":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                journalEvent = new ClearedSaveEvent(timestamp, name);
                                handled = true;
                                break;
                            }
                        case "NewCommander":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                data.TryGetValue("Package", out val);
                                string package = (string)val;
                                journalEvent = new CommanderStartedEvent(timestamp, name, package);
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

                                journalEvent = new CommanderProgressEvent(timestamp, combat, trade, exploration, cqc, empire, federation);
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

                                journalEvent = new CommanderRatingsEvent(timestamp, combat, trade, exploration, cqc, empire, federation);
                                handled = true;
                                break;
                            }
                        case "Screenshot":
                            {
                                object val;
                                data.TryGetValue("Filename", out val);
                                string filename = (string)val;
                                data.TryGetValue("Width", out val);
                                int width = (int)(long)val;
                                data.TryGetValue("Height", out val);
                                int height = (int)(long)val;
                                data.TryGetValue("System", out val);
                                string system = (string)val;
                                data.TryGetValue("Body", out val);
                                string body = (string)val;

                                journalEvent = new ScreenshotEvent(timestamp, filename, width, height, system, body);
                                handled = true;
                                break;
                            }
                        case "BuyTradeData":
                            {
                                object val;
                                data.TryGetValue("System", out val);
                                string system = (string)val;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                journalEvent = new TradeDataPurchasedEvent(timestamp, system, price);
                                handled = true;
                                break;
                            }
                        case "PayFines":
                            {
                                object val;
                                data.TryGetValue("Amount", out val);
                                long amount = (long)val;

                                journalEvent = new FinePaidEvent(timestamp, amount, false);
                                handled = true;
                                break;
                            }
                        case "PayLegacyFines":
                            {
                                object val;
                                data.TryGetValue("Amount", out val);
                                long amount = (long)val;

                                journalEvent = new FinePaidEvent(timestamp, amount, true);
                                handled = true;
                                break;
                            }
                        case "RefuelPartial":
                            {
                                object val;
                                data.TryGetValue("Amount", out val);
                                decimal amount = (decimal)(double)val;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                journalEvent = new ShipRefuelledEvent(timestamp, "Market", price, amount, null);
                                handled = true;
                                break;
                            }
                        case "RefuelAll":
                            {
                                object val;
                                data.TryGetValue("Amount", out val);
                                decimal amount = (decimal)(double)val;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                journalEvent = new ShipRefuelledEvent(timestamp, "Market", price, amount, null);
                                handled = true;
                                break;
                            }
                        case "FuelScoop":
                            {
                                object val;
                                data.TryGetValue("Scooped", out val);
                                decimal amount = (decimal)(double)val;
                                data.TryGetValue("Total", out val);
                                decimal total = (decimal)(double)val;

                                journalEvent = new ShipRefuelledEvent(timestamp, "Scoop", null, amount, total);
                                handled = true;
                                break;
                            }
                        case "RedeemVoucher":
                            // Logging.Report("Redeem voucher", line);
                            break;
                        case "CommunityGoalJoin":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                data.TryGetValue("System", out val);
                                string system = (string)val;

                                journalEvent = new MissionAcceptedEvent(timestamp, null, name, system, null, null, null, null, null, null, null, null, null, true, null);
                                handled = true;
                                break;
                            }
                        case "CommunityGoalReward":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                data.TryGetValue("System", out val);
                                string system = (string)val;
                                data.TryGetValue("Reward", out val);
                                long reward = (val == null ? 0 : (long)val);

                                journalEvent = new MissionCompletedEvent(timestamp, null, name, null, null, null, true, reward, null, 0);
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
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                // Could be a superpower...
                                Superpower superpowerFaction = Superpower.From(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;

                                // Missions with destinations
                                data.TryGetValue("DestinationSystem", out val);
                                string destinationsystem = (string)val;
                                data.TryGetValue("DestinationStation", out val);
                                string destinationstation = (string)val;

                                // Missions with commodities
                                data.TryGetValue("Commodity", out val);
                                Commodity commodity = CommodityDefinitions.FromName((string)val);
                                data.TryGetValue("Count", out val);
                                int? amount = (int?)(long?)val;

                                // Missions with targets
                                data.TryGetValue("Target", out val);
                                string target = (string)val;
                                data.TryGetValue("TargetType", out val);
                                string targettype = (string)val;
                                data.TryGetValue("TargetFaction", out val);
                                string targetfaction = (string)val;
                                // Could be a superpower...
                                Superpower superpowerTargetFaction = Superpower.From(targetfaction);
                                targetfaction = superpowerTargetFaction != null ? superpowerTargetFaction.name : targetfaction;

                                // Missions with passengers
                                data.TryGetValue("PassengerType", out val);
                                string passengertype = (string)val;
                                data.TryGetValue("PassengersWanted", out val);
                                bool? passengerswanted = (bool?)val;
                                data.TryGetValue("PassengerCount", out val);
                                if (val != null)
                                {
                                    amount = (int?)(long?)val;
                                }

                                journalEvent = new MissionAcceptedEvent(timestamp, missionid, name, faction, destinationsystem, destinationstation, commodity, amount, passengertype, passengerswanted, target, targettype, targetfaction, false, expiry);
                                handled = true;
                                break;
                            }
                        case "MissionCompleted":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                data.TryGetValue("Reward", out val);
                                long reward = (val == null ? 0 : (long)val);
                                data.TryGetValue("Donation", out val);
                                long donation = (val == null ? 0 : (long)val);
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                // Could be a superpower...
                                Superpower superpowerFaction = Superpower.From(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;

                                // Missions with commodities
                                data.TryGetValue("Commodity", out val);
                                Commodity commodity = CommodityDefinitions.FromName((string)val);
                                data.TryGetValue("Count", out val);
                                int? amount = (int?)(long?)val;

                                List<CommodityAmount> commodityrewards = new List<CommodityAmount>();
                                data.TryGetValue("CommodityReward", out val);
                                List<object> commodityRewardsData = (List<object>)val;
                                if (commodityRewardsData != null)
                                {
                                    foreach (Dictionary<string, object> commodityRewardData in commodityRewardsData)
                                    {
                                        commodityRewardData.TryGetValue("Name", out val);
                                        Commodity rewardCommodity = CommodityDefinitions.FromName((string)val);
                                        commodityRewardData.TryGetValue("Count", out val);
                                        int count = (int)(long)val;
                                        commodityrewards.Add(new CommodityAmount(rewardCommodity, count));
                                    }
                                }

                                journalEvent = new MissionCompletedEvent(timestamp, missionid, name, faction, commodity, amount, false, reward, commodityrewards, donation);
                                handled = true;
                                break;
                            }
                        case "MissionAbandoned":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                journalEvent = new MissionAbandonedEvent(timestamp, missionid, name);
                                handled = true;
                                break;
                            }
                        case "MissionFailed":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                data.TryGetValue("Name", out val);
                                string name = (string)val;
                                journalEvent = new MissionFailedEvent(timestamp, missionid, name);
                                handled = true;
                                break;
                            }
                        case "Repair":
                            {
                                object val;
                                data.TryGetValue("Item", out val);
                                string item = (string)val;
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
                                journalEvent = new ShipRepairedEvent(timestamp, item, price);
                                handled = true;
                                break;
                            }
                        case "RepairAll":
                            {
                                object val;
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;
                                journalEvent = new ShipRepairedEvent(timestamp, null, price);
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
                                journalEvent = new ShipRebootedEvent(timestamp, modules);
                                handled = true;
                                break;
                            }
                        case "Synthesis":
                            {
                                object val;
                                data.TryGetValue("Name", out val);
                                string synthesis = (string)val;

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
                                        Material material = Material.FromEDName((string)materialJson["Name"]);
                                        materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                    }
                                }

                                journalEvent = new SynthesisedEvent(timestamp, synthesis, materials);
                                handled = true;
                                break;
                            }
                        case "PowerplayJoin":
                            {
                                object val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;

                                journalEvent = new PowerJoinedEvent(timestamp, power);
                                handled = true;
                                break;
                            }
                        case "PowerplayLeave":
                            {
                                object val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;

                                journalEvent = new PowerLeftEvent(timestamp, power);
                                handled = true;
                                break;
                            }
                        case "PowerplayDefect":
                            {
                                object val;
                                data.TryGetValue("FromPower", out val);
                                string frompower = (string)val;
                                data.TryGetValue("ToPower", out val);
                                string topower = (string)val;

                                journalEvent = new PowerDefectedEvent(timestamp, frompower, topower);
                                handled = true;
                                break;
                            }
                        case "PowerplayVote":
                            {
                                object val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;
                                data.TryGetValue("System", out val);
                                string system = (string)val;
                                data.TryGetValue("Votes", out val);
                                int amount = (int)(long)val;

                                journalEvent = new PowerPreparationVoteCast(timestamp, power, system, amount);
                                handled = true;
                                break;
                            }
                        case "PowerplaySalary":
                            {
                                object val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;
                                data.TryGetValue("Amount", out val);
                                int amount = (int)(long)val;

                                journalEvent = new PowerSalaryClaimedEvent(timestamp, power, amount);
                                handled = true;
                                break;
                            }
                        case "PowerplayCollect":
                            {
                                object val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;
                                // Currently using localised information as we don't have commodity definitions for all powerplay commodities
                                data.TryGetValue("Type_Localised", out val);
                                string commodity = (string)val;
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;

                                journalEvent = new PowerCommodityObtainedEvent(timestamp, power, commodity, amount);
                                handled = true;
                                break;
                            }
                        case "PowerplayDeliver":
                            {
                                object val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;
                                // Currently using localised information as we don't have commodity definitions for all powerplay commodities
                                data.TryGetValue("Type_Localised", out val);
                                string commodity = (string)val;
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;

                                journalEvent = new PowerCommodityDeliveredEvent(timestamp, power, commodity, amount);
                                handled = true;
                                break;
                            }
                        case "PowerplayFastTrack":
                            {
                                object val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;
                                data.TryGetValue("Cost", out val);
                                int amount = (int)(long)val;

                                journalEvent = new PowerCommodityFastTrackedEvent(timestamp, power, amount);
                                handled = true;
                                break;
                            }
                        case "PowerplayVoucher":
                            {
                                object val;
                                data.TryGetValue("Power", out val);
                                string power = (string)val;
                                data.TryGetValue("Systems", out val);
                                List<string> systems = ((List<object>)val).Cast<string>().ToList();

                                journalEvent = new PowerVoucherReceivedEvent(timestamp, power, systems);
                                handled = true;
                                break;
                            }
                        case "SystemsShutdown":
                            {
                                journalEvent = new ShipShutdownEvent(timestamp);
                                handled = true;
                                break;
                            }
                    }

                    if (journalEvent != null)
                    {
                        journalEvent.raw = line;
                    }

                    if (handled)
                    {
                        if (journalEvent == null)
                        {
                            Logging.Debug("Handled event");
                        }
                        else
                        {
                            Logging.Debug("Handled event: " + JsonConvert.SerializeObject(journalEvent));
                        }
                    }
                    else
                    {
                        Logging.Debug("Unhandled event: " + line);
                    }
                    return journalEvent;
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to parse line: " + ex.ToString());
                Logging.Error("Exception whilst parsing line", line);
            }
            return null;
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
                by = "Cargo hunter";
            }
            else if (message.StartsWith("$Commuter"))
            {
                by = "Commuter";
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
                by = "Passenger hunter";
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
                by = "Rival power assassin";
            }
            else if (message.StartsWith("$PowersPirate"))
            {
                by = "Rival power pirate";
            }
            else if (message.StartsWith("$PowersSecurity"))
            {
                by = "Rival power security";
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
                by = "Smuggler";
            }
            else if (message.StartsWith("$StarshipOne"))
            {
                by = "Starship One";
            }
            return by;
        }
        private static Ship findShip(int? localId, string model)
        {
            Ship ship = null;
            if (localId == null && model == null)
            {
                // Default to the current ship
                ship = EDDI.Instance.Ship;
            }
            else
            {
                // Find the ship with the given local ID
                if (EDDI.Instance.Ship != null && EDDI.Instance.Ship.LocalId == localId)
                {
                    ship = EDDI.Instance.Ship;
                }
                else
                {
                    ship = EDDI.Instance.Shipyard.FirstOrDefault(v => v.LocalId == localId);
                }
            }

            if (ship == null)
            {
                Logging.Warn("Failed to find ship given ID " + localId + " and model " + model);
                // Provide a basic ship based on the model template
                ship = ShipDefinitions.FromEDModel(model);
                ship.LocalId = localId == null ? 0 : (int)localId;
            }
            return ship;
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
            int result = SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out path);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous";
            }
            else
            {
                throw new ExternalException("Failed to find the saved games directory.", result);
            }
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
    }
}
