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
        public long Cost { get; set; } // The undiscounted cost

        // State of the module
        public long Value { get; set; } // How much we actually paid for it
        public bool Enabled { get; set; }
        public int Priority { get; set; }
        public decimal Health { get; set; }

        public Module(Module Module)
        {
            this.Name = Module.Name;
            this.Class = Module.Class;
            this.Grade = Module.Grade;
            this.Cost = Module.Cost;
        }
        public Module(string Name, int Class, string Grade, long Cost)
        {
            this.Name = Name;
            this.Class = Class;
            this.Grade = Grade;
            this.Cost = Cost;
        }

        public static Module FromProfile(string name, dynamic json)
        {
            long id = (long)json["module"]["id"];
            Module ModuleTemplate = ModuleDefinitions.FromID(id);
            Module Module = new Module(ModuleTemplate);

            Module.Value= (long)json["module"]["value"];
            Module.Enabled = (bool)json["module"]["on"];
            Module.Priority = (int)json["module"]["priority"];
            Module.Health = (decimal)json["module"]["health"] / 10000;
            return Module;
        }

    }
}
