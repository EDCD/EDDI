using System;
using System.Text.RegularExpressions;

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
                Module.Class = 8;
                Module.Grade = "Z"; // TODO anything more useful we could put here?
                string moduleName = json["module"]["name"];
                if (moduleName.EndsWith("Grade1"))
                {
                    Module.Name = "Lightweight Alloy";
                }
                else if (moduleName.EndsWith("Grade2"))
                {
                    Module.Name = "Reinforced Alloy";
                }
                else if (moduleName.EndsWith("Grade3"))
                {
                    Module.Name = "Military Grade Composite";
                }
                else if (moduleName.EndsWith("Mirrored"))
                {
                    Module.Name = "Mirrored Surface Composite";
                }
                else if (moduleName.EndsWith("Reactive"))
                {
                    Module.Name = "Reactive Surface Composite";
                }
            }
            else if (name == "PowerPlant" || name == "MainEngines" || name == "FrameShiftDrive" || name == "LifeSupport" || name == "PowerDistributor" || name == "Radar" || name == "FuelTank")
            {
                // Internal
                Match matches = Regex.Match((string)json["module"]["name"], @"Size([0-9]+).*Class([0-9]+)");
                if (matches.Success)
                {
                    Module.Class = Int32.Parse(matches.Groups[1].Value);
                    Module.Grade = ((char)(70 - Int32.Parse(matches.Groups[2].Value))).ToString();
                }
            }
            else if (name.Contains("Hardpoint"))
            {
                // Hardpoint
                // Could have either a SizeX_ClassY or a _Small/Medium/Large/Huge designator
            }
            else if (name.Contains("Slot"))
            {
                // Compartment
            }
            else
            {
                throw new Exception("Unknown module " + name);
            }
            Module.Enabled = (bool)json["module"]["on"];
            Module.Priority = (int)json["module"]["priority"];
            Module.Integrity = (decimal)json["module"]["health"] / 10000;
            return Module;
        }
    }
}
