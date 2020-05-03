﻿using Newtonsoft.Json;
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

        public InaraCmdr GetCommanderProfile(string cmdrName)
        {
            return GetCommanderProfiles(new[] { cmdrName })?.FirstOrDefault();
        }

        public List<InaraCmdr> GetCommanderProfiles(IEnumerable<string> cmdrNames)
        {
            List<InaraCmdr> cmdrs = new List<InaraCmdr>();

            List<InaraAPIEvent> events = new List<InaraAPIEvent>();
            foreach (string cmdrName in cmdrNames)
            {
                events.Add(new InaraAPIEvent(DateTime.UtcNow, "getCommanderProfile", new Dictionary<string, object>()
                {
                    { "searchName", cmdrName }
                }));
            }
            var responses = SendEventBatch(events, InaraConfiguration.FromFile());
            foreach (InaraResponse inaraResponse in responses)
            {
                string jsonCmdr = JsonConvert.SerializeObject(inaraResponse.eventData);
                cmdrs.Add(JsonConvert.DeserializeObject<InaraCmdr>(jsonCmdr));
            }
            return cmdrs;
        }
    }
}
