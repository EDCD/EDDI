using System.Collections.Generic;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Details about a commander
    /// </summary>
    public class Commander
    {
        public static string[] combatRanks = new string[9] { "Harmless", "Mostly Harmless", "Novice", "Competent", "Expert", "Master", "Dangerous", "Deadly", "Elite" };
        public static string[] tradeRanks = new string[9] { "Penniless", "Mostly Penniless", "Peddlar", "Dealer", "Merchant", "Broker", "Entrepreneur", "Tycoon", "Elite" };
        public static string[] exploreRanks = new string[9] { "Aimless", "Mostly Aimless", "Scout", "Surveyor", "Trailblzer", "Pathfinder", "Ranger", "Pioneer", "Elite" };
        public static string[] empireRanks = new string[15] { "Unknown", "Outsider", "Serf", "Master", "Squire", "Knight", "Lord", "Baron", "Viscount", "Count", "Earl", "Marquis", "Duke", "Prince", "King" };
        public static string[] federationRanks = new string[15] { "Unknown", "Recruit", "Cadet", "Midshipman", "Petty Officer", "Chief Petty Officer", "Warrant Officer", "Ensign", "Lieutenant", "Lieutenant Commander", "Post Commander", "Post Captain", "Rear Admiral", "Vice Admiral", "Admiral" };

        /// <summary>The commander's name</summary>
        public string name { get; set;  }
        /// <summary>The commander's name as spoken</summary>
        public string phoneticname { get; set; }

        /// <summary>The numeric combat rating, 0 to 8</summary>
        public int combatrating { get; set; }
        /// <summary>The name of the combat rating</summary>
        public string combatrank { get; set; }

        /// <summary>The numeric trade rating, 0 to 8</summary>
        public int traderating { get; set; }
        /// <summary>The name of the trade rating</summary>
        public string traderank { get; set; }

        /// <summary>The numeric explorer rating, 0 to 8</summary>
        public int explorationrating { get; set; }
        /// <summary>The name of the explorer rating</summary>
        public string explorationrank { get; set; }

        /// <summary>The numeric empire rating, 0 to 14</summary>
        public int empirerating { get; set; }
        /// <summary>The name of the empire rating</summary>
        public string empirerank { get; set; }

        /// <summary>The numeric federation rating, 0 to 14</summary>
        public int federationrating { get; set; }
        /// <summary>The name of the federation rating</summary>
        public string federationrank { get; set; }

        /// <summary>The number of credits the commander holds</summary>
        public long credits { get; set; }
        /// <summary>The amount of debt the commander owes</summary>
        public long debt { get; set; }
    }
}
