using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Roles for ship
    /// </summary>
    public class Role
    {
        public static readonly List<string> ROLES = new List<string>();

        public static readonly string Assassination = "Assassination";
        public static readonly string BountyHunting = "Bounty hunting";
        public static readonly string Combat = "Combat";
        public static readonly string CombatSupport = "Combat support";
        public static readonly string Exploration = "Exploration";
        public static readonly string Journalism = "Journalism";
        public static readonly string Mining = "Mining";
        public static readonly string MultiCrew = "MultiCrew";
        public static readonly string MultiPurpose = "Multi-purpose";
        public static readonly string Piracy = "Piracy";
        public static readonly string Racing = "Racing";
        public static readonly string Refueling = "Refueling";
        public static readonly string Science = "Science";
        public static readonly string SearchAndRescue = "Search and rescue";
        public static readonly string Smuggling = "Smuggling";
        public static readonly string Trading = "Trading";
        public static readonly string Taxi = "Taxi";

        static Role()
        {
            ROLES.Add(Assassination);
            ROLES.Add(BountyHunting);
            ROLES.Add(Combat);
            ROLES.Add(CombatSupport);
            ROLES.Add(Exploration);
            ROLES.Add(Journalism);
            ROLES.Add(Mining);
            ROLES.Add(MultiCrew);
            ROLES.Add(MultiPurpose);
            ROLES.Add(Piracy);
            ROLES.Add(Racing);
            ROLES.Add(Refueling);
            ROLES.Add(Science);
            ROLES.Add(SearchAndRescue);
            ROLES.Add(Smuggling);
            ROLES.Add(Taxi);
            ROLES.Add(Trading);
        }
    }
}
