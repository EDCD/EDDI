using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiCrimeMonitor
{
    [PublicAPI]
    public class BountyIncurredEvent : Event
    {
        public const string NAME = "Bounty incurred";
        public const string DESCRIPTION = "Triggered when you incur a bounty";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CommitCrime\",\"CrimeType\":\"assault\",\"Faction\":\"The Pilots Federation\",\"Victim\":\"Potapinski\",\"Bounty\":210}";

        [PublicAPI("The type of crime committed")]
        public string crimetype { get; private set; }

        [PublicAPI("The description of the crime committed")]
        public string crime { get; private set; }

        [PublicAPI("The name of the victim of the crime")]
        public string victim { get; private set; }

        [PublicAPI("The name of the faction issuing the bounty")]
        public string faction { get; private set; }

        [PublicAPI("The number of credits issued as the bounty")]
        public long bounty { get; private set; }

        public BountyIncurredEvent(DateTime timestamp, string crimetype, string faction, string victim, long bounty) : base(timestamp, NAME)
        {
            this.crimetype = crimetype;
            this.crime = Crime.FromEDName(crimetype).localizedName;
            this.faction = faction;
            this.victim = victim;
            this.bounty = bounty;
        }
    }
}
