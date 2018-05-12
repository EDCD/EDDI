using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Legal Status
    /// </summary>
    public class LegalStatus : ResourceBasedLocalizedEDName<LegalStatus>
    {
        static LegalStatus()
        {
            resourceManager = Properties.LegalStatus.ResourceManager;
            resourceManager.IgnoreCase = false;

            var None = new LegalStatus("None", 0);
            var Unknown = new LegalStatus("Unknown", 1);
            var Lawless = new LegalStatus("Lawless", 2);
            var Clean = new LegalStatus("Clean", 3);
            var Wanted = new LegalStatus("Wanted", 4);
            var Enemy = new LegalStatus("Enemy", 5);
            var WantedEnemy = new LegalStatus("WantedEnemy", 6);
            var Warrant = new LegalStatus("Warrant", 7);
            var Hunter = new LegalStatus("Hunter", 8);
        }

        public int legalstatus { get; private set; }

        // dummy used to ensure that the static constructor has run
        public LegalStatus() : this("", 0)
        { }

        private LegalStatus(string edname, int rank) : base(edname, edname)
        {
            this.legalstatus = legalstatus;
        }

        public static LegalStatus FromRank(int from)
        {
            LegalStatus result = AllOfThem.FirstOrDefault(v => v.legalstatus == from);
            if (result == null)
            {
                Logging.Report("Unknown Legal Status " + from);
            }
            return result;
        }
    }
}
