using System.Collections.Generic;

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

        public string Model { get; set; }
        public long Value { get; set; }
        public int CargoCapacity { get; set; }

        public string bulkheads { get; set;  }
        public decimal bulkheadsIntegrity { get; set; }
        public Module Bulkheads { get; set; }
        public Module PowerPlant { get; set; }
        public Module Thrusters { get; set; }
        public Module FrameShiftDrive { get; set; }
        public Module LifeSupport { get; set; }
        public Module PowerDistributor { get; set; }
        public Module Sensors { get; set; }
        public Module FuelTank { get; set; }
        public List<Hardpoint> Hardpoints { get; set; }
        public List<Compartment> Compartments { get; set; }

        public Ship()
        {
            Hardpoints = new List<Hardpoint>();
            Compartments = new List<Compartment>();
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

            // Obtain the internals
            Ship.Bulkheads = Module.FromProfile("Armour", json["ship"]["modules"]["Armour"]);
            Ship.PowerPlant = Module.FromProfile("PowerPlant", json["ship"]["modules"]["PowerPlant"]);
            Ship.Thrusters = Module.FromProfile("MainEngines", json["ship"]["modules"]["MainEngines"]);
            Ship.FrameShiftDrive = Module.FromProfile("FrameShiftDrive", json["ship"]["modules"]["FrameShiftDrive"]);
            Ship.LifeSupport = Module.FromProfile("LifeSupport", json["ship"]["modules"]["LifeSupport"]);
            Ship.PowerDistributor = Module.FromProfile("PowerDistributor", json["ship"]["modules"]["PowerDistributor"]);
            Ship.Sensors = Module.FromProfile("Radar", json["ship"]["modules"]["Radar"]);
            Ship.FuelTank = Module.FromProfile("FuelTank", json["ship"]["modules"]["FuelTank"]);

            // Obtain the hardpoints
            foreach (dynamic module in json["ship"]["modules"])
            {
                if (module.Name.Contains("Hardpoint"))
                {
                    Ship.Hardpoints.Add(Hardpoint.FromProfile(module));
                }
            }

            // Obtain the compartments

            return Ship;
        }
    }
}
