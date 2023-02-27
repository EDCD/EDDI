using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// CQC ratings
    /// </summary>
    public class CQCRating : ResourceBasedLocalizedEDName<CQCRating>
    {
        static CQCRating()
        {
            resourceManager = Properties.CQCRatings.ResourceManager;
            resourceManager.IgnoreCase = false;

            Helpless = new CQCRating("Helpless", 0);
            MostlyHelpless = new CQCRating("MostlyHelpless", 1);
            Amateur = new CQCRating("Amateur", 2);
            SemiProfessional = new CQCRating("SemiProfessional", 3);
            Professional = new CQCRating("Professional", 4);
            Champion = new CQCRating("Champion", 5);
            Hero = new CQCRating("Hero", 6);
            Legend = new CQCRating("Legend", 7);
            Elite = new CQCRating("Elite", 8);
            EliteI = new CQCRating("EliteI", 9);
            EliteII = new CQCRating("EliteII", 10);
            EliteIII = new CQCRating("EliteIII", 11);
            EliteIV = new CQCRating("EliteIV", 12);
            EliteV = new CQCRating("EliteV", 13);
        }

        public static readonly CQCRating Helpless;
        public static readonly CQCRating MostlyHelpless;
        public static readonly CQCRating Amateur;
        public static readonly CQCRating SemiProfessional;
        public static readonly CQCRating Professional;
        public static readonly CQCRating Champion;
        public static readonly CQCRating Hero;
        public static readonly CQCRating Legend;
        public static readonly CQCRating Elite;
        public static readonly CQCRating EliteI;
        public static readonly CQCRating EliteII;
        public static readonly CQCRating EliteIII;
        public static readonly CQCRating EliteIV;
        public static readonly CQCRating EliteV;

        [PublicAPI]
        public int rank { get; private set; }

        // dummy used to ensure that the static constructor has run
        public CQCRating() : this("", 0)
        { }

        private CQCRating(string edname, int rank) : base(edname, edname)
        {
            this.rank = rank;
        }

        public static CQCRating FromRank(int from)
        {
            CQCRating result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown CQC Rating rank " + from);
            }
            return result;
        }
    }
}
