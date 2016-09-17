using EDDI;
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

        public JournalMonitor() : base(GetSavedGamesDir(), @"^journal\.[0-9\.]\.+log$", result => HandleJournalEntry(result, Eddi.Instance.eventHandler)) {}

        private static void HandleJournalEntry(string line, Action<Event> callback)
        {
            Match match = JsonRegex.Match(line);
            if (match.Success)
            {
                IDictionary<string, object> data = DeserializeData(line);

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
                    return;
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
                            string allegiance = (string)val;
                            data.TryGetValue("Faction", out val);
                            string faction = (string)val;
                            data.TryGetValue("FactionState", out val);
                            string factionState = (string)val;
                            data.TryGetValue("Economy", out val);
                            string economy = (string)val;
                            data.TryGetValue("Government", out val);
                            string government = (string)val;
                            data.TryGetValue("Security", out val);
                            string security = (string)val;
                            journalEvent = new DockedEvent(timestamp, systemName, stationName, allegiance, faction, factionState, economy, government, security);
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
                            decimal latitude = (decimal)val;
                            data.TryGetValue("Longitude", out val);
                            decimal longitude = (decimal)val;
                            journalEvent = new TouchdownEvent(timestamp, longitude, latitude);
                        }
                        handled = true;
                        break;
                    case "Liftoff":
                        {
                            object val;
                            data.TryGetValue("Latitude", out val);
                            decimal latitude = (decimal)val;
                            data.TryGetValue("Longitude", out val);
                            decimal longitude = (decimal)val;
                            journalEvent = new LiftoffEvent(timestamp, longitude, latitude);
                        }
                        handled = true;
                        break;
                    case "SupercruiseEntry":
                        journalEvent = new EnteredSupercruiseEvent(timestamp);
                        handled = true;
                        break;
                    case "SupercruiseExit":
                        {
                            object val;
                            data.TryGetValue("StarSystem", out val);
                            string system = (string)val;
                            data.TryGetValue("Body", out val);
                            string body = (string)val;
                            journalEvent = new EnteredNormalSpaceEvent(timestamp, system, body);
                        }
                        handled = true;
                        break;
                    case "FSDJump":
                        {
                            object val;

                            data.TryGetValue("StarSystem", out val);
                            string systemName = (string)val;

                            // The system co-ordinates are locked away in a StarPos field, which is the co-ordinates surrounded by square brackets
                            data.TryGetValue("StarPos", out val);
                            string starPos = (string)val;
                            Regex coordsRegex = new Regex(@"^\[(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+)\]$");
                            Match coordsMatch = coordsRegex.Match(starPos);
                            // Co-ordinates are only to 3dp so do a bit of math to calculate the correct values
                            decimal x = Math.Round(decimal.Parse(coordsMatch.Groups[1].Value) * 32) / (decimal)32.0;
                            decimal y = Math.Round(decimal.Parse(coordsMatch.Groups[2].Value) * 32) / (decimal)32.0;
                            decimal z = Math.Round(decimal.Parse(coordsMatch.Groups[3].Value) * 32) / (decimal)32.0;

                            data.TryGetValue("Allegiance", out val);
                            string allegiance = (string)val;
                            data.TryGetValue("Faction", out val);
                            string faction = (string)val;
                            data.TryGetValue("FactionState", out val);
                            string factionState = (string)val;
                            data.TryGetValue("Economy", out val);
                            string economy = (string)val;
                            data.TryGetValue("Government", out val);
                            string government = (string)val;
                            data.TryGetValue("Security", out val);
                            string security = (string)val;

                            journalEvent = new JumpedEvent(timestamp, systemName, x, y, z, allegiance, faction, factionState, economy, government, security);
                        }
                        handled = true;
                        break;
                    case "Location":
                        {
                            object val;

                            data.TryGetValue("StarSystem", out val);
                            string systemName = (string)val;

                            // The system co-ordinates are locked away in a StarPos field, which is the co-ordinates surrounded by square brackets
                            data.TryGetValue("StarPos", out val);
                            string starPos = (string)val;
                            Regex coordsRegex = new Regex(@"^\[(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+)\]$");
                            Match coordsMatch = coordsRegex.Match(starPos);
                            // Co-ordinates are only to 3dp so do a bit of math to calculate the correct values
                            decimal x = Math.Round(decimal.Parse(coordsMatch.Groups[1].Value) * 32) / (decimal)32.0;
                            decimal y = Math.Round(decimal.Parse(coordsMatch.Groups[2].Value) * 32) / (decimal)32.0;
                            decimal z = Math.Round(decimal.Parse(coordsMatch.Groups[3].Value) * 32) / (decimal)32.0;

                            data.TryGetValue("Allegiance", out val);
                            string allegiance = (string)val;
                            data.TryGetValue("Faction", out val);
                            string faction = (string)val;
                            data.TryGetValue("FactionState", out val);
                            string factionState = (string)val;
                            data.TryGetValue("Economy", out val);
                            string economy = (string)val;
                            data.TryGetValue("Government", out val);
                            string government = (string)val;
                            data.TryGetValue("Security", out val);
                            string security = (string)val;

                            journalEvent = new LocationEvent(timestamp, systemName, x, y, z, allegiance, faction, factionState, economy, government, security);
                        }
                        handled = true;
                        break;
                    case "Bounty":
                        {
                            object val;
                            data.TryGetValue("Faction", out val);
                            string awardingFaction = (string)val;
                            data.TryGetValue("Target", out val);
                            string target = (string)val;
                            data.TryGetValue("Reward", out val);
                            decimal reward = (decimal)val;
                            data.TryGetValue("VictimFaction", out val);
                            string victimFaction = (string)val;
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
                            data.TryGetValue("Reward", out val);
                            decimal reward = (decimal)val;
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
                            data.TryGetValue("Victim", out val);
                            string victim = (string)val;
                            // Might be a fine or a bounty
                            if (data.ContainsKey("Fine"))
                            {
                                data.TryGetValue("Fine", out val);
                                decimal fine = (decimal)val;
                                journalEvent = new FineIncurredEvent(timestamp, crimetype, faction, victim, fine);
                            }
                            else
                            {
                                data.TryGetValue("Bounty", out val);
                                decimal bounty = (decimal)val;
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
                                int rank = (int)val;
                                journalEvent = new CombatPromotionEvent(timestamp, rank);
                                handled = true;
                            }
                            else if (data.ContainsKey("Trade"))
                            {
                                data.TryGetValue("Trade", out val);
                                int rank = (int)val;
                                journalEvent = new TradePromotionEvent(timestamp, rank);
                                handled = true;
                            }
                            else if (data.ContainsKey("Explore"))
                            {
                                data.TryGetValue("Explore", out val);
                                int rank = (int)val;
                                journalEvent = new ExplorationPromotionEvent(timestamp, rank);
                                handled = true;
                            }
                        }
                        break;
                    case "CollectCargo":
                        {
                            object val;
                            data.TryGetValue("Type", out val);
                            string cargo = (string)val;
                            data.TryGetValue("Stolen", out val);
                            bool stolen = (bool)val;
                            journalEvent = new CargoCollectedEvent(timestamp, cargo, stolen);
                            handled = true;
                        }
                        handled = true;
                        break;
                    case "EjectCargo":
                        {
                            object val;
                            data.TryGetValue("Type", out val);
                            string cargo = (string)val;
                            data.TryGetValue("Amount", out val);
                            int amount = (int)val;
                            data.TryGetValue("Abandoned", out val);
                            bool abandoned = (bool)val;
                            journalEvent = new CargoEjectedEvent(timestamp, cargo, amount, abandoned);
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
                            decimal distanceFromArrivalLs = (decimal)val;
                            if (data.ContainsKey("StarType"))
                            {
                                // Star
                                data.TryGetValue("StarType", out val);
                                string starType = (string)val;

                                data.TryGetValue("StellarMass", out val);
                                decimal stellarMass = (decimal)val;

                                data.TryGetValue("Radius", out val);
                                decimal radius = (decimal)val;

                                data.TryGetValue("AbsoluteMagnitude", out val);
                                decimal absoluteMagnitude = (decimal)val;

                                journalEvent = new StarScannedEvent(timestamp, name, starType, stellarMass, radius, absoluteMagnitude);
                                handled = true;
                            }
                            else
                            {
                                // Body
                            }
                        }
                        //if (data.ContainsKey("StarType"))
                        //{
                        //    journalEntry.type = "Star scanned";
                        //    journalEntry.stringData.Add("type", (string)data["StarType"]);
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        //if (data.ContainsKey("Landable"))
                        //{
                        //    journalEntry.type = "Body scanned";
                        //    journalEntry.boolData.Add("landable", Int32.Parse("Landable") == 1);
                        //    // TODO add materials
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        break;
                    case "ShipyardBuy":
                        //journalEntry.type = "Ship swapped";
                        //journalEntry.boolData.Add("new", true);
                        //journalEntry.refetchProfile = true;
                        handled = true;
                        break;
                    case "ShipyardSwap":
                        //journalEntry.type = "Ship swapped";
                        //journalEntry.boolData.Add("new", false);
                        //journalEntry.refetchProfile = true;
                        handled = true;
                        break;
                    case "LaunchSRV":
                        //journalEntry.type = "SRV deployed";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "DockSRV":
                        //journalEntry.type = "SRV docked";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "LaunchFighter":
                        //journalEntry.type = "Fighter deployed";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "DockFighter":
                        //journalEntry.type = "Fighter docked";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "ReceiveText":
                        //journalEntry.type = "Message received";
                        //journalEntry.stringData.Add("from", (string)data["from"]);
                        //journalEntry.stringData.Add("message", (string)data["message"]);
                        //journalEntry.refetchProfile = false;
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
                            int landingPad = (int)val;
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
                    case "HeatWarning":
                        journalEvent = new HeatWarningEvent(timestamp);
                        handled = true;
                        break;
                    case "HeatDamage":
                        journalEvent = new HeatDamageEvent(timestamp);
                        handled = true;
                        break;
                    case "HullDamage":
                        //journalEntry.type = "Ship hull damaged";
                        //journalEntry.refetchProfile = true;
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
                    case "Died":
                        //journalEntry.type = "Died";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "MarketBuy":
                        {
                            object val;
                            data.TryGetValue("Type", out val);
                            string cargo = (string)val;
                            data.TryGetValue("Count", out val);
                            int amount = (int)val;
                            data.TryGetValue("BuyPrice", out val);
                            decimal price = (decimal)val;
                            journalEvent = new BoughtFromMarketEvent(timestamp, cargo, amount, price);
                            handled = true;
                            break;
                        }
                    case "MarketSell":
                        {
                            object val;
                            data.TryGetValue("Type", out val);
                            string cargo = (string)val;
                            data.TryGetValue("Count", out val);
                            int amount = (int)val;
                            data.TryGetValue("SellPrice", out val);
                            decimal price = (decimal)val;
                            data.TryGetValue("AvgPricePaid", out val);
                            decimal buyPrice = (decimal)val;
                            // We don't care about buy price, we care about profit per unit
                            decimal profit = price - buyPrice;
                            journalEvent = new SoldToMarketEvent(timestamp, cargo, amount, price, profit);
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
                            data.TryGetValue("Cost", out val);
                            decimal price = (decimal)val;
                            data.TryGetValue("CombatRank", out val);
                            int combatRank = (int)val;
                            journalEvent = new CrewHiredEvent(timestamp, name, faction, price, combatRank);
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
                            data.TryGetValue("ole", out val);
                            string role = (string)val;
                            journalEvent = new CrewAssignedEvent(timestamp, name, role);
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
                        callback(journalEvent);
                    }
                }
                else
                {
                    Logging.Debug("Unhandled event: " + data);
                }
            }
        }

        private static IDictionary<string, object> DeserializeData(string data)
        {
            Logging.Debug("Deserializing " + data);
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

            return DeserializeData(values);
        }

        private static IDictionary<string, object> DeserializeData(JObject data)
        {
            var dict = data.ToObject<Dictionary<string, Object>>();
            if (dict != null)
            {
                return DeserializeData(dict);
            }
            else
            {
                return null;
            }
        }

        private static IDictionary<string, object> DeserializeData(IDictionary<string, object> data)
        {
            foreach (var key in data.Keys.ToArray())
            {
                var value = data[key];

                if (value is JObject)
                    data[key] = DeserializeData(value as JObject);

                if (value is JArray)
                    data[key] = DeserializeData(value as JArray);
            }

            return data;
        }

        private static IList<Object> DeserializeData(JArray data)
        {
            var list = data.ToObject<List<Object>>();

            for (int i = 0; i < list.Count; i++)
            {
                var value = list[i];

                if (value is JObject)
                    list[i] = DeserializeData(value as JObject);

                if (value is JArray)
                    list[i] = DeserializeData(value as JArray);
            }
            return list;
        }

        public string MonitorName()
        {
            return "Journal Monitor";
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return "Plugin to monitor the journal and post relevant events";
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
