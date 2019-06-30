namespace EddiDataDefinitions
{
    public class Conflict
    {
        public string conflict => conflictType?.localizedName;

        public string status { get; set; }

        public string stake => !string.IsNullOrEmpty(faction1Stake) ? faction1Stake : faction2Stake;

        public int conflictdays => faction1dayswon + faction2dayswon;

        public string winningfaction => winningFaction?.name;

        public int winningdays { get; private set; }

        // Faction 1
        public string faction1 => Faction1?.name;

        public int faction1dayswon { get; private set; }

        // Faction 2
        public string faction2 => Faction2?.name;

        public int faction2dayswon { get; private set; }

        // Not intended to be user facing
        public FactionState conflictType { get; private set; }
        public Faction Faction1 { get; private set; }
        public Faction Faction2 { get; private set; }
        private string faction1Stake { get; set; }
        private string faction2Stake { get; set; }
        public Faction winningFaction { get; private set; }

        public Conflict(FactionState conflictType, string status, Faction Faction1, string faction1Stake, int faction1WonDays, Faction Faction2, string faction2Stake, int faction2WonDays)
        {
            this.conflictType = conflictType;
            this.status = status;
            this.Faction1 = Faction1;
            this.faction1Stake = faction1Stake;
            this.faction1dayswon = faction1WonDays;
            this.Faction2 = Faction2;
            this.faction2Stake = faction2Stake;
            this.faction2dayswon = faction2WonDays;

            if (faction1dayswon > faction2dayswon)
            {
                winningFaction = Faction1;
                winningdays = faction1dayswon;
            }
            else if (faction2dayswon > faction1dayswon)
            {
                winningFaction = Faction2;
                winningdays = faction2dayswon;
            }
        }
    }
}
