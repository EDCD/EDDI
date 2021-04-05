using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class CommunityGoalEvent : Event
    {
        public const string NAME = "Community goal";
        public const string DESCRIPTION = "Triggered when the status of a community goal changes";
        public static Event SAMPLE = new CommunityGoalEvent(DateTime.UtcNow, new List<CGUpdate>() { new CGUpdate("Tier", "Increase"), new CGUpdate("Percentile", "Increase") }, new CommunityGoal()
        {
            cgid = 641,
            name = "Defence of the Galactic Summit",
            system = "Sirius",
            station = "Spirit of Laelaps",
            expiryDateTime = DateTime.Parse("2021-03-04T06:00:00Z").ToUniversalTime(),
            iscomplete = false,
            total = 163782436330,
            contribution = 84049848, 
            contributors = 8354,
            TopTier = new TopTier("Tier 8", string.Empty),
            topranksize = 10,
            toprank = false,
            Tier = "Tier 5",
            percentileband = 10, 
            tierreward = 100000000
        });
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommunityGoalEvent()
        {
            VARIABLES.Add("updates", "The updates that triggered the event (as objects)");
            VARIABLES.Add("cgid", "The unique id of the goal");
            VARIABLES.Add("name", "The description of the goal");
            VARIABLES.Add("system", "The star system where the goal is located");
            VARIABLES.Add("station", "The station where the goal is located");
            VARIABLES.Add("expiry", "The expiration time for the goal in seconds");
            VARIABLES.Add("iscomplete", "The completion status of the goal (true/false)");
            VARIABLES.Add("total", "The community's current total contributions");
            VARIABLES.Add("contribution", "The commander's contribution");
            VARIABLES.Add("contributors", "The number of commanders participating");
            VARIABLES.Add("percentileband", "the commander's current rewards percentile");
            VARIABLES.Add("topranksize", "The number of commanders in the top rank (only for goals with a fixed size top rank)");
            VARIABLES.Add("toprank", "Whether the commander is currently in the top rank (true/false) (only for goals with a fixed size top rank)");
            VARIABLES.Add("tier", "The current tier of the goal (only once the 1st tier is reached)");
            VARIABLES.Add("tierreward", "The reward on offer for the current tier");
            VARIABLES.Add("toptier", "The top tier of the goal");
            VARIABLES.Add("toptierreward", "The reward on offer for the top tier");
        }

        [PublicAPI]
        public List<CGUpdate> updates { get; private set; }

        [PublicAPI]
        public long cgid => goal.cgid;

        [PublicAPI]
        public string name => goal.name;

        [PublicAPI]
        public string system => goal.system;

        [PublicAPI]
        public string station => goal.station;

        [PublicAPI]
        public long expiry => goal.expiry;

        [PublicAPI]
        public bool iscomplete => goal.iscomplete;

        [PublicAPI]
        public long total => goal.total;

        [PublicAPI]
        public long contribution => goal.contribution;

        [PublicAPI]
        public int contributors => goal.contributors;

        [PublicAPI]
        public int percentileband => goal.percentileband;

        [PublicAPI]
        public int? topranksize => goal.topranksize;

        [PublicAPI]
        public bool? toprank => goal.toprank;

        [PublicAPI]
        public int? tier => goal.tier;

        [PublicAPI]
        public long? tierreward => goal.tierreward;

        [PublicAPI]
        public int? toptier => goal.toptier;

        [PublicAPI]
        public string toptierreward => goal.toptierreward;

        // Not intended to be user facing

        public CommunityGoal goal { get; private set; }

        public CommunityGoalEvent(DateTime timestamp, List<CGUpdate> cgupdates, CommunityGoal goal) : base(timestamp, NAME)
        {
            this.updates = cgupdates;
            this.goal = goal;
        }
    }

    public class CGUpdate
    {
        [PublicAPI]
        public string type { get; private set; }

        [PublicAPI]
        public string direction { get; private set; }

        public CGUpdate(string updateType, string updateDirection)
        {
            type = updateType;
            direction = updateDirection;
        }
    }
}
