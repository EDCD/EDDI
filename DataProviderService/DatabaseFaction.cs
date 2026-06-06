using System;

namespace EddiDataProviderService
{
    public class DatabaseFaction ( string name )
    {
        public string name { get; private set; } = name;
        public decimal? myreputation { get; set; }
        public DateTime? reputationUpdatedAt { get; set; }
    }
}
