using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiInaraService
{
    public partial class InaraService
    {
        // Documentation: https://inara.cz/inara-api-docs/

        // Returns basic information about commander from Inara like ranks, 
        // squadron, a link to the commander's Inara profile, etc. 

        public InaraCmdr GetCommanderProfile(string commanderName, bool inBeta)
        {
            return GetCommanderProfiles(new string[] { commanderName }, inBeta)?.FirstOrDefault();
        }

        public List<InaraCmdr> GetCommanderProfiles(string[] commanderNames, bool inBeta)
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
            List<InaraResponse> responses = SendEventBatch(ref events, inBeta);
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
        public int userID { get; set; }
        public string userName { get; set; }
        public string commanderName { get; set; }
        public List<InaraCmdrRanks> commanderRanksPilot { get; set; }
        public string preferredAllegianceName { get; set; }
        public string preferredPowerName { get; set; }
        public InaraCmdrSquadron commanderSquadron { get; set; }
        public string preferredGameRole { get; set; }
        public string avatarImageURL { get; set; }
        public string inaraURL { get; set; }
    }

    public class InaraCmdrRanks
    {
        public string rankName { get; set; }
        public int rankValue { get; set; }
        public double rankProgress { get; set; }
    }

    public class InaraCmdrSquadron
    {
        public int SquadronID { get; set; }
        public string SquadronName { get; set; }
        public int SquadronMembersCount { get; set; }
        public string SquadronMemberRank { get; set; }
        public string inaraURL { get; set; }
    }
}
