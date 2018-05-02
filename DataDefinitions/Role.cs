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
            BountyHunting = new Role("Bountyhunting");
            Combat = new Role("Combat");
            CombatSupport = new Role("Combatsupport");
            Exploration = new Role("Exploration");
            Journalism = new Role("Journalism");
            Mining = new Role("Mining");
            MultiCrew = new Role("MultiCrew");
            MultiPurpose = new Role("Multipurpose");
            Piracy = new Role("Piracy");
            Racing = new Role("Racing");
            Refueling = new Role("Refueling");
            Science = new Role("Science");
            SearchAndRescue = new Role("Searchandrescue");
            Smuggling = new Role("Smuggling");
            Taxi = new Role("Taxi");
            Trading = new Role("Trading");
        }

        public static readonly Role Assassination;
        public static readonly Role BountyHunting;
        public static readonly Role Combat;
        public static readonly Role CombatSupport;
        public static readonly Role Exploration;
        public static readonly Role Journalism;
        public static readonly Role Mining;
        public static readonly Role MultiCrew;
        public static readonly Role MultiPurpose;
        public static readonly Role Piracy;
        public static readonly Role Racing;
        public static readonly Role Refueling;
        public static readonly Role Science;
        public static readonly Role SearchAndRescue;
        public static readonly Role Smuggling;
        public static readonly Role Trading;
        public static readonly Role Taxi;

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
                return this.localizedName.CompareTo(rhs.localizedName);
            }
        }

        public static List<Role> Sorted
        {
            get
            {
                AllOfThem.Sort();
                return AllOfThem;
            }
        }
    }
}
