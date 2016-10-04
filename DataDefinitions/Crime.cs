using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
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
        public static readonly Crime Interdiction = new Crime("interdiction", "Interdicting");
        public static readonly Crime IllegalCargo = new Crime("illegalCargo", "Carrying illegal cargo");
        public static readonly Crime DisobeyPolice = new Crime("disobeyPolice", "Disobeying police");
        public static readonly Crime FireInNoFireZone = new Crime("fireInNoFireZone", "Firing in a no-fire zone");
        public static readonly Crime FireInStation = new Crime("fireInStation", "Firing in a station");
        public static readonly Crime DumpingDangerous = new Crime("dumpingDangerous", "Dumping dangerous cargo");
        public static readonly Crime DumpingNearStation = new Crime("dumpingNearStation", "Dumping cargo near a station");
        public static readonly Crime BlockingAirlockMinor = new Crime("dockingMinonBlockingAirlock", "Blocking an airlock");
        public static readonly Crime BlockingAirlockMajor = new Crime("dockingMajorBlockingAirlock", "Blocking an airlock");
        public static readonly Crime BlockingLandingPadMinor = new Crime("dockingMinorBlockingLandingPad", "Blocking a landing pad");
        public static readonly Crime BlockingLandingPadMajor = new Crime("dockingMajorBlockingLandingPad", "Blocking a landing pad");
        public static readonly Crime TrespassMinor = new Crime("dockingMinorTrespass", "Trespass");
        public static readonly Crime TrespassMajor = new Crime("dockingMajorTrespass", "Trespass");
        public static readonly Crime Collided = new Crime("collidedAtSpeedInNoFireZone", "Collision");
        public static readonly Crime CollidedWithDamage = new Crime("collidedAtSpeedInNoFireZone_hulldamage", "Collision resulting in damage");

        public static Crime FromName(string from)
        {
            Crime result = CRIMES.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Crime name " + from);
            }
            return result;
        }

        public static Crime FromEDName(string from)
        {
            string tidiedFrom = from == null ? null : from.ToLowerInvariant();
            Crime result = CRIMES.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Crime ED name " + from);
            }
            return result;
        }
    }
}
