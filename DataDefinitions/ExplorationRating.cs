using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Exploration ratings
    /// </summary>
    public class ExplorationRating : ResourceBasedLocalizedEDName<ExplorationRating>
    {
        static ExplorationRating()
        {
            resourceManager = Properties.ExplorationRatings.ResourceManager;
            resourceManager.IgnoreCase = false;

            Aimless = new ExplorationRating("Aimless", 0);
            MostlyAimless = new ExplorationRating("MostlyAimless", 1);
            Scout = new ExplorationRating("Scout", 2);
            Surveyor = new ExplorationRating("Surveyor", 3);
            Trailblazer = new ExplorationRating("Trailblazer", 4);
            Pathfinder = new ExplorationRating("Pathfinder", 5);
            Ranger = new ExplorationRating("Ranger", 6);
            Pioneer = new ExplorationRating("Pioneer", 7);
            Elite = new ExplorationRating("Elite", 8);
            EliteI = new ExplorationRating("EliteI", 9);
            EliteII = new ExplorationRating("EliteII", 10);
            EliteIII = new ExplorationRating("EliteIII", 11);
            EliteIV = new ExplorationRating("EliteIV", 12);
            EliteV = new ExplorationRating("EliteV", 13);
        }

        public static readonly ExplorationRating Aimless;
        public static readonly ExplorationRating MostlyAimless;
        public static readonly ExplorationRating Scout;
        public static readonly ExplorationRating Surveyor;
        public static readonly ExplorationRating Trailblazer;
        public static readonly ExplorationRating Pathfinder;
        public static readonly ExplorationRating Ranger;
        public static readonly ExplorationRating Pioneer;
        public static readonly ExplorationRating Elite;
        public static readonly ExplorationRating EliteI;
        public static readonly ExplorationRating EliteII;
        public static readonly ExplorationRating EliteIII;
        public static readonly ExplorationRating EliteIV;
        public static readonly ExplorationRating EliteV;

        [PublicAPI]
        public int rank { get; private set; }

        // dummy used to ensure that the static constructor has run
        public ExplorationRating() : this("", 0)
        { }

        private ExplorationRating(string edname, int rank) : base(edname, edname)
        {
            this.rank = rank;
        }

        public static ExplorationRating FromRank(int from)
        {
            ExplorationRating result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown Exploration Rating rank " + from);
            }
            return result;
        }
    }
}
