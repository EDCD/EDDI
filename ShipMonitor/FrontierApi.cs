using EddiCore;
using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace EddiShipMonitor
{
    // Handle the Frontier API definition for ships
    public static class FrontierApi
    {
        private static readonly List<string> HARDPOINT_SIZES = [ "Huge", "Large", "Medium", "Small", "Tiny" ];

        public static async Task<List<Ship>> ShipyardFromJsonAsync(Ship activeShip, dynamic json)
        {
            var shipyard = new List<Ship>();

            foreach (dynamic shipJson in json["ships"])
            {
                if (shipJson != null)
                {
                    // Take underlying value if present
                    JObject shipObj = shipJson.Value ?? shipJson;
                    var ship = ShipFromJson(shipObj);
                    if (activeShip?.LocalId == ship.LocalId)
                    {
                        // This is the active ship so add that instead
                        shipyard.Add(activeShip);
                    }
                    else
                    {
                        if (shipObj["starsystem"] != null)
                        {
                            // Get the ship's location for distance calculations
                            var shipSystemAddress = shipObj["starsystem"]["systemaddress"]?.ToObject<ulong?>();
                            var shipMarketId = shipObj["station"]?["id"]?.ToObject<long?>();
                            var stationWaypoint = shipSystemAddress is null || shipMarketId is null
                                ? null 
                                : await EDDI.Instance.DataProvider.GetOrFetchStationWaypointAsync( (ulong)shipSystemAddress, (long)shipMarketId ).ConfigureAwait(false);
                            ship.StoredLocation =
                                stationWaypoint is null ? null : new Ship.Location( stationWaypoint );
                            ship.distance = ship.DistanceLY( EDDI.Instance.GameState.CurrentStarSystem );
                            shipyard.Add( ship );
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

            var edName = (string)json["name"];
            var Ship = ShipDefinitions.FromEDModel(edName, false);
            if (Ship == null)
            {
                // Unknown ship; report the full object so that we can update the definitions 
                Logging.Info("Ship definition error: " + edName, JsonConvert.SerializeObject(json));

                // Create a basic ship definition & supplement from the info available 
                Ship = new Ship { EDName = edName };
            }

            // We want to return a basic ship if the parsing fails so wrap this
            try
            {
                if (json["id"] is null) { throw new MissingFieldException("Ship 'id' property is missing"); }
                Ship.LocalId = json["id"].Value<int>();
                Ship.name = (string)json.GetValue("shipName");
                Ship.ident = (string)json.GetValue("shipID");

                Ship.value = (long)(json["value"]?["hull"] ?? 0) + (long)(json["value"]?["modules"] ?? 0);

                var healthOutOf1e6 = (decimal?)json["health"]?["hull"];
                if (healthOutOf1e6 != null)
                {
                    var healthPercent = (decimal)healthOutOf1e6 / 10_000M;
                    Ship.health = healthPercent;
                }

                if (json["modules"] != null)
                {
                    // Obtain the internals
                    Ship.bulkheads = ModuleFromJson( (JObject)json["modules"]["Armour"]);
                    Ship.powerplant = ModuleFromJson( (JObject)json["modules"]["PowerPlant"]);
                    Ship.thrusters = ModuleFromJson( (JObject)json["modules"]["MainEngines"]);
                    Ship.frameshiftdrive = ModuleFromJson( (JObject)json["modules"]["FrameShiftDrive"]);
                    Ship.lifesupport = ModuleFromJson( (JObject)json["modules"]["LifeSupport"]);
                    Ship.powerdistributor = ModuleFromJson( (JObject)json["modules"]["PowerDistributor"]);
                    Ship.sensors = ModuleFromJson( (JObject)json["modules"]["Radar"]);
                    Ship.fueltank = ModuleFromJson( (JObject)json["modules"]["FuelTank"]);

                    // Obtain the hardpoints.  Hardpoints can come in any order so first parse them then second put them in the correct order
                    var hardpoints = new Dictionary<string, Hardpoint>();
                    foreach (var module in json["modules"].Cast<JProperty>())
                    {
                        if (module.Name.Contains("Hardpoint"))
                        {
                            hardpoints.Add(module.Name, HardpointFromJson(module));
                        }
                    }
                    foreach (var size in HARDPOINT_SIZES)
                    {
                        for (var i = 1; i < 12; i++)
                        {
                            hardpoints.TryGetValue(size + "Hardpoint" + i, out var hardpoint);
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

        private static Hardpoint HardpointFromJson(dynamic json)
        {
            var Hardpoint = new Hardpoint() { name = json.Name };

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
                if (json.Value.TryGetValue("module", out JToken _))
                {
                    Hardpoint.module = ModuleFromJson( json.Value);
                }
            }

            return Hardpoint;
        }

        private static Compartment CompartmentFromJson(dynamic json)
        {
            var Compartment = new Compartment() { name = json.Name };

            // Compartments have name of form "Slotnn_Sizenn"
            var matches = GeneratedRegex.ShipSlotSizeRegex().Match((string)json.Name);
            if (matches.Success)
            {
                Compartment.size = int.Parse(matches.Groups[2].Value);

                if (json.Value is JObject)
                {
                    if (json.Value.TryGetValue("module", out JToken _))
                    {
                        Compartment.module = ModuleFromJson( json.Value);
                    }
                }
            }
            return Compartment;
        }

        private static Module ModuleFromJson(JObject json)
        {
            var edName = (string)json["module"]["name"];

            var module = new Module(Module.FromEDName(edName, json["module"]) ?? new Module());
            if (module.invariantName == null)
            {
                // Unknown module; report the full object so that we can update the definitions
                Logging.Info("Module definition error: " + edName, JsonConvert.SerializeObject(json["module"]));

                // Create a basic module & supplement from the info available
                module = new Module( edName, edName, -1, "", (long)json["module"]["value"]);
            }

            module.fallbackLocalizedName = (string)json["module"]["locName"];
            module.price = (long)json["module"]["value"]; // How much we actually paid for it in credits
            module.mercPrice = (long?)json[ "module" ][ "additionalCost" ]?[ "mercCoins" ]; // How much we actually paid for it in merc coins
            module.enabled = (bool)json["module"]["on"];
            module.priority = (int)json["module"]["priority"];
            module.health = (decimal)json["module"]["health"] / 10_000M;

            // Engineering modifications
            module.modified = json[ "WorkInProgress_modifications" ] != null || json[ "specialModifications" ] != null;
            if (module.modified)
            {
                var blueprintName = (string)json["engineer"]?["recipeName"];
                var blueprintGrade = (int)json["engineer"]?["recipeLevel"];
                module.modificationEDName = blueprintName;
                module.engineerlevel = blueprintGrade;
                module.engineermodification = Blueprint.FromEDNameAndGrade(blueprintName, blueprintGrade);
                module.blueprintId = module.engineermodification?.blueprintId ?? 0;
                module.engineerExperimentalEffectEDName = json["specialModifications"].ToObject<KeyValuePair<string, string>>().Value;

                if ( module.edname.Contains("hyperdrive") )
                {
                    // Get the ship FSD's optimal mass for jump calculations
                    var fsdOptimalMassMultiplier = json[ "WorkInProgress_modifications" ]?["OutfittingFieldType_FSDOptimalMass"];
                    if ( fsdOptimalMassMultiplier != null )
                    {
                        var baseOptimalMass = Convert.ToDecimal( module.GetFsdBaseOptimalMass() );
                        var modifier = module.modifiers.FirstOrDefault( m => m.EDName == "FSDOptimalMass" );
                        if ( modifier is null )
                        {
                            modifier = new EngineeringModifier
                            {
                                EDName = "FSDOptimalMass",
                                lessIsGood = false,
                            };
                            module.modifiers.Add( modifier );
                        }
                        modifier.currentValue = baseOptimalMass * (decimal)fsdOptimalMassMultiplier[ "value" ];
                        modifier.originalValue = baseOptimalMass;
                    }

                    // Get the ship FSD's max fuel per jump for jump calculations
                    var fsdMaxFuelPerJumpMultiplier = json[ "WorkInProgress_modifications" ]?["OutfittingFieldType_MaxFuelPerJump"];
                    if ( fsdMaxFuelPerJumpMultiplier != null )
                    {
                        var baseMaxFuelPerJump = module.GetFsdMaxFuelPerJump();
                        var modifier = module.modifiers.FirstOrDefault( m => m.EDName == "MaxFuelPerJump" );
                        if ( modifier is null )
                        {
                            modifier = new EngineeringModifier
                            {
                                EDName = "MaxFuelPerJump",
                                lessIsGood = false,
                            };
                            module.modifiers.Add( modifier );
                        }
                        modifier.currentValue = baseMaxFuelPerJump * (decimal)fsdMaxFuelPerJumpMultiplier[ "value" ];
                        modifier.originalValue = baseMaxFuelPerJump;
                    }
                }
            }
            return module;
        }

        private static LaunchBay LaunchBayFromJson(dynamic json, Ship ship)
        {
            var launchbay = new LaunchBay() { name = json.Name };

            foreach (var cpt in ship.compartments)
            {
                if (cpt.name == launchbay.name)
                {
                    if ( cpt.module?.basename?.Contains( "PlanetaryVehicleHangar", StringComparison.OrdinalIgnoreCase ) ?? false )
                    {
                        launchbay.type = "SRV";
                        continue;
                    }

                    if ( cpt.module?.basename?.Contains( "VesselHangar", StringComparison.OrdinalIgnoreCase ) ?? false )
                    {
                        launchbay.type = "Vessel";
                        continue;
                    }
                }
            }

            // Launchbays have name of form "Slotnn_Sizenn", like compartments
            var matches = GeneratedRegex.ShipSlotSizeRegex().Match((string)json.Name);
            if ( matches.Success)
            {
                launchbay.size = int.Parse(matches.Groups[2].Value);

                if (json.Value is JObject)
                {
                    for (var subslot = 0; subslot <= 5; subslot++)
                    {
                        if (json.Value.TryGetValue("SubSlot" + subslot, out JToken value))
                        {
                            launchbay.vehicles.Add(Vehicle.FromJson(subslot, value));
                        }
                    }
                }
            }

            return launchbay;
        }
    }
}
