using EddiEvents;
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
        }

        public string item { get; private set; }
        public bool repairedfully { get; private set; }
        public decimal health { get; private set; }

        public ShipAfmuRepairedEvent(DateTime timestamp, string item, bool repairedfully, decimal health) : base(timestamp, NAME)
        {
            this.item = item;
            this.repairedfully = repairedfully;
            this.health = health;
        }
    }
}
