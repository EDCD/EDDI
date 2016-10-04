using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
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

        public static readonly Superpower None = new Superpower("$faction_None;", "None");
        public static readonly Superpower Federation = new Superpower("$faction_Federation;", "Federation");
        public static readonly Superpower Alliance = new Superpower("$faction_Alliance;", "Alliance");
        public static readonly Superpower Empire = new Superpower("$faction_Empire;", "Empire");
        public static readonly Superpower Independent = new Superpower("$faction_Independent;", "Independent");

        public static Superpower FromName(string from)
        {
            Superpower result = SUPERPOWERS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Superpower name " + from);
            }
            return result;
        }

        // We don't report if the result wasn't found because this is used speculatively, and many factions
        // are not superpowers
        public static Superpower FromEDName(string from)
        {
            string tidiedFrom = from == null ? null : from.ToLowerInvariant();
            return SUPERPOWERS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
        }
    }
}
