using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MaterialCollectedEvent : Event
    {
        public const string NAME = "Material collected";
        public const string DESCRIPTION = "Triggered when you collect a material";
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-05T11:32:57Z\", \"event\":\"MaterialCollected\", \"Category\":\"Encoded\", \"Name\":\"shieldpatternanalysis\", \"Count\":3 }";

        [PublicAPI("The name of the collected material")]
        public string name { get; private set; }

        [PublicAPI("The amount of the collected material")]
        public int amount { get; private set; }

        // Not intended to be user facing

        public string edname { get; private set; }

        public MaterialCollectedEvent(DateTime timestamp, Material material, int amount) : base(timestamp, NAME)
        {
            this.name = material?.localizedName;
            this.amount = amount;
            this.edname = material?.edname;
        }
    }
}
