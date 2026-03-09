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

        public static readonly ExobiologistRating Directionless = new("Directionless", 0);
        public static readonly ExobiologistRating MostlyDirectionless = new("MostlyDirectionless", 1);
        public static readonly ExobiologistRating Compiler = new("Compiler", 2);
        public static readonly ExobiologistRating Collector = new("Collector", 3);
        public static readonly ExobiologistRating Cataloguer = new("Cataloguer", 4);
        public static readonly ExobiologistRating Taxonomist = new("Taxonomist", 5);
        public static readonly ExobiologistRating Ecologist = new("Ecologist", 6);
        public static readonly ExobiologistRating Geneticist = new("Geneticist", 7);
        public static readonly ExobiologistRating Elite = new("Elite", 8);
        public static readonly ExobiologistRating EliteI = new("EliteI", 9);
        public static readonly ExobiologistRating EliteII = new("EliteII", 10);
        public static readonly ExobiologistRating EliteIII = new("EliteIII", 11);
        public static readonly ExobiologistRating EliteIV = new("EliteIV", 12);
        public static readonly ExobiologistRating EliteV = new("EliteV", 13);

        [PublicAPI]
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
