using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MaterialDiscoveredEvent : Event
    {
        public const string NAME = "Material discovered";
        public const string DESCRIPTION = "Triggered when you discover a material";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-21T14:07:19Z\", \"event\":\"MaterialDiscovered\", \"Category\":\"Raw\", \"Name\":\"iron\", \"DiscoveryNumber\":3 }";

        [PublicAPI("The name of the discovered material")]
        public string name { get; private set; }

        public MaterialDiscoveredEvent(DateTime timestamp, Material material) : base(timestamp, NAME)
        {
            this.name = material?.localizedName;
        }
    }
}
