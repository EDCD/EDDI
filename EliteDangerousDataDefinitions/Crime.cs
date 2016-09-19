using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Crime types
    /// </summary>
    public class Crime
    {
        private static readonly List<Crime> CRIMES = new List<Crime>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private Crime(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            CRIMES.Add(this);
        }

        public static readonly Crime None = new Crime("none", "None");
        public static readonly Crime Assault = new Crime("assault", "Assault");
        public static readonly Crime Murder = new Crime("Murder", "Murder");
        public static readonly Crime Piracy = new Crime("Piracy", "Piracy");
        public static readonly Crime Interdiction = new Crime("Interdiction", "Interdiction");
        public static readonly Crime IllegalCargo = new Crime("IllegalCargo", "Illegal cargo");
        public static readonly Crime DisobeyPolice = new Crime("DisobeyPolice", "Disobey police");
        public static readonly Crime FireInNoFireZone = new Crime("fireInNoFireZone", "Fire in no-fire zone");
        public static readonly Crime FireInStation = new Crime("fireInStation", "Fire in station");
        public static readonly Crime DumpingDangerous = new Crime("DumpingDangerous", "Dumping dangerous cargo");
        public static readonly Crime DumpingNearStation = new Crime("DumpingNearStation", "Dumping cargo near a station");
        public static readonly Crime BlockingAirlockMinor = new Crime("DockingMinor_BlockingAirlock", "Blocking an airlock (minor)");
        public static readonly Crime BlockingAirlockMajor = new Crime("DockingMajor_BlockingAirlock", "Blocking an airlock (major)");
        public static readonly Crime BlockingLandingPadMinor = new Crime("DockingMinor_BlockingLandingPad", "Blocking a landing pad (minor)");
        public static readonly Crime BlockingLandingPadMajor = new Crime("DockingMajor_BlockingLandingPad", "Blocking a landing pad (major)");
        public static readonly Crime TrespassMinor = new Crime("DockingMinor_Trespass", "Trespass (minor)");
        public static readonly Crime TrespassMajor = new Crime("DockingMajor_Trespass", "Trespass (major)");
        public static readonly Crime Collided = new Crime("CollidedAtSpeedInNoFireZone", "Collided");
        public static readonly Crime CollidedWithDamage = new Crime("CollidedAtSpeedInNoFireZone_HullDamage", "Collided with damage");

        public static Crime FromName(string from)
        {
            Crime result = CRIMES.First(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Crime name " + from);
            }
            return result;
        }

        public static Crime FromEDName(string from)
        {
            Crime result = CRIMES.First(v => v.edname == from);
            if (result == null)
            {
                Logging.Report("Unknown Crime ED name " + from);
            }
            return result;
        }
    }
}
