using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiInaraService
{
    public class InaraCmdr
    {
        [JsonProperty("userID")]
        public int id { get; set; }

        [PublicAPI, JsonProperty("userName")]
        public string username { get; set; }

        [PublicAPI, JsonProperty("commanderName")]
        public string commandername { get; set; }

        [PublicAPI, JsonProperty("commanderRanksPilot")]
        public List<InaraCmdrRanks> commanderranks { get; set; }

        [PublicAPI, JsonProperty("preferredAllegianceName")]
        public string preferredallegiance { get; set; }

        [PublicAPI, JsonProperty("preferredPowerName")]
        public string preferredpower { get; set; }

        [PublicAPI, JsonProperty("commanderSquadron")]
        public InaraCmdrSquadron squadron { get; set; }

        [PublicAPI, JsonProperty("preferredGameRole")]
        public string preferredrole { get; set; }

        [JsonProperty("avatarImageURL")]
        public string imageurl { get; set; }

        [PublicAPI, JsonProperty("inaraURL")]
        public string url { get; set; }
    }

    public class InaraCmdrRanks
    {
        [PublicAPI, JsonProperty("rankName")]
        public string rank { get; set; }

        [PublicAPI, JsonProperty("rankValue")]
        public int rankvalue { get; set; }

        [PublicAPI, JsonProperty("rankProgress")]
        public double progress { get; set; }
    }

    public class InaraCmdrSquadron
    {
        [JsonProperty("SquadronID")]
        public int id { get; set; }

        [PublicAPI, JsonProperty("SquadronName")]
        public string name { get; set; }

        [PublicAPI, JsonProperty("SquadronMembersCount")]
        public int memberscount { get; set; }

        [PublicAPI, JsonProperty("SquadronMemberRank")]
        public string squadronrank { get; set; }

        [PublicAPI, JsonProperty("inaraURL")]
        public string url { get; set; }
    }
}
