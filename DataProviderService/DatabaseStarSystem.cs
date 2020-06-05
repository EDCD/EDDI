using System;

namespace EddiDataProviderService
{
    public class DatabaseStarSystem
    {
        // Data as read from columns in our database
        public string systemName { get; private set; }
        public long? systemAddress { get; private set; }
        public long? edsmId { get; private set; }
        public string systemJson { get; set; }
        public string comment { get; set; }
        public DateTime lastUpdated { get; set; }
        public DateTime? lastVisit { get; set; }
        public int totalVisits { get; set; }

        public DatabaseStarSystem(string systemName, long? systemAddress, long? edsmId, string systemJson)
        {
            this.systemName = systemName;
            this.systemAddress = systemAddress;
            this.edsmId = edsmId;
            this.systemJson = systemJson;
        }
    }
}
