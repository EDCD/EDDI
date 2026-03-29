using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MaterialThresholdEvent (
        DateTime timestamp,
        Material material,
        string level,
        int limit,
        int amount,
        string change )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Material threshold";
        public const string DESCRIPTION = "Triggered when a material reaches a threshold";
        public static readonly MaterialThresholdEvent SAMPLE = new(DateTime.UtcNow, Material.Carbon, "Minimum", 6, 5, "Reduction");

        [PublicAPI("The material (as an object)")]
        public Material material { get; private set; } = material;

        [PublicAPI("The level that has been triggered (Minimum/Desired/Maximum)")]
        public string level { get; private set; } = level;

        [PublicAPI("The amount of the limit that has been passed")]
        public int limit { get; private set; } = limit;

        [PublicAPI("The current amount of the material")]
        public int amount { get; private set; } = amount;

        [PublicAPI("The change to the inventory (Increase/Reduction)")]
        public string change { get; private set; } = change;
    }
}
