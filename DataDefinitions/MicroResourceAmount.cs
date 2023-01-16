using Newtonsoft.Json;
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
        public long? ownerId { get; }
        public long? missionId { get; }

        public MicroResourceAmount([JsonProperty] string Name, [JsonProperty] long? OwnerID, [JsonProperty] int Count, [JsonProperty] string Type = null, [JsonProperty] string Name_Localised = null, [JsonProperty] long? MissionID = null)
        {
            this.microResource = MicroResource.FromEDName(Name, Name_Localised, Type);
            this.ownerId = OwnerID;
            this.amount = Count;
            this.missionId = MissionID;
        }
    }
}
