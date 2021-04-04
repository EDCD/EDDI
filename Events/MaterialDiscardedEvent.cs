using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class MaterialDiscardedEvent : Event
    {
        public const string NAME = "Material discarded";
        public const string DESCRIPTION = "Triggered when you discard a material";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"MaterialDiscarded\",\"Category\":\"Encoded\",\"Name\":\"shieldcyclerecordings\", \"Count\":3}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MaterialDiscardedEvent()
        {
            VARIABLES.Add("name", "The name of the discarded material");
            VARIABLES.Add("amount", "The amount of the discarded material");
        }

        [PublicAPI]
        public string name { get; private set; }

        [PublicAPI]
        public int amount { get; private set; }

        // Not intended to be user facing

        public string edname { get; private set; }

        public MaterialDiscardedEvent(DateTime timestamp, Material material, int amount) : base(timestamp, NAME)
        {
            this.name = material?.localizedName;
            this.amount = amount;
            this.edname = material?.edname;
        }
    }
}
