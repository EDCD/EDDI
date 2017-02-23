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

                    Event journalEvent = null;
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
                                journalEvent = new DockedEvent(timestamp, systemName, stationName, stationModel, faction, factionState, economy, government);
                            }
                            handled = true;
                            break;
                        case "Undocked":
                            {
                                string stationName = getString(data, "StationName");
                                journalEvent = new UndockedEvent(timestamp, stationName);
                            }
                            handled = true;
                            break;
                        case "Touchdown":
                            {
                                decimal latitude = getDecimal(data, "Latitude");
                                decimal longitude = getDecimal(data, "Longitude");
                                bool? playercontrolled = getOptionalBool(data, "PlayerControlled");
                                journalEvent = new TouchdownEvent(timestamp, longitude, latitude, playercontrolled);
                            }
                            handled = true;
                            break;
                        case "Liftoff":
                            {
                                decimal latitude = getDecimal(data, "Latitude");
                                decimal longitude = getDecimal(data, "Longitude");
                                bool? playercontrolled = getOptionalBool(data, "PlayerControlled");
                                journalEvent = new LiftoffEvent(timestamp, longitude, latitude, playercontrolled);
                            }
                            handled = true;
                            break;
                        case "SupercruiseEntry":
                            {
                                string system = getString(data, "StarySystem");
                                journalEvent = new EnteredSupercruiseEvent(timestamp, system);
                            }
                            handled = true;
                            break;
                        case "SupercruiseExit":
                            {
                                string system = getString(data, "StarSystem");
                                string body = getString(data, "Body");
                                string bodyType = getString(data, "BodyType");
                                journalEvent = new EnteredNormalSpaceEvent(timestamp, system, body, bodyType);
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

                                journalEvent = new JumpedEvent(timestamp, systemName, x, y, z, distance, fuelUsed, fuelRemaining, allegiance, faction, factionState, economy, government, security);
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

                                string station = getString(data, "StationName");
                                string stationtype = getString(data, "StationType");

                                decimal? latitude = getOptionalDecimal(data, "Latitude");
                                decimal? longitude = getOptionalDecimal(data, "Longitude");

                                journalEvent = new LocationEvent(timestamp, systemName, x, y, z, body, bodyType, docked, station, stationtype, allegiance, faction, economy, government, security, longitude, latitude);
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

                                journalEvent = new BountyAwardedEvent(timestamp, target, victimFaction, reward, rewards, shared);
                            }
                            handled = true;
                            break;
                        case "CapShipBond":
                        case "FactionKillBond":
                            {
                                object val;
                                string awardingFaction = getFaction(data, "Faction");
                                data.TryGetValue("Reward", out val);
                                long reward = (long)val;
                                string victimFaction = getString(data, "VictimFaction");

                                journalEvent = new BondAwardedEvent(timestamp, awardingFaction, victimFaction, reward);
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
                                string commodityName = getString(data, "Type");
                                Commodity commodity = CommodityDefinitions.FromName(commodityName);
                                if (commodity == null)
                                {
                                    Logging.Error("Failed to map collectcargo type " + commodityName + " to commodity");
                                }
                                bool stolen = getBool(data, "Stolen");
                                journalEvent = new CommodityCollectedEvent(timestamp, commodity, stolen);
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
                                journalEvent = new CommodityEjectedEvent(timestamp, commodity, amount, abandoned);
                                handled = true;
                            }
                            handled = true;
                            break;
                        case "CockpitBreached":
                            journalEvent = new CockpitBreachedEvent(timestamp);
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
                                journalEvent = new SettlementApproachedEvent(timestamp, name);
                            }
                            handled = true;
                            break;
                        case "Scan":
                            {
                                object val;
                                // Common items
                                string name = getString(data, "BodyName");

                                decimal distancefromarrival = getDecimal(data, "DistanceFromArrivalLS");
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
                                        Composition ringComposition = Composition.FromEDName(getString(ringData, "RingClass"));
                                        decimal ringMass = getDecimal(data, "MassMT");
                                        decimal ringInnerRadius = getDecimal(data, "InnerRad");
                                        decimal ringOuterRadius = getDecimal(data, "OuterRad");

                                        rings.Add(new Ring(ringName, ringComposition, ringMass, ringInnerRadius, ringOuterRadius));
                                    }
                                }

                                if (data.ContainsKey("StarType"))
                                {
                                    // Star
                                    string starType = getString(data, "StarType");
                                    decimal stellarMass = getDecimal(data, "StellarMass");
                                    decimal absoluteMagnitude = getDecimal(data, "AbsoluteMagnitude");
                                    data.TryGetValue("Age_MY", out val);
                                    long age = (long)val * 1000000;
                                    decimal temperature = getDecimal(data, "SurfaceTemperature");

                                    journalEvent = new StarScannedEvent(timestamp, name, starType, stellarMass, radius, absoluteMagnitude, age, temperature, distancefromarrival, orbitalperiod, rotationperiod, semimajoraxis, eccentricity, orbitalinclination, periapsis, rings);
                                    handled = true;
                                }
                                else
                                {
                                    // Body
                                    bool? tidallyLocked = getOptionalBool(data, "TidalLock");

                                    string bodyClass = getString(data, "PlanetClass");

                                    // MKW: Gravity in the Journal is in m/s; must convert it to G
                                    decimal gravity = Body.ms2g(getDecimal(data, "SurfaceGravity"));

                                    decimal? temperature = getOptionalDecimal(data, "SurfaceTemperature");

                                    decimal? pressure = getOptionalDecimal(data, "SurfacePressure");

                                    bool? landable = getOptionalBool(data, "Landable");

                                    string reserves = getString(data, "ReserveLevel");

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

                                    journalEvent = new BodyScannedEvent(timestamp, name, bodyClass, gravity, temperature, pressure, tidallyLocked, landable, atmosphere, volcanism, distancefromarrival, (decimal)orbitalperiod, rotationperiod, semimajoraxis, eccentricity, orbitalinclination, periapsis, rings, reserves, materials, terraformState);
                                    handled = true;
                                }
                            }
                            break;
                        case "ShipyardBuy":
                            {
                                object val;
                                // We don't have a ship ID at this point so use the ship type
                                string shipModel = getString(data, "ShipType");
                                Ship ship = findShip(null, shipModel);

                                data.TryGetValue("ShipPrice", out val);
                                long price = (long)val;

                                data.TryGetValue("StoreShipID", out val);
                                int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                string storedShipModel = getString(data, "StoreOldShip");
                                Ship storedShip = storedShipId == null ? null : findShip(storedShipId, storedShipModel);

                                data.TryGetValue("SellShipID", out val);
                                int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                string soldShipModel = getString(data, "SellOldShip");
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
                                string shipModel = getString(data, "ShipType");
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
                                string shipModel = getString(data, "ShipType");
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
                                string shipModel = getString(data, "ShipType");
                                Ship ship = findShip(shipId, shipModel);

                                data.TryGetValue("StoreShipID", out val);
                                int? storedShipId = (val == null ? (int?)null : (int)(long)val);
                                string storedShipModel = getString(data, "StoreOldShip");
                                Ship storedShip = storedShipId == null ? null : findShip(storedShipId, storedShipModel);

                                data.TryGetValue("SellShipID", out val);
                                int? soldShipId = (val == null ? (int?)null : (int)(long)val);
                                string soldShipModel = getString(data, "SellOldShip");
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
                                string shipModel = getString(data, "ShipType");
                                Ship ship = findShip(shipId, shipModel);

                                string system = getString(data, "System");
                                decimal distance = getDecimal(data, "Distance");
                                data.TryGetValue("TransferPrice", out val);
                                long price = (long)val;

                                journalEvent = new ShipTransferInitiatedEvent(timestamp, ship, system, distance, price);

                                handled = true;
                            }
                            break;
                        case "SetUserShipName":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                string shipModel = getString(data, "Ship");
                                Ship ship = findShip(shipId, shipModel);
                                ship.name = getString(data, "UserShipName");
                                ship.ident = getString(data, "UserShipId");

                                journalEvent = new ShipRenamedEvent(timestamp, ship);
                            }
                            handled = true;
                            break;
                        case "LaunchSRV":
                            {
                                string loadout = getString(data, "Loadout");
                                bool playercontrolled = getBool(data, "PlayerControlled");

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
                                string loadout = getString(data, "Loadout");
                                bool playerControlled = getBool(data, "PlayerControlled");
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
                                string to = getString(data, "To");
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
                                bool submitted = getBool(data, "Submitted");
                                string interdictor = getString(data, "Interdictor");
                                bool iscommander = getBool(data, "IsPlayer");
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));
                                string faction = getFaction(data, "Faction");
                                string power = getString(data, "Power");

                                journalEvent = new ShipInterdictedEvent(timestamp, true, submitted, iscommander, interdictor, rating, faction, power);
                                handled = true;
                            }
                            break;
                        case "EscapeInterdiction":
                            {
                                string interdictor = getString(data, "Interdictor");
                                bool iscommander = getBool(data, "IsPlayer");

                                journalEvent = new ShipInterdictedEvent(timestamp, false, false, iscommander, interdictor, null, null, null);
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

                                journalEvent = new ShipInterdictionEvent(timestamp, success, iscommander, interdictee, rating, faction, power);
                                handled = true;
                            }
                            break;
                        case "PVPKill":
                            {
                                object val;
                                string victim = getString(data, "Victim");
                                data.TryGetValue("CombatRank", out val);
                                CombatRating rating = (val == null ? null : CombatRating.FromRank((int)(long)val));

                                journalEvent = new KilledEvent(timestamp, victim, rating);
                                handled = true;
                            }
                            break;
                        case "MaterialCollected":
                            {
                                object val;
                                Material material = Material.FromEDName(getString(data, "Name"));
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                journalEvent = new MaterialCollectedEvent(timestamp, material, amount);
                                handled = true;
                            }
                            break;
                        case "MaterialDiscarded":
                            {
                                object val;
                                Material material = Material.FromEDName(getString(data, "Name"));
                                data.TryGetValue("Count", out val);
                                int amount = (int)(long)val;
                                journalEvent = new MaterialDiscardedEvent(timestamp, material, amount);
                                handled = true;
                            }
                            break;
                        case "MaterialDiscovered":
                            {
                                Material material = Material.FromEDName(getString(data, "Name"));
                                journalEvent = new MaterialDiscoveredEvent(timestamp, material);
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
                                journalEvent = new MaterialDonatedEvent(timestamp, material, amount);
                                handled = true;
                            }
                            break;
                        case "StartJump":
                            {
                                string target = getString(data, "JumpType");
                                string stellarclass = getString(data, "StarClass");
                                journalEvent = new FSDEngagedEvent(timestamp, target, stellarclass);
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
                                string to = getString(data, "To");
                                to = to.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                string message = getString(data, "Message");
                                journalEvent = new MessageSentEvent(timestamp, to, message);
                            }
                            handled = true;
                            break;
                        case "DockingRequested":
                            {
                                string stationName = getString(data, "StationName");
                                journalEvent = new DockingRequestedEvent(timestamp, stationName);
                            }
                            handled = true;
                            break;
                        case "DockingGranted":
                            {
                                object val;
                                string stationName = getString(data, "StationName");
                                data.TryGetValue("LandingPad", out val);
                                int landingPad = (int)(long)val;
                                journalEvent = new DockingGrantedEvent(timestamp, stationName, landingPad);
                            }
                            handled = true;
                            break;
                        case "DockingDenied":
                            {
                                string stationName = getString(data, "StationName");
                                string reason = getString(data, "Reason");
                                journalEvent = new DockingDeniedEvent(timestamp, stationName, reason);
                            }
                            handled = true;
                            break;
                        case "DockingCancelled":
                            {
                                string stationName = getString(data, "StationName");
                                journalEvent = new DockingCancelledEvent(timestamp, stationName);
                            }
                            handled = true;
                            break;
                        case "DockingTimeout":
                            {
                                string stationName = getString(data, "StationName");
                                journalEvent = new DockingTimedOutEvent(timestamp, stationName);
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
                                decimal health = sensibleHealth(getDecimal(data, "Health") * 100);
                                bool? piloted = getOptionalBool(data, "PlayerPilot");
                                bool? fighter = getOptionalBool(data, "Fighter");

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
                                bool shieldsUp = getBool(data, "ShieldsUp");
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
                                journalEvent = new DiedEvent(timestamp, names, ships, ratings);
                                handled = true;
                            }
                            break;
                        case "BuyExplorationData":
                            {
                                object val;
                                string system = getString(data, "System");
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
                                string source = getString(data, "USSType");
                                data.TryGetValue("USSThreat", out val);
                                int threat = (int)(long)val;
                                journalEvent = new EnteredSignalSourceEvent(timestamp, source, threat);
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
                                journalEvent = new CommodityPurchasedEvent(timestamp, commodity, amount, price);
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
                                journalEvent = new CommoditySoldEvent(timestamp, commodity, amount, price, profit, illegal, stolen, blackmarket);
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
                                journalEvent = new ModificationCraftedEvent(timestamp, engineer, blueprint, level, materials, commodities);
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

                                journalEvent = new ModificationAppliedEvent(timestamp, engineer, blueprint, level);
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

                                journalEvent = new EngineerProgressedEvent(timestamp, engineer, rank);
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
                                    journalEvent = new EnteredCQCEvent(timestamp, commander);
                                    handled = true;
                                    break;
                                }

                                string shipModel = getString(data, "Ship");
                                Ship ship = findShip(shipId, shipModel);
                                // Add ship name and ship ID
                                data.TryGetValue("ShipName", out val);
                                if (val != null)
                                {
                                    ship.name = (string)val;
                                }
                                data.TryGetValue("ShipIdent", out val);
                                if (val != null)
                                {
                                    ship.ident = (string)val;
                                }

                                GameMode mode = GameMode.FromEDName(getString(data, "GameMode"));
                                string group = getString(data, "Group");
                                data.TryGetValue("Credits", out val);
                                decimal credits = (long)val;
                                data.TryGetValue("Loan", out val);
                                decimal loan = (long)val;
                                decimal? fuel = getOptionalDecimal(data, "FuelLevel");

                                journalEvent = new CommanderContinuedEvent(timestamp, commander, ship, mode, group, credits, loan, fuel);
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
                                journalEvent = new CrewHiredEvent(timestamp, name, faction, price, rating);
                                handled = true;
                                break;
                            }
                        case "CrewFire":
                            {
                                string name = getString(data, "Name");
                                journalEvent = new CrewFiredEvent(timestamp, name);
                                handled = true;
                                break;
                            }
                        case "CrewAssign":
                            {
                                string name = getString(data, "Name");
                                string role = getString(data, "Role");
                                journalEvent = new CrewAssignedEvent(timestamp, name, role);
                                handled = true;
                                break;
                            }
                        case "JoinACrew":
                            {
                                string captain = getString(data, "Captain");
                                captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                journalEvent = new CrewJoinedEvent(timestamp, captain);
                                handled = true;
                                break;
                            }
                        case "QuitACrew":
                            {
                                string captain = getString(data, "Captain");
                                captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                journalEvent = new CrewLeftEvent(timestamp, captain);
                                handled = true;
                                break;
                            }
                        case "ChangeCrewRole":
                            {
                                string role = getString(data, "Role");
                                if (role == "FireCon")
                                {
                                    role = "Gunner";
                                }
                                else if (role == "FighterCon")
                                {
                                    role = "Fighter";
                                }

                                journalEvent = new CrewRoleChangedEvent(timestamp, role);
                                handled = true;
                                break;
                            }
                        case "CrewMemberJoins":
                            {
                                string member = getString(data, "Crew");
                                member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                journalEvent = new CrewMemberJoinedEvent(timestamp, member);
                                handled = true;
                                break;
                            }
                        case "CrewMemberQuits":
                            {
                                string member = getString(data, "Crew");
                                member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                journalEvent = new CrewMemberLeftEvent(timestamp, member);
                                handled = true;
                                break;
                            }
                        case "KickCrewMember":
                            {
                                string member = getString(data, "Crew");
                                member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");

                                journalEvent = new CrewMemberRemovedEvent(timestamp, member);
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
                                string name = getString(data, "Name");
                                journalEvent = new ClearedSaveEvent(timestamp, name);
                                handled = true;
                                break;
                            }
                        case "NewCommander":
                            {
                                string name = getString(data, "Name");
                                string package = getString(data, "Package");
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
                                string filename = getString(data, "Filename");
                                data.TryGetValue("Width", out val);
                                int width = (int)(long)val;
                                data.TryGetValue("Height", out val);
                                int height = (int)(long)val;
                                string system = getString(data, "System");
                                string body = getString(data, "Body");

                                journalEvent = new ScreenshotEvent(timestamp, filename, width, height, system, body);
                                handled = true;
                                break;
                            }
                        case "BuyTradeData":
                            {
                                object val;
                                string system = getString(data, "System");
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
                                decimal amount = getDecimal(data, "Amount");
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                journalEvent = new ShipRefuelledEvent(timestamp, "Market", price, amount, null);
                                handled = true;
                                break;
                            }
                        case "RefuelAll":
                            {
                                object val;
                                decimal amount = getDecimal(data, "Amount");
                                data.TryGetValue("Cost", out val);
                                long price = (long)val;

                                journalEvent = new ShipRefuelledEvent(timestamp, "Market", price, amount, null);
                                handled = true;
                                break;
                            }
                        case "FuelScoop":
                            {
                                decimal amount = getDecimal(data, "Scooped");
                                decimal total = getDecimal(data, "Total");

                                journalEvent = new ShipRefuelledEvent(timestamp, "Scoop", null, amount, total);
                                handled = true;
                                break;
                            }
                        case "RedeemVoucher":
                            {
                                object val;

                                string type = getString(data, "Type");
                                string faction = getFaction(data, "Faction");
                                data.TryGetValue("Amount", out val);
                                long amount = (long)val;

                                if (type == "bounty")
                                {
                                    journalEvent = new BountyRedeemedEvent(timestamp, faction, amount);
                                }
                                else if (type == "CombatBond")
                                {
                                    journalEvent = new BondRedeemedEvent(timestamp, faction, amount);
                                }
                                else if (type == "trade")
                                {
                                    journalEvent = new TradeVoucherRedeemedEvent(timestamp, faction, amount);
                                }
                                else if (type == "settlement" || type == "scannable")
                                {
                                    journalEvent = new DataVoucherRedeemedEvent(timestamp, faction, amount);
                                }
                                else
                                {
                                    Logging.Warn("Unhandled voucher type " + type);
                                    Logging.Report("Unhandled voucher type " + type);
                                }

                                handled = true;
                                break;
                            }
                        case "CommunityGoalJoin":
                            {
                                string name = getString(data, "Name");
                                string system = getString(data, "System");

                                journalEvent = new MissionAcceptedEvent(timestamp, null, name, system, null, null, null, null, null, null, null, null, null, true, null, null, null);
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
                                bool? passengerswanted = getOptionalBool(data, "PassengersWanted");
                                data.TryGetValue("PassengerCount", out val);
                                if (val != null)
                                {
                                    amount = (int?)(long?)val;
                                }

                                // Impact on influence and reputation
                                string influence = getString(data, "Influence");
                                string reputation = getString(data, "Reputation");

                                journalEvent = new MissionAcceptedEvent(timestamp, missionid, name, faction, destinationsystem, destinationstation, commodity, amount, passengertype, passengerswanted, target, targettype, targetfaction, false, expiry, influence, reputation);
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

                                journalEvent = new MissionCompletedEvent(timestamp, missionid, name, faction, commodity, amount, false, reward, commodityrewards, donation);
                                handled = true;
                                break;
                            }
                        case "MissionAbandoned":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = getString(data, "Name");
                                journalEvent = new MissionAbandonedEvent(timestamp, missionid, name);
                                handled = true;
                                break;
                            }
                        case "MissionFailed":
                            {
                                object val;
                                data.TryGetValue("MissionID", out val);
                                long missionid = (long)val;
                                string name = getString(data, "Name");
                                journalEvent = new MissionFailedEvent(timestamp, missionid, name);
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

                                journalEvent = new SynthesisedEvent(timestamp, synthesis, materials);
                                handled = true;
                                break;
                            }
                        case "PowerplayJoin":
                            {
                                string power = getString(data, "Power");

                                journalEvent = new PowerJoinedEvent(timestamp, power);
                                handled = true;
                                break;
                            }
                        case "PowerplayLeave":
                            {
                                string power = getString(data, "Power");

                                journalEvent = new PowerLeftEvent(timestamp, power);
                                handled = true;
                                break;
                            }
                        case "PowerplayDefect":
                            {
                                string frompower = getString(data, "FromPower");
                                string topower = getString(data, "ToPower");

                                journalEvent = new PowerDefectedEvent(timestamp, frompower, topower);
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

                                journalEvent = new PowerPreparationVoteCast(timestamp, power, system, amount);
                                handled = true;
                                break;
                            }
                        case "PowerplaySalary":
                            {
                                object val;
                                string power = getString(data, "Power");
                                data.TryGetValue("Amount", out val);
                                int amount = (int)(long)val;

                                journalEvent = new PowerSalaryClaimedEvent(timestamp, power, amount);
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

                                journalEvent = new PowerCommodityObtainedEvent(timestamp, power, commodity, amount);
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

                                journalEvent = new PowerCommodityDeliveredEvent(timestamp, power, commodity, amount);
                                handled = true;
                                break;
                            }
                        case "PowerplayFastTrack":
                            {
                                object val;
                                string power = getString(data, "Power");
                                data.TryGetValue("Cost", out val);
                                int amount = (int)(long)val;

                                journalEvent = new PowerCommodityFastTrackedEvent(timestamp, power, amount);
                                handled = true;
                                break;
                            }
                        case "PowerplayVoucher":
                            {
                                object val;
                                string power = getString(data, "Power");
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
            if (val is long)
            {
                return (decimal?)(long?)val;
            }
            else
            {
                return (decimal?)(double?)val;
            }
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

        public void Handle(Event @event)
        {
        }
    }
}
