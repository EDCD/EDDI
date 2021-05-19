using System.Numerics;

namespace EddiDataDefinitions
{
    public class MicroResourceAmount
    {
        public string name => microResource.localizedName;
        public string category => microResource.category;
        public string edname => microResource.edname;
        public int amount { get; }

        // Not intended to be user facing
        public MicroResource microResource { get; }
        public int? ownerId { get; }
        public decimal? missionId { get; }

        public MicroResourceAmount(MicroResource microResource, int? ownerId, decimal? missionId, int amount)
        {
            this.microResource = microResource;
            this.ownerId = ownerId;
            this.missionId = missionId;
            this.amount = amount;
        }
    }
}
