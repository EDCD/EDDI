using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using System.Linq;

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
            VARIABLES.Add("item", "Either 'All', 'Ship Integrity', 'Hull', 'Paint', the name of the item repaired (if repairing one item), or nothing (if repairing multiple items)");
            VARIABLES.Add("items", "The items repaired (if repairing multiple items)");
            VARIABLES.Add("module", "a module object representing the item repaired");
            VARIABLES.Add("modules", "module objects representing the items repaired (if repairing multiple items)");
            VARIABLES.Add("price", "The price of the repair");
        }

        public string item { get; private set; }

        public List<string> items { get; private set; } = new List<string>();

        public Module module { get; private set; }

        public List<Module> modules { get; private set; } = new List<Module>();

        public long price { get; private set; }

        public ShipRepairedEvent(DateTime timestamp, string item, Module module, long price) : base(timestamp, NAME)
        {
            this.item = localizedModule(module) ?? item;
            this.module = module;
            this.price = price;
        }

        public ShipRepairedEvent(DateTime timestamp, List<string> items, List<Module> modules, long price) : base(timestamp, NAME)
        {
            this.items = modules.Select(localizedModule).ToList();
            this.modules = modules;
            this.price = price;
        }

        private static string localizedModule(Module moduleToLocalize)
        {
            if (moduleToLocalize == null) return null;

            if (moduleToLocalize.mount != null)
            {
                // This is a weapon so provide a bit more information
                string mount = "";
                switch (moduleToLocalize.mount)
                {
                    // FIXME this breaks localisation
                    case Module.ModuleMount.Fixed:
                        mount = "fixed";
                        break;
                    case Module.ModuleMount.Gimballed:
                        mount = "gimballed";
                        break;
                    case Module.ModuleMount.Turreted:
                        mount = "turreted";
                        break;
                }
                return $"{moduleToLocalize.@class}{moduleToLocalize.grade} {mount} {moduleToLocalize.localizedName}";
            }
            else
            {
                return moduleToLocalize.localizedName;
            }
        }
    }
}
