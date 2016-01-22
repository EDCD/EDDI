using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCompanionAppService
{
    public class Module
    {
        // Definition of the module
        public string Name { get; set; }
        public int Class { get; set; }
        public string Grade { get; set; }

        // State of the module
        public bool Enabled { get; set; }
        public int Priority { get; set; }
        public decimal Integrity { get; set; }

        public static Module FromProfile(string name, dynamic json)
        {
            Module Module = new Module();

            // Start by working out what we're dealing with.

            if (name == "Armour")
            {
                // Armour
            }
            else if (name == "PowerPlant" || name == "MainEngines" || name == "FrameShiftDrive" || name == "LifeSupport" || name == "PowerDistributor" || name == "Radar" || name == "FuelTank")
            {
                // Internal
            }
            else if (name.Contains("Hardpoint"))
            {
                // Hardpoint
            }
            else if (name.Contains("Slot"))
            {
                // Compartment
            }
            else
            {
                throw new Exception("Unknown module " + name);
            }
            return Module;
        }
    }
}
