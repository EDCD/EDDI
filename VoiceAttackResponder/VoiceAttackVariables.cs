﻿using EddiCargoMonitor;
using EddiCompanionAppService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiShipMonitor;
using EddiSpeechService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiVoiceAttackResponder
{
    public class VoiceAttackVariables
    {
        // These are reference values for nullable items we monitor to determine whether VoiceAttack values need to be updated
        private static StarSystem CurrentStarSystem { get; set; }
        private static StarSystem HomeStarSystem { get; set; }
        private static StarSystem LastStarSystem { get; set; }
        private static StarSystem NextStarSystem { get; set; }
        private static StarSystem DestinationStarSystem { get; set; }
        private static StarSystem SquadronStarSystem { get; set; }
        private static Body CurrentStellarBody { get; set; }
        private static Station CurrentStation { get; set; }
        private static Station HomeStation { get; set; }
        private static Station DestinationStation { get; set; }
        private static Commander Commander { get; set; }
        private static List<Ship> vaShipyard { get; set; } = new List<Ship>();
        private static decimal DestinationDistanceLy { get; set; }

        public static void setEventValues(dynamic vaProxy, Event theEvent, List<string> setKeys)
        {
            foreach (string key in Events.VARIABLES[theEvent.type].Keys)
            {
                // Obtain the value by name.  Actually looking for a method get_<name>
                System.Reflection.MethodInfo method = theEvent.GetType().GetMethod("get_" + key);
                if (method != null)
                {
                    Type returnType = method.ReturnType;
                    if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        returnType = Nullable.GetUnderlyingType(returnType);
                    }

                    string varname = "EDDI " + theEvent.type.ToLowerInvariant() + " " + key;
                    Logging.Debug("Setting values for " + varname);

                    if (returnType == typeof(string))
                    {
                        vaProxy.SetText(varname, (string)method.Invoke(theEvent, null));
                        setKeys.Add(key);
                    }
                    else if (returnType == typeof(int))
                    {
                        vaProxy.SetInt(varname, (int?)method.Invoke(theEvent, null));
                        setKeys.Add(key);
                    }
                    else if (returnType == typeof(bool))
                    {
                        vaProxy.SetBoolean(varname, (bool?)method.Invoke(theEvent, null));
                        setKeys.Add(key);
                    }
                    else if (returnType == typeof(decimal))
                    {
                        vaProxy.SetDecimal(varname, (decimal?)method.Invoke(theEvent, null));
                        setKeys.Add(key);
                    }
                    else if (returnType == typeof(double))
                    {
                        // Doubles are stored as decimals
                        vaProxy.SetDecimal(varname, (decimal?)(double?)method.Invoke(theEvent, null));
                        setKeys.Add(key);
                    }
                    else if (returnType == typeof(long))
                    {
                        vaProxy.SetDecimal(varname, (decimal?)(long?)method.Invoke(theEvent, null));
                        setKeys.Add(key);
                    }
                }
            }
        }

        /// <summary>
        /// Walk a JSON object and write out all of the possible fields
        /// </summary>
        public static void setEventExtendedValues(dynamic vaProxy, string prefix, dynamic json, List<string> setKeys)
        {
            foreach (JProperty child in json)
            {
                // We ignore the raw key (as it's the raw journal event)
                if (child.Name == "raw")
                {
                    Logging.Debug("Ignoring key " + child.Name);
                    continue;
                }
                // We also ignore any keys that we have already set elsewhere
                if (setKeys.Contains(child.Name))
                {
                    Logging.Debug("Skipping already-set key " + child.Name);
                    continue;
                }

                // Only append the child name to the current prefix if if does not repeat the prior word
                string childName = AddSpacesToTitleCasedName(child.Name).Replace("_", " ").ToLowerInvariant();
                string name;
                if (Regex.Match(prefix, @"(\w+)$").Value == childName)
                {
                    name = prefix;
                }
                else
                {
                    name = prefix + " " + childName;
                }

                if (child.Value == null)
                {
                    // No idea what it might have been so reset everything
                    Logging.Debug(prefix + " " + child.Name + " is null; need to reset all values");
                    vaProxy.SetText(name, null);
                    vaProxy.SetInt(name, null);
                    vaProxy.SetDecimal(name, null);
                    vaProxy.SetBoolean(name, null);
                    vaProxy.SetDate(name, null);
                    continue;
                }
                if (child.Value.Type == JTokenType.Boolean)
                {
                    Logging.Debug("Setting boolean value " + name + " to " + (bool?)child.Value);
                    vaProxy.SetBoolean(name, (bool?)child.Value);
                    setKeys.Add(name);
                }
                else if (child.Value.Type == JTokenType.String)
                {
                    Logging.Debug("Setting string value " + name + " to " + (string)child.Value);
                    vaProxy.SetText(name, (string)child.Value);
                    setKeys.Add(name);
                }
                else if (child.Value.Type == JTokenType.Float)
                {
                    Logging.Debug("Setting decimal value " + name + " to " + (decimal?)(double?)child.Value);
                    vaProxy.SetDecimal(name, (decimal?)(double?)child.Value);
                    setKeys.Add(name);
                }
                else if (child.Value.Type == JTokenType.Integer)
                {
                    // We set integers as decimals
                    Logging.Debug("Setting decimal value " + name + " to " + (decimal?)(long?)child.Value);
                    vaProxy.SetDecimal(name, (decimal?)(long?)child.Value);
                    setKeys.Add(name);
                }
                else if (child.Value.Type == JTokenType.Date)
                {
                    Logging.Debug("Setting date value " + name + " to " + (DateTime?)child.Value);
                    vaProxy.SetDate(name, (DateTime?)child.Value);
                    setKeys.Add(name);
                }
                else if (child.Value.Type == JTokenType.Array)
                {
                    int i = 0;
                    foreach (JToken arrayChild in child.Value.Children())
                    {
                        Logging.Debug("Handling element " + i);
                        childName = name + " " + i;
                        if (arrayChild.Type == JTokenType.Boolean)
                        {
                            Logging.Debug("Setting boolean value " + childName + " to " + arrayChild.Value<bool?>());
                            vaProxy.SetBoolean(childName, arrayChild.Value<bool?>());
                            setKeys.Add(childName);
                        }
                        else if (arrayChild.Type == JTokenType.String)
                        {
                            Logging.Debug("Setting string value " + childName + " to " + arrayChild.Value<string>());
                            vaProxy.SetText(childName, arrayChild.Value<string>());
                            setKeys.Add(childName);
                        }
                        else if (arrayChild.Type == JTokenType.Float)
                        {
                            Logging.Debug("Setting decimal value " + childName + " to " + arrayChild.Value<decimal?>());
                            vaProxy.SetDecimal(childName, arrayChild.Value<decimal?>());
                            setKeys.Add(childName);
                        }
                        else if (arrayChild.Type == JTokenType.Integer)
                        {
                            Logging.Debug("Setting decimal value " + childName + " to " + arrayChild.Value<decimal?>());
                            vaProxy.SetDecimal(childName, arrayChild.Value<decimal?>());
                            setKeys.Add(childName);
                        }
                        else if (arrayChild.Type == JTokenType.Date)
                        {
                            Logging.Debug("Setting date value " + childName + " to " + arrayChild.Value<DateTime?>());
                            vaProxy.SetDate(childName, arrayChild.Value<DateTime?>());
                            setKeys.Add(childName);
                        }
                        else if (arrayChild.Type == JTokenType.Null)
                        {
                            Logging.Debug("Setting null value " + childName);
                            vaProxy.SetText(childName, null);
                            vaProxy.SetInt(childName, null);
                            vaProxy.SetDecimal(childName, null);
                            vaProxy.SetBoolean(childName, null);
                            vaProxy.SetDate(childName, null);
                            setKeys.Add(childName);
                        }
                        else if (arrayChild.Type == JTokenType.Object)
                        {
                            setEventExtendedValues(vaProxy, childName, arrayChild, new List<string>());
                        }
                        i++;
                    }
                    vaProxy.SetInt(name + " entries", i);
                }
                else if (child.Value.Type == JTokenType.Object)
                {
                    Logging.Debug("Found object");
                    setEventExtendedValues(vaProxy, name, child.Value, new List<string>());
                }
                else if (child.Value.Type == JTokenType.Null)
                {
                    // Because the type is NULL we don't know which VA item it was; empty all of them
                    vaProxy.SetBoolean(prefix + " " + child.Name, null);
                    vaProxy.SetText(prefix + " " + child.Name, null);
                    vaProxy.SetDecimal(prefix + " " + child.Name, null);
                    vaProxy.SetDate(prefix + " " + child.Name, null);
                }
                else
                {
                    Logging.Warn(child.Value.Type + ": " + child.Name + "=" + child.Value);
                }
            }
        }

        /// <summary>Set all values</summary>
        public static void setStandardValues(ref dynamic vaProxy)
        {
            // Update our nullable primary objects only if they don't match the state of the EDDI instance.    
            // (For objects that are always constructed, we prefer using event handlers).
            try
            {
                if (!EDDI.Instance.CurrentStarSystem.DeepEquals(CurrentStarSystem))
                {
                    setStarSystemValues(EDDI.Instance.CurrentStarSystem, "System", ref vaProxy);
                    CurrentStarSystem = EDDI.Instance.CurrentStarSystem.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set current system", ex);
            }

            try
            {
                if (!EDDI.Instance.LastStarSystem.DeepEquals(LastStarSystem))
                {
                    setStarSystemValues(EDDI.Instance.LastStarSystem, "Last system", ref vaProxy);
                    LastStarSystem = EDDI.Instance.LastStarSystem.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set last system", ex);
            }

            try
            {
                if (!EDDI.Instance.NextStarSystem.DeepEquals(NextStarSystem))
                {
                    setStarSystemValues(EDDI.Instance.NextStarSystem, "Next system", ref vaProxy);
                    NextStarSystem = EDDI.Instance.NextStarSystem.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set next system", ex);
            }

            try
            {
                if (!EDDI.Instance.DestinationStarSystem.DeepEquals(DestinationStarSystem))
                {
                    setStarSystemValues(EDDI.Instance.DestinationStarSystem, "Destination system", ref vaProxy);
                    DestinationStarSystem = EDDI.Instance.DestinationStarSystem.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set destination system", ex);
            }

            try
            {
                if (EDDI.Instance.DestinationDistanceLy != DestinationDistanceLy)
                {
                    vaProxy.SetDecimal("Destination system distance", EDDI.Instance.DestinationDistanceLy);
                    DestinationDistanceLy = EDDI.Instance.DestinationDistanceLy.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set destination distance", ex);
            }

            try
            {
                if (!EDDI.Instance.DestinationStation.DeepEquals(DestinationStation))
                {
                    setStationValues(EDDI.Instance.DestinationStation, "Destination station", ref vaProxy);
                    DestinationStation = EDDI.Instance.DestinationStation.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set destination station", ex);
            }

            try
            {
                if (!EDDI.Instance.SquadronStarSystem.DeepEquals(SquadronStarSystem))
                {
                    setStarSystemValues(EDDI.Instance.SquadronStarSystem, "Squadron system", ref vaProxy);
                    SquadronStarSystem = EDDI.Instance.SquadronStarSystem.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set squadron system", ex);
            }

            try
            {
                if (!EDDI.Instance.HomeStarSystem.DeepEquals(HomeStarSystem))
                {
                    setStarSystemValues(EDDI.Instance.HomeStarSystem, "Home system", ref vaProxy);
                    HomeStarSystem = EDDI.Instance.HomeStarSystem.Copy();

                    // Backwards-compatibility with 1.x documented variables
                    try
                    {
                        if (EDDI.Instance.HomeStarSystem != null)
                        {
                            vaProxy.SetText("Home system", EDDI.Instance.HomeStarSystem.systemname);
                            vaProxy.SetText("Home system (spoken)", Translations.StarSystem(EDDI.Instance.HomeStarSystem.systemname));
                        }
                        if (EDDI.Instance.HomeStation != null)
                        {
                            vaProxy.SetText("Home station", EDDI.Instance.HomeStation.name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Error("Failed to set 1.x home system values", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set home system", ex);
            }

            try
            {
                if (!EDDI.Instance.CurrentStellarBody.DeepEquals(CurrentStellarBody))
                {
                    setDetailedBodyValues(EDDI.Instance.CurrentStellarBody, "Body", ref vaProxy);
                    CurrentStellarBody = EDDI.Instance.CurrentStellarBody.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set current stellar body", ex);
            }

            try
            {
                if (!EDDI.Instance.CurrentStation.DeepEquals(CurrentStation))
                {
                    setStationValues(EDDI.Instance.CurrentStation, "Last station", ref vaProxy);
                    CurrentStation = EDDI.Instance.CurrentStation.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set last station", ex);
            }

            try
            {
                if (!EDDI.Instance.HomeStation.DeepEquals(HomeStation))
                {
                    setStationValues(EDDI.Instance.HomeStation, "Home station", ref vaProxy);
                    HomeStation = EDDI.Instance.HomeStation.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set home station", ex);
            }


            try
            {
                if (!EDDI.Instance.Cmdr.DeepEquals(Commander))
                {
                    setCommanderValues(EDDI.Instance.Cmdr, ref vaProxy);
                    Commander = EDDI.Instance.Cmdr.Copy();
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set commander values", ex);
            }

            // On every event...    
            // Set miscellaneous values
            try
            {
                vaProxy.SetBoolean("cAPI active", CompanionAppService.Instance?.active ?? false);
                vaProxy.SetBoolean("ipa active", !(SpeechService.Instance?.Configuration.DisableIpa ?? false));
                vaProxy.SetBoolean("icao active", SpeechService.Instance?.Configuration.EnableIcao ?? false);
                vaProxy.SetBoolean("horizons", EDDI.Instance.inHorizons);
                vaProxy.SetBoolean("odyssey", EDDI.Instance.inOdyssey);
                vaProxy.SetText("Environment", EDDI.Instance.Environment);
                vaProxy.SetText("Vehicle", EDDI.Instance.Vehicle);
                vaProxy.SetText("EDDI version", Constants.EDDI_VERSION.ToString());
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set misc values", ex);
            }
        }

        // Set values from a dictionary
        public static void setDictionaryValues(IDictionary<string, object> dict, string prefix, ref dynamic vaProxy)
        {
            foreach (string key in dict.Keys)
            {
                object value = dict[key];
                if (value == null)
                {
                    continue;
                }
                Type valueType = value.GetType();
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    valueType = Nullable.GetUnderlyingType(valueType);
                }

                string varname = "EDDI " + prefix + " " + key;

                if (valueType == typeof(string))
                {
                    vaProxy.SetText(varname, (string)value);
                }
                else if (valueType == typeof(int))
                {
                    vaProxy.SetInt(varname, (int)value);
                }
                else if (valueType == typeof(bool))
                {
                    vaProxy.SetBoolean(varname, (bool?)value);
                }
                else if (valueType == typeof(decimal))
                {
                    vaProxy.SetDecimal(varname, (decimal?)value);
                }
                else if (valueType == typeof(double))
                {
                    vaProxy.SetDecimal(varname, (decimal?)(double?)value);
                }
                else if (valueType == typeof(long))
                {
                    vaProxy.SetDecimal(varname, (decimal?)(long?)value);
                }
                else
                {
                    Logging.Debug("Not handling state value type " + valueType);
                }
            }

        }

        /// <summary>Set values for a station</summary>
        private static void setStationValues(Station station, string prefix, ref dynamic vaProxy)
        {
            Logging.Debug("Setting station information");

            vaProxy.SetText(prefix + " name", station?.name);
            vaProxy.SetDecimal(prefix + " distance from star", station?.distancefromstar);
            vaProxy.SetText(prefix + " government", (station?.Faction?.Government ?? Government.None).localizedName);
            vaProxy.SetText(prefix + " allegiance", (station?.Faction?.Allegiance ?? Superpower.None).localizedName);
            vaProxy.SetText(prefix + " faction", station?.Faction?.name);
            vaProxy.SetText(prefix + " state", (station?.Faction?.presences
                .FirstOrDefault(p => p.systemName == station.systemname)?.FactionState ?? FactionState.None).localizedName);
            vaProxy.SetText(prefix + " primary economy", station?.primaryeconomy);
            vaProxy.SetText(prefix + " secondary economy", station?.secondaryeconomy);
            // Services
            vaProxy.SetBoolean(prefix + " has refuel", station?.hasrefuel);
            vaProxy.SetBoolean(prefix + " has repair", station?.hasrepair);
            vaProxy.SetBoolean(prefix + " has rearm", station?.hasrearm);
            vaProxy.SetBoolean(prefix + " has market", station?.hasmarket);
            vaProxy.SetBoolean(prefix + " has black market", station?.hasblackmarket);
            vaProxy.SetBoolean(prefix + " has outfitting", station?.hasoutfitting);
            vaProxy.SetBoolean(prefix + " has shipyard", station?.hasshipyard);

            Logging.Debug("Set station information");
        }

        private static void setCommanderValues(Commander cmdr, ref dynamic vaProxy)
        {
            try
            {
                vaProxy.SetText("Name", cmdr?.name);
                vaProxy.SetInt("Combat rating", cmdr?.combatrating?.rank);
                vaProxy.SetText("Combat rank", cmdr?.combatrating?.localizedName);
                vaProxy.SetInt("Trade rating", cmdr?.traderating?.rank);
                vaProxy.SetText("Trade rank", cmdr?.traderating?.localizedName);
                vaProxy.SetInt("Explore rating", cmdr?.explorationrating?.rank);
                vaProxy.SetText("Explore rank", cmdr?.explorationrating?.localizedName);
                vaProxy.SetInt("Empire rating", cmdr?.empirerating?.rank);
                vaProxy.SetText("Empire rank", cmdr?.empirerating?.maleRank.localizedName);
                vaProxy.SetInt("Federation rating", cmdr?.federationrating?.rank);
                vaProxy.SetText("Federation rank", cmdr?.federationrating?.localizedName);
                vaProxy.SetInt("Mercenary rating", cmdr?.mercenaryrating?.rank);
                vaProxy.SetText("Mercenary rank", cmdr?.mercenaryrating?.localizedName);
                vaProxy.SetInt("Exobiologist rating", cmdr?.exobiologistrating?.rank);
                vaProxy.SetText("Exobiologist rank", cmdr?.exobiologistrating?.localizedName);
                vaProxy.SetDecimal("Credits", cmdr?.credits);
                vaProxy.SetText("Credits (spoken)", Translations.Humanize(cmdr?.credits));
                vaProxy.SetDecimal("Debt", cmdr?.debt);
                vaProxy.SetText("Debt (spoken)", Translations.Humanize(cmdr?.debt));
                vaProxy.SetText("Title", cmdr?.title ?? Eddi.Properties.EddiResources.Commander);
                vaProxy.SetText("Gender", cmdr?.gender ?? Eddi.Properties.MainWindow.tab_commander_gender_n);
                vaProxy.SetText("Squadron name", cmdr?.squadronname);
                vaProxy.SetText("Squadron id", cmdr?.squadronid);
                vaProxy.SetInt("Squadron rating", cmdr?.squadronrank?.rank);
                vaProxy.SetText("Squadron rank", cmdr?.squadronrank?.localizedName);
                vaProxy.SetText("Squadron allegiance", cmdr?.squadronallegiance?.localizedName);
                vaProxy.SetText("Squadron power", cmdr?.squadronpower?.localizedName);
                vaProxy.SetText("Squadron faction", cmdr?.squadronfaction);
                vaProxy.SetText("Power", cmdr?.Power?.localizedName);

                // Backwards-compatibility with 1.x
                vaProxy.SetText("System rank", cmdr?.title);

                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set commander information", e);
            }

            Logging.Debug("Set commander information");
        }

        protected static void setShipValues(Ship ship, string prefix, ref dynamic vaProxy)
        {
            if (ship != null && ship != vaShipyard.FirstOrDefault(s => s.LocalId == ship.LocalId))
            {
                Logging.Debug("Setting ship information (" + prefix + ")");
                try
                {
                    vaProxy.SetText(prefix + " manufacturer", ship.manufacturer);
                    vaProxy.SetText(prefix + " model", ship.model);
                    vaProxy.SetText(prefix + " model (spoken)", ship.SpokenModel());

                    if (EDDI.Instance.Cmdr?.name != null)
                    {
                        vaProxy.SetText(prefix + " callsign", ship.manufacturer + " " + EDDI.Instance.Cmdr.name.Substring(0, 3).ToUpperInvariant());
                        vaProxy.SetText(prefix + " callsign (spoken)", ship.SpokenManufacturer() + " " + Translations.ICAO(EDDI.Instance.Cmdr.name.Substring(0, 3).ToUpperInvariant()));
                    }

                    vaProxy.SetText(prefix + " name", ship.name);
                    vaProxy.SetText(prefix + " name (spoken)", ship.phoneticName);
                    vaProxy.SetText(prefix + " ident", ship.ident);
                    vaProxy.SetText(prefix + " ident (spoken)", Translations.ICAO(ship.ident, false));
                    vaProxy.SetText(prefix + " role", ship.Role?.localizedName);
                    vaProxy.SetText(prefix + " size", ship.Size?.localizedName);
                    vaProxy.SetDecimal(prefix + " value", ship.value);
                    vaProxy.SetText(prefix + " value (spoken)", Translations.Humanize(ship.value));
                    vaProxy.SetDecimal(prefix + " hull value", ship.hullvalue);
                    vaProxy.SetText(prefix + " hull value (spoken)", Translations.Humanize(ship.hullvalue));
                    vaProxy.SetDecimal(prefix + " modules value", ship.modulesvalue);
                    vaProxy.SetText(prefix + " modules value (spoken)", Translations.Humanize(ship.modulesvalue));
                    vaProxy.SetDecimal(prefix + " rebuy", ship.rebuy);
                    vaProxy.SetText(prefix + " rebuy (spoken)", Translations.Humanize(ship.rebuy));
                    vaProxy.SetDecimal(prefix + " health", ship.health);
                    vaProxy.SetInt(prefix + " cargo capacity", ship.cargocapacity);
                    vaProxy.SetBoolean(prefix + " hot", ship.hot);

                    setShipModuleValues(ship.bulkheads, prefix + " bulkheads", ref vaProxy);
                    setShipModuleOutfittingValues(ship.bulkheads, EDDI.Instance.CurrentStation?.outfitting, prefix + " bulkheads", ref vaProxy);
                    setShipModuleValues(ship.powerplant, prefix + " power plant", ref vaProxy);
                    setShipModuleOutfittingValues(ship.powerplant, EDDI.Instance.CurrentStation?.outfitting, prefix + " power plant", ref vaProxy);
                    setShipModuleValues(ship.thrusters, prefix + " thrusters", ref vaProxy);
                    setShipModuleOutfittingValues(ship.thrusters, EDDI.Instance.CurrentStation?.outfitting, prefix + " thrusters", ref vaProxy);
                    setShipModuleValues(ship.frameshiftdrive, prefix + " frame shift drive", ref vaProxy);
                    setShipModuleOutfittingValues(ship.frameshiftdrive, EDDI.Instance.CurrentStation?.outfitting, prefix + " frame shift drive", ref vaProxy);
                    setShipModuleValues(ship.lifesupport, prefix + " life support", ref vaProxy);
                    setShipModuleOutfittingValues(ship.lifesupport, EDDI.Instance.CurrentStation?.outfitting, prefix + " life support", ref vaProxy);
                    setShipModuleValues(ship.powerdistributor, prefix + " power distributor", ref vaProxy);
                    setShipModuleOutfittingValues(ship.powerdistributor, EDDI.Instance.CurrentStation?.outfitting, prefix + " power distributor", ref vaProxy);
                    setShipModuleValues(ship.sensors, prefix + " sensors", ref vaProxy);
                    setShipModuleOutfittingValues(ship.sensors, EDDI.Instance.CurrentStation?.outfitting, prefix + " sensors", ref vaProxy);
                    setShipModuleValues(ship.fueltank, prefix + " fuel tank", ref vaProxy);
                    setShipModuleOutfittingValues(ship.fueltank, EDDI.Instance.CurrentStation?.outfitting, prefix + " fuel tank", ref vaProxy);

                    // Special for fuel tank - capacity and total capacity
                    vaProxy.SetDecimal(prefix + " fuel tank capacity", ship.fueltankcapacity);
                    vaProxy.SetDecimal(prefix + " total fuel tank capacity", ship.fueltanktotalcapacity);

                    // Special for max jump range and max fuel per jump
                    vaProxy.SetDecimal(prefix + " max jump range", ship.maxjumprange);
                    vaProxy.SetDecimal(prefix + " max fuel per jump", ship.maxfuelperjump);

                    // Hardpoints
                    int numTinyHardpoints = 0;
                    int numSmallHardpoints = 0;
                    int numMediumHardpoints = 0;
                    int numLargeHardpoints = 0;
                    int numHugeHardpoints = 0;
                    foreach (Hardpoint Hardpoint in ship.hardpoints)
                    {
                        string baseHardpointName = prefix;
                        switch (Hardpoint.size)
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

                        vaProxy.SetBoolean(baseHardpointName + " occupied", Hardpoint.module != null);
                        setShipModuleValues(Hardpoint.module, baseHardpointName + " module", ref vaProxy);
                        setShipModuleOutfittingValues(Hardpoint.module, EDDI.Instance.CurrentStation?.outfitting, baseHardpointName + " module", ref vaProxy);
                    }
                    vaProxy.SetInt(prefix + " hardpoints", numTinyHardpoints + numSmallHardpoints + numMediumHardpoints + numLargeHardpoints + numHugeHardpoints);

                    // Compartments
                    int curCompartment = 0;
                    foreach (Compartment Compartment in ship.compartments)
                    {
                        string baseCompartmentName = prefix + " compartment " + ++curCompartment;
                        vaProxy.SetInt(baseCompartmentName + " size", Compartment.size);
                        vaProxy.SetBoolean(baseCompartmentName + " occupied", Compartment.module != null);
                        setShipModuleValues(Compartment.module, baseCompartmentName + " module", ref vaProxy);
                        setShipModuleOutfittingValues(Compartment.module, EDDI.Instance.CurrentStation?.outfitting, baseCompartmentName + " module", ref vaProxy);
                    }
                    vaProxy.SetInt(prefix + " compartments", curCompartment);

                    // Fetch the star system in which the ship is stored
                    if (ship.starsystem != null)
                    {
                        vaProxy.SetText(prefix + " system", ship.starsystem);
                        vaProxy.SetText(prefix + " station", ship.station);
                        vaProxy.SetDecimal(prefix + " distance", ship.distance);
                    }

                    setStatus(ref vaProxy, "Operational");
                }
                catch (Exception e)
                {
                    Logging.Error("Failed to set VoiceAttack ship information", e);
                    setStatus(ref vaProxy, "Failed to set ship information", e);
                }

                Logging.Debug("Set ship information");
            }
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleValues(Module module, string name, ref dynamic vaProxy)
        {
            vaProxy.SetText(name, module?.localizedName);
            vaProxy.SetInt(name + " class", module?.@class);
            vaProxy.SetText(name + " grade", module?.grade);
            vaProxy.SetDecimal(name + " health", module?.health);
            vaProxy.SetDecimal(name + " cost", module?.price);
            vaProxy.SetDecimal(name + " value", module?.value);
            if (module != null && module.price < module.value)
            {
                decimal discount = Math.Round((1 - (module.price / ((decimal)module.value))) * 100, 1);
                vaProxy.SetDecimal(name + " discount", discount > 0.01M ? discount : (decimal?)null);
            }
            else
            {
                vaProxy.SetDecimal(name + " discount", null);
            }
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleOutfittingValues(Module existing, List<Module> outfittingModules, string name, ref dynamic vaProxy)
        {
            if (existing != null && outfittingModules != null)
            {
                foreach (Module Module in outfittingModules)
                {
                    if (existing.EDDBID == Module.EDDBID)
                    {
                        // Found it
                        vaProxy.SetDecimal(name + " station cost", (decimal?)Module.price);
                        if (Module.price < existing.price)
                        {
                            // And it's cheaper
                            vaProxy.SetDecimal(name + " station discount", existing.price - Module.price);
                            vaProxy.SetText(name + " station discount (spoken)", Translations.Humanize(existing.price - Module.price));
                        }
                        return;
                    }
                }
            }
            // Not found so remove any existing
            vaProxy.SetDecimal("Ship " + name + " station cost", (decimal?)null);
            vaProxy.SetDecimal("Ship " + name + " station discount", (decimal?)null);
            vaProxy.SetText("Ship " + name + " station discount (spoken)", (string)null);
        }

        protected static void setShipyardValues(List<Ship> shipyard, ref dynamic vaProxy)
        {
            if (shipyard != null && shipyard != vaShipyard)
            {
                int currentStoredShip = 1;
                foreach (Ship StoredShip in shipyard)
                {
                    Ship vaShip = vaShipyard.FirstOrDefault(s => s.LocalId == StoredShip.LocalId);
                    string vaShipString = vaShip == null ? null : JsonConvert.SerializeObject(vaShip);
                    string storedShipString = JsonConvert.SerializeObject(StoredShip);
                    if (vaShipString != storedShipString)
                    {
                        setShipValues(StoredShip, "Stored ship " + currentStoredShip, ref vaProxy);
                        currentStoredShip++;
                        if (vaShipString is null)
                        {
                            vaShipyard.Add(JsonConvert.DeserializeObject<Ship>(storedShipString));
                        }
                        else
                        {
                            vaShip = JsonConvert.DeserializeObject<Ship>(storedShipString);
                        }
                    }
                }
                vaProxy.SetInt("Stored ship entries", ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.shipyard.Count);
            }
        }

        private static void setStarSystemValues(StarSystem system, string prefix, ref dynamic vaProxy)
        {
            Logging.Debug("Setting system information (" + prefix + ")");
            try
            {
                vaProxy.SetText(prefix + " name", system?.systemname);
                vaProxy.SetText(prefix + " name (spoken)", Translations.StarSystem(system?.systemname));
                vaProxy.SetDecimal(prefix + " population", system?.population);
                vaProxy.SetText(prefix + " population (spoken)", Translations.Humanize(system?.population));
                vaProxy.SetText(prefix + " allegiance", (system?.Faction?.Allegiance ?? Superpower.None).localizedName);
                vaProxy.SetText(prefix + " government", (system?.Faction?.Government ?? Government.None).localizedName);
                vaProxy.SetText(prefix + " faction", system?.Faction?.name);
                vaProxy.SetText(prefix + " primary economy", system?.primaryeconomy);
                vaProxy.SetText(prefix + " state", (system?.Faction?.presences
                    .FirstOrDefault(p => p.systemName == system.systemname)?.FactionState ?? FactionState.None).localizedName);
                vaProxy.SetText(prefix + " security", system?.security);
                vaProxy.SetText(prefix + " power", system?.power);
                vaProxy.SetText(prefix + " power (spoken)", Translations.Power(EDDI.Instance.CurrentStarSystem?.power));
                vaProxy.SetText(prefix + " power state", system?.powerstate);
                vaProxy.SetBoolean(prefix + " requires permit", system?.requirespermit);
                vaProxy.SetText(prefix + " permit name", system?.permitname);
                vaProxy.SetDecimal(prefix + " X", system?.x);
                vaProxy.SetDecimal(prefix + " Y", system?.y);
                vaProxy.SetDecimal(prefix + " Z", system?.z);
                vaProxy.SetInt(prefix + " visits", system?.visits);
                vaProxy.SetDate(prefix + " previous visit", system?.visits > 1 ? system.lastvisit : null);
                vaProxy.SetDecimal(prefix + " minutes since previous visit", system?.visits > 1 && system?.lastvisit.HasValue == true ? (long)(DateTime.UtcNow - system.lastvisit.Value).TotalMinutes : (decimal?)null);
                vaProxy.SetText(prefix + " comment", system?.comment);
                vaProxy.SetDecimal(prefix + " distance from home", system?.distancefromhome);
                vaProxy.SetBoolean(prefix + " scoopable", system?.scoopable);
                vaProxy.SetInt(prefix + " total bodies", system?.totalbodies);
                vaProxy.SetInt(prefix + " scanned bodies", system?.scannedbodies);
                vaProxy.SetInt(prefix + " mapped bodies", system?.mappedbodies);

                if (system != null)
                {
                    foreach (Station Station in system.stations)
                    {
                        vaProxy.SetText(prefix + " station name", Station.name);
                    }
                    vaProxy.SetInt(prefix + " stations", system.stations.Count);
                    vaProxy.SetInt(prefix + " orbital stations", system.stations.Count(s => !s.IsPlanetary()));
                    vaProxy.SetInt(prefix + " starports", system.stations.Count(s => s.IsStarport()));
                    vaProxy.SetInt(prefix + " outposts", system.stations.Count(s => s.IsOutpost()));
                    vaProxy.SetInt(prefix + " planetary stations", system.stations.Count(s => s.IsPlanetary()));
                    vaProxy.SetInt(prefix + " planetary settlements", system.stations.Count(s => s.IsPlanetarySettlement()));

                    Body primaryBody = null;
                    if (system.bodies != null && system.bodies.Count > 0)
                    {
                        primaryBody = (system.bodies[0].distance == 0 ? system.bodies[0] : null);
                    }
                    setBodyValues(primaryBody, prefix + " main star", vaProxy);
                }
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set system information", e);
            }
            Logging.Debug("Set system information (" + prefix + ")");
        }

        private static void setBodyValues(Body body, string prefix, dynamic vaProxy)
        {
            Logging.Debug("Setting body information (" + prefix + ")");
            vaProxy.SetText(prefix + " name", body?.bodyname);
            vaProxy.SetText(prefix + " stellar class", body?.stellarclass);
            vaProxy.SetDecimal(prefix + " age", body?.age);
            Logging.Debug("Set body information (" + prefix + ")");
        }

        private static void setDetailedBodyValues(Body body, string prefix, ref dynamic vaProxy)
        {
            Logging.Debug("Setting current stellar body information");
            vaProxy.SetText(prefix + " type", (body?.bodyType ?? BodyType.None).localizedName);
            vaProxy.SetText(prefix + " name", body?.bodyname);
            vaProxy.SetText(prefix + " short name", body?.shortname);
            vaProxy.SetText(prefix + " system name", body?.systemname);
            if (body?.age == null)
            {
                vaProxy.SetDecimal(prefix + " age", null);
            }
            else
            {
                vaProxy.SetDecimal(prefix + " age", (decimal)(long)body.age);
            }
            vaProxy.SetDecimal(prefix + " distance", body?.distance);
            vaProxy.SetDecimal(prefix + " temperature", body?.temperature);
            // Orbital characteristics
            vaProxy.SetDecimal(prefix + " eccentricity", body?.eccentricity);
            vaProxy.SetDecimal(prefix + " inclination", body?.inclination);
            vaProxy.SetDecimal(prefix + " orbital period", body?.orbitalperiod);
            vaProxy.SetDecimal(prefix + " radius", body?.radius);
            vaProxy.SetDecimal(prefix + " rotational period", body?.rotationalperiod);
            vaProxy.SetDecimal(prefix + " semi major axis", body?.semimajoraxis);
            // Star specific items 
            if (body?.bodyType?.invariantName == "Star")
            {
                vaProxy.SetBoolean(prefix + " main star", body?.mainstar);
                vaProxy.SetText(prefix + " stellar class", body?.stellarclass);
                vaProxy.SetText(prefix + " luminosity class", body?.luminosityclass);
                vaProxy.SetDecimal(prefix + " solar mass", body?.solarmass);
                vaProxy.SetDecimal(prefix + " solar radius", body?.solarradius);
                vaProxy.SetText(prefix + " chromaticity", body?.chromaticity);
                vaProxy.SetDecimal(prefix + " radius probability", body?.radiusprobability);
                vaProxy.SetDecimal(prefix + " mass probability", body?.massprobability);
                vaProxy.SetDecimal(prefix + " temp probability", body?.tempprobability);
                vaProxy.SetDecimal(prefix + " age probability", body?.ageprobability);
                vaProxy.SetDecimal(prefix + " estimated inner hab zone", body?.estimatedhabzoneinner);
                vaProxy.SetDecimal(prefix + " estimated outer hab zone", body?.estimatedhabzoneouter);
                vaProxy.SetBoolean(prefix + " scoopable", body?.scoopable);
            }
            // Body specific items 
            if (body?.bodyType?.invariantName == "Planet")
            {
                vaProxy.SetDecimal(prefix + " periapsis", body?.periapsis);
                vaProxy.SetText(prefix + " atmosphere", (body?.atmosphereclass ?? AtmosphereClass.None).localizedName);
                vaProxy.SetDecimal(prefix + " tilt", body?.tilt);
                vaProxy.SetDecimal(prefix + " earth mass", body?.earthmass);
                vaProxy.SetDecimal(prefix + " gravity", body?.gravity);
                vaProxy.SetDecimal(prefix + " pressure", body?.pressure);
                vaProxy.SetText(prefix + " terraform state", (body?.terraformState ?? TerraformState.NotTerraformable).localizedName);
                vaProxy.SetText(prefix + " planet type", (body?.planetClass ?? PlanetClass.None).localizedName);
                vaProxy.SetText(prefix + " reserves", (body?.reserveLevel ?? ReserveLevel.None).localizedName);
                vaProxy.SetBoolean(prefix + " landable", body?.landable);
                vaProxy.SetBoolean(prefix + " tidally locked", body?.tidallylocked);
            }

            Logging.Debug("Set body information (" + prefix + ")");
        }

        public static void setSpeaking(bool eddiSpeaking, ref dynamic vaProxy)
        {
            vaProxy.SetBoolean("EDDI speaking", eddiSpeaking);
        }

        public static void setStatus(ref dynamic vaProxy, string status, Exception exception = null)
        {
            vaProxy.SetText("EDDI status", status);
            if (status == "Operational")
            {
                vaProxy.SetText("EDDI exception", null);
            }
            else
            {
                Logging.Warn("EDDI exception: " + (exception == null ? "<null>" : exception.ToString()));
                vaProxy.SetText("EDDI exception", exception?.ToString());
            }
        }

        protected static void setStatusValues(Status status, string prefix, ref dynamic vaProxy)
        {
            if (status == null)
            {
                return;
            }

            try
            {
                // Variables set from status flags
                vaProxy.SetText(prefix + " vehicle", status?.vehicle);
                vaProxy.SetBoolean(prefix + " being interdicted", status?.being_interdicted);
                vaProxy.SetBoolean(prefix + " in danger", status?.in_danger);
                vaProxy.SetBoolean(prefix + " near surface", status?.near_surface);
                vaProxy.SetBoolean(prefix + " overheating", status?.overheating);
                vaProxy.SetBoolean(prefix + " low fuel", status?.low_fuel);
                vaProxy.SetText(prefix + " fsd status", status?.fsd_status);
                vaProxy.SetBoolean(prefix + " srv drive assist", status?.srv_drive_assist);
                vaProxy.SetBoolean(prefix + " srv under ship", status?.srv_under_ship);
                vaProxy.SetBoolean(prefix + " srv turret deployed", status?.srv_turret_deployed);
                vaProxy.SetBoolean(prefix + " srv handbrake activated", status?.srv_handbrake_activated);
                vaProxy.SetBoolean(prefix + " srv high beams", status?.srv_high_beams);
                vaProxy.SetBoolean(prefix + " scooping fuel", status?.scooping_fuel);
                vaProxy.SetBoolean(prefix + " silent running", status?.silent_running);
                vaProxy.SetBoolean(prefix + " cargo scoop deployed", status?.cargo_scoop_deployed);
                vaProxy.SetBoolean(prefix + " lights on", status?.lights_on);
                vaProxy.SetBoolean(prefix + " in wing", status?.in_wing);
                vaProxy.SetBoolean(prefix + " hardpoints deployed", status?.hardpoints_deployed);
                vaProxy.SetBoolean(prefix + " flight assist off", status?.flight_assist_off);
                vaProxy.SetBoolean(prefix + " supercruise", status?.supercruise);
                vaProxy.SetBoolean(prefix + " hyperspace", status?.hyperspace);
                vaProxy.SetBoolean(prefix + " shields up", status?.shields_up);
                vaProxy.SetBoolean(prefix + " landing gear down", status?.landing_gear_down);
                vaProxy.SetBoolean(prefix + " landed", status?.landed);
                vaProxy.SetBoolean(prefix + " docked", status?.docked);
                vaProxy.SetBoolean(prefix + " analysis mode", status?.analysis_mode);
                vaProxy.SetBoolean(prefix + " night vision", status?.night_vision);

                // Variables set from pips (these are not always present in the event)
                vaProxy.SetDecimal(prefix + " system pips", status?.pips_sys);
                vaProxy.SetDecimal(prefix + " engine pips", status?.pips_eng);
                vaProxy.SetDecimal(prefix + " weapon pips", status?.pips_wea);

                // Variables set directly from the event (these are not always present in the event)
                vaProxy.SetInt(prefix + " firegroup", status?.firegroup);
                vaProxy.SetText(prefix + " gui focus", status?.gui_focus);
                vaProxy.SetDecimal(prefix + " latitude", status?.latitude);
                vaProxy.SetDecimal(prefix + " longitude", status?.longitude);
                vaProxy.SetDecimal(prefix + " altitude", status?.altitude);
                vaProxy.SetDecimal(prefix + " heading", status?.heading);
                vaProxy.SetDecimal(prefix + " slope", status?.slope);
                vaProxy.SetDecimal(prefix + " fuel", status?.fuel);
                vaProxy.SetDecimal(prefix + " fuel percent", status?.fuel_percent);
                vaProxy.SetInt(prefix + " fuel rate", status?.fuel_seconds);
                vaProxy.SetInt(prefix + " cargo carried", status?.cargo_carried);
                vaProxy.SetText(prefix + " legal status", status?.legalstatus);
                vaProxy.SetText(prefix + " body name", status?.bodyname);
                vaProxy.SetDecimal(prefix + " planet radius", status?.planetradius);
                vaProxy.SetBoolean(prefix + " altitude from average radius", status?.altitude_from_average_radius);
                vaProxy.SetBoolean(prefix + " on foot in station", status?.on_foot_in_station);
                vaProxy.SetBoolean(prefix + " on foot on planet", status?.on_foot_on_planet);
                vaProxy.SetBoolean(prefix + " aim down sight", status?.aim_down_sight);
                vaProxy.SetBoolean(prefix + " low oxygen", status?.low_oxygen);
                vaProxy.SetBoolean(prefix + " low health", status?.low_health);
                vaProxy.SetText(prefix + " on foot temperature", status?.on_foot_temperature);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set real-time status information", e);
            }

            Logging.Debug("Set real-time status information");
        }

        protected static void setCargo(CargoMonitor cargoMonitor, ref dynamic vaProxy)
        {
            try
            {
                vaProxy.SetInt("Ship cargo carried", cargoMonitor?.cargoCarried ?? 0);
                vaProxy.SetInt("Ship limpets carried", cargoMonitor?.GetCargoWithEDName("Drones")?.total ?? 0);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set ship cargo values", ex);
            }
        }

        private static string AddSpacesToTitleCasedName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                {
                    newText.Append(' ');
                }

                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
