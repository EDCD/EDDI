using EliteDangerousDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataProviderService
{
    public interface StarSystemRepository
    {
        StarSystem GetStarSystem(string name);
        StarSystem GetOrCreateStarSystem(string name);
        void SaveStarSystem(StarSystem starSystem);
    }
}
