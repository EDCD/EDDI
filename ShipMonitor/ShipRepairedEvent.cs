using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipRepairedEvent : Event
    {
        public const string NAME = "Ship repaired";
        public const string DESCRIPTION = "Triggered when you repair your ship";
        public const string SAMPLE = "{ \"timestamp\":\"2020-06-11T19:29:59Z\", \"event\":\"Repair\", \"Items\":[ \"$krait_light_cockpit_name;\", \"Hull\", \"$modularcargobaydoor_name;\", \"$Hpt_BasicMissileRack_Fixed_Small_name;\" ,\"Wear\" ], \"Cost\":50830 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        static ShipRepairedEvent()
        {
            VARIABLES.Add("items", "The non-module items repaired (e.g. 'All', 'Ship Integrity', 'Hull', 'Paint')");
            VARIABLES.Add("modules", "Module objects representing module items repaired");
            VARIABLES.Add("price", "The price of the repair");
        }


        public List<string> items { get; private set; } = new List<string>();

        public List<Module> modules { get; private set; } = new List<Module>();

        public long price { get; private set; }

        // Legacy variables
        [Obsolete(@"Please use ""items"" instead")]
        public string item => items.Count > 0 ? items[0] : null;
        [Obsolete(@"Please use ""modules"" instead")]
        public Module module => modules.Count > 0 ? modules[0] : null;

        // Not intended to be user facing
        
        public List<string> itemEDNames { get; private set; } = new List<string>();

        public ShipRepairedEvent(DateTime timestamp, string itemEDName, long price) : base(timestamp, NAME)
        {
            itemEDNames.Add(itemEDName);
            this.price = price;
            handleItems();
        }

        public ShipRepairedEvent(DateTime timestamp, List<string> itemEDNames, long price) : base(timestamp, NAME)
        {
            this.itemEDNames = itemEDNames;
            this.price = price;
            handleItems();
        }

        private void handleItems()
        {
            // Each item might be "All", "Wear", "Hull", "Paint", or the name of a module
            foreach (string itemEDName in itemEDNames)
            {
                if (itemEDName == "Wear")
                {
                    items.Add(EddiDataDefinitions.Properties.Modules.ShipIntegrity);
                }
                else if (itemEDName == "All" || itemEDName == "Paint" || itemEDName == "Hull")
                {
                    items.Add(itemEDName);
                }
                else
                {
                    // Might be a module
                    var currentModule = Module.FromEDName(itemEDName);
                    if (currentModule != null)
                    {
                        modules.Add(currentModule);
                    }
                    else
                    {
                        items.Add(itemEDName);
                    }
                }
            }
        }
    }
}
