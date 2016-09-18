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
        public static readonly Crime FireInNoFireZone = new Crime("fireInNoFireZone", "Fire in no-fire zone");

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
