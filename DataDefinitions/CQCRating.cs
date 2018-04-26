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

            var Helpless = new CQCRating("Helpless", 0);
            var MostlyHelpless = new CQCRating("MostlyHelpless", 1);
            var Amateur = new CQCRating("Amateur", 2);
            var SemiProfessional = new CQCRating("SemiProfessional", 3);
            var Professional = new CQCRating("Professional", 4);
            var Champion = new CQCRating("Champion", 5);
            var Hero = new CQCRating("Hero", 6);
            var Legend = new CQCRating("Legend", 7);
            var Elite = new CQCRating("Elite", 8);
        }

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
                Logging.Report("Unknown CQC Rating rank " + from);
            }
            return result;
        }
    }
}
