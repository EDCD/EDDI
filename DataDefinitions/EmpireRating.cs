using System;
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
        public class MaleRank ( string edname ) : ResourceBasedLocalizedEDName<EmpireRating.MaleRank>( edname, edname )
        {
            static MaleRank()
            {
                resourceManager = Properties.EmpireRatingsMale.ResourceManager;
                resourceManager.IgnoreCase = false;
            }

            // dummy used to ensure that the static constructor has run
            public MaleRank() : this("")
            { }
        }

        public class FemaleRank ( string edname )
            : ResourceBasedLocalizedEDName<EmpireRating.FemaleRank>( edname, edname )
        {
            static FemaleRank()
            {
                resourceManager = Properties.EmpireRatingsFemale.ResourceManager;
                resourceManager.IgnoreCase = false;
            }

            // dummy used to ensure that the static constructor has run
            public FemaleRank() : this("")
            { }
        }

        private static readonly List<EmpireRating> AllOfThem = [ ];
        public string edname { get; }
        public int rank { get; }

        public MaleRank maleRank { get; }

        public FemaleRank femaleRank { get; }

        // Included for consistency with other `Rating` type object definitions as defined by Variables.md

        [PublicAPI("the localized masculine name")]
        public string name => maleRank.localizedName;

        [PublicAPI("the invariant masculine name")]
        public string invariantName => maleRank.invariantName;

        [PublicAPI("the localized feminine name")]
        public string femininename => femaleRank.localizedName;

        [PublicAPI("the invariant feminine name")]
        public string feminineInvariantName => femaleRank.invariantName;

        private EmpireRating(string edname, int rank)
        {
            this.edname = edname;
            this.rank = rank;
            this.maleRank = new MaleRank(edname);
            this.femaleRank = new FemaleRank(edname);
            AllOfThem.Add(this);
        }

        public static readonly EmpireRating None = new("None", 0);
        public static readonly EmpireRating Outsider = new("Outsider", 1);
        public static readonly EmpireRating Serf = new("Serf", 2);
        public static readonly EmpireRating Master = new("Master", 3);
        public static readonly EmpireRating Squire = new("Squire", 4);
        public static readonly EmpireRating Knight = new("Knight", 5);
        public static readonly EmpireRating Lord = new("Lord", 6);
        public static readonly EmpireRating Baron = new("Baron", 7);
        public static readonly EmpireRating Viscount = new("Viscount", 8);
        public static readonly EmpireRating Count = new("Count", 9);
        public static readonly EmpireRating Earl = new("Earl", 10); // normally Countess, but we need to distinguish from rank 9
        public static readonly EmpireRating Marquis = new("Marquis", 11); // or Marchioness <https://en.wikipedia.org/wiki/Marquess>
        public static readonly EmpireRating Duke = new("Duke", 12);
        public static readonly EmpireRating Prince = new("Prince", 13);
        public static readonly EmpireRating King = new("King", 14);

        public static EmpireRating FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            var result = AllOfThem.FirstOrDefault(v =>
                v.maleRank.invariantName == from
                || v.maleRank.localizedName == from
                );
            if (result == null)
            {
                Logging.Info("Unknown Empire Rating name " + from);
            }
            return result;
        }

        public static EmpireRating FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            var result = AllOfThem.FirstOrDefault(v => string.Equals( v.edname, from, StringComparison.OrdinalIgnoreCase ) );
            if (result == null)
            {
                Logging.Info("Unknown Empire Rating ED name " + from);
            }
            return result;
        }

        public static EmpireRating FromRank(int from)
        {
            var result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown Empire Rating rank " + from);
            }
            return result;
        }
    }
}
