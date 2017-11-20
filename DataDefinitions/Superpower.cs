using System.Collections.Generic;
using System.Linq;

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

        public static readonly Superpower None = new Superpower("$faction_none;", "None");
        public static readonly Superpower Federation = new Superpower("$faction_Federation;", "Federation");
        public static readonly Superpower Alliance = new Superpower("$faction_Alliance;", "Alliance");
        public static readonly Superpower Empire = new Superpower("$faction_Empire;", "Empire");
        public static readonly Superpower Independent = new Superpower("$faction_Independent;", "Independent");
        public static readonly Superpower Pirate = new Superpower("$faction_Pirate;", "Pirate");

        public static Superpower From(string from)
        {
            if (from == null)
            {
                return null;
            }

            Superpower result = FromName(from);
            if (result == null)
            {
                result = FromEDName(from);
            }
            return result;
        }

        public static Superpower FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            Superpower result = SUPERPOWERS.FirstOrDefault(v => v.name == from);
            return result;
        }

        // We don't report if the result wasn't found because this is used speculatively, and many factions
        // are not superpowers
        public static Superpower FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            Superpower result;
            if (tidiedFrom == null || tidiedFrom == "")
            {
                result = null;
            }
            else
            {
                result = SUPERPOWERS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            }
            return result;
        }
    }
}
