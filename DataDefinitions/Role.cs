using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Roles for ship
    /// </summary>
    public class Role : ResourceBasedLocalizedEDName<Role>, IComparable<Role>
    {
        static Role()
        {
            resourceManager = Properties.ShipRoles.ResourceManager;
            resourceManager.IgnoreCase = false;

            Assassination = new Role("Assassination");
            AXcombat = new Role("AXcombat");
            AXcombatsupport = new Role("AXcombatsupport");
            BountyHunting = new Role("Bountyhunting");
            Combat = new Role("Combat");
            CombatSupport = new Role("Combatsupport");
            Evacuation = new Role("Evacuation");
            Exploration = new Role("Exploration");
            Journalism = new Role("Journalism");
            Mining = new Role("Mining");
            MultiCrew = new Role("MultiCrew");
            MultiPurpose = new Role("Multipurpose");
            Piracy = new Role("Piracy");
            Racing = new Role("Racing");
            Refueling = new Role("Refueling");
            Repair = new Role("Repair");
            Science = new Role("Science");
            SearchAndRescue = new Role("Searchandrescue");
            Smuggling = new Role("Smuggling");
            Stealth = new Role("Stealth");
            Taxi = new Role("Taxi");
            Tourism = new Role("Tourism");
            Trading = new Role("Trading");
        }

        public static readonly Role Assassination;
        public static readonly Role AXcombat;
        public static readonly Role AXcombatsupport;
        public static readonly Role BountyHunting;
        public static readonly Role Combat;
        public static readonly Role CombatSupport;
        public static readonly Role Evacuation;
        public static readonly Role Exploration;
        public static readonly Role Journalism;
        public static readonly Role Mining;
        public static readonly Role MultiCrew;
        public static readonly Role MultiPurpose;
        public static readonly Role Piracy;
        public static readonly Role Racing;
        public static readonly Role Refueling;
        public static readonly Role Repair;
        public static readonly Role Science;
        public static readonly Role SearchAndRescue;
        public static readonly Role Smuggling;
        public static readonly Role Stealth;
        public static readonly Role Taxi;
        public static readonly Role Tourism;
        public static readonly Role Trading;

        // dummy used to ensure that the static constructor has run
        public Role() : this("")
        { }

        private Role(string edname) : base(edname, edname)
        { }

        public int CompareTo(Role rhs)
        {
            // A null rhs means that this object is greater.
            if (rhs == null)
            {
                return 1;
            }
            else
            {
                return string.Compare(localizedName, rhs.localizedName, StringComparison.Ordinal);
            }
        }

        public static List<Role> Sorted
        {
            get
            {
                lock (resourceLock)
                {
                    AllOfThem.Sort();
                    return AllOfThem;
                }
            }
        }
    }
}
