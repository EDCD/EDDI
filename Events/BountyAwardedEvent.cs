﻿using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class BountyAwardedEvent : Event
    {
        public const string NAME = "Bounty awarded";
        public const string DESCRIPTION = "Triggered when you are awarded a bounty";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:10:11Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""FrogCorp"", ""Reward"":400 }, { ""Faction"":""Federation"", ""Reward"":123187 } ], ""Target"":""federation_dropship_mkii"", ""TotalReward"":123587, ""VictimFaction"":""TZ Arietis Purple Council"" }";

        [PublicAPI("The name of the asset you destroyed (if applicable)")]
        public string target { get; private set; }

        [PublicAPI("The name of the faction whose asset you destroyed")]
        public string faction { get; private set; }

        [PublicAPI("The total number of credits obtained for destroying the asset")]
        public long reward { get; private set; }

        [PublicAPI("The rewards obtained for destroying the asset")]
        public List<Reward> rewards { get; private set; }

        [PublicAPI("True if the rewards have been shared with wing-mates")]
        public bool shared { get; private set; }

        public BountyAwardedEvent(DateTime timestamp, string target, string faction, long reward, List<Reward> rewards, bool shared) : base(timestamp, NAME)
        {
            this.target = target;
            this.faction = faction;
            this.reward = reward;
            this.rewards = rewards;
            this.shared = shared;
        }
    }
}
