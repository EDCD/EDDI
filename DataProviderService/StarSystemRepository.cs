using EddiDataDefinitions;

namespace EddiDataProviderService
{
    public interface StarSystemRepository
    {
        StarSystem GetStarSystem(string name, bool refreshIfOutdated = true);
        StarSystem GetOrCreateStarSystem(string name, bool refreshIfOutdated = true);
        StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true);
        void SaveStarSystem(StarSystem starSystem);
        void LeaveStarSystem(StarSystem starSystem);
    }
}
