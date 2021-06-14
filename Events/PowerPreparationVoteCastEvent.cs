using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PowerPreparationVoteCast : Event
    {
        public const string NAME = "Power preparation vote cast";
        public const string DESCRIPTION = "Triggered when a commander votes for system preparation";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-16T09:28:19Z"", ""event"":""PowerplayVote"", ""Power"":""Zachary Hudson"", ""System"":""Sol"", ""Votes"":10 }";

        [PublicAPI("The name of the power for which the commander is voting")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("The name of the system for which the commander voted (might be missing due to journal bug)")]
        public string system { get; private set; }

        [PublicAPI("The number of votes cast for the system")]
        public int amount { get; private set; }

        // Not intended to be user facing

        public Power Power { get; private set; }

        public PowerPreparationVoteCast(DateTime timestamp, Power Power, string system, int amount) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.system = system;
            this.amount = amount;
        }
    }
}
