using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class Rarity : ResourceBasedLocalizedEDName<Rarity>
    {
        static Rarity()
        {
            resourceManager = Properties.Rarities.ResourceManager;
            resourceManager.IgnoreCase = false;

            Unknown = new Rarity("unknown", 0);
            VeryCommon = new Rarity("verycommon", 1);
            Common = new Rarity("common", 2);
            Standard = new Rarity("standard", 3);
            Rare = new Rarity("rare", 4);
            VeryRare = new Rarity("veryrare", 5);
        }

        public static readonly Rarity Unknown;
        public static readonly Rarity VeryCommon;
        public static readonly Rarity Common;
        public static readonly Rarity Standard;
        public static readonly Rarity Rare;
        public static readonly Rarity VeryRare;

        public int level { get; }

        // dummy used to ensure that the static constructor has run
        public Rarity() : this("", 0)
        {}

        private Rarity(string edname, int level) : base(edname, edname)
        {
            this.level = level;
        }
    }
}
