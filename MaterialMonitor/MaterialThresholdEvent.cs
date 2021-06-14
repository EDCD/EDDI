using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiMaterialMonitor
{
    [PublicAPI]
    public class MaterialThresholdEvent : Event
    {
        public const string NAME = "Material threshold";
        public const string DESCRIPTION = "Triggered when a material reaches a threshold";
        public static MaterialThresholdEvent SAMPLE = new MaterialThresholdEvent(DateTime.UtcNow, Material.FromEDName("carbon"), "Minimum", 6, 5, "Reduction");

        [PublicAPI("The material (as an object)")]
        public Material material { get; private set; }

        [PublicAPI("The level that has been triggered (Minimum/Desired/Maximum)")]
        public string level { get; private set; }

        [PublicAPI("The amount of the limit that has been passed")]
        public int limit { get; private set; }

        [PublicAPI("The current amount of the material")]
        public int amount { get; private set; }

        [PublicAPI("The change to the inventory (Increase/Reduction)")]
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
