using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class ShipLoadoutEvent : Event
    {
        public const string NAME = "Ship loadout";
        public const string DESCRIPTION = "Triggered when you obtain the loadout of your ship";
        public const string SAMPLE = @"{ ""timestamp"":""2017-03-04T17:42:49Z"", ""event"":""Loadout"", ""Ship"":""Empire_Courier"", ""ShipID"":32, ""ShipName"":""Mosquito"", ""ShipIdent"":""JGM-01"", ""Modules"":[ { ""Slot"":""MediumHardpoint1"", ""Item"":""Hpt_DrunkMissileRack_Fixed_Medium"", ""On"":true, ""Priority"":0, ""Health"":1.000000, ""Value"":674447, ""EngineerBlueprint"":""Weapon_HighCapacity"", ""EngineerLevel"":5 }, { ""Slot"":""MediumHardpoint2"", ""Item"":""Hpt_DrunkMissileRack_Fixed_Medium"", ""On"":true, ""Priority"":0, ""Health"":1.000000, ""Value"":674447, ""EngineerBlueprint"":""Weapon_HighCapacity"", ""EngineerLevel"":5 }, { ""Slot"":""MediumHardpoint3"", ""Item"":""Hpt_PulseLaserBurst_Fixed_Medium"", ""On"":true, ""Priority"":0, ""AmmoInClip"":1, ""AmmoInHopper"":1, ""Health"":1.000000, ""Value"":22425, ""EngineerBlueprint"":""Weapon_RapidFire"", ""EngineerLevel"":3 }, { ""Slot"":""TinyHardpoint1"", ""Item"":""Hpt_HeatSinkLauncher_Turret_Tiny"", ""On"":true, ""Priority"":0, ""AmmoInClip"":1, ""AmmoInHopper"":2, ""Health"":1.000000, ""Value"":308 }, { ""Slot"":""TinyHardpoint2"", ""Item"":""Hpt_HeatSinkLauncher_Turret_Tiny"", ""On"":true, ""Priority"":0, ""AmmoInClip"":1, ""AmmoInHopper"":2, ""Health"":1.000000, ""Value"":308 }, { ""Slot"":""TinyHardpoint3"", ""Item"":""Hpt_PlasmaPointDefence_Turret_Tiny"", ""On"":true, ""Priority"":0, ""AmmoInClip"":12, ""AmmoInHopper"":9736, ""Health"":1.000000, ""Value"":1629 }, { ""Slot"":""TinyHardpoint4"", ""Item"":""Hpt_PlasmaPointDefence_Turret_Tiny"", ""On"":true, ""Priority"":0, ""AmmoInClip"":8, ""AmmoInHopper"":9748, ""Health"":1.000000, ""Value"":1629 }, { ""Slot"":""PaintJob"", ""Item"":""PaintJob_Empire_Courier_BlackFriday_01"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""Armour"", ""Item"":""Empire_Courier_Armour_Grade1"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0, ""EngineerBlueprint"":""Armour_HeavyDuty"", ""EngineerLevel"":5 }, { ""Slot"":""PowerPlant"", ""Item"":""Int_Powerplant_Size4_Class5"", ""On"":true, ""Priority"":1, ""Health"":0.960674, ""Value"":141286 }, { ""Slot"":""MainEngines"", ""Item"":""Int_Engine_Size3_Class5_Fast"", ""On"":true, ""Priority"":0, ""Health"":0.992647, ""Value"":4229901, ""EngineerBlueprint"":""Engine_Dirty"", ""EngineerLevel"":5 }, { ""Slot"":""FrameShiftDrive"", ""Item"":""Int_Hyperdrive_Size3_Class5"", ""On"":true, ""Priority"":0, ""Health"":1.000000, ""Value"":44570 }, { ""Slot"":""LifeSupport"", ""Item"":""Int_LifeSupport_Size1_Class2"", ""On"":true, ""Priority"":0, ""Health"":0.936717, ""Value"":1135, ""EngineerBlueprint"":""LifeSupport_LightWeight"", ""EngineerLevel"":1 }, { ""Slot"":""PowerDistributor"", ""Item"":""Int_PowerDistributor_Size3_Class5"", ""On"":true, ""Priority"":0, ""Health"":0.951020, ""Value"":154373, ""EngineerBlueprint"":""PowerDistributor_HighFrequency"", ""EngineerLevel"":2 }, { ""Slot"":""Radar"", ""Item"":""Int_Sensors_Size2_Class2"", ""On"":true, ""Priority"":0, ""Health"":0.849514, ""Value"":3529 }, { ""Slot"":""FuelTank"", ""Item"":""Int_FuelTank_Size2_Class3"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":330 }, { ""Slot"":""Slot01_Size3"", ""Item"":""Int_ShieldGenerator_Size2_Class3_Fast"", ""On"":true, ""Priority"":0, ""Health"":0.934162, ""Value"":2344 }, { ""Slot"":""Slot02_Size3"", ""Item"":""Int_HullReinforcement_Size1_Class2"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":13163, ""EngineerBlueprint"":""HullReinforcement_HeavyDuty"", ""EngineerLevel"":5 }, { ""Slot"":""Slot03_Size2"", ""Item"":""Int_HullReinforcement_Size1_Class2"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":13163, ""EngineerBlueprint"":""HullReinforcement_HeavyDuty"", ""EngineerLevel"":5 }, { ""Slot"":""Slot04_Size2"", ""Item"":""Int_HullReinforcement_Size1_Class2"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":13163, ""EngineerBlueprint"":""HullReinforcement_HeavyDuty"", ""EngineerLevel"":5 }, { ""Slot"":""Slot05_Size2"", ""Item"":""Int_ModuleReinforcement_Size2_Class2"", ""On"":true, ""Priority"":1, ""Health"":0.000000, ""Value"":3159 }, { ""Slot"":""Slot06_Size1"", ""Item"":""Int_ModuleReinforcement_Size1_Class2"", ""On"":true, ""Priority"":1, ""Health"":0.325801, ""Value"":1317 }, { ""Slot"":""PlanetaryApproachSuite"", ""Item"":""Int_PlanetApproachSuite"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":438 }, { ""Slot"":""Bobble01"", ""Item"":""Bobble_PilotFemale"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""Bobble10"", ""Item"":""Bobble_PilotMale"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""WeaponColour"", ""Item"":""WeaponCustomisation_Purple"", ""On"":true, ""Priority"":1, ""Health"":1.000000, ""Value"":0 }, { ""Slot"":""ShipCockpit"", ""Item"":""Empire_Courier_Cockpit"", ""On"":true, ""Priority"":1, ""Health"":0.884571, ""Value"":0 }, { ""Slot"":""CargoHatch"", ""Item"":""ModularCargoBayDoor"", ""On"":true, ""Priority"":2, ""Health"":1.000000, ""Value"":0 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipLoadoutEvent()
        {
            VARIABLES.Add("ship", "The ship");
            VARIABLES.Add("shipid", "The ID of the ship");
            VARIABLES.Add("shipname", "The name of the ship");
            VARIABLES.Add("shipident", "The identification string of the ship");
            VARIABLES.Add("paintjob", "The paintjob on the ship");
            VARIABLES.Add("compartments", "The compartments of the ship");
            VARIABLES.Add("hardpoints", "The hardpoints of the ship");
        }

        public int? shipid { get; private set; }
        public string ship { get; private set; }
        public string shipname { get; private set; }
        public string shipident { get; private set; }
        public string paintjob { get; private set; }
        public List<Hardpoint> hardpoints { get; private set;  }
        public List<Compartment> compartments { get; private set; }

        public ShipLoadoutEvent(DateTime timestamp, string ship, int? shipId, string shipName, string shipIdent, List<Compartment> compartments, List<Hardpoint> hardpoints, string paintjob) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.shipid = shipId;
            this.shipident = shipIdent;
            this.shipident = shipIdent;
            this.paintjob = paintjob;
            this.hardpoints = hardpoints;
            this.compartments = compartments;
        }
    }
}
