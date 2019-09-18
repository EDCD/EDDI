using System.Collections.Generic;
using Newtonsoft.Json;

namespace EddiInaraService
{
    public class InaraCmdr
    {
        [JsonProperty("userID")]
        public int id { get; set; }

        [JsonProperty("userName")]
        public string username { get; set; }

        [JsonProperty("commanderName")]
        public string commandername { get; set; }

        [JsonProperty("commanderRanksPilot")]
        public List<InaraCmdrRanks> commanderranks { get; set; }

        [JsonProperty("preferredAllegianceName")]
        public string preferredallegiance { get; set; }

        [JsonProperty("preferredPowerName")]
        public string preferredpower { get; set; }

        [JsonProperty("commanderSquadron")]
        public InaraCmdrSquadron squadron { get; set; }

        [JsonProperty("preferredGameRole")]
        public string preferredrole { get; set; }

        [JsonProperty("avatarImageURL")]
        public string imageurl { get; set; }

        [JsonProperty("inaraURL")]
        public string url { get; set; }
    }

    public class InaraCmdrRanks
    {
        [JsonProperty("rankName")]
        public string rank { get; set; }

        [JsonProperty("rankValue")]
        public int rankvalue { get; set; }

        [JsonProperty("rankProgress")]
        public double progress { get; set; }
    }

    public class InaraCmdrSquadron
    {
        [JsonProperty("SquadronID")]
        public int id { get; set; }

        [JsonProperty("SquadronName")]
        public string name { get; set; }

        [JsonProperty("SquadronMembersCount")]
        public int memberscount { get; set; }

        [JsonProperty("SquadronMemberRank")]
        public string squadronrank { get; set; }

        [JsonProperty("inaraURL")]
        public string url { get; set; }
    }
}
