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
            Leader = new SquadronRank("Leader", 1);
            SeniorOfficer = new SquadronRank("SeniorOfficer", 2);
            Officer = new SquadronRank("Officer", 3);
            Agent = new SquadronRank("Agent", 4);
            Rookie = new SquadronRank("Rookie", 5);
        }

        public static readonly SquadronRank None;
        public static readonly SquadronRank Leader;
        public static readonly SquadronRank SeniorOfficer;
        public static readonly SquadronRank Officer;
        public static readonly SquadronRank Agent;
        public static readonly SquadronRank Rookie;

        [PublicAPI]
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
