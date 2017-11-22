using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Roles for ship
    /// </summary>
    public class Role
    {
        public static readonly List<string> ROLES = new List<string>();

        public static readonly string Assassination = I18N.GetString("Role_Assassination");  // "Assassination";
        public static readonly string BountyHunting = I18N.GetString("Role_Bountyhunting"); //"Bounty hunting";
        public static readonly string Combat = I18N.GetString("Role_Combat"); // "Combat";
        public static readonly string CombatSupport = I18N.GetString("Role_Combatsupport"); // "Combat support";
        public static readonly string Exploration = I18N.GetString("Role_Exploration"); // "Exploration";
        public static readonly string Journalism = I18N.GetString("Role_Journalism"); // "Journalism";
        public static readonly string Mining = I18N.GetString("Role_Mining"); // "Mining";
        public static readonly string MultiCrew = I18N.GetString("Role_MultiCrew"); // "MultiCrew";
        public static readonly string MultiPurpose = I18N.GetString("Role_Multipurpose"); // "Multi-purpose";
        public static readonly string Piracy = I18N.GetString("Role_Piracy"); // "Piracy";
        public static readonly string Racing = I18N.GetString("Role_Racing"); // "Racing";
        public static readonly string Refueling = I18N.GetString("Role_Refueling"); // "Refueling";
        public static readonly string Science = I18N.GetString("Role_Science"); // "Science";
        public static readonly string SearchAndRescue = I18N.GetString("Role_Searchandrescue"); // "Search and rescue";
        public static readonly string Smuggling = I18N.GetString("Role_Smuggling"); // "Smuggling";
        public static readonly string Trading = I18N.GetString("Role_Trading"); // "Trading";
        public static readonly string Taxi = I18N.GetString("Role_Taxi"); // "Taxi";
        public static readonly string Repair = I18N.GetString("Role_Repair"); // "Repair";
        public static readonly string Brothel = I18N.GetString("Role_Brothel"); // "Repair";

        static Role()
        {
            ROLES.Add(Assassination);
            ROLES.Add(BountyHunting);
            ROLES.Add(Brothel);
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
            ROLES.Add(Repair);
            ROLES.Add(Science);
            ROLES.Add(SearchAndRescue);
            ROLES.Add(Smuggling);
            ROLES.Add(Taxi);
            ROLES.Add(Trading);
        }
    }
}
