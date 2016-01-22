using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCompanionAppService
{
    public class Ship
    {
        private static Dictionary<string, string> shipTranslations = new Dictionary<string, string>()
        {
            { "Adder" , "Adder"},
            { "Anaconda", "Anaconda" },
            { "Asp", "Asp Explorer" },
            { "Asp_Scout", "Asp Scout" },
            { "CobraMkIII", "Cobra MkIII" },
            { "CobraMkIV", "Cobra MkIV" },
            { "Cutter", "Imperial Cutter" },
            { "Diamondback", "Diamondback Scout" },
            { "DiamondbackXL", "Diamondback Explorer" },
            { "Eagle", "Eagle" },
            { "Empire_Courier", "Imperial Courier" },
            { "Empire_Eagle", "Imperial Eagle" },
            { "Empire_Fighter", "Imperial Fighter" },
            { "Empire_Trader", "Imperial Clipper" },
            { "Federation_Corvette", "Federal Corvette" },
            { "Federation_dropship", "Federal Dropship" },
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
            { "Viper", "Viper MkIII" },
            { "Viper_MkIV", "Viper MkIV" },
            { "Vulture", "Vulture" }
        };

        private static string[] shipBulkheadsNames = new string[5] { "Lightweight Alloy", "Reinforced Alloy", "Military Grade Composite", "Mirrored Surface Composite", "Reactive Surface Composite" };

        public string Model { get; set; }
        public long Value { get; set; }
        public int CargoCapacity { get; set; }

        public string bulkheads { get; set;  }
        public decimal bulkheadsIntegrity { get; set; }
        public Module PowerPlant { get; set; }
        public string powerPlant { get; set; }
        public decimal powerPlantIntegrity { get; set; }
        public string thrusters { get; set; }
        public int thrustersPriority { get; set; }
        public bool thrustersEnabled { get; set; }
        public decimal thrustersIntegrity { get; set; }
        public string frameShiftDrive { get; set; }
        public int frameShiftDrivePriority { get; set; }
        public bool frameShiftDriveEnabled { get; set; }
        public decimal frameShiftDriveIntegrity { get; set; }
        public string lifeSupport { get; set; }
        public int lifeSupportPriority { get; set; }
        public bool lifeSupportEnabled { get; set; }
        public decimal lifeSupportIntegrity { get; set; }
        public string powerDistributor { get; set; }
        public int powerDistributorPriority { get; set; }
        public bool powerDistributorEnabled { get; set; }
        public decimal powerDistributorIntegrity { get; set; }
        public string sensors { get; set; }
        public int sensorsPriority { get; set; }
        public bool sensorsEnabled { get; set; }
        public decimal sensorsIntegrity { get; set; }
        public string fuelTank { get; set; }
        public List<Hardpoint> hardpoints { get; set; }
        public List<Compartment> compartments { get; set; }

        public Ship()
        {
            hardpoints = new List<Hardpoint>();
            compartments = new List<Compartment>();
        }

        public static Ship FromProfile(dynamic json)
        {
            Ship Ship = new Ship();
            Ship.Model = json["ship"]["name"];
            if (shipTranslations.ContainsKey(Ship.Model))
            {
                Ship.Model = shipTranslations[Ship.Model];
            }

            Ship.Value = (long)json["ship"]["value"]["hull"] + (long)json["ship"]["value"]["modules"];

            Ship.CargoCapacity = (int)json["ship"]["cargo"]["capacity"];

            return Ship;
        }
    }
}
