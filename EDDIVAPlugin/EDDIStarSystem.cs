using EliteDangerousDataDefinitions;
using System;

namespace EDDIVAPlugin
{
    /// <summary>
    /// A starsystem with additional data for EDDI
    /// </summary>
    public class EDDIStarSystem
    {
        public int? EliteID { get; set; }
        public int? EDDBID { get; set; }
        public string Name { get; set; }
        public int TotalVisits { get; set; }
        public DateTime LastVisit { get; set; }
        public DateTime? PreviousVisit { get; set; }
        public StarSystem StarSystem { get; set; }
        public DateTime StarSystemLastUpdated { get; set; }
    }

    public interface IEDDIStarSystemRepository
    {
        EDDIStarSystem GetEDDIStarSystem(string name);
        void SaveEDDIStarSystem(EDDIStarSystem starSystem);
    }
}
