using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataProviderService
{
    public interface StarSystemRepository
    {
        StarSystem GetStarSystem(string name, bool refreshIfOutdated = true);
        StarSystem GetOrCreateStarSystem(string name, bool refreshIfOutdated = true);
        StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true);
        void SaveStarSystem(StarSystem starSystem);
    }
}
