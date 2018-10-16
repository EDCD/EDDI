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

            var Assassination = new Role("Assassination");
            var BountyHunting = new Role("BountyHunting");
            var Combat = new Role("Combat");
            var CombatSupport = new Role("CombatSupport");
            var Exploration = new Role("Exploration");
            var Journalism = new Role("Journalism");
            var Mining = new Role("Mining");
            var MultiCrew = new Role("MultiCrew");
            var MultiPurpose = new Role("MultiPurpose");
            var Piracy = new Role("Piracy");
            var Racing = new Role("Racing");
            var Refueling = new Role("Refueling");
            var Science = new Role("Science");
            var SearchAndRescue = new Role("SearchAndRescue");
            var Smuggling = new Role("Smuggling");
            var Taxi = new Role("Taxi");
            var Trading = new Role("Trading");
        }

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
