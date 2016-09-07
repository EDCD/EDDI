using EliteDangerousEvents;
using EliteDangerousNetLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousJournalMonitor
{
    public class JournalMonitor : LogMonitor
    {
        private static Regex JsonRegex = new Regex(@"^{.*}$");
        public JournalMonitor(NetLogConfiguration configuration, Action<Event> callback) : base(configuration.path, @"^journal\.log$", (result) => HandleJournalEntry(result, callback))
        {
        }

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
                            data.TryGetValue("StationName", out val);
                            string stationName = (string)val;
                            journalEvent = new DockedEvent(timestamp, stationName);
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
                        //journalEntry.type = "Ship landed";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "Liftoff":
                        //journalEntry.type = "Ship lifted off";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "SupercruiseEntry":
                        journalEvent = new EnteredSupercruiseEvent(timestamp);
                        handled = true;
                        break;
                    case "SupercruiseExit":
                        {
                            object val;
                            data.TryGetValue("Body", out val);
                            string bodyName = (string)val;
                            journalEvent = new EnteredNormalSpaceEvent(timestamp, bodyName);
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

                            journalEvent = new JumpedEvent(timestamp, systemName, x, y, z);
                        }
                        handled = true;
                        break;
                    case "Died":
                        //journalEntry.type = "Died";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "Bounty":
                        //journalEntry.type = "Bounty awarded";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "CapShipBond":
                    case "FactionKillBond":
                        //journalEntry.type = "Bond awarded";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "CommitCrime":
                        // Might be a fine or a bounty
                        //if (data.ContainsKey("Fine"))
                        //{
                        //    journalEntry.type = "Fine incurred";
                        //    journalEntry.decimalData.Add("amount", Decimal.Parse((string)data["Fine"]));
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        //else
                        //{
                        //    journalEntry.type = "Bounty incurred";
                        //    journalEntry.decimalData.Add("amount", Decimal.Parse((string)data["Bounty"]));
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        //journalEntry.refetchProfile = false;
                        break;
                    case "Promotion":
                        //if (data.ContainsKey("Combat"))
                        //{
                        //    journalEntry.type = "Combat promotion";
                        //    journalEntry.intData.Add("Rating", Int32.Parse((string)data["Combat"]));
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        //if (data.ContainsKey("Trade"))
                        //{
                        //    journalEntry.type = "Trade promotion";
                        //    journalEntry.intData.Add("Rating", Int32.Parse((string)data["Trade"]));
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        //if (data.ContainsKey("Explore"))
                        //{
                        //    journalEntry.type = "Exploration promotion";
                        //    journalEntry.intData.Add("Rating", Int32.Parse((string)data["Explore"]));
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        //if (data.ContainsKey("CQC"))
                        //{
                        //    journalEntry.type = "CQC promotion";
                        //    journalEntry.intData.Add("Rating", Int32.Parse((string)data["CQC"]));
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        break;
                    case "CollectCargo":
                        //if ((int)data["Stolen"] == 1)
                        //{
                        //    journalEntry.type = "Illegal cargo scooped";
                        //    // TODO obtain and translate cargo type
                        //    journalEntry.refetchProfile = false;
                        //    handled = true;
                        //}
                        handled = true;
                        break;
                    case "CockpitBreached":
                        //journalEntry.type = "Cockpit breached";
                        //handled = true;
                        break;
                    case "Scan":
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
                        //journalEntry.type = "Docking request sent";
                        //journalEntry.stringData.Add("station", (string)data["stationname"]);
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "DockingGranted":
                        //journalEntry.type = "Docking request granted";
                        //journalEntry.stringData.Add("station", (string)data["stationname"]);
                        //journalEntry.intData.Add("pad", (int)data["landingpad"]);
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "DockingDenied":
                        //journalEntry.type = "Docking request denied";
                        //journalEntry.stringData.Add("station", (string)data["stationname"]);
                        //journalEntry.stringData.Add("reason", (string)data["reason"]);
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "DockingCancelled":
                        //journalEntry.type = "Docking request cancelled";
                        //journalEntry.stringData.Add("station", (string)data["stationname"]);
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "DockingTimeout":
                        //journalEntry.type = "Docking request timed out";
                        //journalEntry.stringData.Add("station", (string)data["stationname"]);
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "HeatWarning":
                        //journalEntry.type = "Ship overheating";
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
                    case "HullDamage":
                        //journalEntry.type = "Ship hull damaged";
                        //journalEntry.refetchProfile = true;
                        handled = true;
                        break;
                    case "ShieldState":
                        //if ((int)data["ShieldsUp"] == 1)
                        //{
                        //    journalEntry.type = "Shields up";
                        //}
                        //else
                        //{
                        //    journalEntry.type = "Shields down";
                        //}
                        //journalEntry.refetchProfile = false;
                        handled = true;
                        break;
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
            var dict = data.ToObject<Dictionary<String, Object>>();
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
    }
}
