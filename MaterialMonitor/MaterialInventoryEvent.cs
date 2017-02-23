using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiMaterialMonitor
{
    class MaterialInventoryEvent : Event
    {
        public const string NAME = "Material inventory";
        public const string DESCRIPTION = "Triggered when a limit material inventory has been passed";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        public static MaterialInventoryEvent SAMPLE = new MaterialInventoryEvent(DateTime.Now, Material.AnomalousBulkScanData, "Minimum", 6, 5, "Reduction");

        static MaterialInventoryEvent()
        {
            VARIABLES.Add("material", "The material");
            VARIABLES.Add("level", "The level that has been triggered (Minimum/Desired/Maximum)");
            VARIABLES.Add("limit", "The amount of the limit that has been passed");
            VARIABLES.Add("amount", "The current amount of the material");
            VARIABLES.Add("change", "The change to the inventory (Increase/Reduction)");
        }

        Material material;
        string level;
        int limit;
        int amount;
        string change;

        public MaterialInventoryEvent(DateTime timestamp, Material material, string level, int limit, int amount, string change) : base(timestamp, NAME)
        {
            this.material = material;
            this.level = level;
            this.limit = limit;
            this.amount = amount;
            this.change = change;
        }
    }
}
