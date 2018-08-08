using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiShipMonitor
{
    // Handle the Frontier API definition for ships
    public class FrontierApi
    {
        private static List<string> HARDPOINT_SIZES = new List<string>() { "Huge", "Large", "Medium", "Small", "Tiny" };

        public static List<Ship> ShipyardFromJson(Ship activeShip, dynamic json)
        {
            List<Ship> shipyard = new List<Ship>();

            foreach (dynamic shipJson in json["ships"])
            {
                if (shipJson != null)
                {
                    // Take underlying value if present
                    JObject shipObj = shipJson.Value ?? shipJson;
                    if (shipObj != null)
                    {
                        Ship ship = ShipFromJson(shipObj);
                        if (activeShip?.LocalId == ship.LocalId)
                        {
                            // This is the active ship so add that instead
                            shipyard.Add(activeShip);
                        }
                        else
                        {
                            if (shipObj["starsystem"] != null)
                            {
                                ship.starsystem = (string)shipObj["starsystem"]["name"];
                                ship.station = (string)shipObj["station"]["name"];
                            }
                            shipyard.Add(ship);
                        }
                    }
                }
            }

            return shipyard;
        }

        public static Ship ShipFromJson(JObject json)
        {
            if (json == null)
            {
                return null;
            }

            string edName = (string)json["name"];
            Ship Ship = ShipDefinitions.FromEDModel(edName);
            if (Ship == null)
            {
                // Unknown ship; report the full object so that we can update the definitions 
                Logging.Info("Ship definition error: " + edName, JsonConvert.SerializeObject(json));

                // Create a basic ship definition & supplement from the info available 
                Ship = new Ship();
                Ship.EDName = edName;
            }

            // We want to return a basic ship if the parsing fails so wrap this
            try
            {
                Ship.LocalId = json.GetValue("id").Value<int>();
                Ship.name = (string)json.GetValue("shipName");
                Ship.ident = (string)json.GetValue("shipID");

                Ship.value = (long)(json["value"]?["hull"] ?? 0) + (long)(json["value"]?["modules"] ?? 0);
                Ship.cargocapacity = 0;

                decimal? healthOutOf1e6 = (decimal?)(json["health"]?["hull"]);
                if (healthOutOf1e6 != null)
                {
                    decimal healthPercent = (decimal)healthOutOf1e6 / 10_000M;
                    Ship.health = healthPercent;
                }

                if (json["modules"] != null)
                {
                    // Obtain the internals
                    Ship.bulkheads = ModuleFromJson("Armour", (JObject)json["modules"]["Armour"]);
                    Ship.powerplant = ModuleFromJson("PowerPlant", (JObject)json["modules"]["PowerPlant"]);
                    Ship.thrusters = ModuleFromJson("MainEngines", (JObject)json["modules"]["MainEngines"]);
                    Ship.frameshiftdrive = ModuleFromJson("FrameShiftDrive", (JObject)json["modules"]["FrameShiftDrive"]);
                    Ship.lifesupport = ModuleFromJson("LifeSupport", (JObject)json["modules"]["LifeSupport"]);
                    Ship.powerdistributor = ModuleFromJson("PowerDistributor", (JObject)json["modules"]["PowerDistributor"]);
                    Ship.sensors = ModuleFromJson("Radar", (JObject)json["modules"]["Radar"]);
                    Ship.fueltank = ModuleFromJson("FuelTank", (JObject)json["modules"]["FuelTank"]);
                    if (Ship.fueltank != null)
                    {
                        Ship.fueltankcapacity = (decimal)Math.Pow(2, Ship.fueltank.@class);
                    }
                    Ship.fueltanktotalcapacity = Ship.fueltankcapacity;
                    Ship.paintjob = (string)(json["modules"]?["PaintJob"]?["name"] ?? null);

                    // Obtain the hardpoints.  Hardpoints can come in any order so first parse them then second put them in the correct order
                    Dictionary<string, Hardpoint> hardpoints = new Dictionary<string, Hardpoint>();
                    foreach (JProperty module in json["modules"])
                    {
                        if (module.Name.Contains("Hardpoint"))
                        {
                            hardpoints.Add(module.Name, HardpointFromJson(module));
                        }
                    }
                    foreach (string size in HARDPOINT_SIZES)
                    {
                        for (int i = 1; i < 12; i++)
                        {
                            hardpoints.TryGetValue(size + "Hardpoint" + i, out Hardpoint hardpoint);
                            if (hardpoint != null)
                            {
                                Ship.hardpoints.Add(hardpoint);
                            }
                        }
                    }

                    // Obtain the compartments
                    foreach (dynamic module in json["modules"])
                    {
                        if (module.Name.Contains("Slot"))
                        {
                            Compartment compartment = CompartmentFromJson(module);
                            string moduleName = compartment.module?.invariantName ?? "";
                            if (moduleName == "Fuel Tank")
                            {
                                Ship.fueltanktotalcapacity += (decimal)Math.Pow(2, compartment.module.@class);
                            }
                            if (moduleName.Contains("Cargo Rack"))
                            {
                                Ship.cargocapacity += (int)Math.Pow(2, compartment.module.@class);
                            }

                            Ship.compartments.Add(compartment);
                        }
                    }
                }

                // Obtain the launchbays
                if (json["launchBays"] != null)
                {
                    foreach (dynamic launchbay in json["launchBays"])
                    {
                        if (launchbay.Name.Contains("Slot"))
                        {
                            Ship.launchbays.Add(LaunchBayFromJson(launchbay, Ship));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to parse Frontier API ship details", ex);
            }

            return Ship;
        }

        public static Hardpoint HardpointFromJson(dynamic json)
        {
            Hardpoint Hardpoint = new Hardpoint() { name = json.Name };

            string name = json.Name;
            if (name.StartsWith("Huge"))
            {
                Hardpoint.size = 4;
            }
            else if (name.StartsWith("Large"))
            {
                Hardpoint.size = 3;
            }
            else if (name.StartsWith("Medium"))
            {
                Hardpoint.size = 2;
            }
            else if (name.StartsWith("Small"))
            {
                Hardpoint.size = 1;
            }
            else if (name.StartsWith("Tiny"))
            {
                Hardpoint.size = 0;
            }

            if (json.Value is JObject)
            {
                if (json.Value.TryGetValue("module", out JToken value))
                {
                    Hardpoint.module = ModuleFromJson(name, json.Value);
                }
            }

            return Hardpoint;
        }

        public static Compartment CompartmentFromJson(dynamic json)
        {
            Compartment Compartment = new Compartment() { name = json.Name };

            // Compartments have name of form "Slotnn_Sizenn"
            Match matches = Regex.Match((string)json.Name, @"Size([0-9]+)");
            if (matches.Success)
            {
                Compartment.size = Int32.Parse(matches.Groups[1].Value);

                if (json.Value is JObject)
                {
                    if (json.Value.TryGetValue("module", out JToken value))
                    {
                        Compartment.module = ModuleFromJson((string)json.Name, json.Value);
                    }
                }
            }
            return Compartment;
        }

        public static Module ModuleFromJson(string name, JObject json)
        {
            long id = (long)json["module"]["id"];
            string edName = (string)json["module"]["name"];

            Module module = new Module(Module.FromEliteID(id) ?? Module.FromEDName(edName) ?? new Module());
            if (module.invariantName == null)
            {
                // Unknown module; report the full object so that we can update the definitions
                Logging.Info("Module definition error: " + edName, JsonConvert.SerializeObject(json["module"]));

                // Create a basic module & supplement from the info available
                module = new Module(id, edName, -1, edName, -1, "", (long)json["module"]["value"]);
            }

            module.price = (long)json["module"]["value"]; // How much we actually paid for it
            module.enabled = (bool)json["module"]["on"];
            module.priority = (int)json["module"]["priority"];
            module.health = (decimal)json["module"]["health"] / 10_000M;

            // Flag if module has engineering modifications
            module.modified = json["engineer"] != null ? true : false;

            return module;
        }

        public static LaunchBay LaunchBayFromJson(dynamic json, Ship ship)
        {
            LaunchBay launchbay = new LaunchBay() { name = json.Name };

            foreach (Compartment cpt in ship.compartments)
            {
                if (cpt.name == launchbay.name)
                {
                    switch (cpt.module.basename)
                    {
                        case "PlanetaryVehicleHangar":
                            launchbay.type = "SRV";
                            break;
                        case "FighterHangar":
                            launchbay.type = "Fighter";
                            break;
                    }
                }
            }

            // Launchbays have name of form "Slotnn_Sizenn", like compartments
            Match matches = Regex.Match((string)json.Name, @"Size([0-9]+)");
            if (matches.Success)
            {
                launchbay.size = Int32.Parse(matches.Groups[1].Value);

                if (json.Value is JObject)
                {
                    for (int subslot = 0; subslot <= 5; subslot++)
                    {
                        if (json.Value.TryGetValue("SubSlot" + subslot, out JToken value))
                        {
                            launchbay.vehicles.Add(VehicleFromJson(subslot, value));
                        }
                    }
                }
            }

            return launchbay;
        }

        public static Vehicle VehicleFromJson(int subslot, dynamic json)
        {
            Vehicle vehicle = new Vehicle
            {
                subslot = subslot,
                EDName = (string)json["name"],
                name = (string)json["locName"],
                rebuilds = (int)json["rebuilds"]
            };

            string loadout = (string)json["loadoutName"];
            loadout = loadout.Replace("(", "").Replace("&NBSP", "").Replace(")", "");
            string[] loadoutParts = loadout.Split(';');
            vehicle.loadout = loadoutParts[0];
            if (loadoutParts.Length > 1)
            {
                vehicle.mount = loadoutParts[1];
            }
            else
            {
                vehicle.mount = "";
            }

            return vehicle;
        }
    }
}
