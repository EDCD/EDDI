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

            var None = new LegalStatus("None");
            var Unknown = new LegalStatus("Unknown");
            var Lawless = new LegalStatus("Lawless");
            var Clean = new LegalStatus("Clean");
            var Wanted = new LegalStatus("Wanted");
            var Enemy = new LegalStatus("Enemy");
            var WantedEnemy = new LegalStatus("WantedEnemy");
            var Warrant = new LegalStatus("Warrant");
            var Hunter = new LegalStatus("Hunter");
        }

        // dummy used to ensure that the static constructor has run
        public LegalStatus() : this("")
        { }

        private LegalStatus(string edname) : base(edname, edname)
        { }
    }
}
