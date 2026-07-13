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
        [PublicAPI( "the faction state of the factions in conflict (e.g. war, civil war, or election)" )]
        public string state => factionState?.localizedName;

        [PublicAPI( "the status of the conflict" )]
        public string status { get; set; } = status;

        [PublicAPI( "the system asset at stake in the conflict (if any)" )]
        public string stake => !string.IsNullOrEmpty(faction1Stake) ? faction1Stake : faction2Stake;

        [PublicAPI( "the number of days that the conflict has been ongoing" )]
        public int conflictdays => faction1dayswon + faction2dayswon;

        [PublicAPI( "the difference between the number of days won by one faction vs. the other" )]
        public int margin => Math.Abs(faction1dayswon - faction2dayswon);

        // Faction 1
        [PublicAPI( "the name of the first faction in the conflict" )]
        public string faction1 => Faction1?.name;

        [PublicAPI( "the number of days that the first faction has won" )]
        public int faction1dayswon { get; private set; } = faction1WonDays;

        // Faction 2
        [PublicAPI( "the name of the second faction in the conflict" )]
        public string faction2 => Faction2?.name;

        [PublicAPI( "the number of days that the second faction has won" )]
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
