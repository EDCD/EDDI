using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class CommunityGoalEvent : Event
    {
        public const string NAME = "Community goal";
        public const string DESCRIPTION = "Triggered when checking the status of a community goal";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-14T13:20:28Z\", \"event\":\"CommunityGoal\", \"CurrentGoals\":[ { \"CGID\":726, \"Title\":\"Alliance Research Initiative – Trade\", \"SystemName\":\"Kaushpoos\", \"MarketName\":\"Neville Horizons\", \"Expiry\":\"2017-08-17T14:58:14Z\", \"IsComplete\":false, \"CurrentTotal\":10062, \"PlayerContribution\":562, \"NumContributors\":101, \"TopRankSize\":10, \"PlayerInTopRank\":false, \"TierReached\":\"Tier 1\", \"PlayerPercentileBand\":50, \"Bonus\":200000 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommunityGoalEvent()
        {
            VARIABLES.Add("cgid", "the unique id of the goal (this is a list if participating in multiple goals)");
            VARIABLES.Add("name", "The description of the goal (this is a list if participating in multiple goals)");
            VARIABLES.Add("system", "The system where the goal is located (this is a list if participating in multiple goals)");
            VARIABLES.Add("station", "The station where the goal is located (this is a list if participating in multiple goals)");
            VARIABLES.Add("expiry", "The expiration time and date for the goal (this is a list if participating in multiple goals)");
            VARIABLES.Add("iscomplete", "The completion status of the goal (true/false) (this is a list if participating in multiple goals)");
            VARIABLES.Add("total", "The community's current total contributions (this is a list if participating in multiple goals)");
            VARIABLES.Add("contribution", "The commander's contribution (this is a list if participating in multiple goals)");
            VARIABLES.Add("contributors", "The number of commanders participating (this is a list if participating in multiple goals)");
            VARIABLES.Add("percentileband", "the commander's current rewards percentile (this is a list if participating in multiple goals)");
            VARIABLES.Add("topranksize", "The number of commanders in the top tank (only for goals with a fixed size top rank) (this is a list if participating in multiple goals)");
            VARIABLES.Add("toprank", "Whether the commander is currently in the top rank (true/false) (only for goals with a fixed size top rank) (this is a list if participating in multiple goals)");
            VARIABLES.Add("tier", "The current tier of the goal (only once the 1st tier is reached) (this is a list if participating in multiple goals)");
            VARIABLES.Add("tierreward", "The reward on offer for the current tier");
        }

        public List<long> cgid { get; private set; }

        public List<string> name { get; private set; }

        public List<string> system { get; private set; }

        public List<string> station { get; private set; }

        public List<DateTime> expiry { get; private set; }

        public List<bool> iscomplete { get; private set; }

        public List<int> total { get; private set; }

        public List<int> contribution { get; private set; }

        public List<int> contributors { get; private set; }

        public List<decimal> percentileband { get; private set; }

        public List<int?> topranksize { get; private set; }

        public List<bool?> toprank { get; private set; }

        public List<string> tier { get; private set; }

        public List<long?> tierreward { get; private set; }

        public CommunityGoalEvent(DateTime timestamp, List<long> cgid, List<string> name, List<string> system, List<string> station, List<DateTime> expiry, List<bool> iscomplete, List<int> total, List<int> contribution, List<int> contributors, List<decimal> percentileband, List<int?> topranksize, List<bool?> toprank, List<string> tier, List<long?> tierreward) : base(timestamp, NAME)

        {
            this.cgid = cgid;
            this.name = name;
            this.system = system;
            this.station = station;
            this.expiry = expiry;
            this.iscomplete = iscomplete;
            this.total = total;
            this.contribution = contribution;
            this.contributors = contributors;
            this.percentileband = percentileband;
            this.topranksize = topranksize;
            this.toprank = toprank;
            this.tier = tier;
            this.tierreward = tierreward;
        }
    }
}
