using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace EliteDangerousCompanionAppService
{
    public class Commander
    {
        private static string[] combatRanks = new string[9] { "Harmless", "Mostly Harmless", "Novice", "Competent", "Expert", "Master", "Dangerous", "Deadly", "Elite" };
        private static string[] tradeRanks = new string[9] { "Penniless", "Mostly Penniless", "Peddlar", "Dealer", "Merchant", "Broker", "Entrepreneur", "Tycoon", "Elite" };
        private static string[] exploreRanks = new string[9] { "Aimless", "Mostly Aimless", "Scout", "Surveyor", "Trailblzer", "Pathfinder", "Ranger", "Tycoon", "Pioneer" };
        private static string[] empireRanks = new string[15] { "Unknown", "Outsider", "Serf", "Master", "Squire", "Knight", "Lord", "Baron", "Viscount", "Count", "Earl", "Marquis", "Duke", "Prince", "King" };
        private static string[] federationRanks = new string[15] { "Unknown", "Recruit", "Cadet", "Midshipman", "Petty Officer", "Chief Petty Officer", "Warrant Officer", "Ensign", "Lieutenant", "Lieutenant Commander", "Post Commander", "Post Captain", "Rear Admiral", "Vice Admiral", "Admiral" };

        public string Name { get; set;  }

        public int CombatRating { get; set; }
        public string CombatRank { get; set; }

        public int TradeRating { get; set; }
        public string TradeRank { get; set; }

        public int ExploreRating { get; set; }
        public string ExploreRank { get; set; }

        public int EmpireRating { get; set; }
        public string EmpireRank { get; set; }

        public int FederationRating { get; set; }
        public string FederationRank { get; set; }

        public long Credits { get; set; }
        public long Debt { get; set; }

        public Ship Ship { get; set; }

        public static Commander FromProfile(JObject json)
        {
            Commander Commander = new Commander();

            //
            // Commander data
            //
            Commander.Name = (string)json["commander"]["name"];

            Commander.CombatRating = (int)json["commander"]["rank"]["combat"];
            Commander.CombatRank = combatRanks[Commander.CombatRating];

            Commander.TradeRating = (int)json["commander"]["rank"]["trade"];
            Commander.TradeRank = tradeRanks[Commander.TradeRating];

            Commander.ExploreRating = (int)json["commander"]["rank"]["explore"];
            Commander.ExploreRank = exploreRanks[Commander.ExploreRating];

            Commander.EmpireRating = (int)json["commander"]["rank"]["empire"];
            Commander.EmpireRank = federationRanks[(int)Commander.EmpireRating];
            Commander.FederationRating = (int)json["commander"]["rank"]["federation"];
            Commander.FederationRank = federationRanks[(int)Commander.FederationRating];

            Commander.Credits = (long)json["commander"]["credits"];
            Commander.Debt = (long)json["commander"]["debt"];

            Commander.Ship = Ship.FromProfile(json);

            return Commander;
        }
    }
}
