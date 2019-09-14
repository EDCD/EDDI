using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiInaraService
{
    public partial class InaraService
    {
        // Documentation: https://inara.cz/inara-api-docs/

        // Returns basic information about commander from Inara like ranks, 
        // squadron, a link to the commander's Inara profile, etc. 

        public InaraCmdr GetCommanderProfile(string commanderName)
        {
            return GetCommanderProfiles(new string[] { commanderName } )?.FirstOrDefault();
        }

        public List<InaraCmdr> GetCommanderProfiles(string[] commanderNames)
        {
            List<InaraCmdr> cmdrs = new List<InaraCmdr>();

            List<InaraAPIEvent> events = new List<InaraAPIEvent>();
            foreach (string commanderName in commanderNames)
            {
                events.Add(new InaraAPIEvent(DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>()
                {
                    { "searchName", commanderName }
                }));
            }
            List<InaraResponse> responses = SendEventBatch(ref events, sendEvenForBetaGame:true);
            foreach (InaraResponse inaraResponse in responses)
            {
                string jsonCmdr = JsonConvert.SerializeObject(inaraResponse.eventData);
                cmdrs.Add(JsonConvert.DeserializeObject<InaraCmdr>(jsonCmdr));
            }
            return cmdrs;
        }
    }

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
