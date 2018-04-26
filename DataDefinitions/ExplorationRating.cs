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

            var Aimless = new ExplorationRating("Aimless", 0);
            var MostlyAimless = new ExplorationRating("MostlyAimless", 1);
            var Scout = new ExplorationRating("Scout", 2);
            var Surveyor = new ExplorationRating("Surveyor", 3);
            var Trailblazer = new ExplorationRating("Trailblazer", 4);
            var Pathfinder = new ExplorationRating("Pathfinder", 5);
            var Ranger = new ExplorationRating("Ranger", 6);
            var Pioneer = new ExplorationRating("Pioneer", 7);
            var Elite = new ExplorationRating("Elite", 8);
        }

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
                Logging.Report("Unknown Exploration Rating rank " + from);
            }
            return result;
        }
    }
}
