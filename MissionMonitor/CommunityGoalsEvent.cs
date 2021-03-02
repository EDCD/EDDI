using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommunityGoalsEvent : Event
    {
        public const string NAME = "Community goals";
        public const string DESCRIPTION = "Triggered when checking the status of community goals";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-14T13:20:28Z\", \"event\":\"CommunityGoal\", \"CurrentGoals\":[ { \"CGID\":726, \"Title\":\"Alliance Research Initiative – Trade\", \"SystemName\":\"Kaushpoos\", \"MarketName\":\"Neville Horizons\", \"Expiry\":\"2017-08-17T14:58:14Z\", \"IsComplete\":false, \"CurrentTotal\":10062, \"PlayerContribution\":562, \"NumContributors\":101, \"TopRankSize\":10, \"PlayerInTopRank\":false, \"TierReached\":\"Tier 1\", \"PlayerPercentileBand\":50, \"Bonus\":200000 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommunityGoalsEvent()
        {
        }

        // Not intended to be user facing
        public List<CommunityGoal> goals { get; private set; }

        public CommunityGoalsEvent(DateTime timestamp, List<CommunityGoal> goals) : base(timestamp, NAME)
        {
            this.goals = goals;
        }
    }
}
