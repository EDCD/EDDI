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
        StarSystem GetStarSystem(string name);
        StarSystem GetOrCreateStarSystem(string name);
        void SaveStarSystem(StarSystem starSystem);
    }
}
