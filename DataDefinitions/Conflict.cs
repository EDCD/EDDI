﻿using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class Conflict
    {
        [PublicAPI]
        public string state => factionState?.localizedName;

        [PublicAPI]
        public string status { get; set; }

        [PublicAPI]
        public string stake => !string.IsNullOrEmpty(faction1Stake) ? faction1Stake : faction2Stake;

        [PublicAPI]
        public int conflictdays => faction1dayswon + faction2dayswon;

        [PublicAPI]
        public int margin => Math.Abs(faction1dayswon - faction2dayswon);

        // Faction 1
        [PublicAPI]
        public string faction1 => Faction1?.name;

        [PublicAPI]
        public int faction1dayswon { get; private set; }

        // Faction 2
        [PublicAPI]
        public string faction2 => Faction2?.name;

        [PublicAPI]
        public int faction2dayswon { get; private set; }

        // Not intended to be user facing
        public FactionState factionState { get; private set; }
        public Faction Faction1 { get; private set; }
        public Faction Faction2 { get; private set; }
        private string faction1Stake { get; set; }
        private string faction2Stake { get; set; }
        public Faction winningFaction { get; private set; }

        public Conflict(FactionState conflictType, string status, Faction Faction1, string faction1Stake, int faction1WonDays, Faction Faction2, string faction2Stake, int faction2WonDays)
        {
            this.factionState = conflictType;
            this.status = status;
            this.Faction1 = Faction1;
            this.faction1Stake = faction1Stake;
            this.faction1dayswon = faction1WonDays;
            this.Faction2 = Faction2;
            this.faction2Stake = faction2Stake;
            this.faction2dayswon = faction2WonDays;
        }
    }
}
