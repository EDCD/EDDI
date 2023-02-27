using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Combat ratings
    /// </summary>
    public class CombatRating : ResourceBasedLocalizedEDName<CombatRating>
    {
        static CombatRating()
        {
            resourceManager = Properties.CombatRatings.ResourceManager;
            resourceManager.IgnoreCase = false;

            Harmless = new CombatRating("Harmless", 0);
            MostlyHarmless = new CombatRating("MostlyHarmless", 1);
            Novice = new CombatRating("Novice", 2);
            Competent = new CombatRating("Competent", 3);
            Expert = new CombatRating("Expert", 4);
            Master = new CombatRating("Master", 5);
            Dangerous = new CombatRating("Dangerous", 6);
            Deadly = new CombatRating("Deadly", 7);
            Elite = new CombatRating("Elite", 8);
            EliteI = new CombatRating("EliteI", 9);
            EliteII = new CombatRating("EliteII", 10);
            EliteIII = new CombatRating("EliteIII", 11);
            EliteIV = new CombatRating("EliteIV", 12);
            EliteV = new CombatRating("EliteV", 13);
        }

        public static readonly CombatRating Harmless;
        public static readonly CombatRating MostlyHarmless;
        public static readonly CombatRating Novice;
        public static readonly CombatRating Competent;
        public static readonly CombatRating Expert;
        public static readonly CombatRating Master;
        public static readonly CombatRating Dangerous;
        public static readonly CombatRating Deadly;
        public static readonly CombatRating Elite;
        public static readonly CombatRating EliteI;
        public static readonly CombatRating EliteII;
        public static readonly CombatRating EliteIII;
        public static readonly CombatRating EliteIV;
        public static readonly CombatRating EliteV;

        [PublicAPI]
        public int rank { get; private set; }

        // dummy used to ensure that the static constructor has run
        public CombatRating() : this("", 0)
        { }

        private CombatRating(string edname, int rank) : base(edname, edname)
        {
            this.rank = rank;
        }

        public static CombatRating FromRank(int from)
        {
            CombatRating result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown Combat Rating rank " + from);
            }
            return result;
        }
    }
}
