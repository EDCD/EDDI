using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiMaterialMonitor
{
    class MaterialThresholdEvent : Event
    {
        public const string NAME = "Material threshold";
        public const string DESCRIPTION = "Triggered when a material reaches a threshold";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        public static MaterialThresholdEvent SAMPLE = new MaterialThresholdEvent(DateTime.UtcNow, Material.FromEDName("carbon"), "Minimum", 6, 5, "Reduction");

        static MaterialThresholdEvent()
        {
            VARIABLES.Add("material", "The material");
            VARIABLES.Add("level", "The level that has been triggered (Minimum/Desired/Maximum)");
            VARIABLES.Add("limit", "The amount of the limit that has been passed");
            VARIABLES.Add("amount", "The current amount of the material");
            VARIABLES.Add("change", "The change to the inventory (Increase/Reduction)");
        }

        [PublicAPI]
        public Material material { get; private set; }

        [PublicAPI]
        public string level { get; private set; }

        [PublicAPI]
        public int limit { get; private set; }

        [PublicAPI]
        public int amount { get; private set; }

        [PublicAPI]
        public string change { get; private set; }

        public MaterialThresholdEvent(DateTime timestamp, Material material, string level, int limit, int amount, string change) : base(timestamp, NAME)
        {
            this.material = material;
            this.level = level;
            this.limit = limit;
            this.amount = amount;
            this.change = change;
        }
    }
}
