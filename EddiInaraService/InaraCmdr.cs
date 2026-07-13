using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiInaraService
{
    public class InaraCmdr
    {
        [JsonProperty("userID")]
        public int id { get; set; }

        [PublicAPI( "The commander's Inara username" ), JsonProperty("userName")]
        public string username { get; set; }

        [PublicAPI( "The commander name" ), JsonProperty("commanderName")]
        public string commandername { get; set; }

        [PublicAPI( "The commander's last reported ranks (this is a list of *inaracmdrranks* objects)" ), JsonProperty("commanderRanksPilot")]
        public List<InaraCmdrRanks> commanderranks { get; set; }

        [PublicAPI( "The commander's last reported preferred allegiance" ), JsonProperty("preferredAllegianceName")]
        public string preferredallegiance { get; set; }

        [PublicAPI( "The commander's last reported preferred allegiance" ), JsonProperty("preferredPowerName")]
        public string preferredpower { get; set; }

        [PublicAPI( "The commander's last reported Inara squadron (this is an *inaracmdrsquadron* object)" ), JsonProperty("commanderSquadron")]
        public InaraCmdrSquadron squadron { get; set; }

        [PublicAPI( "The commander's last reported in-game role" ), JsonProperty("preferredGameRole")]
        public string preferredrole { get; set; }

        [JsonProperty("avatarImageURL")]
        public string imageurl { get; set; }

        [PublicAPI( "The url of the commander's profile on https://inara.cz" ), JsonProperty("inaraURL")]
        public string url { get; set; }
    }

    public class InaraCmdrRanks
    {
        [PublicAPI( "The name of the rank" ), JsonProperty("rankName")]
        public string rank { get; set; }

        [PublicAPI( "The rank as an integer value" ), JsonProperty("rankValue")]
        public int rankvalue { get; set; }

        [PublicAPI( "The process which has been made toward's the commander's next advancement in the rank" ), JsonProperty("rankProgress")]
        public double progress { get; set; }
    }

    public class InaraCmdrSquadron
    {
        [JsonProperty("SquadronID")]
        public int id { get; set; }

        [PublicAPI( "The commander's last reported squadron name" ), JsonProperty("SquadronName")]
        public string name { get; set; }

        [PublicAPI( "The commander's squadron's last reported membership count" ), JsonProperty("SquadronMembersCount")]
        public int memberscount { get; set; }

        [PublicAPI( "The commander's last reported rank with their Inara squadron" ), JsonProperty("SquadronMemberRank")]
        public string squadronrank { get; set; }

        [PublicAPI( "The url of the commander's squadron's profile on https://inara.ca" ), JsonProperty("inaraURL")]
        public string url { get; set; }
    }
}
