using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipLoadoutEvent : Event
    {
        public const string NAME = "Ship loadout";
        public const string DESCRIPTION = "Triggered when you obtain the loadout of your ship";
        public const string SAMPLE = "{ \"timestamp\":\"2018-02-07T07:08:15Z\", \"event\":\"Loadout\", \"Ship\":\"TypeX\", \"ShipID\":70, \"ShipName\":\"\", \"ShipIdent\":\"TK-04T\", \"HullValue\":1818295, \"ModulesValue\":12319809, \"Rebuy\":90915, \"Modules\":[ { \"Slot\":\"LargeHardpoint1\", \"Item\":\"Hpt_PlasmaAccelerator_Fixed_Large\", \"On\":true, \"Priority\":2, \"AmmoInClip\":5, \"AmmoInHopper\":100, \"Health\":0.992188, \"Value\":297492, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673523, \"BlueprintName\":\"Weapon_Efficient\", \"Level\":4, \"Quality\":0.951000, \"Modifiers\":[ { \"Label\":\"PowerDraw\", \"Value\":1.261588, \"OriginalValue\":1.970000, \"LessIsGood\":1 }, { \"Label\":\"DamagePerSecond\", \"Value\":28.984520, \"OriginalValue\":24.173912, \"LessIsGood\":0 }, { \"Label\":\"Damage\", \"Value\":99.996597, \"OriginalValue\":83.400002, \"LessIsGood\":0 }, { \"Label\":\"DistributorDraw\", \"Value\":8.906640, \"OriginalValue\":13.600000, \"LessIsGood\":1 }, { \"Label\":\"ThermalLoad\", \"Value\":10.355176, \"OriginalValue\":21.750000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"LargeHardpoint2\", \"Item\":\"Hpt_PlasmaAccelerator_Fixed_Large\", \"On\":true, \"Priority\":0, \"AmmoInClip\":5, \"AmmoInHopper\":100, \"Health\":0.976563, \"Value\":297492, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673524, \"BlueprintName\":\"Weapon_Efficient\", \"Level\":5, \"Quality\":0.802500, \"Modifiers\":[ { \"Label\":\"PowerDraw\", \"Value\":1.055132, \"OriginalValue\":1.970000, \"LessIsGood\":1 }, { \"Label\":\"DamagePerSecond\", \"Value\":29.784678, \"OriginalValue\":24.173912, \"LessIsGood\":0 }, { \"Label\":\"Damage\", \"Value\":102.757141, \"OriginalValue\":83.400002, \"LessIsGood\":0 }, { \"Label\":\"DistributorDraw\", \"Value\":7.720721, \"OriginalValue\":13.600000, \"LessIsGood\":1 }, { \"Label\":\"ThermalLoad\", \"Value\":8.937075, \"OriginalValue\":21.750000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"MediumHardpoint1\", \"Item\":\"Hpt_PlasmaAccelerator_Fixed_Medium\", \"On\":true, \"Priority\":2, \"AmmoInClip\":5, \"AmmoInHopper\":100, \"Health\":1.000000, \"Value\":81335 }, { \"Slot\":\"SmallHardpoint1\", \"Item\":\"Hpt_Railgun_Fixed_Small\", \"On\":true, \"Priority\":2, \"AmmoInClip\":1, \"AmmoInHopper\":80, \"Health\":0.984375, \"Value\":5031, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673605, \"BlueprintName\":\"Weapon_LightWeight\", \"Level\":1, \"Quality\":1.000000, \"ExperimentalEffect\":\"special_feedback_cascade\", \"ExperimentalEffect_Localised\":\"Feedback Cascade\", \"Modifiers\":[ { \"Label\":\"Mass\", \"Value\":1.400000, \"OriginalValue\":2.000000, \"LessIsGood\":1 }, { \"Label\":\"Integrity\", \"Value\":32.000000, \"OriginalValue\":40.000000, \"LessIsGood\":0 }, { \"Label\":\"DamagePerSecond\", \"Value\":29.638098, \"OriginalValue\":37.047619, \"LessIsGood\":0 }, { \"Label\":\"Damage\", \"Value\":18.672001, \"OriginalValue\":23.340000, \"LessIsGood\":0 } ] } }, { \"Slot\":\"SmallHardpoint2\", \"Item\":\"Hpt_Railgun_Fixed_Small\", \"On\":true, \"Priority\":2, \"AmmoInClip\":1, \"AmmoInHopper\":80, \"Health\":0.987500, \"Value\":5031, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673610, \"BlueprintName\":\"Weapon_LongRange\", \"Level\":1, \"Quality\":1.000000, \"Modifiers\":[ { \"Label\":\"Mass\", \"Value\":2.200000, \"OriginalValue\":2.000000, \"LessIsGood\":1 }, { \"Label\":\"PowerDraw\", \"Value\":1.184500, \"OriginalValue\":1.150000, \"LessIsGood\":1 }, { \"Label\":\"MaximumRange\", \"Value\":3600.000244, \"OriginalValue\":3000.000000, \"LessIsGood\":0 }, { \"Label\":\"DamageFalloffRange\", \"Value\":3600.000244, \"OriginalValue\":1000.000000, \"LessIsGood\":0 } ] } }, { \"Slot\":\"SmallHardpoint3\", \"Item\":\"Hpt_Railgun_Fixed_Small\", \"On\":true, \"Priority\":2, \"AmmoInClip\":1, \"AmmoInHopper\":80, \"Health\":0.937500, \"Value\":5031 }, { \"Slot\":\"Armour\", \"Item\":\"TypeX_Armour_Reactive\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":4454188, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673654, \"BlueprintName\":\"Armour_Thermic\", \"Level\":5, \"Quality\":0.980000, \"ExperimentalEffect\":\"special_armour_chunky\", \"ExperimentalEffect_Localised\":\"Deep Plating\", \"Modifiers\":[ { \"Label\":\"DefenceModifierHealthMultiplier\", \"Value\":278.000031, \"OriginalValue\":250.000000, \"LessIsGood\":0 }, { \"Label\":\"KineticResistance\", \"Value\":13.480001, \"OriginalValue\":25.000000, \"LessIsGood\":0 }, { \"Label\":\"ThermicResistance\", \"Value\":13.278121, \"OriginalValue\":-39.999996, \"LessIsGood\":0 }, { \"Label\":\"ExplosiveResistance\", \"Value\":7.712001, \"OriginalValue\":19.999998, \"LessIsGood\":0 } ] } }, { \"Slot\":\"PowerPlant\", \"Item\":\"Int_Powerplant_Size6_Class5\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":1577505, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673766, \"BlueprintName\":\"PowerPlant_Boosted\", \"Level\":2, \"Quality\":1.000000, \"ExperimentalEffect\":\"special_powerplant_cooled\", \"ExperimentalEffect_Localised\":\"Thermal Spread\", \"Modifiers\":[ { \"Label\":\"Integrity\", \"Value\":111.599998, \"OriginalValue\":124.000000, \"LessIsGood\":0 }, { \"Label\":\"PowerCapacity\", \"Value\":29.988003, \"OriginalValue\":25.200001, \"LessIsGood\":0 }, { \"Label\":\"HeatEfficiency\", \"Value\":0.396000, \"OriginalValue\":0.400000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"MainEngines\", \"Item\":\"Int_Engine_Size6_Class5\", \"On\":true, \"Priority\":0, \"Health\":0.990512, \"Value\":1577505, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673659, \"BlueprintName\":\"Engine_Dirty\", \"Level\":5, \"Quality\":0.985700, \"ExperimentalEffect\":\"special_engine_cooled\", \"ExperimentalEffect_Localised\":\"Thermal Spread\", \"Modifiers\":[ { \"Label\":\"Mass\", \"Value\":42.000000, \"OriginalValue\":40.000000, \"LessIsGood\":1 }, { \"Label\":\"Integrity\", \"Value\":105.400002, \"OriginalValue\":124.000000, \"LessIsGood\":0 }, { \"Label\":\"PowerDraw\", \"Value\":8.467200, \"OriginalValue\":7.560000, \"LessIsGood\":1 }, { \"Label\":\"EngineOptimalMass\", \"Value\":1260.000000, \"OriginalValue\":1440.000000, \"LessIsGood\":0 }, { \"Label\":\"EngineOptPerformance\", \"Value\":139.899994, \"OriginalValue\":100.000000, \"LessIsGood\":0 }, { \"Label\":\"EngineHeatRate\", \"Value\":1.872000, \"OriginalValue\":1.300000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"FrameShiftDrive\", \"Item\":\"Int_Hyperdrive_Size5_Class5\", \"On\":true, \"Priority\":0, \"Health\":0.995098, \"Value\":497636, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673694, \"BlueprintName\":\"FSD_LongRange\", \"Level\":5, \"Quality\":0.974000, \"ExperimentalEffect\":\"special_fsd_cooled\", \"ExperimentalEffect_Localised\":\"Thermal Spread\", \"Modifiers\":[ { \"Label\":\"Mass\", \"Value\":26.000000, \"OriginalValue\":20.000000, \"LessIsGood\":1 }, { \"Label\":\"Integrity\", \"Value\":102.000000, \"OriginalValue\":120.000000, \"LessIsGood\":0 }, { \"Label\":\"PowerDraw\", \"Value\":0.690000, \"OriginalValue\":0.600000, \"LessIsGood\":1 }, { \"Label\":\"FSDOptimalMass\", \"Value\":1624.770020, \"OriginalValue\":1050.000000, \"LessIsGood\":0 }, { \"Label\":\"FSDHeatRate\", \"Value\":24.299999, \"OriginalValue\":27.000000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"LifeSupport\", \"Item\":\"Int_LifeSupport_Size5_Class5\", \"On\":true, \"Priority\":3, \"Health\":0.995652, \"Value\":121029 }, { \"Slot\":\"PowerDistributor\", \"Item\":\"Int_PowerDistributor_Size6_Class5\", \"On\":true, \"Priority\":1, \"Health\":0.995968, \"Value\":338880, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673739, \"BlueprintName\":\"PowerDistributor_HighFrequency\", \"Level\":5, \"Quality\":0.992200, \"ExperimentalEffect\":\"special_powerdistributor_fast\", \"ExperimentalEffect_Localised\":\"Super Conduits\", \"Modifiers\":[ { \"Label\":\"WeaponsCapacity\", \"Value\":45.599998, \"OriginalValue\":50.000000, \"LessIsGood\":0 }, { \"Label\":\"WeaponsRecharge\", \"Value\":7.837814, \"OriginalValue\":5.200000, \"LessIsGood\":0 }, { \"Label\":\"EnginesCapacity\", \"Value\":31.920000, \"OriginalValue\":35.000000, \"LessIsGood\":0 }, { \"Label\":\"EnginesRecharge\", \"Value\":4.823270, \"OriginalValue\":3.200000, \"LessIsGood\":0 }, { \"Label\":\"SystemsCapacity\", \"Value\":31.920000, \"OriginalValue\":35.000000, \"LessIsGood\":0 }, { \"Label\":\"SystemsRecharge\", \"Value\":4.823270, \"OriginalValue\":3.200000, \"LessIsGood\":0 } ] } }, { \"Slot\":\"Radar\", \"Item\":\"Int_Sensors_Size4_Class5\", \"On\":true, \"Priority\":1, \"Health\":0.982955, \"Value\":43225 }, { \"Slot\":\"FuelTank\", \"Item\":\"Int_FuelTank_Size4_Class3\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":2411 }, { \"Slot\":\"Slot01_Size5\", \"Item\":\"Int_Repairer_Size5_Class5\", \"On\":true, \"Priority\":4, \"AmmoInClip\":6700, \"Health\":0.990909, \"Value\":829049 }, { \"Slot\":\"Slot02_Size5\", \"Item\":\"Int_HullReinforcement_Size5_Class2\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":43875 }, { \"Slot\":\"Slot03_Size4\", \"Item\":\"Int_FSDInterdictor_Size4_Class5\", \"On\":true, \"Priority\":4, \"Health\":1.000000, \"Value\":2080391 }, { \"Slot\":\"Slot04_Size2\", \"Item\":\"Int_ModuleReinforcement_Size2_Class2\", \"On\":true, \"Priority\":1, \"Health\":0.985714, \"Value\":3510 }, { \"Slot\":\"Slot05_Size2\", \"Item\":\"Int_BuggyBay_Size2_Class2\", \"On\":true, \"Priority\":4, \"Health\":1.000000, \"Value\":2106 }, { \"Slot\":\"Military01\", \"Item\":\"Int_HullReinforcement_Size4_Class2\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":19013 }, { \"Slot\":\"Military02\", \"Item\":\"Int_HullReinforcement_Size4_Class2\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":19013 }, { \"Slot\":\"Military03\", \"Item\":\"Int_HullReinforcement_Size4_Class2\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":19013 }, { \"Slot\":\"PlanetaryApproachSuite\", \"Item\":\"Int_PlanetApproachSuite\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":48 }, { \"Slot\":\"VesselVoice\", \"Item\":\"VoicePack_Verity\", \"On\":true, \"Priority\":1, \"Health\":1.000000 }, { \"Slot\":\"ShipCockpit\", \"Item\":\"TypeX_Cockpit\", \"On\":true, \"Priority\":1, \"Health\":1.000000 }, { \"Slot\":\"CargoHatch\", \"Item\":\"ModularCargoBayDoor\", \"On\":true, \"Priority\":4, \"Health\":0.825000 } ] }";

        [PublicAPI("The ID of the ship")]
        public int shipid { get; private set; }

        [PublicAPI("The ship model")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The name of the ship")]
        public string shipname { get; private set; }

        [PublicAPI("The identification string of the ship")]
        public string shipident { get; private set; }

        [PublicAPI("The total value of the ship (hull + modules)")]
        public long? value => hullvalue + modulesvalue;

        [PublicAPI("The value of the ship's hull (less modules)")]
        public long? hullvalue { get; private set; }

        [PublicAPI("The value of the ship's modules (less hull)")]
        public long? modulesvalue { get; private set; }

        [PublicAPI("The unladen mass of the ship")]
        public decimal unladenmass { get; private set; }

        [PublicAPI("The max unlaiden jump range of the ship")]
        public decimal maxjumprange { get; private set; }

        [ PublicAPI( "The optimal mass value of the frame shift drive" ) ]
        public decimal optimalmass => Convert.ToDecimal( frameShiftDrive.GetFsdOptimalMass() );

        [PublicAPI("The rebuy value of the ship")]
        public long rebuy { get; private set; }

        [PublicAPI("The health of the ship's hull")]
        public decimal hullhealth { get; private set; }

        [PublicAPI("True if the ship is `hot`")]
        public bool hot { get; private set; }

        [PublicAPI("The paintjob of the ship")]
        public string paintjob { get; private set; }

        [PublicAPI("The hardpoints (objects) of the ship")]
        public List<Hardpoint> hardpoints { get; private set; }

        [PublicAPI("The compartments (objects) of the ship")]
        public List<Compartment> compartments { get; private set; }

        // Not intended to be user facing

        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);

        private Module frameShiftDrive { get; set; }

        public string edModel { get; private set; }

        public ShipLoadoutEvent(DateTime timestamp, string ship, int shipId, string shipName, string shipIdent, long? hullValue, long? modulesValue, decimal hullHealth, decimal unladenmass, decimal maxjumprange, long rebuy, bool hot, List<Compartment> compartments, List<Hardpoint> hardpoints, string paintjob) : base(timestamp, NAME)
        {
            this.edModel = ship;
            this.shipid = shipId;
            this.shipname = shipName;
            this.shipident = shipIdent;
            this.hullvalue = hullValue;
            this.modulesvalue = modulesValue;
            this.hullhealth = hullHealth;
            this.unladenmass = unladenmass;
            this.maxjumprange = maxjumprange;
            this.rebuy = rebuy;
            this.hot = hot;
            this.paintjob = paintjob;
            this.hardpoints = hardpoints ?? new List<Hardpoint>();
            this.compartments = compartments ?? new List<Compartment>();

            frameShiftDrive = this.compartments.FirstOrDefault( c =>
                c.name?.Equals( "FrameShiftDrive", StringComparison.InvariantCultureIgnoreCase ) ?? false )?.module;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var shipId = JsonParsing.getInt(data, "ShipID");
            var ship = JsonParsing.getString(data, "Ship");
            var shipName = JsonParsing.getString(data, "ShipName");
            var shipIdent = JsonParsing.getString(data, "ShipIdent");

            var hullValue = JsonParsing.getOptionalLong(data, "HullValue");
            var modulesValue = JsonParsing.getOptionalLong(data, "ModulesValue");
            var hullHealth = EventParsing.sensibleHealth((JsonParsing.getOptionalDecimal(data, "HullHealth") ?? 1) * 100);
            var unladenMass = JsonParsing.getOptionalDecimal(data, "UnladenMass") ?? 0;
            var maxJumpRange = JsonParsing.getOptionalDecimal(data, "MaxJumpRange") ?? 0;

            var rebuy = JsonParsing.getLong(data, "Rebuy");

            // If ship is 'hot', then modules are also 'hot'
            var hot = JsonParsing.getOptionalBool(data, "Hot") ?? false;

            data.TryGetValue( "Modules", out var val );
            var modulesData = (List<object>)val;

            string paintjob = null;
            var hardpoints = new List<Hardpoint>();
            var compartments = new List<Compartment>();
            if ( modulesData != null )
            {
                foreach ( var moduleData in modulesData.Cast<IDictionary<string, object>>() )
                {
                    // Common items
                    var slot = JsonParsing.getString(moduleData, "Slot");
                    var item = JsonParsing.getString(moduleData, "Item");
                    var enabled = JsonParsing.getBool(moduleData, "On");
                    var priority = JsonParsing.getInt(moduleData, "Priority");
                    // Health is as 0->1 but we want 0->100, and to a sensible number of decimal places
                    var health = JsonParsing.getDecimal(moduleData, "Health") * 100;
                    health = health < 5 ? Math.Round( health, 1 ) : Math.Round( health );

                    // Some built-in modules don't give "Value" keys in the Loadout event. We'll set them to zero to match the Frontier API.
                    var price = JsonParsing.getOptionalLong(moduleData, "Value") ?? 0;

                    // Ammunition
                    var clip = JsonParsing.getOptionalInt(moduleData, "AmmoInClip");
                    var hopper = JsonParsing.getOptionalInt(moduleData, "AmmoInHopper");

                    // Engineering modifications
                    moduleData.TryGetValue( "Engineering", out var engineeringVal );
                    var modified = engineeringVal != null;
                    var engineeringData = (Dictionary<string, object>)engineeringVal;
                    var blueprint = modified ? JsonParsing.getString(engineeringData, "BlueprintName") : null;
                    var blueprintId = modified ? JsonParsing.getLong(engineeringData, "BlueprintID") : 0;
                    var level = modified ? JsonParsing.getInt(engineeringData, "Level") : 0;
                    var modification = Blueprint.FromEliteID(blueprintId, engineeringData)
                                                ?? Blueprint.FromEDNameAndGrade(blueprint, level) ?? Blueprint.None;
                    var quality = modified ? JsonParsing.getDecimal(engineeringData, "Quality") : 0;
                    var experimentalEffect = modified ? JsonParsing.getString(engineeringData, "ExperimentalEffect") : null;
                    var modifiers = new List<EngineeringModifier>();
                    if ( modified )
                    {
                        engineeringData.TryGetValue( "Modifiers", out var modifiersVal );
                        var modifiersData = (List<object>)modifiersVal;
                        foreach ( var modifier in modifiersData.Cast<IDictionary<string, object>>() )
                        {
                            try
                            {
                                var edname = JsonParsing.getString(modifier, "Label");
                                var currentValue = JsonParsing.getOptionalDecimal(modifier, "Value");
                                var originalValue = JsonParsing.getOptionalDecimal(modifier, "OriginalValue");
                                var lessIsGood = JsonParsing.getOptionalInt(modifier, "LessIsGood") == 1;
                                var valueStr = JsonParsing.getString(modifier, "ValueStr");
                                modifiers.Add( new EngineeringModifier
                                {
                                    EDName = edname,
                                    currentValue = currentValue,
                                    originalValue = originalValue,
                                    lessIsGood = lessIsGood,
                                    valueStr = valueStr
                                } );
                            }
                            catch ( Exception e )
                            {
                                Logging.Error( $"Failed to parse engineering modification for item {JsonConvert.SerializeObject( item )}", e );
                            }
                        }
                    }
                    if ( slot.Contains( "Hardpoint" ) )
                    {
                        // This is a hardpoint
                        var hardpoint = new Hardpoint() { name = slot };
                        if ( hardpoint.name.StartsWith( "Tiny" ) )
                        {
                            hardpoint.size = 0;
                        }
                        else if ( hardpoint.name.StartsWith( "Small", StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            hardpoint.size = 1;
                        }
                        else if ( hardpoint.name.StartsWith( "Medium", StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            hardpoint.size = 2;
                        }
                        else if ( hardpoint.name.StartsWith( "Large", StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            hardpoint.size = 3;
                        }
                        else if ( hardpoint.name.StartsWith( "Huge", StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            hardpoint.size = 4;
                        }

                        var module = new Module(Module.FromEDName(item, moduleData) ?? new Module());
                        if ( module.edname == null )
                        {
                            Logging.Info( "Unknown module " + item, JsonConvert.SerializeObject( moduleData ) );
                        }
                        else
                        {
                            module.hot = hot;
                            module.enabled = enabled;
                            module.priority = priority;
                            module.health = health;
                            module.price = price;
                            module.ammoinclip = clip;
                            module.ammoinhopper = hopper;
                            module.modified = modified;
                            module.modificationEDName = blueprint;
                            module.engineermodification = modification;
                            module.engineerlevel = level;
                            module.engineerquality = quality;
                            module.engineerExperimentalEffectEDName = experimentalEffect;
                            module.modifiers = modifiers;
                            hardpoint.module = module;
                            hardpoints.Add( hardpoint );
                        }
                    }
                    else if ( slot.Equals( "PaintJob", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // This is a paintjob
                        paintjob = item;
                    }
                    else if ( slot.Equals( "PlanetaryApproachSuite", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore planetary approach suite for now
                    }
                    else if ( slot.StartsWith( "Bobble", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore bobbles
                    }
                    else if ( slot.StartsWith( "Decal", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore decals
                    }
                    else if ( slot.StartsWith( "StringLights", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore string lights
                    }
                    else if ( slot.Equals( "WeaponColour", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore weapon colour
                    }
                    else if ( slot.Equals( "EngineColour", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore engine colour
                    }
                    else if ( slot.StartsWith( "ShipKit", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore ship kits
                    }
                    else if ( slot.StartsWith( "ShipName", StringComparison.InvariantCultureIgnoreCase ) || slot.StartsWith( "ShipID", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore nameplates
                    }
                    else if ( slot.Equals( "VesselVoice", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore the chosen voice
                    }
                    else if ( slot.Equals( "DataLinkScanner", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore the data link scanner
                    }
                    else if ( slot.Equals( "CodexScanner", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore the codex scanner
                    }
                    else if ( slot.Equals( "Hologram", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // Ignore hologram cosmetics
                    }
                    else if ( slot.Equals( "CargoHatch", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // The cargo hatch is a special slot. Every ship has a cargo hatch. Some have unique names but there's no functional difference between them.
                        var compartment = EventParsing.ShipCompartment(ship, slot);
                        var module = new Module(Module.FromEDName("ModularCargoBayDoor", moduleData) ?? new Module())
                        {
                            enabled = enabled,
                            priority = priority,
                            health = health
                        };
                        compartment.module = module;
                        compartments.Add( compartment );
                    }
                    else if ( slot.Equals( "ShipCockpit", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        // The cockpit is a special slot. Every ship has a cockpit module with a unique name but there's no functional difference between them.
                        var compartment = EventParsing.ShipCompartment(ship, slot);
                        var module = new Module(Module.FromEDName("Cockpit", moduleData) ?? new Module())
                        {
                            enabled = enabled,
                            priority = priority,
                            health = health
                        };
                        compartment.module = module;
                        compartments.Add( compartment );
                    }
                    else
                    {
                        // This is a compartment
                        var compartment = EventParsing.ShipCompartment(ship, slot);
                        // Compartment slots may be in the form of "Slotnn_Sizen" or "Militarynn"

                        var module = new Module(Module.FromEDName(item, moduleData) ?? new Module());
                        if ( module.edname == null )
                        {
                            Logging.Info( "Unknown module " + item, JsonConvert.SerializeObject( moduleData ) );
                        }
                        else
                        {
                            module.hot = hot;
                            module.enabled = enabled;
                            module.priority = priority;
                            module.health = health;
                            module.price = price;
                            module.ammoinclip = clip;
                            module.ammoinhopper = hopper;
                            module.modified = modified;
                            module.modificationEDName = blueprint;
                            module.engineermodification = modification;
                            module.engineerlevel = level;
                            module.engineerquality = quality;
                            module.engineerExperimentalEffectEDName = experimentalEffect;
                            module.modifiers = modifiers;
                            compartment.module = module;
                            compartments.Add( compartment );
                        }
                    }
                }
            }
            events.Add( new ShipLoadoutEvent( timestamp, ship, shipId, shipName, shipIdent, hullValue, modulesValue, hullHealth, unladenMass, maxJumpRange, rebuy, hot, compartments, hardpoints, paintjob ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
