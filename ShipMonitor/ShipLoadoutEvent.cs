using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipLoadoutEvent : Event
    {
        public const string NAME = "Ship loadout";
        public const string DESCRIPTION = "Triggered when you obtain the loadout of your ship";
        public const string SAMPLE = "{ \"timestamp\":\"2017-03-31T11:49:31Z\", \"event\":\"Loadout\", \"Ship\":\"Federation_Corvette\", \"ShipID\":67, \"ShipName\":\"\", \"ShipIdent\":\"\", \"Modules\":[ { \"Slot\":\"HugeHardpoint1\", \"Item\":\"Hpt_MultiCannon_Gimbal_Huge\", \"On\":true, \"Priority\":0, \"AmmoInClip\":90, \"AmmoInHopper\":2100, \"Health\":1.000000, \"Value\":621816 }, { \"Slot\":\"HugeHardpoint2\", \"Item\":\"Hpt_MultiCannon_Fixed_Huge\", \"On\":true, \"Priority\":0, \"AmmoInClip\":100, \"AmmoInHopper\":2100, \"Health\":1.000000, \"Value\":114816 }, { \"Slot\":\"LargeHardpoint1\", \"Item\":\"Hpt_MultiCannon_Fixed_Large\", \"On\":true, \"Priority\":0, \"AmmoInClip\":100, \"AmmoInHopper\":2100, \"Health\":1.000000, \"Value\":13689 }, { \"Slot\":\"MediumHardpoint1\", \"Item\":\"Hpt_MultiCannon_Gimbal_Medium\", \"On\":true, \"Priority\":0, \"AmmoInClip\":90, \"AmmoInHopper\":2100, \"Health\":1.000000, \"Value\":5558 }, { \"Slot\":\"MediumHardpoint2\", \"Item\":\"Hpt_MultiCannon_Fixed_Medium\", \"On\":true, \"Priority\":0, \"AmmoInClip\":100, \"AmmoInHopper\":2100, \"Health\":1.000000, \"Value\":3705 }, { \"Slot\":\"SmallHardpoint1\", \"Item\":\"Hpt_MultiCannon_Gimbal_Small\", \"On\":true, \"Priority\":0, \"AmmoInClip\":90, \"AmmoInHopper\":2100, \"Health\":1.000000, \"Value\":1390 }, { \"Slot\":\"SmallHardpoint2\", \"Item\":\"Hpt_MultiCannon_Fixed_Small\", \"On\":true, \"Priority\":0, \"AmmoInClip\":100, \"AmmoInHopper\":2100, \"Health\":1.000000, \"Value\":927 }, { \"Slot\":\"Armour\", \"Item\":\"Federation_Corvette_Armour_Grade1\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":0 }, { \"Slot\":\"PowerPlant\", \"Item\":\"Int_PowerPlant_Size8_Class1\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":195705 }, { \"Slot\":\"MainEngines\", \"Item\":\"Int_Engine_Size7_Class1\", \"On\":true, \"Priority\":2, \"Health\":1.000000, \"Value\":61737 }, { \"Slot\":\"FrameShiftDrive\", \"Item\":\"Int_Hyperdrive_Size6_Class1\", \"On\":true, \"Priority\":2, \"Health\":1.000000, \"Value\":19475 }, { \"Slot\":\"LifeSupport\", \"Item\":\"Int_LifeSupport_Size5_Class1\", \"On\":true, \"Priority\":2, \"Health\":1.000000, \"Value\":3098 }, { \"Slot\":\"PowerDistributor\", \"Item\":\"Int_PowerDistributor_Size8_Class1\", \"On\":true, \"Priority\":2, \"Health\":1.000000, \"Value\":68014 }, { \"Slot\":\"Radar\", \"Item\":\"Int_Sensors_Size8_Class1\", \"On\":true, \"Priority\":2, \"Health\":1.000000, \"Value\":68014 }, { \"Slot\":\"FuelTank\", \"Item\":\"Int_FuelTank_Size5_Class3\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":9530 }, { \"Slot\":\"Slot01_Size7\", \"Item\":\"Int_ShieldGenerator_Size7_Class1\", \"On\":true, \"Priority\":2, \"Health\":1.000000, \"Value\":61737 }, { \"Slot\":\"Slot02_Size7\", \"Item\":\"Int_CargoRack_Size6_Class1\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":35352 }, { \"Slot\":\"Slot08_Size4\", \"Item\":\"Int_CargoRack_Size3_Class1\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":1029 }, { \"Slot\":\"Slot09_Size4\", \"Item\":\"Int_StellarBodyDiscoveryScanner_Standard\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":97 }, { \"Slot\":\"Slot10_Size3\", \"Item\":\"Int_CargoRack_Size2_Class1\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":316 }, { \"Slot\":\"PlanetaryApproachSuite\", \"Item\":\"Int_PlanetApproachSuite\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":48 }, { \"Slot\":\"ShipCockpit\", \"Item\":\"Federation_Corvette_Cockpit\", \"On\":true, \"Priority\":1, \"Health\":1.000000, \"Value\":0 }, { \"Slot\":\"CargoHatch\", \"Item\":\"ModularCargoBayDoor\", \"On\":true, \"Priority\":2, \"Health\":1.000000, \"Value\":0 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipLoadoutEvent()
        {
            VARIABLES.Add("ship", "The ship");
            VARIABLES.Add("shipid", "The ID of the ship");
            VARIABLES.Add("shipname", "The name of the ship");
            VARIABLES.Add("shipident", "The identification string of the ship");
            VARIABLES.Add("paintjob", "The paintjob of the ship");
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
            this.shipname = shipName;
            this.shipident = shipIdent;
            this.paintjob = paintjob;
            this.hardpoints = hardpoints;
            this.compartments = compartments;
        }
    }
}
