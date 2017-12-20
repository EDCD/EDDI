using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Empire ratings
    /// </summary>
    public class EmpireRating
    {
        private static readonly List<EmpireRating> RATINGS = new List<EmpireRating>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public string femininename { get; private set; }

        public int rank { get; private set; }

        private EmpireRating(string edname, int rank, string name) : this(edname, rank, name, name)
        {}

        private EmpireRating(string edname, int rank, string name, string femininename)
        {
            this.edname = edname;
            this.rank = rank;
            this.name = name;
            this.femininename = femininename;

            RATINGS.Add(this);
        }

        public static readonly EmpireRating None = new EmpireRating("None", 0, "None");
        public static readonly EmpireRating Outsider = new EmpireRating("Outsider", 1, "Outsider");
        public static readonly EmpireRating Serf = new EmpireRating("Serf", 2, "Serf");
        public static readonly EmpireRating Master = new EmpireRating("Master", 3, "Master", "Mistress");
        public static readonly EmpireRating Squire = new EmpireRating("Squire", 4, "Squire");
        public static readonly EmpireRating Knight = new EmpireRating("Knight", 5, "Knight", "Dame");
        public static readonly EmpireRating Lord = new EmpireRating("Lord", 6, "Lord", "Lady");
        public static readonly EmpireRating Baron = new EmpireRating("Baron", 7, "Baron", "Baroness");
        public static readonly EmpireRating Viscount = new EmpireRating("Viscount", 8, "Viscount", "Viscountess");
        public static readonly EmpireRating Count = new EmpireRating("Count", 9, "Count", "Countess");
        public static readonly EmpireRating Earl = new EmpireRating("Earl", 10, "Earl"); // normally Countess, but we need to distinguish from rank 9
        public static readonly EmpireRating Marquis = new EmpireRating("Marquis", 11, "Marquis", "Marquise"); // or Marchioness <https://en.wikipedia.org/wiki/Marquess>
        public static readonly EmpireRating Duke = new EmpireRating("Duke", 12, "Duke", "Duchess");
        public static readonly EmpireRating Prince = new EmpireRating("Prince", 13, "Prince", "Princess");
        public static readonly EmpireRating King = new EmpireRating("King", 14, "King", "Queen");

        public static EmpireRating FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            EmpireRating result = RATINGS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Empire Rating name " + from);
            }
            return result;
        }

        public static EmpireRating FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            EmpireRating result = RATINGS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Empire Rating ED name " + from);
            }
            return result;
        }

        public static EmpireRating FromRank(int from)
        {
            EmpireRating result = RATINGS.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Report("Unknown Empire Rating rank " + from);
            }
            return result;
        }
    }
}
