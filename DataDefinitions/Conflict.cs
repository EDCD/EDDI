using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class Conflict (
        FactionState conflictType,
        string status,
        Faction faction1,
        string faction1Stake,
        int faction1WonDays,
        Faction faction2,
        string faction2Stake,
        int faction2WonDays )
    {
        [PublicAPI]
        public string state => factionState?.localizedName;

        [PublicAPI]
        public string status { get; set; } = status;

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
        public int faction1dayswon { get; private set; } = faction1WonDays;

        // Faction 2
        [PublicAPI]
        public string faction2 => Faction2?.name;

        [PublicAPI]
        public int faction2dayswon { get; private set; } = faction2WonDays;

        // Not intended to be user facing
        public FactionState factionState { get; private set; } = conflictType;
        public Faction Faction1 { get; private set; } = faction1;
        public Faction Faction2 { get; private set; } = faction2;
        private string faction1Stake { get; set; } = faction1Stake;
        private string faction2Stake { get; set; } = faction2Stake;
        public Faction winningFaction { get; private set; }
    }
}
