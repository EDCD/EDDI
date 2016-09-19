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
        public static readonly Crime Murder = new Crime("murder", "Murder");
        public static readonly Crime Piracy = new Crime("piracy", "Piracy");
        public static readonly Crime Interdiction = new Crime("interdiction", "Interdiction");
        public static readonly Crime IllegalCargo = new Crime("illegalCargo", "Illegal cargo");
        public static readonly Crime DisobeyPolice = new Crime("disobeyPolice", "Disobey police");
        public static readonly Crime FireInNoFireZone = new Crime("fireInNoFireZone", "Fire in no-fire zone");
        public static readonly Crime FireInStation = new Crime("fireInStation", "Fire in station");
        public static readonly Crime DumpingDangerous = new Crime("dumpingDangerous", "Dumping dangerous cargo");
        public static readonly Crime DumpingNearStation = new Crime("dumpingNearStation", "Dumping cargo near a station");
        public static readonly Crime BlockingAirlockMinor = new Crime("dockingMinor_BlockingAirlock", "Blocking an airlock (minor)");
        public static readonly Crime BlockingAirlockMajor = new Crime("dockingMajor_BlockingAirlock", "Blocking an airlock (major)");
        public static readonly Crime BlockingLandingPadMinor = new Crime("dockingMinor_BlockingLandingPad", "Blocking a landing pad (minor)");
        public static readonly Crime BlockingLandingPadMajor = new Crime("dockingMajor_BlockingLandingPad", "Blocking a landing pad (major)");
        public static readonly Crime TrespassMinor = new Crime("dockingMinor_Trespass", "Trespass (minor)");
        public static readonly Crime TrespassMajor = new Crime("dockingMajor_Trespass", "Trespass (major)");
        public static readonly Crime Collided = new Crime("collidedAtSpeedInNoFireZone", "Collided");
        public static readonly Crime CollidedWithDamage = new Crime("collidedAtSpeedInNoFireZone_HullDamage", "Collided with damage");

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
