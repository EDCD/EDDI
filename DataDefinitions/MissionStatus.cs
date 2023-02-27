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

            Active = new MissionStatus("Active");
            Complete = new MissionStatus("Complete"); // Missions where a `MissionCompleted` event has been written and which are pending removal from the missions log. 
            Failed = new MissionStatus("Failed"); // Missions where a `MissionFailed` or `MissionAbandoned` event has been written and which are pending removal from the missions log.
            Claim = new MissionStatus("Claim"); // The requirements have been satisfied and any timer has been set to zero. Rewards have yet to be claimed.
        }

        public static readonly MissionStatus Active;
        public static readonly MissionStatus Complete;
        public static readonly MissionStatus Failed;
        public static readonly MissionStatus Claim;
        
        // dummy used to ensure that the static constructor has run
        public MissionStatus() : this("")
        { }

        private MissionStatus(string edname) : base(edname, edname)
        { }
    }
}
