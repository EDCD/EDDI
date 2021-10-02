using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
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

        [PublicAPI("The updates that triggered the event (as objects)")]
        public List<CGUpdate> updates { get; private set; }

        [PublicAPI("The unique id of the goal")]
        public long cgid => goal.cgid;

        [PublicAPI("The description of the goal")]
        public string name => goal.name;

        [PublicAPI("The star system where the goal is located")]
        public string system => goal.system;

        [PublicAPI("The station where the goal is located")]
        public string station => goal.station;

        [PublicAPI("The expiration time for the goal in seconds")]
        public long expiry => goal.expiry;

        [PublicAPI("The completion status of the goal (true/false)")]
        public bool iscomplete => goal.iscomplete;

        [PublicAPI("The community's current total contributions")]
        public long total => goal.total;

        [PublicAPI("The commander's contribution")]
        public long contribution => goal.contribution;

        [PublicAPI("The number of commanders participating")]
        public int contributors => goal.contributors;

        [PublicAPI("the commander's current rewards percentile")]
        public int percentileband => goal.percentileband;

        [PublicAPI("The number of commanders in the top rank (only for goals with a fixed size top rank)")]
        public int? topranksize => goal.topranksize;

        [PublicAPI("Whether the commander is currently in the top rank (true/false) (only for goals with a fixed size top rank)")]
        public bool? toprank => goal.toprank;

        [PublicAPI("The current tier of the goal (only once the 1st tier is reached)")]
        public int? tier => goal.tier;

        [PublicAPI("The reward on offer for the current tier")]
        public long? tierreward => goal.tierreward;

        [PublicAPI("The top tier of the goal")]
        public int? toptier => goal.toptier;

        [PublicAPI("The reward on offer for the top tier")]
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
