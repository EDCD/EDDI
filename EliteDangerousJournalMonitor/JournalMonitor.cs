using EDDI;
using EliteDangerousDataDefinitions;
using EliteDangerousEvents;
using EliteDangerousNetLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EliteDangerousJournalMonitor
{
    public class JournalMonitor : LogMonitor, EDDIMonitor
    {
        private static Regex JsonRegex = new Regex(@"^{.*}$");

        public JournalMonitor() : base(GetSavedGamesDir(), @"^Journal\.[0-9\.]+\.log$", result => ForwardJournalEntry(result, Eddi.Instance.eventHandler)) {}

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
                            timestamp = (DateTime)data["timestamp"];
                        }
                        else
                        {
                            timestamp = DateTime.Parse((string)data["timestamp"]);
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
                                data.TryGetValue("Allegiance", out val);
                                Superpower allegiance = Superpower.FromEDName((string)val);
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.FromEDName(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("FactionState", out val);
                                State factionState = State.FromEDName((string)val);
                                data.TryGetValue("Economy", out val);
                                Economy economy = Economy.FromEDName((string)val);
                                data.TryGetValue("Government", out val);
                                Government government = Government.FromEDName((string)val);
                                data.TryGetValue("Security", out val);
                                SecurityLevel securityLevel = SecurityLevel.FromEDName((string)val);
                                journalEvent = new DockedEvent(timestamp, systemName, stationName, allegiance, faction, factionState, economy, government, securityLevel);
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

                                data.TryGetValue("Allegiance", out val);
                                Superpower allegiance = Superpower.FromEDName((string)val);
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.FromEDName(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("FactionState", out val);
                                State factionState = State.FromEDName((string)val);
                                data.TryGetValue("Economy", out val);
                                Economy economy = Economy.FromEDName((string)val);
                                data.TryGetValue("Government", out val);
                                Government government = Government.FromEDName((string)val);
                                data.TryGetValue("Security", out val);
                                SecurityLevel security = SecurityLevel.FromEDName((string)val);

                                journalEvent = new JumpedEvent(timestamp, systemName, x, y, z, allegiance, faction, factionState, economy, government, security);
                            }
                            handled = true;
                            break;
                        case "Location":
                            {
                                object val;

                                data.TryGetValue("StarSystem", out val);
                                string systemName = (string)val;

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
                                data.TryGetValue("Allegiance", out val);
                                Superpower allegiance = Superpower.FromEDName((string)val);
                                data.TryGetValue("Faction", out val);
                                string faction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.FromEDName(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("FactionState", out val);
                                State factionState = State.FromEDName((string)val);
                                data.TryGetValue("Economy", out val);
                                Economy economy = Economy.FromEDName((string)val);
                                data.TryGetValue("Government", out val);
                                Government government = Government.FromEDName((string)val);
                                data.TryGetValue("Security", out val);
                                SecurityLevel security = SecurityLevel.FromEDName((string)val);


                                journalEvent = new LocationEvent(timestamp, systemName, x, y, z, body, bodyType, docked, allegiance, faction, factionState, economy, government, security);
                            }
                            handled = true;
                            break;
                        case "Bounty":
                            {
                                object val;
                                data.TryGetValue("Faction", out val);
                                string awardingFaction = (string)val;
                                // Might be a superpower...
                                Superpower superpowerFaction = Superpower.FromEDName(awardingFaction);
                                awardingFaction = superpowerFaction != null ? superpowerFaction.name : awardingFaction;
                                data.TryGetValue("Target", out val);
                                string target = (string)val;
                                data.TryGetValue("Reward", out val);
                                long reward = (long)val;
                                data.TryGetValue("VictimFaction", out val);
                                string victimFaction = (string)val;
                                // Might be a superpower...
                                superpowerFaction = Superpower.FromEDName(victimFaction);
                                victimFaction = superpowerFaction != null ? superpowerFaction.name : victimFaction;

                                journalEvent = new BountyAwardedEvent(timestamp, awardingFaction, target, victimFaction, reward);
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
                                Superpower superpowerFaction = Superpower.FromEDName(awardingFaction);
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
                                Superpower superpowerFaction = Superpower.FromEDName(faction);
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
                                decimal orbitalperiod = (decimal)(double)val;

                                data.TryGetValue("RotationPeriod", out val);
                                decimal rotationperiod = (decimal)(double)val;

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

                                    data.TryGetValue("Age", out val);
                                    // TODO remove the conditional here and elsewhere when the new version of the journal arrives
                                    long age = (val == null ? 8000000 : (long)val);

                                    data.TryGetValue("Temperature", out val);
                                    decimal temperature = (val == null ? 3000 : (decimal)(double)val);

                                    journalEvent = new StarScannedEvent(timestamp, name, starType, stellarMass, radius, absoluteMagnitude, age, temperature, distancefromarrival, orbitalperiod, rotationperiod, rings);
                                    handled = true;
                                }
                                else
                                {
                                    // Body
                                    data.TryGetValue("TidalLock", out val);
                                    bool tidallyLocked = (bool)val;

                                    data.TryGetValue("PlanetClass", out val);
                                    string bodyClass = (string)val;

                                    data.TryGetValue("SurfaceGravity", out val);
                                    decimal gravity = (decimal)(double)val;

                                    data.TryGetValue("SurfaceTemperature", out val);
                                    decimal temperature = (decimal)(double)val;

                                    data.TryGetValue("SurfacePressure", out val);
                                    decimal pressure = (decimal)(double)val;

                                    data.TryGetValue("Landable", out val);
                                    bool landable = (bool)val;

                                    data.TryGetValue("Materials", out val);
                                    IDictionary<string, object> materialsData = (IDictionary<string, object>)val;
                                    List<MaterialPresence> materials = new List<MaterialPresence>();
                                    if (materialsData != null)
                                    {
                                        foreach (KeyValuePair<string, object> kv in materialsData)
                                        {
                                            Material material = Material.FromEDName(kv.Key);
                                            if (material != null)
                                            {
                                                materials.Add(new MaterialPresence(material, (decimal)(double)kv.Value));
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

                                    journalEvent = new BodyScannedEvent(timestamp, name, bodyClass, gravity, temperature, pressure, tidallyLocked, landable, atmosphere, volcanism, materials);
                                    handled = true;
                                }
                            }
                            break;
                        case "ShipyardBuy":
                            {
                                object val;
                                // We don't have a ship ID at this point so use the ship type
                                data.TryGetValue("ShipType", out val);
                                Ship ship = ShipDefinitions.FromEDModel((string)val);

                                Ship storedShip = null;
                                data.TryGetValue("StoreShipID", out val);
                                if (val != null)
                                {
                                    // We are storing a ship as part of the swap
                                    int storedShipId = (int)(long)val;

                                    // We should be storing our own ship; confirm this using the ship ID
                                    if (storedShipId == Eddi.Instance.Ship.LocalId)
                                    {
                                        storedShip = Eddi.Instance.Ship;
                                    }
                                    else
                                    {
                                        // The ship might already be registered as stored; see if we can find it
                                        storedShip = Eddi.Instance.StoredShips.FirstOrDefault(v => v.LocalId == storedShipId);
                                        if (storedShip == null)
                                        {
                                            // No luck finding the ship; provide a basic definition
                                            data.TryGetValue("StoreOldShip", out val);
                                            ship = ShipDefinitions.FromEDModel((string)val);
                                            ship.LocalId = storedShipId;
                                        }
                                    }
                                }

                                Ship soldShip = null;
                                data.TryGetValue("SellShipID", out val);
                                if (val != null)
                                {
                                    // We are selling a ship as part of the swap
                                    int soldShipId = (int)(long)val;

                                    // We should be selling our current ship; confirm this using the ship ID
                                    if (soldShipId == Eddi.Instance.Ship.LocalId)
                                    {
                                        soldShip = Eddi.Instance.Ship;
                                    }
                                    else
                                    {
                                        // The ship might be registered as stored; see if we can find it
                                        soldShip = Eddi.Instance.StoredShips.FirstOrDefault(v => v.LocalId == soldShipId);
                                        if (soldShip == null)
                                        {
                                            // No luck finding the ship; provide a basic definition
                                            data.TryGetValue("SellOldShip", out val);
                                            soldShip = ShipDefinitions.FromEDModel((string)val);
                                            soldShip.LocalId = soldShipId;
                                        }
                                    }
                                }

                                data.TryGetValue("SellPrice", out val);
                                decimal? soldPrice = (long?)val;
                                journalEvent = new ShipPurchasedEvent(timestamp, ship, soldShip, soldPrice, storedShip);
                            }
                            handled = true;
                            break;
                        case "ShipyardSell":
                            {
                                object val;
                                data.TryGetValue("SellShipID", out val);
                                int shipId = (int)(long)val;
                                // We should be able to provide our ship information given the ship ID
                                Ship ship = Eddi.Instance.StoredShips.FirstOrDefault(v => v.LocalId == shipId);
                                if (ship == null)
                                {
                                    Logging.Debug("Failed to find ship for ID " + shipId + "; using template");
                                    // Failed to find the ship; provide a basic definition
                                    data.TryGetValue("ShipType", out val);
                                    ship = ShipDefinitions.FromEDModel((string)val);
                                    ship.LocalId = shipId;
                                }
                                data.TryGetValue("ShipPrice", out val);
                                decimal price = (long)val;
                                journalEvent = new ShipSoldEvent(timestamp, ship, price);
                            }
                            handled = true;
                            break;
                        case "ShipyardSwap":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                // We should be able to provide our ship information given the ship ID
                                Ship ship = Eddi.Instance.StoredShips.FirstOrDefault(v => v.LocalId == shipId);
                                if (ship == null)
                                {
                                    Logging.Debug("Failed to find ship for ID " + shipId + "; using template");
                                    // Failed to find the ship; provide a basic definition
                                    data.TryGetValue("ShipType", out val);
                                    ship = ShipDefinitions.FromEDModel((string)val);
                                    ship.LocalId = shipId;
                                }

                                Ship storedShip = null;
                                data.TryGetValue("StoreShipID", out val);
                                if (val != null)
                                {
                                    // We are storing a ship as part of the swap
                                    int storedShipId = (int)(long)val;

                                    // We should be storing our own ship; confirm this using the ship ID
                                    if (storedShipId == Eddi.Instance.Ship.LocalId)
                                    {
                                        storedShip = Eddi.Instance.Ship;
                                    }
                                    else
                                    {
                                        // The ship might already be registered as stored; see if we can find it
                                        storedShip = Eddi.Instance.StoredShips.FirstOrDefault(v => v.LocalId == storedShipId);
                                        if (storedShip == null)
                                        {
                                            // No luck finding the ship; provide a basic definition
                                            data.TryGetValue("StoreOldShip", out val);
                                            ship = ShipDefinitions.FromEDModel((string)val);
                                            ship.LocalId = storedShipId;
                                        }
                                    }
                                }

                                Ship soldShip = null;
                                data.TryGetValue("SellShipID", out val);
                                if (val != null)
                                {
                                    // We are selling a ship as part of the swap
                                    int soldShipId = (int)(long)val;

                                    // We should be selling our current ship; confirm this using the ship ID
                                    if (soldShipId == Eddi.Instance.Ship.LocalId)
                                    {
                                        soldShip = Eddi.Instance.Ship;
                                    }
                                    else
                                    {
                                        // The ship might be registered as stored; see if we can find it
                                        soldShip = Eddi.Instance.StoredShips.FirstOrDefault(v => v.LocalId == soldShipId);
                                        if (soldShip == null)
                                        {
                                            // No luck finding the ship; provide a basic definition
                                            data.TryGetValue("SellOldShip", out val);
                                            soldShip = ShipDefinitions.FromEDModel((string)val);
                                            soldShip.LocalId = soldShipId;
                                        }
                                    }
                                }

                                journalEvent = new ShipSwappedEvent(timestamp, ship, soldShip, storedShip);
                            }
                            handled = true;
                            break;
                        case "ShipyardTransfer":
                            {
                                object val;
                                data.TryGetValue("ShipID", out val);
                                int shipId = (int)(long)val;
                                // We should be able to provide our ship information given the ship ID
                                Ship ship = Eddi.Instance.StoredShips.FirstOrDefault(v => v.LocalId == shipId);
                                if (ship == null)
                                {
                                    Logging.Debug("Failed to find ship for ID " + shipId + "; using template");
                                    // Failed to find the ship; provide a basic definition
                                    data.TryGetValue("ShipType", out val);
                                    ship = ShipDefinitions.FromEDModel((string)val);
                                    ship.LocalId = shipId;
                                }

                                data.TryGetValue("System", out val);
                                string system = (string)val;

                                data.TryGetValue("Distance", out val);
                                decimal distance = (decimal)(double)val;

                                data.TryGetValue("TransferPrice", out val);
                                decimal cost = (long)val;

                                journalEvent = new ShipTransferInitiatedEvent(timestamp, ship, system, distance, cost);

                                handled = true;
                            }
                            break;
                        case "LaunchSRV":
                            {
                                object val;
                                data.TryGetValue("Loadout", out val);
                                string loadout = (string)val;
                                journalEvent = new SRVLaunchedEvent(timestamp, loadout);
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
                                int amount = (val == null ? 1 : (int)(long)val);
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
                        case "ReceiveText":
                            {
                                object val;
                                data.TryGetValue("From", out val);
                                string from = (string)val;
                                from = from.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "");
                                data.TryGetValue("Message", out val);
                                string message = (string)val;
                                journalEvent = new MessageReceivedEvent(timestamp, from, message);
                            }
                            handled = true;
                            break;
                        case "SendText":
                            {
                                object val;
                                data.TryGetValue("To", out val);
                                string to = (string)val;
                                to = to.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "");
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
                                journalEvent = new HullDamagedEvent(timestamp, health);
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
                                decimal cost = (long)val;
                                journalEvent = new ExplorationDataPurchasedEvent(timestamp, system, cost);
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
                                decimal price = (long)val;
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
                                decimal price = (long)val;
                                data.TryGetValue("AvgPricePaid", out val);
                                decimal buyPrice = (long)val;
                                // We don't care about buy price, we care about profit per unit
                                decimal profit = price - buyPrice;
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
                        case "LoadGame":
                            {
                                object val;
                                data.TryGetValue("Commander", out val);
                                string commander = (string)val;
                                data.TryGetValue("Ship", out val);
                                Ship ship = ShipDefinitions.FromEDModel((string)val);
                                data.TryGetValue("GameMode", out val);
                                GameMode mode = GameMode.FromEDName((string)val);
                                data.TryGetValue("Group", out val);
                                string group = (string)val;
                                data.TryGetValue("Credits", out val);
                                decimal credits = (long)val;
                                journalEvent = new CommanderContinuedEvent(timestamp, commander, ship, mode, group, credits);
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
                                Superpower superpowerFaction = Superpower.FromEDName(faction);
                                faction = superpowerFaction != null ? superpowerFaction.name : faction;
                                data.TryGetValue("Cost", out val);
                                decimal price = (long)val;
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
                                decimal cost = (long)val;

                                journalEvent = new TradeDataPurchasedEvent(timestamp, system, cost);
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
                Logging.Error(@"Exception whilst parsing line """ + line + @""": " + ex.StackTrace);
            }
            return null;
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
