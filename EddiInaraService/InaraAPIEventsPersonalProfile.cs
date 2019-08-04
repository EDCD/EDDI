using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiInaraService
{
    // Documentation: https://inara.cz/inara-api-docs/
    // Inara API events related to customizing the commander's personal profile

    public class addCommanderPermit : InaraAPIEvent
    {
        public addCommanderPermit(DateTime timestamp, string starsystemName)
        {
            // Adds star system permit for the commander. You do not need to handle permits granted for the 
            // Pilots Federation or Navy rank promotion, but you should handle any other ways (like mission 
            // rewards).
            if (string.IsNullOrEmpty(starsystemName))
            {
                return;
            }
            eventName = "addCommanderPermit";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "starsystemName", starsystemName }
            };
        }
    }

    public class setCommanderCredits : InaraAPIEvent
    {
        public setCommanderCredits(DateTime timestamp, long? commanderCredits, long commanderLoan = 0)
        {
            // Sets current credits and loans. A record is added to the credits log (if the value differs).
            // Warning: Do NOT set credits/assets unless you are absolutely sure they are correct. 
            // The journals currently doesn't contain crew wage cuts, so credit gains are very probably off 
            // for most of the players. Also, please, do not send each minor credits change, as it will 
            // spam player's credits log with unusable data and they won't be most likely very happy about it. 
            // It may be good to set credits just on the session start, session end and on the big changes 
            // or in hourly intervals.
            if (commanderCredits is null)
            {
                return;
            }
            eventName = "setCommanderCredits";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
            {
                { "commanderCredits", (long)commanderCredits },
                { "commanderLoan", commanderLoan }
            };
        }
    }

    public class setCommanderGameStatistics : InaraAPIEvent
    {
        public setCommanderGameStatistics(DateTime timestamp, string rawStatistics)
        {
            // Sets commander's in-game statistics. Please note that the statistics are always overridden 
            // as a whole, so any partial updates will cause erasing of the rest.
            if (rawStatistics is null)
            {
                return;
            }
            eventName = "setCommanderGameStatistics";
            eventTimeStamp = timestamp;
            eventData = rawStatistics;
        }
    }

    public class setCommanderRankEngineer : InaraAPIEvent
    {
        // Progress events may contain data about a single engineer
        public setCommanderRankEngineer(DateTime timestamp, Dictionary<string, object> engineerEventData)
        {
            eventName = "setCommanderRankEngineer";
            eventTimeStamp = timestamp;
            eventData = engineerEventData;
        }

        // Startup events will contain data about all known engineers
        public setCommanderRankEngineer(DateTime timestamp, List<Dictionary<string, object>> engineerEventData)
        {
            eventName = "setCommanderRankEngineer";
            eventTimeStamp = timestamp;
            eventData = engineerEventData;
        }
    }

    public class setCommanderRankPilot : InaraAPIEvent
    {
        public setCommanderRankPilot(DateTime timestamp, string rankName, int rankValue = 0, float rankProgress = 0)
        {
            // Sets commander Elite and Navy ranks. You can set just rank or its progress or both at once. 
            // If there is a newer value already stored (compared by timestamp), the update is ignored. 
            // Possible star system permits tied to ranks are awarded to the commander automatically.
            // Pilots federation/Navy rank name as are in the journals (["combat", "trade", "explore", "cqc", "federation", "empire"]) 
            // Rank value (range [0..8] for Pilots federation ranks, range [0..14] for Navy ranks)
            // Rank progress (range: [0..1], which corresponds to 0% - 100%)
            if (rankName is null
                || rankValue < 0 || rankValue > 8
                || rankProgress < 0 || rankProgress > 1
                || rankValue + rankProgress == 0)
            {
                return;
            }
            eventName = "setCommanderRankPilot";
            eventTimeStamp = timestamp;
            eventData = new Dictionary<string, object>()
                {
                    { "rankName", rankName }
                };
            if (rankValue > 0)
            {
                ((Dictionary<string, object>)eventData).Add("rankValue", rankValue);
            }
            if (rankProgress > 0)
            {
                ((Dictionary<string, object>)eventData).Add("rankProgress", rankProgress);
            }
        }
    }

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
}
