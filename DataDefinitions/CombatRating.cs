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

            var Harmless = new CombatRating("Harmless", 0);
            var MostlyHarmless = new CombatRating("MostlyHarmless", 1);
            var Novice = new CombatRating("Novice", 2);
            var Competent = new CombatRating("Competent", 3);
            var Expert = new CombatRating("Expert", 4);
            var Master = new CombatRating("Master", 5);
            var Dangerous = new CombatRating("Dangerous", 6);
            var Deadly = new CombatRating("Deadly", 7);
            var Elite = new CombatRating("Elite", 8);
        }

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
                Logging.Report("Unknown Combat Rating rank " + from);
            }
            return result;
        }
    }
}
