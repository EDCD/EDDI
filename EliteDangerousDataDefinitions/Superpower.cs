using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Superpowers
    /// </summary>
    public class Superpower
    {
        private static readonly List<Superpower> SUPERPOWERS = new List<Superpower>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private Superpower(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            SUPERPOWERS.Add(this);
        }

        public static readonly Superpower None = new Superpower("None", "None");
        public static readonly Superpower Federation = new Superpower("Federation", "Federation");
        public static readonly Superpower Alliance = new Superpower("Alliance", "Alliance");
        public static readonly Superpower Empire = new Superpower("Empire", "Empire");
        public static readonly Superpower Independent = new Superpower("Independent", "Independent");

        public static Superpower FromName(string from)
        {
            Superpower result = SUPERPOWERS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Superpower name " + from);
            }
            return result;
        }

        public static Superpower FromEDName(string from)
        {
            string tidiedFrom = from == null ? null : from.ToLowerInvariant();
            Superpower result = SUPERPOWERS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Superpower ED name " + from);
            }
            return result;
        }
    }
}
