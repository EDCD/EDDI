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

        public static readonly string BountyHunting = "Bounty hunting";
        public static readonly string Combat = "Combat";
        public static readonly string Exploration = "Exploration";
        public static readonly string Mining = "Mining";
        public static readonly string MultiPurpose = "Multi-purpose";
        public static readonly string Piracy = "Piracy";
        public static readonly string Smuggling = "Smuggling";
        public static readonly string Trading = "Trading";

        static Role()
        {
            ROLES.Add(BountyHunting);
            ROLES.Add(Combat);
            ROLES.Add(Exploration);
            ROLES.Add(Mining);
            ROLES.Add(MultiPurpose);
            ROLES.Add(Piracy);
            ROLES.Add(Smuggling);
            ROLES.Add(Trading);
        }
    }
}
