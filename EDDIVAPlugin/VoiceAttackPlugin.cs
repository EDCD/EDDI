using EliteDangerousCompanionAppService;
using EliteDangerousDataProviderService;
using EliteDangerousDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using EliteDangerousNetLogMonitor;
using System.Threading;
using System.Diagnostics;
using EliteDangerousStarMapService;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using EliteDangerousSpeechService;
using Utilities;
using EliteDangerousJournalMonitor;
using EDDI;
using EliteDangerousEvents;

namespace EDDIVAPlugin
{
    public class VoiceAttackPlugin
    {
        public static BlockingCollection<Event> EventQueue = new BlockingCollection<Event>();

        private static SpeechService speechService = new SpeechService(SpeechServiceConfiguration.FromFile());

        public static string VA_DisplayName()
        {
            return "EDDI " + Eddi.EDDI_VERSION;
        }

        public static string VA_DisplayInfo()
        {
            return "Elite: Dangerous Data Interface\r\nVersion " + Eddi.EDDI_VERSION;
        }

        public static Guid VA_Id()
        {
            return new Guid("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");
        }

        public static void VA_Init1(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Info("Initialising EDDI VoiceAttack plugin");

            try
            {
                // Set the initial values from the main EDDI objects
                setValues(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

                Eddi.Instance.Start();

                setString(ref textValues, "EDDI plugin profile status", "Enabled");
                Logging.Info("Initialised EDDI VoiceAttack plugin");
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to initialise", e);
            }
        }

        public static void VA_Exit1(ref Dictionary<string, object> state)
        {
            Eddi.Instance.Stop();
        }

        public static void VA_Invoke1(String context, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                switch (context)
                {
                    case "coriolis":
                        InvokeCoriolis(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "eddbsystem":
                        InvokeEDDBSystem(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "eddbstation":
                        InvokeEDDBStation(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "profile":
                        InvokeUpdateProfile(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "say":
                        InvokeSay(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "transmit":
                        InvokeTransmit(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "receive":
                        InvokeReceive(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "generate callsign":
                        InvokeGenerateCallsign(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "event queue":
                        InvokeEventQueue(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    case "system note":
                        InvokeStarMapSystemComment(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        break;
                    default:
                        //if (context.ToLower().StartsWith("event:"))
                        //{
                        //    // Inject an event
                        //    string data = context.Replace("event: ", "");
                        //    JObject eventData = JObject.Parse(data);
                        //    //LogQueue.Add(eventData);
                        //}
                        break;
                 }
                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to invoke action", e);
            }
            Logging.Debug("Leaving");
        }

        /// <summary>Force-update EDDI's information</summary>
        private static void InvokeUpdateProfile(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Eddi.Instance.refreshProfile();
            setValues(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
        }

        public static void InvokeEventQueue(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            bool somethingToReport = false;
            while (!somethingToReport)
            {
                try
                {
                    Event theEvent = EventQueue.Take();
                    setString(ref textValues, "EDDI event", theEvent.type);
                    // Update all values
                    setValues(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    // Event-specific information
                    if (theEvent is EnteredNormalSpaceEvent)
                    {
                        setString(ref textValues, "EDDI event body", ((EnteredNormalSpaceEvent)theEvent).body);
                    }
                    setPluginStatus(ref textValues, "Operational", null, null);
                    somethingToReport = true;
                }
                catch (Exception e)
                {
                    setPluginStatus(ref textValues, "Failed", "Failed to send ship data to coriolis", e);
                }
                Logging.Debug("Leaving");
            }
        }

        public static void InvokeEDDBSystem(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Entered");
            try
            {
                if (Eddi.Instance.CurrentStarSystem == null)
                {
                    Logging.Debug("No information on current system");
                    return;
                }
                string systemUri = "https://eddb.io/system/" + Eddi.Instance.CurrentStarSystem.EDDBID;

                Logging.Debug("Starting process with uri " + systemUri);

                setPluginStatus(ref textValues, "Operational", null, null);

                Process.Start(systemUri);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to send system data to EDDB", e);
            }
            Logging.Debug("Leaving");
        }

        public static void InvokeEDDBStation(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Entered");
            try
            {
                if (Eddi.Instance.CurrentStarSystem == null)
                {
                    Logging.Debug("No information on current station");
                    return;
                }
                Station thisStation = Eddi.Instance.CurrentStarSystem.stations.SingleOrDefault(s => s.name == Eddi.Instance.LastStation.name);
                if (thisStation == null)
                {
                    // Missing current star system information
                    Logging.Debug("No information on current station");
                    return;
                }
                string stationUri = "https://eddb.io/station/" + thisStation.EDDBID;

                Logging.Debug("Starting process with uri " + stationUri);

                Process.Start(stationUri);

                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to send station data to EDDB", e);
            }
            Logging.Debug("Leaving");
        }

        public static void InvokeCoriolis(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Entered");
            try
            {
                if (Eddi.Instance.Ship == null)
                {
                    Logging.Debug("No information on ship");
                    return;
                }

                string shipUri = Coriolis.ShipUri(Eddi.Instance.Ship);

                Logging.Debug("Starting process with uri " + shipUri);

                Process.Start(shipUri);

                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to send ship data to coriolis", e);
            }
            Logging.Debug("Leaving");
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleValues(Module module, string name, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            setString(ref textValues, name, module == null ? null : module.Name);
            setInt(ref intValues, name + " class", module == null ? (int?)null : module.Class);
            setString(ref textValues, name + " grade", module == null ? null : module.Grade);
            setDecimal(ref decimalValues, name + " health", module == null ? (decimal?)null : module.Health);
            setDecimal(ref decimalValues, name + " cost", module == null ? (decimal?)null : (decimal)module.Cost);
            setDecimal(ref decimalValues, name + " value", module == null ? (decimal?)null : (decimal)module.Value);
            if (module != null && module.Cost < module.Value)
            {
                decimal discount = Math.Round((1 - (((decimal)module.Cost) / ((decimal)module.Value))) * 100, 1);
                setDecimal(ref decimalValues, name + " discount", discount > 0.01M ? discount : (decimal?)null);
            }
            else
            {
                setDecimal(ref decimalValues, name + " discount", null);
            }
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleOutfittingValues(Module existing, List<Module> outfittingModules, string name, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            if (existing != null && outfittingModules != null)
            {
                foreach (Module Module in outfittingModules)
                {
                    if (existing.EDDBID == Module.EDDBID)
                    {
                        // Found it
                        setDecimal(ref decimalValues, name + " station cost", (decimal?)Module.Cost);
                        if (Module.Cost < existing.Cost)
                        {
                            // And it's cheaper
                            setDecimal(ref decimalValues, name + " station discount", existing.Cost - Module.Cost);
                            setString(ref textValues, name + " station discount (spoken)", Translations.Humanize(existing.Cost - Module.Cost));
                        }
                        return;
                    }
                }
            }
            // Not found so remove any existing
            setDecimal(ref decimalValues, "Ship " + name + " station cost", (decimal?)null);
            setDecimal(ref decimalValues, "Ship " + name + " station discount", (decimal?)null);
            setString(ref textValues, "Ship " + name + " station discount (spoken)", (string)null);
        }


        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        public static void InvokeSay(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string script = null;
                foreach (string key in textValues.Keys)
                {
                    if (key.EndsWith(" script"))
                    {
                        script = textValues[key];
                        break;
                    }
                }
                if (script == null)
                {
                    return;
                }
                speechService.Say(Eddi.Instance.Cmdr, Eddi.Instance.Ship, script);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to run internal speech system", e);
            }
        }

        /// <summary>Transmit something on the radio with text-to-speech</summary>
        public static void InvokeTransmit(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string script = null;
                foreach (string key in textValues.Keys)
                {
                    if (key.EndsWith(" script"))
                    {
                        script = textValues[key];
                        break;
                    }
                }
                if (script == null)
                {
                    return;
                }
                speechService.Transmit(Eddi.Instance.Cmdr, Eddi.Instance.Ship, script);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to run internal speech system", e);
            }
        }

        /// <summary>Receive something on the radio with text-to-speech</summary>
        public static void InvokeReceive(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string script = null;
                foreach (string key in textValues.Keys)
                {
                    if (key.EndsWith(" script"))
                    {
                        script = textValues[key];
                        break;
                    }
                }
                if (script == null)
                {
                    return;
                }
                speechService.Receive(Eddi.Instance.Cmdr, Eddi.Instance.Ship, script);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to run internal speech system", e);
            }
        }

        /// <summary>
        /// Generate a callsign
        /// </summary>
        public static void InvokeGenerateCallsign(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string callsign = Ship.generateCallsign();
                setString(ref textValues, "EDDI generated callsign", callsign);
                setString(ref textValues, "EDDI generated callsign (spoken)", Translations.CallSign(callsign));
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to generate callsign", e);
            }
        }

        /// <summary>
        /// Send a comment to the starmap service and store locally
        /// </summary>
        public static void InvokeStarMapSystemComment(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            try
            {
                string comment = null;
                foreach (string key in textValues.Keys)
                {
                    if (key == "EDDI system comment")
                    {
                        comment = textValues[key];
                    }
                }
                if (comment == null)
                {
                    return;
                }

                if (Eddi.Instance.Cmdr != null)
                {
                    // Store locally
                    StarSystem here = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(Eddi.Instance.CurrentStarSystem.name);
                    here.comment = comment == "" ? null : comment;
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(here);

                    if (Eddi.Instance.starMapService != null)
                    {
                        // Store in EDSM
                        Eddi.Instance.starMapService.sendStarMapComment(Eddi.Instance.CurrentStarSystem.name, comment);
                    }
                }
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to store system comment", e);
            }
        }

        /// <summary>Set all values</summary>
        private static void setValues(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Setting values");
            setCommanderValues(Eddi.Instance.Cmdr, ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            setShipValues(Eddi.Instance.Ship, "Ship", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            int currentStoredShip = 1;
            foreach (Ship StoredShip in Eddi.Instance.StoredShips)
            {
                setShipValues(StoredShip, "Stored ship " + currentStoredShip, ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                currentStoredShip++;
            }
            setStarSystemValues(Eddi.Instance.CurrentStarSystem, "System", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            setStarSystemValues(Eddi.Instance.LastStarSystem, "Last system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
            setStarSystemValues(Eddi.Instance.HomeStarSystem, "Home system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

            // Backwards-compatibility with 1.x
            if (Eddi.Instance.HomeStarSystem != null)
            {
                setString(ref textValues, "Home system", Eddi.Instance.HomeStarSystem.name);
                setString(ref textValues, "Home system (spoken)", Translations.StarSystem(Eddi.Instance.HomeStarSystem.name));
            }
            if (Eddi.Instance.HomeStation != null)
            {
                setString(ref textValues, "Home station", Eddi.Instance.HomeStation.name);
            }

            if (Eddi.Instance.Insurance != null)
            {
                setDecimal(ref decimalValues, "Insurance", Eddi.Instance.Insurance);
            }

            setString(ref textValues, "Environment", Eddi.Instance.Environment);

            Logging.Debug("Set values");
        }

        /// <summary>Set values for a station</summary>
        private static void setStationValues(Station station, string prefix, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Setting station information");

            if (station == null)
            {
                // We don't have any data so remove any info that we might have in history
                setDecimal(ref decimalValues, prefix + " distance from star", null);
                setString(ref textValues, prefix + " government", null);
                setString(ref textValues, prefix + " allegiance", null);
                setString(ref textValues, prefix + " faction", null);
                setString(ref textValues, prefix + " state", null);
                setString(ref textValues, prefix + " primary economy", null);
                setString(ref textValues, prefix + " secondary economy", null);
                setString(ref textValues, prefix + " tertiary economy", null);
                setBoolean(ref booleanValues, prefix + " has refuel", null);
                setBoolean(ref booleanValues, prefix + " has repair", null);
                setBoolean(ref booleanValues, prefix + " has rearm", null);
                setBoolean(ref booleanValues, prefix + " has market", null);
                setBoolean(ref booleanValues, prefix + " has black market", null);
                setBoolean(ref booleanValues, prefix + " has outfitting", null);
                setBoolean(ref booleanValues, prefix + " has shipyard", null);
            }
            else
            {
                setString(ref textValues, prefix + " name", station.name);
                setDecimal(ref decimalValues, prefix + " distance from star", station.distancefromstar);
                setString(ref textValues, prefix + " government", station.government);
                setString(ref textValues, prefix + " allegiance", station.allegiance);
                setString(ref textValues, prefix + " faction", station.faction);
                setString(ref textValues, prefix + " state", station.state);
                if (station.economies != null)
                {
                    if (station.economies.Count > 0)
                    {
                        setString(ref textValues, prefix + " primary economy", station.economies[0]);
                    }
                    if (station.economies.Count > 1)
                    {
                        setString(ref textValues, prefix + " secondary economy", station.economies[1]);
                    }
                    if (station.economies.Count > 2)
                    {
                        setString(ref textValues, prefix + " tertiary economy", station.economies[2]);
                    }
                }
                // Services
                setBoolean(ref booleanValues, prefix + " has refuel", station.hasrefuel);
                setBoolean(ref booleanValues, prefix + " has repair", station.hasrepair);
                setBoolean(ref booleanValues, prefix + " has rearm", station.hasrearm);
                setBoolean(ref booleanValues, prefix + " has market", station.hasmarket);
                setBoolean(ref booleanValues, prefix + " has black market", station.hasblackmarket);
                setBoolean(ref booleanValues, prefix + " has outfitting", station.hasoutfitting);
                setBoolean(ref booleanValues, prefix + " has shipyard", station.hasshipyard);
            }

            Logging.Debug("Set station information");
        }

        private static void setCommanderValues(Commander cmdr, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Setting commander information");
            try
            {
                setString(ref textValues, "Name", cmdr == null ? null : cmdr.name);
                setInt(ref intValues, "Combat rating", cmdr == null ? (int?)null : cmdr.combatrating);
                setString(ref textValues, "Combat rank", cmdr == null ? null : cmdr.combatrank);
                setInt(ref intValues, "Trade rating", cmdr == null ? (int?)null : cmdr.traderating);
                setString(ref textValues, "Trade rank", cmdr == null ? null : cmdr.traderank);
                setInt(ref intValues, "Explore rating", cmdr == null ? (int?)null : cmdr.explorationrating);
                setString(ref textValues, "Explore rank", cmdr == null ? null : cmdr.explorationrank);
                setInt(ref intValues, "Empire rating", cmdr == null ? (int?)null : cmdr.empirerating);
                setString(ref textValues, "Empire rank", cmdr == null ? null : cmdr.empirerank);
                setInt(ref intValues, "Federation rating", cmdr == null ? (int?)null : cmdr.federationrating);
                setString(ref textValues, "Federation rank", cmdr == null ? null : cmdr.federationrank);
                setDecimal(ref decimalValues, "Credits", cmdr == null ? (decimal?)null : cmdr.credits);
                setString(ref textValues, "Credits (spoken)", cmdr == null ? null : Translations.Humanize(cmdr.credits));
                setDecimal(ref decimalValues, "Debt", cmdr == null ? (decimal?)null : cmdr.debt);
                setString(ref textValues, "Debt (spoken)", cmdr == null ? null : Translations.Humanize(cmdr.debt));

                setString(ref textValues, "Title", cmdr == null ? null : cmdr.title);

                // Backwards-compatibility with 1.x
                setString(ref textValues, "System rank", cmdr == null ? null : cmdr.title);

                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to set commander information", e);
            }

            Logging.Debug("Set commander information");
        }

        private static void setShipValues(Ship ship, string prefix, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Setting ship information (" + prefix + ")");
            try
            {
                setString(ref textValues, prefix + " model", ship == null ? null : ship.model);
                setString(ref textValues, prefix + " model (spoken)", ship == null ? null : Translations.ShipModel(ship.model));
                setString(ref textValues, prefix + " callsign", ship == null ? null : ship.callsign);
                setString(ref textValues, prefix + " callsign (spoken)", ship == null ? null : Translations.CallSign(ship.callsign));
                setString(ref textValues, prefix + " name", ship == null ? null : ship.name);
                setString(ref textValues, prefix + " role", ship == null ? null : ship.role.ToString());
                setString(ref textValues, prefix + " size", ship == null ? null : ship.size.ToString());
                setDecimal(ref decimalValues, prefix + " value", ship == null ? (decimal?)null : ship.value);
                setString(ref textValues, prefix + " value (spoken)", ship == null ? null : Translations.Humanize(ship.value));
                setDecimal(ref decimalValues, prefix + " health", ship == null ? (decimal?)null : ship.Health);
                setInt(ref intValues, prefix + " cargo capacity", ship == null ? (int?)null : ship.cargocapacity);
                setInt(ref intValues, prefix + " cargo carried", ship == null ? (int?)null : ship.cargocarried);
                // Add number of limpets carried
                if (ship == null || ship.cargo == null)
                {
                    setInt(ref intValues, prefix + " limpets carried", null);
                }
                else
                {
                    int limpets = 0;
                    foreach (Cargo cargo in ship.cargo)
                    {
                        if (cargo.Commodity.Name == "Limpet")
                        {
                            limpets += cargo.Quantity;
                        }
                    }
                    setInt(ref intValues, prefix + " limpets carried", limpets);
                }

                setShipModuleValues(ship == null ? null : ship.Bulkheads, prefix + " bulkheads", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleOutfittingValues(ship == null ? null : ship.Bulkheads, Eddi.Instance.LastStation.outfitting, prefix + " bulkheads", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleValues(ship == null ? null : ship.PowerPlant, prefix + " power plant", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleOutfittingValues(ship == null ? null : ship.PowerPlant, Eddi.Instance.LastStation.outfitting, prefix + " power plant", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleValues(ship == null ? null : ship.Thrusters, prefix + " thrusters", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleOutfittingValues(ship == null ? null : ship.Thrusters, Eddi.Instance.LastStation.outfitting, prefix + " thrusters", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleValues(ship == null ? null : ship.FrameShiftDrive, prefix + " frame shift drive", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleOutfittingValues(ship == null ? null : ship.FrameShiftDrive, Eddi.Instance.LastStation.outfitting, prefix + " frame shift drive", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleValues(ship == null ? null : ship.LifeSupport, prefix + " life support", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleOutfittingValues(ship == null ? null : ship.LifeSupport, Eddi.Instance.LastStation.outfitting, prefix + " life support", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleValues(ship == null ? null : ship.PowerDistributor, prefix + " power distributor", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleOutfittingValues(ship == null ? null : ship.PowerDistributor, Eddi.Instance.LastStation.outfitting, prefix + " power distributor", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleValues(ship == null ? null : ship.Sensors, prefix + " sensors", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleOutfittingValues(ship == null ? null : ship.Sensors, Eddi.Instance.LastStation.outfitting, prefix + " sensors", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleValues(ship == null ? null : ship.FuelTank, prefix + " fuel tank", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                setShipModuleOutfittingValues(ship == null ? null : ship.FuelTank, Eddi.Instance.LastStation.outfitting, prefix + " fuel tank", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

                // Hardpoints
                if (ship != null)
                {
                    int numTinyHardpoints = 0;
                    int numSmallHardpoints = 0;
                    int numMediumHardpoints = 0;
                    int numLargeHardpoints = 0;
                    int numHugeHardpoints = 0;
                    foreach (Hardpoint Hardpoint in ship.Hardpoints)
                    {
                        string baseHardpointName = prefix;
                        switch (Hardpoint.Size)
                        {
                            case 0:
                                baseHardpointName = prefix + " tiny hardpoint " + ++numTinyHardpoints;
                                break;
                            case 1:
                                baseHardpointName = prefix + " small hardpoint " + ++numSmallHardpoints;
                                break;
                            case 2:
                                baseHardpointName = prefix + " medium hardpoint " + ++numMediumHardpoints;
                                break;
                            case 3:
                                baseHardpointName = prefix + " large hardpoint " + ++numLargeHardpoints;
                                break;
                            case 4:
                                baseHardpointName = prefix + " huge hardpoint " + ++numHugeHardpoints;
                                break;
                        }

                        setBoolean(ref booleanValues, baseHardpointName + " occupied", Hardpoint.Module != null);
                        setShipModuleValues(Hardpoint.Module, baseHardpointName + " module", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        setShipModuleOutfittingValues(ship == null ? null : Hardpoint.Module, Eddi.Instance.LastStation.outfitting, baseHardpointName + " module", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    }

                    setInt(ref intValues, prefix + " hardpoints", numSmallHardpoints + numMediumHardpoints + numLargeHardpoints + numHugeHardpoints);
                    setInt(ref intValues, prefix + " utility slots", numTinyHardpoints);
                    // Compartments
                    int curCompartment = 0;
                    foreach (Compartment Compartment in ship.Compartments)
                    {
                        string baseCompartmentName = prefix + " compartment " + ++curCompartment;
                        setInt(ref intValues, baseCompartmentName + " size", Compartment.Size);
                        setBoolean(ref booleanValues, baseCompartmentName + " occupied", Compartment.Module != null);
                        setShipModuleValues(Compartment.Module, baseCompartmentName + " module", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                        setShipModuleOutfittingValues(ship == null ? null : Compartment.Module, Eddi.Instance.LastStation.outfitting, baseCompartmentName + " module", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
                    }
                    setInt(ref intValues, prefix + " compartments", curCompartment);
                }

                // Fetch the star system in which the ship is stored
                if (ship != null && ship.StarSystem != null)
                {
                    setString(ref textValues, prefix + " system", ship.StarSystem);
                    setString(ref textValues, prefix + " station", ship.Station);
                    StarSystem StoredShipStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(ship.StarSystem);
                    // Have to grab a local copy of our star system as CurrentStarSystem might not have been initialised yet
                    StarSystem ThisStarSystem = StarSystemSqLiteRepository.Instance.GetStarSystem(Eddi.Instance.CurrentStarSystem.name);

                    // Work out the distance to the system where the ship is stored if we can
                    if (ThisStarSystem != null && ThisStarSystem.x != null && StoredShipStarSystem.x != null)
                    {
                        decimal distance = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(ThisStarSystem.x - StoredShipStarSystem.x), 2)
                            + Math.Pow((double)(ThisStarSystem.y - StoredShipStarSystem.y), 2)
                            + Math.Pow((double)(ThisStarSystem.z - StoredShipStarSystem.z), 2)), 2);
                        setDecimal(ref decimalValues, prefix + " distance", distance);
                    }
                    else
                    {
                        // We don't know how far away the ship is
                        setDecimal(ref decimalValues, prefix + " distance", null);
                    }
                }

                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to set ship information", e);
            }

            Logging.Debug("Set ship information");
        }

        private static void setStarSystemValues(StarSystem system, string prefix, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> shortIntValues, ref Dictionary<string, string> textValues, ref Dictionary<string, int?> intValues, ref Dictionary<string, decimal?> decimalValues, ref Dictionary<string, Boolean?> booleanValues, ref Dictionary<string, DateTime?> dateTimeValues, ref Dictionary<string, object> extendedValues)
        {
            Logging.Debug("Setting system information (" + prefix + ")");
            try
            {
                setString(ref textValues, prefix + " name", system == null ? null : system.name);
                setString(ref textValues, prefix + " name (spoken)", system == null ? null : Translations.StarSystem(system.name));
                setDecimal(ref decimalValues, prefix + " population", system == null ? null : (decimal?)system.population);
                setString(ref textValues, prefix + " population (spoken)", system == null ? null : Translations.Humanize(system.population));
                setString(ref textValues, prefix + " allegiance", system == null ? null : system.allegiance);
                setString(ref textValues, prefix + " government", system == null ? null : system.government);
                setString(ref textValues, prefix + " faction", system == null ? null : system.faction);
                setString(ref textValues, prefix + " primary economy", system == null ? null : system.primaryeconomy);
                setString(ref textValues, prefix + " state", system == null ? null : system.state);
                setString(ref textValues, prefix + " security", system == null ? null : system.security);
                setString(ref textValues, prefix + " power", system == null ? null : system.power);
                setString(ref textValues, prefix + " power (spoken)", Translations.Power(Eddi.Instance.CurrentStarSystem.power));
                setString(ref textValues, prefix + " power state", system == null ? null : system.powerState);
                setDecimal(ref decimalValues, prefix + " X", system == null ? null : system.x);
                setDecimal(ref decimalValues, prefix + " Y", system == null ? null : system.y);
                setDecimal(ref decimalValues, prefix + " Z", system == null ? null : system.z);
                setInt(ref intValues, prefix + " visits", system == null ? (int?)null : system.visits);
                setString(ref textValues, prefix + " comment", system == null ? null : system.comment);

                if (system != null && Eddi.Instance.HomeStarSystem != null && Eddi.Instance.HomeStarSystem.x != null && system.x != null)
                {
                    setDecimal(ref decimalValues, prefix + " distance from home", (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(system.x - Eddi.Instance.HomeStarSystem.x), 2) + Math.Pow((double)(system.y - Eddi.Instance.HomeStarSystem.y), 2) + Math.Pow((double)(system.z - Eddi.Instance.HomeStarSystem.z), 2)), 2));
                }
                else
                {
                    setDecimal(ref decimalValues, prefix + " distance from home", null);
                }

                if (system != null)
                {
                    foreach (Station Station in system.stations)
                    {
                        setString(ref textValues, prefix + " station name", Station.name);
                    }
                    setInt(ref intValues, prefix + " stations", system.stations.Count);
                    setInt(ref intValues, prefix + " orbital stations", system.stations.Count(s => !s.IsPlanetary()));
                    setInt(ref intValues, prefix + " starports", system.stations.Count(s => s.IsStarport()));
                    setInt(ref intValues, prefix + " outposts", system.stations.Count(s => s.IsOutpost()));
                    setInt(ref intValues, prefix + " planetary stations", system.stations.Count(s => s.IsPlanetary()));
                    setInt(ref intValues, prefix + " planetary outposts", system.stations.Count(s => s.IsPlanetaryOutpost()));
                    setInt(ref intValues, prefix + " planetary ports", system.stations.Count(s => s.IsPlanetaryPort()));
                }

                setPluginStatus(ref textValues, "Operational", null, null);
            }
            catch (Exception e)
            {
                setPluginStatus(ref textValues, "Failed", "Failed to set system information", e);
            }
            Logging.Debug("Set system information (" + prefix + ")");
        }

        private static void setInt(ref Dictionary<string, int?> values, string key, int? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setDecimal(ref Dictionary<string, decimal?> values, string key, decimal? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setBoolean(ref Dictionary<string, bool?> values, string key, bool? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setString(ref Dictionary<string, string> values, string key, string value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setDateTime(ref Dictionary<string, DateTime?> values, string key, DateTime? value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
        }

        private static void setPluginStatus(ref Dictionary<string, string> values, string status, string error, Exception exception)
        {
            setString(ref values, "EDDI status", status);
            if (status == "Operational")
            {
                setString(ref values, "EDDI error", null);
                setString(ref values, "EDDI exception", null);
            }
            else
            {
                Logging.Warn("EDDI error: " + error);
                Logging.Warn("EDDI exception: " + (exception == null ? "<null>" : exception.ToString()));
                setString(ref values, "EDDI error", error);
                setString(ref values, "EDDI exception", exception == null ? null : exception.ToString());
            }
        }
    }
}
