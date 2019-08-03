using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiInaraService
{
    // Documentation: https://inara.cz/inara-api-docs/
    // Inara API events related to adding friends, removing friends, and obtaining commander profiles
    public class getCommanderProfile : InaraAPIEvent
    {
        public getCommanderProfile(DateTime timestamp, string searchName)
        {
            // Returns basic information about commander from Inara like ranks, 
            // squadron, a link to the commander's Inara profile, etc. 
            if (string.IsNullOrEmpty(searchName))
            {
                return;
            }
            eventName = "getCommanderProfile";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "searchName", searchName }
            };
        }
    }

    public class addCommanderFriend : InaraAPIEvent
    {
        public addCommanderFriend(DateTime timestamp, string friendName, string gamePlatform)
        {
            // Adds a friend request to the target commander on Inara. 
            // The request may not be performed when such commander is not found under his/her in-game name.
            if (string.IsNullOrEmpty(friendName) || string.IsNullOrEmpty(gamePlatform))
            {
                return;
            }
            eventName = "addCommanderFriend";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "commanderName", friendName },
                { "gamePlatform", gamePlatform}
            };
        }
    }

    public class delCommanderFriend : InaraAPIEvent
    {
        public delCommanderFriend(DateTime timestamp, string friendName, string gamePlatform)
        {
            // Removes a target commander from the friends list on Inara. 
            // The request may not be performed when such commander is not found under his/her in-game name.
            if (string.IsNullOrEmpty(friendName) || string.IsNullOrEmpty(gamePlatform))
            {
                return;
            }
            eventName = "delCommanderFriend";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "commanderName", friendName },
                { "gamePlatform", gamePlatform}
            };
        }
    }
}
