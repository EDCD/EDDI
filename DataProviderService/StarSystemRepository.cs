using EddiDataDefinitions;
using System.Collections.Generic;

namespace EddiDataProviderService
{
    public interface StarSystemRepository
    {
        StarSystem GetStarSystem(string name, bool refreshIfOutdated = true);
        StarSystem GetOrCreateStarSystem(string name, bool fetchIfMissing = true, bool refreshIfOutdated = true);
        StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true, bool refreshIfOutdated = true);
        void SaveStarSystem(StarSystem starSystem);
        void LeaveStarSystem(StarSystem starSystem);

        List<StarSystem> GetStarSystems(string[] names, bool refreshIfOutdated = true);
        List<StarSystem> GetOrCreateStarSystems(string[] names, bool fetchIfMissing = true, bool refreshIfOutdated = true);
        List<StarSystem> GetOrFetchStarSystems(string[] names, bool fetchIfMissing = true, bool refreshIfOutdated = true);
        void SaveStarSystems(List<StarSystem> starSystems);
    }
}
