using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Legal Status
    /// </summary>
    public class MissionStatus : ResourceBasedLocalizedEDName<MissionStatus>
    {
        static MissionStatus()
        {
            resourceManager = Properties.MissionStatus.ResourceManager;
            resourceManager.IgnoreCase = false;

            var Active = new MissionStatus("Active", 0);
            var Complete = new MissionStatus("Complete", 1);
            var Failed = new MissionStatus("Failed", 2);
            var Claim = new MissionStatus("Claim", 3);
        }

        public int status { get; private set; }

        // dummy used to ensure that the static constructor has run
        public MissionStatus() : this("", 0)
        { }

        private MissionStatus(string edname, int status) : base(edname, edname)
        {
            this.status = status;
        }

        public static MissionStatus FromStatus(int from)
        {
            MissionStatus result = AllOfThem.FirstOrDefault(v => v.status == from);
            if (result == null)
            {
                Logging.Error("Unknown MissionStatus status " + from);
            }
            return result;
        }

    }
}
