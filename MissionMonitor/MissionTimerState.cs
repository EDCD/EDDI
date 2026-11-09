using System;

namespace EddiMissionMonitor
{
    internal sealed class MissionTimerState ( DateTime deadline )
    {
        public DateTime deadline { get; } = deadline;
        public bool expirationEmitted { get; set; }
        public bool warningEmitted { get; set; }
    }
}
