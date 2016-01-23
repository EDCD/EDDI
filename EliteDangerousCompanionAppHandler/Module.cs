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

        // Admin
        public long EDDBID { get; set; }

        public Module(Module Module)
        {
            this.Name = Module.Name;
            this.Class = Module.Class;
            this.Grade = Module.Grade;
            this.Cost = Module.Cost;
            this.EDDBID = Module.EDDBID;
        }

        public Module(long EDDBID, string Name, int Class, string Grade, long Cost)
        {
            this.EDDBID = EDDBID;
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
            // Be sensible with health - round it unless it's very low
            decimal Health = (decimal)json["module"]["health"] / 10000;
            if (Health < 5)
            {
                Module.Health = Math.Round(Health, 1);
            }
            else
            {
                Module.Health = Math.Round(Health);
            }
            return Module;
        }

    }
}
