﻿using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ShipLoadoutEvent : Event
    {
        public const string NAME = "Ship loadout";
        public const string DESCRIPTION = "Triggered when you obtain the loadout of your ship";
        public const string SAMPLE = "{ \"timestamp\":\"2018-02-07T07:08:15Z\", \"event\":\"Loadout\", \"Ship\":\"TypeX\", \"ShipID\":70, \"ShipName\":\"\", \"ShipIdent\":\"TK-04T\", \"HullValue\":1818295, \"ModulesValue\":12319809, \"Rebuy\":90915, \"Modules\":[ { \"Slot\":\"LargeHardpoint1\", \"Item\":\"Hpt_PlasmaAccelerator_Fixed_Large\", \"On\":true, \"Priority\":2, \"AmmoInClip\":5, \"AmmoInHopper\":100, \"Health\":0.992188, \"Value\":297492, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673523, \"BlueprintName\":\"Weapon_Efficient\", \"Level\":4, \"Quality\":0.951000, \"Modifiers\":[ { \"Label\":\"PowerDraw\", \"Value\":1.261588, \"OriginalValue\":1.970000, \"LessIsGood\":1 }, { \"Label\":\"DamagePerSecond\", \"Value\":28.984520, \"OriginalValue\":24.173912, \"LessIsGood\":0 }, { \"Label\":\"Damage\", \"Value\":99.996597, \"OriginalValue\":83.400002, \"LessIsGood\":0 }, { \"Label\":\"DistributorDraw\", \"Value\":8.906640, \"OriginalValue\":13.600000, \"LessIsGood\":1 }, { \"Label\":\"ThermalLoad\", \"Value\":10.355176, \"OriginalValue\":21.750000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"LargeHardpoint2\", \"Item\":\"Hpt_PlasmaAccelerator_Fixed_Large\", \"On\":true, \"Priority\":0, \"AmmoInClip\":5, \"AmmoInHopper\":100, \"Health\":0.976563, \"Value\":297492, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673524, \"BlueprintName\":\"Weapon_Efficient\", \"Level\":5, \"Quality\":0.802500, \"Modifiers\":[ { \"Label\":\"PowerDraw\", \"Value\":1.055132, \"OriginalValue\":1.970000, \"LessIsGood\":1 }, { \"Label\":\"DamagePerSecond\", \"Value\":29.784678, \"OriginalValue\":24.173912, \"LessIsGood\":0 }, { \"Label\":\"Damage\", \"Value\":102.757141, \"OriginalValue\":83.400002, \"LessIsGood\":0 }, { \"Label\":\"DistributorDraw\", \"Value\":7.720721, \"OriginalValue\":13.600000, \"LessIsGood\":1 }, { \"Label\":\"ThermalLoad\", \"Value\":8.937075, \"OriginalValue\":21.750000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"MediumHardpoint1\", \"Item\":\"Hpt_PlasmaAccelerator_Fixed_Medium\", \"On\":true, \"Priority\":2, \"AmmoInClip\":5, \"AmmoInHopper\":100, \"Health\":1.000000, \"Value\":81335 }, { \"Slot\":\"SmallHardpoint1\", \"Item\":\"Hpt_Railgun_Fixed_Small\", \"On\":true, \"Priority\":2, \"AmmoInClip\":1, \"AmmoInHopper\":80, \"Health\":0.984375, \"Value\":5031, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673605, \"BlueprintName\":\"Weapon_LightWeight\", \"Level\":1, \"Quality\":1.000000, \"ExperimentalEffect\":\"special_feedback_cascade\", \"ExperimentalEffect_Localised\":\"Feedback Cascade\", \"Modifiers\":[ { \"Label\":\"Mass\", \"Value\":1.400000, \"OriginalValue\":2.000000, \"LessIsGood\":1 }, { \"Label\":\"Integrity\", \"Value\":32.000000, \"OriginalValue\":40.000000, \"LessIsGood\":0 }, { \"Label\":\"DamagePerSecond\", \"Value\":29.638098, \"OriginalValue\":37.047619, \"LessIsGood\":0 }, { \"Label\":\"Damage\", \"Value\":18.672001, \"OriginalValue\":23.340000, \"LessIsGood\":0 } ] } }, { \"Slot\":\"SmallHardpoint2\", \"Item\":\"Hpt_Railgun_Fixed_Small\", \"On\":true, \"Priority\":2, \"AmmoInClip\":1, \"AmmoInHopper\":80, \"Health\":0.987500, \"Value\":5031, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673610, \"BlueprintName\":\"Weapon_LongRange\", \"Level\":1, \"Quality\":1.000000, \"Modifiers\":[ { \"Label\":\"Mass\", \"Value\":2.200000, \"OriginalValue\":2.000000, \"LessIsGood\":1 }, { \"Label\":\"PowerDraw\", \"Value\":1.184500, \"OriginalValue\":1.150000, \"LessIsGood\":1 }, { \"Label\":\"MaximumRange\", \"Value\":3600.000244, \"OriginalValue\":3000.000000, \"LessIsGood\":0 }, { \"Label\":\"DamageFalloffRange\", \"Value\":3600.000244, \"OriginalValue\":1000.000000, \"LessIsGood\":0 } ] } }, { \"Slot\":\"SmallHardpoint3\", \"Item\":\"Hpt_Railgun_Fixed_Small\", \"On\":true, \"Priority\":2, \"AmmoInClip\":1, \"AmmoInHopper\":80, \"Health\":0.937500, \"Value\":5031 }, { \"Slot\":\"Armour\", \"Item\":\"TypeX_Armour_Reactive\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":4454188, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673654, \"BlueprintName\":\"Armour_Thermic\", \"Level\":5, \"Quality\":0.980000, \"ExperimentalEffect\":\"special_armour_chunky\", \"ExperimentalEffect_Localised\":\"Deep Plating\", \"Modifiers\":[ { \"Label\":\"DefenceModifierHealthMultiplier\", \"Value\":278.000031, \"OriginalValue\":250.000000, \"LessIsGood\":0 }, { \"Label\":\"KineticResistance\", \"Value\":13.480001, \"OriginalValue\":25.000000, \"LessIsGood\":0 }, { \"Label\":\"ThermicResistance\", \"Value\":13.278121, \"OriginalValue\":-39.999996, \"LessIsGood\":0 }, { \"Label\":\"ExplosiveResistance\", \"Value\":7.712001, \"OriginalValue\":19.999998, \"LessIsGood\":0 } ] } }, { \"Slot\":\"PowerPlant\", \"Item\":\"Int_Powerplant_Size6_Class5\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":1577505, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673766, \"BlueprintName\":\"PowerPlant_Boosted\", \"Level\":2, \"Quality\":1.000000, \"ExperimentalEffect\":\"special_powerplant_cooled\", \"ExperimentalEffect_Localised\":\"Thermal Spread\", \"Modifiers\":[ { \"Label\":\"Integrity\", \"Value\":111.599998, \"OriginalValue\":124.000000, \"LessIsGood\":0 }, { \"Label\":\"PowerCapacity\", \"Value\":29.988003, \"OriginalValue\":25.200001, \"LessIsGood\":0 }, { \"Label\":\"HeatEfficiency\", \"Value\":0.396000, \"OriginalValue\":0.400000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"MainEngines\", \"Item\":\"Int_Engine_Size6_Class5\", \"On\":true, \"Priority\":0, \"Health\":0.990512, \"Value\":1577505, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673659, \"BlueprintName\":\"Engine_Dirty\", \"Level\":5, \"Quality\":0.985700, \"ExperimentalEffect\":\"special_engine_cooled\", \"ExperimentalEffect_Localised\":\"Thermal Spread\", \"Modifiers\":[ { \"Label\":\"Mass\", \"Value\":42.000000, \"OriginalValue\":40.000000, \"LessIsGood\":1 }, { \"Label\":\"Integrity\", \"Value\":105.400002, \"OriginalValue\":124.000000, \"LessIsGood\":0 }, { \"Label\":\"PowerDraw\", \"Value\":8.467200, \"OriginalValue\":7.560000, \"LessIsGood\":1 }, { \"Label\":\"EngineOptimalMass\", \"Value\":1260.000000, \"OriginalValue\":1440.000000, \"LessIsGood\":0 }, { \"Label\":\"EngineOptPerformance\", \"Value\":139.899994, \"OriginalValue\":100.000000, \"LessIsGood\":0 }, { \"Label\":\"EngineHeatRate\", \"Value\":1.872000, \"OriginalValue\":1.300000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"FrameShiftDrive\", \"Item\":\"Int_Hyperdrive_Size5_Class5\", \"On\":true, \"Priority\":0, \"Health\":0.995098, \"Value\":497636, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673694, \"BlueprintName\":\"FSD_LongRange\", \"Level\":5, \"Quality\":0.974000, \"ExperimentalEffect\":\"special_fsd_cooled\", \"ExperimentalEffect_Localised\":\"Thermal Spread\", \"Modifiers\":[ { \"Label\":\"Mass\", \"Value\":26.000000, \"OriginalValue\":20.000000, \"LessIsGood\":1 }, { \"Label\":\"Integrity\", \"Value\":102.000000, \"OriginalValue\":120.000000, \"LessIsGood\":0 }, { \"Label\":\"PowerDraw\", \"Value\":0.690000, \"OriginalValue\":0.600000, \"LessIsGood\":1 }, { \"Label\":\"FSDOptimalMass\", \"Value\":1624.770020, \"OriginalValue\":1050.000000, \"LessIsGood\":0 }, { \"Label\":\"FSDHeatRate\", \"Value\":24.299999, \"OriginalValue\":27.000000, \"LessIsGood\":1 } ] } }, { \"Slot\":\"LifeSupport\", \"Item\":\"Int_LifeSupport_Size5_Class5\", \"On\":true, \"Priority\":3, \"Health\":0.995652, \"Value\":121029 }, { \"Slot\":\"PowerDistributor\", \"Item\":\"Int_PowerDistributor_Size6_Class5\", \"On\":true, \"Priority\":1, \"Health\":0.995968, \"Value\":338880, \"Engineering\":{ \"Engineer\":\"The Dweller\", \"EngineerID\":300180, \"BlueprintID\":128673739, \"BlueprintName\":\"PowerDistributor_HighFrequency\", \"Level\":5, \"Quality\":0.992200, \"ExperimentalEffect\":\"special_powerdistributor_fast\", \"ExperimentalEffect_Localised\":\"Super Conduits\", \"Modifiers\":[ { \"Label\":\"WeaponsCapacity\", \"Value\":45.599998, \"OriginalValue\":50.000000, \"LessIsGood\":0 }, { \"Label\":\"WeaponsRecharge\", \"Value\":7.837814, \"OriginalValue\":5.200000, \"LessIsGood\":0 }, { \"Label\":\"EnginesCapacity\", \"Value\":31.920000, \"OriginalValue\":35.000000, \"LessIsGood\":0 }, { \"Label\":\"EnginesRecharge\", \"Value\":4.823270, \"OriginalValue\":3.200000, \"LessIsGood\":0 }, { \"Label\":\"SystemsCapacity\", \"Value\":31.920000, \"OriginalValue\":35.000000, \"LessIsGood\":0 }, { \"Label\":\"SystemsRecharge\", \"Value\":4.823270, \"OriginalValue\":3.200000, \"LessIsGood\":0 } ] } }, { \"Slot\":\"Radar\", \"Item\":\"Int_Sensors_Size4_Class5\", \"On\":true, \"Priority\":1, \"Health\":0.982955, \"Value\":43225 }, { \"Slot\":\"FuelTank\", \"Item\":\"Int_FuelTank_Size4_Class3\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":2411 }, { \"Slot\":\"Slot01_Size5\", \"Item\":\"Int_Repairer_Size5_Class5\", \"On\":true, \"Priority\":4, \"AmmoInClip\":6700, \"Health\":0.990909, \"Value\":829049 }, { \"Slot\":\"Slot02_Size5\", \"Item\":\"Int_HullReinforcement_Size5_Class2\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":43875 }, { \"Slot\":\"Slot03_Size4\", \"Item\":\"Int_FSDInterdictor_Size4_Class5\", \"On\":true, \"Priority\":4, \"Health\":1.000000, \"Value\":2080391 }, { \"Slot\":\"Slot04_Size2\", \"Item\":\"Int_ModuleReinforcement_Size2_Class2\", \"On\":true, \"Priority\":1, \"Health\":0.985714, \"Value\":3510 }, { \"Slot\":\"Slot05_Size2\", \"Item\":\"Int_BuggyBay_Size2_Class2\", \"On\":true, \"Priority\":4, \"Health\":1.000000, \"Value\":2106 }, { \"Slot\":\"Military01\", \"Item\":\"Int_HullReinforcement_Size4_Class2\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":19013 }, { \"Slot\":\"Military02\", \"Item\":\"Int_HullReinforcement_Size4_Class2\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":19013 }, { \"Slot\":\"Military03\", \"Item\":\"Int_HullReinforcement_Size4_Class2\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":19013 }, { \"Slot\":\"PlanetaryApproachSuite\", \"Item\":\"Int_PlanetApproachSuite\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":48 }, { \"Slot\":\"VesselVoice\", \"Item\":\"VoicePack_Verity\", \"On\":true, \"Priority\":1, \"Health\":1.000000 }, { \"Slot\":\"ShipCockpit\", \"Item\":\"TypeX_Cockpit\", \"On\":true, \"Priority\":1, \"Health\":1.000000 }, { \"Slot\":\"CargoHatch\", \"Item\":\"ModularCargoBayDoor\", \"On\":true, \"Priority\":4, \"Health\":0.825000 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipLoadoutEvent()
        {
            VARIABLES.Add("ship", "The ship model");
            VARIABLES.Add("shipid", "The ID of the ship");
            VARIABLES.Add("shipname", "The name of the ship");
            VARIABLES.Add("shipident", "The identification string of the ship");
            VARIABLES.Add("hullvalue", "The value of the ship's hull (less modules)");
            VARIABLES.Add("modulesvalue", "The value of the ship's modules (less hull)");
            VARIABLES.Add("value", "The total value of the ship (hull + modules)");
            VARIABLES.Add("hullhealth", "The health of the ship's hull");
            VARIABLES.Add("unladenmass", "The unladen mass of the ship");
            VARIABLES.Add("maxjumprange", "The max unlaiden jump range of the ship");
            VARIABLES.Add("optimalmass", "The optimal mass value of the frame shift drive");
            VARIABLES.Add("rebuy", "The rebuy value of the ship");
            VARIABLES.Add("hot", "True if the ship is `hot`");
            VARIABLES.Add("paintjob", "The paintjob of the ship");
            VARIABLES.Add("compartments", "The compartments (objects) of the ship");
            VARIABLES.Add("hardpoints", "The hardpoints (objects) of the ship");
        }

        [PublicAPI]
        public int? shipid { get; private set; }

        [PublicAPI]
        public string ship => shipDefinition?.model;

        [PublicAPI]
        public string shipname { get; private set; }

        [PublicAPI]
        public string shipident { get; private set; }

        [PublicAPI]
        public long? value => hullvalue + modulesvalue;

        [PublicAPI]
        public long? hullvalue { get; private set; }

        [PublicAPI]
        public long? modulesvalue { get; private set; }

        [PublicAPI]
        public decimal unladenmass { get; private set; }

        [PublicAPI]
        public decimal maxjumprange { get; private set; }

        [PublicAPI]
        public decimal optimalmass { get; private set; }

        [PublicAPI]
        public long rebuy { get; private set; }

        [PublicAPI]
        public decimal hullhealth { get; private set; }

        [PublicAPI]
        public bool hot { get; private set; }

        [PublicAPI]
        public string paintjob { get; private set; }

        [PublicAPI]
        public List<Hardpoint> hardpoints { get; private set; }

        [PublicAPI]
        public List<Compartment> compartments { get; private set; }

        // Not intended to be user facing

        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);

        public string edModel { get; private set; }

        public ShipLoadoutEvent(DateTime timestamp, string ship, int? shipId, string shipName, string shipIdent, long? hullValue, long? modulesValue, decimal hullHealth, decimal unladenmass, decimal maxjumprange, decimal optimalmass, long rebuy, bool hot, List<Compartment> compartments, List<Hardpoint> hardpoints, string paintjob) : base(timestamp, NAME)
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
            this.optimalmass = optimalmass;
            this.rebuy = rebuy;
            this.hot = hot;
            this.paintjob = paintjob;
            this.hardpoints = hardpoints;
            this.compartments = compartments;
        }
    }
}
