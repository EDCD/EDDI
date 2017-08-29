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

        public static readonly string Assassination = "Assassinat";
        public static readonly string BountyHunting = "Chasse à prime";
        public static readonly string Combat = "Combat";
        public static readonly string CombatSupport = "Support au combat";
        public static readonly string Exploration = "Exploration";
        public static readonly string Mining = "Minage";
        public static readonly string MultiPurpose = "Multi-rôle";
        public static readonly string Piracy = "Piratage";
        public static readonly string Smuggling = "Contrebande";
        public static readonly string Trading = "Commerce";
        public static readonly string Taxi = "Taxi";
        public static readonly string Medicorps = "Médi-Corps";
        public static readonly string Fuel = "Fuel R.A.T.S.";
        public static readonly string Press = "Photo/Presse";
        public static readonly string Brothel = "Lupanar de luxe";
        public static readonly string Course = "Course";
        public static readonly string SaveRescue = "Sauvetage - Secours";
	
		

        static Role()
        {
            ROLES.Add(Assassination);
            ROLES.Add(BountyHunting);
            ROLES.Add(Combat);
            ROLES.Add(Trading);
            ROLES.Add(Smuggling);
            ROLES.Add(Course);
            ROLES.Add(Exploration);
            ROLES.Add(Fuel);
            ROLES.Add(Brothel);
            ROLES.Add(Medicorps);
            ROLES.Add(Mining);
            ROLES.Add(MultiPurpose);
            ROLES.Add(Press);
            ROLES.Add(Piracy);
            ROLES.Add(SaveRescue);
            ROLES.Add(CombatSupport);
            ROLES.Add(Taxi);
        }
    }
}
