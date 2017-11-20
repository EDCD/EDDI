using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// CQC ratings
    /// </summary>
    public class CQCRating
    {
        private static readonly List<CQCRating> RATINGS = new List<CQCRating>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public int rank { get; private set; }

        private CQCRating(string edname, int rank, string name)
        {
            this.edname = edname;
            this.rank = rank;
            this.name = name;

            RATINGS.Add(this);
        }

        public static readonly CQCRating Helpless = new CQCRating("Helpless", 0, "Helpless");
        public static readonly CQCRating MostlyHelpless = new CQCRating("MostlyHelpless", 1, "Mostly Helpless");
        public static readonly CQCRating Amateur = new CQCRating("Amateur", 2, "Amateur");
        public static readonly CQCRating SemiProfessional = new CQCRating("SemiProfessional", 3, "Semi-professional");
        public static readonly CQCRating Professional = new CQCRating("Professional", 4, "Professional");
        public static readonly CQCRating Champion = new CQCRating("Champion", 5, "Champion");
        public static readonly CQCRating Hero = new CQCRating("Hero", 6, "Hero");
        public static readonly CQCRating Legend = new CQCRating("Legend", 7, "Legend");
        public static readonly CQCRating Elite = new CQCRating("Elite", 8, "Elite");

        public static CQCRating FromName(string from)
        {
            CQCRating result = RATINGS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown CQC Rating name " + from);
            }
            return result;
        }

        public static CQCRating FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            CQCRating result = RATINGS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown CQC Rating ED name " + from);
            }
            return result;
        }

        public static CQCRating FromRank(int from)
        {
            CQCRating result = RATINGS.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Report("Unknown CQC Rating rank " + from);
            }
            return result;
        }
    }
}
