using System;

namespace EddiMissionMonitor
{
    internal sealed class MissionSnapshotReconciliationState ( DateTime reconcileAt )
    {
        public DateTime reconcileAt { get; } = reconcileAt;
    }
}
