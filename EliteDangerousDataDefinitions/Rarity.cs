using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Rarities
    /// </summary>
    public class Rarity
    {
        private static readonly List<Rarity> RARITIES = new List<Rarity>();

        public string name { get; private set; }

        public int level { get; private set; }

        public string edname { get; private set; }

        private Rarity(string edname, int level, string name)
        {
            this.edname = edname;
            this.level = level;
            this.name = name;

            RARITIES.Add(this);
        }

        public static readonly Rarity VeryCommon= new Rarity("verycommon", 1, "Very common");
        public static readonly Rarity Common = new Rarity("common", 2, "Common");
        public static readonly Rarity Standard = new Rarity("standard", 3, "Standard");
        public static readonly Rarity Rare = new Rarity("rare", 4, "Rare");
        public static readonly Rarity VeryRare = new Rarity("veryrare", 5, "VeryRare");

        public static Rarity FromName(string from)
        {
            Rarity result = RARITIES.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Rarity name " + from);
            }
            return result;
        }

        public static Rarity FromLevel(int from)
        {
            Rarity result = RARITIES.FirstOrDefault(v => v.level == from);
            if (result == null)
            {
                Logging.Report("Unknown Rarity level " + from);
            }
            return result;
        }

        public static Rarity FromEDName(string from)
        {
            string tidiedFrom = from == null ? null : from.ToLowerInvariant();
            Rarity result = RARITIES.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Rarity ED name " + from);
            }
            return result;
        }
    }
}
