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

            var Active = new MissionStatus("Active");
            var Complete = new MissionStatus("Complete");
            var Failed = new MissionStatus("Failed");
        }

        // dummy used to ensure that the static constructor has run
        public MissionStatus() : this("")
        { }

        private MissionStatus(string edname) : base(edname, edname)
        { }
    }
}
