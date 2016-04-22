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
        public string Name { get; set;  }
        /// <summary>The commander's name as spoken</summary>
        public string PhoneticName { get; set; }

        /// <summary>The numeric combat rating, 0 to 8</summary>
        public int CombatRating { get; set; }
        /// <summary>The name of the combat rating</summary>
        public string CombatRank { get; set; }

        /// <summary>The numeric trade rating, 0 to 8</summary>
        public int TradeRating { get; set; }
        /// <summary>The name of the trade rating</summary>
        public string TradeRank { get; set; }

        /// <summary>The numeric explorer rating, 0 to 8</summary>
        public int ExploreRating { get; set; }
        /// <summary>The name of the explorer rating</summary>
        public string ExploreRank { get; set; }

        /// <summary>The numeric empire rating, 0 to 14</summary>
        public int EmpireRating { get; set; }
        /// <summary>The name of the empire rating</summary>
        public string EmpireRank { get; set; }

        /// <summary>The numeric federation rating, 0 to 14</summary>
        public int FederationRating { get; set; }
        /// <summary>The name of the federation rating</summary>
        public string FederationRank { get; set; }

        /// <summary>The number of credits the commander holds</summary>
        public long Credits { get; set; }
        /// <summary>The amount of debt the commander owes</summary>
        public long Debt { get; set; }

        /// <summary>The name of the current starsystem</summary>
        public string StarSystem { get; set; }

        /// <summary>The commander's current ship</summary>
        public Ship Ship { get; set; }

        /// <summary>The commander's stored ships</summary>
        public List<Ship> StoredShips { get; set; }

        /// <summary>The name of the last station the commander docked at</summary>
        public string LastStation { get; set; }

        // The following shouldn't be in here but they come as part of the profile.  Move them somewhere a little more sane when we can access this data separately
        /// <summary>The modules available at the station the commander last docked at</summary>
        public List<Module> Outfitting;

        public Commander()
        {
            StoredShips = new List<Ship>();
            Outfitting = new List<Module>();
        }
    }
}
