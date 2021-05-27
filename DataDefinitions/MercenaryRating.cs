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

        public static readonly MercenaryRating Defenceless = new MercenaryRating("Defenceless", 0);
        public static readonly MercenaryRating MostlyDefenceless = new MercenaryRating("MostlyDefenceless", 1);
        public static readonly MercenaryRating Rookie = new MercenaryRating("Rookie", 2);
        public static readonly MercenaryRating Mercenary = new MercenaryRating("Soldier", 3);
        public static readonly MercenaryRating Gunslinger = new MercenaryRating("Gunslinger", 4);
        public static readonly MercenaryRating Warrior = new MercenaryRating("Warrior", 5);
        public static readonly MercenaryRating Gladiator = new MercenaryRating("Gladiator", 6);
        public static readonly MercenaryRating Deadeye = new MercenaryRating("Deadeye", 7);
        public static readonly MercenaryRating Elite = new MercenaryRating("Elite", 8);
        public static readonly MercenaryRating EliteI = new MercenaryRating("EliteI", 9);
        public static readonly MercenaryRating EliteII = new MercenaryRating("EliteII", 10);
        public static readonly MercenaryRating EliteIII = new MercenaryRating("EliteIII", 11);
        public static readonly MercenaryRating EliteIV = new MercenaryRating("EliteIV", 12);
        public static readonly MercenaryRating EliteV = new MercenaryRating("EliteV", 13);

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
            MercenaryRating result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown Mercenary Rating rank " + from);
            }
            return result;
        }
    }
}
