using System;

namespace EddiDataProviderService
{
    public class DatabaseStarSystem ( string systemName, ulong systemAddress, string systemJson )
    {
        // Data as read from columns in our database
        public string systemName { get; private set; } = systemName;
        public ulong systemAddress { get; private set; } = systemAddress;
        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public decimal? z { get; set; }
        public string systemJson { get; set; } = systemJson;
        public long? population { get; set; }
        public string comment { get; set; }
        public DateTime lastUpdated { get; set; }
        public DateTime? lastVisit { get; set; }
        public int totalVisits { get; set; }
    }
}
