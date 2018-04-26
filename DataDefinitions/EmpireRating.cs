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
        public class MaleRank : ResourceBasedLocalizedEDName<EmpireRating.MaleRank>
        {
            static MaleRank()
            {
                resourceManager = Properties.EmpireRatingsMale.ResourceManager;
                resourceManager.IgnoreCase = false;
            }

            // dummy used to ensure that the static constructor has run
            public MaleRank() : this("")
            {}

            public MaleRank(string edname) : base(edname, edname)
            {}
        }

        public class FemaleRank : ResourceBasedLocalizedEDName<EmpireRating.FemaleRank>
        {
            static FemaleRank()
            {
                resourceManager = Properties.EmpireRatingsFemale.ResourceManager;
                resourceManager.IgnoreCase = false;
            }

            // dummy used to ensure that the static constructor has run
            public FemaleRank() : this("")
            { }

            public FemaleRank(string edname) : base(edname, edname)
            { }
        }

        public static List<EmpireRating> AllOfThem = new List<EmpireRating>();
        public string edname { get; }
        public int rank { get; }
        public MaleRank maleRank { get; }
        public FemaleRank femaleRank { get; }

        private EmpireRating(string edname, int rank)
        {
            this.edname = edname;
            this.rank = rank;
            this.maleRank = new MaleRank(edname);
            this.femaleRank = new FemaleRank(edname);
            AllOfThem.Add(this);
        }

        public static readonly EmpireRating None = new EmpireRating("None", 0);
        public static readonly EmpireRating Outsider = new EmpireRating("Outsider", 1);
        public static readonly EmpireRating Serf = new EmpireRating("Serf", 2);
        public static readonly EmpireRating Master = new EmpireRating("Master", 3);
        public static readonly EmpireRating Squire = new EmpireRating("Squire", 4);
        public static readonly EmpireRating Knight = new EmpireRating("Knight", 5);
        public static readonly EmpireRating Lord = new EmpireRating("Lord", 6);
        public static readonly EmpireRating Baron = new EmpireRating("Baron", 7);
        public static readonly EmpireRating Viscount = new EmpireRating("Viscount", 8);
        public static readonly EmpireRating Count = new EmpireRating("Count", 9);
        public static readonly EmpireRating Earl = new EmpireRating("Earl", 10); // normally Countess, but we need to distinguish from rank 9
        public static readonly EmpireRating Marquis = new EmpireRating("Marquis", 11); // or Marchioness <https://en.wikipedia.org/wiki/Marquess>
        public static readonly EmpireRating Duke = new EmpireRating("Duke", 12);
        public static readonly EmpireRating Prince = new EmpireRating("Prince", 13);
        public static readonly EmpireRating King = new EmpireRating("King", 14);

        public static EmpireRating FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            EmpireRating result = AllOfThem.FirstOrDefault(v => 
                v.maleRank.invariantName == from
                || v.maleRank.localizedName == from
                );
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
            EmpireRating result = AllOfThem.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Empire Rating ED name " + from);
            }
            return result;
        }

        public static EmpireRating FromRank(int from)
        {
            EmpireRating result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Report("Unknown Empire Rating rank " + from);
            }
            return result;
        }
    }
}
