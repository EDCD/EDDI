using System;

namespace EddiDataProviderService
{
    public class DatabaseStarSystem
    {
        // Data as read from columns in our database
        public string systemName { get; private set; }
        public ulong? systemAddress { get; private set; }
        public string systemJson { get; set; }
        public string comment { get; set; }
        public DateTime lastUpdated { get; set; }
        public DateTime? lastVisit { get; set; }
        public int totalVisits { get; set; }

        public DatabaseStarSystem(string systemName, ulong? systemAddress, string systemJson)
        {
            this.systemName = systemName;
            this.systemAddress = systemAddress;
            this.systemJson = systemJson;
        }
    }
}
