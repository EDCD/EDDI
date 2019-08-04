using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiInaraService
{
    // Documentation: https://inara.cz/inara-api-docs/
    // Inara API events related to customizing the commander's personal profile
    
        /*
    public class setCommanderRankPower : InaraAPIEvent
    {
        public setCommanderRankPower(DateTime timestamp, Power power, int rankValue)
        {
            // Sets commander pledged power rank, related to Powerplay. 
            // If there is a newer value already stored (compared by timestamp), the update is ignored.
            if (power is null || rankValue < 0 || rankValue > 5)
            {
                return;
            }
            eventName = "setCommanderRankPower";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "powerName", power.invariantName },
                { "rankValue", rankValue }
            };
        }
    }

    public class setCommanderReputationMajorFaction : InaraAPIEvent
    {
        public setCommanderReputationMajorFaction(DateTime timestamp, string majorfactionName, float majorfactionReputation)
        {
            // Sets commander's reputation with the major factions like Federation, Empire, etc.
            // Reputation progress in a range: [-1..1], which corresponds to a reputation range from -100% (hostile) to 100% (allied).
            if (majorfactionName is null || majorfactionReputation < -1 || majorfactionReputation > 1)
            {
                return;
            }
            eventName = "setCommanderReputationMajorFaction";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "majorfactionName", majorfactionName },
                { "majorfactionReputation", majorfactionReputation }
            };
        }
    }

    public class setCommanderReputationMinorFaction : InaraAPIEvent
    {
        public setCommanderReputationMinorFaction(DateTime timestamp, string minorfactionName, float minorfactionReputation)
        {
            // Sets commander's reputation with the minor faction. The values can be found in journal's FSDJump and Location events, as a MyReputation property.
            // Reputation progress in a range: [-1..1], which corresponds to a reputation range from -100% (hostile) to 100% (allied).
            if (minorfactionName is null || minorfactionReputation < -1 || minorfactionReputation > 1)
            {
                return;
            }
            eventName = "setCommanderReputationMinorFaction";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "majorfactionName", minorfactionName },
                { "majorfactionReputation", minorfactionReputation }
            };
        }
    }
    */
}
