using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MaterialDonatedEvent ( DateTime timestamp, Material material, int amount, long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Material donated";
        public const string DESCRIPTION = "Triggered when you donate a material";
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-05T11:32:57Z\", \"event\":\"ScientificResearch\", \"Name\":\"nickel\", \"Category\":\"Raw\", \"Count\":5, \"MarketID\": 128666762 }";

        [PublicAPI("The name of the donated material")]
        public string name { get; private set; } = material?.localizedName;

        [PublicAPI("The amount of the donated material")]
        public int amount { get; private set; } = amount;

        [PublicAPI( "The total amount of the donated material remaining in your inventory" )]
        public int total { get; set; }

        // Not intended to be user facing

        public string edname { get; private set; } = material?.edname;

        public long marketId { get; private set; } = marketId;
    }
}
