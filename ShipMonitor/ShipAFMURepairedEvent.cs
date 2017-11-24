using EddiEvents;
using EddiDataDefinitions;
using Utilities;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipAfmuRepairedEvent : Event
    {
        public const string NAME = "AFMU repairs";
        public const string DESCRIPTION = "Triggered when repairing modules using the Auto Field Maintenance Unit (AFMU)";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-14T15:41:50Z\", \"event\":\"AfmuRepairs\", \"Module\":\"$modularcargobaydoor_name;\", \"Module_Localised\":\"Cargo Hatch\", \"FullyRepaired\":true, \"Health\":1.000000 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        static ShipAfmuRepairedEvent()
        {
            VARIABLES.Add("item", "The module that was repaired");
            VARIABLES.Add("repairedfully", "Whether the module was fully repaired (true/false)");
            VARIABLES.Add("health", "The health of the module (1.000000 = fully repaired)");
            VARIABLES.Add("name", "The localized name of the item repaired");
            VARIABLES.Add("@class", "The class of the item repaired");
            VARIABLES.Add("grade", "The grade of the item repaired");
            VARIABLES.Add("mount", "The localized type of mount of the item repaired if it is a weapon");
        }

        public string item { get; private set; }
        public bool repairedfully { get; private set; }
        public decimal health { get; private set; }
        public string name { get; private set; }
        public string @class { get; private set; }
        public string grade { get; private set; }
        public string mount { get; private set; } = null;

        public ShipAfmuRepairedEvent(DateTime timestamp, string item, string mount, Module module, bool repairedfully, decimal health) : base(timestamp, NAME)
        {
            this.item = item;
            this.repairedfully = repairedfully;
            this.health = health;
            this.name = module.LocalName;
            this.@class = module.@class.ToString();
            this.grade = module.grade;
            if (module.mount != null) { this.mount = I18N.GetString(mount); }
        }
    }
}
