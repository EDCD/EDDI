using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Combat ratings
    /// </summary>
    public class CombatRating
    {
        private static readonly List<CombatRating> RATINGS = new List<CombatRating>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public int rank { get; private set; }

        private CombatRating(string edname, int rank, string name)
        {
            this.edname = edname;
            this.rank = rank;
            this.name = name;

            RATINGS.Add(this);
        }

        public static readonly CombatRating Harmless = new CombatRating("Harmless", 0, "Harmless");
        public static readonly CombatRating MostlyHarmless = new CombatRating("MostlyHarmless", 1, "Mostly Harmless");
        public static readonly CombatRating Novice = new CombatRating("Novice", 2, "Novice");
        public static readonly CombatRating Competent = new CombatRating("Competent", 3, "Competent");
        public static readonly CombatRating Expert = new CombatRating("Expert", 4, "Expert");
        public static readonly CombatRating Master = new CombatRating("Master", 5, "Master");
        public static readonly CombatRating Dangerous = new CombatRating("Dangerous", 6, "Dangerous");
        public static readonly CombatRating Deadly = new CombatRating("Deadly", 7, "Deadly");
        public static readonly CombatRating Elite = new CombatRating("Elite", 8, "Elite");

        public static CombatRating FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            CombatRating result = RATINGS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Combat Rating name " + from);
            }
            return result;
        }

        public static CombatRating FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            CombatRating result = RATINGS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Combat Rating ED name " + from);
            }
            return result;
        }

        public static CombatRating FromRank(int from)
        {
            CombatRating result = RATINGS.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Report("Unknown Combat Rating rank " + from);
            }
            return result;
        }
    }
}
