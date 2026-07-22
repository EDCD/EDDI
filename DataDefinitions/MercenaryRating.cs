using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Mercenary ratings
    /// </summary>
    public class MercenaryRating : ResourceBasedLocalizedEDName<MercenaryRating>
    {
        static MercenaryRating()
        {
            resourceManager = Properties.MercenaryRatings.ResourceManager;
            resourceManager.IgnoreCase = false;
        }

        public static readonly MercenaryRating Defenceless = new("Defenceless", 0);
        public static readonly MercenaryRating MostlyDefenceless = new("MostlyDefenceless", 1);
        public static readonly MercenaryRating Rookie = new("Rookie", 2);
        public static readonly MercenaryRating Mercenary = new("Soldier", 3);
        public static readonly MercenaryRating Gunslinger = new("Gunslinger", 4);
        public static readonly MercenaryRating Warrior = new("Warrior", 5);
        public static readonly MercenaryRating Gladiator = new("Gladiator", 6);
        public static readonly MercenaryRating Deadeye = new("Deadeye", 7);
        public static readonly MercenaryRating Elite = new("Elite", 8);
        public static readonly MercenaryRating EliteI = new("EliteI", 9);
        public static readonly MercenaryRating EliteII = new("EliteII", 10);
        public static readonly MercenaryRating EliteIII = new("EliteIII", 11);
        public static readonly MercenaryRating EliteIV = new("EliteIV", 12);
        public static readonly MercenaryRating EliteV = new("EliteV", 13);

        [PublicAPI( "the numeric rank, from 0 to 13" )]
        public int rank { get; private set; }

        // dummy used to ensure that the static constructor has run
        public MercenaryRating() : this("", 0)
        { }

        private MercenaryRating(string edname, int rank) : base(edname, edname)
        {
            this.rank = rank;
        }

        public static MercenaryRating FromRank(int from)
        {
            var result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown Mercenary Rating rank " + from);
            }
            return result;
        }
    }
}
