using Utilities;

namespace EddiDataDefinitions
{
    public class MicroResourceAmount
    {
        [PublicAPI]
        public string name => microResource.localizedName;

        [PublicAPI]
        public string category => microResource.category;

        public string edname => microResource.edname;

        [PublicAPI]
        public int amount { get; }

        [PublicAPI]
        public int? price { get; }

        // Not intended to be user facing
        public MicroResource microResource { get; }
        public int? ownerId { get; }
        public decimal? missionId { get; }

        public MicroResourceAmount(MicroResource microResource, int? ownerId, decimal? missionId, int amount, int? price = null)
        {
            this.microResource = microResource;
            this.ownerId = ownerId;
            this.missionId = missionId;
            this.amount = amount;
            this.price = price;
        }
    }
}
