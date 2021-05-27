using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Exobiologist ratings
    /// </summary>
    public class ExobiologistRating : ResourceBasedLocalizedEDName<ExobiologistRating>
    {
        static ExobiologistRating()
        {
            resourceManager = Properties.ExobiologistRatings.ResourceManager;
            resourceManager.IgnoreCase = false;
        }

        public static readonly ExobiologistRating Directionless = new ExobiologistRating("Directionless", 0);
        public static readonly ExobiologistRating MostlyDirectionless = new ExobiologistRating("MostlyDirectionless", 1);
        public static readonly ExobiologistRating Compiler = new ExobiologistRating("Compiler", 2);
        public static readonly ExobiologistRating Collector = new ExobiologistRating("Collector", 3);
        public static readonly ExobiologistRating Cataloguer = new ExobiologistRating("Cataloguer", 4);
        public static readonly ExobiologistRating Taxonomist = new ExobiologistRating("Taxonomist", 5);
        public static readonly ExobiologistRating Ecologist = new ExobiologistRating("Ecologist", 6);
        public static readonly ExobiologistRating Geneticist = new ExobiologistRating("Geneticist", 7);
        public static readonly ExobiologistRating Elite = new ExobiologistRating("Elite", 8);
        public static readonly ExobiologistRating EliteI = new ExobiologistRating("EliteI", 9);
        public static readonly ExobiologistRating EliteII = new ExobiologistRating("EliteII", 10);
        public static readonly ExobiologistRating EliteIII = new ExobiologistRating("EliteIII", 11);
        public static readonly ExobiologistRating EliteIV = new ExobiologistRating("EliteIV", 12);
        public static readonly ExobiologistRating EliteV = new ExobiologistRating("EliteV", 13);
        public int rank { get; private set; }

        // dummy used to ensure that the static constructor has run
        public ExobiologistRating() : this("", 0)
        { }

        private ExobiologistRating(string edname, int rank) : base(edname, edname)
        {
            this.rank = rank;
        }

        public static ExobiologistRating FromRank(int from)
        {
            ExobiologistRating result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown Exobiologist Rating rank " + from);
            }
            return result;
        }
    }
}
