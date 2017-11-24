using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ShipRepairedEvent : Event
    {
        public const string NAME = "Ship repaired";
        public const string DESCRIPTION = "Triggered when you repair your ship";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:31:38Z\", \"event\":\"Repair\", \"Item\":\"Wear\", \"Cost\":2824 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        static ShipRepairedEvent()
        {
            VARIABLES.Add("item", "The item repaired, if repairing a specific item");
            VARIABLES.Add("price", "The price of the repair");
            VARIABLES.Add("name", "The localized name of the item repaired");
            VARIABLES.Add("@class", "The class of the item repaired");
            VARIABLES.Add("grade", "The grade of the item repaired");
            VARIABLES.Add("mount", "The localized type of mount of the item repaired if it is a weapon");
        }

        public string item { get; private set; }
        public long price { get; private set; }

        public string name { get; private set; } = null;
        public string @class { get; private set; } = null;
        public string grade { get; private set; } = null;
        public string mount { get; private set; } = null;

        public ShipRepairedEvent(DateTime timestamp, string item, string mount, Module module, long price) : base(timestamp, NAME)
        {
            this.item = item;
            this.price = price;
            if (module != null)
            {
                this.name = module.LocalName;
                this.@class = module.@class.ToString();
                this.grade = module.grade;
                if (module.mount != null) { this.mount = I18N.GetString(mount); }
            }
        }
    }
}
