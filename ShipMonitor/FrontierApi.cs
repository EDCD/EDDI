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

        // Translations from the internal names used by Frontier to clean human-readable
        private static Dictionary<string, string> shipTranslations = new Dictionary<string, string>()
        {
            { "Adder" , "Adder"},
            { "Anaconda", "Anaconda" },
            { "Asp", "Asp Explorer" },
            { "Asp_Scout", "Asp Scout" },
            { "BelugaLiner", "Beluga Liner" },
            { "CobraMkIII", "Cobra Mk. III" },
            { "CobraMkIV", "Cobra Mk. IV" },
            { "Cutter", "Imperial Cutter" },
            { "DiamondBack", "Diamondback Scout" },
            { "DiamondBackXL", "Diamondback Explorer" },
            { "Dolphin", "Dolphin" },
            { "Eagle", "Eagle" },
            { "Empire_Courier", "Imperial Courier" },
            { "Empire_Eagle", "Imperial Eagle" },
            { "Empire_Fighter", "Imperial Fighter" },
            { "Empire_Trader", "Imperial Clipper" },
            { "Federation_Corvette", "Federal Corvette" },
            { "Federation_Dropship", "Federal Dropship" },
            { "Federation_Dropship_MkII", "Federal Assault Ship" },
            { "Federation_Gunship", "Federal Gunship" },
            { "Federation_Fighter", "F63 Condor" },
            { "FerDeLance", "Fer-de-Lance" },
            { "Hauler", "Hauler" },
            { "Independant_Trader", "Keelback" },
            { "Orca", "Orca" },
            { "Python", "Python" },
            { "SideWinder", "Sidewinder" },
            { "Type6", "Type-6 Transporter" },
            { "Type7", "Type-7 Transporter" },
            { "Type9", "Type-9 Heavy" },
            { "Viper", "Viper Mk. III" },
            { "Viper_MkIV", "Viper Mk. IV" },
            { "Vulture", "Vulture" }
        };

        public static List<Ship> ShipyardFromJson(dynamic json)
        {
            List<Ship> shipyard = new List<Ship>();

            foreach (dynamic shipJson in json["ships"])
            {
                if (shipJson != null)
                {
                    // Take underlying value if present
                    JObject shipObj = shipJson.Value == null ? shipJson : shipJson.Value;
                    if (shipObj != null)
                    {
                        Ship ship = ShipFromJson(shipObj);
                        if (shipObj["starsystem"] != null)
                        {
                            ship.starsystem = (string)shipObj["starsystem"]["name"];
                            ship.station = (string)shipObj["station"]["name"];
                        }
                        shipyard.Add(ship);
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

            Ship Ship = ShipDefinitions.FromEDModel(json.GetValue("name").Value<string>());

            Ship.raw = json.ToString(Formatting.None);

            Ship.LocalId = json.GetValue("id").Value<int>();

            Logging.Debug("Local ID is " + Ship.LocalId);

            // Some ship information is just skeleton data of the ship's ID.  Use value as our canary to see if there is more data
            if (json["value"] != null)
            {
                Ship.value = (long)json["value"]["hull"] + (long)json["value"]["modules"];

                Ship.cargocapacity = (int)json["cargo"]["capacity"];
                Ship.cargocarried = (int)json["cargo"]["qty"];

                // Be sensible with health - round it unless it's very low
                decimal Health = (decimal)json["health"]["hull"] / 10000;
                if (Health < 5)
                {
                    Ship.health = Math.Round(Health, 1);
                }
                else
                {
                    Ship.health = Math.Round(Health);
                }

                // Obtain the internals
                Ship.bulkheads = ModuleFromJson("Armour", (JObject)json["modules"]["Armour"]);
                Ship.powerplant = ModuleFromJson("PowerPlant", (JObject)json["modules"]["PowerPlant"]);
                Ship.thrusters = ModuleFromJson("MainEngines", (JObject)json["modules"]["MainEngines"]);
                Ship.frameshiftdrive = ModuleFromJson("FrameShiftDrive", (JObject)json["modules"]["FrameShiftDrive"]);
                Ship.lifesupport = ModuleFromJson("LifeSupport", (JObject)json["modules"]["LifeSupport"]);
                Ship.powerdistributor = ModuleFromJson("PowerDistributor", (JObject)json["modules"]["PowerDistributor"]);
                Ship.sensors = ModuleFromJson("Radar", (JObject)json["modules"]["Radar"]);
                Ship.fueltank = ModuleFromJson("FuelTank", (JObject)json["modules"]["FuelTank"]);
                Ship.fueltankcapacity = (decimal)Math.Pow(2, Ship.fueltank.@class);
                Ship.fueltanktotalcapacity = (decimal)json["fuel"]["main"]["capacity"];

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
                        Hardpoint hardpoint;
                        hardpoints.TryGetValue(size + "Hardpoint" + i, out hardpoint);
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
                        Ship.compartments.Add(CompartmentFromJson(module));
                    }
                }

                // Obtain the cargo
                Ship.cargo = new List<Cargo>();
                if (json["cargo"] != null && json["cargo"]["items"] != null)
                {
                    foreach (dynamic cargoJson in json["cargo"]["items"])
                    {
                        if (cargoJson != null && cargoJson["commodity"] != null)
                        {
                            string name = (string)cargoJson["commodity"];
                            Cargo cargo = new Cargo();
                            cargo.commodity = CommodityDefinitions.FromName(name);
                            if (cargo.commodity.name == null)
                            {
                                // Unknown commodity; log an error so that we can update the definitions
                                Logging.Error("No commodity definition for cargo", cargoJson.ToString(Formatting.None));
                                cargo.commodity.name = name;
                            }
                            cargo.amount = (int)cargoJson["qty"];
                            cargo.price = (long)cargoJson["value"] / cargo.amount;
                            cargo.missionid = (long?)cargoJson["mission"];
                            cargo.stolen = ((int?)(long?)cargoJson["marked"]) == 1;

                            Ship.cargo.Add(cargo);
                        }
                    }
                }
            }

            return Ship;
        }

        public static Hardpoint HardpointFromJson(dynamic json)
        {
            Hardpoint Hardpoint = new Hardpoint();

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
                JToken value;
                if (json.Value.TryGetValue("module", out value))
                {
                    Hardpoint.module = ModuleFromJson(name, json.Value);
                }
            }

            return Hardpoint;
        }

        public static Compartment CompartmentFromJson(dynamic json)
        {
            Compartment Compartment = new Compartment();

            // Compartments have name of form "Slotnn_Sizenn"
            Match matches = Regex.Match((string)json.Name, @"Size([0-9]+)");
            if (matches.Success)
            {
                Compartment.size = Int32.Parse(matches.Groups[1].Value);

                if (json.Value is JObject)
                {
                    JToken value;
                    if (json.Value.TryGetValue("module", out value))
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
            Module module = ModuleDefinitions.ModuleFromEliteID(id);
            if (module.name == null)
            {
                // Unknown module; log an error so that we can update the definitions
                Logging.Error("No definition for ship module", json["module"].ToString(Formatting.None));
            }

            module.price = (long)json["module"]["value"];
            module.enabled = (bool)json["module"]["on"];
            module.priority = (int)json["module"]["priority"];
            // Be sensible with health - round it unless it's very low
            decimal Health = (decimal)json["module"]["health"] / 10000;
            if (Health < 5)
            {
                module.health = Math.Round(Health, 1);
            }
            else
            {
                module.health = Math.Round(Health);
            }

            // Flag if module has modifications
            if (json["module"]["modifiers"] != null)
            {
                module.modified = true;
            }

            return module;
        }
    }
}
