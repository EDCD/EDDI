using EddiDataDefinitions;
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
            return GetCommanderProfiles(new string[] { commanderName })?.FirstOrDefault();
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
            List<InaraResponse> responses = SendEventBatch(ref events, sendEvenForBetaGame: true);
            foreach (InaraResponse inaraResponse in responses)
            {
                string jsonCmdr = JsonConvert.SerializeObject(inaraResponse.eventData);
                cmdrs.Add(JsonConvert.DeserializeObject<InaraCmdr>(jsonCmdr));
            }
            return cmdrs;
        }
    }
}
