using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Squadron Ranks
    /// </summary>
    public class SquadronRank : ResourceBasedLocalizedEDName<SquadronRank>
    {
        static SquadronRank()
        {
            resourceManager = Properties.SquadronRanks.ResourceManager;
            resourceManager.IgnoreCase = false;

            None = new SquadronRank("None", 0);
            var Leader = new SquadronRank("Leader", 1);
            var SeniorOfficer = new SquadronRank("SeniorOfficer", 2);
            var Officer = new SquadronRank("Officer", 3);
            var Agent = new SquadronRank("Agent", 4);
            var Rookie = new SquadronRank("Rookie", 5);
        }

        public static readonly SquadronRank None;

        public int rank { get; private set; }

        // dummy used to ensure that the static constructor has run
        public SquadronRank() : this("", 0)
        { }

        private SquadronRank(string edname, int rank) : base(edname, edname)
        {
            this.rank = rank;
        }

        public static SquadronRank FromRank(int from)
        {
            SquadronRank result = AllOfThem.FirstOrDefault(v => v.rank == from);
            if (result == null)
            {
                Logging.Info("Unknown Squadron rank " + from);
            }
            return result;
        }
    }
}
